namespace Document.AWS
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using Amazon;
    using Amazon.EC2.Model;
    using Connect;
    using NLog;

    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static List<SecurityGroup> securityGroups;
        private static List<AWSSubnet> privateSubnets;
        private static List<AWSSubnet> publicSubnets;
        private static List<AWSSubnet> allSubnets;
        private static ServerIcons serverIcons;
        private static string stylesFile;

        public static void Main(string[] args)
        {
            var awsEnvironmentsFile = ConfigurationManager.AppSettings[Constants.AWSEnvironmentsFileKey];
            var serverIconsFile = ConfigurationManager.AppSettings[Constants.ServerIconsFileKey];
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            if (basePath != null && !Path.IsPathRooted(awsEnvironmentsFile))
            {
                awsEnvironmentsFile = Path.Combine(basePath, ConfigurationManager.AppSettings[Constants.AWSEnvironmentsFileKey]);
            }

            if (basePath != null && !Path.IsPathRooted(serverIconsFile))
            {
                serverIconsFile = Path.Combine(basePath, ConfigurationManager.AppSettings[Constants.ServerIconsFileKey]);
            }

            stylesFile = ConfigurationManager.AppSettings[Constants.StylesFileKey];
            serverIcons = LoadServerIcons(serverIconsFile);
            var awsEnvironments = LoadAWSEnvironments(awsEnvironmentsFile);
            if (awsEnvironments != null)
            {
                foreach (var awsEnvironment in awsEnvironments.List)
                {
                    BuildEnvironmentPage(awsEnvironment);
                }
            }
        }

        private static ServerIcons LoadServerIcons(string serverIconsFile)
        {
            try
            {
                ServerIcons tempServerIcons;
                var serverIconsSerializer = new ServerIconsSerializer();
                using (var textReader = File.OpenText(serverIconsFile))
                {
                    tempServerIcons = serverIconsSerializer.Deserialize(textReader);
                }

                return tempServerIcons;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in LoadServerIcons()");
            }

            return null;
        }

        private static AWSEnvironments LoadAWSEnvironments(string awsEnvironmentsFile)
        {
            try
            {
                AWSEnvironments awsEnvironments;
                var awsEnvironmentsSerializer = new AWSEnvironmentsSerializer();
                using (var textReader = File.OpenText(awsEnvironmentsFile))
                {
                    awsEnvironments = awsEnvironmentsSerializer.Deserialize(textReader);
                }

                return awsEnvironments;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in LoadAWSEnvironments()");
            }

            return null;
        }

        private static string LoadScriptsFile(string scriptsFile)
        {
            try
            {
                if (File.Exists(scriptsFile))
                {
                    return File.ReadAllText(scriptsFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in LoadScriptsFile()");
            }

            return string.Empty;
        }

        private static void BuildEnvironmentPage(AWSEnvironment environment)
        {
            try
            {
                if (!string.IsNullOrEmpty(environment.AccessKeyID) && !string.IsNullOrEmpty(environment.SecretAccessKey))
                {
                    foreach (var region in RegionEndpoint.EnumerableAllRegions)
                    {
                        if (string.Equals(region.SystemName, environment.Region))
                        {
                            var allInstances = new List<AWSInstance>();
                            var rdsInstances = RDS.GetAllRDSInstances(environment, region);
                            securityGroups = EC2.GetAllSecurityGroups(environment, region);
                            allSubnets = EC2.GetAllAWSSubnets(environment, region);
                            privateSubnets = SplitSubnets(false);
                            publicSubnets = SplitSubnets(true);
                            privateSubnets.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
                            publicSubnets.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
                            var ec2Instances = EC2.GetAllEC2Instances(environment, region);
                            allInstances.AddRange(ec2Instances);
                            allInstances.AddRange(rdsInstances);
                            if (allInstances.Count > 0)
                            {
                                allInstances.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
                                foreach (var server in allInstances)
                                {
                                    server.Icon = GetIconFromRole(server.Role);
                                }

                                BuildHTMLPage(environment, region, allInstances);
                            }
                        }

                    }
                }
                else
                {
                    Display.Error("AWS credentials are blank, unable to proceed with " + environment.Name);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in BuildEnvironmentPage()");
            }
        }

        private static List<AWSSubnet> SplitSubnets(bool isPublic)
        {
            return allSubnets.Where(subnet => subnet.Public == isPublic).ToList();
        }

        private static void BuildHTMLPage(AWSEnvironment environment, RegionEndpoint region, List<AWSInstance> servers)
        {
            try
            {
                var scriptsFile = ConfigurationManager.AppSettings[Constants.ScriptsFileKey];
                var htmlBody = string.Format(Constants.HTMLHeader, environment.Name, region.DisplayName) + Environment.NewLine + Constants.HTMLMeta + Environment.NewLine;
                htmlBody += string.Format(Constants.HTMLCSSLink, stylesFile) + Environment.NewLine;
                htmlBody += LoadScriptsFile(scriptsFile);
                htmlBody += Constants.HTMLHeaderEnd + Environment.NewLine + string.Format(Constants.InstancesHeaderLine, environment.Name, region.DisplayName) + Environment.NewLine + Constants.LineDivStartLine + Environment.NewLine + Constants.LegendDiv + Environment.NewLine;
                htmlBody += CreateHTMLForSubnets(publicSubnets, servers, true);
                htmlBody += CreateHTMLForSubnets(privateSubnets, servers, false);
                htmlBody += Constants.LineDivEndLine + Environment.NewLine + Constants.HTMLFooter + Environment.NewLine;
                File.WriteAllText(environment.HTMLPage, htmlBody);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in BuildHTMLPage()");
            }
        }

        private static string CreateHTMLForSubnets(IEnumerable<AWSSubnet> subnets, List<AWSInstance> servers, bool isPublic)
        {
            var result = string.Empty;
            foreach (var subnet in subnets)
            {
                var subnetServers = GetAllServersForSubnet(servers, subnet.SubnetId);
                if (subnetServers.Count > 0)
                {
                    var type = "private_subnet";
                    if (isPublic)
                    {
                        type = "public_subnet";
                    }

                    result += string.Format(Constants.SubnetDivStartLine, type) + Environment.NewLine;
                    result += string.Format(Constants.SubnetHeaderLine, subnet.Name, subnet.CidrBlock);
                    foreach (var server in subnetServers)
                    {
                        var divBox = GetDivBoxStateFromState(server.State);
                        result += string.Format(Constants.ServerDivStartLine, divBox) + Environment.NewLine;
                        result += GetServerIconAndTooltip(server);
                        if (!server.IsRDS)
                        {
                            result += string.Format(Constants.InfoDivLine, server.Name, server.InstanceType, server.Platform, server.State) + Environment.NewLine;
                        }
                        else
                        {
                            result += string.Format(Constants.InfoDivLineRDS, server.Name, server.InstanceType, server.Platform, server.Role.ToLower()) + Environment.NewLine;
                        }

                        result += GetServerDetails(server) + Environment.NewLine;
                        result += Constants.ServerDivEndLine + Environment.NewLine;
                    }

                    result += Constants.SubnetDivEndLine + Environment.NewLine;
                }
            }

            return result;
        }

        private static List<AWSInstance> GetAllServersForSubnet(List<AWSInstance> servers, string subnetID)
        {
            var subnetServers = new List<AWSInstance>();
            foreach (var server in servers)
            {
                foreach (var serverSubnetID in server.SubnetIDs)
                {
                    if (serverSubnetID == subnetID)
                    {
                        subnetServers.Add(server);
                    }
                }
            }

            return subnetServers;
        }

        private static string GetServerIconAndTooltip(AWSInstance ec2Instance)
        {
            var result = string.Empty;
            try
            {
                foreach (var securityGroupID in ec2Instance.SecurityGroupIDs)
                {
                    var securityGroup = GetSecurityGroupFromID(securityGroupID);
                    result = string.Format(Constants.IconDivLine, ec2Instance.Icon) + Environment.NewLine;
                    foreach (var rule in securityGroup.IpPermissions)
                    {
                        if (rule.IpRanges.Count > 0)
                        {
                            foreach (var ipRange in rule.IpRanges)
                            {
                                result += GetRangeText(rule, ipRange);
                            }
                        }
                        else
                        {
                            foreach (var secGroup in rule.UserIdGroupPairs)
                            {
                                result += GetRuleText(rule, secGroup.GroupId);
                            }
                        }
                    }
                }

                result += Constants.IconDivLineEnd + Environment.NewLine;
                return result;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in GetServerIconAndTooltip()");
            }

            return string.Empty;
        }

        private static string GetRangeText(IpPermission rule, string ipRange)
        {
            var rangeText = string.Empty;
            if (!string.IsNullOrEmpty(ipRange))
            {
                var prefix = string.Empty;
                if (rule.FromPort <= 0 && rule.ToPort <= 65535)
                {
                    prefix = "All ";
                }

                var from = ipRange;
                if (from == "0.0.0.0/0")
                {
                    from = "anywhere";
                }

                var to = "N/A";
                if (rule.ToPort >= 0)
                {
                    to = "port " + rule.ToPort;
                }

                if (rule.FromPort >= 0 && rule.FromPort < rule.ToPort)
                {
                    to = "ports " + rule.FromPort + "-" + rule.ToPort;
                }

                if ((rule.FromPort == 0 && rule.ToPort == 0) || (rule.FromPort == 0 && rule.ToPort == 65535))
                {
                    to = "all ports";
                }

                var protocol = rule.IpProtocol.ToUpper();
                if (rule.IpProtocol.Equals("-1"))
                {
                    protocol = "All protocol";
                }

                rangeText += prefix + protocol + " traffic to " + to + " from " + from + "." + Environment.NewLine;
            }

            return rangeText;
        }

        private static string GetRuleText(IpPermission rule, string securityGroupId)
        {
            var ruleText = string.Empty;
            if (rule != null)
            {
                var prefix = string.Empty;
                if (rule.FromPort <= 0 && rule.ToPort <= 65535)
                {
                    prefix = "All ";
                }

                var from = "Unknown";
                if (rule.UserIdGroupPairs.Count > 0)
                {
                    var group = GetSecurityGroupFromID(securityGroupId);
                    from = group.GroupName + " security group";
                }

                var to = "N/A";
                if (rule.ToPort >= 0)
                {
                    to = "port " + rule.ToPort;
                }

                if (rule.FromPort >= 0 && rule.FromPort < rule.ToPort)
                {
                    to = "ports " + rule.FromPort + "-" + rule.ToPort;
                }

                if ((rule.FromPort == 0 && rule.ToPort == 0) || (rule.FromPort == 0 && rule.ToPort == 65535))
                {
                    to = "all ports";
                }

                var protocol = rule.IpProtocol.ToUpper();
                if (rule.IpProtocol.Equals("-1"))
                {
                    protocol = "All protocol";
                }

                ruleText += prefix + protocol + " traffic to " + to + " from " + from + "." + Environment.NewLine;
            }

            return ruleText;
        }

        private static SecurityGroup GetSecurityGroupFromID(string securityGroupID)
        {
            return securityGroups.FirstOrDefault(securityGroup => securityGroup.GroupId == securityGroupID);
        }

        private static string GetSubnetNameFromID(string subnetID)
        {
            return allSubnets.Where(subnet => subnet.SubnetId == subnetID).Select(subnet => subnet.Name).FirstOrDefault();
        }

        private static string GetServerDetails(AWSInstance ec2Instance)
        {
            try
            {
                var details = "Desc: <b>No description found</b>" + Constants.LineBreak + Environment.NewLine;
                var owner = "Owner: <b>No owner found</b>" + Constants.LineBreak + Environment.NewLine;
                if (!ec2Instance.IsRDS)
                {
                    if (!string.IsNullOrEmpty(ec2Instance.Description))
                    {
                        details = "Desc: <b>" + ec2Instance.Description + "</b>" + Constants.LineBreak + Environment.NewLine;
                    }

                    if (!string.IsNullOrEmpty(ec2Instance.Owner))
                    {
                        owner = "Owner: " + ec2Instance.Owner + Constants.LineBreak + Environment.NewLine;
                    }
                }
                else
                {
                    details = "Desc: <b>RDS Instance</b>" + Constants.LineBreak + Environment.NewLine;
                    if (ec2Instance.PubliclyAccessible)
                    {
                        owner = "Public: <b>Yes</b>" + Constants.LineBreak + Environment.NewLine;
                    }
                    else
                    {
                        owner = "Public: <b>No</b>" + Constants.LineBreak + Environment.NewLine;
                    }

                    if (ec2Instance.Encrypted)
                    {
                        owner += "Encrypted: <b>Yes</b>" + Constants.LineBreak + Environment.NewLine;
                    }
                    else
                    {
                        owner += "Encrypted: <b>No</b>" + Constants.LineBreak + Environment.NewLine;
                    }
                }

                details += owner + "Subnet(s): " + Constants.LineBreak + Environment.NewLine;
                foreach (var subnetID in ec2Instance.SubnetIDs)
                {
                    details += GetSubnetNameFromID(subnetID) + "&nbsp;&nbsp(" + subnetID + ")" + Constants.LineBreak + Environment.NewLine;
                }

                details += "Security Group(s): " + Constants.LineBreak + Environment.NewLine;

                foreach (var securityGroupID in ec2Instance.SecurityGroupIDs)
                {
                    details += GetSecurityGroupFromID(securityGroupID).GroupName + Constants.LineBreak + Environment.NewLine;
                }

                if (ec2Instance.PrivateIPs.Count > 0)
                {
                    details += "Private IPs:" + Constants.LineBreak + Environment.NewLine;
                    details = ec2Instance.PrivateIPs.Aggregate(details, (current, privateIP) => current + (privateIP + Constants.LineBreak + Environment.NewLine));
                }

                if (ec2Instance.PublicIPs.Count > 0)
                {
                    details += "Public IPs:" + Constants.LineBreak + Environment.NewLine;
                    details = ec2Instance.PublicIPs.Aggregate(details, (current, publicIP) => current + (publicIP + Constants.LineBreak + Environment.NewLine));
                }

                if (ec2Instance.Volumes.Count > 0)
                {
                    details += "Volumes:" + Constants.LineBreak + Environment.NewLine;
                    foreach (var volume in ec2Instance.Volumes)
                    {
                        var device = volume.Attachments[0].Device;
                        if (volume.Encrypted)
                        {
                            details += device + "&nbsp;&nbsp;(" + volume.Size + "GB encrypted)" + Constants.LineBreak + Environment.NewLine;
                        }
                        else
                        {
                            details += device + "&nbsp;&nbsp;(" + volume.Size + "GB)" + Constants.LineBreak + Environment.NewLine;
                        }
                    }
                }

                details = string.Format(Constants.DetailsDivLine, details);
                details += Constants.DetailsDivEndLine;
                return details;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception in GetServerDetails()");
            }

            return string.Empty;
        }

        private static string GetDivBoxStateFromState(States state)
        {
            switch (state)
            {
                case States.Running:
                    return "box_on";
                case States.Stopped:
                    return "box_off";
                case States.Terminated:
                    return "box_term";
            }

            return "box_unknown";
        }

        private static string GetIconFromRole(string role)
        {
            if (!string.IsNullOrEmpty(role))
            {
                foreach (var serverIcon in serverIcons.List.Where(serverIcon => serverIcon.Role.ToLower().Equals(role.ToLower())))
                {
                    return serverIcon.Icon;
                }
            }

            return GetUnknownIcon();
        }

        private static string GetUnknownIcon()
        {
            foreach (var serverIcon in serverIcons.List.Where(serverIcon => serverIcon.Role.ToLower().Equals("unknown")))
            {
                return serverIcon.Icon;
            }

            return string.Empty;
        }
    }
}