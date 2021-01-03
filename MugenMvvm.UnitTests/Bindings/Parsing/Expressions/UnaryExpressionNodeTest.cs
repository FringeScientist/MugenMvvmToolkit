﻿using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class UnaryExpressionNodeTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var exp = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Unary);
            exp.Operand.ShouldEqual(target);
            exp.Token.ShouldEqual(UnaryTokenType.BitwiseNegation);
            exp.ToString().ShouldEqual("~\"1\"");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldVisitWithCorrectOrder(bool isPostOrder)
        {
            var nodes = new List<IExpressionNode>();
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(DefaultMetadata);
                    return node;
                },
                IsPostOrder = isPostOrder
            };

            var target = new ConstantExpressionNode("1");
            var exp = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target);

            var result = isPostOrder ? new IExpressionNode[] {target, exp} : new IExpressionNode[] {exp, target};
            exp.Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AcceptShouldCreateNewNode1(bool isPostOrder)
        {
            var target = new ConstantExpressionNode("1");
            var targetChanged = new ConstantExpressionNode("1-");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == target)
                        return targetChanged;
                    return node;
                },
                IsPostOrder = isPostOrder
            };
            var exp = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target);
            var expressionNode = (UnaryExpressionNode) exp.Accept(testExpressionVisitor, DefaultMetadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Operand.ShouldEqual(targetChanged);
            expressionNode.Token.ShouldEqual(UnaryTokenType.BitwiseNegation);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target).Accept(testExpressionVisitor, DefaultMetadata).ShouldEqual(target);
        }


        [Fact]
        public void GetShouldReturnMacrosMembers()
        {
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Self).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Self).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, MacrosConstant.This)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.This)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, new MemberExpressionNode(null, MacrosConstant.Target)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, new MemberExpressionNode(null, MacrosConstant.Target)).ShouldEqual(UnaryExpressionNode.TargetMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Context).ShouldEqual(UnaryExpressionNode.ContextMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Context).ShouldEqual(UnaryExpressionNode.ContextMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Source).ShouldEqual(UnaryExpressionNode.SourceMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Source).ShouldEqual(UnaryExpressionNode.SourceMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.EventArgs).ShouldEqual(UnaryExpressionNode.EventArgsMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.EventArgs).ShouldEqual(UnaryExpressionNode.EventArgsMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Binding).ShouldEqual(UnaryExpressionNode.BindingMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Binding).ShouldEqual(UnaryExpressionNode.BindingMacros);
            UnaryExpressionNode.Get(UnaryTokenType.StaticExpression, MemberExpressionNode.Action).ShouldEqual(UnaryExpressionNode.ActionMacros);
            UnaryExpressionNode.Get(UnaryTokenType.DynamicExpression, MemberExpressionNode.Action).ShouldEqual(UnaryExpressionNode.ActionMacros);

            var node = UnaryExpressionNode.Get(UnaryTokenType.LogicalNegation, MemberExpressionNode.Source);
            node.ShouldNotEqual(UnaryExpressionNode.SourceMacros);
            node.Operand.ShouldEqual(MemberExpressionNode.Source);
            node.Token.ShouldEqual(UnaryTokenType.LogicalNegation);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var target = new ConstantExpressionNode("1");
            var node = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, target, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> {{"k", null}};
                var updated = (UnaryExpressionNode) node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Token.ShouldEqual(node.Token);
                updated.Operand.ShouldEqual(node.Operand);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, GetTestEqualityExpression(comparer, 1), new Dictionary<string, object?> {{"k", null}});
            var exp2 = new UnaryExpressionNode(UnaryTokenType.BitwiseNegation, GetTestEqualityExpression(comparer, 1), new Dictionary<string, object?> {{"k", null}});
            HashCode.Combine(GetBaseHashCode(exp1), UnaryTokenType.BitwiseNegation.GetHashCode(), 1).ShouldEqual(exp1.GetHashCode(comparer));
            ((TestExpressionNode) exp1.Operand).GetHashCodeCount.ShouldEqual(1);

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode) exp1.Operand).EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Operand).EqualsCount.ShouldEqual(1);

            if (comparer == null)
                return;
            comparer.GetHashCode = node =>
            {
                ReferenceEquals(node, exp1).ShouldBeTrue();
                return int.MaxValue;
            };
            comparer.Equals = (x1, x2) =>
            {
                ReferenceEquals(x1, exp1).ShouldBeTrue();
                ReferenceEquals(x2, exp2).ShouldBeTrue();
                return false;
            };
            exp1.GetHashCode(comparer).ShouldEqual(int.MaxValue);
            exp1.Equals(exp2, comparer).ShouldBeFalse();
            ((TestExpressionNode) exp1.Operand).EqualsCount.ShouldEqual(1);
        }

        #endregion
    }
}