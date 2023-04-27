using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetUtil.Utilities
{
    public class CsvStreamWriter : IDisposable
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Task? flushTask = null;
        private readonly object lockObj = new object();
        private readonly bool leaveStreamOpen = false;
        private readonly Stream stream;
        private StringBuilder rowData;
        private bool rowStarted = false;
        private int columnsWritten = 0;

        public CsvStreamWriter(string path, bool writeBOM = true)
        {
            stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            Initialize();
            if (writeBOM)
                WriteBOM();
        }

        public CsvStreamWriter(Stream stream, bool writeBOM = true, bool leaveOpen = false)
        {
            this.stream = stream;
            leaveStreamOpen = leaveOpen;
            Initialize();
            if (writeBOM)
                WriteBOM();
        }

        private void Initialize()
        {
            flushTask = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(1000, cts.Token);
                        lock (lockObj)
                        {
                            stream.Flush();
                        }
                    }
                }
                catch { }
            });
        }

        private void WriteBOM()
        {
            // Byte order marker
            byte[] bomBuffer = new byte[3];
            bomBuffer[0] = 0xEF;
            bomBuffer[1] = 0xBB;
            bomBuffer[2] = 0xBF;
            stream.Write(bomBuffer, 0, 3);
        }

        public void WriteEntireRow(params object[] values)
        {
            lock (lockObj)
            {
                if (rowStarted)
                    throw new InvalidOperationException("Cannot write entire row, the previous row has not ended");

                StartRow();
                foreach (object value in values)
                {
                    // Invoke specific formatting for certain types
                    if (value is bool boolValue)
                        WriteColumnValue(boolValue);
                    else if (value is string stringValue)
                        WriteColumnValue(stringValue);
                    else if (value is DateTime dateTimeValue)
                        WriteColumnValue(dateTimeValue);
                    else
                        WriteColumnValue(value);
                }
                EndRow();
            }
        }

        public void WriteEmptyRow()
        {
            StartRow();
            EndRow();
        }

        public void StartRow()
        {
            lock (lockObj)
            {
                if (rowStarted)
                    throw new InvalidOperationException("Cannot start row, the row has already been started");

                rowData = new StringBuilder();
                rowStarted = true;
                columnsWritten = 0;
            }
        }

        public void WriteRawString(string value)
        {
            byte[] rowBuffer = Encoding.UTF8.GetBytes(value);
            stream.Write(rowBuffer, 0, rowBuffer.Length);
        }

        public void WriteColumnValue(bool value)
        {
            lock (lockObj)
            {
                if (!rowStarted)
                    throw new InvalidOperationException("Cannot write column data, the row has not been started");

                if (columnsWritten != 0)
                {
                    rowData.Append(",");
                }
                rowData.Append(value.ToString().ToLower());
                columnsWritten++;
            }
        }

        public void WriteColumnValue(string value)
        {
            lock (lockObj)
            {
                if (!rowStarted)
                    throw new InvalidOperationException("Cannot write column data, the row has not been started");

                if (columnsWritten != 0)
                {
                    rowData.Append(",");
                }
                if (value == null)
                {
                    rowData.Append(string.Empty);
                }
                else
                {
                    rowData.Append(CsvEscape(value));
                }
                columnsWritten++;
            }
        }

        public void WriteColumnValue(DateTime value)
        {
            lock (lockObj)
            {
                if (!rowStarted)
                    throw new InvalidOperationException("Cannot write column data, the row has not been started");

                if (columnsWritten != 0)
                {
                    rowData.Append(",");
                }
                string strValue = GetDateTimeString(value);
                rowData.Append(CsvEscape(strValue));
                columnsWritten++;
            }
        }

        public void WriteColumnValue(object value)
        {
            lock (lockObj)
            {
                if (!rowStarted)
                    throw new InvalidOperationException("Cannot write column data, the row has not been started");

                if (columnsWritten != 0)
                {
                    rowData.Append(",");
                }

                if (value == null)
                {
                    rowData.Append(string.Empty);
                }
                else
                {
                    string strValue = strValue = value.ToString();
                    rowData.Append(CsvEscape(strValue));
                }
                columnsWritten++;
            }
        }

        public void EndRow()
        {
            lock (lockObj)
            {
                if (!rowStarted)
                    throw new InvalidOperationException("Cannot end row, the row has not been started");

                rowData.Append(Environment.NewLine);
                byte[] rowBuffer = Encoding.UTF8.GetBytes(rowData.ToString());
                stream.Write(rowBuffer, 0, rowBuffer.Length);
                rowStarted = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (stream != null && !leaveStreamOpen)
            {
                cts.Cancel();
                flushTask?.Wait();
                stream.Close();
            }
        }

        public void Close()
        {
            Dispose();
        }

        private static string GetDateTimeString(DateTime dateTime)
        {
            //string iso8601dt = dateTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture); // Does not include milliseconds
            //string iso8601dt = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc); // 7 fractional digits for seconds - python can only parse 6 digits (microseconds)
            string iso8601dt = dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            return iso8601dt;
        }

        private static string CsvEscape(string str)
        {
            if (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }
    }
}
