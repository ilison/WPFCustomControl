#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UWP
using Windows.Devices.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    #region NodeCurrentCellValueChangedEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValueChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellValueChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void TreeGridCurrentCellValueChangedEventHandler(object sender, TreeGridCurrentCellValueChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValueChangedEvent"/> event.
    /// </summary>
    public class TreeGridCurrentCellValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCurrentCellValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCurrentCellValueChangedEventArgs(object originalSource)
        {

        }
        /// <summary>
        /// Gets a value indicating the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the cell that the event occurs for.
        /// </summary>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the record of the corresponding cell value changes occurs for.
        /// </summary>
        /// <value>
        /// The record that contains cell value changes occurs for.
        /// </value>
        public object Record
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the column of the corresponding cell value changes occurs for.
        /// </summary>
        /// <value>
        /// The column that contains the cell value changes occurs for.
        /// </value>
        public TreeGridColumn Column
        {
            get;
            internal set;
        }
    }

    #endregion

    #region GridCurrentCellBeginEditEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellBeginEditEvent"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellBeginEditEventArgs"/> that contains the event data.
    /// </param>
    public delegate void TreeGridCurrentCellBeginEditEventHandler(object sender, TreeGridCurrentCellBeginEditEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellBeginEdit"/> event.
    /// </summary>
    public class TreeGridCurrentCellBeginEditEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCurrentCellBeginEditEventArgs(object originalSource)
        {

        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell for which the event occurred.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell for which the event occurred.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the column that contains the current cell for which the event occurred.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn"/> that contains the cell to be edited.
        /// </value>
        public TreeGridColumn Column
        {
            get;
            internal set;
        }

    }
    #endregion

    #region CellTappedEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellTapped"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellTappedEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridCellTappedEventHandler(object sender, TreeGridCellTappedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellTapped"/> event.
    /// </summary>
    public class TreeGridCellTappedEventArgs : CellTappedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellTappedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCellTappedEventArgs(object originalSource) : base(originalSource)
        {

        }

        /// <summary>
        /// Gets the TreeNode of tapped cell.
        /// </summary>
        /// <value>
        /// The TreeNode of tapped cell.
        /// </value>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion

    #region CellDoubleTappedEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellDoubleTapped"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellDoubleTappedEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridCellDoubleTappedEventHandler(object sender, TreeGridCellDoubleTappedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellDoubleTapped"/> event.
    /// </summary>
    public class TreeGridCellDoubleTappedEventArgs : CellDoubleTappedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellDoubleTappedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCellDoubleTappedEventArgs(object originalSource)
            : base(originalSource)
        {

        }

        /// <summary>
        /// Gets the TreeNode of tapped cell.
        /// </summary>
        /// <value>
        /// The TreeNode of tapped cell.
        /// </value>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion
    #region CellToolTipOpeningEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellToolTipOpening"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellToolTipOpeningEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridCellToolTipOpeningEventHandler(object sender, TreeGridCellToolTipOpeningEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellToolTipOpening"/> event.
    /// </summary>
    public class TreeGridCellToolTipOpeningEventArgs : CellToolTipOpeningEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellToolTipOpeningEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCellToolTipOpeningEventArgs(object originalSource) : base(originalSource)
        {

        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNode"> of hovered cell.
        /// </summary>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion
    #region CellValidatingEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidating"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellValidatingEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridCurrentCellValidatingEventHandler(object sender, TreeGridCurrentCellValidatingEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidating"/> event.
    /// </summary>
    public class TreeGridCurrentCellValidatingEventArgs : CurrentCellValidatingEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellValidatingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCurrentCellValidatingEventArgs(object originalSource)
            : base(originalSource)
        {

        }

        /// <summary>
        /// Gets the GridColumn of the cell triggers this event.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn"/> of the cell which triggered this event. 
        /// </value>
        /// <remarks>
        /// GridTemplateColumn cells will not triggers this event.
        /// </remarks>
        public TreeGridColumn Column
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the TreeNode .
        /// </summary>
        /// <value>
        /// The TreeNode .
        /// </value>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion    

    #region CellValidatedEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidated"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellValidatedEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridCurrentCellValidatedEventHandler(object sender, TreeGridCurrentCellValidatedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidated"/> event.
    /// </summary>
    public class TreeGridCurrentCellValidatedEventArgs : CurrentCellValidatedEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellValidatedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridCurrentCellValidatedEventArgs(object originalSource)
            : base(originalSource)
        {

        }

        /// <summary>
        /// Gets the TreeNode .
        /// </summary>
        /// <value>
        /// The TreeNode .
        /// </value>
        public TreeNode Node
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the GridColumn of the cell triggers this event.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn"/> of the cell which triggered this event. 
        /// </value>
        /// <remarks>
        /// GridTemplateColumn cells will not triggers this event.
        /// </remarks>
        public TreeGridColumn Column
        {
            get;
            internal set;
        }
    }

    #endregion

    #region RowValidatingEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowValidating"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowValidatingEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridRowValidatingEventHandler(object sender, TreeGridRowValidatingEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowValidating"/> event.
    /// </summary>
    public class TreeGridRowValidatingEventArgs : RowValidatingEventArgs
    {
        public TreeGridRowValidatingEventArgs(object _rowData, int _rowIndex, Dictionary<string, string> errorMessages, object originalSource, TreeNode treeNode)
            : base(_rowData, _rowIndex, errorMessages, originalSource)
        {
            Node = treeNode;
        }

        /// <summary>
        /// Gets the TreeNode .
        /// </summary>
        /// <value>
        /// The TreeNode .
        /// </value>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion

    #region RowValidatedEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowValidated"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the TreeGridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowValidatedEventArgs"/> that contains the event data.</param>
    public delegate void TreeGridRowValidatedEventHandler(object sender, TreeGridRowValidatedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowValidated"/> event.
    /// </summary>
    public class TreeGridRowValidatedEventArgs : RowValidatedEventArgs
    {
        public TreeGridRowValidatedEventArgs(object _rowData, int _rowIndex, Dictionary<string, string> errorMessages, object originalSource, TreeNode treeNode)
            : base(_rowData, _rowIndex, errorMessages, originalSource)
        {
            Node = treeNode;
        }

        /// <summary>
        /// Gets the TreeNode .
        /// </summary>
        /// <value>
        /// The TreeNode .
        /// </value>
        public TreeNode Node
        {
            get;
            internal set;
        }
    }

    #endregion
}
