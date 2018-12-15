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
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Syncfusion.Data;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Syncfusion.Data;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Automation.Peers;
using System.Windows.Media.Animation;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if WPF
    /// <summary>
    /// Represents the class to provide the common functionalities for GridCell and VirtualizingCellsControl. 
    /// </summary>
    public class GridElement : ContentControl
    {
        /// <summary>
        /// Gets or sets the value that animate the GridCell's content.
        /// </summary>
        public double AnimationOpacity
        {
            get { return (double)GetValue(AnimationOpacityProperty); }
            set { SetValue(AnimationOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AnimationOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationOpacityProperty =
            DependencyProperty.Register("AnimationOpacity", typeof(double), typeof(GridElement), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Inherits));
    }

#endif

    [ClassReference(IsReviewed = false)]
#if WPF    
    public class VirtualizingCellsControl : GridElement, IDisposable
#else
    public class VirtualizingCellsControl : ContentControl
#endif
    {
        #region Fields

        internal Panel ItemsPanel;
        internal Func<double> GetVisibleLineOrigin;
        internal Func<bool> AllowRowHoverHighlighting;
        internal Func<RowGenerator> GetRowGenerator;
#if WPF
        /// <summary>        
        /// The property that used to stores the <see cref="Syncfusion.UI.Xaml.Grid.VirtaulizingCellsControl.AnimationOpacity"/> animation.        
        /// </summary>
        protected internal Storyboard ScrollAnimation;
#endif

        private bool HasError;
        double oldOrigin;
        double oldActualWidth;
        double oldActualHeight;
        private bool isdisposed = false;
#if UWP
        protected Border SelectionBorder = null;
        private Border RowHighlightBorder = null;
        private Rectangle RowBackgroundClipRect = null;
#endif

        #endregion

        #region Dependency Region

        /// <summary>
        /// Gets or sets SelectionBorder visibility.
        /// Which is bind to the Selection Border visibility property
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Visibility SelectionBorderVisiblity
        {
            get { return (Visibility)GetValue(SelectionBorderVisiblityProperty); }
            set { SetValue(SelectionBorderVisiblityProperty, value); }
        }

        /// <summary>
        /// Dependency registration for SelectionborderVisiblity
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty SelectionBorderVisiblityProperty =
            DependencyProperty.Register("SelectionBorderVisiblity", typeof(Visibility), typeof(VirtualizingCellsControl), new PropertyMetadata(Visibility.Collapsed, OnSelectionBorderVisiblityChanged));


        /// <summary>
        /// Gets or Sets HighlightSelectionBorder visibility.
        /// Which is bind to the Selection Border visibility property
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Visibility HighlightSelectionBorderVisiblity
        {
            get { return (Visibility)GetValue(HighlightSelectionBorderVisiblityProperty); }
            set { SetValue(HighlightSelectionBorderVisiblityProperty, value); }
        }

        /// <summary>
        /// Dependency registration for SelectionborderVisiblity
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty HighlightSelectionBorderVisiblityProperty =
            DependencyProperty.Register("HighlightSelectionBorderVisiblity", typeof(Visibility), typeof(VirtualizingCellsControl), new PropertyMetadata(Visibility.Collapsed, OnHighlightBorderVisiblityChanged));


        /// <summary>
        /// Gets or sets value for Selection Background.
        /// Which is bind to the Selection Border Background property.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Brush RowHoverBackgroundBrush
        {
            get { return (Brush)GetValue(RowHoverBackgroundBrushProperty); }
            set { SetValue(RowHoverBackgroundBrushProperty, value); }
        }

        /// <summary>
        /// Dependeny registration for SelectionBackground.
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty RowHoverBackgroundBrushProperty =
            DependencyProperty.Register("RowHoverBackgroundBrush", typeof(Brush), typeof(VirtualizingCellsControl), new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

        /// <summary>
        /// Gets or sets value for Highlight Border Thickness.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Thickness RowHighlightBorderThickness
        {
            get { return (Thickness)GetValue(RowHighlightBorderThicknessProperty); }
            set { SetValue(RowHighlightBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Dependeny registration for SelectionBackground.
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty RowHighlightBorderThicknessProperty =
            DependencyProperty.Register("RowHighlightBorderThickness", typeof(Thickness), typeof(VirtualizingCellsControl), new PropertyMetadata(new Thickness(1)));

        /// <summary>
        /// Gets or sets value for Selection Background.
        /// Which is bind to the Selection Border Background property.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Brush RowSelectionBrush
        {
            get { return (Brush)GetValue(RowSelectionBrushProperty); }
            set { SetValue(RowSelectionBrushProperty, value); }
        }

        /// <summary>
        /// Dependeny registration for SelectionBackground.
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty RowSelectionBrushProperty =
            DependencyProperty.Register("RowSelectionBrush", typeof(Brush), typeof(VirtualizingCellsControl), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));


        /// <summary>
        /// Gets or sets the value for SelectionForegroundBrush
        /// </summary>

        public Brush SelectionForegroundBrush
        {
            get { return (Brush)GetValue(SelectionForegroundBrushProperty); }
            set { SetValue(SelectionForegroundBrushProperty, value); }
        }

        /// <summary>
        /// Dependency Registration for SelectionForegroundBrush
        /// </summary>
        public static readonly DependencyProperty SelectionForegroundBrushProperty =
            GridDependencyProperty.Register("SelectionForegroundBrush", typeof(Brush), typeof(VirtualizingCellsControl), new GridPropertyMetadata(SfDataGrid.GridSelectionForgroundBrush, OnSelectionForegroundBrushPropertyChanged));

        private static void OnSelectionForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cellsControl = d as VirtualizingCellsControl;
            if (cellsControl == null)
                return;
            if (cellsControl.SelectionBorderVisiblity == Visibility.Visible)
                cellsControl.Foreground = cellsControl.SelectionForegroundBrush;
        }

        /// <summary>
        /// Gets or sets the GroupCaptionRowSelectionBrush
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Brush GroupRowSelectionBrush
        {
            get { return (Brush)GetValue(GroupRowSelectionBrushProperty); }
            set { SetValue(GroupRowSelectionBrushProperty, value); }
        }

        public static readonly DependencyProperty GroupRowSelectionBrushProperty =
            DependencyProperty.Register("GroupRowSelectionBrush", typeof(Brush), typeof(VirtualizingCellsControl), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 120, 120, 120))));

        public RectangleGeometry SelectionBorderClipRect
        {
            get { return (RectangleGeometry)GetValue(SelectionBorderClipRectProperty); }
            set { SetValue(SelectionBorderClipRectProperty, value); }
        }

        public static readonly DependencyProperty SelectionBorderClipRectProperty =
            DependencyProperty.Register("SelectionBorderClipRect", typeof(RectangleGeometry), typeof(VirtualizingCellsControl), new PropertyMetadata(null));

        public RectangleGeometry HighlightBorderClipRect
        {
            get { return (RectangleGeometry)GetValue(HighlightBorderClipRectProperty); }
            set { SetValue(HighlightBorderClipRectProperty, value); }
        }

        public static readonly DependencyProperty HighlightBorderClipRectProperty =
            DependencyProperty.Register("HighlightBorderClipRect", typeof(RectangleGeometry), typeof(VirtualizingCellsControl), new PropertyMetadata(null));

        public RectangleGeometry RowBackgroundClip
        {
            get { return (RectangleGeometry)GetValue(RowBackgroundClipProperty); }
            set { SetValue(RowBackgroundClipProperty, value); }
        }

        public static readonly DependencyProperty RowBackgroundClipProperty =
            DependencyProperty.Register("RowBackgroundClip", typeof(RectangleGeometry), typeof(VirtualizingCellsControl), new PropertyMetadata(null));

        /// <summary>
        /// Property which decides whethe Focus border is visible or not.
        /// </summary>
        public Visibility CurrentFocusRowVisibility
        {
            get { return (Visibility)GetValue(CurrentFocusRowVisibilityProperty); }
            set { SetValue(CurrentFocusRowVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CurrentFocusRowVisibilityProperty =
            DependencyProperty.Register("CurrentFocusRowVisibility", typeof(Visibility), typeof(VirtualizingCellsControl), new PropertyMetadata(Visibility.Collapsed, OnCurrentFocusRowVisiblityChanged));

        /// <summary>
        /// Preperty which holds the margin value to avoid the overlapping in Indent Cell.
        /// </summary>
        public Thickness CurrentFocusBorderMargin
        {
            get { return (Thickness)GetValue(CurrentFocusBorderMarginProperty); }
            set { SetValue(CurrentFocusBorderMarginProperty, value); }
        }

        public static readonly DependencyProperty CurrentFocusBorderMarginProperty =
            DependencyProperty.Register("CurrentFocusBorderMargin", typeof(Thickness), typeof(VirtualizingCellsControl), new PropertyMetadata(new Thickness(2, 2, 2, 2)));

        #endregion

        #region Dependency Call Back

        private static void OnSelectionBorderVisiblityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var rowControl = obj as VirtualizingCellsControl;
            rowControl.UpdateSelectionBorderClip();
            rowControl.UpdateSelectionForegroundBrush();         
        }
        internal void UpdateSelectionForegroundBrush()
        {
            // If SelectionBorderVisiblity is Visible, bind VirtualizingCellsControl Foreground with SelectionForegroundBrush
            if (this.SelectionBorderVisiblity == Visibility.Visible)
            {
                var dataGrid = this.GetRowGenerator().Owner as SfDataGrid;
                // Assign SelectionForegroundBrush to Foreground when the SelectionForegroundBrush is set explicity of AddNewRowControl doesn't equal with SfDataGrid.GridSelectionForegroundBrush.                
                if (this.SelectionForegroundBrush != SfDataGrid.GridSelectionForgroundBrush)
                    this.Foreground = this.SelectionForegroundBrush;
                // SelectionForegroundBrush is not to set explicitly to AddNewRowControl, then assign SfDataGrid SelectionForegroundBrush to Foreground.
                // Otherwise, Foreground Brush is set as default value of AddNewRowControl.
                else if (dataGrid.SelectionForegroundBrush != SfDataGrid.GridSelectionForgroundBrush)
                    this.Foreground = dataGrid.SelectionForegroundBrush;
#if UWP           
                else if (ForegroundProperty.GetMetadata(typeof(FrameworkElement)).DefaultValue == this.Foreground)
                    this.Foreground = SfDataGrid.GridSelectionForgroundBrush;
#endif                            
            }
            else
                this.ClearValue(VirtualizingCellsControl.ForegroundProperty);
        }

        private static void OnHighlightBorderVisiblityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rowControl = d as VirtualizingCellsControl;
            rowControl.UpdateHighlightBorderClip();
        }

        private static void OnCurrentFocusRowVisiblityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var rowControl = obj as VirtualizingCellsControl;
            rowControl.UpdateFocusRowPosition();
        }

        #endregion

        #region Ctor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VirtualizingCellsControl()
        {
            this.DefaultStyleKey = typeof(VirtualizingCellsControl);
            SetContent();
            this.IsTabStop = false;
            WireEvents();
        }
        
        protected virtual void SetContent()
        {
            this.Content = this.ItemsPanel = new OrientedCellsPanel();
        }

        #endregion

        #region Override

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
#if UWP
            SelectionBorder = GetTemplateChild("PART_RowSelectionBorder") as Border;
            RowHighlightBorder = GetTemplateChild("PART_RowHighlightBorder") as Border;
            RowBackgroundClipRect = GetTemplateChild("PART_RowBackgroundClipRect") as Rectangle;
#endif
            if (!(this is AddNewRowControl))
                this.ApplyValidationVisualState(false);

            ApplyFixedRowVisualState(false);
        }

#if WinRT || UNIVERSAL
        protected override void OnPointerReleased(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
            base.OnPointerReleased(e);
        }
#endif

#if WinRT || UNIVERSAL
        protected override void OnPointerEntered(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
#else
        protected override void OnMouseEnter(MouseEventArgs e)   
#endif
        {
            //  UWP-705 Need to collapse the old DataRow's HighlightSelectionBorderVisiblity
            if (GetRowGenerator != null)
            {
                GetRowGenerator().Items.ForEach(row =>
                 {
                     if (row.WholeRowElement.HighlightSelectionBorderVisiblity == Visibility.Visible)
                         row.WholeRowElement.HighlightSelectionBorderVisiblity = Visibility.Collapsed;
                 });
            }
            if (AllowRowHoverHighlighting != null && AllowRowHoverHighlighting())
                this.HighlightSelectionBorderVisiblity = Visibility.Visible;
#if WinRT || UNIVERSAL
            base.OnPointerEntered(e);
#else
            base.OnMouseEnter(e);
#endif
        }


#if WinRT || UNIVERSAL
        protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
#else
        protected override void OnMouseLeave(MouseEventArgs e)
#endif
        {
            if (AllowRowHoverHighlighting != null && AllowRowHoverHighlighting())
                this.HighlightSelectionBorderVisiblity = Visibility.Collapsed;
#if WinRT || UNIVERSAL
            base.OnPointerExited(e);
#else
            base.OnMouseLeave(e);
#endif
        }

        #endregion

        #region internal methods

        internal void InitializeVirtualizingRowControl(Func<DataRowBase> GetDataRowFunc)
        {
            var orientedCellsPanel = this.ItemsPanel as OrientedCellsPanel;
            if (orientedCellsPanel == null) return;
            orientedCellsPanel.GetDataRow = GetDataRowFunc;
        }

        internal void UpdateSelectionBorderClip()
        {
            if (this.SelectionBorderVisiblity == Visibility.Visible && this.GetVisibleLineOrigin != null)
            {
                double origin = this.GetVisibleLineOrigin();
                if (origin > 0)
                {
#if UWP
                    //WRT-3519 Clipping moved to code behind due to issues while switching themes
                    if (SelectionBorder != null)
                        SelectionBorder.Clip = this.SelectionBorderClipRect = new RectangleGeometry() { Rect = new Rect(new Point(origin, 0), new Size(this.ActualWidth, this.ActualHeight)) };
#else
                    this.SelectionBorderClipRect = new RectangleGeometry() { Rect = new Rect(new Point(origin, 0), new Size(this.ActualWidth, this.ActualHeight)) };
#endif
                }
                else
                {
#if UWP
                    if (SelectionBorder != null)
                        SelectionBorder.Clip = this.SelectionBorderClipRect = null;
#else
                    this.SelectionBorderClipRect = null;
#endif
                }
            }
            else
            {
#if UWP
                if (SelectionBorder != null)
                    SelectionBorder.Clip = this.SelectionBorderClipRect = null;
#else
                this.SelectionBorderClipRect = null;
#endif
            }
        }

        internal void UpdateHighlightBorderClip()
        {
            if (this.HighlightSelectionBorderVisiblity != Visibility.Visible ||
                this.GetVisibleLineOrigin == null) return;
            double orgin = this.GetVisibleLineOrigin();
            if (orgin >= 0)
            {
#if UWP
                //WRT-3519 Clipping moved to code behind due to issues while switching themes
                if (RowHighlightBorder != null)
                    RowHighlightBorder.Clip = this.HighlightBorderClipRect = new RectangleGeometry() { Rect = new Rect(new Point(orgin, 0), new Size(this.ActualWidth, this.ActualHeight)) };
#else
                this.HighlightBorderClipRect = new RectangleGeometry() { Rect = new Rect(new Point(orgin, 0), new Size(this.ActualWidth, this.ActualHeight)) };
#endif
            }
        }

        internal void UpdateRowBackgroundClip()
        {
            if (this.GetVisibleLineOrigin != null)
            {
                double origin = this.GetVisibleLineOrigin();
                if (origin > 0 && this.ActualWidth > 0 && (origin != oldOrigin || oldActualWidth != this.ActualWidth || oldActualHeight != this.ActualHeight))
                {
#if UWP
                    //WRT-3519 Clipping moved to code behind due to issues while switching themes
                    if (RowBackgroundClipRect != null)
                        RowBackgroundClipRect.Clip = RowBackgroundClip = new RectangleGeometry() { Rect = new Rect(new Point(origin, 0), new Size(this.ActualWidth, this.ActualHeight)) };
#else
                    RowBackgroundClip = new RectangleGeometry() { Rect = new Rect(new Point(origin, 0), new Size(this.ActualWidth, this.ActualHeight)) };
#endif
                    oldOrigin = origin;
                    oldActualWidth = ActualWidth;
                    oldActualHeight = ActualHeight;
                }
                else if (origin == 0 && RowBackgroundClip != null)
                {
#if UWP
                    if (RowBackgroundClipRect != null)
                        RowBackgroundClipRect.Clip = RowBackgroundClip = null;
#else
                    RowBackgroundClip = null;
#endif
                    oldOrigin = origin;
                    oldActualWidth = ActualWidth;
                    oldActualHeight = ActualHeight;
                }
            }
        }

        /// <summary>
        /// Method which helps to update the CurrentFocus border position.
        /// </summary>
        internal void UpdateFocusRowPosition()
        {
            if (this.GetVisibleLineOrigin != null)
            {
                double origin = this.GetVisibleLineOrigin();
                CurrentFocusBorderMargin = new Thickness(origin + 2, 2, 2, 2);
            }
        }

        internal void SetError()
        {
            this.HasError = true;
            ApplyValidationVisualState();
        }

        internal void RemoveError()
        {
            this.HasError = false;
            ApplyValidationVisualState();
        }

        internal void ApplyValidationVisualState(bool canApplyDefaultState = true)
        {
            if (HasError)
                VisualStateManager.GoToState(this, "HasError", true);
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "NoError", true);
        }

        #endregion

        #region Private Methods

#if WPF
        private void OnVirtualizingCellsControlTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (ScrollAnimation == null)
                return;

            BeginStoryboard(ScrollAnimation);
        }

        /// <summary>
        /// Creates animation for <see cref="Syncfusion.UI.Xaml.Grid.VirtaulizingCellsControl.AnimationOpacity"/>
        /// </summary>
        protected internal virtual void OnCreateScrollAnimation()
        {            
            ScrollAnimation = new Storyboard();
            var doubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = new Duration(TimeSpan.FromSeconds(0.1)),
            };
            doubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = TimeSpan.FromSeconds(0), Value = 0.5 });
            doubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame() { KeyTime = TimeSpan.FromSeconds(0.1), Value = 1 });
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("AnimationOpacity"));
            ScrollAnimation.Children.Add(doubleAnimation);
        }
