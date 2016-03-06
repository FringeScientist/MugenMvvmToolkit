﻿#region Copyright

// ****************************************************************************
// <copyright file="MvvmViewController.cs">
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
using Foundation;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using UIKit;

namespace MugenMvvmToolkit.iOS.Views
{
    public abstract class MvvmViewController : UIViewController, IViewControllerView
    {
        #region Fields

        private IMvvmViewControllerMediator _mediator;

        #endregion

        #region Constructors

        protected MvvmViewController()
        {
        }

        protected MvvmViewController(NSCoder coder)
            : base(coder)
        {
        }

        protected MvvmViewController(NSObjectFlag t)
            : base(t)
        {
        }

        protected MvvmViewController(IntPtr handle)
            : base(handle)
        {
        }

        protected MvvmViewController(string nibName, NSBundle bundle)
            : base(nibName, bundle)
        {
        }

        #endregion

        #region Implementation of IViewControllerView

        public virtual IMvvmViewControllerMediator Mediator => this.GetOrCreateMediator(ref _mediator);

        #endregion

        #region Overrides of UIViewController

        public override void DecodeRestorableState(NSCoder coder)
        {
            Mediator.DecodeRestorableState(base.DecodeRestorableState, coder);
        }

        public override void EncodeRestorableState(NSCoder coder)
        {
            Mediator.EncodeRestorableState(base.EncodeRestorableState, coder);
        }

        public override void ViewDidAppear(bool animated)
        {
            Mediator.ViewDidAppear(base.ViewDidAppear, animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            Mediator.ViewDidDisappear(base.ViewDidDisappear, animated);
        }

        public override void ViewDidLoad()
        {
            Mediator.ViewDidLoad(base.ViewDidLoad);
        }

        public override void ViewWillAppear(bool animated)
        {
            Mediator.ViewWillAppear(base.ViewWillAppear, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            Mediator.ViewWillDisappear(base.ViewWillDisappear, animated);
        }

        protected override void Dispose(bool disposing)
        {
            if (_mediator == null)
                base.Dispose(disposing);
            else
                Mediator.Dispose(base.Dispose, disposing);
        }

        #endregion
    }
}
