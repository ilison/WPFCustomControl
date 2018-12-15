#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Threading;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Represents a class that provides base implementation for automatic scrolling of content in SfTreeGrid.    
    /// </summary>
    public class TreeGridAutoScroller : IDisposable
    {
        private AutoScrollOrientation autoScrolling = AutoScrollOrientation.None;
        private Rect autoScrollBounds = Rect.Empty;
        DispatcherTimer autoScrollTimer = null;
        TimeSpan intervalTime = new TimeSpan(0, 0, 0, 0, 100);
        private RowColumnIndex currentScrollIndex = RowColumnIndex.Empty;
        Size insideScrollMargin = new Size(10, 10);

#if UWP
        internal Point MousePoint { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller"/> class.
        /// </summary>
        public TreeGridAutoScroller()
        {
            IsEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the auto-scrolling is enabled in SfTreeGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the auto-scrolling is enabled; otherwise, <b>false</b>.
        /// </value>        
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mouse position during move operation performed in TreeGridPanel.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Point"/> that specifies the corresponding mouse move position.
        /// </value>
        protected internal Point MouseMovePosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> .
        /// </summary>
        public TreeGridPanel TreeGridPanel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the horizontal scrollbar value of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> .
        /// </summary>        
        public IScrollBar HScrollBar
        {
            get
            {
                return TreeGridPanel.HScrollBar;
            }
        }

        /// <summary>
        /// Gets the vertical scrollbar value of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> .
        /// </summary>      
        public IScrollBar VScrollBar
        {
            get
            {
                return TreeGridPanel.VScrollBar;
            }
        }
        #region AutoScrolling
        /// <summary>
        /// Gets or sets the orientation of auto-scrolling.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoScrollOrientation"/> that specifies the orientation of auto-scrolling.
        /// </value>
        public AutoScrollOrientation AutoScrolling
        {
            get
            {
                return autoScrolling;
            }
            set
            {
                if (value != autoScrolling)
                {
                    autoScrolling = value;
                    if (autoScrolling == AutoScrollOrientation.None)
                    {
                        currentScrollIndex = RowColumnIndex.Empty;
                        StopAutoScrollTimer();
                    }
                    else if (IsEnabled)
                        StartAutoScrollTimer();
                    OnAutoScrollingChanged(EventArgs.Empty);
                }
            }
        }


        /// <summary>
        /// Occurs when the auto-scrolling is being performed in SfTreeGrid..
        /// </summary>
        /// <remarks>
        /// If you want to prevent auto-scrolling, handle this event
        /// and reset the AutoScrolling property.
        /// </remarks>
        public event EventHandler AutoScrollingChanged;

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller.AutoScrollingChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnAutoScrollingChanged(EventArgs e)
        {
            if (AutoScrollingChanged != null)
                AutoScrollingChanged(this, e);
        }

        /// <summary>
        /// Gets or sets the display rectangle of outer scroll area. Typically the client area of the control.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Rect"/> that contains the rectangle information about the outer scroll area.
        /// </value>
        public Rect AutoScrollBounds
        {
            get
            {
                if (autoScrollBounds.IsEmpty)
                    return new Rect(new Point(0, 0), TreeGridPanel.RenderSize);
                return autoScrollBounds;
            }
            set
            {
                autoScrollBounds = value;
            }
        }



        /// <summary>
        /// Gets the display rectangle of inside scroll area. The control will scroll if the user drag
        /// the mouse outside of TreeGridPanel area.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Rect"/> that contains the rectangle information of inside scroll area.
        /// </value>
        public virtual Rect InsideScrollBounds
        {
            get
            {
                if (this.TreeGridPanel.FrozenColumns > 0 || this.TreeGridPanel.FrozenRows > 0 || this.TreeGridPanel.FooterColumns > 0 || this.TreeGridPanel.FooterRows > 0)
                {
                    var rect = this.TreeGridPanel.GetClipRect(ScrollAxisRegion.Body, ScrollAxisRegion.Body);
                    if (rect.IsEmpty)
                        return Rect.Empty;
                    return new Rect(rect.X + insideScrollMargin.Width, rect.Y + insideScrollMargin.Height, rect.Width - insideScrollMargin.Width, rect.Height - insideScrollMargin.Width);
                }
                return Rect.Empty;
            }
        }

        /// <summary>
        /// Gets or Sets the default margin for the scrolling area when the user moves the mouse point to the margin 
        /// between InsideScrollBounds and AutoScrollBounds.
        /// </summary>
        public Size InsideScrollMargin
        {
            get
            {
                return insideScrollMargin;
            }
            set
            {
                insideScrollMargin = value;
            }
        }

        /// <summary>
        /// Resets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller.InsideScrollMargin"/> property to its default value.
        /// </summary>
        public void ResetInsideScrollMargins()
        {
            insideScrollMargin = new Size(10, 10);
        }

        /// <summary>
        /// Gets or sets the timer interval for auto scrolling.
        /// </summary>
        public TimeSpan IntervalTime
        {
            get
            {
                return intervalTime;
            }
            set
            {
                intervalTime = value;
            }
        }


        void StopAutoScrollTimer()
        {
            if (autoScrollTimer != null)
            {
#if UWP
                autoScrollTimer.Tick -= autoScrollTimer_Tick;
#else
                autoScrollTimer.Tick -= new EventHandler(autoScrollTimer_Tick);
#endif
                autoScrollTimer.Stop();
                autoScrollTimer = null;
            }
        }

        void StartAutoScrollTimer()
        {

            if (autoScrollTimer == null)
            {
                autoScrollTimer = new DispatcherTimer();
                autoScrollTimer.Interval = IntervalTime;
#if UWP
                autoScrollTimer.Tick += autoScrollTimer_Tick;
#else
                autoScrollTimer.Tick += new EventHandler(autoScrollTimer_Tick);
#endif
                autoScrollTimer.Start();


                MouseMovePosition = new Point(0, 0);

            }
        }

#if UWP
        void autoScrollTimer_Tick(object sender, object e)
#else
        void autoScrollTimer_Tick(object sender, EventArgs e)
#endif
        {
            if (this.TreeGridPanel == null || this.TreeGridPanel.RowGenerator == null)
            {
                if (this.AutoScrolling != AutoScrollOrientation.None)
                    this.AutoScrolling = AutoScrollOrientation.None;
                return;
            }
            if (MouseMovePosition.X > 0 || MouseMovePosition.Y > 0)
                AutoScroll(MouseMovePosition);

        }

        /// <summary>
        /// Scrolls automatically for the specified mouse point.
        /// </summary>
        /// <param name="mousePoint">
        /// The mouse point to scroll automatically.
        /// </param>
        protected virtual void AutoScroll(Point mousePoint)
        {
            bool isLineLeft = false, isLineRight = false, isLineUp = false, isLineDown = false;
            Rect autoScrollBounds = AutoScrollBounds;
            Rect insideScrollBounds = InsideScrollBounds;
            var treeGrid = (TreeGridPanel.RowGenerator as TreeGridRowGenerator).Owner;
            if (AutoScrolling != AutoScrollOrientation.None &&
                !autoScrollBounds.IsEmpty &&
                !insideScrollBounds.IsEmpty || autoScrollBounds.Contains(mousePoint))
            {
                currentScrollIndex = treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;
                var pointToCellIndex = TreeGridPanel.PointToCellRowColumnIndex(mousePoint, true);

                if ((AutoScrolling & AutoScrollOrientation.Horizontal) != 0)
                {
                    bool rightToLeft = false;
                    var firstCellIndex = treeGrid.SelectionController.CurrentCellManager.GetFirstCellIndex();
                    var lastCellIndex = treeGrid.SelectionController.CurrentCellManager.GetLastCellIndex();

                    if (mousePoint.X < insideScrollBounds.Left &&
                        (!rightToLeft && HScrollBar.Value > HScrollBar.Minimum
                        || rightToLeft && HScrollBar.Value + HScrollBar.LargeChange <= HScrollBar.Maximum || treeGrid.FrozenColumnCount > 0))
                    {
                        if (treeGrid.FrozenColumnCount > 0 && currentScrollIndex.ColumnIndex == pointToCellIndex.ColumnIndex)
                            return;

                        isLineLeft = true;
                        if (currentScrollIndex.ColumnIndex > firstCellIndex)
                            currentScrollIndex.ColumnIndex = TreeGridPanel.ScrollColumns.GetPreviousScrollLineIndex(currentScrollIndex.ColumnIndex);
                        TreeGridPanel.ScrollColumns.ScrollToPreviousLine();
                    }
                    else if (mousePoint.X > insideScrollBounds.Right - insideScrollMargin.Width &&
                        (!rightToLeft && HScrollBar.Value + HScrollBar.LargeChange <= HScrollBar.Maximum
                        || rightToLeft && HScrollBar.Value > HScrollBar.Minimum))
                    {
                        if (currentScrollIndex.ColumnIndex == pointToCellIndex.ColumnIndex)
                            return;

                        isLineRight = true;
                        if (currentScrollIndex.ColumnIndex < lastCellIndex && currentScrollIndex.ColumnIndex > firstCellIndex)
                            currentScrollIndex.ColumnIndex = TreeGridPanel.ScrollColumns.GetNextScrollLineIndex(currentScrollIndex.ColumnIndex);
                        TreeGridPanel.ScrollColumns.ScrollToNextLine();
                    }
                }

                if ((AutoScrolling & AutoScrollOrientation.Vertical) != 0)
                {
                    if (mousePoint.Y < insideScrollBounds.Top &&
                        (VScrollBar.Value > VScrollBar.Minimum))
                    {

                        if (currentScrollIndex.RowIndex == pointToCellIndex.RowIndex)
                            return;

                        isLineUp = true;
                        if (currentScrollIndex.RowIndex > treeGrid.GetFirstDataRowIndex())
                            currentScrollIndex.RowIndex = this.GetPreviousRow(currentScrollIndex.RowIndex, treeGrid);
                        TreeGridPanel.ScrollRows.ScrollToPreviousLine();
                    }
                    else if (mousePoint.Y > insideScrollBounds.Bottom - insideScrollMargin.Width
                        && (VScrollBar.Value + VScrollBar.LargeChange <= VScrollBar.Maximum || VScrollBar.Value > VScrollBar.Minimum))
                    {

                        if (currentScrollIndex.RowIndex == pointToCellIndex.RowIndex)
                            return;

                        isLineDown = true;
                        if (currentScrollIndex.RowIndex < treeGrid.GetLastDataRowIndex() && currentScrollIndex.RowIndex > treeGrid.GetFirstDataRowIndex())
                            currentScrollIndex.RowIndex = this.GetNextRow(currentScrollIndex.RowIndex, treeGrid);
                        TreeGridPanel.ScrollRows.ScrollToNextLine();

                    }
                }

                if (isLineDown || isLineUp || isLineLeft || isLineRight)
                {
                    TreeGridPanel.InvalidateMeasureInfo();
                    this.RaiseAutoScrollerValueChanged(isLineUp, isLineDown, isLineLeft, isLineRight, currentScrollIndex);

                }
#if WPF
                else if (this.InsideScrollBounds.Contains(mousePoint) && this.TreeGridPanel.IsMouseCaptured)
                {
                    this.TreeGridPanel.ReleaseMouseCapture();
                    this.AutoScrolling = AutoScrollOrientation.None;
                }
#endif
            }
        }

        private int GetNextRow(int currentIndex, SfTreeGrid treeGrid)
        {
            currentIndex = TreeGridPanel.ScrollRows.GetNextScrollLineIndex(currentIndex);
            return currentIndex;
        }

        private int GetPreviousRow(int previousIndex, SfTreeGrid treeGrid)
        {
            previousIndex = TreeGridPanel.ScrollRows.GetPreviousScrollLineIndex(previousIndex);
            return previousIndex;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller.AutoScrollerValueChanged"/> event.
        /// </summary>
        /// <param name="isLineUp">
        /// Indicates whether the content is scrolled upward to the SfTreeGrid during drag selection.
        /// </param>
        /// <param name="isLineDown">
        /// Indicates whether the content is scrolled downward to the SfTreeGrid during drag selection.
        /// </param>
        /// <param name="isLineLeft">
        /// Indicates whether the content is scrolled left to the SfTreeGrid during drag selection.
        /// </param>
        /// <param name="isLineRight">
        /// Indicates whether the content is scrolled right to the SfTreeGrid during drag selection.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding scroll row column index while performing dragging in SfTreeGrid.   
        /// </param>
        protected virtual void RaiseAutoScrollerValueChanged(bool isLineUp, bool isLineDown, bool isLineLeft, bool isLineRight, RowColumnIndex rowColumnIndex)
        {
            if (this.AutoScrollerValueChanged != null)
            {
                this.AutoScrollerValueChanged(this, new AutoScrollerValueChangedEventArgs(isLineUp, isLineDown, isLineLeft, isLineRight, rowColumnIndex, this));
            }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller"/> class.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release all the resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.TreeGridPanel = null;
                if (this.autoScrollTimer != null)
                {
                    this.autoScrollTimer.Tick -= autoScrollTimer_Tick;
                    this.autoScrollTimer = null;
                }
            }
        }

        /// <summary>
        /// Occurs when the drag selection is performed .
        /// </summary>
        /// <remarks>
        /// Drag selection is enabled when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionMode"/> is Multiple or Extended.
        /// </remarks>
        public event AutoScrollerValueChangedEventHandler AutoScrollerValueChanged;

        /// <summary>
        /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller.AutoScrollerValueChanged"/> event.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// A <see cref="Syncfusion.UI.Xaml.Grid.AutoScrollerValueChangedEventArgs"/> that contains the event data.
        /// </param>
        public delegate void AutoScrollerValueChangedEventHandler(object sender, AutoScrollerValueChangedEventArgs e);


        #endregion
    }
}

