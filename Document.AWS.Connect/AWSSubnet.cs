namespace Document.AWS.Connect
{
    using System.Linq;

    public class AWSSubnet
    {
        public string Name { get; set; }
        public string SubnetId { get; set; }
        public string CidrBlock { get; set; }
        public bool Public { get; set; }

        public AWSSubnet(Amazon.EC2.Model.Subnet subnet)
        {
            Name = "Unnamed";
            if (subnet != null)
            {
                foreach (var tag in subnet.Tags.Where(tag => tag.Key == "Name"))
                {
                    Name = tag.Value;
                }

                SubnetId = subnet.SubnetId;
                CidrBlock = subnet.CidrBlock;
            }
        }
    }
}
