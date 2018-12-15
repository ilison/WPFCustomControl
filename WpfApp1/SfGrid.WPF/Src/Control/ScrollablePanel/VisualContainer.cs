#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.Generic;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Linq;
#if WinRT || UNIVERSAL
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
#else
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    [ClassReference(IsReviewed = false)]
    public class VisualContainer : Panel, IScrollableInfo, IDisposable
    {
        #region Fields
        ScrollInfo _hScrollBar;
        internal double previousArrangeWidth;
        ScrollInfo _vScrollBar;
        ScrollViewer _scrollOwner;
        IPaddedEditableLineSizeHost rowHeightsProvider;
        IPaddedEditableLineSizeHost columnWidthsProvider;
        ScrollAxisBase _scrollRows;
        ScrollAxisBase _scrollColumns;
        bool verticalPixelScroll = true;
        bool horizontalPixelScroll = true;
        internal Brush DragBorderBrush;
        internal Thickness DragBorderThickness;
        Size PreviousAvailableSize = Size.Empty;
        Line horizontalLine = new Line();
        Line verticalLine = new Line();

        internal bool NeedToRefreshColumn;
        internal bool SuspendManipulationScroll;
        private bool isdisposed = false;

#if !WPF
        internal Func<KeyEventArgs, bool> ContainerKeydown;
#endif

#if WP
        private double prevZoomScale = 1;
        private int zoomScrollRowIndex = -1;
        private int zoomScrollColumnIndex = -1;
        private double initialHLineOrigin = -1;
        private bool HoffsetIncrement = false;
        Point midPoint = new Point();
        private double thumbWidth = 45;
        internal Action<double> SetZoomScale;
        private Matrix _transformation;
        internal double _ZoomScale = 1;
        ScaleTransform scaleTransform = new ScaleTransform() { ScaleX = 1.0, ScaleY = 1.0 };
        private MatrixTransform _matrixTransform;
        private Point _cumulativeTranslation;
        private bool _dragStarted;
        private const double PreFeedbackTranslationX = 50d;
        private const double PreFeedbackTranslationY = 50d;
        private double _initialThreshold = 16.0;
        private DateTime _lastTimeStamp;
        private Point _velocity;
        private PanningInfo _panningInfo;
#elif WinRT || UNIVERSAL
        internal Size ViewPortSize;		
#endif

        #endregion

        #region Property

        public IRowGenerator RowsGenerator { get; set; }
        public RowHeightManager RowHeightManager { get; set; }

        private double verticalPadding;
        public double VerticalPadding
        {
            get
            {
                return verticalPadding;
            }
            set
            {
                if (!SuspendManipulationScroll)
                    verticalPadding = value;
            }
        }

        private double horizontalPadding;
        public double HorizontalPadding
        {
            get
            {
                return horizontalPadding;
            }
            set
            {
                if (!SuspendManipulationScroll)
                    horizontalPadding = value;
            }
        }

        public ScrollInfo HScrollBar
        {
            get { return _hScrollBar ?? (_hScrollBar = new ScrollInfo()); }
        }

        public ScrollInfo VScrollBar
        {
            get { return _vScrollBar ?? (_vScrollBar = new ScrollInfo()); }
        }

        public IPaddedEditableLineSizeHost RowHeights
        {
            get
            {
                return this.rowHeightsProvider;
            }
        }

        public IPaddedEditableLineSizeHost ColumnWidths
        {
            get
            {
                return columnWidthsProvider;
            }
        }

        public ScrollAxisBase ScrollRows
        {
            get
            {
                if (_scrollRows == null)
                {
                    _scrollRows = CreateScrollAxis(Orientation.Vertical, verticalPixelScroll, VScrollBar, RowHeights);
                    _scrollRows.Name = "ScrollRows";
                    }
                return this._scrollRows;
            }
        }

        public ScrollAxisBase ScrollColumns
        {
            get
            {
                if (_scrollColumns == null)
                {
                    _scrollColumns = CreateScrollAxis(Orientation.Horizontal, horizontalPixelScroll, HScrollBar, ColumnWidths);
                    _scrollColumns.Name = "ScrollColumns";
                    _scrollColumns.Changed += OnScrollColumnsChanged;
                }

                return this._scrollColumns;
            }
        }

        public bool VerticalPixelScroll
        {
            get
            {
                return ScrollRows.IsPixelScroll;
            }
            set
            {
                if (VerticalPixelScroll != value)
                {
                    verticalPixelScroll = value;
                    ResetScrollRows();
                }
            }
        }

        public bool HorizontalPixelScroll
        {
            get
            {
                return ScrollColumns.IsPixelScroll;
            }
            set
            {
                if (HorizontalPixelScroll != value)
                {
                    horizontalPixelScroll = value;
                    ResetScrollColumns();
                }
            }
        }

        public int RowCount
        {
            get
            {
                return this.rowHeightsProvider.LineCount;
            }
            set
            {
                if (value > RowCount)
                    InsertRows(RowCount, value - RowCount);
                else if (value < RowCount)
                    RemoveRows(value, RowCount - value);
            }
        }

        public int ColumnCount
        {
            get
            {
                return this.columnWidthsProvider.LineCount;
            }
            set
            {
                if (value > ColumnCount)
                    InsertColumns(ColumnCount, value - ColumnCount);
                else if (value < ColumnCount)
                    RemoveColumns(value, ColumnCount - value);
            }
        }

        public int FrozenRows
        {
            get
            {
                return this.rowHeightsProvider.HeaderLineCount;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Negative values not allowed.");
                this.rowHeightsProvider.HeaderLineCount = value;
            }
        }

        public int FooterRows
        {
            get
            {
                return this.rowHeightsProvider.FooterLineCount;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Negative Values not Allowed.");
                this.rowHeightsProvider.FooterLineCount = value;
            }
        }

        public int FrozenColumns
        {
            get
            {
                return this.columnWidthsProvider.HeaderLineCount;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Negative values are not allowed.");
                this.columnWidthsProvider.HeaderLineCount = value;
            }
        }

        public int FooterColumns
        {
            get
            {
                return this.columnWidthsProvider.FooterLineCount;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Negative values are not allowed.");
                this.columnWidthsProvider.FooterLineCount = value;
            }
        }

        public bool AllowFixedGroupCaptions { get; set; }

        #endregion

        #region Dependency Properties
#if WinRT || UNIVERSAL
        /// <summary>
        /// Gets or sets the vertical offset.
        /// </summary>
        /// <value>
        /// The vertical offset.
        /// </value>
        public double VerticalScrollBarOffset
        {
            get { return (double)GetValue(VerticalScrollBarOffsetProperty); }
            set { SetValue(VerticalScrollBarOffsetProperty, value); }
        }

        /// <summary>
        /// The vertical offset property
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarOffsetProperty =
            DependencyProperty.Register("VerticalScrollBarOffset", typeof(double), typeof(VisualContainer), new PropertyMetadata(double.NegativeInfinity,
            (o, args) =>
            {
                var visualContainer = o as VisualContainer;
                if (visualContainer != null)
                    visualContainer.SetVerticalOffset((double)args.NewValue);
            }));

        /// <summary>
        /// Gets or sets the hortizontal offset.
        /// </summary>
        /// <value>
        /// The hortizontal offset.
        /// </value>
        public double HortizontalScrollBarOffset
        {
            get { return (double)GetValue(HortizontalScrollBarOffsetProperty); }
            set { SetValue(HortizontalScrollBarOffsetProperty, value); }
        }

        /// <summary>
        /// The hortizontal offset property
        /// </summary>
        public static readonly DependencyProperty HortizontalScrollBarOffsetProperty =
            DependencyProperty.Register("HortizontalScrollBarOffset", typeof(double), typeof(VisualContainer), new PropertyMetadata(double.NegativeInfinity,
                (o, args) =>
                {
                    var visualContainer = o as VisualContainer;
                    if (visualContainer != null)
                        visualContainer.SetHorizontalOffset((double)args.NewValue);
                }));
#endif

        
#if WP
        internal double ZoomScale
        {
            get { return _ZoomScale; }
            set
            {
                _ZoomScale = value;
                ApplyLayoutTransform();
            }
        }

        public double ScrollableHeight
        {
            get { return Math.Max((double)0.0, (double)(this.ExtentHeight - this.ViewportHeight)); }
            internal set { base.SetValue(ScrollableHeightProperty, value); }
        }

        public double ScrollableWidth
        {
            get { return Math.Max((double)0.0, (double)(this.ExtentWidth - this.ViewportWidth)); }
            internal set { base.SetValue(ScrollableWidthProperty, value); }
        }

        public static readonly DependencyProperty ScrollableHeightProperty =
            DependencyProperty.Register("ScrollableHeight", typeof(double), typeof(ScrollViewer), null);

        public static readonly DependencyProperty ScrollableWidthProperty =
            DependencyProperty.Register("ScrollableWidth", typeof(double), typeof(ScrollViewer), null);
#endif
        #endregion

        #region Ctor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VisualContainer()
        {
            RowsGenerator = null;
            RowHeightManager = new RowHeightManager();
            this.rowHeightsProvider = OnCreateRowHeights();
            this.columnWidthsProvider = OnCreateColumnWidths();
#if WinRT || UNIVERSAL
            this.rowHeightsProvider.DefaultLineSize = 45;
            this.columnWidthsProvider.DefaultLineSize = 120;
#elif WP
            _matrixTransform = new MatrixTransform();
            RenderTransform = _matrixTransform;
            this.rowHeightsProvider.DefaultLineSize = 75;
            this.columnWidthsProvider.DefaultLineSize = 180;
#else
            this.rowHeightsProvider.DefaultLineSize = 24;
            this.columnWidthsProvider.DefaultLineSize = 150;
#endif

            this.Children.Add(horizontalLine);
            this.Children.Add(verticalLine);
            WireScrollLineEvents();
#if !WPF
            WireEvents();
#endif
        }
        #endregion

        #region Override methods

        #region MeasureOverride

        bool IsDoubleValueSet(DependencyProperty dp)
        {
            object value = GetValue(dp);
            return value != DependencyProperty.UnsetValue && !double.IsNaN((double)value);
        }
 
        internal bool suspendUpdates = false;

#if UWP
        internal void SetRowHeights()
#else
        internal void SetRowHeights(bool isInfiniteHeight = false)
#endif
        {
            if (this.RowsGenerator.Owner == null)
                return;

            if (!this.RowsGenerator.Owner.CanQueryRowHeight())
                return;

            var visibleRows = this.ScrollRows.GetVisibleLines();
                     

            //Header   
            int headerStart = 0;
            int headerEnd = ScrollRows.HeaderLineCount -1;                                   
            RowHeightHelper(headerStart, headerEnd, RowRegion.Header);

            //Update Range for Header
            RowHeightManager.UpdateRegion(headerStart,headerEnd,RowRegion.Header);

            // Footer
            //Condition added to avoid exception when only header is avialble in view, but ScrollRows having same count number aFirstBodyVisiblieIndex.
            int footerStart = visibleRows.Count > visibleRows.FirstBodyVisibleIndex && ScrollRows.FooterLineCount > 0 ? visibleRows[visibleRows.FirstFooterVisibleIndex].LineIndex : -1;
            int footerEnd = ScrollRows.FooterLineCount > 0 ? ScrollRows.LineCount - 1 : -1;
            RowHeightHelper(footerStart, footerEnd, RowRegion.Footer);

            //UpdateRegion for Footer    
            RowHeightManager.UpdateRegion(footerStart, footerEnd, RowRegion.Footer);

            int startIndex = 0; int endIndex = 0;
            if (visibleRows.Count > visibleRows.FirstBodyVisibleIndex)
                startIndex = visibleRows[visibleRows.firstBodyVisibleIndex].LineIndex;
            else
                return;
            if (visibleRows.LastBodyVisibleIndex > 0)
            endIndex = visibleRows[visibleRows.LastBodyVisibleIndex].LineIndex;
            suspendUpdates = true;
            (this.RowHeights as LineSizeCollection).SuspendUpdates();

            // Body              
            if (visibleRows.FirstFooterVisibleIndex - 1 > 0)
            {
                var bodystart = visibleRows[visibleRows.FirstBodyVisibleIndex].Origin;
                var bodyend = visibleRows[visibleRows.FirstFooterVisibleIndex - 1].Corner;
                var bodystartlineindex = visibleRows[visibleRows.FirstBodyVisibleIndex].LineIndex;

                var current = bodystart;
                var currentEnd = endIndex;
#if WPF
                for (int index = bodystartlineindex;
                            ((current < bodyend && index < ScrollRows.FirstFooterLineIndex) || ((current < (ViewportHeight + bodystart) || isInfiniteHeight) && index < ScrollRows.FirstFooterLineIndex));
                            index++)
#else
                for (int index = bodystartlineindex;
                            ((current < bodyend && index < ScrollRows.FirstFooterLineIndex) || (current < (ViewportHeight + bodystart) && index < ScrollRows.FirstFooterLineIndex));
                            index++)
#endif
                {
                    var height = RowHeights[index];
                    if (!RowHeightManager.Contains(index, RowRegion.Body))
                    {
                        if (this.RowsGenerator.QueryRowHeight(index, ref height))
                        {
                            this.RowHeights.SetRange(index, index, height);
                        }
                    }
                    current += height;
                    currentEnd = index;
                }

                //Update range for Body
                RowHeightManager.UpdateRegion(bodystartlineindex, currentEnd, RowRegion.Body);
            }

            if (RowHeightManager.DirectoryRows.Count > 0)
            {
                foreach (var rowind in RowHeightManager.DirectoryRows)
                {
                    if (rowind < 0 || rowind >= RowHeights.LineCount)
                        continue;
                    var height = RowHeights[rowind];
                    if (this.RowsGenerator.QueryRowHeight(rowind, ref height))
                    {
                        this.RowHeights.SetRange(rowind, rowind, height);
                    }
                }
                RowHeightManager.DirectoryRows.Clear();
            }
#if WPF
            //175697 - when grid is loaded into scrollviewer, no changes need to be done in ColumnSizer.
            if (isInfiniteHeight)
                this.RowsGenerator.Owner.GridColumnSizer.Suspend();
#endif
            (this.RowHeights as LineSizeCollection).ResumeUpdates();
#if WPF
            if (isInfiniteHeight)
                this.RowsGenerator.Owner.GridColumnSizer.Resume();
#endif
            ScrollRows.UpdateScrollBar();
            suspendUpdates = false;               
        }

        internal void RowHeightHelper(int startIndex, int endIndex, RowRegion region)
        {
            if (startIndex < 0 || endIndex < 0)
                return;
            for (int index = startIndex; index <= endIndex; index++)
            {
                if (!RowHeightManager.Contains(index, region))
                {
                    double height = 0;
                    if (this.RowsGenerator.QueryRowHeight(index, ref height))
                    {
                        this.RowHeights.SetRange(index, index, height);
                    }
                }
            }
        }

#if WPF
        internal int SetRowHeights(int fromRowIndex, ExpandDirection direction)
#else
        internal int SetRowHeights(int fromRowIndex, Key direction)
#endif
        {
            if (fromRowIndex < 0)
                return 0;
            var extent = 0.0;
            int currentIndex = 0;  
            int index = fromRowIndex;
            var datagrid = this.RowsGenerator.Owner as SfDataGrid;
            suspendUpdates = true;
            (this.RowHeights as LineSizeCollection).SuspendUpdates();
            while (extent < ViewportHeight && datagrid!=null && index >= datagrid.GetFirstNavigatingRowIndex() && index <= datagrid.GetLastNavigatingRowIndex())
                {
                    var height = this.RowHeights[index];
                    if (!this.RowHeightManager.Contains(index, RowRegion.Body))
                    {
                        if (this.RowsGenerator.QueryRowHeight(index, ref height))
                        {
                            this.RowHeights.SetRange(index, index, height);
                        }
                    }
                    extent += height;
                    currentIndex = index;
#if WPF
                index = (direction == ExpandDirection.Down) ? ++index : --index;
#else
                index = (direction == Key.Down) ? ++index : --index;
#endif
            }
            (this.RowHeights as LineSizeCollection).ResumeUpdates();
            suspendUpdates = false;
                return currentIndex;
        
        }

#if UWP
ScrollContentPresenter PART_ScrollContentPresenter = null;
#endif
        protected override Size MeasureOverride(Size constraint)
        {
            if (RowsGenerator == null)
#if UWP
                return double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height) ? new Size() : constraint;
#else
                return base.MeasureOverride(constraint);

            //To know that the constraint height is infinity.
            bool isInfiniteWidth = false;
            bool isInfiniteHeight = false;
#endif

            if ((double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height)) && (ScrollOwner != null || this.ScrollableOwner != null))
            {
#if WPF
                isInfiniteWidth = double.IsInfinity(constraint.Width) ? ScrollOwner != null ? true : false : false;
                isInfiniteHeight = double.IsInfinity(constraint.Height) ? ScrollOwner != null ? true : false : false;
#endif
                if (!IsDoubleValueSet(FrameworkElement.HeightProperty) && double.IsInfinity(constraint.Height))
                {
                    if (ScrollRows is PixelScrollAxis)
                        constraint.Height = Math.Min(constraint.Height, ((PixelScrollAxis) ScrollRows).TotalExtent - RowHeights.PaddingDistance);
                }
                if (!IsDoubleValueSet(FrameworkElement.WidthProperty) && double.IsInfinity(constraint.Width))
                {
                    if (ScrollColumns is PixelScrollAxis)
                        constraint.Width = Math.Min(constraint.Width, ((PixelScrollAxis)ScrollColumns).TotalExtent);
                }
            }
