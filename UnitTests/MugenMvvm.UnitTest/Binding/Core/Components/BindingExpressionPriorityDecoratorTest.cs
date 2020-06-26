﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingExpressionPriorityDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseBindingExpressionShouldIgnoreNonListResult()
        {
            var request = "";
            var exp = new TestBindingBuilder();
            var decorator = new BindingExpressionPriorityDecorator();
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, type, arg3) =>
                {
                    o.ShouldEqual(request);
                    type.ShouldEqual(request.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                    return exp;
                }
            };
            ((IComponentCollectionDecorator<IBindingExpressionParserComponent>) decorator).Decorate(new List<IBindingExpressionParserComponent> {decorator, component}, DefaultMetadata);

            decorator.TryParseBindingExpression(request, DefaultMetadata).AsList().Single().ShouldEqual(exp);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TryParseBindingExpressionShouldOrderList(int inputParameterState)
        {
            const string maxPriorityName = "T1";
            const string minPriorityName = "T2";

            var expressions = new IBindingBuilder[]
            {
                new NoPriorityBindingBuilder(),
                new TestBindingBuilder {TargetExpression = new MemberExpressionNode(new MemberExpressionNode(null, minPriorityName), Guid.NewGuid().ToString())},
                new TestBindingBuilder {TargetExpression = new TestBindingMemberExpressionNode(maxPriorityName)},
                new HasPriorityBindingBuilder {Priority = int.MaxValue - 1},
                new TestBindingBuilder {TargetExpression = new HasPriorityExpressionNode {Priority = 1}}
            };
            var expected = new[] {expressions[2], expressions[3], expressions[4], expressions[0], expressions[1]};
            IList<IBindingBuilder> result;
            if (inputParameterState == 1)
                result = expressions;
            else if (inputParameterState == 2)
                result = expressions.ToList();
            else
                result = new Collection<IBindingBuilder>(expressions);

            var decorator = new BindingExpressionPriorityDecorator();
            decorator.BindingMemberPriorities.Clear();
            decorator.BindingMemberPriorities[maxPriorityName] = int.MaxValue;
            decorator.BindingMemberPriorities[minPriorityName] = int.MinValue;
            var component = new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, type, arg3) => ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>>.FromRawValue(result)
            };
            ((IComponentCollectionDecorator<IBindingExpressionParserComponent>) decorator).Decorate(new List<IBindingExpressionParserComponent> {decorator, component}, DefaultMetadata);

            var bindingExpressions = decorator.TryParseBindingExpression("", DefaultMetadata).AsList();
            bindingExpressions.ShouldEqual(expected);
        }

        #endregion

        #region Nested types

        private sealed class HasPriorityExpressionNode : ExpressionNodeBase, IHasPriority, IMemberExpressionNode
        {
            #region Properties

            public override ExpressionNodeType ExpressionType => ExpressionNodeType.Index;

            public int Priority { get; set; }

            public IExpressionNode? Target { get; } = null!;

            public string Member { get; } = null!;

            #endregion

            #region Implementation of interfaces

            public IMemberExpressionNode UpdateTarget(IExpressionNode? target)
            {
                throw new NotSupportedException();
            }

            #endregion

            #region Methods

            protected override IExpressionNode VisitInternal(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        private sealed class NoPriorityBindingBuilder : IBindingBuilder
        {
            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        private sealed class HasPriorityBindingBuilder : IHasPriority, IBindingBuilder
        {
            #region Properties

            public int Priority { get; set; }

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                throw new NotSupportedException();
            }

            #endregion
        }

        #endregion
    }
}