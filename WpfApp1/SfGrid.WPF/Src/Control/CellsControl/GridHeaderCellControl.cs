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
using Syncfusion.UI.Xaml.Grid.Helpers;
#if !WPF
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Syncfusion.UI.Xaml.Utils;
using System.Collections.Specialized;
using Windows.UI.Xaml.Data;
using Syncfusion.Data;
#else
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Threading;
using Syncfusion.Data;
#endif
using System.Threading.Tasks;
using System.Collections.Generic;
using Syncfusion.Data.Extensions;

namespace Syncfusion.UI.Xaml.Grid
{
#if !WPF
    using MouseEventArgs = PointerRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using GestureEventArgs = HoldingRoutedEventArgs;
    using ManipulationStartedEventArgs = ManipulationStartedRoutedEventArgs;
#endif

    [TemplatePart(Name = "PART_FilterToggleButton", Type = typeof(FilterToggleButton))]
    [TemplatePart(Name = "PART_FilterPopUpPresenter", Type = typeof(Border))]
    public class GridHeaderCellControl : ContentControl, IDisposable
    {
        #region Fields

        protected internal SfDataGrid DataGrid;
        private bool isdisposed = false;
        internal string hiddenResizingVisualState = null;
#if WPF
        internal bool isTouchPressed = false;
        private Point mouseDownPoint;
        private TouchDevice touchDevice;
#else
        private Pointer pointer = null;
        private Point pointerDown;
#endif
        private Point gridMouseDown;
        internal static bool isFilterToggleButtonPressed = false;

        #endregion
        
        public GridHeaderCellControl()
        {
            this.DefaultStyleKey = typeof(GridHeaderCellControl);
#if !WPF
            this.IsHoldingEnabled = true;
            this.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
#else
            this.IsManipulationEnabled = true;
#endif
            this.IsTabStop = false;
        }
        
        /// <summary>
        /// Gets Filter popup Control type of GridFilterControl.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        protected GridFilterControl FilterPopupHost;

        /// <summary>
        /// Gets or sets Filter toggle button.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>

        protected FilterToggleButton FilterToggleButton;

        /// <summary>
        /// Gets or sets Presenter for Filter Popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        protected Border FilterPopUpPresenter;

        /// <summary>
        /// Gets a value indicating whether the filter s applied .
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        protected internal bool IsFilterApplied
        {
            get { return this.Column.FilterPredicates.Count > 0 ? true : false; }
        }
        
        #region Visual State Properties

        private GridCellRegion gridCellRegion;

        public GridCellRegion GridCellRegion
        {
            get 
            { 
                return gridCellRegion; 
            }
            set 
            { 
                gridCellRegion = value;
                UpdateGridHeaderCellBorderState();
            }
        }

        public void UpdateGridHeaderCellBorderState(bool canApplyDefaultState = true)
        {
            if (canApplyDefaultState)
                VisualStateManager.GoToState(this, gridCellRegion.ToString(), true);
            else if (gridCellRegion != GridCellRegion.NormalCell)
                VisualStateManager.GoToState(this, gridCellRegion.ToString(), true);
        }

        #endregion
        
        /// <summary>
        /// Gets or sets Associated GridColumn.
        /// </summary>
        /// <value></value>
        /// <remarks>Using this Column all other operations will be done</remarks>
        public GridColumn Column
        {
            get { return (GridColumn)GetValue(ColumnProperty); }
            set
            {
                SetValue(ColumnProperty, value);
            }
        }

        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register("Column", typeof(GridColumn), typeof(GridHeaderCellControl), new PropertyMetadata(null, OnColumnPropertyChanged));