#if UWP
            var availableSize = GetAvailableSize(constraint);
            UpdateAxis(availableSize);
#else
            UpdateAxis(constraint);
#endif

            PreGenerateItems();
#if UWP
            if (this.ColumnCount > 0 && previousArrangeWidth != availableSize.Width)
            {
                // WRT-4883  For DetailsViewDataGrid, column sizer is calculated based on parent grid available width, so here 0 is passed as available width
                if (this.RowsGenerator.Owner is DetailsViewDataGrid)
                    this.RowsGenerator.ApplyColumnSizeronInitial(0);
                else
                    this.RowsGenerator.ApplyColumnSizeronInitial(availableSize.Width);
                previousArrangeWidth = availableSize.Width;
            }
            this.SetRowHeights();
#else
            double currentAvailableWidth = constraint.Width;
            //when maximize the winodow or add record at run time in underlying collection we need to refresh column sizer based on previousArrangeWidth and availableWidth
            //if (this.ColumnCount > 0 && (previousArrangeWidth == 0.0 && constraint.Width != 0))
            if (this.ColumnCount > 0 && previousArrangeWidth != constraint.Width && !(this.RowsGenerator.Owner is DetailsViewDataGrid))
            {
                //Before Fix-21074 : this.RowHeights.TotalExtent > ViewPortHeight
                //ViewPortHeight did not include header and footer height. so hasVerticalScrollbar check is incorrectly calculated for some scenarios.
                //Hence changed the condition with ScrollRows.RenderSize.
                bool hasVerticalScrollBar = false;
                // When the constraint height is infinity from given size, while using ScrollViewer type of control, we no need consider about Scroll bar width to a calculation.
                if (isInfiniteWidth)
                    hasVerticalScrollBar = this.RowHeights.TotalExtent > ScrollRows.RenderSize;
                else if (this.ScrollableOwner != null)
                    hasVerticalScrollBar = this.ScrollableOwner.ComputedVerticalScrollBarVisibility == Visibility.Collapsed && this.RowHeights.TotalExtent > ScrollRows.RenderSize;
                else if (this.ScrollOwner != null)
                    hasVerticalScrollBar = this.ScrollOwner.ComputedVerticalScrollBarVisibility == Visibility.Collapsed && this.RowHeights.TotalExtent > ScrollRows.RenderSize;

                double _vScrollBarWidth = 0;
                if (hasVerticalScrollBar)
                {
                    _vScrollBarWidth = SystemParameters.ScrollWidth;
                    if (this.ScrollOwner != null)
                    {
                        var vscroll = this.ScrollOwner.Template.FindName("PART_VerticalScrollBar", this.ScrollOwner) as ScrollBar;
                        _vScrollBarWidth = vscroll != null && vscroll.ActualWidth != 0 ? vscroll.ActualWidth : SystemParameters.ScrollWidth;
                    }
                }

                if (constraint.Width - _vScrollBarWidth != previousArrangeWidth)
                {
                    this.RowsGenerator.Owner.GridColumnSizer.InitialRefresh(constraint.Width - _vScrollBarWidth, false);
                }
                currentAvailableWidth = constraint.Width - _vScrollBarWidth;
            }
            else if (this.ColumnCount > 0 && this.RowsGenerator.Owner is DetailsViewDataGrid && previousArrangeWidth != constraint.Width)
            {
                this.RowsGenerator.ApplyColumnSizeronInitial(constraint.Width);
                currentAvailableWidth = constraint.Width;
            }
            // WPF-35776 isInfiniteHeight is passed as parameter, to determine whether the grid is loaded inside stackpanel or scrollViewer
            this.SetRowHeights(isInfiniteHeight);
#endif

            // WPF- 21039 set the height and width based on window parameters such as SizeToContent.
            if (!(this.RowsGenerator.Owner is DetailsViewDataGrid))
            {
#if UWP
                if (this.RowHeights.TotalExtent < constraint.Height)
#else
                //(isInfiniteHeight && this.RowHeights.TotalExtent > constraint.Height) condition and UpdateAxis code added to fix WPF-35776 - Vertical ScrollBar is shown incorrectly while using QueryRowHeight.
                //This is because the constraint height and the renderer size is not updated properly.
                if (this.RowHeights.TotalExtent < constraint.Height || (isInfiniteHeight && this.RowHeights.TotalExtent > constraint.Height))
#endif
                {
                    constraint.Height = this.RowHeights.TotalExtent;
#if WPF
                    if (isInfiniteHeight)
                        UpdateAxis(constraint);
#endif
                }
                if (this.ColumnWidths.TotalExtent < constraint.Width)
                    constraint.Width = this.ColumnWidths.TotalExtent;
            }
           
#if UWP
            if ((!PreviousAvailableSize.IsEmpty && PreviousAvailableSize.Width != availableSize.Width) || NeedToRefreshColumn)
#else
            if ((!PreviousAvailableSize.IsEmpty && previousArrangeWidth != currentAvailableWidth) || NeedToRefreshColumn)
#endif
            {
                EnsureItems(true);
                NeedToRefreshColumn = false;
            }
            else
                EnsureItems(false);

#if UWP
            PreviousAvailableSize = availableSize;
            horizontalLine.Measure(availableSize);
            verticalLine.Measure(availableSize);
#else
            previousArrangeWidth = currentAvailableWidth;
            PreviousAvailableSize = constraint;
            horizontalLine.Measure(constraint);
            verticalLine.Measure(constraint);
#endif
            InvalidatScrollInfo();

            MeasureRows();
            return constraint;
        }

#if UWP
        private Size GetAvailableSize(Size constraint)
        {
            Size availableSize = new Size();
            //Due to RowSpan and ColumnSpan settings in ScrollContentPresenter or ScrollableContentPresenter of ScrollViewer or ScrollableContentViewer, content will be overlapped 
            // on ScrollBar. If we remove that settings, not able to see the last row completely, since available size calculation has been done above with complete ViewPort size of
            //ScrollViewer or ScrollableContentViewer. Hence the size has been calculated with ScrollBar size too. Therefore available size has been assigned with ScrollContentPresenter or ScrollableContentPresenter of ScrollViewer or ScrollableContentViewer
            if (ScrollOwner != null)
            {
                // UWP-5643 - ScrollOwner width and height varied based on resolution. So ScrollOwner's ActualWidth and ActualHeight are taken as available size.
                availableSize.Width = (ScrollOwner.ActualWidth == 0 && !double.IsNaN(ScrollOwner.ActualWidth) && !double.IsInfinity(ScrollOwner.ActualWidth)) ? (!double.IsInfinity(ViewPortSize.Width) ? ViewPortSize.Width : constraint.Width) : ScrollOwner.ActualWidth;
                availableSize.Height = (ScrollOwner.ActualHeight == 0 && !double.IsNaN(ScrollOwner.ActualHeight) && !double.IsInfinity(ScrollOwner.ActualHeight)) ? (!double.IsInfinity(ViewPortSize.Height) ? ViewPortSize.Height : constraint.Height) : ScrollOwner.ActualHeight;

                //UWP-6250- ScrollOwner's ActualWidth has been changed each time(with the screen resolution 125% and the TargetFrameworkVersion windows 10 anniversary edition (10.0; Build 14393)).Hence the ColumnSizer will be refreshed and MeasureOverrride called infinity times(LayoutCycle detected)
                //So we have assigned the ViewPortSize Width as availableSizeWidth if the ScrollOwner's ActualWidth greater than ViewPortSize.Width >
                if (ScrollOwner.ActualWidth != ViewPortSize.Width && ScrollOwner.ActualWidth > ViewPortSize.Width)
                    availableSize.Width = ViewPortSize.Width;

                if (PART_ScrollContentPresenter == null)
                {
                    PART_ScrollContentPresenter =
                        GridUtil.FindDescendantChildByName(ScrollOwner, "ScrollContentPresenter") as
                            ScrollContentPresenter;
                }
                if (PART_ScrollContentPresenter != null)
                {
                    if (Windows.UI.Xaml.Controls.Grid.GetRowSpan(PART_ScrollContentPresenter) == 1)
                    {
                        if (!double.IsNaN(PART_ScrollContentPresenter.ActualWidth)
                          && PART_ScrollContentPresenter.ActualWidth != 0.0 && !double.IsInfinity(PART_ScrollContentPresenter.ActualWidth))
                        {
                            availableSize.Width = PART_ScrollContentPresenter.ActualWidth;
                        }
                    }
                    if (Windows.UI.Xaml.Controls.Grid.GetColumnSpan(PART_ScrollContentPresenter) == 1)
                    {
                        if (!double.IsNaN(PART_ScrollContentPresenter.ActualHeight) &&
                        PART_ScrollContentPresenter.ActualHeight != 0.0 && !double.IsInfinity(PART_ScrollContentPresenter.ActualHeight))
                        {
                            availableSize.Height = PART_ScrollContentPresenter.ActualHeight;
                        }
                    }
                }
            }
            else
            {
                availableSize.Width = (ScrollableOwner != null && !double.IsInfinity(ViewPortSize.Width)) ? ViewPortSize.Width : constraint.Width;
                availableSize.Height = (ScrollableOwner != null && !double.IsInfinity(ViewPortSize.Height)) ? ViewPortSize.Height : constraint.Height;
            }
            return availableSize;
        }
