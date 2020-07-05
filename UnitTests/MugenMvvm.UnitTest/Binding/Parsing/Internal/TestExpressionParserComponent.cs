﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestExpressionParserComponent : IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly IExpressionParser? _parser;

        #endregion

        #region Constructors

        public TestExpressionParserComponent(IExpressionParser? parser = null)
        {
            _parser = parser;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>>? TryParse { get; set; }

        #endregion

        #region Implementation of interfaces

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(IExpressionParser parser, in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            _parser?.ShouldEqual(parser);
            return TryParse?.Invoke(expression!, typeof(TExpression), metadata) ?? default;
        }

        #endregion
    }
}