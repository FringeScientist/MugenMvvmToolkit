﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvm.Commands;
using MugenMvvm.Delegates;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Threading.Components;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IEntityTrackingCollection GetTrackingCollection<TRequest>(this IEntityManager entityManager, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            var collection = entityManager.TryGetTrackingCollection(request, metadata);
            if (collection == null)
                ExceptionManager.ThrowObjectNotInitialized<IEntityTrackingCollectionProviderComponent>(entityManager);
            return collection;
        }

        public static IEntityStateSnapshot GetSnapshot<TState>(this IEntityManager entityManager, object entity, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(entityManager, nameof(entityManager));
            var snapshot = entityManager.TryGetSnapshot(entity, state, metadata);
            if (snapshot == null)
                ExceptionManager.ThrowObjectNotInitialized<IEntityStateSnapshotProviderComponent>(entityManager);
            return snapshot;
        }

        public static Stream Serialize<TRequest>(this ISerializer serializer, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            var result = serializer.TrySerialize(request, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<ISerializerComponent>(serializer);
            return result;
        }

        public static object? Deserialize(this ISerializer serializer, Stream stream, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            if (!serializer.TryDeserialize(stream, metadata, out var result))
                ExceptionManager.ThrowObjectNotInitialized<ISerializerComponent>(serializer);
            return result;
        }

        public static IValidator GetValidator<TRequest>(this IValidatorProvider validatorProvider, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validatorProvider, nameof(validatorProvider));
            var result = validatorProvider.TryGetValidator(request, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IValidatorProviderComponent>(validatorProvider);
            return result;
        }

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> Show<TRequest>(this IPresenter presenter, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(presenter, nameof(presenter));
            var result = presenter.TryShow(request, cancellationToken, metadata);
            if (result.IsNullOrEmpty())
                ExceptionManager.ThrowPresenterCannotShowRequest(request, metadata);
            return result;
        }

        public static IWeakReference GetWeakReference(this IWeakReferenceProvider weakReferenceProvider, object? item, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(weakReferenceProvider, nameof(weakReferenceProvider));
            var result = weakReferenceProvider.TryGetWeakReference(item, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<IWeakReferenceProviderComponent>(weakReferenceProvider);
            return result;
        }

        public static IComponentCollection GetComponentCollection(this IComponentCollectionProvider provider, object owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(provider, nameof(provider));
            Should.NotBeNull(owner, nameof(owner));
            var collection = provider.TryGetComponentCollection(owner, metadata);
            if (collection == null)
                ExceptionManager.ThrowObjectNotInitialized<IComponentCollectionProviderComponent>(provider);
            return collection;
        }

        public static void SetErrors(this IValidator validator, object target, string memberName, ItemOrList<object, IReadOnlyList<object>> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(validator, nameof(validator));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(memberName, nameof(memberName));
            InlineValidatorComponent? component = null;
            var components = validator.GetComponents<InlineValidatorComponent>();
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].Target == target)
                {
                    component = components[i];
                    break;
                }
            }

            if (component == null)
            {
                component = new InlineValidatorComponent(target);
                validator.AddComponent(component);
            }

            component.SetErrors(memberName, errors, metadata);
        }

        public static void Execute<THandler, TState>(IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, [DisallowNull] in THandler genericHandler, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (!threadDispatcher.DefaultIfNull().TryExecute(executionMode, genericHandler, state, metadata))
                ExceptionManager.ThrowObjectNotInitialized<IThreadDispatcherComponent>(threadDispatcher.DefaultIfNull());
        }

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, Action action, IReadOnlyMetadataContext? metadata = null)
        {
            Execute(threadDispatcher, executionMode, action, (object?)null, metadata);
        }

        public static void Execute<TState>(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, in TState state, Action<TState> action, IReadOnlyMetadataContext? metadata = null)
        {
            Execute(threadDispatcher, executionMode, action, state, metadata);
        }

        public static void Execute(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, IThreadDispatcherHandler handler, IReadOnlyMetadataContext? metadata = null)
        {
            Execute(threadDispatcher, executionMode, handler, (object?)null, metadata);
        }

        public static void Execute<TState>(this IThreadDispatcher? threadDispatcher, ThreadExecutionMode executionMode, IThreadDispatcherHandler<TState> handler, TState state, IReadOnlyMetadataContext? metadata = null)
        {
            Execute(threadDispatcher, executionMode, genericHandler: handler, state, metadata);
        }

        public static TValue GetOrAdd<TItem, TValue>(this IAttachedValueProvider valueProvider, TItem item, string path, Func<TItem, TValue> valueFactory)
            where TItem : class
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.GetOrAdd(item, path, valueFactory, (it, s) => s(it));
        }

        public static void Set<TValue>(this IAttachedValueProvider valueProvider, object item, string path, TValue value)
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            valueProvider.Set(item, path, value, out _);
        }

        public static bool Clear(this IAttachedValueProvider valueProvider, object item, string path)
        {
            Should.NotBeNull(valueProvider, nameof(valueProvider));
            return valueProvider.Clear(item, path, out _);
        }

        public static ICompositeCommand GetCommand(this ICommandProvider? mediatorProvider, Action execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<object>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<T>(this ICommandProvider? mediatorProvider, Action<T> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<T>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand(this ICommandProvider? mediatorProvider, Func<Task> execute, Func<bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<object>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<T>(this ICommandProvider? mediatorProvider, Func<T, Task> execute, Func<T, bool>? canExecute = null, bool? allowMultipleExecution = null,
            CommandExecutionMode? executionMode = null, ThreadExecutionMode? eventThreadMode = null, IReadOnlyList<object>? notifiers = null, Func<object, bool>? canNotify = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetCommandInternal<T>(mediatorProvider, execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify, metadata);
        }

        public static ICompositeCommand GetCommand<TRequest>(this ICommandProvider commandProvider, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(commandProvider, nameof(commandProvider));
            var result = commandProvider.TryGetCommand(request, metadata);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized<ICommandProviderComponent>(commandProvider);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BusyMessageHandlerType value, BusyMessageHandlerType flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingFlags value, BindingFlags flag)
        {
            return (value & flag) == flag;
        }

        public static TTo CastGeneric<TFrom, TTo>(in TFrom value)
        {
            if (typeof(TFrom) == typeof(TTo))
                return ((FuncIn<TFrom, TTo>)(object)GenericCaster<TFrom>.Cast).Invoke(value);
            return (TTo)(object)value!;
        }

        public static bool MemberNameEqual(string changedMember, string listenedMember, bool emptyListenedMemberResult = false)
        {
            if (string.Equals(changedMember, listenedMember) || string.IsNullOrEmpty(changedMember))
                return true;
            if (string.IsNullOrEmpty(listenedMember))
                return emptyListenedMemberResult;

            if (listenedMember[0] == '[')
            {
                if (Default.IndexerName.Equals(changedMember))
                    return true;
                if (changedMember.StartsWith("Item[", StringComparison.Ordinal))
                {
                    int i = 4, j = 0;
                    while (i < changedMember.Length)
                    {
                        if (j >= listenedMember.Length)
                            return false;
                        var c1 = changedMember[i];
                        var c2 = listenedMember[j];
                        if (c1 == c2)
                        {
                            ++i;
                            ++j;
                        }
                        else if (c1 == '"')
                            ++i;
                        else if (c2 == '"')
                            ++j;
                        else
                            return false;
                    }

                    return j == listenedMember.Length;
                }
            }

            return false;
        }

        [StringFormatMethod("format")]
        public static string Format(this string format, params object?[] args)
        {
            return string.Format(format, args);
        }

        public static void TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task, TaskContinuationOptions continuationOptions = TaskContinuationOptions.ExecuteSynchronously)
        {
            Should.NotBeNull(tcs, nameof(tcs));
            Should.NotBeNull(task, nameof(task));
            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                        tcs.TrySetCanceled();
                        break;
                    case TaskStatus.Faulted:
                        tcs.TrySetException(task.Exception.InnerExceptions);
                        break;
                    case TaskStatus.RanToCompletion:
                        var t = task as Task<TResult>;
                        tcs.TrySetResult(t == null ? default! : t.Result);
                        break;
                }
            }
            else
                task.ContinueWith((t, o) => ((TaskCompletionSource<TResult>)o).TrySetFromTask(t), tcs, continuationOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Execute<TState>(this IThreadDispatcherHandler<TState> handler, object state)
        {
            handler.Execute((TState)state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Invoke<TState>(this Action<TState> handler, object state)
        {
            handler.Invoke((TState)state);
        }

        internal static void ReleaseWeakReference(this IValueHolder<IWeakReference>? valueHolder)
        {
            valueHolder?.Value?.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IWeakReference ToWeakReference(this object? item)
        {
            return MugenService.WeakReferenceProvider.GetWeakReference(item);
        }

        internal static void TrySetExceptionEx<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            if (e is AggregateException aggregateException)
                tcs.TrySetException(aggregateException.InnerExceptions);
            else
                tcs.SetException(e);
        }

        internal static bool LazyInitialize<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class
        {
            return Interlocked.CompareExchange(ref item, value, null) == null;
        }

        internal static bool LazyInitializeDisposable<T>([NotNullIfNotNull("value")] ref T? item, T value) where T : class, IDisposable
        {
            if (!LazyInitialize(ref item, value))
            {
                value.Dispose();
                return false;
            }

            return true;
        }

#if SPAN_API
        //https://github.com/dotnet/runtime/pull/295
        internal static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator)
            => new SpanSplitEnumerator<char>(span, separator);
#endif

        private static ICompositeCommand GetCommandInternal<T>(ICommandProvider? mediatorProvider, Delegate execute, Delegate? canExecute, bool? allowMultipleExecution, CommandExecutionMode? executionMode,
            ThreadExecutionMode? eventThreadMode, IReadOnlyList<object>? notifiers, Func<object, bool>? canNotify, IReadOnlyMetadataContext? metadata)
        {
            var request = new DelegateCommandRequest((in DelegateCommandRequest r, DelegateCommandRequest.IProvider provider, IReadOnlyMetadataContext? m) => provider.TryGetCommand<T>(r, m),
                execute, canExecute, allowMultipleExecution, executionMode, eventThreadMode, notifiers, canNotify);
            return mediatorProvider.DefaultIfNull().GetCommand(request, metadata);
        }

        #endregion

        #region Nested types

#if SPAN_API
        public ref struct SpanSplitEnumerator<T> where T : IEquatable<T>
        {
            private readonly ReadOnlySpan<T> _buffer;

            private readonly ReadOnlySpan<T> _separators;
            private readonly T _separator;

            private readonly int _separatorLength;
            private readonly bool _splitOnSingleToken;

            private readonly bool _isInitialized;

            private int _startCurrent;
            private int _endCurrent;
            private int _startNext;

            /// <summary>
            /// Returns an enumerator that allows for iteration over the split span.
            /// </summary>
            /// <returns>Returns a <see cref="System.SpanSplitEnumerator{T}"/> that can be used to iterate over the split span.</returns>
            public SpanSplitEnumerator<T> GetEnumerator() => this;

            /// <summary>
            /// Returns the current element of the enumeration.
            /// </summary>
            /// <returns>Returns a <see cref="System.Range"/> instance that indicates the bounds of the current element withing the source span.</returns>
            public Range Current => new Range(_startCurrent, _endCurrent);

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separators)
            {
                _isInitialized = true;
                _buffer = span;
                _separators = separators;
                _separator = default!;
                _splitOnSingleToken = false;
                _separatorLength = _separators.Length != 0 ? _separators.Length : 1;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, T separator)
            {
                _isInitialized = true;
                _buffer = span;
                _separator = separator;
                _separators = default;
                _splitOnSingleToken = true;
                _separatorLength = 1;
                _startCurrent = 0;
                _endCurrent = 0;
                _startNext = 0;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the enumeration.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the enumeration.</returns>
            public bool MoveNext()
            {
                if (!_isInitialized || _startNext > _buffer.Length)
                {
                    return false;
                }

                ReadOnlySpan<T> slice = _buffer.Slice(_startNext);
                _startCurrent = _startNext;

                int separatorIndex = _splitOnSingleToken ? slice.IndexOf(_separator) : slice.IndexOf(_separators);
                int elementLength = (separatorIndex != -1 ? separatorIndex : slice.Length);

                _endCurrent = _startCurrent + elementLength;
                _startNext = _endCurrent + _separatorLength;
                return true;
            }
        }
#endif

        private static class GenericCaster<T>
        {
            #region Fields

            public static readonly FuncIn<T, T> Cast = (in T arg1) => arg1;

            #endregion
        }

        #endregion
    }
}