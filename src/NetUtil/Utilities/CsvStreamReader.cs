using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetUtil.Utilities
{
    public class CsvStreamReader : IDisposable
    {
        StreamReader sr;
        bool leaveStreamOpen = false;

        /// <summary>
        /// Encoding extracted from byte order mark otherwise the file
        /// </summary>
        public CsvStreamReader(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            sr = new StreamReader(fs, true);
        }

        /// <summary>
        /// Encoding extracted from byte order mark otherwise the file
        /// </summary>
        public CsvStreamReader(Stream stream, bool leaveOpen = false)
        {
            sr = new StreamReader(stream, true);
            leaveStreamOpen = leaveOpen;
        }

        public CsvStreamReader(string path, Encoding encoding)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            sr = new StreamReader(fs, encoding);
        }

        public CsvStreamReader(Stream stream, Encoding encoding, bool leaveOpen = false)
        {
            sr = new StreamReader(stream, encoding);
            leaveStreamOpen = leaveOpen;
        }

        public string[] RowData { get; private set; } = null;
        public int LineNumber { get; private set; } = 0;

        public bool ReadRow()
        {
            string line = sr.ReadLine();
            if (line == null)
                return false;

            line = line.Trim();

            // If line ends with a comma, add a dummy quoted element as this is a valid empty field
            if (line.EndsWith(","))
                line = line + "\"\"";

            int splitPos = 0;
            int pos = 0;
            List<string> items = new List<string>();

            bool quoted = false;
            char[] chars = line.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                pos = i;
                if (chars[i] == '"')
                    quoted = !quoted;

                if (chars[i] == ',' && !quoted)
                {
                    // Add element prior to current comma
                    splitPos = AddString(line, splitPos, pos, items);
                }
            }
            if (pos != 0)
            {
                // Add Last Element
                AddString(line, splitPos, pos, items);
            }

            RowData = items.ToArray();
            LineNumber++;

            return true;
        }

        private static int AddString(string line, int startPos, int currentPos, List<string> items)
        {
            string item;
            int start;
            int length;

            if (startPos == 0)
                start = startPos;
            else
                start = startPos + 1;

            if (startPos == 0)
                length = currentPos - startPos; // First element
            else if (currentPos >= line.Length - 1)
                length = currentPos - startPos; // Last element
            else
                length = currentPos - startPos - 1; // Other element

            item = line.Substring(start, length).Trim();

            if (item == "\"\"")
            {
                item = "";
            }
            else if (item.Length > 2)
            {
                // Remove outer quotes if they exist
                if (item[0] == '"' && item[item.Length - 1] == '"')
                    item = item.Substring(1, item.Length - 2);
            }

            if (item != ",")
                items.Add(item);
            else
                items.Add("");
            startPos = currentPos;

            return startPos;
        }

        public void Dispose()
        {
            if (sr != null && !leaveStreamOpen)
                sr.Close();
        }

        public void Close()
        {
            Dispose();
        }
    }
}
