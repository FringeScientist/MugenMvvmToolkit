﻿using System;
using System.IO;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer//todo review, can serialize instance??
    {
        [Pure]
        ISerializationContext GetContext(IServiceProvider? serviceProvider, IMetadataContext? metadata);

        [Pure]
        bool CanSerialize(Type type, IReadOnlyMetadataContext metadata);

        Stream Serialize(object item, ISerializationContext? context);

        object Deserialize(Stream stream, ISerializationContext? context);
    }
}