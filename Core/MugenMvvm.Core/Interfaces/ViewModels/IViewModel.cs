﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.ViewModels
{
    public interface IViewModel : INotifyPropertyChangedEx, IDisposableObject<IViewModel>, IHasMemento, IHasMetadata<IObservableMetadataContext>
    {
        IBusyIndicatorProvider BusyIndicatorProvider { get; }


        void Publish(object message, IMessengerContext? messengerContext = null);

        void Publish(object sender, object message, IMessengerContext? messengerContext = null);

        void Subscribe(object item, ThreadExecutionMode? executionMode = null);

        void Unsubscribe(object item);
    }
}