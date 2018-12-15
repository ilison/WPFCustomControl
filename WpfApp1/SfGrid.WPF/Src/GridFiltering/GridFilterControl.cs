#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Syncfusion.Data;
using System.Linq.Expressions;
using System.Globalization;
using System.Data;
#if UWP
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Animation;
using Key = Windows.System.VirtualKey;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using ListSortDirection = Syncfusion.Data.ListSortDirection;
using System.Collections;
using System.Threading.Tasks;
using Syncfusion.UI.Xaml.Utility;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections;
using System.Threading;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    [TemplatePart(Name = "PART_CheckBox", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_FilterPopup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_OkButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_DeleteButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ItemsControl", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ClearFilterButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_SortAscendingButton", Type = typeof(SortButton))]
    [TemplatePart(Name = "PART_SortDescendingButton", Type = typeof(SortButton))]
    [TemplatePart(Name = "PART_SearchTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ThumbGripper", Type = typeof(Thumb))]
    [TemplatePart(Name = "CheckboxFilterControl", Type = typeof(CheckboxFilterControl))]
    /// <summary>
    /// Represents a control that contains filter popup to filter the data in GridFilterControl
    /// </summary>
    public class GridFilterControl : ContentControl, IDisposable, INotifyPropertyChanged
    {

        #region Private Members
        ItemPropertiesProvider provider = null;
        List<FilterElement> distinctCollection = null;
        private bool HasSearchedItems = false;
        static double minWidth;
        static double minHeight;
        double filterPopupHeight;
        double filterPopupWidth;
        bool isResizing;
        string colName;
        private string emptyStringValue = string.Empty;
        GridColumn column;
        private AdvancedFilterType advancedFilterType = AdvancedFilterType.TextFilter;
        private bool isdisposed = false;
#if !WPF
        Rect localToggleRect;
        Point localWindowPoint;
        double localTogglepadding;
#if UWP
        Thickness localHeaderCellPading;
#endif
        List<Control> filterPopUpChildControls = new List<Control>();
#endif
        bool allowBlankFilters;
        int checkedItemsCount, unCheckedItemsCount;
        List<FilterPredicate> filterPredicate = null;
        ICollectionViewAdv excelFilterView;
        System.Linq.Expressions.Expression predicate;
        ParameterExpression paramExpression;
#if WPF
        BackgroundWorker bgWorkertoPopulate = new BackgroundWorker();
#endif
        bool canGenerateUniqueItems;
        #endregion

        #region Ctor

        public GridFilterControl()
        {
            this.DefaultStyleKey = typeof(GridFilterControl);
            this.Loaded += OnGridFilterControlLoaded;
        }

        #endregion

        #region CLR Properties

        /// <summary>
        /// Gets or sets the column.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        internal GridColumn Column
        {
            get { return column; }
            set
            {
                column = value;
                if (column != null)
                {
                    colName = column.MappingName;

                    //WPF-25650 - column.DataGrid != null condition added to avoid null exception when update. 
                    //Ref -> OnUpdateEditBinding in GridHeaderCellRenderer -> Clones the column with DataGrid null
                    if (AdvancedFilterControl != null && column.DataGrid != null)
                    {
#if !WP
                        if (Column.IsUnbound || Column.DataGrid.View.IsDynamicBound)
                            AdvancedFilterControl.ColumnDataType = typeof(string);
#else
                        if (Column.IsUnbound)
                            AdvancedFilterControl.ColumnDataType = typeof(string);
#endif
                        else
                        {
                            var pdc = this.Column.DataGrid.View.GetItemProperties();
                            if (pdc == null)
                                AdvancedFilterControl.ColumnDataType = typeof(string);
                            var pd = pdc.GetPropertyDescriptor(this.Column.MappingName);
                            if (pd == null)
                                AdvancedFilterControl.ColumnDataType = typeof(string);
                            else
                                AdvancedFilterControl.ColumnDataType = pd.PropertyType;
                        }
                        var advfiltertype = FilterHelpers.GetAdvancedFilterType(this.Column);
                        if (this.AdvancedFilterType != advfiltertype)
                        {
                            this.AdvancedFilterType = advfiltertype;
                            AdvancedFilterControl.FilterTypeComboItems = null;
                        }
                    }
#if WPF
                    if (Column is GridMaskColumn)
                    {
                        var column1 = Column as GridMaskColumn;
                        emptyStringValue = MaskedEditorModel.GetMaskedText(column1.Mask, string.Empty,
                        column1.DateSeparator,
                        column1.TimeSeparator,
                        column1.DecimalSeparator,
                        NumberFormatInfo.CurrentInfo.NumberGroupSeparator,
                        column1.PromptChar,
                        NumberFormatInfo.CurrentInfo.CurrencySymbol);
                    }
#endif
                    this.FilteredFrom = Column.FilteredFrom;
                }
            }
        }

        /// <summary>
        /// Gets or sets AdvancedFilterType for GridFilterControl.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterType"/> enumeration that specifies which Advanced filter type needs to be loaded.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterType.TextFilter"/>.
        /// </value>        
        public AdvancedFilterType AdvancedFilterType
        {
            get
            {
                return advancedFilterType;
            }
            set
            {
                if (advancedFilterType != value)
                {
                    advancedFilterType = value;
                    SetFilterColumnType();
                }
            }
        }


        #endregion

        #region Dependency Properties
        /// <summary>
        /// Gets or sets the style applied to <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/>.
        /// </summary>
        /// <value>
        /// The style which is applied to <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a AdvancedFilter, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/>.
        /// </remarks>
        public Style AdvancedFilterStyle
        {
            get { return (Style)GetValue(AdvancedFilterStyleProperty); }
            set { SetValue(AdvancedFilterStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.AdvancedFilterStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridFilterControl.AdvancedFilterStyle dependency property.
        /// </remarks>      
        public static readonly DependencyProperty AdvancedFilterStyleProperty =
            GridDependencyProperty.Register("AdvancedFilterStyle", typeof(Style), typeof(GridFilterControl), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style applied to <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/>.
        /// </summary>
        /// <value>
        /// The style which is applied to <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/> in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a CheckboxFilter, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/>.
        /// </remarks>
        public Style CheckboxFilterStyle
        {
            get { return (Style)GetValue(CheckboxFilterStyleProperty); }
            set { SetValue(CheckboxFilterStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.AdvancedFilterStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridFilterControl.AdvancedFilterStyle dependency property.
        /// </remarks>   
        public static readonly DependencyProperty CheckboxFilterStyleProperty =
            GridDependencyProperty.Register("CheckboxFilterStyle", typeof(Style), typeof(GridFilterControl), new GridPropertyMetadata(null));


        #region ResizingThumbVisibility

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.ResizingThumbVisibility dependency property.
        /// </summary>        
        public static readonly DependencyProperty ResizingThumbVisibilityProperty = DependencyProperty.Register(
           "ResizingThumbVisibility", typeof(Visibility), typeof(GridFilterControl), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets a value indicating the ResizingThumb Visibility
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies the visibility of ResizingThumb
        /// The default mode is <see cref="System.Windows.Visibility.Visible"/>. 
        /// </value> 
        public Visibility ResizingThumbVisibility
        {
            get { return (Visibility)this.GetValue(GridFilterControl.ResizingThumbVisibilityProperty); }
            set { this.SetValue(GridFilterControl.ResizingThumbVisibilityProperty, value); }
        }

        #endregion

        #region SortOptionVisibilityProperty

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.SortOptionVisibility dependency property.
        /// </summary>    
        public static readonly DependencyProperty SortOptionVisibilityProperty = DependencyProperty.Register(
          "SortOptionVisibility", typeof(Visibility), typeof(GridFilterControl), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets a value indicating the SortOptionVisibility.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies the visibility of SortOption.
        /// The default mode is <see cref="System.Windows.Visibility.Visible"/>. 
        /// </value> 
        public Visibility SortOptionVisibility
        {
            get { return (Visibility)this.GetValue(GridFilterControl.SortOptionVisibilityProperty); }
            set { this.SetValue(GridFilterControl.SortOptionVisibilityProperty, value); }
        }

        #endregion

        #region AllowBlankFiltersProperty

        /// <summary>
        /// DependencyProperty Registration for AllowBlankFilters
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty AllowBlankFiltersProperty = DependencyProperty.Register(
              "AllowBlankFilters", typeof(bool), typeof(GridFilterControl), new PropertyMetadata(true, null));

        /// Gets or sets a value indicating whether to allow the Blank Filters.
        /// 
        /// <value><see langword="true"/> if ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool AllowBlankFilters
        {
            get { return (bool)this.GetValue(GridFilterControl.AllowBlankFiltersProperty); }
            set { this.SetValue(GridFilterControl.AllowBlankFiltersProperty, value); }
        }

        #endregion

        #region ImmediateUpdateColumnFilter

        /// <summary>
        /// DependencyProperty registration for ImmediateUpdateColumnFilter
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty ImmediateUpdateColumnFilterProperty =
            DependencyProperty.Register("ImmediateUpdateColumnFilter", typeof(bool), typeof(GridFilterControl), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether ImmediateUpdateColumnFilter.
        /// </summary>
        /// <value><see langword="true"/> if ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool ImmediateUpdateColumnFilter
        {
            get { return (bool)GetValue(ImmediateUpdateColumnFilterProperty); }
            set { SetValue(ImmediateUpdateColumnFilterProperty, value); }
        }

        #endregion

        #region FilterPopupHeight

        /// <summary>
        /// DependencyProperty registration for FilterPopupHeight
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty FilterPopupHeightProperty =
            DependencyProperty.Register("FilterPopupHeight", typeof(double), typeof(GridFilterControl), new PropertyMetadata(450.00d, OnFilterPopupHeightChanged));

        private static void OnFilterPopupHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var excelFilterControl = d as GridFilterControl;
            if (!excelFilterControl.isResizing)
                excelFilterControl.filterPopupHeight = (double)e.NewValue;
        }

        /// <summary>
        /// Gets or sets height for Filter popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public double FilterPopupHeight
        {
            get { return (double)GetValue(FilterPopupHeightProperty); }
            set { SetValue(FilterPopupHeightProperty, value); }
        }

        #endregion

        #region FilterPopupWidth

        /// <summary>
        /// DependencyProperty registration for FilterPopupWidth
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty FilterPopupWidthProperty =
            DependencyProperty.Register("FilterPopupWidth", typeof(double), typeof(GridFilterControl), new PropertyMetadata(300.00d, OnFilterPopupWidthChanged));

        private static void OnFilterPopupWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var excelFilterControl = d as GridFilterControl;
            if (!excelFilterControl.isResizing)
                excelFilterControl.filterPopupWidth = (double)e.NewValue;
        }

        /// <summary>
        /// Gets or sets width for filter popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public double FilterPopupWidth
        {
            get { return (double)GetValue(FilterPopupWidthProperty); }
            set { SetValue(FilterPopupWidthProperty, value); }
        }

        #endregion

        #region IsAdvancedFilterVisible
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.IsAdvancedFilterVisible dependency property.
        /// </summary>    
        public static readonly DependencyProperty IsAdvancedFilterVisibleProperty = DependencyProperty.Register(
          "IsAdvancedFilterVisible", typeof(bool), typeof(GridFilterControl), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether AdvancedFilter should be visible or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if AdvancedFilter is visible; otherwise,<b>false</b>. The default value is <b>false</b>. 
        /// </value>  
        public bool IsAdvancedFilterVisible
        {
            get { return (bool)this.GetValue(GridFilterControl.IsAdvancedFilterVisibleProperty); }
            set { this.SetValue(GridFilterControl.IsAdvancedFilterVisibleProperty, value); }
        }

        #endregion

        #region FilterMode
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.FilterMode dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridFilterControl.FilterMode dependency property.
        /// </remarks> 
        public static readonly DependencyProperty FilterModeProperty = DependencyProperty.Register(
          "FilterMode", typeof(FilterMode), typeof(GridFilterControl), new PropertyMetadata(FilterMode.Both, OnFilterModePropertyChanged));

        /// <summary>
        /// Gets or sets a value to specify the FilterMode in GridFilterControl.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.FilterMode"/> enumeration that specifies the FilterMode in SfDataGrid
        /// The default mode is <see cref="Syncfusion.UI.Xaml.Grid.FilterMode.Both"/>. 
        /// </value>                
        public FilterMode FilterMode
        {
            get { return (FilterMode)this.GetValue(GridFilterControl.FilterModeProperty); }
            set { this.SetValue(GridFilterControl.FilterModeProperty, value); }
        }
        /// <summary>
        /// Dependency call back for FilterMode property Changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnFilterModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var filterCtrl = obj as GridFilterControl;
            if (filterCtrl.FilterMode == FilterMode.AdvancedFilter)
                filterCtrl.IsAdvancedFilterVisible = true;
            else if (filterCtrl.FilterMode == FilterMode.CheckboxFilter)
                filterCtrl.IsAdvancedFilterVisible = false;
#if !WPF && !UWP
            filterCtrl.SetFilteredFromVisibility();
#endif
        }

        #endregion

        #region FilteredFrom
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.FilteredFrom dependency property.
        /// </summary>  
        public static readonly DependencyProperty FilteredFromProperty = DependencyProperty.Register(
          "FilteredFrom", typeof(FilteredFrom), typeof(GridFilterControl), new PropertyMetadata(FilteredFrom.None, OnFilteredFromChanged));

        /// <summary>
        /// Gets or sets a value indicating whether column is filtered from Checkbox or Advanced filter.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.FilteredFrom"/> enumeration that specifies the FilteredFrom
        /// The default mode is <see cref="Syncfusion.UI.Xaml.Grid.FilteredFrom.None"/>. 
        /// </value>  
        public FilteredFrom FilteredFrom
        {
            get { return (FilteredFrom)this.GetValue(GridFilterControl.FilteredFromProperty); }
            set { this.SetValue(GridFilterControl.FilteredFromProperty, value); }
        }
        private static void OnFilteredFromChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var gridFilterCtrl = obj as GridFilterControl;
#if !WPF
            if (gridFilterCtrl.CheckboxFilterControl != null)
                gridFilterCtrl.CheckboxFilterControl.FilteredFrom = gridFilterCtrl.FilteredFrom;
#if UWP
            gridFilterCtrl.UpdateFilterState();
#else
            gridFilterCtrl.SetFilteredFromVisibility();
#endif
#endif

            gridFilterCtrl.Column.FilteredFrom = gridFilterCtrl.FilteredFrom;
        }

        #endregion
#if !WPF
        private Visibility filteredFromVisibility = Visibility.Collapsed;
        public Visibility FilteredFromVisibility
        {
            get { return filteredFromVisibility; }
            set
            {
                if (filteredFromVisibility != value)
                {
                    filteredFromVisibility = value;
                    OnPropertyChanged("FilteredFromVisibility");
                }
            }
        }        
#endif
        /// <summary>
        /// Gets or sets the AscendingSortString of GridFilterControl.
        /// </summary>
        /// <value>
        /// A string that specifies AscendingSortString of GridFilterControl. The default value is <c>string.Empty</c>.
        /// </value>
        public string AscendingSortString
        {
            get { return (string)GetValue(AscendingSortStringProperty); }
            set { SetValue(AscendingSortStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.AscendingSortString dependency property.
        /// </summary>
        public static readonly DependencyProperty AscendingSortStringProperty =
            DependencyProperty.Register("AscendingSortString", typeof(string), typeof(GridFilterControl), new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the DescendingSortString of GridFilterControl.
        /// </summary>
        /// <value>
        /// A string that specifies DescendingSortString of GridFilterControl. The default value is <c>string.Empty</c>.
        /// </value>
        public string DescendingSortString
        {
            get { return (string)GetValue(DescendingSortStringProperty); }
            set { SetValue(DescendingSortStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.DescendingSortString dependency property.
        /// </summary>
        public static readonly DependencyProperty DescendingSortStringProperty =
            DependencyProperty.Register("DescendingSortString", typeof(string), typeof(GridFilterControl), new PropertyMetadata(""));

        #region IsOpenProperty

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.IsOpen dependency property.
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
              "IsOpen", typeof(bool), typeof(GridFilterControl), new PropertyMetadata(false, OnIsOpenPropertyChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the filter popup is opened or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if the filter popup is opened; otherwise, <b>false</b>. 
        /// The default value is <b>false</b>.
        /// </value>         
        public bool IsOpen
        {
            get { return (bool)this.GetValue(GridFilterControl.IsOpenProperty); }
            set { this.SetValue(GridFilterControl.IsOpenProperty, value); }
        }

        /// <summary>
        /// Dependency call back for IsOpen property Changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnIsOpenPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

            var gridFilterCtrl = obj as GridFilterControl;
            if (gridFilterCtrl.Column.MappingName == null)
                throw new InvalidOperationException("MappingName is neccessary for Sorting, Grouping and Filtering");

            //WPF-20153 skip to get the PropertyDescriptor where the collection is dynamic. Because it always return null for the dynamic type properties.
            if (gridFilterCtrl.Column != null && !gridFilterCtrl.Column.IsUnbound && gridFilterCtrl.Column.DataGrid.View != null && !gridFilterCtrl.Column.DataGrid.View.IsDynamicBound)
            {
#if !WPF
                PropertyInfoCollection typeInfos = gridFilterCtrl.Column.DataGrid.View.GetItemProperties();
                var typeInfo = typeInfos.GetPropertyInfo(gridFilterCtrl.Column.MappingName);
                if (typeInfo != null && (typeInfo.PropertyType == typeof(int) || typeInfo.PropertyType == typeof(Double) || typeInfo.PropertyType == typeof(Decimal) || typeInfo.PropertyType == typeof(int?) || typeInfo.PropertyType == typeof(double?) || typeInfo.PropertyType == typeof(decimal?) || typeInfo.PropertyType == typeof(long) || typeInfo.PropertyType == typeof(long?)
                   || typeInfo.PropertyType == typeof(short) || typeInfo.PropertyType == typeof(short?) || typeInfo.PropertyType == typeof(ushort) || typeInfo.PropertyType == typeof(ushort?)
                   || typeInfo.PropertyType == typeof(float) || typeInfo.PropertyType == typeof(float?) || typeInfo.PropertyType == typeof(byte) || typeInfo.PropertyType == typeof(byte?) || typeInfo.PropertyType == typeof(sbyte) | typeInfo.PropertyType == typeof(sbyte?)
                   || typeInfo.PropertyType == typeof(uint) || typeInfo.PropertyType == typeof(uint?) || typeInfo.PropertyType == typeof(ulong) || typeInfo.PropertyType == typeof(ulong?)))

#else
                PropertyDescriptorCollection typeInfos = gridFilterCtrl.Column.DataGrid.View.GetItemProperties();
                if (typeInfos == null)
                    return;
                var typeInfo = typeInfos.GetPropertyDescriptor(gridFilterCtrl.Column.MappingName);

                if (typeInfo != null && (typeInfo.PropertyType == typeof(int) || typeInfo.PropertyType == typeof(Double) || typeInfo.PropertyType == typeof(Decimal) || typeInfo.PropertyType == typeof(int?) || typeInfo.PropertyType == typeof(double?) || typeInfo.PropertyType == typeof(decimal?) || typeInfo.PropertyType == typeof(long) || typeInfo.PropertyType == typeof(long?)
                    || typeInfo.PropertyType == typeof(short) || typeInfo.PropertyType == typeof(short?) || typeInfo.PropertyType == typeof(ushort) || typeInfo.PropertyType == typeof(ushort?)
                    || typeInfo.PropertyType == typeof(float) || typeInfo.PropertyType == typeof(float?) || typeInfo.PropertyType == typeof(byte) || typeInfo.PropertyType == typeof(byte?) || typeInfo.PropertyType == typeof(sbyte) | typeInfo.PropertyType == typeof(sbyte?)
                    || typeInfo.PropertyType == typeof(uint) || typeInfo.PropertyType == typeof(uint?) || typeInfo.PropertyType == typeof(ulong) || typeInfo.PropertyType == typeof(ulong?)))
#endif
                {
                    gridFilterCtrl.AscendingSortString = GridResourceWrapper.SortNumberAscending;
                    gridFilterCtrl.DescendingSortString = GridResourceWrapper.SortNumberDescending;
                }
#if !WPF
                else if (typeInfo != null && (typeInfo.PropertyType == typeof(DateTime) || typeInfo.PropertyType == typeof(DateTime?) || typeInfo.PropertyType == typeof(TimeSpan) || typeInfo.PropertyType == typeof(TimeSpan?)))
#else
                else if (typeInfo != null && (typeInfo.PropertyType == typeof(DateTime) || typeInfo.PropertyType == typeof(DateTime?) || typeInfo.PropertyType == typeof(TimeSpan) || typeInfo.PropertyType == typeof(TimeSpan?)))
#endif
                {
                    gridFilterCtrl.AscendingSortString = GridResourceWrapper.SortDateAscending;
                    gridFilterCtrl.DescendingSortString = GridResourceWrapper.SortDateDescending;
                }
                else
                {
                    gridFilterCtrl.AscendingSortString = GridResourceWrapper.SortStringAscending;
                    gridFilterCtrl.DescendingSortString = GridResourceWrapper.SortStringDescending;
                }

            }
            else
            {
                gridFilterCtrl.AscendingSortString = GridResourceWrapper.SortStringAscending;
                gridFilterCtrl.DescendingSortString = GridResourceWrapper.SortStringDescending;
            }
            if (gridFilterCtrl.Column.FilterBehavior == FilterBehavior.StringTyped)
            {
                gridFilterCtrl.AscendingSortString = GridResourceWrapper.SortStringAscending;
                gridFilterCtrl.DescendingSortString = GridResourceWrapper.SortStringDescending;
            }
            gridFilterCtrl.InitializeGridFilterPane();
        }


        #endregion

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridFilterControl.FilterColumnType dependency property.
        /// </summary>
        public static readonly DependencyProperty FilterColumnTypeProperty = DependencyProperty.Register(
            "FilterColumnType", typeof(string), typeof(GridFilterControl), new PropertyMetadata(GridResourceWrapper.TextFilters));

        /// <summary>
        /// Gets or sets a value indicating FilterColumnType in GridFilterControl.
        /// </summary>
        /// <value>
        /// A string that specifies FilterColumnType of GridFilterControl
        /// The default value is TextFilters. 
        /// </value> 
        public string FilterColumnType
        {
            get { return (string)this.GetValue(GridFilterControl.FilterColumnTypeProperty); }
            set { this.SetValue(GridFilterControl.FilterColumnTypeProperty, value); }
        }
        #endregion

#if WPF
        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.Column.DataGrid.View != null)
                this.GenerateItemSource();
        }
        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (CheckboxFilterControl.HasItemsSource)
                SetItemSource();
        }
#endif

#if !WPF
        #region HorizontalOffsetProperty

        /// <summary>
        /// Dependency property Registration for HorizontalOffset
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
              "HorizontalOffset", typeof(double), typeof(GridFilterControl), new PropertyMetadata(0d));

        /// <summary>
        /// Gets or sets HorizontalOffset for the Popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public double HorizontalOffset
        {
            get { return (double)this.GetValue(GridFilterControl.HorizontalOffsetProperty); }
            set { this.SetValue(GridFilterControl.HorizontalOffsetProperty, value); }
        }

        #endregion

        #region VerticalOffsetProperty

        /// <summary>
        /// Dependency property Registration for VerticalOffset
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
              "VerticalOffset", typeof(double), typeof(GridFilterControl), new PropertyMetadata(0d));

        /// <summary>
        /// Gets or sets the VerticalOffset for Popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public double VerticalOffset
        {
            get { return (double)this.GetValue(GridFilterControl.VerticalOffsetProperty); }
            set { this.SetValue(GridFilterControl.VerticalOffsetProperty, value); }
        }


        #endregion
#endif

        #region UIElements

        /// <summary>
        /// Gets or sets the ok button.
        /// </summary>
        /// <value>The ok button.</value>
        internal Button OkButton;

        /// <summary>
        /// Gets or sets the cancel button.
        /// </summary>
        /// <value>The cancel button.</value>
        private Button CancelButton;

        /// <summary>
        /// Gets or sets the resizing thumb.
        /// </summary>
        /// <value>The resizing thumb.</value>
        Thumb ResizingThumb;

        /// <summary>
        /// Gets or sets the filter pop up.
        /// </summary>
        /// <value>The filter pop up.</value>
        public Popup FilterPopUp;

        /// <summary>
        /// Gets or sets the Button for clear filters.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        Button Part_ClearFilterButton;

        /// <summary>
        /// Gets or sets the Button for Advance filters.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        Button Part_AdvancedFilterButton;

        /// <summary>
        /// Gets or sets the button for Ascending sort.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        SortButton PART_SortAscendingButton;

        /// <summary>
        /// Gets or sets the button for Descending sort.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        SortButton PART_SortDescendingButton;

#if !WPF
        /// <summary>
        /// Gets or sets the Filter PopUp Border.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        Border FilterPopUpBorder;
#endif

        /// <summary>
        /// Gets or sets the control for CheckboxFilterControl.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        CheckboxFilterControl CheckboxFilterControl;

        /// <summary>
        /// Gets or sets the control for AdvancedFilterControl.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        AdvancedFilterControl AdvancedFilterControl;

        #endregion

#if UWP
        /// <summary>
        /// Called When[VisibleBoundSizeChanged]
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="VisibleBoundsChangedEventArgs"/> instance containing the event data.</param>

        private void OnFilterPopupVisibleBoundsChanged(ApplicationView sender, object args)
        {            
            var top = sender.VisibleBounds.Top;
            var bottom = sender.VisibleBounds.Bottom;

            if (Window.Current.Bounds.Height == bottom)

                this.MinHeight = Math.Abs(Window.Current.Bounds.Height - top);
            else
                this.MinHeight = Math.Abs(this.MinHeight - (Window.Current.Bounds.Height - bottom));            
        }

#endif

        #region Wire/UnWire Events

        /// <summary>
        /// Wires the events.
        /// </summary>
        private void WireEvents()
        {
#if WPF
            bgWorkertoPopulate.DoWork += bgWorker_DoWork;
            bgWorkertoPopulate.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
#endif
#if UWP
            // UWP - 1815 This event is raised when the value of VisibleBounds changes, 
            // typically as a result of the status bar, app bar, or other chrome being shown or hidden.
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile"
                 && ApplicationView.GetForCurrentView() != null)
            {
                ApplicationView.GetForCurrentView().VisibleBoundsChanged += OnFilterPopupVisibleBoundsChanged;
            }
            
#endif
            if (FilterPopUp != null)
            {
                FilterPopUp.Opened += OnFilterPopUpOpened;
#if !WPF
                FilterPopUpBorder.KeyDown += OnFilterPopUpBorderKeyDown;
#endif
#if WPF
                FilterPopUp.AddHandler(Popup.KeyDownEvent, (KeyEventHandler)OnKeyDown, true);
#endif
            }

            if (OkButton != null)
                OkButton.Click += OnFilterOkButtonClick;

            if (CancelButton != null)
                CancelButton.Click += OnCancelButtonClick;

            if (Part_AdvancedFilterButton != null)
                Part_AdvancedFilterButton.Click += OnAdvancedFiltersButtonClick;

            if (Part_ClearFilterButton != null)
                Part_ClearFilterButton.Click += OnClearFilterClick;

            if (PART_SortAscendingButton != null)
                PART_SortAscendingButton.Click += OnSortAscendingButtonClick;

            if (PART_SortDescendingButton != null)
                PART_SortDescendingButton.Click += OnSortDescendingButtonClick;

            if (this.ResizingThumb != null)
            {
#if UWP
                this.ResizingThumb.PointerEntered += OnResizingThumbEntered;
                this.ResizingThumb.PointerMoved += OnResizingThumbMoved;
                this.ResizingThumb.PointerExited += OnResizingThumbExited;
                this.ResizingThumb.PointerReleased += OnResizingThumbReleased;
#else
                this.ResizingThumb.DragDelta += OnResizingThumbMoved;
#endif

            }

            if (this.Column != null && this.Column.DataGrid != null && this.Column.DataGrid.View != null)
                provider = this.Column.DataGrid.View.GetPropertyAccessProvider();

            this.Unloaded += OnFilterDropDownUnloaded;
            if (this.Column != null && this.Column.DataGrid != null && this.Column.DataGrid.View != null)
                excelFilterView = this.Column.DataGrid.View;

        }

        /// <summary>
        /// While resuing DetailsViewDataGrid's header row, update excelFilterView and provider
        /// </summary>
        internal void Update()
        {
            if (this.Column != null && this.Column.DataGrid != null && this.Column.DataGrid.View != null && excelFilterView != this.Column.DataGrid.View)
            {
                excelFilterView = this.Column.DataGrid.View;
                provider = this.Column.DataGrid.View.GetPropertyAccessProvider();
            }
        }

        /// <summary>
        /// UnWires the events.
        /// </summary>
#if UWP
        private async void UnWireEvents()
#else
        private void UnWireEvents()
#endif
        {
#if WPF
            bgWorkertoPopulate.DoWork -= bgWorker_DoWork;
            bgWorkertoPopulate.RunWorkerCompleted -= bgWorker_RunWorkerCompleted;
#endif

#if UWP
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile"
                && ApplicationView.GetForCurrentView() != null)
            {
                ApplicationView.GetForCurrentView().VisibleBoundsChanged -= OnFilterPopupVisibleBoundsChanged;
            }                         
#endif
            if (this.distinctCollection != null)
            {
                this.distinctCollection.Clear();
                this.distinctCollection = null;
            }
            if (this.filterPredicate != null)
            {
                this.filterPredicate.Clear();
                this.filterPredicate = null;
            }

            if (FilterPopUp != null)
            {
                FilterPopUp.Opened -= OnFilterPopUpOpened;
#if !WPF
                FilterPopUpBorder.KeyDown -= OnFilterPopUpBorderKeyDown;
#endif
#if WPF
                FilterPopUp.RemoveHandler(Popup.KeyDownEvent, (KeyEventHandler)OnKeyDown);
#endif
            }

            if (OkButton != null)
                OkButton.Click -= OnFilterOkButtonClick;

            if (CancelButton != null)
                CancelButton.Click -= OnCancelButtonClick;

            if (Part_AdvancedFilterButton != null)
                Part_AdvancedFilterButton.Click -= OnAdvancedFiltersButtonClick;

            if (Part_ClearFilterButton != null)
                Part_ClearFilterButton.Click -= OnClearFilterClick;

            if (PART_SortAscendingButton != null)
                PART_SortAscendingButton.Click -= OnSortAscendingButtonClick;

            if (PART_SortDescendingButton != null)
                PART_SortDescendingButton.Click -= OnSortDescendingButtonClick;

            if (this.ResizingThumb != null)
            {
#if UWP
                this.ResizingThumb.PointerEntered -= OnResizingThumbEntered;
                this.ResizingThumb.PointerMoved -= OnResizingThumbMoved;
                this.ResizingThumb.PointerExited -= OnResizingThumbExited;
                this.ResizingThumb.PointerReleased -= OnResizingThumbReleased;
#else
                this.ResizingThumb.DragDelta -= OnResizingThumbMoved;
#endif
            }
#if UWP
            await ThreadPool.RunAsync(delegate(IAsyncAction operation)
            {
                  if (this.CheckboxFilterControl != null && this.CheckboxFilterControl.FilterListBoxItem != null)
                    CheckboxFilterControl.FilterListBoxItem.ForEach((lstItem) =>
                    {
                        lstItem.PropertyChanged -= this.OnFilterElementPropertyChanged;
                        lstItem.FormattedString = null;
                    });
            }, WorkItemPriority.Normal);

#else
            if (this.CheckboxFilterControl != null && this.CheckboxFilterControl.FilterListBoxItem != null)
                CheckboxFilterControl.FilterListBoxItem.ForEach((lstItem) =>
                    {
                        lstItem.PropertyChanged -= this.OnFilterElementPropertyChanged;
                        lstItem.FormattedString = null;
                    });
#endif
            this.Unloaded -= OnFilterDropDownUnloaded;
        }

        /// <summary>
        /// OnFilterDropDownUnloaded event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void OnFilterDropDownUnloaded(object sender, RoutedEventArgs e)
        {
            this.UnWireEvents();
        }
        #endregion

        #region Popup State Listner

        /// <summary>
        /// Called when [filter pop up opened].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OnFilterPopUpOpened(object sender, object e)
        {
            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(Column.DataGrid);
            if ((Column != null && Column.DataGrid != null && !skipValidation && !Column.DataGrid.ValidationHelper.CheckForValidation(true)))
                ((Popup)sender).IsOpen = false;
        }

#if WPF
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (this.IsOpen)
                        this.IsOpen = false;
                    break;
            }
        }
#endif
        #endregion

        #region Clear Filter

        /// <summary>
        /// Called when [clear filter clicked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void OnClearFilterClick(object sender, RoutedEventArgs e)
        {
            this.Column.DataGrid.RunWork(new Action(() =>
                {
                    if (this.Column != null)
                    {
                        this.Column.DataGrid.GridModel.ClearFilters(this.Column);
                        this.Column.DataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(false)));
                    }
                    this.IsOpen = false;
                    if (this.FilteredFrom == FilteredFrom.AdvancedFilter && OkButton != null)
                        OkButton.IsEnabled = false;
                    this.FilteredFrom = FilteredFrom.None;
                    this.CheckboxFilterControl.FilterListBoxItem = null;
                    this.CheckboxFilterControl.FilterListBoxItem = new List<FilterElement>();
                }), this.Column.DataGrid.ShowBusyIndicator);
        }
       
        #endregion

        #region Button Click Listner

        /// <summary>
        /// Called when [cancel button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
#if UWP
            this.Column.DataGrid.Focus(FocusState.Programmatic);
#else
            this.Column.DataGrid.Focus();
#endif
        }

        /// <summary>
        /// Called when [AdvancedFilter button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void OnAdvancedFiltersButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.FilterMode == FilterMode.Both)
            {
                IsAdvancedFilterVisible = !IsAdvancedFilterVisible;
                if (IsAdvancedFilterVisible && FilteredFrom == FilteredFrom.CheckboxFilter)
                    this.AdvancedFilterControl.ResetAdvancedFilterControlValues();
                ResetVisbleFilteringControl();
            }            

            if (CheckboxFilterControl.SearchTextBox != null && string.IsNullOrEmpty(CheckboxFilterControl.SearchTextBox.Text))
            {
                this.CheckboxFilterControl.SearchTextBlockVisibility = Visibility.Visible;
            }
            
            if (this.Column.DataGrid.View == null || !(AdvancedFilterControl.IsFilterHasvalues()) && OkButton != null)
                OkButton.IsEnabled = false;
            //if  AdvancedFilterVisible is false then check the CheckboxFilterControl seleceted items and enable ok button WPF-17915
            if (!IsAdvancedFilterVisible)
            {
                var source = (CheckboxFilterControl.ItemsSource as IEnumerable<FilterElement>);
                if (source != null && source.Count(x => x.IsSelected) != 0 && OkButton != null)
                    OkButton.IsEnabled = true;
            }
            else
            {
                //Use case of this condition is ok button is enabled when filter has values. 
                //so filter ok button is set to enable if the filter type has NUll,NOTNULL,EMPTY,NOTEMPTY types 
                //or any one of the filter combo box has values.WPF-17915
                if (this.Column.DataGrid.View != null && ((AdvancedFilterControl.IsNullOrEmptyFilterType(AdvancedFilterControl.FilterType1)) ||
                    (AdvancedFilterControl.IsNullOrEmptyFilterType(AdvancedFilterControl.FilterType2))) && !(AdvancedFilterControl.IsFilterHasvalues()) && OkButton != null)
                    OkButton.IsEnabled = true;
            }
 
        }

        /// <summary>
        /// Called when [ok button click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void OnFilterOkButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsAdvancedFilterVisible)
            {
                var error1 = this.AdvancedFilterControl["FilterValue1"];
                var error2 = this.AdvancedFilterControl["FilterValue2"];
                if (!string.IsNullOrEmpty(error1) || !string.IsNullOrEmpty(error2))
                    return;
            }
            this.InvokeFilter();
            this.IsOpen = false;
        }

        /// <summary>
        /// Called when Ascending Sort button click
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
#if UWP
        async void OnSortAscendingButtonClick(object sender, RoutedEventArgs e)
#else
        void OnSortAscendingButtonClick(object sender, RoutedEventArgs e)
#endif
        {
#if UWP
            this.Column.DataGrid.SetBusyState("Busy");
            await Task.Delay(100);
#else
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, ae) =>
            {
                Thread.Sleep(50);
                Dispatcher.Invoke(new Action(() =>
                {
#endif
                    SortAsceding();
#if UWP

            this.Column.DataGrid.SetBusyState("Normal");
#else
                }));

            };
            worker.RunWorkerCompleted += (obj, args) => { this.Column.DataGrid.SetBusyState("Normal"); };
            this.Column.DataGrid.SetBusyState("Busy");
            worker.RunWorkerAsync();
#endif
        }

        /// <summary>
        /// Called when Descending Sort button click
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
#if UWP
        async void OnSortDescendingButtonClick(object sender, RoutedEventArgs e)
