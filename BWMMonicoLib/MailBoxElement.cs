using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BWMMonacoLib
{
    public sealed class MailBoxElement : ConfigurationElement
    {
        [ConfigurationProperty("ServerName", IsRequired = true)]
        public string ServerName
        {
            get
            {
                return (string)base["ServerName"];
            }
            set
            {
                base["Servername"] = value;
            }
        }

        [ConfigurationProperty("UserName", IsKey = true, IsRequired = true)]
        public string UserName
        {
            get
            {
                return (string)base["UserName"];
            }
            set
            {
                base["UserName"] = value;
            }
        }

        [ConfigurationProperty("UNCShare", IsRequired = true)]
        public string UNCShare
        {
            get
            {
                return (string)base["UNCShare"];
            }
            set
            {
                base["UNCShare"] = value;
            }
        }

        public override string ToString()
        {
            string output = "MailBoxElement : \n";
            output += string.Format("Servername = {0}\n", ServerName);
            output += string.Format("UserName = {0}\n", UserName);
            output += string.Format("UNCShare = {0}\n", UNCShare);
            return output;
        }
    }
}
