using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil.Domain
{
    public class TcpEvent
    {
        public TcpEventType Type { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int ConnectionID { get; set; }
        public DateTime TimeStampUtc { get; set; }
        public byte[]? Data { get; set; }
    }
}
