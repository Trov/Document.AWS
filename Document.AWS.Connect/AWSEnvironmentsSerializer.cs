namespace Document.AWS.Connect
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using NLog;

    public class AWSEnvironmentsSerializer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private XmlSerializer serializer;
        private Type type;

        public AWSEnvironmentsSerializer()
        {
            Initialize();
        }

        public AWSEnvironments Deserialize(string xml)
        {
            TextReader reader = new StringReader(xml);
            return Deserialize(reader);
        }

        public AWSEnvironments Deserialize(XmlDocument doc)
        {
            TextReader reader = new StringReader(doc.OuterXml);
            return Deserialize(reader);
        }

        public AWSEnvironments Deserialize(TextReader reader)
        {
            try
            {
                var awsEnvironments = (AWSEnvironments)serializer.Deserialize(reader);
                reader.Close();
                return awsEnvironments;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in Deserialize()");
            }

            return null;
        }

        public XmlDocument Serialize(AWSEnvironments awsEnvironments)
        {
            try
            {
                var xml = StringSerialize(awsEnvironments);
                var doc = new XmlDocument { PreserveWhitespace = true };
                doc.LoadXml(xml);
                doc = Clean(doc);
                return doc;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in Serialize()");
            }

            return null;
        }

        private string StringSerialize(AWSEnvironments awsEnvironments)
        {
            try
            {
                var textWriter = WriterSerialize(awsEnvironments);
                var xml = textWriter.ToString();
                textWriter.Close();
                return xml;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in StringSerialize()");
            }

            return string.Empty;
        }

        private TextWriter WriterSerialize(AWSEnvironments awsEnvironments)
        {
            try
            {
                TextWriter stringWriter = new StringWriter();
                serializer = new XmlSerializer(type);
                serializer.Serialize(stringWriter, awsEnvironments);
                stringWriter.Flush();
                return stringWriter;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in WriterSerialize()");
            }

            return null;
        }

        private static XmlDocument Clean(XmlDocument doc)
        {
            doc.RemoveChild(doc.FirstChild);
            var first = doc.FirstChild;
            foreach (var node in doc.ChildNodes.Cast<XmlNode>().Where(node => node.NodeType == XmlNodeType.Element))
            {
                first = node;
                break;
            }

            if (first.Attributes != null)
            {
                var a = first.Attributes["xmlns:xsd"];
                if (a != null)
                {
                    first.Attributes.Remove(a);
                }

                a = first.Attributes["xmlns:xsi"];
                if (a != null)
                {
                    first.Attributes.Remove(a);
                }
            }

            return doc;
        }

        public static AWSEnvironments ReadFile(string file)
        {
            var serializer = new AWSEnvironmentsSerializer();

            try
            {
                string xml;
                using (var reader = new StreamReader(file))
                {
                    xml = reader.ReadToEnd();
                    reader.Close();
                }

                return serializer.Deserialize(xml);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in ReadFile()");
            }

            return new AWSEnvironments();
        }

        public static bool WriteFile(string file, AWSEnvironments config)
        {
            var serializer = new AWSEnvironmentsSerializer();

            try
            {
                var xml = serializer.Serialize(config).OuterXml;
                using (var writer = new StreamWriter(file, false))
                {
                    writer.Write(xml);
                    writer.Flush();
                    writer.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in WriteFile()");
            }

            return false;
        }

        private void Initialize()
        {
            type = typeof(AWSEnvironments);
            serializer = new XmlSerializer(type);
        }
    }
}
