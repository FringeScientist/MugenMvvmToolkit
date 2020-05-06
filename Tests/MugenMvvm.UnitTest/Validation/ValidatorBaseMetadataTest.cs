﻿using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.UnitTest.Validation.Internal;

namespace MugenMvvm.UnitTest.Validation
{
    public class ValidatorBaseMetadataTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new TestValidatorBase<object>(false, metadata, metadataContextProvider: metadataContextProvider);
        }

        #endregion
    }
}