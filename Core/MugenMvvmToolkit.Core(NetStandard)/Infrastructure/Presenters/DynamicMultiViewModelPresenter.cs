﻿#region Copyright

// ****************************************************************************
// <copyright file="DynamicMultiViewModelPresenter.cs">
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

using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Presenters
{
    public class DynamicMultiViewModelPresenter : IDynamicViewModelPresenter
    {
        #region Fields

        protected static readonly DataConstant<object> IgnoreConstant;

        #endregion

        #region Constructors

        static DynamicMultiViewModelPresenter()
        {
            IgnoreConstant = DataConstant.Create<object>(typeof(DynamicMultiViewModelPresenter), nameof(IgnoreConstant), false);
        }

        public DynamicMultiViewModelPresenter([NotNull] IMultiViewModel multiViewModel,
            IOperationCallbackManager callbackManager = null, Func<IViewModel, IDataContext, IViewModelPresenter, bool> canShowViewModel = null)
        {
            Should.NotBeNull(multiViewModel, nameof(multiViewModel));
            MultiViewModel = multiViewModel;
            CallbackManager = callbackManager ?? multiViewModel.GetIocContainer(true).Get<IOperationCallbackManager>();
            CanShowViewModel = canShowViewModel;
        }

        #endregion

        #region Properties

        protected IMultiViewModel MultiViewModel { get; }

        protected IOperationCallbackManager CallbackManager { get; }

        protected Func<IViewModel, IDataContext, IViewModelPresenter, bool> CanShowViewModel { get; }

        #endregion

        #region Methods

        protected virtual IAsyncOperation TryShowInternalAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (MultiViewModel.ItemsSource.Any(vm => vm == viewModel))
                MultiViewModel.SelectedItem = viewModel;
            else
                MultiViewModel.AddViewModel(viewModel, true);
            var operation = new AsyncOperation<object>();
            CallbackManager.Register(OperationType.TabNavigation, viewModel, operation.ToOperationCallback(), context);
            return operation;
        }

        protected virtual Task<bool> TryCloseInternalAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            return MultiViewModel.RemoveViewModelAsync(viewModel, context);
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        public int Priority => ViewModelPresenter.DefaultMultiViewModelPresenterPriority;

        public IAsyncOperation TryShowAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (!MultiViewModel.ViewModelType.IsInstanceOfType(viewModel))
                return null;
            bool data;
            if (context.TryGetData(NavigationConstants.SuppressTabNavigation, out data) && data)
                return null;
            var canShow = CanShowViewModel ?? ApplicationSettings.MultiViewModelPresenterCanShowViewModel;
            if (canShow != null && !canShow(viewModel, context, parentPresenter))
                return null;

            return TryShowInternalAsync(viewModel, context, parentPresenter);
        }

        public Task<bool> TryCloseAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            if (!MultiViewModel.ViewModelType.IsInstanceOfType(viewModel) || context.Contains(IgnoreConstant) || !MultiViewModel.ItemsSource.Contains(viewModel))
                return null;
            context = context.ToNonReadOnly();
            context.AddOrUpdate(IgnoreConstant, null);
            return TryCloseInternalAsync(viewModel, context, parentPresenter);
        }

        #endregion
    }
}