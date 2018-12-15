#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnbounColumnValue"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridUnboundColumnEventsArgs"/> that contains the event data.
    /// </param>
    public delegate void QueryUnbounColumnValueHandler(object sender, GridUnboundColumnEventsArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnbounColumnValue"/> event.
    /// </summary>
    public class GridUnboundColumnEventsArgs : GridEventArgs
    {
        #region ctor
        internal GridUnboundColumnEventsArgs(UnBoundActions action, object value, GridColumn column, object record, object originalSource)
            : base(originalSource)
        {
            this.UnBoundAction = action;
            this.Value = value;
            this.Column = column;
            this.Record = record;
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets the GridColumn of the cell triggers this event.
        /// </summary>
        public GridColumn Column { get; internal set; }

        /// <summary>
        /// Gets or sets the value for GridUnBoundColumn cell based on UnBoundAction. 
        /// </summary>
        /// <value>
        /// An object that contains the value for the GridUnBoundColumn.
        /// </value>        
        public object Value { get; set; }

        /// <summary>        
        /// Gets the data object associated with the row which has the grid cell triggered this event.
        /// </summary>
        public object Record { get; internal set; }

        /// <summary>
        /// Defines the constants that specifies the actions for triggering the QueryUnboundColumnValue event. 
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.UnBoundActions"/> which triggered this event.
        /// </value>
        /// <remarks>The UnBoundAction – QueryData denotes, the event is raised to get or query the value  for cell and the UnBoundAction - CommitData denotes, the event is raised to notify or commit the edited value.</remarks>
        public UnBoundActions UnBoundAction { get; internal set; }
        #endregion
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnBoundRow"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRowEventsArgs"/> that contains the event data.
    /// </param>
    public delegate void QueryUnBoundRowHandler(object sender, GridUnBoundRowEventsArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnBoundRow"/> event.
    /// </summary>
    public class GridUnBoundRowEventsArgs :GridHandledEventArgs

    {
        internal GridUnBoundRowEventsArgs(GridUnBoundRow GridUnBoundRow, UnBoundActions action,object value, GridColumn column,String cellType, object originalSource,RowColumnIndex rowColumnIndex) 
            :base(originalSource)
        {
            this.GridUnboundRow = GridUnBoundRow;
            this.UnBoundAction = action;
            this.Value = value;
            this.Column = column;
            this.CellType = cellType;
            this.RowColumnIndex = rowColumnIndex;            
            this.CellTemplate = null;
            this.EditTemplate = null;
        }

        internal bool hasCellTemplate;
        internal bool hasEditTemplate;

        /// <summary>
        /// Gets the associated GridUnBoundRow of the cell triggered event.
        /// </summary>
        public GridUnBoundRow GridUnboundRow { get; internal set; }

        /// <summary>
        /// Defines the constants that specifies the actions for triggering the QueryUnboundRow event. 
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.UnBoundActions"/> which triggered this event.
        /// </value>
        /// <remarks>The UnBoundAction – QueryData denotes, the event is raised to get or query the value and settings for cell and the UnBoundAction - CommitData denotes, the event is raised to notify or commit the edited value.</remarks>
        public UnBoundActions UnBoundAction { get; internal set; }

        /// <summary>
        /// Gets or sets the value for the cell in unbound row based on UnboundAction .
        /// </summary>
        /// <value>
        /// An object that contains the value for the unbound cell.
        /// </value>
        public object Value { get; set; }

        /// <summary>
        /// Gets the GridColumn of the UnboundRow cell triggers this event.
        /// </summary>
        public GridColumn Column { get; internal set; }

        /// <summary>        
        /// Gets or sets the cell type associated with SfDataGrid.UnBoundRowCellRenderers for UnboundRow cell. 
        /// </summary>
        /// <value>
        /// A string that specifies the cell type of unbound cell.
        /// </value>
        /// <remarks>SfDataGrid allows to add custom cell types by adding associated renderer in SfDataGrid.UnBoundRowCellRenderers.</remarks>
        public string CellType {get;set;}

        /// <summary>
        /// Gets or sets the RowColumnIndex of UnboundRow cell which triggers this event.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the unbound cell which triggers this event.
        /// </value>
        public RowColumnIndex RowColumnIndex { get; set; }

        /// <summary>        
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> that is used to display the contents of UnboundRow cell is in display mode.
        /// </summary>
        /// <value>
        /// The template that is used to display the contents of unbound cell that is not in editing mode. 
        /// </value>
        public DataTemplate CellTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> that is used to display the contents of UnboundRow cell is in editing mode.
        /// </summary>
        /// <value>
        /// The template that is used to display the contents of unbound cell that is in editing mode.
        /// </value>
        public DataTemplate EditTemplate { get; set; }

    }
    #region Enum UnBoundAction
    /// <summary>
    /// Defines the constants that specify the possible actions for unbound column or unbound row in SfDataGrid.
    /// </summary>
    public enum UnBoundActions
    {
        /// <summary>
        /// The value for unbound column or unbound row being queried from the user.
        /// </summary>
        QueryData,

        /// <summary>
        /// The value is being committed in unbound column or unbound row.
        /// </summary>
        CommitData,

        /// <summary>
        /// The value is being pasted in unbound column or unbound row.
        /// </summary>
        PasteData
    }
    #endregion

}
