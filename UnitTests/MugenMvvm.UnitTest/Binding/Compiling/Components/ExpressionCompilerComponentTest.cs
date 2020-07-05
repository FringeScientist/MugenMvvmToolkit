﻿using System.Linq;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling.Components
{
    public class ExpressionCompilerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryCompileShouldReturnCompiledExpressionWithComponents()
        {
            var expressionCompiler = new ExpressionCompiler();
            var component = new ExpressionCompilerComponent();
            expressionCompiler.AddComponent(component);

            var compiledExpression = (CompiledExpression)component.TryCompile(expressionCompiler, ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.ShouldBeEmpty();

            var testBuilder = new TestExpressionBuilderComponent();
            expressionCompiler.AddComponent(testBuilder);

            compiledExpression = (CompiledExpression)component.TryCompile(expressionCompiler, ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.Single().ShouldEqual(testBuilder);

            expressionCompiler.RemoveComponent(testBuilder);
            compiledExpression = (CompiledExpression)component.TryCompile(expressionCompiler, ConstantExpressionNode.False, DefaultMetadata)!;
            compiledExpression.ExpressionBuilders.ShouldBeEmpty();
        }

        #endregion
    }
}