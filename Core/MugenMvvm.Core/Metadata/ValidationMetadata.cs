﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Metadata
{
    public static class ValidationMetadata
    {
        #region Fields

        private static IMetadataContextKey<ICollection<string>>? _ignoredMembers;

        #endregion

        #region Properties

        [AllowNull]
        public static IMetadataContextKey<ICollection<string>> IgnoreMembers
        {
            get => _ignoredMembers ??= GetBuilder<ICollection<string>>(nameof(IgnoreMembers)).NotNull().Build();
            set => _ignoredMembers = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(ValidationMetadata), name);
        }

        #endregion
    }
}