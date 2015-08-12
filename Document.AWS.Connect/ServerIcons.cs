namespace Document.AWS.Connect
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot("ServerIcons")]
    public class ServerIcons
    {
        public ServerIcons()
        {
            List = new List<ServerIcon>();
        }

        [XmlArrayItem("ServerIcon", typeof(ServerIcon))]
        [XmlArray("List")]
        public List<ServerIcon> List { get; set; }
    }
}
