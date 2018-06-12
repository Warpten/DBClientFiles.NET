using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.AutoMapper.Utils
{
    internal static class MiscExtensions
    {
        public static Type GetMemberType(this MemberInfo member)
        {
            if (member is PropertyInfo prop)
                return prop.PropertyType;

            if (member is FieldInfo field)
                return field.FieldType;

            if (member is EventInfo ev)
                return ev.EventHandlerType;

            return null;
        }
    }
}
