namespace Document.AWS.Connect
{
    using System.Xml.Serialization;

    [XmlRoot("AWSEnvironment")]
    public class AWSEnvironment
    {
        [XmlElement("AccessKeyID")]
        public string AccessKeyID { get; set; }
        [XmlElement("SecretAccessKey")]
        public string SecretAccessKey { get; set; }
        [XmlElement("HTMLPage")]
        public string HTMLPage { get; set; }
        [XmlElement("Region")]
        public string Region { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }
    }
}
