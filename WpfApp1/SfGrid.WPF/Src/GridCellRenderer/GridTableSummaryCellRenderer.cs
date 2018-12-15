#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
#if WinRT || UNIVERSAL
using Syncfusion.UI.Xaml.ScrollAxis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows.Controls;
using System.Windows.Media;

#endif


namespace Syncfusion.UI.Xaml.Grid.Cells
{
    public class GridTableSummaryCellRenderer :GridVirtualizingCellRenderer<TextBlock, GridTableSummaryCell>
    {
        public GridTableSummaryCellRenderer()
        {
            this.UseOnlyRendererElement = true;
            this.SupportsRenderOptimization = false;
            this.IsFocusible = false;
            this.IsEditable = false;
        }

#if WPF
        protected override void OnRenderCellBorder(DrawingContext dc, Rect cellRect,Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell)
        {
            var borderThickness = gridCell.BorderThickness;
            var borderBursh = gridCell.BorderBrush;
            switch (gridCell.GridCellRegion)
            {
                case "LastColumnCell":                    
                    var needClip = false;
                    if (dataColumnBase.ColumnElement.Clip != null)
                    {
                        if (clipGeometry != null)
                        {
                            clipGeometry.Freeze();
                            dc.PushClip(clipGeometry);
                            needClip = true;
                        }
                    }
                    cellRect.Y = cellRect.Y - (borderThickness.Bottom / 2);
                    cellRect.X = cellRect.X - (borderThickness.Right / 2);
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, false, true, false); // Renders Right border.                     
                    if (needClip)
                        dc.Pop();
                    break;
            }
        }

        protected override void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads  its EditElement as usual in UseLightweightTemplate true case also.
        }
#endif

        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridTableSummaryCell uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            if (dataContext is SummaryRecordEntry)
            {
                var record = dataContext as SummaryRecordEntry;
                if (record.SummaryRow.ShowSummaryInRow)
                    uiElement.Content = SummaryCreator.GetSummaryDisplayTextForRow(record, this.DataGrid.View);
                else
                    uiElement.Content = SummaryCreator.GetSummaryDisplayText(record, column.MappingName, this.DataGrid.View);
            }
        }

        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridTableSummaryCell element, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            if (dataContext is SummaryRecordEntry)
            {
                var record = dataContext as SummaryRecordEntry;
                if (record.SummaryRow.ShowSummaryInRow)
                    element.Content = SummaryCreator.GetSummaryDisplayTextForRow(record, this.DataGrid.View);
                else
                    element.Content = SummaryCreator.GetSummaryDisplayText(record, column.MappingName, this.DataGrid.View);
            }
        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var summaryCell = cell as GridTableSummaryCell;
            
            if (summaryCell != null && DataGrid != null)
            {
                bool hasTableSummaryCellStyleSelector = DataGrid.hasTableSummaryCellStyleSelector;
                bool hasTableSummaryCellStyle = DataGrid.hasTableSummaryCellStyle;                

                if (!hasTableSummaryCellStyleSelector && !hasTableSummaryCellStyle)
                    return;
                Style newStyle = null;

                if (hasTableSummaryCellStyleSelector && hasTableSummaryCellStyle)
                {
                    newStyle = DataGrid.TableSummaryCellStyleSelector.SelectStyle(record, cell);
                    newStyle = newStyle ?? DataGrid.TableSummaryCellStyle;                 
                }
                else if (hasTableSummaryCellStyleSelector)
                {
                    newStyle = DataGrid.TableSummaryCellStyleSelector.SelectStyle(record, cell);
                }
                else if (hasTableSummaryCellStyle)
                {
                    newStyle = DataGrid.TableSummaryCellStyle;
                }
                // WPF-35961 - When the TableSummaryCellStyle is explicitly set null value,then we need to clear the TableSummaryStyle for the datagrid.
                if (newStyle != null)
                    summaryCell.Style = newStyle;
                else
                    summaryCell.ClearValue(FrameworkElement.StyleProperty);
            }
        }
    }
}
