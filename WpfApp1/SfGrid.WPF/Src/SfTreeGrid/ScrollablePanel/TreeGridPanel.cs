#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using System;
using System.Linq;
using Syncfusion.UI.Xaml.Grid;
#if UWP
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

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    public class TreeGridPanel : Panel, IScrollableInfo, IDisposable
    {
        #region Fields
        ScrollInfo _hScrollBar;
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
        internal double previousArrangeWidth;
        internal bool NeedToRefreshColumn;
#if WPF
        internal bool SuspendManipulationScroll;
#endif
#if UWP
        internal Size ViewPortSize;
        internal Func<KeyEventArgs, bool> ContainerKeydown;
#endif
        #endregion

        #region Property

        public ITreeGridRowGenerator RowGenerator { get; internal set; }
        internal void SetRowGenerator(TreeGridRowGenerator rg)
        {
            this.RowGenerator = rg;
            horizontalLine.Stroke = DragBorderBrush;
            horizontalLine.StrokeThickness = DragBorderThickness.Top;
            verticalLine.Stroke = DragBorderBrush;
            verticalLine.StrokeThickness = DragBorderThickness.Left;
        }

        private double verticalPadding;
        public double VerticalPadding
        {
            get
            {
                return verticalPadding;
            }
            set
            {
#if WPF
                if (!SuspendManipulationScroll)
#endif
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
#if WPF
                if (!SuspendManipulationScroll)
#endif
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

        #endregion

        #region Dependency Properties
#if UWP
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
            DependencyProperty.Register("VerticalScrollBarOffset", typeof(double), typeof(TreeGridPanel), new PropertyMetadata(double.NegativeInfinity,
            (o, args) =>
            {
                var treePanel = o as TreeGridPanel;
                if (treePanel != null)
                    treePanel.SetVerticalOffset((double)args.NewValue);
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
            DependencyProperty.Register("HortizontalScrollBarOffset", typeof(double), typeof(TreeGridPanel), new PropertyMetadata(double.NegativeInfinity,
                (o, args) =>
                {
                    var treeGridPanel = o as TreeGridPanel;
                    if (treeGridPanel != null)
                        treeGridPanel.SetHorizontalOffset((double)args.NewValue);
                }));
#endif

        #endregion

        #region Ctor
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridPanel()
        {
            RowGenerator = null;

            this.rowHeightsProvider = OnCreateRowHeights();
            this.columnWidthsProvider = OnCreateColumnWidths();
#if UWP
            this.rowHeightsProvider.DefaultLineSize = 45;
            this.columnWidthsProvider.DefaultLineSize = 120;
#else
            this.rowHeightsProvider.DefaultLineSize = 24;
            this.columnWidthsProvider.DefaultLineSize = 150;
#endif

            this.Children.Add(horizontalLine);
            this.Children.Add(verticalLine);
            WireScrollLineEvents();
#if UWP
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


        #endregion

        #region ArrangeOverride

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (RowGenerator == null)
                return finalSize;
            ArrangeRow();
            this.RowGenerator.RowsArranged();
            return finalSize;
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

#if UWP
        #region Event Handlers

        /// <summary>
        /// Wires the events.
        /// </summary>
        private void WireEvents()
        {
            this.KeyDown += OnContainerKeyDown;
            this.ManipulationDelta += OnContainerOnManipulationDelta;
            this.PointerWheelChanged += OnContainerPointerWheelChanged;
        }

        /// <summary>
        /// UnWires the scroll viewer events.
        /// </summary>
        private void UnWireEvents()
        {
            this.KeyDown -= OnContainerKeyDown;
            this.ManipulationDelta -= OnContainerOnManipulationDelta;
            this.PointerWheelChanged -= OnContainerPointerWheelChanged;
            UnwireScrollEvents();
        }


        protected virtual void OnContainerKeyDown(object sender, KeyEventArgs e)
        {
            ContainerKeydown(e);
        }

        private void UnwireScrollEvents()
        {
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
        }


        /// <summary>
        /// Containers the on manipulation delta.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ManipulationDeltaRoutedEventArgs"/> instance containing the event data.</param>
        private void OnContainerOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.OriginalSource != this || e.PointerDeviceType == PointerDeviceType.Mouse)
                return;
            if (ScrollOwner != null)
            {
                if (ScrollOwner.HorizontalScrollMode == ScrollMode.Disabled && ScrollOwner.VerticalScrollMode == ScrollMode.Disabled)
                    return;
                var verticalOffset = e.Delta.Translation.Y;
                var horizontalOffset = e.Delta.Translation.X;
                this.ScrollOwner.ChangeView(null, VerticalOffset - verticalOffset, null, true);
                this.ScrollOwner.ChangeView(HorizontalOffset - horizontalOffset, null, null, true);
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
                this.RowGenerator.ColumnInserted(insertAtColumnIndex, count);
        }

        public void RemoveColumns(int removeAtColumnIndex, int count)
        {
            var cannotifyrowgenerator = this.ColumnCount != 0;
            this.RowGenerator.ColumnRemoved(removeAtColumnIndex, count);
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
            // Need to pass allowOutSideLines as true to return row column index while re ordering columns.
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
        internal Rect GetClipRect(ScrollAxisRegion rowRegion, ScrollAxisRegion columnRegion)
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


        internal void UpdateScrollBars()
        {
            //While updating the Row Column Count we need to update the scroll bar values. Otherwise visiblelines will be calculated wrongly.
            this.ScrollRows.UpdateScrollBar();
            this.ScrollColumns.UpdateScrollBar();
        }

        #endregion

        #region Private methods

        private void OnScrollColumnsChanged(object sender, Syncfusion.UI.Xaml.ScrollAxis.ScrollChangedEventArgs e)
        {
            this.NeedToRefreshColumn = true;
            this.InvalidateMeasure();
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
#if WPF
        /// <summary>
        /// Method that decides to scroll vertically or horizontally when column has invalid data by GridValidationMode with InEdit.
        /// </summary>
        /// <returns></returns>
        private bool CanScroll()
        {
            var treeGrid = this.RowGenerator.Owner;
            if (treeGrid == null) return false;

            var dataColumnBase = treeGrid.SelectionController.CurrentCellManager.CurrentCell as TreeDataColumnBase;
            if (dataColumnBase == null || dataColumnBase.TreeGridColumn == null || !dataColumnBase.IsEditing) return false;

            var gridCell = dataColumnBase.ColumnElement as TreeGridCell;
            if (gridCell == null) return false;

            if ((dataColumnBase.TreeGridColumn.GridValidationMode != GridValidationMode.InEdit)
                ||
                (GridColumnBase.GridValidationModeProperty == DependencyProperty.UnsetValue && treeGrid.GridValidationMode != GridValidationMode.InEdit))
                return false;

            // WPF-25164 - When all columns have been created in VisibleColumns and VisibleInfo is not available for that we need to allow the scrolling vertically and horizontally.
            var columnVisibleInfo = this.ScrollColumns.GetVisibleLineAtLineIndex(dataColumnBase.ColumnIndex);
            var rowVisibleInfo = this.ScrollRows.GetVisibleLineAtLineIndex(dataColumnBase.RowIndex);
            if (columnVisibleInfo == null || rowVisibleInfo == null)
                return false;

            return ((!string.IsNullOrEmpty(gridCell.attributeErrorMessage) || !string.IsNullOrEmpty(gridCell.bindingErrorMessage)) && !TreeGridValidationHelper.IsCurrentCellValidated);
        }

#endif

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
            if (this.RowGenerator.Items.Count != 0 || this.RowCount <= 0)
                return;
            var visibleRows = ScrollRows.GetVisibleLines();
            var visibleColumns = ScrollColumns.GetVisibleLines();
            this.RowGenerator.PregenerateRows(visibleRows, visibleColumns);
        }

        private void EnsureItems(bool ensureColumns)
        {
            var visibleRows = ScrollRows.GetVisibleLines();
            this.RowGenerator.EnsureRows(visibleRows);
            if (ensureColumns)
            {
                var visibleColumns = ScrollColumns.GetVisibleLines();
                if (visibleColumns.Count > 0 && visibleColumns.FirstBodyVisibleIndex <= visibleColumns.Count)
                    this.RowGenerator.EnsureColumns(visibleColumns);
            }

            //Here we subtracting 2 drag lines from children count
            if (this.RowGenerator != null && this.RowGenerator.Items.Count != (this.Children.Count - 2))
            {
                foreach (IElement row in this.RowGenerator.Items)
                {
                    if (!this.Children.Contains(row.Element))
                    {
                        this.Children.Add(row.Element);
                    }
                }
            }
        }

        private void MeasureRows()
        {
            var orderedItems = this.RowGenerator.Items.OrderBy(row => row.Index);
            foreach (var item in orderedItems)
            {
                if (item.Element.Visibility != Visibility.Visible) continue;
                var line = this.GetRowVisibleLineInfo(item.Index);
                if (line != null)
                    item.MeasureElement(new Size(this.ScrollColumns.ViewSize, line.Size));
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (RowGenerator == null)
#if UWP
                return double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height) ? new Size() : constraint;
#else
                return constraint;

            //To know that the constraint height is infinity.
            bool isInfiniteWidth = false;
#endif
            if ((double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height)) && (ScrollOwner != null || this.ScrollableOwner != null))
            {
#if WPF
                if (ScrollOwner != null)
                    isInfiniteWidth = true;
#endif

                if (!IsDoubleValueSet(FrameworkElement.HeightProperty) && double.IsInfinity(constraint.Height))
                {
                    if (ScrollRows is PixelScrollAxis)
                        constraint.Height = Math.Min(constraint.Height, ((PixelScrollAxis)ScrollRows).TotalExtent - RowHeights.PaddingDistance);
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
                this.RowGenerator.ApplyColumnSizerOnInitial(availableSize.Width);
                previousArrangeWidth = availableSize.Width;
            }
#else
            double currentAvailableWidth = constraint.Width;
            //when maximize the winodow or add record at run time in underlying collection we need to refresh column sizer based on previousArrangeWidth and availableWidth
            //if (this.ColumnCount > 0 && (previousArrangeWidth == 0.0 && constraint.Width != 0))
            if (this.ColumnCount > 0 && previousArrangeWidth != constraint.Width)
            {               
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
                    this.RowGenerator.Owner.TreeGridColumnSizer.InitialRefresh(constraint.Width - _vScrollBarWidth, false);
                }
                currentAvailableWidth = constraint.Width - _vScrollBarWidth;
            }          
#endif

            
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
#if WPF
            previousArrangeWidth = currentAvailableWidth;
#endif


            InvalidatScrollInfo();
#if UWP
            PreviousAvailableSize = availableSize;
            horizontalLine.Measure(availableSize);
            verticalLine.Measure(availableSize);
#else
            PreviousAvailableSize = constraint;
            horizontalLine.Measure(constraint);
            verticalLine.Measure(constraint);
#endif
            MeasureRows();

            // WPF- 21039 set the height and width based on window parameters such as SizeToContent.
            if (this.RowHeights.TotalExtent < constraint.Height)
                constraint.Height = this.RowHeights.TotalExtent;
            if (this.ColumnWidths.TotalExtent < constraint.Width)
                constraint.Width = this.ColumnWidths.TotalExtent;
            return constraint;
        }


#if UWP

        ScrollContentPresenter PART_ScrollContentPresenter = null;
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

        private void ArrangeRow()
        {
            if (this.RowGenerator == null) return;
            double y = this.ScrollRows.HeaderExtent;
            var extendedHeaderHeight = this.ScrollRows.HeaderExtent;
            double xPosition, yPosition;
#if UWP
            //In WinRT the Panel(VisualContainer) scrolls with the ScrollBar, hence we are measuring the VerticalOffset in panelDelta to compute the Row's Position
            double panelDelta = 0.0;
#endif
            var orderedItems = this.RowGenerator.Items.OrderBy(row => row.Index);

            foreach (var item in orderedItems)
            {
                if (item.Element.Visibility == Visibility.Visible)
                {
                    var line = this.GetRowVisibleLineInfo(item.Index);
                    if (line != null)
                    {
                        Rect rect;
                        if (this.ScrollRows.RenderSize == 0)
                        {
                            rect = new Rect(0, 0, 0, 0);
                        }
                        else
                        {
                            xPosition = 0.15 * HorizontalPadding;
                            yPosition = (item.RowType == TreeRowType.HeaderRow)
                                ? line.Origin
                                : line.Origin + (0.15 * VerticalPadding);
#if UWP
                            if (ScrollOwner != null)
                            {
                                panelDelta = this.VerticalOffset;
                                yPosition += this.VerticalOffset;
                                xPosition += this.HorizontalOffset;
                            }
#endif
                            rect = new Rect(xPosition, yPosition, this.ScrollColumns.ViewSize, line.Size);
                        }

                        if ((HorizontalPadding != 0 || VerticalPadding != 0) && (item.RowType == TreeRowType.DefaultRow) && (VerticalPadding > 0))
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
                            if (line.IsClippedBody && line.IsClippedOrigin && line.IsClippedCorner)
                                item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, line.Size - line.ClippedSize - line.ClippedCornerExtent, this.ScrollColumns.ViewSize, line.ClippedSize) };
                            else if (line.IsClippedBody && line.IsClippedCorner)
                                item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, 0, this.ScrollColumns.ViewSize, this.GetClippedSize(line)) };
                            else if (line.IsClippedBody && line.IsClippedOrigin)
                                item.Element.Clip = new RectangleGeometry { Rect = new Rect(0, line.Size - line.ClippedSize - line.ClippedCornerExtent, this.ScrollColumns.ViewSize, line.Size) };
                            else
                                item.Element.Clip = null;
                        }
                        item.ArrangeElement(rect);
                        y += line.Size;
                    }

                    else
                    {
                        var rect = new Rect(0, y, this.ScrollColumns.ViewSize, this.ScrollRows.DefaultLineSize);
                        item.ArrangeElement(rect);
                        y += this.ScrollRows.DefaultLineSize;
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

        internal void OnItemSourceChanged()
        {
            previousArrangeWidth = 0.0;
            if (this.Children.Count > 0)
            {
                this.Children.Clear();
            }
            else
                this.InvalidateMeasure();
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
#if UWP
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
#if UWP
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
            if (this.RowGenerator != null)
                this.RowGenerator.ColumnHiddenChanged(e);
            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateMeasure();
            else if (this.ScrollableOwner != null)
                this.ScrollableOwner.InvalidateMeasure();
        }

        private void OnRowLineHiddenChanged(object sender, HiddenRangeChangedEventArgs e)
        {
            if (suspendUpdates)
                return;

            if (this.RowGenerator != null)
                this.RowGenerator.RowHiddenChanged(e);
            if (this.ScrollOwner != null)
                this.ScrollOwner.InvalidateScrollInfo();
            else if (this.scrollableOwner != null)
                this.scrollableOwner.InvalidateScrollInfo();
        }

        #endregion



        #region IScrollableInfo

        public void LineDown()
        {
#if WPF
            if (SuspendManipulationScroll)
                return;

            if (CanScroll())
                return;
#endif
            ScrollRows.ScrollToNextLine();
            this.InvalidateMeasure();
        }

        public void LineLeft()
        {
#if WPF
            if (CanScroll())
                return;
#endif
            ScrollColumns.ScrollToPreviousLine();
            this.InvalidateMeasure();
        }

        public void LineRight()
        {
#if WPF
            if (CanScroll())
                return;
#endif
            ScrollColumns.ScrollToNextLine();
            this.InvalidateMeasure();
        }

        public void LineUp()
        {
#if WPF
            if (SuspendManipulationScroll)
                return;

            if (CanScroll())
                return;
#endif
            ScrollRows.ScrollToPreviousLine();
            this.InvalidateMeasure();
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
#if WPF
            if (SuspendManipulationScroll)
                return;
            if (CanScroll())
                return;
#endif
            var value = (float)offset + HScrollBar.Minimum;
            if (HScrollBar.Value != value)
            {
                HScrollBar.Value = value;
                this.InvalidateMeasure();
            }
        }

        public void SetVerticalOffset(double offset)
        {
#if WPF
            if (SuspendManipulationScroll)
                return;
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
            }
        }

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
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            UnWireScrollLineEvents();
#if UWP
            UnWireEvents();
#endif
            if (_scrollColumns != null)
            {
                if (_scrollColumns != null)
                    _scrollColumns.Changed -= OnScrollColumnsChanged;
                this._scrollColumns.Dispose();
            }
            if (this._scrollRows != null)
            {
                this._scrollRows.Dispose();
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
            this.RowGenerator = null;
            this._scrollColumns = null;
            this._scrollRows = null;
        }

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
            }
        }

        public Rect MakeVisible(UIElement visual, Rect rectangle)
        {
            return rectangle;
        }
    }
}