#else
        void OnSortDescendingButtonClick(object sender, RoutedEventArgs e)
#endif
        {
#if UWP
            this.Column.DataGrid.SetBusyState("Busy");
            await Task.Delay(100);
#else
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (o, ae) =>
            {
                Thread.Sleep(50);
                Dispatcher.Invoke(new Action(() =>
                {
#endif
                    SortDescending();
#if UWP
            this.Column.DataGrid.SetBusyState("Normal");
#else
                }));

            };
            worker.RunWorkerCompleted += (obj, args) => { this.Column.DataGrid.SetBusyState("Normal"); };
            this.Column.DataGrid.SetBusyState("Busy");
            worker.RunWorkerAsync();
#endif
        }
        #endregion

        #region Property Change Listner

        /// <summary>
        /// Called when [filter element property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        internal void OnFilterElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!CheckboxFilterControl.propertyChangedFromSelectAll)
            {
                if (this.ImmediateUpdateColumnFilter)
                {
                    this.InvokeFilter();
                    // When filtering is applied through CheckBoxFilter, need to set PreviousItemsSource
                    if (!IsAdvancedFilterVisible)
                        this.SetPreviousItemsSource();
                }
                this.CheckboxFilterControl.MaintainSelectAllCheckBox();
            }
        }

        #endregion

        #region Methods

