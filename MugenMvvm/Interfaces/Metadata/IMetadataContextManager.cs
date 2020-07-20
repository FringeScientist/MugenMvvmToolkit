﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataContextManager : IComponentOwner<IMetadataContextManager>
    {
        IReadOnlyMetadataContext? TryGetReadOnlyMetadataContext(object? target = null, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>> values = default);

        IMetadataContext? TryGetMetadataContext(object? target = null, ItemOrList<KeyValuePair<IMetadataContextKey, object?>, IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>> values = default);
    }
}