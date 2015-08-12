using System.Collections.Generic;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace Document.AWS.Connect
{
    public enum States
    {
        Unknown = 0,
        Running,
        Stopped,
        Terminated
    }

    public enum Platforms
    {
        Unknown = 0,
        Linux,
        Windows,
        Aurora,
        MSSQL,
        MySql,
        Oracle,
        Postgres
    }

    public class AWSInstance
    {
        public string VPCID { get; set; }
        public string InstanceID { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string Role { get; set; }
        public bool IsRDS { get; set; }
        public bool Encrypted { get; set; }
        public bool PubliclyAccessible { get; set; }
        public InstanceType InstanceType { get; set; }
        public States State { get; set; }
        public Platforms Platform { get; set; }
        public List<string> PrivateIPs { get; set; }
        public List<string> PublicIPs { get; set; }
        public List<Volume> Volumes { get; set; }
        public List<string> SubnetIDs { get; set; }
        public List<string> SecurityGroupIDs { get; set; }

        public AWSInstance()
        {
            SecurityGroupIDs = new List<string>();
            PrivateIPs = new List<string>();
            PublicIPs = new List<string>();
            SubnetIDs = new List<string>();
            Volumes = new List<Volume>();
            PubliclyAccessible = false;
            IsRDS = false;
        }
    }
}
