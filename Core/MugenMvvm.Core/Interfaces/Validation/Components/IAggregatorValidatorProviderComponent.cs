﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IAggregatorValidatorProviderComponent : IComponent<IValidatorProvider>
    {
        IAggregatorValidator? TryGetAggregatorValidator<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}