        private static void OnColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GridHeaderCellControl).OnColumnChanged(e);
        }

        protected virtual void OnColumnChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets ColumnOptionsWidth.
        /// </summary>
        /// <value></value>
        /// <remarks>this width will be set for column width rest than for Content presenter</remarks>
        public double ColumnOptionsWidth
        {
            get { return (double)this.GetValue(ColumnOptionsWidthProperty); }
            internal set { this.SetValue(ColumnOptionsWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnOptionsWidthProperty =
            DependencyProperty.Register("ColumnOptionsWidth", typeof(double), typeof(GridHeaderCellControl),
                                        new PropertyMetadata((double)0, null));

        /// <summary>
        /// Gets or sets Order/Number for sort columns.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string SortNumber
        {
            get { return (string)this.GetValue(SortNumberProperty); }
            internal set { this.SetValue(SortNumberProperty, value); }
        }

        public static readonly DependencyProperty SortNumberProperty = DependencyProperty.Register("SortNumber", typeof(string), typeof(GridHeaderCellControl), new PropertyMetadata(String.Empty));

        /// <summary>
        /// Gets or sets Sorting Number visibility.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Visibility SortNumberVisibility
        {
            get { return (Visibility)this.GetValue(SortNumberVisibilityProperty); }
            set { this.SetValue(SortNumberVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SortNumberVisibilityProperty =
            DependencyProperty.Register("SortNumberVisibility", typeof(Visibility), typeof(GridHeaderCellControl),
                                        new PropertyMetadata(Visibility.Collapsed, null));

        ///<summary>
        /// Gets or sets Path direction (Ascending/Descending).
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public object SortDirection
        {
            get { return (object)this.GetValue(SortDirectionProperty); }
            set { this.SetValue(SortDirectionProperty, value); }
        }

        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register("SortDirection", typeof(object), typeof(GridHeaderCellControl), new PropertyMetadata(null,OnSortDirectionChanged));

        private static void OnSortDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var header = obj as GridHeaderCellControl;
            header.OnSortDirectionChanged(args);
        }

        protected virtual void OnSortDirectionChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        public Visibility FilterIconVisiblity
        {
            get { return (Visibility)this.GetValue(FilterIconVisiblityProperty); }
            set { this.SetValue(FilterIconVisiblityProperty, value); }
        }

        public static readonly DependencyProperty FilterIconVisiblityProperty =
            DependencyProperty.Register("FilterIconVisiblity", typeof(Visibility), typeof(GridHeaderCellControl),
                                        new PropertyMetadata(Visibility.Collapsed));
        
        #region Override and Drag & Drop

#if !WPF
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            if (FilterPopUpPresenter != null)
            {
                FilterPopUpPresenter.Child = null;
            }
            UnWireGridHeaderCellControlEvents();
#if WPF
            UnWireExcelLikeFilteringEvents(true);
#else
            UnWireExcelLikeFilteringEvents(false);
#endif
            base.OnApplyTemplate();

            FilterToggleButton = GetTemplateChild("PART_FilterToggleButton") as FilterToggleButton;
            FilterPopUpPresenter = GetTemplateChild("PART_FilterPopUpPresenter") as Border;
            WireGridHeaderCellControlEvents();
            WireExcelLikeFilteringEvents();
            ApplyFilterToggleButtonVisualState();
            // Applying Visual state for Hidden Column Resizing
            if (!string.IsNullOrEmpty(hiddenResizingVisualState))
                VisualStateManager.GoToState(this, hiddenResizingVisualState, true);
            if (this.DataGrid != null && this.DataGrid.AllowResizingColumns && this.DataGrid.AllowResizingHiddenColumns)
                this.DataGrid.ColumnResizingController.ProcessResizeStateManager(this.Column);
            UpdateGridHeaderCellBorderState(false);
        }

        private void GridHeaderCellControlLoaded(object sender, RoutedEventArgs e)
        {
            ApplyFilterToggleButtonVisualState();
#if WPF
            this.ContextMenuOpening += OnContextMenuOpening;
#endif
            if (this.DataGrid != null && this.DataGrid.AllowResizingColumns && this.DataGrid.AllowResizingHiddenColumns)
                this.DataGrid.ColumnResizingController.ProcessResizeStateManager(this.Column);
        }

        /// <summary>
        /// Opens the context menu at the specified position.
        /// </summary>
        /// <param name="position">The position to display context menu.</param>
        /// <returns>
        /// <b>true</b> If the context menu opened;Otherwise<b>false</b>
        /// </returns>
#if UWP
        protected virtual internal bool ShowContextMenu(Point position)
#else
        protected virtual internal bool ShowContextMenu()
#endif
        {
            if (this.DataGrid == null || this.DataGrid.HeaderContextMenu == null)
                return false;
         
            var rowColIndex = new RowColumnIndex(DataGrid.GetHeaderIndex(), DataGrid.ResolveToScrollColumnIndex(DataGrid.Columns.IndexOf(Column)));
            var menuinfo = new GridColumnContextMenuInfo() { Column = this.Column };
            if (DataGrid is DetailsViewDataGrid)
            {
                menuinfo.DataGrid = DataGrid;
                menuinfo.SourceDataGrid = (DataGrid as DetailsViewDataGrid).GetTopLevelParentDataGrid();
            }
            else
                menuinfo.DataGrid = DataGrid;
            var args = new GridContextMenuEventArgs(DataGrid.HeaderContextMenu, menuinfo, rowColIndex, ContextMenuType.Header, this.DataGrid);
            if (!this.DataGrid.RaiseGridContextMenuEvent(args))
            {
#if WPF
                if (args.ContextMenuInfo != null)
                    DataGrid.HeaderContextMenu.DataContext = args.ContextMenuInfo;
                DataGrid.HeaderContextMenu.PlacementTarget = this;
                DataGrid.HeaderContextMenu.IsOpen = true;
#else
                if (args.ContextMenuInfo != null)
                   foreach (var item in args.ContextMenu.Items)
                       item.DataContext = args.ContextMenuInfo;
                args.ContextMenu.ShowAt(this, position);
#endif
                return true;
            }
            return false;
        }

        private void GridHeaderCellControlUnloaded(object sender, RoutedEventArgs e)
        {
#if WPF
            this.ContextMenuOpening -= OnContextMenuOpening;
#endif
        }

        /// <summary>
        /// The method that used to set tooltip for GridHeaderCellControl.
        /// </summary>        
        private void ShowToolTip()
        {
            if (Column == null || !this.Column.ShowHeaderToolTip)
            {
                this.ClearValue(ToolTipService.ToolTipProperty);
                return;
            }

            var obj = ToolTipService.GetToolTip(this);
            ToolTip tooltip;
            if (obj is ToolTip)
                tooltip = obj as ToolTip;
            else
                tooltip = new ToolTip();

            if (this.Column.hasHeaderToolTipTemmplate)
                tooltip.Content = Column.HeaderToolTipTemplate.LoadContent();
            else
            {
                //UWP-2846 - ToolTip value shown as MappingName of the header cell instead of HeaderText
                //So modified to set the tooltip content as HeaderText
                //tooltip.Content = Column.MappingName;
                tooltip.Content = Column.HeaderText;
                if (string.IsNullOrEmpty(Column.HeaderText))
                    tooltip.IsEnabled = false;
            }
            //Specifies to raise tooltip opening event for the corresponding cell
            if (RaiseCellToolTipOpening(tooltip))
                ToolTipService.SetToolTip(this, tooltip);
            else
                this.ClearValue(ToolTipService.ToolTipProperty);
        }
        /// <summary>
        /// Specifies to raise tooltip opened event for the header cell
        /// </summary>
        /// <param name="tooltip"></param>
        internal bool RaiseCellToolTipOpening(ToolTip tooltip)
        {
            if (this.DataGrid != null && this.DataGrid.CanCellToolTipOpening())
            {
                var cellToolTipOpeningEventArgs = new GridCellToolTipOpeningEventArgs(this)
                {
                    Column = this.Column,
                    RowColumnIndex = new RowColumnIndex(DataGrid.GetHeaderIndex(), DataGrid.ResolveToScrollColumnIndex(DataGrid.Columns.IndexOf(Column))),
                    Record = this.DataContext,
                    ToolTip = tooltip
                };
                this.DataGrid.RaiseCellToolTipOpeningEvent(cellToolTipOpeningEventArgs);
            }
            return tooltip.IsEnabled;
        }

#if !WPF
        protected override void OnManipulationStarting(ManipulationStartingRoutedEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (this.DataGrid.ColumnResizingController.dragLine != null)
                this.CapturePointer(pointer);
            base.OnManipulationStarting(e);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(true))
                return;

            var device = pointer;
            if (!DataGrid.ColumnResizingController.isHovering && device != null)
            {
                if (CanShowPopup())
                {
                    //UWP-1817 Converted Screen Co-ordinates into dataGrid points. 
                    var point = this.TransformToVisual(this.DataGrid).TransformPoint(new Point(0, 0));
                    Rect rect = new Rect(point.X, point.Y, this.ActualWidth, this.ActualHeight);
                    VisualStateManager.GoToState(this, "Normal", true);
                    if (this.DataGrid.GridColumnDragDropController != null)
                        this.DataGrid.GridColumnDragDropController.ShowPopup(this.Column, rect, pointerDown, false,
                                                              device);
                    e.Handled = true;
                    return;
                }
            }
            base.OnManipulationStarted(e);
        }

        protected override void OnPointerCaptureLost(MouseButtonEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            //WPF-18630  while resizing column sometime OnPointerReleased event is not fired because of mouse release it's left button from out side of the grid region.
            //in this case line resize index and lineresize size are not reseted.to avoid this case using OnMouseLeftButtonReleased method to process entire OnPointerReleased event.
            OnMouseLeftButtonReleased(e);
            if (this.DataGrid.ColumnResizingController.dragLine != null)
            {
                this.DataGrid.ColumnResizingController.dragLine = null;
                this.DataGrid.ColumnResizingController.SetPointerCursor(CoreCursorType.Arrow);               
            }
            //WRT-4369 after release the mouse pointer if column pop up is shown need to close it. 
            if (this.DataGrid.GridColumnDragDropController.DraggablePopup != null && this.DataGrid.GridColumnDragDropController.DraggablePopup.IsOpen)
                this.DataGrid.GridColumnDragDropController.DraggablePopup.IsOpen = false;

            base.OnPointerCaptureLost(e);
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            this.DataGrid.ColumnResizingController.dragLine = null;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(false))
                return;

            if (Column.AllowSorting &&
                this.DataGrid.SortClickAction == SortClickAction.DoubleClick && !this.DataGrid.ColumnResizingController.isHovering)
                Sort();

            Point pp = e.GetPosition(this.DataGrid.VisualContainer);
            var cursor = CoreCursorType.Arrow;
            var dragline = this.DataGrid.ColumnResizingController.HitTest(pp, out cursor);
            if (cursor != CoreCursorType.Arrow)
                this.DataGrid.ColumnResizingController.SetPointerCursor(cursor);
            if (Window.Current.CoreWindow.PointerPosition.X > 0 && Window.Current.CoreWindow.PointerPosition.Y > 0)
            {
                if (CanResizeColumn() && Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow && dragline != null)
                {
                    var colIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(dragline.LineIndex);
                    if (colIndex >= 0 && colIndex < this.DataGrid.Columns.Count)
                    {
                        var args = new ResizingColumnsEventArgs(this.DataGrid)
                        {
                            ColumnIndex = dragline.LineIndex,
                            Width = this.DataGrid.GridColumnSizer.CalculateAutoFitWidth(DataGrid.Columns[colIndex])
                        };
                        if (!this.DataGrid.RaiseResizingColumnsEvent(args))
                        {
                            DataGrid.Columns[colIndex].ActualWidth = args.Width;
                            // after resizing, width should be set to refresh the column sizer
                            DataGrid.Columns[colIndex].Width = args.Width;
                            DataGrid.VisualContainer.ColumnWidths[dragline.LineIndex] = args.Width;
                        }
                    }
                }
                else
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            }
            base.OnDoubleTapped(e);
        }

        /// <summary>
        /// When Right click the SfDataGrid Cell, Context menu appears for the selected cell. we are using this event for context menu support in Header cell.
        /// </summary>
        /// <param name="e">Right tapped event arguments</param>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            if (ShowContextMenu(position))
                e.Handled = true;
            base.OnRightTapped(e);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(false))
                return;

            if (Column.AllowSorting &&
                this.DataGrid.SortClickAction == SortClickAction.SingleClick && !this.DataGrid.ColumnResizingController.isHovering)
                Sort();
            base.OnTapped(e);
        }

        protected override void OnPointerMoved(MouseEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (this.CanResizeHiddenColumn() && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                this.DataGrid.ColumnResizingController.DoActionOnMouseMove(e.GetCurrentPoint(this.DataGrid.VisualContainer), this);
            }
            base.OnPointerMoved(e);
        }

        protected override void OnPointerEntered(MouseButtonEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                VisualStateManager.GoToState(this, "PointerOver", true);
            ShowToolTip();
            base.OnPointerEntered(e);
        }

        protected override void OnPointerExited(MouseButtonEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                if (this.DataGrid == null || this.DataGrid.VisualContainer == null)
                    return;

                PointerPoint pp = e.GetCurrentPoint(this.DataGrid.VisualContainer);
                if (pp != null)
                {
                    if (Window.Current != null && Window.Current.CoreWindow != null)
                    {
                        if (!pp.Properties.IsLeftButtonPressed &&
                            Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
                        {
                            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                        }
                    }
                }
            }
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                VisualStateManager.GoToState(this, "Normal", true);                
            base.OnPointerExited(e);
        }


        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (e.HoldingState == HoldingState.Started && e.PointerDeviceType != PointerDeviceType.Mouse &&
                pointer != null)
            {
                if (CanResizeColumn())
                {
                    GridColumn gridColumn = this.Column;
                    Point mouseDown = e.GetPosition(this);
                    int colIndex = this.DataGrid.Columns.IndexOf(gridColumn);

                    int columnindex = this.DataGrid.ResolveToScrollColumnIndex(colIndex);
                    var line = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnindex);

                    var point = this.TransformToVisual(this.DataGrid).TransformPoint(new Point(0, 0));

                    //UWP-4063 Resizing popup size is varied when column width exceeds the total width of the grid.
                    if (line != null && line.IsClipped)
                    {
                        this.DataGrid.VisualContainer.ScrollColumns.ScrollInView(columnindex);

                        var distance = gridColumn.ActualWidth - line.ClippedSize;

                        if (line.IsClippedOrigin)
                            point.X = point.X + distance;
                        else
                            point.X = point.X - distance;
                    }

                    Rect rect = Rect.Empty;
                    //Here we have calculated the Point value for the Touch position to show the pop-up for the Hidden Column
                    //the Touch position is nearer to the ColumnHeaderline it will check the previous or next column is in hidden state.
                    if ((mouseDown.X < 20 || mouseDown.X > this.ActualWidth - 20) && this.DataGrid.AllowResizingHiddenColumns && colIndex > 0 && colIndex < this.DataGrid.Columns.Count - 1)
                    {
                        int index = mouseDown.X < 20 ? colIndex - 1 : colIndex + 1;
                        GridColumn hiddenColumn = this.DataGrid.Columns[index];
                        if (hiddenColumn.IsHidden)
                        {
                            gridColumn = mouseDown.X < 20 ? hiddenColumn : this.DataGrid.Columns[this.DataGrid.GetNextFocusGridColumnIndex(index)];
                            if (mouseDown.X > this.ActualWidth - 20)
                            {
                                index = this.DataGrid.Columns.IndexOf(gridColumn);
                                gridColumn = gridColumn.IsHidden ? this.DataGrid.Columns[index] : this.DataGrid.Columns[index - 1];
                                rect = new Rect(point.X + this.ActualWidth, point.Y, this.ActualWidth, this.ActualHeight);
                            }
                            this.DataGrid.ColumnResizingController.HiddenLineIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(gridColumn));
                        }
                    }
                    if (rect.IsEmpty)
                        rect = new Rect(point.X, point.Y, this.ActualWidth, this.ActualHeight);
                    this.DataGrid.GridColumnDragDropController.ShowPopup(gridColumn, rect, mouseDown, true, pointer);
                    e.Handled = true;
                }
            }
            // When long press SfDataGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Header Cell.
            if (e.HoldingState == HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                var position = e.GetPosition(this);
                if (ShowContextMenu(position))
                    e.Handled = true;
            }
            base.OnHolding(e);
        }

