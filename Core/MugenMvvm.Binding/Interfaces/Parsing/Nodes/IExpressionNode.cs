﻿using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Parsing.Nodes
{
    public interface IExpressionNode
    {
        ExpressionNodeType NodeType { get; }

        IExpressionNode Accept(IExpressionVisitor visitor);
    }
}