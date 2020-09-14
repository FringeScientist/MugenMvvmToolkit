﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Convert;
using MugenMvvm.Binding.Convert.Components;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observation;
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
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
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
        }

        #endregion

        #region Methods

        protected virtual void InitializeThreadDispatcher()
        {
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
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