#else
        
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(false))
                return;
            if (Column.AllowSorting && this.DataGrid.SortClickAction == SortClickAction.DoubleClick && !this.DataGrid.ColumnResizingController.isHovering && this.Cursor == Cursors.Arrow)
                Sort();
            Point pp = e.GetPosition(this.DataGrid.VisualContainer);
            var cursor = this.Cursor;
            var dragline = this.DataGrid.ColumnResizingController.HitTest(pp, out cursor);
            var indentColumnCount = this.DataGrid.ResolveToScrollColumnIndex(0);
            if (CanResizeColumn() && cursor != Cursors.Arrow && dragline != null &&
                dragline.LineIndex >= indentColumnCount)
            {
                var colIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(dragline.LineIndex);
                var args = new ResizingColumnsEventArgs(this.DataGrid)
                {
                    ColumnIndex = dragline.LineIndex,
                    Width = this.DataGrid.GridColumnSizer.CalculateAutoFitWidth(DataGrid.Columns[colIndex])
                };
                if (!this.DataGrid.RaiseResizingColumnsEvent(args))
                {
                    DataGrid.Columns[colIndex].ActualWidth = args.Width;
                    // after resizing, width should be set to refresh the column sizer
                    DataGrid.Columns[colIndex].Width = args.Width;
                    DataGrid.VisualContainer.ColumnWidths[dragline.LineIndex] = args.Width;
                }
                e.Handled = true;
                this.ReleaseMouseCapture();
            }
        }

        //WPF-18630  while resizing column sometime mouseup event is not fired because of mouse release it's left button from out side of the grid region.
        //in this case line resize index and lineresize size are not reseted.to avoid this case using OnLostMouseCapture event to  process the OnMouseLeftButtonUp codes.
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            OnMouseLeftButtonReleased(e);
            base.OnLostMouseCapture(e);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            touchDevice = e.TouchDevice;
            mouseDownPoint = e.GetTouchPoint(this).Position;
            gridMouseDown = e.GetTouchPoint(this.DataGrid.VisualContainer).Position;
            if (!DataGrid.ValidationHelper.CheckForValidation(true))
            {
                e.Handled = true;
                return;
            }
            if (this.CanResizeColumn() && this.DataGrid.ColumnResizingController.isHovering)
            {
                var cursor = this.Cursor;
                this.DataGrid.ColumnResizingController.dragLine = this.DataGrid.ColumnResizingController.HitTest(gridMouseDown, out cursor);
                if (this.DataGrid.ColumnResizingController.dragLine != null)
                    this.CaptureTouch(e.TouchDevice);
                e.Handled = true;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
            //When touch the filter toggle button, avoid the sorting by not set the isTouchPressed. Based on this only, we have sort the column in TouchUp event.
            if (!isFilterToggleButtonPressed)
                isTouchPressed = true;
            base.OnTouchDown(e);
        }
        
        protected override void OnTouchUp(TouchEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            var gridMouseUP = e.GetTouchPoint(this.DataGrid.VisualContainer).Position;
            if (this.DataGrid.ColumnResizingController.dragLine != null && Math.Abs(gridMouseUP.X - gridMouseDown.X) <= 0)
                this.DataGrid.ColumnResizingController.dragLine = null;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(false))
                return;
            if (Column.AllowSorting && this.DataGrid.SortClickAction == SortClickAction.SingleClick && !this.DataGrid.ColumnResizingController.isHovering && this.Cursor == Cursors.Arrow && isTouchPressed)
                Sort();
            isTouchPressed = false;
            base.OnTouchUp(e);
            
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (this.DataGrid == null)
                return;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (this.CanResizeColumn())
            {
                this.DataGrid.ColumnResizingController.DoActionOnMouseMove(touchDevice.GetTouchPoint(this.DataGrid.VisualContainer).Position, this);
            }
            if (!DataGrid.ColumnResizingController.isHovering && isTouchPressed && this.Cursor == Cursors.Arrow)
            {
                if (CanShowPopup())
                {
                    List<IManipulator> devices = e.Manipulators.ToList();
                    var pointerMove = devices[0].GetPosition(this);
                    ShowPopup(pointerMove, touchDevice);
                    isTouchPressed = false;
                    e.Handled = true;
                    return;
                }
            }
            base.OnManipulationDelta(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
            ShowToolTip();
            base.OnMouseEnter(e);
        }

        //Pressed state were introduced in headercell for theme work in SfDataGrid.
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "Pressed", true);
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            base.OnMouseUp(e);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (this.DataGrid != null)
            {
                Point pp = e.GetPosition(this.DataGrid.VisualContainer);
                isMouseLeftButtonPressed = false;             
                if (this.Cursor == this.DataGrid.ColumnResizingController.ResizingCursor && this.DataGrid.ColumnResizingController.dragLine == null && !isMouseLeftButtonPressed)
                {
                    this.DataGrid.ColumnResizingController.SetPointerCursor(Cursors.Arrow, this);
                }
            }
            VisualStateManager.GoToState(this, "Normal", true);
            base.OnMouseLeave(e);
        }

        internal bool isMouseLeftButtonPressed = false;        
        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            var newPosition = Math.Abs(mouseDownPoint.X - e.GetPosition(this).X);
            if (this.DataGrid == null)
                return;
            if (this.CanResizeHiddenColumn() && newPosition > 0)
            {
                this.DataGrid.ColumnResizingController.DoActionOnMouseMove(e.GetPosition(this.DataGrid.VisualContainer), this);
            }
            //WPF-29684 - isMouseLeftButtonPressed is false by touching the header. so used the IsTouchPressed.
            if (!this.DataGrid.ColumnResizingController.isHovering && ((isMouseLeftButtonPressed && e.LeftButton == MouseButtonState.Pressed) || isTouchPressed) && this.Cursor == Cursors.Arrow)
            {
                if (CanShowPopup())
                {
                    if (ShowPopup(e.GetPosition(this), null))
                        isMouseLeftButtonPressed = false;
                    e.Handled = true;
                    return;
                }
            }
            base.OnMouseMove(e);
        }
