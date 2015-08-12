namespace Document.AWS.Connect
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("AWSEnvironments")]
    public class AWSEnvironments
    {
        public AWSEnvironments()
        {
            List = new List<AWSEnvironment>();
        }

        [XmlArrayItem("AWSEnvironment", typeof(AWSEnvironment))]
        [XmlArray("List")]
        public List<AWSEnvironment> List { get; set; }
    }
}
