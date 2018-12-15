#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI.Text;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Represents a class that provides the implementation to calculate column widths based on different column sizer options for SfTreeGrid(<see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer"/>).
    /// </summary>
    public class TreeGridColumnSizer : ColumnSizerBase<SfTreeGrid>, IDisposable
    {
        /// <summary>
        /// Gets the reference to the SfTreeGrid control.
        /// </summary>
        protected internal SfTreeGrid TreeGrid
        {
            get { return this.GridBase; }
        }
        public TreeGridColumnSizer(SfTreeGrid _treeGrid)
        {
            GridBase = _treeGrid;
        }

        public TreeGridColumnSizer()
        {
        }

#if UWP
        // Along with this width, 2 is added to show the full ExpanderCell in View
        private double expanderWidth = 18;
#else
        // Along with this width, 2 is added to show the full ExpanderCell in View
        private double expanderWidth = 14;
#endif

        /// <summary>
        /// Gets or sets the width of expander for column width calculation
        /// </summary>
        /// <value>
        /// The width of the expander. The default value is 18.
        /// </value>
        public double ExpanderWidth
        {
            get { return expanderWidth; }
            set { expanderWidth = value; }
        }

        // Along with this width, 1 is added to show the full CheckBox in View.
#if UWP        
        private double checkBoxWidth = 24;
#else
        private double checkBoxWidth = 19;
#endif
        /// <summary>
        /// Gets or sets the width of node check box for column width calculation when <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ShowCheckBox"/> is true.
        /// </summary> 
        /// <value>
        /// The width of the CheckBox. The default value is 24.
        /// </value>
        public double CheckBoxWidth
        {
            get { return checkBoxWidth; }
            set { checkBoxWidth = value; }
        }

        private void SetWidth(double viewPortWidth)
        {
            double totalColumnSize = 0d;
            var calculatedColumns = new List<TreeGridColumn>();
            autoFillColumn = GetColumnToFill();
            SetWidthBasedOnColumnSettings(viewPortWidth, ref totalColumnSize, ref calculatedColumns);
            SetWidthBasedonGridColumnSizer(totalColumnSize, calculatedColumns, viewPortWidth);
            autoFillColumn = null;
        }

        private TreeGridColumn autoFillColumn;

        private void SetWidthBasedOnColumnSettings(double viewPortWidth, ref double totalColumnSize, ref List<TreeGridColumn> calculatedColumns)
        {
            var expanderColumnWidth = TreeGrid.View != null ? ((TreeGrid.View.Nodes.MaxLevel + 1) * ExpanderWidth + 2) : 0;
            if (TreeGrid.ShowCheckBox)
                expanderColumnWidth += TreeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;

            var hiddenColumns = this.TreeGrid.Columns.Where(col => col.IsHidden);
            foreach (var column in hiddenColumns)
            {
                var index = this.TreeGrid.Columns.IndexOf(column);
                var scrollColumnIndex = this.TreeGrid.ResolveToScrollColumnIndex(index);
                if (!this.TreeGrid.ColumnResizingController.IsExpanderColumn(column))
                {
                    this.TreeGrid.TreeGridPanel.ColumnWidths.SetHidden(scrollColumnIndex, scrollColumnIndex, true);
                    column.ActualWidth = 0;
                    calculatedColumns.Add(column);
                }
                else
                {
                    this.TreeGrid.TreeGridPanel.ColumnWidths.SetHidden(scrollColumnIndex, scrollColumnIndex, false);
                    column.ActualWidth = expanderColumnWidth;
                    this.TreeGrid.TreeGridPanel.ColumnWidths[scrollColumnIndex] = column.ActualWidth;
                    totalColumnSize += column.ActualWidth;
                    calculatedColumns.Add(column);
                }
            }

            var widthColumns = this.TreeGrid.Columns.Where(col => !double.IsNaN(col.Width));
            foreach (var column in widthColumns)
            {
                if (calculatedColumns.Contains(column))
                    continue;
                totalColumnSize += ChangeColumnWidth(column, column.Width);
                calculatedColumns.Add(column);
            }

            var noneColumns = this.TreeGrid.Columns.Where(col => col.ReadLocalValue(TreeGridColumn.ColumnSizerProperty) != DependencyProperty.UnsetValue && col.ColumnSizer == TreeColumnSizer.None);
            foreach (var column in noneColumns)
            {
                if (calculatedColumns.Contains(column))
                    continue;
                totalColumnSize += GetColumnSizerWidth(column, TreeColumnSizer.None);
                calculatedColumns.Add(column);
            }

            // Set width based on SizeToHeader
            var sizeToHeaderColumns = this.TreeGrid.Columns.Except(calculatedColumns).Where(column => column.ColumnSizer == TreeColumnSizer.SizeToHeader);
            foreach (TreeGridColumn column in sizeToHeaderColumns)
            {
                totalColumnSize += GetColumnSizerWidth(column, TreeColumnSizer.SizeToHeader);
                calculatedColumns.Add(column);
            }

            // Set width based on SizeToCells
            var sizeToCellsColumns = this.TreeGrid.Columns.Except(calculatedColumns).Where(column => column.ColumnSizer == TreeColumnSizer.SizeToCells);
            foreach (TreeGridColumn column in sizeToCellsColumns)
            {
                totalColumnSize += GetColumnSizerWidth(column, TreeColumnSizer.SizeToCells);
                calculatedColumns.Add(column);
            }

            // Set width based on Auto and AutoWithLastColumnFill
            var lastColumn = this.TreeGrid.Columns.LastOrDefault(x => !x.IsHidden);
            var autoColumns = this.TreeGrid.Columns.Except(calculatedColumns).Where(column => column.ColumnSizer == TreeColumnSizer.Auto);

            var autoWithLastColumnFills = this.TreeGrid.Columns.Except(calculatedColumns).Where(col => (col.ColumnSizer == TreeColumnSizer.AutoFillColumn || col.ColumnSizer == TreeColumnSizer.FillColumn) && !IsFillOrAutoFillColumn(col)).ToList();
            autoColumns = autoColumns.Union(autoWithLastColumnFills);
            foreach (var column in autoColumns)
            {
                if (double.IsNaN(column.AutoWidth))
                {
                    var columnwidth = GetColumnSizerWidth(column, TreeColumnSizer.Auto);
                    totalColumnSize += columnwidth;
                }
                else
                    totalColumnSize += ChangeColumnWidth(column, column.AutoWidth);
                calculatedColumns.Add(column);
            }

            var expanderColumn = GetExpanderColumn();

            if (this.TreeGrid.showRowHeader)
                totalColumnSize += this.TreeGrid.RowHeaderWidth;
            if (!calculatedColumns.Contains(expanderColumn))
                totalColumnSize += expanderColumnWidth;
        }

        /// <summary>
        /// Gets the column to fill the remaining view port size based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.AutoFillColumn"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.FillColumn"/> column sizer.
        /// </summary>
        /// <returns>the column to fill.</returns>        
        protected virtual TreeGridColumn GetColumnToFill()
        {
            var column = TreeGrid.Columns.LastOrDefault(c => !c.IsHidden && double.IsNaN(c.Width) && (c.ColumnSizer == TreeColumnSizer.AutoFillColumn || c.ColumnSizer == TreeColumnSizer.FillColumn));
            if (column != null)
                return column;
            else
            {
                if (TreeGrid.ColumnSizer == TreeColumnSizer.AutoFillColumn || TreeGrid.ColumnSizer == TreeColumnSizer.FillColumn)
                {
                    var lastColumn = TreeGrid.Columns.LastOrDefault(c => !c.IsHidden && double.IsNaN(c.Width));
                    if (lastColumn == null)
                        return null;
                    if (lastColumn.ReadLocalValue(TreeGridColumn.ColumnSizerProperty) == DependencyProperty.UnsetValue)
                        return lastColumn;
                }
            }
            return null;
        }

        /// <summary>
        /// Change the width if column is expander column by considering expander and checkbox and set width.
        /// </summary>
        /// <param name="column">specified column.</param>
        /// <param name="width">width.</param>
        /// <returns>changed width.</returns>
        private double ChangeColumnWidth(TreeGridColumn column, double width)
        {
            if (column == GetExpanderColumn())
            {
                return SetColumnWidth(column, CalculateExpanderColumnWidth(column, width));
            }
            else
                return SetColumnWidth(column, width);
        }

        /// <summary>
        /// Checks whether specified column is fill column or auto fill column.
        /// </summary>
        /// <param name="column">the column which needs to be checked.</param>
        private bool IsFillOrAutoFillColumn(TreeGridColumn column)
        {
            if (column == autoFillColumn)
                return true;
            return false;
        }

        /// <summary>
        /// Get column width based on column sizer except star column sizer
        /// </summary>
        /// <param name="column">the specified column.</param>
        /// <param name="columnSizer">column sizer value.</param>
        /// <returns>width.</returns>
        private double GetColumnSizerWidth(TreeGridColumn column, TreeColumnSizer columnSizer)
        {
            double width = 0;
            switch (columnSizer)
            {
                case TreeColumnSizer.None:
                    if (column != GetExpanderColumn())
                        width = SetNoneWidth(column, TreeGrid.TreeGridPanel.ColumnWidths.DefaultLineSize);
                    else
                        width = SetNoneWidth(column, CalculateExpanderColumnWidth(column, TreeGrid.TreeGridPanel.ColumnWidths.DefaultLineSize));
                    return width;
                case TreeColumnSizer.SizeToCells:
                    if (double.IsNaN(column.AutoWidth))
                    {
                        width = CalculateCellWidth(column);
                        if (TreeGrid.View != null)
                            SetAutoWidth(column, width);
                        width = ChangeColumnWidth(column, width);
                    }
                    else
                        width = ChangeColumnWidth(column, column.AutoWidth);
                    return width;
                case TreeColumnSizer.SizeToHeader:
                    width = CalculateHeaderWidth(column);
                    if (TreeGrid.View != null)
                        SetAutoWidth(column, width);
                    width = ChangeColumnWidth(column, width);
                    return width;
                case TreeColumnSizer.Auto:
                    if (double.IsNaN(column.AutoWidth))
                    {
                        width = CalculateAutoFitWidth(column);
                        if (TreeGrid.View != null)
                            SetAutoWidth(column, width);
                        width = ChangeColumnWidth(column, width);
                    }
                    else
                        width = ChangeColumnWidth(column, column.AutoWidth);
                    return width;
            }
            return 0;
        }

        internal override DataContextHelper GetDataContextHelper(GridColumnBase column, object record)
        {
            var dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(r => r.RowData == record);
            var dataContextHelper = new TreeGridDataContextHelper { Record = record, DataRow = dataRow };
            dataContextHelper.SetValueBinding(column.ValueBinding, record);
            return dataContextHelper;
        }

        /// <summary>
        /// Calculates the width for the column to fit the content when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to calculate the width when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
        /// </param>       
        /// <returns>
        /// Returns the column width based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/> column sizer.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the calculation of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/> column sizer.
        /// </remarks>
        protected internal virtual double CalculateAutoFitWidth(TreeGridColumn column)
        {
            double headerWidth = CalculateHeaderWidth(column);
            double cellWidth = CalculateCellWidth(column);
            double width;
            if (cellWidth > headerWidth)
                width = cellWidth;
            else
                width = headerWidth;
            return width;
        }


        /// <summary>
        /// Calculates the width for the column based on cell value when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to calculate the cell width based on cell value.  
        /// </param>       
        /// <returns>
        /// Returns the corresponding width for the specified column.
        /// </returns>
        /// <remarks>
        /// This method is invoked when the column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/> and you can customize column width calculation by overriding this method.
        /// </remarks>
        protected virtual double CalculateCellWidth(TreeGridColumn column)
        {
            if (this.TreeGrid.View == null)
                return column.ActualWidth;
            return this.GetCellWidth(column);
        }

        /// <summary>
        /// Calculate width for Cells based on given column.
        /// </summary>
        /// <param name="column">the specified column.</param>        
        /// <returns>the width calculated by considering the cell values.</returns>       
        private double GetCellWidth(TreeGridColumn column)
        {
            double resultWidth = 0;
            var colIndex = this.TreeGrid.Columns.IndexOf(column);
            var nodeCount = this.TreeGrid.View.Nodes.Count;
            if (nodeCount == 0)
                return double.NaN;

            int scrollColumnIndex = this.TreeGrid.ResolveToScrollColumnIndex(colIndex);
            double colWidth = this.TreeGrid.TreeGridPanel.ColumnWidths[scrollColumnIndex];
            double rowHeight = this.TreeGrid.TreeGridPanel.RowHeights.DefaultLineSize;
            var isInDefaultMode = AutoFitMode == AutoFitMode.Default || (column.IsTemplate && TreeGrid.hasCellTemplateSelector) || column.hasCellTemplate || column.hasCellTemplateSelector;
            var clientSize = new Size(colWidth, rowHeight);
            TreeNode treeNode = null;
            textLength = 0;
            int stringLenth = 0;
            prevColumnWidth = 0;
            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                var node = this.TreeGrid.View.Nodes[nodeIndex];
                if (node == null)
                    continue;

                if (isInDefaultMode)
                {
                    var textsize = this.GetCellSize(clientSize, column, node, GridQueryBounds.Width);
                    if (textsize.IsEmpty)
                        continue;

                    if (resultWidth < textsize.Width)
                        resultWidth = textsize.Width;
                }
                else
                {
                    var text = this.GetDisplayText(column, node.Item);
                    if (text.Length >= stringLenth)
                    {
                        stringLenth = text.Length;
                        treeNode = node;
                    }
                }
            }
            if (!isInDefaultMode)
            {
                var textsize = this.GetCellSize(clientSize, column, treeNode, GridQueryBounds.Width);
                resultWidth = textsize.Width;
            }
            textLength = 0;
            prevColumnWidth = 0;
            //UWP - 2044 - While calculating AutoWidth, the point values is reduced. Hence the below decimal point 2 is added.
            return Math.Round(resultWidth, 2);
        }


        /// <summary>
        /// Gets the size of the cell to calculate the width of the specified column when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
        /// </summary>
        /// <param name="rect">
        /// The corresponding display rectangle of the cell to measure cell size.
        /// </param>
        /// <param name="column">
        /// The corresponding column to measure its cell size.
        /// </param>
        /// <param name="node">
        /// The corresponding node to measure the text size in cell.
        /// </param>
        /// <param name="bounds">
        /// Indicates whether the cell size is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of the cell.
        /// </returns>
        protected virtual Size GetCellSize(Size rect, TreeGridColumn column, TreeNode node, GridQueryBounds bounds)
        {
            Size textSize = Size.Empty;

            if ((column.IsTemplate && TreeGrid.hasCellTemplateSelector) || column.hasCellTemplate || column.hasCellTemplateSelector)
            {
                textSize = this.MeasureTemplate(rect, node.Item, column, bounds);
            }
            else
            {
                var text = this.GetDisplayText(column, node.Item);
                //WPF-19471 Need to compare column width if 1st condition(based on text length) is failed. 
                //because some case like if column width is small compare than previous  column but content length also small compare than previous column,it skips the height calculation 
                //so row is clipped  while auto row height is applied.
                if (text.Length >= textLength || prevColumnWidth >= column.ActualWidth)
                {
                    textSize = MeasureText(rect, text, column, node.Item, bounds);
                    textLength = text.Length;
                    prevColumnWidth = column.ActualWidth;
                }
            }
            return textSize;
        }

        /// <summary>
        /// Calculates the width of the column based on header text when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToHeader"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to calculate its header width.
        /// </param>      
        /// <returns>
        /// Returns the width of the specified column.
        /// </returns>
        /// <remarks>
        /// This method is invoked when the column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToHeader"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/> and you can customize column width calculation by overriding this method.
        /// </remarks>
        protected virtual double CalculateHeaderWidth(TreeGridColumn column)
        {
            double width = this.GetHeaderCellWidth(column);

            bool hasSorting = false;
            if (column.AllowSorting)
            {
                width += SortIconWidth;
                hasSorting = true;
            }

            if (this.TreeGrid.RowGenerator.Items.Any() && !hasSorting)
            {
                TreeDataRowBase dataRow = this.TreeGrid.RowGenerator.Items[this.TreeGrid.GetHeaderIndex()];
                if (dataRow != null)
                {
                    if (dataRow.VisibleColumns.Any(col => col.TreeGridColumn != null && col.TreeGridColumn.MappingName == column.MappingName))
                    {
                        TreeDataColumnBase dataColumn = dataRow.VisibleColumns.FirstOrDefault(col => col.TreeGridColumn != null && col.TreeGridColumn.MappingName == column.MappingName);
                        if (dataColumn != null)
                        {
                            var headerCell = dataColumn.ColumnElement as TreeGridHeaderCell;
                            if (headerCell != null)
                            {
                                if (headerCell.SortDirection != null)
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
            return Math.Round(width);
        }


        /// <summary>
        /// Gets the display text of cell for the specified column and data.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the display text of cell.
        /// </param>
        /// <param name="data">
        /// The corresponding record to get the display text.
        /// </param>
        /// <returns>
        /// Returns the display text of the corresponding column.
        /// </returns>
        protected virtual string GetDisplayText(TreeGridColumn column, object data)
        {
            var value = this.TreeGrid.View.GetPropertyAccessProvider().GetFormattedValue(data, column.MappingName);
            if (value != null)
                return value.ToString();
            return string.Empty;
        }

        /// <summary>
        /// Calculate width for header based on given column
        /// </summary>
        /// <param name="column">the specified column.</param>     
        /// <returns>header text width.</returns>      
        private double GetHeaderCellWidth(TreeGridColumn column)
        {
            var colIndex = this.TreeGrid.Columns.IndexOf(column);
            int scrollColumnIndex = this.TreeGrid.ResolveToScrollColumnIndex(colIndex);
            double colWidth = this.TreeGrid.TreeGridPanel.ColumnWidths[scrollColumnIndex];
            string text;
            Size textSize;
            bool isInDefaultMode = column.hasHeaderTemplate || AutoFitMode == AutoFitMode.Default;
            double rowHeight = this.TreeGrid.TreeGridPanel.RowHeights[0];
            var clientSize = new Size(colWidth, rowHeight);
            text = column.HeaderText ?? column.MappingName;
            textSize = MeasureHeaderText(clientSize, text, column);
            var width = textSize.Width;
            return width;
        }

        /// <summary>
        /// Measures the size of the template when the column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
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
        protected virtual Size MeasureTemplate(Size rect, object record, TreeGridColumn column, GridQueryBounds bounds)
        {
            var ctrl = GetControl(column, record);
            return base.MeasureTemplate(rect, record, column, bounds, ctrl);
        }

        /// <summary>
        /// Measures the size of the header template for the specified column when the column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToHeader"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
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
        protected virtual Size MeasureHeaderTemplate(TreeGridColumn column, Size rect, GridQueryBounds bounds)
        {
            var ctrl = GetControl(column, null);
            return base.MeasureHeaderTemplate(column, rect, bounds, ctrl);
        }


        /// <summary>
        /// Measure HeaderText size
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="displayText"></param>     
        /// <param name="column"></param>
        /// <returns></returns>
        /// <remarks>calculation based on text displayed on header</remarks>
        private Size MeasureHeaderText(Size rectangle, string displayText, TreeGridColumn column)
        {
            //Calculating column width based on header template.
            if (column.hasHeaderTemplate)
                return MeasureHeaderTemplate(column, rectangle, GridQueryBounds.Width);
            return MeasureText(rectangle, displayText, column, null, GridQueryBounds.Width);
        }

        /// <summary>
        /// Measures the text of the specified column when the column sizing is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
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
        protected virtual Size MeasureText(Size rectangle, string displayText, TreeGridColumn column, object record, GridQueryBounds queryBounds)
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
        protected virtual FormattedText GetFormattedText(TreeGridColumn column, object record, string displaytext)
        {
            return base.GetFormattedText(column, record, displaytext);
        }
#endif

        /// <summary>
        /// Gets the TextBlock to measure the text when the column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
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
        protected virtual TextBlock GetTextBlock(TreeGridColumn column, object record, GridQueryBounds queryBounds)
        {
            return base.GetTextBlock(column, record, queryBounds);
        }

        internal override double GetDefaultLineSize()
        {
            return TreeGrid.TreeGridPanel.ColumnWidths.DefaultLineSize;
        }

#if WPF

        /// <summary>
        /// Gets the content control to measure the template when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
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
        protected virtual ContentControl GetControl(TreeGridColumn column, object record)
        {
            return base.GetControl(column, record);
        }
#else

        /// <summary>
        /// Gets the content presenter to measure the template when column sizer is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Auto"/>.
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
        protected virtual ContentPresenter GetControl(TreeGridColumn column, object record)
        {
            return base.GetControl(column, record);
        }

#endif
        private void SetWidthBasedonGridColumnSizer(double totalColumnSize, List<TreeGridColumn> calculatedColumns, double viewPortWidth)
        {
            double expanderColumnWidth = TreeGrid.View != null ? ((TreeGrid.View.Nodes.MaxLevel + 1) * ExpanderWidth + 2) : 0;
            if (TreeGrid.ShowCheckBox)
                expanderColumnWidth += CheckBoxWidth + 1;
            var autoColumnFillCount = this.TreeGrid.Columns.Count(c => c.ColumnSizer == TreeColumnSizer.AutoFillColumn);
            foreach (var column in this.TreeGrid.Columns)
            {
                if (calculatedColumns.Contains(column))
                    continue;

                if (column.ColumnSizer == TreeColumnSizer.Star || IsFillOrAutoFillColumn(column))
                    continue;

                switch (this.TreeGrid.ColumnSizer)
                {
                    case TreeColumnSizer.None:
                        goto case TreeColumnSizer.SizeToCells;
                    case TreeColumnSizer.SizeToHeader:
                        goto case TreeColumnSizer.SizeToCells;
                    case TreeColumnSizer.SizeToCells:
                        totalColumnSize += GetColumnSizerWidth(column, this.TreeGrid.ColumnSizer);
                        if (column == GetExpanderColumn())
                        {
                            totalColumnSize -= expanderColumnWidth;
                        }

                        calculatedColumns.Add(column);
                        break;
                    case TreeColumnSizer.FillColumn:
                        goto case TreeColumnSizer.Auto;
                    case TreeColumnSizer.AutoFillColumn:
                        goto case TreeColumnSizer.Auto;
                    case TreeColumnSizer.Auto:
                        totalColumnSize += GetColumnSizerWidth(column, TreeColumnSizer.Auto);
                        if (column == GetExpanderColumn())
                        {
                            totalColumnSize -= expanderColumnWidth;
                        }
                        calculatedColumns.Add(column);
                        break;
                }
            }

            var remainingColumns = this.TreeGrid.Columns.Except(calculatedColumns);
            if (viewPortWidth == 0)
            {
                if (this.TreeGrid.TreeGridPanel.ScrollOwner != null && this.TreeGrid.TreeGridPanel.ScrollOwner.ActualWidth != 0)
                    viewPortWidth = this.TreeGrid.TreeGridPanel.ScrollOwner.ActualWidth;
                else if (this.TreeGrid.TreeGridPanel.ScrollableOwner != null && this.TreeGrid.TreeGridPanel.ScrollableOwner.ActualWidth != 0)
                    viewPortWidth = this.TreeGrid.TreeGridPanel.ScrollableOwner.ActualWidth;
              
#if WPF
                if (this.TreeGrid.TreeGridPanel.ScrollRows.RenderSize < this.TreeGrid.TreeGridPanel.ExtentHeight)
                {                    
                    var _vScrollBarWidth = SystemParameters.ScrollWidth;
                    if (this.TreeGrid.TreeGridPanel.ScrollOwner != null)
                    {
                        var vscroll = this.TreeGrid.TreeGridPanel.ScrollOwner.Template.FindName("PART_VerticalScrollBar", this.TreeGrid.TreeGridPanel.ScrollOwner) as ScrollBar;
                        _vScrollBarWidth = vscroll != null && vscroll.ActualWidth != 0 ? vscroll.ActualWidth : SystemParameters.ScrollWidth;
                    }
                    viewPortWidth -= _vScrollBarWidth;
                }
#endif
            }
            double remainingColumnWidths = viewPortWidth - totalColumnSize;

            if (remainingColumnWidths > 0 && (totalColumnSize != 0 || (totalColumnSize == 0 && remainingColumns.Count() == 1) || (this.TreeGrid.Columns.Any(col => col.ColumnSizer == TreeColumnSizer.Star) || this.TreeGrid.ColumnSizer == TreeColumnSizer.Star)))
                SetStarWidth(remainingColumnWidths, remainingColumns);
            else
                SetRemainingWidth(remainingColumns);
        }


        /// <summary>
        /// Sets the width for the specified column based on <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase.MinimumWidth"/> and <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase.MaximumWidth"/> property value changes.
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
        public virtual double SetColumnWidth(TreeGridColumn column, double Width)
        {
            var columnIndex = this.TreeGrid.Columns.IndexOf(column);
            var scrollColumnIndex = this.TreeGrid.ResolveToScrollColumnIndex(columnIndex);

            Width = GetColumnWidth(column, Width);
            column.ActualWidth = Width;
            this.TreeGrid.TreeGridPanel.ColumnWidths[scrollColumnIndex] = column.ActualWidth;
            return Width;
        }


        /// <summary>
        /// Gets the width of column based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.MinimumWidth"/> and <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.MaximumWidth"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the width.
        /// </param>
        /// <param name="Width">
        /// The corresponding column width.
        /// </param>
        /// <returns>the calculated width.</returns>
        internal double GetColumnWidth(TreeGridColumn column, double Width)
        {
            var colIndex = this.TreeGrid.ResolveToScrollColumnIndex(this.TreeGrid.Columns.IndexOf(column));

            double width = this.TreeGrid.TreeGridPanel.ColumnWidths[colIndex];

            if (column != GetExpanderColumn())
            {
                var resultWidth = CheckWidthConstraints(column, Width, width);
                return resultWidth;
            }

            if (!double.IsNaN(Width))
                width = Width;

            return width;
        }

        internal void InitializeColumnWPropertyChangedDelegate()
        {
            foreach (var column in this.TreeGrid.Columns)
            {
                if (column.ColumnPropertyChanged == null)
                    column.ColumnPropertyChanged = OnTreeColumnPropertyChanged;
            }
        }

        internal override void ResetAutoCalculations()
        {
            foreach (var column in TreeGrid.Columns)
                column.AutoWidth = double.NaN;
        }

        internal void InitialRefresh(double availableWidth, bool needToInvalidateMeasure = true)
        {
            (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).SuspendUpdates();
            Refresh(availableWidth, needToInvalidateMeasure);
            (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).ResumeUpdates();
            if (!(this.TreeGrid.AllowResizingColumns && this.TreeGrid.AllowResizingHiddenColumns))
                return;
            this.TreeGrid.Columns.ForEach(col =>
            {
                this.TreeGrid.ColumnResizingController.ProcessResizeStateManager(col);
            });
        }

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column on which the property value changes.
        /// </param>
        /// <param name="property">
        /// The name of property that value has been changed.
        /// </param>
        protected virtual void OnTreeColumnPropertyChanged(TreeGridColumn column, string property)
        {
            if (isInSuspend)
                return;
            switch (property)
            {
                case "Width":
                case "FontWeight":
                case "FontStretch":
                case "FontFamily":
                case "Margin":
                case "FontSize":
                case "MaximumWidth":
                case "ColumnSizer":
                case "MinimumWidth":
                    this.ResetAutoCalculation(column);
                    (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).SuspendUpdates();
                    SetWidth(0);
                    (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).ResumeUpdates();
                    this.TreeGrid.TreeGridPanel.NeedToRefreshColumn = true;
                    this.TreeGrid.TreeGridPanel.InvalidateMeasureInfo();
                    break;
                case "IsHidden":
                    if (!column.IsHidden)
                    {
                        var index = this.TreeGrid.ResolveToScrollColumnIndex(this.TreeGrid.Columns.IndexOf(column));
                        this.TreeGrid.TreeGridPanel.ColumnWidths.SetHidden(index, index, false);
                    }
                    (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).SuspendUpdates();
                    this.SetWidth(0);
                    (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).ResumeUpdates();
                    if (this.TreeGrid.AllowResizingColumns && this.TreeGrid.AllowResizingHiddenColumns &&
                        !this.TreeGrid.ColumnResizingController.IsExpanderColumn(column))
                        this.TreeGrid.ColumnResizingController.ProcessResizeStateManager(column);
                    this.TreeGrid.TreeGridPanel.NeedToRefreshColumn = true;
                    this.TreeGrid.TreeGridPanel.InvalidateMeasureInfo();
                    if (this.TreeGrid.ColumnDragDropController != null)
                        this.TreeGrid.ColumnDragDropController.ColumnHiddenChanged(column);
                    break;
                case "HeaderTemplate":
                case "CellStyle":
                case "CellStyleSelector":
                case "HeaderStyle":
                case "CellTemplate":
                case "CellTemplateSelector":
                    this.TreeGrid.OnColumnStyleChanged(column, property);
                    break;
            }
        }

        /// <summary>
        /// Sets the column width based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Star"/> column sizer.
        /// </summary>
        /// <param name="remainingColumnWidth">
        /// The available width to adjust the column based on Star column sizer.
        /// </param>
        /// <param name="remainingColumns">
        /// The collection columns that need to be set star width.
        /// </param>
        /// <remarks>
        /// Override this method to customize the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.Star"/> width calculation in SfTreeGrid.
        /// </remarks>
        protected virtual void SetStarWidth(double remainingColumnWidth, IEnumerable<TreeGridColumn> remainingColumns)
        {
            var removedColumn = new List<TreeGridColumn>();
            var columns = remainingColumns.ToList();
            TreeGridColumn fillColumn = null;
            double expanderColumnWidth = TreeGrid.View != null ? ((TreeGrid.View.Nodes.MaxLevel + 1) * ExpanderWidth + 2) : 0;
            if (TreeGrid.ShowCheckBox)
                expanderColumnWidth += CheckBoxWidth + 1;
            var totalRemainingStarValue = remainingColumnWidth;

            double removedWidth = 0;
            bool isremoved;
            while (columns.Count > 0)
            {
                isremoved = false;
                removedWidth = 0;
                double starWidth = Math.Floor((totalRemainingStarValue / columns.Count));
                var column = columns.First();
                if (column == autoFillColumn && (column.ColumnSizer == TreeColumnSizer.AutoFillColumn || (TreeGrid.ColumnSizer == TreeColumnSizer.AutoFillColumn && column.ColumnSizer != TreeColumnSizer.FillColumn)))
                {
                    columns.Remove(column);
                    fillColumn = column;
                    continue;
                }
                double columnWidth = 0;
                if (column == GetExpanderColumn())
                {
                    if (columns.Count == 1)
                        columnWidth = expanderColumnWidth + starWidth;
                    else
                        columnWidth = CalculateExpanderColumnWidth(column, starWidth);
                }
                else
                    columnWidth = starWidth;
                double computedWidth = SetColumnWidth(column, columnWidth);
                if (starWidth != computedWidth && starWidth > 0)
                {
                    isremoved = true;
                    columns.Remove(column);
                    foreach (var remColumn in removedColumn)
                    {
                        if (!columns.Contains(remColumn))
                        {
                            removedWidth += remColumn.ActualWidth;
                            if (remColumn == GetExpanderColumn())
                                removedWidth -= expanderColumnWidth;
                            columns.Add(remColumn);
                        }
                    }
                    removedColumn.Clear();
                    totalRemainingStarValue += removedWidth;
                }

                totalRemainingStarValue = totalRemainingStarValue - computedWidth;
                if (column == GetExpanderColumn())
                    totalRemainingStarValue = totalRemainingStarValue + expanderColumnWidth;
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
                var lastColumn = fillColumn;
                if (double.IsNaN(lastColumn.AutoWidth))
                {
                    columnWidth = CalculateAutoFitWidth(lastColumn);
                    if (TreeGrid.View != null)
                        SetAutoWidth(lastColumn, columnWidth);
                }
                else
                    columnWidth = lastColumn.AutoWidth;
                if (lastColumn == GetExpanderColumn())
                    totalRemainingStarValue += expanderColumnWidth;
                if (totalRemainingStarValue < columnWidth)
                    SetColumnWidth(fillColumn, columnWidth);
                else
                    SetColumnWidth(fillColumn, totalRemainingStarValue);
            }
        }
        /// <summary>
        /// Set Width for column when ColumnSizer is AutoWithLastColumnFill(when column is last column) or Star (if remaining column width is less than 0).
        /// </summary>
        /// <param name="remainingColumns">remainingColumns for which width need to be calculated.</param>        
        private void SetRemainingWidth(IEnumerable<TreeGridColumn> remainingColumns)
        {
            var remCols = new List<TreeGridColumn>();
            var lastcolumn = TreeGrid.Columns.LastOrDefault(c => !c.IsHidden);
            foreach (var column in remainingColumns)
            {
                if (IsFillOrAutoFillColumn(column))
                {
                    if (column.ColumnSizer == TreeColumnSizer.AutoFillColumn || (TreeGrid.ColumnSizer == TreeColumnSizer.AutoFillColumn && column.ColumnSizer != TreeColumnSizer.FillColumn))
                        GetColumnSizerWidth(column, TreeColumnSizer.Auto);
                    else
                        GetColumnSizerWidth(column, TreeColumnSizer.None);
                }
                else
                {
                    GetColumnSizerWidth(column, TreeColumnSizer.None);
                }
            }
        }


        /// <summary>
        /// Sets the width for the specified column based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.None"/> column sizer.
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
        protected virtual double SetNoneWidth(TreeGridColumn column, double width)
        {
            return SetColumnWidth(column, width);
        }

        /// <summary>
        /// Refreshes the column width when the ColumnSizer property value changes at SfTreeGrid or TreeGridColumn level.
        /// </summary>
        /// <param name="AvailableWidth">
        /// The available width to refresh the column width.
        /// </param>
        protected virtual void Refresh(double viewPortWidth)
        {
            this.Refresh(viewPortWidth, true);
        }

        /// <summary>
        /// Refreshes the column width when the ColumnSizer property value changes at SfTreeGrid or TreeGridColumn level.
        /// </summary>
        /// <param name="AvailableWidth">The available width to refresh the column width.</param>
        /// <param name="needToInvalidateMeasure">true if need to invalidate the measure of visual container </param>
        internal void Refresh(double viewPortWidth, bool needToInvalidateMeasure)
        {
            InitializeColumnWPropertyChangedDelegate();
            SetWidth(viewPortWidth);
            if (!needToInvalidateMeasure)
                return;
            if (this.TreeGrid.TreeGridPanel.ScrollOwner != null)
            {
                this.TreeGrid.TreeGridPanel.NeedToRefreshColumn = true;
                this.TreeGrid.TreeGridPanel.InvalidateMeasureInfo();
                this.TreeGrid.TreeGridPanel.ScrollOwner.InvalidateMeasure();
                this.TreeGrid.TreeGridPanel.UpdateScrollBars();
            }
            else if (this.TreeGrid.TreeGridPanel.ScrollableOwner != null)
            {
                this.TreeGrid.TreeGridPanel.NeedToRefreshColumn = true;
                this.TreeGrid.TreeGridPanel.InvalidateMeasureInfo();
                this.TreeGrid.TreeGridPanel.ScrollableOwner.InvalidateMeasure();
                this.TreeGrid.TreeGridPanel.UpdateScrollBars();
            }
        }

        /// <summary>
        /// Refreshes column widths when the ColumnSizer property value changes at SfTreeGrid or TreeGridColumn level.
        /// </summary>
        public void Refresh()
        {
            (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).SuspendUpdates();
            Refresh(0);
            (this.TreeGrid.TreeGridPanel.ColumnWidths as LineSizeCollection).ResumeUpdates();
        }


        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnSizer"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                base.Dispose(isDisposing);
                autoFillColumn = null;
            }
        }

        // maxLevel is maintained to restrict the column width refresh when node's maxLevel is not changed.
        private int maxLevel = 0;
        internal void SetMaxLevel(int level)
        {
            maxLevel = level;
        }

        /// <summary>
        /// Change expander column width based on node's level.
        /// </summary>
        protected internal virtual void ChangeExpanderColumnWidth()
        {
            if (!TreeGrid.AllowAutoSizingExpanderColumn || !TreeGrid.Columns.Any() || maxLevel == TreeGrid.View.Nodes.MaxLevel)
            {
                SetMaxLevel(TreeGrid.View.Nodes.MaxLevel);
                return;
            }
            SetMaxLevel(TreeGrid.View.Nodes.MaxLevel);
            if (TreeGrid.ColumnSizer == TreeColumnSizer.Star || TreeGrid.ColumnSizer == TreeColumnSizer.AutoFillColumn || TreeGrid.ColumnSizer == TreeColumnSizer.FillColumn || TreeGrid.Columns.Any(c => !c.IsHidden && double.IsNaN(c.Width) && (c.ColumnSizer == TreeColumnSizer.Star || c.ColumnSizer == TreeColumnSizer.FillColumn || c.ColumnSizer == TreeColumnSizer.AutoFillColumn)))
            {
                Refresh();
            }
            else
            {
                var column = GetExpanderColumn();
                var columnIndex = TreeGrid.Columns.IndexOf(column);
                var scrollColumnIndex = TreeGrid.ResolveToScrollColumnIndex(columnIndex);
                var actualWidth = 0.0;
                if (!double.IsNaN(column.Width))
                    actualWidth = column.Width;
                else if (!double.IsNaN(column.AutoWidth))
                    actualWidth = column.AutoWidth;
                else
                    actualWidth = TreeGrid.TreeGridPanel.ColumnWidths.DefaultLineSize;
                var width = CalculateExpanderColumnWidth(column, actualWidth);
                column.ActualWidth = width;
                TreeGrid.TreeGridPanel.ColumnWidths[scrollColumnIndex] = width;
            }
        }


        /// <summary>
        /// Calculate expander column width by considering expander cell, CheckBox, MinWidth and MaxWidth.
        /// </summary>
        /// <param name="column">the expander column.</param>
        /// <param name="width">the column width.</param>
        /// <returns>calculated width.</returns>
        protected virtual internal double CalculateExpanderColumnWidth(TreeGridColumn column, double width)
        {
            var calculatedWidth = width;
            var computedWidth = 0.0;
            var extendedWidth = 0.0;
            if (TreeGrid.View != null)
            {
                SetMaxLevel(TreeGrid.View.Nodes.MaxLevel);
                var expanderColumnWidth = (TreeGrid.View.Nodes.MaxLevel + 1) * ExpanderWidth + 2;
                if (TreeGrid.ShowCheckBox)
                    expanderColumnWidth += CheckBoxWidth + 1;
                if (TreeGrid.AllowAutoSizingExpanderColumn)
                    calculatedWidth += expanderColumnWidth;

                if (!double.IsNaN(column.MinimumWidth) || !double.IsNaN(column.MaximumWidth))
                {
                    if (!double.IsNaN(column.MinimumWidth))
                    {
                        if (column.MinimumWidth + (expanderColumnWidth) < calculatedWidth)
                        {
                            if (width > column.MaximumWidth + expanderColumnWidth)
                                computedWidth = column.MaximumWidth + expanderColumnWidth;
                            else
                                computedWidth = calculatedWidth;
                        }
                        else
                            computedWidth = column.MinimumWidth + expanderColumnWidth;
                    }
                    else if (!double.IsNaN(column.MaximumWidth))
                    {
                        if (column.MaximumWidth + expanderColumnWidth > calculatedWidth)
                            computedWidth = calculatedWidth;
                        else
                            computedWidth = column.MaximumWidth + expanderColumnWidth;
                    }
                    calculatedWidth = computedWidth;
                }
                var indentWidth = ExpanderWidth;
                if (TreeGrid.ShowCheckBox)
                    indentWidth += CheckBoxWidth + 1;
                if (column.IsHidden || width < indentWidth)
                {
                    calculatedWidth = this.ExpanderWidth + 2;
                    calculatedWidth += width;
                    extendedWidth = TreeGrid.View.Nodes.MaxLevel * ExpanderWidth;
                    if (TreeGrid.ShowCheckBox)
                        extendedWidth += CheckBoxWidth + 1;
                }
            }

            return calculatedWidth + extendedWidth;
        }

        /// <summary>
        /// Gets the column in which expander cell is loaded.
        /// </summary>
        /// <returns>the expander column.</returns>
        protected internal TreeGridColumn GetExpanderColumn()
        {
            var column = TreeGrid.Columns.FirstOrDefault(c => c.MappingName == TreeGrid.ExpanderColumn);
            if (column == null)
            {
                column = TreeGrid.Columns.FirstOrDefault();
            }
            return column;
        }
    }
}
