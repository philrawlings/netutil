using NetUtil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil.Servers
{
    internal class TcpProxyServerConfig
    {
        public IPEndPoint Bind { get; set; }
        public IPEndPoint Destination { get; set; }
        public bool IncludeDataEvents { get; set; } = false;
    }
}
