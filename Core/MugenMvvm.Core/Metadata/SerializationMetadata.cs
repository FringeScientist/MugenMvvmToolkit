﻿using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class SerializationMetadata
    {
        #region Fields

        private static IMetadataContextKey<bool> _noCache;

        #endregion

        #region Properties

        public static IMetadataContextKey<bool> NoCache
        {
            get
            {
                if (_noCache == null)
                    _noCache = GetBuilder<bool>(nameof(NoCache)).Serializable().Build();
                return _noCache;
            }
            set => _noCache = value;
        }

        #endregion

        #region Methods

        private static MetadataContextKey.Builder<T> GetBuilder<T>(string name)
        {
            return MetadataContextKey.Create<T>(typeof(SerializationMetadata), name);
        }

        #endregion
    }
}