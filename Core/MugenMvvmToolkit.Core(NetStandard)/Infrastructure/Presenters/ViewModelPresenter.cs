﻿#region Copyright

// ****************************************************************************
// <copyright file="ViewModelPresenter.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Nested types

        private sealed class DynamicPresentersCollection : ICollection<IDynamicViewModelPresenter>
        {
            #region Fields

            private readonly ViewModelPresenter _presenter;
            private readonly OrderedListInternal<IDynamicViewModelPresenter> _list;

            #endregion

            #region Constructors

            public DynamicPresentersCollection(ViewModelPresenter presenter)
            {
                _presenter = presenter;
                _list = new OrderedListInternal<IDynamicViewModelPresenter>(new DelegateComparer<IDynamicViewModelPresenter>(ComparerDelegate));
            }

            #endregion

            #region Methods

            private static int ComparerDelegate(IDynamicViewModelPresenter x1, IDynamicViewModelPresenter x2)
            {
                return x2.Priority.CompareTo(x1.Priority);
            }

            #endregion

            #region Implementation of ICollection<IDynamicViewModelPresenter>

            public IEnumerator<IDynamicViewModelPresenter> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(IDynamicViewModelPresenter item)
            {
                Should.NotBeNull(item, nameof(item));
                _list.Add(item);
                _presenter.OnDynamicPresenterAdded(item);
            }

            public void Clear()
            {
                var values = _list.ToArrayEx();
                _list.Clear();
                for (int index = 0; index < values.Length; index++)
                    _presenter.OnDynamicPresenterRemoved(values[index]);
            }

            public bool Contains(IDynamicViewModelPresenter item)
            {
                return _list.Contains(item);
            }

            public void CopyTo(IDynamicViewModelPresenter[] array, int arrayIndex)
            {
                _list.CopyTo(array, arrayIndex);
            }

            public bool Remove(IDynamicViewModelPresenter item)
            {
                Should.NotBeNull(item, nameof(item));
                var remove = _list.Remove(item);
                if (remove)
                    _presenter.OnDynamicPresenterRemoved(item);
                return remove;
            }

            public int Count => _list.Count;

            public bool IsReadOnly => false;

            #endregion
        }

        #endregion

        #region Fields

        public const int DefaultNavigationPresenterPriority = -1;
        public const int DefaultMultiViewModelPresenterPriority = 0;
        public const int DefaultWindowPresenterPriority = 1;

        private readonly INavigationDispatcher _navigationDispatcher;
        private readonly DynamicPresentersCollection _dynamicPresenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(INavigationDispatcher navigationDispatcher)
        {
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            _navigationDispatcher = navigationDispatcher;
            _dynamicPresenters = new DynamicPresentersCollection(this);
        }

        #endregion

        #region Properties

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher;

        #endregion

        #region Implementation of IViewModelPresenter

        public ICollection<IDynamicViewModelPresenter> DynamicPresenters => _dynamicPresenters;

        public IAsyncOperation ShowAsync(IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            return ShowInternalAsync(context);
        }

        public void Restore(IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            RestoreInternal(context);
        }

        public Task<bool> CloseAsync(IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            return CloseInternalAsync(context);
        }

        #endregion

        #region Methods

        protected virtual IAsyncOperation ShowInternalAsync(IDataContext context)
        {
            var presenters = _dynamicPresenters.ToArrayEx();
            for (int i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryShowAsync(context, this);
                if (operation != null)
                {
                    if (Tracer.TraceInformation)
                        Tracer.Info("The request {0} is shown by {1}", ContextToString(context), presenters[i].GetType().FullName);
                    return operation;
                }
            }
            throw ExceptionManager.PresenterCannotShowRequest(GetType(), ContextToString(context));
        }

        protected virtual void RestoreInternal(IDataContext context)
        {
            var presenters = _dynamicPresenters.ToArrayEx();
            for (int i = 0; i < presenters.Length; i++)
            {
                var presenter = presenters[i] as IRestorableDynamicViewModelPresenter;
                if (presenter != null && presenter.Restore(context, this))
                {
                    if (Tracer.TraceInformation)
                        Tracer.Info("The request {0} is restored by {1}", ContextToString(context), presenter.GetType().FullName);
                    return;
                }
            }
        }

        [NotNull]
        protected virtual Task<bool> CloseInternalAsync(IDataContext context)
        {
            var presenters = _dynamicPresenters.ToArrayEx();
            for (int i = 0; i < presenters.Length; i++)
            {
                var operation = presenters[i].TryCloseAsync(context, this);
                if (operation != null)
                {
                    if (Tracer.TraceInformation)
                        Tracer.Info("The request {0} is closed by {1}", ContextToString(context), presenters[i].GetType().FullName);
                    return operation;
                }
            }
            var wrapperViewModel = context.GetData(NavigationConstants.ViewModel)?.Settings.Metadata.GetData(ViewModelConstants.WrapperViewModel);
            if (wrapperViewModel != null)
            {
                context.AddOrUpdate(NavigationConstants.ViewModel, wrapperViewModel);
                return CloseInternalAsync(context);
            }
            var navigationContext = context as INavigationContext;
            if (navigationContext == null)
                return Empty.FalseTask;
            return NavigationDispatcher.OnNavigatingFromAsync(navigationContext);
        }

        protected virtual void OnDynamicPresenterAdded([NotNull] IDynamicViewModelPresenter presenter)
        {
        }

        protected virtual void OnDynamicPresenterRemoved([NotNull] IDynamicViewModelPresenter presenter)
        {
        }

        private static string ContextToString(IDataContext context)
        {
            var builder = new StringBuilder("(");
            var values = context.ToList();
            foreach (var item in values)
                builder.Append(item.DataConstant.Id).Append("=").Append(item.Value).Append(";");
            builder.Append(")");
            return builder.ToString();
        }

        #endregion
    }
}