#endif
#endregion

#region ArrangeOverride

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (RowsGenerator == null)
                return finalSize;
            ArrangeRow();
            this.RowsGenerator.RowsArranged(finalSize);
            return finalSize;
        }

        internal void UpdateRegion()
        {
            var visibleRows = ScrollRows.GetVisibleLines();

            //Update Body
            this.RowHeightManager.ResetBody();

            //update Header
            int headerStart = 0;
            int headerEnd = this.RowsGenerator.Owner.HeaderLineCount - 1;
            this.RowHeightManager.UpdateRegion(headerStart, headerEnd, RowRegion.Header);

            //Update footer
            int footerStart = ScrollRows.FooterLineCount > 0 ? (visibleRows[visibleRows.FirstFooterVisibleIndex].LineIndex) + this.RowsGenerator.Owner.FooterRowsCount : -1;
            int footerEnd = ScrollRows.FooterLineCount > 0 ? ScrollRows.LineCount - 1 : -1;
            this.RowHeightManager.UpdateRegion(footerStart, footerEnd, RowRegion.Footer);
        }
#endregion

#if WPF
#region Manipulation Boundry Feedback
        /// <summary>
        /// Called when the <see cref="E:System.Windows.UIElement.ManipulationBoundaryFeedback" /> event occurs.
        /// </summary>
        /// <param name="e">The data for the event.</param>
        protected override void OnManipulationBoundaryFeedback(System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
#endregion
#endif

#endregion

#if !WPF
#region Event Handlers

        /// <summary>
        /// Wires the events.
        /// </summary>
        private void WireEvents()
        {
            this.KeyDown += OnContainerKeyDown;
#if WinRT || UNIVERSAL
            this.ManipulationDelta += OnContainerOnManipulationDelta;
            this.PointerWheelChanged += OnContainerPointerWheelChanged;
#endif
        }

        /// <summary>
        /// UnWires the scroll viewer events.
        /// </summary>
        private void UnWireEvents()
        {
            this.KeyDown -= OnContainerKeyDown;
#if WinRT || UNIVERSAL
            this.ManipulationDelta -= OnContainerOnManipulationDelta;
            this.PointerWheelChanged -= OnContainerPointerWheelChanged;         

            UnwireScrollEvents();
#endif
        }

        private void UnwireScrollEvents()
        {
#if WinRT || UNIVERSAL
            if (_scrollOwner != null)
            {
                _scrollOwner.Loaded -= OnScrollOwnerLoaded;

                var verticalScrollBar = GridUtil.FindDescendantChildByName(ScrollOwner, "VerticalScrollBar");
                if (verticalScrollBar != null)
                    verticalScrollBar.PointerWheelChanged -= OnContainerPointerWheelChanged;
                var scrollBarSeparator = GridUtil.FindDescendantChildByName(ScrollOwner, "ScrollBarSeparator");
                if (scrollBarSeparator != null)
                    scrollBarSeparator.PointerWheelChanged -= OnContainerPointerWheelChanged;
            }

            if (scrollableOwner != null)
            {
                scrollableOwner.Loaded -= OnScrollableOwnerLoaded;

                var verticalScrollBar = GridUtil.FindDescendantChildByName(ScrollableOwner, "PART_VerticalScrollBar");
                if (verticalScrollBar != null)
                    verticalScrollBar.PointerWheelChanged -= OnContainerPointerWheelChanged;
                var scrollBarSeparator = GridUtil.FindDescendantChildByName(ScrollableOwner, "PART_ScrollBarSeparator");
                if (scrollBarSeparator != null)
                    scrollBarSeparator.PointerWheelChanged -= OnContainerPointerWheelChanged;
            }
#endif
#if WP
            if (scrollableOwner != null)
            {
                this.ScrollableOwner.ManipulationStarted -= OnManipulationStarted;
                this.ScrollableOwner.ManipulationDelta -= OnManipulationDelta;
                this.ScrollableOwner.ManipulationCompleted -= OnManipulationCompleted;
            }
#endif
        }
        

        /// <summary>
        /// Containers the key down.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyRoutedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnContainerKeyDown(object sender, KeyEventArgs e)
        {
            ContainerKeydown(e);
        }

#if WinRT || UNIVERSAL
        /// <summary>
        /// Containers the on manipulation delta.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ManipulationDeltaRoutedEventArgs"/> instance containing the event data.</param>
        private void OnContainerOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((e.OriginalSource != this && !(e.OriginalSource is DetailsViewDataGrid)) || e.PointerDeviceType == PointerDeviceType.Mouse)
                return;
            if (ScrollOwner != null)
            {
                if (ScrollOwner.HorizontalScrollMode == ScrollMode.Disabled && ScrollOwner.VerticalScrollMode == ScrollMode.Disabled)
                    return;
                var verticalOffset = e.Delta.Translation.Y;
                var horizontalOffset = e.Delta.Translation.X;
                //this.ScrollOwner.ChangeView(null, VerticalOffset - verticalOffset, null, true);
                //this.ScrollOwner.ChangeView(HorizontalOffset - horizontalOffset, null, null, true);
                this.ScrollOwner.ChangeView(HorizontalOffset - horizontalOffset, VerticalOffset - verticalOffset, null, true);
                e.Handled = true;
            }
            if (ScrollableOwner != null)
            {
                var verticalOffset = e.Delta.Translation.Y;
                var horizontalOffset = e.Delta.Translation.X;
                this.ScrollableOwner.ScrollToVerticalOffset(VerticalOffset - verticalOffset);
                this.ScrollableOwner.ScrollToHorizontalOffset(HorizontalOffset - horizontalOffset);
                e.Handled = true;
            }
        }
        /// <summary>
        /// Called when [container pointer wheel changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PointerRoutedEventArgs"/> instance containing the event data.</param>

        private void OnContainerPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {            
            if (ScrollOwner != null)
            {
                if (ScrollOwner.HorizontalScrollMode == ScrollMode.Disabled && ScrollOwner.VerticalScrollMode == ScrollMode.Disabled)
                    return;
                var verticalOffset = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
                // UWP-4615 - Vertical scrolling are not working, when we placed treegrid inside the scrollviewer.
                if (verticalOffset < 0)
                {
                    //<= condition check to deal with the case where records are less than the viewport height
                    if (this.ScrollOwner.IsVerticalScrollChainingEnabled && this.VScrollBar.Maximum <= this.VScrollBar.LargeChange + this.VScrollBar.Value)
                        return;
                    this.ScrollOwner.ChangeView(null, VerticalOffset - verticalOffset, null, true);
                    e.Handled = true;
                }
                else if (verticalOffset > 0)
                { 
                    if (this.ScrollOwner.IsVerticalScrollChainingEnabled && this.VScrollBar.Minimum == this.VScrollBar.Value)
                       return;
                    this.ScrollOwner.ChangeView(null, VerticalOffset - verticalOffset, null, true);
                    e.Handled = true;
                }
            }
            if (ScrollableOwner != null)
            {
                var verticalOffset = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
                this.ScrollableOwner.ScrollToVerticalOffset(VerticalOffset - verticalOffset);
                e.Handled = true;
            }           
        }

        /// <summary>
        /// Called when Scroll owner loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void OnScrollOwnerLoaded(object sender, RoutedEventArgs e)
        {
            var verticalScrollBar = GridUtil.FindDescendantChildByName(ScrollOwner, "VerticalScrollBar");
            if (verticalScrollBar != null)
                verticalScrollBar.PointerWheelChanged += OnContainerPointerWheelChanged;
            var scrollBarSeparator = GridUtil.FindDescendantChildByName(ScrollOwner, "ScrollBarSeparator");
            if (scrollBarSeparator != null)
                scrollBarSeparator.PointerWheelChanged += OnContainerPointerWheelChanged;
        }

        /// <summary>
        /// Called when ScrollableOwner loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void OnScrollableOwnerLoaded(object sender, RoutedEventArgs e)
        {
            var verticalScrollBar = GridUtil.FindDescendantChildByName(ScrollableOwner, "PART_VerticalScrollBar");
            if (verticalScrollBar != null)
                verticalScrollBar.PointerWheelChanged += OnContainerPointerWheelChanged;
            var scrollBarSeparator = GridUtil.FindDescendantChildByName(ScrollableOwner, "PART_ScrollBarSeparator");
            if (scrollBarSeparator != null)
                scrollBarSeparator.PointerWheelChanged += OnContainerPointerWheelChanged;
        }


#endif
#endregion
#endif

#region Virtual methods

        protected virtual IPaddedEditableLineSizeHost OnCreateRowHeights()
        {
            var lineSizeCollection = new LineSizeCollection();
            return lineSizeCollection;
        }

        protected virtual IPaddedEditableLineSizeHost OnCreateColumnWidths()
        {
            var lineSizeCollection = new LineSizeCollection();
            return lineSizeCollection;
        }

        protected virtual ScrollAxisBase CreateScrollAxis(Orientation orientation, bool pixelScroll, IScrollBar scrollBar, ILineSizeHost lineSizes)
        {
            if (pixelScroll)
                return new PixelScrollAxis(scrollBar, lineSizes, lineSizes as IDistancesHost);
            else
                return new LineScrollAxis(scrollBar, lineSizes);
        }

#endregion

#region Public methods

        public void InsertRows(int insertAtRowIndex, int count)
        {
            this.rowHeightsProvider.InsertLines(insertAtRowIndex, count, null);
        }

        public void RemoveRows(int removeAtRowIndex, int count)
        {
            this.rowHeightsProvider.RemoveLines(removeAtRowIndex, count, null);
        }

        public void InsertColumns(int insertAtColumnIndex, int count)
        {
            var cannotifyrowgenerator = this.ColumnCount != 0;
            this.columnWidthsProvider.InsertLines(insertAtColumnIndex, count, null);
            if (cannotifyrowgenerator)
                this.RowsGenerator.ColumnInserted(insertAtColumnIndex, count);
        }

        public void RemoveColumns(int removeAtColumnIndex, int count)
        {
            var cannotifyrowgenerator = this.ColumnCount != 0;
            this.RowsGenerator.ColumnRemoved(removeAtColumnIndex, count);
            if (cannotifyrowgenerator)
                this.columnWidthsProvider.RemoveLines(removeAtColumnIndex, count, null);
        }

        /// <summary>
        /// Determines the cell under the mouse location.
        /// </summary>
        /// <param name="p">The point in client coordinates.</param>
        /// <returns>
        /// The cells row and column index under the mouse location.
        /// </returns>
        public RowColumnIndex PointToCellRowColumnIndex(Point p, bool allowOutSideLines = false)
        {
            // Need to pass allowOutSideLines as true to return rowcolumn index while re ordering columns.
            VisibleLineInfo visibleRow = ScrollRows.GetVisibleLineAtPoint(p.Y, allowOutSideLines);
            VisibleLineInfo visibleColumn = ScrollColumns.GetVisibleLineAtPoint(p.X, allowOutSideLines);

            if (visibleRow == null || visibleColumn == null)
                return RowColumnIndex.Empty;

            return new RowColumnIndex(visibleRow.LineIndex, visibleColumn.LineIndex);
        }

        /// <summary>
        /// For internal use.
        /// </summary>
        /// <param name="rowRegion">Scroll axis region for row.</param>
        /// <param name="columnRegion">Scroll axis region for column.</param>
        /// <param name="range">Cell range.</param>
        /// <param name="allowEstimatesForOutOfViewRows">If set to true, allows estimate for out of view rows.</param>
        /// <param name="allowEstimatesForOutOfViewColumns">If set to true, allows estimate for out of view columns.</param>
        /// <returns>Visible rectangle for the given range.</returns>
        public Rect RangeToRect(ScrollAxisRegion rowRegion, ScrollAxisRegion columnRegion, RowColumnIndex rowcolumn, bool allowEstimatesForOutOfViewRows, bool allowEstimatesForOutOfViewColumns)
        {
            if (rowcolumn.IsEmpty)
                return Rect.Empty;

            DoubleSpan ySpan = ScrollRows.RangeToPoints(rowRegion, rowcolumn.RowIndex, rowcolumn.RowIndex, allowEstimatesForOutOfViewRows);
            DoubleSpan xSpan = ScrollColumns.RangeToPoints(columnRegion, rowcolumn.ColumnIndex, rowcolumn.ColumnIndex, allowEstimatesForOutOfViewColumns);

            if (ySpan.IsEmpty || xSpan.IsEmpty)
                return Rect.Empty;

            return new Rect(xSpan.Start, ySpan.Start, xSpan.Length, ySpan.Length);
        }

        /// <summary>
        /// Gets the clipping bounds for the specified row and column region.
        /// </summary>
        /// <param name="rowRegion">The row region.</param>
        /// <param name="columnRegion">The column region.</param>
        /// <returns>A <see cref="Rect"/> with clipping bounds.</returns>
        public Rect GetClipRect(ScrollAxisRegion rowRegion, ScrollAxisRegion columnRegion)
        {
            DoubleSpan ySpan = ScrollRows.GetClipPoints(rowRegion);
            DoubleSpan xSpan = ScrollColumns.GetClipPoints(columnRegion);

            if (ySpan.IsEmpty || xSpan.IsEmpty)
                return Rect.Empty;

            return new Rect(xSpan.Start, ySpan.Start, xSpan.Length, ySpan.Length);
        }

        /// <summary>
        /// Gets clipped size from the specified line info.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private double GetClippedSize(VisibleLineInfo line)
        {
            if (line.ClippedSize < 1 || !line.IsClippedCorner || this.FooterRows <= 0)
                return line.ClippedSize;
            return line.ClippedSize - 1;
        }