#if UWP
        private void UpdateFilterState()
        {
            if (Part_AdvancedFilterButton != null)
            {
                if (FilteredFrom == FilteredFrom.AdvancedFilter && FilterMode == FilterMode.Both)
                    VisualStateManager.GoToState(Part_AdvancedFilterButton, "Filtered", true);
                else
                    VisualStateManager.GoToState(Part_AdvancedFilterButton, "UnFiltered", true);
            }
        }
#elif !WPF
        private void SetFilteredFromVisibility()
        {
            if (FilteredFrom == FilteredFrom.AdvancedFilter && FilterMode == FilterMode.Both)
                FilteredFromVisibility = Visibility.Visible;
            else
                FilteredFromVisibility = Visibility.Collapsed;
        }
#endif

        private void SetFilterColumnType()
        {
            if (this.AdvancedFilterType == AdvancedFilterType.TextFilter)
                this.FilterColumnType = GridResourceWrapper.TextFilters;
            else if (this.AdvancedFilterType == AdvancedFilterType.NumberFilter)
                this.FilterColumnType = GridResourceWrapper.NumberFilters;
            else
                this.FilterColumnType = GridResourceWrapper.DateFilters;
        }

        internal void InvokeFilter()
        {        
#if WPF  
            this.Column.DataGrid.RunWork(new Action(() =>
            {  
                ApplyFilters();
            }));                        
#else
            ApplyFilters();
#endif
        }

        internal bool IsInSuspend
        {
            get
            {
                if (column == null || column.DataGrid == null || column.DataGrid.GridModel == null)
                    return true;
                return column.DataGrid.GridModel.FilterSuspend;
            }
        }

#if UWP
        private async void ApplyFilters()
#else
        private void ApplyFilters()
