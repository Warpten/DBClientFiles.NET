using System;
using System.Collections.Generic;

using System.Linq.Expressions;
using Expr = System.Linq.Expressions.Expression;

namespace DBClientFiles.NET.Parsing.Runtime
{
    /// <summary>
    /// This class allows to rent and return parameters used in a method.
    /// </summary>
    internal class ParameterProvider
    {
        private readonly Stack<Method.Parameter> _parameters;
        private readonly Type _type;

        public ParameterProvider(Type parameterType)
        {
            _type = parameterType;
            _parameters = new Stack<Method.Parameter>();
        }

        public Method.Parameter Rent()
        {
            if (!_parameters.TryPop(out var parameter))
                parameter = new Method.Parameter(_type);

            return parameter;
        }

        public void Return(Method.Parameter parameter) => _parameters.Push(parameter);
    }
}
