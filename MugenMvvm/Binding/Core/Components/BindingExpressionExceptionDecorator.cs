﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionExceptionDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression<TExpression>([DisallowNull] in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            try
            {
                var result = Components.TryParseBindingExpression(expression, metadata);
                if (result.Item != null)
                    return ExceptionWrapperBindingBuilder.Wrap(result.Item);

                var items = result.List;
                if (items != null)
                {
                    var expressions = new IBindingBuilder[items.Count];
                    for (var i = 0; i < expressions.Length; i++)
                        expressions[i] = ExceptionWrapperBindingBuilder.Wrap(items[i]);
                    return expressions;
                }

                BindingExceptionManager.ThrowCannotParseExpression(expression);
            }
            catch (Exception e)
            {
                return new InvalidBinding(e);
            }

            return default;
        }

        #endregion

        #region Nested types

        private sealed class ExceptionWrapperBindingBuilder : IBindingBuilder, IWrapper<IBindingBuilder>
        {
            #region Fields

            private readonly IBindingBuilder _bindingExpression;

            #endregion

            #region Constructors

            private ExceptionWrapperBindingBuilder(IBindingBuilder bindingExpression)
            {
                _bindingExpression = bindingExpression;
            }

            #endregion

            #region Properties

            IBindingBuilder IWrapper<IBindingBuilder>.Target => _bindingExpression;

            #endregion

            #region Implementation of interfaces

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null)
            {
                try
                {
                    return _bindingExpression.Build(target, source, metadata);
                }
                catch (Exception e)
                {
                    return new InvalidBinding(e);
                }
            }

            #endregion

            #region Methods

            public static ExceptionWrapperBindingBuilder Wrap(IBindingBuilder expression)
            {
                if (expression is ExceptionWrapperBindingBuilder wrapper)
                    return wrapper;
                return new ExceptionWrapperBindingBuilder(expression);
            }

            #endregion
        }

        #endregion
    }
}