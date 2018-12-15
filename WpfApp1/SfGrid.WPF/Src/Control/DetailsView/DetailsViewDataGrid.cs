#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Linq;
#if !WinRT && !UNIVERSAL
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#else
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
#endif
using Syncfusion.Data.Extensions;

namespace Syncfusion.UI.Xaml.Grid
{
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    /// <summary>
    /// Represents a control that is used to display the hierarchical data in SfDataGrid
    /// </summary>    
    public class DetailsViewDataGrid : SfDataGrid, IDetailsViewInfo
    {

#if WPF
        static DetailsViewDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DetailsViewDataGrid), new FrameworkPropertyMetadata(typeof(DetailsViewDataGrid)));
        }
#endif
        public DetailsViewDataGrid()
        {
#if !WPF
            this.DefaultStyleKey = typeof (DetailsViewDataGrid);
#endif
            this.AllowDetailsViewPadding = true;
        }

        internal void InitializeDetailsViewDataGrid()
        {
            container = new VisualContainer();
            this.RefreshContainerAndView();
        }

        /// <summary>
        /// Builds the visual tree for the DetailsViewDataGrid when a new template is applied.
        /// </summary>
#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            var cp = this.GetTemplateChild("PART_ContentPresenter") as ContentPresenter;          
            //WPF-32820 GridColumn.CellStyle and GridColumn.HeaderStyle as null is not applied for DetailsViewDataGrid column
            foreach (var column in this.Columns)
            {
                if (column.ReadLocalValue(GridColumn.HeaderStyleProperty) != DependencyProperty.UnsetValue)
                    column.hasHeaderStyle = true;
                if (column.ReadLocalValue(GridColumn.CellStyleProperty) != DependencyProperty.UnsetValue)
                    column.hasCellStyle = true;
            }
            if (cp != null)
                cp.Content = container;
            this.RefreshUnBoundRows();
        }

        /// <summary>
        ///  Refreshes VisualContainer and View properties based on DetailsViewDataGrid property settings.        
        /// </summary>
        protected override void RefreshContainerAndView()
        {
           base.RefreshContainerAndView();
        }

        internal override void RefreshUnBoundRows(bool resetUnBoundRowIndex = false)
        {
            base.RefreshUnBoundRows(resetUnBoundRowIndex);
        }        

        protected internal override void RefreshHeaderLineCount()
        {
            headerLineCount = 1;
            if (StackedHeaderRows.Count > 0)
                headerLineCount += StackedHeaderRows.Count;
            if (AddNewRowPosition == AddNewRowPosition.FixedTop)
                headerLineCount += 1;
            if (FilterRowPosition == FilterRowPosition.FixedTop)
                headerLineCount += 1;
            headerLineCount += this.GetTableSummaryCount(TableSummaryRowPosition.Top);
            
            var frozenCount = this.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);
            headerLineCount += frozenCount;
        }

        /// <summary>
        /// Wires the events associated with the DetailsViewDataGrid.
        /// </summary>
        protected override void WireEvents()
        {
            base.WireEvents();
        }

        /// <summary>
        /// Unwire the events associated with the DetailsViewDataGrid.
        /// </summary>
        protected override void UnWireEvents()
        {
            base.UnWireEvents();
        }

        protected override void DisposeViewOnItemsSourceChanged()
        {
            //base.DisposeViewOnItemsSourceChanged();
        }

        /// <summary>
        /// While expanding first time, DetailsviewDataRow will be visible. In that case, need to raise the event, if it is not already raised
        /// </summary>
        internal bool IsLoadedEventFired = false;
        internal override void SetSourceList(object itemsSource)
        {
            this.UnWireEvents();

            if (itemsSource == null)
            {
                this.RowGenerator.Items.ForEach(datarow =>
                    {
                        datarow.VisibleColumns.ForEach(datacolumn => datacolumn.ColumnIndex = -1);
                    });
            }

            CreateCollectionView(itemsSource);
            this.WireEvents();
            if (this.View == null)
            {
                return;
            }
            var needsRefresh = false;
            if (this.View.SortDescriptions.Count != this.SortColumnDescriptions.Count || this.View.GroupDescriptions.Count != this.GroupColumnDescriptions.Count)
            {
                needsRefresh = true;
            }
            else
            {
                foreach (var sortdescription in this.SortColumnDescriptions)
                {
                    var sort = this.View.SortDescriptions.Any(x => !x.PropertyName.Equals(sortdescription.ColumnName) && !x.Direction.Equals(sortdescription.SortDirection));
                    if (sort)
                    {
                        needsRefresh = true;
                        break;
                    }
                }

                foreach (var groupdescription in this.GroupColumnDescriptions)
                {
                    var group = this.View.GroupDescriptions.Any(x => !x.GroupNames.Equals(groupdescription.ColumnName) && !x.GroupNames.Equals(groupdescription.Converter));
                    if (group)
                    {
                        needsRefresh = true;
                        break;
                    }
                }

                foreach (var summaryRow in this.GroupSummaryRows)
                {
                    var summary = this.View.SummaryRows.Any(x => x == summaryRow);
                    if(!summary)
                    {
                        needsRefresh = true;
                        break;
                    }
                }

                foreach (var summaryRow in this.TableSummaryRows)
                {
                    var summary = this.View.TableSummaryRows.Any(x => x == summaryRow);
                    if (!summary)
                    {
                        needsRefresh = true;
                        break;
                    }
                }

                if(this.View.CaptionSummaryRow != this.CaptionSummaryRow)
                {
                    needsRefresh = true;
                }
            }
            if (needsRefresh)
                DeferRefresh();
        }

        #region IDetailsViewScrollInfo

        /// <summary>
        /// Set clip for DetailsViewDataGrid
        /// </summary>
        /// <param name="rect">The clip rect.</param>
        public void SetClipRect(Rect rect)
        {
            var width = this.VisualContainer.ColumnWidths.TotalExtent + 1;
            var height = rect.Height + VisualContainer.RowHeights.PaddingDistance;         
            var clipRect = new Rect(0, 0, 0, 0);
            // WPF-19997(Issue 1) - Rect is Empty in some cases. So below condition is added
            if (!rect.IsEmpty)
                clipRect = new Rect(0, 0, width < rect.Width ? width : rect.Width, rect.Height);              
            this.Clip = new RectangleGeometry { Rect = clipRect };
        }

        /// <summary>
        /// Sets the Horizontal offset of VisualContainer.
        /// </summary>
        /// <param name="offset">Horizontal offset.</param>
        public void SetHorizontalOffset(double offset)
        {
            if (!double.IsNaN(offset))
                this.VisualContainer.SetHorizontalOffset(offset);
        }

        /// <summary>
        /// Sets the vertical offset of VisualContainer.
        /// </summary>
        /// <param name="offset">vertical offset.</param>
        public void SetVerticalOffset(double offset)
        {
            if (!double.IsNaN(offset))
                this.VisualContainer.SetVerticalOffset(offset);
        }

        /// <summary>
        /// Gets the TotalExtent column width of VisualContainer.
        /// </summary>
        /// <returns>TotalExtent.</returns>
        public double GetExtendedWidth()
        {
            return this.VisualContainer.ColumnWidths.TotalExtent;
        }

        #endregion
    }
}