#endif
        protected virtual void WireEvents()
        {
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
#if WPF
            this.TargetUpdated += OnVirtualizingCellsControlTargetUpdated;
#endif
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AllowRowHoverHighlighting != null && AllowRowHoverHighlighting() && e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0)
                this.HighlightSelectionBorderVisiblity = Visibility.Collapsed;
            this.UpdateRowBackgroundClip();
            this.UpdateSelectionBorderClip();
            this.UpdateFocusRowPosition();
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.UpdateSelectionBorderClip();
        }

        protected virtual void UnwireEvents()
        {
            this.Loaded -= OnLoaded;
            this.SizeChanged -= OnSizeChanged;
#if WPF
            this.TargetUpdated -= OnVirtualizingCellsControlTargetUpdated;
#endif
        }

        #endregion


        private string rowBorderState = "NormalRow";

        /// <summary>
        /// This property is used for Frozen and Footer rows to highlight the freezed rows.
        /// </summary>
        public string RowBorderState
        {
            get
            {
                return rowBorderState;
            }
            set
            {
                if (rowBorderState != value)
                {
                    rowBorderState = value;
                    this.ApplyFixedRowVisualState();
                }
            }
        }

        protected internal virtual void ApplyFixedRowVisualState(bool canApplyDefaultState = true)
        {
            if (canApplyDefaultState)
                VisualStateManager.GoToState(this, this.rowBorderState, true);
            else if (this.rowBorderState != "NormalRow")
                VisualStateManager.GoToState(this, this.rowBorderState, true);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnwireEvents();
            if (isDisposing)
            {
                if (this.ItemsPanel != null)
                {
                    if (ItemsPanel is IDisposable)
                        (this.ItemsPanel as IDisposable).Dispose();
                    this.ItemsPanel = null;
                }
                this.GetVisibleLineOrigin = null;
                this.AllowRowHoverHighlighting = null;
                this.GetRowGenerator = null;
            }
            isdisposed = true;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

# if WPF
        # region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new SfGridRowAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        # endregion
#endif
    }

    [ClassReference(IsReviewed = false)]
    public class HeaderRowControl : VirtualizingCellsControl
    {
        #region Ctor

        public HeaderRowControl() : base()
        {
            this.DefaultStyleKey = typeof(HeaderRowControl);
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class TableSummaryRowControl : VirtualizingCellsControl
    {
        #region Ctor

        public TableSummaryRowControl() : base()
        {
            this.DefaultStyleKey = typeof(TableSummaryRowControl);
        }

        #endregion

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
        }


        protected internal override void ApplyFixedRowVisualState(bool canApplyDefaultState = true)
        {
            VisualStateManager.GoToState(this, this.RowBorderState, true);
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
#if !WinRT && !UNIVERSAL
    [TemplatePart(Name = "PART_CaptionSummaryRowGrid", Type = typeof(System.Windows.Controls.Grid))]
#else
    [TemplatePart(Name = "PART_CaptionSummaryRowGrid", Type = typeof(Windows.UI.Xaml.Controls.Grid))]
#endif
    [TemplatePart(Name = "PART_CaptionSummaryRowBorder", Type = typeof(Border))]
    public class CaptionSummaryRowControl : VirtualizingCellsControl
    {
        #region Fields

        private Func<int, bool, double> GetColumnSize;
        private Func<bool> ShowRowHeader;
        private Func<int, VisibleLineInfo> GetColumnVisibleLineInfo;
        Point pointerPressedPosition;
        private bool isdisposed = false;

        #endregion

        #region Internal Properties
#if WinRT || UNIVERSAL
        internal Windows.UI.Xaml.Controls.Grid RowPanel;
#else
        internal System.Windows.Controls.Grid RowPanel;
#endif
        internal Border ExpanderBorder;
        internal ExpandeChanged IsExpandedChanged;
        internal delegate void ExpandeChanged(bool isExpand);
        internal Func<bool> CheckForValidation;

        #endregion

        #region Ctor

        public CaptionSummaryRowControl()
        {
            this.DefaultStyleKey = typeof(CaptionSummaryRowControl);
        }

        #endregion

        #region Dependency Registration

        /// <summary>
        /// Gets or sets a value indicating whether this instance Expanded or Collapsed
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(CaptionSummaryRowControl), new PropertyMetadata(false));

        #endregion

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
#if WinRT || UNIVERSAL
            RowPanel = this.GetTemplateChild("PART_CaptionSummaryRowGrid") as Windows.UI.Xaml.Controls.Grid;
#else
            RowPanel = this.GetTemplateChild("PART_CaptionSummaryRowGrid") as System.Windows.Controls.Grid;
#endif

            ExpanderBorder = this.GetTemplateChild("PART_CaptionSummaryRowBorder") as Border;

            var group = this.DataContext as Group;
            if (group != null)
                this.IsExpanded = group.IsExpanded;
        }

#if UWP
        protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            pointerPressedPosition = e.GetCurrentPoint(null).Position;
        }
#else
        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            pointerPressedPosition = e.GetPosition(null);
        }
