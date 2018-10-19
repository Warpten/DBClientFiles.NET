using System;

namespace DBClientFiles.NET.Utils
{
    internal static class TypeExtensions
    {
        /// <remarks>
        /// Shamelessly stolen from <a href="http://geekswithblogs.net/mrsteve/archive/2012/01/11/csharp-expression-trees-create-instance-from-type-extension-method.aspx">Steve Wilkes</a>.
        /// </remarks>
        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool IsSigned(this Type t)
        {
            if (t == typeof(int))
                return true;

            if (t == typeof(short))
                return true;

            if (t == typeof(sbyte))
                return true;

            if (t == typeof(float))
                return true;

            return false;
        }
    }
}