#endif
        {
            this.Column.DataGrid.GridModel.FilterSuspend = true;
            this.Column.FilterRowText = null;
            HasSearchedItems = CheckboxFilterControl.searchedItems.Any();
            var source = (CheckboxFilterControl.ItemsSource as IEnumerable<FilterElement>);
            bool CheckboxFilter = !IsAdvancedFilterVisible;

            if (CheckboxFilter)
            {
#if UWP
            this.Column.DataGrid.SetBusyState("Busy");
            await ThreadPool.RunAsync(delegate(IAsyncAction operation)
            {
#endif
                checkedItemsCount = source.Count(x => x.IsSelected);
                unCheckedItemsCount = this.CheckboxFilterControl.FilterListBoxItem.Count() - checkedItemsCount;
#if UWP
            }, WorkItemPriority.Normal);
#endif
            }
            this.RaiseOkButtonClick(this.CheckboxFilterControl.FilterListBoxItem.Where(x => x.IsSelected), this.CheckboxFilterControl.FilterListBoxItem.Where(x => !x.IsSelected));
#if UWP
            if (!this.AdvancedFilterControl.CanGenerateUniqueItems)
            {
                var filter1 = AdvancedFilterControl.GetFirstFilterValue();
                var filter2 = AdvancedFilterControl.GetSecondFilterValue();
                if (filter1 != null && TypeConverterHelper.CanConvert(typeof(DateTime), filter1.ToString()))
                {
                    DateTime dt = Convert.ToDateTime(filter1);
                    AdvancedFilterControl.FilterValue1 = dt.Date;
                    AdvancedFilterControl.DateFilterValue1 = dt.Date;
                }
                if (filter2 != null && TypeConverterHelper.CanConvert(typeof(DateTime), filter2.ToString()))
                {
                    DateTime dt = Convert.ToDateTime(filter2);
                    AdvancedFilterControl.FilterValue2 = dt.Date;
                    AdvancedFilterControl.DateFilterValue2 = dt.Date;
                }
            }
#endif

            var ea = new OkButtonClikEventArgs()
            {
                FilterType1 = AdvancedFilterControl.FilterType1,
                FilterType2 = AdvancedFilterControl.FilterType2,
                FilterValue1 = AdvancedFilterControl.GetFirstFilterValue(),
                FilterValue2 = AdvancedFilterControl.GetSecondFilterValue(),
                ColumnType = AdvancedFilterControl.gridFilterCtrl.AdvancedFilterType,
                IsCaseSensitive1 = AdvancedFilterControl.IsCaseSensitive1,
                IsCaseSensitive2 = AdvancedFilterControl.IsCaseSensitive2,
                PredicateType =(bool)AdvancedFilterControl.IsORChecked ? "OR" : "AND"
            };

#if UWP
            if (CheckboxFilter)
                await ThreadPool.RunAsync(operation => this.CreateFilterPredicates(source), WorkItemPriority.Normal);
            else
                await ThreadPool.RunAsync(operation => this.CreateAdvancedFilterPredicates(ea), WorkItemPriority.Normal);
            this.RefreshFilter();
            this.Column.DataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(CheckboxFilterControl.gridFilterCtrl.IsOpen)));
#else

            var worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
            {
                if (CheckboxFilter)
                    CreateFilterPredicates(source);
                else
                    CreateAdvancedFilterPredicates(ea);
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                this.RefreshFilter();
                this.Column.DataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering,
      new GridFilteringEventArgs(CheckboxFilterControl.gridFilterCtrl.IsOpen)
      {
          IsProgrammatic = false
      }));
            };
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
            }
#endif

            this.Column.DataGrid.GridModel.FilterSuspend = false;
#if UWP
            this.Column.DataGrid.SetBusyState("Normal");
#endif
        }

        private void SortAsceding()
        {
            if (this.Column.DataGrid.View != null)
            {
                this.Column.DataGrid.GridModel.MakeSort(Column, ListSortDirection.Ascending);
            }
            this.IsOpen = false;
        }

        private void SortDescending()
        {
            if (this.Column.DataGrid.View != null)
            {
                this.Column.DataGrid.GridModel.MakeSort(Column, ListSortDirection.Descending);
            }
            this.IsOpen = false;
        }

        /// <summary>
        /// Invokes to Add predicates in Filter Predicates  collection for CheckBox fitler
        /// </summary>
        /// <param name="source">Fitler Elements Collection</param>
#if UWP
        private async void CreateFilterPredicates(IEnumerable<FilterElement> source)
#else
        private void CreateFilterPredicates(IEnumerable<FilterElement> source)
#endif
        {
            var sourceHashset = new HashSet<object>(source);
            if (unCheckedItemsCount == 0 && !HasSearchedItems)
            {
                if (filterPredicate == null)
                    filterPredicate = new List<FilterPredicate>();
                if (filterPredicate.Count > 0)
                    filterPredicate.Clear();
            }
            else
            {
                bool useSelected = true;
                if (checkedItemsCount > unCheckedItemsCount && unCheckedItemsCount > 0)
                    useSelected = false;

                if (this.Column.ColumnFilter == ColumnFilter.DisplayText)
                {
#if UWP
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
#else
                    this.Dispatcher.Invoke((Action)(() =>
#endif
                    {
                        filterPredicate = source.Where(x => x.IsSelected == useSelected).Select(x =>
                        {
                            var filterType = useSelected ? FilterType.Equals : FilterType.NotEquals;
                            var filterValue = x.DisplayText != GridResourceWrapper.Blanks ? x.DisplayText : x.ActualValue;
                            var predicateType = useSelected ? PredicateType.Or : PredicateType.And;
                            return GetPredicate(filterValue, filterType, true, FilterBehavior.StringTyped, predicateType, Column.ColumnFilter);
                        }).ToList();
#if WPF
                    }));
#else
                    });
#endif
                }
                else
                {
                    filterPredicate = source.Where(x => x.IsSelected == useSelected).Select(x =>
                    {
                        var filterType = useSelected ? FilterType.Equals : FilterType.NotEquals;
                        var predicateType = useSelected ? PredicateType.Or : PredicateType.And;
                        return GetPredicate(x.ActualValue, filterType, true, FilterBehavior.StronglyTyped, predicateType, Column.ColumnFilter);
                    }).ToList();
                }

                ///when creating filter predicates with unchecked items, 
                ///we need to consider the items which are not in view (filtering using search)
                if (!useSelected && CheckboxFilterControl.isSourceChangedasSearchedItems)
                {
                    this.CheckboxFilterControl.FilterListBoxItem.ForEach((o) =>
                    {
                        if (!sourceHashset.Contains(o))
                        {
                            var predicate = GetPredicate(o.ActualValue, FilterType.NotEquals, true, FilterBehavior.StronglyTyped, PredicateType.And, Column.ColumnFilter);
                            filterPredicate.Add(predicate);
                        }
                    });
                }

                var flag = Column.DataGrid.View.FilterPredicates.Any(x => !x.Equals(Column) && x.FilterPredicates.Any());

                if (Column.FilterPredicates.Any() && flag && filterPredicate.Any())
                {
                    var isTypeEqual = filterPredicate.FirstOrDefault().FilterType == column.FilterPredicates.FirstOrDefault().FilterType;

                    if (isTypeEqual)
                    {
                        foreach (var fp in Column.FilterPredicates)
                        {
                            var fpElement =
                                source.FirstOrDefault(
                                    x =>
                                    (x.ActualValue != null && x.ActualValue.Equals(fp.FilterValue)) ||
                                    (x.ActualValue == fp.FilterValue));

                            if (fpElement == null)
                                continue;

                            flag = filterPredicate.Any(
                                    x => (x.FilterValue != null && x.FilterValue.Equals(fp.FilterValue)) ||
                                         (x.FilterValue == fp.FilterValue));
                            if (flag)
                                continue;

                            if ((fp.FilterType == FilterType.NotEquals && !fpElement.IsSelected) ||
                                (fp.FilterType == FilterType.Equals && fpElement.IsSelected))
                            {
                                throw new NotImplementedException("FilterPredicate creation leads to crash");
                                //filterPredicate.Add(new FilterPredicate()
                                //    {
                                //        FilterBehavior = fp.FilterBehavior,
                                //        FilterType = fp.FilterType,
                                //        FilterValue = fp.FilterValue,
                                //        PredicateType = fp.PredicateType
                                //    });
                            }
                        }
                    }
                }
            }
            if (filterPredicate != null && filterPredicate.Count > 0)
            {
                filterPredicate[0].PredicateType = PredicateType.And;
                SetFilteredFrom(FilteredFrom.CheckboxFilter);
            }
            else if(filterPredicate != null && filterPredicate.Count == 0)
            {
                //WPF-29697 - Based on FilteredFrom property, the PART_FilteredFromCheck1 visibility is changed. Hence the FilteredFrom property changed as None.
                SetFilteredFrom(FilteredFrom.None);
            }
        }

        /// <summary>
        /// Invokes to Add predicates in Filter Predicates collection for advanced filter.
        /// </summary>
        /// <param name="args">OkButtonClikEventArgs argument</param>
        private void CreateAdvancedFilterPredicates(OkButtonClikEventArgs args)
        {
            this.AdvancedFilterControl.propertyChangedfromsettingControlValues = true;

            #region Initialize properties from args

            PredicateType predicateType = FilterHelpers.GetPredicateType(args.PredicateType.ToString());
            var filterBehavior = FilterBehavior.StringTyped;
            var filterMode = Column.ColumnFilter;

            var filterType1 = FilterHelpers.GetFilterType(args.FilterType1 == null ? string.Empty : args.FilterType1.ToString());
            var filterType2 = FilterHelpers.GetFilterType(args.FilterType2 == null ? string.Empty : args.FilterType2.ToString());

            if (args.ColumnType == AdvancedFilterType.TextFilter)
                filterMode = Column.ColumnFilter;
            else if (args.ColumnType == AdvancedFilterType.NumberFilter)
            {
                filterBehavior = FilterBehavior.StronglyTyped;
                filterMode = ColumnFilter.Value;
            }

            if (filterPredicate != null)
                filterPredicate.Clear();
            else
                filterPredicate = new List<FilterPredicate>();

            #endregion

            #region filters has string.Empty values

            // Below the code has executed when Filtervalue1 has String.Empty value and FilterValue2 has value and Filter Type not as Empty Filter
            if (args.FilterValue1 != null && args.FilterValue1.Equals(string.Empty) && args.FilterType1.ToString() != GridResourceWrapper.Empty)
            {
                var predicate = GetPredicate(args.FilterValue2, filterType2, args.IsCaseSensitive2, filterBehavior, predicateType, filterMode);
                filterPredicate.Add(predicate);
                this.AdvancedFilterControl.propertyChangedfromsettingControlValues = false;
                if (filterPredicate.Count > 0)
                    SetFilteredFrom(FilteredFrom.AdvancedFilter);
                return;
            }
            // Below the code has executed when Filtervalue2 has String.Empty value , FilterValue1 has value and Filter Type not as Empty Filter
            if (args.FilterValue2 != null && args.FilterValue2.ToString() == string.Empty && args.FilterType2.ToString() != GridResourceWrapper.Empty)
            {
                var predicate = GetPredicate(args.FilterValue1, filterType1, args.IsCaseSensitive1, filterBehavior, predicateType, filterMode);
                filterPredicate.Add(predicate);
                this.AdvancedFilterControl.propertyChangedfromsettingControlValues = false;
                if (filterPredicate.Count > 0)
                    SetFilteredFrom(FilteredFrom.AdvancedFilter);
                return;
            }

            #endregion

            bool canCreateFilterpredicate = false;

            #region TextFilters
            if (args.ColumnType == AdvancedFilterType.TextFilter)
            {
                //Preicate 1
                if (args.FilterValue1 != null && args.FilterType1 != null)
                {
                    if (args.FilterValue1.Equals(GridResourceWrapper.Blanks))
                        args.FilterValue1 = null;
                    canCreateFilterpredicate = true;
                }
                else if (args.FilterValue1 == null && args.FilterType1 != null)
                {
                    if (args.FilterType1.Equals(GridResourceWrapper.Null) || args.FilterType1.Equals(GridResourceWrapper.NotNull))
                    {
                        args.FilterValue1 = null;
                        canCreateFilterpredicate = true;
                    }
                    else if (args.FilterType1.Equals(GridResourceWrapper.Empty) || args.FilterType1.Equals(GridResourceWrapper.NotEmpty))
                    {
                        args.FilterValue1 = emptyStringValue;
                        canCreateFilterpredicate = true;
                    }
                }

                if (canCreateFilterpredicate)
                {
                    var predicate = GetPredicate(args.FilterValue1, filterType1, args.IsCaseSensitive1, filterBehavior, PredicateType.And, filterMode);
                    filterPredicate.Add(predicate);
                }

                //If the filtering applied only in second option
                if (!filterPredicate.Any())
                    args.PredicateType = "And";

                //Preicate 2
                canCreateFilterpredicate = false;

                if (args.FilterValue2 != null && args.FilterType2 != null)
                {
                    if (args.FilterValue2.Equals(GridResourceWrapper.Blanks))
                        args.FilterValue2 = null;
                    canCreateFilterpredicate = true;
                }
                else if (args.FilterValue2 == null && args.FilterType2 != null)
                {
                    if (args.FilterType2.ToString() == GridResourceWrapper.Null || args.FilterType2.ToString() == GridResourceWrapper.NotNull)
                    {
                        args.FilterValue2 = null;
                        canCreateFilterpredicate = true;
                    }
                    else if (args.FilterType2.ToString() == GridResourceWrapper.Empty || args.FilterType2.ToString() == GridResourceWrapper.NotEmpty)
                    {
                        args.FilterValue2 = emptyStringValue;
                        canCreateFilterpredicate = true;
                    }
                }

                if(canCreateFilterpredicate)
                {
                    var predicate = GetPredicate(args.FilterValue2, filterType2, args.IsCaseSensitive2, filterBehavior, predicateType, filterMode);
                    filterPredicate.Add(predicate);
                }
            }
                #endregion

            #region NumberFilters

            else if (args.ColumnType == AdvancedFilterType.NumberFilter)
            {
                canCreateFilterpredicate = false;
                //Predicate 1
                if (args.FilterValue1 != null && args.FilterType1 != null)
                {
                    if (args.FilterValue1.Equals(GridResourceWrapper.Blanks))
                        args.FilterValue1 = null;
                    canCreateFilterpredicate = true;
                }
                else if (args.FilterValue1 == null && args.FilterType1 != null)
                {
                    if (args.FilterType1.ToString() == GridResourceWrapper.Null || args.FilterType1.ToString() == GridResourceWrapper.NotNull)
                    {
                        args.FilterValue1 = null;
                        canCreateFilterpredicate = true;
                    }
                }

                if (canCreateFilterpredicate)
                {
                    var predicate = GetPredicate(args.FilterValue1, filterType1, true, FilterBehavior.StronglyTyped, PredicateType.And, ColumnFilter.Value);
                    filterPredicate.Add(predicate);
                }
                //If the filtering applied only in second option
                if (!filterPredicate.Any())
                    args.PredicateType = "And";

                canCreateFilterpredicate = false;

                //Predicate 2
                if (args.FilterValue2 != null && args.FilterType2 != null)
                {
                    if (args.FilterValue2.Equals(GridResourceWrapper.Blanks))
                        args.FilterValue2 = null;
                    canCreateFilterpredicate = true;
                }
                else if (args.FilterValue2 == null && args.FilterType2 != null)
                {
                    if (args.FilterType2.Equals(GridResourceWrapper.Null) || args.FilterType2.Equals(GridResourceWrapper.NotNull))
                    {
                        args.FilterValue2 = null;
                        canCreateFilterpredicate = true;
                    }
                }

                if (canCreateFilterpredicate)
                {
                    var predicate = GetPredicate(args.FilterValue2, filterType2, true, FilterBehavior.StronglyTyped, predicateType, ColumnFilter.Value);
                    filterPredicate.Add(predicate);
                }
            }

            #endregion

            #region DateFilter

            else if (args.ColumnType == AdvancedFilterType.DateFilter)
            {
                if (args.FilterValue1 != null && args.FilterType1 != null)
                {
                    if (args.FilterValue1.Equals(GridResourceWrapper.Blanks))
                        args.FilterValue1 = null;

                    if(Column.ColumnFilter==ColumnFilter.DisplayText && (filterType1==FilterType.Equals ||filterType1 == FilterType.NotEquals))
                    {
#if WPF
                        this.Dispatcher.Invoke((Action)(() =>  {
#endif
                        var filterValue = !canGenerateUniqueItems ? GetFormattedString(args.FilterValue1) : (AdvancedFilterControl.FilterSelectedItem1 != null &&
                                                    ((AdvancedFilterControl.FilterSelectedItem1 is FilterElement))) ? (AdvancedFilterControl.FilterSelectedItem1 as FilterElement).DisplayText : args.FilterValue1;
                        var predicate = GetPredicate(filterValue, filterType1, true, FilterBehavior.StringTyped, PredicateType.And, Column.ColumnFilter);
                        filterPredicate.Add(predicate);
#if WPF
                            }));
#endif
                    }
                    else
                    {
                        var predicate = GetPredicate(args.FilterValue1, filterType1, true, FilterBehavior.StronglyTyped, PredicateType.And, ColumnFilter.Value);
                        filterPredicate.Add(predicate);
                    }
                }
                else if (args.FilterValue1 == null && args.FilterType1 != null)
                {
                    if (args.FilterType1.Equals(GridResourceWrapper.Null) || args.FilterType1.Equals(GridResourceWrapper.NotNull))
                    {
                        args.FilterValue1 = null;
                        if (Column.ColumnFilter == ColumnFilter.DisplayText && (filterType1 == FilterType.Equals || filterType1 == FilterType.NotEquals))
                        {
#if WPF
                            this.Dispatcher.Invoke((Action)(() => {
#endif
                            var filterValue = !this.AdvancedFilterControl.CanGenerateUniqueItems ? GetFormattedString(args.FilterValue1) : (AdvancedFilterControl.FilterSelectedItem1 != null &&
                                                        ((AdvancedFilterControl.FilterSelectedItem1 is FilterElement))) ? (AdvancedFilterControl.FilterSelectedItem1 as FilterElement).DisplayText : args.FilterValue1;
                            var predicate = GetPredicate(filterValue, filterType1, true, FilterBehavior.StringTyped, PredicateType.And, Column.ColumnFilter);
                            filterPredicate.Add(predicate);
#if WPF
                                }));
#endif
                        }
                        else
                        {
                            var predicate = GetPredicate(args.FilterValue1, filterType1, true, FilterBehavior.StronglyTyped, PredicateType.And, ColumnFilter.Value);
                            filterPredicate.Add(predicate);
                        }
                    }
                }
                //If the filtering applied only in second option
                if (!filterPredicate.Any())
                    args.PredicateType = "And";
                //Predicate 2
                if (args.FilterValue2 != null && args.FilterType2 != null)
                {
                    if (args.FilterValue2.Equals(GridResourceWrapper.Blanks))
                        args.FilterValue2 = null;
                    if (Column.ColumnFilter == ColumnFilter.DisplayText && (filterType2 == FilterType.Equals || filterType2 == FilterType.NotEquals))
                    {
#if WPF
                        this.Dispatcher.Invoke((Action)(() =>  {
#endif
                        var filterValue = !canGenerateUniqueItems ? GetFormattedString(args.FilterValue2) : (AdvancedFilterControl.FilterSelectedItem2 != null &&
                                                    ((AdvancedFilterControl.FilterSelectedItem2 is FilterElement))) ? (AdvancedFilterControl.FilterSelectedItem2 as FilterElement).DisplayText : args.FilterValue2;
                        var predicate = GetPredicate(filterValue, filterType2, true, FilterBehavior.StringTyped, predicateType, Column.ColumnFilter);
                        filterPredicate.Add(predicate);
#if WPF
                            }));
#endif
                    }
                    else
                    {
                        var predicate = GetPredicate(args.FilterValue2, filterType2, true, FilterBehavior.StronglyTyped, predicateType, ColumnFilter.Value);
                        filterPredicate.Add(predicate);
                    }
                }
                else if (args.FilterValue2 == null && args.FilterType2 != null)
                {
                    if (args.FilterType2.Equals(GridResourceWrapper.Null) || args.FilterType2.Equals(GridResourceWrapper.NotNull))
                    {
                        args.FilterValue2 = null;
                        if (Column.ColumnFilter == ColumnFilter.DisplayText && (filterType2 == FilterType.Equals || filterType2 == FilterType.NotEquals))
                        {
#if WPF
                            this.Dispatcher.Invoke((Action)(() => {
#endif
                            var filterValue = !this.canGenerateUniqueItems ? GetFormattedString(args.FilterValue2) : (AdvancedFilterControl.FilterSelectedItem2 != null &&
                                                        ((AdvancedFilterControl.FilterSelectedItem2 is FilterElement))) ? (AdvancedFilterControl.FilterSelectedItem2 as FilterElement).DisplayText : args.FilterValue2;
                            var predicate = GetPredicate(filterValue, filterType2, true, FilterBehavior.StringTyped, predicateType, Column.ColumnFilter);
                            filterPredicate.Add(predicate);
#if WPF
                            }));    
#endif
                        }
                        else
                        {
                            var predicate = GetPredicate(args.FilterValue2, filterType2, true, FilterBehavior.StronglyTyped, predicateType, ColumnFilter.Value);
                            filterPredicate.Add(predicate);
                        }                      
                    }
                }

            }

            #endregion

            this.AdvancedFilterControl.propertyChangedfromsettingControlValues = false;
            if (filterPredicate.Count > 0)
                SetFilteredFrom(FilteredFrom.AdvancedFilter);
        }

        /// <summary>
        /// Invokes to Get Filter Predicate
        /// </summary>
        /// <param name="filterValue">filter value</param>
        /// <param name="filterType">Filter Type</param>
        /// <param name="isCaseSensitive">IsCaseSensitive</param>
        /// <param name="behavior">Filter Behavior</param>
        /// <param name="predicateType">Predicate Type</param>
        /// <param name="filterMode">Filter Mode</param>
        /// <returns>FilterPredicate</returns>
        private FilterPredicate GetPredicate(object filterValue, FilterType filterType, bool isCaseSensitive, FilterBehavior behavior, PredicateType predicateType, ColumnFilter filterMode)
        {
            return new FilterPredicate()
            {
                FilterValue = filterValue,
                FilterType = filterType,
                IsCaseSensitive = isCaseSensitive,
                FilterBehavior = behavior,
                PredicateType = predicateType,
                FilterMode = filterMode
            };
        }

        /// <summary>
        /// Invokes to Set FitleredFrom field. 
        /// </summary>
        /// <param name="filteredFrom">FilteredFrom</param>