#endif

#if UWP
        protected override void OnPointerReleased(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (CheckForValidation != null && !CheckForValidation())
                return;

            var pointerReleasedRowPosition = e.GetCurrentPoint(null).Position;
            var ctrlKey = Window.Current.CoreWindow.GetAsyncKeyState(VirtualKey.Control);
#else
        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            var element = GridUtil.FindDescendant(e.OriginalSource, typeof(GridRowHeaderCell));
            if (element != null)
                return;
            if (CheckForValidation != null && !CheckForValidation())
                return;
            var pointerReleasedRowPosition = e.GetPosition(null);
            var ctrlKey = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
#endif

            double xPosChange = Math.Abs(pointerReleasedRowPosition.X - pointerPressedPosition.X);
            double yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pointerPressedPosition.Y);

            if (xPosChange < 30 && yPosChange < 30)
            {
                if (this.DataContext is Group)
                {
#if UWP
                    if (ctrlKey != CoreVirtualKeyStates.Down)
#else
                    if (!ctrlKey)
#endif
                    {
                        var group = this.DataContext as Group;
                        //Need to update the group expanded state before updating the expanded state for caption summary row
                        this.IsExpandedChanged(!@group.IsExpanded);
                        this.IsExpanded = group.IsExpanded;
                    }
                }
            }

