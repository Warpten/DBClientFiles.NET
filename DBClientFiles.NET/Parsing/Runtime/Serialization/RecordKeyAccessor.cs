using System;
using DBClientFiles.NET.Parsing.Reflection;

using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Runtime.Serialization
{
    internal readonly struct RecordKeyAccessor<T>
    {
        public delegate int TypeKeyGetter(in T source);
        public delegate void TypeKeySetter(out T source, int key);

        private readonly Lazy<TypeKeyGetter> _keyGetter;
        private readonly Lazy<TypeKeySetter> _keySetter;

        public TypeKeyGetter GetRecordKey => _keyGetter.Value;
        public TypeKeySetter SetRecordKey => _keySetter.Value;

        public RecordKeyAccessor(TypeToken type, int indexColumn, TypeTokenType tokenType)
        {
            var rootExpression = Expr.Parameter(typeof(T).MakeByRefType(), "instance");

            var (indexColumnMemberToken, memberAccess) = type.MakeMemberAccess(ref indexColumn, rootExpression, tokenType);
            if (indexColumnMemberToken == null)
                throw new InvalidOperationException($"Invalid structure: Unable to find an index column.");

            if (indexColumnMemberToken.TypeToken != typeof(int) && indexColumnMemberToken.TypeToken != typeof(uint))
                throw new InvalidOperationException($"Invalid structure: {memberAccess} is expected to be the index, but its type doesn't match. Needs to be (u)int.");

            _keyGetter = new Lazy<TypeKeyGetter>(() =>
            {
                return Expr.Lambda<TypeKeyGetter>(
                    // Box to int if type mismatches
                    memberAccess.Type == typeof(int)
                        ? memberAccess
                        : Expr.ConvertChecked(memberAccess, typeof(int)),
                    rootExpression).Compile();
            });

            { /* key setter */
                var paramValue = Expr.Parameter(typeof(int));

                _keySetter = new Lazy<TypeKeySetter>(() =>
                {
                    return Expr.Lambda<TypeKeySetter>(
                        Expr.Assign(memberAccess,
                            // Box to target type if not int
                            memberAccess.Type == typeof(int)
                                ? memberAccess
                                : Expr.ConvertChecked(paramValue, memberAccess.Type)
                    ), rootExpression, paramValue).Compile();
                });
            }
        }
}
}
