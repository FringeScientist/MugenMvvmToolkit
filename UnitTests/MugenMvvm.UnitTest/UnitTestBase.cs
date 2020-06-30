﻿using System;
using MugenMvvm.Binding.Convert;
using MugenMvvm.Binding.Convert.Components;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Binding.Resources.Components;
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
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Threading.Internal;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MugenMvvm.UnitTest
{
    public class UnitTestBase
    {
        #region Fields

        protected static readonly IReadOnlyMetadataContext DefaultMetadata = new ReadOnlyMetadataContext(Default.Array<MetadataContextValue>());

        #endregion

        #region Constructors

        public UnitTestBase()
        {
            MugenService.Configuration.InitializeFallback(null);
            MugenService.Configuration.InitializeInstance<IComponentCollectionProvider>(new ComponentCollectionProvider());

            var attachedValueProvider = new AttachedValueProvider();
            attachedValueProvider.AddComponent(new ConditionalWeakTableAttachedValueProvider());
            MugenService.Configuration.InitializeInstance<IAttachedValueProvider>(attachedValueProvider);

            var metadataContextProvider = new MugenMvvm.Metadata.MetadataContextProvider();
            metadataContextProvider.AddComponent(new MugenMvvm.Metadata.Components.MetadataContextProviderComponent());
            MugenService.Configuration.InitializeInstance<IMetadataContextProvider>(metadataContextProvider);

            var weakReferenceProvider = new WeakReferenceProvider();
            weakReferenceProvider.AddComponent(new WeakReferenceProviderComponent());
            MugenService.Configuration.InitializeInstance<IWeakReferenceProvider>(weakReferenceProvider);

            InitializeThreadDispatcher();

            var reflectionDelegateProvider = new ReflectionDelegateProvider();
            reflectionDelegateProvider.AddComponent(new ExpressionReflectionDelegateProvider());
            MugenService.Configuration.InitializeInstance<IReflectionDelegateProvider>(reflectionDelegateProvider);

            var commandProvider = new CommandManager();
            MugenService.Configuration.InitializeInstance<ICommandManager>(commandProvider);

            var converter = new GlobalValueConverter();
            converter.AddComponent(new GlobalValueConverterComponent());
            MugenService.Configuration.InitializeInstance<IGlobalValueConverter>(converter);

            var resourceResolver = new ResourceResolver();
            resourceResolver.AddComponent(new TypeResolverComponent());
            MugenService.Configuration.InitializeInstance<IResourceResolver>(resourceResolver);

            var memberManager = new MemberManager();
            MugenService.Configuration.InitializeInstance<IMemberManager>(memberManager);

            IBindingManager bindingManager = new BindingManager();
            MugenService.Configuration.InitializeInstance(bindingManager);
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