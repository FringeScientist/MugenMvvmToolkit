﻿using System;
using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Bindings.Convert;
using MugenMvvm.Bindings.Convert.Components;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Commands;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Threading.Internal;
using MugenMvvm.ViewModels;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MugenMvvm.UnitTests
{
    public class UnitTestBase
    {
        #region Fields

        protected static readonly IReadOnlyMetadataContext DefaultMetadata = new ReadOnlyMetadataContext(Default.Array<KeyValuePair<IMetadataContextKey, object?>>());

        #endregion

        #region Constructors

        public UnitTestBase()
        {
            MugenService.Configuration.InitializeFallback(null);
            MugenService.Configuration.InitializeInstance<IComponentCollectionManager>(new ComponentCollectionManager());

            var attachedValueManager = new AttachedValueManager();
            attachedValueManager.AddComponent(new ConditionalWeakTableAttachedValueStorage());
            attachedValueManager.AddComponent(new StaticTypeAttachedValueStorage());
            MugenService.Configuration.InitializeInstance<IAttachedValueManager>(attachedValueManager);

            var weakReferenceManager = new WeakReferenceManager();
            weakReferenceManager.AddComponent(new WeakReferenceProviderComponent());
            MugenService.Configuration.InitializeInstance<IWeakReferenceManager>(weakReferenceManager);

            InitializeThreadDispatcher();

            var reflectionManager = new ReflectionManager();
            reflectionManager.AddComponent(new ExpressionReflectionDelegateProvider());
            MugenService.Configuration.InitializeInstance<IReflectionManager>(reflectionManager);

            var commandManager = new CommandManager();
            MugenService.Configuration.InitializeInstance<ICommandManager>(commandManager);

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

            IObservationManager observationManager = new ObservationManager();
            MugenService.Configuration.InitializeInstance(observationManager);

            IViewModelManager viewModelManager = new ViewModelManager();
            MugenService.Configuration.InitializeInstance(viewModelManager);

            IMessenger messenger = new Messenger();
            MugenService.Configuration.InitializeInstance(messenger);
        }

        #endregion

        #region Methods

        protected static void WaitCompletion(int milliseconds = 10) => Thread.Sleep(milliseconds);

        protected virtual void InitializeThreadDispatcher()
        {
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent {Priority = int.MinValue});
            MugenService.Configuration.InitializeInstance<IThreadDispatcher>(threadDispatcher);
        }

        protected static void ShouldThrow<T>(Action action) where T : Exception => Assert.Throws<T>(action);

        protected void ShouldThrow(Action action) => Assert.ThrowsAny<Exception>(action);

        protected static Exception GetOriginalException(AggregateException aggregateException)
        {
            Exception exception = aggregateException;
            while (aggregateException != null)
            {
                exception = aggregateException.InnerException!;
                aggregateException = (exception as AggregateException)!;
            }

            return exception;
        }

        #endregion
    }
}