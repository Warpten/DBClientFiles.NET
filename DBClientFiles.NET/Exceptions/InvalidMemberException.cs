using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Exceptions
{
    public class InvalidMemberException : Exception
    {
        public override string Message { get; }

        public InvalidMemberException(MemberInfo targetMember, string fmt, params object[] args)
        {
            var @params = new object[args.Length + 2];
            @params[0] = targetMember.Name;
            @params[1] = targetMember.DeclaringType?.FullName ?? "[Unknown type]";
            Array.Copy(args, 0, @params, 2, args.Length);

            Message = string.Format(fmt, @params);
        }
    }
}