#if UWP
        private async void SetFilteredFrom(FilteredFrom filteredFrom)
#else
        private void SetFilteredFrom(FilteredFrom filteredFrom)
#endif
        {
#if UWP
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FilteredFrom = filteredFrom;
            });
#else
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                FilteredFrom = filteredFrom;
            }));
#endif
        }

        private void RefreshFilter()
        {
            this.Column.DataGrid.GridModel.CommitCurrentRow(this.Column.DataGrid);
            this.Column.DataGrid.GridModel.FilterColumn(this.Column, filterPredicate);
            // #136660 code changes suggested by Akuna Capital
            var headerCell = Column.DataGrid.GetHeaderCell(Column);

            if (headerCell != null)
                headerCell.ApplyFilterToggleButtonVisualState();

            this.column.DataGrid.UpdateFreezePaneRows();
            this.MaintainClearFilterButtonEnable();
        }

        private void MaintainDataGridAPIChanges()
        {
            if (this.Column == null || this.Column.DataGrid == null)
                return;

            var dataGrid = this.Column.DataGrid;
            this.ImmediateUpdateColumnFilter = this.Column.ImmediateUpdateColumnFilter;
            this.AllowBlankFilters = this.Column.AllowBlankFilters;
            if (Column.ReadLocalValue(GridColumn.FilterPopupStyleProperty)!= DependencyProperty.UnsetValue)            
                this.Style = Column.FilterPopupStyle;
            else if (dataGrid.ReadLocalValue(SfDataGrid.FilterPopupStyleProperty) != DependencyProperty.UnsetValue)
                this.Style = dataGrid.FilterPopupStyle;
            else
                this.ClearValue(GridFilterControl.StyleProperty);

            if (Column.ReadLocalValue(GridColumn.FilterPopupTemplateProperty) != DependencyProperty.UnsetValue)            
                this.ContentTemplate = Column.FilterPopupTemplate;
            else if (dataGrid.ReadLocalValue(SfDataGrid.FilterPopupTemplateProperty) != DependencyProperty.UnsetValue)
                this.ContentTemplate = dataGrid.FilterPopupTemplate;
            else
                this.ClearValue(GridFilterControl.ContentTemplateProperty);
        }

        private void MaintainSortIndication()
        {
            if (this.PART_SortAscendingButton == null || this.PART_SortDescendingButton == null)
                return;

            if (this.CanAllowSort())
            {
                this.PART_SortAscendingButton.IsEnabled = true;
                this.PART_SortDescendingButton.IsEnabled = true;
            }
            else
            {
                this.PART_SortAscendingButton.IsEnabled = false;
                this.PART_SortDescendingButton.IsEnabled = false;
            }
            if (this.Column.DataGrid.View != null && this.Column.DataGrid.View.SortDescriptions != null && this.Column.DataGrid.View.SortDescriptions.Count > 0
                    && this.Column.DataGrid.View.SortDescriptions.Any(x => x.PropertyName.Equals(this.Column.MappingName)))
            {
                var sortColumn =
                    this.Column.DataGrid.View.SortDescriptions.FirstOrDefault(
                        x => x.PropertyName.Equals(this.Column.MappingName));
                if (sortColumn != null && sortColumn.Direction == ListSortDirection.Ascending)
                {
                    this.PART_SortAscendingButton.IsSorted = true;
                    this.PART_SortDescendingButton.IsSorted = false;
                }
                else if (sortColumn != null && sortColumn.Direction == ListSortDirection.Descending)
                {
                    this.PART_SortAscendingButton.IsSorted = false;
                    this.PART_SortDescendingButton.IsSorted = true;
                }
                else
                {
                    this.PART_SortAscendingButton.IsSorted = false;
                    this.PART_SortDescendingButton.IsSorted = false;
                }
            }
            else
            {
                this.PART_SortAscendingButton.IsSorted = false;
                this.PART_SortDescendingButton.IsSorted = false;
            }
        }

        private void MaintainClearFilterButtonEnable()
        {
            if (this.Part_ClearFilterButton != null && Column.FilterPredicates.Count == 0)
                this.Part_ClearFilterButton.IsEnabled = false;
            else if (this.Part_ClearFilterButton != null && Column.FilterPredicates.Count > 0)
                this.Part_ClearFilterButton.IsEnabled = true;
        }

        private bool CanAllowSort()
        {
            bool canSetAllowSort = false;
            var valueColumn = this.Column.ReadLocalValue(GridColumn.AllowSortingProperty);
            if (valueColumn != DependencyProperty.UnsetValue)
                canSetAllowSort = this.Column.AllowSorting;
            if ((valueColumn == DependencyProperty.UnsetValue) && this.Column.DataGrid.AllowSorting)
                canSetAllowSort = true;
            return canSetAllowSort;
        }

#if UWP
        private async void InitializeGridFilterPane()
#else
        private void InitializeGridFilterPane()