#endif

#if !WPF
        protected override void OnPointerPressed(MouseButtonEventArgs e)
#else
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
#endif
        {
            if (this.DataGrid == null)
                return;

            if (this.DataGrid.AutoScroller.AutoScrolling != AutoScrollOrientation.None)
                this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;

            ValidationHelper.SetFocusSetBack(false);

            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);
            if (!skipValidation && !DataGrid.ValidationHelper.CheckForValidation())
            {
                e.Handled = true;
                return;
            }
            gridMouseDown = e.GetPosition(this.DataGrid.VisualContainer);
#if WPF
            mouseDownPoint = e.GetPosition(this);
            //WPF-29684 - when touch the column header, no need to set the isMouseLeftButtonPressed as true. Use IsTouchPressed for touch cases.
            if(e.StylusDevice == null)
                isMouseLeftButtonPressed = true;
            if (this.CanResizeHiddenColumn() && this.DataGrid.ColumnResizingController.isHovering)
#else
            pointerDown = e.GetCurrentPoint(this).Position;
            if (this.CanResizeHiddenColumn() && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && this.DataGrid.ColumnResizingController.isHovering && this.DataGrid.ColumnResizingController.dragLine == null)
#endif
            {

#if !WPF
                double hScrollChange = 0;

                for (int i = 0; i < this.DataGrid.VisualContainer.FrozenColumns; i++)
                {
                    hScrollChange += this.DataGrid.VisualContainer.ColumnWidths[i];
                }
                var pointerPoint = new Point(Math.Abs(gridMouseDown.X - (DataGrid.VisualContainer.HScrollBar.Value - hScrollChange)), Math.Abs(gridMouseDown.Y - DataGrid.VisualContainer.VScrollBar.Value));
                var cursor = CoreCursorType.Arrow;
                this.DataGrid.ColumnResizingController.dragLine = this.DataGrid.ColumnResizingController.HitTest(pointerPoint, out cursor);
                if (cursor != CoreCursorType.Arrow)
                    this.DataGrid.ColumnResizingController.SetPointerCursor(cursor);
#else
                var cursor = this.Cursor;
                this.DataGrid.ColumnResizingController.dragLine = this.DataGrid.ColumnResizingController.HitTest(gridMouseDown, out cursor);
                if (this.DataGrid.ColumnResizingController.dragLine != null)
                    this.CaptureMouse();
                e.Handled=true;
#endif
            }
            else
            {
#if WPF
                this.Cursor = Cursors.Arrow;
#else
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
#endif
            }
