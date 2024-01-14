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

<<<<<<< HEAD
        private readonly TypeToken _recordTypeToken;
        private readonly TypeTokenType _indexMemberTypeTokenType;
        private readonly int _indexColumn;

        public RecordKeyAccessor(TypeToken type, int indexColumn, TypeTokenType tokenType)
=======
        public RecordKeyAccessor(TypeToken type, int indexColumn, TypeTokenKind tokenType)
>>>>>>> 1c58d47 (What is all this? I'm not sure, so let's commit it.)
        {
            _recordTypeToken = type;
            _indexMemberTypeTokenType = tokenType;
            _indexColumn = indexColumn;

            var rootExpression = Expr.Parameter(typeof(T).MakeByRefType(), "instance");

            var (indexColumnMemberToken, memberAccess) = MakeAccess(rootExpression);
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

        private (MemberToken memberToken, Expr memberAccess) MakeAccess(Expr instance)
        {
            var (token, access) = _recordTypeToken.MakeMemberAccess(_indexColumn, instance, _indexMemberTypeTokenType);
            return (token, access);
        }

        public Expr AccessIndex(Expr instance) => MakeAccess(instance).memberAccess;
    }
}
