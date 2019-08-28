using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using DBClientFiles.NET.Parsing.Reflection;
using DBClientFiles.NET.Utils.Expressions.Extensions;

namespace DBClientFiles.NET.Parsing.Serialization.Generators
{
    /// <summary>
    /// The base class in charge of generating deserialization methods for a given <see cref="{T}"/>.
    /// </summary>
    /// <typeparam name="T">The record type for which a deserializer must be generated.</typeparam>
    internal abstract class TypedSerializerGenerator<T> : SerializerGenerator
    {
        public TypedSerializerGenerator(TypeToken root, TypeTokenType memberType) : base(root, memberType)
        {
            Debug.Assert(root == typeof(T));
        }

        protected TMethod GenerateDeserializer<TMethod>() where TMethod : Delegate
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

        protected abstract Expression Instance { get; }
    }

    /// <summary>
    /// The base class in charge of generating deserialization methods for a given <see cref="{T}"/>.
    /// </summary>
    /// <typeparam name="T">The record type for which a deserializer must be generated.</typeparam>
    /// <typeparam name="TGenerationState">A state object that is used when generating reader calls.</typeparam>
    internal abstract class TypedSerializerGenerator<T, TGenerationState> : TypedSerializerGenerator<T>
    {
        protected TGenerationState State { get; set; }

        public TypedSerializerGenerator(TypeToken root, TypeTokenType memberType, TGenerationState state) : base(root, memberType)
        {
            Debug.Assert(root == typeof(T));

            State = state;
        }
    }
}
