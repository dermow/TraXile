using System;
using System.IO;
using System.Text;

namespace TraXile
{
    /// <summary>
    /// Reads Client.txt line by line and knows the byte offset of every line, so the log parser can store
    /// its resume point as a file position and seek there on the next start instead of scanning the whole file.
    /// </summary>
    /// <remarks>
    /// Only lines terminated by '\n' are returned; a trailing "\r" is removed. A line the game has not finished
    /// writing yet stays buffered until its terminator arrives, so it is never returned in two parts.
    /// A UTF-8 byte order mark at the start of the file is skipped, like StreamReader does.
    /// </remarks>
    public sealed class TrX_LogLineReader : IDisposable
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false, false);

        private readonly Stream _stream;
        private byte[] _buffer;

        // Unconsumed data is _buffer[_start.._end)
        private int _start;
        private int _end;

        // File offset of _buffer[0]
        private long _bufferOffset;

        public TrX_LogLineReader(Stream stream, int bufferSize = 64 * 1024)
        {
            _stream = stream;
            _buffer = new byte[Math.Max(16, bufferSize)];
            _bufferOffset = stream.Position;
        }

        /// <summary>
        /// Byte offset of the next line to be read.
        /// </summary>
        public long Position => _bufferOffset + _start;

        /// <summary>
        /// Continue reading at the given byte offset, which must be the start of a line.
        /// </summary>
        public void Seek(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
            _bufferOffset = offset;
            _start = 0;
            _end = 0;
        }

        /// <summary>
        /// Reads the next complete line. Returns false at the end of the file (or of the complete lines in it);
        /// calling it again later returns lines appended in the meantime.
        /// </summary>
        /// <param name="line">The line without its terminator</param>
        /// <param name="lineStart">Byte offset of the line in the file</param>
        public bool TryReadLine(out string line, out long lineStart)
        {
            int scanFrom = _start;

            while (true)
            {
                int newline = Array.IndexOf(_buffer, (byte)'\n', scanFrom, _end - scanFrom);
                if (newline >= 0)
                {
                    lineStart = Position;
                    int first = _start;
                    int length = newline - _start;

                    if (length > 0 && _buffer[newline - 1] == (byte)'\r')
                    {
                        length--;
                    }

                    if (lineStart == 0 && length >= 3 && _buffer[first] == 0xEF && _buffer[first + 1] == 0xBB && _buffer[first + 2] == 0xBF)
                    {
                        first += 3;
                        length -= 3;
                    }

                    line = Utf8.GetString(_buffer, first, length);
                    _start = newline + 1;
                    return true;
                }

                // No terminator in the buffered data: make room and read more
                if (_start > 0)
                {
                    int pending = _end - _start;
                    Buffer.BlockCopy(_buffer, _start, _buffer, 0, pending);
                    _bufferOffset += _start;
                    _start = 0;
                    _end = pending;
                }
                else if (_end == _buffer.Length)
                {
                    Array.Resize(ref _buffer, _buffer.Length * 2);
                }

                scanFrom = _end;
                int read = _stream.Read(_buffer, _end, _buffer.Length - _end);
                if (read <= 0)
                {
                    line = null;
                    lineStart = Position;
                    return false;
                }
                _end += read;
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
