using DBClientFiles.NET.Parsing.Binding;
using DBClientFiles.NET.Parsing.Types;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DBClientFiles.NET.Parsing.Serialization
{
    interface IMemberSerializer
    {
        ITypeMember MemberInfo { get; }

        IEnumerable<Expression> Visit(Expression recordReader, ExtendedMemberExpression rootExpression);
    }
}