#endregion

#region Internal methods

        /// <summary>
        /// Clearing Children
        /// </summary>
        /// <remarks>
        /// Row Generator items are cleared when itemsSource Changed so Child should be clear
        /// </remarks>
        internal void OnItemSourceChanged()
        {
            previousArrangeWidth = 0.0;
#if !WP
            // If ReuseRowsOnItemssourceChange is true, Owner is DetailsViewDataGrid and IsInDeserialize is false, no need to clear the Children in VisualContainer
            if (this.RowsGenerator != null && this.RowsGenerator.Owner is DetailsViewDataGrid && this.RowsGenerator.Owner.ReuseRowsOnItemssourceChange && !this.RowsGenerator.Owner.IsInDeserialize && this.RowsGenerator.Owner.ItemsSource != null)
                return;
#endif
            //WPF - 33610 ScrollBar value is not set to minimum when changing the ItemsSource
            ResetScrollBars();
            if (this.Children.Count > 0)
            {
                this.Children.Clear();
            }
            else
                this.InvalidateMeasure();
        }

        internal void SetRowGenerator(RowGenerator rg)
        {
            this.RowsGenerator = rg;
            horizontalLine.Stroke = DragBorderBrush;
            horizontalLine.StrokeThickness = DragBorderThickness.Top;
            verticalLine.Stroke = DragBorderBrush;
            verticalLine.StrokeThickness = DragBorderThickness.Left;
        }

        internal void ResetScrollBars()
        {
            this.ScrollRows.ScrollBar.Value = this.ScrollRows.ScrollBar.Minimum;
        }

        internal void UpdateScrollBars()
        {

            //While updating the Row Column Count we need to update the scroll bar values. Otherwise visiblelines will be calculated wrongly.

            ///While removing hidden row visiblelines not updated as it is not marked as dirty. So ScrollRows.MarkDirty called from here.
            ///In other cases, In ScrollAxisBase scrollBar_PropertyChanged clear the visible lines to reset.
            this.ScrollRows.MarkDirty();
            this.ScrollRows.UpdateScrollBar();
            this.ScrollColumns.UpdateScrollBar();
        }

#endregion

#region Private methods

        private void OnScrollColumnsChanged(object sender, Syncfusion.UI.Xaml.ScrollAxis.ScrollChangedEventArgs e)
        {
            if (e.Action == ScrollChangedAction.LineResized)
            {
                this.NeedToRefreshColumn = true;
                this.RowsGenerator.LineSizeChanged();
                this.InvalidateMeasure();
            }
            else
            {
                this.NeedToRefreshColumn = true;
                this.InvalidateMeasure();
            }
        }

        private void ResetScrollRows()
        {
            if (_scrollRows != null)
            {
                _scrollRows.Dispose();
            }
            _scrollRows = null;
        }

        private void ResetScrollColumns()
        {
            if (_scrollColumns != null)
            {
                _scrollColumns.Dispose();
            }
            _scrollColumns = null;
        }

        public void UpdateAxis(Size availableSize)
        {
            ScrollRows.RenderSize = availableSize.Height;
            ScrollColumns.RenderSize = availableSize.Width;

            if (Clip is RectangleGeometry)
            {
                var rg = Clip;
                Rect rect = rg.Bounds;
                ScrollRows.Clip = new DoubleSpan(rect.Top, rect.Bottom);
                ScrollColumns.Clip = new DoubleSpan(rect.Left, rect.Right);
            }
            else
            {
                ScrollRows.Clip = DoubleSpan.Empty;
                ScrollColumns.Clip = DoubleSpan.Empty;
            }
        }

        private void PreGenerateItems()
        {
            if (this.RowsGenerator.Items.Count != 0 || this.RowCount <= 0)
                return;

            var visibleRows = ScrollRows.GetVisibleLines();
            var visibleColumns = ScrollColumns.GetVisibleLines();
            this.RowsGenerator.PregenerateRows(visibleRows, visibleColumns);
        }

        private void EnsureItems(bool ensureColumns)
        {
            var visibleRows = ScrollRows.GetVisibleLines();
            this.RowsGenerator.EnsureRows(visibleRows);
            if (ensureColumns)
            {
                var visibleColumns = ScrollColumns.GetVisibleLines();
                if (visibleColumns.Count > 0 && visibleColumns.FirstBodyVisibleIndex <= visibleColumns.Count)
                    this.RowsGenerator.EnsureColumns(visibleColumns);
            }

            //Here we substracting 2 drag lines from children count
            if (this.RowsGenerator.Items.Count != (this.Children.Count - 2))
            {
                foreach (IElement row in this.RowsGenerator.Items)
                {
                    if (this.Children.Contains(row.Element))
                        continue;                                                                
                    this.Children.Add(row.Element);                    
                }
            }
        }


        private IRowElement GetPreviousFixedRow(IEnumerable<IRowElement> enumerable, IRowElement currentrowelement, Predicate<IRowElement> condition)
        {
            var enumerator = enumerable.GetEnumerator();
            IRowElement nextrowelement = null;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == currentrowelement)
                    break;
                if (condition(enumerator.Current) && enumerator.Current.Element.Visibility == Visibility.Visible)
                    nextrowelement = enumerator.Current;
            }
            if (nextrowelement != null && (nextrowelement.Element.Clip is RectangleGeometry &&
                                           (nextrowelement.Element.Clip as RectangleGeometry).Rect == Rect.Empty))
            {
                nextrowelement = GetPreviousFixedRow(enumerable, nextrowelement, condition);
            }
            return nextrowelement;
        }


        private void MeasureRows()
        {
            foreach (var item in this.RowsGenerator.Items)
            {
                if (item.Index == -1)
                    continue;

                if (item.Element.Visibility != Visibility.Visible) continue;

                var line = this.GetRowVisibleLineInfo(item.Index);
                if (line == null) continue;
                item.MeasureElement(new Size(this.ScrollColumns.ViewSize, line.Size));
            }
        }
        
        private void ArrangeRow()
        {
            if (this.RowsGenerator == null) return;
            double y = this.ScrollRows.HeaderExtent;

            previousFixedRowElement = null;
            extendedHeaderHeight = this.ScrollRows.HeaderExtent;
            double xPosition = 0, _voffset = 0;
            double panelDelta = 0.0;
            int panelIndex = 0;

            var orderedItems = this.RowsGenerator.Items.OrderBy(row => row.Index);

            var canQueryConveredRange = this.RowsGenerator.Owner.CanQueryCoveredRange();
            if(canQueryConveredRange)
                panelIndex = this.RowCount;
            
            xPosition = 0.15 * HorizontalPadding;
#if UWP
            if (ScrollOwner != null)
            {
                panelDelta = this.VerticalOffset;
                _voffset = this.VerticalOffset;
                xPosition += this.HorizontalOffset;
            }
#endif
             
            foreach (var item in orderedItems)
            {
                if (item.Element.Visibility == Visibility.Collapsed)
                    continue;

                var line = this.GetRowVisibleLineInfo(item.Index);
                if (line != null)
                {
                    Rect rect;
                    if (this.ScrollRows.RenderSize == 0)
                        rect = new Rect(0, 0, 0, 0);
                    else
                    {
                        var yPosition = _voffset;
                        yPosition += (item.RowRegion != RowRegion.Body)
                            ? line.Origin
                            : line.Origin + (0.15 * VerticalPadding);

                        rect = new Rect(xPosition, yPosition, this.ScrollColumns.ViewSize, line.Size);
                        if (this.AllowFixedGroupCaptions && item.IsFixedRow && (yPosition - panelDelta) < extendedHeaderHeight && (previousFixedRowElement == null || (previousFixedRowElement.Level < item.Level)))
                            rect.Y = extendedHeaderHeight + panelDelta;
                    }
                    
                    if ((HorizontalPadding != 0 || VerticalPadding != 0) && (item.RowRegion == RowRegion.Body) && (!AllowFixedGroupCaptions || VerticalPadding > 0))
                    {
                        if (rect.Y <= this.ScrollRows.HeaderExtent)
                            item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, this.ScrollRows.HeaderExtent - rect.Y, this.ScrollColumns.ViewSize, line.Size) };
                        else if ((rect.Y + line.Size) > (this.ScrollRows.ViewSize - this.ScrollRows.FooterExtent))
                        {
                            var height = (this.ScrollRows.ViewSize - this.ScrollRows.FooterExtent) - (rect.Y + line.Size);
                            item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, height, this.ScrollColumns.ViewSize, line.Size) };
                        }
                        else
                            item.Element.Clip = null;
                    }
                    else
                    {
                        if (!AllowFixedGroupCaptions)
                        {
                            if (line.IsClippedBody && line.IsClippedOrigin && line.IsClippedCorner)
                                item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, line.Size - line.ClippedSize - line.ClippedCornerExtent, this.ScrollColumns.ViewSize, line.ClippedSize) };
                            else if (line.IsClippedBody && line.IsClippedCorner)
                                //Top border for Footer rows is not shown while the previous row is in selection, because we have set -1 to Margin for row controls. 
                                //Hence the ClippedSize is reduced to -1 when the row is clipped with selection.
                                item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, 0, this.ScrollColumns.ViewSize, this.GetClippedSize(line)) };
                            else if (line.IsClippedBody && line.IsClippedOrigin)
                                item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, line.Size - line.ClippedSize - line.ClippedCornerExtent, this.ScrollColumns.ViewSize, line.Size) };
                            else
                                item.Element.Clip = null;
                        }
                        else
                            this.ArrangeRowsForFixedGroupCaption(item, line, ref rect, panelDelta, orderedItems);
                    }
                    item.ArrangeElement(rect);
                    item.ArrangeRect = rect;

                    if (canQueryConveredRange)
                    {
                        if (item.RowRegion == RowRegion.Body && (item.RowType == RowType.DefaultRow || item.RowType == RowType.DetailsViewRow || item.RowType == RowType.UnBoundRow))
                        {
#if WPF
                            item.Element.SetValue(Panel.ZIndexProperty, panelIndex);
#else
                            item.Element.SetValue(Canvas.ZIndexProperty, panelIndex);
#endif
                            panelIndex--;
                        }
                        else if (item.RowRegion == RowRegion.Footer)
                        {
#if WPF
                            item.Element.SetValue(Panel.ZIndexProperty, this.RowCount);
#else
                            item.Element.SetValue(Canvas.ZIndexProperty, this.RowCount);
#endif
                        }
                    }
                    // UWP-4785 While adding StackedHeaderRow in runtime, it has to be overlay on the GridHeaderRowControl.
                    // In that case, we can't access the HeaderCell control of Non-StackedHeaderColumn. So, that the 
                    // Sorting perfomrance not happening when we click above the column header. We have reset Z-index
                    // for which one is appear above in the HeaderRegion.
                    if (item.RowRegion == RowRegion.Header)
                    {
#if WPF
                        item.Element.SetValue(Panel.ZIndexProperty, item.Index);
#else
                        item.Element.SetValue(Canvas.ZIndexProperty, item.Index);
#endif
                    }
                    y += line.Size;
                }
#if UWP
                //WRT-4919 - If CurrentRow not in View, then arranging the CurrentRow alone outside of View
                //WRT-6043 - Row is clipped but the Clip is not calculated because we have check item is isEditing
                //so else part is not excuted and clip is not calculated. 
                else if (line == null && item.IsCurrentRow && item is DataRowBase && (item as DataRowBase).IsEditing)
                {
                    var rect = new Rect(0, -this.ScrollRows.DefaultLineSize * 2, this.ScrollColumns.ViewSize,
                        this.ScrollRows.DefaultLineSize);
                    item.ArrangeElement(rect);
                    item.ArrangeRect = rect;
                }
