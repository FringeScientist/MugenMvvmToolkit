﻿#region Copyright

// ****************************************************************************
// <copyright file="NavigationDispatcherTest.cs">
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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Navigation
{
    [TestClass]
    public class NavigationDispatcherTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void DispatcherShouldNotifyAboutCancel()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<TestWorkspaceViewModel>();
            var vmTo = GetViewModel<TestWorkspaceViewModel>();
            IOperationResult result = null;
            OperationCallbackManager.SetResult = (o, r) =>
            {
                result.ShouldBeNull();
                result = r;
            };

            Action<INavigationContext> fromAssert = navContext =>
            {
                result.OperationContext.ShouldEqual(navContext);
                result.Source.ShouldEqual(vmFrom);
                result.IsCanceled.ShouldBeTrue();
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                result.OperationContext.ShouldEqual(navContext);
                result.Source.ShouldEqual(vmTo);
                result.IsCanceled.ShouldBeTrue();
            };
            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                result = null;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                navigationDispatcher.OnNavigationCanceled(ctx);
                tuple.Item2(ctx);
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutException()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<TestWorkspaceViewModel>();
            var vmTo = GetViewModel<TestWorkspaceViewModel>();
            var exception = new Exception();
            IOperationResult result = null;
            OperationCallbackManager.SetResult = (o, r) =>
            {
                result.ShouldBeNull();
                result = r;
            };

            Action<INavigationContext> fromAssert = navContext =>
            {
                result.OperationContext.ShouldEqual(navContext);
                result.Source.ShouldEqual(vmFrom);
                result.Exception.ShouldEqual(exception);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                result.OperationContext.ShouldEqual(navContext);
                result.Source.ShouldEqual(vmTo);
                result.Exception.ShouldEqual(exception);
            };
            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                result = null;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                navigationDispatcher.OnNavigationFailed(ctx, exception);
                tuple.Item2(ctx);
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatedNavigableViewModel()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();
            int navigatedFromCount = 0;
            int navigatedToCount = 0;
            vmFrom.OnNavigatedToDelegate = context => { throw new NotSupportedException(); };
            vmFrom.OnNavigatedFromDelegate = context => navigatedFromCount++;
            vmTo.OnNavigatedFromDelegate = context => { throw new NotSupportedException(); };
            vmTo.OnNavigatedToDelegate = context => navigatedToCount++;

            Action<INavigationContext> fromAssert = navContext =>
            {
                navigatedFromCount.ShouldEqual(1);
                navigatedToCount.ShouldEqual(1);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                navigatedFromCount.ShouldEqual(1);
                navigatedToCount.ShouldEqual(1);
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                navigatedFromCount = 0;
                navigatedToCount = 0;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                navigationDispatcher.OnNavigated(ctx);
                tuple.Item2(ctx);
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatedCallback()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();

            IOperationResult result = null;
            OperationCallbackManager.SetResult = (o, r) =>
            {
                result.ShouldBeNull();
                result = r;
            };

            Action<INavigationContext> fromAssert = navContext =>
            {
                result.OperationContext.ShouldEqual(navContext);
                result.Source.ShouldEqual(vmFrom);
                result.IsCanceled.ShouldBeFalse();
                result.IsFaulted.ShouldBeFalse();
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                result.ShouldBeNull();
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                result = null;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                navigationDispatcher.OnNavigated(ctx);
                tuple.Item2(ctx);
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatedCloseableViewModel()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();
            int closedCount = 0;
            vmFrom.ClosedDelegate = context => closedCount++;
            vmTo.ClosedDelegate = context => { throw new NotSupportedException(); };

            Action<INavigationContext> fromAssert = navContext =>
            {
                closedCount.ShouldEqual(1);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                closedCount.ShouldEqual(0);
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                closedCount = 0;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                navigationDispatcher.OnNavigated(ctx);
                tuple.Item2(ctx);
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatedCloseableViewModelEvent()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();
            INavigationContext ctx = null;
            int closedCount = 0;
            vmFrom.AddClosedHandler((sender, args) =>
            {
                closedCount++;
                args.ViewModel.ShouldEqual(vmFrom);
                args.Context.ShouldEqual(ctx);
            });
            vmTo.AddClosedHandler((sender, args) => { throw new NotSupportedException(); });

            Action<INavigationContext> fromAssert = navContext =>
            {
                closedCount.ShouldEqual(1);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                closedCount.ShouldEqual(0);
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                closedCount = 0;
                ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                navigationDispatcher.OnNavigated(ctx);
                tuple.Item2(ctx);
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatedEvent()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();

            NavigatedEventArgs eventArgs = null;
            navigationDispatcher.Navigated += (sender, args) =>
            {
                sender.ShouldEqual(navigationDispatcher);
                eventArgs.ShouldBeNull();
                eventArgs = args;
            };

            var ctx = new NavigationContext(NavigationType.Page, NavigationMode.New, vmFrom, vmTo, this);
            navigationDispatcher.OnNavigated(ctx);
            eventArgs.Context.ShouldEqual(ctx);
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatingNavigableViewModel()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();
            int navigatingFromCount = 0;
            vmFrom.OnNavigatingFromDelegate = context =>
            {
                navigatingFromCount++;
                return Empty.TrueTask;
            };
            vmTo.OnNavigatingFromDelegate = context => { throw new NotSupportedException(); };

            Action<INavigationContext> fromAssert = navContext =>
            {
                navigatingFromCount.ShouldEqual(1);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                navigatingFromCount.ShouldEqual(1);
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                navigatingFromCount = 0;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                var task = navigationDispatcher.OnNavigatingFromAsync(ctx);
                tuple.Item2(ctx);
                task.Result.ShouldBeTrue();
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatingCloseableViewModel()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();
            int closingCount = 0;
            vmFrom.ClosingDelegate = context =>
            {
                closingCount++;
                return Empty.TrueTask;
            };
            vmTo.ClosingDelegate = context => { throw new NotSupportedException(); };

            Action<INavigationContext> fromAssert = navContext =>
            {
                closingCount.ShouldEqual(1);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                closingCount.ShouldEqual(0);
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                closingCount = 0;
                var ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                var task = navigationDispatcher.OnNavigatingFromAsync(ctx);
                tuple.Item2(ctx);
                task.Result.ShouldBeTrue();
            }
        }

        [TestMethod]
        public void DispatcherShouldNotifyAboutNavigatingCloseableViewModelEvent()
        {
            var navigationDispatcher = CreateNavigationDispatcher();
            var vmFrom = GetViewModel<NavigableViewModelMock>();
            var vmTo = GetViewModel<NavigableViewModelMock>();
            int closingCount = 0;
            INavigationContext ctx = null;
            vmFrom.AddClosingHandler((sender, args) =>
            {
                sender.ShouldEqual(vmFrom);
                args.ViewModel.ShouldEqual(vmFrom);
                args.Context.ShouldEqual(ctx);
                closingCount++;
                args.AddCancelTask(Empty.FalseTask);
            });
            vmTo.AddClosingHandler((sender, args) => { throw new NotSupportedException(); });

            Action<INavigationContext> fromAssert = navContext =>
            {
                closingCount.ShouldEqual(1);
            };
            Action<INavigationContext> toAssert = navContext =>
            {
                closingCount.ShouldEqual(0);
            };

            var tuples = new List<Tuple<NavigationMode, Action<INavigationContext>>>
            {
                Tuple.Create(NavigationMode.Back, fromAssert),
                Tuple.Create(NavigationMode.Remove, fromAssert),
                Tuple.Create(NavigationMode.New, toAssert),
                Tuple.Create(NavigationMode.Forward, toAssert),
                Tuple.Create(NavigationMode.Refresh, toAssert),
                Tuple.Create(NavigationMode.Undefined, toAssert)
            };
            foreach (var tuple in tuples)
            {
                closingCount = 0;
                ctx = new NavigationContext(NavigationType.Page, tuple.Item1, vmFrom, vmTo, this);
                var task = navigationDispatcher.OnNavigatingFromAsync(ctx);
                tuple.Item2(ctx);
                task.Result.ShouldBeTrue();
            }
        }

        protected virtual INavigationDispatcher CreateNavigationDispatcher()
        {
            return new NavigationDispatcher(OperationCallbackManager);
        }

        #endregion
    }
}