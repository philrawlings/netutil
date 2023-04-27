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

        public string GetFormattedDataString(DataFormat format)
        {
            if (Data is null || Data.Length == 0)
                return string.Empty;

            switch (format)
            {
                case DataFormat.Binary:
                    return BitConverter.ToString(Data);
                case DataFormat.AsciiText:
                    StringBuilder sb = new StringBuilder(Data.Length);
                    foreach (var val in Data)
                    {
                        if (val >= 0x20 && val < 0x7F)
                        {
                            sb.Append((char)val);
                        }
                        else
                        {
                            sb.Append($"[{val:x2}]");
                        }
                    }
                    return sb.ToString();
                case DataFormat.Utf8Text:
                    return Encoding.UTF8.GetString(Data); // TODO, need improvement
                default:
                    throw new NotSupportedException($"Data format {format} is not currently supported.");
            }
        }
    }
}
