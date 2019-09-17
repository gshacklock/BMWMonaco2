using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BWMMonacoLib
{
    public sealed class RightFAXServersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RightFAXServersElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RightFAXServersElement)element).ServerName;
        }
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        protected override string ElementName
        {
            get
            {
                return "RightFAXServer";
            }
        }
    }
}