#if !WPF
            pointer = e.Pointer;
            base.OnPointerPressed(e);
#else
            base.OnMouseLeftButtonDown(e);
#endif
        }
       
#if !WPF
        protected override void OnPointerReleased(MouseButtonEventArgs e)
#else
        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
#endif
        {
            OnMouseLeftButtonReleased(e);
#if !WPF
            base.OnPointerReleased(e);
#else
            base.OnMouseLeftButtonUp(e);
#endif
        }

        /// <summary>
        /// To process entire Mouse left button released event task
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Input.MouseEventArgs">MouseEventArgs</see> that contains the event data.</param>
        private void OnMouseLeftButtonReleased(MouseEventArgs e)
        {
            if (this.DataGrid == null)
            {
#if WPF
                isMouseLeftButtonPressed = false;
#endif
                return;
            }
            var gridMouseUp = e.GetPosition(this.DataGrid.VisualContainer);
            //Fix for WPF-17813 .check whehter the HeaderCell is touched means it retuned to end of the event . 
            //Skipped further mouse event process when touched Header Cell  
#if WPF

            if (e.StylusDevice != null)
                return;
            //WPF-19593 if MouseUpPosition.X and MouseDownPostion.X are equal at some time and this case occurs only when both X position is O, since consider the following case,
            //Start to resizing the header by mouse- At sometimes, when start to resizing, mouse is under starting point of next GridHeaderCellControl which is next to cuurent column that means it denotes X as 0
            //Stop to resizng the header by mouse- After stopped the resizing, Position of next GridHeaderCellControl has been shifted and sometimes it may have possiblity as mouse under the same starting point of that GridHeaderCellControl and it denotes the X as 0 too.
            //The situation will arise only in above cases and mouseup, mousedown event will be called for same next GridHeaderCellControl.
            //So changed condition as Math.Abs(gridMouseUp.X - gridMouseDown.X) >= 0 .
            //Other way of fixing this issue by checking mouse points of SfDataGrid or OrientedCellsPanel, but need to introduce some more variable for this case.
            if (this.CanResizeHiddenColumn() && this.DataGrid.ColumnResizingController.dragLine != null
                //WRT-4573 - While clicking on HeaderCell border automatically the Column width is changed to minimal value,
                //hence the MouseUp and Down position condition is added.
                && Math.Abs(gridMouseUp.X - gridMouseDown.X) >= 2)
#else
            if (this.CanResizeHiddenColumn() && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse 
                && this.DataGrid.ColumnResizingController.dragLine != null && Math.Abs(gridMouseUp.X - gridMouseDown.X) >= 2)
#endif
            {
                this.DataGrid.ColumnResizingController.DoActionOnMouseUp(e, this);
            }

            gridMouseDown = new Point(0, 0);
            if (this.DataGrid.ColumnResizingController.dragLine != null)
            {
                this.DataGrid.ColumnResizingController.dragLine = null;
#if WPF
                this.ReleaseMouseCapture();          
#endif
            }

            if (!this.DataGrid.ValidationHelper.CheckForValidation(false))
                return;
#if WPF
            if (Column.AllowSorting && this.DataGrid.SortClickAction == SortClickAction.SingleClick && !this.DataGrid.ColumnResizingController.isHovering && this.Cursor == Cursors.Arrow && isMouseLeftButtonPressed)
                Sort();
            isMouseLeftButtonPressed = false;
#endif
        }

        #endregion

        #region InitialUpdation For Sorting
        /// <summary>
        /// Makes Sure headercell property for updation
        /// </summary>
        /// <remarks>
        /// initial sort
        /// itemsource change checking
        /// </remarks>
        public void Update()
        {
            // While resuing header rows, if View is null, need to change FilterIconVisiblity as collapsed
            if (this.DataGrid != null && this.DataGrid.View == null)
                FilterIconVisiblity = Visibility.Collapsed;
            if (this.DataGrid != null && this.DataGrid.View != null)
            {
                #region SortIcon Visibility stuff
                if (this.DataGrid.View.SortDescriptions.Any(x => x.PropertyName == this.Column.MappingName))
                {
                    var sortColumn = this.DataGrid.View.SortDescriptions.FirstOrDefault(x => x.PropertyName == this.Column.MappingName);
                    var sortNumber = this.DataGrid.View.SortDescriptions.IndexOf(sortColumn) + 1;
                    this.SortDirection = sortColumn.Direction;
                    if (this.DataGrid.View.SortDescriptions.Count > 1 && this.DataGrid.ShowSortNumbers)
                    {
                        this.SortNumber = sortNumber.ToString();
                        this.SortNumberVisibility = Visibility.Visible;
                        //this.ColumnOptionsWidth = 25.0d;
                    }
                    else
                    {
                        this.ColumnOptionsWidth = 18.0d;
                        this.SortNumberVisibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    this.SortDirection = null;
                    this.SortNumber = string.Empty;
                    this.SortNumberVisibility = Visibility.Collapsed;
                }
                #endregion

                #region FilterIconVisibility Stuff

                if (this.DataGrid.CanSetAllowFilters(Column))
                    FilterIconVisiblity = Visibility.Visible;
                else
                    FilterIconVisiblity = Visibility.Collapsed;

                #endregion

                if (this.FilterPopupHost != null)
                {
                    this.FilterPopupHost.Column = this.Column;
                    this.FilterPopupHost.Update();
                }
                ApplyFilterToggleButtonVisualState();

            }
        }
        #endregion

        #region Sorting Module
        
        private void Sort()
        {
            if (this.DataGrid == null)
                return;

            this.DataGrid.RunWork(new Action(() =>
            {
                if (this.DataGrid != null)
                    this.DataGrid.GridModel.MakeSort(Column);
            }));           
        }

        private bool CanShowPopup()
        {
            return this.DataGrid.GridColumnDragDropController.CanShowPopup(this.Column);
        }

        private bool CanResizeColumn()
        {
            return this.DataGrid.ColumnResizingController.CanResizeColumn(this.Column);
        }

        internal bool CanResizeHiddenColumn()
        {
            var canResizeColumn = false;
            var isResizingandHidden = false;
            int rc;
            if (this.DataGrid == null)
                return canResizeColumn || isResizingandHidden;
            if (this.DataGrid.ColumnResizingController.dragLine != null)
            {
                var columnIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(this.DataGrid.ColumnResizingController.dragLine.LineIndex);
                if (columnIndex >= 0 && columnIndex <= this.DataGrid.Columns.Count)
                {
                    var column = this.DataGrid.Columns[columnIndex];
                    canResizeColumn = this.DataGrid.ColumnResizingController.CanResizeColumn(column);
                }
            }
            else
                canResizeColumn = this.CanResizeColumn();
            var lineIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(Column));
            if (lineIndex > 0 && lineIndex < this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.Count))
            {
                if (DataGrid.VisualContainer.ColumnWidths.GetHidden(lineIndex - 1, out rc))
                    isResizingandHidden = true;
            }           
         return canResizeColumn || isResizingandHidden;
        }
