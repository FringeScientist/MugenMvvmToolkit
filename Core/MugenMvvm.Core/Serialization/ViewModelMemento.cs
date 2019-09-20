﻿//using System;
//using System.Collections.Generic;
//using System.Runtime.Serialization;
//using System.Xml.Serialization;
//using MugenMvvm.Attributes;
//using MugenMvvm.Constants;
//using MugenMvvm.Enums;
//using MugenMvvm.Interfaces.BusyIndicator;
//using MugenMvvm.Interfaces.Components;
//using MugenMvvm.Interfaces.Messaging;
//using MugenMvvm.Interfaces.Metadata;
//using MugenMvvm.Interfaces.Models;
//using MugenMvvm.Interfaces.Serialization;
//using MugenMvvm.Interfaces.ViewModels;
//using MugenMvvm.Messaging;
//using MugenMvvm.Metadata;
//
//namespace MugenMvvm.Serialization
//{
//    [Serializable]
//    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
//    [Preserve(Conditional = true, AllMembers = true)]
//    public class ViewModelMemento : IMemento //todo saving/restoring state/ cancel restore/ split methods to control default behavior
//    {
//        #region Fields
//
//        [IgnoreDataMember]
//        [XmlIgnore]
//        [NonSerialized]
//        private IViewModelBase? _viewModel;
//
//        [DataMember(Name = "B")]
//        protected internal IList<IComponent<IBusyIndicatorProvider>?>? BusyComponents;
//
//        [DataMember(Name = "C")]
//        protected internal IMetadataContext? Metadata;
//
//        [DataMember(Name = "N")]
//        protected internal bool NoState;
//
//        [DataMember(Name = "S")]
//        protected internal IList<MessengerSubscriberInfo>? Subscribers;
//
//        [DataMember(Name = "T")]
//        protected internal Type? ViewModelType;
//
//        protected static readonly object RestorationLocker = new object();
//
//        #endregion
//
//        #region Constructors
//
//        internal ViewModelMemento()
//        {
//        }
//
//        public ViewModelMemento(IViewModelBase viewModel)
//        {
//            _viewModel = viewModel;
//            Metadata = viewModel.Metadata;
//            ViewModelType = viewModel.GetType();
//        }
//
//        #endregion
//
//        #region Properties
//
//        [IgnoreDataMember]
//        [XmlIgnore]
//        public Type TargetType => ViewModelType!;
//
//        #endregion
//
//        #region Implementation of interfaces
//
//        public void Preserve(ISerializationContext serializationContext)
//        {
//            if (_viewModel == null)
//                return;
//            var dispatcher = serializationContext.ServiceProvider.GetService<IViewModelManager>();
//            dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Preserving, serializationContext.Metadata);
//
//            if (_viewModel.Metadata.Get(ViewModelMetadata.NoState))
//            {
//                NoState = true;
//                Metadata = null;
//                Subscribers = null;
//                BusyComponents = null;
//            }
//            else
//            {
//                NoState = false;
//                Metadata = _viewModel.Metadata;
//                Subscribers = _viewModel.TryGetServiceOptional<IMessenger>()?.GetSubscribers().ToSerializable(serializationContext.Serializer);
//                BusyComponents = _viewModel.TryGetServiceOptional<IBusyIndicatorProvider>()?.GetComponents().ToSerializable(serializationContext.Serializer);
//            }
//
//            OnPreserveInternal(_viewModel!, NoState, serializationContext);
//            dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Preserved, serializationContext.Metadata);
//        }
//
//        public IMementoResult Restore(ISerializationContext serializationContext)
//        {
//            if (NoState)
//                return MementoResult.Unrestored;
//
//            Should.NotBeNull(Metadata, nameof(Metadata));
//            Should.NotBeNull(ViewModelType, nameof(ViewModelType));
//            if (_viewModel != null)
//                return new MementoResult(_viewModel, serializationContext);
//
//            var dispatcher = serializationContext.ServiceProvider.GetService<IViewModelManager>();
//            lock (RestorationLocker)
//            {
//                if (_viewModel != null)
//                    return new MementoResult(_viewModel, serializationContext);
//
//                if (!serializationContext.Metadata.Get(SerializationMetadata.NoCache))
//                {
//                    _viewModel = dispatcher.TryGetViewModel(Metadata);
//                    if (_viewModel != null)
//                        return new MementoResult(_viewModel, serializationContext);
//                }
//
//                _viewModel = RestoreInternal(serializationContext);
//                dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restoring, serializationContext.Metadata);
//                RestoreInternal(_viewModel);
//                OnRestoringInternal(_viewModel, serializationContext);
//                dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restored, serializationContext.Metadata);
//                return new MementoResult(_viewModel, serializationContext);
//            }
//        }
//
//        #endregion
//
//        #region Methods
//
//        protected virtual void OnPreserveInternal(IViewModelBase viewModel, bool noState, ISerializationContext serializationContext)
//        {
//        }
//
//        protected virtual IViewModelBase RestoreInternal(ISerializationContext serializationContext)
//        {
//            return (IViewModelBase)serializationContext.ServiceProvider.GetService(ViewModelType);
//        }
//
//        protected virtual void OnRestoringInternal(IViewModelBase viewModel, ISerializationContext serializationContext)
//        {
//        }
//
//        private void RestoreInternal(IViewModelBase viewModel)
//        {
//            var components = Metadata!.GetComponents();
//            for (var index = 0; index < components.Length; index++)
//                viewModel.Metadata.AddComponent(components[index]);
//
//            viewModel.Metadata.Merge(Metadata!);
//
//            if (BusyComponents != null && viewModel is IHasService<IBusyIndicatorProvider> hasBusyIndicatorProvider)
//            {
//                for (var index = 0; index < BusyComponents.Count; index++)
//                {
//                    var component = BusyComponents[index];
//                    if (component != null)
//                        hasBusyIndicatorProvider.Service.AddComponent(component);
//                }
//            }
//
//            if (Subscribers != null && viewModel is IHasService<IMessenger> hasMessenger)
//            {
//                for (var index = 0; index < Subscribers.Count; index++)
//                {
//                    var subscriber = Subscribers[index];
//                    if (subscriber.Subscriber != null)
//                        hasMessenger.Service.Subscribe(subscriber.Subscriber, subscriber.ExecutionMode);
//                }
//            }
//        }
//
//        #endregion
//    }
//}