#endif
                else
                {
                    if (!AllowFixedGroupCaptions || !item.IsFixedRow)
                    {
                        var rect = new Rect(0, y, this.ScrollColumns.ViewSize, this.ScrollRows.DefaultLineSize);
                        item.ArrangeElement(rect);
                        if (AllowFixedGroupCaptions)
                            item.ArrangeRect = rect;
                        y += this.ScrollRows.DefaultLineSize;
                    }
                    else
                    {
                        var rect = Rect.Empty;
                        this.ArrangeRowsForFixedGroupCaption(item, null, ref rect, panelDelta, orderedItems);
                    }
                }
            }

            if (this.ScrollRows.ViewSize - this.ScrollRows.FooterExtent <= 0 || this.RowHeights.TotalExtent - this.ScrollRows.FooterExtent - this.ScrollRows.HeaderExtent <= 0)
                return;
            var horizontalDragRect = new Rect(HorizontalPadding * 0.15, this.ScrollRows.HeaderExtent + VerticalPadding * 0.15,
                                               this.ColumnWidths.TotalExtent, this.ScrollRows.ViewSize - this.ScrollRows.FooterExtent);
            var verticalDragRect = new Rect(HorizontalPadding * 0.15, VerticalPadding * 0.15,
                                             this.ScrollColumns.ViewSize, this.RowHeights.TotalExtent - this.ScrollRows.FooterExtent - this.ScrollRows.HeaderExtent);

            horizontalLine.X2 = this.ScrollColumns.ViewSize;
            verticalLine.Y2 = this.ScrollRows.ViewSize;

            if (VerticalPadding > 0 && HorizontalPadding > 0)
            {
                horizontalLine.Arrange(horizontalDragRect);
                verticalLine.Arrange(verticalDragRect);
                if (horizontalLine.Visibility == Visibility.Collapsed)
                    horizontalLine.Visibility = Visibility.Visible;
                if (verticalLine.Visibility == Visibility.Collapsed)
                    verticalLine.Visibility = Visibility.Visible;
            }
            else if (VerticalPadding > 0 && HorizontalPadding <= 0)
            {
                horizontalLine.Arrange(horizontalDragRect);
                if (horizontalLine.Visibility == Visibility.Collapsed)
                    horizontalLine.Visibility = Visibility.Visible;
                if (verticalLine.Visibility == Visibility.Visible)
                    verticalLine.Visibility = Visibility.Collapsed;
            }
            else if (HorizontalPadding > 0 && VerticalPadding <= 0)
            {
                verticalLine.Arrange(verticalDragRect);
                if (verticalLine.Visibility == Visibility.Collapsed)
                    verticalLine.Visibility = Visibility.Visible;
                if (horizontalLine.Visibility == Visibility.Visible)
                    horizontalLine.Visibility = Visibility.Collapsed;
            }
            else if (VerticalPadding > 0)
            {
                horizontalLine.Arrange(horizontalDragRect);
                if (horizontalLine.Visibility == Visibility.Collapsed)
                    horizontalLine.Visibility = Visibility.Visible;
            }
            else if (HorizontalPadding > 0)
            {
                verticalLine.Arrange(verticalDragRect);
                if (verticalLine.Visibility == Visibility.Collapsed)
                    verticalLine.Visibility = Visibility.Visible;
            }
            else
            {
                if (horizontalLine.Visibility == Visibility.Visible)
                    horizontalLine.Visibility = Visibility.Collapsed;
                if (verticalLine.Visibility == Visibility.Visible)
                    verticalLine.Visibility = Visibility.Collapsed;
            }
        }

        IRowElement previousFixedRowElement = null;
        double extendedHeaderHeight = 0.0;
        private void ArrangeRowsForFixedGroupCaption(IRowElement item, VisibleLineInfo line, ref Rect rect, double panelDelta, IOrderedEnumerable<IRowElement> orderedItems)
        {
            if (line == null)
            {
                var xPosition = 0.15*HorizontalPadding;
                var height = this.RowHeights[item.Index];
#if UWP
                if (ScrollOwner != null)
                {
                    xPosition += this.HorizontalOffset;
                }
#endif
                rect = new Rect(xPosition, extendedHeaderHeight + panelDelta, this.ScrollColumns.ViewSize, height);
                previousFixedRowElement = item;
                extendedHeaderHeight += height;
                item.Element.Clip = null;
                item.ArrangeElement(rect);
                item.ArrangeRect = rect;
                return;
            }

            if (!item.IsFixedRow)
            {
                if (line.IsClippedBody && line.IsClippedCorner)
                    //Top border for Footer rows is not shown while the previous row is in selection, because we have set -1 to Margin for row controls. 
                    //Hence the ClippedSize is reduced to -1 when the row is clipped with selection.
                    item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, 0, this.ScrollColumns.ViewSize, this.GetClippedSize(line)) };
                else if (line.IsClippedBody && line.IsClippedOrigin)
                    item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, line.Size - line.ClippedSize, this.ScrollColumns.ViewSize, line.Size) };
                else
                    item.Element.Clip = null;

                if (item.RowType == RowType.CaptionRow || item.RowType == RowType.CaptionCoveredRow)
                {
                    var currentitem = item;
                    //To check whether the caption row is clipped when AllowFrozenGroupHeader is enabled.
                    var hasCliped = false;
                    while (true)
                    {
                        var previouselement = GetPreviousFixedRow(orderedItems, currentitem, element => (element.RowType == RowType.CaptionRow || element.RowType == RowType.CaptionCoveredRow));
                        if (previouselement != null && previouselement.IsFixedRow)
                        {
                            if (previouselement.Level < item.Level)
                            {
                                if (item.RowRegion == RowRegion.Body && (extendedHeaderHeight + panelDelta) > rect.Y)
                                {
                                    if (((previouselement.ArrangeRect.Y + previouselement.ArrangeRect.Height) - rect.Y) >= rect.Height)
                                        item.Element.Clip = new RectangleGeometry()
                                        {
                                            Rect = Rect.Empty
                                        };
                                    else
                                        item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, (previouselement.ArrangeRect.Y + previouselement.ArrangeRect.Height) - rect.Y, this.ScrollColumns.ViewSize, line.Size) };
                                }
                                break;
                            }
                            if (rect.Y <= previouselement.ArrangeRect.Y)
                            {
                                previouselement.Element.Clip = new RectangleGeometry() { Rect = Rect.Empty };
                            }
                            else if (previouselement.ArrangeRect.Y + previouselement.ArrangeRect.Height > rect.Y)
                            {
                                previouselement.Element.Clip = new RectangleGeometry() { Rect = new Rect(0, (rect.Y - (previouselement.ArrangeRect.Y + previouselement.ArrangeRect.Height)), this.ScrollColumns.ViewSize, line.Size) };
                                hasCliped = true;
                                break;
                            }
                            else
                                break;
                            currentitem = previouselement;
                        }
                        else
                            break;
                    }
                    //If the previous CaptionRow is cliped in FreezedGroupHeader, line has been drawn for current row.
                    if (hasCliped)
                        this.RowsGenerator.ApplyFixedRowVisualState(item.Index, true);
                    else
                        this.RowsGenerator.ApplyFixedRowVisualState(item.Index, false);
                }
                else
                {
                    if (item.RowRegion == RowRegion.Body && (extendedHeaderHeight + panelDelta) > rect.Y)
                        item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, ((extendedHeaderHeight + panelDelta) - rect.Y), this.ScrollColumns.ViewSize, line.Size) };
                }
            }
            else
            {
                item.ArrangeRect = rect;
                var currentitem = item;
                var extendedHeightChanged = false;
                var hasCliped = false;
                while (true)
                {
                    var previouselement = GetPreviousFixedRow(orderedItems, currentitem, element => element.IsFixedRow);
                    if (previouselement != null)
                    {
                        var previouselementy = Math.Round((previouselement.ArrangeRect.Y + previouselement.ArrangeRect.Height), 3, MidpointRounding.AwayFromZero);
                        var recty = Math.Round(rect.Y, 3, MidpointRounding.AwayFromZero);

                        if (previouselementy <= recty)
                        {
                            //previouselement.Element.Clip = null;
                            if (currentitem == item)
                                extendedHeaderHeight += rect.Height;
                            break;
                        }
                        if (previouselement.Element.Clip == null)
                        {
                            previouselement.Element.Clip = new RectangleGeometry { Rect = new Rect(0, rect.Y - (extendedHeaderHeight + panelDelta), ScrollColumns.ViewSize, line.Size) };
                            hasCliped = true;
                        }
                        if ((extendedHeaderHeight + panelDelta) - rect.Y > line.Size)
                            extendedHeaderHeight -= line.Size;
                        else
                        {
                            var rectangleGeometry = previouselement.Element.Clip as RectangleGeometry;
                            if (rectangleGeometry != null && !rectangleGeometry.Rect.IsEmpty)
                                extendedHeaderHeight += (line.Size - ((extendedHeaderHeight + panelDelta) - previouselement.ArrangeRect.Y));
                        }
                        if (previouselement.Level == item.Level && rect.Y < previouselement.ArrangeRect.Y)
                        {
                            rect.Y = previouselement.ArrangeRect.Y;
                            previouselement.Element.Clip = new RectangleGeometry { Rect = Rect.Empty };
                            hasCliped = false;
                            this.RowsGenerator.ApplyFixedRowVisualState(item.Index, false);
                            extendedHeaderHeight += line.Size;
                            break;
                        }
                        currentitem = previouselement;
                        extendedHeightChanged = true;
                    }
                    else
                    {
                        item.Element.Clip = null;
                        if (!extendedHeightChanged)
                            extendedHeaderHeight += line.Size;
                        break;
                    }
                }
                //If we again invoke the ApplyFixedRowVisualState for not hasCliped means this is processed for 
                //every scrolling so the line is not drawn for CaptionRow.
                if (item.RowType == RowType.CaptionRow || item.RowType == RowType.CaptionCoveredRow)
                {
                    //If the previous CaptionRow is clipped in FreezedGroupHeader, line has been drawn for current row.
                    if (hasCliped)
                        this.RowsGenerator.ApplyFixedRowVisualState(item.Index, true);
                }
                previousFixedRowElement = item;
            }
        }

        private void InvalidatScrollInfoAndMeasure()
        {
            this.InvalidateMeasure();
            this.InvalidatScrollInfo();
        }

        public void InvalidateMeasureInfo()
        {
            this.InvalidateMeasure();
        }

#if !WPF
        private async void InvalidatScrollInfo()
#else
        private void InvalidatScrollInfo()
#endif
        {
            if (this.ScrollOwner != null)
            {
#if WinRT || UNIVERSAL
                if (VerticalOffset != ScrollOwner.VerticalOffset)
                {
                    if (ScrollOwner.ScrollableHeight < VerticalOffset)
                    {
                        var view = Windows.ApplicationModel.Core.CoreApplication.MainView;
                        if (view != null && view.CoreWindow.Dispatcher != null)
                        {
                            await view.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                             () =>
                             {
                                 ScrollOwner.ChangeView(null, VerticalOffset, null, true);
                             });
                        }
                        else
                            ScrollOwner.ChangeView(null, VerticalOffset, null, true);
                    }
                    else
                        ScrollOwner.ChangeView(null, VerticalOffset, null, true);

                }
                if (HorizontalOffset != ScrollOwner.HorizontalOffset)
                {
                    if (ScrollOwner.ScrollableWidth < HorizontalOffset)
                    {
                        var view = Windows.ApplicationModel.Core.CoreApplication.MainView;
                        if (view != null && view.CoreWindow.Dispatcher != null)
                        {
                            await view.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                           () =>
                           {
                               ScrollOwner.ChangeView(HorizontalOffset, null, null, true);
                           });
                        }
                        else
                            ScrollOwner.ChangeView(HorizontalOffset, null, null, true);
                    }
                    else
                        ScrollOwner.ChangeView(HorizontalOffset, null, null, true);
                }
#endif
                this.ScrollOwner.InvalidateScrollInfo();
            }
            else
            {
                if (scrollableOwner != null)
                {
#if WinRT || UNIVERSAL
                    if (VerticalOffset != scrollableOwner.VerticalOffset)
                        scrollableOwner.ScrollToVerticalOffset(VerticalOffset);

                    if (HorizontalOffset != scrollableOwner.HorizontalOffset)
                        scrollableOwner.ScrollToHorizontalOffset(HorizontalOffset);
#endif
                    this.scrollableOwner.InvalidateScrollInfo();
                }
            }
        }

        private VisibleLineInfo GetRowVisibleLineInfo(int index)
        {
            return this.ScrollRows.GetVisibleLineAtLineIndex(index);
        }

        private VisibleLineInfo GetColumnVisibleLineInfo(int index)
        {
            return this.ScrollColumns.GetVisibleLineAtLineIndex(index);
        }

        private void WireScrollLineEvents()
        {
            if (this.RowHeights != null)
                this.RowHeights.LineHiddenChanged += OnRowLineHiddenChanged;
            if (this.ColumnWidths != null)
                this.ColumnWidths.LineHiddenChanged += OnColumnLineHiddenChanged;
        }

        private void UnWireScrollLineEvents()
        {
            if (this.RowHeights != null)
                this.RowHeights.LineHiddenChanged -= OnRowLineHiddenChanged;            
            if (this.ColumnWidths != null)
                this.ColumnWidths.LineHiddenChanged -= OnColumnLineHiddenChanged;
        }

        private void OnColumnLineHiddenChanged(object sender, HiddenRangeChangedEventArgs e)
        {
            if (this.RowsGenerator != null)
                this.RowsGenerator.ColumnHiddenChanged(e);
            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateMeasure();
            else if (this.ScrollableOwner != null)
                this.ScrollableOwner.InvalidateMeasure();
        }

        private void OnRowLineHiddenChanged(object sender, HiddenRangeChangedEventArgs e)
        {
            if (suspendUpdates)
                return;

            if (this.RowsGenerator != null)
                this.RowsGenerator.RowHiddenChanged(e);
            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateScrollInfo();
            else if (this.scrollableOwner != null)
                this.scrollableOwner.InvalidateScrollInfo();
        }

