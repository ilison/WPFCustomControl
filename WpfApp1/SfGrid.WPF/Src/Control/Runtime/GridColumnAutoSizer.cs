#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.Generic;
using System.Linq;
using System;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
#if UWP
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid.Cells;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.Data;
#endif


namespace Syncfusion.UI.Xaml.Grid
{

    /*  Work Flow
     ** InitialRefresh method
      * 1. Call Refresh method.
      * 2. Wire LineHiddenChanged event
      * 3. Apply VSM for hidden columns
     * *  Refresh method
           * 1. InitializeColumnWPropertyChangedDelegate
           * 2. InitializeUnboundColumnPropertiesDelegate
         * * 3. SetSizerWidth 
                * Set column width based on individual columns width and column sizer
                 Hide hidden columns
                 Column width based on column.Width (explicitly defined)
                 SizeToCells
                 SizeToHeader
                 Auto and AutoWithLastColumnFill
                 Set Indent column width
                * SetGridSizerWidth
                     SizeToCells
                     SizeToHeader
                     Auto and AutoWithLastColumnFill
                     None
                     Adjust width for DetailsView
                     * SetStarWidth
                       Set Star, None and AutoWidthLastColumnFill width
     
        * 4. InvalidateMeasure
     */
    /// <summary>
    /// Represents a class that provides the implementation to calculate column widths based on different column sizer options for SfDataGrid(<see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType"/>).
    /// </summary>
    public class GridColumnSizer : ColumnSizerBase<SfDataGrid>, IDisposable
    {
        #region Fields
        private bool gridRowsFitInToView;
        private bool isdisposed = false;
        private double filterIconWidth = 28;

        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        public GridColumnSizer(SfDataGrid dataGrid)
        {
            GridBase = dataGrid;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer"/> class.
        /// </summary>
        public GridColumnSizer()
        {

        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the reference to the SfDataGrid control.
        /// </summary>
        public SfDataGrid DataGrid
        {
            get { return GridBase; }
        }

        /// <summary>
        /// Gets or sets the width of filter icon for column sizing calculation..
        /// </summary>
        /// <value>
        /// The width of the filter icon. The default value is 28.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToHeader"/> and <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> type of column sizer calculates the column width based on static filter icon width.
        /// When the filter icon width is customized, that is need to be initialized to this property for customizing column sizer calculation based on new filter icon width.
        /// </remarks>
        public double FilterIconWidth
        {
            get { return filterIconWidth; }
            set { filterIconWidth = value; }
        }


        #endregion

        private GridColumn autoFillColumn;


        #region Private Methods

        /// <summary>
        /// Gets the column to fill the remaining view port size based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.AutoLastColumnFill"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.AutoWithLastColumnFill"/> column sizer.
        /// </summary>
        /// <returns>the column to fill.</returns>        
        protected virtual GridColumn GetColumnToFill()
        {
            var column = DataGrid.Columns.LastOrDefault(c => !c.IsHidden && double.IsNaN(c.Width) && (c.ColumnSizer == GridLengthUnitType.AutoLastColumnFill || c.ColumnSizer == GridLengthUnitType.AutoWithLastColumnFill));
            if (column != null)
                return column;
            else
            {
                if (DataGrid.ColumnSizer == GridLengthUnitType.AutoLastColumnFill || DataGrid.ColumnSizer == GridLengthUnitType.AutoWithLastColumnFill)
                {
                    var lastColumn = DataGrid.Columns.LastOrDefault(c => !c.IsHidden && double.IsNaN(c.Width));
                    if (lastColumn == null)
                        return null;
                    if (lastColumn.ReadLocalValue(GridColumn.ColumnSizerProperty) == DependencyProperty.UnsetValue)
                        return lastColumn;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether specified column is fill column or auto fill column.
        /// </summary>
        /// <param name="column">the column which needs to be checked.</param>
        private bool IsFillOrAutoFillColumn(GridColumn column)
        {
            if (column == autoFillColumn)
                return true;
            return false;
        }


        /// <summary>
        /// Set Width according to individual column's ColumnSizer
        /// </summary>
        /// <remarks></remarks>
        private void SetSizerWidth(double viewPortWidth)
        {
            double totalColumnSize = 0d;
            var calculatedColumns = new List<GridColumn>();

            autoFillColumn = GetColumnToFill();

            // Hide Hidden columns
            var hiddenColumns = this.DataGrid.Columns.Where(column => column.IsHidden);
            foreach (var column in hiddenColumns)
            {
                var index = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));
                this.DataGrid.VisualContainer.ColumnWidths.SetHidden(index, index, true);
                calculatedColumns.Add(column);
            }

            // Set width based on Column.Width
            var widthColumns = this.DataGrid.Columns.Except(hiddenColumns).Where(column => !double.IsNaN(column.Width));
            foreach (var column in widthColumns)
            {
                totalColumnSize += SetColumnWidth(column, column.Width);
                calculatedColumns.Add(column);
            }

            // Set width based on SizeToCells
            var sizeToCellColumns = this.DataGrid.Columns.Except(hiddenColumns).Where(column => column.ColumnSizer == GridLengthUnitType.SizeToCells && double.IsNaN(column.Width));
            foreach (var column in sizeToCellColumns)
            {
                if (double.IsNaN(column.AutoWidth))
                {
                    var columnwidth = GetColumnSizerWidth(column, GridLengthUnitType.SizeToCells);
                    totalColumnSize += columnwidth;
                    if (DataGrid.View != null)
                        SetAutoWidth(column, columnwidth);
                }
                else
                    totalColumnSize += SetColumnWidth(column, column.AutoWidth);
                calculatedColumns.Add(column);
            }
            // Set width based on SizeToHeader
            var sizeToHeaderColumns = this.DataGrid.Columns.Except(hiddenColumns).Where(column => column.ColumnSizer == GridLengthUnitType.SizeToHeader && double.IsNaN(column.Width));
            foreach (GridColumn column in sizeToHeaderColumns)
            {
                totalColumnSize += GetColumnSizerWidth(column, GridLengthUnitType.SizeToHeader);
                calculatedColumns.Add(column);
            }

            // Set width based on Auto and AutoWithLastColumnFill
            var lastColumn = this.DataGrid.Columns.LastOrDefault(x => !x.IsHidden);
            var autoColumns = this.DataGrid.Columns.Where(column => column.ColumnSizer == GridLengthUnitType.Auto && !column.IsHidden && double.IsNaN(column.Width));

            // WPF-17135 - Need to exclude the calculated columns while applying column sizer
            var autoWithLastColumnFills = this.DataGrid.Columns.Except(calculatedColumns).Where(col => (col.ColumnSizer == GridLengthUnitType.AutoLastColumnFill || col.ColumnSizer == GridLengthUnitType.AutoWithLastColumnFill) && !IsFillOrAutoFillColumn(col)).ToList();
            autoColumns = autoColumns.Union(autoWithLastColumnFills);
            foreach (var column in autoColumns)
            {
                if (double.IsNaN(column.AutoWidth))
                {
                    var columnwidth = GetColumnSizerWidth(column, GridLengthUnitType.Auto);
                    totalColumnSize += columnwidth;
                    if (DataGrid.View != null)
                        SetAutoWidth(column, columnwidth);
                }
                else
                    totalColumnSize += SetColumnWidth(column, column.AutoWidth);
                calculatedColumns.Add(column);
            }

            // Set Width for indent columns
            if (this.DataGrid.GridModel.HasGroup)
            {
                totalColumnSize += (this.DataGrid.IndentColumnWidth) * this.DataGrid.View.GroupDescriptions.Count;
            }

            if (this.DataGrid.showRowHeader)
                totalColumnSize += this.DataGrid.RowHeaderWidth;

            if (this.DataGrid.DetailsViewManager.HasDetailsView)
                totalColumnSize += this.DataGrid.ExpanderColumnWidth;

            SetWidthBasedonGridColumnSizer(totalColumnSize, calculatedColumns, lastColumn, viewPortWidth);
            autoFillColumn = null;
        }

        /// <summary>
        /// Get column width based on column sizer except star,None column sizer
        /// </summary>
        /// <param name="column"></param>
        /// <param name="columnSizer"></param>
        /// <returns></returns>
        private double GetColumnSizerWidth(GridColumn column, GridLengthUnitType columnSizer)
        {
            double width;
            switch (columnSizer)
            {
                case GridLengthUnitType.SizeToCells:
                    width = CalculateCellWidth(column);
                    return SetColumnWidth(column, width);
                case GridLengthUnitType.SizeToHeader:
                    width = CalculateHeaderWidth(column);
                    return SetColumnWidth(column, width);
                case GridLengthUnitType.Auto:
                    width = CalculateAutoFitWidth(column, true);
                    return SetColumnWidth(column, width);
            }
            return 0;
        }

        /// <summary>
        /// Set Width based in Grid's column sizer
        /// </summary>
        /// <param name="totalColumnSize"></param>
        /// <param name="calculatedColumns"></param>
        /// <param name="lastColumn"></param>
        /// <param name="viewPortWidth"></param>
        private void SetWidthBasedonGridColumnSizer(double totalColumnSize, List<GridColumn> calculatedColumns, GridColumn lastColumn, double viewPortWidth)
        {
            foreach (var column in this.DataGrid.Columns)
            {
                if (calculatedColumns.Contains(column))
                    continue;

                if (column.ColumnSizer == GridLengthUnitType.Star || IsFillOrAutoFillColumn(column))
                    continue;

                switch (this.DataGrid.ColumnSizer)
                {
                    case GridLengthUnitType.SizeToCells:
                        if (double.IsNaN(column.AutoWidth))
                        {
                            var columnwidth = GetColumnSizerWidth(column, GridLengthUnitType.SizeToCells);
                            totalColumnSize += columnwidth;
                            if (DataGrid.View != null)
                                SetAutoWidth(column, columnwidth);
                        }
                        else
                            totalColumnSize += SetColumnWidth(column, column.AutoWidth);
                        calculatedColumns.Add(column);
                        break;
                    case GridLengthUnitType.SizeToHeader:
                        totalColumnSize += GetColumnSizerWidth(column, GridLengthUnitType.SizeToHeader);
                        calculatedColumns.Add(column);
                        break;
                    case GridLengthUnitType.AutoWithLastColumnFill:                       
                            goto case GridLengthUnitType.Auto;                    
                    case GridLengthUnitType.AutoLastColumnFill:                      
                            goto case GridLengthUnitType.Auto;                      
                    case GridLengthUnitType.Auto:
                        if (double.IsNaN(column.AutoWidth))
                        {
                            var columnWidth = GetColumnSizerWidth(column, GridLengthUnitType.Auto);
                            totalColumnSize += columnWidth;
                            if (DataGrid.View != null)
                                SetAutoWidth(column, columnWidth);
                        }
                        else
                            totalColumnSize += SetColumnWidth(column, column.AutoWidth);
                        calculatedColumns.Add(column);
                        break;

                    case GridLengthUnitType.None:
                        if (!column.IsHidden)
                        {
                            totalColumnSize += SetColumnWidth(column, this.DataGrid.VisualContainer.ColumnWidths.DefaultLineSize);
                            calculatedColumns.Add(column);
                        }
                        break;
                }
            }

            var remainingColumns = this.DataGrid.Columns.Except(calculatedColumns);
            if (viewPortWidth == 0)
            {
                if (this.DataGrid.VisualContainer.ScrollOwner != null && this.DataGrid.VisualContainer.ScrollOwner.ActualWidth != 0)
                    viewPortWidth = this.DataGrid.VisualContainer.ScrollOwner.ActualWidth;
                else if (this.DataGrid.VisualContainer.ScrollableOwner != null && this.DataGrid.VisualContainer.ScrollableOwner.ActualWidth != 0)
                    viewPortWidth = this.DataGrid.VisualContainer.ScrollableOwner.ActualWidth;
                else if (this.DataGrid is DetailsViewDataGrid)
                {
                    // WRT-4883  For DetailsViewDataGrid, column sizer is calculated based on parent grid available width
                    var parentGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
                    if (parentGrid.VisualContainer.ScrollOwner != null && parentGrid.VisualContainer.ScrollOwner.ActualWidth != 0)
                        viewPortWidth = parentGrid.VisualContainer.ScrollOwner.ActualWidth;
                    else if (parentGrid.VisualContainer.ScrollableOwner != null && parentGrid.VisualContainer.ScrollableOwner.ActualWidth != 0)
                        viewPortWidth = parentGrid.VisualContainer.ScrollableOwner.ActualWidth;
                    else
                        viewPortWidth = parentGrid.ActualWidth;
                    var indentColumns = parentGrid.ResolveToStartColumnIndex();
                    var padding = this.DataGrid.BorderThickness.Left + this.DataGrid.BorderThickness.Right + parentGrid.DetailsViewPadding.Left + parentGrid.DetailsViewPadding.Right;
                    // Since AdjustParentsWidth will be called from InitializeDetailsViewDataRow, in some cases ExtendedWidth will be set unwantedly. To adjust that, 17 is subtracted from viewPortWidth
#if WPF
                    viewPortWidth = viewPortWidth - ((indentColumns * parentGrid.ExpanderColumnWidth) + padding)- 17;
#else
                    viewPortWidth = viewPortWidth - ((indentColumns * parentGrid.ExpanderColumnWidth) + padding);
#endif
                }
#if WPF
                // DetailsViewDataGrid does not have ScrollOwner. So below condition is skipped for DetailsViewDataGrid
                //WPF-32768  Need to check ViewPort height with ExtendedHight instead of renderer size.
                if (!(this.DataGrid is DetailsViewDataGrid) && this.DataGrid.VisualContainer.ViewportHeight < this.DataGrid.VisualContainer.ExtentHeight)
                {
                    //Before Fix-21074 : this.DataGrid.VisualContainer.ViewPortHeight < this.DataGrid.VisualContainer.ExtentHeight
                    //ViewPortHeight did not include header and footer height. so VerticalScrollbar check is incorrectly calculated for some scenarios.
                    //Hence changed the condition with ScrollRows.RenderSize.
                    var _vScrollBarWidth = SystemParameters.ScrollWidth;
                    if (this.DataGrid.VisualContainer.ScrollOwner != null)
                    {
                        var vscroll = this.DataGrid.VisualContainer.ScrollOwner.Template.FindName("PART_VerticalScrollBar", this.DataGrid.VisualContainer.ScrollOwner) as ScrollBar;
                        _vScrollBarWidth = vscroll != null && vscroll.ActualWidth != 0 ? vscroll.ActualWidth : SystemParameters.ScrollWidth;
                    }
                    viewPortWidth -= _vScrollBarWidth;
                }
#endif
            }
            double remainingColumnWidths = viewPortWidth - totalColumnSize;

            //WPF 20212 - if there is only one column without row header, totalcolumnsize in 0.
            //so if totalcolumnsize is 0 and remaining column count is 0, then we can set the width to that column.
            if (remainingColumnWidths > 0 && (totalColumnSize != 0 || (totalColumnSize == 0 && remainingColumns.Count() == 1) || (this.DataGrid.Columns.Any(col => col.ColumnSizer == GridLengthUnitType.Star) || this.DataGrid.ColumnSizer == GridLengthUnitType.Star)))
                SetStarWidth(remainingColumnWidths, remainingColumns);
            else
                SetRemainingWidth(remainingColumns);
        }

        /// <summary>
        /// Set Width for column when ColumnSizer is set to None and AutoWithLastColumnFill(when column is last column)
        /// </summary>
        /// <param name="remainingColumns"></param>
        /// <remarks></remarks>
        private void SetRemainingWidth(IEnumerable<GridColumn> remainingColumns)
        {
            var lastcolumn = DataGrid.Columns.LastOrDefault(c => !c.IsHidden);
            foreach (var column in remainingColumns)
            {
                //If remaining column width is less than 0, need to set default line size or Auto width based on column sizer.
                if (IsFillOrAutoFillColumn(column))
                {
                    if (column.ColumnSizer == GridLengthUnitType.AutoLastColumnFill || (DataGrid.ColumnSizer == GridLengthUnitType.AutoLastColumnFill && column.ColumnSizer != GridLengthUnitType.AutoWithLastColumnFill))
                        GetColumnSizerWidth(column, GridLengthUnitType.Auto);
                    else
                        SetNoneWidth(column, this.DataGrid.VisualContainer.ColumnWidths.DefaultLineSize);
                }
                else
                {
                    SetNoneWidth(column, this.DataGrid.VisualContainer.ColumnWidths.DefaultLineSize);
                }
            }
        }


        /// <summary>
        /// Calculate width for header based on given column
        /// </summary>
        /// <param name="column"></param>
        /// <param name="resultWidth"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private double GetHeaderCellWidth(GridColumn column)
        {
            var colIndex = this.DataGrid.Columns.IndexOf(column);
            int rowCount = this.DataGrid.HeaderLineCount;
            int scrollColumnIndex = this.DataGrid.ResolveToScrollColumnIndex(colIndex);
            double colWidth = this.DataGrid.VisualContainer.ColumnWidths[scrollColumnIndex];
            string colHeader = string.Empty;
            int stringLength = 0;
            double resultWidth = 0;
            Size columnSize = new Size(0, 0);
            bool isInDefaultMode = column.hasHeaderTemplate || AutoFitMode == AutoFitMode.Default;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                double rowHeight = this.DataGrid.VisualContainer.RowHeights[rowIndex];
                var clientSize = new Size(colWidth, rowHeight);
                var text = column.HeaderText ?? column.MappingName;
                if (isInDefaultMode)
                {
                    var textSize = MeasureHeaderText(clientSize, text, column);
                    var width = textSize.Width;
                    if (resultWidth < width)
                        resultWidth = width;
                }
                else
                {
                    if (text.Length >= stringLength)
                    {
                        stringLength = text.Length;
                        columnSize = clientSize;
                        colHeader = text;
                    }
                }
            }

            if (!isInDefaultMode)
            {
                var textSize = MeasureHeaderText(columnSize, colHeader, column);
                resultWidth = textSize.Width;
            }

            return resultWidth;
        }

        /// <summary>
        /// Calculate width for Cells based on given column
        /// </summary>
        /// <param name="column"></param>
        /// <param name="resultWidth"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private double GetCellWidth(GridColumn column)
        {
            double resultWidth = 0;
            var colIndex = this.DataGrid.Columns.IndexOf(column);
            var recordCount = this.DataGrid.GetRecordsCount(false);
            if (recordCount == 0)
                return double.NaN;

            int scrollColumnIndex = this.DataGrid.ResolveToScrollColumnIndex(colIndex);
            double colWidth = this.DataGrid.VisualContainer.ColumnWidths[scrollColumnIndex];
            double rowHeight = this.DataGrid.VisualContainer.RowHeights.DefaultLineSize;
            textLength = 0;
            prevColumnWidth = 0;
            int stringLenth = 0;
            var isInDefaultMode = AutoFitMode == AutoFitMode.Default || (column.IsTemplate && DataGrid.hasCellTemplateSelector) || column.hasCellTemplate || column.hasCellTemplateSelector;
            var clientSize = new Size(colWidth, rowHeight);
            object record = null;

            //WPF-35112:At previous,we have calculated the column width based on all records . Now we have changes the code to calculate the column width for view records only
            foreach (var data in this.DataGrid.View.Records.GetSource())
            {
                if (data == null)
                    continue;
                if (isInDefaultMode)
                {
                    var textsize = this.GetCellSize(clientSize, column, data, GridQueryBounds.Width);
                    if (textsize.IsEmpty)
                        continue;

                    if (resultWidth < textsize.Width)
                        resultWidth = textsize.Width;
                }
                else
                {
                    var text = this.GetDisplayText(column, data);
                    if (text.Length >= stringLenth)
                    {
                        stringLenth = text.Length;
                        record = data;
                    }
                }
            }

            if (!isInDefaultMode)
            {
                var textsize = this.GetCellSize(clientSize, column, record, GridQueryBounds.Width);
                resultWidth = textsize.Width;
            }

            textLength = 0;
            prevColumnWidth = 0;
            return Math.Round(resultWidth);
        }

        /// <summary>
        /// Calculates the height of the Row based on the content present in the cell. 
        /// </summary>
        /// <param name="rowindex"></param>
        /// Corresponding index of the row.
        /// <param name="data"></param>
        /// Records of the corresponding row.
        /// <param name="options"></param>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridRowSizingOptions"/> to get row height.
        /// <returns>
        /// Returns the height of the corresponding based on the content present in the cell.
        /// </returns>
        internal double GetRowHeight(int rowindex, object data, GridRowSizingOptions options)
        {
            double resultHeight = -1;
            double rowHeight = rowindex > -1 ? (this.DataGrid.VisualContainer.RowHeights[rowindex])
                : this.DataGrid.RowHeight;
            GridColumn gridColumn = null;
            int stringLenth = 0;
            var columnSize = new Size(0, 0);
            var isInDefaultMode = AutoFitMode == AutoFitMode.Default ||
                                  this.DataGrid.Columns.Any(column => (column.IsTemplate && DataGrid.hasCellTemplateSelector) || column.hasCellTemplate || column.hasCellTemplateSelector);
            for (int i = 0; i < this.DataGrid.Columns.Count; i++)
            {
                var column = this.DataGrid.Columns[i];
                if (column.IsHidden && !options.CanIncludeHiddenColumns)
                    continue;

                if (options.ExcludeColumns.Contains(column.MappingName))
                    continue;

                var colIndex = i;
                var recordCount = this.DataGrid.GetRecordsCount();
                if (recordCount == 0)
                    return double.NaN;

                int scrollColumnIndex = this.DataGrid.ResolveToScrollColumnIndex(colIndex);
                double colWidth = this.DataGrid.VisualContainer.ColumnWidths[scrollColumnIndex];

                var clientSize = new Size(colWidth, rowHeight);
                if (isInDefaultMode)
                {
                    var textsize = this.GetCellSize(clientSize, column, data, GridQueryBounds.Height);
                    if (textsize.IsEmpty)
                        continue;

                    if (resultHeight < textsize.Height)
                        resultHeight = textsize.Height;
                }
                else
                {
                    var text = this.GetDisplayText(column, data);
                    if (text.Length >= stringLenth)
                    {
                        stringLenth = text.Length;
                        gridColumn = column;
                        columnSize = clientSize;
                    }
                }
            }

            if (!isInDefaultMode)
            {
                var textsize = this.GetCellSize(columnSize, gridColumn, data, GridQueryBounds.Height);
                resultHeight = textsize.Height;
            }
            textLength = 0;
            prevColumnWidth = 0;
            return resultHeight;
        }


        /// <summary>
        /// Calculates the height of the Row based on the content present in the record. 
        /// </summary>
        /// <param name="recordIndex">
        /// Corresponding index of the row.
        /// </param>
        /// <param name="rowindex">
        /// Records of the corresponding row.
        /// </param>
        /// <param name="options">
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridRowSizingOptions"/> to get row height.
        /// </param>
        /// <returns>
        /// Returns the height of the corresponding based on the content present in the recod.
        /// </returns>
        private double GetRecordCellHeight(int recordIndex, int rowindex, GridRowSizingOptions options)
        {
            double resultHeight = -1;
            textLength = 0;
            prevColumnWidth = 0;
            object data = null;
            if (!this.DataGrid.GridModel.HasGroup)
                data = this.DataGrid.View.Records[recordIndex].Data;
            else
            {
                var recordentry = this.DataGrid.View.TopLevelGroup.DisplayElements[recordIndex];
                if (!(recordentry is RecordEntry))
                    return -1;
                data = (recordentry as RecordEntry).Data;
            }

            if (data == null)
                return -1;

            resultHeight = GetRowHeight(rowindex, data, options);
            return Math.Round(resultHeight);
        }


        /// <summary>
        /// Returns the header row height.
        /// </summary>
        /// <param name="headerRow">DataRowBase</param>
        /// <param name="rowIndex">RowIndex</param>
        /// <param name="options">GridRowSizingOptions</param>
        /// <returns>auto-header row height</returns>
        private double GetHeaderCellHeight(int rowIndex, GridRowSizingOptions options)
        {
            double resultHeight = -1;
            int stringLenth = 0;
            //For StackedHeaderRow.

            if (rowIndex != this.DataGrid.GetHeaderIndex())
            {
                var spannedHeader = this.DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex) as SpannedDataRow;

                string text = string.Empty;
                double colWidth = 0;

                //Based on the coveredcells, stackedcolumn widths are calculated.
                foreach (var cells in spannedHeader.CoveredCells)
                {
                    var columnWidth = spannedHeader.GetCoveredColumnSize(cells.Left, cells.Right);
                    if (string.IsNullOrEmpty(cells.Name))
                        continue;

                    if (AutoFitMode == AutoFitMode.Default)
                    {
                        var calculatedSize = MeasureText(new Size(columnWidth, 0), cells.Name, null, null, GridQueryBounds.Height);
                        if (resultHeight < calculatedSize.Height)
                            resultHeight = calculatedSize.Height;
                    }
                    else
                    {
                        if (cells.Name.Length >= stringLenth)
                        {
                            text = cells.Name;
                            stringLenth = text.Length;
                            colWidth = columnWidth;
                        }
                    }

                }
                if (AutoFitMode == AutoFitMode.SmartFit)
                {
                    var calculatedSize = MeasureText(new Size(colWidth, 0), text, null, null, GridQueryBounds.Height);
                    resultHeight = calculatedSize.Height;
                }
                return Math.Round(resultHeight);
            }

            double rowHeight = this.DataGrid.VisualContainer.RowHeights[rowIndex];
            Size columnSize = new Size(0, 0);
            GridColumn gridColumn = null;
            var isInDefaultMode = AutoFitMode == AutoFitMode.Default ||
                                  this.DataGrid.Columns.Any(column => (column.IsTemplate && DataGrid.hasCellTemplateSelector) || column.hasCellTemplate || column.hasCellTemplateSelector);
            for (int i = 0; i < this.DataGrid.Columns.Count; i++)
            {
                var column = this.DataGrid.Columns[i];
                if (column.IsHidden && !options.CanIncludeHiddenColumns)
                    continue;

                if (options.ExcludeColumns.Contains(column.MappingName))
                    continue;

                var colIndex = i;
                int scrollColumnIndex = this.DataGrid.ResolveToScrollColumnIndex(colIndex);
                double colWidth = this.DataGrid.VisualContainer.ColumnWidths[scrollColumnIndex];

                var clientSize = new Size(colWidth, rowHeight);
                if (isInDefaultMode)
                {
                    var textsize = this.GetHeaderCellSize(clientSize, column);
                    if (textsize.IsEmpty)
                        continue;

                    if (resultHeight < textsize.Height)
                        resultHeight = textsize.Height;
                }
                else
                {
                    var text = column.HeaderText;
                    if (text.Length >= stringLenth)
                    {
                        stringLenth = text.Length;
                        columnSize = clientSize;
                        gridColumn = column;
                    }
                }
            }

            if (!isInDefaultMode)
            {
                var calculatedSize = this.GetHeaderCellSize(columnSize, gridColumn);
                resultHeight = calculatedSize.Height;
            }

            textLength = 0;
            prevColumnWidth = 0;
            return Math.Round(resultHeight);
        }

        /// <summary>
        /// Returns the measured header cell size.
        /// </summary>
        /// <param name="rect">Column Size</param>
        /// <param name="column">GridColumn</param>
        /// <returns>Measured size of the header cell.</returns>
        private Size GetHeaderCellSize(Size rect, GridColumn column)
        {
            Size textSize = Size.Empty;
            if (column.hasHeaderTemplate)
            {
                //if the column contains header template, then the header cell height will be calculated from the header template.
                textSize = this.MeasureHeaderTemplate(column, rect, GridQueryBounds.Height);
            }
            else
            {
                if (!string.IsNullOrEmpty(column.HeaderText) && (column.HeaderText.Length >= textLength || prevColumnWidth >= column.ActualWidth))
                {
                    textSize = MeasureText(rect, column.HeaderText, column, null, GridQueryBounds.Height);
                    textLength = column.HeaderText.Length;
                    prevColumnWidth = column.ActualWidth;
                }
            }

            return textSize;
        }

        #endregion

        #region internal methods
        /// <summary>
        /// Initialize Delegate for GridColumn
        /// </summary>
        /// <param name="columns"></param>
        /// <remarks></remarks>
        internal void InitializeColumnWPropertyChangedDelegate(Columns columns)
        {
            foreach (var column in columns)
            {
                if (column.ColumnPropertyChanged == null)
                    column.ColumnPropertyChanged = OnColumnPropertyChanged;
            }
        }

        /// <summary>
        /// Wire events and apply Resizing VSM
        /// </summary>
        /// <param name="AvailableWidth"></param>
        internal void InitialRefresh(double AvailableWidth, bool needToInvalidateMeasure= true)
        {
            this.DataGrid.Columns.ForEach(column => column.ExtendedWidth = double.NaN);
            //Reset the Previous DetailsviewcolumnWidthList
            if (this.DataGrid.DetailsViewManager.detailsViewColumnWidthList.Count != 0)
                this.DataGrid.DetailsViewManager.detailsViewColumnWidthList.Clear();
            (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();
            Refresh(AvailableWidth, needToInvalidateMeasure);
            (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();

            // To adjust parent width based on child grid width and to refreh details view data row(SL-2905)
            if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null )
            {
                (this.DataGrid.RowGenerator as RowGenerator).LineSizeChanged();
            }

            if (this.DataGrid.VisualContainer != null)
            {
                if (this.DataGrid.VisualContainer.ViewportHeight > this.DataGrid.VisualContainer.ExtentHeight)
                    gridRowsFitInToView = true;
                this.DataGrid.VisualContainer.RowHeights.LineHiddenChanged += RowHeights_LineHiddenChanged;
            }

            if (!(this.DataGrid.AllowResizingColumns && this.DataGrid.AllowResizingHiddenColumns))
                return;
            this.DataGrid.Columns.ForEach(col =>
            {
                this.DataGrid.ColumnResizingController.ProcessResizeStateManager(col);
            });
        }

        /// <summary>
        /// Gets the width of column based on <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.MinimumWidth"/> and <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.MaximumWidth"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the width.
        /// </param>
        /// <param name="Width">
        /// The corresponding column width.
        /// </param>
        /// <returns></returns>
        internal double GetColumnWidth(GridColumn column, double Width)
        {
            var colIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));

            if (!double.IsNaN(column.ExtendedWidth) && column.Width < column.ActualWidth)
            {
                return Width;
            }

            if (!double.IsNaN(column.ExtendedWidth))
            {
                if (column.ExtendedWidth < column.ActualWidth)
                    return column.ExtendedWidth;
                return column.ActualWidth;
            }

            double width = this.DataGrid.VisualContainer.ColumnWidths[colIndex];

            var resultWidth = CheckWidthConstraints(column, Width, width);
            return resultWidth;
        }
        internal override DataContextHelper GetDataContextHelper(GridColumnBase column, object record)
        {
            var dataContextHelper = new DataContextHelper { Record = record };
            dataContextHelper.SetValueBinding(column.ValueBinding, record);
            return dataContextHelper;
        }
        internal override void ResetAutoCalculations()
        {
            foreach (var column in DataGrid.Columns)
                column.AutoWidth = double.NaN;
        }