#endif
        {
            if (this.FilterPopUp != null)
            {
                if (!IsOpen)
                {
#if WPF
                    // reseting the filterpopup placement on closing.
                    this.FilterPopUp.Placement = PlacementMode.Bottom;
#endif
                    this.distinctCollection = null;
                    return;
                }
#if WPF
                // check the filterpopup intersects with screen or not
                var screenRect = new Rect(0, 0, System.Windows.SystemParameters.PrimaryScreenWidth, System.Windows.SystemParameters.PrimaryScreenHeight);

                // Need to convert the popup position relative to screen
                Point point = this.FilterPopUp.PointToScreen(new Point(0, 0));
                var popupRect = new Rect(point.X, point.Y, this.FilterPopUp.ActualWidth, this.FilterPopUp.ActualHeight);

                var currPopupWidth = popupRect.Width = 300;
                var currPopupHeight = popupRect.Height = 450;
                popupRect.Intersect(screenRect);
                // if intersects changing the placement mode for the filterpopup
                if (currPopupHeight > popupRect.Height)
                {
                    this.FilterPopUp.Placement = PlacementMode.Left;
                }
#endif
            }

            this.MaintainClearFilterButtonEnable();
            this.ResetVisualStates();
            this.MaintainDataGridAPIChanges();
            this.MaintainSortIndication();
            this.ResetPopupHeight();
            if (Column.FilterPredicates == null || !Column.FilterPredicates.Any())
                FilteredFrom = FilteredFrom.None;

            if (this.CheckboxFilterControl != null)
            {
                this.CheckboxFilterControl.SearchTextBlockVisibility = Visibility.Visible;
                this.CheckboxFilterControl.MaintainAPIChanges();
                this.CheckboxFilterControl.IsItemSourceLoaded = false;
#if UWP
                //WRT-5193 BusyIndicator not displayed in filter popup when loading more items. Here HasItemsSource set as true to enable
                //busyindicator in filter popup, because dispatcher delayed to set this flag. Based on this, we have enabled the busyindicator. If we bind empty collection, we have set this flag as false.
                CheckboxFilterControl.HasItemsSource = true;
#endif
                this.CheckboxFilterControl.isSourceChangedasSearchedItems = false;
                if (this.CheckboxFilterControl.SearchTextBox != null && this.excelFilterView != null)
                {
                    this.CheckboxFilterControl.SearchTextBox.ClearValue(TextBox.TextProperty);
                    this.CheckboxFilterControl.searchedItems = new List<FilterElement>();
                    HasSearchedItems = false;
                }
                if (this.CheckboxFilterControl.PartScrollViewer != null)
#if !WPF
                    this.CheckboxFilterControl.PartScrollViewer.ChangeView(null, 0, null, true);
#else
                    this.CheckboxFilterControl.PartScrollViewer.ScrollToVerticalOffset(0);
#endif
                this.allowBlankFilters = this.AllowBlankFilters;
                var sourceFromEvent = this.RaisePopupOpened();
                if (sourceFromEvent != null && sourceFromEvent.Any())
                {
                    this.distinctCollection = sourceFromEvent.ToList();
                    this.CheckboxFilterControl.FilterListBoxItem = this.distinctCollection;
                    this.CheckboxFilterControl.ItemsSource = this.distinctCollection;
                }
                else
                {
                    var e = new GridFilterItemsPopulatingEventArgs(this.distinctCollection, this.Column, this, this.Column.DataGrid);
                    if (!this.Column.DataGrid.RaiseFilterListItemsPopulating(e))
                    {
                        if (e.ItemsSource != null && e.ItemsSource.Any())
                        {
                            this.distinctCollection = (e.ItemsSource as IEnumerable<FilterElement>).ToList();
                            this.CheckboxFilterControl.HasItemsSource = true;
                            this.SetItemSource();
                            return;
                        }
#if UWP
                        if (this.CheckboxFilterControl != null)
                        {
                            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                this.GenerateItemSource();
                                if (this.CheckboxFilterControl.HasItemsSource)
                                    this.SetItemSource();
                            });
                        }
#else
                        if (this.CheckboxFilterControl != null && !this.bgWorkertoPopulate.IsBusy)
                            this.bgWorkertoPopulate.RunWorkerAsync();
#endif
                    }
                    else
                    {
                        this.distinctCollection = (e.ItemsSource as IEnumerable<FilterElement>).ToList();
                        this.distinctCollection.ForEach(item =>
                        {
                            if (item.FormattedString == null)
                                item.FormattedString = this.GetFormattedString;
                        });
                        this.CheckboxFilterControl.HasItemsSource = true;
                        this.SetItemSource();
                    }
                }

#if UWP
                this.Focus(FocusState.Programmatic);
#endif
            }

            if (this.AdvancedFilterControl != null)
            {
                this.AdvancedFilterControl.MaintainAPIChanges();
#if WPF
                this.AdvancedFilterControl.CasingButtonVisibility = this.Column.DataGrid.IsLegacyDataTable;
#endif
                //WPF-20169 Issues in Numeric column and ImageColumn properties 
                //Below code added the Filter pane based on FilterBehaviour at runtime
                //this.AdvancedFilterType= this.GetAdvancedFilterType();
                this.AdvancedFilterControl.GenerateFilterTypeComboItems();
                this.AdvancedFilterControl.propertyChangedfromsettingControlValues = true;
                this.AdvancedFilterControl.ComboItemsSource = this.distinctCollection == null ? null : new ObservableCollection<FilterElement>(this.distinctCollection);
                this.AdvancedFilterControl.propertyChangedfromsettingControlValues = false;
                if (this.AdvancedFilterControl.ComboItemsSource != null)
                {
#if WPF
                    var nullfilter = this.AdvancedFilterControl.ComboItemsSource.FirstOrDefault(filter => filter.ActualValue == null || filter.ActualValue == DBNull.Value);
#else
                    var nullfilter = this.AdvancedFilterControl.ComboItemsSource.FirstOrDefault(filter => filter.ActualValue == null);
#endif
                    this.AdvancedFilterControl.ComboItemsSource.Remove(nullfilter);
                    var emptyfilter = this.AdvancedFilterControl.ComboItemsSource.FirstOrDefault(filter => filter.ActualValue != null && filter.ActualValue.Equals(string.Empty));
                    this.AdvancedFilterControl.ComboItemsSource.Remove(emptyfilter);
                }
                if (this.FilteredFrom == FilteredFrom.AdvancedFilter)
                {
                    if (this.FilterMode == FilterMode.Both)
                        IsAdvancedFilterVisible = true;
                    if (Column.FilterPredicates != null && Column.FilterPredicates.Any())
                        this.AdvancedFilterControl.SetAdvancedFilterControlValues(Column.FilterPredicates);
                    else
                        this.AdvancedFilterControl.ResetAdvancedFilterControlValues();
                }
                else
                {
                    if (this.FilterMode == FilterMode.Both)
                        IsAdvancedFilterVisible = false;
                    this.AdvancedFilterControl.ResetAdvancedFilterControlValues();
                }
                this.ResetVisbleFilteringControl();
#if !WPF
                // WRT-4455 - Need to reset DatePickerVisibility here based on FilterColumnType
                if (AdvancedFilterControl.gridFilterCtrl != null && AdvancedFilterControl.gridFilterCtrl.FilterColumnType == GridResourceWrapper.DateFilters)
                {
                    AdvancedFilterControl.DatePickerVisibility = !AdvancedFilterControl.CanGenerateUniqueItems ? false : true;
                }
                else
                    // For text filters and number filters true is set(since reverse visibility converter is used)
                    AdvancedFilterControl.DatePickerVisibility = true;
#endif
            }
        }
#if !WPF

#if UWP
        internal void SetPopupPosition(Rect toggleRect, Point windowPoint, double togglePadding, Thickness headerCellPadding)
#else
        internal void SetPopupPosition(Rect toggleRect, Point windowPoint, double togglePadding)
#endif
        {
            if (FilterPopUpBorder == null)
            {
                localToggleRect = toggleRect;
                localWindowPoint = windowPoint;
                localTogglepadding = togglePadding;
#if UWP
                localHeaderCellPading = headerCellPadding;
#endif
                return;
            }
           
#if UWP
            var screenWidth = Window.Current.Bounds.Width;
            var screenHeight = Window.Current.Bounds.Height;

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                Rect VisibleBound;
                //UWP-1815 - Setting FilterPopup Height and Width based on VisibleBounds Height and Width                
                if (ApplicationView.GetForCurrentView() != null)
                {
                    VisibleBound = ApplicationView.GetForCurrentView().VisibleBounds;
                    this.MinWidth = VisibleBound.Width;
                    this.MinHeight = VisibleBound.Height;
                }  
                else
                {
                    var actaulheight = FilterPopUpBorder.ActualHeight > 0 ? FilterPopUpBorder.ActualHeight : this.MinHeight;
                    var actaulwidth = FilterPopUpBorder.ActualWidth > 0 ? FilterPopUpBorder.ActualWidth : this.MinWidth;

                    this.HorizontalOffset = (screenWidth - actaulwidth) - (windowPoint.X - (toggleRect.X - localHeaderCellPading.Left));
                    this.VerticalOffset = (screenHeight - actaulheight) - (windowPoint.Y - (toggleRect.Y - localHeaderCellPading.Top));
                    return;
                }             

                //UWP-1815 - Skip the NavigationBar Size while displaying the FilterPopup.

                var actualheight = FilterPopUpBorder.ActualHeight > 0 ? FilterPopUpBorder.ActualHeight : VisibleBound.Height;
                var actualwidth = FilterPopUpBorder.ActualWidth > 0 ? FilterPopUpBorder.ActualWidth : VisibleBound.Width;
                
                var bottom = VisibleBound.Bottom;
                var navigationBarSize = screenHeight - bottom;
                //UWP-4337 FilterPopup HorizontalOffset changed based on FlowDirection of SfDataGrid 

                if (this.Column.DataGrid.FlowDirection == FlowDirection.LeftToRight)
                    this.HorizontalOffset = (screenWidth - actualwidth) - (windowPoint.X - (toggleRect.X - localHeaderCellPading.Left));
                else
                    this.HorizontalOffset = (screenWidth - actualwidth) - ((actualwidth - windowPoint.X) - (toggleRect.X - localHeaderCellPading.Left));
                this.VerticalOffset = (screenHeight == bottom) ? (screenHeight - actualheight) - (windowPoint.Y - (toggleRect.Y - localHeaderCellPading.Top)) 
                                        : (screenHeight - actualheight) - (windowPoint.Y - (toggleRect.Y - navigationBarSize - localHeaderCellPading.Top));
                return;
            }
#endif
            var actualHeight = FilterPopUpBorder.ActualHeight > 0 ? FilterPopUpBorder.ActualHeight : FilterPopUpBorder.MinHeight;
            var actualWidth = FilterPopUpBorder.ActualWidth > 0 ? FilterPopUpBorder.ActualWidth : FilterPopUpBorder.MinWidth;        

            //UWP-4337 FilterPopup HorizontalOffset changed based on FlowDirection of SfDataGrid             
            if ((windowPoint.X + actualWidth > screenWidth && this.Column.DataGrid.FlowDirection == FlowDirection.LeftToRight)
                ||(windowPoint.X + actualWidth < screenWidth && this.Column.DataGrid.FlowDirection == FlowDirection.RightToLeft))
                this.HorizontalOffset = (toggleRect.X + togglePadding) - actualWidth;
            else
                this.HorizontalOffset = toggleRect.X - togglePadding;
#if UWP
            //WRT-4115 Filter popup shows incorrect position while displaying the grid at the end of the screen 
            if (windowPoint.Y + actualHeight > screenHeight - (toggleRect.Height - toggleRect.Y))
            {
                if (windowPoint.Y > actualHeight)
                    this.VerticalOffset = -actualHeight;
                else
                {
                    this.VerticalOffset = -actualHeight + (actualHeight - windowPoint.Y) + toggleRect.Y ;
                    //UWP-4337 FilterPopup HorizontalOffset changed based on FlowDirection of SfDataGrid 
                    if ((windowPoint.X + actualWidth > screenWidth && this.Column.DataGrid.FlowDirection == FlowDirection.LeftToRight)
                        || (windowPoint.X + actualWidth < screenWidth && this.Column.DataGrid.FlowDirection == FlowDirection.RightToLeft))
                        this.HorizontalOffset = (toggleRect.X - togglePadding) - actualWidth;
                    else
                        this.HorizontalOffset = toggleRect.X + (toggleRect.Width - togglePadding);
                }
                if (this.FilterPopUp.ChildTransitions.Count > 0 && (this.FilterPopUp.ChildTransitions[0] is PaneThemeTransition) && (this.FilterPopUp.ChildTransitions[0] as PaneThemeTransition).Edge == EdgeTransitionLocation.Top)
                    (this.FilterPopUp.ChildTransitions[0] as PaneThemeTransition).Edge = EdgeTransitionLocation.Bottom;

            }
            else
            {
#endif
                this.VerticalOffset = toggleRect.Height;
#if UWP
                if (this.FilterPopUp.ChildTransitions.Count > 0 && (this.FilterPopUp.ChildTransitions[0] is PaneThemeTransition) && (this.FilterPopUp.ChildTransitions[0] as PaneThemeTransition).Edge == EdgeTransitionLocation.Top)
                    (this.FilterPopUp.ChildTransitions[0] as PaneThemeTransition).Edge = EdgeTransitionLocation.Top;
            }
#endif

        }