#if WPF
            base.OnPreviewMouseLeftButtonUp(e);
#else
            e.Handled = true;
            base.OnPointerReleased(e);
#endif
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.RowPanel != null && this.GetColumnVisibleLineInfo != null)
            {
                var group = this.DataContext as Group;
                var line = this.GetColumnVisibleLineInfo(group.Level + (this.ShowRowHeader() ? 1 : 0) - 1);
                ExpanderBorder.Width = GetColumnSize(group.Level + (this.ShowRowHeader() ? 1 : 0) - 1, false);
                if (this.ShowRowHeader())
                {
                    if (line != null)
                    {
                        var rowHeaderSize = GetColumnSize(0, false);
                        if (rowHeaderSize > line.Origin)
                            ExpanderBorder.Clip = new RectangleGeometry() { Rect = new Rect((rowHeaderSize - line.Origin), 0, ExpanderBorder.Width, finalSize.Height) };
                        else
                            ExpanderBorder.Clip = null;
                    }
                }
                if (line != null)
                    this.RowPanel.RenderTransform = new TranslateTransform() { X = line.Origin, Y = 0 };
            }
            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region Internal Methods

        internal void UpdateVisibleColumn(Func<DataRowBase> GetDataRowFunc, Func<bool> showRowHeader, Func<int, VisibleLineInfo> getColumnVisibleLineInfo, Func<int, bool, double> getColumnSize, Func<DataColumnBase, bool, double> getRowSize)
        {
            this.GetColumnSize = getColumnSize;
            this.ShowRowHeader = showRowHeader;
            this.GetColumnVisibleLineInfo = getColumnVisibleLineInfo;
            base.InitializeVirtualizingRowControl(GetDataRowFunc);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.CaptionSummaryRowControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                this.ExpanderBorder = null;
                this.GetColumnSize = null;
                this.GetColumnVisibleLineInfo = null;
                this.IsExpandedChanged = null;
                this.RowPanel = null;
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class GroupSummaryRowControl : VirtualizingCellsControl
    {
        #region Ctor

        public GroupSummaryRowControl()
        {
            this.DefaultStyleKey = typeof(GroupSummaryRowControl);
        }

        #endregion

        #region Internal Methods

        internal void UpdateVisibleColumns(Func<DataRowBase> GetDataRowFunc, Func<int, VisibleLineInfo> getColumnVisibleLineInfo, Func<int, bool, double> getColumnSize)
        {
            base.InitializeVirtualizingRowControl(GetDataRowFunc);
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class AddNewRowControl : VirtualizingCellsControl
    {
        #region Fields
        internal bool addNewRowVisualState;
#if UWP
        Border WM_TextBorder = null;
#endif

        #endregion

        #region Ctor

        public AddNewRowControl() : base()
        {
            DefaultStyleKey = typeof(AddNewRowControl);
        }

        #endregion

        #region Dependency Property

        /// <summary>
        /// Get or Set the text displayed in AddNewROw watermark.
        /// </summary>
        public string AddNewRowText
        {
            get { return (string)GetValue(AddNewRowTextProperty); }
            set { SetValue(AddNewRowTextProperty, value); }
        }

        public static readonly DependencyProperty AddNewRowTextProperty =
            DependencyProperty.Register("AddNewRowText", typeof(string), typeof(AddNewRowControl), new PropertyMetadata(GridResourceWrapper.AddNewRowText));

        /// <summary>
        /// Property which helps to position the AddNewRow text when the row header is displayed.
        /// </summary>
        public Thickness TextMargin
        {
            get { return (Thickness)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }

        public static readonly DependencyProperty TextMarginProperty =
            DependencyProperty.Register("TextMargin", typeof(Thickness), typeof(AddNewRowControl), new PropertyMetadata(null));

        public RectangleGeometry TextBorderClip
        {
            get { return (RectangleGeometry)GetValue(TextBorderClipProperty); }
            set { SetValue(TextBorderClipProperty, value); }
        }

        public static readonly DependencyProperty TextBorderClipProperty =
            DependencyProperty.Register("TextBorderClip", typeof(RectangleGeometry), typeof(AddNewRowControl), new PropertyMetadata(null));

        #endregion

        #region Methods

        internal void SetEditMode()
        {
            this.addNewRowVisualState = true;
            ApplyAddNewRowVisualState();
        }

        internal void RemoveEditMode()
        {
            this.addNewRowVisualState = false;
            ApplyAddNewRowVisualState();
        }

        internal void ApplyAddNewRowVisualState(bool canApplyDefaultState = true)
        {
            if (addNewRowVisualState)
                VisualStateManager.GoToState(this, "Edit", true);
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "Normal", true);
        }
        
        /// <summary>
        /// Updates the WaterMarkText wrapper clipping.
        /// </summary>
        internal void UpdateTextBorder()
        {
            if (this.GetVisibleLineOrigin != null)
            {
                double origin = this.GetVisibleLineOrigin();
                if (origin > 0)
                {
#if !WinRT
                    this.TextMargin = new Thickness(origin + 3, 0, 0, 0);
#else
                    this.TextMargin = new Thickness(origin + 8, 0, 0, 0);
#endif

#if !WPF
                    if (WM_TextBorder != null)
                        WM_TextBorder.Clip = this.TextBorderClip = new RectangleGeometry() { Rect = new Rect(new Point(origin, 0), new Size(this.DesiredSize.Width, this.DesiredSize.Height)) };
#else
                    this.TextBorderClip = new RectangleGeometry() { Rect = new Rect(new Point(origin, 0), new Size(this.DesiredSize.Width, this.DesiredSize.Height)) };
#endif

                }
                else
                {
#if !WinRT
                    this.TextMargin = new Thickness(3, 0, 0, 0);
#else
                    this.TextMargin = new Thickness(8, 0, 0, 0);
#endif

#if !WPF
                    if (WM_TextBorder != null)
                        WM_TextBorder.Clip = this.TextBorderClip = null;
#else
                    this.TextBorderClip = null;
#endif
                }
            }
        }

        protected override void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateTextBorder();
            base.OnSizeChanged(sender, e);
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
                VisualStateManager.GoToState(this, "Edit", true);
            UpdateTextBorder();
            base.OnLoaded(sender, e);
        }



#if WPF
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            // For AddNewRowControl, Foreground is differ from normal VirtualizingCellsControl. So SelectionForegroundBrushProperty is not set, need to set Foreground
            this.UpdateSelectionForegroundBrush();
#if UWP
            WM_TextBorder = GetTemplateChild("WM_TextBorder") as Border;
#endif

            //(WRT-3519) Below Code inserted to apply the correct VisualState for AddNewRow while switching themes
            if (this.addNewRowVisualState)
            {
                this.ApplyAddNewRowVisualState(false);
#if UWP
                //(WRT-3519) Below Code inserted to apply the correct VisualState for AddNewRow while switching themes
                this.UpdateSelectionBorderClip();
#endif
            }
            else
            {
                this.ApplyAddNewRowVisualState(false);
#if UWP
                this.UpdateTextBorder();
#endif
            }

        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class UnBoundRowControl : VirtualizingCellsControl
    {
        #region Ctr

        public UnBoundRowControl()
        {
            this.DefaultStyleKey = typeof(UnBoundRowControl);
        }
        #endregion
    }
}
