﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingExpressionExceptionDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseBindingExpressionShouldWrapExceptionToInvalidBinding()
        {
            var request = "";
            var exception = new Exception();
            var decorator = new BindingExpressionExceptionDecorator();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, type, arg3) =>
                {
                    o.ShouldEqual(request);
                    type.ShouldEqual(request.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                    throw exception;
                }
            };
            ((IComponentCollectionDecorator<IBindingExpressionParserComponent>) decorator).Decorate(new List<IBindingExpressionParserComponent> {decorator, component}, DefaultMetadata);

            var expression = decorator.TryParseBindingExpression(request, DefaultMetadata).Item!;
            expression.ShouldNotBeNull();

            var binding = (InvalidBinding) expression.Build(this, this, DefaultMetadata);
            binding.Exception.ShouldEqual(exception);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryParseBindingExpressionShouldWrapBuildExceptionToInvalidBinding(int count)
        {
            var target = new object();
            var source = new object();
            var exception = new Exception();
            var expressions = new IBindingBuilder[count];
            for (var i = 0; i < expressions.Length; i++)
            {
                expressions[i] = new TestBindingBuilder
                {
                    Build = (t, s, arg3) =>
                    {
                        t.ShouldEqual(target);
                        source.ShouldEqual(s);
                        arg3.ShouldEqual(DefaultMetadata);
                        throw exception;
                    }
                };
            }

            var decorator = new BindingExpressionExceptionDecorator();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, type, arg3) => expressions
            };
            ((IComponentCollectionDecorator<IBindingExpressionParserComponent>) decorator).Decorate(new List<IBindingExpressionParserComponent> {decorator, component}, DefaultMetadata);

            var result = decorator.TryParseBindingExpression("", DefaultMetadata).AsList();
            result.Count.ShouldEqual(count);

            for (var i = 0; i < result.Count; i++)
            {
                var binding = (InvalidBinding) result[i].Build(target, source, DefaultMetadata);
                binding.Exception.ShouldEqual(exception);
            }
        }

        #endregion
    }
}