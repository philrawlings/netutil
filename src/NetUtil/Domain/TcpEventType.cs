using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil.Domain
{
    public enum TcpEventType
    {
        Connected,
        OutboundData,
        InboundData,
        Disconnected,
        Error
    }
}
