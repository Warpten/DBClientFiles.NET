using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    internal abstract class TypedSerializerGenerator<T> : SerializerGenerator
    {
        public TypedSerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Debug.Assert(root == typeof(T));
        }

        public TMethod GenerateDeserializer<TMethod>() where TMethod : Delegate
        {
            var body = GenerateDeserializationMethodBody();
            
#if DEBUG && NETCOREAPP
            // Meh
            var header = string.Join(", ", Parameters.Select(p => string.Join(' ', p.Type.Name.Replace("`1", $"<{Instance.Type.Name}>"), p.Name)));
            Console.WriteLine($"({header}) => ");
            Console.Write(body.AsString());
#endif

            return Expression.Lambda<TMethod>(body, Parameters).Compile();
        }

        protected override TreeNode MakeRootNode()
        {
            return new TreeNode() {
                AccessExpression = Instance,
                MemberToken = null,
                TypeToken = Root
            };
        }

        protected override sealed Expression MakeRootMemberAccess(MemberToken token)
        {
            return token.MakeAccess(Instance);
        }

        protected override sealed Expression MakeReturnExpression()
        {
            return Instance;
        }

        protected abstract Expression RecordReader { get; }
        protected abstract Expression FileParser { get; }

        protected abstract Expression Instance { get; }
    }
}
