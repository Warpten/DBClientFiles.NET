using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    internal abstract class TypedSerializerGenerator<T, TMethod> : SerializerGenerator where TMethod : Delegate
    {
        public TypedSerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Debug.Assert(root == typeof(T));
        }

        protected override void PrepareMethodParameters()
        {
            var methodType = typeof(TMethod).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (methodType != null)
            {
                var methodParams = methodType.GetParameters();
                foreach (var methodParam in methodParams)
                    Parameters.Add(Expression.Parameter(methodParam.ParameterType, methodParam.Name));
            }
        }

        public TMethod GenerateDeserializer()
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

        protected Expression RecordReader => Parameters[0];
        protected Expression FileParser => Parameters[1];

        private Expression Instance => Parameters[2];
    }
}
