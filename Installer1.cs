using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Install;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace BrowserAutomation01
{
    [RunInstaller(true)]
    public partial class Installer1 : System.Configuration.Install.Installer
    {
        
        public Installer1()
        {
            InitializeComponent();
        }

       
        // Override the 'Install' method.
        public override void Install(IDictionary savedState)
        {
            base.Install(savedState);
            


            string UserProfilePath = this.Context.Parameters["USERPROFILEPATH"].ToString().Trim();
            string TwilioMoNum = this.Context.Parameters["TWILIOMONUM"].ToString().Trim();
            string ToMoNum = this.Context.Parameters["TOMONUM"].ToString().Trim();
            string TwAccountSid = this.Context.Parameters["TWACCOUNTSID"].ToString().Trim();
            string TwAuthToken = this.Context.Parameters["TWAUTHTOKEN"].ToString().Trim();
           

            if (TwilioMoNum.Length != 0 && ToMoNum.Length == 0)
                throw new Exception("To activate SMS feature you need both sender's and receiver's cell num, sender's number should be a Twilo number1 ");
            if (TwilioMoNum.Length == 0 && ToMoNum.Length != 0)
                throw new Exception("To activate SMS feature you need both sender's and receiver's cell num, sender's number should be a Twilo number2 ");
            if ((TwilioMoNum.Length != 0 && ToMoNum.Length != 0) && (TwAccountSid.Length == 0 && TwAuthToken.Length == 0))
                throw new Exception("To activate SMS feature you need Twilo account3");
            if ((TwilioMoNum.Length != 0 && ToMoNum.Length != 0) && (TwAccountSid.Length != 0 && TwAuthToken.Length == 0))
                throw new Exception("To activate SMS feature you need Twilo account4");
            if ((TwilioMoNum.Length != 0 && ToMoNum.Length != 0) && (TwAccountSid.Length == 0 && TwAuthToken.Length != 0))
                throw new Exception("To activate SMS feature you need Twilo account5");
            //if ((TwilioMoNum.Length == 0 && ToMoNum.Length == 0) && (TwAccountSid.Length == 0 && TwAuthToken.Length == 0))
            //    MessageBox.Show("Application will work, but without Twillo account and mobile numbers SMS feature won't work!!");


        }
        // Override the 'Commit' method.
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);

            AddConfigurationFileDetails();
            int length = Context.Parameters["TargetDir"].ToString().Length - 1;
            string targetDir = Context.Parameters["TargetDir"].ToString().Substring(0, length);
            string xml = targetDir + @"jobtemplate\CheckOut.xml";
            
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xml);
                FindAndSetNodeValue(doc.ChildNodes, targetDir);
                doc.Save(xml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        // Override the 'Rollback' method.
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }


        //private void showParameters()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    StringDictionary myStringDictionary = this.Context.Parameters;
        //    if (this.Context.Parameters.Count > 0)
        //    {
        //        foreach (string myString in this.Context.Parameters.Keys)
        //        {
        //            sb.AppendFormat("String={0} Value= {1}\n", myString,
        //            this.Context.Parameters[myString]);
        //        }
        //    }
        //    MessageBox.Show(sb.ToString());
        //}

        public void AddConfigurationFileDetails()
        {
            string UserProfilePath = this.Context.Parameters["USERPROFILEPATH"].ToString().Trim();
            string TwilioMoNum = this.Context.Parameters["TWILIOMONUM"].ToString().Trim();
            string ToMoNum = this.Context.Parameters["TOMONUM"].ToString().Trim();
            string TwAccountSid = this.Context.Parameters["TWACCOUNTSID"].ToString().Trim();
            string TwAuthToken = this.Context.Parameters["TWAUTHTOKEN"].ToString().Trim();

            if (UserProfilePath.Length == 0 && TwilioMoNum.Length == 0 && ToMoNum.Length == 0 && TwAccountSid.Length == 0 && TwAuthToken.Length == 0)
            {
                // do nothing
            }
            else
            {
                try
                {
                    // Get the path to the executable file that is being installed on the target computer  
                    string assemblypath = Context.Parameters["assemblypath"];
                    //MessageBox.Show("assemblypath="+assemblypath);
                    string appConfigPath = assemblypath + ".config";

                    // Write the path to the app.config file  
                    XmlDocument doc = new XmlDocument();
                    doc.Load(appConfigPath);

                    XmlNode configuration = null;
                    foreach (XmlNode node in doc.ChildNodes)
                        if (node.Name == "configuration")
                            configuration = node;

                    if (configuration != null)
                    {
                        //MessageBox.Show("configuration != null");  
                        // Get the ‘appSettings’ node  
                        XmlNode settingNode = null;
                        foreach (XmlNode node in configuration.ChildNodes)
                        {
                            if (node.Name == "appSettings")
                                settingNode = node;
                        }

                        if (settingNode != null)
                        {
                            //MessageBox.Show("settingNode != null");  
                            //Reassign values in the config file  
                            foreach (XmlNode node in settingNode.ChildNodes)
                            {
                                //MessageBox.Show("node.Value = " + node.Value);  
                                if (node.Attributes == null)
                                    continue;
                                XmlAttribute attribute = node.Attributes["value"];
                                //MessageBox.Show("attribute != null ");  
                                //MessageBox.Show("node.Attributes['value'] = " + node.Attributes["value"].Value);  
                                if (node.Attributes["key"] != null)
                                {
                                    //MessageBox.Show("node.Attributes['key'] != null ");  
                                    //MessageBox.Show("node.Attributes['key'] = " + node.Attributes["key"].Value);  
                                    //switch (node.Attributes["key"].Value)
                                    //{
                                    //    case "TestParameter":
                                    //        attribute.Value = TESTPARAMETER;
                                    //        break;
                                    //}


                                    if (node.Attributes["key"].Value == "UserProfilePath" && UserProfilePath.Length!=0)
                                    {
                                        attribute.Value = UserProfilePath;
                                    }
                                    if (node.Attributes["key"].Value == "TwilioMoNum" && TwilioMoNum.Length != 0)
                                    {
                                        attribute.Value = TwilioMoNum;
                                    }
                                    if (node.Attributes["key"].Value == "ToMoNum" && ToMoNum.Length != 0)
                                    {
                                        attribute.Value = ToMoNum;
                                    }
                                    if (node.Attributes["key"].Value == "TwAccountSid" && TwAccountSid.Length != 0)
                                    {
                                        attribute.Value = TwAccountSid;
                                    }
                                    if (node.Attributes["key"].Value == "TwAuthToken" && TwAuthToken.Length != 0)
                                    {
                                        attribute.Value = TwAuthToken;
                                    }
                                }
                            }
                        }
                        doc.Save(appConfigPath);
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public void FindAndSetNodeValue(XmlNodeList pnodes, string targetdir)
        {

            string jobUser = Environment.UserDomainName + "\\" + Environment.UserName;
            foreach (XmlNode chnode in pnodes)
            {
                XmlNode settingNode = null;
                string nodename = chnode.Name;
                switch (nodename)
                {
                    case "Author":
                        settingNode = chnode;
                        settingNode.InnerText = jobUser;
                        break;

                    case "UserId":
                        settingNode = chnode;
                        settingNode.InnerText = jobUser;
                        break;
                    case "Command":
                        settingNode = chnode;
                        settingNode.InnerText = targetdir + "BrowserAutomation01.exe";
                        break;
                    case "WorkingDirectory":
                        settingNode = chnode;
                        settingNode.InnerText = targetdir;
                        break;
                }

                FindAndSetNodeValue(chnode.ChildNodes, targetdir);

            }

        }
    }


}
