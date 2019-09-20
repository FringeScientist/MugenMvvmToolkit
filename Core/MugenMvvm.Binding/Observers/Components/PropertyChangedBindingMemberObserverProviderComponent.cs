﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Observers.Components
{
    public sealed class PropertyChangedBindingMemberObserverProviderComponent : IBindingMemberObserverProviderComponent<string>, BindingMemberObserver.IHandler, IHasPriority
    {
        #region Fields

        private readonly IAttachedDictionaryProvider? _attachedDictionaryProvider;
        private static readonly Func<INotifyPropertyChanged, object?, object?, WeakPropertyChangedListener> CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public PropertyChangedBindingMemberObserverProviderComponent(IAttachedDictionaryProvider? attachedDictionaryProvider = null)
        {
            _attachedDictionaryProvider = attachedDictionaryProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public BindingMemberObserver TryGetMemberObserver(Type type, in string member, IReadOnlyMetadataContext? metadata)
        {
            if (typeof(INotifyPropertyChanged).IsAssignableFromUnified(type))
                return new BindingMemberObserver(this, member);
            return default;
        }

        IDisposable? BindingMemberObserver.IHandler.TryObserve(object? source, object member, IBindingEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            if (source == null)
                return null;
            return _attachedDictionaryProvider
                .ServiceIfNull()
                .GetOrAdd((INotifyPropertyChanged)source, BindingInternalConstants.PropertyChangedObserverMember, null, null, CreateWeakPropertyListenerDelegate)
                .Add(listener, (string)member);
        }

        #endregion

        #region Methods

        private static WeakPropertyChangedListener CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object? _, object? __)
        {
            var listener = new WeakPropertyChangedListener();
            propertyChanged.PropertyChanged += listener.Handle;
            return listener;
        }

        #endregion

        #region Nested types

        private sealed class WeakPropertyChangedListener //todo opt
        {
            #region Fields

            private KeyValuePair<WeakBindingEventListener, string>[] _listeners;
            private ushort _removedSize;
            private ushort _size;

            #endregion

            #region Constructors

            public WeakPropertyChangedListener()
            {
                _listeners = Default.EmptyArray<KeyValuePair<WeakBindingEventListener, string>>();
            }

            #endregion

            #region Methods

            public void Handle(object sender, PropertyChangedEventArgs args)
            {
                var hasDeadRef = false;
                var listeners = _listeners;
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (i >= _size)
                        break;
                    var pair = listeners[i];
                    if (pair.Key.IsEmpty)
                    {
                        hasDeadRef = true;
                        continue;
                    }

                    if (MugenExtensions.MemberNameEqual(args.PropertyName, pair.Value, true))
                    {
                        if (!pair.Key.TryHandle(sender, args))
                            hasDeadRef = true;
                    }
                }

                if (hasDeadRef)
                {
                    lock (this)
                    {
                        Cleanup();
                    }
                }
            }

            public IDisposable Add(IBindingEventListener target, string path)
            {
                return AddInternal(target.ToWeak(), path);
            }

            private void Cleanup()
            {
                var size = _size;
                _size = 0;
                _removedSize = 0;
                for (var i = 0; i < size; i++)
                {
                    var reference = _listeners[i];
                    if (reference.Key.IsAlive)
                        _listeners[_size++] = reference;
                }

                if (_size == 0)
                    _listeners = Default.EmptyArray<KeyValuePair<WeakBindingEventListener, string>>();
                else if (_listeners.Length / (float)_size > 2)
                {
                    var listeners = new KeyValuePair<WeakBindingEventListener, string>[_size + (_size >> 2)];
                    Array.Copy(_listeners, 0, listeners, 0, _size);
                    _listeners = listeners;
                }
            }

            private IDisposable AddInternal(WeakBindingEventListener weakItem, string path)
            {
                lock (this)
                {
                    if (_listeners.Length == 0)
                    {
                        _listeners = new[] { new KeyValuePair<WeakBindingEventListener, string>(weakItem, path) };
                        _size = 1;
                        _removedSize = 0;
                    }
                    else
                    {
                        if (_removedSize == 0)
                        {
                            if (_size == _listeners.Length)
                                BindingEventListenerCollection.EnsureCapacity(ref _listeners, _size, _size + 1);
                            _listeners[_size++] = new KeyValuePair<WeakBindingEventListener, string>(weakItem, path);
                        }
                        else
                        {
                            for (var i = 0; i < _size; i++)
                            {
                                if (_listeners[i].Key.IsEmpty)
                                {
                                    _listeners[i] = new KeyValuePair<WeakBindingEventListener, string>(weakItem, path);
                                    --_removedSize;
                                    break;
                                }
                            }
                        }
                    }
                }

                return new Unsubscriber(this, weakItem, path);
            }

            private void Remove(WeakBindingEventListener weakItem, string propertyName)
            {
                lock (this)
                {
                    for (var i = 0; i < _listeners.Length; i++)
                    {
                        var pair = _listeners[i];
                        if (!pair.Key.IsEmpty && pair.Value == propertyName && ReferenceEquals(pair.Key.Source, weakItem.Source))
                        {
                            ++_removedSize;
                            _listeners[i] = default;
                            return;
                        }
                    }
                }
            }

            #endregion

            #region Nested types

            private sealed class Unsubscriber : IDisposable
            {
                #region Fields

                private readonly string _propertyName;

                private WeakPropertyChangedListener? _eventListener;
                private WeakBindingEventListener _weakItem;

                #endregion

                #region Constructors

                public Unsubscriber(WeakPropertyChangedListener eventListener, WeakBindingEventListener weakItem, string propertyName)
                {
                    _eventListener = eventListener;
                    _weakItem = weakItem;
                    _propertyName = propertyName;
                }

                #endregion

                #region Implementation of interfaces

                public void Dispose()
                {
                    var listener = _eventListener;
                    var weakItem = _weakItem;
                    if (listener != null && !weakItem.IsEmpty)
                    {
                        _eventListener = null;
                        _weakItem = default;
                        listener.Remove(weakItem, _propertyName);
                    }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}