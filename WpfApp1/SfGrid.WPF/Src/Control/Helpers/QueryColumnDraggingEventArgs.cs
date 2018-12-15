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
using System.Windows;
#if WinRT || UNIVERSAL
using Windows.Foundation;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryColumnDragging"/> event.
    /// </summary>
    public class QueryColumnDraggingEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.QueryColumnDraggingEventArgs"/> event.
        /// </summary>
        /// <param name="dataGrid">
        /// The source of the event.
        /// </param>
        public QueryColumnDraggingEventArgs(SfDataGrid dataGrid) : base(dataGrid)
        {            
        }

        /// <summary>
        /// Gets the index of the column that is being dragged.    
        /// </summary>
        /// <value>
        /// An index of the column being dragged.
        /// </value>
        public int From { get; internal set; }

        /// <summary>
        /// Gets the index of the column that is being dropped.
        /// </summary>
        /// <value>
        /// An index of the column being dropped.
        /// </value>
        public int To { get; internal set; }
        
        /// <summary>
        /// Gets the position of popup during a column drag-and-drop operation.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Point"/> that represents the position of popup during the column drag-and-drop operation.
        /// </value>
        public Point PopupPosition { get; internal set; }

        /// <summary>
        /// Gets the reason for column drag-and-drop operation.
        /// </summary>
        /// <value>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.QueryColumnDraggingReason"/> that specifies the reason for column dragging operation.
        /// </value>
        public QueryColumnDraggingReason Reason { get; internal set; }
    }

    /// <summary>
    /// Specifies the reason for column dragging operation.
    /// </summary>
    public enum QueryColumnDraggingReason
    {
        /// <summary>
        /// Specifies the dragging operation is being initiated on the column.
        /// </summary>
        DragStarting,

        /// <summary>
        /// Specifies the dragging operation after initialized on the column.
        /// </summary>
        DragStarted,

        /// <summary>
        /// Specifies the column is being dragged in SfDataGrid.
        /// </summary>
        Dragging,

        /// <summary>
        /// Specifies the column is being dropped on the DataGrid.
        /// </summary>
        Dropping,

        /// <summary>
        /// Specifies the column is after dropped in SfDataGrid.
        /// </summary>
        Dropped
    }

     /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryColumnDragging"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.QueryColumnDraggingEventArgs"/> that contains the event data.
    /// </param>    
    public delegate void QueryColumnDraggingEventHandler(object sender, QueryColumnDraggingEventArgs e);

}
