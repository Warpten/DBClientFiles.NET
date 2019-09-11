using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DBClientFiles.NET.Parsing.Serialization.Method
{
    internal class ParameterProvider
    {
        private Stack<ParameterExpression> _parameters;

        private Type _type;

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

        public void Return(ParameterExpression parameter) => _parameters.Push(parameter);
    }

    internal class ParameterProvider<T> : ParameterProvider
    {
        private static Lazy<ParameterProvider<T>> _lazy = new Lazy<ParameterProvider<T>>(() => new ParameterProvider<T>());
        public static ParameterProvider<T> Instance => _lazy.Value;

        private ParameterProvider() : base(typeof(T)) { }
    }
}
