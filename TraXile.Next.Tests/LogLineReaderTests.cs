using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace TraXile.Next.Tests
{
    /// <summary>
    /// The log parser stores the byte offset of its resume line and seeks there on the next start.
    /// </summary>
    public class LogLineReaderTests
    {
        private static List<(string Line, long Offset)> ReadAll(TrX_LogLineReader reader)
        {
            var lines = new List<(string, long)>();
            while (reader.TryReadLine(out string line, out long offset))
            {
                lines.Add((line, offset));
            }
            return lines;
        }

        [Fact]
        public void Returns_lines_with_their_byte_offsets()
        {
            byte[] data = Encoding.UTF8.GetBytes("first\r\nsecond\nthird\r\n");
            var reader = new TrX_LogLineReader(new MemoryStream(data));

            Assert.Equal(new[] { ("first", 0L), ("second", 7L), ("third", 14L) }, ReadAll(reader));
            Assert.Equal(data.Length, reader.Position);
        }

        [Fact]
        public void Offsets_count_bytes_not_characters()
        {
            byte[] data = Encoding.UTF8.GetBytes("Grüße äöü\r\nnext\r\n");
            var reader = new TrX_LogLineReader(new MemoryStream(data));

            var lines = ReadAll(reader);
            Assert.Equal("Grüße äöü", lines[0].Line);
            Assert.Equal(Encoding.UTF8.GetByteCount("Grüße äöü\r\n"), lines[1].Offset);
        }

        [Fact]
        public void Seek_continues_at_a_stored_offset()
        {
            byte[] data = Encoding.UTF8.GetBytes("aaa\r\nbbb\r\nccc\r\n");
            var reader = new TrX_LogLineReader(new MemoryStream(data));
            ReadAll(reader);

            reader.Seek(5);
            Assert.Equal(new[] { ("bbb", 5L), ("ccc", 10L) }, ReadAll(reader));
        }

        [Fact]
        public void Skips_a_UTF8_byte_order_mark()
        {
            byte[] data = Encoding.UTF8.GetPreamble();
            data = Concat(data, Encoding.UTF8.GetBytes("line\r\n"));
            var reader = new TrX_LogLineReader(new MemoryStream(data));

            Assert.Equal(new[] { ("line", 0L) }, ReadAll(reader));
        }

        [Fact]
        public void Holds_back_an_unfinished_line_until_it_is_terminated()
        {
            var stream = new MemoryStream();
            var reader = new TrX_LogLineReader(stream);

            Append(stream, "done\r\nhalf");
            Assert.Equal(new[] { ("done", 0L) }, ReadAll(reader));

            Append(stream, " written\r\n");
            Assert.Equal(new[] { ("half written", 6L) }, ReadAll(reader));
        }

        [Fact]
        public void Handles_lines_longer_than_the_buffer()
        {
            string longLine = new string('x', 100);
            byte[] data = Encoding.UTF8.GetBytes($"short\r\n{longLine}\r\nend\r\n");
            var reader = new TrX_LogLineReader(new MemoryStream(data), bufferSize: 16);

            Assert.Equal(new[] { ("short", 0L), (longLine, 7L), ("end", 109L) }, ReadAll(reader));
        }

        private static void Append(MemoryStream stream, string text)
        {
            long position = stream.Position;
            stream.Seek(0, SeekOrigin.End);
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = position;
        }

        private static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length + b.Length];
            a.CopyTo(result, 0);
            b.CopyTo(result, a.Length);
            return result;
        }
    }
}
