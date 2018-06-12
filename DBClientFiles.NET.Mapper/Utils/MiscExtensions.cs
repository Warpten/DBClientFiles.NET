using System;
using System.Reflection;

namespace DBClientFiles.NET.Mapper.Utils
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
