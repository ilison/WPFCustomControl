#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Data;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml.Media;
using System.Linq;
using Windows.UI.Text;
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
#endif


namespace Syncfusion.UI.Xaml.Grid.Cells
{
    /// <summary>
    /// Rederer for Covered cell which is used in Summary Rows and GroupCaption
    /// </summary>
    /// <remarks></remarks>
    [ClassReference(IsReviewed = false)]
    public class GridSummaryCellRenderer : GridVirtualizingCellRenderer<GridGroupSummaryCell,GridGroupSummaryCell>
    {
        public GridSummaryCellRenderer()
        {
            this.SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
            this.IsEditable = false;
            this.IsFocusible = false;
        }

#if WPF
        protected override void OnRenderCellBorder(DrawingContext dc, Rect cellRect,Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell)
        {
            var borderThickness = gridCell.BorderThickness;
            var borderBursh = gridCell.BorderBrush;            
            var needClip = false;
            if (clipGeometry != null)
            {
                clipGeometry.Freeze();
                dc.PushClip(clipGeometry);
                needClip = true;
            }     
            cellRect.Y = cellRect.Y - (borderThickness.Bottom / 2);
            cellRect.X = cellRect.X - (borderThickness.Right / 2);
            switch (gridCell.GridCellRegion)
            {
                case "NormalCell":
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, false, false, true);// Bottom border.
                    break;
                case "LastColumnCell":
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, false, true, true); // Renders Right, Bottom border.                                        
                    break;
            }
            if (needClip)
                dc.Pop();
        }

        protected override void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads its Element as usual in UseLightweightTemplate true case also.
        }

        protected override void OnRenderCurrentCell(DrawingContext dc, Rect cellRect,Geometry clipGeometry,DataColumnBase dataColumnBase, GridCell gridCell)
        {
            // Need to render background alone.
            if(dataColumnBase.IsSelectedColumn)
                base.OnRenderCurrentCell(dc, cellRect, clipGeometry, dataColumnBase, gridCell);
        }
#endif
        public override void OnInitializeEditElement(DataColumnBase dataColumn,GridGroupSummaryCell uiElement, object dataContext)
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

        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridGroupSummaryCell element, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            if (element.DataContext is SummaryRecordEntry)
            {
                var record = element.DataContext as SummaryRecordEntry;
                if (record.SummaryRow.ShowSummaryInRow)
                    element.Content = SummaryCreator.GetSummaryDisplayTextForRow(record, this.DataGrid.View);
                else
                    element.Content = SummaryCreator.GetSummaryDisplayText(record, column.MappingName, this.DataGrid.View);
            }
        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn,object record)
        {
            var cell = dataColumn.ColumnElement;
            var summaryCell = cell as GridGroupSummaryCell;
            
            if (summaryCell != null && DataGrid != null)
            {
                bool hasGroupSummaryCellStyleSelector = DataGrid.hasGroupSummaryCellStyleSelector;
                bool hasGroupSummaryCellStyle = DataGrid.hasGroupSummaryCellStyle;

                if (!hasGroupSummaryCellStyleSelector && !hasGroupSummaryCellStyle)
                    return;

                Style newStyle = null;
                if (hasGroupSummaryCellStyleSelector&& hasGroupSummaryCellStyle)
                {
                    newStyle = DataGrid.GroupSummaryCellStyleSelector.SelectStyle(record, cell);
                    newStyle = newStyle ?? DataGrid.GroupSummaryCellStyle;                 
                }
                else if (hasGroupSummaryCellStyleSelector)
                {
                    newStyle = DataGrid.GroupSummaryCellStyleSelector.SelectStyle(record, cell);
                }
                else if (hasGroupSummaryCellStyle)
                {
                    newStyle = DataGrid.GroupSummaryCellStyle;
                }
                // WPF-35961 - When the GroupSummaryCellStyle is explicitly set null value,then we need to clear the GroupSummaryStyle for the datagrid.
                if (newStyle != null)
                    summaryCell.Style = newStyle;
                else
                    summaryCell.ClearValue(FrameworkElement.StyleProperty);
            }            
        }
    }

    [ClassReference(IsReviewed = false)]
    public class GridCaptionSummaryCellRenderer : GridVirtualizingCellRenderer<GridCaptionSummaryCell, GridCaptionSummaryCell>
    {
        public GridCaptionSummaryCellRenderer()
        {
            this.SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
            this.IsEditable = false;
            this.IsFocusible = false;
        }
#if WPF
        protected override void OnRenderCellBorder(DrawingContext dc, Rect cellRect, Geometry clipGeometry,DataColumnBase dataColumnBase, GridCell gridCell)
        {
            if (gridCell.GridCellRegion.Equals("NormalCell"))
            {
                var borderBursh = gridCell.BorderBrush;
                var borderThickness = gridCell.BorderThickness;
                var needClip = false;
                if (clipGeometry != null)
                {
                    clipGeometry.Freeze();
                    dc.PushClip(clipGeometry);
                    needClip = true;
                }

                cellRect.Y = cellRect.Y - (borderThickness.Bottom / 2);
                cellRect.X = cellRect.X - (borderThickness.Right / 2);
                dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, false, false, true);// Renders Bottom border.                                          
                if (needClip)
                    dc.Pop();
            }
            else
                base.OnRenderCellBorder(dc, cellRect, clipGeometry, dataColumnBase, gridCell);
        }
        protected override void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads its Element as usual in UseLightweightTemplate true case also.
        }

        protected override void OnRenderCurrentCell(DrawingContext dc, Rect cellRect,Geometry clipGeometry,DataColumnBase dataColumnBase, GridCell gridCell)
        {
            // We wont draw current cell border for this CurrentCellElement.            
            if (dataColumnBase.IsSelectedColumn)
                base.OnRenderCurrentCell(dc, cellRect,clipGeometry, dataColumnBase, gridCell);
        }
