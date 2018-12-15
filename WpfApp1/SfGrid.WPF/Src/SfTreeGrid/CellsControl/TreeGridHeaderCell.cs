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
using System.Diagnostics;
using Syncfusion.UI.Xaml.Grid;
#if UWP
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
using System.Threading.Tasks;
using Syncfusion.Data;
#else
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using MouseEventArgs = PointerRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using GestureEventArgs = HoldingRoutedEventArgs;
    using ManipulationStartedEventArgs = ManipulationStartedRoutedEventArgs;
#endif

    internal static class HelperExtensions
    {
#if UWP
        public static Point GetPosition(this PointerRoutedEventArgs e, UIElement relativeTo)
        {
            return e.GetCurrentPoint(relativeTo).Position;
        }
#endif

        public static Rect GetControlRect(this FrameworkElement control, UIElement relativeTo)
        {
#if UWP
            var point = control.TransformToVisual(relativeTo).TransformPoint(new Point(0, 0));
#else
            var point = control.TranslatePoint(new Point(0, 0), relativeTo);
#endif
            return new Rect(point.X, point.Y, control.ActualWidth, control.ActualHeight);
        }
    }

    public class TreeGridHeaderCell : ContentControl, IDisposable
    {

        #region Fields

        /// <summary>
        /// Flag used to skip sorting on mouse up once dragging started in OnMouseMove.
        /// </summary>
        internal bool isMouseLeftButtonPressed = false;
        private Point gridMouseDown;
#if WPF
        private Point mouseDownPoint;
        private TouchDevice touchDevice;
        /// <summary>
        /// Flag denotes whether mouse or touch triggered the dragging.
        /// </summary>
        internal bool isTouchPressed = false;
#else
        private Pointer pointer = null;
#endif

        #endregion

        #region Constructor

        public TreeGridHeaderCell()
        {
            this.DefaultStyleKey = typeof(TreeGridHeaderCell);
#if WPF
            this.IsManipulationEnabled = true;
#else
            this.IsHoldingEnabled = true;
            this.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
#endif
            this.IsTabStop = false;
        }

        #endregion

        /// <summary>
        /// Opens the context menu at the specified position.
        /// </summary>
        /// <param name="position">The position to display context menu.</param>
        /// <b>true</b> If the context menu opened;Otherwise<b>false</b>
        /// </returns>
#if UWP
        protected virtual internal bool ShowContextMenu(Point position)
#else
        protected virtual internal bool ShowContextMenu()
#endif
        {
            if (this.TreeGrid == null || this.TreeGrid.HeaderContextMenu == null)
                return false;

            var rowColIndex = new RowColumnIndex(TreeGrid.GetHeaderIndex(), TreeGrid.ResolveToScrollColumnIndex(TreeGrid.Columns.IndexOf(Column)));
            var menuinfo = new TreeGridColumnContextMenuInfo() { Column = this.Column, TreeGrid = this.TreeGrid };
            var args = new TreeGridContextMenuEventArgs(TreeGrid.HeaderContextMenu, menuinfo, rowColIndex, ContextMenuType.HeaderCell, this);
            if (!this.TreeGrid.RaiseTreeGridContextMenuEvent(args))
            {
#if WPF
                if (args.ContextMenuInfo != null)
                    this.TreeGrid.HeaderContextMenu.DataContext = args.ContextMenuInfo;
                this.TreeGrid.HeaderContextMenu.PlacementTarget = this;
                this.TreeGrid.HeaderContextMenu.IsOpen = true;
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

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            UnWireTreeGridHeaderCellControlEvents();
            WireTreeGridHeaderCellControlEvents();

            if (this.TreeGrid != null && this.TreeGrid.AllowResizingColumns && this.TreeGrid.AllowResizingHiddenColumns)
                this.TreeGrid.ColumnResizingController.ProcessResizeStateManager(this.Column);
#if WPF
            this.ContextMenuOpening += OnContextMenuOpening;
#endif
        }
#if WPF
        /// <summary>
        /// Occurs when any context menu on the element is opened.
        /// </summary>
        /// <param name="sender">The sender which contains SfTreeGrid</param>
        /// <param name="e">Context menu event arguments</param>
        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ShowContextMenu())
                e.Handled = true;
        }
