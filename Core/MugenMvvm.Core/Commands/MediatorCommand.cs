﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MugenMvvm.Commands.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Commands
{
    public class MediatorCommand : IMediatorCommand, IValueHolder<IWeakReference>
    {
        #region Fields

        private ICommandMediator? _mediator;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        private MediatorCommand(IReadOnlyMetadataContext metadata)
        {
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasCanExecute => Mediator.HasCanExecute();

        public ICommandMediator Mediator
        {
            get
            {
                if (_mediator == null)
                {
                    var metadata = _metadata;
                    _metadata = null;
                    if (metadata != null)
                        MugenExtensions.LazyInitializeDisposable(ref _mediator, GetMediator(metadata));
                }

                return _mediator!;
            }
        }

        IWeakReference? IValueHolder<IWeakReference>.Value { get; set; }

        public bool IsSuspended => Mediator.IsSuspended;

        #endregion

        #region Events

        public event EventHandler CanExecuteChanged
        {
            add => Mediator.AddCanExecuteChanged(value);
            remove => Mediator.RemoveCanExecuteChanged(value);
        }

        #endregion

        #region Implementation of interfaces

        public void RaiseCanExecuteChanged()
        {
            Mediator.RaiseCanExecuteChanged();
        }

        public bool CanExecute(object parameter)
        {
            return Mediator.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            Mediator.ExecuteAsync(parameter);
        }

        public void Dispose()
        {
            Mediator.Dispose();
        }

        public ActionToken Suspend()
        {
            return Mediator.Suspend();
        }

        #endregion

        #region Methods

        protected virtual ICommandMediator GetMediator(IReadOnlyMetadataContext metadata)
        {
            return MugenService.CommandMediatorProvider.GetCommandMediator(this, metadata);
        }

        public static MediatorCommand Create(Action execute, IReadOnlyMetadataContext? metadata = null)
        {
            return Create(execute, null, null, null, metadata);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand Create<T>(Action<T> execute, IReadOnlyMetadataContext? metadata = null)
        {
            return Create(execute, null, null, null, metadata);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return Create(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata,
            params object[] notifiers)
        {
            return Create(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, IReadOnlyMetadataContext? metadata = null)
        {
            return CreateFromTask(execute, null, null, null, metadata);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, bool allowMultipleExecution, IReadOnlyMetadataContext? metadata,
            params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, IReadOnlyMetadataContext? metadata = null)
        {
            return CreateFromTask(execute, null, null, null, metadata);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, null);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, null, notifiers, metadata);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, bool allowMultipleExecution, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: null);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, bool allowMultipleExecution,
            IReadOnlyMetadataContext? metadata, params object[] notifiers)
        {
            return CreateFromTask(execute, canExecute, allowMultipleExecution, notifiers, metadata: metadata);
        }

        public static MediatorCommand Create(Action execute, Func<bool>? canExecute, bool? allowMultipleExecution,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return GetMediatorCommand<object>(execute, canExecute, request);
        }

        public static MediatorCommand Create<T>(Action<T> execute, Func<T, bool>? canExecute, bool? allowMultipleExecution,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return GetMediatorCommand<T>(execute, canExecute, request);
        }

        public static MediatorCommand CreateFromTask(Func<Task> execute, Func<bool>? canExecute, bool? allowMultipleExecution,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return GetMediatorCommand<object>(execute, canExecute, request);
        }

        public static MediatorCommand CreateFromTask<T>(Func<T, Task> execute, Func<T, bool>? canExecute, bool? allowMultipleExecution,
            IReadOnlyCollection<object>? notifiers, IReadOnlyMetadataContext? metadata)
        {
            var request = metadata.ToNonReadonly();
            if (allowMultipleExecution.HasValue)
                request.Set(MediatorCommandMetadata.AllowMultipleExecution, allowMultipleExecution.Value);
            if (notifiers != null && notifiers.Count != 0)
                request.Set(MediatorCommandMetadata.Notifiers, notifiers);
            return GetMediatorCommand<T>(execute, canExecute, request);
        }

        private static MediatorCommand GetMediatorCommand<T>(Delegate execute, Delegate? canExecute, IMetadataContext metadata)
        {
            var commandExecutor = new DelegateExecutorCommandMediatorComponent<T>(execute, canExecute);
            metadata.Set(MediatorCommandMetadata.Executor, commandExecutor);
            return new MediatorCommand(metadata);
        }

        #endregion
    }
}