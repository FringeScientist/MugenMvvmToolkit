﻿using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public class SinglePathObserver : ObserverBase, IEventListener, IValueHolder<IWeakReference>
    {
        #region Fields

        protected readonly BindingMemberFlags MemberFlags;

        private object? _lastMemberOrException;
        private Unsubscriber _lastMemberUnsubscriber;
        private byte _state;

        #endregion

        #region Constructors

        public SinglePathObserver(object target, IMemberPath path, BindingMemberFlags memberFlags, bool observable, bool optional)
            : base(target)
        {
            Should.NotBeNull(path, nameof(path));
            MemberFlags = memberFlags;
            if (observable)
                _state |= ObservableFlag;
            if (optional)
                _state |= OptionalFlag;
            Path = path;
        }

        #endregion

        #region Properties

        public override IMemberPath Path { get; }

        public bool IsWeak => false;

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        protected bool Observable => CheckFlag(ObservableFlag);

        protected bool Optional => CheckFlag(OptionalFlag);

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle(object sender, object? message)
        {
            OnLastMemberChanged();
            return true;
        }

        #endregion

        #region Methods

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_lastMemberOrException is IBindingMemberInfo member)
            {
                var target = Target;
                if (target == null)
                    return default;
                return new MemberPathMembers(target, new[] { member });
            }

            if (_lastMemberOrException is Exception e)
                return new MemberPathMembers(e);
            return default;
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_lastMemberOrException is IBindingMemberInfo member)
            {
                var target = Target;
                if (target == null)
                    return default;
                return new MemberPathLastMember(target, member);
            }

            if (_lastMemberOrException is Exception e)
                return new MemberPathLastMember(e);
            return default;
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            UpdateIfNeed();
            if (Observable && _lastMemberUnsubscriber.IsEmpty && _lastMemberOrException is IBindingMemberInfo lastMember)
            {
                var target = Target;
                if (target == null)
                    _lastMemberUnsubscriber = Unsubscriber.NoDoUnsubscriber;
                else
                    SubscribeLastMember(target, lastMember);
            }
        }

        protected override void OnListenersRemoved()
        {
            UnsubscribeLastMember();
        }

        protected override void OnDisposed()
        {
            UnsubscribeLastMember();
            this.ReleaseWeakReference();
            _lastMemberOrException = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateIfNeed()
        {
            if (!CheckFlag(InitializedFlag))
            {
                _state |= InitializedFlag;
                Update();
            }
        }

        private void Update()
        {
            try
            {
                var target = Target;
                if (target == null)
                {
                    SetLastMember(null, null);
                    return;
                }

                if (_lastMemberOrException is IBindingMemberInfo)
                    return;

                var lastMember = MugenBindingService
                      .MemberProvider
                      .GetMember(GetTargetType(target, MemberFlags), Path.Path, BindingMemberType.Event | BindingMemberType.Field | BindingMemberType.Property, MemberFlags);
                if (lastMember == null)
                {
                    if (Optional)
                        SetLastMember(null, null);
                    else
                        BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), Path.Path);
                    return;
                }

                if (Observable && HasListeners)
                    SubscribeLastMember(target, lastMember);
                SetLastMember(lastMember, null);
            }
            catch (Exception e)
            {
                SetLastMember(null, e);
                OnError(e);
            }
        }

        private void SetLastMember(IBindingMemberInfo? lastMember, Exception? exception)
        {
            _lastMemberOrException = (object?)exception ?? lastMember;
            OnLastMemberChanged();
        }

        protected virtual void SubscribeLastMember(object target, IBindingMemberInfo? lastMember)
        {
            _lastMemberUnsubscriber.Unsubscribe();
            if (lastMember is IObservableBindingMemberInfo observable)
                _lastMemberUnsubscriber = observable.TryObserve(target, this);
            if (_lastMemberUnsubscriber.IsEmpty)
                _lastMemberUnsubscriber = Unsubscriber.NoDoUnsubscriber;
        }

        protected virtual void UnsubscribeLastMember()
        {
            _lastMemberUnsubscriber.Unsubscribe();
            _lastMemberUnsubscriber = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion
    }
}