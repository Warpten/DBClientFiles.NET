using DBClientFiles.NET.IO;
using System.IO;
using System.Text;

namespace DBClientFiles.NET.Utils.Extensions
{
    internal static class StreamExtensions
    {
        /// <summary>
        /// Creates a view of given length into the provided stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="maxLength">The maximum length of the stream returned.</param>
        /// <param name="disposing">Whether or not disposing of the returned stream also disposes the provided stream.</param>
        /// <returns></returns>
        public static Stream Limit(this Stream stream, long maxLength, bool disposing = true)
            => new LimitedStream(stream, maxLength, disposing);

        /// <summary>
        /// Creates a view of a given stream, setting the current position as the starting point.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="disposing">Whether or not disposing of the returned stream also disposes the provided stream.</param>
        /// <returns></returns>
        /// <remarks>This method will throw if the provided stream does not support seek operations. Use <see cref="Rebase(Stream, int, bool)"/> instead.</remarks>
        public static Stream Rebase(this Stream stream, bool disposing = true)
            => Rebase(stream, stream.Position, disposing);

        /// <summary>
        /// Creates a view of a given stream, setting the current position as the starting point.
        /// <br/><br/>
        /// Use this overload when the stream being operated on
        /// does not provide seek operations.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset">What would usually be returned from <see cref="Stream.Position"/> if <see cref="Stream.CanSeek"/> returned <code>true</code>.</param>
        /// <param name="disposing">Whether or not disposing of the returned stream also disposes the provided stream.</param>
        /// <returns></returns>
        public static Stream Rebase(this Stream stream, long offset, bool disposing = true)
            => new OffsetStream(stream, offset, disposing);

        public static string ReadCString(this Stream stream)
        {
            var sb = new StringBuilder(128);
            int @char;
            while ((@char = stream.ReadByte()) != '\0')
                sb.Append((char) @char);

            return sb.ToString();
        }
    }
}