#endregion

#region Filtering Module

        /// <summary>
        /// Wires the excel like filtering events.
        /// </summary>
        void WireExcelLikeFilteringEvents()
        {
            if (FilterToggleButton != null)
            {
#if WPF
                FilterToggleButton.MouseEnter += OnFilterToggleButtonMouseEnter;
                FilterToggleButton.MouseLeave += OnFilterToggleButtonMouseLeave;
                FilterToggleButton.PreviewMouseDown += OnFilterToggleButtonPreviewMouseDown;
                FilterToggleButton.TouchDown += OnFilterToggleButtonTouchDown;
#endif
                FilterToggleButton.Click += OnFilterToggleButtonClick;

#if !WPF
                FilterToggleButton.Tapped += OnFilterToggleButtonTapped;
#endif
            }

            if (this.FilterPopupHost != null)
            {
                FilterPopupHost.Column = this.Column;
            }
        }

#if WPF
        void OnFilterToggleButtonTouchDown(object sender, TouchEventArgs e)
        {
            isFilterToggleButtonPressed = true;
        }
#endif

        private void OnFilterToggleButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ValidationHelper.SetFocusSetBack(false);
            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);
            if (!skipValidation && !DataGrid.ValidationHelper.CheckForValidation(true))
            {
                ValidationHelper.SetFocusSetBack(true);
                OpenFilterPopUp();
                e.Handled = true;
                return;
            }
        }
