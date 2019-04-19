﻿using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;

namespace MugenMvvm.Infrastructure.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class SerializableWeakReference
    {
        #region Constructors

        public SerializableWeakReference(WeakReference weakReference)
        {
            Target = weakReference;
        }

        public SerializableWeakReference(object? target)
            : this(MugenExtensions.GetWeakReference(target))
        {
        }

        #endregion

        #region Properties

        [DataMember(Name = "S")]
        internal object? SerializableTarget
        {
            get => Target.Target;
            set => Target = MugenExtensions.GetWeakReference(value);
        }

        [field: IgnoreDataMember]
        [field: XmlIgnore]
        [field: NonSerialized]
        public WeakReference Target { get; private set; }

        #endregion
    }
}