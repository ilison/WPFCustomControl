#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.ComponentModel;
using System.Collections.Generic;
#if WinRT
using Windows.UI.Xaml;
#else
using System.Windows;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    #region Event args & Handlers

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanging"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionChangingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridSelectionChangingEventHandler(object sender, GridSelectionChangingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanging"/> event.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridSelectionChangingEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionChangingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridSelectionChangingEventArgs(object originalSource)
            : base(originalSource)
        {

        }

        /// <summary>
        /// Gets a list that contains the items that are being selected.
        /// </summary>
        /// <value>
        /// The list that contains the items that are being selected.
        /// </value>        
        public IList<object> AddedItems { get; internal set; }

        /// <summary>
        /// Gets a list that contains the items that are being unselected.
        /// </summary>
        /// <value>
        /// The list that contains the items that are being unselected.
        /// </value> 
        public IList<object> RemovedItems { get; internal set; }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridSelectionChangedEventHandler(object sender, GridSelectionChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanged"/> event.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridSelectionChangedEventArgs : GridEventArgs
    {

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridSelectionChangedEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets a list that contains the items that were selected.
        /// </summary>
        /// <value>
        /// The list that contains the items that were selected.
        /// </value>      
        public IList<object> AddedItems { get; internal set; }

        /// <summary>
        /// Gets a list that contains the items that were unselected.
        /// </summary>
        /// <value>
        /// The list that contains the items that are being unselected.
        /// </value>      
        public IList<object> RemovedItems { get; internal set; }
    }

    #endregion
}
