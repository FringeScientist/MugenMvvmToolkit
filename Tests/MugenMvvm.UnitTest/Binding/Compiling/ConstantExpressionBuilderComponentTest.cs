﻿using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class ConstantExpressionBuilderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotConstantExpression()
        {
            var component = new ConstantExpressionBuilderComponent();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, MemberExpressionNode.Self).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldBuildConstantExpression()
        {
            var component = new ConstantExpressionBuilderComponent();
            var ctx = new TestExpressionBuilderContext();
            var build = component.TryBuild(ctx, ConstantExpressionNode.False)!;
            build.ShouldEqual(ConstantExpressionNode.False.ConstantExpression);
            build.Invoke().ShouldEqual(false);

            var constantExpressionNode = new ConstantExpressionNode(1);
            build = component.TryBuild(ctx, constantExpressionNode)!;
            build.Invoke().ShouldEqual(constantExpressionNode.Value);
        }

        #endregion
    }
}