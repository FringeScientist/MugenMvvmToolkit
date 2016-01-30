﻿#region Copyright

// ****************************************************************************
// <copyright file="INavigationService.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
#if WPF
using System.Windows.Navigation;

namespace MugenMvvmToolkit.WPF.Interfaces.Navigation
#elif ANDROID
using Android.App;

namespace MugenMvvmToolkit.Android.Interfaces.Navigation
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Interfaces.Navigation
#elif XAMARIN_FORMS
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Interfaces.Navigation
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Interfaces.Navigation
#elif WINDOWS_PHONE
using System.Windows.Navigation;

namespace MugenMvvmToolkit.WinPhone.Interfaces.Navigation
#endif
{
    public interface INavigationService
    {
        bool CanGoBack { get; }

        bool CanGoForward { get; }

        object CurrentContent { get; }

        void GoBack();

        void GoForward();

#if WPF || WINDOWS_PHONE
        JournalEntry RemoveBackEntry();
#elif ANDROID
        void OnPauseActivity([NotNull] Activity activity, IDataContext context = null);

        void OnResumeActivity([NotNull] Activity activity, IDataContext context = null);

        void OnStartActivity([NotNull] Activity activity, IDataContext context = null);

        void OnCreateActivity([NotNull] Activity activity, IDataContext context = null);

        bool OnFinishActivity([NotNull] Activity activity, bool isBackNavigation, IDataContext context = null);
#elif XAMARIN_FORMS
        void UpdateRootPage(NavigationPage page);
#endif
        [CanBeNull]
        string GetParameterFromArgs([NotNull]EventArgs args);

        bool Navigate([NotNull] NavigatingCancelEventArgsBase args, [CanBeNull] IDataContext dataContext);

        bool Navigate([NotNull] IViewMappingItem source, [CanBeNull] string parameter, [CanBeNull] IDataContext dataContext);

        bool CanClose([NotNull] IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        bool TryClose([NotNull]IViewModel viewModel, [CanBeNull] IDataContext dataContext);

        event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;
    }
}
