using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipArchiveMaker
{
    /*
     * Copy from https://github.com/morkt/GARbro/blob/master/ArcFormats/CommonStreams.cs
     * (edited)
     */
    public class ProxyStream : Stream
    {
        Stream m_stream;
        bool m_should_dispose;

        public ProxyStream(Stream input, bool leave_open = false)
        {
            m_stream = input;
            m_should_dispose = !leave_open;
        }

        public Stream BaseStream { get { return m_stream; } }

        public override bool CanRead { get { return m_stream.CanRead; } }
        public override bool CanSeek { get { return m_stream.CanSeek; } }
        public override bool CanWrite { get { return m_stream.CanWrite; } }
        public override long Length { get { return m_stream.Length; } }
        public override long Position
        {
            get { return m_stream.Position; }
            set { m_stream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return m_stream.Read(buffer, offset, count);
        }

        public override void Flush()
        {
            m_stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_stream.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            m_stream.SetLength(length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_stream.Write(buffer, offset, count);
        }

        bool _proxy_disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!_proxy_disposed)
            {
                if (m_should_dispose && disposing)
                    m_stream.Dispose();
                _proxy_disposed = true;
                base.Dispose(disposing);
            }
        }
    }
    public class XoredStream : ProxyStream
    {
        private byte m_key;

        public XoredStream(Stream stream, byte key, bool leave_open = false)
            : base(stream, leave_open)
        {
            m_key = key;
        }

        #region System.IO.Stream methods
        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = BaseStream.Read(buffer, offset, count);
            for (int i = 0; i < read; ++i)
            {
                buffer[offset + i] ^= m_key;
            }
            return read;
        }

        public override int ReadByte()
        {
            int b = BaseStream.ReadByte();
            if (-1 != b)
            {
                b ^= m_key;
            }
            return b;
        }

        byte[] write_buf;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (null == write_buf)
                write_buf = new byte[81920];
            while (count > 0)
            {
                int chunk = Math.Min(write_buf.Length, count);
                for (int i = 0; i < chunk; ++i)
                {
                    write_buf[i] = (byte)(buffer[offset + i] ^ m_key);
                }
                BaseStream.Write(write_buf, 0, chunk);
                offset += chunk;
                count -= chunk;
            }
        }

        public override void WriteByte(byte value)
        {
            BaseStream.WriteByte((byte)(value ^ m_key));
        }
        #endregion
    }
}
