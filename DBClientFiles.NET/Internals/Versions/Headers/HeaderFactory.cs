using System;
using System.IO;
using System.Text;
using DBClientFiles.NET.Internals.Segments;

namespace DBClientFiles.NET.Internals.Versions.Headers
{
    internal static class HeaderFactory
    {
        public static IFileHeader ReadHeader(Stream dataStream)
        {
            using (var binaryReader = new BinaryReader(dataStream, Encoding.UTF8, true))
            {
                switch ((Signatures)binaryReader.ReadInt32())
                {
                    case Signatures.WDBC: return new WDBC(binaryReader);
                    case Signatures.WDB2: return new WDB2(binaryReader);
                    case Signatures.WDB3:
                    case Signatures.WDB4:
                        break;
                    case Signatures.WDB5: return new WDB5(binaryReader);
                    case Signatures.WDB6: return new WDB6(binaryReader);
                    case Signatures.WDC1: return new WDC1(binaryReader);
                    case Signatures.WDC2: return new WDC2(binaryReader);
                }
            }

            throw new InvalidOperationException();
        }
    }
}