#if WPF
        void OnFilterToggleButtonMouseLeave(object sender, MouseEventArgs e)
        {
            if(this.FilterPopupHost != null && this.FilterPopupHost.FilterPopUp != null)
                this.FilterPopupHost.FilterPopUp.StaysOpen = !this.FilterPopupHost.FilterPopUp.StaysOpen; ;
        }

        void OnFilterToggleButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (this.FilterPopupHost != null && this.FilterPopupHost.FilterPopUp != null)
                this.FilterPopupHost.FilterPopUp.StaysOpen = !this.FilterPopupHost.FilterPopUp.StaysOpen;
        }

#endif
        /// <summary>
        /// UnWire the excel like filtering events.
        /// </summary>
        void UnWireExcelLikeFilteringEvents(bool candispose)
        {
            if (FilterToggleButton != null)
            {
#if WPF
                FilterToggleButton.MouseEnter -= OnFilterToggleButtonMouseEnter;
                FilterToggleButton.MouseLeave -= OnFilterToggleButtonMouseLeave;
                FilterToggleButton.PreviewMouseDown -= OnFilterToggleButtonPreviewMouseDown;
                FilterToggleButton.TouchDown -= OnFilterToggleButtonTouchDown;
#endif
                FilterToggleButton.Click -= OnFilterToggleButtonClick;
#if !WPF
                FilterToggleButton.Tapped -= OnFilterToggleButtonTapped;
#endif
                FilterToggleButton = null;
            }

            if (this.FilterPopupHost != null)
            {
                if (candispose)
                {
                    (FilterPopupHost as IDisposable).Dispose();
                    FilterPopupHost = null;
                }
                else
                    FilterPopupHost = null;
            }
        }
        
        /// <summary>
        /// FilterPredicates Collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFilterPredicatesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ApplyFilterToggleButtonVisualState();
        }

        protected internal virtual void ApplyFilterToggleButtonVisualState()
        {
            if (this.FilterToggleButton != null && this.Column != null)
                this.FilterToggleButton.toggleButtonVisualState = VisualStateManager.GoToState(this.FilterToggleButton, IsFilterApplied ? "Filtered" : "UnFiltered", true) ? string.Empty : (IsFilterApplied ? "Filtered" : "UnFiltered");
        }

        //WPF-35842:We made this method as virtual by providing the support to load any control while clicking FilterToggleButton
        protected virtual void OpenFilterPopUp()
        {
            this.DataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.FilterPopupOpening,
       new GridFilteringEventArgs(false)
       {
           IsProgrammatic = false
       }));


            if (FilterPopupHost == null)
            {
                FilterPopupHost = new GridFilterControl()
                {
                    Column = this.Column,
                    AdvancedFilterType = FilterHelpers.GetAdvancedFilterType(this.Column)
                };
                FilterPopUpPresenter.Child = FilterPopupHost;
                Binding binding = new Binding();
                binding.Source = FilterToggleButton;
                binding.Path = new PropertyPath("IsChecked");
                binding.Mode = BindingMode.TwoWay;
                FilterPopupHost.SetBinding(GridFilterControl.IsOpenProperty, binding);
            }
        }
        
        void OnFilterToggleButtonClick(object sender, RoutedEventArgs e)
        {
#if !WPF
            isFilterToggleButtonPressed = true;
            ValidationHelper.SetFocusSetBack(false);
            if (!this.DataGrid.ValidationHelper.CheckForValidation(true))
            {
                ValidationHelper.SetFocusSetBack(true);
                return;
            }
#endif
            OpenFilterPopUp();
#if WPF
            isFilterToggleButtonPressed = false;
#endif
#if !WPF
            var toggleBtnPoint = this.FilterToggleButton.TransformToVisual(this).TransformPoint(new Point(0, 0));
            var windowPoint = this.FilterToggleButton.TransformToVisual(null).TransformPoint(new Point(0, 0));
            Rect toggleRect = new Rect(toggleBtnPoint.X, toggleBtnPoint.Y, FilterToggleButton.ActualWidth, this.ActualHeight);
            if (FilterPopupHost != null)
                FilterPopupHost.SetPopupPosition(toggleRect, windowPoint, FilterToggleButton.Padding.Left, this.Padding);
#endif
        }

