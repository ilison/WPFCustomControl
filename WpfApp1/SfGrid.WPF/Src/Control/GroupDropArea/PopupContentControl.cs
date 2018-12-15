#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if !WPF
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    [TemplatePart(Name = "PART_LeftThumbGripper", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_RightThumbGripper", Type = typeof(Thumb))]
#if WPF
    [TemplatePart(Name = "PART_DragIndicator", Type = typeof(System.Windows.Controls.Grid))]
#endif
    public class PopupContentControl : ContentControl, IDisposable
    {
        #region Fields
        Thumb LeftThumb;
        Thumb RightThumb;
        bool isInResize;
        bool canDrop;           
        internal PopupContentPositionChanged PopupContentPositionChanged;
        internal PopupContentDropped PopupContentDropped;
        internal PopupContentResizing PopupContentResizing;
        internal PopupContentResized PopupContentResized;
        Control dataGrid;
        private bool isdisposed = false;
#if WPF
        internal System.Windows.Controls.Grid DragIndicator;
        Point pointFromGrid;
#else
        double mouseHorizontalPosition;
        double mouseVerticalPosition;
#endif
        #endregion

        #region Property

        // Provides the popup is dragged from GroupDropArea or not
        public bool IsDragFromGroupDropArea { get; internal set; }

        // Provides the popup is dragged from ColumnChooser or not
        public bool IsDragFromColumnChooser { get; set; }

        internal Rect InitialRect { get; set; }

        // Provides the popup is in touch or not
        public bool IsOpenInTouch { get; internal set; }
        
        public Visibility LeftResizeThumbVisibility
        {
            get { return (Visibility)GetValue(LeftResizeThumbVisibilityProperty); }
            set { SetValue(LeftResizeThumbVisibilityProperty, value); }
        }

        public static readonly DependencyProperty LeftResizeThumbVisibilityProperty =
            DependencyProperty.Register("LeftResizeThumbVisibility", typeof(Visibility), typeof(PopupContentControl), new PropertyMetadata(Visibility.Visible));

        public Visibility RightResizeThumbVisibility
        {
            get { return (Visibility)GetValue(RightResizeThumbVisibilityProperty); }
            set { SetValue(RightResizeThumbVisibilityProperty, value); }
        }

        public static readonly DependencyProperty RightResizeThumbVisibilityProperty =
            DependencyProperty.Register("RightResizeThumbVisibility", typeof(Visibility), typeof(PopupContentControl), new PropertyMetadata(Visibility.Visible));

        public double ThumbWidth
        {
            get { return (double)GetValue(ThumbWidthProperty); }
            set { SetValue(ThumbWidthProperty, value); }
        }
        
        public static readonly DependencyProperty ThumbWidthProperty =
            DependencyProperty.Register("ThumbWidth", typeof(double), typeof(PopupContentControl), new PropertyMetadata(20d));

        #endregion

        #region Constructor

        public PopupContentControl(Control dataGrid)
        {
            this.DefaultStyleKey = typeof(PopupContentControl);
            this.dataGrid = dataGrid;
#if WPF
            this.IsManipulationEnabled = true;
#endif
        }

        #endregion

        #region override methods

#if !WPF
        protected override void OnApplyTemplate() 
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
#if WPF            
            this.DragIndicator = base.GetTemplateChild("PART_DragIndicator") as System.Windows.Controls.Grid;
#endif

            if (RightThumb != null)
            {
                this.RightThumb.DragDelta -= OnRightThumbDragDelta;
                this.RightThumb.DragCompleted -= OnRightThumbDragCompleted;
            }
            if (this.LeftThumb != null)
            {
                this.LeftThumb.DragDelta -= OnLeftThumbDragDelta;
                this.LeftThumb.DragCompleted -= OnLeftThumbDragCompleted;
            }

            this.LeftThumb = base.GetTemplateChild("PART_LeftThumbGripper") as Thumb;
            this.RightThumb = base.GetTemplateChild("PART_RightThumbGripper") as Thumb;
            if (RightThumb != null)
            {
                this.RightThumb.DragDelta += OnRightThumbDragDelta;
                this.RightThumb.DragCompleted += OnRightThumbDragCompleted;
            }
            if (this.LeftThumb != null)
            {
                this.LeftThumb.DragDelta += OnLeftThumbDragDelta;
                this.LeftThumb.DragCompleted += OnLeftThumbDragCompleted;
            }
            if (beforeLoaded)
            {
                ApplyState(true);
                beforeLoaded = false;
            }
        }

#if !WPF
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
#else
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
#endif
        {
            if (!isInResize)
            {
                e.Handled = true;
            }
#if !WPF
            this.CapturePointer(e.Pointer);
            base.OnPointerPressed(e);
#else
            this.CaptureMouse();
            base.OnPreviewMouseDown(e);
#endif
        }
        
#if !WPF
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
#else
        protected override void OnPreviewMouseUp(System.Windows.Input.MouseButtonEventArgs e)
#endif
        {
#if !WPF
            if (!isInResize && (canDrop || e.Pointer.PointerDeviceType == PointerDeviceType.Mouse))
#else
            if (!isInResize && (canDrop))
#endif
            {
                if (this.PopupContentDropped != null)
                {
                    this.PopupContentDropped(e.GetPosition(this.dataGrid));
                }
                canDrop = false;
            }
#if !WPF
            this.ReleasePointerCapture(e.Pointer);
            base.OnPointerReleased(e);
#else
            this.ReleaseMouseCapture();
            base.OnPreviewMouseUp(e);
#endif
        }

#if WPF
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            if (!isInResize && (canDrop))
            {
                if (this.PopupContentDropped != null)
                    this.PopupContentDropped(pointFromGrid);
                canDrop = false;
            }
            this.ReleaseAllTouchCaptures();
            if (dataGrid is SfDataGrid)
            {
                (dataGrid as SfDataGrid).GridColumnDragDropController.HidePopup();
                (dataGrid as SfDataGrid).GridColumnDragDropController.CloseDragIndication();
            }   
            base.OnManipulationCompleted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            List<IManipulator> touchDevice = e.Manipulators.ToList();
            if (!isInResize)
            {
                pointFromGrid = touchDevice[0].GetPosition(this.dataGrid);
                Point p = touchDevice[0].GetPosition(this.dataGrid);
                DragPopup(p);
                e.Handled = true;
            }
            this.CaptureTouch(touchDevice[0] as TouchDevice);
            base.OnManipulationDelta(e);
        }

        protected override void OnPreviewMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            if (!isInResize)
            {
                Point p = e.GetPosition(this.dataGrid);
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                {
                    DragPopup(p);
                    e.Handled = true;
                }
            }
            this.CaptureMouse();
            base.OnPreviewMouseMove(e);
        }

