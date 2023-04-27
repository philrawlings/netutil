using NetUtil.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil.Servers
{
    public class TcpClientConfig
    {
        public IPEndPoint Connect { get; set; }
        public bool IncludeDataEvents { get; set; } = false;
    }
}
