﻿using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Observers
{
    public sealed class MultiPathObserver : ObserverBase, IEventListener, IWeakReferenceHolder
    {
        #region Fields

        private readonly IDisposable?[]? _listeners;
        private readonly MemberFlags _memberFlags;
        private Exception? _exception;
        private IEventListener? _lastMemberListener;
        private IBindingMemberInfo[]? _members;
        private IWeakReference? _penultimateValue;

        private byte _state;

        #endregion

        #region Constructors

        public MultiPathObserver(IWeakReference source, IMemberPath path,
            MemberFlags memberFlags, bool hasStablePath, bool observable, bool optional)
            : base(source)
        {
            _memberFlags = memberFlags;
            if (hasStablePath)
                _state |= HasStablePathFlag;
            if (optional)
                _state |= OptionalFlag;
            if (observable)
                _listeners = new IDisposable[path.Members.Length];
            Path = path;
        }

        #endregion

        #region Properties

        public override IMemberPath Path { get; }

        public bool IsWeak => false;

        public IWeakReference? WeakReference { get; set; }

        private bool HasStablePath => CheckFlag(HasStablePathFlag);

        private bool Optional => CheckFlag(OptionalFlag);

        private bool IsInitialized
        {
            get => CheckFlag(InitializedFlag);
            set
            {
                if (value)
                    _state |= InitializedFlag;
            }
        }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle(object sender, object? message)
        {
            return Update();
        }

        #endregion

        #region Methods

        public override MemberPathMembers GetMembers(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathMembers(Path, _exception);

            var source = Source;
            if (source == null)
                return default;

            var target = _penultimateValue?.Target;
            if (target == null || _members == null)
                return default;

            return new MemberPathMembers(Path, source, target, _members, _members[_members.Length - 1]);
        }

        public override MemberPathLastMember GetLastMember(IReadOnlyMetadataContext? metadata = null)
        {
            UpdateIfNeed();
            if (_exception != null)
                return new MemberPathLastMember(Path, _exception);

            var target = _penultimateValue?.Target;
            if (target == null || _members == null)
                return default;

            return new MemberPathLastMember(Path, target, _members[_members.Length - 1]);
        }

        protected override void OnListenerAdded(IMemberPathObserverListener listener)
        {
            UpdateIfNeed();
            if (_listeners != null && _members != null && _listeners[_listeners.Length - 1] == null && _penultimateValue != null)
            {
                var target = _penultimateValue.Target;
                if (target != null)
                    _listeners[_listeners.Length - 1] =
                        (_members[_members.Length - 1] as IObservableBindingMemberInfo)?.TryObserve(target, GetLastMemberListener()) ?? Default.Disposable;
            }
        }

        protected override void OnListenersRemoved()
        {
            var listeners = _listeners;
            var listener = listeners?[listeners.Length - 1];
            if (listener != null)
            {
                listener.Dispose();
                listeners![listeners.Length - 1] = null;
            }
        }

        protected override void OnDisposed()
        {
            ClearListeners();
            _penultimateValue?.Release();
            _penultimateValue = null;
            _members = null;
            _exception = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateIfNeed()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                Update();
            }
        }

        private bool Update()
        {
            try
            {
                var source = Source;
                if (source == null)
                {
                    SetMembers(null, null, null);
                    return false;
                }

                ClearListeners();

                if (HasStablePath && _members != null)
                {
                    UpdateHasStablePath(_members, source);
                    return true;
                }

                var paths = Path.Members;
                var members = new IBindingMemberInfo[paths.Length];
                var provider = MugenBindingService.MemberProvider;
                var lastIndex = members.Length - 1;
                for (var i = 0; i < members.Length; i++)
                {
                    var member = provider.GetMember(source!.GetType(), paths[i],
                        i == lastIndex ? BindingMemberType.Field | BindingMemberType.Property : BindingMemberType.Field | BindingMemberType.Property | BindingMemberType.Event,
                        _memberFlags);
                    if (member == null)
                    {
                        if (Optional)
                            SetMembers(null, null, null);
                        else
                            BindingExceptionManager.ThrowInvalidBindingMember(source.GetType(), paths[i]);
                        return true;
                    }

                    members[i] = member;
                    if (i == lastIndex)
                        break;

                    if (_listeners != null)
                        _listeners[i] = (member as IObservableBindingMemberInfo)?.TryObserve(source, this);

                    source = (member as IBindingPropertyInfo)?.GetValue(source);
                    if (source.IsNullOrUnsetValue())
                    {
                        SetMembers(null, null, null);
                        return true;
                    }
                }

                if (_listeners != null && HasListeners)
                    _listeners[_listeners.Length - 1] =
                        (members[members.Length - 1] as IObservableBindingMemberInfo)?.TryObserve(source, GetLastMemberListener()) ?? Default.Disposable;
                SetMembers(source.ToWeakReference(), members, null);
            }
            catch (Exception e)
            {
                SetMembers(null, null, e);
                OnError(e);
            }

            return true;
        }

        private void UpdateHasStablePath(IBindingMemberInfo[] members, object source)
        {
            for (var index = 0; index < members.Length - 1; index++)
            {
                var member = members[index];
                if (_listeners != null)
                    _listeners[index] = (member as IObservableBindingMemberInfo)?.TryObserve(source, this);

                source = (member as IBindingPropertyInfo)?.GetValue(source)!;
                if (source.IsNullOrUnsetValue())
                {
                    SetMembers(null, members, null);
                    return;
                }
            }

            if (_listeners != null && HasListeners)
                _listeners[_listeners.Length - 1] =
                    (members[members.Length - 1] as IObservableBindingMemberInfo)?.TryObserve(source, GetLastMemberListener()) ?? Default.Disposable;

            SetMembers(source.ToWeakReference(), members, null);
        }

        private void SetMembers(IWeakReference? penultimateValue, IBindingMemberInfo[]? members, Exception? exception)
        {
            _penultimateValue = penultimateValue;
            _members = members;
            _exception = exception;
            OnPathMembersChanged();
        }

        private IEventListener GetLastMemberListener()
        {
            if (_lastMemberListener == null)
                _lastMemberListener = new LastMemberListener(this.ToWeakReference());
            return _lastMemberListener;
        }

        private void ClearListeners()
        {
            if (_listeners != null)
            {
                for (var index = 0; index < _listeners.Length; index++)
                {
                    _listeners[index]?.Dispose();
                    _listeners[index] = null;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckFlag(byte flag)
        {
            return (_state & flag) == flag;
        }

        #endregion

        #region Nested types

        private sealed class LastMemberListener : IEventListener
        {
            #region Fields

            private readonly IWeakReference _observer;

            #endregion

            #region Constructors

            public LastMemberListener(IWeakReference observer)
            {
                _observer = observer;
            }

            #endregion

            #region Properties

            public bool IsAlive => _observer.Target != null;

            public bool IsWeak => true;

            #endregion

            #region Implementation of interfaces

            public bool TryHandle(object sender, object? message)
            {
                var observer = (MultiPathObserver?)_observer.Target;
                if (observer == null)
                    return false;
                observer.OnLastMemberChanged();
                return true;
            }

            #endregion
        }

        #endregion
    }
}