﻿using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Bindings.Parsing.Visitors;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Visitors
{
    public class ConstantToBindingParameterVisitorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldConvertConstantToBindingInstanceMember()
        {
            var visitor = new ConstantToBindingParameterVisitor();
            var expression = new ConstantExpressionNode("1");
            expression.Accept(visitor, DefaultMetadata).ShouldEqual(new BindingInstanceMemberExpressionNode(expression, "", -1, default, default, null, expression, expression.Metadata));

            var memberExp = new MemberExpressionNode(expression, "M");
            memberExp.Accept(visitor, DefaultMetadata).ShouldEqual(memberExp.UpdateTarget(new BindingInstanceMemberExpressionNode(expression, "", -1, default, default, null, expression, expression.Metadata)));
        }

        #endregion
    }
}