        #endregion

        #region Protected Virtual methods

        /// <summary>
        /// Gets the size of the cell to calculate the width of the specified column when column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="rect">
        /// The corresponding display rectangle of the cell to measure cell size.
        /// </param>
        /// <param name="column">
        /// The corresponding column to measure its cell size.
        /// </param>
        /// <param name="data">
        /// The corresponding data to measure the text size in cell.
        /// </param>
        /// <param name="bounds">
        /// Indicates whether the cell size is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of the cell.
        /// </returns>
        protected virtual Size GetCellSize(Size rect, GridColumn column, Object data, GridQueryBounds bounds)
        {
            Size textSize = Size.Empty;

            if ((column.IsTemplate && DataGrid.hasCellTemplateSelector) || column.hasCellTemplate || column.hasCellTemplateSelector)
            {
                textSize = this.MeasureTemplate(rect, data, column, bounds);
            }
            else
            {
                var text = this.GetDisplayText(column, data);
                //WPF-19471 Need to compare column width if 1st condition(based on text length) is failed. 
                //because some case like if column width is small compare than previous  column but content length also small compare than previous column,it skips the height calculation 
                //so row is clipped  while auto row height is applied.
                if (text.Length >= textLength || prevColumnWidth >= column.ActualWidth)
                {
                    textSize = MeasureText(rect, text, column, data, bounds);
                    textLength = text.Length;
                    prevColumnWidth = column.ActualWidth;
                }
            }
            return textSize;
        }


