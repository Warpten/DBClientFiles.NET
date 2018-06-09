using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.AutoMapper.Utils
{
    internal static class MiscExtensions
    {
        public static float ReinterpretFloat(this int value)
        {
            ftou f = value;
            return f.Single;
        }

        public static float ReinterpretFloat(this uint value)
        {
            ftou f = value;
            return f.Single;
        }

        public static int ReinterpretInt(this float f)
        {
            ftou i = f;
            return i.Int32;
        }

        public static int ReinterpretInt(this uint f)
        {
            return (int)f;
        }

        public static uint ReinterpretUInt(this float f)
        {
            ftou i = f;
            return i.UInt32;
        }

        public static uint ReinterpretUInt(this int f)
        {
            return (uint)f;
        }

        [StructLayout(LayoutKind.Explicit, Size = 4)]
        private struct ftou
        {
            [FieldOffset(0)] public int Int32;
            [FieldOffset(0)] public uint UInt32;
            [FieldOffset(0)] public float Single;

            public static implicit operator ftou(int i)
            {
                return new ftou { Int32 = i };
            }

            public static implicit operator ftou(uint i)
            {
                return new ftou { UInt32 = i };
            }

            public static implicit operator ftou(float i)
            {
                return new ftou { Single = i };
            }

            public static implicit operator int(ftou ftou) => ftou.Int32;
            public static implicit operator uint(ftou ftou) => ftou.UInt32;
            public static implicit operator float(ftou ftou) => ftou.Single;
        }
    }
}
