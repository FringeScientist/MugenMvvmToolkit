﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views.Components
{
    public sealed class ExecutionModeDecoratorViewInitializerComponent : DecoratorComponentBase<IViewManager, IViewInitializerComponent>, IViewInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        public ExecutionModeDecoratorViewInitializerComponent(IThreadDispatcher? threadDispatcher = null)
        {
            _threadDispatcher = threadDispatcher;
            InitializeExecutionMode = ThreadExecutionMode.Main;
            CleanupExecutionMode = ThreadExecutionMode.MainAsync;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        public ThreadExecutionMode InitializeExecutionMode { get; set; }

        public ThreadExecutionMode CleanupExecutionMode { get; set; }

        #endregion

        #region Implementation of interfaces

        public Task<ViewInitializationResult>? TryInitializeAsync(IViewModelViewMapping mapping, object? view, IViewModelBase? viewModel, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(InitializeExecutionMode))
                return Components.TryInitializeAsync(mapping, view, viewModel, cancellationToken, metadata);

            var tcs = new TaskCompletionSource<ViewInitializationResult>();
            var valueTuple = (this, tcs, mapping, view, viewModel, cancellationToken, metadata);
            dispatcher.Execute(InitializeExecutionMode, state =>
            {
                try
                {
                    if (state.cancellationToken.IsCancellationRequested)
                    {
                        state.tcs.TrySetCanceled(state.cancellationToken);
                        return;
                    }

                    var task = state.Item1.Components.TryInitializeAsync(state.mapping, state.view, state.viewModel, state.cancellationToken, state.metadata);
                    if (task == null)
                        ExceptionManager.ThrowObjectNotInitialized(state.Item1);
                    state.tcs.TrySetFromTask(task);
                }
                catch (Exception e)
                {
                    state.tcs.TrySetException(e);
                }
            }, valueTuple, metadata: metadata);
            return tcs.Task;
        }


        public Task? TryCleanupAsync(IView view, IViewModelBase? viewModel, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(CleanupExecutionMode))
                return Components.TryCleanupAsync(view, viewModel, cancellationToken, metadata);

            var tcs = new TaskCompletionSource<ViewInitializationResult>();
            var valueTuple = (this, tcs, view, viewModel, cancellationToken, metadata);
            dispatcher.Execute(CleanupExecutionMode, state =>
            {
                try
                {
                    if (state.cancellationToken.IsCancellationRequested)
                    {
                        state.tcs.TrySetCanceled(state.cancellationToken);
                        return;
                    }

                    var task = state.Item1.Components.TryCleanupAsync(state.view, state.viewModel, state.cancellationToken, state.metadata);
                    if (task == null)
                        ExceptionManager.ThrowObjectNotInitialized(state.Item1);
                    state.tcs.TrySetFromTask(task);
                }
                catch (Exception e)
                {
                    state.tcs.TrySetException(e);
                }
            }, valueTuple, metadata: metadata);
            return tcs.Task;
        }

        #endregion
    }
}