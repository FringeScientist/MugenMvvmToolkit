﻿using System;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Interfaces.Wrapping.Components;
using MugenMvvm.Wrapping.Components;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IWrapperManagerComponent AddWrapper(this IWrapperManager wrapperManager, Func<IWrapperManager, Type, Type, IReadOnlyMetadataContext?, bool> condition,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, object?> wrapperFactory, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            var factory = new DelegateWrapperManagerComponent(condition, wrapperFactory);
            wrapperManager.Components.Add(factory, metadata);
            return factory;
        }

        public static IWrapperManagerComponent AddWrapper(this IWrapperManager wrapperManager, Type wrapperType, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, object>? wrapperFactory = null,
            IReflectionDelegateProvider? reflectionDelegateProvider = null)
        {
            Should.NotBeNull(wrapperManager, nameof(wrapperManager));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.BeOfType(implementation, nameof(implementation), wrapperType);
            if (implementation.IsInterface || implementation.IsAbstract)
                ExceptionManager.ThrowWrapperTypeShouldBeNonAbstract(implementation);

            if (wrapperFactory == null)
            {
                var constructor = implementation
                    .GetConstructors(BindingFlagsEx.InstanceOnly)
                    .FirstOrDefault()
                    ?.GetActivator(reflectionDelegateProvider);
                if (constructor == null)
                    ExceptionManager.ThrowCannotFindConstructor(implementation);
                wrapperFactory = (manager, o, arg3, arg4) => constructor.Invoke(new[] {o});
            }

            return wrapperManager.AddWrapper((manager, type, arg3, arg4) => wrapperType == arg3, wrapperFactory); //todo closure check
        }

        public static IWrapperManagerComponent AddWrapper<TWrapper>(this IWrapperManager wrapperManager, Type implementation,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, TWrapper>? wrapperFactory = null)
            where TWrapper : class
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), implementation, wrapperFactory);
        }

        public static IWrapperManagerComponent AddWrapper<TWrapper, TImplementation>(this IWrapperManager wrapperManager,
            Func<IWrapperManager, object, Type, IReadOnlyMetadataContext?, TWrapper>? wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            return wrapperManager.AddWrapper(typeof(TWrapper), typeof(TImplementation), wrapperFactory);
        }

        #endregion
    }
}