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
        [Verb("proxy-server", HelpText = "Proxy server with optional data logging.")]
        public class ProxyServer
        {
            [Option('b', "bind", Required = true, HelpText = "End point to bind to, e.g. 0.0.0.0:5000.")]
            public string BindEndPoint { get; set; } = string.Empty;

            [Option('c', "connect", Required = true, HelpText = "End point to connect to, e.g. 192.168.1.1:5000.")]
            public string ConnectEndPoint { get; set; } = string.Empty;

            [Option('p', "protocol", Required = false, HelpText = "IP protocol. Either Tcp or Udp.")]
            public Protocol Protocol { get; set; } = Protocol.Tcp;

            [Option('f', "format", Required = false, HelpText = "Data format (for rendering in console/event log).")]
            public DataFormat Format { get; set; } = DataFormat.Binary;

            [Option('d', "display-data", Required = false, HelpText = "Write data to console.")]
            public bool DisplayData { get; set; } = false;

            [Option('e', "event-log-file", Required = false, HelpText = "Event log file path.")]
            public string EventLogFilePath { get; set; } = string.Empty;

            // Future expansion
            //[Option('o', "out-data-file", Required = false, HelpText = "Outbound data log file path.")]
            //public string OutboundDataFilePath { get; set; } = string.Empty;

            //[Option('i', "in-data-file", Required = false, HelpText = "Inbound data log file path.")]
            //public string InboundDataFilePath { get; set; } = string.Empty;
        }

        [Verb("echo-server", HelpText = "Server which echoes data back to client.")]
        public class EchoServer
        {
            [Option('b', "bind", Required = true, HelpText = "End point to bind to, e.g. 0.0.0.0:5000.")]
            public string BindEndPoint { get; set; } = string.Empty;

            [Option('p', "protocol", Required = false, HelpText = "IP protocol. Either Tcp or Udp.")]
            public Protocol Protocol { get; set; } = Protocol.Tcp;

            [Option('f', "format", Required = false, HelpText = "Data format (for rendering in console/event log).")]
            public DataFormat Format { get; set; } = DataFormat.Binary;

            [Option('d', "display-data", Required = false, HelpText = "Write data to console.")]
            public bool DisplayData { get; set; } = false;

            [Option('e', "event-log-file", Required = false, HelpText = "Event log file path.")]
            public string EventLogFilePath { get; set; } = string.Empty;

            // Future expansion
            //[Option('o', "out-data-file", Required = false, HelpText = "Outbound data log file path.")]
            //public string OutboundDataFilePath { get; set; } = string.Empty;

            //[Option('i', "in-data-file", Required = false, HelpText = "Inbound data log file path.")]
            //public string InboundDataFilePath { get; set; } = string.Empty;
        }

        [Verb("receive-client", HelpText = "Receive data from remote server.")]
        public class RecieveClient
        {
            [Option('c', "connect", Required = true, HelpText = "End point to connect to, e.g. 192.168.1.1:5000.")]
            public string ConnectEndPoint { get; set; } = string.Empty;

            [Option('p', "protocol", Required = false, HelpText = "IP protocol. Either Tcp or Udp.")]
            public Protocol Protocol { get; set; } = Protocol.Tcp;

            [Option('f', "format", Required = false, HelpText = "Data format (for rendering in console/event log).")]
            public DataFormat Format { get; set; } = DataFormat.Binary;

            [Option('d', "display-data", Required = false, HelpText = "Write data to console.")]
            public bool DisplayData { get; set; } = false;

            [Option('e', "event-log-file", Required = false, HelpText = "Event log file path.")]
            public string EventLogFilePath { get; set; } = string.Empty;

            // Future expansion
            //[Option('i', "in-data-file", Required = false, HelpText = "Inbound data log file path.")]
            //public string InboundDataFilePath { get; set; } = string.Empty;
        }
    }
}
