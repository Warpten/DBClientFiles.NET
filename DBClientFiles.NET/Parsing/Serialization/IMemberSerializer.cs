using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Types;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.Serialization
{
    interface IMemberSerializer
    {
        ITypeMember MemberInfo { get; }

        void Visit(Expression recordReader, ref ExtendedMemberExpression rootExpression);

        IEnumerable<Expression> Output { get; }
    }
}
