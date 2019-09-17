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
    public sealed class RightFAXServersSection : ConfigurationSection
    {
        public RightFAXServersSection()
        {
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public RightFAXServersCollection RightFAXServers
        {
            get
            {
                return (RightFAXServersCollection)base[""];
            }
        }
    }
}
