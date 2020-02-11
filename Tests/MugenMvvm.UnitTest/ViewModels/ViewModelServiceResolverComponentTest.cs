﻿using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.ViewModels
{
    public class ViewModelServiceResolverComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetServiceShouldReturnMetadataContext()
        {
            var context = new MetadataContext();
            var metadataProvider = new MetadataContextProvider();
            metadataProvider.AddComponent(new TestMetadataContextProviderComponent
            {
                TryGetMetadataContext = (o, list) => context
            });

            var component = new ViewModelServiceResolverComponent(metadataContextProvider: metadataProvider);
            component.TryGetService(new TestViewModel(), typeof(IMetadataContext), DefaultMetadata).ShouldEqual(context);
        }

        [Fact]
        public void TryGetServiceShouldReturnMessenger()
        {
            var component = new ViewModelServiceResolverComponent();
            var service = (IMessenger) component.TryGetService(new TestViewModel(), typeof(IMessenger), DefaultMetadata)!;
            service.GetComponent<IMessagePublisherComponent>().ShouldNotBeNull();
            service.GetComponent<IMessengerSubscriberComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnBusyManager()
        {
            var component = new ViewModelServiceResolverComponent();
            var service = (IBusyManager) component.TryGetService(new TestViewModel(), typeof(IBusyManager), DefaultMetadata)!;
            service.GetComponent<IBusyManagerComponent>().ShouldNotBeNull();
        }

        [Fact]
        public void TryGetServiceShouldReturnNullUnknownComponent()
        {
            var component = new ViewModelServiceResolverComponent();
            component.TryGetService(new TestViewModel(), typeof(object), DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}