        /// <summary>
        /// Calculates the width for the column to fit the content when column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to calculate the width based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> column sizer.
        /// </param>
        /// <param name="isAuto">
        /// Indicates whether the column sizer type is Auto.
        /// </param>
        /// <returns>
        /// Returns the column width based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> column sizer.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the calculation of <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> column sizer.
        /// </remarks>
        internal protected virtual double CalculateAutoFitWidth(GridColumn column, bool isAuto = false)
        {
            double headerWidth = CalculateHeaderWidth(column, false);
            double cellWidth = CalculateCellWidth(column, false);
            double width;
            if (cellWidth > headerWidth)
                width = cellWidth;
            else
                width = headerWidth;
            if (isAuto)
                return width;
            return GetColumnWidth(column, width);
        }

        #endregion

        #region Protected Virtual methods

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column on which the property value changes.
        /// </param>
        /// <param name="property">
        /// The name of property that value has been changed.
        /// </param>
        protected virtual void OnColumnPropertyChanged(GridColumn column, string property)
        {
            if (isInSuspend)
                return;
            switch (property)
            {
                case "Width":
                    this.ResetAutoCalculation(column);
                    goto case "AllowSorting";
                case "MinimumWidth":
                case "FontWeight":
                case "FontStretch":
                case "FontFamily":
                case "Margin":
                case "FontSize":
                case "ColumnSizer":
                    this.ResetAutoCalculation(column);
                    goto case "AllowSorting";
                case "MaximumWidth":
                case "AllowSorting":
                    // While changing the column width, RB tree is incorrectly constructed. So Suspend and Resume updates used.
                    (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();
                    this.SetSizerWidth(0);
                    (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();

                    //To adjust parent width based on child grid width and to refresh details view data row
                    if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                        (this.DataGrid.RowGenerator as RowGenerator).LineSizeChanged();

                    this.DataGrid.VisualContainer.NeedToRefreshColumn = true;
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    break;
                case "IsHidden":
                    if (!column.IsHidden)
                    {
                        var index = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));
                        this.DataGrid.VisualContainer.ColumnWidths.SetHidden(index, index, false);
                    }
                    (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();
                    this.SetSizerWidth(0);
                    (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();
                    if (DataGrid.AllowResizingColumns && DataGrid.AllowResizingHiddenColumns)
                        this.DataGrid.ColumnResizingController.ProcessResizeStateManager(column);
                    this.DataGrid.VisualContainer.NeedToRefreshColumn = true;
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    if (this.DataGrid.GridColumnDragDropController != null)
                        this.DataGrid.GridColumnDragDropController.ColumnHiddenChanged(column);
                    break;
                case "AllowFiltering":
                    if (this.DataGrid.View is Syncfusion.Data.PagedCollectionView)
                    {
                        if ((this.DataGrid.View as Syncfusion.Data.PagedCollectionView).UseOnDemandPaging)
                            return;
                    }
                    var header = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.DataGrid.GetHeaderIndex());
                    if (header != null)
                    {
                        var columnBase = header.VisibleColumns.FirstOrDefault(col => col.GridColumn != null && col.GridColumn.MappingName.Equals(column.MappingName));
                        if (columnBase != null)
                        {
                            (columnBase.ColumnElement as GridHeaderCellControl).FilterIconVisiblity = column.AllowFiltering ? Visibility.Visible : Visibility.Collapsed;
                        }
                    }
                    break;
                case "FilterRowCellStyle":
                case "HeaderTemplate":
                case "CellStyle":
                case "CellStyleSelector":
                case "HeaderStyle":
                case "CellTemplate":
                case "CellTemplateSelector":
                    this.DataGrid.OnColumnStyleChanged(column,property);
                    break;
            }
        }

        /// <summary>
        /// Calculates the width of the column based on header text when column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToHeader"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to calculate its header width.
        /// </param>
        /// <param name="setWidth">
        /// Indicates whether calculated header width is set as an actual width of the column.
        /// </param>
        /// <returns>
        /// Returns the width of the specified column.
        /// </returns>
        /// <remarks>
        /// This method is invoked when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToHeader"/> and you can customize column width calculation by overriding this method.
        /// </remarks>
        protected virtual double CalculateHeaderWidth(GridColumn column, bool setWidth = true)
        {
            double width = this.GetHeaderCellWidth(column);

            if (this.DataGrid.CanSetAllowFilters(column))
                width += FilterIconWidth;

            bool tempWidthSortFlag = false;
            if (column.AllowSorting)
            {
                width += SortIconWidth;
                tempWidthSortFlag = true;
            }

            if (this.DataGrid.RowGenerator.Items.Count > 0 && !tempWidthSortFlag)
            {
                DataRowBase dataRow = this.DataGrid.RowGenerator.Items[this.DataGrid.GetHeaderIndex()];
                if (dataRow != null)
                {
                    if (dataRow.VisibleColumns.Any(col => col.GridColumn != null && col.GridColumn.MappingName == column.MappingName))
                    {
                        DataColumnBase dataColumn = dataRow.VisibleColumns.FirstOrDefault(col => col.GridColumn != null && col.GridColumn.MappingName == column.MappingName);
                        if (dataColumn != null)
                        {
                            if (dataColumn.ColumnElement is GridHeaderCellControl)
                            {
                                if ((dataColumn.ColumnElement as GridHeaderCellControl).SortDirection != null)
                                    width += SortIconWidth;
                            }
                        }
                    }
                }
            }
#if WPF
            //WPF-19593 while using Formatted text to calculate width it  differ constantly 5 from textblock measure method so add 5 constantly to all columns.
            if (AllowMeasureTextByFormattedText)
                width += 2;
#endif
            if (setWidth)
                column.ActualWidth = Math.Round(width);
            return Math.Round(width);
        }

        /// <summary>
        /// Calculates the width for the column based on cell value when column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to calculate the cell width.  
        /// </param>
        /// <param name="setWidth">
        /// Indicates whether calculated cell width is set as an actual width of the column.
        /// </param>
        /// <returns>
        /// Returns the corresponding width for the specified column.
        /// </returns>
        /// <remarks>
        /// This method is invoked when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> and you can customize column width calculation by overriding this method.
        /// </remarks>
        protected virtual double CalculateCellWidth(GridColumn column, bool setWidth = true)
        {
            if (this.DataGrid.View == null)
                return column.ActualWidth;
            if (setWidth)
            {
                column.ActualWidth = this.GetCellWidth(column);
                return column.ActualWidth;
            }
            else
                return this.GetCellWidth(column);
        }

        /// <summary>
        /// Gets the display text of cell for the specified column and record.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the display text of cell.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the display text.
        /// </param>
        /// <returns>
        /// Returns the display text of the corresponding column.
        /// </returns>
        protected virtual string GetDisplayText(GridColumn column, object record)
        {
            if (!column.IsUnbound)
            {
                var boundValue = this.DataGrid.View.GetPropertyAccessProvider().GetFormattedValue(record, column.MappingName);
                if (boundValue != null)
                    return boundValue.ToString();
            }
            else
            {
                var unboundValue = this.DataGrid.GetUnBoundCellValue(column, record);
                if (unboundValue != null)
                    return unboundValue.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Refreshes the column width when the ColumnSizer property value changes at SfDataGrid or GridColumn level.
        /// </summary>
        /// <param name="AvailableWidth">
        /// The available width to refresh the column width.
        /// </param>
        protected virtual void Refresh(double AvailableWidth)
        {
            this.Refresh(AvailableWidth, true);
        }
        
        /// <summary>
        /// Refreshes the column width when the ColumnSizer property value changes at SfDataGrid or GridColumn level.
        /// </summary>
        /// <param name="AvailableWidth">The available width to refresh the column width.</param>
        /// <param name="needToInvalidateMeasure">true if need to invalidate the measure of visual container </param>
        internal void Refresh(double AvailableWidth, bool needToInvalidateMeasure)
        {
            var sizerColumns = this.DataGrid.Columns.Where(x => x.ReadLocalValue(GridColumn.ColumnSizerProperty) == DependencyProperty.UnsetValue);
            if ((int)this.DataGrid.ColumnSizer != -1 || sizerColumns.Count() > 0)
            {
                InitializeColumnWPropertyChangedDelegate(this.DataGrid.Columns);
                InitializeUnboundColumnPropertiesDelegate(this.DataGrid.Columns);
                this.SetSizerWidth(AvailableWidth);

                if (!needToInvalidateMeasure)
                    return;

                if (this.DataGrid.VisualContainer.ScrollOwner != null)
                {
                    this.DataGrid.VisualContainer.NeedToRefreshColumn = true;
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    this.DataGrid.VisualContainer.ScrollOwner.InvalidateMeasure();
                    this.DataGrid.VisualContainer.UpdateScrollBars();
                }
                else if (this.DataGrid.VisualContainer.ScrollableOwner != null)
                {
                    this.DataGrid.VisualContainer.NeedToRefreshColumn = true;
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    this.DataGrid.VisualContainer.ScrollableOwner.InvalidateMeasure();
                    this.DataGrid.VisualContainer.UpdateScrollBars();
                }
            }
        }

        /// <summary>
        /// Measures the text of the specified column when the column sizing is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="rectangle">
        /// The corresponding display rectangle to measure the text.
        /// </param>
        /// <param name="displayText">
        /// The displayText to measure.
        /// </param>
        /// <param name="column">
        /// The corresponding column to measure the text.
        /// </param>
        /// <param name="record">
        /// The corresponding record to measure the text.
        /// </param>
        /// <param name="bounds">
        /// Indicates whether the text is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of text.
        /// </returns>
        protected virtual Size MeasureText(Size rectangle, string displayText, GridColumn column, object record, GridQueryBounds queryBounds)
        {
            GridQueryBounds queryBound = queryBounds;
#if WPF
            if (AllowMeasureTextByFormattedText)
            {
                //WRT-4791 Using FormattedText method to calculate the Column with and Row Height.Because it is faster than previous method(textblock measure method).
                //To enable this option to set AllowMeasureTextByFormattedText API is true .By Default it is Enabled.
                FormattedText formattedtext = GetFormattedText(column, record, displayText);
                return MeasureTextByFormattedText(rectangle, column, record, queryBound, formattedtext);
            }
            else
#endif
            {
                var textBlock = GetTextBlock(column, record, queryBound);
                return MeasureTextByTextBlock(rectangle, displayText, column, record, queryBound, textBlock);
            }
        }

#if WPF
        /// <summary>
        /// Gets the formatted text for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the formatted text.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the formatted text.
        /// </param>
        /// <param name="displaytext">
        /// The corresponding display text to get formatted text. 
        /// </param>
        /// <returns>
        /// Returns the formatted text for the specified column.
        /// </returns>
        protected virtual FormattedText GetFormattedText(GridColumn column, object record, string displaytext)
        {
            return base.GetFormattedText(column, record, displaytext);
        }
#endif
        /// <summary>
        /// Gets the TextBlock to measure the text when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the TextBlock.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the TextBlock.
        /// </param>
        /// <param name="queryBounds">
        /// Indicates whether the text is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the TextBlock for the specified column and record.
        /// </returns>
        protected virtual TextBlock GetTextBlock(GridColumn column, object record, GridQueryBounds queryBounds)
        {
            return base.GetTextBlock(column, record, queryBounds);
        }

#if WPF

        /// <summary>
        /// Gets the content control to measure the template when column sizer is SizeToCells.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the content control.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the content control.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Controls.ContentControl"/> for the specified column and record.
        /// </returns>
        protected virtual ContentControl GetControl(GridColumn column, object record)
        {
            return base.GetControl(column, record);
        }
#else
        /// <summary>
        /// Gets the content presenter to measure the template when column sizer is SizeToCells.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the content presenter.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the content presenter.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Controls.ContentPresenter"/> for the specified column and record.
        /// </returns>
        protected virtual ContentPresenter GetControl(GridColumn column, object record)
        {
            return base.GetControl(column, record);
        }


#endif
        /// <summary>
        /// Measures the size of the template when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>        
        /// <param name="rect">
        /// The corresponding display rectangle to measure the template.
        /// </param>
        /// <param name="record">
        /// The corresponding record to measure the template.
        /// </param>
        /// <param name="column">
        /// The corresponding column to measure the template.
        /// </param>        
        /// <param name="bounds">
        /// Indicates whether the template is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of template.
        /// </returns>       
        protected virtual Size MeasureTemplate(Size rect, object record, GridColumn column, GridQueryBounds bounds)
        {
            var ctrl = GetControl(column, record);
            return base.MeasureTemplate(rect, record, column, bounds, ctrl);
        }

        /// <summary>
        /// Measure HeaderText size
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="displayText"></param>     
        /// <param name="column"></param>
        /// <returns></returns>
        /// <remarks>calculation based on text displayed on header</remarks>
        private Size MeasureHeaderText(Size rectangle, string displayText, GridColumn column)
        {
            //Calculating column width based on header template.
            if (column.hasHeaderTemplate)
                return MeasureHeaderTemplate(column, rectangle, GridQueryBounds.Width);
            return MeasureText(rectangle, displayText, column, null, GridQueryBounds.Width);
        }

        /// <summary>
        /// Measures the size of the header template for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to measure the header template.
        /// </param>
        /// <param name="rect">
        /// The corresponding display rectangle to measure the template.
        /// </param>               
        /// <param name="bounds">
        /// Indicates whether the template is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of the header template for the specified column.
        /// </returns>
        protected virtual Size MeasureHeaderTemplate(GridColumn column, Size rect, GridQueryBounds bounds)
        {
            var ctrl = this.GetControl(column, null);
            return base.MeasureHeaderTemplate(column, rect, bounds, ctrl);
        }

        internal override double GetDefaultLineSize()
        {
            return DataGrid.VisualContainer.ColumnWidths.DefaultLineSize;
        }

        /// <summary>
        /// Sets the column width based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Star"/> column sizer.
        /// </summary>
        /// <param name="remainingColumnWidth">
        /// The available width to adjust the column based on Star column sizer.
        /// </param>
        /// <param name="remainingColumns">
        /// The collection columns that need to be set star width.
        /// </param>
        /// <remarks>
        /// Override this method to customize the <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Star"/> width calculation in SfDataGrid.
        /// </remarks>
        protected virtual void SetStarWidth(double remainingColumnWidth, IEnumerable<GridColumn> remainingColumns)
        {
            var removedColumn = new List<GridColumn>();
            var columns = remainingColumns.ToList();
            var totalRemainingStarValue = remainingColumnWidth;
            double removedWidth = 0;
            GridColumn fillColumn = null;
            bool isremoved;
            while (columns.Count > 0)
            {
                isremoved = false;
                removedWidth = 0;
                double starWidth = Math.Floor((totalRemainingStarValue / columns.Count));
                var column = columns.First();
                if (column == autoFillColumn && (column.ColumnSizer == GridLengthUnitType.AutoLastColumnFill || (DataGrid.ColumnSizer == GridLengthUnitType.AutoLastColumnFill && column.ColumnSizer != GridLengthUnitType.AutoWithLastColumnFill)))
                {
                    columns.Remove(column);
                    fillColumn = column;
                    continue;
                }
                double computedWidth = SetColumnWidth(column, starWidth);
                if (starWidth != computedWidth && starWidth > 0)
                {
                    isremoved = true;
                    columns.Remove(column);
                    foreach (var remColumn in removedColumn)
                    {
                        if (!columns.Contains(remColumn))
                        {
                            removedWidth += remColumn.ActualWidth;
                            columns.Add(remColumn);
                        }
                    }
                    removedColumn.Clear();
                    totalRemainingStarValue += removedWidth;
                }
                column.ActualWidth = computedWidth;
                totalRemainingStarValue = totalRemainingStarValue - computedWidth;
                if (!isremoved)
                {
                    columns.Remove(column);
                    if (!removedColumn.Contains(column))
                        removedColumn.Add(column);
                }
            }

            if (fillColumn != null)
            {
                double columnWidth = 0;              
                if (double.IsNaN(fillColumn.AutoWidth))
                {
                    columnWidth = GetColumnSizerWidth(fillColumn, GridLengthUnitType.Auto);
                    if (DataGrid.View != null)
                        SetAutoWidth(fillColumn, columnWidth);
                }
                else
                    columnWidth = fillColumn.AutoWidth;
                if (totalRemainingStarValue < columnWidth)
                    SetColumnWidth(fillColumn, columnWidth);
                else
                    SetColumnWidth(fillColumn, totalRemainingStarValue);               
            }
        }


        /// <summary>
        /// Gets the width for the specified column when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> .
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the auto width. 
        /// </param>
        /// <returns>
        /// Returns the auto width for the specified column.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the column is null.</exception>
        protected virtual double GetAutoWidth(GridColumn column)
        {
            if (column == null)
                throw new InvalidOperationException("Column Should not be null to perform this operation");
            return column.AutoWidth;
        }

        /// <summary>
        /// Sets the width for the specified column based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.None"/> column sizer.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set None width.
        /// </param>
        /// <param name="width">
        /// The width to set as None width.
        /// </param>
        /// <returns>
        /// Returns the None width for the specified column.
        /// </returns>
        protected virtual double SetNoneWidth(GridColumn column, double width)
        {
            return SetColumnWidth(column, width);
        }

        #endregion       

        #region Public Methods

        /// <summary>
        /// Resets the auto width for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to reset the auto width.
        /// </param>
        /// <remarks>
        /// The column width is reset to <b>double.Nan</b> ,if the column sizing is need to recalculate based on the data.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Thrown when the column is null.</exception>
        public void ResetAutoCalculation(GridColumn column)
        {
            if (column == null)
                throw new InvalidOperationException("Column Should not be null to perform this operation");
            column.AutoWidth = double.NaN;
        }

        /// <summary>
        /// Sets the width for the specified column based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> column sizer.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set Auto width.
        /// </param>
        /// <param name="width">
        /// The corresponding width set as Auto width.
        /// </param>
        /// <exception cref="System.InvalidOperationException">Thrown when the column is null.</exception>
        public virtual void SetAutoWidth(GridColumn column, double width)
        {
            if (column == null)
                throw new InvalidOperationException("Column Should not be null to perform this operation");
            column.AutoWidth = width;
        }


        /// <summary>
        /// Sets the width for the specified column based on <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.MinimumWidth"/> and <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.MaximumWidth"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set the width.
        /// </param>
        /// <param name="Width">
        /// The corresponding width to set.
        /// </param>
        /// <returns>
        /// Returns the corresponding width for the specified column.
        /// </returns>
        public virtual double SetColumnWidth(GridColumn column, double Width)
        {
            var colIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));
            var width = GetColumnWidth(column, Width);
            // While setting ExtendedWidth itself, we have set ActualWidth and ColumnWidths. So the following code is skipped
            if (double.IsNaN(column.ExtendedWidth))
            {
                column.ActualWidth = width;
                this.DataGrid.VisualContainer.ColumnWidths[colIndex] = column.ActualWidth;
            }
            return width;

        }

        /// <summary>
        /// Refreshes column widths when the ColumnSizer property value changes at SfDataGrid or GridColumn level.
        /// </summary>
        public void Refresh()
        {
            (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();
            Refresh(0);
            (this.DataGrid.VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();
        }

        /// <summary>
        /// Calculates the row height based on the cell contents of the row.
        /// </summary>
        /// <param name="RowIndex">
        /// The corresponding index of row to get row height.
        /// </param>
        /// <param name="options">
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridRowSizingOptions"/> to get row height.
        /// </param>
        /// <param name="rowHeight">
        /// Calculated height of the row.      
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the row height is calculated for record and header rows, otherwise <b>false</b>.
        /// </returns>
        /// <remarks>
        /// This can be used inside QueryRowHeight event handler to auto-size the rows based on content in on-demand without affecting performance.
        /// </remarks>
        public bool GetAutoRowHeight(int RowIndex, GridRowSizingOptions options, out double rowHeight)
        {
            columnAutoFitMode = options.AutoFitMode;
            //if recordindex is less than zero, then it is not a datarow. After this only need to check whether it is header row or not.
            //Because, for every data row, no need to check whether this is header row or not.
            var recordindex = this.DataGrid.ResolveToRecordIndex(RowIndex);
            if (recordindex < 0)
            {
                if (RowIndex <= this.DataGrid.GetHeaderIndex())
                    rowHeight = GetHeaderCellHeight(RowIndex, options);
                else
                    rowHeight = -1;
            }
            else
            {
                //GetCellHeight method renamed as GetRecordCellHeight.
                rowHeight = GetRecordCellHeight(recordindex, RowIndex, options);
            }
            if (rowHeight < 0)
                return false;
            return true;
        }

        /// <summary>
        /// Calculates the row height based on the cell contents of the row.
        /// </summary>
        /// <param name="record">
        /// Records of the corresponding row to get row height.
        /// </param>
        /// <param name="options">
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridRowSizingOptions"/> to get row height.
        /// </param>
        /// <param name="rowHeight">
        /// Calculated height of the row.      
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the row height is calculated for record and header rows, otherwise <b>false</b>.
        /// </returns>
        /// <remarks>
        /// This can be used inside QueryRowHeight event handler to auto-size the rows based on content for printing.
        /// </remarks>

        public bool GetAutoRowHeight(object record, GridRowSizingOptions options, out double rowHeight)
        {
            double resultHeight = -1;
            textLength = 0;
            prevColumnWidth = 0;
            rowHeight = 0.0;
            object data = (record is RecordEntry) ? (record as RecordEntry).Data : record;
            if (data != null)
                resultHeight = GetRowHeight(-1, data, options);
            else
                return false;
            rowHeight = Math.Round(resultHeight);
            if (resultHeight < 0)
                return false;
            return true;
        }

        //public void ResizeRowToFit(int RowIndex, GridRowSizingOptions options)
        //{            
        //    resizingOptions = options;
        //    ResizeRowToFit(RowIndex);
        //}
        #endregion      

        #region Dispose Method

        /// <summary>
        /// Disposes all the resources used by <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            base.Dispose(isDisposing);
            UnwireEvents();
            autoFillColumn = null;
            isdisposed = true;
#if UWP
            popup = null;
#endif
        }

        internal void UnwireEvents()
        {
            if (this.DataGrid != null && this.DataGrid.VisualContainer != null)
                this.DataGrid.VisualContainer.RowHeights.LineHiddenChanged -= RowHeights_LineHiddenChanged;
        }

        #endregion      

        #region UnBoundColumn Refreshing

        /// <summary>
        /// Refresh UnBoundColumn value
        /// </summary>
        /// <param name="column"></param>
        /// <remarks>invokes when Unbound Expression,Format changedS</remarks>
        private void OnUnBoundPropertiesChanged(GridUnBoundColumn column)
        {
            if (column.IsHidden)
                return;

#if WPF
            //WPF-20153 throws whether the expression used in unboundcolumn with Datatable collection.
            if ((!string.IsNullOrEmpty(column.Expression) || !string.IsNullOrEmpty(column.Format)) && column.DataGrid.View.IsLegacyDataTable)
                throw new NotSupportedException("Unbound column is not supported in DataTable.So use Expression column in the DataTable");
#endif

            this.DataGrid.RowGenerator.Items.ForEach(row => row.VisibleColumns.ForEach(col =>
            {
                if (col.GridColumn != null && col.GridColumn.IsUnbound && col.GridColumn.MappingName == column.MappingName)
                {
                    col.UpdateBinding(row.RowData);
                }
            }));
        }

        /// <summary>
        /// Initialize UnBoundColumnProperties delegate
        /// </summary>
        /// <param name="columns"></param>
        /// <remarks></remarks>
        internal void InitializeUnboundColumnPropertiesDelegate(Columns columns)
        {
            foreach (var column in columns)
            {
                if (column.IsUnbound)
                {
                    var col = (column as GridUnBoundColumn);
                    if (col.UnboundPropertiesChanged == null)
                        col.UnboundPropertiesChanged = OnUnBoundPropertiesChanged;
                }
            }
        }

        #endregion

        #region Events

        void RowHeights_LineHiddenChanged(object sender, HiddenRangeChangedEventArgs e)
        {
            //WPF-36237 When grid is loaded inside ScrollViewer, No changes need to be done in ColumnSizer.
            if (isInSuspend)
                return;
            if (this.DataGrid== null || !this.DataGrid.Columns.Any()) return;

            var lastColumnIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.Count - 1);
            if (this.DataGrid.VisualContainer.ColumnCount <= lastColumnIndex)
                return;

            // WPF-19724 - when DetailsViewDataRow is hidden, need to update ExtendedWidth
            // WPF-21372 - while clearing all records in child grid, need to update extended width            
            if (e.From == e.To && e.Hide && this.DataGrid.IsInDetailsViewIndex(e.From))
            {
                var record = this.DataGrid.DetailsViewManager.GetDetailsViewRecord(e.From);
                // Get DetailsView order from DetailsViewDataRow index
                int index = this.DataGrid.GetOrderForDetailsViewBasedOnIndex(e.From - 1);
                if (this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.View != null)
                    this.DataGrid.DetailsViewManager.UpdateExtendedWidth(record, index);
            }

            if (this.DataGrid is DetailsViewDataGrid)
                return;

            if (gridRowsFitInToView)
            {
                if (this.DataGrid.VisualContainer.ViewportHeight < this.DataGrid.VisualContainer.ExtentHeight)
                {
                    //WPF-32768 need to reset the ExtenedWidth of the column before Refresh the ColumnSizer
                    if (this.DataGrid.DetailsViewManager.HasDetailsView && !this.DataGrid.View.Records.Any(record => record.IsExpanded))
                        this.DataGrid.Columns.ForEach(column => column.ExtendedWidth = double.NaN);
                    Refresh();
                    gridRowsFitInToView = false;
                }
            }
            else
            {
                if (this.DataGrid.VisualContainer.ViewportHeight > this.DataGrid.VisualContainer.ExtentHeight)
                {
                    //WPF-32768 need to reset the ExtenedWidth of the column before Refresh the ColumnSizer
                    if (this.DataGrid.DetailsViewManager.HasDetailsView && !this.DataGrid.View.Records.Any(record => record.IsExpanded))
                        this.DataGrid.Columns.ForEach(column => column.ExtendedWidth = double.NaN);
                    Refresh();
                    gridRowsFitInToView = true;
                }
            }
        }

        #endregion

    }
}
