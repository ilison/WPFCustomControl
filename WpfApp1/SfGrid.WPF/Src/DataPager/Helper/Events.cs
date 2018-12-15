#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Syncfusion.UI.Xaml.Controls.DataPager
{
    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanging"/> event.
    /// </summary>
    /// <remarks>c
    /// To skip the page navigation, you have to set Cancel as true in <see cref="System.ComponentModel.CancelEventArgs"/> of <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanging"/> event.
    /// </remarks>
    public class PageIndexChangingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Get the previous PageIndex while navigating to the other page.
        /// </summary>
        public int OldPageIndex { get; internal set; }

        /// <summary>
        /// Gets or sets the NewPageIndex while navigating from the previous page.
        /// </summary>
        /// <remarks>
        /// While navigating, NewPageIndex can be changed in <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanging"/> event.
        /// </remarks>
        public int NewPageIndex { get; set; }
    }

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanged"/> event.
    /// </summary>
    public class PageIndexChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previous PageIndex once navigated to the new page.
        /// </summary>
        public int OldPageIndex { get; internal set; }

        /// <summary>
        /// Gets the current PageIndex once navigated to the new page.
        /// </summary>
        public int NewPageIndex { get; internal set; }
    }

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.OnDemandLoading"/> event.
    /// </summary>
    public class OnDemandLoadingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the index based on <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndex"/> which is (Number of previous pages * <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageSize"/>).
        /// </summary>
        public int StartIndex { get; internal set; }

        /// <summary>
        /// Gets the number of records to be displayed in the page.
        /// </summary>
        public int PageSize { get; internal set; }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void PageIndexChangedEventhandler(object sender, PageIndexChangedEventArgs e);

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanging"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void PageIndexChangingEventhandler(object sender, PageIndexChangingEventArgs e);

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.OnDemandLoading"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Controls.DataPager.OnDemandLoadingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void OnDemandLoadingEventHandler(object sender, OnDemandLoadingEventArgs e);
}