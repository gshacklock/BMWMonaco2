namespace BWMMonacoLib
{
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd", IsNullable=false)]
    public class BatchContainer {
        
        /// <remarks/>
        public AbstractBatch batch;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ScanBatch))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FaxBatch))]
    public abstract class AbstractBatch {
        
        /// <remarks/>
        public int id;
        
        /// <remarks/>
        public System.DateTime dateTime;
        
        /// <remarks/>
        public string timezone;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("documents")]
        public Document[] documents;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("attributes")]
        public Attribute[] attributes;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd")]
    public class Document {
        
        /// <remarks/>
        public int id;
        
        /// <remarks/>
        public int pageCount;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("attributes")]
        public Attribute[] attributes;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd")]
    public class Attribute {
        
        /// <remarks/>
        public string key;
        
        /// <remarks/>
        public string value;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd")]
    public class ScanBatch : AbstractBatch {
        
        /// <remarks/>
        public string scanLocation;
        
        /// <remarks/>
        public string discriminator;
        
        /// <remarks/>
        public string userId;
        
        /// <remarks/>
        public string mimeTypeIdentifier;
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://barclays.co.uk/iaw/scanning/Batch.xsd")]
    public class FaxBatch : AbstractBatch {
        
        /// <remarks/>
        public string sourceCli;
        
        /// <remarks/>
        public string destinationCli;
    }
}
