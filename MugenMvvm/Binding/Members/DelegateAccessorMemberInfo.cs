﻿using System;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class DelegateAccessorMemberInfo<TTarget, TValue, TState> : DelegateObservableMemberInfo<TTarget, TState>, IAccessorMemberInfo where TTarget : class?
    {
        #region Fields

        private readonly GetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? _getValue;
        private readonly SetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? _setValue;

        #endregion

        #region Constructors

        public DelegateAccessorMemberInfo(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, in TState state,
            GetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? getValue, SetValueDelegate<DelegateAccessorMemberInfo<TTarget, TValue, TState>, TTarget, TValue>? setValue,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
            : base(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserve, raise)
        {
            if (getValue == null)
                Should.NotBeNull(setValue, nameof(setValue));
            _getValue = getValue;
            _setValue = setValue;
        }

        #endregion

        #region Properties

        public bool CanRead => _getValue != null;

        public bool CanWrite => _setValue != null;

        public override MemberType MemberType => MemberType.Accessor;

        #endregion

        #region Implementation of interfaces

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata)
        {
            if (_getValue == null)
                BindingExceptionManager.ThrowBindingMemberMustBeReadable(this);
            return BoxingExtensions.Box(_getValue(this, (TTarget)target!, metadata));
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata)
        {
            if (_setValue == null)
                BindingExceptionManager.ThrowBindingMemberMustBeWritable(this);
            _setValue(this, (TTarget)target!, (TValue)value!, metadata);
        }

        #endregion
    }
}