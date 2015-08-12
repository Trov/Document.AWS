namespace Document.AWS
{
    public static class Constants
    {
        public static string AWSEnvironmentsFileKey = "AWSEnvironmentsFile";
        public static string ServerIconsFileKey = "ServerIconsFile";
        public static string ScriptsFileKey = "ScriptsFile";
        public static string StylesFileKey = "StylesFile";
        public static string HTMLHeader = "<html><head><title>{0} Instances - {1}</title>";
        public static string HTMLMeta = @"   <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
   <meta http-equiv=""refresh"" content=""60"" />";
        public static string HTMLCSSLink = "<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\">";
        public static string HTMLHeaderEnd = @"</head>
<body>";
        public static string LineDivStartLine = "<div id=\"wrap\">";
        public static string LegendDiv = @"  <div id=""wrap_legend"">
    <h3>&nbsp;Color Legend</h3>
    <div id=""legend_on"" align=""center"">
        <h3>Running</h3>
    </div>
    <div id=""legend_off"" align=""center"">
        <h3>Stopped</h3>
    </div>
    <div id=""legend_term"" align=""center"">
        <h3>Terminated</h3>
    </div>
    <div id=""legend_unknown"" align=""center"">
        <h3>Unknown</h3>
    </div>
  </div>";
        public static string SubnetDivStartLine = "  <div id=\"{0}\">";
        public static string SubnetHeaderLine = "  <h3>&nbsp;{0}&nbsp;&nbsp;({1})</h3>";
        public static string InstancesHeaderLine = "<h2>{0} Instances - {1}</h2>";
        public static string ServerDivStartLine = "    <div id=\"{0}\">";
        public static string IconDivLine = "        <div id=\"icon\"><img src=\"{0}\" height=\"105\" width=\"105\" title=\"Inbound Rule(s):";
        public static string IconDivLineEnd = @"""/>
    </div>";
        public static string InfoDivLineRDS = "        <div id=\"info\"><b>{0}</b><br />Size: <b>{1}</b><br />Platform: <b>{2}</b><br />Type: <b>{3}</b></div>";
        public static string InfoDivLine = "        <div id=\"info\"><b>{0}</b><br />Size: <b>{1}</b><br />Platform: <b>{2}</b><br />State: <b>{3}</b></div>";
        public static string DetailsDivLine = "        <div id=\"details\">{0}";
        public static string DetailsDivEndLine = "        </div>";
        public static string ServerDivEndLine = "    </div>";
        public static string LineDivEndLine = "</div>";
        public static string SubnetDivEndLine = "  </div>";
        public static string LineBreak = "<br />";
        public static string HTMLFooter = @"</body>
</html>";
    }
}
