namespace Document.AWS.Connect
{
    using System.Xml.Serialization;

    [XmlRoot("ServerIcon")]
    public class ServerIcon
    {
        [XmlElement("Role")]
        public string Role { get; set; }

        [XmlElement("Icon")]
        public string Icon { get; set; }
        [XmlElement("IsUnknown")]
        public bool IsUnknown { get; set; }
    }
}
