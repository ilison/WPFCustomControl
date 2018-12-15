#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using MouseEventArgs = PointerRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using Windows.UI.Input;
    using Windows.UI.Core;
    using System.Diagnostics;
#endif
    /// <summary>
    /// Provides the base implementation for column drag-and-drop operations in SfTreeGrid.
    /// </summary>
    /// <remarks>
    /// It provides the set of public properties and virtual method to customize the drag-and-drop operation.
    /// </remarks>
    public class TreeGridColumnDragDropController : IDisposable
    {

        #region fields

        SfTreeGrid treeGrid;

        #region DraggablePopup Fields

        private bool isPopupInitlized = false;
        private Popup upIndicator;
        private Popup downIndicator;
        bool allowScrollOnHorizontalRightLimits = false;
        bool allowScrollOnHorizontalLeftLimits = false;
        private Point previousIndicatorPosition;
        internal double PreviousLeftLineSize;
        internal double PreviousRightLineSize;
        internal bool needToUpdatePosition;
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
        /// <summary>
        /// Gets the PopupContentControl for drag-and-drop operation.
        /// </summary>        
        protected PopupContentControl PopupContentControl
        {
            get { return PopupContent; }
        }
        private PopupContentControl PopupContent;
        internal Popup DraggablePopup;
        internal VisibleLineInfo DragLeftLine;
        internal VisibleLineInfo DragRightLine;
        internal Storyboard reverseAnimationStoryboard;
        private EasingDoubleKeyFrame horizontalDoubleAnimation;
        private EasingDoubleKeyFrame verticalDoubleAnimation;
        private double mouseHorizontalPosition;
        private double mouseVerticalPosition;
        internal bool isDragState;

        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDragDropController"/> class.
        /// </summary>
        /// <param name="TreeGrid">The SfTreeGrid.</param>
        public TreeGridColumnDragDropController(SfTreeGrid TreeGrid)
        {
            this.treeGrid = TreeGrid;
            InitializePopup();
        }
        #endregion

        #region public properties

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

        internal void ShowPopup(TreeGridColumn treeGridColumn, Rect rect, Point adjustPoint, bool isInTouch,
                              object pointer)
        {
            if (!this.treeGrid.ValidationHelper.CheckForValidation(false))
                return;
            if (this.PopupContent != null)
            {
#if !WPF
                if (this.DraggablePopup.IsOpen || pointer == null)
                    return;
                this.PopupContent.CapturePointer(pointer as Pointer);
#endif
                if (this.DraggablePopup.IsOpen)
                    return;

                if (!isInTouch)
                {
                    var args = new TreeGridColumnDraggingEventArgs()
                    {
                        From = this.treeGrid.Columns.IndexOf(treeGridColumn),
                        To = -1,
                        PopupPosition = new Point(rect.X, rect.Y),
                        Reason = QueryColumnDraggingReason.DragStarting
                    };
                    if (this.treeGrid.RaiseQueryColumnDragging(args))
                    {
                        this.DraggablePopup.IsOpen = false;
                        upIndicator.IsOpen = false;
                        downIndicator.IsOpen = false;
                        return;
                    }
                }

                if (treeGridColumn.AllowResizing)
                {
                    //In the ShowPopUpmethod we have used the ColIndex variable to get the column index for the particular tree grid column.
                    //Based on this columnIndex we can check the Previous or next column is hidden or Not if it is hidden we can set the DragLeftLine.
                    int colIndex = this.treeGrid.Columns.IndexOf(treeGridColumn);
                    int index = this.treeGrid.ResolveToScrollColumnIndex(colIndex);
                    this.DragRightLine = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(index);
                    this.DragLeftLine = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(index - 1);

                    //Here Checked the DragLine Value and Previous column is Hidden or because while dragging the Line the Previous Column has to be dragged.
                    //When the don't have the RowHeader and IndentCell the DragLeftLine will be null.
                    //So that it will try to get the Previous columnIndex value. But here the columnIndex value is zero.
                    if (this.DragLeftLine == null && colIndex > 0 && this.treeGrid.Columns[colIndex - 1].IsHidden)
                    {
                        index = this.treeGrid.ResolveToScrollColumnIndex(this.treeGrid.GetPreviousFocusTreeGridColumnIndex(colIndex - 1));
                        this.DragLeftLine = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(index);
                    }
                    if (this.DragRightLine != null)
                        this.PreviousRightLineSize = this.DragRightLine.Size;
                    if (this.DragLeftLine != null)
                        this.PreviousLeftLineSize = this.DragLeftLine.Size;

                    var indentCellCount = this.treeGrid.ResolveToStartColumnIndex();

                    if (isInTouch)
                    {
                        if (DragLeftLine != null && (DragLeftLine == null || DragLeftLine.LineIndex < indentCellCount) &&
                            !(this.treeGrid.FlowDirection == FlowDirection.RightToLeft &&
                              this.treeGrid.ResolveToScrollColumnIndex(DragLeftLine.LineIndex) == this.treeGrid.ResolveToStartColumnIndex()))
                            this.PopupContent.LeftResizeThumbVisibility = Visibility.Collapsed;
                        else
                            this.PopupContent.LeftResizeThumbVisibility = Visibility.Visible;

                        // To Do - Need to check GroupDescriptions count here after grouping is given
                        if (DragRightLine != null && (this.treeGrid.FlowDirection == FlowDirection.RightToLeft &&
                            DragRightLine.LineIndex < indentCellCount))
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

                this.PopupContent.InitialRect = rect;
                mouseHorizontalPosition = adjustPoint.X;
                mouseVerticalPosition = adjustPoint.Y;
                this.PopupContent.IsOpenInTouch = isInTouch;
                this.PopupContent.Tag = treeGridColumn;
                this.PopupContent.Content = CreatePopupContent(treeGridColumn);
                var indentWidth = (this.treeGrid.View.Nodes.MaxLevel + 1) * this.treeGrid.TreeGridColumnSizer.ExpanderWidth;
                if (treeGrid.ShowCheckBox)
                    indentWidth += this.treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;
                if (!double.IsNaN(treeGridColumn.MinimumWidth))
                {
                    this.PopupContent.MinWidth = treeGridColumn.MinimumWidth;
                    if (this.treeGrid.ColumnResizingController.IsExpanderColumn(treeGridColumn))
                    {
                        this.PopupContent.MinWidth += indentWidth;
                    }
                }
                else
                    this.PopupContent.MinWidth = this.PopupMinWidth;

                if (!double.IsNaN(treeGridColumn.MaximumWidth))
                {
                    this.PopupContent.MaxWidth = treeGridColumn.MaximumWidth + this.PopupContent.ThumbWidth;
                    if (this.treeGrid.ColumnResizingController.IsExpanderColumn(treeGridColumn))
                    {
                        this.PopupContent.MaxWidth += indentWidth;
                    }
                }
                else
                {
                    if (this.PopupContent.LeftResizeThumbVisibility == Visibility.Visible || this.PopupContent.RightResizeThumbVisibility == Visibility.Visible)
                        this.PopupContent.MaxWidth = double.PositiveInfinity;
                    else
                        this.PopupContent.MaxWidth = this.PopupMaxWidth;
                }
                this.PopupContent.MinHeight = PopupMinHeight;
                this.PopupContent.MaxHeight = PopupMaxHeight;

                // Need to set the column width to PopupContent width for the Expander Column alone
                if (treeGridColumn.IsHidden && !this.treeGrid.ColumnResizingController.IsExpanderColumn(treeGridColumn))
                    this.PopupContent.Width = this.PopupMinWidth;
                else
                    this.PopupContent.Width = rect.Width + this.PopupContent.ThumbWidth;

                this.PopupContent.Height = rect.Height;

                var popupActualWidth = 0d;
                var popupActualHeight = 0d;
                if (this.PopupContent.Width > this.PopupContent.MaxWidth)
                {
                    popupActualWidth = this.PopupContent.MaxWidth;
                }
                else if (this.PopupContent.Width < this.PopupContent.MinWidth)
                {
                    popupActualWidth = this.PopupContent.MinWidth;
                }
                else
                    popupActualWidth = this.PopupContent.Width;

                if (PopupContent.Height > this.PopupContent.MaxHeight)
                    popupActualHeight = this.PopupContent.MaxHeight;
                else if (this.PopupContent.Height < this.PopupContent.MinHeight)
                    popupActualHeight = this.PopupContent.MinHeight;
                else
                    popupActualHeight = this.PopupContent.Height;

#if WPF
                var pt = this.treeGrid.PointToScreen(new Point(rect.X, rect.Y));
                var popupPositionRect = new Rect(pt.X - this.PopupContent.ThumbWidth / 2, pt.Y,
                                                 rect.Width, rect.Height);
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
                var point = this.treeGrid.TransformToVisual(null).TransformPoint(new Point(rect.X, rect.Y));
                rect.X = point.X;
                rect.Y = point.Y;

                var horizontalOffset = rect.X - (((this.PopupContent.ThumbWidth / 2) + ((this.PopupContent.InitialRect.Width * 0.05) / 2)));
                if (this.treeGrid.FlowDirection == FlowDirection.RightToLeft && (!treeGridColumn.IsHidden || (this.treeGrid.ColumnResizingController.IsExpanderColumn(treeGridColumn) && treeGridColumn.IsHidden)))
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

                UpdateReverseAnimationOffset(this.DraggablePopup.HorizontalOffset,
                                             this.DraggablePopup.VerticalOffset, false);
                this.PopupContent.ApplyState(true);
                needToUpdatePosition = false;
                this.DraggablePopup.Opacity = 1;

                if (!isInTouch)
                {
                    var args = new TreeGridColumnDraggingEventArgs();
                    args.From = treeGrid.Columns.IndexOf(treeGridColumn);
                    args.To = -1;
                    args.Reason = QueryColumnDraggingReason.DragStarted;
                    args.PopupPosition = new Point(rect.X, rect.Y);
                    if (this.treeGrid.RaiseQueryColumnDragging(args))
                        return;
                }

                this.DraggablePopup.IsOpen = true;
                VisualStateManager.GoToState(this.PopupContent, "Valid", true);

#if WPF
                // WPF-32058 - Change DraggablePopup height and width based on the Popupcontentcontrol height and width.
                this.DraggablePopup.Height = PopupContent.ActualHeight + PopupContent.DragIndicator.Height;
                this.DraggablePopup.Width = PopupContent.ActualWidth + PopupContent.DragIndicator.Width;
                DraggablePopup.PlacementTarget = this.treeGrid;
                if (pointer != null)
                {
                    var touch = pointer as TouchDevice;
                    this.PopupContent.CaptureTouch(touch);
                }
                else
                    this.PopupContent.CaptureMouse();
#endif
            }
        }

        internal void ColumnHiddenChanged(TreeGridColumn column)
        {
            this.OnColumnHiddenChanged(column);
        }


        internal void UpdatePopupPosition()
        {
            if (!needToUpdatePosition || !this.DraggablePopup.IsOpen)
                return;

            if (this.PopupContent.Tag != null && this.PopupContent.Tag is TreeGridColumn)
            {
                var treeColumn = this.PopupContent.Tag as TreeGridColumn;
                var index = this.treeGrid.Columns.IndexOf(treeColumn);
                if (treeColumn.IsHidden && !this.treeGrid.ColumnResizingController.IsExpanderColumn(treeColumn))
                    index = this.treeGrid.GetNextFocusTreeGridColumnIndex(index, this.treeGrid.FlowDirection);
                SetPopupPosition(this.treeGrid.ResolveToScrollColumnIndex(index));
                needToUpdatePosition = false;
            }
        }

        /// <summary>
        /// Hides the Draggable Popup
        /// </summary>
        /// <remarks></remarks>
        internal void HidePopup()
        {
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
            PopupContent = new PopupContentControl(this.treeGrid)
            {
                PopupContentPositionChanged = OnPopupContentPositionChanged,
                PopupContentDropped = OnPopupContentDropped,
                PopupContentResizing = this.treeGrid.ColumnResizingController.OnPopupContentResizing,
                PopupContentResized = this.treeGrid.ColumnResizingController.OnPopupContentResized
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

            DraggablePopup.FlowDirection = this.treeGrid.FlowDirection;
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
            if (this.PopupContent.IsOpenInTouch)
            {
                this.PopupContent.ApplyState(true);

                if (DragLeftLine != null)
                    this.PopupContent.LeftResizeThumbVisibility = Visibility.Collapsed;
                else
                    this.PopupContent.LeftResizeThumbVisibility = Visibility.Visible;

                if (DragRightLine != null)
                    this.PopupContent.RightResizeThumbVisibility = Visibility.Collapsed;
                else
                    this.PopupContent.RightResizeThumbVisibility = Visibility.Visible;

                this.PopupContent.IsOpenInTouch = false;
            }
            else
            {
                this.DraggablePopup.IsOpen = false;
#if !WPF
                this.treeGrid.Focus(FocusState.Programmatic);
#else
                this.treeGrid.Focus();
#endif
            }
            this.PopupContent.ResetMousePosition();
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
            this.PopupContent.ApplyState(false);
            this.PopupContent.ResetMousePosition();
        }

        /// <summary>
        /// Calls for show drag indicator for popup
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="mousePointOverGrid"></param>
        /// <remarks></remarks>
        private void ShowDragIndication(Rect rect, Point mousePointOverGrid)
        {
            if (upIndicator != null && downIndicator != null)
            {
                bool canShowIndicator = false;
                var headerRowRect = GetHeaderRowRect();
                int indicatorColumnIndex;
                var column = this.PopupContent.Tag as TreeGridColumn;
                var point = this.GetArrowIndicatorLocation(rect, column, mousePointOverGrid, out canShowIndicator, out indicatorColumnIndex);

                if (canShowIndicator && !previousIndicatorPosition.Equals(point))
                {
                    var args = new TreeGridColumnDraggingEventArgs()
                    {
                        From = this.treeGrid.Columns.IndexOf(column),
                        To = indicatorColumnIndex,
                        PopupPosition = mousePointOverGrid,
                        Reason = QueryColumnDraggingReason.Dragging
                    };
                    if (this.treeGrid.RaiseQueryColumnDragging(args))
                    {
                        upIndicator.IsOpen = false;
                        downIndicator.IsOpen = false;
                        return;
                    }
                }

                previousIndicatorPosition = point;
                double VOffsetForUpIndicator, VOffsetForDownIndicator, HOffset = point.X;

                VOffsetForUpIndicator = headerRowRect.Y + headerRowRect.Height;
                VOffsetForDownIndicator = headerRowRect.Y - (downIndicator.Child as ContentControl).ActualHeight;
#if !WPF
                if (VOffsetForDownIndicator < 0)
                    VOffsetForDownIndicator = 0;
#endif
                var upIndicatorContent = upIndicator.Child as UpIndicatorContentControl;
                var downIndicatorContent = downIndicator.Child as DownIndicatorContentControl;

                if (!canShowIndicator)
                    CloseDragIndication();
                else
                {
#if WPF
                    upIndicator.PlacementTarget = this.treeGrid;
                    downIndicator.PlacementTarget = this.treeGrid;
#else
                    upIndicatorContent.IsOpen = true;
                    downIndicatorContent.IsOpen = true;
#endif
                    upIndicator.IsOpen = true;
                    downIndicator.IsOpen = true;

                    VisualStateManager.GoToState(upIndicatorContent, "Open", false);
                    VisualStateManager.GoToState(downIndicatorContent, "Open", false);
                }
#if WPF
                var upPoint = this.treeGrid.PointToScreen(new Point(HOffset, VOffsetForUpIndicator));
                var downPoint = this.treeGrid.PointToScreen(new Point(HOffset, VOffsetForDownIndicator));

                HOffset = upPoint.X;
                VOffsetForUpIndicator = upPoint.Y;
                VOffsetForDownIndicator = downPoint.Y;
                upIndicator.PlacementRectangle = new Rect(HOffset, VOffsetForUpIndicator, upIndicatorContent.ActualWidth, upIndicatorContent.ActualHeight);
                downIndicator.PlacementRectangle = new Rect(HOffset, VOffsetForDownIndicator, downIndicatorContent.ActualWidth, downIndicatorContent.ActualHeight);
#else
                var upPoint = this.treeGrid.TransformToVisual(null).TransformPoint(new Point(HOffset, VOffsetForUpIndicator));
                var downPoint = this.treeGrid.TransformToVisual(null).TransformPoint(new Point(HOffset, VOffsetForDownIndicator));

                upIndicator.HorizontalOffset = downIndicator.HorizontalOffset = upPoint.X;
                upIndicator.VerticalOffset = upPoint.Y;
                downIndicator.VerticalOffset = downPoint.Y;
#endif
            }
        }

        /// <summary>
        /// Gets Arrow Indicator Location for Draggable popup
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="column"></param>
        /// <param name="mousePointOverGrid"></param>
        /// <param name="canShowIndicators">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="indicatorColumnIndex"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private Point GetArrowIndicatorLocation(Rect rect, TreeGridColumn column, Point mousePointOverGrid, out bool canShowIndicator, out int indicatorColumnIndex)
        {
            bool isPointerInHeaderArea;
            var headerRowRect = this.GetHeaderRowRect();
            isPointerInHeaderArea = headerRowRect.Contains(mousePointOverGrid);

            if (!column.AllowDragging)
            {
                canShowIndicator = false;
                indicatorColumnIndex = -1;
                return new Point(0, 0);
            }

            var rowColumnIndex = this.treeGrid.TreeGridPanel.PointToCellRowColumnIndex(mousePointOverGrid, true);
            canShowIndicator = true;
            var columnRect = this.treeGrid.TreeGridPanel.RangeToRect(ScrollAxisRegion.Header, ScrollAxisRegion.Body, rowColumnIndex, false, true);

            // Consider frozen column to calculate column Rect.
            if (this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtPoint(mousePointOverGrid.X, true) != null && !columnRect.IsEmpty && this.treeGrid.TreeGridPanel.FrozenColumns > 0)
                columnRect.X = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtPoint(mousePointOverGrid.X, true).ClippedOrigin;

            if (!columnRect.IsEmpty)
                columnRect.X = (columnRect.X < 0) ? this.treeGrid.TreeGridPanel.ScrollColumns.HeaderExtent : columnRect.X;

            Point point = new Point();
            const int adjustValue = 8;

            if (this.treeGrid.FlowDirection == FlowDirection.LeftToRight)
            {
                if (mousePointOverGrid.X > (columnRect != Rect.Empty ? columnRect.X + columnRect.Width / 2 : 0))
                {
                    var xPoint = rect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + (columnRect != Rect.Empty ? columnRect.Width : 0);
                    if (xPoint > rect.Right)
                        xPoint = xPoint - (columnRect != Rect.Empty ? columnRect.Width : 0);
                    point = new Point(xPoint - adjustValue, (columnRect != Rect.Empty ? columnRect.Y : 0) + rect.Y);
                    indicatorColumnIndex = rowColumnIndex.ColumnIndex;
                }
                else
                {
                    var xPoint = rect.X + (columnRect != Rect.Empty ? columnRect.X : 0);
                    if (xPoint < rect.Left)
                        xPoint = point.X + (columnRect != Rect.Empty ? columnRect.Width : 0);
                    point = new Point(xPoint - adjustValue, (columnRect != Rect.Empty ? columnRect.Y : 0) + rect.Y);
                    indicatorColumnIndex = rowColumnIndex.ColumnIndex - 1;
                }
            }
            else
            {
                if (mousePointOverGrid.X > (columnRect != Rect.Empty ? columnRect.X + columnRect.Width / 2 : 0))
                {
#if !WPF
                    point.X = rect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + (columnRect != Rect.Empty ? columnRect.Width : 0) + adjustValue;
#else
                    point.X = rect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + (columnRect != Rect.Empty ? columnRect.Width : 0) - adjustValue;
#endif
                    if (point.X < rect.Left)
                        point.X += (columnRect != Rect.Empty ? columnRect.Width : 0);
                    indicatorColumnIndex = rowColumnIndex.ColumnIndex;
                }
                else
                {
#if !WPF
                    point.X = rect.X + (columnRect != Rect.Empty ? columnRect.X : 0) + adjustValue;
#else
                    point.X = rect.X + (columnRect != Rect.Empty ? columnRect.X : 0) - adjustValue;
#endif
                    indicatorColumnIndex = rowColumnIndex.ColumnIndex - 1;
                }
                point.Y = (columnRect != Rect.Empty ? columnRect.Y : 0) + rect.Y;
            }
            canShowIndicator = true;
            return point;
        }


        /// <summary>
        /// Sets the popup position
        /// </summary>
        /// <param name="index"></param>
        /// <remarks></remarks>
        private void SetPopupPosition(int index)
        {
            var treeRow = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == TreeRowType.HeaderRow);
            if (treeRow != null && treeRow.VisibleColumns != null)
            {
                var treeColumn = treeRow.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == index);

                // Need to find previous column index if last column is hidden to set popup position
                var resolvedIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(index);
                if (this.treeGrid.FlowDirection == FlowDirection.LeftToRight && this.treeGrid.Columns[resolvedIndex].IsHidden)
                {
                    if ((resolvedIndex - 1) == this.treeGrid.ResolveToGridVisibleColumnIndex(this.treeGrid.GetLastColumnIndex()))
                        resolvedIndex = resolvedIndex - 1;
                    treeColumn = treeRow.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == this.treeGrid.ResolveToScrollColumnIndex(resolvedIndex));
                }

                if (treeColumn != null && treeColumn.ColumnElement != null)
                {
#if WPF
                    var controlRect = (treeColumn.ColumnElement as Control).GetControlRect(this.treeGrid);
                    var point = new Point(controlRect.X, controlRect.Y);
#else
                    var point = treeColumn.ColumnElement.TransformToVisual(null).TransformPoint(new Point(0, 0));
#endif
                    var rect = new Rect(point.X, point.Y, treeColumn.ColumnElement.ActualWidth, treeColumn.ColumnElement.ActualHeight);
                    var horizontalOffset = rect.X - ((this.PopupContent.ThumbWidth / 2) + ((this.PopupContent.InitialRect.Width * 0.05) / 2));
                    if (this.treeGrid.FlowDirection == FlowDirection.RightToLeft)
                        horizontalOffset -= rect.Width;

                    // Need to set popup position to previous column if last column is hidden
                    else if (this.treeGrid.GetLastColumnIndex() != ((this.treeGrid.Columns.Count - 1) + this.treeGrid.ResolveToStartColumnIndex())
                            && this.PopupContent.Width <= PopupMinWidth)
                    {
                        if (this.treeGrid.Columns[resolvedIndex + 1].IsHidden)
                            horizontalOffset += rect.Width;
                    }
                    double verticalOffset;
#if UWP
                    verticalOffset = this.treeGrid.TransformToVisual(null).TransformPoint(new Point(0, GetPopupRect().Y)).Y;
#else
                    verticalOffset = this.treeGrid.PointToScreen(new Point(0, GetPopupRect().Y)).Y;
#endif
                    this.DraggablePopup.HorizontalOffset = Math.Round(horizontalOffset);
                    this.DraggablePopup.VerticalOffset = Math.Round(verticalOffset);
                    this.UpdateReverseAnimationOffset(this.DraggablePopup.HorizontalOffset, this.DraggablePopup.VerticalOffset, false);
                }
            }
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Invoked when the position of popup content is changed in SfTreeGrid.
        /// </summary>
        /// <param name="HorizontalDelta">
        /// The corresponding horizontal distance of the popup content position changes. 
        /// </param>
        /// <param name="VerticalDelta">
        /// The corresponding vertical distance of the popup content position changes. 
        /// </param>
        /// <param name="mousePointOverGrid">
        /// Indicates whether the mouse point is hover on the TreeGrid.
        /// </param>
        protected virtual void OnPopupContentPositionChanged(double HorizontalDelta, double VerticalDelta, Point mousePointOverGrid)
        {
            if (this.DraggablePopup.IsOpen)
            {
#if WPF
                var point = this.treeGrid.PointToScreen(new Point(HorizontalDelta, VerticalDelta));
                this.DraggablePopup.PlacementRectangle = new Rect(point.X - mouseHorizontalPosition,
                                                                  point.Y - mouseVerticalPosition,
                                                                  this.PopupContent.ActualWidth,
                                                                  this.PopupContent.ActualHeight);
#else
                var point = this.treeGrid.TransformToVisual(null).TransformPoint(new Point(HorizontalDelta, VerticalDelta));
                this.DraggablePopup.HorizontalOffset = point.X - mouseHorizontalPosition;
                this.DraggablePopup.VerticalOffset = point.Y - mouseVerticalPosition;
#endif

                var gridRect = this.treeGrid.GetControlRect(this.treeGrid);
                allowScrollOnHorizontalRightLimits = this.treeGrid.FlowDirection == FlowDirection.RightToLeft ?
                                                     gridRect.X > mousePointOverGrid.X :
                                                     gridRect.Right < mousePointOverGrid.X;

                // need to consider header extent to auto scroll, when frozen column count is set.
                allowScrollOnHorizontalLeftLimits = this.treeGrid.FlowDirection == FlowDirection.RightToLeft ?
                                                    (gridRect.Right - this.treeGrid.TreeGridPanel.ScrollColumns.HeaderExtent) < mousePointOverGrid.X :
                                                    (gridRect.X - 20 + this.treeGrid.TreeGridPanel.ScrollColumns.HeaderExtent) <= mousePointOverGrid.X;

                if (gridRect.Contains(mousePointOverGrid) && GetHeaderRowRect().Bottom > mousePointOverGrid.Y)
                {
                    this.ShowDragIndication(gridRect, mousePointOverGrid);
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

                if (allowScrollOnHorizontalRightLimits || allowScrollOnHorizontalLeftLimits)
                    this.treeGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.Horizontal;
                else
                    this.treeGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
                this.treeGrid.AutoScroller.MouseMovePosition = mousePointOverGrid;
            }
        }

        /// <summary>
        /// Invoked when the popup content is dropped on header row in SfTreeGrid.
        /// </summary>
        /// <param name="oldIndex">
        /// The corresponding old index of the column before dropped the popup content on header row.
        /// </param>
        /// <param name="newColumnIndex">
        /// The new index of the column after dropped the popup content on header row.
        /// </param>
        /// <remarks>
        /// Override this method and customize the drag-and-drop interaction between columns in SfTreeGrid.
        /// </remarks>
        protected virtual void PopupContentDroppedOnHeaderRow(int oldIndex, int newColumnIndex)
        {
            var column = this.PopupContent.Tag as TreeGridColumn;
            if (oldIndex != newColumnIndex)
            {
                if (column.AllowDragging)
                {
                    if (this.treeGrid.SelectionController.CurrentCellManager.EndEdit(false))
                    {
#if WPF
                        SfTreeGrid.suspendForColumnMove = true;
                        this.treeGrid.Columns.MoveTo(oldIndex, newColumnIndex);
                        if (this.treeGrid.AllowResizingColumns && this.treeGrid.AllowResizingHiddenColumns)
                        {
                            this.treeGrid.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(oldIndex - 1, newColumnIndex - 1);
                            this.treeGrid.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(oldIndex + 1, newColumnIndex + 1);
                        }
                        SfTreeGrid.suspendForColumnMove = false;
#else
                    this.treeGrid.Columns.Move(oldIndex, newColumnIndex);
#endif

#if WPF
                        this.treeGrid.TreeGridColumnSizer.Refresh();
#endif
                    }
                }
            }
            this.DraggablePopup.IsOpen = false;
        }

        /// <summary>
        /// Invoked when the popup content dropped on SfTreeGrid.
        /// </summary>
        /// <param name="point">
        /// The corresponding position where the popup content is dropped in SfTreeGrid.
        /// </param>
        protected virtual void PopupContentDroppedOnGrid(Point point)
        {
            var column = this.PopupContent.Tag as TreeGridColumn;
            if (column != null)
            {
                this.DraggablePopup.IsOpen = false;
            }
            HidePopup();
        }

        /// <summary>
        /// Invoked when the popup content dropped on SfTreeGrid.
        /// </summary>
        /// <param name="pointOverGrid">
        /// Indicates whether the mouse point over on SfTreeGrid.
        /// </param>
        protected virtual void OnPopupContentDropped(Point pointOverGrid)
        {
            bool arrowPopupIsOpen = upIndicator.IsOpen || downIndicator.IsOpen ? upIndicator.IsOpen : false;
            this.CloseDragIndication();
            this.treeGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
            if (this.DraggablePopup.IsOpen)
            {
                TreeGridColumnDraggingEventArgs args = null;
                if (this.PopupContent.Tag is TreeGridColumn)
                {
                    var column = this.PopupContent.Tag as TreeGridColumn;
                    var oldIndex = this.treeGrid.Columns.IndexOf(column);
                    var newColumnIndex = -1;
                    var region = this.PointToGridRegion(pointOverGrid);

                    var rowColumnIndex = this.treeGrid.TreeGridPanel.PointToCellRowColumnIndex(pointOverGrid);
                    var columnIndex = rowColumnIndex.ColumnIndex;
                    var cellRect = this.treeGrid.TreeGridPanel.RangeToRect(ScrollAxisRegion.Header, ScrollAxisRegion.Body, rowColumnIndex, false, true);
                    if (pointOverGrid.X < cellRect.X + cellRect.Width / 2)
                        columnIndex = rowColumnIndex.ColumnIndex - 1;

                    if (region == TreeGridRegion.Header)
                    {
                        newColumnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);

                        if (newColumnIndex < 0 || newColumnIndex >= treeGrid.Columns.Count)
                            newColumnIndex = newColumnIndex < 0 ? this.treeGrid.ResolveToStartColumnIndex() : treeGrid.Columns.Count - 1;

                        else if (oldIndex < newColumnIndex)
                        {
                            if (pointOverGrid.X < (cellRect.X + cellRect.Width / 2))
                                newColumnIndex = --newColumnIndex;
                        }
                        else if (oldIndex > newColumnIndex)
                        {
                            if (pointOverGrid.X > (cellRect.X + cellRect.Width / 2))
                                newColumnIndex = ++newColumnIndex;
                        }

                        args = new TreeGridColumnDraggingEventArgs()
                        {
                            From = oldIndex,
                            To = columnIndex,
                            PopupPosition = pointOverGrid,
                            Reason = QueryColumnDraggingReason.Dropping
                        };
                    }
                    else
                    {
                        args = new TreeGridColumnDraggingEventArgs()
                        {
                            From = oldIndex,
                            To = columnIndex,
                            PopupPosition = pointOverGrid,
                            Reason = QueryColumnDraggingReason.Dropping
                        };
                    }

                    if (this.treeGrid.RaiseQueryColumnDragging(args))
                    {
                        this.DraggablePopup.IsOpen = false;
                        upIndicator.IsOpen = false;
                        downIndicator.IsOpen = false;
                        return;
                    }

                    if (region == TreeGridRegion.Header)
                    {
                        PopupContentDroppedOnHeaderRow(oldIndex, newColumnIndex);
                    }
                    else if (region == TreeGridRegion.Grid)
                    {
                        if (arrowPopupIsOpen)
                        {
                            var insertIndex = pointOverGrid.X == 0 ? 0 : this.treeGrid.Columns.Count - 1;
                            PopupContentDroppedOnHeaderRow(oldIndex, insertIndex);
                        }
                        else
                            PopupContentDroppedOnGrid(pointOverGrid);
                    }
                    else
                        HidePopup();

                    args.Reason = QueryColumnDraggingReason.Dropped;
                    this.treeGrid.RaiseQueryColumnDragging(args);
#if WPF
                    this.treeGrid.Focus();
#else
                    this.treeGrid.Focus(FocusState.Programmatic);
#endif
                }
            }
        }


        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.IsHidden"/> property value changes.
        /// </summary>
        /// <param name="column">
        /// The corresponding column on which the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.IsHidden"/> property value changes occurs.
        /// </param>
        protected virtual void OnColumnHiddenChanged(TreeGridColumn column)
        {

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
        protected virtual UIElement CreatePopupContent(TreeGridColumn column)
        {
            var textBlock = new TextBlock { Text = column.HeaderText };
            return textBlock;
        }

        /// <summary>
        /// Gets the display rectangle for the popup.
        /// </summary>
        /// <returns>
        /// The display rectangle for the popup.
        /// </returns>  
        protected Rect GetPopupRect()
        {
            var rect = this.PopupContent.GetControlRect(this.treeGrid);

            if (this.treeGrid.FlowDirection == FlowDirection.RightToLeft)
                rect.X -= this.PopupContent.ActualWidth;

            return rect;
        }

        /// <summary>
        /// Gets the display rectangle for the header row in SfTreeGrid.
        /// </summary>
        /// <returns>
        /// The display rectangle for the header row.
        /// </returns> 
        protected Rect GetHeaderRowRect()
        {
            if (this.treeGrid.RowGenerator.Items.Count > 0)
            {
                double actualHeight = 0;
                var headerRows = this.treeGrid.RowGenerator.Items.Where(row => row.RowType == TreeRowType.HeaderRow);
                var lastHeaderRow = this.treeGrid.RowGenerator.Items.LastOrDefault(row => row.RowType == TreeRowType.HeaderRow);
                var firstHeaderRow = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == TreeRowType.HeaderRow);

                headerRows.ForEach(row =>
                {
                    if (row != null && row.RowElement != null)
                        actualHeight += row.RowElement.ActualHeight;
                });

                if (firstHeaderRow != null && firstHeaderRow.RowElement != null && lastHeaderRow != null)
                {
                    Rect lastRow = lastHeaderRow.RowElement.GetControlRect(this.treeGrid);

                    Rect firstRow = firstHeaderRow.RowElement.GetControlRect(this.treeGrid);

                    var rect = new Rect(lastRow.X, firstRow.Y, lastRow.Width, actualHeight);

                    return rect;
                }
            }
            return Rect.Empty;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets the corresponding <see cref="Syncfusion.UI.Xaml.Grid.TreeGridRegion"/> at the specified point co-ordinate value.
        /// </summary>
        /// <param name="point">
        /// The position to get the corresponding tree grid region.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.TreeGridRegion"/> at the specified point.
        /// </returns>    
        public virtual TreeGridRegion PointToGridRegion(Point point)
        {
            var popupRect = GetPopupRect();
            var headerRowRect = GetHeaderRowRect();
            headerRowRect.Intersect(popupRect);
            if (headerRowRect.Height >= 5 && headerRowRect.Contains(point))
                return TreeGridRegion.Header;

            var gridRect = this.treeGrid.GetControlRect(this.treeGrid);

            if (gridRect.Contains(point))
                return TreeGridRegion.Grid;
            else
                return TreeGridRegion.None;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDragDropController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDragDropController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (this.PopupContent != null)
            {
                this.HidePopup();
                this.PopupContent.PopupContentPositionChanged -= OnPopupContentPositionChanged;
                this.PopupContent.PopupContentDropped -= OnPopupContentDropped;
                this.PopupContent.PopupContentResizing -= this.treeGrid.ColumnResizingController.OnPopupContentResizing;
                this.PopupContent.PopupContentResized -= this.treeGrid.ColumnResizingController.OnPopupContentResized;
                this.PopupContent.Dispose();
                this.PopupContent = null;
            }
            this.DraggablePopup = null;
            this.upIndicator = null;
            this.downIndicator = null;
            this.DragLeftLine = null;
            this.DragRightLine = null;
            this.horizontalDoubleAnimation = null;
            this.verticalDoubleAnimation = null;
            this.reverseAnimationStoryboard = null;
            if (treeGrid != null)
            {
                if (this.treeGrid.ColumnResizingController != null)
                    this.treeGrid.ColumnResizingController.dragLine = null;
                this.treeGrid = null;
            }
        }


        #endregion

    }
}
