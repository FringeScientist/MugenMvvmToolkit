﻿using System;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Observers;

namespace MugenMvvm.Binding.Members.Builders
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct EventBuilder<TTarget> where TTarget : class?
    {
        #region Fields

        private readonly Type _declaringType;
        private readonly string _name;
        private readonly Type _eventType;
        private MemberAttachedDelegate<INotifiableMemberInfo, TTarget>? _attachedHandler;
        private TryObserveDelegate<INotifiableMemberInfo, TTarget>? _subscribe;
        private RaiseDelegate<INotifiableMemberInfo, TTarget>? _raise;
        private object? _underlyingMember;
        private bool _isStatic;

        #endregion

        #region Constructors

        public EventBuilder(string name, Type declaringType, Type eventType)
        {
            Should.NotBeNull(name, nameof(name));
            Should.NotBeNull(declaringType, nameof(declaringType));
            Should.NotBeNull(eventType, nameof(eventType));
            _name = name;
            _declaringType = declaringType;
            _attachedHandler = null;
            _subscribe = null;
            _raise = null;
            _isStatic = false;
            _eventType = eventType ?? typeof(EventHandler);
            _underlyingMember = null;
        }

        #endregion

        #region Methods

        public EventBuilder<TTarget> Static()
        {
            Should.BeSupported(_attachedHandler == null, nameof(AttachedHandler));
            _isStatic = true;
            return this;
        }

        public EventBuilder<TTarget> UnderlyingMember(object member)
        {
            Should.NotBeNull(member, nameof(member));
            _underlyingMember = member;
            return this;
        }

        public EventBuilder<TTarget> CustomImplementation(TryObserveDelegate<INotifiableMemberInfo, TTarget> subscribe, RaiseDelegate<IObservableMemberInfo, TTarget>? raise = null)
        {
            Should.NotBeNull(subscribe, nameof(subscribe));
            _subscribe = subscribe;
            _raise = raise;
            return this;
        }

        public EventBuilder<TTarget> AttachedHandler(MemberAttachedDelegate<INotifiableMemberInfo, TTarget> attachedHandler)
        {
            Should.NotBeNull(attachedHandler, nameof(attachedHandler));
            Should.BeSupported(!_isStatic, nameof(Static));
            _attachedHandler = attachedHandler;
            return this;
        }

        public INotifiableMemberInfo Build()
        {
            //custom implementation
            if (_subscribe != null && _attachedHandler == null)
                return Event<object?>(null, _subscribe, _raise);

            //auto implementation static
            if (_isStatic)
            {
                var id = GenerateMemberId(true);
                return Event(id, (member, target, listener, metadata) => AttachedMemberBuilder.AddStaticEvent(member.State, listener),
                    (member, target, message, metadata) => AttachedMemberBuilder.RaiseStaticEvent(member.State, message, metadata));
            }

            //auto implementation
            if (_attachedHandler == null)
            {
                var id = GenerateMemberId(true);
                return Event(id, (member, target, listener, metadata) => EventListenerCollection.GetOrAdd(target!, member.State).Add(listener),
                    (member, target, message, metadata) => EventListenerCollection.Raise(target!, member.State, message, metadata));
            }

            //auto implementation with attached handler
            var attachedHandlerId = GenerateMemberId(false);
            if (_subscribe == null)
            {
                var id = GenerateMemberId(true);
                return Event((id, attachedHandlerId, _attachedHandler), (member, target, listener, metadata) =>
                {
                    AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedHandlerId, target, member, member.State._attachedHandler, metadata);
                    return EventListenerCollection.GetOrAdd(target!, member.State.id).Add(listener);
                }, (member, target, message, metadata) => EventListenerCollection.Raise(target!, member.State.id, message, metadata));
            }

            //custom implementation with attached handler
            return Event((_subscribe, attachedHandlerId, _attachedHandler), (member, target, listener, metadata) =>
            {
                AttachedMemberBuilder.RaiseMemberAttached(member.State.attachedHandlerId, target, member, member.State._attachedHandler, metadata);
                return member.State._subscribe(member, target, listener, metadata);
            }, _raise);
        }

        private string GenerateMemberId(bool isEventId)
        {
            return AttachedMemberBuilder.GenerateMemberId(isEventId ? BindingInternalConstant.AttachedEventPrefix : BindingInternalConstant.AttachedHandlerEventPrefix, _declaringType, _name);
        }

        private DelegateObservableMemberInfo<TTarget, TState> Event<TState>(in TState state, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget> tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise = null)
        {
            return new DelegateObservableMemberInfo<TTarget, TState>(_name, _declaringType, _eventType, AttachedMemberBuilder.GetFlags(_isStatic), _underlyingMember, state, tryObserve, raise);
        }

        #endregion
    }
}