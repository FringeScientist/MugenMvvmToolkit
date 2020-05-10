﻿using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingParameterInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IExpressionCompiler? _compiler;
        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;
        private readonly BindingMemberExpressionVisitor _memberExpressionVisitor;

        private static readonly
            FuncIn<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression), IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?>
            GetParametersComponentDelegate = GetParametersComponent;

        #endregion

        #region Constructors

        public BindingParameterInitializer(IExpressionCompiler? compiler = null)
        {
            _memberExpressionVisitor = new BindingMemberExpressionVisitor { IgnoreIndexMembers = true, IgnoreMethodMembers = true };
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _compiler = compiler;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingParameterInitializer;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingExpressionInitializerContext context)
        {
            if (context.BindingComponents.ContainsKey(BindingParameterNameConstant.ParameterHandler) || context.BindingComponents.ContainsKey(BindingParameterNameConstant.EventHandler))
                return;

            _memberExpressionVisitor.MemberFlags = MemberFlags;
            _memberExpressionVisitor.Flags = BindingMemberExpressionFlags.Observable;
            var metadata = context.GetMetadataOrDefault();
            var converter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.Converter, metadata);
            var converterParameter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.ConverterParameter, metadata);
            var fallback = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.Fallback, metadata);
            var targetNullValue = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.TargetNullValue, metadata);
            if (!converter.IsEmpty || !converterParameter.IsEmpty || !fallback.IsEmpty || !targetNullValue.IsEmpty)
            {
                var state = (converter, converterParameter, fallback, targetNullValue);
                var provider =
                    new DelegateBindingComponentProvider<(BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression)>(GetParametersComponentDelegate, state);
                context.BindingComponents[BindingParameterNameConstant.ParameterHandler] = provider;
            }
        }

        #endregion

        #region Methods

        private static IComponent<IBinding> GetParametersComponent(in (BindingParameterExpression, BindingParameterExpression, BindingParameterExpression, BindingParameterExpression) state,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            var (converter, converterParameter, fallback, targetNullValue) = state;
            return new ParameterHandlerBindingComponent(converter.ToBindingParameter(target, source, metadata), converterParameter.ToBindingParameter(target, source, metadata),
                fallback.ToBindingParameter(target, source, metadata), targetNullValue.ToBindingParameter(target, source, metadata));
        }

        #endregion
    }
}