using NetUtil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil.Servers
{
    public class TcpEchoServerConfig
    {
        public IPEndPoint Bind { get; set; }
        public DataFormat Format { get; set; }
        public string EventLogFilePath { get; set; }
        public bool SendDataEventsToEventChannel { get; set; } = false;
    }
}
