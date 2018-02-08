using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Blackduck.Hub
{
    class Options
    {
        [Value(0, Required = true, HelpText = "Assembly to be scanned", MetaName = "Target assembly")]
        public string AssemblyToScan { get; set; }

        [Option(shortName: 'o', longName: "outputFile", Min = 1, Max = 1, HelpText = "The path of the file to be created for manual upload to the hub. Required when hub settings are not configured in scanner.ini", Hidden = false, Required = false)]
        public IEnumerable<string> OutputFile { get; set; } = new List<string>();

        [Option(shortName: 'i', longName: "ignoreSslErrors", HelpText = "If set to true, all SSL certificate validation errors will be ignored.", Default = false, Hidden = false)]
        public bool IgnoreSslErrors { get; set; }


    }
}
