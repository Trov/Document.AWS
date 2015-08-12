namespace Document.AWS.Connect
{
    using System.Collections.Generic;
    using System.Linq;
    using Amazon;
    using Amazon.EC2;
    using Amazon.EC2.Model;
    using NLog;

    public static class EC2
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static List<AWSSubnet> GetAllAWSSubnets(AWSEnvironment environment, RegionEndpoint region)
        {
            var subnets = new List<AWSSubnet>();
            try
            {
                var ec2Client = AWSClientFactory.CreateAmazonEC2Client(environment.AccessKeyID, environment.SecretAccessKey, region);
                var regionSubnets = ec2Client.DescribeSubnets();
                foreach (var subnet in regionSubnets.Subnets)
                {
                    subnets.Add(new AWSSubnet(subnet) { Public = IsSubnetPublic(ec2Client, subnet.SubnetId) });
                }
            }
            catch (AmazonEC2Exception aex)
            {
                Logger.Log(LogLevel.Error, aex, $"AmazonEC2Exception in GetAllAWSSubnets() : {aex.Message}");
            }

            return subnets;
        }

        private static bool IsSubnetPublic(IAmazonEC2 ec2Client, string subnetID)
        {
            try
            {
                var describeRouteTablesRequest = new DescribeRouteTablesRequest();
                var filter = new Filter { Name = "association.subnet-id" };
                filter.Values.Add(subnetID);
                describeRouteTablesRequest.Filters.Add(filter);
                var regionRoutes = ec2Client.DescribeRouteTables(describeRouteTablesRequest);
                if (regionRoutes.RouteTables.Any(routeTable => routeTable.Routes.Any(route => route.DestinationCidrBlock == "0.0.0.0/0" && !string.IsNullOrEmpty(route.GatewayId) && route.GatewayId.StartsWith("igw-"))))
                {
                    return true;
                }
            }
            catch (AmazonEC2Exception aex)
            {
                Logger.Log(LogLevel.Error, aex, $"AmazonEC2Exception in IsSubnetPublic() : {aex.Message}");
            }

            return false;
        }

        public static List<SecurityGroup> GetAllSecurityGroups(AWSEnvironment environment, RegionEndpoint region)
        {
            try
            {
                var ec2Client = AWSClientFactory.CreateAmazonEC2Client(environment.AccessKeyID, environment.SecretAccessKey, region);
                var regionSecurityGroups = ec2Client.DescribeSecurityGroups();
                return regionSecurityGroups.SecurityGroups;
            }
            catch (AmazonEC2Exception aex)
            {
                Logger.Log(LogLevel.Error, aex, $"AmazonEC2Exception in GetAllSecurityGroups() : {aex.Message}");
            }

            return null;
        }

        public static List<AWSInstance> GetAllEC2Instances(AWSEnvironment environment, RegionEndpoint region)
        {
            var servers = new List<AWSInstance>();

            try
            {
                var ec2Client = AWSClientFactory.CreateAmazonEC2Client(environment.AccessKeyID, environment.SecretAccessKey, region);
                var regionInstances = ec2Client.DescribeInstances();
                foreach (var reservations in regionInstances.Reservations)
                {
                    foreach (var instance in reservations.Instances)
                    {
                        var server = new AWSInstance
                        {
                            InstanceID = instance.InstanceId,
                            InstanceType = instance.InstanceType
                        };

                        foreach (var blockDevice in instance.BlockDeviceMappings)
                        {
                            server.Volumes.Add(GetInstanceVolumes(environment, region, blockDevice));
                        }

                        server.Platform = string.IsNullOrEmpty(instance.Platform) ? Platforms.Linux : Platforms.Windows;
                        foreach (var securityGroup in instance.SecurityGroups)
                        {
                            server.SecurityGroupIDs.Add(securityGroup.GroupId);
                        }

                        server.State = GetInstanceState(instance.State);
                        foreach (var tag in instance.Tags)
                        {
                            if (tag.Key == "Name")
                            {
                                server.Name = tag.Value;
                            }

                            if (tag.Key == "Description")
                            {
                                server.Description = tag.Value;
                            }

                            if (tag.Key == "Owner")
                            {
                                server.Owner = tag.Value;
                            }

                            if (tag.Key == "Role")
                            {
                                server.Role = tag.Value;
                            }
                        }

                        foreach (var networkInterface in instance.NetworkInterfaces)
                        {
                            foreach (var privateIP in networkInterface.PrivateIpAddresses)
                            {
                                if (!string.IsNullOrEmpty(privateIP.Association?.PublicIp))
                                {
                                    server.PublicIPs.Add(privateIP.Association.PublicIp);
                                }

                                server.PrivateIPs.Add(privateIP.PrivateIpAddress);
                            }
                        }

                        server.VPCID = instance.VpcId;
                        server.SubnetIDs.Add(instance.SubnetId);
                        servers.Add(server);
                    }
                }
            }
            catch (AmazonEC2Exception aex)
            {
                Logger.Log(LogLevel.Error, aex, $"AmazonEC2Exception in GetAllEC2Instances() : {aex.Message}");
            }

            return servers;
        }

        private static States GetInstanceState(InstanceState instanceState)
        {
            switch (instanceState.Name)
            {
                case "running":
                    return States.Running;
                case "stopped":
                case "stopping":
                    return States.Stopped;
                case "terminated":
                    return States.Terminated;
                default:
                    return States.Unknown;
            }
        }

        private static Volume GetInstanceVolumes(AWSEnvironment environment, RegionEndpoint region, InstanceBlockDeviceMapping blockDevice)
        {
            try
            {
                var ec2Client = AWSClientFactory.CreateAmazonEC2Client(environment.AccessKeyID, environment.SecretAccessKey, region);
                var regionVolumes = ec2Client.DescribeVolumes();
                foreach (var ec2Volume in regionVolumes.Volumes)
                {
                    if (ec2Volume.State == VolumeState.InUse && ec2Volume.VolumeId == blockDevice.Ebs.VolumeId)
                    {
                        return ec2Volume;
                    }
                }
            }
            catch (AmazonEC2Exception aex)
            {
                Logger.Log(LogLevel.Error, aex, $"AmazonEC2Exception in GetInstanceVolumes() : {aex.Message}");
            }

            return null;
        }
    }
}
