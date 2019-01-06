﻿using System;
using System.Collections.Generic;
using MugenMvvm.Delegates;

namespace MugenMvvm.Interfaces
{
    public interface IAttachedValueProvider
    {
        TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, TValue addValue,
            UpdateValueDelegate<TItem, TValue, TValue, TState1, TState2> updateValueFactory, TState1 state1, TState2 state2);

        TValue AddOrUpdate<TItem, TValue, TState1, TState2>(TItem item, string path, Func<TItem, TState1, TState2, TValue> addValueFactory,
            UpdateValueDelegate<TItem, Func<TItem, TState1, TState2, TValue>, TValue, TState1, TState2> updateValueFactory, TState1 state1, TState2 state2);

        TValue GetOrAdd<TItem, TValue, TState1, TState2>(TItem item, string path, Func<TItem, TState1, TState2, TValue> valueFactory, TState1 state1, TState2 state2);

        TValue GetOrAdd<TValue>(object item, string path, TValue value);

        bool TryGetValue<TValue>(object item, string path, out TValue value);

        void SetValue(object item, string path, object? value);

        bool Contains(object item, string path);

        IReadOnlyList<KeyValuePair<string, object?>> GetValues(object item, Func<string, object?, bool>? predicate);

        bool Clear(object item);

        bool Clear(object item, string path);
    }
}