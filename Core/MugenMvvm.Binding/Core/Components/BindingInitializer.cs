﻿using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Components;
using MugenMvvm.Delegates;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingInitializer : AttachableComponentBase<IBindingManager>, IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IExpressionCompiler? _compiler;
        private readonly FuncIn<(BindingParameterExpression, bool), IBinding, object, object?, IReadOnlyMetadataContext?, IComponent<IBinding>?> _getEventHandlerDelegate;
        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;
        private readonly BindingMemberExpressionVisitor _memberExpressionVisitor;
        private readonly IMemberManager? _memberManager;

        #endregion

        #region Constructors

        public BindingInitializer(IExpressionCompiler? compiler = null, IMemberManager? memberManager = null)
        {
            _memberExpressionVisitor = new BindingMemberExpressionVisitor();
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
            _getEventHandlerDelegate = GetEventHandlerComponent;
            _compiler = compiler;
            _memberManager = memberManager;
        }

        #endregion

        #region Properties

        public BindingMemberExpressionFlags Flags { get; set; } = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        public bool IgnoreMethodMembers { get; set; }

        public bool IgnoreIndexMembers { get; set; }

        public bool ToggleEnabledState { get; set; }

        public int Priority { get; set; } = BindingComponentPriority.BindingInitializer;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingExpressionInitializerContext context)
        {
            var metadata = context.GetMetadataOrDefault();
            _memberExpressionVisitor.MemberFlags = MemberFlags;
            _memberExpressionVisitor.Flags = Flags;
            _memberExpressionVisitor.IgnoreMethodMembers = context.TryGetParameterValue<bool?>(BindingParameterNameConstant.IgnoreMethodMembers).GetValueOrDefault(IgnoreMethodMembers);
            _memberExpressionVisitor.IgnoreIndexMembers = context.TryGetParameterValue<bool?>(BindingParameterNameConstant.IgnoreIndexMembers).GetValueOrDefault(IgnoreIndexMembers);
            context.ApplyFlags(_memberExpressionVisitor, BindingParameterNameConstant.Observable, BindingMemberExpressionFlags.Observable);
            context.ApplyFlags(_memberExpressionVisitor, BindingParameterNameConstant.Optional, BindingMemberExpressionFlags.Optional);
            context.ApplyFlags(_memberExpressionVisitor, BindingParameterNameConstant.HasStablePath, BindingMemberExpressionFlags.StablePath);
            context.ApplyFlags(_memberExpressionVisitor, BindingParameterNameConstant.ObservableMethod, BindingMemberExpressionFlags.ObservableMethod);

            context.TargetExpression = _memberExpressionVisitor.Visit(context.TargetExpression, metadata);
            if (!IsEvent(context.Target, context.Source, context.TargetExpression, metadata))
            {
                context.SourceExpression = _memberExpressionVisitor.Visit(context.SourceExpression, metadata);
                return;
            }

            _memberExpressionVisitor.Flags &= ~(BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod);
            _memberExpressionVisitor.IgnoreIndexMembers = true;
            _memberExpressionVisitor.IgnoreMethodMembers = true;
            context.SourceExpression = _memberExpressionVisitor.Visit(context.SourceExpression, metadata);
            context.BindingComponents[BindingParameterNameConstant.Mode] = null;
            if (!context.BindingComponents.ContainsKey(BindingParameterNameConstant.EventHandler))
            {
                _memberExpressionVisitor.Flags |= BindingMemberExpressionFlags.Observable;
                var parameter = context.TryGetParameterExpression(_compiler, _memberExpressionVisitor, _memberExpressionCollectorVisitor, BindingParameterNameConstant.CommandParameter, metadata);
                var toggle = context.TryGetParameterValue<bool?>(BindingParameterNameConstant.ToggleEnabled).GetValueOrDefault(ToggleEnabledState);
                context.BindingComponents[BindingParameterNameConstant.EventHandler] = new DelegateBindingComponentProvider<(BindingParameterExpression, bool)>(_getEventHandlerDelegate, (parameter, toggle));
            }
        }

        #endregion

        #region Methods

        private bool IsEvent(object target, object? source, IExpressionNode targetExpression, IReadOnlyMetadataContext? metadata)
        {
            if (targetExpression is IBindingMemberExpressionNode bindingMemberExpression)
            {
                target = bindingMemberExpression.GetTarget(target, source, metadata, out var path, out var flags);
                return path.GetLastMemberFromPath(flags.GetTargetType(target), target, flags, MemberType.Event, metadata, _memberManager) != null;
            }

            return false;
        }

        private IComponent<IBinding> GetEventHandlerComponent(in (BindingParameterExpression value, bool toggle) state, IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return new EventHandlerBindingComponent(state.value.ToBindingParameter(target, source, metadata), state.toggle, Owner);
        }

        #endregion
    }
}