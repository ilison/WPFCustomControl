#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using System.Collections.Specialized;
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.UI.Xaml.Grid;
#if WinRT || UNIVERSAL
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Automation.Peers;
#endif
using Syncfusion.Data.Extensions;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Syncfusion.UI.Xaml.Controls.DataPager
{
    [TemplatePart(Name = "PART_NumericButtonPanel", Type = typeof(NumericButtonPanel))]
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollableContentViewer))]
    [TemplatePart(Name = "PART_FirstPageButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_PreviousPageButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_LastPageButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_NextPageButton", Type = typeof(Button))]
    [StyleTypedProperty(Property = "NumericButtonStyle", StyleTargetType = typeof(NumericButton))]
    public class SfDataPager : Control, IDisposable
    {
        #region Private Members

        private Button firstPageButton;
        private Button lastPageButton;
        private Button previousPageButton;
        private Button nextPageButton;

        private bool isPageCountNotSet;
        private bool pageIndexChangedInternally;
        private bool isLoaded;
        private bool isDelayApplyVisualState;
        private bool pageIndexChangedBeforeLoad;
        private bool isElipsisElementClicked;
        private bool enableGridPaging = true;
        private bool isPageSizeDefinedBeforeLoad;
        private bool isPageCountSetInternal = false;
        private bool isdisposed = false;

        #endregion

        #region Internal Members

        internal ItemGenerator ItemGenerator;
        internal NumericButtonPanel ItemsPanel;
        internal ScrollableContentViewer ScrollViewer;
        internal PageNavigationController NavigationController;

        internal bool InManipulation;

        #endregion

        #region Public Members

        public bool EnableGridPaging
        {
            get { return enableGridPaging; }
            set { enableGridPaging = value; }
        }

        #endregion

        #region Dependency Registration

        /// <summary>
        /// Gets or sets a collection that is used to generate the content of the SfDataPager.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of the SfDataPager. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// No need to set the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.Source"/> when <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.UseOnDemandPaging"/> is enabled.
        /// </remarks>
        public IEnumerable Source
        {
            get { return (IEnumerable) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.Source dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.Source dependency property.
        /// </remarks>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof (IEnumerable), typeof (SfDataPager),
                                        new PropertyMetadata(null, OnSourcePropertyChanged));

        /// <summary>
        /// Gets or sets the collection that is used to bind the SfDataPager source to other elements.
        /// </summary>
        /// <value>
        /// The collection that is used to bind the SfDataPager to other elements.
        /// </value>
        public PagedCollectionView PagedSource
        {
            get { return (PagedCollectionView) GetValue(PagedSourceProperty); }
            set { SetValue(PagedSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PagedSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PagedSource dependency property.
        /// </remarks>
        public static readonly DependencyProperty PagedSourceProperty =
            DependencyProperty.Register("PagedSource", typeof (PagedCollectionView), typeof (SfDataPager),
                                        new PropertyMetadata(null));

        internal int cachedpagecount = -1;

        /// <summary>
        /// Gets or sets the page count when OnDemandPaging is enabled.
        /// </summary>
        /// <value>
        /// Number of pages to be displayed when OnDemandPaging is enabled. The default value is <b>zero</b>.
        /// </value>
        /// <remarks>
        /// Specify the number of pages the data needs to populate when <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.UseOnDemandPaging"/> is enabled.
        /// </remarks>
        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set 
            {
                if (UseOnDemandPaging || isPageCountSetInternal)
                    SetValue(PageCountProperty, value);
                else
                    cachedpagecount = value;
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageCount dependency property.
        /// </remarks>
        public static readonly DependencyProperty PageCountProperty =
            DependencyProperty.Register("PageCount", typeof(int), typeof(SfDataPager), new PropertyMetadata(0,OnPageCountChanged));


        /// <summary>
        /// Gets or sets the number of records to display on a page in SfDataPager.
        /// </summary>
        /// <value>
        /// The number of records to display on a single page. The default value is <b>zero</b>.
        /// </value>
        /// <remarks>
        /// Based on <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageSize"/>, the total number of pages will be created. When the PageSize is not specified, all the records will be displayed in a single page.
        /// </remarks>
        public int PageSize
        {
            get { return (int) GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageSize dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageSize dependency property.
        /// </remarks>
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register("PageSize", typeof(int), typeof(SfDataPager), new PropertyMetadata(0, OnPageSizeChanged));

        /// <summary>
        /// Gets or sets the number of numeric button that needs to be displayed in SfDataPager.
        /// </summary>
        /// <value>
        /// Number of numeric buttons.
        /// </value>
        /// <remarks>
        /// Based on the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageSize"/>, total number of pages will be created. If the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageCount"/> is greater than 5, only five numeric buttons will be displayed and it can be changed by using this property.
        /// </remarks>
        public int NumericButtonCount
        {
            get { return (int) GetValue(NumericButtonCountProperty); }
            set { SetValue(NumericButtonCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButtonCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButtonCount dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumericButtonCountProperty =
            DependencyProperty.Register("NumericButtonCount", typeof (int), typeof (SfDataPager),
                                        new PropertyMetadata(5, OnNumericButtonCountChanged));

        /// <summary>
        /// Gets or sets the index of selected page in SfDataPager.
        /// </summary>
        /// <value>
        /// Index of selected page in SfDataPager. The default value is <b>zero</b>.
        /// </value>
        public int PageIndex
        {
            get { return (int) GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndex dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndex dependency property.
        /// </remarks>
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register("PageIndex", typeof (int), typeof (SfDataPager),
                                        new PropertyMetadata(0, OnPageIndexChanged));

        /// <summary>
        /// Gets or sets the style applied to all the numeric button in SfDataPager.
        /// </summary>
        /// <value>
        /// The style that is applied to all the numeric button in SfDataPager. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a numeric button, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButton"/>.
        /// </remarks>
        public Style NumericButtonStyle
        {
            get { return (Style) GetValue(NumericButtonStyleProperty); }
            set { SetValue(NumericButtonStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButtonStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButtonStyle dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumericButtonStyleProperty =
            DependencyProperty.Register("NumericButtonStyle", typeof (Style), typeof (SfDataPager),
                                        new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that defines the button to be displayed in SfDataPager.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageDisplayMode"/> enumeration that specifies how the buttons are displayed in SfDataPager for user interactions.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageDisplayMode.FirstLastPreviousNextNumeric"/>.
        /// </value>
        /// <remarks>
        /// The numeric and navigation buttons can be displayed in different ways using <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageDisplayMode"/>. 
        /// </remarks>
        public PageDisplayMode DisplayMode
        {
            get { return (PageDisplayMode) GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.DisplayMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.DisplayMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof (PageDisplayMode), typeof (SfDataPager),
                                        new PropertyMetadata(PageDisplayMode.FirstLastPreviousNextNumeric,
                                                             OnDisplayModeChanged));

        /// <summary>
        /// Gets or sets a value that indicates how the auto ellipsis button are displayed in SfDataPager to navigate the next set of pages when PageCount is greater than the NumericButtonCount.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisMode"/> enumeration that specifies how the auto ellipsis button are displayed in SfDataPager.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisMode.None"/>.
        /// </value>
        /// <remarks>
        /// When the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageCount"/> is greater than the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButtonCount"/>, auto ellipsis button will be displayed.
        ///<seealso cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisText"/>
        /// </remarks>
        public AutoEllipsisMode AutoEllipsisMode
        {
            get { return (AutoEllipsisMode) GetValue(AutoEllipsisModeProperty); }
            set { SetValue(AutoEllipsisModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoEllipsisModeProperty =
            DependencyProperty.Register("AutoEllipsisMode", typeof(AutoEllipsisMode), typeof(SfDataPager),
                                        new PropertyMetadata(AutoEllipsisMode.None, OnAutoEllipsisModeChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the OnDemandPaging is enabled or not.
        /// </summary>
        /// <value>
        /// The value which denotes whether the OnDemandPaging is enabled or not. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// By default, entire data collection will be loaded to the SfDataPager control. 
        /// You can also load the data to current page only in on-demand using OnDemandPaging.
        /// OnDemandPaging can be used by the following properties and the event,
        /// 1. Set <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.UseOnDemandPaging"/> as true.
        /// 2. Set <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageCount"/> to specify the number of pages the data needs to populate.
        /// 3. <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.OnDemandLoading"/> event to populate the records.
        /// <b>Note</b>: No need to set the < see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.Source"/> property while using OnDemandPaging technique.
        /// </remarks>
        public bool UseOnDemandPaging
        {
            get { return (bool) GetValue(UseOnDemandPagingProperty); }
            set { SetValue(UseOnDemandPagingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.UseOnDemandPaging dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.UseOnDemandPaging dependency property.
        /// </remarks>
        public static readonly DependencyProperty UseOnDemandPagingProperty =
            DependencyProperty.Register("UseOnDemandPaging", typeof (bool), typeof (SfDataPager),
                                        new PropertyMetadata(false, OnUseOnDemandPagingChanged));

        private static void OnUseOnDemandPagingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var pager = obj as SfDataGrid;
            if (pager == null)
                return;

            
        }

        /// <summary>
        /// Gets or sets the background of selected page button and navigation buttons.
        /// </summary> 
        /// <value>
        /// The brush that highlights the background of selected page button and navigation buttons. The default value is <b>DarkGray</b>.
        /// </value>
        public Brush AccentBackground
        {
            get { return (Brush)GetValue(AccentBackgroundProperty); }
            set { SetValue(AccentBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AccentBackground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AccentBackground dependency property.
        /// </remarks>
        public static readonly DependencyProperty AccentBackgroundProperty =
            DependencyProperty.Register("AccentBackground", typeof(Brush), typeof(SfDataPager), new PropertyMetadata(new SolidColorBrush(Colors.DarkGray), OnAccentThemeBrushChanged));

        /// <summary>
        /// Gets or sets the text that needs to be displayed in auto ellipsis button.
        /// </summary>
        /// <value>
        /// The text content which needs to be displayed in the auto ellipsis button.
        /// The default value is <b>...</b>
        /// </value>
        /// <remarks>
        /// When the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageCount"/> is greater than the <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.NumericButtonCount"/>, you can display the auto ellipsis button by setting <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisMode"/>.
        /// </remarks>
        public string AutoEllipsisText
        {
            get { return (string)GetValue(AutoEllipsisTextProperty); }
            set { SetValue(AutoEllipsisTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AutoEllipsisText dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoEllipsisTextProperty =
            DependencyProperty.Register("AutoEllipsisText", typeof(string), typeof(SfDataPager), new PropertyMetadata("...",OnAutoEllipsisTextChanged));

        /// <summary>
        /// Gets or sets the foreground of selected page button and navigation buttons.
        /// </summary>
        /// <value>
        /// The brush that highlights the foreground of selected page button and navigation buttons. The default value is <b>White</b>.
        /// </value>
        public Brush AccentForeground
        {
            get { return (Brush)GetValue(AccentForegroundProperty); }
            set { SetValue(AccentForegroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AccentForeground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.AccentForeground dependency property.
        /// </remarks>
        public static readonly DependencyProperty AccentForegroundProperty =
            DependencyProperty.Register("AccentForeground", typeof(Brush), typeof(SfDataPager), new PropertyMetadata(new SolidColorBrush(Colors.White), OnAccentForegroundBrushChanged));


        /// <summary>
        /// Gets or sets a value that indicates the dimension by which child elements are stacked.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Controls.Orientation"/> enumeration that specifies the orientation of SfDataPager.
        /// The default value is <see cref="System.Windows.Controls.Orientation.Horizontal"/>.
        /// </value>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.Orientation dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.Orientation dependency property.
        /// </remarks>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SfDataPager), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));
        #endregion

        #region Public Events

        /// <summary>
        /// Occurs once the pages are navigated.
        /// </summary>
        public event PageIndexChangedEventhandler PageIndexChanged;

        /// <summary>
        /// Occurs while navigating between the pages.
        /// </summary>
        /// <remarks>
        /// Page navigation can be skipped by using <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndexChanging"/> event.
        /// </remarks>
        public event PageIndexChangingEventhandler PageIndexChanging;

        /// <summary>
        /// Occurs when the pager moves to corresponding page and load the data in on-demand.
        /// </summary>
        /// <remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Controls.DataPager.OnDemandLoadingEventHandler"/>.
        /// <seealso cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.LoadDynamicItems"/> method.
        /// </remarks>
        public event OnDemandLoadingEventHandler OnDemandLoading;

        #endregion

        #region Ctor

        public SfDataPager()
        {
            this.DefaultStyleKey = typeof (SfDataPager);
            ItemGenerator = new ItemGenerator(this);
            NavigationController = new PageNavigationController(this);
        }

        #endregion

        #region Dependency Callback Methods

        private static void OnPageIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            if (!dataPager.pageIndexChangedInternally && dataPager.isLoaded)
            {
                dataPager.NavigationController.HideCurrentPage((int) args.OldValue);
                    dataPager.MoveToPage(Convert.ToInt32(args.OldValue),Convert.ToInt32(args.NewValue));
            }

            if (!dataPager.isLoaded && !dataPager.pageIndexChangedInternally)
            {
                dataPager.pageIndexChangedBeforeLoad = true;
            }
        }

        private static void OnDisplayModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            dataPager.SetDisplayMode((PageDisplayMode) args.NewValue);
        }

        private static void OnSourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;

            if (dataPager.PagedSource != null)
            {
                dataPager.UnWireEvents();
                var count = dataPager.Source != null ? dataPager.Source.Cast<object>().Count() : 0;

                if (args.NewValue == null || count == 0)
                {
                    dataPager.ItemGenerator.Items.Clear();
                    if (dataPager.ItemsPanel != null)
                        dataPager.ItemsPanel.Children.Clear();
                    dataPager.ClearValue(SfDataPager.PagedSourceProperty);
                    dataPager.ClearValue(SfDataPager.PageIndexProperty);
                    dataPager.ClearValue(SfDataPager.NumericButtonCountProperty);
                    dataPager.ClearValue(SfDataPager.PageCountProperty);
                    //dataPager.ClearValue(SfDataPager.PageSizeProperty);
                }
            }
                            
            dataPager.InitiatePageSource((IEnumerable) args.NewValue);
            
            if (dataPager.PageSize > 0)
            {
                dataPager.InitializePageCount((IEnumerable) args.NewValue);
            }
            else
            {
                dataPager.isPageCountNotSet = true;
            }            
            
            if(dataPager.PagedSource!=null)
            dataPager.WireEvents();
            if (dataPager.ItemsPanel != null && !dataPager.pageIndexChangedBeforeLoad)
                dataPager.RefreshView();
                     
            if (dataPager.ItemsPanel != null)
            {
                if (dataPager.pageIndexChangedBeforeLoad)
                    dataPager.MoveToPage(dataPager.PageIndex);
                else
                    dataPager.MoveToFirstPage();
                dataPager.pageIndexChangedBeforeLoad = false;
            }
            dataPager.SetVisualState();
        }

        private static void OnAccentThemeBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            if (dataPager.ItemGenerator != null)
            {
                dataPager.ItemGenerator.HighlightThemeBrush = (Brush) args.NewValue;
            }
        }

        private static void OnAccentForegroundBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            if (dataPager.ItemGenerator != null)
            {
                dataPager.ItemGenerator.HighlightForegroundBrush = (Brush)args.NewValue;
            }
        }

        private static void OnPageSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            //Page Size value is not set when using binding with Display mode without NumericButton like FirstLastPreviousNext
            //Because the this loop is exected based on the ItemGenerator, but the ItemGenerator is set while having the numeric button
            if (dataPager.isLoaded && dataPager.PagedSource != null && (dataPager.PagedSource.Count != 0 || dataPager.UseOnDemandPaging))
            {
                dataPager.isPageCountSetInternal = true;
                dataPager.PagedSource.PageSize = (int)args.NewValue;
                dataPager.PagedSource.ResetCache();
                if (dataPager.UseOnDemandPaging)
                {
                    dataPager.PagedSource.MaxItemsCount = (int)args.NewValue * dataPager.PageCount;
                }
                dataPager.MoveToFirstPage();
                if (dataPager.PageSize == 0)
                {
                    dataPager.PageCount = 1;
                    (dataPager.ItemGenerator.Items.FirstOrDefault().Element as NumericButton).IsCurrentPage = true;
                    dataPager.SetVisualState();
                }
                else
                {
                    // WPF-23042-PageCount is not updated while changing the PageSize in OnDemandLoading 
                    if (!dataPager.UseOnDemandPaging)
                        dataPager.PageCount = dataPager.PagedSource.PageCount;
                    dataPager.RefreshView();
                }
                dataPager.isPageCountSetInternal = false;
            }
            else
                dataPager.isPageSizeDefinedBeforeLoad = true;
        }
        

        private static void OnNumericButtonCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            if (dataPager.ItemGenerator.Items.Count() != 0)
            {
                if (dataPager.isLoaded)
                {
                    dataPager.MoveToFirstPage();
                    if (dataPager.ItemsPanel != null)
                        dataPager.RefreshView();
                }
            }
        }

        private static void OnAutoEllipsisModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;
            if (dataPager != null && dataPager.ItemsPanel != null)
                dataPager.RefreshView();
        }
        private static void OnAutoEllipsisTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataPager = d as SfDataPager;              
            if(dataPager.ItemGenerator!=null && dataPager.ItemGenerator.Items!=null)
            { 
            var ellipsisElement=dataPager.ItemGenerator.Items.Where(item => item.IsElipsisElement);                
            foreach(var element in ellipsisElement)
            (element.Element as NumericButton).Content = e.NewValue.ToString();
            }
        } 
        private static void OnPageCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataPager = obj as SfDataPager;

            if (dataPager == null || dataPager.ItemsPanel == null)
                return;
            //Page Count value is not set when using binding with Display mode without NumericButton like FirstLastPreviousNext
            //Because the this loop is exected based on the ItemGenerator, but the ItemGenerator is set while having the numeric button
            if (dataPager.UseOnDemandPaging && !dataPager.isPageCountSetInternal && dataPager.PagedSource!=null)
            {
                dataPager.PagedSource.MaxItemsCount = (int)args.NewValue * dataPager.PageSize;
                dataPager.SetVisualState();
                //WPF-21511 PageIndex changed while changing the PageCount at run time                
                if ((int)args.NewValue < (int)args.OldValue && (int)args.NewValue<=dataPager.PageIndex)
                {
                    dataPager.MoveToPage(((int)args.NewValue - 1));
                }
                dataPager.RefreshView();
            }
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var sfDataPager = (SfDataPager) d;
            sfDataPager.SetOrientationMode();
            //Since we need to Rearrange the items, to transform the individual items and Auto-Elipse Item. We are calling InvalidateArrange here.
            if (sfDataPager.ItemsPanel == null)
            {
                sfDataPager.isDelayApplyVisualState = true;
                return;
            }
            sfDataPager.ItemsPanel.InvalidateMeasure();
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
            isLoaded = true;
            if (ItemsPanel != null)
            {
                ItemsPanel.DataPager = null;
                ItemsPanel.Children.Clear();
            }
            UnWireButtonClickEvents();

            ItemsPanel = this.GetTemplateChild("PART_NumericButtonPanel") as NumericButtonPanel;
            ScrollViewer = this.GetTemplateChild("PART_ScrollViewer") as ScrollableContentViewer;
            firstPageButton = this.GetTemplateChild("PART_FirstPageButton") as Button;
            lastPageButton = this.GetTemplateChild("PART_LastPageButton") as Button;
            previousPageButton = this.GetTemplateChild("PART_PreviousPageButton") as Button;
            nextPageButton = this.GetTemplateChild("PART_NextPageButton") as Button;
            if (ItemsPanel != null)
            {
                ItemsPanel.DataPager = this;
                if (isDelayApplyVisualState)
                    SetOrientationMode();
                isDelayApplyVisualState = false;
            }

            this.WireButtonClickEvents();            

            if (this.PagedSource != null && this.isPageSizeDefinedBeforeLoad)
            {
                this.PagedSource.PageSize = this.PageSize;
            }

            if (this.isPageCountNotSet && !UseOnDemandPaging)
            {
                this.InitializePageCount(this.Source);
            }
            else
            {
                //To set pagecount in OnDemandPaging when PageCount set before set UseOnDemandPaging to true
                if (cachedpagecount != -1 && cachedpagecount > 0)
                    this.PageCount = cachedpagecount;
            }

            if (UseOnDemandPaging)
            {
                if (EnableGridPaging)
                    this.PagedSource = new GridPagedCollectionViewWrapper();
                else
                    this.PagedSource = new PagedCollectionView();

                this.WireEvents();
                this.PagedSource.UseOnDemandPaging = this.UseOnDemandPaging;
                this.PagedSource.PageSize = this.PageSize;
                this.PagedSource.MaxItemsCount = this.PageCount * this.PageSize;
                this.PagedSource.MoveToPage(this.PageIndex);
            }

            if (pageIndexChangedBeforeLoad && this.PagedSource != null)
            {
                this.MoveToPage(this.PageIndex);
                pageIndexChangedBeforeLoad = false;
            }

            this.ItemGenerator.HighlightThemeBrush = AccentBackground;
            this.ItemGenerator.HighlightForegroundBrush = AccentForeground;
            this.SetVisualState();
            this.SetDisplayMode(this.DisplayMode);
        }

        private void WireButtonClickEvents()
        {
            if (firstPageButton != null)
                firstPageButton.Click += OnFirstPageButtonClick;
            if (lastPageButton != null)
                lastPageButton.Click += OnLastPageButtonClick;
            if (previousPageButton != null)
                previousPageButton.Click += OnPreviousPageButtonClick;
            if (nextPageButton != null)
                nextPageButton.Click += OnNextPageButtonClick;
        }

        private void UnWireButtonClickEvents()
        {
            if (firstPageButton != null)
                firstPageButton.Click -= OnFirstPageButtonClick;
            if (lastPageButton != null)
                lastPageButton.Click -= OnLastPageButtonClick;
            if (previousPageButton != null)
                previousPageButton.Click -= OnPreviousPageButtonClick;
            if (nextPageButton != null)
                nextPageButton.Click -= OnNextPageButtonClick;
        }
#if WinRT || UNIVERSAL
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            this.HandleKey(e.Key);
            e.Handled = true;
        }
#else
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            this.HandleKey(e.Key);
            e.Handled = true;
        }
#endif

#if WinRT || UNIVERSAL
        protected override void OnManipulationStarted(ManipulationStartedRoutedEventArgs e)
#else
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
#endif
        {
            base.OnManipulationStarted(e);
            this.InManipulation = true;
            e.Handled = true;
        }

#if WinRT || UNIVERSAL
        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
#else
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
#endif
        {
            base.OnManipulationCompleted(e);
            this.InManipulation = false;
            e.Handled = true;
        }

#if WinRT || UNIVERSAL
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            VisualStateManager.GoToState(this, "PointerEnterd", true);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            VisualStateManager.GoToState(this, "PointerExited", true);
        }
#else

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            VisualStateManager.GoToState(this, "PointerEnterd", true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            VisualStateManager.GoToState(this, "PointerExited", true);
        }

#endif

        #endregion

        #region Private Methods

        private void SetOrientationMode()
        {
            switch (Orientation)
            {
                case Orientation.Vertical:
                    {
                        VisualStateManager.GoToState(this, "Vertical", true);
                        if (ScrollViewer != null)
                        {
                            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                        }
#if !WinRT && !UNIVERSAL
                        if (this.ItemsPanel != null)
                        {
                            this.ItemsPanel.DefaultItemSize.Height = 35;
                            this.ItemsPanel.DefaultItemSize.Width = 40;
                        }
#endif
                        break;
                    }
                case Orientation.Horizontal:
                    {
                        VisualStateManager.GoToState(this, "Horizontal", true);
                        if (ScrollViewer != null)
                        {
                            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                        }
#if !WinRT && !UNIVERSAL
                        if (this.ItemsPanel != null)
                        {
                            this.ItemsPanel.DefaultItemSize.Height = 40;
                            this.ItemsPanel.DefaultItemSize.Width = 35;
                        }
#endif
                        break;
                    }
            }
        }

        private void SetDisplayMode(PageDisplayMode displayMode)
        {
            if(!isLoaded)
                return;

            if (displayMode.HasFlag(PageDisplayMode.First))
            {
                if (firstPageButton != null)
                    this.firstPageButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (firstPageButton != null)
                    this.firstPageButton.Visibility = Visibility.Collapsed;
            }

            if (displayMode.HasFlag(PageDisplayMode.Last))
            {
                if (lastPageButton != null)
                    this.lastPageButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (lastPageButton != null)
                    this.lastPageButton.Visibility = Visibility.Collapsed;
            }

            if (displayMode.HasFlag(PageDisplayMode.Previous))
            {
                if (previousPageButton != null)
                    this.previousPageButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (previousPageButton != null)
                    this.previousPageButton.Visibility = Visibility.Collapsed;
            }

            if (displayMode.HasFlag(PageDisplayMode.Next))
            {
                if (nextPageButton != null)
                    this.nextPageButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (nextPageButton != null)
                    this.nextPageButton.Visibility = Visibility.Collapsed;
            }

            if (displayMode.HasFlag(PageDisplayMode.Numeric))
            {
                if (ScrollViewer != null)
                    this.ScrollViewer.Visibility = Visibility.Visible;
            }
            else
            {
                if (ScrollViewer != null)
                    this.ScrollViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void SetVisualState()
        {
            var count = this.Source != null && !UseOnDemandPaging ? 
                        (this.Source is IQueryable ? this.Source.AsQueryable().Count() : 
                        this.Source.Cast<object>().Count()) : 0;
            //If the Page size is less than zero we need to Disable First, Previous, Next and LastPage page Button 
            if ((this.PageSize==0 && this.PageCount==1) ||(this.PageIndex == 0 && this.PageIndex == this.PageCount - 1) 
                || (this.Source == null && this.PageCount == 0)
                || (count == 0 && !UseOnDemandPaging) || (this.PageSize < 0))
                VisualStateManager.GoToState(this, "LeftRightButtonsDisabled", true);
            else if (this.PageIndex == 0)
                VisualStateManager.GoToState(this, "LeftButtonsDisabled", true);
            else if (this.PageIndex == this.PageCount - 1)
				VisualStateManager.GoToState(this, "RightButtonsDisabled", true);
			else
                VisualStateManager.GoToState(this, "Normal", true); 
	    }
#if WinRT || UNIVERSAL
        private void HandleKey(VirtualKey key)
        {
            switch (key)
            {
                case VirtualKey.Right:
                    {
                        if(Orientation == Orientation.Horizontal)
	                        this.MoveToNextPage();
                    }
                    break;
                case VirtualKey.Left:
                    {
                        if(Orientation == Orientation.Horizontal)
	                        this.MoveToPreviousPage();
                    }
                    break;
                case VirtualKey.Up:
                    {
                        if (Orientation == Orientation.Vertical)
                            this.MoveToPreviousPage();
                    }
                    break;
                case VirtualKey.Down:
                    {
                        if (Orientation == Orientation.Vertical)
                            this.MoveToNextPage();
                    }
                    break;
                case VirtualKey.Home:
                    {
                        this.MoveToFirstPage();
                    }
                    break;
                case VirtualKey.End:
                    {
                        this.MoveToLastPage();
                    }
                    break;
            }
        }
#else
        private void HandleKey(Key key)
        {
            switch (key)
            {
                case Key.Right:
                    {
                        if(Orientation == Orientation.Horizontal)
                            this.MoveToNextPage();
                    }
                    break;
                case Key.Left:
                    {
                        if (Orientation == Orientation.Horizontal)
                            this.MoveToPreviousPage();
                    }
                    break;
                case Key.Up:
                    {
                        if (Orientation == Orientation.Vertical)
                            this.MoveToPreviousPage();
                    }
                    break;
                case Key.Down:
                    {
                        if (Orientation == Orientation.Vertical)
                            this.MoveToNextPage();
                    }
                    break;
                case Key.Home:
                    {
                        this.MoveToFirstPage();
                    }
                    break;
                case Key.End:
                    {
                        this.MoveToLastPage();
                    }
                    break;
            }
        }
#endif

        private void InitiatePageSource(IEnumerable source)
        {
            if (source != null)
            {
                if (EnableGridPaging)
                    this.PagedSource = new GridPagedCollectionViewWrapper(source);
                else
                    this.PagedSource = new PagedCollectionView(source);
            }
        }

        private void InitializePageCount(IEnumerable source)
        {
            this.isPageCountSetInternal=true;
            if (source != null && this.PageSize > 0)
            {
                this.PageCount = Math.Max(1,
                    (int) Math.Ceiling((double) (source is IQueryable ? source.AsQueryable().Count() : this.Source.Cast<object>().Count())/this.PageSize));
                this.PagedSource.PageSize = this.PageSize;
                this.PagedSource.MoveToPage(this.PageIndex);
            }
            else if (!UseOnDemandPaging)
            {
                this.PageCount = 0;
                //If the PageSize is zero,one NumericButton should be displayed, so we need to set NumericButtonCount and PageCount as 1,
                if (this.PageSize == 0)
                {
                    this.NumericButtonCount = 1;
                    this.PageCount = 1;
                }
            }
            this.isPageCountSetInternal=false;
        }

        public void WireEvents()
        {
            this.PagedSource.PropertyChanged += OnPagedSourcePropertyChanged;

            if (UseOnDemandPaging)
                this.PagedSource.OnDemandItemsLoading += OnDemandItemsLoading;           
        }

        public void UnWireEvents()
        {
            if(this.PagedSource!=null)
               this.PagedSource.PropertyChanged -= OnPagedSourcePropertyChanged;

            if (UseOnDemandPaging && this.PagedSource!=null)
                this.PagedSource.OnDemandItemsLoading -= OnDemandItemsLoading;
        }

        private void RefreshView()
        {
            this.ItemsPanel.internalOffset = true;
            this.ItemsPanel.InvalidateMeasure();
        }

        #endregion

        #region Internal Methods

        internal void MoveToPage(int pageIndex, bool isElipsisElementClicked)
        {
            this.isElipsisElementClicked = isElipsisElementClicked;
            this.MoveToPage(pageIndex);
        }

        #endregion

        #region Public Method's

        /// <summary>
        /// Navigate to the first page.
        /// </summary>
        public void MoveToFirstPage()
        {
            this.MoveToPage(0);
        }

        /// <summary>
        /// Navigate to the last page.
        /// </summary>
        public void MoveToLastPage()
        {
            this.MoveToPage((this.PageCount - 1));
        }

        /// <summary>
        /// Move to the next page.
        /// </summary>
        public void MoveToNextPage()
        {
            var nextPageIndex = this.PageIndex + 1;
            if (nextPageIndex >= this.PageCount)
                return;
            this.MoveToPage(nextPageIndex);
        }

        /// <summary>
        /// Navigate to the previous page.
        /// </summary>
        public void MoveToPreviousPage()
        {
            if (this.PageIndex == 0)
                return;
            var prevIndex = this.PageIndex - 1;
            this.MoveToPage(prevIndex);
        }

        /// <summary>
        /// Navigate to the particular page based on PageIndex.
        /// </summary>
        /// <param name="pageIndex">Index of the page to be navigate.</param>
        public void MoveToPage(int pageIndex)
        {
            MoveToPage(this.PageIndex, pageIndex);
        }

        private void MoveToPage(int oldPageIndex, int pageIndex)
        {
            PageIndexChangingEventArgs changingArgs = null;

            if(oldPageIndex != pageIndex)
            changingArgs = new PageIndexChangingEventArgs()
            {
                NewPageIndex = pageIndex,
                OldPageIndex = oldPageIndex
            };            

            if (changingArgs != null && (this.RaisePageIndexChangingEvent(changingArgs) || changingArgs.NewPageIndex < 0))
                return;

            var newPageIndex = changingArgs == null ? pageIndex : changingArgs.NewPageIndex;

            if (this.PagedSource != null)
            {
                if (this.PagedSource.MoveToPage(newPageIndex))
                {
                    this.NavigationController.MoveToPage(newPageIndex, isElipsisElementClicked);
                    pageIndexChangedInternally = true;
                    this.PageIndex = newPageIndex;
                    pageIndexChangedInternally = false;


                    if (newPageIndex != oldPageIndex)
                    {
                        var changedArgs = new PageIndexChangedEventArgs()
                        {
                            NewPageIndex = newPageIndex,
                            OldPageIndex = changingArgs.OldPageIndex
                        };
                        this.RaisePageIndexChangedEvent(changedArgs);
                    }
                    this.SetVisualState();
                }
            }                        
        }

        /// <summary>
        /// Loads the ItemsSource for corresponding page.
        /// </summary>
        /// <param name="startIndex">Index based on <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageIndex"/> which is ( Number of previous pages * <see cref="Syncfusion.UI.Xaml.Controls.DataPager.SfDataPager.PageSize"/>).</param>
        /// <param name="items">ItemsSource to load</param>
        /// <remarks>
        /// This method can be used when OnDemandPaging technique is used to load the data.
        /// </remarks>
        public void LoadDynamicItems(int startIndex, IEnumerable items)
        {
            if (this.UseOnDemandPaging)
                this.PagedSource.LoadDynamicItems(startIndex, items);
        }

        #endregion

        #region Event Call Back Methods

        private void OnNextPageButtonClick(object sender, RoutedEventArgs e)
        {
            this.MoveToNextPage();
        }

        private void OnPreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            this.MoveToPreviousPage();
        }

        private void OnLastPageButtonClick(object sender, RoutedEventArgs e)
        {
            this.MoveToLastPage();
        }

        private void OnFirstPageButtonClick(object sender, RoutedEventArgs e)
        {
            this.MoveToFirstPage();
        }

        internal bool NeedsFocusToCurrentPage = false;
        private void OnPagedSourcePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ItemsCount")
            {
                if (this.PageCount != this.PagedSource.PageCount)
                {
                    this.isPageCountSetInternal=true;
                    this.PageCount = this.PagedSource.PageCount;
                    this.isPageCountSetInternal=false;
                    if (this.PageIndex >= this.PageCount)
                    {
                        this.MoveToPage((this.PageCount - 1));
                    }
                    this.ItemsPanel.internalOffset = true;
                    this.ItemsPanel.InvalidateMeasure();
                    this.SetVisualState();
                }
                else
                {
                    if (this.PagedSource.ItemCount == 0 && this.PagedSource.PageCount == 0) 
                    {
                        this.isPageCountSetInternal = true;        
                        this.PageCount = 0;
                        this.isPageCountSetInternal = false;
                        this.RefreshView();
                    }
                }               
            }

            if (e.PropertyName == "FilterPredicates")
            {               
                this.MoveToFirstPage();              
            }
        }       

        private void OnDemandItemsLoading(object sender, OnDemandItemsLoadingEventArgs args)
        {
            if (this.OnDemandLoading != null)
            {
                var eventArgs = new OnDemandLoadingEventArgs() {PageSize = args.PageSize, StartIndex = args.StartIndex};
                this.OnDemandLoading(this, eventArgs);
            }
        }

        private bool RaisePageIndexChangingEvent(PageIndexChangingEventArgs args)
        {
            if (this.PageIndexChanging != null)
            {
                this.PageIndexChanging(this, args);
            }
            return args.Cancel;
        }

        private void RaisePageIndexChangedEvent(PageIndexChangedEventArgs args)
        {
            if (this.PageIndexChanged != null)
            {
                this.PageIndexChanged(this, args);
            }
        }

        #endregion
# if WPF
        # region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new SfDataPagerAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
#endregion
#endif
        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SfDataPager"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SfDataPager"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                ScrollViewer.Dispose();
                if (ItemsPanel != null)
                {
                    ItemsPanel.Children.Clear();
                    ItemsPanel = null;
                }
                if (ItemGenerator != null)
                {
                    ItemGenerator.Items.Clear();
                    ItemGenerator = null;
                }
                NavigationController = null;
                PagedSource = null;
            }
            this.UnWireEvents();
            this.UnWireButtonClickEvents();
            isdisposed = true;
        }
    }

    /// <summary>
    /// Provides classes, interfaces and enumerators to create SfDataPager which enable a user 
    /// to paging functionality for data-bound controls. 
    /// </summary>
    class NamespaceDoc
    {
    }

}