#endregion

#if WPF
        /// <summary>
        /// Method that decides to scroll vertically or horizontally when column has invalid data by GridValidationMode with InEdit.
        /// </summary>
        /// <returns></returns>
        bool CanScroll()
        {
            if (this.RowsGenerator == null || this.RowsGenerator.Owner == null)
                return false;

            var dataGrid = this.RowsGenerator.Owner.GetDataGrid();            
            if (dataGrid == null) return false;

            var dataColumnBase = dataGrid.SelectionController.CurrentCellManager.CurrentCell as DataColumnBase;                            
            if(dataColumnBase == null || dataColumnBase.GridColumn == null || !dataColumnBase.IsEditing) return false;            

            var gridCell = dataColumnBase.ColumnElement as GridCell;
            if (gridCell == null) return false;

            if((dataColumnBase.GridColumn.GridValidationMode != GridValidationMode.InEdit)
                ||
                (GridColumn.GridValidationModeProperty== DependencyProperty.UnsetValue && dataGrid.GridValidationMode != GridValidationMode.InEdit))
            return false;                        

            // WPF-25164 - When all columns have been created in VisibleColumns and VisibleInfo is not available for that we need to allow the scrolling vertically and horizontally.
            var columnVisibleInfo = this.ScrollColumns.GetVisibleLineAtLineIndex(dataColumnBase.ColumnIndex);
            var rowVisibleInfo = this.ScrollRows.GetVisibleLineAtLineIndex(dataColumnBase.RowIndex);
            if (columnVisibleInfo == null || rowVisibleInfo == null)
                return false;
            
            return ((!string.IsNullOrEmpty(gridCell.attributeErrorMessage) || !string.IsNullOrEmpty(gridCell.bindingErrorMessage)) && !ValidationHelper.IsCurrentCellValidated);
        }

#endif