#endif
        private void ResetVisualStates()
        {
            if (PART_SortAscendingButton != null && PART_SortAscendingButton.IsEnabled)
                VisualStateManager.GoToState(PART_SortAscendingButton, "Normal", false);
            if (PART_SortDescendingButton != null && PART_SortDescendingButton.IsEnabled)
                VisualStateManager.GoToState(PART_SortDescendingButton, "Normal", false);
            if (Part_ClearFilterButton != null && Part_ClearFilterButton.IsEnabled)
                VisualStateManager.GoToState(Part_ClearFilterButton, "Normal", false);
            if (OkButton != null && OkButton.IsEnabled)
                VisualStateManager.GoToState(OkButton, "Focused", false);
            if (CancelButton != null)
                VisualStateManager.GoToState(CancelButton, "Normal", false);
        }

        private void ResetPopupHeight()
        {
            minHeight = this.MinHeight;
            minWidth = this.MinWidth;
            this.FilterPopupWidth = filterPopupWidth;
            this.FilterPopupHeight = filterPopupHeight;
        }

        private void ResetVisbleFilteringControl()
        {
            if (IsAdvancedFilterVisible)
            {
                VisualStateManager.GoToState(Part_AdvancedFilterButton, "Collapsed", true);
                VisualStateManager.GoToState(this, "AdvancedFilter", true);
                //Use case of this condition is ok button is enabled when filter has values. 
                //so filter ok button is set to enable if the filter type has NUll,NOTNULL,EMPTY,NOTEMPTY types 
                //or any one of the filter combo box has values.WPF-17915
                if ((AdvancedFilterControl.IsNullOrEmptyFilterType(AdvancedFilterControl.FilterType1)) ||
                   (AdvancedFilterControl.IsNullOrEmptyFilterType(AdvancedFilterControl.FilterType2)) || (AdvancedFilterControl.IsFilterHasvalues()) && OkButton != null)
                    OkButton.IsEnabled = true;
                else if (OkButton != null)
                    OkButton.IsEnabled = false;
            }
            else
            {
                VisualStateManager.GoToState(Part_AdvancedFilterButton, "Expanded", true);
                VisualStateManager.GoToState(this, "CheckboxFilter", true);
                if (FilteredFrom == FilteredFrom.AdvancedFilter)
                {
                    if (CheckboxFilterControl.SelectAllCheckBox != null)
                        CheckboxFilterControl.SelectAllCheckBox.IsChecked = false;
                    OkButton.IsEnabled = false;
                    // if ImmediateUpdateColumnFilter is used, need to reset previousItemSource
                    if (this.ImmediateUpdateColumnFilter)
                        CheckboxFilterControl.previousItemSource = null;           
                }
                else
                {
                    CheckboxFilterControl.MaintainSelectAllCheckBox();
                }
                if (CheckboxFilterControl.ItemsSource == null)
                    OkButton.IsEnabled = false;
            }
        }

        private void GenerateItemSource()
        {
            IEnumerable<object> items = null;
            bool hasItemsSource = true;

            if (this.Column.DataGrid.isIQueryable)
            {
                try
                {
                    hasItemsSource = (this.Column.DataGrid.View as QueryableCollectionView).ViewSource.Select(colName).ToList<object>().Any();
                }
                catch
                {
                    hasItemsSource = false;
                }
            }
#if WPF
            else if (this.Column.DataGrid.View is DataTableCollectionView)
            {
                // WPF-33492 - Populate ItemsSource basedon SourceCollection in CheckBoxFilterControl
                hasItemsSource = this.Column.DataGrid.View.SourceCollection != null && ((DataView)this.Column.DataGrid.View.SourceCollection).Table.Rows.Count > 0;
                if (hasItemsSource)
                    items = this.Column.DataGrid.View.Records.GetSource();
            }                           
#endif
            else
            {
                hasItemsSource = this.Column.DataGrid.View.SourceCollection != null && this.Column.DataGrid.View.SourceCollection.Cast<object>().Any();

                if (this.Column.DataGrid.View is Syncfusion.Data.PagedCollectionView)
                {
                    if (hasItemsSource)
                        items = (this.Column.DataGrid.View as PagedCollectionView).GetInternalList().Cast<object>();
                }
                else if (this.Column.DataGrid.View is VirtualizingCollectionView)
                {
                    if (hasItemsSource)
                        items = (this.Column.DataGrid.View as VirtualizingCollectionView).GetInternalSource().Cast<object>();
                }
                else
                {
                    if (hasItemsSource)
                        items = this.Column.DataGrid.View.Records.GetSource();
                }                                
            }
#if WPF
            this.Dispatcher.Invoke((Action)(() =>
            {
#endif
                CheckboxFilterControl.HasItemsSource = hasItemsSource;
#if WPF
            }));
#endif
            if (!hasItemsSource)
                return;

            var displayTextFilter = Column.ColumnFilter == ColumnFilter.DisplayText;
            IEnumerable<RecordValuePair> viewRecords = null;
            if (column.isDisplayMultiBinding && displayTextFilter && !column.useBindingValue)
                throw new InvalidOperationException("Set UseBindingValue to true to reflect the property value as GridColumn.DisplayBinding is MultiBinding");
#if WPF
            this.Dispatcher.Invoke((Action)(() =>
            {
#endif
                if (!(this.Column.DataGrid.isIQueryable))
                {
                    viewRecords = items.Select((x) =>
                    {
                        if (this.Column.IsUnbound)
                        {
                            var value = this.Column.DataGrid.GetUnBoundCellValue(this.Column, x);
                            return new RecordValuePair(value, x);
                        }
                        else if (displayTextFilter && Column.isDisplayMultiBinding)
                            return new RecordValuePair(this.provider.GetFormattedValue(x, Column.MappingName), x);
                        else
                            return new RecordValuePair(this.provider.GetValue(x, Column.MappingName), x);

                    }).Distinct(new RecordValueEqualityComparer<RecordValuePair>());
                }
#if WPF
            }));
#endif
           
#if WPF
            this.Dispatcher.Invoke((Action)(() =>
                {
#endif
                    if (this.Column.DataGrid.isIQueryable)
                    {
                        var query = (this.Column.DataGrid.View as QueryableCollectionView).ViewSource;
                        IEnumerable<object> viewObjects = query.Select(this.Column.MappingName).ToList<object>().Distinct<object>();
                        this.distinctCollection = viewObjects.Select(item =>
                                                                    new FilterElement
                                                                    {
                                                                        IsSelected = this.FilteredFrom != FilteredFrom.AdvancedFilter,
                                                                        ActualValue = item,
                                                                        FormattedString = this.GetFormattedString
                                                                    }).ToList();

                    }
                    else
                    {
                        this.distinctCollection = viewRecords.Select(item =>
                                                                    new FilterElement
                                                                    {
                                                                        IsSelected = this.FilteredFrom != FilteredFrom.AdvancedFilter,
                                                                        ActualValue = item.Value,
                                                                        FormattedString = this.GetFormattedString,
#if WPF
                                                                        Record = Column.isDisplayMultiBinding || (displayTextFilter && !excelFilterView.IsLegacyDataTable) ? item.Record : null
#else
                                            Record = Column.isDisplayMultiBinding || displayTextFilter ? item.Record : null
#endif
                                                                    }).ToList();
                    }
#if WPF
                }));
#endif


            if (Column.FilterPredicates.Count > 0)
            {
#if WPF
                this.Dispatcher.Invoke((Action)(() =>
                    {
                        if (!excelFilterView.IsLegacyDataTable) //!(excelFilterView is GridDataTableCollectionViewWrapper))
                        {
#endif
                            var view = excelFilterView as IFilterExt;
                            System.Linq.Expressions.Expression columnPredicate = null;
                            ParameterExpression columnParamExpression = null;
                            paramExpression = null;
                            predicate = null;

                            var filteredRecords = this.Column.DataGrid.View.SourceCollection.AsQueryable();

                            columnPredicate = view.GetPredicateExpression(filteredRecords,
                                                                                     out columnParamExpression,
                                                                                     Column.MappingName, true);

                            filteredRecords = filteredRecords.Where(columnParamExpression,
                                                                    System.Linq.Expressions.Expression.Not(
                                                                        columnPredicate));
                            predicate = view.GetPredicateExpression(filteredRecords,
                                                                                     out paramExpression,
                                                                                     Column.MappingName, false);
                            if (predicate != null)
                                filteredRecords = filteredRecords.Where(paramExpression, predicate);

                            IEnumerable<object> items_filtered;
                            if (!(filteredRecords is EnumerableQuery))
                                items_filtered = ((IEnumerable<object>)filteredRecords);
                            else
                                items_filtered = filteredRecords.Cast<object>();

                            IEnumerable<RecordValuePair> distinctRecords = null;
                            if (this.Column.DataGrid.isIQueryable)
                            {
                                IEnumerable<object> filteredObjects = filteredRecords.Select(this.Column.MappingName).ToList<object>();
                                filteredObjects.Distinct().ForEach((o) => this.distinctCollection.Add(new FilterElement()
                                {
                                    ActualValue = o,
                                    FormattedString = this.GetFormattedString
                                }));
                            }
                            else
                            {
                                distinctRecords = items_filtered.Select((x) =>
                                {
                                    if (this.Column.IsUnbound)
                                    {
                                        var value = this.Column.DataGrid.GetUnBoundCellValue(this.Column, x);
                                        return new RecordValuePair(value, x);
                                    }
                                    else if (displayTextFilter && Column.isDisplayMultiBinding)
                                        return new RecordValuePair(this.provider.GetFormattedValue(x, Column.MappingName), x);
                                    else
                                        return new RecordValuePair(this.provider.GetValue(x, Column.MappingName), x);
                                }).Distinct(new RecordValueEqualityComparer<RecordValuePair>());

                                distinctRecords.ForEach((o) => this.distinctCollection.Add(new FilterElement()
                                {
                                    ActualValue = o.Value,
                                    Record = Column.isDisplayMultiBinding || displayTextFilter ? o.Record : null,
                                    FormattedString = this.GetFormattedString
                                }));
                            }
#if WPF
                        }
                        else
                        {
                            PropertyDescriptorCollection clonedItemsProperties = null;
                            var view = excelFilterView as CollectionViewAdv;
                            var source = view.GetClonedSource().DefaultView;
                            clonedItemsProperties = ((ITypedList)(source)).GetItemProperties(null);

                            var columnFilterString = view.GetFilterString(colName, true);
                            var filterString = view.GetFilterString(colName, false);

                            columnFilterString = "NOT(" + columnFilterString + ")";

                            if (!string.IsNullOrEmpty(filterString))
                                filterString = ("(" + columnFilterString + ")").AndPredicate() + filterString;
                            else
                                filterString = columnFilterString;

                            source.RowFilter = filterString;

                            var distinctRecords =
                                source.Cast<object>().Select(x => clonedItemsProperties.GetValue(x, colName)).Distinct();
                            distinctRecords.ForEach((o) => this.distinctCollection.Add(new FilterElement()
                                {
                                    ActualValue = o,
                                    FormattedString = this.GetFormattedString
                                }));
                        }

                    }));
#endif
            }

            if (!this.allowBlankFilters)
                this.distinctCollection = this.distinctCollection.Where(x => x.ActualValue != null).ToList();

            if (this.Column.ColumnFilter == ColumnFilter.DisplayText)
            {
#if WPF
                this.Dispatcher.Invoke((Action)(() =>
                 {
#endif
                     this.distinctCollection = this.distinctCollection.Distinct(new FilterControlEqualityComparer<FilterElement>()).ToList();
#if WPF
                 }));
#endif
            }
            this.distinctCollection.Sort(new FilterElementAscendingOrder());
        }

        private void SetItemSource()
        {
            if (this.Column != null)
            {
                var args = new GridFilterItemsPopulatedEventArgs(this.distinctCollection, this.Column, this, this.Column.DataGrid);
                this.Column.DataGrid.RaiseFilterListItemsPopulated(args);
                this.distinctCollection = args.ItemsSource != null ? args.ItemsSource.ToList() : null;
            }

            if (this.distinctCollection != null)
                this.distinctCollection.ForEach(lstItem => lstItem.PropertyChanged += this.OnFilterElementPropertyChanged);
            this.CheckboxFilterControl.FilterListBoxItem = this.distinctCollection;
            // If filtering is applied through CheckboxFilter, need to set previous itemssource
            if (this.FilteredFrom == FilteredFrom.CheckboxFilter)
                this.SetPreviousItemsSource();
            else
                this.CheckboxFilterControl.previousItemSource = null;
            this.CheckboxFilterControl.ItemsSource = this.distinctCollection;
            this.AdvancedFilterControl.propertyChangedfromsettingControlValues = true;
            this.AdvancedFilterControl.ComboItemsSource = this.distinctCollection == null ? null : new ObservableCollection<FilterElement>(this.distinctCollection);
            this.AdvancedFilterControl.propertyChangedfromsettingControlValues = false;
            if (this.AdvancedFilterControl.ComboItemsSource != null)
            {
#if WPF
                var nullfilter = this.AdvancedFilterControl.ComboItemsSource.FirstOrDefault(filter => filter.ActualValue == null || filter.ActualValue == DBNull.Value);
#else
                var nullfilter = this.AdvancedFilterControl.ComboItemsSource.FirstOrDefault(filter => filter.ActualValue == null);
#endif
                this.AdvancedFilterControl.ComboItemsSource.Remove(nullfilter);
                var emptyfilter = this.AdvancedFilterControl.ComboItemsSource.FirstOrDefault(filter => filter.ActualValue != null && filter.ActualValue.Equals(string.Empty));
                this.AdvancedFilterControl.ComboItemsSource.Remove(emptyfilter);
                if (Column.FilterPredicates != null && Column.FilterPredicates.Any() && this.FilteredFrom == FilteredFrom.AdvancedFilter)
                    this.AdvancedFilterControl.SetAdvancedFilterControlValues(Column.FilterPredicates);
            }
            this.CheckboxFilterControl.MaintainSelectAllCheckBox();
            this.CheckboxFilterControl.IsItemSourceLoaded = true;
#if WPF
            this.Focus();
#endif
        }

        /// <summary>
        /// After filtering, set previousItemSource from FilterListBoxItem
        /// </summary>
        private void SetPreviousItemsSource()
        {
            this.CheckboxFilterControl.previousItemSource = new List<FilterElement>();          
            IEnumerable<FilterElement> filterListBoxItem;
            if (checkedItemsCount > unCheckedItemsCount && unCheckedItemsCount > 0)
                filterListBoxItem = this.CheckboxFilterControl.FilterListBoxItem.Where(i => !(i.IsSelected));
            else
                filterListBoxItem = this.CheckboxFilterControl.FilterListBoxItem.Where(i => i.IsSelected);
            foreach (var item in filterListBoxItem)
                this.CheckboxFilterControl.previousItemSource.Add(new FilterElement() { ActualValue = item.ActualValue, DisplayText = item.DisplayText, FormattedString = item.FormattedString, IsSelected = item.IsSelected });
        }

        /// <summary>
        /// Set ColumnDataType of AdvancedFilterControl.
        /// </summary>   
        /// <param name="type">specifies the Type </param>
        public void SetColumnDataType(Type type)
        {
            if (this.AdvancedFilterControl != null)
                this.AdvancedFilterControl.ColumnDataType = type;
        }

        ///<summary>
        /// Gets the formatted text of Actual Value.
        /// </summary>
        /// <param name="item">Actual value</param>
        public string GetFormattedString(object item)
        {
            if (item != null && item is FilterElement)
            {
                var fitlerElement = item as FilterElement;
                if (fitlerElement.Record != null)
                    return FilterHelpers.GetFormattedString(Column, provider, fitlerElement.Record, true);
                return FilterHelpers.GetFormattedString(Column, provider, fitlerElement.ActualValue, false);
            }
            else
                return FilterHelpers.GetFormattedString(Column, provider, item, false);
        }

#endregion

#region Events

