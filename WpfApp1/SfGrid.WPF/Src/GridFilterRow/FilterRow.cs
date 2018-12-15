#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.RowFilter;
using System.ComponentModel.DataAnnotations;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Linq;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
    /// <summary>
    /// Represents a class that used to initialize the FilterRow with different Renderers
    /// </summary>
    public class FilterRow : DataRow
    {

        #region Ctr

        public FilterRow()
        {
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Method which ensure the each columns in particular DataRow.
        /// </summary>
        /// <param name="visibleColumnLines"></param>
        internal override void EnsureColumns(VisibleLinesCollection visibleColumnLines)
        {
            if (this.DataGrid.FilterRowPosition == FilterRowPosition.None)
                return;

            base.EnsureColumns(visibleColumnLines);
        }

        /// <summary>
        /// Method which used to create the column for the FilterRow.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="columnHeightIncrementation"></param>
        /// <returns></returns>
        internal override DataColumnBase CreateColumn(int index, int columnHeightIncrementation)
        {
            DataColumnBase dc = new DataColumn();
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.RowSpan = columnHeightIncrementation;
            var columnIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(index);
            dc.GridColumn = this.DataGrid.Columns[columnIndex];
            dc.Renderer = this.GetRenderer(dc.GridColumn);

            dc.InitializeColumnElement(this.RowData, false);
            dc.SelectionController = this.DataGrid.SelectionController;
            SetCurrentCellBorderBinding(dc.ColumnElement);
            return dc;
        }

        /// <summary>
        /// Update Renderer and UnloadUIElement if needed
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal override bool UpdateRenderer(DataColumnBase dataColumn, GridColumn column)
        {
            IGridCellRenderer newRenderer = this.GetRenderer(column);
            if (dataColumn.Renderer == null)
                return false;

            if (dataColumn.Renderer != newRenderer)
            {
                dataColumn.Renderer.UnloadUIElements(dataColumn);
                dataColumn.Renderer = newRenderer;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the Column properties in particular DataColumn.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="index"></param>
        internal override void UpdateColumn(DataColumnBase dc, int index)
        {
            if (index < 0 || index >= this.DataGrid.VisualContainer.ColumnCount)
            {
                dc.ColumnVisibility = Visibility.Collapsed;
            }
            else
            {
                dc.ColumnIndex = index;
                dc.RowSpan = 0;

                var currentColumn = this.DataGrid.Columns[this.DataGrid.ResolveToGridVisibleColumnIndex(index)];
                this.UpdateRenderer(dc, currentColumn);

                dc.GridColumn = currentColumn;

                if (dc.ColumnVisibility == Visibility.Collapsed)
                    dc.ColumnVisibility = Visibility.Visible;

                dc.InitializeColumnElement(this.RowData, dc.IsEditing);
                dc.UpdateCellStyle(this.RowData);
                //WPF - 27647 - update the filter cell styles when horizontally scrolling the view.
                var gridFilterRowCell = dc.ColumnElement as GridFilterRowCell;
                if(gridFilterRowCell != null)
                    gridFilterRowCell.OnColumnChanged();
            }
        }

        /// <summary>
        /// Gets the corresponding renderer for the given GridColumn.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected IGridCellRenderer GetRenderer(GridColumn column)
        {
            IGridCellRenderer renderer = null;
            
            if (this.DataGrid.FilterRowCellRenderers.ContainsKey(column.FilterRowEditorType))
                renderer = this.DataGrid.FilterRowCellRenderers[column.FilterRowEditorType];
            else
                renderer = this.DataGrid.FilterRowCellRenderers["TextBox"];

            return renderer;
        }

        #endregion

    }

    /// <summary>
    /// Provides classes and interfaces for processing FilterRow, that enable a user to filter the data in SfDataGrid.
    /// </summary>
    class NamespaceDoc
    { }
}
