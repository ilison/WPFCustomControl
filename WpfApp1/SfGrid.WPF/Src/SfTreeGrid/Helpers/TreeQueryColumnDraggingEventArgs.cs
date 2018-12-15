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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if UWP
using Windows.Foundation;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ColumnDragging"/> event.
    /// </summary>
    public class TreeGridColumnDraggingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDraggingEventArgs"/> event.
        /// </summary>
        public TreeGridColumnDraggingEventArgs()
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
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ColumnDragging"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDraggingEventArgs"/> that contains the event data.
    /// </param>    
    public delegate void TreeGridColumnDraggingEventHandler(object sender, TreeGridColumnDraggingEventArgs e);
}