#region OkButtonClick

        /// <summary>
        /// Occurs when the OkButton is clicked
        /// </summary>  
        public event OkButtonClickEventHandler OkButtonClick;

        /// <summary>
        /// Raises the ok button click.
        /// </summary>
        internal void RaiseOkButtonClick(IEnumerable<FilterElement> checkedElement, IEnumerable<FilterElement> unCheckedElement)
        {
            OnOkButtonClick(new OkButtonClikEventArgs(this) { UnCheckedElements = unCheckedElement, CheckedElements = checkedElement });
        }

        /// <summary>
        /// Raises the <see cref="OkButtonClick"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Controls.Grid.OkButtonClikEventArgs"/> instance containing the event data.</param>
        private void OnOkButtonClick(OkButtonClikEventArgs e)
        {
            if (this.OkButtonClick != null)
                this.OkButtonClick(this, e);
        }

#endregion

#region PopupOpened

        /// <summary>
        /// Occurs when the filter Popup is opened.
        /// </summary>   
        public event PopupOpenedEventHandler PopupOpened;

        /// <summary>
        /// Raises the popup opened.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<FilterElement> RaisePopupOpened()
        {
            return OnPopupOpened(new PopupOpenedEventArgs());
        }

        /// <summary>
        /// Raises the <see cref="GridControlBase.CurrentCellActivated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private IEnumerable<FilterElement> OnPopupOpened(PopupOpenedEventArgs e)
        {
            if (PopupOpened != null)            
                PopupOpened(this, e);

            return e.ItemsSource;
        }

#endregion

#region OnFilterElementPropertyChanged

        /// <summary>
        /// Occurs when the FilterElement is changed.
        /// </summary>   
        public event OnFilterElementPropertyChangedEventHandler OnFilterElementChanged;

        /// <summary>
        /// Raises the on filter element property changed.
        /// </summary>
        /// <param name="FilterElement">The filter element.</param>
        /// <param name="SelectAllChecked">The select all checked.</param>
        internal void RaiseOnFilterElementPropertyChanged(FilterElement FilterElement, Nullable<bool> SelectAllChecked)
        {
            FilterElementPropertyChanged(new OnFilterElementPropertyChangedEventArgs() { FilterElement = FilterElement, SelectAllChecked = SelectAllChecked });
        }

        /// <summary>
        /// Filters the element property changed.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Controls.Grid.OnFilterElementPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void FilterElementPropertyChanged(OnFilterElementPropertyChangedEventArgs e)
        {
            if (this.OnFilterElementChanged != null)
                this.OnFilterElementChanged(this, e);
        }

#endregion

#region Reszing

#if UWP
        void OnResizingThumbMoved(object sender, MouseEventArgs e)
#else
        void OnResizingThumbMoved(object sender, DragDeltaEventArgs e)
#endif
        {

#if UWP
            this.ResizingThumb.CapturePointer(e.Pointer);
            PointerPoint pt = e.GetCurrentPoint(FilterPopUpBorder);
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 1);
            Point point = pt.Position;
            if (pt.Properties.IsLeftButtonPressed)
            {
                var actualWidth = point.X;
                var actualHeight = point.Y;
#else
            {
                var actualWidth = this.FilterPopupWidth + e.HorizontalChange;
                var actualHeight = this.FilterPopupHeight + e.VerticalChange;
#endif
                if (!(actualWidth > this.MaxWidth || actualWidth < this.MinWidth) && actualWidth >= 0)
                {
                    isResizing = true;
                    this.FilterPopupWidth = actualWidth;
                    isResizing = false;
                }
                if (!(actualHeight > this.MaxHeight || actualHeight < this.MinHeight) && actualHeight >= 0)
                {
                    isResizing = true;
                    this.FilterPopupHeight = actualHeight;
                    isResizing = false;
                }
                return;
            }
        }

#if UWP
        void OnResizingThumbExited(object sender, MouseEventArgs e)
        {
            this.ResizingThumb.ReleasePointerCapture(e.Pointer);
            if (Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        void OnResizingThumbEntered(object sender, MouseEventArgs e)
        {
            this.ResizingThumb.CapturePointer(e.Pointer);
            if (Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.SizeNorthwestSoutheast)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 1);
        }

        void OnResizingThumbReleased(object sender, MouseButtonEventArgs e)
        {
            this.ResizingThumb.ReleasePointerCapture(e.Pointer);
            if (Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }
#endif

#endregion

#region OnFilterControlLoaded

        void OnGridFilterControlLoaded(object sender, RoutedEventArgs e)
        {
            if (this.FilterPopUp != null)
            {
                WireEvents();
                InitializeGridFilterPane();
            }
        }

#endregion

#endregion

#region Overrides
        /// <summary>
        /// Builds the visual tree for the GridFilterControl when a new template is applied.
        /// </summary>
#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            FilterPopUp = this.GetTemplateChild("PART_FilterPopup") as Popup;
            OkButton = this.GetTemplateChild("PART_OkButton") as Button;
            CancelButton = this.GetTemplateChild("PART_CancelButton") as Button;
            CheckboxFilterControl = this.GetTemplateChild("PART_CheckboxFilterControl") as CheckboxFilterControl;
            Part_ClearFilterButton = this.GetTemplateChild("PART_ClearFilterButton") as Button;
            PART_SortAscendingButton = this.GetTemplateChild("PART_SortAscendingButton") as SortButton;
            PART_SortDescendingButton = this.GetTemplateChild("PART_SortDescendingButton") as SortButton;
            ResizingThumb = this.GetTemplateChild("PART_ThumbGripper") as Thumb;
#if !WPF
            FilterPopUpBorder = this.GetTemplateChild("PART_FilterPopUpBorder") as Border;
#endif
            Part_AdvancedFilterButton = this.GetTemplateChild("PART_AdvancedFilterButton") as Button;
            AdvancedFilterControl = this.GetTemplateChild("PART_AdvancedFilterControl") as AdvancedFilterControl;
            if (AdvancedFilterControl != null)
            {
                AdvancedFilterControl.gridFilterCtrl = this;
                canGenerateUniqueItems = AdvancedFilterControl.CanGenerateUniqueItems;
            }
            if (CheckboxFilterControl != null)
                CheckboxFilterControl.gridFilterCtrl = this;
            
            if (Column != null && AdvancedFilterControl != null && Column.DataGrid != null && Column.DataGrid.View != null)
            {
                // #136660 code changes suggested by Akuna Capital
                this.FilteredFrom = Column.FilteredFrom;
                if (Column.IsUnbound || Column.DataGrid.View.IsDynamicBound)
                    AdvancedFilterControl.ColumnDataType = typeof(string);
                else
                {
                    var pdc = this.Column.DataGrid.View.GetItemProperties();
                    if (pdc == null)
                        AdvancedFilterControl.ColumnDataType = typeof(string);
                    if (pdc != null)
                    {
                        var pd = pdc.GetPropertyDescriptor(this.Column.MappingName);
                        if (pd == null)
                            AdvancedFilterControl.ColumnDataType = typeof(string);
                        else
                            AdvancedFilterControl.ColumnDataType = pd.PropertyType;
                    }
                }
            }
#if WPF
            if (FilterPopUp != null)
                FilterPopUp.StaysOpen = true;
#endif
#if !WPF
            if (FilterPopUp != null && localToggleRect != null)
#if UWP
                SetPopupPosition(localToggleRect, localWindowPoint, localTogglepadding, localHeaderCellPading);
#else
                SetPopupPosition(localToggleRect, localWindowPoint, localTogglepadding);
#endif
#endif

        }
#if UWP
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            base.OnPointerMoved(e);

        }
#else
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.Cursor != Cursors.Arrow)
                this.Cursor = Cursors.Arrow;
            base.OnMouseMove(e);
        }
#endif
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!this.IsOpen)
                return;
#if WPF
            UIElement focusedElement = Keyboard.FocusedElement as UIElement;
#endif
            switch (e.Key)
            {
                case Key.Escape:
                    if (this.IsOpen)
                        this.IsOpen = false;
                    break;
#if WPF
                case Key.Up:
                case Key.Left:
                    if (this.FilterPopUp.IsKeyboardFocusWithin)
                    {
                        focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        e.Handled = true;
                        return;
                    }
                    this.FilterPopUp.Child.Focus();
                    break;

                case Key.Down:
                case Key.Right:
                    if (this.FilterPopUp.IsKeyboardFocusWithin)
                    {
                        focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                        return;
                    }
                    this.FilterPopUp.Child.Focus();
                    break;

                case Key.Tab:
                    if (this.FilterPopUp.IsKeyboardFocusWithin)
                        return;

                    this.FilterPopUp.Child.Focus();
                    var tabFocusedElement = Keyboard.FocusedElement as UIElement;
                    tabFocusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    e.Handled = true;
                    break;
#endif
#if UWP
                case Key.Tab:
                    SetFocus();
                    e.Handled = true;
                    break;
                case Key.Up:
                    SetFocus();
                    e.Handled = true;
                    break;
                case Key.Down:
                    SetFocus();
                    e.Handled = true;
                    break;
#endif
            }
            base.OnKeyDown(e);
        }
#if UWP
        private void SetFocus()
        {
            if (filterPopUpChildControls.Count == 0)
                GridUtil.GetNavigatableDescendants(this.FilterPopUpBorder, ref filterPopUpChildControls);
            foreach (var control in filterPopUpChildControls)
            {
                if (control.IsEnabled)
                {
                    control.Focus(FocusState.Programmatic);
                    break;
                }
            }
        }
#endif
#if !WPF
        void OnFilterPopUpBorderKeyDown(object sender, KeyEventArgs e)
        {

            Control focusedElement = FocusManager.GetFocusedElement() as Control;

            if (filterPopUpChildControls.Count == 0)
                GridUtil.GetNavigatableDescendants(this.FilterPopUpBorder, ref filterPopUpChildControls);

            switch (e.Key)
            {
                case Key.Escape:
                    if (this.IsOpen)
                        this.IsOpen = false;
                    break;
                case Key.Up:
                    {
                        var previousSiblingIndex = filterPopUpChildControls.IndexOf(focusedElement);
                        previousSiblingIndex = --previousSiblingIndex < 0 ? filterPopUpChildControls.Count - 1 : previousSiblingIndex;
                        var nextFocusableElement = filterPopUpChildControls[previousSiblingIndex].IsEnabled ? filterPopUpChildControls[previousSiblingIndex] : filterPopUpChildControls[--previousSiblingIndex];
                        ((Control)nextFocusableElement).Focus(FocusState.Keyboard);
                        e.Handled = true;
                    }

                    break;

                case Key.Down:
                    {
                        var nextSiblingIndex = filterPopUpChildControls.IndexOf(focusedElement);
                        nextSiblingIndex = ++nextSiblingIndex < filterPopUpChildControls.Count ? nextSiblingIndex : 0;
                        var nextFocusableElement = filterPopUpChildControls[nextSiblingIndex].IsEnabled ? filterPopUpChildControls[nextSiblingIndex] : filterPopUpChildControls[++nextSiblingIndex];
                        ((Control)nextFocusableElement).Focus(FocusState.Keyboard);
                        e.Handled = true;
                    }
                    break;
            }
        }
#endif

#if WPF
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        { 
            e.Handled = true;
            base.OnMouseLeftButtonUp(e);
        }

        //WPF - 16133 . While scrolling the mouse wheel on view other than CheckboxFilterControl in FilterPopup, 
        //it scrolls the grid from LineDown() method in VisualContainer. So we need to handle Mouse wheel for GridFilterControl.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            base.OnMouseWheel(e);
        }
#endif

#endregion

#region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnWireEvents();
            if (isDisposing)
            {
                if (CheckboxFilterControl != null)
                {
                    CheckboxFilterControl.Dispose();
                    CheckboxFilterControl = null;
                }
                if (AdvancedFilterControl != null)
                {
                    AdvancedFilterControl.Dispose();
                    AdvancedFilterControl = null;
                }
                if (provider != null)
                    provider = null;

                if (excelFilterView != null)
                    excelFilterView = null;

#if WPF
                if(bgWorkertoPopulate != null)
                {
                    bgWorkertoPopulate.Dispose();
                    bgWorkertoPopulate = null;
                }
#endif
            }
            this.Loaded -= OnGridFilterControlLoaded;
            isdisposed = true;
        }

#endregion

#region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

#endregion

    }

    public class SortButton : Button
    {
#region Ctor

        public SortButton()
        {
            base.DefaultStyleKey = typeof(SortButton);
        }

#endregion

#region IsSortedProperty

        public bool IsSorted
        {
            get { return (bool)this.GetValue(SortButton.IsSortedProperty); }
            set { this.SetValue(SortButton.IsSortedProperty, value); }
        }

        public static readonly DependencyProperty IsSortedProperty = DependencyProperty.Register(
              "IsSorted",
              typeof(bool),
              typeof(SortButton),
              new PropertyMetadata(false, OnIsSortedChanged));

        private static void OnIsSortedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SortButton).ApplyState((bool)e.NewValue);
        }

#endregion

#region Icon

        /// <summary>
        /// Gets or sets the UIElement of the SortButton.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.UIElement"/> of the SortButton.
        /// </value>
        public UIElement Icon
        {
            get { return (UIElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SortButton.Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(UIElement), typeof(SortButton), new PropertyMetadata(null, OnSortIconChanged));

        private static void OnSortIconChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var buton = obj as SortButton;
            buton.OnSortIconChanged(args);
        }

        private void OnSortIconChanged(DependencyPropertyChangedEventArgs args)
        {
            if (PART_IconPresenter != null)
                PART_IconPresenter.Child = Icon;
        }
#endregion

#region Overrides

        Border PART_IconPresenter = null;
        /// <summary>
        /// Builds the visual tree for the SortButton when a new template is applied.
        /// </summary>
#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            if (PART_IconPresenter != null && PART_IconPresenter.Child != null)
                PART_IconPresenter.Child = null;
            base.OnApplyTemplate();
            PART_IconPresenter = this.GetTemplateChild("PART_IconPresenter") as Border;
            if (PART_IconPresenter != null)
                PART_IconPresenter.Child = Icon as UIElement;
            this.ApplyState(this.IsSorted);
        }

#endregion

#region Private Methods

        private void ApplyState(bool isSorted)
        {
            if (isSorted)
                VisualStateManager.GoToState(this, "Sorted", true);
            else
                VisualStateManager.GoToState(this, "UnSorted", true);
        }
#endregion
    }
}
