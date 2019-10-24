﻿using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Binding.Parsing.Expressions
{
    public sealed class NullConditionalMemberExpressionNode : ExpressionNodeBase, IHasTargetExpressionNode<NullConditionalMemberExpressionNode>
    {
        #region Constructors

        public NullConditionalMemberExpressionNode(IExpressionNode target)
        {
            Should.NotBeNull(target, nameof(target));
            Target = target;
        }

        #endregion

        #region Properties

        public override ExpressionNodeType NodeType => ExpressionNodeType.Member;

        public IExpressionNode? Target { get; }

        #endregion

        #region Implementation of interfaces

        public NullConditionalMemberExpressionNode UpdateTarget(IExpressionNode target)
        {
            if (ReferenceEquals(target, Target))
                return this;

            return new NullConditionalMemberExpressionNode(target);
        }

        #endregion

        #region Methods

        protected override IExpressionNode VisitInternal(IExpressionVisitor visitor)
        {
            if (Target == null)
                return this;
            var changed = false;
            var node = VisitWithCheck(visitor, Target, false, ref changed);
            if (changed)
                return new NullConditionalMemberExpressionNode(node);
            return this;
        }

        public override string ToString()
        {
            return Target + "?";
        }

        #endregion
    }
}