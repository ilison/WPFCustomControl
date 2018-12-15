#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridSortColumnsChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridSortColumnsChangedEventHandler(object sender, GridSortColumnsChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanged"/> event.
    /// </summary>
    [ClassReference(IsReviewed = false)]   
    public sealed class GridSortColumnsChangedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSortColumnsChangedEventArgs"/> class.
        /// </summary>
        /// <param name="addedItems">
        /// The list of items that were sorted in view.
        /// </param>
        /// <param name="removedItems">
        /// The list of items that were unsorted in view.
        /// </param>
        /// <param name="action">
        /// Indicates the corresponding collection changed actions performed on the data.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridSortColumnsChangedEventArgs(IList<SortColumnDescription> addedItems, IList<SortColumnDescription> removedItems, NotifyCollectionChangedAction action, object originalSource)
            : base(originalSource)
        {
            this.AddedItems = addedItems;
            this.RemovedItems = removedItems;
            this.Action = action;
        }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> that were sorted in view.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> collection that were sorted in view.
        /// </value>
        public IList<SortColumnDescription> AddedItems { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> that were unsorted in view.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> collection that were unsorted in view.
        /// </value>
        public IList<SortColumnDescription> RemovedItems { get; private set; }

        /// <summary>
        /// Gets the corresponding collection changed actions performed during sorting operation.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Collections.Specialized.NotifyCollectionChangedAction"/> performed during sorting operation.
        /// </value>
        public NotifyCollectionChangedAction Action { get; private set; }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanging"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridSortColumnsChangingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridSortColumnsChangingEventHandler(object sender, GridSortColumnsChangingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanging"/> event.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public sealed class GridSortColumnsChangingEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSortColumnsChangingEventArgs"/> class.
        /// </summary>
        /// <param name="addedItems">
        /// The list of SortColumnDescription that are being sorted in view.
        /// </param>
        /// <param name="removedItems">
        /// The list of SortColumnDescription that are being unsorted in view.
        /// </param>
        /// <param name="action">
        /// Indicates the corresponding collection changed actions during sorting operation.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridSortColumnsChangingEventArgs(IList<SortColumnDescription> addedItems, IList<SortColumnDescription> removedItems, NotifyCollectionChangedAction action, object originalSource)
            : base(originalSource)
        {
            this.AddedItems = addedItems;
            this.RemovedItems = removedItems;
            this.Action = action;
        }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> that are being sorted in view.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> collection that were sorted in view.
        /// </value>
        public IList<SortColumnDescription> AddedItems { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> that are being unsorted in view.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> collection that were unsorted in view.
        /// </value>
        public IList<SortColumnDescription> RemovedItems { get; private set; }

        /// <summary>
        /// Gets the corresponding collection changed actions performed during sorting operation.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Collections.Specialized.NotifyCollectionChangedAction"/> performed during sorting operation.
        /// </value>
        public NotifyCollectionChangedAction Action { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SfDataGrid should scroll to CurrentItem after sorting.
        /// </summary>
        /// <value>
        /// <b>true</b> if the scrolling is cancelled while sorting; otherwise, <b>false</b>.
        /// </value>        
        public bool CancelScroll { get; set; }
    }

    /// <summary>
    /// Provides data for handling selection in sorted column.
    /// </summary>
    public class SortColumnChangedHandle
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnChangedHandle"/> class.
        /// </summary>
        public SortColumnChangedHandle()
        {

        }
        /// <summary>
        /// Gets or sets a value that indicates whether SfDataGrid scroll to CurrentItem after sorting.
        /// </summary>
        /// <value>
        /// <b>true</b> if scrolled to CurrentItem; otherwise, <b>false</b>.
        /// </value>
        public bool ScrollToCurrentItem { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether IsProgrammatic will be set whether the sorting applied programmatically.
        /// <value>
        /// <b>true</b> if IsProgrammatic set from code behind; otherwise, <b>false</b>.
        /// </value>
        public bool IsProgrammatic { get; set; }
    }
}
