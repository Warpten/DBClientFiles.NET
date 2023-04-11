using DBClientFiles.NET.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream Limit(this Stream stream, long maxLength, bool disposing = true)
            => new LimitedStream(stream, maxLength, disposing);

        /// <summary>
        /// Creates a view of a given stream, setting the current position as the starting point.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="disposing">Whether or not disposing of the returned stream also disposes the provided stream.</param>
        /// <returns></returns>
        /// <remarks>This method will throw if the provided stream does not support seek operations. Use <see cref="Rebase(Stream, long, bool)"/> instead.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static Stream Rebase(this Stream stream, long offset, bool disposing = true)
            => new OffsetStream(stream, offset, disposing);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream MakeSeekable(this Stream stream)
        {
            if (stream.CanSeek)
                return stream;

            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream, 0x8000);
            // We took ownership of memory management, release the stream the user provided
            stream.Dispose();

            memoryStream.Position = 0;
            return memoryStream;
        }

        public static string ReadCString(this Stream stream)
        {
            var sb = new StringBuilder(128);
            int @char;
            while ((@char = stream.ReadByte()) != '\0')
                sb.Append((char) @char);

            return sb.ToString();
        }

        public static unsafe T Read<T>(this Stream dataStream) where T : struct
        {
            var value = default(T);
            var span = new Span<byte>(Unsafe.AsPointer(ref value), Unsafe.SizeOf<T>());
#if DEBUG
            Debug.Assert(Unsafe.SizeOf<T>() == dataStream.Read(span), $"Unable to read {typeof(T).Name} from stream, stream too short");
#else
            dataStream.Read(span);
#endif

            return value;
        }

        public static void Read<T>(this Stream dataStream, ref T value) where T : struct
        {
            var span = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));
#if DEBUG
            Debug.Assert(Unsafe.SizeOf<T>() == dataStream.Read(span), $"Unable to read {typeof(T).Name} from stream, stream too short");
#else
            dataStream.Read(span);
#endif
        }
    }
}