#region IScrollableInfo

        public void LineDown()
        {
            if (!SuspendManipulationScroll)
            {
                // avoid update to vertical scroll bar value when scroll by down repeat button of vertical scroll bar.
#if WPF
                if (CanScroll())
                    return;
#endif
                ScrollRows.ScrollToNextLine();
                this.InvalidateMeasure();
            }
        }

        public void LineLeft()
        {
            // avoid update to horizontal scroll bar value when scroll by left repeat button of horizontal scroll bar.
#if WPF
            if (CanScroll())
                return;
#endif
            ScrollColumns.ScrollToPreviousLine();
            this.InvalidateMeasure();
        }

        public void LineRight()
        {
            // avoid update to horizontal scroll bar value when scroll by right repeat button of horizontal scroll bar.
#if WPF
            if (CanScroll())
                return;
#endif
            ScrollColumns.ScrollToNextLine();
            this.InvalidateMeasure();
        }

        public void LineUp()
        {
            if (!SuspendManipulationScroll)
            {
                // avoid update to vertical scroll bar value when scroll by up repeat button of vertical scroll bar.
#if WPF
                if (CanScroll())
                    return;
#endif
                ScrollRows.ScrollToPreviousLine();
                this.InvalidateMeasure();
            }
        }

        public Rect MakeVisible(UIElement visual, Rect rectangle)
        {
            return rectangle;
        }

        public void MouseWheelDown()
        {
            LineDown();
        }

        public void MouseWheelLeft()
        {
            LineLeft();
        }

        public void MouseWheelRight()
        {
            LineRight();
        }

        public void MouseWheelUp()
        {
            LineUp();
        }

        public void PageDown()
        {
#if WPF
			// avoid update to vertical scroll bar value when scroll on thumb of scroll bar towards down
            if (CanScroll())
                return;
#endif
            ScrollRows.ScrollToNextPage();
            this.InvalidateMeasure();
        }

        public void PageLeft()
        {
#if WPF
			// avoid update to horizontal scroll bar value when scroll on thumb of scroll bar towards left
            if (CanScroll())
                return;
#endif
            ScrollColumns.ScrollToPreviousPage();
            this.InvalidateMeasure();
        }

        public void PageRight()
        {
#if WPF
			// avoid update to horizontal scroll bar value when scroll on thumb of scroll bar towards right
            if (CanScroll())
                return;
#endif
            ScrollColumns.ScrollToNextPage();
            this.InvalidateMeasure();
        }

        public void PageUp()
        {
#if WPF
			// avoid update to vertical scroll bar value when scroll on thumb of scroll bar towards up.
            if (CanScroll())
                return;
#endif
            ScrollRows.ScrollToPreviousPage();
            this.InvalidateMeasure();
        }

        public void SetHorizontalOffset(double offset)
        {
            if (SuspendManipulationScroll)
                return;

            // avoid updating the horizontal scroll bar value when cell has invalid data with GridValdiationMode InEdit by scroll bar hit
#if WPF
                if (CanScroll())
                    return;
#endif
            var value=(float)offset + HScrollBar.Minimum;
            if (HScrollBar.Value != value)
            {
                HScrollBar.Value = value;
                this.InvalidateMeasure();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (SuspendManipulationScroll)
                return;            
#if WPF
            //Avoid updating the vertical scroll bar value when cell has invalid data with GridValdiationMode InEdit by scroll bar hit.
            if (CanScroll())
                return;
#endif
            var value = (float)offset + VScrollBar.Minimum;
            if (VScrollBar.Value != value)
            {
                VScrollBar.Value = value;
                this.InvalidateMeasureInfo();
            }
        }

        public bool CanHorizontallyScroll
        {
            get
            {
                return HScrollBar.Enabled;
            }
            set
            {
                HScrollBar.Enabled = value;
            }
        }

        public bool CanVerticallyScroll
        {
            get
            {
                return VScrollBar.Enabled;
            }
            set
            {
                VScrollBar.Enabled = value;
            }
        }

        public double ExtentHeight
        {
            get { return VScrollBar.Maximum - VScrollBar.Minimum; }
        }

        public double ExtentWidth
        {
            get { return HScrollBar.Maximum - HScrollBar.Minimum; }
        }

        public double HorizontalOffset
        {
            get { return HScrollBar.Value - HScrollBar.Minimum; }
        }


        public ScrollViewer ScrollOwner
        {
            get
            {
                return this._scrollOwner;
            }
            set
            {
#if UWP
                UnwireScrollEvents();
#endif
                this._scrollOwner = value;
#if UWP
                if (this._scrollOwner != null)
                    this._scrollOwner.Loaded += OnScrollOwnerLoaded;
#endif
#if WP
                if (this.ScrollOwner != null)
                {
                    this.ScrollOwner.ManipulationStarted += OnManipulationStarted;
                    this.ScrollOwner.ManipulationDelta += OnManipulationDelta;
                    this.ScrollOwner.ManipulationCompleted += OnManipulationCompleted;
                }
#endif
            }
        }
#if WP
        
#region WP7 Scroll Behaviour

        public PointerDeviceType GetPointerType(RoutedEventArgs originalArgs)
        {
            if (originalArgs is ManipulationDeltaEventArgs)
            {
                return PointerDeviceType.Touch;
            }
            return PointerDeviceType.Mouse;
        }

        public Point GetPosition(RoutedEventArgs args, UIElement relativeTo)
        {
            if (args is System.Windows.Input.MouseEventArgs)
            {
                return ((System.Windows.Input.MouseEventArgs)args).GetPosition(relativeTo);
            }
            if (args is GestureEventArgs)
            {
                return ((GestureEventArgs)args).GetPosition(relativeTo);
            }
            if (args is ManipulationDeltaEventArgs)
            {
                ManipulationDeltaEventArgs args2 = args as ManipulationDeltaEventArgs;
                return args2.ManipulationContainer.TransformToVisual(relativeTo).Transform(args2.ManipulationOrigin);
            }
            if (args is ManipulationStartedEventArgs)
            {
                ManipulationStartedEventArgs args3 = args as ManipulationStartedEventArgs;
                return args3.ManipulationContainer.TransformToVisual(relativeTo).Transform(args3.ManipulationOrigin);
            }
            return new Point();
        }

        protected void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ChangeScrollVisualStates("Scrolling");

            e.ManipulationContainer = this;
            var info = new PanningInfo
            {
                OriginalHorizontalOffset = this.HorizontalOffset,
                OriginalVerticalOffset = this.VerticalOffset
            };
            this._panningInfo = info;
            double num = this.ViewportWidth + 1.0;
            double num2 = this.ViewportHeight + 1.0;
            this._panningInfo.DeltaPerHorizontalOffet = DoubleUtil.AreClose(num, 0.0) ? 0.0 : (base.ActualWidth / num);
            this._panningInfo.DeltaPerVerticalOffset = DoubleUtil.AreClose(num2, 0.0)
                                                           ? 0.0
                                                           : (base.ActualHeight / num2);

            this.Start(e);
            e.Handled = true;
            //base.OnManipulationStarted(e);
        }

        void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                if (e.PinchManipulation.CumulativeScale == 1)
                {
                    prevZoomScale = 1;
                    return;
                }
                if (e.PinchManipulation.CumulativeScale > 1)
                {
                    if ((e.PinchManipulation.CumulativeScale - prevZoomScale) > 0)
                        NeedToRefreshColumn = false;
                    else
                        NeedToRefreshColumn = true;
                    _ZoomScale = _ZoomScale + (e.PinchManipulation.CumulativeScale - prevZoomScale);
                    prevZoomScale = e.PinchManipulation.CumulativeScale;
                }
                else if (e.PinchManipulation.CumulativeScale < 1)
                {
                    if ((prevZoomScale - e.PinchManipulation.CumulativeScale) > 0)
                        NeedToRefreshColumn = true;
                    else
                        NeedToRefreshColumn = false;
                    _ZoomScale = _ZoomScale - (prevZoomScale - e.PinchManipulation.CumulativeScale);
                    prevZoomScale = e.PinchManipulation.CumulativeScale;
                }
                if (_ZoomScale > 2)
                    _ZoomScale = 2;
                if (_ZoomScale < 0.5)
                    _ZoomScale = 0.5;
                if (zoomScrollRowIndex == -1 && zoomScrollColumnIndex == -1)
                {
                    midPoint.X = (e.PinchManipulation.Current.PrimaryContact.X +
                                  e.PinchManipulation.Current.SecondaryContact.X) / 2;
                    midPoint.Y = (e.PinchManipulation.Current.PrimaryContact.Y +
                                  e.PinchManipulation.Current.SecondaryContact.Y) / 2;
                    if (this.ScrollRows.GetVisibleLineAtPoint(midPoint.Y) == null || this.ScrollColumns.GetVisibleLineAtPoint(midPoint.X) == null)
                        return;
                    zoomScrollRowIndex = this.ScrollRows.GetVisibleLineAtPoint(midPoint.Y).LineIndex;
                    zoomScrollColumnIndex = this.ScrollColumns.GetVisibleLineAtPoint(midPoint.X).LineIndex;
                    if (zoomScrollColumnIndex < 0 && zoomScrollRowIndex < 0)
                        return;
                    var verticalLine = this.ScrollRows.GetVisibleLineAtLineIndex(zoomScrollRowIndex);
                    var horizontalLine = this.ScrollColumns.GetVisibleLineAtLineIndex(zoomScrollColumnIndex);
                    initialHLineOrigin = horizontalLine.Origin;
                    thumbWidth = ViewportWidth * (this.ViewportWidth / (HScrollBar.Maximum - HScrollBar.Minimum + ViewportWidth));

                    if (midPoint.X - horizontalLine.Origin > 50)
                        HoffsetIncrement = true;
                }


                if (_ZoomScale < 2.0 && _ZoomScale >= 0.5)
                {
                    this.SetZoomScale(_ZoomScale);
                    if (initialHLineOrigin > 0)
                        this.ScrollColumns.ScrollInView(zoomScrollColumnIndex);
                    this.ScrollRows.ScrollInView(zoomScrollRowIndex);


                    midPoint.X = (e.PinchManipulation.Current.PrimaryContact.X +
                                  e.PinchManipulation.Current.SecondaryContact.X) / 2;
                    midPoint.Y = (e.PinchManipulation.Current.PrimaryContact.Y +
                                  e.PinchManipulation.Current.SecondaryContact.Y) / 2;

                    var verticalLine = this.ScrollRows.GetVisibleLineAtLineIndex(zoomScrollRowIndex);
                    var horizontalLine = this.ScrollColumns.GetVisibleLineAtLineIndex(zoomScrollColumnIndex);

                    this.SetVerticalOffset(this.VerticalOffset +
                                           ((verticalLine.Origin + verticalLine.Size) - midPoint.Y));

                    if (initialHLineOrigin != horizontalLine.Origin && initialHLineOrigin > 0 && !HoffsetIncrement)
                    {
                        this.SetHorizontalOffset(this.HorizontalOffset + (Math.Abs(horizontalLine.Origin) - midPoint.X));
                        initialHLineOrigin = horizontalLine.Origin;
                    }
                    else if (HoffsetIncrement)
                    {
                        var thumbSize = ViewportWidth *
                                        (this.ViewportWidth / (HScrollBar.Maximum - HScrollBar.Minimum + ViewportWidth));
                        var diffThumb = thumbWidth - thumbSize;

                        this.SetHorizontalOffset(this.HorizontalOffset + diffThumb);
                        thumbWidth = thumbSize;
                    }


                }
                return;
            }

            Point deltaTranslation = e.DeltaManipulation.Translation;
            Point point2 = e.CumulativeManipulation.Translation;
            if (!this._dragStarted && (((Math.Abs(point2.X) > this._initialThreshold)) || ((Math.Abs(point2.Y) > this._initialThreshold))))
            {
                this.Start(e);
            }
            if (this._dragStarted)
            {
                this.ManipulateScroll(e);
            }

            e.Handled = true;
        }
        TimeSpan duration;
        TimeSpan update;
        void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            zoomScrollColumnIndex = zoomScrollRowIndex = -1;
            prevZoomScale = 1;

            if (this._panningInfo != null)
            {
                if (this._dragStarted)
                {
                    if (e.IsInertial)
                    {
                        this._cumulativeTranslation = e.TotalManipulation.Translation;
                        update = new TimeSpan(0, 0, 0, 0, 1);
                        duration = new TimeSpan(0, 0, 0, 0, Math.Max((int)(Math.Abs(e.FinalVelocities.LinearVelocity.X * ZoomScale / 1000) * 80), (int)(Math.Abs(e.FinalVelocities.LinearVelocity.Y * ZoomScale / 1000) * 80)));
                        this.StartInertia(e, new Point(e.FinalVelocities.LinearVelocity.X * ZoomScale / 1000.0, e.FinalVelocities.LinearVelocity.Y * ZoomScale / 1000.0));
                    }
                    else
                    {
                        this.Complete();
                        ChangeScrollVisualStates("NotScrolling");
                        ManipulationAnimation();
                        //return;
                    }
                }

                if (!e.IsInertial && !this._panningInfo.IsPanning)
                {
                    e.Handled = true;
                    return;
                }

                ManipulationAnimation();
            }
        }
        
        public Transform LayoutTransform
        {
            get { return (Transform)GetValue(LayoutTransformProperty); }
            set { SetValue(LayoutTransformProperty, value); }
        }

        /// <summary>
        /// Identifies the LayoutTransform DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty LayoutTransformProperty = DependencyProperty.Register(
            "LayoutTransform", typeof(Transform), typeof(VisualContainer), new PropertyMetadata(LayoutTransformChanged));

        private static void LayoutTransformChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            // Casts are safe because Silverlight is enforcing the types
            ((o as VisualContainer)).ProcessTransform((Transform)e.NewValue);
        }
        private void ProcessTransform(Transform transform)
        {
            // Get the transform matrix and apply it
            _transformation = RoundMatrix(GetTransformMatrix(transform), 4);
            if (null != _matrixTransform)
            {
                _matrixTransform.Matrix = _transformation;
            }
            // New transform means re-layout is necessary
            InvalidateMeasure();
        }
        private static Matrix RoundMatrix(Matrix matrix, int decimals)
        {
            return new Matrix(
                Math.Round(matrix.M11, decimals),
                Math.Round(matrix.M12, decimals),
                Math.Round(matrix.M21, decimals),
                Math.Round(matrix.M22, decimals),
                matrix.OffsetX,
                matrix.OffsetY);
        }

        private Matrix GetTransformMatrix(Transform transform)
        {
            if (null != transform)
            {
                // WPF equivalent of this entire method:
                // return transform.Value;

                // Process the TransformGroup
                TransformGroup transformGroup = transform as TransformGroup;
                if (null != transformGroup)
                {
                    Matrix groupMatrix = Matrix.Identity;
                    foreach (Transform child in transformGroup.Children)
                    {
                        groupMatrix = MatrixMultiply(groupMatrix, GetTransformMatrix(child));
                    }
                    return groupMatrix;
                }

                // Process the RotateTransform
                RotateTransform rotateTransform = transform as RotateTransform;
                if (null != rotateTransform)
                {
                    double angle = rotateTransform.Angle;
                    double angleRadians = (2 * Math.PI * angle) / 360;
                    double sine = Math.Sin(angleRadians);
                    double cosine = Math.Cos(angleRadians);
                    return new Matrix(cosine, sine, -sine, cosine, 0, 0);
                }

                // Process the ScaleTransform
                ScaleTransform scaleTransform = transform as ScaleTransform;
                if (null != scaleTransform)
                {
                    double scaleX = scaleTransform.ScaleX;
                    double scaleY = scaleTransform.ScaleY;
                    return new Matrix(scaleX, 0, 0, scaleY, 0, 0);
                }

                // Process the SkewTransform
                SkewTransform skewTransform = transform as SkewTransform;
                if (null != skewTransform)
                {
                    double angleX = skewTransform.AngleX;
                    double angleY = skewTransform.AngleY;
                    double angleXRadians = (2 * Math.PI * angleX) / 360;
                    double angleYRadians = (2 * Math.PI * angleY) / 360;
                    return new Matrix(1, angleYRadians, angleXRadians, 1, 0, 0);
                }

                // Process the MatrixTransform
                MatrixTransform matrixTransform = transform as MatrixTransform;
                if (null != matrixTransform)
                {
                    return matrixTransform.Matrix;
                }

                // TranslateTransform has no effect in LayoutTransform
            }

            // Fall back to no-op transformation
            return Matrix.Identity;
        }

        private static Matrix MatrixMultiply(Matrix matrix1, Matrix matrix2)
        {
            // WPF equivalent of following code:
            // return Matrix.Multiply(matrix1, matrix2);
            return new Matrix(
                (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21),
                (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22),
                (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21),
                (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22),
                ((matrix1.OffsetX * matrix2.M11) + (matrix1.OffsetY * matrix2.M21)) + matrix2.OffsetX,
                ((matrix1.OffsetX * matrix2.M12) + (matrix1.OffsetY * matrix2.M22)) + matrix2.OffsetY);
        }

        public void ApplyLayoutTransform()
        {
            if (scaleTransform.ScaleX != ZoomScale || scaleTransform.ScaleY != ZoomScale)
            {
                scaleTransform.ScaleX = ZoomScale;
                scaleTransform.ScaleY = ZoomScale;
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(scaleTransform);
                LayoutTransform = transformGroup;
            }
        }

        private Size ComputeLargestTransformedSize(Size arrangeBounds)
        {

            // Computed largest transformed size
            Size computedSize = Size.Empty;

            // Detect infinite bounds and constrain the scenario
            bool infiniteWidth = double.IsInfinity(arrangeBounds.Width);
            if (infiniteWidth)
            {
                arrangeBounds.Width = arrangeBounds.Height;
            }
            bool infiniteHeight = double.IsInfinity(arrangeBounds.Height);
            if (infiniteHeight)
            {
                arrangeBounds.Height = arrangeBounds.Width;
            }

            // Capture the matrix parameters
            double a = _transformation.M11;
            double b = _transformation.M12;
            double c = _transformation.M21;
            double d = _transformation.M22;

            // Compute maximum possible transformed width/height based on starting width/height
            // These constraints define two lines in the positive x/y quadrant
            double maxWidthFromWidth = Math.Abs(arrangeBounds.Width / a);
            double maxHeightFromWidth = Math.Abs(arrangeBounds.Width / c);
            double maxWidthFromHeight = Math.Abs(arrangeBounds.Height / b);
            double maxHeightFromHeight = Math.Abs(arrangeBounds.Height / d);

            // The transformed width/height that maximize the area under each segment is its midpoint
            // At most one of the two midpoints will satisfy both constraints
            double idealWidthFromWidth = maxWidthFromWidth / 2;
            double idealHeightFromWidth = maxHeightFromWidth / 2;
            double idealWidthFromHeight = maxWidthFromHeight / 2;
            double idealHeightFromHeight = maxHeightFromHeight / 2;

            // Compute slope of both constraint lines
            double slopeFromWidth = -(maxHeightFromWidth / maxWidthFromWidth);
            double slopeFromHeight = -(maxHeightFromHeight / maxWidthFromHeight);

            if ((0 == arrangeBounds.Width) || (0 == arrangeBounds.Height))
            {
                // Check for empty bounds
                computedSize = new Size(arrangeBounds.Width, arrangeBounds.Height);
            }
            else if (infiniteWidth && infiniteHeight)
            {
                // Check for completely unbound scenario
                computedSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            }
            //else if (!MatrixHasInverse(_transformation))
            //{
            //    // Check for singular matrix
            //    computedSize = new Size(0, 0);
            //}
            else if ((0 == b) || (0 == c))
            {
                // Check for 0/180 degree special cases
                double maxHeight = (infiniteHeight ? double.PositiveInfinity : maxHeightFromHeight);
                double maxWidth = (infiniteWidth ? double.PositiveInfinity : maxWidthFromWidth);
                if ((0 == b) && (0 == c))
                {
                    // No constraints
                    computedSize = new Size(maxWidth, maxHeight);
                }
                else if (0 == b)
                {
                    // Constrained by width
                    double computedHeight = Math.Min(idealHeightFromWidth, maxHeight);
                    computedSize = new Size(
                        maxWidth - Math.Abs((c * computedHeight) / a),
                        computedHeight);
                }
                else if (0 == c)
                {
                    // Constrained by height
                    double computedWidth = Math.Min(idealWidthFromHeight, maxWidth);
                    computedSize = new Size(
                        computedWidth,
                        maxHeight - Math.Abs((b * computedWidth) / d));
                }
            }
            else if ((0 == a) || (0 == d))
            {
                // Check for 90/270 degree special cases
                double maxWidth = (infiniteHeight ? double.PositiveInfinity : maxWidthFromHeight);
                double maxHeight = (infiniteWidth ? double.PositiveInfinity : maxHeightFromWidth);
                if ((0 == a) && (0 == d))
                {
                    // No constraints
                    computedSize = new Size(maxWidth, maxHeight);
                }
                else if (0 == a)
                {
                    // Constrained by width
                    double computedHeight = Math.Min(idealHeightFromHeight, maxHeight);
                    computedSize = new Size(
                        maxWidth - Math.Abs((d * computedHeight) / b),
                        computedHeight);
                }
                else if (0 == d)
                {
                    // Constrained by height
                    double computedWidth = Math.Min(idealWidthFromWidth, maxWidth);
                    computedSize = new Size(
                        computedWidth,
                        maxHeight - Math.Abs((a * computedWidth) / c));
                }
            }
            else if (idealHeightFromWidth <= ((slopeFromHeight * idealWidthFromWidth) + maxHeightFromHeight))
            {
                // Check the width midpoint for viability (by being below the height constraint line)
                computedSize = new Size(idealWidthFromWidth, idealHeightFromWidth);
            }
            else if (idealHeightFromHeight <= ((slopeFromWidth * idealWidthFromHeight) + maxHeightFromWidth))
            {
                // Check the height midpoint for viability (by being below the width constraint line)
                computedSize = new Size(idealWidthFromHeight, idealHeightFromHeight);
            }
            else
            {
                // Neither midpoint is viable; use the intersection of the two constraint lines instead
                // Compute width by setting heights equal (m1*x+c1=m2*x+c2)
                double computedWidth = (maxHeightFromHeight - maxHeightFromWidth) / (slopeFromWidth - slopeFromHeight);
                // Compute height from width constraint line (y=m*x+c; using height would give same result)
                computedSize = new Size(
                    computedWidth,
                    (slopeFromWidth * computedWidth) + maxHeightFromWidth);
            }

            // Return result
            return computedSize;
        }

        private void ChangeScrollVisualStates(string State)
        {
            if (this.ScrollOwner == null)
                return;
            if (State.Equals("Scrolling"))
                VisualStateManager.GoToState(this.ScrollOwner, "Scrolling", true);
            else
                VisualStateManager.GoToState(this.ScrollOwner, "NotScrolling", true);
        }

        private void ManipulateScroll(double delta, double cumulativeTranslation, bool isHorizontal)
        {
            double num = isHorizontal ? this._panningInfo.UnusedTranslation.X : this._panningInfo.UnusedTranslation.Y;
            double num2 = isHorizontal ? this.HorizontalOffset : this.VerticalOffset;
            double num3 = isHorizontal ? this.ScrollableWidth : this.ScrollableHeight;
            if (DoubleUtil.AreClose(num3, 0.0))
            {
                // If the Scrollable length in this direction is 0,
                // then we should neither scroll nor report the boundary feedback
                num = 0.0;
                delta = 0.0;
            }
            else if ((DoubleUtil.GreaterThan(delta, 0.0) && DoubleUtil.AreClose(num2, 0.0)) ||
                     (DoubleUtil.LessThan(delta, 0.0) && DoubleUtil.AreClose(num2, num3)))
            {
                // If we are past the boundary and the delta is in the same direction,
                // then add the delta to the unused vector
                num += delta;
                delta = 0.0;
            }
            else if (DoubleUtil.LessThan(delta, 0.0) && DoubleUtil.GreaterThan(num, 0.0))
            {
                // If we are past the boundary in positive direction
                // and the delta is in negative direction,
                // then compensate the delta from unused vector.
                double num4 = Math.Max((double)(num + delta), (double)0.0);
                delta += num - num4;
                num = num4;
            }
            else if (DoubleUtil.GreaterThan(delta, 0.0) && DoubleUtil.LessThan(num, 0.0))
            {
                // If we are past the boundary in negative direction
                // and the delta is in positive direction,
                // then compensate the delta from unused vector.
                double num5 = Math.Min((double)(num + delta), (double)0.0);
                delta += num - num5;
                num = num5;
            }
            if (isHorizontal)
            {
                if (!DoubleUtil.AreClose(delta, 0.0))
                {
                    this.SetHorizontalOffset(this._panningInfo.OriginalHorizontalOffset -
                                                  Math.Round(
                                                      (double)
                                                      (cumulativeTranslation / this._panningInfo.DeltaPerHorizontalOffet)));
                }
                this._panningInfo.UnusedTranslation = new Point(num, this._panningInfo.UnusedTranslation.Y);
            }
            else
            {
                if (!DoubleUtil.AreClose(delta, 0.0))
                {
                    this.SetVerticalOffset(this._panningInfo.OriginalVerticalOffset -
                                                Math.Round(
                                                    (double)
                                                    (cumulativeTranslation / this._panningInfo.DeltaPerVerticalOffset)));
                }
                this._panningInfo.UnusedTranslation = new Point(this._panningInfo.UnusedTranslation.X, num);
            }
        }

        private void ManipulateScroll(Point delta)
        {
            this.ManipulateScroll(delta.X, this._cumulativeTranslation.X, true);
            this.ManipulateScroll(delta.Y, this._cumulativeTranslation.Y, false);
        }

        private void ManipulateScroll(ManipulationDeltaEventArgs e)
        {
            this.ManipulateScroll(e.DeltaManipulation.Translation.X, e.CumulativeManipulation.Translation.X, true);
            this.ManipulateScroll(e.DeltaManipulation.Translation.Y, e.CumulativeManipulation.Translation.Y, false);

            if (e.IsInertial)
            {
                e.Complete();
            }
            else
            {
                double y = this._panningInfo.UnusedTranslation.Y;
                if (!this._panningInfo.InVerticalFeedback && DoubleUtil.LessThan(Math.Abs(y), PreFeedbackTranslationY))
                {
                    y = 0.0;
                }
                this._panningInfo.InVerticalFeedback = !DoubleUtil.AreClose(y, 0.0);
                this.VerticalPadding = y;
                double x = this._panningInfo.UnusedTranslation.X;
                if (!this._panningInfo.InHorizontalFeedback && DoubleUtil.LessThan(Math.Abs(x), PreFeedbackTranslationX))
                {
                    x = 0.0;
                }
                this._panningInfo.InHorizontalFeedback = !DoubleUtil.AreClose(x, 0.0);
                this.HorizontalPadding = x;
                if (x != 0 || y != 0)
                {
                    this.SetHorizontalOffset(this.HorizontalOffset);
                }
            }
        }

        private void StartInertia(RoutedEventArgs originalArgs, Point velocities)
        {
            //this.RaiseDragInertiaStarted(originalArgs, velocities);
            CompositionTarget.Rendering += (new EventHandler(this.OnRendering));
            this._velocity = velocities;
            this._lastTimeStamp = DateTime.Now;
        }

        private void Start(RoutedEventArgs originalArgs)
        {
            if (this._dragStarted)
            {
                this.Complete();
            }
            this._cumulativeTranslation = new Point();
            this._dragStarted = true;
        }

        internal void Complete()
        {
            if (this._dragStarted)
            {

                this._dragStarted = false;
                this._velocity = new Point();
                CompositionTarget.Rendering -= new EventHandler(this.OnRendering);
                //ManipulationAnimation();
                //this._panningInfo = null;
            }
        }

        /// <summary>
        /// This Method is used for Translation When the User Pulls the Grid along the Edges
        /// </summary>
        private async void ManipulationAnimation()
        {
            double YanimationRatio = Math.Abs(this.VerticalPadding) / 2;
            double XanimationRatio = Math.Abs(this.HorizontalPadding) / 2;
            if ((this.VerticalPadding >= 0) && (this.HorizontalPadding >= 0))
            {
                while ((this.VerticalPadding > 0) || (this.HorizontalPadding > 0))
                {
                    await Task.Delay(1);

                    this.VerticalPadding = Math.Round(this.VerticalPadding) < 0
                                                          ? 0
                                                          : this.VerticalPadding - YanimationRatio;
                    this.HorizontalPadding = Math.Round(this.HorizontalPadding) < 0
                                                            ? 0
                                                            : this.HorizontalPadding - XanimationRatio;
                    this.VerticalPadding = Math.Max(0.0, this.VerticalPadding);
                    this.HorizontalPadding = Math.Max(0.0, this.HorizontalPadding);
                    this.SetHorizontalOffset(this.HorizontalOffset);
                }
            }
            else if ((this.VerticalPadding <= 0) && (this.HorizontalPadding <= 0))
            {
                while ((this.VerticalPadding < 0) || (this.HorizontalPadding < 0))
                {
                    await Task.Delay(1);
                    this.VerticalPadding = Math.Round(this.VerticalPadding) > 0
                                                          ? 0
                                                          : this.VerticalPadding + YanimationRatio;
                    this.HorizontalPadding = Math.Round(this.HorizontalPadding) > 0
                                                            ? 0
                                                            : this.HorizontalPadding + XanimationRatio;
                    this.VerticalPadding = Math.Min(0.0, this.VerticalPadding);
                    this.HorizontalPadding = Math.Min(0.0, this.HorizontalPadding);
                    this.SetHorizontalOffset(this.HorizontalOffset);
                }
            }
            else if ((this.VerticalPadding <= 0) && (this.HorizontalPadding >= 0))
            {
                while ((this.VerticalPadding < 0) || (this.HorizontalPadding > 0))
                {
                    await Task.Delay(1);
                    this.VerticalPadding = Math.Round(this.VerticalPadding) > 0
                                                          ? 0
                                                          : this.VerticalPadding + YanimationRatio;
                    this.HorizontalPadding = Math.Round(this.HorizontalPadding) < 0
                                                            ? 0
                                                            : this.HorizontalPadding - XanimationRatio;
                    this.VerticalPadding = Math.Min(0.0, this.VerticalPadding);
                    this.HorizontalPadding = Math.Max(0.0, this.HorizontalPadding);
                    this.SetHorizontalOffset(this.HorizontalOffset);
                }
            }
            else
            {
                while ((this.VerticalPadding > 0) || (this.HorizontalPadding < 0))
                {
                    await Task.Delay(1);
                    this.VerticalPadding = Math.Round(this.VerticalPadding) < 0
                                                          ? 0
                                                          : this.VerticalPadding - YanimationRatio;
                    this.HorizontalPadding = Math.Round(this.HorizontalPadding) > 0
                                                            ? 0
                                                            : this.HorizontalPadding + XanimationRatio;
                    this.VerticalPadding = Math.Max(0.0, this.VerticalPadding);
                    this.HorizontalPadding = Math.Min(0.0, this.HorizontalPadding);
                    this.SetHorizontalOffset(this.HorizontalOffset);
                }
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            Point delta = new Point();
            DateTime now = DateTime.Now;
            TimeSpan span2 = (TimeSpan)(now - this._lastTimeStamp);
            double num3 = span2.TotalMilliseconds;
            this._lastTimeStamp = now;
            double totalMilliseconds = duration.Milliseconds - update.Milliseconds;
            update += new TimeSpan(0, 0, 0, 0, 12);
            double velocity = Math.Max(Math.Abs(this._velocity.X), Math.Abs(this._velocity.Y));
            Point finalVelocity = this.DeceleratePoint(this._velocity, totalMilliseconds);
            Point point2 = new Point();
            if (velocity != Math.Abs(this._velocity.X))
            {
                point2.X = (((this._velocity.X / velocity) * finalVelocity.X) * num3);
                if (this._velocity.Y < 0)
                    point2.Y = -((0.01 / 2) * Math.Pow(totalMilliseconds, 2));
                else
                    point2.Y = ((0.01 / 2) * Math.Pow(totalMilliseconds, 2));
            }
            else
            {
                point2.Y = (((this._velocity.Y / velocity) * finalVelocity.Y) * num3);
                //point2.X = (((this._velocity.X / velocity) * finalVelocity.X) * num3);
                if (this._velocity.X < 0)
                    point2.X = -((0.01 / 2) * Math.Pow(totalMilliseconds, 2)) * ZoomScale;
                else
                    point2.X = ((0.01 / 2) * Math.Pow(totalMilliseconds, 2)) * ZoomScale;
            }
            delta = point2;
            this._cumulativeTranslation = new Point(this._cumulativeTranslation.X + delta.X, this._cumulativeTranslation.Y + delta.Y);
            if (this._panningInfo != null)
            {
                this.ManipulateScroll(delta);
            }
            if (totalMilliseconds <= 0)
            {
                this.Complete();
                ChangeScrollVisualStates("NotScrolling");
            }
        }


        private Point DeceleratePoint(Point velocity, double elapsedTimeMilliseconds)
        {
            velocity = new Point(Math.Abs(velocity.X) - (0.01 * elapsedTimeMilliseconds), Math.Abs(velocity.Y) - (0.01 * elapsedTimeMilliseconds));
            return velocity;
        }
#endregion

#endif
        public double VerticalOffset
        {
            get { return VScrollBar.Value - VScrollBar.Minimum; }
        }

        public double ViewportHeight
        {
            get { return VScrollBar.LargeChange; }
        }

        public double ViewportWidth
        {
            get { return HScrollBar.LargeChange; }
        }

#endregion

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnWireScrollLineEvents();
#if WinRT || UNIVERSAL
            UnWireEvents();
#endif
            if (isDisposing)
            {
                if (_scrollColumns != null)
                {
                    if (_scrollColumns != null)
                        _scrollColumns.Changed -= OnScrollColumnsChanged;
                    this._scrollColumns.Dispose();
                    //this._scrollColumns = null;
                }
                if (this._scrollRows != null)
                {
                    this._scrollRows.Dispose();
                    //this._scrollRows = null;
                }
                if (this.columnWidthsProvider != null)
                {
                    this.columnWidthsProvider.Dispose();
                    this.columnWidthsProvider = null;
                }
                if (this.rowHeightsProvider != null)
                {
                    this.rowHeightsProvider.Dispose();
                    this.rowHeightsProvider = null;
                }
                this.Children.Clear();
                this.DragBorderBrush = null;
                this.horizontalLine = null;
                this.verticalLine = null;
                this._hScrollBar = null;
                this._vScrollBar = null;
                this._scrollOwner = null;
                this.RowsGenerator = null;
                this.RowHeightManager = null;
                this._scrollColumns = null;
                this._scrollRows = null;
            }
            isdisposed = true;
        }

