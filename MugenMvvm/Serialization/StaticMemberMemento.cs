﻿using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Internal;

namespace MugenMvvm.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class StaticMemberMemento : IMemento
    {
        #region Fields

        [DataMember(Name = "M")]
        internal MemberInfo? Member;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        internal StaticMemberMemento()
        {
        }
#pragma warning restore CS8618

        private StaticMemberMemento(object target, MemberInfo member)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(member, nameof(member));
            TargetType = target.GetType();
            Member = member;
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        [XmlIgnore]
        [field: DataMember(Name = "T")]
        public Type TargetType { get; internal set; }

        #endregion

        #region Implementation of interfaces

        public void Preserve(ISerializationContext serializationContext)
        {
        }

        public MementoResult Restore(ISerializationContext serializationContext)
        {
            if (Member == null)
                return default;

            object? target;
            if (Member is PropertyInfo propertyInfo)
                target = propertyInfo.GetValue(null);
            else
                target = ((FieldInfo) Member).GetValue(null);
            if (target == null)
                return default;
            return new MementoResult(target);
        }

        #endregion

        #region Methods

        public static StaticMemberMemento? Create(object target, Type type, string fieldOrPropertyName)
        {
            var member = type.GetField(fieldOrPropertyName, BindingFlagsEx.StaticOnly) ??
                         (MemberInfo?) type.GetProperty(fieldOrPropertyName, BindingFlagsEx.StaticOnly);
            if (member == null)
            {
                Tracer.Error()?.Trace(MessageConstant.FieldOrPropertyNotFoundFormat2, fieldOrPropertyName, type);
                return null;
            }

            return new StaticMemberMemento(target, member);
        }

        #endregion
    }
}