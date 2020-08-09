﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.App
{
    public sealed class MugenApplication : IMugenApplication
    {
        #region Fields

        private IComponentCollection? _components;
        private IDeviceInfo? _deviceInfo;

        #endregion

        #region Constructors

        public MugenApplication(IReadOnlyMetadataContext? metadata = null)
        {
            Metadata = metadata.ToNonReadonly();
            MugenService.Configuration.InitializeInstance<IMugenApplication>(this);
        }

        #endregion

        #region Properties

        public bool HasMetadata => Metadata.Count != 0;

        public bool HasComponents => _components != null && _components.Count != 0;

        public IMetadataContext Metadata { get; }

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionManager.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        public IDeviceInfo DeviceInfo
        {
            get => _deviceInfo ??= new DeviceInfo(PlatformType.Unknown);
            private set => _deviceInfo = value;
        }

        #endregion

        #region Implementation of interfaces

        public void OnUnhandledException(Exception exception, UnhandledExceptionType type, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<IApplicationUnhandledExceptionComponent>().OnUnhandledException(this, exception, type, metadata);

        public void OnLifecycleChanged(ApplicationLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata = null) =>
            Components.Get<IApplicationLifecycleDispatcherComponent>().OnLifecycleChanged(this, lifecycleState, state, metadata);

        public void Initialize(IDeviceInfo deviceInfo, object? state, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(deviceInfo, nameof(deviceInfo));
            if (_deviceInfo != null && _deviceInfo.HasMetadata)
                deviceInfo.Metadata.Merge(_deviceInfo.Metadata);
            DeviceInfo = deviceInfo;
            OnLifecycleChanged(ApplicationLifecycleState.Initializing, state, metadata);
            OnLifecycleChanged(ApplicationLifecycleState.Initialized, state, metadata);
        }

        #endregion
    }
}