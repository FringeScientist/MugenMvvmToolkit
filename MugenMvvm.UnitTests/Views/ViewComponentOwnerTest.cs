﻿using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewComponentOwnerTest : ComponentOwnerTestBase<IView>
    {
        #region Methods

        protected override IView GetComponentOwner(IComponentCollectionManager? collectionProvider = null) =>
            new View(new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel(), null, collectionProvider);

        #endregion
    }
}