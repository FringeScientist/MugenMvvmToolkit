﻿using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class MemberExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Member;

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (!(expression is MemberExpression memberExpression))
                return null;

            if (context.TryConvertExtension(memberExpression.Member, expression, out var result))
                return result;

            return MemberExpressionNode.Get(context.ConvertTarget(memberExpression.Expression, memberExpression.Member), memberExpression.Member.Name);
        }

        #endregion
    }
}