#endif

        private void TreeGridHeaderCell_Unloaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void TreeGridHeaderCell_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.TreeGrid != null && this.TreeGrid.AllowResizingColumns && this.TreeGrid.AllowResizingHiddenColumns)
                this.TreeGrid.ColumnResizingController.ProcessResizeStateManager(this.Column);
        }

        private void WireTreeGridHeaderCellControlEvents()
        {
            this.Loaded += TreeGridHeaderCell_Loaded;
            this.Unloaded += TreeGridHeaderCell_Unloaded;
        }

        private void UnWireTreeGridHeaderCellControlEvents()
        {
            this.Loaded -= TreeGridHeaderCell_Loaded;
            this.Unloaded -= TreeGridHeaderCell_Unloaded;
        }
        public TreeGridColumn Column
        {
            get { return (TreeGridColumn)GetValue(ColumnProperty); }
            set { SetValue(ColumnProperty, value); }
        }

        public static readonly DependencyProperty ColumnProperty =
    DependencyProperty.Register("Column", typeof(TreeGridColumn), typeof(TreeGridHeaderCell), new PropertyMetadata(null));

#if WPF
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
#else
        protected override void OnPointerReleased(MouseButtonEventArgs e)
#endif
        {
            OnMouseLeftButtonReleased(e);
#if !WPF
            base.OnPointerReleased(e);
#else
            base.OnMouseLeftButtonUp(e);
#endif
        }

#if WPF
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
#else
        protected override void OnPointerPressed(MouseButtonEventArgs e)
