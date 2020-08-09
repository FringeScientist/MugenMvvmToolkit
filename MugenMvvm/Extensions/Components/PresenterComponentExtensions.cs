﻿using System.Collections.Generic;
using System.Threading;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class PresenterComponentExtensions
    {
        #region Methods

        public static bool CanShow(this IConditionPresenterComponent[] components, IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            Should.NotBeNull(presenterComponent, nameof(presenterComponent));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanShow(presenter, presenterComponent, results, request, metadata))
                    return false;
            }

            return true;
        }

        public static bool CanClose(this IConditionPresenterComponent[] components, IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results,
            object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            Should.NotBeNull(presenterComponent, nameof(presenterComponent));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClose(presenter, presenterComponent, results, request, metadata))
                    return false;
            }

            return true;
        }

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow(this IPresenterComponent[] components, IPresenter presenter, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryShow(presenter, request, cancellationToken, metadata);
            var result = ItemOrListEditor.Get<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryShow(presenter, request, cancellationToken, metadata));
            return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
        }

        public static ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose(this IPresenterComponent[] components, IPresenter presenter, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(presenter, nameof(presenter));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryClose(presenter, request, cancellationToken, metadata);
            var result = ItemOrListEditor.Get<IPresenterResult>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryClose(presenter, request, cancellationToken, metadata));
            return result.ToItemOrList<IReadOnlyList<IPresenterResult>>();
        }

        #endregion
    }
}