#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Linq;
using Syncfusion.Data.Extensions;
using System.Collections.Generic;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridRowPanel : Panel, IDisposable
    {
        #region Fields

        internal Func<TreeDataRowBase> GetDataRow;

        #endregion

        #region Ctor

        public TreeGridRowPanel()
        {

        }

        #endregion

        #region override Methods

        #region MeasureOverride
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.GetDataRow == null)
                return base.MeasureOverride(availableSize);
            EnsureItems(availableSize);
            return availableSize;
        }
        #endregion

        #region ArrangeOverride

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.GetDataRow == null)
                return base.ArrangeOverride(finalSize);
            ArrangeColumns(finalSize);
            return finalSize;
        }

        #endregion

        #endregion


        private void ArrangeColumns(Size finalSize)
        {
            var rowHeight = finalSize.Height;
            var treeRow = this.GetDataRow();
            if (treeRow.RowIndex < 0)
                return;
            var visibleColumns = treeRow.VisibleColumns;
#if UWP
            //UWP-5800 - Header cell border gets flickered while resizing - If background color applied to the cells
            // then next cell background color overlapped on the current cell right border while resizing
            //To avoid this behavior, we have set ZIndex to the header cells in reverse order
            int panelIndex = -1;
            if (treeRow.RowType == TreeRowType.HeaderRow)
                panelIndex = treeRow.VisibleColumns.Count;
#endif
            foreach (var column in visibleColumns)
            {
                if (column.Element.Visibility != Visibility.Visible) continue;
#if UWP
                //UWP-5800 - Header cell border gets flickered while resizing - If background color applied to the cells
                // then next cell background color overlapped on the current cell right border while resizing
                //To avoid this behavior, we have set ZIndex to the header cells in reverse order
                if (panelIndex > -1)
                {
                    column.Element.SetValue(Canvas.ZIndexProperty, panelIndex);
                    panelIndex--;
                }
#endif
                double ClippedWidth = 0;
                var lineInfo = new List<VisibleLineInfo>();
                var line = treeRow.GetColumnVisibleLineInfo(column.Index);
                var lineSize = treeRow.GetColumnSize(column.Index, false);
                double clippedSize = 0;

                if (line != null)
                {
                    if (line.IsClippedBody && line.IsClippedOrigin && line.IsClippedCorner)
                    {
                        if (clippedSize == 0)
                            column.Element.Clip = new RectangleGeometry() { Rect = new Rect(line.Size - (line.ClippedSize + line.ClippedCornerExtent), 0, line.ClippedSize, (rowHeight + rowHeight)) };
                        else
                            column.Element.Clip = new RectangleGeometry() { Rect = new Rect(lineSize - (clippedSize + line.ClippedCornerExtent), 0, line.ClippedSize, (rowHeight + rowHeight)) };
                    }
                    else if (line.IsClippedBody && line.IsClippedOrigin)
                    {
                        if (clippedSize == 0)
                            column.Element.Clip = new RectangleGeometry() { Rect = new Rect(line.Size - line.ClippedSize, 0, lineSize, (rowHeight + rowHeight)) };
                        else
                            column.Element.Clip = new RectangleGeometry() { Rect = new Rect(lineSize - clippedSize, 0, lineSize, (rowHeight + rowHeight)) };
                    }
                    else if (line.IsClippedBody && line.IsClippedCorner)
                        column.Element.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, line.ClippedSize, (rowHeight + rowHeight)) };
                    else
                    {
                        if (column.Element.Clip != null)
                            column.Element.Clip = null;
                    }

                    Rect rect;
                    rect = new Rect(line.Origin, 0, line.Size, rowHeight);
                    rect.Width = treeRow.GetColumnSize(column.Index, false) - ClippedWidth;
                    if (column.Renderer != null)
                    {
                        column.Renderer.Arrange(new RowColumnIndex(column.RowIndex, column.Index), column.Element, rect);
                    }
                    else
                        column.Element.Arrange(rect);
                }

                //WPF-33827 - When we have more number of columns and some columns are in out of view, need to set column visibility as Collapsed for the columns which are in out of view. 
                else
                {
                    if (column.ColumnVisibility == Visibility.Visible)
                        column.ColumnVisibility = Visibility.Collapsed;
                }
            }
        }

        private void EnsureItems(Size availableSize)
        {
            double rowHeight = availableSize.Height;
            var treeRow = this.GetDataRow();

            if (treeRow.RowIndex < 0)
                return;

            var visibleColumns = treeRow.VisibleColumns;

            foreach (ITreeDataColumnElement column in visibleColumns)
            {
                if (column.Element.Visibility != Visibility.Visible || column.Index < 0)
                    continue;
                if (!this.Children.Contains(column.Element))
                {
                    this.Children.Add(column.Element);
                    column.UpdateCellStyle();
                }

                Size size = new Size(treeRow.GetColumnSize(column.Index, false), rowHeight);
                column.Element.Measure(size);
            }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowPanel"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowPanel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            this.GetDataRow = null;
            this.Children.Clear();
        }
    }
}
