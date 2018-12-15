#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
#if WinRT || UNIVERSAL
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCopyPaste"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridCopyPasteEventHandler(object sender, GridCopyPasteEventArgs e);

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CopyGridCellContent"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridCopyPasteCellEventHandler(object sender, GridCopyPasteCellEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCopyPaste"/> event.
    /// </summary>
    public class GridCopyPasteEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Get or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPaste"/> event was handled.
        /// </summary>
        /// <value>
        /// <b>true</b> if the event is handled; otherwise, <b>false</b>.
        /// </value>
        public bool Handled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/> class.
        /// </summary>
        /// <param name="handled">
        /// Indicates whether the event is handled.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridCopyPasteEventArgs(bool handled,
                                      object originalSource)
            : base(originalSource)
        {
            this.Handled = handled;
        }

    }

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CopyGridCellContent"/> event.
    /// </summary>
    public class GridCopyPasteCellEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Get or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.CopyGridCellContent"/> event was handled.
        /// </summary>
        /// <value>
        /// <b>true</b> if the event is handle; otherwise, <b>false</b>.
        /// </value>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the corresponding column that contains the selected cells in the row.
        /// </summary>
        public GridColumn Column { get; internal set; }

        /// <summary>
        /// Gets the data item for the row for which the event occurred.
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
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs"/> class.
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
        public GridCopyPasteCellEventArgs(bool handled, GridColumn column,object OriginalSender, object rowData,
                                                                  object clipBoardValue)
            : base(OriginalSender)
        {
            this.Handled = handled;
            this.RowData = rowData;
            this.ClipBoardValue = clipBoardValue;
            this.Column = column;
        }
    }
}
