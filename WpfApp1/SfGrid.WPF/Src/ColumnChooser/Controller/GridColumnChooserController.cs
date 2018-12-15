#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if !WPF
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows.Controls.Primitives;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a controller that serves the functionality to add or remove columns dynamically at column chooser window.
    /// </summary>
    public class GridColumnChooserController : GridColumnDragDropController
    {
        #region Fields
        internal SfDataGrid dataGrid;
        IColumnChooser chooser;
        Style style;
        bool popUpFromChooser;
        bool allowHidingForFinalColumn;
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the last column can be drag in the view of column chooser.
        /// </summary>
        /// <value>
        /// <b>true</b> if the last column should be hidden in the view of column chooser; otherwise , <b>false</b>.
        /// </value>
        /// <remarks></remarks>
        public bool AllowHidingForFinalColumn
        {
            get { return allowHidingForFinalColumn; } 
            set { allowHidingForFinalColumn = value; }
        }
        
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnChooserController"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="columnChooserwindow">
        /// The column chooser window.
        /// </param>
        public GridColumnChooserController(SfDataGrid dataGrid, IColumnChooser columnChooserwindow)
            : base(dataGrid)
        {
            this.dataGrid = dataGrid;
            this.chooser = columnChooserwindow;
            style = this.PopupContentControl.Style;
        }
        #endregion

        #region Public method
        /// <summary>
        /// Shows the popup for the specified column index to enable drag and drop operation.
        /// </summary>
        /// <param name="colIndex">
        /// The index of the column to enable popup.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseEventArgs"/> that contains the corresponding mouse point.
        /// </param>        
#if WPF
        public void Show(int colIndex, MouseEventArgs e)
        {
            var point = e.GetPosition(null);
            this.ShowPopup(colIndex, new Rect(point.X - 60, point.Y, 120, 30), null);
            popUpFromChooser = true;
        }
#else
        /// <summary>
        /// Shows the popup for the specified column index to enable drag and drop operation.
        /// </summary>
        /// <param name="colIndex">
        /// The index of the column to enable popup.
        /// </param>
        /// <param name="e">
        /// The <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs"/> that contains the corresponding mouse point.
        /// </param>    
        public void Show(int colIndex, PointerRoutedEventArgs e)
        {
            //UWP-2543 - Need to convert pointer position of the screen co-ordinates to its dataGrid points.
            var point = e.GetCurrentPoint(dataGrid).Position;
            this.ShowPopup(colIndex, new Rect(point.X - 70, point.Y, 140, 45), e.Pointer);
            popUpFromChooser = true;
        }
#endif
        #endregion

        #region Overrides
        /// <summary>
        /// Gets the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridRegion"/> at the specified pointer position.
        /// </summary>
        /// <param name="point">
        /// The position to get the corresponding grid region.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridRegion"/> at the specified point.
        /// </returns>
        public override GridRegion PointToGridRegion(Point point)
        {
            if (this.chooser.GetControlRect().Contains(point) && this.PopupContentControl != null)
                return GridRegion.ColumnChooser;
            return base.PointToGridRegion(point);
        }

        /// <summary>
        /// Determines whether the popup is displayed at the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to show the popup.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the popup is displayed  at the specified column; otherwise, <b>false</b>.
        /// </returns>
        public override bool CanShowPopup(GridColumn column)
        {
            return true;
        }

        /// <summary>
        /// Invoked when the position of popup content is changed in SfDataGrid.
        /// </summary>
        /// <param name="HorizontalDelta">
        /// The corresponding horizontal distance of the popup content position changes. 
        /// </param>
        /// <param name="VerticalDelta">
        /// The corresponding vertical distance of the popup content position changes. 
        /// </param>
        /// <param name="mousePointOverGrid">
        /// Indicates whether the mouse point is hovered inside or out of the SfDataGrid.
        /// </param>
        protected override void OnPopupContentPositionChanged(double HorizontalDelta, double VerticalDelta, Point mousePointOverGrid)
        {
            base.OnPopupContentPositionChanged(HorizontalDelta, VerticalDelta, mousePointOverGrid);
            var rect = this.chooser.GetControlRect();
            if (rect.Contains(mousePointOverGrid) && this.PopupContentControl != null)
            {
                this.CloseDragIndication();
                if (this.dataGrid.Columns.Where(col => col.IsHidden).Count() == this.dataGrid.Columns.Count - 1 && !AllowHidingForFinalColumn)
                {
                    VisualStateManager.GoToState(this.PopupContentControl, "InValid", true);
                }
                else
                    VisualStateManager.GoToState(this.PopupContentControl, "Valid", true);
            }
        }

        /// <summary>
        /// Invoked when the popup content dropped on SfDataGrid.
        /// </summary>
        /// <param name="pointOverGrid">
        /// Indicates whether the mouse point is hovered inside or out of the SfDataGrid.
        /// </param>
        protected override void OnPopupContentDropped(Point pointOverGrid)
        {
            var rect = this.chooser.GetControlRect();
            var headerRowRect = this.GetHeaderRowRect();
            var groupDropAreaRect = this.GetGroupDropAreaRect();
            var gridRect = this.dataGrid.GetControlRect(this.dataGrid);
            var gridColumn = this.PopupContentControl.Tag as GridColumn;

            //WPF-28403 Need to check mouse position in datagrid value instead of pointoscreen value whether it is lie in rect or not
            if (rect.Contains(pointOverGrid) && this.PopupContentControl != null)
            {
                var count = this.dataGrid.Columns.Where(col => col.IsHidden).Count();
                if (this.dataGrid.Columns.Where(col => col.IsHidden).Count() == this.dataGrid.Columns.Count - 1 && !AllowHidingForFinalColumn)
                {
                    (this.PopupContentControl.Parent as Popup).IsOpen = false;
                    return;
                }
                //gridColumn.IsHidden = true;
#if !WPF
                this.SuspendReverseAnimation(true);
#endif
                if (this.PopupContentControl.IsDragFromGroupDropArea)
                    base.OnPopupContentDropped(pointOverGrid);
                else
                {
                    this.HidePopup();
                    base.dpTimer.Stop();
                }
                //WPF-28403 While dragging from GroupDropArea, the IsHidden property is set to false while removing group, 
                //Hence the below line is moved from above.
                gridColumn.IsHidden = true;               
            }
            else if (popUpFromChooser && this.PopupContentControl != null)
            {
#if !WPF
                if (!gridColumn.IsHidden)
                    this.SuspendReverseAnimation(true);
                else
                    this.SuspendReverseAnimation(false);
#endif
                base.OnPopupContentDropped(pointOverGrid);

                //WPF-28403 Need to assign grid width to header width to drop the column when pointer is inside the datagrid
                headerRowRect.Width = gridRect.Width;
                if ((this.dataGrid.AllowGrouping &&
                    (groupDropAreaRect.Contains(pointOverGrid) && this.dataGrid.ShowColumnWhenGrouped)) || 
                    (headerRowRect.Contains(pointOverGrid)))
                {
                    if (gridColumn.Width == 0)
                        gridColumn.Width = 150;
                    gridColumn.IsHidden = false;
                }
            }
            else
            {
#if !WPF
                this.SuspendReverseAnimation(false);
#endif
                base.OnPopupContentDropped(pointOverGrid);
            }
            popUpFromChooser = false;
        }

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.IsHidden"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column on which the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.IsHidden"/> property value changes occurs.
        /// </param>
        protected override void OnColumnHiddenChanged(GridColumn column)
        {
            if (column.IsHidden)
            {
                this.chooser.AddChild(column);
            }           
            else
                this.chooser.RemoveChild(column);
            base.OnColumnHiddenChanged(column);
        }
        #endregion
    }
}