#endif
        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridCaptionSummaryCell uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;

            if (dataContext is Group)
            {
                var groupRecord = dataContext as Group;
                if (this.DataGrid.CaptionSummaryRow == null)
                {
                    var groupedColumn = this.GetGroupedColumn(groupRecord);
                    
                    //WPF-20212 - If we add the column in GroupColumnDescription which is not placed in SfDataGrid column collections, skipped here to group by that column.                                                                 
                    ColumnGroupDescription groupDesc = null;
                    if (groupedColumn == null)
                    {
                        groupDesc = this.DataGrid.View.GroupDescriptions[groupRecord.Level - 1] as ColumnGroupDescription;
                        if(groupDesc == null)
                            return;
                    }
                    string stringFormat = this.DataGrid.GroupCaptionTextFormat ?? this.DataGrid.GroupCaptionConstant;
                    var headerText = groupedColumn != null ? groupedColumn.HeaderText : groupDesc.PropertyName;
                    uiElement.Content = this.DataGrid.View.TopLevelGroup.GetGroupCaptionText(groupRecord, stringFormat, headerText);
                }
                else if (this.DataGrid.CaptionSummaryRow.ShowSummaryInRow)
                {
                    uiElement.Content = SummaryCreator.GetSummaryDisplayTextForRow(groupRecord.SummaryDetails, this.DataGrid.View);
                }
                else
                    uiElement.Content = SummaryCreator.GetSummaryDisplayText(groupRecord.SummaryDetails, column.MappingName, this.DataGrid.View);
            }
        }

        public override void OnUpdateEditBinding(DataColumnBase dataColumn,GridCaptionSummaryCell element,object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            if (element.DataContext is Group && this.DataGrid.View.GroupDescriptions.Count > 0)
            {
                var groupRecord = element.DataContext as Group;
                var groupedColumn = this.GetGroupedColumn(groupRecord);

                ColumnGroupDescription groupDesc = null;
                //WPF-20212 - If we add the column in GroupColumnDescription which is not placed in SfDataGrid column collections, skipped here to group by that column.
                if (groupedColumn == null)
                {
                    groupDesc = this.DataGrid.View.GroupDescriptions[groupRecord.Level - 1] as ColumnGroupDescription;
                    if (groupDesc == null)
                        return;
                }
				// WPF-36158 When we defined GroupColumnDescription column name are not mentioned in SfGrid columns. So the header text are not applied to summary rows.
				var headerText = groupedColumn != null ? groupedColumn.HeaderText : groupDesc.PropertyName;
                if (this.DataGrid.CaptionSummaryRow == null)
                {
                    if (this.DataGrid.View.GroupDescriptions.Count < groupRecord.Level)
                        return;
                    var stringFormat = this.DataGrid.GroupCaptionTextFormat ?? this.DataGrid.GroupCaptionConstant;                    
                    element.Content = this.DataGrid.View.TopLevelGroup.GetGroupCaptionText(groupRecord, stringFormat, headerText);
                }
                else if (this.DataGrid.CaptionSummaryRow.ShowSummaryInRow)
                {
                    element.Content = SummaryCreator.GetSummaryDisplayTextForRow(groupRecord.SummaryDetails, this.DataGrid.View, headerText);
                }
                else
                    element.Content = SummaryCreator.GetSummaryDisplayText(groupRecord.SummaryDetails, column.MappingName, this.DataGrid.View);
            }           
        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var summaryCell = cell as GridCaptionSummaryCell;
            
            if (summaryCell != null && DataGrid != null)
            {
                bool hasCaptionSummaryCellStyleSelector = DataGrid.hasCaptionSummaryCellStyleSelector;
                bool hasCaptionSummaryCellStyle = DataGrid.hasCaptionSummaryCellStyle;
                
                if (!hasCaptionSummaryCellStyleSelector && !hasCaptionSummaryCellStyle)
                    return;
                Style newStyle = null;

                if (hasCaptionSummaryCellStyleSelector && hasCaptionSummaryCellStyle)
                {
                    newStyle = DataGrid.CaptionSummaryCellStyleSelector.SelectStyle(record, cell);
                    newStyle = newStyle ?? DataGrid.CaptionSummaryCellStyle;                 
                }
                else if (hasCaptionSummaryCellStyleSelector)
                {
                    newStyle = DataGrid.CaptionSummaryCellStyleSelector.SelectStyle(record, cell);
                }                
                else if (hasCaptionSummaryCellStyle)
                {
                    newStyle = DataGrid.CaptionSummaryCellStyle;
                }
                // WPF-35961 - When the CaptionSummaryCellStyle is explicitly set null value,then we need to clear the CaptionSummaryStyle for the datagrid.
                if (newStyle != null)
                    summaryCell.Style = newStyle;
                else
                    summaryCell.ClearValue(FrameworkElement.StyleProperty);
            }
            
        }

        private GridColumn GetGroupedColumn(Group group)
        {
            var groupDesc = this.DataGrid.View.GroupDescriptions[group.Level - 1] as ColumnGroupDescription;
            //return this.dataGrid.Columns.FirstOrDefault(col => col.MappingName == groupDesc.PropertyName);
            foreach (var column in this.DataGrid.Columns)
            {
                if (column.MappingName == groupDesc.PropertyName)
                {
                    return column;
                }
            }
            return null;
        }
    }
}
