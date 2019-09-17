using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BWMMonacoLib
{
    public sealed class MailBoxCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MailBoxElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MailBoxElement)element).UserName;
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
                return "MailBox";
            }
        }
    }
}
