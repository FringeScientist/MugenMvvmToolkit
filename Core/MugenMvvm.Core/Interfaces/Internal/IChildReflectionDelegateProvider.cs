﻿using System;
using System.Reflection;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IChildReflectionDelegateProvider : IHasPriority
    {
        bool CanCreateDelegate(IReflectionDelegateProvider provider, Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(IReflectionDelegateProvider provider, Type delegateType, object? target, MethodInfo method);

        Func<object?[], object>? TryGetActivator(IReflectionDelegateProvider provider, ConstructorInfo constructor);

        Func<object?, object?[], object?>? TryGetMethodInvoker(IReflectionDelegateProvider provider, MethodInfo method);

        Delegate? TryGetMethodInvoker(IReflectionDelegateProvider provider, Type delegateType, MethodInfo method);

        Func<object?, TType>? TryGetMemberGetter<TType>(IReflectionDelegateProvider provider, MemberInfo member);

        Action<object?, TType>? TryGetMemberSetter<TType>(IReflectionDelegateProvider provider, MemberInfo member);
    }
}