#endif
        {
            if (this.TreeGrid == null)
                return;
            if (!TreeGrid.ValidationHelper.CheckForValidation(true))
            {
                e.Handled = true;
                return;
            }
            gridMouseDown = e.GetPosition(this.TreeGrid.TreeGridPanel);
#if UWP
            if (this.CanResizeHiddenColumn() && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse && this.TreeGrid.ColumnResizingController.isHovering && this.TreeGrid.ColumnResizingController.dragLine == null)
#else
            mouseDownPoint = e.GetPosition(this);
            isMouseLeftButtonPressed = true;
            if (CanResizeHiddenColumn() && this.TreeGrid.ColumnResizingController.isHovering)
#endif
            {
#if UWP
                double hScrollChange = 0;

                for (int i = 0; i < this.TreeGrid.TreeGridPanel.FrozenColumns; i++)
                {
                    hScrollChange += this.TreeGrid.TreeGridPanel.ColumnWidths[i];
                }
                var pointerPoint = new Point(Math.Abs(gridMouseDown.X - (TreeGrid.TreeGridPanel.HScrollBar.Value - hScrollChange)), Math.Abs(gridMouseDown.Y - TreeGrid.TreeGridPanel.VScrollBar.Value));
                var cursor = CoreCursorType.Arrow;
                this.TreeGrid.ColumnResizingController.dragLine = this.TreeGrid.ColumnResizingController.HitTest(pointerPoint, out cursor);
                if (cursor != CoreCursorType.Arrow)
                    this.TreeGrid.ColumnResizingController.SetPointerCursor(cursor);
#else
                var cursor = this.Cursor;
                this.TreeGrid.ColumnResizingController.dragLine = this.TreeGrid.ColumnResizingController.HitTest(gridMouseDown, out cursor);
                if (this.TreeGrid.ColumnResizingController.dragLine != null)
                    this.CaptureMouse();
                e.Handled = true;
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
#if UWP
            pointer = e.Pointer;
            base.OnPointerPressed(e);
#else
            base.OnMouseLeftButtonDown(e);
#endif
        }

#if WPF
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var newPosition = Math.Abs(e.GetPosition(this).X - mouseDownPoint.X);
            if (this.TreeGrid == null)
                return;
            if (CanResizeHiddenColumn() && newPosition > 0)
            {
                this.TreeGrid.ColumnResizingController.DoActionOnMouseMove(e.GetPosition(this.TreeGrid.TreeGridPanel), this);
            }

            if (!this.TreeGrid.ColumnResizingController.isHovering && isMouseLeftButtonPressed && this.Cursor == Cursors.Arrow)
            {
                if (this.Column.AllowDragging)
                {
                    if (ShowPopup(e.GetPosition(this), null))
                        isMouseLeftButtonPressed = false;
                    e.Handled = true;
                    return;
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            //  WPF-33940 - While resizing the column sometimes mouseup event is not fired, because mouse left button is release out side of the grid region.
            //in this case line resize index, drag line and lineresize size are not reset.To avoid this below method is added to process the OnMouseLeftButtonUp codes.
            OnMouseLeftButtonReleased(e);
            base.OnLostMouseCapture(e);
        }
        /// <summary>
        /// Method override to set tooltip for TreeGridHeaderCell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
            ShowToolTip();
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (this.TreeGrid != null)
            {
                isMouseLeftButtonPressed = false;
                if (this.Cursor == this.TreeGrid.ColumnResizingController.ResizingCursor && this.TreeGrid.ColumnResizingController.dragLine == null && !isMouseLeftButtonPressed)
                {
                    this.TreeGrid.ColumnResizingController.SetPointerCursor(Cursors.Arrow, this);
                }
            }
            VisualStateManager.GoToState(this, "Normal", true);
            base.OnMouseLeave(e);
        }
#endif

        /// <summary>
        /// To process entire Mouse left button released event task
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Input.MouseEventArgs">MouseEventArgs</see> that contains the event data.</param>
        private void OnMouseLeftButtonReleased(MouseEventArgs e)
        {
            if (this.TreeGrid == null)
            {
#if WPF
                isMouseLeftButtonPressed = false;
#endif
                return;
            }
            var gridMouseUp = e.GetPosition(this.TreeGrid.TreeGridPanel);
#if WPF
            if (e.StylusDevice != null)
                return;

            if (this.CanResizeHiddenColumn() && this.TreeGrid.ColumnResizingController.dragLine != null
                && Math.Abs(gridMouseUp.X - gridMouseDown.X) >= 2)
#else
            if (this.CanResizeHiddenColumn() && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse
                && this.TreeGrid.ColumnResizingController.dragLine != null && Math.Abs(gridMouseUp.X - gridMouseDown.X) >= 2)
#endif
            {
                this.TreeGrid.ColumnResizingController.DoActionOnMouseUp(gridMouseUp, this);
            }

            gridMouseDown = new Point(0, 0);
            if (this.TreeGrid.ColumnResizingController.dragLine != null)
            {
                this.TreeGrid.ColumnResizingController.dragLine = null;
#if WPF
                this.ReleaseMouseCapture();
#endif
            }

            if (!this.TreeGrid.ValidationHelper.CheckForValidation(false))
                return;
#if WPF
            if (Column.AllowSorting && this.TreeGrid.SortClickAction == SortClickAction.SingleClick && !this.TreeGrid.ColumnResizingController.isHovering && this.Cursor == Cursors.Arrow && isMouseLeftButtonPressed)
                this.TreeGrid.TreeGridModel.MakeSort(Column);
            isMouseLeftButtonPressed = false;
#endif
        }

        /// <summary>
        /// The method that used to set tooltip for TreeGridHeaderCell
        /// </summary>
        private void ShowToolTip()
        {
            if (Column == null || !Column.ShowHeaderToolTip)
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
            if (this.TreeGrid != null && this.TreeGrid.CanCellToolTipOpening())
            {
                var cellToolTipOpeningEventArgs = new TreeGridCellToolTipOpeningEventArgs(this)
                {
                    Column = this.Column,
                    RowColumnIndex = new RowColumnIndex(TreeGrid.GetHeaderIndex(), TreeGrid.ResolveToScrollColumnIndex(TreeGrid.Columns.IndexOf(Column))),
                    Record = this.DataContext,
                    ToolTip = tooltip
                };
                this.TreeGrid.RaiseCellToolTipOpeningEvent(cellToolTipOpeningEventArgs);
            }
            return tooltip.IsEnabled;
        }
#if UWP
        protected override void OnPointerExited(MouseButtonEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                if (this.TreeGrid == null || this.TreeGrid.TreeGridPanel == null)
                    return;

                PointerPoint pp = e.GetCurrentPoint(this.TreeGrid.TreeGridPanel);
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

        /// <summary>
        /// When Right click the SfTreeGrid Cell, Context menu appears for the selected cell. we are using this event for context menu support in Header cell.
        /// </summary>
        /// <param name="e">Right tapped event arguments</param>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            if (ShowContextMenu(position))
                e.Handled = true;
            base.OnRightTapped(e);
        }
        /// <summary>
        /// Method override to set tooltip for TreeGridHeaderCell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerEntered(MouseButtonEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                VisualStateManager.GoToState(this, "PointerOver", true);
            ShowToolTip();
            base.OnPointerEntered(e);
        }
        protected override void OnPointerMoved(MouseEventArgs e)
        {
            if (this.CanResizeHiddenColumn() && e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
            {
                this.TreeGrid.ColumnResizingController.DoActionOnMouseMove(e.GetCurrentPoint(this.TreeGrid.TreeGridPanel), this);
            }
            base.OnPointerMoved(e);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (this.TreeGrid == null)
                return;
            if (!this.TreeGrid.ValidationHelper.CheckForValidation(false))
                return;

            if (Column.AllowSorting &&
                this.TreeGrid.SortClickAction == SortClickAction.SingleClick && !this.TreeGrid.ColumnResizingController.isHovering)
                this.TreeGrid.TreeGridModel.MakeSort(Column);
            base.OnTapped(e);
        }

        protected override void OnManipulationStarting(ManipulationStartingRoutedEventArgs e)
        {
            if (this.TreeGrid.ColumnResizingController.dragLine != null)
                this.CapturePointer(pointer);
            base.OnManipulationStarting(e);
        }

        protected override void OnPointerCaptureLost(MouseButtonEventArgs e)
        {
            OnPointerReleased(e);
            if (this.TreeGrid.ColumnResizingController.isHovering)
                this.TreeGrid.ColumnResizingController.isHovering = false;
            if (this.TreeGrid.ColumnResizingController.dragLine != null)
            {
                this.TreeGrid.ColumnResizingController.dragLine = null;
                this.TreeGrid.ColumnResizingController.SetPointerCursor(CoreCursorType.Arrow);
            }
            if (this.TreeGrid.ColumnDragDropController.DraggablePopup != null && this.TreeGrid.ColumnDragDropController.DraggablePopup.IsOpen)
                this.TreeGrid.ColumnDragDropController.DraggablePopup.IsOpen = false;
            base.OnPointerCaptureLost(e);
        }

        protected override void OnHolding(GestureEventArgs e)
        {
            if (!this.TreeGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (e.HoldingState == HoldingState.Started && e.PointerDeviceType != PointerDeviceType.Mouse
                && pointer != null)
            {
                if (Column.AllowResizing)
                {
                    TreeGridColumn treeColumn = this.Column;
                    Point mouseDown = e.GetPosition(this);
                    int colIndex = this.TreeGrid.Columns.IndexOf(treeColumn);

                    int columnindex = this.TreeGrid.ResolveToScrollColumnIndex(colIndex);
                    var line = this.TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(columnindex);

                    var point = this.GetControlRect(this.TreeGrid);

                    //UWP-4063 Resizing popup size is varied when column width exceeds the total width of the grid.
                    if (line != null && line.IsClipped)
                    {
                        this.TreeGrid.TreeGridPanel.ScrollColumns.ScrollInView(columnindex);

                        var distance = treeColumn.ActualWidth - line.ClippedSize;

                        if (line.IsClippedOrigin)
                            point.X = point.X + distance;
                        else
                            point.X = point.X - distance;
                    }
                    Rect rect = Rect.Empty;
                    if ((mouseDown.X < 20 || mouseDown.X > this.ActualWidth - 20) && this.TreeGrid.AllowResizingHiddenColumns && colIndex > 0 && colIndex < this.TreeGrid.Columns.Count - 1)
                    {
                        int index = mouseDown.X < 20 ? colIndex - 1 : colIndex + 1;
                        TreeGridColumn hiddenColumn = this.TreeGrid.Columns[index];
                        if (hiddenColumn.IsHidden)
                        {
                            treeColumn = mouseDown.X < 20 ? hiddenColumn : this.TreeGrid.Columns[index];
                            if (mouseDown.X > this.ActualWidth - 20)
                            {
                                index = this.TreeGrid.Columns.IndexOf(treeColumn);
                                treeColumn = treeColumn.IsHidden ? this.TreeGrid.Columns[index] : this.TreeGrid.Columns[index - 1];
                                rect = new Rect(point.X + this.ActualWidth, point.Y, this.ActualWidth, this.ActualHeight);
                            }
                            this.TreeGrid.ColumnResizingController.HiddenLineIndex = this.TreeGrid.ResolveToScrollColumnIndex(this.TreeGrid.Columns.IndexOf(treeColumn));
                        }
                    }
                    if (rect.IsEmpty)
                        rect = new Rect(point.X, point.Y, this.ActualWidth, this.ActualHeight);
                    this.TreeGrid.ColumnDragDropController.ShowPopup(treeColumn, rect, mouseDown, true, pointer);
                    e.Handled = true;
                }
            }
            // When long press SfTreeGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Header Cell.
            if (e.HoldingState == HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                var position = e.GetPosition(this);
                if (ShowContextMenu(position))
                    e.Handled = true;
            }
            base.OnHolding(e);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            if (!this.TreeGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (this.TreeGrid.ColumnDragDropController == null)
                return;

            var device = pointer;
            if (device != null && !this.TreeGrid.ColumnResizingController.isHovering)
            {
                if (this.Column.AllowDragging)
                {
                    VisualStateManager.GoToState(this, "Normal", true);
                    var rect = this.GetControlRect(this.TreeGrid);
                    this.TreeGrid.ColumnDragDropController.ShowPopup(this.Column, rect, e.Position, false,
                                                          device);
                    e.Handled = true;
                    return;
                }
            }
            base.OnManipulationStarted(e);
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            if (!this.TreeGrid.ValidationHelper.CheckForValidation(false))
                return;
            if (Column.AllowSorting &&
                this.TreeGrid.SortClickAction == SortClickAction.DoubleClick)
                this.TreeGrid.TreeGridModel.MakeSort(Column);
            Point pp = e.GetPosition(this.TreeGrid.TreeGridPanel);
            var cursor = CoreCursorType.Arrow;
            var dragline = this.TreeGrid.ColumnResizingController.HitTest(pp, out cursor);
            if (cursor != CoreCursorType.Arrow)
                this.TreeGrid.ColumnResizingController.SetPointerCursor(cursor);
            if (Window.Current.CoreWindow.PointerPosition.X > 0 && Window.Current.CoreWindow.PointerPosition.Y > 0)
            {
                if (this.Column.AllowResizing && Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow && dragline != null)
                {
                    SetAutoWidth(dragline.LineIndex);
                }
                else
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            }
            base.OnDoubleTapped(e);
        }

#else
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            if (this.Column.AllowResizing)
            {
                this.TreeGrid.ColumnResizingController.DoActionOnMouseMove(touchDevice.GetTouchPoint(this.TreeGrid.TreeGridPanel).Position, this);
            }
            if (!this.TreeGrid.ColumnResizingController.isHovering && isTouchPressed && this.Cursor == Cursors.Arrow)
            {
                if (this.Column.AllowDragging)
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

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            var gridMouseUp = touchDevice.GetTouchPoint(this.TreeGrid.TreeGridPanel).Position;
            if (this.CanResizeHiddenColumn() && this.TreeGrid.ColumnResizingController.dragLine != null
                && Math.Abs(gridMouseUp.X - gridMouseDown.X) >= 2)
            {
                this.TreeGrid.ColumnResizingController.DoActionOnMouseUp(gridMouseUp, this);
            }
            this.ReleaseTouchCapture(touchDevice);
            base.OnManipulationCompleted(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (this.TreeGrid == null)
                return;
            if (!this.TreeGrid.ValidationHelper.CheckForValidation(false))
                return;
            if (Column.AllowSorting && this.TreeGrid.SortClickAction == SortClickAction.DoubleClick)
                this.TreeGrid.TreeGridModel.MakeSort(Column);
            Point pp = e.GetPosition(this.TreeGrid.TreeGridPanel);
            var cursor = this.Cursor;
            var dragline = this.TreeGrid.ColumnResizingController.HitTest(pp, out cursor);
            var indentColumnCount = this.TreeGrid.ResolveToScrollColumnIndex(0);
            if (Column.AllowResizing && cursor != Cursors.Arrow && dragline != null &&
                dragline.LineIndex >= indentColumnCount)
            {
                SetAutoWidth(dragline.LineIndex);
                e.Handled = true;
                this.ReleaseMouseCapture();
            }
        }


        protected override void OnTouchUp(TouchEventArgs e)
        {
            var gridMouseUp = e.GetTouchPoint(this.TreeGrid.TreeGridPanel).Position;
            if (this.TreeGrid.ColumnResizingController.dragLine != null && Math.Abs(gridMouseUp.X - gridMouseDown.X) <= 0)
                this.TreeGrid.ColumnResizingController.dragLine = null;
            if (Column.AllowSorting && this.TreeGrid.SortClickAction == SortClickAction.SingleClick && !this.TreeGrid.ColumnResizingController.isHovering && this.Cursor == Cursors.Arrow && isTouchPressed)
                this.TreeGrid.TreeGridModel.MakeSort(Column);
            isTouchPressed = false;
            base.OnTouchUp(e);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            touchDevice = e.TouchDevice;
            mouseDownPoint = e.GetTouchPoint(this).Position;
            gridMouseDown = e.GetTouchPoint(this.TreeGrid.TreeGridPanel).Position;
            if (this.Column.AllowResizing && this.TreeGrid.ColumnResizingController.isHovering)
            {
                var cursor = this.Cursor;
                this.TreeGrid.ColumnResizingController.dragLine = this.TreeGrid.ColumnResizingController.HitTest(gridMouseDown, out cursor);
                if (this.TreeGrid.ColumnResizingController.dragLine != null)
                    this.CaptureTouch(e.TouchDevice);
                e.Handled = true;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
            isTouchPressed = true;
            base.OnTouchDown(e);
        }

        private bool ShowPopup(Point currentposition, TouchDevice touchDevice)
        {
            if (this.TreeGrid.ColumnDragDropController == null)
                return false;

            var rect = this.GetControlRect(this.TreeGrid);
            if (Math.Abs(currentposition.Y - mouseDownPoint.Y) >= 1 || Math.Abs(currentposition.X - mouseDownPoint.X) >= 1)
            {
                this.TreeGrid.ColumnDragDropController.ShowPopup(this.Column, rect, currentposition, false, touchDevice);
                return true;
            }
            return false;
        }
#endif
        private void SetAutoWidth(int lineIndex)
        {
            var colIndex = this.TreeGrid.ResolveToGridVisibleColumnIndex(lineIndex);
            if (colIndex >= 0 && colIndex < this.TreeGrid.Columns.Count)
            {
                var column = TreeGrid.Columns[colIndex];
                var autoWidth = this.TreeGrid.TreeGridColumnSizer.CalculateAutoFitWidth(column);

                var args = new ResizingColumnsEventArgs(this.TreeGrid)
                {
                    ColumnIndex = lineIndex,
                    Width = autoWidth
                };
                if (!this.TreeGrid.RaiseResizingColumnsEvent(args))
                {
                    var width = args.Width;
                    if (column == this.TreeGrid.TreeGridColumnSizer.GetExpanderColumn())
                    {
                        width = TreeGrid.TreeGridColumnSizer.CalculateExpanderColumnWidth(column, width);
                    }
                    column.ActualWidth = width;
                    // after resizing, width should be set to refresh the column sizer
                    column.Width = args.Width;
                    TreeGrid.TreeGridPanel.ColumnWidths[lineIndex] = width;
                }
            }
        }

        protected internal SfTreeGrid TreeGrid { get; set; }

        public string SortNumber
        {
            get { return (string)this.GetValue(SortNumberProperty); }
            internal set { this.SetValue(SortNumberProperty, value); }
        }

        public static readonly DependencyProperty SortNumberProperty = DependencyProperty.Register("SortNumber", typeof(string), typeof(TreeGridHeaderCell), new PropertyMetadata(String.Empty));


        public Visibility SortNumberVisibility
        {
            get { return (Visibility)this.GetValue(SortNumberVisibilityProperty); }
            set { this.SetValue(SortNumberVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SortNumberVisibilityProperty =
            DependencyProperty.Register("SortNumberVisibility", typeof(Visibility), typeof(TreeGridHeaderCell),
                                        new PropertyMetadata(Visibility.Collapsed, null));


        public object SortDirection
        {
            get { return (object)this.GetValue(SortDirectionProperty); }
            set { this.SetValue(SortDirectionProperty, value); }
        }

        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register("SortDirection", typeof(object), typeof(TreeGridHeaderCell), new PropertyMetadata(null, OnSortDirectionChanged));

        private static void OnSortDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var header = obj as TreeGridHeaderCell;
            header.OnSortDirectionChanged(args);
        }

        protected virtual void OnSortDirectionChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }
        internal void Update()
        {
            if (this.TreeGrid == null || this.TreeGrid.View == null)
                return;

            var sortColumn = this.TreeGrid.SortColumnDescriptions.FirstOrDefault(x => x.ColumnName == this.Column.MappingName);
            if (sortColumn != null)
            {
                this.SortDirection = sortColumn.SortDirection;
                if (this.TreeGrid.ShowSortNumbers && this.TreeGrid.SortColumnDescriptions.Count > 1)
                {
                    var sortNumber = this.TreeGrid.SortColumnDescriptions.IndexOf(sortColumn) + 1;
                    this.SortNumber = sortNumber.ToString();
                    this.SortNumberVisibility = Visibility.Visible;
                }
                else
                {
                    this.SortNumberVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.SortDirection = null;
                this.SortNumber = string.Empty;
                this.SortNumberVisibility = Visibility.Collapsed;
            }
        }

        internal bool CanResizeHiddenColumn()
        {
            var canResizeColumn = false;
            var isResizingandHidden = false;
            int rc;
            if (this.TreeGrid == null)
                return canResizeColumn || isResizingandHidden;
            if (this.TreeGrid.ColumnResizingController.dragLine != null)
            {
                var columnIndex = this.TreeGrid.ResolveToGridVisibleColumnIndex(this.TreeGrid.ColumnResizingController.dragLine.LineIndex);
                if (columnIndex >= 0 && columnIndex <= this.TreeGrid.Columns.Count)
                {
                    var column = this.TreeGrid.Columns[columnIndex];
                    canResizeColumn = column.AllowResizing;
                }
            }
            else
                canResizeColumn = Column.AllowResizing;
            var lineIndex = this.TreeGrid.ResolveToScrollColumnIndex(this.TreeGrid.Columns.IndexOf(Column));
            if (lineIndex > 0 && lineIndex < this.TreeGrid.ResolveToScrollColumnIndex(this.TreeGrid.Columns.Count))
            {
                if (TreeGrid.TreeGridPanel.ColumnWidths.GetHidden(lineIndex - 1, out rc))
                    isResizingandHidden = true;
            }
            return canResizeColumn || isResizingandHidden;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridHeaderCell"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridHeaderCell"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            UnWireTreeGridHeaderCellControlEvents();
            TreeGrid = null;
            Column = null;
#if UWP
            pointer = null;
#else
            this.ContextMenuOpening -= OnContextMenuOpening;
#endif
        }
    }
}