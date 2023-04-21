using CommandLine;
using NetUtil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil
{
    internal class Options
    {
        [Verb("proxy", HelpText = "Proxy server with optional data logging.")]
        public class Proxy
        {
            [Option('b', "bind", Required = true, HelpText = "Socket to bind to, e.g. 0.0.0.0:5000.")]
            public string Bind { get; set; } = string.Empty;

            [Option('d', "destination", Required = true, HelpText = "Socket to bind to, e.g. 0.0.0.0:5000.")]
            public string Destination { get; set; } = string.Empty;

            [Option('p', "protocol", Required = false, HelpText = "IP protocol. Either Tcp or Udp.")]
            public Protocol Protocol { get; set; } = Protocol.Tcp;

            [Option('f', "format", Required = false, HelpText = "Data format")]
            public DataFormat Format { get; set; } = DataFormat.Binary;

            [Option('c', "console", Required = false, HelpText = "Show data in console")]
            public bool WriteDataToConsole { get; set; } = false;

            [Option('o', "out-file", Required = false, HelpText = "Outbound data log file path")]
            public string OutboundLogFilePath { get; set; } = string.Empty;

            [Option('i', "in-file", Required = false, HelpText = "Inbound data log file path")]
            public string InputLogFilePath { get; set; } = string.Empty;
        }
    }
}