#if WPF

        public static DependencyObject GetParent(DependencyObject current)
        {
            if (current == null)
            {
                throw new ArgumentNullException("current");
            }
            var element = current as FrameworkElement;
            if (element != null)
            {
                if (element.Parent != null)
                    return element.Parent;
                else if (element.TemplatedParent != null)
                    return element.TemplatedParent;
            }
            var element2 = current as FrameworkContentElement;
            if (element2 != null)
            {
                return element2.Parent;
            }
            return null;
        }

        public static readonly DependencyProperty WantsMouseInputProperty = DependencyProperty.Register(
            "WantsMouseInput", typeof(bool?), typeof(VisualContainer), null);

        public static bool? GetWantsMouseInput(DependencyObject dpo, UIElement falseIfParent)
        {
            while (dpo.GetValue(WantsMouseInputProperty) == null)
            {
                var parent = VisualContainer.GetParent(dpo);
                if (parent == falseIfParent)
                    return false;
                if (parent == null)
                    return null;
                dpo = parent;
            }
            return (bool?)dpo.GetValue(WantsMouseInputProperty);
        }

        public static void SetWantsMouseInput(DependencyObject dpo, bool? value)
        {
            dpo.SetValue(WantsMouseInputProperty, value);
        }


#endif
#if WPF

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return Rect.Empty;
        }
#endif

        private ScrollableContentViewer scrollableOwner;

        public ScrollableContentViewer ScrollableOwner
        {
            get { return scrollableOwner; }
            set
            {
#if UWP
                UnwireScrollEvents();
#endif
                scrollableOwner = value;
#if UWP

                if (this.scrollableOwner != null)
                    this.scrollableOwner.Loaded += OnScrollableOwnerLoaded;
#endif
#if WP
                if (this.ScrollableOwner != null)
                {
                    this.ScrollableOwner.ManipulationStarted += OnManipulationStarted;
                    this.ScrollableOwner.ManipulationDelta += OnManipulationDelta;
                    this.ScrollableOwner.ManipulationCompleted += OnManipulationCompleted;
                }
#endif
            }
        }

        internal void UpdateRowInfo(LineSizeCollection lines, double deafultRowHeight)
        {
            // In detailsview grid, we reset rowHeightsProvider while changing itemssource. So need to unwire event
            UnWireScrollLineEvents();
            this.rowHeightsProvider = lines ?? this.OnCreateRowHeights();
            // Need to wire event after setting rowHeightsProvider
            WireScrollLineEvents();
            this.rowHeightsProvider.DefaultLineSize = deafultRowHeight;
#if WinRT || UNIVERSAL
            this.columnWidthsProvider.DefaultLineSize = 120;
#else
            this.columnWidthsProvider.DefaultLineSize = 150;
#endif
            this._scrollRows = null;
        }
    }
}

