using System.Collections.Generic;
using Amazon;
using Amazon.EC2;
using NLog;

namespace Document.AWS.Connect
{
    public static class RDS
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static List<AWSInstance> GetAllRDSInstances(AWSEnvironment environment, RegionEndpoint region)
        {
            var dbInstances = new List<AWSInstance>();

            try
            {
                var rdsClient = AWSClientFactory.CreateAmazonRDSClient(environment.AccessKeyID, environment.SecretAccessKey, region);
                var rdsInstances = rdsClient.DescribeDBInstances();
                foreach (var dbInstance in rdsInstances.DBInstances)
                {
                    var rdsInstance = new AWSInstance
                    {
                        Role = dbInstance.Engine.ToUpper(),
                        Name = dbInstance.DBInstanceIdentifier,
                        InstanceType = dbInstance.DBInstanceClass,
                        PubliclyAccessible = dbInstance.PubliclyAccessible,
                        Encrypted = dbInstance.StorageEncrypted,
                        Platform = Platforms.Linux,
                        State = States.Running,
                        IsRDS = true
                    };

                    if (rdsInstance.Role == "MSSQL")
                    {
                        rdsInstance.Platform = Platforms.Windows;
                    }

                    foreach (var subnet in dbInstance.DBSubnetGroup.Subnets)
                    {
                        rdsInstance.SubnetIDs.Add(subnet.SubnetIdentifier);
                    }

                    foreach (var vpcSecurityGroup in dbInstance.VpcSecurityGroups)
                    {
                        rdsInstance.SecurityGroupIDs.Add(vpcSecurityGroup.VpcSecurityGroupId);
                    }

                    dbInstances.Add(rdsInstance);
                }
            }
            catch (AmazonEC2Exception aex)
            {
                Logger.Log(LogLevel.Error, aex, $"AmazonEC2Exception in GetAllRDSInstances() : {aex.Message}");
            }

            return dbInstances;
        }
    }
}
