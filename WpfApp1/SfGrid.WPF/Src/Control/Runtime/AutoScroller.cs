#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Threading.Tasks;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;
#else
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Threading;
#endif

namespace Syncfusion.UI.Xaml.Grid
{

    /// <summary>
    /// Represents a class that provides base implementation for automatic scrolling of content in SfDataGrid.    
    /// </summary>
    public class AutoScroller
    {
        private AutoScrollOrientation autoScrolling = AutoScrollOrientation.None;
        private Rect autoScrollBounds = Rect.Empty;
        DispatcherTimer autoScrollTimer = null;
        TimeSpan intervalTime = new TimeSpan(0, 0, 0, 0, 300);
        private RowColumnIndex currentScrollIndex = RowColumnIndex.Empty;
        Size insideScrollMargin = new Size(10, 10);       

#if WinRT || UNIVERSAL
        internal Point MousePoint { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller"/> class.
        /// </summary>
        public AutoScroller()
        {
            IsEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the auto-scrolling is enabled in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the auto-scrolling is enabled; otherwise, <b>false</b>.
        /// </value>        
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the mouse position during move operation performed in VisualContainer.
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
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> .
        /// </summary>
        public VisualContainer VisualContainer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the horizontal scrollbar value of <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> .
        /// </summary>        
        public IScrollBar HScrollBar
        {
            get
            {
                return VisualContainer.HScrollBar;
            }
        }

        /// <summary>
        /// Gets the vertical scrollbar value of <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> .
        /// </summary>      
        public IScrollBar VScrollBar
        {
            get
            {
                return VisualContainer.VScrollBar;
            }
        }
        #region AutoScrolling
        /// <summary>
        /// Gets or sets the orientation of auto-scrolling.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoScrollOrientation"/> that specifies the orientation of auto-scrolling.
        /// </value>
#if !WinRT && !UNIVERSAL
        [Browsable(false)]
#endif
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
        /// Occurs when the auto-scrolling is being performed in SfDataGrid..
        /// </summary>
        /// <remarks>
        /// If you want to prevent auto-scrolling, handle this event
        /// and reset the AutoScrolling property.
        /// </remarks>
#if !WinRT && !UNIVERSAL
        [Description("Occurs when AutoScrolling property is changed."),
        Category("Behavior")]
#endif
        public event EventHandler AutoScrollingChanged;

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller.AutoScrollingChanged"/> event.
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
#if !WinRT && !UNIVERSAL
        [Browsable(false)]
#endif
        public Rect AutoScrollBounds
        {
            get
            {
                if (autoScrollBounds.IsEmpty)
                    return new Rect(new Point(0, 0), VisualContainer.RenderSize);
                return autoScrollBounds;
            }
            set
            {
                autoScrollBounds = value;
            }
        }



        /// <summary>
        /// Gets the display rectangle of inside scroll area. The control will scroll if the user drag
        /// the mouse outside of VisualContainer area.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Rect"/> that contains the rectangle information of inside scroll area.
        /// </value>
#if !WinRT && !UNIVERSAL
        [Browsable(false)]
#endif
        public virtual Rect InsideScrollBounds
        {
            get
            {
                if (this.VisualContainer.FrozenColumns > 0 || this.VisualContainer.FrozenRows > 0 || this.VisualContainer.FooterColumns > 0 || this.VisualContainer.FooterRows > 0)
                {
                    var rect = this.VisualContainer.GetClipRect(ScrollAxisRegion.Body, ScrollAxisRegion.Body);
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
#if !WinRT && !UNIVERSAL
        [Browsable(false)]
#endif
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
        /// Resets the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller.InsideScrollMargin"/> property to its default value.
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
#if WinRT || UNIVERSAL
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
#if WinRT || UNIVERSAL
                autoScrollTimer.Tick += autoScrollTimer_Tick;
#else
                autoScrollTimer.Tick += new EventHandler(autoScrollTimer_Tick);
#endif
                autoScrollTimer.Start();
                

                MouseMovePosition = new Point(0, 0);

            }
        }

#if WinRT || UNIVERSAL
        void autoScrollTimer_Tick(object sender, object e)
#else
        void autoScrollTimer_Tick(object sender, EventArgs e)
#endif
        {
            if (this.VisualContainer == null || this.VisualContainer.RowsGenerator == null)
            {
                //Need to skip AutoScrolling when gird is disposed
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
            var dataGrid = (VisualContainer.RowsGenerator as RowGenerator).Owner;
            if (AutoScrolling != AutoScrollOrientation.None &&
                !autoScrollBounds.IsEmpty &&
                !insideScrollBounds.IsEmpty || autoScrollBounds.Contains(mousePoint))
            {
                currentScrollIndex = dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;
                var pointToCellIndex = VisualContainer.PointToCellRowColumnIndex(mousePoint, true);             

                if ((AutoScrolling & AutoScrollOrientation.Horizontal) != 0)
                {
                    bool rightToLeft = false;
                    var firstCellIndex = dataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex();
                    var lastCellIndex = dataGrid.SelectionController.CurrentCellManager.GetLastCellIndex();

                    if (mousePoint.X < insideScrollBounds.Left &&
                        (!rightToLeft && HScrollBar.Value > HScrollBar.Minimum
                        || rightToLeft && HScrollBar.Value + HScrollBar.LargeChange <= HScrollBar.Maximum || dataGrid.FrozenColumnCount > 0))
                    {
                        if (dataGrid.FrozenColumnCount > 0 && currentScrollIndex.ColumnIndex == pointToCellIndex.ColumnIndex)
                            return;

                        isLineLeft = true;
                        if (currentScrollIndex.ColumnIndex > firstCellIndex)
                            currentScrollIndex.ColumnIndex = VisualContainer.ScrollColumns.GetPreviousScrollLineIndex(currentScrollIndex.ColumnIndex);
                        VisualContainer.ScrollColumns.ScrollToPreviousLine();
                    }
                    else if (mousePoint.X > insideScrollBounds.Right - insideScrollMargin.Width &&
                        (!rightToLeft && HScrollBar.Value + HScrollBar.LargeChange <= HScrollBar.Maximum
                        || rightToLeft && HScrollBar.Value > HScrollBar.Minimum || dataGrid.FooterColumnCount > 0))
                    {
                        if (dataGrid.FooterColumnCount > 0 && currentScrollIndex.ColumnIndex == pointToCellIndex.ColumnIndex)
                            return;

                        isLineRight = true;
                        //AutoScrolling Would be happen, while no other rows are selected. if we dont select the row the currentScrollIndex.ColumnIndex is -1 so the exception throws.
                        //Hence currentScrollIndex.ColumnIndex is checked with the firstCellIndex.
                        if (currentScrollIndex.ColumnIndex < lastCellIndex && currentScrollIndex.ColumnIndex > firstCellIndex) 
                            currentScrollIndex.ColumnIndex = VisualContainer.ScrollColumns.GetNextScrollLineIndex(currentScrollIndex.ColumnIndex);
                        VisualContainer.ScrollColumns.ScrollToNextLine();
                    }
                }

                if ((AutoScrolling & AutoScrollOrientation.Vertical) != 0)
                {
                    if (mousePoint.Y < insideScrollBounds.Top &&
                        (VScrollBar.Value > VScrollBar.Minimum || dataGrid.FrozenRowsCount > 0))
                    {
                        
                        if (dataGrid.FrozenRowsCount > 0 && currentScrollIndex.RowIndex == pointToCellIndex.RowIndex)
                            
                            return;

                        isLineUp = true;
                        if (currentScrollIndex.RowIndex > dataGrid.GetFirstRowIndex())
                            currentScrollIndex.RowIndex = this.GetPreviousRow(currentScrollIndex.RowIndex, dataGrid);
                        VisualContainer.ScrollRows.ScrollToPreviousLine();
                    }
                    else if (mousePoint.Y > insideScrollBounds.Bottom - insideScrollMargin.Width 
                        && (VScrollBar.Value + VScrollBar.LargeChange <= VScrollBar.Maximum || VScrollBar.Value > VScrollBar.Minimum || dataGrid.FooterRowsCount > 0))
                    {
                        
                        if (dataGrid.FooterRowsCount > 0 && currentScrollIndex.RowIndex == pointToCellIndex.RowIndex)                            
                            return;

                        // WPF-15889 - to prevent scrolling when row is last row
                        if (dataGrid is DetailsViewDataGrid)
                        {
                            var _rowColumnIndex = VisualContainer.PointToCellRowColumnIndex(mousePoint, true);
                            var rowCount = VisualContainer.RowCount - 1 - dataGrid.DetailsViewDefinition.Count;
                            if (_rowColumnIndex.RowIndex > 0 && _rowColumnIndex.RowIndex < rowCount)
                            {
                                isLineDown = true;
                                //AutoScrolling would be happen, while no other rows are selected. if we dont select the row the currentScrollIndex.RowIndex is -1 so exception throws.
                                //Hence currentScrollIndex.RowIndex is checked with the  dataGrid.GetFirstRowIndex().
                                if (currentScrollIndex.RowIndex < dataGrid.GetLastRowIndex() && currentScrollIndex.RowIndex > dataGrid.GetFirstRowIndex())
                                    currentScrollIndex.RowIndex = this.GetNextRow(currentScrollIndex.RowIndex, dataGrid);
                                VisualContainer.ScrollRows.ScrollToNextLine();
                            }
                            else if (_rowColumnIndex.RowIndex == rowCount)
                            {
                                var line = dataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(_rowColumnIndex.RowIndex);
                                if (line.IsClipped)
                                {
                                    isLineDown = true;
                                    //AutoScrolling would be happen, while no other rows are selected. if we dont select the row the currentScrollIndex.RowIndex is -1 so exception throws.
                                    //Hence currentScrollIndex.RowIndex is checked with the  dataGrid.GetFirstRowIndex().

                                    if (currentScrollIndex.RowIndex < dataGrid.GetLastRowIndex() && currentScrollIndex.RowIndex > dataGrid.GetFirstRowIndex())
                                        currentScrollIndex.RowIndex = this.GetNextRow(currentScrollIndex.RowIndex, dataGrid);
                                    dataGrid.VisualContainer.ScrollRows.ScrollInView(_rowColumnIndex.RowIndex);

                                }
                            }
                        }
                        else
                        {
                            isLineDown = true;
                            //AutoScrolling Would be happen, while no other rows are selected. if we dont select the row the currentScrollIndex.RowIndex is -1 so exception throws.
                            //Hence currentScrollIndex.RowIndex is checked with the  dataGrid.GetFirstRowIndex().
                            if (currentScrollIndex.RowIndex < dataGrid.GetLastRowIndex() && currentScrollIndex.RowIndex > dataGrid.GetFirstRowIndex())
                                currentScrollIndex.RowIndex = this.GetNextRow(currentScrollIndex.RowIndex, dataGrid);
                            VisualContainer.ScrollRows.ScrollToNextLine();

                        }                       
                    }
                }
                
                if (isLineDown || isLineUp || isLineLeft || isLineRight)
                {
                    VisualContainer.InvalidateMeasureInfo();
                    this.RaiseAutoScrollerValueChanged(isLineUp, isLineDown, isLineLeft, isLineRight, currentScrollIndex);
                   
                }
#if WPF
                //VisualContainer enter event is not fired as expected in WPF, hence this code is used to release the mouse capture.
                else if (this.InsideScrollBounds.Contains(mousePoint) && this.VisualContainer.IsMouseCaptured)
                {
                    this.VisualContainer.ReleaseMouseCapture();
                    this.AutoScrolling = AutoScrollOrientation.None;
                }
#endif
            }
        }

        //UWP-711 When the next row index is DetailsViewIndex, the autoscroller will fails. Hence the below method is used to get next row index.
        private int GetNextRow(int currentIndex,SfDataGrid dataGrid)
        {
            currentIndex = VisualContainer.ScrollRows.GetNextScrollLineIndex(currentIndex);
            while (dataGrid.IsInDetailsViewIndex(currentIndex))
                currentIndex = VisualContainer.ScrollRows.GetNextScrollLineIndex(currentIndex);
            return currentIndex;
        }
        //UWP-711 When the previous row index is DetailsViewIndex, the autoscroller will fails. Hence the below method is used to get previous row index. 
        private int GetPreviousRow(int previousIndex,SfDataGrid dataGrid)
        {
            previousIndex = VisualContainer.ScrollRows.GetPreviousScrollLineIndex(previousIndex);
            while (dataGrid.IsInDetailsViewIndex(previousIndex))
                previousIndex = VisualContainer.ScrollRows.GetPreviousScrollLineIndex(previousIndex);
            return previousIndex;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller.AutoScrollerValueChanged"/> event.
        /// </summary>
        /// <param name="isLineUp">
        /// Indicates whether the content is scrolled upward to the SfDataGrid during drag selection.
        /// </param>
        /// <param name="isLineDown">
        /// Indicates whether the content is scrolled downward to the SfDataGrid during drag selection.
        /// </param>
        /// <param name="isLineLeft">
        /// Indicates whether the content is scrolled left to the SfDataGrid during drag selection.
        /// </param>
        /// <param name="isLineRight">
        /// Indicates whether the content is scrolled right to the SfDataGrid during drag selection.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding scroll rowcolumnindex while performing dragging in SfDataGrid.   
        /// </param>
        protected virtual void RaiseAutoScrollerValueChanged(bool isLineUp, bool isLineDown, bool isLineLeft, bool isLineRight, RowColumnIndex rowColumnIndex)
        {
            if (this.AutoScrollerValueChanged != null)
            {
                this.AutoScrollerValueChanged(this, new AutoScrollerValueChangedEventArgs(isLineUp, isLineDown, isLineLeft, isLineRight, rowColumnIndex, this));
            }
        }

        /// <summary>
        /// Occurs when the drag selection is performed .
        /// </summary>
        /// <remarks>
        /// Drag selection is enabled when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> is Multiple or Extended.
        /// </remarks>
        public event AutoScrollerValueChangedEventHandler AutoScrollerValueChanged;

        /// <summary>
        /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller.AutoScrollerValueChanged"/> event.
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
