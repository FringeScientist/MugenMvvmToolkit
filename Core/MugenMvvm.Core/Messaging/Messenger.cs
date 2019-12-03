﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;

namespace MugenMvvm.Messaging
{
    public sealed class Messenger : ComponentOwnerBase<IMessenger>, IMessenger, IComponentOwnerAddedCallback, IComponentOwnerRemovedCallback, IEqualityComparer<MessengerSubscriberInfo>
    {
        #region Fields

        private readonly TypeLightDictionary<ThreadExecutionModeDictionary?> _cache;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        private readonly HashSet<MessengerSubscriberInfo> _subscribers;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        public Messenger(IThreadDispatcher? threadDispatcher = null,
            IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null) : base(componentCollectionProvider)
        {
            _cache = new TypeLightDictionary<ThreadExecutionModeDictionary?>(3);
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _subscribers = new HashSet<MessengerSubscriberInfo>(this);
        }

        #endregion

        #region Implementation of interfaces

        void IComponentOwnerAddedCallback.OnComponentAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!(component is IMessengerHandlerComponent))
                return;
            lock (_subscribers)
            {
                _cache.Clear();
            }
        }

        void IComponentOwnerRemovedCallback.OnComponentRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (!(component is IMessengerHandlerComponent))
                return;
            lock (_subscribers)
            {
                _cache.Clear();
            }
        }

        bool IEqualityComparer<MessengerSubscriberInfo>.Equals(MessengerSubscriberInfo x, MessengerSubscriberInfo y)
        {
            return x.Subscriber.Equals(y.Subscriber);
        }

        int IEqualityComparer<MessengerSubscriberInfo>.GetHashCode(MessengerSubscriberInfo obj)
        {
            return obj.Subscriber.GetHashCode();
        }

        public void Dispose()
        {
            this.UnsubscribeAll();
            this.ClearComponents(null);
        }

        public bool Subscribe(object subscriber, ThreadExecutionMode? executionMode = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            if (executionMode == null)
                executionMode = ThreadExecutionMode.Current;

            var components = GetComponents<IMessengerSubscriberComponent>(metadata);
            var added = false;
            for (var i = 0; i < components.Length; i++)
            {
                var handler = components[i].TryGetSubscriber(subscriber, executionMode, metadata);
                if (handler == null)
                    continue;

                if (AddSubscriber(handler, executionMode, metadata))
                    added = true;
            }

            return added;
        }

        public bool Unsubscribe(object subscriber, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(subscriber, nameof(subscriber));
            var removed = false;
            lock (_subscribers)
            {
                var info = new MessengerSubscriberInfo(subscriber, ThreadExecutionMode.Current);
                while (_subscribers.Remove(info))
                    removed = true;

                if (removed)
                    _cache.Clear();
            }

            if (removed)
            {
                var components = GetComponents<IMessengerSubscriberListener>(metadata);
                for (var i = 0; i < components.Length; i++)
                    components[i].OnUnsubscribed(this, subscriber, metadata);
            }

            return removed;
        }

        public IReadOnlyList<MessengerSubscriberInfo> GetSubscribers()
        {
            lock (_subscribers)
            {
                return _subscribers.ToArray();
            }
        }

        public IMessageContext Publish(object? sender, object message, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(message, nameof(message));
            IMessageContext? ctx = null;
            var providers = GetComponents<IMessageContextProviderComponent>(metadata);
            for (var i = 0; i < providers.Length; i++)
            {
                ctx = providers[i].TryGetMessengerContext(sender, message, metadata);
                if (ctx != null)
                    break;
            }

            if (ctx == null)
                ctx = new MessageContext(this, sender, message, metadata);

            Publish(ctx);
            return ctx;
        }

        public void Publish(IMessageContext messageContext)
        {
            Should.NotBeNull(messageContext, nameof(messageContext));
            var threadDispatcher = _threadDispatcher.DefaultIfNull();
            ThreadExecutionModeDictionary? dictionary;
            lock (_subscribers)
            {
                var key = messageContext.Message.GetType();
                if (!_cache.TryGetValue(key, out dictionary))
                {
                    dictionary = GetHandlers(key);
                    _cache[key] = dictionary;
                }
            }

            if (dictionary != null)
            {
                foreach (var dispatcherExecutor in dictionary)
                    threadDispatcher.Execute(dispatcherExecutor.Key, dispatcherExecutor.Value, messageContext);
            }
        }

        #endregion

        #region Methods

        private ThreadExecutionModeDictionary? GetHandlers(Type messageType)
        {
            ThreadExecutionModeDictionary? dictionary = null;
            foreach (var subscriber in _subscribers)
            {
                var canHandle = false;
                var handlerComponents = GetComponents<IMessengerHandlerComponent>(null);
                for (var i = 0; i < handlerComponents.Length; i++)
                {
                    if (handlerComponents[i].CanHandle(subscriber.Subscriber, messageType))
                    {
                        canHandle = true;
                        break;
                    }
                }

                if (!canHandle)
                    continue;

                if (dictionary == null)
                    dictionary = new ThreadExecutionModeDictionary();

                if (!dictionary.TryGetValue(subscriber.ExecutionMode, out var value))
                {
                    value = new MessageThreadExecutor(this);
                    dictionary[subscriber.ExecutionMode] = value;
                }

                value.Add(subscriber.Subscriber);
            }

            return dictionary;
        }

        private bool AddSubscriber(object subscriber, ThreadExecutionMode executionMode, IReadOnlyMetadataContext? metadata)
        {
            bool added;
            lock (_subscribers)
            {
                added = _subscribers.Add(new MessengerSubscriberInfo(subscriber, executionMode));
                if (added)
                    _cache.Clear();
            }

            var components = GetComponents<IMessengerSubscriberListener>(metadata);
            if (added)
            {
                for (var i = 0; i < components.Length; i++)
                    components[i].OnSubscribed(this, subscriber, executionMode, metadata);
            }

            return added;
        }

        #endregion

        #region Nested types

        private sealed class ThreadExecutionModeDictionary : LightDictionary<ThreadExecutionMode, MessageThreadExecutor>
        {
            #region Constructors

            public ThreadExecutionModeDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(ThreadExecutionMode x, ThreadExecutionMode y)
            {
                return x == y;
            }

            protected override int GetHashCode(ThreadExecutionMode key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class MessageThreadExecutor : List<object>, IThreadDispatcherHandler<IMessageContext>, IValueHolder<Delegate>
        {
            #region Fields

            private readonly Messenger _messenger;

            #endregion

            #region Constructors

            public MessageThreadExecutor(Messenger messenger)
            {
                _messenger = messenger;
            }

            #endregion

            #region Properties

            Delegate? IValueHolder<Delegate>.Value { get; set; }

            #endregion

            #region Implementation of interfaces

            public void Execute(IMessageContext messageContext)
            {
                var metadata = messageContext.GetMetadataOrDefault();
                var handlers = _messenger.GetComponents<IMessengerHandlerComponent>(metadata);
                var listeners = _messenger.GetComponents<IMessengerHandlerListener>(metadata);
                for (var i = 0; i < Count; i++)
                {
                    var subscriber = this[i];
                    for (var j = 0; j < listeners.Length; j++)
                        listeners[j].OnHandling(subscriber, messageContext);

                    MessengerResult? result = null;
                    for (var j = 0; j < handlers.Length; j++)
                    {
                        result = handlers[j].TryHandle(subscriber, messageContext);
                        if (result != null)
                            break;
                    }

                    for (var j = 0; j < listeners.Length; j++)
                        listeners[j].OnHandled(result, subscriber, messageContext);

                    if (result == MessengerResult.Invalid)
                        _messenger.Unsubscribe(subscriber, messageContext.Metadata);
                }
            }

            #endregion
        }

        public sealed class MessageContext : IMessageContext
        {
            #region Fields

            private readonly Messenger _messenger;
            private IReadOnlyMetadataContext? _metadata;

            #endregion

            #region Constructors

            public MessageContext(Messenger messenger, object? sender, object message, IReadOnlyMetadataContext? metadata)
            {
                _metadata = metadata;
                _messenger = messenger;
                Sender = sender;
                Message = message;
            }

            #endregion

            #region Properties

            public bool HasMetadata => !_metadata.IsNullOrEmpty();

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata is IMetadataContext ctx)
                        return ctx;
                    return _messenger._metadataContextProvider.LazyInitializeNonReadonly(ref _metadata, this);
                }
            }

            public object? Sender { get; }

            public object Message { get; }

            #endregion
        }

        #endregion
    }
}