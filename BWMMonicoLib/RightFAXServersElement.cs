using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BWMMonacoLib
{
    public sealed class RightFAXServersElement : ConfigurationElement
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

        [ConfigurationProperty("UserName", IsRequired = true)]
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

        [ConfigurationProperty("Password", IsRequired = true)]
        public string Password
        {
            get
            {
                return (string)base["Password"];
            }
            set
            {
                base["Password"] = value;
            }
        }

        public override string ToString()
        {
            string output = "MailBoxElement : \n\r";
            output += string.Format("Servername = {0} : ", ServerName);
            output += string.Format("UserName = {0} : ", UserName);
            output += string.Format("Password = ****");
            return output;
        }
    }
}