#else
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            if (!isInResize)
            {
                PointerPoint pp = e.GetCurrentPoint(null);
                Point p = e.GetPosition(this.dataGrid);
                if (pp.Properties.IsLeftButtonPressed)
                {
                    DragPopup(p);
                    e.Handled = true;
                }
                mouseVerticalPosition = pp.Position.Y;
                mouseHorizontalPosition = pp.Position.X;
            }
            this.CapturePointer(e.Pointer);
            base.OnPointerMoved(e);
        }   
#endif

        #endregion

        #region internal method

        internal void ResetMousePosition()
        {
#if !WPF
            mouseVerticalPosition = 0;
            mouseHorizontalPosition = 0;
#endif
            isInResize = false;
        }

        bool beforeLoaded;
        internal void ApplyState(bool isOpen)
        {
            if (LeftThumb == null)
            {
                beforeLoaded = true;
                return;
            }

            VisualStateManager.GoToState(this, isOpen ? "Open" : "Drag", true);
        }

        #endregion

        #region private methods
        
        private void OnRightThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            isInResize = true;
            double deltaHorizontal = Math.Min(-e.HorizontalChange, this.ActualWidth);
            bool changesize = false;
            if (this.PopupContentResizing != null)
                changesize = this.PopupContentResizing(false, -deltaHorizontal);
            if (changesize)
            {
                var width = this.Width;
                width -= deltaHorizontal - (deltaHorizontal * 0.05);
                if (width >= 0)
                    this.Width = width;
            }
        }

        private void OnLeftThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            isInResize = true;
            double deltaHorizontal = Math.Min(e.HorizontalChange, this.ActualWidth);
            bool changesize = false;
            if (this.PopupContentResizing != null)
                changesize = this.PopupContentResizing(true, deltaHorizontal);
            if (changesize)
            {
                var width = this.Width;
                width -= deltaHorizontal - (deltaHorizontal * 0.05);
                if (width >= 0)
                    this.Width = width;
            }
        }

        private void OnRightThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            isInResize = false;
            double deltaHorizontal = Math.Min(-e.HorizontalChange, this.ActualWidth -this.MinWidth);
            bool changesize = false;
            if (this.PopupContentResized != null)
                changesize = PopupContentResized(false, -deltaHorizontal);
        }

        private void OnLeftThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            isInResize = false;
            double deltaHorizontal = Math.Min(e.HorizontalChange, this.ActualWidth - this.MinWidth);
            bool changesize = false;
            if (this.PopupContentResized != null)
                changesize = PopupContentResized(true, deltaHorizontal);
        }

        private void ChangeThumbVisibility()
        {
            if (this.LeftResizeThumbVisibility == Visibility.Visible)
                this.LeftResizeThumbVisibility = Visibility.Collapsed;
            if (this.RightResizeThumbVisibility == Visibility.Visible)
                this.RightResizeThumbVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Invoked when the popup is dragging on SfDataGrid/SfTreeGrid.
        /// </summary>
        /// <param name="p">
        /// Indicates whether the mouse point over on SfDataGrid/SfTreeGrid.
        /// </param>

        private void DragPopup(Point p)
        {
#if !WPF
            if (((this.LeftThumb != null && this.LeftThumb.Visibility == Windows.UI.Xaml.Visibility.Visible) || (this.RightThumb != null && this.RightThumb.Visibility == Windows.UI.Xaml.Visibility.Visible)) && !canDrop)
            {
                mouseVerticalPosition = 0;
                mouseHorizontalPosition = 0;
                isInResize = true;
                return;
            }
            if (mouseVerticalPosition <= 0 || mouseHorizontalPosition <= 0)
                return;
#endif
            if (this.PopupContentPositionChanged != null)
                this.PopupContentPositionChanged(p.X, p.Y, p);

            if (!canDrop && !this.InitialRect.IsEmpty)
            {
                //var rect = this.GetControlRect(this.dataGrid);
                //rect.X = Math.Round(rect.X + ((rect.Width * 0.05) / 2));
                //rect.Y = Math.Round(rect.Y + ((rect.Height * 0.05)) / 2);
                //if ((rect.X >= this.InitialRect.X + 5) || (rect.X <= this.InitialRect.X - 5) ||
                //    (rect.Y >= this.InitialRect.Y + 5) || (rect.Y <= this.InitialRect.Y - 5))

                // Need to allow dropping when dragging is happened
                canDrop = true;
                ChangeThumbVisibility();
            }
        }

        #endregion

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PopupContentControl"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PopupContentControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (this.LeftThumb != null)
            {
                this.LeftThumb.DragDelta -= OnLeftThumbDragDelta;
                this.LeftThumb.DragCompleted -= OnLeftThumbDragCompleted;
            }
            if (RightThumb != null)
            {
                this.RightThumb.DragDelta -= OnRightThumbDragDelta;
                this.RightThumb.DragCompleted -= OnRightThumbDragCompleted;
            }
            if (isDisposing)
            {
                this.LeftThumb = null;
                this.RightThumb = null;
                this.PopupContentDropped = null;
                this.PopupContentPositionChanged = null;
                this.PopupContentResized = null;
                this.PopupContentResizing = null;
                this.dataGrid = null;
            }
            isdisposed = true;
        }
    }

    public class UpIndicatorContentControl : ContentControl, IDisposable
    {
        public UpIndicatorContentControl()
        {
            base.DefaultStyleKey = typeof(UpIndicatorContentControl);
        }

#if !WPF

        public bool IsOpen { get; set; }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (IsOpen)
                VisualStateManager.GoToState(this, "Open", false);
        }
#endif

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
                        
        }
    }

    public class DownIndicatorContentControl : ContentControl, IDisposable
    {
        public DownIndicatorContentControl()
        {
            base.DefaultStyleKey = typeof(DownIndicatorContentControl);
        }

#if !WPF

        public bool IsOpen { get; set; }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (IsOpen)
                VisualStateManager.GoToState(this, "Open", false);
        }
#endif

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {

        }
    }

    internal delegate void PopupContentPositionChanged(double HorizontalDelta, double VerticalDelta, Point mousePointOverGrid);

    internal delegate bool PopupContentResizing(bool IsinLeft, double Width);

    internal delegate bool PopupContentResized(bool IsinLeft, double ActualWidth);

    internal delegate void PopupContentDropped(Point pointOverGrid);

}
