using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Voice;
using Thread = System.Threading.Thread;

namespace BrowserAutomation01
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static IDictionary<string, object> vars { get; private set; }

        static Configuration config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetEntryAssembly().Location);

        //static string runJob = string.Empty;
        static string pathToCurrentUserProfile = string.Empty;
        static string goToUrl = string.Empty;
        static string twilioMoNum = string.Empty;
        static string toMoNum = string.Empty;

        [Obsolete]
        static void Main(string[] args)
        {
            AppSettingsSection appSettingsSection = (AppSettingsSection)config.GetSection("appSettings");
            KeyValueConfigurationCollection settings = appSettingsSection.Settings;
            if (settings.Count != 0)
            {

                string runJob = ReturnSettings(settings, MyAppSettings.RunJob);
                pathToCurrentUserProfile = ReturnSettings(settings, MyAppSettings.UserProfilePath);
                goToUrl = ReturnSettings(settings, MyAppSettings.GoToUrl);
                twilioMoNum = ReturnSettings(settings, MyAppSettings.TwilioMoNum);
                toMoNum = ReturnSettings(settings, MyAppSettings.ToMoNum);

                if (runJob.ToLower() == "yes")
                    Checkout(settings);
                else if (runJob.ToLower() == "no")
                    log.Info("Checkout presently stopped!!");
                else if (runJob.ToLower().Trim().Length == 0)
                {
                    log.Error("No Run Job Direction");
                }

            }
            else
            {
                log.Error("No App Setting Values Exists!!");
            }




        }
        static string ReturnSettings(KeyValueConfigurationCollection settings, MyAppSettings settingKey)
        {
            if (settings[settingKey.Value] != null && settings[settingKey.Value].Value.Trim().Length != 0)
                return settings[settingKey.Value].Value.Trim();
            else
                return string.Empty;
        }
        private static void Checkout(KeyValueConfigurationCollection settings)
        {
            log.Info("Checkout started!!");
            //Get value from AppSettings
            
            
            string pathToCurrentUserProfilesDirectory = Environment.ExpandEnvironmentVariables("%APPDATA%") + @"\Mozilla\Firefox\Profiles";
            string[] pathsToProfiles = Directory.GetDirectories(pathToCurrentUserProfilesDirectory);

            string pathtoDefaultProfile = pathsToProfiles[0];
            if (pathToCurrentUserProfile != string.Empty)
                pathtoDefaultProfile = pathToCurrentUserProfile;



            log.Info("Profile Path=" + pathtoDefaultProfile);
            

            if (pathsToProfiles.Length != 0)
            {

                FirefoxProfile profile = new FirefoxProfile(pathtoDefaultProfile);
                profile.SetPreference("browser.tabs.loadInBackground", true);

                FirefoxOptions options = new FirefoxOptions();
                options.Profile = profile;
                var driverService = FirefoxDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                FirefoxDriver driver = new FirefoxDriver(driverService, options);
                WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, 5));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                vars = new Dictionary<string, object>();
                try
                {

                    //Get value from AppSettings
                    if (goToUrl == string.Empty)
                        driver.Navigate().GoToUrl("https://www.bigbasket.com/basket/");
                    else
                        driver.Navigate().GoToUrl(goToUrl);

                    //driver.Manage().Window.Size = new System.Drawing.Size(1659, 917);
                    driver.Manage().Window.Maximize();
                    driver.FindElement(By.CssSelector("#checkout > .icon")).Click();

                    log.Info("Checkout click Successful!!");
                    Thread.Sleep(5000);


                    try
                    {
                        IWebElement slotButton = driver.FindElement(By.CssSelector(".slotmodal-btn-confirm"));
                        Thread.Sleep(5000);
                        IWebElement slotcontent = driver.FindElement(By.XPath("//p[@id=\'slotContent\']"));
                        Thread.Sleep(5000);

                        vars["ConfirmationMsg"] = slotcontent.Text;
                        string txt = vars["ConfirmationMsg"].ToString();


                        if (txt.Length != 0)
                        {
                            log.Info(txt);
                            Actions act = new Actions(driver);
                            act.MoveToElement(slotButton).Click().Perform();

                            if (txt.ToLower().Contains("unfortunately"))
                            {
                                log.Info("No slots available");
                            }
                            else
                            {
                                StopCheckout(settings);
                                log.Info("Checkout Stopped");
                                SendSMS("Might have been successful, please check", settings);
                                
                            }

                        }
                        else
                        {
                            log.Info(txt);
                            StopCheckout(settings);
                            log.Info("Checkout Stopped");
                            SendSMS("Might have been successful, please check", settings);
                            
                        }

                    }
                    catch (Exception ex)
                    {
                        StopCheckout(settings);
                        log.Info("Checkout Stopped");
                        SendSMS("Might have been successful, please check", settings);
                        log.Error(ex.Message);
                    }





                    log.Info("Checkout ends!!");

                    driver.Close();
                    driver.Quit();

                }
                catch (Exception ex)
                {
                    StopCheckout(settings);
                    log.Info("Checkout Stopped");
                    SendSMS("has an error", settings);
                    log.Error(ex.Message);
                    driver.Close();
                    driver.Quit();
                }
            }
            else
                log.Info("No profile presents");
        }

        private static void StopCheckout(KeyValueConfigurationCollection settings)
        {

            string presentValue = settings["RunJob"].Value;
            if (presentValue.ToLower() == "yes")
            {
                settings["RunJob"].Value = "no";
            }
            config.Save();
        }

        private static void SendSMS(string success, KeyValueConfigurationCollection settings)
        {

            try
            {
                //Get value from AppSettings
                string accountSid = ReturnSettings(settings, MyAppSettings.TwAccountSid);
                //Get value from AppSettings
                string authToken = ReturnSettings(settings, MyAppSettings.TwAuthToken);
                //Get value from AppSettings
                string twilioMoNum = ReturnSettings(settings, MyAppSettings.TwilioMoNum);
                //Get value from AppSettings
                string toMoNum = ReturnSettings(settings, MyAppSettings.ToMoNum);


                if (accountSid == string.Empty)
                    throw new Exception("No SID");
                if (authToken == string.Empty)
                    throw new Exception("No Auth Token");
                if (twilioMoNum == string.Empty)
                    throw new Exception("No Twillo Number");
                if (toMoNum == string.Empty)
                    throw new Exception("No Mobile Number to send messages");

                TwilioClient.Init(accountSid, authToken);

                //Get cell nums from AppSettings
                if (toMoNum.Contains(","))
                {
                    string[] tonums = toMoNum.Split(',');
                    for (int i = 0; i < tonums.Length; i++)
                    {
                        try
                        {
                            SendSMS(success, tonums[i].Trim());
                        }
                        catch (Exception ex)
                        {
                            log.Info("Message to " + tonums[i] + " has been " + "fail" + ".");
                            log.Error(ex.Message);
                        }
                    }
                }
                else
                {
                    try
                    {
                        SendSMS(success, toMoNum.Trim());
                    }
                    catch (Exception ex)
                    {
                        log.Info("Message to " + toMoNum + " has been " + "fail" + ".");
                        log.Error(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info("SMS sent fail");
                log.Error(ex.Message);
            }

        }

        private static void SendSMS(string success, string tomo)
        {
            var message = MessageResource.Create(
                                            body: "BigBasker Shoppinf checkout attempt " + success,
                                            from: new Twilio.Types.PhoneNumber(twilioMoNum),
                                            to: new Twilio.Types.PhoneNumber(tomo)
                                        );
            log.Info("Message to " + tomo + " has been " + message.Status + ".");
        }

        public class MyAppSettings
        {
            private MyAppSettings(string value) { Value = value; }

            public string Value { get; set; }

            public static MyAppSettings RunJob { get { return new MyAppSettings("RunJob"); } }
            public static MyAppSettings UserProfilePath { get { return new MyAppSettings("UserProfilePath"); } }
            public static MyAppSettings GoToUrl { get { return new MyAppSettings("GoToUrl"); } }
            public static MyAppSettings TwilioMoNum { get { return new MyAppSettings("TwilioMoNum"); } }
            public static MyAppSettings ToMoNum { get { return new MyAppSettings("ToMoNum"); } }
            public static MyAppSettings TwAccountSid { get { return new MyAppSettings("TwAccountSid"); } }
            public static MyAppSettings TwAuthToken { get { return new MyAppSettings("TwAuthToken"); } }
        }


    }
}

        



    

