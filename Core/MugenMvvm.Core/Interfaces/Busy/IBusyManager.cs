﻿using System.Collections.Generic;
using MugenMvvm.Delegates;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Busy
{
    public interface IBusyManager : IComponentOwner<IBusyManager>, ISuspendable
    {
        IBusyToken BeginBusy<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);

        IBusyToken? TryGetToken<TState>(in TState state, FuncIn<TState, IBusyToken, IReadOnlyMetadataContext?, bool> filter, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IBusyToken> GetTokens(IReadOnlyMetadataContext? metadata = null);
    }
}