#if !WPF
        private void OnFilterToggleButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
#endif
#endregion

#if WPF
        private bool ShowPopup(Point currentPosition, TouchDevice touchDevice)
        {
            var rect = this.GetControlRect(this.DataGrid);
            if (Math.Abs(currentPosition.Y - mouseDownPoint.Y) >= 1 || Math.Abs(currentPosition.X - mouseDownPoint.X) >= 1)
            {
                if (this.DataGrid.GridColumnDragDropController != null)
                {
                    this.DataGrid.GridColumnDragDropController.ShowPopup(this.Column, rect, currentPosition, false, touchDevice);
                    return true;
                }
            }
            return false;
        }
#endif

        private void WireGridHeaderCellControlEvents()
        {
            this.Loaded += GridHeaderCellControlLoaded;
            this.Unloaded += GridHeaderCellControlUnloaded;
        }

        private void UnWireGridHeaderCellControlEvents()
        {
            this.Loaded -= GridHeaderCellControlLoaded;
            this.Unloaded -= GridHeaderCellControlUnloaded;
        }

#if WPF
        /// <summary>
        /// Occurs when any context menu on the element is opened.
        /// </summary>
        /// <param name="sender">The sender which contains SfDataGrid</param>
        /// <param name="e">Context menu event arguments</param>
        void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ShowContextMenu())
                e.Handled = true;
        }
#endif

        /// <summary>
        /// Disposes all the resources <see cref="Syncfusion.UI.Xaml.Grid.GridHeaderCellControl"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources <see cref="Syncfusion.UI.Xaml.Grid.GridHeaderCellControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            this.UnWireExcelLikeFilteringEvents(true);
            UnWireGridHeaderCellControlEvents();
            if (isDisposing)
            {
                this.DataGrid = null;
#if !WPF
                this.pointer = null;
#endif
            }
            isdisposed = true;
        }

#if WPF
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridHeaderCellControlAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
#endif
    }

    public class FilterToggleButton : ToggleButton
    {
        internal string toggleButtonVisualState;
        public FilterToggleButton()
        {
            this.DefaultStyleKey = typeof(FilterToggleButton);
        }

#if !WPF
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {

            base.OnApplyTemplate();
            if (!string.IsNullOrEmpty(toggleButtonVisualState))
                VisualStateManager.GoToState(this, toggleButtonVisualState, true);
        }
    }

    public sealed class GridStackedHeaderCellControl : ContentControl, IDisposable
    {
        public GridStackedHeaderCellControl()
        {
            this.DefaultStyleKey = typeof(GridStackedHeaderCellControl);
        }
        public void Dispose()
        {

        }

#if WPF
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridStackedHeaderCellControlAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
#endif
    }

    public class SortDirectionToWidthConverter : IValueConverter
    {
#if !WPF
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value == null)
                return 0;
            return 18;
        }

#if !WPF
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }

    }

    public class SortDirectionToVisibilityConverter : IValueConverter
    {
#if !WPF
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value == null)
                return Visibility.Collapsed;

            var direction = (ListSortDirection)value;
            if (parameter.ToString().Equals("Ascending"))
            {
                if (direction == ListSortDirection.Ascending)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            else
            {
                if (direction == ListSortDirection.Descending)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

#if !WPF
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }
}