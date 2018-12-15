#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Represents the nodes by handling the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CopyCellContent"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteCellEventArgs"/> that contains the event data.
    /// </param>
    public delegate void TreeGridCopyPasteCellEventHandler(object sender, TreeGridCopyPasteCellEventArgs e);

    /// <summary>
    /// Provides the node cell value by handling <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CopyCellContent"/> event.
    /// </summary>
    public class TreeGridCopyPasteCellEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Get or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CopyCellContent"/> event was handled.
        /// </summary>
        /// <value>
        /// <b>true</b> if the event is handle; otherwise, <b>false</b>.
        /// </value>
        public bool Handled { get; set; }
        /// <summary>
        /// Gets the corresponding column that contains the selected cells in the row.
        /// </summary>
        public TreeGridColumn Column { get; internal set; }
        /// <summary>
        /// Gets the node item for the row for which the event occurred.
        /// </summary>
        public object RowData { get; internal set; }

        /// <summary>
        /// Gets or sets an object that represents the value of the selected cells being copied.
        /// </summary>
        /// <value>
        /// An object that contains the value of selected cells.
        /// </value>
        public object ClipBoardValue { get; set; }
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteCellEventArgs"/> class.
        /// </summary>
        /// <param name="handled">
        /// Indicates whether the event is canceled.
        /// </param>
        /// <param name="column">
        /// The corresponding column that contains the selected cells in row.
        /// </param>
        /// <param name="OriginalSender">
        /// The source of the event.
        /// </param>
        /// <param name="rowData">
        /// The data item for the row for which the event occurred.
        /// </param>
        /// <param name="clipBoardValue">
        /// Contains the value selected cells for copy operation.
        /// </param>
        public TreeGridCopyPasteCellEventArgs(bool handled, TreeGridColumn column, object OriginalSender, object rowData,
                                                                  object clipBoardValue)
            : base(OriginalSender)
        {
            this.Handled = handled;
            this.Column = column;
            this.RowData = rowData;
            this.ClipBoardValue = clipBoardValue;
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ItemsSourceChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.GridItemsSourceChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void TreeGridItemsSourceChangedEventHandler(object sender, TreeGridItemsSourceChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ItemsSourceChanged"/> event.
    /// </summary>
    public class TreeGridItemsSourceChangedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.GridItemsSourceChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        /// <param name="oldItemsSource">
        /// The old ItemsSource of the SfTreeGrid.
        /// </param>
        /// <param name="newItemsSource">
        /// The new ItemsSource of the SfTreeGrid.
        public TreeGridItemsSourceChangedEventArgs(object originalSource, object oldItemsSource, object newItemsSource)
            : base(originalSource)
        {
            this.OldItemsSource = oldItemsSource;
            this.NewItemsSource = newItemsSource;
        }

        /// <summary>
        /// Gets an old ItemsSource of SfTreeGrid that is need to be replaced.
        /// </summary>
        /// <value>
        /// An object that contains the old ItemsSource of SfTreeGrid.
        /// </value>
        public object OldItemsSource { get; internal set; }

        /// <summary>
        /// Gets the new ItemsSource of SfTreeGrid.
        /// </summary>
        /// <value>
        /// An object that contains the new ItemsSource of SfTreeGrid.
        /// </value>
        public object NewItemsSource { get; internal set; }

    }
}
