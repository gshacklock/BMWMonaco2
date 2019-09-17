using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration; 
using System.Reflection;
using System.Text;


namespace BWMMonacoLib
{
    /// <summary>
    /// </summary>
    public sealed class MailBoxSection : ConfigurationSection
    {
        public MailBoxSection()
        {
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public MailBoxCollection MailBoxes
        {
            get
            {
                return (MailBoxCollection)base[""];
            }
        }
    }
}
