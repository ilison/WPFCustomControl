#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data;
using System;
using System.Linq;
using Syncfusion.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
#if !WPF
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Data;
using System.Threading;
using Pointer = System.Object;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if !WPF
    using MouseEventArgs = PointerRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using Windows.UI.Input;
    using Windows.UI.Core;
    using System.Diagnostics;
#endif
    /// <summary>
    /// Provides the base implementation for column drag-and-drop operations in SfDataGrid.
    /// </summary>
    /// <remarks>
    /// It provides the set of public properties and virtual method to customize the drag-and-drop operation.
    /// </remarks>
    public class GridColumnDragDropController : IDisposable
    {
        #region Fields
        SfDataGrid dataGrid;
        #region DraggablePopup Fields
        private bool isPopupInitlized = false;
        private bool isdisposed = false;
        protected DispatcherTimer dpTimer = new DispatcherTimer();
        private Popup upIndicator;
        private Popup downIndicator;
        bool allowScrollOnHorizontalRightLimits = false;
        bool allowScrollOnHorizontalLeftLimits = false;
        private Point previousIndicatorPosition;
        private int dropedColIndex = 0;
        private double _PopupMinWidth = 40d;
        private double _PopupMaxWidth = 250d;
        private double _PopupMinHeight =
#if !WPF
                40d
#else
                20d
#endif
                ;
        private double _PopupMaxHeight =
#if !WPF
                60d
#else
                34d
#endif
                ;

#if !WPF
        private bool suspendReverseAnimationByColumnChooser;
#endif

        /// <summary>
        /// Gets the PopupContentControl for drag-and-drop operation.
        /// </summary>        
        protected PopupContentControl PopupContentControl
        {
            get { return PopupContent; }
        }
        internal PopupContentControl PopupContent;
        internal Popup DraggablePopup;

        internal VisibleLineInfo DragLeftLine;
        internal VisibleLineInfo DragRightLine;
        internal Storyboard reverseAnimationStoryboard;
        private EasingDoubleKeyFrame horizontalDoubleAnimation;
        private EasingDoubleKeyFrame verticalDoubleAnimation;
        internal double PreviousLeftLineSize;
        internal double PreviousRightLineSize;
        private double mouseHorizontalPosition;
        private double mouseVerticalPosition;
        internal bool needToUpdatePosition;
        internal bool isDragState;
        private bool isExpandedInternally;

        #endregion

        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class.
        /// </summary>
        /// <param name="dataGrid">The SfDataGrid.</param>
        public GridColumnDragDropController(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
            dpTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            InitializePopup();
        }
        #endregion

        #region Draggable popup

        #region Public properties

        /// <summary>
        /// Gets or sets the minimum width constraint of the popup for drag-and-drop operation.
        /// </summary>
        /// <value>
        /// The minimum width constraint of the popup.
        /// </value>        
        public double PopupMinWidth
        {
            get { return _PopupMinWidth; }
            set { _PopupMinWidth = value; }
        }

        /// <summary>
        /// Gets or sets the maximum width constraint of the popup for drag-and-drop operation.
        /// </summary>
        /// <value>
        /// The maximum width constraint of the popup.
        /// </value>  
        public double PopupMaxWidth
        {
            get { return _PopupMaxWidth; }
            set { _PopupMaxWidth = value; }
        }

        /// <summary>
        /// Gets or sets the minimum height constraint of the popup for drag-and-drop operation.
        /// </summary>
        /// <value>
        /// The minimum height constraint of the popup.
        /// </value>      
        public double PopupMinHeight
        {
            get { return _PopupMinHeight; }
            set { _PopupMinHeight = value; }
        }

        /// <summary>
        /// Gets or sets the maximum height constraint of the popup for drag-and-drop operation.
        /// </summary>
        /// <value>
        /// The maximum height constraint of the popup.
        /// </value>      
        public double PopupMaxHeight
        {
            get { return _PopupMaxHeight; }
            set { _PopupMaxHeight = value; }
        }
        #endregion

        #region internal Methods

        internal void ShowPopup(GridColumn gridColumn, Rect rect, Point adjustPoint, bool isInTouch,
                              object pointer)
        {
            ShowPopup(gridColumn, rect, isInTouch, adjustPoint, false, false, pointer);
        }

        internal void ShowPopup(GridColumn gridColumn, Rect rect, bool isInTouch, Point adjustPoint,
                              bool isDragFromGroupDropArea, bool isDragFromColumnChooser, object pointer)
        {

            if (!this.dataGrid.ValidationHelper.CheckForValidation(false))
                return;

            if (this.PopupContent == null)
                return;

#if !WPF
            if (this.DraggablePopup.IsOpen || pointer == null)
                return;

            this.PopupContent.CapturePointer(pointer as Pointer);
#endif
            if (this.DraggablePopup.IsOpen)
                return;

            if (!isInTouch)
            {
                var args = new QueryColumnDraggingEventArgs(this.dataGrid)
                {
                    From = this.dataGrid.Columns.IndexOf(gridColumn),
                    To = -1,
                    PopupPosition = new Point(rect.X, rect.Y),
                    Reason = QueryColumnDraggingReason.DragStarting
                };
                if (this.dataGrid.RaiseQueryColumnDragging(args))
                {
                    this.DraggablePopup.IsOpen = false;
                    upIndicator.IsOpen = false;
                    downIndicator.IsOpen = false;
                    return;
                }
            }

            if (CanResizeColumn(gridColumn))
            {
                //In the ShowPopUpmethod we have used the ColIndex variable to get the column index for the particular gridcolumn.
                //Based on this columnIndex we can check the Previous or next column is hidden or Not if it is hidden we can set the DragLeftLine.
                int colIndex = this.dataGrid.Columns.IndexOf(gridColumn);
                int index = this.dataGrid.ResolveToScrollColumnIndex(colIndex);
                this.DragRightLine = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(index);
                this.DragLeftLine =
                    this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(index - 1);

                //Here Checked the DragLine Value and Previous column is Hidden or because while dragging the Line the Previous Column has to be dragged.
                //When the don't have the RowHeader and IndentCell the DragLeftLine will be null.
                //So that it will try to get the Previous columnIndex value. But here the columnIndex value is zero.
                if (this.DragLeftLine == null && colIndex > 0 && this.dataGrid.Columns[colIndex - 1].IsHidden)
                {
                    index = this.dataGrid.ResolveToScrollColumnIndex(this.dataGrid.GetPreviousFocusGridColumnIndex(colIndex - 1));
                    this.DragLeftLine = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(index);
                }
                if (this.DragRightLine != null)
                    this.PreviousRightLineSize = this.DragRightLine.Size;
                if (this.DragLeftLine != null)
                    this.PreviousLeftLineSize = this.DragLeftLine.Size;
                var indentCellCount = this.dataGrid.ResolveToScrollColumnIndex(0);

                if (isInTouch && !isDragFromGroupDropArea)
                {
                    if (DragLeftLine != null && this.dataGrid.View != null &&
                        (DragLeftLine.LineIndex < this.dataGrid.View.GroupDescriptions.Count || DragLeftLine == null || DragLeftLine.LineIndex < indentCellCount))
                        this.PopupContent.LeftResizeThumbVisibility = Visibility.Collapsed;
                    else
                        this.PopupContent.LeftResizeThumbVisibility = Visibility.Visible;

                    if (DragRightLine != null && this.dataGrid.View != null &&
                        (DragRightLine.LineIndex < this.dataGrid.View.GroupDescriptions.Count))
                        this.PopupContent.RightResizeThumbVisibility = Visibility.Collapsed;
                    else
                        this.PopupContent.RightResizeThumbVisibility = Visibility.Visible;
                }
                else
                {
                    this.PopupContent.LeftResizeThumbVisibility = Visibility.Collapsed;
                    this.PopupContent.RightResizeThumbVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.PopupContent.LeftResizeThumbVisibility = Visibility.Collapsed;
                this.PopupContent.RightResizeThumbVisibility = Visibility.Collapsed;
            }
            this.PopupContent.IsDragFromGroupDropArea = isDragFromGroupDropArea;
            this.PopupContent.IsDragFromColumnChooser = isDragFromColumnChooser;
            this.PopupContent.InitialRect = rect;
            mouseHorizontalPosition = adjustPoint.X;
            mouseVerticalPosition = adjustPoint.Y;
            this.PopupContent.IsOpenInTouch = isInTouch;
            this.PopupContent.Content = CreatePopupContent(gridColumn);
            if (!double.IsNaN(gridColumn.MinimumWidth))
                this.PopupContent.MinWidth = gridColumn.MinimumWidth;
            else
                this.PopupContent.MinWidth = this.PopupMinWidth;
            if (!double.IsNaN(gridColumn.MaximumWidth))
                this.PopupContent.MaxWidth = gridColumn.MaximumWidth + this.PopupContent.ThumbWidth;
            else
            {
                if (this.PopupContent.LeftResizeThumbVisibility == Visibility.Visible || this.PopupContent.RightResizeThumbVisibility == Visibility.Visible)
                    this.PopupContent.MaxWidth = double.PositiveInfinity;
                else
                    this.PopupContent.MaxWidth = this.PopupMaxWidth;
            }
            this.PopupContent.MinHeight = PopupMinHeight;
            this.PopupContent.MaxHeight = PopupMaxHeight;
            //Set the Pop-Up Content width for the Hidded column. Which has to be used to Drag the Hidden column.
            //Added the Condition to check the drag is from the ColumnHeader or none other Area.
            //Because to show the Minimum popup while resizing the hidden column.
            if (gridColumn.IsHidden && !isDragFromGroupDropArea && !isDragFromColumnChooser)
                this.PopupContent.Width = this.PopupMinWidth;
            else
                this.PopupContent.Width = rect.Width + this.PopupContent.ThumbWidth;
            this.PopupContent.Height = rect.Height;
            this.PopupContent.Tag = gridColumn;
            var popupActualWidth = 0d;
            var popupActualHeight = 0d;
            if (PopupContent.Width > this.PopupContent.MaxWidth || this.PopupContent.Width < this.PopupContent.MinWidth)
            {
                if (PopupContent.Width > PopupContent.MaxWidth)
                    popupActualWidth = PopupContent.MaxWidth;
                if (PopupContent.Width < PopupContent.MinWidth)
                    popupActualWidth = PopupContent.MinWidth;
            }
            else
                popupActualWidth = this.PopupContent.Width;

            if (PopupContent.Height > this.PopupContent.MaxHeight || this.PopupContent.Height < this.PopupContent.MinHeight)
            {
                if (PopupContent.Height > PopupContent.MaxHeight)
                    popupActualHeight = PopupContent.MaxHeight;
                if (PopupContent.Height < PopupContent.MinHeight)
                    popupActualHeight = PopupContent.MinHeight;
            }
            else
                popupActualHeight = this.PopupContent.Height;
#if WPF
            var point = this.dataGrid.PointToScreen(new Point(rect.X, rect.Y));
            var popupPositionRect = new Rect(point.X - this.PopupContent.ThumbWidth / 2, point.Y, rect.Width,
                                             rect.Height);
            if (popupActualWidth != PopupContent.Width && (rect.X + popupActualWidth) < (mouseHorizontalPosition + popupPositionRect.X))
            {
                popupPositionRect.X = mouseHorizontalPosition + popupPositionRect.X - (popupActualWidth / 2);
                popupPositionRect.Width = popupActualWidth;
                mouseHorizontalPosition = popupActualWidth / 2;
            }

            if (popupActualHeight != PopupContent.Width && (rect.Y + popupActualHeight) < (mouseVerticalPosition + popupPositionRect.Y))
            {
                popupPositionRect.Y = mouseVerticalPosition + popupPositionRect.Y - (popupActualHeight / 2);
                popupPositionRect.Height = popupActualHeight;
                mouseVerticalPosition = popupActualHeight / 2;
            }
            this.DraggablePopup.PlacementRectangle = popupPositionRect;
#else
            //Need to convert the values to its related screen co-ordinates
            var point = this.dataGrid.TransformToVisual(null).TransformPoint(new Point(rect.X, rect.Y));
            rect.X = point.X;
            rect.Y = point.Y;

            //Changed the condition with this.popupContent.InitialRect.Width instead of rect.X because while pressing the column the popup 
            //shows with Deviation. Here we have set the pop-up to the Rect so it will shows Exactly with the GridColumn.
            var horizontalOffset = rect.X - (((this.PopupContent.ThumbWidth / 2) + ((this.PopupContent.InitialRect.Width * 0.05) / 2)));
            if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft && !gridColumn.IsHidden)
                horizontalOffset -= rect.Width;
            if (popupActualWidth != PopupContent.Width && (rect.X + popupActualWidth) < (mouseHorizontalPosition + horizontalOffset))
            {
                horizontalOffset = mouseHorizontalPosition + horizontalOffset - (popupActualWidth / 2);
                mouseHorizontalPosition = (popupActualWidth / 2);
            }
            var verticalOffset = rect.Y - ((rect.Height * 0.05)) / 2;
            if (popupActualHeight != PopupContent.Width && (rect.Y + popupActualHeight) < (mouseVerticalPosition + verticalOffset))
            {
                verticalOffset = mouseVerticalPosition + verticalOffset - (popupActualHeight / 2);
                mouseVerticalPosition = (popupActualHeight / 2);
            }
            this.DraggablePopup.HorizontalOffset = Math.Round(horizontalOffset);
            this.DraggablePopup.VerticalOffset = Math.Round(verticalOffset);
#endif
            this.dataGrid.VisualContainer.SuspendManipulationScroll = true;
            if (!isDragFromGroupDropArea)
                UpdateReverseAnimationOffset(this.DraggablePopup.HorizontalOffset,
                                             this.DraggablePopup.VerticalOffset, false);
            this.PopupContent.ApplyState(true);
            needToUpdatePosition = false;
            this.DraggablePopup.Opacity = 1;
            if (!isInTouch)
            {
                var args = new QueryColumnDraggingEventArgs(this.dataGrid);
                args.From = dataGrid.Columns.IndexOf(gridColumn);
                args.To = -1;
                args.Reason = QueryColumnDraggingReason.DragStarted;
                args.PopupPosition = new Point(rect.X, rect.Y);
                if (this.dataGrid.RaiseQueryColumnDragging(args))
                    return;
            }
            this.DraggablePopup.IsOpen = true;
            VisualStateManager.GoToState(this.PopupContent, "Valid", true);
#if WPF
            // WPF-32058 - Change DraggablePopup height and width based on the Popupcontentcontrol height and width.
            this.DraggablePopup.Height = PopupContent.ActualHeight + PopupContent.DragIndicator.Height;
            this.DraggablePopup.Width = PopupContent.ActualWidth + PopupContent.DragIndicator.Width;

            // When creating two windows the second created window does not perform drag and drop if first window is closed.
            DraggablePopup.PlacementTarget = this.dataGrid;
            if (pointer != null)
            {
                var touch = pointer as TouchDevice;
                this.PopupContent.CaptureTouch(touch);
            }
            else
                this.PopupContent.CaptureMouse();
#endif
        }

        internal void UpdatePopupPosition()
        {
            if (!needToUpdatePosition || !this.DraggablePopup.IsOpen)
                return;

            if (this.PopupContent.Tag != null && this.PopupContent.Tag is GridColumn)
            {
                var gridcolumn = this.PopupContent.Tag as GridColumn;
                var index = this.dataGrid.Columns.IndexOf(gridcolumn);
                //Checked the Condition to show the popup at correct position. if it is hidden column the popup 
                //will show with the next column leftDragLine.
                if (gridcolumn.IsHidden)
                    index = this.dataGrid.GetNextFocusGridColumnIndex(index, this.dataGrid.FlowDirection);
                SetPopupPosition(this.dataGrid.ResolveToScrollColumnIndex(index));
                needToUpdatePosition = false;
            }        
        }

        internal void UnWireEvents()
        {
            if (DraggablePopup != null)
                DraggablePopup.Closed -= OnDraggablePopupClosed;
            if (reverseAnimationStoryboard != null)
                reverseAnimationStoryboard.Completed -= OnReverseAnimationStoryboardCompleted;
        }

        internal void ColumnHiddenChanged(GridColumn column)
        {
            this.OnColumnHiddenChanged(column);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Initialize the Draggable popup
        /// </summary>
        /// <remarks></remarks>
        private void InitializePopup()
        {
            if (isPopupInitlized)
                return;

            if (DraggablePopup == null)
                DraggablePopup = new Popup();

            if (upIndicator == null)
                upIndicator = new Popup();

            if (downIndicator == null)
                downIndicator = new Popup();
#if !WPF
            DraggablePopup.IsLightDismissEnabled = true;
            upIndicator.IsLightDismissEnabled = true;
            downIndicator.IsLightDismissEnabled = true;
#else
            DraggablePopup.Placement = PlacementMode.Absolute;
            DraggablePopup.StaysOpen = false;
            DraggablePopup.AllowsTransparency = true;
            upIndicator.Placement = PlacementMode.Absolute;
            upIndicator.StaysOpen = false;
            upIndicator.AllowsTransparency = true;
            downIndicator.Placement = PlacementMode.Absolute;
            downIndicator.StaysOpen = false;
            downIndicator.AllowsTransparency = true;
#endif
            PopupContent = new PopupContentControl(this.dataGrid)
            {
                PopupContentPositionChanged = OnPopupContentPositionChanged,
                PopupContentDropped = OnPopupContentDropped,
                PopupContentResizing = this.dataGrid.ColumnResizingController.OnPopupContentResizing,
                PopupContentResized = this.dataGrid.ColumnResizingController.OnPopupContentResized
            };
            DraggablePopup.HorizontalAlignment = HorizontalAlignment.Center;
            DraggablePopup.VerticalAlignment = VerticalAlignment.Center;
            DraggablePopup.Child = PopupContent;
            CreateReverseAnimationStoryboard();
            DraggablePopup.Closed += OnDraggablePopupClosed;

            if (upIndicator.Child == null)
                upIndicator.Child = new UpIndicatorContentControl();

            if (downIndicator.Child == null)
                downIndicator.Child = new DownIndicatorContentControl();

            dpTimer.Tick += OnDispatcherTimerTick;
            DraggablePopup.FlowDirection = this.dataGrid.FlowDirection;
        }
        
        private void CreateReverseAnimationStoryboard()
        {
            var duration = new TimeSpan(0, 0, 0, 0, 200);
            horizontalDoubleAnimation = new EasingDoubleKeyFrame();
            verticalDoubleAnimation = new EasingDoubleKeyFrame();
            horizontalDoubleAnimation.KeyTime = KeyTime.FromTimeSpan(duration);
            verticalDoubleAnimation.KeyTime = KeyTime.FromTimeSpan(duration);
            horizontalDoubleAnimation.EasingFunction = new CircleEase();
            verticalDoubleAnimation.EasingFunction = new CircleEase();
            DoubleAnimationUsingKeyFrames horizontalAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames verticalAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
#if !WPF
            horizontalAnimationUsingKeyFrames.EnableDependentAnimation = true;
            verticalAnimationUsingKeyFrames.EnableDependentAnimation = true;
#endif
            horizontalAnimationUsingKeyFrames.KeyFrames.Add(horizontalDoubleAnimation);
            verticalAnimationUsingKeyFrames.KeyFrames.Add(verticalDoubleAnimation);
            reverseAnimationStoryboard = new Storyboard { Duration = duration };
            reverseAnimationStoryboard.Children.Add(horizontalAnimationUsingKeyFrames);
            reverseAnimationStoryboard.Children.Add(verticalAnimationUsingKeyFrames);
            Storyboard.SetTarget(horizontalAnimationUsingKeyFrames, this.DraggablePopup);
            Storyboard.SetTarget(verticalAnimationUsingKeyFrames, this.DraggablePopup);
#if !WPF
            Storyboard.SetTargetProperty(horizontalAnimationUsingKeyFrames, "HorizontalOffset");
            Storyboard.SetTargetProperty(verticalAnimationUsingKeyFrames, "VerticalOffset");
#else
            Storyboard.SetTargetProperty(horizontalAnimationUsingKeyFrames, new PropertyPath("HorizontalOffset"));
            Storyboard.SetTargetProperty(verticalAnimationUsingKeyFrames, new PropertyPath("VerticalOffset"));
#endif
            reverseAnimationStoryboard.Completed += OnReverseAnimationStoryboardCompleted;
            this.DraggablePopup.Resources.Add("storyboard", reverseAnimationStoryboard);
            isPopupInitlized = true;
        }

        private void OnReverseAnimationStoryboardCompleted(object sender, object e)
        {
            if (this.PopupContent.IsOpenInTouch && !this.PopupContent.IsDragFromGroupDropArea)
            {
                this.PopupContent.ApplyState(true);
                var gridColumn = this.PopupContent.Tag as GridColumn;
                if (CanResizeColumn(gridColumn))
                {
                    if (DragLeftLine != null && this.dataGrid.View != null && DragLeftLine.LineIndex < this.dataGrid.View.GroupDescriptions.Count)
                        this.PopupContent.LeftResizeThumbVisibility = Visibility.Collapsed;
                    else
                        this.PopupContent.LeftResizeThumbVisibility = Visibility.Visible;

                    if (DragRightLine != null && this.dataGrid.View != null && DragRightLine.LineIndex < this.dataGrid.View.GroupDescriptions.Count)
                        this.PopupContent.RightResizeThumbVisibility = Visibility.Collapsed;
                    else
                        this.PopupContent.RightResizeThumbVisibility = Visibility.Visible;
                }
                this.PopupContent.IsOpenInTouch = false;
            }
            else
            {
                this.DraggablePopup.IsOpen = false;
#if !WPF
                this.dataGrid.Focus(FocusState.Programmatic);
#else
                this.dataGrid.Focus();
#endif
            }
            this.PopupContent.ResetMousePosition();
            this.dataGrid.VisualContainer.SuspendManipulationScroll = false;
            this.DraggablePopup.ClearValue(Popup.HorizontalOffsetProperty);
            this.DraggablePopup.ClearValue(Popup.VerticalOffsetProperty);
        }

        private void UpdateReverseAnimationOffset(double horizontalOffset, double verticalOffset, bool canIncrement)
        {
            if (canIncrement)
            {
                this.horizontalDoubleAnimation.Value += horizontalOffset;
                this.verticalDoubleAnimation.Value += verticalOffset;
            }
            else
            {
                this.horizontalDoubleAnimation.Value = horizontalOffset;
                this.verticalDoubleAnimation.Value = verticalOffset;
            }
        }

        /// <summary>
        /// Calls on Popup closed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void OnDraggablePopupClosed(object sender, object e)
        {
            isDragState = false;
            if (this.isExpandedInternally)
            {
                this.dataGrid.GroupDropArea.IsExpanded = false;
                this.isExpandedInternally = false;
            }
            this.PopupContent.ApplyState(false);
            this.PopupContent.ResetMousePosition();
            this.dataGrid.VisualContainer.SuspendManipulationScroll = false;
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
        /// Indicates whether the mouse point is hover on the DataGrid.
        /// </param>
        protected virtual void OnPopupContentPositionChanged(double HorizontalDelta, double VerticalDelta, Point mousePointOverGrid)
        {
            if (this.DraggablePopup.IsOpen)
            {
                if (!isDragState && HorizontalDelta != 0 && VerticalDelta != 0)
                {
                    this.PopupContent.ApplyState(false);
                    isDragState = true;
                    if (this.dataGrid.GroupDropArea != null && !this.dataGrid.GroupDropArea.IsExpanded)
                    {
                        this.dataGrid.GroupDropArea.IsExpanded = true;
                        this.isExpandedInternally = true;
                    }
                }
#if WPF
                var point = this.dataGrid.PointToScreen(new Point(HorizontalDelta, VerticalDelta));
                this.DraggablePopup.PlacementRectangle = new Rect(point.X - mouseHorizontalPosition,
                                                                  point.Y - mouseVerticalPosition,
                                                                  this.PopupContent.ActualWidth,
                                                                  this.PopupContent.ActualHeight);
#else
                //Need to convert the values related to its screen coordinates
                var point = this.dataGrid.TransformToVisual(null).TransformPoint(new Point(HorizontalDelta, VerticalDelta));
                this.DraggablePopup.HorizontalOffset = point.X - mouseHorizontalPosition;
                this.DraggablePopup.VerticalOffset = point.Y - mouseVerticalPosition;
#endif
                var gridRect = this.dataGrid.GetControlRect(this.dataGrid);

                //UWP-3341 - Considered the FreezePanes and FlowDirection for autoscroll while dragging the column
                double frozenColumnValue = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ?
                                    gridRect.Right - this.dataGrid.VisualContainer.ScrollColumns.HeaderExtent :
                                    gridRect.Left + this.dataGrid.VisualContainer.ScrollColumns.HeaderExtent;
                double footerColumnValue = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ?
                                    gridRect.Left + this.dataGrid.VisualContainer.ScrollColumns.FooterExtent :
                                    gridRect.Right - this.dataGrid.VisualContainer.ScrollColumns.FooterExtent;

                bool isRightRect;
                bool isLeftRect;

                bool isFrozenPoint = mousePointOverGrid.X >= frozenColumnValue - 20 && mousePointOverGrid.X <= frozenColumnValue + 20;
                bool isFooterPoint = mousePointOverGrid.X >= footerColumnValue - 20 && mousePointOverGrid.X <= footerColumnValue + 20;
                
                if(this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
                {
                    isRightRect = frozenColumnValue - 20 < mousePointOverGrid.X;
                    isLeftRect = footerColumnValue + 20 > mousePointOverGrid.X;
                }
                else
                {
                    isRightRect = footerColumnValue - 20 < mousePointOverGrid.X;
                    isLeftRect = frozenColumnValue + 20 > mousePointOverGrid.X;
                }

                if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
                {
                    allowScrollOnHorizontalLeftLimits = (this.dataGrid.FooterColumnCount > 0) ? isFrozenPoint : isRightRect;
                    allowScrollOnHorizontalRightLimits = (this.dataGrid.FrozenColumnCount > 0) ? isFooterPoint : isLeftRect;
                }
                else
                {
                    allowScrollOnHorizontalRightLimits = (this.dataGrid.FooterColumnCount > 0) ? isFooterPoint : isRightRect;
                    allowScrollOnHorizontalLeftLimits = (this.dataGrid.FrozenColumnCount > 0) ? isFrozenPoint : isLeftRect;
                }

                bool isTimerRunning = false;
                if (gridRect.Contains(mousePointOverGrid) && GetHeaderRowRect().Bottom +5 > mousePointOverGrid.Y)
                {
                    this.ShowDragIndication(gridRect, mousePointOverGrid);
                    //Separate popup's view to show whether the popup can be able to drop in specific index or not
                    if (upIndicator.IsOpen && downIndicator.IsOpen)
                        VisualStateManager.GoToState(this.PopupContent, "Valid", true);
                    else
                        VisualStateManager.GoToState(this.PopupContent, "InValid", true);
                }
                else
                {
                    this.CloseDragIndication();
                    VisualStateManager.GoToState(this.PopupContent, "InValid", true);
                }

                if (allowScrollOnHorizontalRightLimits || allowScrollOnHorizontalLeftLimits && !isTimerRunning)
                {
                    //UWP-722 Need to calling start() method only if timer is not enabled already
                    if (!dpTimer.IsEnabled)
                        dpTimer.Start();
                    isTimerRunning = true;
                }
                else
                {
                    dpTimer.Stop();
                    isTimerRunning = false;
                }
            }
        }

        private void OnDispatcherTimerTick(object sender, object e)
        {
            var linesCollection = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
            if (allowScrollOnHorizontalRightLimits)
            {
                var rightLastColumn =this.dataGrid.FlowDirection==FlowDirection.RightToLeft?linesCollection[linesCollection.FirstBodyVisibleIndex]: linesCollection[linesCollection.LastBodyVisibleIndex];
                if (this.dataGrid.VisualContainer.ColumnCount >= rightLastColumn.LineIndex + 1)
                {
                    //WPF-23066 avoid scrollindex is -1  to check rightLastColumn.LineIndex value before it minus 1
                    int scollIndex = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ? ((rightLastColumn.LineIndex < 1) ? 0 : rightLastColumn.LineIndex - 1) : rightLastColumn.LineIndex + 1;
                    int count;
                    while (this.dataGrid.VisualContainer.ColumnWidths.GetHidden(scollIndex, out count))
                    {
                        scollIndex += 1;
                        if (scollIndex + 1 > this.dataGrid.VisualContainer.ColumnCount)
                        {
                            scollIndex = this.dataGrid.VisualContainer.ColumnCount - 1;
                            break;
                        }
                    }
                    this.dataGrid.VisualContainer.ScrollColumns.ScrollInView(scollIndex);
                    this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                }
            }
            else if (allowScrollOnHorizontalLeftLimits && linesCollection.Count > linesCollection.FirstBodyVisibleIndex)
            {                
                var leftLastColumn =this.dataGrid.FlowDirection==FlowDirection.RightToLeft? linesCollection[linesCollection.LastBodyVisibleIndex]: linesCollection[linesCollection.FirstBodyVisibleIndex];             
                // WPF-33923 - Set the scrollindex value as zero whether the LeftLastColumn is partially visible , when dragging the other column into first position.
                // So, that the visualcontainer scrolling horizontally towards left side.                                
                if (leftLastColumn.LineIndex >= 0)
                {
                    int scollIndex = -1;
                    if (leftLastColumn.LineIndex - 1 < 0)
                        scollIndex = 0;
                    else
                        scollIndex = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ? leftLastColumn.LineIndex + 1 : leftLastColumn.LineIndex - 1;
                    int count;
                    while (this.dataGrid.VisualContainer.ColumnWidths.GetHidden(scollIndex, out count))
                    {
                        scollIndex -= 1;
                        if (scollIndex - 1 < 0)
                        {
                            scollIndex = 0;
                            break;
                        }
                    }
                    this.dataGrid.VisualContainer.ScrollColumns.ScrollInView(scollIndex);
                    this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                }
            }
        }

        /// <summary>
        /// Calls for show drag indicator for popup
        /// </summary>
        /// <param name="gridRect"></param>
        /// <param name="mousePointOverGrid"></param>
        /// <remarks></remarks>
        private void ShowDragIndication(Rect gridRect, Point mousePointOverGrid)
        {
            if (upIndicator != null && downIndicator != null)
            {
                dropedColIndex = 0;
                bool canShowIndicator = false;
                bool isPointerInGroupArea;
                var headerRowRect = GetHeaderRowRect();
                int indicatorColumnIndex;
                var column = this.PopupContent.Tag as GridColumn;
                var point = this.GetArrowIndicatorLocation(gridRect, column, mousePointOverGrid, out isPointerInGroupArea, out canShowIndicator, out indicatorColumnIndex);

                if (canShowIndicator && !isPointerInGroupArea && !previousIndicatorPosition.Equals(point))
                {
                    var args = new QueryColumnDraggingEventArgs(this.dataGrid)
                    {
                        From = this.dataGrid.Columns.IndexOf(column),
                        To = indicatorColumnIndex,
                        PopupPosition = mousePointOverGrid,
                        Reason = QueryColumnDraggingReason.Dragging
                    };
                    if (this.dataGrid.RaiseQueryColumnDragging(args))
                    {
                        upIndicator.IsOpen = false;
                        downIndicator.IsOpen = false;
                        return;
                    }
                }
                previousIndicatorPosition = point;
                double VOffsetForUpIndicator, VOffsetForDownIndicator, HOffset = point.X;
                if (isPointerInGroupArea)
                {
                    var adjustValue = this.dataGrid.GroupDropArea.groupItemsGrid.ActualHeight / 2;
                    VOffsetForUpIndicator = point.Y + adjustValue;
                    VOffsetForDownIndicator = point.Y - adjustValue - (downIndicator.Child as ContentControl).ActualHeight;
                }
                else
                {
#if WPF
                    VOffsetForUpIndicator = headerRowRect.Y + headerRowRect.Height;
                    VOffsetForDownIndicator = headerRowRect.Y - (downIndicator.Child as ContentControl).ActualHeight;
#else
                    //UWP-1818 indicator position should be shown in inner header to avoid hidding the downIndicator in title bar view
                    VOffsetForUpIndicator = headerRowRect.Y + headerRowRect.Height -  (upIndicator.Child as ContentControl).ActualHeight;
                    VOffsetForDownIndicator = headerRowRect.Y;
#endif
                }
                var upIndicatorContent = upIndicator.Child as UpIndicatorContentControl;
                var downIndicatorContent = downIndicator.Child as DownIndicatorContentControl;

                if (!canShowIndicator)
                    this.CloseDragIndication();
                else
                {
#if WPF
                    // When creating two windows the second created window does not show the Up and Down indicator if first window is closed.
                    upIndicator.PlacementTarget = this.dataGrid;
                    downIndicator.PlacementTarget = this.dataGrid;
#endif
                    upIndicator.IsOpen = true;
                    downIndicator.IsOpen = true;
#if !WPF
                    upIndicatorContent.IsOpen = true;
                    downIndicatorContent.IsOpen = true;
#else
                    VisualStateManager.GoToState(upIndicatorContent, "Open", false);
                    VisualStateManager.GoToState(downIndicatorContent, "Open", false);
#endif
                }
#if WPF
                //Need to convert the calculated values into related screen coordinates to show drag indicators in correct position
                var pt = this.dataGrid.PointToScreen(new Point(HOffset, VOffsetForUpIndicator));
                var pt1 = this.dataGrid.PointToScreen(new Point(HOffset, VOffsetForDownIndicator));

                HOffset = pt.X;
                VOffsetForUpIndicator = pt.Y;
                VOffsetForDownIndicator = pt1.Y;

                upIndicator.PlacementRectangle = new Rect(HOffset, VOffsetForUpIndicator, upIndicatorContent.ActualWidth, upIndicatorContent.ActualHeight);
                downIndicator.PlacementRectangle = new Rect(HOffset, VOffsetForDownIndicator, downIndicatorContent.ActualWidth, downIndicatorContent.ActualHeight);
#else
                //Need to convert the calculated values into related screen coordinates to show drag indicators in correct position
                var pt = this.dataGrid.TransformToVisual(null).TransformPoint(new Point(HOffset, VOffsetForUpIndicator));
                var pt1 = this.dataGrid.TransformToVisual(null).TransformPoint(new Point(HOffset, VOffsetForDownIndicator));
                
                upIndicator.HorizontalOffset = downIndicator.HorizontalOffset = pt.X;
                upIndicator.VerticalOffset = pt.Y;
                downIndicator.VerticalOffset = pt1.Y;
#endif
            }
        }


        /// <summary>
        /// Gets Arrow Indicator Location for Draggable popup
        /// </summary>
        /// <param name="gridRect"></param>
        /// <param name="column"></param>
        /// <param name="mousePointOverGrid"></param>
        /// <param name="isPointerInGroupArea">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="canShowIndicators">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="indicatorColumnIndex"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private Point GetArrowIndicatorLocation(Rect gridRect, GridColumn column, Point mousePointOverGrid, out bool isPointerInGroupArea, out bool canShowIndicators, out int indicatorColumnIndex)
        {
            bool isPointerInHeaderArea;
            var groupAreaRect = this.GetGroupDropAreaRect();
            var headerAreaRect = this.GetHeaderRowRect();
            isPointerInHeaderArea = headerAreaRect.Contains(mousePointOverGrid);
            isPointerInGroupArea = groupAreaRect.Contains(mousePointOverGrid) && this.dataGrid.ShowGroupDropArea;
            if (!isPointerInGroupArea)
            {
                if (!CanDropColumn(column))
                {
                    canShowIndicators = false;
                    indicatorColumnIndex = -1;
                    return new Point(0, 0);
                }
                var rowColIndex = this.dataGrid.VisualContainer.PointToCellRowColumnIndex(mousePointOverGrid,true);
                if (this.dataGrid.ResolveToGridVisibleColumnIndex(rowColIndex.ColumnIndex) >= 0)
                    canShowIndicators = true;
                else
                    canShowIndicators = false;
                if (isPointerInHeaderArea)
                    canShowIndicators = true;
                Rect columnRect = Rect.Empty;
                if (isPointerInHeaderArea && this.dataGrid.ResolveToGridVisibleColumnIndex(rowColIndex.ColumnIndex) < 0)
                    columnRect = this.dataGrid.VisualContainer.RangeToRect(ScrollAxisRegion.Header, ScrollAxisRegion.Body, rowColIndex, false, true);
                else
                    columnRect = this.dataGrid.VisualContainer.RangeToRect(ScrollAxisRegion.Header, ScrollAxisRegion.Body, rowColIndex, false, true);
                
                //WPF-19230 while using footer columns ,column rect x position is calculated incorrectly .
                //so here using MousepointoverGrid  to get the correct x position of the Column.
                if (this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtPoint(mousePointOverGrid.X, true) != null && !columnRect.IsEmpty && (this.dataGrid.VisualContainer.FrozenColumns > 0 || this.dataGrid.VisualContainer.FooterColumns > 0))
                    columnRect.X = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtPoint(mousePointOverGrid.X, true).ClippedOrigin;

                if (!columnRect.IsEmpty)
                    columnRect.X = (columnRect.X < 0) ? this.dataGrid.VisualContainer.ScrollColumns.HeaderExtent : columnRect.X;

                if (isPointerInHeaderArea && this.dataGrid.ResolveToGridVisibleColumnIndex(rowColIndex.ColumnIndex) < 0)
                {
                    var increasedValue = 0d;
                    if (this.dataGrid.ShowRowHeader)
                        increasedValue += this.dataGrid.RowHeaderWidth;
                    if (this.dataGrid.View != null)
                        this.dataGrid.View.GroupDescriptions.ForEach(desc => increasedValue += this.dataGrid.IndentColumnWidth);
                    if (this.dataGrid.DetailsViewManager.HasDetailsView)
                        increasedValue += this.dataGrid.ExpanderColumnWidth;
                    if (!columnRect.IsEmpty)
                        columnRect.X = increasedValue;
                }
                Point pt=new Point();
                const int adjustValue = 8;

                //WPF-28270 Need to calculate indicators position using mousepoint in datagrid instead of pointoscreen cconverted value
                if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
                {
                    if (mousePointOverGrid.X > (columnRect != Rect.Empty ? columnRect.X + columnRect.Width / 2 : 0))
                    {
#if !WPF
                        pt.X = gridRect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + (columnRect != Rect.Empty ? columnRect.Width : 0) + adjustValue;
#else
                        pt.X = gridRect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + (columnRect != Rect.Empty ? columnRect.Width : 0) - adjustValue;
#endif
                        if (pt.X < gridRect.Left)
                            pt.X += (columnRect != Rect.Empty ? columnRect.Width : 0);
                        indicatorColumnIndex = rowColIndex.ColumnIndex;
                    }
                    else
                    {
#if !WPF
                        pt.X = gridRect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + adjustValue;
#else
                        pt.X = gridRect.X + (columnRect != Rect.Empty ? columnRect.X : 0) - adjustValue;
#endif
                    }
                    pt.Y = (columnRect != Rect.Empty ? columnRect.Y : 0) + gridRect.Y;
                    indicatorColumnIndex = rowColIndex.ColumnIndex - 1;
                }
                else
                {
                    //WPF-19230 while using footer columns if normal column is clipped it shows incorrect drag indicator while drag and drop the  column.
                    // so. calculate the column width  using clipped corner and clipped origin for getting correct visible column width.
                    var visibleline = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtPoint(mousePointOverGrid.X, false);
                    var width = visibleline != null && this.dataGrid.VisualContainer.FooterColumns > 0 ? visibleline.ClippedCorner - visibleline.ClippedOrigin : columnRect.Width;
                    if (mousePointOverGrid.X > (((columnRect != Rect.Empty ? columnRect.X : 0) - adjustValue) + ((columnRect != Rect.Empty ? columnRect.Width : 0) / 2)))
                    {
                        var xPoint = (gridRect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + (columnRect != Rect.Empty ? width : 0));
                        if (xPoint > gridRect.Right)
                            xPoint = xPoint - (columnRect != Rect.Empty ? width : 0);

                        pt = new Point(xPoint - adjustValue, (columnRect != Rect.Empty ? columnRect.Y : 0) + gridRect.Y);
                        indicatorColumnIndex = rowColIndex.ColumnIndex;
                    }
                    else
                    {
                        var xPoint = gridRect.X + (columnRect != Rect.Empty ? columnRect.X : 0);
                        if (xPoint < gridRect.Left)
                            xPoint = xPoint + (columnRect != Rect.Empty ? columnRect.Width : 0);

                        pt = new Point(xPoint - adjustValue, (columnRect != Rect.Empty ? columnRect.Y : 0) + gridRect.Y);
                        indicatorColumnIndex = rowColIndex.ColumnIndex - 1;
                    }
                }            
                return pt;
            }
            else
            {
                if (column != null && !this.CanGroupColumn(column))
                {
                    canShowIndicators = false;
                    indicatorColumnIndex = -1;
                    return new Point(0, 0);
                }
                double Vmiddlepoint = groupAreaRect.Height / 2 + groupAreaRect.Y;
                double Hpoint = groupAreaRect.X + this.dataGrid.GroupDropArea.groupItemsGrid.Margin.Left;

                if (this.dataGrid.GroupDropArea.Panel.Children.Count >= 0)
                {
                    foreach (GroupDropAreaItem item in this.dataGrid.GroupDropArea.Panel.Children)
                    {
#if !WPF
                        Rect itemRect = new Rect(item.TransformToVisual(this.dataGrid).TransformPoint(new Point(0, 0)), item.DesiredSize);
#else
                        var itemRect = item.GetControlRect(this.dataGrid);
#endif
                        double margins = item.Margin.Left + item.Margin.Right;

                        //WPF-28270 Calculate the horizontal and vertical position of indicators based on mouse position in datagrid
                        if (itemRect.Right > mousePointOverGrid.X)
                        {
                            if (mousePointOverGrid.X >= (itemRect.X + (itemRect.Width / 2)))
                            {
#if WPF
                                Hpoint = itemRect.Right;
#else
                                Hpoint = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ? itemRect.Right : itemRect.Right - margins;
#endif
                                dropedColIndex++;
                            }
                            else
                            {
#if WPF
                                Hpoint = itemRect.X - margins;
#else
                                Hpoint = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ? itemRect.X : itemRect.X - margins;
#endif
                                }
                            break;
                        }
                        else
#if WPF
                            Hpoint = itemRect.Right;
#else
                            Hpoint = this.dataGrid.FlowDirection == FlowDirection.RightToLeft ? itemRect.Right : itemRect.Right - margins;
#endif
                        dropedColIndex++;
                    }
                }
                canShowIndicators = true;
                indicatorColumnIndex = dropedColIndex;
                return new Point(Hpoint, Vmiddlepoint);
            }
        }

        /// <summary>
        /// Hides the Draggable Popup
        /// </summary>
        /// <remarks></remarks>
        internal void HidePopup()
        {
            if (this.isExpandedInternally)
            {
                this.dataGrid.GroupDropArea.IsExpanded = false;
                this.isExpandedInternally = false;
            }
#if !WPF
            if (!suspendReverseAnimationByColumnChooser)
                reverseAnimationStoryboard.Begin();
#else
            //When deleting any with expanding the DetailsViewGrid or else pressing the Ctrl + End key the DetailsViewGrid invokes the,
            //Dispose method which sets the focus to child grid, hence the below condition is added.
            else if(this.DraggablePopup.IsOpen)
                this.dataGrid.Focus();
#endif
            this.DraggablePopup.IsOpen = false;
        }

        /// <summary>
        /// Close the Drag arrow indication
        /// </summary>
        /// <remarks></remarks>
        internal void CloseDragIndication()
        {
            if (upIndicator != null && downIndicator != null)
            {
                upIndicator.IsOpen = false;
                downIndicator.IsOpen = false;
            }
        }

        /// <summary>
        /// Invoked when the popup content is dropped on the GroupDropArea in SfDataGrid.
        /// </summary>
        /// <param name="column">
        /// The corresponding column that is dropped to the GroupDropArea.
        /// </param>
        /// <remarks>
        /// Override this method and customize the drag-and-drop interaction between column and GroupDropArea in SfDataGrid.
        /// </remarks>
        protected virtual void PopupContentDroppedOnGroupDropArea(GridColumn column)
        {
            if (this.PopupContent.IsDragFromGroupDropArea)
            {
                var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(desc => desc.ColumnName == column.MappingName);
                var direction = sortColumn != null ? sortColumn.SortDirection : ListSortDirection.Ascending;
                var dropIndex = 0;
                if (this.dataGrid.View != null && dropedColIndex > this.dataGrid.View.GroupDescriptions.IndexOf(this.dataGrid.View.GroupDescriptions.FirstOrDefault(col => (col as ColumnGroupDescription).PropertyName.Equals(column.MappingName))))
                    dropIndex = dropedColIndex - 1 >= 0 ? dropedColIndex - 1 : 0;
                else
                    dropIndex = dropedColIndex;
                this.dataGrid.GroupDropArea.MoveGroupDropAreaItem(column, direction, dropIndex);
                this.DraggablePopup.IsOpen = false;
            }
            else
            {
                if (!this.dataGrid.GroupDropArea.IsExpanded)
                    this.dataGrid.GroupDropArea.IsExpanded = true;

                if (CanGroupColumn(column))
                {
                    this.DraggablePopup.IsOpen = false;
                    if (this.isExpandedInternally)
                        this.isExpandedInternally = false;

                    if (this.dataGrid.GroupDropArea.Panel.Children.Count > 0)
                    {
                        var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(desc => desc.ColumnName == column.MappingName);
                        var direction = sortColumn != null ? sortColumn.SortDirection : ListSortDirection.Ascending;
                        this.dataGrid.GroupDropArea.AddGroupAreaItem(column, direction, dropedColIndex);
                    }
                    else
                    {
                        var sortColumn = this.dataGrid.SortColumnDescriptions.FirstOrDefault(desc => desc.ColumnName == column.MappingName);
                        var direction = sortColumn != null ? sortColumn.SortDirection : ListSortDirection.Ascending;
                        this.dataGrid.GroupDropArea.AddGroupAreaItem(column, direction);
                    }
                }
                else
                {
                    if (this.isExpandedInternally)
                    {
                        this.dataGrid.GroupDropArea.IsExpanded = false;
                        this.isExpandedInternally = false;
                    }
#if !WPF
                    if (!this.PopupContent.IsDragFromColumnChooser)
                        reverseAnimationStoryboard.Begin();
                    else
                        this.DraggablePopup.IsOpen = false;
#else
                                this.DraggablePopup.IsOpen = false;
                                this.dataGrid.Focus();
#endif
                    return;
                }
            }
        }

        /// <summary>
        /// Invoked when the popup content is dropped on header row in SfDataGrid.
        /// </summary>
        /// <param name="oldIndex">
        /// The corresponding old index of the column before dropped the popup content on header row.
        /// </param>
        /// <param name="newColumnIndex">
        /// The new index of the column after dropped the popup content on header row.
        /// </param>
        /// <remarks>
        /// Override this method and customize the drag-and-drop interaction between columns in SfDataGrid.
        /// </remarks>
        protected virtual void PopupContentDroppedOnHeaderRow(int oldIndex, int newColumnIndex)
        {
            var column = this.PopupContent.Tag as GridColumn;
            if (oldIndex != newColumnIndex || this.PopupContent.IsDragFromGroupDropArea || this.PopupContent.IsDragFromColumnChooser)
            {
                if (CanDropColumn(column))
                {
#if WPF
                    SfDataGrid.suspendForColumnMove = true;
                    var currCellIndex = this.dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;
#endif
                    if (this.dataGrid.SelectionController.CurrentCellManager.EndEdit(false))
                    {
                        if (oldIndex != newColumnIndex)
#if !WPF
                            this.dataGrid.Columns.Move(oldIndex, newColumnIndex);
#else
                            this.dataGrid.Columns.MoveTo(oldIndex, newColumnIndex);
                        if (this.dataGrid.Columns.Count - 1 == oldIndex)
                        {
                            this.dataGrid.Columns[newColumnIndex].ExtendedWidth = double.NaN;
                        }
                        if (this.dataGrid.AllowResizingColumns && this.dataGrid.AllowResizingHiddenColumns)
                        {
                            this.dataGrid.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(oldIndex - 1, newColumnIndex - 1);
                            this.dataGrid.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(oldIndex + 1, newColumnIndex + 1);
                        }
#endif
                    }
#if WPF
                    SfDataGrid.suspendForColumnMove = false;
                    // If the grid is DetailsViewDataGrid
                    //WPF-18643 - After resizing, Drag and drop columns in NestedGrid resets the resized widths of columns
                    ///Below code refreshes the widths of NestedGrid's after drag and drop based on SourceDataGrid column widths
                    if (this.dataGrid.NotifyListener != null)
                    {
                        foreach (var grid in (this.dataGrid.NotifyListener.SourceDataGrid.NotifyListener as DetailsViewNotifyListener).ClonedDataGrid)
                        {
                            if (grid.NotifyListener == null)
                                continue;
                            var parentGrid = grid.NotifyListener.GetParentDataGrid();
                            var record = parentGrid.GetGridDetailsViewRecord(grid as DetailsViewDataGrid);
                            if (record is RecordEntry)
                            {
                                if ((record as RecordEntry).IsExpanded)
                                {
                                    (grid as IDetailsViewNotifier).SuspendNotifyListener();
                                    if (grid.GridColumnSizer != null)
                                        grid.GridColumnSizer.Refresh();
                                    (grid as IDetailsViewNotifier).ResumeNotifyListener();
                                }
                            }
                        }
                    }
                    else
                        this.dataGrid.GridColumnSizer.Refresh();
#endif
                }
                //Flow is changed, the column is removed from GroupDropArea after changing the position of column.
                //It is changed when dragging the column from GroupDropArea to first positon the second cell will be selected as a CurrentCell.
                if (this.PopupContent.IsDragFromGroupDropArea)
                {
                    this.dataGrid.GroupDropArea.RemoveGroupDropAreaItem(column);
                    if (this.dataGrid.StackedHeaderRows.Count > 0)
                        this.dataGrid.RowGenerator.RefreshStackedHeaders();
                }
            }
            if (this.isExpandedInternally)
            {
                this.dataGrid.GroupDropArea.IsExpanded = false;
                this.isExpandedInternally = false;
            }
            this.DraggablePopup.IsOpen = false;
        }

        /// <summary>
        /// Invoked when the popup content dropped on SfDataGrid.
        /// </summary>
        /// <param name="point">
        /// The corresponding position where the popup content is dropped in SfDataGrid.
        /// </param>
        protected virtual void PopupContentDroppedOnGrid(Point point)
        {
            var column = this.PopupContent.Tag as GridColumn;
            if (this.PopupContent.IsDragFromGroupDropArea && column != null)
            {
                this.dataGrid.GroupDropArea.RemoveGroupDropAreaItem(column);
                this.DraggablePopup.IsOpen = false;
            }
            HidePopup();
        }

#if !WPF
        /// <summary>
        /// Suspends the reverse animation for the popup
        /// </summary>
        /// <param name="suspend">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <remarks></remarks>
        protected void SuspendReverseAnimation(bool suspend)
        {
            suspendReverseAnimationByColumnChooser = suspend;
        }
#endif

        /// <summary>
        /// Invoked when the popup content dropped on SfDataGrid.
        /// </summary>
        /// <param name="pointOverGrid">
        /// Indicates whether the mouse point over on SfDataGrid.
        /// </param>
        protected virtual void OnPopupContentDropped(Point pointOverGrid)
        {
            bool arrowPopupIsOpen = upIndicator.IsOpen || downIndicator.IsOpen ? upIndicator.IsOpen : false;
            this.CloseDragIndication();
            dpTimer.Stop();
            if (this.DraggablePopup.IsOpen)
            {
                isDragState = false;

                QueryColumnDraggingEventArgs args = null;
                if (this.PopupContent.Tag is GridColumn)
                {
                    var column = this.PopupContent.Tag as GridColumn;
                    var oldIndex = this.dataGrid.Columns.IndexOf(column);
                    var newColumnIndex = -1;
                    var region = PointToGridRegion(pointOverGrid);

                    var rowColIndex = this.dataGrid.VisualContainer.PointToCellRowColumnIndex(pointOverGrid, true);
                    var columnIndex = rowColIndex.ColumnIndex;
                    var cellrect = this.dataGrid.VisualContainer.RangeToRect(ScrollAxisRegion.Header, ScrollAxisRegion.Body, rowColIndex, false, true);
                    if (pointOverGrid.X < cellrect.X + cellrect.Width / 2)
                        columnIndex = rowColIndex.ColumnIndex - 1;

                    if (region == GridRegion.Header)
                    {
                        newColumnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(rowColIndex.ColumnIndex);

                        if (newColumnIndex < 0 || newColumnIndex >= dataGrid.Columns.Count)
                            newColumnIndex = newColumnIndex < 0 ? 0 : dataGrid.Columns.Count - 1;
                        
                        else if (oldIndex < newColumnIndex)
                        {
                            if (pointOverGrid.X < (cellrect.X + cellrect.Width/2))
                                newColumnIndex = --newColumnIndex;
                        }
                        else if (oldIndex > newColumnIndex)
                        {
                            if (pointOverGrid.X > (cellrect.X + cellrect.Width/2))
                                newColumnIndex = ++newColumnIndex;
                        }

                        args = new QueryColumnDraggingEventArgs(this.dataGrid)
                        {
                            From = oldIndex,
                            To = columnIndex,
                            PopupPosition = pointOverGrid,
                            Reason = QueryColumnDraggingReason.Dropping
                        };
                    }
                    else
                    {
                        args = new QueryColumnDraggingEventArgs(this.dataGrid)
                        {
                            From = oldIndex,
                            To = columnIndex,
                            PopupPosition = pointOverGrid,
                            Reason = QueryColumnDraggingReason.Dropping
                        };
                    }

                    if (this.dataGrid.RaiseQueryColumnDragging(args))
                    {
                        this.DraggablePopup.IsOpen = false;
                        upIndicator.IsOpen = false;
                        downIndicator.IsOpen = false;
                        return;
                    }
                    if (region == GridRegion.GroupDropArea)
                    {
                        if (!CanGroupColumn(column))
                        {
                            this.DraggablePopup.IsOpen = false;
                            return;
                        }
                        PopupContentDroppedOnGroupDropArea(this.PopupContent.Tag as GridColumn);
                    }
                    else
                    {
                        if (region == GridRegion.Header)
                        {
                            PopupContentDroppedOnHeaderRow(oldIndex, newColumnIndex);
                        }
                        else if (region == GridRegion.Grid)
                        {
                            if (arrowPopupIsOpen)
                            {
                                var insertIndex = pointOverGrid.X == 0.0 ? 0 : this.dataGrid.Columns.Count - 1;
                                PopupContentDroppedOnHeaderRow(oldIndex, insertIndex);
                            }
                            else
                                PopupContentDroppedOnGrid(pointOverGrid);
                        }
                        else
                        {
                            if (this.PopupContent.IsDragFromGroupDropArea)
                            {
                                this.dataGrid.GroupDropArea.RemoveGroupDropAreaItem(column);
                                this.DraggablePopup.IsOpen = false;
                            }
                            else
                            {
                                HidePopup();
                            }
                        }
                    }
                    args.Reason = QueryColumnDraggingReason.Dropped;
                    this.dataGrid.RaiseQueryColumnDragging(args);
                    //this.DraggablePopup.IsOpen = false;
#if !WPF
                    this.dataGrid.Focus(FocusState.Programmatic);
#else
                    this.dataGrid.Focus();
#endif
                }
            }
        }

        /// <summary>
        /// Returns whether the column can be grouped or not
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool CanGroupColumn(GridColumn column)
        {
            if (column.MappingName == null)
            {
                throw new InvalidOperationException("MappingName is necessary for Sorting,Grouping & Filtering");
            }
            var groupcolumn = column.ReadLocalValue(GridColumn.AllowGroupingProperty);
            if (groupcolumn != DependencyProperty.UnsetValue)
                return column.AllowGrouping;
            return dataGrid.AllowGrouping;
        }

        /// <summary>
        /// Returns whether the column can be drop or not
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool CanDropColumn(GridColumn column)
        {
            if (column.MappingName == null)
            {
                throw new InvalidOperationException("MappingName is necessary for Sorting,Grouping & Filtering");
            }
            var dragColumn = column.ReadLocalValue(GridColumn.AllowDraggingProperty);
            if (dragColumn != DependencyProperty.UnsetValue)
                return column.AllowDragging;
            return this.dataGrid.AllowDraggingColumns;
        }

        /// <summary>
        /// Gets the display rectangle for the GroupDropArea.
        /// </summary>
        /// <returns>
        /// The display rectangle for the GroupDropArea.
        /// </returns>        
        protected Rect GetGroupDropAreaRect()
        {
            if (this.dataGrid.GroupDropArea == null)
                return Rect.Empty;

            var rect = this.dataGrid.GroupDropArea.GetControlRect(this.dataGrid);

            return rect;
        }

        /// <summary>
        /// Gets the display rectangle for the popup.
        /// </summary>
        /// <returns>
        /// The display rectangle for the popup.
        /// </returns>        
        protected Rect GetPopupRect()
        {
            var rect = this.PopupContent.GetControlRect(this.dataGrid);

            //WPF-28212 Need to calculate rect.x value as below for RightToLeft flow direction 
            if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
                rect.X -= this.PopupContent.ActualWidth;

            return rect;
        }

        /// <summary>
        /// Gets the display rectangle for the header row in SfDataGrid.
        /// </summary>
        /// <returns>
        /// The display rectangle for the header row.
        /// </returns> 
        protected Rect GetHeaderRowRect()
        {
            if (this.dataGrid.RowGenerator.Items.Count > 0)
            {
                double actualHeight = 0;
                //based on RowType, we need to calculate the header rows rect.
                var headerRows = this.dataGrid.RowGenerator.Items.Where(row => row.RowType == RowType.HeaderRow);
                var firstHeaderRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.HeaderRow);
                var lastHeaderRow = this.dataGrid.RowGenerator.Items.LastOrDefault(row => row.RowType == RowType.HeaderRow);
                headerRows.ForEach(row =>
                {
                    if (row != null && row.WholeRowElement != null)
                    {
                        actualHeight += row.WholeRowElement.ActualHeight;
                    }
                });
                if (firstHeaderRow != null && firstHeaderRow.WholeRowElement != null && lastHeaderRow != null)
                {
                    Rect lastRow = lastHeaderRow.WholeRowElement.GetControlRect(this.dataGrid);

                    Rect firstRow = firstHeaderRow.WholeRowElement.GetControlRect(this.dataGrid);

                    var rect = new Rect(lastRow.X, firstRow.Y, lastRow.Width, actualHeight);

                   return rect;
                }
            }
            return Rect.Empty;
        }

        /// <summary>
        /// Returns whether the column can be resize or not
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool CanResizeColumn(GridColumn column)
        {
            var resizeColumn = column.ReadLocalValue(GridColumn.AllowResizingProperty);
            if (resizeColumn != DependencyProperty.UnsetValue)
                return column.AllowResizing;
            return dataGrid.AllowResizingColumns;
        }

        /// <summary>
        /// Sets the popup position
        /// </summary>
        /// <param name="index"></param>
        /// <remarks></remarks>
        private void SetPopupPosition(int index)
        {
            var datarow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.HeaderRow && row.RowIndex == this.dataGrid.GetHeaderIndex());
            if (datarow != null && datarow.VisibleColumns != null)
            {
                var datacolumn = datarow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                if (datacolumn != null && datacolumn.ColumnElement != null)
                {
                    var controlRect = (datacolumn.ColumnElement as Control).GetControlRect(this.dataGrid);
                    
                    var rect = new Rect(controlRect.X, controlRect.Y, datacolumn.ColumnElement.ActualWidth, datacolumn.ColumnElement.ActualHeight);
                    var horizontalOffset = rect.X - ((this.PopupContent.ThumbWidth / 2) + ((this.PopupContent.InitialRect.Width * 0.05) / 2));
                    //UWP-4605 Resizing Popup is shown at different position while FlowDirection is RightToLeft 
                    //since horizontaloffset is calculated wrongly.
                    if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
                    {
                        rect.X = controlRect.Right;
                        horizontalOffset = rect.X + ((this.PopupContent.ThumbWidth / 2) + ((this.PopupContent.InitialRect.Width * 0.05) / 2));
                    }
                    var verticalOffset = GetPopupRect().Y;
#if UWP
                    //Need to convert vertical offset and horizontal offset position to its Screen coordinates
                    var convertpoint = this.dataGrid.TransformToVisual(null).TransformPoint(new Point(horizontalOffset, verticalOffset));
                    verticalOffset = convertpoint.Y;
                    horizontalOffset = convertpoint.X;
#endif
                    this.DraggablePopup.HorizontalOffset = Math.Round(horizontalOffset);
                    this.DraggablePopup.VerticalOffset = Math.Round(verticalOffset);
                    this.UpdateReverseAnimationOffset(this.DraggablePopup.HorizontalOffset, this.DraggablePopup.VerticalOffset, false);
                }
            }
        }
        #endregion

        #region public methods

        /// <summary>
        /// Displays the popup at the specified column index, display rectangle and position.
        /// </summary>
        /// <param name="gridColumnIndex">
        /// The corresponding column index to show the popup.
        /// </param>
        /// <param name="rect">
        /// The corresponding display rectangle to show the popup.
        /// </param>
        /// <param name="pointer">
        /// The corresponding position to show the popup.
        /// </param>
        public void ShowPopup(int gridColumnIndex, Rect rect, Pointer pointer)
        {
            var GridColumn = this.dataGrid.Columns[gridColumnIndex];
            if (GridColumn != null)
            {
                ShowPopup(GridColumn, rect, false, new Point(rect.Width / 2, rect.Height / 2), false, true, pointer);
            }

        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Gets the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridRegion"/> at the specified point co-ordinate value.
        /// </summary>
        /// <param name="point">
        /// The position to get the corresponding grid region.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridRegion"/> at the specified point.
        /// </returns>        
        public virtual GridRegion PointToGridRegion(Point point)
        {
            var groupDropAreaRect = GetGroupDropAreaRect();
            var popupRect = GetPopupRect();
            groupDropAreaRect.Intersect(popupRect);
            var isOverGroupDropArea = this.dataGrid.ShowGroupDropArea && groupDropAreaRect.Height >= 10 && groupDropAreaRect.Contains(point);
            if (isOverGroupDropArea)
                return GridRegion.GroupDropArea;

            var headerRowRect = GetHeaderRowRect();
            headerRowRect.Intersect(popupRect);
            if (headerRowRect.Height >= 5 && headerRowRect.Contains(point))
                return GridRegion.Header;

            var gridRect = this.dataGrid.GetControlRect(this.dataGrid);

            if (gridRect.Contains(point))
                return GridRegion.Grid;
            else
                return GridRegion.None;
        }

        /// <summary>
        /// Determines whether the popup is displayed on the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to decide whether the popup is displayed on it or not.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the popup is displayed on the column; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool CanShowPopup(GridColumn column)
        {
            var canShowPopup = false;
            var dragColumn = column.ReadLocalValue(GridColumn.AllowDraggingProperty);
            var groupcolumn = column.ReadLocalValue(GridColumn.AllowGroupingProperty);
            if (groupcolumn != DependencyProperty.UnsetValue)
                canShowPopup = column.AllowGrouping;
            if (!canShowPopup && dragColumn != DependencyProperty.UnsetValue)
                canShowPopup = column.AllowDragging;
            if ((groupcolumn == DependencyProperty.UnsetValue && (this.dataGrid.AllowGrouping && this.dataGrid.ShowGroupDropArea)) ||
                (dragColumn == DependencyProperty.UnsetValue && this.dataGrid.AllowDraggingColumns))
                canShowPopup = true;
            return canShowPopup;
        }

        /// <summary>
        /// Creates the popup content for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to create popup content.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.UIElement"/> for the popup.
        /// </returns>
        protected virtual UIElement CreatePopupContent(GridColumn column)
        {
            var textblock = new TextBlock { Text = column.HeaderText };
            return textblock;
        }

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.IsHidden"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column on which the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.IsHidden"/> property value changes occurs.
        /// </param>
        protected virtual void OnColumnHiddenChanged(GridColumn column)
        {
 
        }

        #endregion

        #endregion

        #region Dispose

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnWireEvents();
            if (this.PopupContent != null)
            {
                this.HidePopup();
                this.PopupContent.PopupContentPositionChanged -= OnPopupContentPositionChanged;
                this.PopupContent.PopupContentDropped -= OnPopupContentDropped;
                this.PopupContent.PopupContentResizing -= this.dataGrid.ColumnResizingController.OnPopupContentResizing;
                this.PopupContent.PopupContentResized -= this.dataGrid.ColumnResizingController.OnPopupContentResized;
            }
            this.dpTimer.Tick -= OnDispatcherTimerTick;
            if (isDisposing)
            {
                this.PopupContent.Dispose();
                this.PopupContent = null;
                this.DraggablePopup = null;
                this.upIndicator = null;
                this.downIndicator = null;
                this.DragLeftLine = null;
                this.DragRightLine = null;
                this.horizontalDoubleAnimation = null;
                this.verticalDoubleAnimation = null;
                this.reverseAnimationStoryboard = null;
                if (dataGrid != null)
                {
                    this.dataGrid.ColumnResizingController.dragLine = null;
                    this.dataGrid = null;
                }
            }
            isdisposed = true;
        }
        
        #endregion   
    }
}
