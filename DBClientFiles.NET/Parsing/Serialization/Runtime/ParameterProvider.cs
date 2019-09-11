using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DBClientFiles.NET.Parsing.Serialization.Runtime
{
    internal class ParameterProvider
    {
        private readonly Stack<ParameterExpression> _parameters;

        private readonly Type _type;

        public ParameterProvider(Type parameterType)
        {
            _type = parameterType;
            _parameters = new Stack<ParameterExpression>();
        }

        public ParameterExpression Rent()
        {
            if (!_parameters.TryPop(out var parameter))
                parameter = Expression.Parameter(_type);

            return parameter;
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public void Return(ParameterExpression parameter) => _parameters.Push(parameter);
    }

    internal class ParameterProvider<T> : ParameterProvider
    {
        public static ParameterProvider<T> Instance { get; } = new ParameterProvider<T>();

        private ParameterProvider() : base(typeof(T)) { }
    }
}
