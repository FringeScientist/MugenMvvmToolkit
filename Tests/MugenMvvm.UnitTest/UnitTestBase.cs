﻿using System;
using MugenMvvm.Commands;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Metadata.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Threading;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MugenMvvm.UnitTest
{
    public class UnitTestBase
    {
        #region Fields

        protected static readonly IReadOnlyMetadataContext DefaultMetadata = new ReadOnlyMetadataContext(Default.EmptyArray<MetadataContextValue>());

        #endregion

        #region Constructors

        public UnitTestBase()
        {
            MugenService.Configuration.InitializeInstance<IComponentCollectionProvider>(new ComponentCollectionProvider());

            var metadataContextProvider = new MetadataContextProvider();
            metadataContextProvider.AddComponent(new MetadataContextProviderComponent());
            MugenService.Configuration.InitializeInstance<IMetadataContextProvider>(metadataContextProvider);

            var weakReferenceProvider = new WeakReferenceProvider();
            weakReferenceProvider.AddComponent(new WeakReferenceProviderComponent());
            MugenService.Configuration.InitializeInstance<IWeakReferenceProvider>(weakReferenceProvider);

            InitializeThreadDispatcher();

            var reflectionDelegateProvider = new ReflectionDelegateProvider();
            reflectionDelegateProvider.AddComponent(new ExpressionReflectionDelegateProviderComponent());
            MugenService.Configuration.InitializeInstance<IReflectionDelegateProvider>(reflectionDelegateProvider);

            var commandProvider = new CommandProvider();
            MugenService.Configuration.InitializeInstance<ICommandProvider>(commandProvider);
        }

        #endregion

        #region Methods

        protected virtual void InitializeThreadDispatcher()
        {
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            MugenService.Configuration.InitializeInstance<IThreadDispatcher>(threadDispatcher);
        }

        protected static void ShouldThrow<T>(Action action) where T : Exception
        {
            Assert.Throws<T>(action);
        }

        protected void ShouldThrow(Action action)
        {
            Assert.ThrowsAny<Exception>(action);
        }

        protected static Exception GetOriginalException(AggregateException aggregateException)
        {
            Exception exception = aggregateException;
            while (aggregateException != null)
            {
                exception = aggregateException.InnerException;
                aggregateException = (exception as AggregateException)!;
            }

            return exception;
        }

        #endregion
    }
}