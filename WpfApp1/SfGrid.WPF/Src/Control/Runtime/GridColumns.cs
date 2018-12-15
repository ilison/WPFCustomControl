#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Controls.Input;
using Syncfusion.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using Syncfusion.Windows.Shared;
using MaxValidation = Syncfusion.Windows.Shared.MaxValidation;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides the base functionalities for all the column types in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    [StyleTypedProperty(Property = "CellStyle", StyleTargetType = typeof(GridCell))]
    [StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(GridHeaderCellControl))]
    [StyleTypedProperty(Property = "FilterPopupStyle", StyleTargetType = typeof(GridFilterControl))]

    public abstract class GridColumn : GridColumnBase, INotifyDependencyPropertyChanged, IDisposable, IFilterDefinition
    {
        #region Fields
        private Type columnMemberType;
        internal ColumnPropertyChanged ColumnPropertyChanged;
        internal double ExtendedWidth = Double.NaN;

        internal bool hasFilterRowCellStyle;
        private bool isUnbound = false;
        private bool isdisposed = false;

        private FilteredFrom filteredfrom = FilteredFrom.None;
        internal bool useBindingValue = false;

        private ObservableCollection<FilterPredicate> filterPredicates;
        private FilterBehavior filterBehavior;
        private ColumnFilter columnFilter;
        private string filterRowEditorType = "Default";
#if !WPF
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string formatString;
#endif
        #endregion

        #region Dependency Property

        /// <summary>
        /// Gets or sets the member type to load the appropriate <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterType"/> to <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> .       
        /// </summary>
        /// <value>
        /// The type that loads the appropriate <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterType"/> to <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> in column.
        /// </value>
        /// <remarks> 
        /// By default, the <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> loads TextFilter , when the underlying data source is dynamic.
        /// So , you can decide the appropriate <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterType"/> by using this property based on its member type.
        /// For example, the TextFilter for string type , NumberFilter for numeric type and the DateFilter for DateTime type of member.
        /// </remarks>
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public Type ColumnMemberType
        {
            get { return columnMemberType; }
            set { columnMemberType = value; }
        }

        internal override void OnGridValidationModeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "GridValidationMode");
            this.UpdateValidationMode();
        }

        /// <summary>
        /// Gets or sets the style applied to the FilterRow cell of the column.
        /// </summary>
        /// <value>
        /// The style that is applied to the FilterRow cell of the column. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a FilterRow cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.RowFilter.GridFilterRowCell"/>.
        /// </remarks>
        public Style FilterRowCellStyle
        {
            get { return (Style)GetValue(FilterRowCellStyleProperty); }
            set
            {
                SetValue(FilterRowCellStyleProperty, value);
                if (this.ReadLocalValue(FilterRowCellStyleProperty) != DependencyProperty.UnsetValue)
                {
                    hasFilterRowCellStyle = true;
                    if (this.ColumnPropertyChanged != null)
                        this.ColumnPropertyChanged(this, "FilterRowCellStyle");
                }
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowCellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowCellStyle dependency property.
        /// </remarks>   
        public static readonly DependencyProperty FilterRowCellStyleProperty =
            GridDependencyProperty.Register("FilterRowCellStyle", typeof(Style), typeof(GridColumn), new GridPropertyMetadata(null, OnFilterRowCellStyleChanged));

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase.CellTemplateSelector"/> dependency property is changed in column.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for the <b>CellTemplateSelector</b> dependency property changes.
        /// </param>
        protected override void OnCellTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnCellTemplateSelectorChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellTemplateSelector");
        }

        internal override void OnAllowSortChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null && this.AllowSorting)
                this.ColumnPropertyChanged(this, "AllowSorting");
        }
        internal override void SetValueBinding(bool internalset, BindingBase value)
        {
            base.SetValueBinding(internalset, value);
            UpdateBindingInfo();
        }

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase.DisplayBinding"/> of column.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase.DisplayBinding"/>.
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {

        }

        internal override void SetDisplayBinding(bool internalset, BindingBase value)
        {
            base.SetDisplayBinding(internalset, value);
            UpdateBindingInfo();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(0 + padLeft, padTop, 0 + padRight, padBotton)
                           : new Thickness(2, 2, 2, 2);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                          ? new Thickness(3 + padLeft, 1 + padTop, 3 + padRight, 1 + padBotton)
                          : new Thickness(3, 1, 3, 1);
#endif
        }       

        /// <summary>
        /// Gets or sets a value that indicates whether the binding gets property value
        /// from column wrapper instead of reflecting value from the item properties.
        /// </summary>
        /// <remarks>
        /// This property helps to perform sorting , filtering ,grouping operations on
        /// complex or indexer properties.
        /// </remarks>
        /// <value>
        /// <b>true</b> if the property value gets from column wrapper;
        /// otherwise, <b>false</b> . The default value is <b>false</b>.
        /// </value>
        public bool UseBindingValue
        {
            get { return (bool)GetValue(UseBindingValueProperty); }
            set { SetValue(UseBindingValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.UseBindingValue dependency
        /// property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.UseBindingValue dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty UseBindingValueProperty =
            DependencyProperty.Register("UseBindingValue", typeof(bool), typeof(GridColumn), new PropertyMetadata(false, OnUseBindingValuePropertyChanged));

        private static void OnUseBindingValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumn);
            if (column != null)
                column.UseBindingValue = (bool)e.NewValue;

            column.useBindingValue = column.UseBindingValue;
        }

        /// <summary>
        /// Gets or sets the value that indicates how the column width is determined.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType"/> enumeration that adjust the column
        /// width.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.None"/>.
        /// </value>      
        public GridLengthUnitType ColumnSizer
        {
            get { return (GridLengthUnitType)GetValue(ColumnSizerProperty); }
            set { SetValue(ColumnSizerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.ColumnSizer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.ColumnSizer dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ColumnSizerProperty =
            GridDependencyProperty.Register("ColumnSizer", typeof(GridLengthUnitType), typeof(GridColumn), new GridPropertyMetadata(GridLengthUnitType.None, OnGridColumnSizerChanged));



        /// <summary>
        ///  Gets or sets a value that indicates whether the user can rearrange the columns.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can re arrange the columns; otherwise, <b>false</b>. The default value is <b>false</b> .
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfGridBase.AllowDraggingColumns"/>
        public bool AllowDragging
        {
            get { return (bool)GetValue(AllowDraggingProperty); }
            set { SetValue(AllowDraggingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.AllowDragging dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.AllowDragging dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty AllowDraggingProperty =
            GridDependencyProperty.Register("AllowDragging", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicates whether the user can drag and drop the column to GroupDropArea.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can drag and drop the column to GroupDropArea; otherwise, <b>false</b>.
        /// The default value is <b>true</b> .
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowGrouping"/>
        public bool AllowGrouping
        {
            get { return (bool)GetValue(AllowGroupingProperty); }
            set { SetValue(AllowGroupingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.AllowGrouping dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.AllowGrouping dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty AllowGroupingProperty =
            GridDependencyProperty.Register("AllowGrouping", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(true));

        /// <summary>
        ///  Gets or sets a value that indicates whether the user can resize the column. 
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can adjust the column width ; otherwise , <b>false</b>. The default value is <b>false</b> .
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowResizingColumns"/>
        public bool AllowResizing
        {
            get { return (bool)GetValue(AllowResizingProperty); }
            set { SetValue(AllowResizingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.AllowResizing dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.AllowResizing dependency
        /// property.
        /// </remarks>
        public static readonly DependencyProperty AllowResizingProperty =
            GridDependencyProperty.Register("AllowResizing", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether to enable UI Filtering in column header to filter the column.
        /// </summary>
        /// <value>
        /// <b>true</b> if the UI filtering is enabled in the column ; otherwise, <b>false</b>.The default value is <b>false</b>.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowFiltering"/>
        public bool AllowFiltering
        {
            get
            {
                var valueColumn = this.ReadLocalValue(GridColumn.AllowFilteringProperty);
                if (DataGrid != null && valueColumn == DependencyProperty.UnsetValue)
                    return this.DataGrid.AllowFiltering;
                else
                    return (bool)GetValue(AllowFilteringProperty);
            }
            set { SetValue(AllowFilteringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.AllowFiltering dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.AllowFiltering dependency
        /// property.
        /// </remarks>
        public static readonly DependencyProperty AllowFilteringProperty =
            GridDependencyProperty.Register("AllowFiltering", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(false, OnAllowFilteringChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the data is automatically filtered as soon as an user selects or types value in the filter pop-up of column.
        /// </summary>
        /// <value>
        /// <b>true</b> if the data is filtered automatically ; otherwise <b>false</b> .The default value is <b>false</b>.
        /// </value>
        public bool ImmediateUpdateColumnFilter
        {
            get { return (bool)GetValue(ImmediateUpdateColumnFilterProperty); }
            set { SetValue(ImmediateUpdateColumnFilterProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.ImmediateUpdateColumnFilter dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.ImmediateUpdateColumnFilter dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty ImmediateUpdateColumnFilterProperty =
            GridDependencyProperty.Register("ImmediateUpdateColumnFilter", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(false, null));

        /// <summary>
        /// Gets or sets a value that indicates whether the FilterRowOptions button is visible in the GridFilterRowCell.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies the FilterRowOptions button visiblity in GridFilterRowCell; The default value is <b> Visibility.Visible</b> .
        /// </value> 
        public Visibility FilterRowOptionsVisibility
        {
            get { return (Visibility)GetValue(FilterRowOptionsVisibilityProperty); }
            set { SetValue(FilterRowOptionsVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowOptionsVisibility dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowOptionsVisibility dependency property.
        /// </remarks>
        public static readonly DependencyProperty FilterRowOptionsVisibilityProperty =
            GridDependencyProperty.Register("FilterRowOptionsVisibility", typeof(Visibility), typeof(GridColumn), new GridPropertyMetadata(Visibility.Visible, OnFilterRowOptionsVisibilityPropertyChanged));


        /// <summary>
        /// Gets or sets a value that decides the default FilterRowCondition that have to be filter while typing in corresponding FilterRow cell.
        /// </summary>
        /// <remarks>
        /// 	<para>The FilterRowCell which loads the TextBox, Numeric or DateTime editor
        /// allows you to edit the values to filter in the corresponding column. Where you can 
        /// change the default conditions that want to be applied for the particular filtering as per
        /// the editor that have been loaded. </para>
        /// 	<list type="table">
        /// 		<listheader>
        /// 			<term>Editor</term>
        /// 			<description>Default FilterRowCondition</description>
        /// 		</listheader>
        /// 		<item>
        /// 			<term>TextBox</term>
        /// 			<description>BeginsWith</description>       
        /// 		</item>
        /// 		<item>
        /// 			<term>Numeric</term>
        /// 			<description>Equals</description>
        /// 		</item>
#if WPF
        /// 		<item>
        /// 			<term>DateTime</term>
        /// 			<description>Equals</description>
        /// 		</item>		
#endif
        /// 	</list>
        /// </remarks>
        public FilterRowCondition FilterRowCondition
        {
            get
            {
                var filterRowType = this.ReadLocalValue(GridColumn.FilterRowConditionProperty);
                if (filterRowType == DependencyProperty.UnsetValue)
                {
                    var rowFilterType = this.GetRowFilterType();
                    if (rowFilterType == "TextBox")
                        return FilterRowCondition.BeginsWith;
                    else
                        return FilterRowCondition.Equals;
                }
                return (FilterRowCondition)GetValue(FilterRowConditionProperty);
            }
            set
            {
                SetValue(FilterRowConditionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowCondition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowCondition dependency property.
        /// </remarks>
        public static readonly DependencyProperty FilterRowConditionProperty =
            GridDependencyProperty.Register("FilterRowCondition", typeof(FilterRowCondition), typeof(GridColumn), new GridPropertyMetadata(Grid.FilterRowCondition.Equals, OnFilterRowConditionChanged));

        private static void OnFilterRowConditionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var column = obj as GridColumn;
            if (column.DataGrid == null || !column.DataGrid.HasView)
                return;
            column.CheckFilterType((FilterRowCondition)args.NewValue);
        }


        /// <summary>
        /// Gets or sets the style applied to the filter popup in column.
        /// </summary>
        /// <value>
        /// The style that is applied to the filter pop-up in GridColumn. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for filter pop-up, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterPopupStyle"/>
        public Style FilterPopupStyle
        {
            get { return (Style)GetValue(FilterPopupStyleProperty); }
            set { SetValue(FilterPopupStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.FilterPopupStyle dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.FilterPopupStyle dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty FilterPopupStyleProperty =
            GridDependencyProperty.Register("FilterPopupStyle", typeof(Style), typeof(GridColumn), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the filter pop-up in GridColumn.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the filter pop-up in GridColumn. The default value is <b>null</b>.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterPopupTemplate"/>
        public DataTemplate FilterPopupTemplate
        {
            get { return (DataTemplate)GetValue(FilterPopupTemplateProperty); }
            set { SetValue(FilterPopupTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.FilterPopupTemplate dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.FilterPopupTemplate dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty FilterPopupTemplateProperty =
            GridDependencyProperty.Register("FilterPopupTemplate", typeof(DataTemplate), typeof(GridColumn), new GridPropertyMetadata(null));

        /// <summary>
        /// Get or sets a value that indicates whether the blank values are allowed for filtering in column.
        /// </summary>        
        /// <value>
        /// <b>true</b> if the blank values are allowed for filtering in column ; otherwise , <b>false</b> . The default value is <b>false</b>.
        /// </value>        
        public bool AllowBlankFilters
        {
            get { return (bool)this.GetValue(GridColumn.AllowBlankFiltersProperty); }
            set { this.SetValue(GridColumn.AllowBlankFiltersProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.AllowBlankFilters dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.AllowBlankFilters dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty AllowBlankFiltersProperty =
            GridDependencyProperty.Register("AllowBlankFilters", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(true, null));

        /// <summary>
        /// Gets the filtered value of the particular column where the filtering has been applied through FilterRow.
        /// </summary>
        /// <value>
        /// The filtered value that is applied FilterRow in the particular column.
        /// </value>
        public object FilterRowText
        {
            get { return (object)GetValue(FilterRowTextProperty); }
            protected internal set { SetValue(FilterRowTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.FilterRowText dependency property.
        /// </remarks> 
        public static readonly DependencyProperty FilterRowTextProperty =
            GridDependencyProperty.Register("FilterRowText", typeof(object), typeof(GridColumn), new GridPropertyMetadata(null));


        /// <summary>
        /// Gets or sets value that indicates whether the case sensitive filtering is enabled on FilterRowCell of a particular column.
        /// </summary>
        /// <value>
        /// <b>true </b> if the case sensitive filtering is enabled on the FilterRowCell of a column; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool IsCaseSensitiveFilterRow
        {
            get { return (bool)GetValue(IsCaseSensitiveFilterRowProperty); }
            set { SetValue(IsCaseSensitiveFilterRowProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.IsCaseSensitiveFilterRow dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.IsCaseSensitiveFilterRow dependency property.
        /// </remarks> 
        public static readonly DependencyProperty IsCaseSensitiveFilterRowProperty =
            GridDependencyProperty.Register("IsCaseSensitiveFilterRow", typeof(bool), typeof(GridColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that decides whether grouping is processed based on display value or edit value of column. 
        /// </summary>
        /// <value>
        /// Default <see cref="Syncfusion.Data.DataReflectionMode.Default"/> mode performs default grouping operation based on value.
        /// Display <see cref="Syncfusion.Data.DataReflectionMode.Display"/> mode performs grouping operation based on display value of column.
        /// Value <see cref="Syncfusion.Data.DataReflectionMode.Value"/> mode performs grouping operation based on actual value of column.
        /// </value> 
        /// <remarks>
        /// Group key is set based on  <see cref="yncfusion.UI.Xaml.Grid.GridColumn.GroupMode"/> property.
        /// </remarks>
        public DataReflectionMode GroupMode
        {
            get { return (DataReflectionMode)GetValue(GroupModeProperty); }
            set { SetValue(GroupModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumn.GroupMode dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumn.GroupMode dependency
        /// property.
        /// </remarks>
        public static readonly DependencyProperty GroupModeProperty =
        DependencyProperty.Register("GroupMode", typeof(DataReflectionMode), typeof(GridColumn), new PropertyMetadata(DataReflectionMode.Default));

        #endregion

        #region CLR Property
        /// <summary>
        /// Gets the cell type of the column which denotes renderer associated with column.
        /// </summary>
        /// <value>
        /// A string that represents the cell type of the column.
        /// </value>
        public string CellType
        {
            get;
            internal set;
        }

        // #136660 code changes suggested by Akuna Capital
        /// <summary>
        /// Gets a value that determines whether the column is filtered from Check box filter UI or Advanced filter in UI Filtering.
        /// </summary>
        /// <value>
        /// One of <see cref="Syncfusion.UI.Xaml.Grid.FilteredFrom"/> enumeration that specifies when the column is from .
        /// </value>
        public FilteredFrom FilteredFrom
        {
            get { return filteredfrom; }
            set { filteredfrom = value; }
        }

        internal bool IsUnbound
        {
            get { return isUnbound; }
            set { isUnbound = value; }
        }

        /// <summary>
        /// Gets the reference to the SfDataGrid control.
        /// </summary>
        protected internal SfDataGrid DataGrid
        {
            get { return (SfDataGrid)this.GridBase; }
            private set { this.GridBase = value; }
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the collection of <see cref="Syncfusion.Data.FilterPredicate"/> in the column.
        /// </summary>
        /// <value>
        /// The collection of filter predicate in the column.
        /// </value>
        public ObservableCollection<FilterPredicate> FilterPredicates
        {
            get
            {
                if (this.filterPredicates == null)
                {
                    this.filterPredicates = new ObservableCollection<FilterPredicate>();
                }
                return this.filterPredicates;
            }
        }

        /// <summary>
        /// Gets or sets a value that decides whether filter value should be considered as string type or its underlying type while filtering.
        /// </summary>
        /// <value>One of the <see cref="Syncfusion.UI.Xaml.Grid.FilterBehavior"/> enumeration that specifies the filter behavior of the column.</value>
        /// <remarks>
        /// The Advanced Filter UI will be loaded based on this property.
        /// </remarks>
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public FilterBehavior FilterBehavior
        {
            get { return filterBehavior; }
            set { filterBehavior = value; }
        }

        /// <summary>
        /// Gets or sets a value that decides whether to filter based on display value or based on MappingName. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Data.ColumnFilter"/> enumeration that decides how the items populated for the filter control in the column. 
        /// By default, the filter value is populated based on <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> property. 
        /// </value>     
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public ColumnFilter ColumnFilter
        {
            get { return columnFilter; }
            set { columnFilter = value; }
        }

        /// <summary>
        /// Gets or sets a value which denotes the Editor which have to be load in corresponding FilterRowCell.
        /// </summary>
        /// <remarks>
        /// 	<para>The name which refers the renderers in <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterRowCellRenderers"/> collection. 
        ///      By default, the corresponding renderer will be loaded based on underlying type. </para>
        /// 	<list type="table">
        /// 		<listheader>
        /// 			<term>Name</term>
        /// 			<description>Renderers</description>
        /// 		</listheader>
        /// 		<item>
        /// 			<term>TextBox</term>
        /// 			<description>GridFilterRowTextBoxRenderer</description>       
        /// 		</item>
        /// 		<item>
        /// 			<term>Numeric</term>
        /// 			<description>GridFilterRowNumericRenderer</description>
        /// 		</item>
        ///         <item>
        /// 			<term>CheckBox</term>
        /// 			<description>GridFilterRowCheckBoxRenderer</description>
        /// 		</item>		
#if WPF
        /// 		<item>
        /// 			<term>DateTime</term>
        /// 			<description>GridFilterRowDateTimeRenderer</description>
        /// 		</item>	
        ///         <item>
        /// 			<term>ComboBox</term>
        /// 			<description>GridFilterRowComboBoxRenderer</description>
        /// 		</item>	
        ///         <item>
        /// 			<term>MultiSelectComboBox</term>
        /// 			<description>GridFilterRowMultiSelectRenderer</description>
        /// 		</item>	
#endif
        /// 	</list>
        /// </remarks>
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public string FilterRowEditorType
        {
            get
            {
                return this.GetRowFilterType();
            }
            set
            {
                filterRowEditorType = value;
            }
        }

        #endregion

        #region Callback

        /// <summary>
        /// Dependency call back for GridColumnSizer property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnGridColumnSizerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumn);
            if (column != null && column.ColumnPropertyChanged != null)
                column.ColumnPropertyChanged(column, "ColumnSizer");
        }

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/> dependency property value changed in the column.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnCellTemplateChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellTemplate");
        }

        internal override void OnSetCellBoundValueChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnSetCellBoundValueChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "SetCellBoundValue");
        }

        internal override void OnHeaderDataTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnHeaderDataTemplateChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "HeaderTemplate");
        }

        /// <summary>
        /// Dependency call back for FilterRowCellStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnFilterRowCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumn);
            if (column == null) return;
            column.hasFilterRowCellStyle = true;
            if (column.ColumnPropertyChanged != null)
                column.ColumnPropertyChanged(column, "FilterRowCellStyle");
        }

        internal override void OnCellStyleChanged()
        {
            base.OnCellStyleChanged();
#if WPF
            if(this.DataGrid != null && this.DataGrid.useDrawing)
              this.drawingTypeface = null;
#endif
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellStyle");
        }

        internal override void OnCellStyleSelectorChanged()
        {
            base.OnCellStyleSelectorChanged();
#if WPF
            if (this.DataGrid != null && this.DataGrid.useDrawing)
                this.drawingTypeface = null;
#endif
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellStyleSelector");
        }

        internal override void OnHeaderStyleChanged()
        {
            base.OnHeaderStyleChanged();
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "HeaderStyle");
        }

        internal override void OnWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "Width");
            //WPF-19593  while change width in code behind need to reset body if auto row height is used in grid
            if (this.DataGrid != null && this.DataGrid.CanQueryRowHeight() && this.DataGrid.VisualContainer != null)
            {
                this.DataGrid.VisualContainer.RowHeightManager.Reset();
            }
        }
        internal override void OnIsHiddenChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "IsHidden");
        }

        internal override void OnMaximumWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "MaximumWidth");
        }

        private static void OnAllowFilteringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumn);
            if (column != null && (column.ColumnPropertyChanged != null))
                column.ColumnPropertyChanged(column, "AllowFiltering");
        }

        private static void OnFilterRowOptionsVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumn);
            if (column != null && (column.ColumnPropertyChanged != null))
                column.ColumnPropertyChanged(column, "FilterRowOptionsVisibility");
        }

        internal override void OnAllowEditingChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.DataGrid == null || this.DataGrid.SelectionController == null)
                return;

            if (this.AllowEditing || !this.DataGrid.SelectionController.CurrentCellManager.HasCurrentCell)
                return;

            if (this.DataGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                this.DataGrid.SelectionController.CurrentCellManager.EndEdit();
        }

        internal override void OnMinimumWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "MinimumWidth");
        }

        internal override void OnColumnPropertyChanged(string property)
        {
            if (ColumnPropertyChanged != null)
                ColumnPropertyChanged(this, property);
        }

        #endregion

        #region Methods
        internal void SetGrid(SfDataGrid grid)
        {
            this.DataGrid = grid;
            this.UpdateBindingForValidation(grid == null ? GridValidationMode.None : grid.GridValidationMode);
        }
        /// <summary>
        /// Determines whether to increment and decrement the cell value in mouse wheel and up,down arrow key is pressed. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell value can be rotated using mouse wheel or up and down arrow key ; otherwise <b>false</b> .
        /// </returns>
        protected internal virtual bool CanAllowSpinOnMouseScroll()
        {
            return false;
        }

        /// <summary>
        /// Releases all resources used by the SfDataGrid and Columns.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                this.DataGrid = null;
                this.ColumnPropertyChanged = null;
                ClearValue(ColumnSizerProperty);
            }
            isdisposed = true;
        }

        internal void CheckFilterType(FilterRowCondition filterCondition)
        {
            //No need to check the FilterRowCondition when custom FilterRowEditorType is used. Hence the below filterRowEditorType condition is used.
            if (filterRowEditorType != "Default" || filterCondition == Grid.FilterRowCondition.Equals || filterCondition == Grid.FilterRowCondition.NotEquals
                || filterCondition == Grid.FilterRowCondition.NotNull || filterCondition == Grid.FilterRowCondition.Null)
                return;
            var rowFilterType = this.GetRowFilterType();
            if (filterCondition == Grid.FilterRowCondition.NotEmpty || filterCondition == Grid.FilterRowCondition.Empty
                || filterCondition == Grid.FilterRowCondition.EndsWith || filterCondition == Grid.FilterRowCondition.BeginsWith
                || filterCondition == Grid.FilterRowCondition.Contains)
            {
                if (rowFilterType != "TextBox")
                    throw new InvalidOperationException("Invaid FilterRowCondition for TextBox editor in " + this.MappingName + " Column");
            }
            else if (filterCondition == Grid.FilterRowCondition.Before || filterCondition == Grid.FilterRowCondition.BeforeOrEqual
                || filterCondition == Grid.FilterRowCondition.After || filterCondition == Grid.FilterRowCondition.AfterOrEqual)
            {
                if (rowFilterType != "DateTime")
                    throw new InvalidOperationException("Invaid FilterRowCondition for DateTime editor in " + this.MappingName + " Column");
            }
            else if (filterCondition == Grid.FilterRowCondition.GreaterThan || filterCondition == Grid.FilterRowCondition.GreaterThanOrEqual
                || filterCondition == Grid.FilterRowCondition.LessThan || filterCondition == Grid.FilterRowCondition.LessThanOrEqual)
            {
                if (rowFilterType != "Numeric")
                    throw new InvalidOperationException("Invaid FilterRowCondition for Numeric editor in " + this.MappingName + " Column");
            }
        }

        /// <summary>
        /// Gets the corresponding FilterBehavior for the particular column.
        /// </summary>
        /// <returns></returns>
        internal FilterBehavior GetFilterBehavior()
        {
            var rowFilterType = this.GetRowFilterType();
            if (rowFilterType != "TextBox")
                return Data.FilterBehavior.StronglyTyped;
            else
                return Data.FilterBehavior.StringTyped;
        }

        /// <summary>
        /// Gets the AdvancedFilterType of the parituclar column
        /// </summary>
        /// <returns>
        /// Returns the editor type which need to be load in GridFilterRowCell.
        /// </returns>
        protected internal virtual string GetRowFilterType()
        {
            if (this.DataGrid == null || !this.DataGrid.HasView)
                return string.Empty;
            Type columnType = null;

            if (this.filterRowEditorType != "Default")
                return this.filterRowEditorType;
            if (this.IsUnbound || (this.DataGrid.View.IsDynamicBound && this.ColumnMemberType == null))
                return "TextBox";
            else if (this.DataGrid.View.IsDynamicBound)
                columnType = this.ColumnMemberType;
            var pdc = this.DataGrid.View.GetItemProperties();
            if (pdc == null)
                return "TextBox";
            if (!this.DataGrid.View.IsDynamicBound)
            {
                var pd = pdc.GetPropertyDescriptor(this.MappingName);
                if (pd == null)
                    return "TextBox";
                columnType = pd.PropertyType;
            }
            if (columnType == typeof(int) || columnType == typeof(Double) || columnType == typeof(Decimal) || columnType == typeof(int?) || columnType == typeof(double?) || columnType == typeof(decimal?) || columnType == typeof(long) || columnType == typeof(long?) || columnType == typeof(uint) || columnType == typeof(uint?) || columnType == typeof(byte) || columnType == typeof(byte?) || columnType == typeof(float)
               || columnType == typeof(float?) || columnType == typeof(sbyte) || columnType == typeof(sbyte?) || columnType == typeof(ulong) || columnType == typeof(ulong?) || columnType == typeof(short) || columnType == typeof(short?) || columnType == typeof(ushort) || columnType == typeof(ushort?))
                return "Numeric";
#if WPF
            else if (columnType == typeof(DateTime) || columnType == typeof(DateTime?))
                return "DateTime";
#endif
            else if (columnType == typeof(bool) || columnType == typeof(bool?))
                return "CheckBox";
            else
                return "TextBox";
        }

        /// <summary>
        /// Gets the value that indicates the visibility of FilterOption button.
        /// </summary>
        /// <returns>
        /// Returns <b>Visible</b> if FilterOptions is required for the particular FilterRowCell.
        /// </returns>
        protected internal virtual Visibility GetFilterRowOptionsVisibility()
        {
            if (this.FilterRowEditorType == "ComboBox"
                || this.FilterRowEditorType == "MultiSelectComboBox"
                || this.FilterRowEditorType == "CheckBox")
                return Visibility.Collapsed;
            return this.FilterRowOptionsVisibility;
        }

        /// <summary>
        /// Updates the RowFilterType with the given value.
        /// </summary>
        /// <param name="_filterType"></param>
        internal void UpdateFilterType(FilterRowCondition _filterType)
        {
            this.FilterRowCondition = _filterType;
        }

        internal override void UpdateBindingInfo()
        {
            if (IsInSuspend)
                return;

            if (DataGrid != null)
                DataGrid.RowGenerator.UpdateBinding(this);
        }

        /// <summary>
        /// Sets the cell type which indicates the renderer for the column.
        /// </summary>
        /// <param name="cellType">
        /// Specifies the corresponding cell type of the column.
        /// </param>
        protected void SetCellType(string cellType)
        {
            this.CellType = cellType;
        }

        /// <summary>
        /// Determines whether the corresponding column can receive focus.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the column is loaded with editor in its <b>CellTemplate</b>.
        /// </returns>      
        protected internal virtual bool CanFocus()
        {
            return hasCellTemplate || hasCellTemplateSelector;
        }

        /// <summary>
        /// Determines whether the column is editable.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if all the cells with in the column is editable .
        /// </returns>
        protected internal virtual bool CanEditCell(int rowIndex = -1)
        {
            //WPF-28122 In some columns, we have to edit the cell in FilterRow, hence the new isFilterRow has been introduced.
            bool isFilterRow = this.DataGrid.IsFilterRowIndex(rowIndex);
            if (isFilterRow)
                return (this.FilterRowCondition != FilterRowCondition.Empty &&
                                        this.FilterRowCondition != FilterRowCondition.NotEmpty &&
                                        this.FilterRowCondition != FilterRowCondition.Null &&
                                        this.FilterRowCondition != FilterRowCondition.NotNull);
            else
                return true;
        }

#if WPF
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> class.
        /// </summary>
        /// <returns>
        /// Returns the new instance of column in SfDataGrid.
        /// </returns>
        protected override Freezable CreateInstanceCore()
        {
            if (this is GridTextColumn)
                return new GridTextColumn();
            else if (this is GridCheckBoxColumn)
                return new GridCheckBoxColumn();
            else if (this is GridTemplateColumn)
                return new GridTemplateColumn();
            else if (this is GridComboBoxColumn)
                return new GridComboBoxColumn();
            else if (this is GridCurrencyColumn)
                return new GridCurrencyColumn();
            else if (this is GridDateTimeColumn)
                return new GridDateTimeColumn();
            else if (this is GridHyperlinkColumn)
                return new GridHyperlinkColumn();
            else if (this is GridImageColumn)
                return new GridImageColumn();
            else if (this is GridMaskColumn)
                return new GridMaskColumn();
            else if (this is GridMultiColumnDropDownList)
                return new GridMultiColumnDropDownList();
            else if (this is GridNumericColumn)
                return new GridNumericColumn();
            else if (this is GridPercentColumn)
                return new GridPercentColumn();
            else if (this is GridTimeSpanColumn)
                return new GridTimeSpanColumn();
            throw new NotImplementedException();
        }

#endif
        internal override void UpdateValidationMode()
        {
            this.UpdateBindingForValidation(this.GridValidationMode);

            if (this.DataGrid == null || DataGrid.VisualContainer == null)
                return;
            foreach (var item in DataGrid.VisualContainer.RowsGenerator.Items)
            {
                var row = item as DataRowBase;
                if (row.RowType != RowType.DefaultRow && row.RowType != RowType.AddNewRow)
                    continue;
                var dataColumn = row.VisibleColumns.FirstOrDefault(column => column.GridColumn != null && column.GridColumn.MappingName == this.MappingName);
                if (dataColumn == null)
                    continue;
                if (this.GridValidationMode == GridValidationMode.None)
                {
                    (dataColumn.ColumnElement as GridCell).RemoveError();
                }
                else
                {
                    DataGrid.ValidationHelper.ValidateColumn(row.RowData, MappingName, dataColumn.ColumnElement as GridCell, new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex));
                }
                dataColumn.UpdateBinding(row.RowData);
            }
        }

        /// <summary>
        /// Checks whether the column is boolean type or not.
        /// </summary>
        /// <remarks>
        /// override this method in corresponding boolean column and returns true.
        /// </remarks>
        /// <returns>false</returns>
        protected internal virtual bool CanEndEditColumn()
        {
            return false;
        }

        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            if (this.ValueBinding != null)
            {
                BindingBase bind = this.ValueBinding.CreateEditBinding(this.GridValidationMode != Grid.GridValidationMode.None, this);
                this.SetValueBinding(true, bind);
            }
        }

        internal override double GetFilterIconWidth()
        {
            if (AllowFiltering)
                return DataGrid.GridColumnSizer.FilterIconWidth;
            return 0;
        }

        /// <summary>
        /// Set the FilterRowText while applying the Filtered from FilterRow
        /// </summary>
        /// <param name="filterRowText">FilterText of that column</param>
        protected internal virtual void SetFilterRowText(string filterRowText)
        {
            this.FilterRowText = filterRowText;
        }
        #endregion

        /// <summary>
        /// Invoked whenever the value of any dependency property in the column has been updated.
        /// </summary>
        /// <param name="propertyName">
        /// The property name that has changed in column.
        /// </param>
        /// <param name="e">
        /// <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains the data for various dependency property changed events.
        /// </param>
        public void OnDependencyPropertyChanged(string propertyName, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataGrid == null) return;
            var index = this.DataGrid.Columns.IndexOf(this);
            // Below code is added to skip the crash, while opening GridMultiColumnDropDownList in DetailsViewDataGrid
            if (index == -1)
                return;
            // Need to get NotifyListener from SourceDataGrid and call NotifyPropertyChanged method
            if (this.DataGrid.NotifyListener != null)
                this.DataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyPropertyChanged(this, propertyName, e, datagrid => datagrid.Columns[index], DataGrid, typeof(GridColumn));
        }
    }

    internal delegate void ColumnPropertyChanged(GridColumn column, string property);
    /// <summary>
    /// Represents the collection of GridColumn.
    /// </summary>
    [ClassReference(IsReviewed = false)]
#if WPF

    public class Columns : FreezableCollection<GridColumn>, IDisposable
#else
    public class Columns : ObservableCollection<GridColumn>, IDisposable
#endif
    {
        // flag used to suspend/resume the UI update in Grid Column collection changed in DataGrid.
        internal bool suspendUpdate = false;
        private bool isdisposed = false;
#if WPF
        /// <summary>
        /// Initializes a new instance of columns class.
        /// </summary>
        /// <returns>Returns the new instance of column collection.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new Columns();
        }
#endif
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the Columns class. 
        /// </summary>        
        public Columns()
        {

        }

        #endregion

        #region Property
        /// <summary>
        /// Gets or sets the column at the specified mapping name .
        /// </summary>
        /// <param name="mappingName">
        /// The mapping name of the column.
        /// </param>
        /// <returns>
        /// Returns the column with corresponding to its mapping name.
        /// </returns>
        public GridColumn this[string mappingName]
        {
            get
            {
                var column = this.FirstOrDefault(col => col.MappingName == mappingName);
                return column;
            }
        }

        #endregion

        /// <summary>
        /// Suspends the UI refresh when the columns are being added or removed.
        /// </summary>
        public void Suspend()
        {
            this.suspendUpdate = true;
        }

        /// <summary>
        /// Resumes the UI refresh when the columns are being added or removed.
        /// </summary>
        /// <remarks>
        /// Update columns by calling <see cref="Syncfusion.UI.Xaml.Grid.Helpers.GridHelper.RefreshColumns"/> method when the column updates are resumed.
        /// </remarks>
        public void Resume()
        {
            this.suspendUpdate = false;
        }

        /// <summary>
        /// Disposes the memory of all columns used by the <see cref="Syncfusion.UI.Xaml.Grid.Columns"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.Columns"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                foreach (var item in this)
                {
#if WPF
                    item.drawingTypeface = null;
#endif
                    item.Dispose();
                }
                this.Clear();
            }
            isdisposed = true;
        }
    }

    /// <summary>
    /// Represents a column that contains template-specified cell content
    /// </summary>
    public class GridTemplateColumn : GridTextColumnBase
    {
        internal bool hasEditTemplate = false;
        internal bool hasEditTemplateSelector = false;
        /// <summary>
        /// Gets or sets the horizontal alignment for the column .
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.HorizontalAlignment"/> enumeration that specifies the horizontal alignment of the column.
        /// The default value is  <see cref="System.Windows.HorizontalAlignment">Stretch</see>.
        /// </value>
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTemplateColumn.HorizontalAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTemplateColumn.HorizontalAlignment dependency property.
        /// </remarks>        
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            GridDependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(GridTemplateColumn), new GridPropertyMetadata(HorizontalAlignment.Stretch));

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> to load in editing mode.
        /// </summary>
        /// <value>
        /// The template that is used to display the contents of cell in a column that is in editing mode. The default is <b>null</b>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.SetCellBoundValue"/> decides whether the data context of the <b>EditTemplate</b> is based on Record or <see cref="DataContextHelper"/> class.        
        /// By default, Record will be the DataContext for template. If SetCellBoundValue is true, <see cref="DataContextHelper"/> will be the data context.        
        ///</remarks>
        public DataTemplate EditTemplate
        {
            get { return (DataTemplate)GetValue(EditTemplateProperty); }
            set { SetValue(EditTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplate dependency property.
        /// </remarks>
        public static readonly DependencyProperty EditTemplateProperty =
            GridDependencyProperty.Register("EditTemplate", typeof(DataTemplate), typeof(GridTemplateColumn), new GridPropertyMetadata(null, OnEditTemplateChanged));

        /// <summary>
        /// Dependency call back for EditTemplate property.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.EditTemplate"/> dependency property value changed in the GridTemplateColumn.
        /// </summary>
        /// <param name="d">The <c>DependencyObject</c> that contains the GridTemplateColumn.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains the data for the <b>EditTemplate</b> property changes.</param>
        public static void OnEditTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridTemplateColumn);
            if (column != null)
            {
                column.hasEditTemplate = e.NewValue != null;

                if (column.ColumnPropertyChanged != null)
                    column.ColumnPropertyChanged(column, "EditTemplate");
            }
        }

        /// <summary>        
        /// Gets or sets the  <see cref="System.Windows.DataTemplate"/>  by choosing a template based on bound data objects and data-bound element in editing mode.        
        /// </summary>
        /// <value>
        /// A custom <see cref="System.Windows.Controls.DataTemplateSelector"/> object that provides logic and returns a <see cref="System.Windows.DataTemplate"/> that is in edit mode of column. The default is null.
        /// </value>   
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplate"/>
        public DataTemplateSelector EditTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(EditTemplateSelectorProperty); }
            set { SetValue(EditTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplateSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplateSelector dependency property.
        /// </remarks>        
        public static readonly DependencyProperty EditTemplateSelectorProperty =
            GridDependencyProperty.Register("EditTemplateSelector", typeof(DataTemplateSelector), typeof(GridTemplateColumn), new GridPropertyMetadata(null, OnEditTemplateSelectorChanged));

        public static void OnEditTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridTemplateColumn);
            if (column != null)
            {
                column.hasEditTemplateSelector = e.NewValue != null;

                if (column.ColumnPropertyChanged != null)
                    column.ColumnPropertyChanged(column, "EditTemplateSelector");
            }
        }

        /// <summary>
        /// Determines whether the GridTemplateColumn can receive focus.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the column is loaded with editor in its <b>CellTemplate</b>.
        /// </returns>
        protected internal override bool CanFocus()
        {
            return (this.hasCellTemplate || this.hasCellTemplateSelector || (this.DataGrid != null && this.DataGrid.hasCellTemplateSelector)) && !(this.hasEditTemplate || this.hasEditTemplateSelector);
        }

        /// <summary>
        /// Determines whether the cells in GridTemplateColumn can be edited. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the template column has loaded with <see cref="Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplate"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridTemplateColumn.EditTemplateSelector"/>. 
        /// If the GridTemplateColumn loaded with <see cref="Syncfusion.UI.Xaml.Grid.GridTemplateColumn.CellTemplate"/> , returns <b>false</b>.
        /// </returns>
        protected internal override bool CanEditCell(int rowIndex = -1)
        {
            bool isFilterRow = this.DataGrid.IsFilterRowIndex(rowIndex);
            return isFilterRow || (this.hasEditTemplate || this.hasEditTemplateSelector);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridTemplateColumn"/> class.
        /// </summary>
        public GridTemplateColumn()
        {
            this.CellType = "Template";
            IsTemplate = true;
        }

#region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridTemplateColumn.
        /// </summary>              
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            if (this.hasEditTemplate || this.hasEditTemplateSelector)
                base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> of GridTemplateColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/>.
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {

        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            
        }

#endregion
    }

    /// <summary>
    /// Provides the base implementation of text formatting in the column.
    /// </summary>
    public abstract class GridTextColumnBase : GridColumn
    {
        /// <summary>
        /// Gets or sets the text trimming to apply when the cell content overflows the content area. 
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.TextTrimming"/> values that specifies the text trimming behavior of cell content. The default value is <see cref="System.Windows.TextTrimming.None"/>.
        /// </value>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTextColumnBase.TextTrimming dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTextColumnBase.TextTrimming dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextTrimmingProperty =
            GridDependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(GridTextColumnBase), new GridPropertyMetadata(TextTrimming.None, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates how cell content should wrap the text in the column.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.TextWrapping"/> enumeration that specifies wrapping behavior of cell content. 
        /// The default value is <see cref="System.Windows.TextWrapping.NoWrap"/>.
        /// </value>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTextColumnBase.TextWrapping dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTextColumnBase.TextWrapping dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextWrappingProperty =
#if WPF
 GridDependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(GridTextColumnBase), new GridPropertyMetadata(TextWrapping.NoWrap, OnUpdateBindingInfo));
#else
        GridDependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(GridTextColumnBase), new GridPropertyMetadata(TextWrapping.Wrap, OnUpdateBindingInfo));
#endif

#if WPF
        /// <summary>
        /// Gets or sets a <see cref="System.Windows.TextDecorationCollection"/> that contains text decorations to apply on the cell content of the column.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.TextDecorationCollection"/> enumeration that contains text decorations to apply to the cell content of the column.
        /// </value>
        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTextColumnBase.TextDecorations dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTextColumnBase.TextDecorations dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextDecorationsProperty =
            GridDependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(GridTextColumnBase), new GridPropertyMetadata(new TextDecorationCollection(), OnUpdateBindingInfo));
#endif
        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> of column.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> .
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {
#if WPF
            if (!isDisplayMultiBinding)
#endif
                if ((DisplayBinding as Binding).Converter == null)
                    (DisplayBinding as Binding).Converter = new CultureFormatConverter(this);
        }

        internal override void OnUpdateBindingInfo(DependencyPropertyChangedEventArgs e)
        {
            this.textTrimming = this.TextTrimming;
            this.textWrapping = this.TextWrapping;
#if WPF
            this.textDecoration = this.TextDecorations;
#endif
            base.OnUpdateBindingInfo(e);
        }
    }
    /// <summary>
    /// Represents a column that  is used to display the string content in its cells and host TextBox in edit mode.
    /// </summary>
    public class GridTextColumn : GridTextColumnBase
    {
#if UWP
        /// <summary>
        /// Gets or sets that indicates whether the spell check is enabled or not.
        /// </summary>
        public bool IsSpellCheckEnabled
        {
            get { return (bool)GetValue(IsSpellCheckEnabledProperty); }
            set { SetValue(IsSpellCheckEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTextColumn.IsSpellCheckEnabled dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTextColumn.IsSpellCheckEnabled dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsSpellCheckEnabledProperty =
            GridDependencyProperty.Register("IsSpellCheckEnabled", typeof(bool), typeof(GridTextColumn), new GridPropertyMetadata(false));
#endif

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridTextColumn"/> class.
        /// </summary>
        public GridTextColumn()
        {
            this.CellType = "TextBox";
#if UWP
            this.Padding = new Thickness(3, 1, 2, 0);
#endif
        }

#region overrides     
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridTextColumn.
        /// </summary>             
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }
        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> of GridTextColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> .
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {

        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(0 + padLeft, padTop, 0 + padRight, padBotton)
                           : new Thickness(2, 2, 2, 2);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                          ? new Thickness(3 + padLeft, 1 + padTop, 3 + padRight, 1 + padBotton)
                          : new Thickness(3, 1, 3, 1);
#endif
        }
#endregion
    }

    /// <summary>
    /// Represents a column that display enumeration as its cell content.
    /// </summary>
    public class GridMultiColumnDropDownList : GridTextColumnBase
    {
#region Internal Properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal object itemsSource = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string displayMemberPath = string.Empty;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string valueMemberPath = string.Empty;

#endregion

#region Public Properties
        /// <summary>
        /// Default minimum height of the pop-up.
        /// </summary>
        public const double DefaultPopupMinHeight = 300.0;
        /// <summary>
        /// Default minimum width of the pop-up.
        /// </summary>
        public const double DefaultPopupMinWidth = 400.0;

#endregion

#region Dependency Properties
        /// <summary>
        /// Gets or sets a string that specifies the name of data member to represent its value to the display mode of the SfMultiColumnDropDownControl .
        /// </summary>
        /// <value>
        /// A string that specifies the name of data member to be displayed in the SfMultiColumnDropDownControl . 
        /// The default value is <b>string.Empty</b> .
        /// </value>        
        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }
            set { SetValue(DisplayMemberProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.DisplayMember dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.DisplayMember dependency property.
        /// </remarks>
        public static readonly DependencyProperty DisplayMemberProperty =
            GridDependencyProperty.Register("DisplayMember", typeof(string), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that specifies the name of data member to display its values to the drop-down list of SfMultiColumnDropDownControl.
        /// </summary>
        /// <value>
        /// A string that specifies the name of data member to display its values in to the drop-down list. The default value is <b>string.Empty</b> .
        /// </value>        
        public string ValueMember
        {
            get { return (string)GetValue(ValueMemberProperty); }
            set { SetValue(ValueMemberProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ValueMember dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ValueMember dependency property.
        /// </remarks>
        public static readonly DependencyProperty ValueMemberProperty =
            GridDependencyProperty.Register("ValueMember", typeof(string), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the resizing cursor is visible at the edge of the drop-down pop-up.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies the resizing cursor is enabled ; The default value is <b> Visibility.Visible</b> .
        /// </value>       
        public Visibility ShowResizeThumb
        {
            get { return (Visibility)GetValue(ShowResizeThumbProperty); }
            set { SetValue(ShowResizeThumbProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ShowResizeThumb dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ShowResizeThumb dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowResizeThumbProperty =
            GridDependencyProperty.Register("ShowResizeThumb", typeof(Visibility), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets the height of the drop-down pop-up.
        /// </summary>
        /// <value>
        /// The height of the drop-down pop-up . The <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMinHeight"/> is set as default height of drop-down popup.
        /// </value>     
        /// <remarks>
        /// The pop-up adjusts the height based on the <c>PopUpHeight</c> when its value greater than the <c>PopUpMinHeight</c> and lesser than the <c>PopUpMaxHeight</c>.
        /// </remarks>
        public double PopUpHeight
        {
            get { return (double)GetValue(PopUpHeightProperty); }
            set { SetValue(PopUpHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopUpHeightProperty =
            GridDependencyProperty.Register("PopUpHeight", typeof(double), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(DefaultPopupMinHeight));

        /// <summary>
        /// Gets or sets the width of the drop-down pop-up.
        /// </summary>
        /// <value>
        /// The width of the drop-down popup.The <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMinWidth"/> is set as default width of drop-down popup.
        /// </value>
        /// <remarks>
        /// The pop-up adjusts the width based on the <c>PopUpWidth</c> when its value greater than the <c>PopUpMinWidth</c> and lesser than the <c>PopUpMaxWidth</c>.
        /// </remarks>
        public double PopUpWidth
        {
            get { return (double)GetValue(PopUpWidthProperty); }
            set { SetValue(PopUpWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopUpWidthProperty =
            GridDependencyProperty.Register("PopUpWidth", typeof(double), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(DefaultPopupMinWidth));

        /// <summary>
        /// Gets or sets the collection that is used to generate the content of the GridMultiColumnDropDownList column.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of the GridMultiColumnDropDownList column.
        /// </value>        
        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ItemsSource dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ItemsSourceProperty =
            GridDependencyProperty.Register("ItemsSource", typeof(object), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value to complete the user input rather than typing the entire entry from the SfMultiColumnDropDwonControl.
        /// </summary>
        /// <value>
        /// <b> true </b> if the auto completion is activated ; otherwise , <b> false</b>.
        /// The default value is <b>true</b>.
        /// </value>        
        public bool AllowAutoComplete
        {
            get { return (bool)GetValue(AllowAutoCompleteProperty); }
            set { SetValue(AllowAutoCompleteProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowAutoComplete dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowAutoComplete dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowAutoCompleteProperty =
            GridDependencyProperty.Register("AllowAutoComplete", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in the GridMultiColumnDropDownList column.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullInput
        {
            get { return (bool)GetValue(AllowNullInputProperty); }
            set { SetValue(AllowNullInputProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowNullInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowAutoComplete dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullInputProperty =
            GridDependencyProperty.Register("AllowNullInput", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the columns should be created automatically.
        /// </summary>
        /// <value> 
        /// <b>true</b> if the columns are automatically generated; otherwise , <b>false</b>. The default value is <b>true</b>.
        /// </value>  
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumns"/>
        public bool AutoGenerateColumns
        {
            get { return (bool)GetValue(AutoGenerateColumnsProperty); }
            set { SetValue(AutoGenerateColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AutoGenerateColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AutoGenerateColumns dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            GridDependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates how the columns are generated during automatic column generation.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoGenerateColumnsMode"/> enumeration that specifies the mode of automatic column generation.The default value is <b>Syncfusion.UI.Xaml.SfDataGrid.AutoGenerateColumnsMode.None</b>.
        /// </value> 
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumnsMode"/>
        public AutoGenerateColumnsMode AutoGenerateColumnsMode
        {
            get { return (AutoGenerateColumnsMode)GetValue(AutoGenerateColumnsModeProperty); }
            set { SetValue(AutoGenerateColumnsModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AutoGenerateColumnsMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AutoGenerateColumnsMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoGenerateColumnsModeProperty =
            GridDependencyProperty.Register("AutoGenerateColumnsMode", typeof(AutoGenerateColumnsMode), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(AutoGenerateColumnsMode.Reset));

        /// <summary>
        /// Gets or sets the collection that contains all the columns in the GridMultiColumnDropDownList column.
        /// </summary>
        /// <value>
        /// The collection that contains all the columns in the GridMultiColumnDropDownList column.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.Columns"/>
        public Columns Columns
        {
            get { return (Columns)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.Columns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.Columns dependency property.
        /// </remarks>
        public static readonly DependencyProperty ColumnsProperty =
            GridDependencyProperty.Register("Columns", typeof(Columns), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(new Columns()));

        /// <summary>
        /// Gets or sets a value that indicates how the width of GridMultiColumnDropDownList is determined.
        /// </summary>       
        /// <value>
        /// One of the enumeration in <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType"/>that adjust the column
        /// width.The default value is
        /// <b>Syncfusion.UI.Xaml.Grid.GridLengthUnitType.None</b>.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridColumnSizer"/>
        public GridLengthUnitType GridColumnSizer
        {
            get { return (GridLengthUnitType)GetValue(GridColumnSizerProperty); }
            set { SetValue(GridColumnSizerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.GridColumnSizer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.GridColumnSizer dependency property.
        /// </remarks>
        public static readonly DependencyProperty GridColumnSizerProperty =
            GridDependencyProperty.Register("GridColumnSizer", typeof(GridLengthUnitType), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(GridLengthUnitType.None));

        /// <summary>
        /// Gets or sets value that indicates whether the pop-up size is adjusted automatically based on its content.
        /// </summary>
        /// <value>
        /// <b>true</b> if the pop-up size is adjusted based on its content ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool IsAutoPopupSize
        {
            get { return (bool)GetValue(IsAutoPopupSizeProperty); }
            set { SetValue(IsAutoPopupSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsAutoPopupSize dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsAutoPopupSize dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsAutoPopupSizeProperty =
            GridDependencyProperty.Register("IsAutoPopupSize", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(true));

        /// <summary>        
        /// Gets or sets a value that indicates whether the user can change the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell values of GridMultiColumnDropDownList column can be changed; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>        
        public bool AllowSpinOnMouseWheel
        {
            get { return (bool)GetValue(AllowSpinOnMouseWheelProperty); }
            set { SetValue(AllowSpinOnMouseWheelProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowSpinOnMouseWheel dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowSpinOnMouseWheel dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowSpinOnMouseWheelProperty =
            GridDependencyProperty.Register("AllowSpinOnMouseWheel", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can filter the values from drop-down grid dynamically being characters entered on the cell in GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can filter the values from drop-down grid dynamically being characters entered on the cell ; otherwise , <b> false</b> .
        /// The default value is true. 
        /// </value>
        /// <remarks>Records are filtered based on <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition"/> when <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering"/> is enabled.
        /// </remarks>
        public bool AllowIncrementalFiltering
        {
            get { return (bool)GetValue(AllowIncrementalFilteringProperty); }
            set
            {
                SetValue(AllowIncrementalFilteringProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowIncrementalFilteringProperty =
            GridDependencyProperty.Register("AllowIncrementalFiltering", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value for search condition when <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering"/> is enabled.
        /// </summary>
        /// <value>A value of the enumeration <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition"/>.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition.StartsWith"/>.
        /// <remarks>Records are filtered based on <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition"/> when <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering"/> is enabled.
        /// </remarks>
        public SearchCondition SearchCondition
        {
            get { return (SearchCondition)GetValue(SearchConditionProperty); }
            set { SetValue(SearchConditionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.SearchCondition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.SearchCondition dependency property.
        /// </remarks>
        public static readonly DependencyProperty SearchConditionProperty =
            DependencyProperty.Register("SearchCondition", typeof(SearchCondition), typeof(GridMultiColumnDropDownList), new PropertyMetadata(SearchCondition.StartsWith));

        /// <summary>
        /// Gets or sets a value that indicates whether to enable the case-sensitive during <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowIncrementalFiltering"/> in GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// <b>true</b> if the case-sensitive is enabled during incremental filtering is applied in GridMultiColumnDropDownList; otherwise, <b>false</b> .
        /// The default value is <b>false</b>.
        /// </value>        
        public bool AllowCasingforFilter
        {
            get { return (bool)GetValue(AllowCasingforFilterProperty); }
            set { SetValue(AllowCasingforFilterProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowCasingforFilter dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.AllowCasingforFilter dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowCasingforFilterProperty =
            GridDependencyProperty.Register("AllowCasingforFilter", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the upper bound for the pop-up height of GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// The maximum height constraint of the pop-up in GridMultiColumnDropDownList.
        /// </value>        
        public double PopUpMaxHeight
        {
            get { return (double)GetValue(PopUpMaxHeightProperty); }
            set { SetValue(PopUpMaxHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMaxHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMaxHeight dependency property.
        /// </remarks>        
        public static readonly DependencyProperty PopUpMaxHeightProperty =
            GridDependencyProperty.Register("PopUpMaxHeight", typeof(double), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(double.MaxValue));

        /// <summary>
        /// Gets or sets the upper bound for the pop-up width of GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// The maximum width constraint of the pop-up in GridMultiColumnDropDownList.
        /// </value>        
        public double PopUpMaxWidth
        {
            get { return (double)GetValue(PopUpMaxWidthProperty); }
            set { SetValue(PopUpMaxWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMaxWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMaxWidth dependency property.
        /// </remarks>        
        public static readonly DependencyProperty PopUpMaxWidthProperty =
            GridDependencyProperty.Register("PopUpMaxWidth", typeof(double), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(double.MaxValue));

        /// <summary>
        /// Gets or sets the lower bound for the pop-up height of GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// The minimum height constraint of the pop-up in GridMultiColumnDropDownList. The default value is 300.0.
        /// </value>        
        public double PopUpMinHeight
        {
            get { return (double)GetValue(PopUpMinHeightProperty); }
            set { SetValue(PopUpMinHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMinHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMinHeight dependency property.
        /// </remarks>        
        public static DependencyProperty PopUpMinHeightProperty =
            GridDependencyProperty.Register("PopUpMinHeight", typeof(double), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(DefaultPopupMinHeight));

        /// <summary>
        /// Gets or sets the lower bound for the pop-up width of GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// The minimum width constraint of the pop-up in GridMultiColumnDropDownList. The default value is 200.0 .
        /// </value>        
        public double PopUpMinWidth
        {
            get { return (double)GetValue(PopUpMinWidthProperty); }
            set { SetValue(PopUpMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMinWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.PopUpMinWidth dependency property.
        /// </remarks>
        public static DependencyProperty PopUpMinWidthProperty =
            GridDependencyProperty.Register("PopUpMinWidth", typeof(double), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(DefaultPopupMinWidth - 200));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can enter the value in the editor of GridMultiColumnDropDownList.
        /// </summary>
        /// <value>
        /// <b>true</b> if the editor of GridMultiColumnDropDownList is read-only ; otherwise , <b>false</b>. 
        /// The default value is <b>false</b>.
        /// </value>
        public bool IsTextReadOnly
        {
            get { return (bool)GetValue(IsTextReadOnlyProperty); }
            set { SetValue(IsTextReadOnlyProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsTextReadOnly dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsTextReadOnly dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsTextReadOnlyProperty = GridDependencyProperty.Register("IsTextReadOnly", typeof(bool), typeof(GridMultiColumnDropDownList), new GridPropertyMetadata(false));
#endregion

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> of GridMultiColumnDropDownList column.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/>.
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {
#if WPF
            if (!isDisplayMultiBinding)
#endif
                if ((DisplayBinding as Binding).Converter == null)
                {
                    (DisplayBinding as Binding).Converter = new DisplayMemberConverter(this);
                }
        }

#region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList"/> class. 
        /// </summary>
        public GridMultiColumnDropDownList()
        {
            this.CellType = "MultiColumnDropDown";
            IsDropDown = true;
        }
#endregion

#region Overrides

        /// <summary>
        /// Determines whether the cell value is rotated using mouse wheel or up and down arrow key is pressed. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell value can be rotated using mouse wheel or up and down arrow key ; otherwise <b>false</b> .
        /// </returns>
        protected internal override bool CanAllowSpinOnMouseScroll()
        {
            return this.AllowSpinOnMouseWheel;
        }

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in GridMultiColumnDropDownList column.
        /// </summary>            
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;
            this.itemsSource = this.ItemsSource;
            this.displayMemberPath = this.DisplayMember;
            this.valueMemberPath = this.ValueMember;
            base.UpdateBindingInfo();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 3 + padTop, 25 + padRight, padBotton)
                           : new Thickness(3, 1, 2, 0);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, padTop, 3 + padRight, padBotton)
                           : new Thickness(3, 0, 3, 0);
#endif
        }

#endregion
    }

    /// <summary>
    /// Represents a column that host the ComboBox and enumeration as its cell content in edit mode.
    /// </summary>
    public class GridComboBoxColumn : GridColumn
    {
#region Internal Properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal IEnumerable itemsSource = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string displayMemberPath = string.Empty;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string valueMemberPath = null;

#endregion

        /// <summary>
        /// Gets or sets the path that is used to get the SelectedValue from the SelectedItem.
        /// </summary>
        /// <value>
        /// The path that is used to get the SelectedValue from the SelectedItem.
        /// </value>
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.SelectedValuePath dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.SelectedValuePath dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectedValuePathProperty =
            GridDependencyProperty.Register("SelectedValuePath", typeof(string), typeof(GridComboBoxColumn), new GridPropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a collection used to generate the content of GridComboBoxColumn.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of GridComboBoxColumn. The default value is null.
        /// </value>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ItemsSource dependency property.
        /// </remarks>
        public static readonly DependencyProperty ItemsSourceProperty =
            GridDependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(GridComboBoxColumn), new GridPropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the path that is used to display the visual representation of object.
        /// </summary>
        /// <value>
        /// A string that represents the path to display the visual representation of object.
        /// </value>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.DisplayMemberPath dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.DisplayMemberPath dependency property.
        /// </remarks>        
        public static readonly DependencyProperty DisplayMemberPathProperty =
            GridDependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(GridComboBoxColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));
#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether a GridComboBoxColumn that opens and displays a drop-down control when a user clicks its text area .
        /// </summary>
        /// <value>
        /// <b>true</b> to keep the drop-down control open when the user clicks on the text area to start editing; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool StaysOpenOnEdit
        {
            get { return (bool)GetValue(StaysOpenOnEditProperty); }
            set { SetValue(StaysOpenOnEditProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.StaysOpenOnEdit dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.StaysOpenOnEdit dependency property.
        /// </remarks>
        public static readonly DependencyProperty StaysOpenOnEditProperty =
            GridDependencyProperty.Register("StaysOpenOnEdit", typeof(bool), typeof(GridComboBoxColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can edit the cell value by typing through editor of GridComboBoxColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is edited by typing through the editor of GridComboBoxColumn ; otherwise , <b>false</b>. 
        /// The default value is <b>false</b> .
        /// </value>
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsEditable dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsEditable dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsEditableProperty =
            GridDependencyProperty.Register("IsEditable", typeof(bool), typeof(GridComboBoxColumn), new GridPropertyMetadata(false));
#endif
        /// <summary>
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> that is used to display each item in GridComboBoxColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.DataTemplate"/> that is used to display each item in GridComboBoxColumn.
        /// The default value is null.
        /// </value>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ItemTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.ItemTemplate dependency property.
        /// </remarks>
        public static readonly DependencyProperty ItemTemplateProperty =
            GridDependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(GridComboBoxColumn), new GridPropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> of GridComboBoxColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> .
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {
#if WPF
            if (!isDisplayMultiBinding)
#endif
                if ((DisplayBinding as Binding).Converter == null)
                    (DisplayBinding as Binding).Converter = new DisplayMemberConverter(this);


        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridComboBoxColumn"/> class.
        /// </summary>
        public GridComboBoxColumn()
        {
            this.CellType = "ComboBox";
#if WPF
            Padding = new Thickness(4, 2, 4, 2);
#endif
            IsDropDown = true;
        }

#region overrides

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridComboBoxColumn.
        /// </summary>           
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;
            this.itemsSource = this.ItemsSource;
            this.displayMemberPath = this.DisplayMemberPath;
            this.valueMemberPath = this.SelectedValuePath;
            base.UpdateBindingInfo();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if WinRT || UNIVERSAL
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(7.5 + padLeft, padTop, 2 + padRight, padBotton)
                           : new Thickness(7.5, 1, 2, 0);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(1 + padLeft, 0 + padTop, 1 + padRight, 0 + padBotton)
                           : new Thickness(1, 0, 1, 0);
#endif
        }

#endregion
    }
    /// <summary>
    /// Represents a column that used to display and edit boolean values and hosts CheckBox as its cell content.
    /// </summary>
    public class GridCheckBoxColumn : GridColumn
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn"/> class.
        /// </summary>
        public GridCheckBoxColumn()
        {
            this.CellType = "CheckBox";
        }

        /// <summary>
        /// Gets or sets the horizontal alignment for the column .
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.HorizontalAlignment"/> enumeration that specifies the horizontal alignment. The default value is <b>HorizontalAlignment.Stretch)</b>
        /// </value>
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.HorizontalAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.HorizontalAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            GridDependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(GridCheckBoxColumn), new GridPropertyMetadata((HorizontalAlignment.Center), OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can enable the Intermediate state of the CheckBox other than the Checked and Unchecked state.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Intermediate state is enabled in GridCheckBoxColumn; otherwise <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool IsThreeState
        {
            get { return (bool)GetValue(IsThreeStateProperty); }
            set { SetValue(IsThreeStateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.IsThreeState dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.IsThreeState dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsThreeStateProperty =
            GridDependencyProperty.Register("IsThreeState", typeof(bool), typeof(GridCheckBoxColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));

#region overrides

        internal override void OnAllowEditingChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateBindingInfo();
            base.OnAllowEditingChanged(e);
        }
        protected internal override bool CanEndEditColumn()
        {
            return true;
        }

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/> dependency property value changed in the GridCheckBoxColumn.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            throw new NotSupportedException("The " + this.ToString() + " does not implement CellTemplate property");
        }

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridCheckBoxColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            
        }

#endregion
    }

    /// <summary>
    /// Represents a column that displays the image in its cell content.
    /// </summary>   
    public class GridImageColumn : GridColumn
    {
        /// <summary>
        /// Gets or sets a value that specifies how an <see cref=" System.Windows.Controls.Image"/>
        ///  can be stretched to fill the destination rectangle.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Media.Stretch"/> enumeration that specifies how the image is stretched.
        /// The default value is <b>Stretch.Uniform</b>.
        /// </value>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.Stretch dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.Stretch dependency property.
        /// </remarks>
        public static readonly DependencyProperty StretchProperty =
            GridDependencyProperty.Register("Stretch", typeof(Stretch), typeof(GridImageColumn), new GridPropertyMetadata(Stretch.Uniform, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates how the image is scaled.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Controls.StretchDirection"/> values. The default value is <b>StretchDirection.Both</b>.
        /// </value>
        public StretchDirection StretchDirection
        {
            get { return (StretchDirection)GetValue(StretchDirectionProperty); }
            set { SetValue(StretchDirectionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.StretchDirection dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn.StretchDirection dependency property.
        /// </remarks>        
        public static readonly DependencyProperty StretchDirectionProperty =
            GridDependencyProperty.Register("StretchDirection", typeof(StretchDirection), typeof(GridImageColumn), new GridPropertyMetadata(StretchDirection.Both, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the width of the image.
        /// </summary>
        /// <value>
        /// The width of the image. The default value positive infinity.
        /// </value>
        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridImageColumn.ImageWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridImageColumn.ImageWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty ImageWidthProperty =
            GridDependencyProperty.Register("ImageWidth", typeof(double), typeof(GridImageColumn), new GridPropertyMetadata(double.PositiveInfinity, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the height of the image.
        /// </summary>
        /// <value>
        /// The height of the image. The default value is positive infinity.
        /// </value>
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridImageColumn.ImageHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridImageColumn.ImageHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty ImageHeightProperty =
            GridDependencyProperty.Register("ImageHeight", typeof(double), typeof(GridImageColumn), new GridPropertyMetadata(double.PositiveInfinity, OnUpdateBindingInfo));

#region overrides
        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/> dependency property value changed in GridImageColumn.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            throw new NotSupportedException("The " + this.ToString() + " does not implement CellTemplate property");
        }

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in GridImageColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {

        }

        /// <summary>
        /// Determines whether the cells in GridImageColumn can be edited.
        /// </summary>
        /// <returns>
        /// Returns <b>false</b> for GridImageColumn.
        /// </returns>
        protected internal override bool CanEditCell(int rowIndex = -1)
        {
            return this.DataGrid.IsFilterRowIndex(rowIndex);
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            
        }

#endregion
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridImageColumn"/> class.
        /// </summary>
        public GridImageColumn()
        {
            this.CellType = "Image";
        }
    }

#if WPF
    /// <summary>
    /// Provides the base implementation for all the editor columns in the SfDataGrid.
    /// </summary>
    public abstract class GridEditorColumn : GridTextColumnBase
    {
#region Internal Proeprties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal decimal minValue = decimal.MinValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal decimal maxValue = decimal.MaxValue;

#endregion

#region Dependency Properties
        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is changed using the mouse wheel or up and down arrow key; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>
        public bool AllowScrollingOnCircle
        {
            get { return (bool)GetValue(AllowScrollingOnCircleProperty); }
            set { SetValue(AllowScrollingOnCircleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.AllowScrollingOnCircle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.AllowScrollingOnCircle dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowScrollingOnCircleProperty =
            GridDependencyProperty.Register("AllowScrollingOnCircle", typeof(bool), typeof(GridEditorColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed to the editor columns.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null value is allowed ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullValue
        {
            get { return (bool)GetValue(AllowNullValueProperty); }
            set { SetValue(AllowNullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.AllowNullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.AllowNullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullValueProperty =
            GridDependencyProperty.Register("AllowNullValue", typeof(bool), typeof(GridEditorColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets an object that is displayed instead of null value if the cell value is null.
        /// </summary>          
        /// <value>
        /// An object that is displayed instead of null value in the cell.
        /// </value>
        /// <remarks>
        /// The <b>NullValue</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public object NullValue
        {
            get { return (object)GetValue(NullValueProperty); }
            set { SetValue(NullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.NullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.NullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty NullValueProperty =
            GridDependencyProperty.Register("NullValue", typeof(object), typeof(GridEditorColumn), new GridPropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value is null.
        /// </summary>
        /// <value>
        /// A string that is displayed instead of null value in the cell.
        /// </value>
        /// <remarks>
        /// The <b>NullText</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public string NullText
        {
            get { return (string)GetValue(NullTextProperty); }
            set { SetValue(NullTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.NullText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.NullText dependency property.
        /// </remarks>
        public static DependencyProperty NullTextProperty =
            GridDependencyProperty.Register("NullText", typeof(string), typeof(GridEditorColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValue"/> can be validated key press or focus lost on editor in GridEditorColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.MaxValidation"/> enumeration that specifies how the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValue"/> is validated. The default value is <b>MaxValidation.OnKeyPress</b>.
        /// </value>
        public MaxValidation MaxValidation
        {
            get { return (MaxValidation)GetValue(MaxValidationProperty); }
            set { SetValue(MaxValidationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValidation dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValidation dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValidationProperty =
            GridDependencyProperty.Register("MaxValidation", typeof(MaxValidation), typeof(GridEditorColumn), new GridPropertyMetadata(MaxValidation.OnKeyPress));

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValue"/> can be validated key press or focus lost on editor in GridEditorColumn
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.MaxValidation"/>enumeration that specifies how the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValue"/> can be validated. The default value is <b>MinValidation.OnKeyPress</b>.
        /// </value>
        public MinValidation MinValidation
        {
            get { return (MinValidation)GetValue(MinValidationProperty); }
            set { SetValue(MinValidationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValidation dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValidation dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValidationProperty =
            GridDependencyProperty.Register("MinValidation", typeof(MinValidation), typeof(GridEditorColumn), new GridPropertyMetadata(MinValidation.OnKeyPress));

        /// <summary>
        /// Gets or sets the minimum value constraint of the column.
        /// </summary>
        /// <value>
        /// The minimum value constraint of the column.
        /// </value>
        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MinValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValueProperty =
            GridDependencyProperty.Register("MinValue", typeof(decimal), typeof(GridEditorColumn), new GridPropertyMetadata(decimal.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum value constraint of the column.
        /// </summary>
        /// <value>
        /// The maximum value constraint of the column.
        /// </value>
        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridEditorColumn.MaxValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValueProperty =
            GridDependencyProperty.Register("MaxValue", typeof(decimal), typeof(GridEditorColumn), new GridPropertyMetadata(decimal.MaxValue, OnUpdateBindingInfo));

#endregion

#region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridEditorColumn"/> class.
        /// </summary>
        protected GridEditorColumn()
        {
            base.TextAlignment = TextAlignment.Right;
        }
#endregion

#region Overrides
        /// <summary>
        /// Determines whether the cell value is changed using mouse wheel or up and down arrow key is pressed. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell value can be rotated using mouse wheel or up and down arrow key ; otherwise <b>false</b> .
        /// </returns>
        protected internal override bool CanAllowSpinOnMouseScroll()
        {
            return this.AllowScrollingOnCircle;
        }
#endregion
    }

    /// <summary>
    /// Represents a column that displays the currency values in its cell content.
    /// </summary>
    public class GridCurrencyColumn : GridEditorColumn
    {
#region Dependency Properties
        /// <summary>
        /// Gets or sets the number of decimal places to use in currency values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in currency values.The default value is 2.
        /// </value>
        public int CurrencyDecimalDigits
        {
            get { return (int)GetValue(CurrencyDecimalDigitsProperty); }
            set { SetValue(CurrencyDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyDecimalDigitsProperty =
            GridDependencyProperty.Register("CurrencyDecimalDigits", typeof(int), typeof(GridCurrencyColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the group of digits to the left of the decimal in currency values.
        /// </summary>
        /// <value>
        /// The string that separates the group of digits to the left of the decimal .The default value is ",".
        /// </value>
        public string CurrencyGroupSeparator
        {
            get { return (string)GetValue(CurrencyGroupSeparatorProperty); }
            set { SetValue(CurrencyGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyGroupSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyGroupSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyGroupSeparatorProperty =
            GridDependencyProperty.Register("CurrencyGroupSeparator", typeof(string), typeof(GridCurrencyColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string to use as the currency symbol.
        /// </summary>
        /// <value>
        ///  The string that is used as the currency symbol.
        /// </value>
        public string CurrencySymbol
        {
            get { return (string)GetValue(CurrencySymbolProperty); }
            set { SetValue(CurrencySymbolProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencySymbol dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencySymbol dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencySymbolProperty =
            GridDependencyProperty.Register("CurrencySymbol", typeof(string), typeof(GridCurrencyColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencySymbol, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the decimal part in currency values.
        /// </summary>
        /// <value>
        /// The string that separates the decimal part in currency values. 
        /// </value>
        public string CurrencyDecimalSeparator
        {
            get { return (string)GetValue(CurrencyDecimalSeparatorProperty); }
            set { SetValue(CurrencyDecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyDecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyDecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyDecimalSeparatorProperty =
            GridDependencyProperty.Register("CurrencyDecimalSeparator", typeof(string), typeof(GridCurrencyColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in currency values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in currency values.
        /// </value>
        public Int32Collection CurrencyGroupSizes
        {
            get { return (Int32Collection)GetValue(CurrencyGroupSizesProperty); }
            set { SetValue(CurrencyGroupSizesProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyGroupSizes dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyGroupSizes dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyGroupSizesProperty =
            GridDependencyProperty.Register("CurrencyGroupSizes", typeof(Int32Collection), typeof(GridCurrencyColumn), new GridPropertyMetadata(new Int32Collection { 0 }, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern of positive currency values.
        /// </summary>
        /// <value>
        /// The format pattern of positive currency values.
        /// </value>
        public int CurrencyPositivePattern
        {
            get { return (int)GetValue(CurrencyPositivePatternProperty); }
            set { SetValue(CurrencyPositivePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyPositivePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyPositivePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyPositivePatternProperty =
            GridDependencyProperty.Register("CurrencyPositivePattern", typeof(int), typeof(GridCurrencyColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyPositivePattern, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern of negative currency values.
        /// </summary>
        /// <value>
        /// The format pattern of negative currency values.
        /// </value>
        public int CurrencyNegativePattern
        {
            get { return (int)GetValue(CurrencyNegativePatternProperty); }
            set { SetValue(CurrencyNegativePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyNegativePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCurrencyColumn.CurrencyNegativePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyNegativePatternProperty =
            GridDependencyProperty.Register("CurrencyNegativePattern", typeof(int), typeof(GridCurrencyColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyNegativePattern, OnUpdateBindingInfo));

#endregion
#region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCurrencyColumn"/> class.
        /// </summary>
        public GridCurrencyColumn()
        {
            CellType = "Currency";
        }
#endregion

#region overrides
        /// <summary>
        /// Updates the binding for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.AllowEditing"/> property changes in GridCurrencyColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;
            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;
            this.minValue = this.MinValue;
            this.maxValue = this.MaxValue;
            if (binding.Converter is CultureFormatConverter)
                (binding.Converter as CultureFormatConverter).UpdateFormatProvider();
            base.UpdateBindingInfo();
        }

#if WPF
        internal override void OnUpdateBindingInfo(DependencyPropertyChangedEventArgs e)
        {
            if (this.CurrencyDecimalSeparator.Equals(string.Empty))
                throw new ArgumentException("Decimal Separator Cannot be the Empty String", "CurrencyDecimalSeparator");
            if (this.CurrencyPositivePattern > 3 || this.CurrencyPositivePattern < 0)
                throw new ArgumentOutOfRangeException("CurrencyPositivePattern", "Valid Values are between 0 and 3, inclusive");
            if (this.CurrencyNegativePattern > 15 || this.CurrencyNegativePattern < 0)
                throw new ArgumentOutOfRangeException("CurrencyNegativePattern", "Valid Values are between 0 and 15, inclusive");
            base.OnUpdateBindingInfo(e);
        }
#endif

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 2 + padTop, 3 + padRight, 5 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(2 + padLeft, 2 +padTop, 3 + padRight, 2+ padBotton)
                           : new Thickness(2, 2, 3, 2);
#endif
        }
#endregion
    }

    /// <summary>
    /// Represents a column that displays the percent values in its cell content.
    /// </summary>
    public class GridPercentColumn : GridEditorColumn
    {
#region Internal Properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal PercentEditMode percentEditMode = PercentEditMode.DoubleMode;

#endregion

#region Dependency Properties
        /// <summary>
        /// Gets or sets the number of decimal places to use in percent values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in percent values. 
        /// </value>
        public int PercentDecimalDigits
        {
            get { return (int)GetValue(PercentDecimalDigitsProperty); }
            set { SetValue(PercentDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentDecimalDigitsProperty =
            GridDependencyProperty.Register("PercentDecimalDigits", typeof(int), typeof(GridPercentColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.PercentDecimalDigits, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string to use as the decimal separator in percent values.
        /// </summary>
        /// <value>
        /// The string to use as the decimal separator in percent values.
        /// </value>
        public string PercentDecimalSeparator
        {
            get { return (string)GetValue(PercentDecimalSeparatorProperty); }
            set { SetValue(PercentDecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentDecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentDecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentDecimalSeparatorProperty =
            GridDependencyProperty.Register("PercentDecimalSeparator", typeof(string), typeof(GridPercentColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.PercentDecimalSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in percent values.
        /// </summary>
        /// <value>
        /// The string that separates groups of digits to the left of the decimal in percent values. 
        /// </value>
        public string PercentGroupSeparator
        {
            get { return (string)GetValue(PercentGroupSeparatorProperty); }
            set { SetValue(PercentGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentGroupSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentGroupSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentGroupSeparatorProperty =
            GridDependencyProperty.Register("PercentGroupSeparator", typeof(string), typeof(GridPercentColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.PercentGroupSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the percent editor loads percent or double value being edited in GridPercentColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.PercentEditMode"/> that decides the type of value loads in GridPercentColumn being edited.
        /// The default value is <b>PercentEditMode.DoubleMode</b>.
        /// </value>
        public PercentEditMode PercentEditMode
        {
            get { return (PercentEditMode)GetValue(PercentEditModeProperty); }
            set { SetValue(PercentEditModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentEditMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentEditMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentEditModeProperty =
            DependencyProperty.Register("PercentEditMode", typeof(PercentEditMode), typeof(GridPercentColumn), new PropertyMetadata(PercentEditMode.DoubleMode, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in percent values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in percent values.
        /// </value>
        public Int32Collection PercentGroupSizes
        {
            get { return (Int32Collection)GetValue(PercentGroupSizesProperty); }
            set { SetValue(PercentGroupSizesProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentGroupSizes dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentGroupSizes dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentGroupSizesProperty =
            GridDependencyProperty.Register("PercentGroupSizes", typeof(Int32Collection), typeof(GridPercentColumn), new GridPropertyMetadata(new Int32Collection { 0 }, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern for negative values in GridPercentColumn.
        /// </summary>
        /// <value>
        /// The format pattern for negative percent values in GridPercentColumn. 
        /// </value>
        public int PercentNegativePattern
        {
            get { return (int)GetValue(PercentNegativePatternProperty); }
            set { SetValue(PercentNegativePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentNegativePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentNegativePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentNegativePatternProperty =
            GridDependencyProperty.Register("PercentNegativePattern", typeof(int), typeof(GridPercentColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.PercentNegativePattern, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern for the positive values in GridPercentColumn.
        /// </summary>
        /// <value>
        /// The percent positive pattern in GridPercentColumn.
        /// </value>
        public int PercentPositivePattern
        {
            get { return (int)GetValue(PercentPositivePatternProperty); }
            set { SetValue(PercentPositivePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentPositivePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentPositivePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentPositivePatternProperty =
            GridDependencyProperty.Register("PercentPositivePattern", typeof(int), typeof(GridPercentColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.PercentPositivePattern, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string to use as the percent symbol.
        /// </summary>
        /// <value>
        /// The string to use as the percent symbol.
        /// </value>
        public string PercentSymbol
        {
            get { return (string)GetValue(PercentSymbolProperty); }
            set { SetValue(PercentSymbolProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentSymbol dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridPercentColumn.PercentSymbol dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentSymbolProperty =
            GridDependencyProperty.Register("PercentSymbol", typeof(string), typeof(GridPercentColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.PercentSymbol, OnUpdateBindingInfo));

#endregion
#region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridPercentColumn"/> class.
        /// </summary>
        public GridPercentColumn()
        {
            CellType = "Percent";
        }
#endregion

#region overrides

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridPercentColumn.
        /// </summary>       
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;
            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;
            this.minValue = this.MinValue;
            this.maxValue = this.MaxValue;
            this.percentEditMode = this.PercentEditMode;
            if (binding.Converter is CultureFormatConverter)
                (binding.Converter as CultureFormatConverter).UpdateFormatProvider();
            base.UpdateBindingInfo();
        }
#if WPF
        internal override void OnUpdateBindingInfo(DependencyPropertyChangedEventArgs e)
        {
            if (this.PercentDecimalSeparator.Equals(string.Empty))
                throw new ArgumentException("Decimal Separator Cannot be the Empty String", "PercentDecimalSeparator");
            if (this.PercentPositivePattern > 3 || this.PercentPositivePattern < 0)
                throw new ArgumentOutOfRangeException("PercentPositivePattern", "Valid Values are between 0 and 3, inclusive");
            if (this.PercentNegativePattern > 11 || this.PercentNegativePattern < 0)
                throw new ArgumentOutOfRangeException("PercentNegativePattern", "Valid Values are between 0 and 11, inclusive");
            base.OnUpdateBindingInfo(e);
        }
#endif
        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(2 + padLeft, 2 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(2, 2, 3, 2);
#endif
        }
#endregion
    }

    /// <summary>
    /// Represents a column that displays the numeric values in its cell contents.
    /// </summary>
    public class GridNumericColumn : GridEditorColumn
    {
#region Dependency Properties
        /// <summary>
        /// Gets or sets the number of decimal places to use in numeric values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in numeric values.
        /// </value>
        public int NumberDecimalDigits
        {
            get { return (int)GetValue(NumberDecimalDigitsProperty); }
            set { SetValue(NumberDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberDecimalDigitsProperty =
            GridDependencyProperty.Register("NumberDecimalDigits", typeof(int), typeof(GridNumericColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.NumberDecimalDigits, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or Sets a value that decides the type to cast the cell value in edit mode of <see cref="Syncfusion.UI.Xaml.Grid.GridNumericColumn"/>.
        /// </summary>
        ///<value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.ParseMode"/> enumeration that specifies the type that want to cast on edit mode.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.ParseMode.Double"/>. 
        ///</value> 
#if WPF
        public ParseMode ParsingMode
        {
            get { return (ParseMode)GetValue(ParsingModeProperty); }
            set { SetValue(ParsingModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.ParsingMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.ParsingMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty ParsingModeProperty =
             GridDependencyProperty.Register("ParsingMode", typeof(ParseMode), typeof(GridNumericColumn), new GridPropertyMetadata(ParseMode.Double, OnUpdateBindingInfo));
#endif
        /// <summary>
        /// Gets or sets the string to use as the decimal separator in numeric values.
        /// </summary>
        /// <value>
        /// The string to use as the decimal separator in numeric values.
        /// </value>
        public string NumberDecimalSeparator
        {
            get { return (string)GetValue(NumberDecimalSeparatorProperty); }
            set { SetValue(NumberDecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberDecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberDecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberDecimalSeparatorProperty =
            GridDependencyProperty.Register("NumberDecimalSeparator", typeof(string), typeof(GridNumericColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in numeric values.
        /// </summary>
        /// <value>
        /// The string that separates groups of digits to the left of the decimal in numeric values.
        /// </value>
        public string NumberGroupSeparator
        {
            get { return (string)GetValue(NumberGroupSeparatorProperty); }
            set { SetValue(NumberGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberGroupSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberGroupSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberGroupSeparatorProperty =
            GridDependencyProperty.Register("NumberGroupSeparator", typeof(string), typeof(GridNumericColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in numeric values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in numeric values.
        /// </value>
        public Int32Collection NumberGroupSizes
        {
            get { return (Int32Collection)GetValue(NumberGroupSizesProperty); }
            set { SetValue(NumberGroupSizesProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberGroupSizes dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberGroupSizes dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberGroupSizesProperty =
            GridDependencyProperty.Register("NumberGroupSizes", typeof(Int32Collection), typeof(GridNumericColumn), new GridPropertyMetadata(new Int32Collection { 0 }, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern for negative numeric values.
        /// </summary>
        /// <value>
        /// The format pattern for negative numeric values.
        /// </value>
        public int NumberNegativePattern
        {
            get { return (int)GetValue(NumberNegativePatternProperty); }
            set { SetValue(NumberNegativePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberNegativePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.NumberNegativePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberNegativePatternProperty =
            GridDependencyProperty.Register("NumberNegativePattern", typeof(int), typeof(GridNumericColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.NumberNegativePattern, OnUpdateBindingInfo));

#endregion

#region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridNumericColumn"/> class.
        /// </summary>
        public GridNumericColumn()
        {
            CellType = "Numeric";
        }
#endregion

#region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridNumericColumn.
        /// </summary>       
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }
#if WPF
        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.ValueBinding"/> of GridNumericColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.ValueBinding"/>.
        /// </remarks>
        protected override void SetValueBindingConverter()
        {
            if (!isValueMultiBinding)
                if ((ValueBinding as Binding).Converter == null)
                    (ValueBinding as Binding).Converter = new ValueBindingConverter(this);
        }
#endif

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;

            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;

            this.minValue = this.MinValue;
            this.maxValue = this.MaxValue;
            if (binding.Converter is CultureFormatConverter)
                (binding.Converter as CultureFormatConverter).UpdateFormatProvider();
            base.UpdateBindingInfo();
        }

#if WPF
        internal override void OnUpdateBindingInfo(DependencyPropertyChangedEventArgs e)
        {
            if (this.NumberDecimalSeparator.Equals(string.Empty))
                throw new ArgumentException("Decimal Separator Cannot be the Empty String", "NumberDecimalSeparator");
            if (this.NumberNegativePattern > 4 || this.NumberNegativePattern < 0)
                throw new ArgumentOutOfRangeException("NumberNegativePattern", "Valid Values are between 0 and 4, inclusive");
            base.OnUpdateBindingInfo(e);
        }
#endif

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft,  padTop, 2 + padRight,padBotton)
                           : new Thickness(3, 1, 3, 0);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#endif
        }

#endregion
    }

    /// <summary>
    /// Represents a column that displays the masked data in its cell content.
    /// </summary>
    public class GridMaskColumn : GridTextColumnBase
    {
#region Dependency Properties
#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether the GridMaskColumn that loads the numeric values in it.
        /// </summary>
        /// <value>
        /// <b>true</b> if the GridMaskColumn loaded with numeric values; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool IsNumeric
        {
            get { return (bool)GetValue(IsNumericProperty); }
            set { SetValue(IsNumericProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.IsNumeric dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.IsNumeric dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsNumericProperty =
            GridDependencyProperty.Register("IsNumeric", typeof(bool), typeof(GridMaskColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the components of date,that is, day ,month and year in GridMaskColumn.
        /// </summary>
        /// <value>
        /// The string that separates the components of date,that is, day ,month and year in GridMaskColumn. 
        /// </value>        
        public string DateSeparator
        {
            get { return (string)GetValue(DateSeparatorProperty); }
            set { SetValue(DateSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.DateSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.DateSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty DateSeparatorProperty =
           GridDependencyProperty.Register("DateSeparator", typeof(string), typeof(GridMaskColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in values.
        /// </summary>
        /// <value>
        /// The string that separates groups of digits to the left of the decimal in values. 
        /// </value>
        public string DecimalSeparator
        {
            get { return (string)GetValue(DecimalSeparatorProperty); }
            set { SetValue(DecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.DecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.DecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty DecimalSeparatorProperty =
            GridDependencyProperty.Register("DecimalSeparator", typeof(string), typeof(GridMaskColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the components of time, that is, the hour , minutes and seconds .
        /// </summary>
        /// <value>
        /// The string that separates the components of time, that is, the hour , minutes and seconds .
        /// </value>       
        public string TimeSeparator
        {
            get { return (string)GetValue(TimeSeparatorProperty); }
            set { SetValue(TimeSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.TimeSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.TimeSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty TimeSeparatorProperty =
            GridDependencyProperty.Register("TimeSeparator", typeof(string), typeof(GridMaskColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));
#else
        /// <summary>
        /// Gets or sets the type of mask used in GridMaskColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.MaskType"> that specifies the type of mask. The default mask type is <b>MaskType.Simple</b>.
        /// </value>      
        public MaskType MaskType
        {
            get { return (MaskType)GetValue(MaskTypeProperty); }
            set { SetValue(MaskTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.MaskType dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.MaskType dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaskTypeProperty =
            GridDependencyProperty.Register("MaskType", typeof(MaskType), typeof(GridMaskColumn), new GridPropertyMetadata(MaskType.Simple));

        /// <summary>
        /// Gets or sets culture information associated with the GridMaskColumn.
        /// </summary>
        /// <value>
        ///  The <see cref="System.Globalization.CultureInfo"/> representing the culture supported by GridMaskColumn.
        /// </value>
        public CultureInfo Culture
        {
            get { return (CultureInfo)GetValue(CultureProperty); }
            set { SetValue(CultureProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.Culture dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.Culture dependency property.
        /// </remarks>
        public static readonly DependencyProperty CultureProperty =
            GridDependencyProperty.Register("Culture", typeof(CultureInfo), typeof(GridMaskColumn), new GridPropertyMetadata(CultureInfo.CurrentCulture));

        /// <summary>
        /// Gets or sets a value that indicates whether the input validated key press or focus lost on editor in GridMaskColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.Input.InputValidationMode"/> that specifies the validation mode for GridMaskColumn.
        /// The default value is <b>InputValidationMode.KeyPress</b>.
        /// </value>
        public InputValidationMode ValidationMode
        {
            get { return (InputValidationMode)GetValue(ValidationModeProperty); }
            set { SetValue(ValidationModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.ValidationMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.ValidationMode dependency property.
        /// </remarks>       
        public static readonly DependencyProperty ValidationModeProperty =
            DependencyProperty.Register("ValidationMode", typeof(InputValidationMode), typeof(SfMaskedEdit), new PropertyMetadata(InputValidationMode.KeyPress));

        /// <summary>
        /// Gets or sets the keyboard options for the GridMaskColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.Input.KeyboardOptions"/> that specifies the keyboard options for GridMaskColumn. 
        /// The default value is <b>KeyboardOptions.Default</b>.
        /// </value>
        public KeyboardOptions KeyboardType
        {
            get { return (KeyboardOptions)GetValue(KeyboardTypeProperty); }
            set { SetValue(KeyboardTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.KeyboardType dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.KeyboardType dependency property.
        /// </remarks> 
        public static readonly DependencyProperty KeyboardTypeProperty =
            DependencyProperty.Register("KeyboardType", typeof(KeyboardOptions), typeof(SfMaskedEdit), new PropertyMetadata(KeyboardOptions.Default));
#endif
        /// <summary>
        /// Gets or sets the input mask to use at runtime.
        /// </summary>
        /// <value>
        /// A string that representing the current mask. The default value is the empty string which allows any input.
        /// </value>
        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.Mask dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.Mask dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaskProperty =
            GridDependencyProperty.Register("Mask", typeof(string), typeof(GridMaskColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format of masked input.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.MaskFormat"/> that specifies the format of masked input.
        /// The default value is <see cref="Syncfusion.Windows.Shared.MaskFormat.ExcludePromptAndLiterals"/>.
        /// </value>
        public MaskFormat MaskFormat
        {
            get { return (MaskFormat)GetValue(MaskFormatProperty); }
            set { SetValue(MaskFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.MaskFormat dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.MaskFormat dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaskFormatProperty =
            GridDependencyProperty.Register("MaskFormat", typeof(MaskFormat), typeof(GridMaskColumn), new GridPropertyMetadata(MaskFormat.ExcludePromptAndLiterals));

        /// <summary>
        /// Gets or sets the character used to represent the absence of user input in GridMaskColumn.
        /// </summary>
        /// <value>
        /// The character used to prompt the user for input. The default is an underscore (_).
        /// </value>
        public char PromptChar
        {
            get { return (char)GetValue(PromptCharProperty); }
            set { SetValue(PromptCharProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.PromptChar dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.PromptChar dependency property.
        /// </remarks>
        public static readonly DependencyProperty PromptCharProperty =
            GridDependencyProperty.Register("PromptChar", typeof(char), typeof(GridMaskColumn), new GridPropertyMetadata('_', OnUpdateBindingInfo));


        /// <summary>
        /// Gets or sets a value that indicates whether the entire cell value is selected when it receives focus.
        /// </summary>
        /// <value>
        /// <b>true</b> if the entire cell value is selected when it receives focus ; otherwise , <b>false</b>.
        /// The default value is <b>false</b>.
        /// </value>
        [Obsolete]
        public bool SelectTextOnFocus
        {
            get { return (bool)GetValue(SelectTextOnFocusProperty); }
            set { SetValue(SelectTextOnFocusProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridMaskColumn.SelectTextOnFocus dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridMaskColumn.SelectTextOnFocus dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectTextOnFocusProperty =
            GridDependencyProperty.Register("SelectTextOnFocus", typeof(bool), typeof(GridMaskColumn), new GridPropertyMetadata(false));
#endregion

#region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridMaskColumn"/> class.
        /// </summary>
        public GridMaskColumn()
        {
            CellType = "Mask";
            this.FilterBehavior = FilterBehavior.StringTyped;
        }
#endregion

#region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridMaskColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(13 + padLeft, 7 + padTop, 13 + padRight, 5 + padBotton)
                           : new Thickness(3, 0, 3, 0);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(10 + padLeft, 3 + padTop, 10 + padRight, 5 + padBotton)
                           : new Thickness(2, 3, 2, 2);
#endif
        }
#endregion
    }

    /// <summary>
    /// Represents a column that displays the time span values in its cell content.
    /// </summary>
    public class GridTimeSpanColumn : GridTextColumnBase
    {

#region Internal Property

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string format = @"d:h:m:s";

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal TimeSpan minValue = TimeSpan.MinValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal TimeSpan maxValue = TimeSpan.MaxValue;

#endregion

#region Dependency Property
        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in GridTimeSpanColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>    
        public bool AllowNull
        {
            get { return (bool)GetValue(AllowNullProperty); }
            set { SetValue(AllowNullProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.AllowNull dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.AllowNull dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullProperty =
            GridDependencyProperty.Register("AllowNull", typeof(bool), typeof(GridTimeSpanColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell contains null value.
        /// </summary>
        /// <value>
        /// A string that is displayed instead of null value in the cell.
        /// </value>
        /// <remarks>
        /// The <b>NullText</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.AllowNull"/> property is enabled.
        /// </remarks>
        public string NullText
        {
            get { return (string)GetValue(NullTextProperty); }
            set { SetValue(NullTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.NullText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.NullText dependency property.
        /// </remarks>
        public static readonly DependencyProperty NullTextProperty =
            GridDependencyProperty.Register("NullText", typeof(object), typeof(GridTimeSpanColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that specifies to format the time span value.
        /// </summary>
        /// <value>
        /// A string that specifies the format the time span value. The default format is ""d:h:m:s".
        /// </value>
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.Format dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.Format dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatProperty =
            GridDependencyProperty.Register("Format", typeof(string), typeof(GridTimeSpanColumn), new GridPropertyMetadata(@"d:h:m:s", OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the cell values using mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is changed using the mouse wheel or up and down arrow key; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>
        public bool AllowScrollingOnCircle
        {
            get { return (bool)GetValue(AllowScrollingOnCircleProperty); }
            set { SetValue(AllowScrollingOnCircleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.AllowScrollingOnCircle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.AllowScrollingOnCircle dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowScrollingOnCircleProperty =
            GridDependencyProperty.Register("AllowScrollingOnCircle", typeof(bool), typeof(GridTimeSpanColumn), new GridPropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether to show the arrow button control.
        /// </summary>
        /// <value>
        /// <b>true</b> if the arrow button is used to adjust the time span value ; otherwise , <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool ShowArrowButtons
        {
            get { return (bool)GetValue(ShowArrowButtonsProperty); }
            set { SetValue(ShowArrowButtonsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.ShowArrowButtons dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.ShowArrowButtons dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowArrowButtonsProperty =
            GridDependencyProperty.Register("ShowArrowButtonsProperty", typeof(bool), typeof(GridTimeSpanColumn), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets the maximum value allowed for GridTimeSpanColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.TimeSpan"/> representing the maximum time span value for GridTimeSpanColumn.
        /// </value>
        public TimeSpan MaxValue
        {
            get { return (TimeSpan)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.MaxValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.MaxValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValueProperty =
            GridDependencyProperty.Register("MaxValueProperty", typeof(TimeSpan), typeof(GridTimeSpanColumn), new GridPropertyMetadata(System.TimeSpan.MaxValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the minimum value allowed for GridTimeSpanColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.TimeSpan"/> representing the minimum time span value for GridTimeSpanColumn.
        /// </value>
        public TimeSpan MinValue
        {
            get { return (TimeSpan)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.MinValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn.MinValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValueProperty =
            GridDependencyProperty.Register("MinValueProperty", typeof(TimeSpan), typeof(GridTimeSpanColumn), new GridPropertyMetadata(System.TimeSpan.MinValue, OnUpdateBindingInfo));

#endregion

#region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridTimeSpanColumn"/> class.
        /// </summary>
        public GridTimeSpanColumn()
        {
            CellType = "TimeSpan";
        }
#endregion
#region Overrides
        /// <summary>
        /// Determines whether the cell value is rotated using mouse wheel or up and down arrow key is pressed. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell value can be rotated using mouse wheel or up and down arrow key; otherwise <b>false</b> .
        /// </returns>
        protected internal override bool CanAllowSpinOnMouseScroll()
        {
            return this.AllowScrollingOnCircle;
        }

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridTimeSpanColumn.
        /// </summary>
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {

            if (this.DataGrid == null)
                return;
            this.minValue = this.MinValue;
            this.maxValue = this.MaxValue;
            this.format = this.Format;
            base.UpdateBindingInfo();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);

            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, padTop, 3 + padRight, padBotton)
                           : new Thickness(3, 0, 3, 0);
        }
#endregion
    }

#else
    /// <summary>
    /// Represents a column that displays the numeric values in its cell content.
    /// </summary>
    public class GridNumericColumn : GridTextColumnBase
    {
#region Internal properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal PercentDisplayMode percentDisplayMode = PercentDisplayMode.Compute;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal Parsers parsingMode = Parsers.Double;

#endregion

#region Dependency Properties

        /// <summary>
        /// Gets or sets a value indicating whether the characters is blocked from an user input.
        /// </summary>
        /// <value>
        /// <b>true</b> if the characters blocked; otherwise, <b>false</b>.
        /// </value>
        public bool BlockCharactersOnTextInput
        {
            get { return (bool)GetValue(BlockCharactersOnTextInputProperty); }
            set { SetValue(BlockCharactersOnTextInputProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.BlockCharactersOnTextInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.BlockCharactersOnTextInput dependency property.
        /// </remarks>
        public static readonly DependencyProperty BlockCharactersOnTextInputProperty =
            GridDependencyProperty.Register("BlockCharactersOnTextInput", typeof(bool), typeof(GridNumericColumn), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in GridNumericColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullInput
        {
            get { return (bool)GetValue(AllowNullInputProperty); }
            set { SetValue(AllowNullInputProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.AllowNullInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.AllowNullInput dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullInputProperty =
            GridDependencyProperty.Register("AllowNullInput", typeof(bool), typeof(GridNumericColumn), new GridPropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that specifies how to format the bound value in GridNumericColumn.
        /// </summary>
        /// <value>
        /// A string that specifies how to format the bound value in GridNumericColumn. The default value is string.Empty.
        /// </value>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.FormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.FormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatStringProperty =
            GridDependencyProperty.Register("FormatString", typeof(string), typeof(GridNumericColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that decides indicates the type to be parsed among numeric types (int, double, deciaml) in GridNumericColumn. 
        /// As GridNumericColumn is used to edit all (int, double, decimal) numeric types. If you are binding decimal type data with column, then you have to set ParsingMode as Decimal.
        /// simillarly for Double,You Have to Set ParseMode as Double
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.Input.Parsers"/> that specifies the parsing mode of GridNumericColumn.
        /// The default mode is <b> Parsers.Double </b>.
        /// </value>
        public Parsers ParsingMode
        {
            get { return (Parsers)GetValue(ParsingModeProperty); }
            set { SetValue(ParsingModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.ParsingMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.ParsingMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty ParsingModeProperty =
            GridDependencyProperty.Register("ParsingMode", typeof(Parsers), typeof(GridNumericColumn), new GridPropertyMetadata(Parsers.Double, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the content displayed as a watermark in GridNumericColumn when its cell contains empty value.
        /// </summary>
        /// <value>
        /// The content displayed as a watermark in GridNumericColumn when its cell contains empty value.
        /// The default value is <b>string.Empty</b>.
        /// </value>
        public object WaterMark
        {
            get { return (object)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.WaterMark dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.WaterMark dependency property.
        /// </remarks>
        public static readonly DependencyProperty WaterMarkProperty =
            GridDependencyProperty.Register("WaterMark", typeof(object), typeof(GridNumericColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the number of decimal digits associated with the GridNumericColumn.
        /// </summary>
        /// <value>
        /// By default, any number of decimal digits are allowed.
        /// </value>
        public int MaximumNumberDecimalDigits
        {
            get { return (int)GetValue(MaximumNumberDecimalDigitsProperty); }
            set { SetValue(MaximumNumberDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.MaximumNumberDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.MaximumNumberDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaximumNumberDecimalDigitsProperty =
            DependencyProperty.Register("MaximumNumberDecimalDigits", typeof(int), typeof(GridNumericColumn), new PropertyMetadata(Int32.MaxValue));

        /// <summary>
        /// Gets or Sets the value in percentage associated with GridNumericColumn.
        /// </summary>
        /// <value>
        /// The Default value is <see cref="Syncfusion.UI.Xaml.Controls.Input.PercentDisplayMode">PercentDisplayMode.Compute</see>.
        /// </value>
        public PercentDisplayMode PercentDisplayMode
        {
            get { return (PercentDisplayMode)GetValue(PercentDisplayModeProperty); }
            set { SetValue(PercentDisplayModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridNumericColumn.PercentDisplayMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridNumericColumn.PercentDisplayMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentDisplayModeProperty =
            DependencyProperty.Register("PercentDisplayMode", typeof(PercentDisplayMode), typeof(GridNumericColumn), new PropertyMetadata(PercentDisplayMode.Compute, OnUpdateBindingInfo));

#endregion

#region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridNumericColumn"/> class.
        /// </summary>
        public GridNumericColumn()
        {
            this.CellType = "Numeric";
            base.TextAlignment = TextAlignment.Right;
        }
#endregion

#region  overrides

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;

            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;
            this.parsingMode = this.ParsingMode;
            this.percentDisplayMode = this.PercentDisplayMode;
            this.formatString = this.FormatString;
            if (binding.Converter is CultureFormatConverter)
                (binding.Converter as CultureFormatConverter).UpdateFormatProvider();
            base.UpdateBindingInfo();
        }
#endregion
    }
#endif
    /// <summary>
    /// Represents a column that displays the date time values in its cell content.
    /// </summary>
    public class GridDateTimeColumn : GridTextColumnBase
    {
#region Internal Proeprties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal DateTime minDateTime = System.DateTime.MinValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal DateTime maxDateTime = System.DateTime.MaxValue;
#if WPF
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal DateTimePattern pattern = DateTimePattern.ShortDate;
#endif
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal DateTimeFormatInfo dateTimeFormat = DateTimeFormatInfo.CurrentInfo;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string customPattern = string.Empty;

#endregion
#if UWP
#region Dependency Properties
        /// <summary>
        /// Gets or sets a value that indicates whether the editing is enabled for GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the editing is enabled; otherwise, <b>false</b>.
        /// </value>
        public bool AllowInlineEditing
        {
            get { return (bool)GetValue(AllowInlineEditingProperty); }
            set { SetValue(AllowInlineEditingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowInlineEditing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowInlineEditing dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowInlineEditingProperty =
            GridDependencyProperty.Register("AllowInlineEditing", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a string that specifies how to format the bounded value in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// A string that specifies how to format the bound value in GridDateTimeColumn. The default value is string.Empty.
        /// </value>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.FormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.FormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatStringProperty =
            GridDependencyProperty.Register("FormatString", typeof(string), typeof(GridDateTimeColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether a drop-down button control is used to adjust the date time value.
        /// </summary>
        /// <value>
        /// <b>true</b> if the drop-down button is used to adjust the date time value ; otherwise , <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool ShowDropDownButton
        {
            get { return (bool)GetValue(ShowDropDownButtonProperty); }
            set { SetValue(ShowDropDownButtonProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.ShowDropDownButton dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.ShowDropDownButton dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowDropDownButtonProperty =
            GridDependencyProperty.Register("ShowDropDownButton", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the minimum date and time that can be selected in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The minimum date and time that can be selected in GridDateTimeColumn. 
        /// </value>
        public DateTime MinDate
        {
            get { return (DateTime)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MinDate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MinDate dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinDateProperty =
            GridDependencyProperty.Register("MinDate", typeof(DateTime), typeof(GridDateTimeColumn), new GridPropertyMetadata(System.DateTime.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum date and time that can be selected in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The maximum date and time that can be selected in GridDateTimeColumn. 
        /// </value>
        public DateTime MaxDate
        {
            get { return (DateTime)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MaxDate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MaxDate dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxDateProperty =
            GridDependencyProperty.Register("MaxDate", typeof(DateTime), typeof(GridDateTimeColumn), new GridPropertyMetadata(System.DateTime.MaxValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the content displayed as a watermark in GridDateTimeColumn when its cell contains empty value.
        /// </summary>
        /// <value>
        /// The content displayed as a watermark in GridDateTimeColumn when its cell contains empty value.
        /// The default value is string.Empty .
        /// </value>
        public object WaterMark
        {
            get { return (object)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.WaterMark dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.WaterMark dependency property.
        /// </remarks>
        public static readonly DependencyProperty WaterMarkProperty =
            GridDependencyProperty.Register("WaterMark", typeof(object), typeof(GridDateTimeColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the Accent brush for the date selector items of GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The brush for the date selector items of GridDateTimeColumn. The default value is <b>SlateBlue</b>.
        /// </value>
        public Brush AccentBrush
        {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AccentBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AccentBrush dependency property.
        /// </remarks>
        public static readonly DependencyProperty AccentBrushProperty =
            GridDependencyProperty.Register("AccentBrush", typeof(Brush), typeof(GridDateTimeColumn), new GridPropertyMetadata(new SolidColorBrush(Colors.SlateBlue)));

        /// <summary>
        /// Gets or sets the height of the drop-down of GridDateTimeColumn.      
        /// </summary>
        /// <value>
        /// The height of the drop-down of GridDateTimeColumn. The default value is 400.0 .
        /// </value>        
        public double DropDownHeight
        {
            get { return (double)GetValue(DropDownHeightProperty); }
            set { SetValue(DropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.DropDownHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.DropDownHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty DropDownHeightProperty =
            GridDependencyProperty.Register("DropDownHeight", typeof(double), typeof(GridDateTimeColumn), new GridPropertyMetadata(400.0));

        /// <summary>
        /// Gets or sets a value that indicates whether the drop-down of GridDateTimeColumn is currently open.
        /// </summary>
        /// <value>
        /// <b>true</b> if the drop-down is open; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value> 
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.IsDropDownOpen dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.IsDropDownOpen dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            GridDependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the input scope of the on-screen keyboard for GridDateTimeColumn.
        /// </summary> 
        /// <value>
        /// One of the <see cref="Windows.UI.Xaml.Input.InputScopeNameValue"/> that specifies the input scope for GridDateTimeColumn. The default value <see cref="Windows.UI.Xaml.Input.InputScopeNameValue.Default"/>.
        /// </value>    
        public InputScopeNameValue InputScope
        {
            get { return (InputScopeNameValue)GetValue(InputScopeProperty); }
            set { SetValue(InputScopeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.InputScope dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.InputScope dependency property.
        /// </remarks>
        public static readonly DependencyProperty InputScopeProperty =
            GridDependencyProperty.Register("InputScope", typeof(InputScopeNameValue), typeof(GridDateTimeColumn), new GridPropertyMetadata(InputScopeNameValue.Default));

        /// <summary>
        /// Gets or sets the SelectorItemCount for the date selector items
        /// </summary>
        /// <value> The default value is 0 </value>
        public int SelectorItemCount
        {
            get { return (int)GetValue(SelectorItemCountProperty); }
            set { SetValue(SelectorItemCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.InputScope dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.InputScope dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemCountProperty =
            GridDependencyProperty.Register("SelectorItemCount", typeof(int), typeof(GridDateTimeColumn), new GridPropertyMetadata(0));

        /// <summary>
        /// Gets or sets the space for between the date, month and year items in the selector of GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The space between the items in the selector. The default value is 4. 
        /// </value>
        public double SelectorItemSpacing
        {
            get { return (double)GetValue(SelectorItemSpacingProperty); }
            set { SetValue(SelectorItemSpacingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorItemSpacing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorItemSpacing dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemSpacingProperty =
            GridDependencyProperty.Register("SelectorItemSpacing", typeof(double), typeof(GridDateTimeColumn), new GridPropertyMetadata(4.0));

        /// <summary>
        /// Gets or sets the width of the date selector items in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The width of the date selector items. The default value is 80.
        /// </value>
        public double SelectorItemWidth
        {
            get { return (double)GetValue(SelectorItemWidthProperty); }
            set { SetValue(SelectorItemWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorItemWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorItemWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemWidthProperty =
            GridDependencyProperty.Register("SelectorItemWidth", typeof(double), typeof(GridDateTimeColumn), new GridPropertyMetadata(80.0));

        /// <summary>
        /// Gets or sets the height of the date selector items in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The height of the date selector items. The default value is 80.
        /// </value>
        public double SelectorItemHeight
        {
            get { return (double)GetValue(SelectorItemHeightProperty); }
            set { SetValue(SelectorItemHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorItemHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorItemHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemHeightProperty =
            GridDependencyProperty.Register("SelectorItemHeight", typeof(double), typeof(GridDateTimeColumn), new GridPropertyMetadata(80.0));


        /// <summary>
        /// Gets or sets the SelectorFormatString for the date picker column      
        /// </summary>        
        public object SelectorFormatString
        {
            get { return (object)GetValue(SelectorFormatStringProperty); }
            set { SetValue(SelectorFormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorFormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.SelectorFormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorFormatStringProperty =
            GridDependencyProperty.Register("SelectorFormatString", typeof(object), typeof(GridDateTimeColumn), new GridPropertyMetadata("m:d:y"));

#endregion
#else
#region Dependency Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is changed using the mouse wheel or up and down arrow key; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>
        public bool AllowScrollingOnCircle
        {
            get { return (bool)GetValue(AllowScrollingOnCircleProperty); }
            set { SetValue(AllowScrollingOnCircleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowScrollingOnCircle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowScrollingOnCircle dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowScrollingOnCircleProperty =
            GridDependencyProperty.Register("AllowScrollingOnCircle", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that is displayed instead of null value if the cell value is null.
        /// </summary>          
        /// <value>
        /// A <see cref="System.DateTime"/> that is displayed instead of null value in the cells of GridDateTimeColumn.
        /// </value>
        /// <remarks>
        /// The <b>NullValue</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public DateTime? NullValue
        {
            get { return (DateTime?)GetValue(NullValueProperty); }
            set { SetValue(NullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.NullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.NullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty NullValueProperty =
            GridDependencyProperty.Register("NullValue", typeof(DateTime?), typeof(GridDateTimeColumn), new GridPropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value is null.
        /// </summary>
        /// <value>
        /// A string that is displayed instead of null value in the cell of GridDateTimeColumn.
        /// </value>
        /// <remarks>
        /// The <b>NullText</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public string NullText
        {
            get { return (string)GetValue(NullTextProperty); }
            set { SetValue(NullTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.NullText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.NullText dependency property.
        /// </remarks>
        public static DependencyProperty NullTextProperty =
            GridDependencyProperty.Register("NullText", typeof(string), typeof(GridDateTimeColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the classic style is enabled on the drop-down of GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the classic style is enabled; otherwise ,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool EnableClassicStyle
        {
            get { return (bool)GetValue(EnableClassicStyleProperty); }
            set { SetValue(EnableClassicStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.EnableClassicStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.EnableClassicStyle dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableClassicStyleProperty =
            GridDependencyProperty.Register("EnableClassicStyle", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the date selection is disabled on the calendar pop-up of GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the date selection is disabled on the calendar pop-up; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool DisableDateSelection
        {
            get { return (bool)GetValue(DisableDateSelectionProperty); }
            set { SetValue(DisableDateSelectionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.DisableDateSelection dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.DisableDateSelection dependency property.
        /// </remarks>
        public static readonly DependencyProperty DisableDateSelectionProperty =
            GridDependencyProperty.Register("DisableDateSelection", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether a repeat button control is used to adjust the date and time value in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the repeat button control is used to adjust the date and time value; otherwise , <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool ShowRepeatButton
        {
            get { return (bool)GetValue(ShowRepeatButtonProperty); }
            set { SetValue(ShowRepeatButtonProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.ShowRepeatButton dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.ShowRepeatButton dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowRepeatButtonProperty =
            GridDependencyProperty.Register("ShowRepeatButton", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the format string for a date and time value.
        /// </summary>
        /// <value>
        /// The format string for a date and time value in GridDateTimeColumn.The default value is <see cref="Syncfusion.Windows.Shared.DateTimePattern.ShortDate"/>.
        /// </value>
        public DateTimePattern Pattern
        {
            get { return (DateTimePattern)GetValue(PatternProperty); }
            set { SetValue(PatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.Pattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.Pattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty PatternProperty =
            GridDependencyProperty.Register("Pattern", typeof(DateTimePattern), typeof(GridDateTimeColumn), new GridPropertyMetadata(DateTimePattern.ShortDate, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a <see cref="System.Globalization.DateTimeFormatInfo"/> that defines the format of date and time values.
        /// </summary>
        /// <value>
        /// A <see cref="System.Globalization.DateTimeFormatInfo"/> that defines the format of date and time values.
        /// </value>
        public DateTimeFormatInfo DateTimeFormat
        {
            get { return (DateTimeFormatInfo)GetValue(DateTimeFormatProperty); }
            set { SetValue(DateTimeFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.DateTimeFormat dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.DateTimeFormat dependency property.
        /// </remarks>
        public static readonly DependencyProperty DateTimeFormatProperty =
            GridDependencyProperty.Register("DateTimeFormat", typeof(DateTimeFormatInfo), typeof(GridDateTimeColumn), new GridPropertyMetadata(DateTimeFormatInfo.CurrentInfo, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the minimum date value allowed in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the minimum date value in GridDateTimeColumn.
        /// </value>
        public DateTime MinDateTime
        {
            get { return (DateTime)GetValue(MinDateTimeProperty); }
            set { SetValue(MinDateTimeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MinDateTime dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MinDateTime dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinDateTimeProperty =
            DependencyProperty.Register("MinDateTime", typeof(DateTime), typeof(GridDateTimeColumn), new PropertyMetadata(System.DateTime.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum date value allowed in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the maximum date value in GridDateTimeColumn.
        /// </value>
        public DateTime MaxDateTime
        {
            get { return (DateTime)GetValue(MaxDateTimeProperty); }
            set { SetValue(MaxDateTimeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MaxDateTime dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.MaxDateTime dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxDateTimeProperty =
            DependencyProperty.Register("MaxDateTime", typeof(DateTime), typeof(GridDateTimeColumn), new PropertyMetadata(System.DateTime.MaxValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the custom pattern for date and time value.
        /// </summary>
        /// <value>
        /// The custom pattern for date and time value. The default value is string.Empty.
        /// </value>
        /// <remarks>
        /// To apply a CustomPattern, specify the <see cref="Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.Pattern"/> as <see cref="DateTimePattern.CustomPattern"/>.
        /// </remarks>
        public string CustomPattern
        {
            get { return (string)GetValue(CustomPatternProperty); }
            set { SetValue(CustomPatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.CustomPattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.CustomPattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty CustomPatternProperty =
            DependencyProperty.Register("CustomPattern", typeof(string), typeof(GridDateTimeColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that decides whether the date and time value can be edited.
        /// </summary>
        /// <value>
        /// <b>true</b> if the date and time value can be edited ; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.CanEdit dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.CanEdit dependency property.
        /// </remarks>
        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(GridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether a user can delete the date and time value by using Delete key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Delete key is enabled; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The <b>EnableDeleteKey</b> worked based on <see cref="Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.CanEdit"/> property.
        /// </remarks>
        public bool EnableBackspaceKey
        {
            get { return (bool)GetValue(EnableBackspaceKeyProperty); }
            set { SetValue(EnableBackspaceKeyProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.EnableBackspaceKey dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.EnableBackspaceKey dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableBackspaceKeyProperty =
            DependencyProperty.Register("EnableBackspaceKey", typeof(bool), typeof(GridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether a user can delete the date and time value by using Delete key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Delete key is enabled; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The <b>EnableDeleteKey</b> worked based on <see cref="Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.CanEdit"/> property.
        /// </remarks>
        public bool EnableDeleteKey
        {
            get { return (bool)GetValue(EnableDeleteKeyProperty); }
            set { SetValue(EnableDeleteKeyProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.EnableDeleteKey dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.EnableDeleteKey dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableDeleteKeyProperty =
            DependencyProperty.Register("EnableDeleteKey", typeof(bool), typeof(GridDateTimeColumn), new PropertyMetadata(false));

#endregion
#endif
        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed in GridDateTimeColumn; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullValue
        {
            get { return (bool)GetValue(AllowNullValueProperty); }
            set { SetValue(AllowNullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowNullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridDateTimeColumn.AllowNullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullValueProperty =
            GridDependencyProperty.Register("AllowNullValue", typeof(bool), typeof(GridDateTimeColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridDateTimeColumn"/> class.
        /// </summary>
        public GridDateTimeColumn()
        {
            CellType = "DateTime";
        }

#region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in GridDateTimeColumn.
        /// </summary>            
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;
            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;
#if WPF
            this.minDateTime = this.MinDateTime;
            this.maxDateTime = this.MaxDateTime;
            this.dateTimeFormat = this.DateTimeFormat;
            this.customPattern = this.CustomPattern;
            this.pattern = this.Pattern;
#else
            this.minDateTime = this.MinDate;
            this.maxDateTime = this.MaxDate;
            this.formatString = this.FormatString;
#endif
            if (binding.Converter is CultureFormatConverter)
                (binding.Converter as CultureFormatConverter).UpdateFormatProvider();
            base.UpdateBindingInfo();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 5 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, padTop, 2 + padRight, padBotton)
                           : new Thickness(3, 1, 2, 0);
#endif
        }
#endregion
    }

#if UWP
    /// <summary>
    /// Represents a column that displays the UpDown value in its cell content.
    /// </summary>        
    public class GridUpDownColumn : GridColumn
    {
#region Internal Properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal double minValue = double.MinValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal double maxValue = double.MaxValue;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal int numberDecimalDigits = NumberFormatInfo.CurrentInfo.NumberDecimalDigits;

#endregion

#region Dependency Properties
        /// <summary>
        /// Gets or sets a value indicating whether [block characters on text input].
        /// </summary>
        /// <value>
        /// <c>true</c> if [block characters on text input]; otherwise, <c>false</c>.
        /// </value>
        public bool BlockCharactersOnTextInput
        {
            get { return (bool)GetValue(BlockCharactersOnTextInputProperty); }
            set { SetValue(BlockCharactersOnTextInputProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.BlockCharactersOnTextInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.BlockCharactersOnTextInput dependency property.
        /// </remarks>
        public static readonly DependencyProperty BlockCharactersOnTextInputProperty =
            GridDependencyProperty.Register("BlockCharactersOnTextInput", typeof(bool), typeof(GridUpDownColumn), new GridPropertyMetadata(true));

#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether the user can rotate the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is rotated using the mouse wheel or up and down arrow key; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>
        public bool AllowScrollingOnCircle
        {
            get { return (bool)GetValue(AllowScrollingOnCircleProperty); }
            set { SetValue(AllowScrollingOnCircleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.AllowScrollingOnCircle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.AllowScrollingOnCircle dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowScrollingOnCircleProperty =
            GridDependencyProperty.Register("AllowScrollingOnCircle", typeof(bool), typeof(GridUpDownColumn), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a <see cref="System.Globalization.NumberFormatInfo"/> that defines the format of UpDown value.
        /// </summary>
        /// <value>
        /// A <see cref="System.Globalization.NumberFormatInfo"/> that defines the format of UpDown value.
        /// </value>
        public NumberFormatInfo NumberFormat
        {
            get { return (NumberFormatInfo)GetValue(NumberFormatProperty); }
            set { SetValue(NumberFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.NumberFormat dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.NumberFormat dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberFormatProperty =
            GridDependencyProperty.Register("NumberFormat", typeof(NumberFormatInfo), typeof(GridUpDownColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in GridUpDownColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed in GridUpDownColumn; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullValue
        {
            get { return (bool)GetValue(AllowNullValueProperty); }
            set { SetValue(AllowNullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.AllowNullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.AllowNullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullValueProperty =
            GridDependencyProperty.Register("AllowNullValue", typeof(bool), typeof(GridUpDownColumn), new GridPropertyMetadata(false, OnUpdateBindingInfo));
#endif

#if UWP
        /// <summary>
        /// Gets or sets the minimum allowed value for the GridUpDownColumn. 
        /// </summary>
        /// <value>
        /// The minimum allowed value for the GridUpDownColumn. The default value is double.MinValue.
        /// </value>
        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MinValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MinValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValueProperty =
            GridDependencyProperty.Register("MinValue", typeof(double), typeof(GridUpDownColumn), new GridPropertyMetadata(double.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum allowed value for the GridUpDownColumn. 
        /// </summary>
        /// <value>
        /// The maximum allowed value for the GridUpDownColumn. The default value is double.MaxValue.
        /// </value>
        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MaxValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MaxValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValueProperty =
            GridDependencyProperty.Register("MaxValue", typeof(double), typeof(GridUpDownColumn), new GridPropertyMetadata(double.MaxValue, OnUpdateBindingInfo));
#else
        /// <summary>
        /// Gets or sets the minimum allowed value for the GridUpDownColumn. 
        /// </summary>
        /// <value>
        /// The minimum allowed value for the GridUpDownColumn. The default value is decimal.MaxValue.
        /// </value>
        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MinValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MinValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValueProperty =
            GridDependencyProperty.Register("MinValue", typeof(decimal), typeof(GridUpDownColumn), new GridPropertyMetadata(decimal.MinValue, OnUpdateBindingInfo));

       /// <summary>
        /// Gets or sets the maximum allowed value for the GridUpDownColumn. 
        /// </summary>
        /// <value>
        /// The maximum allowed value for the GridUpDownColumn. The default value is decimal.MaxValue.
        /// </value>
        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MaxValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.MaxValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValueProperty =
            GridDependencyProperty.Register("MaxValue", typeof(decimal), typeof(GridUpDownColumn), new GridPropertyMetadata(decimal.MaxValue, OnUpdateBindingInfo));
#endif
        /// <summary>
        /// Gets or sets the number of decimal places to use in numeric values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in numeric values.
        /// </value>
        public int NumberDecimalDigits
        {
            get { return (int)GetValue(NumberDecimalDigitsProperty); }
            set { SetValue(NumberDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.NumberDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.NumberDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberDecimalDigitsProperty =
            GridDependencyProperty.Register("NumberDecimalDigits", typeof(int), typeof(GridUpDownColumn), new GridPropertyMetadata(NumberFormatInfo.CurrentInfo.NumberDecimalDigits, OnUpdateBindingInfo));

#if UWP
        /// <summary>
        /// Gets or sets a value that specifies whether the column automatically reverses the value when it reaches MinValue or MaxValue.
        /// </summary>
        /// <value>
        /// <b>true</b> if the column automatically reverses the value when it reaches the MinValue or MaxValue; otherwise, <b>false</b>.
        /// </value>
        public bool AutoReverse
        {
            get { return (bool)GetValue(AutoReverseProperty); }
            set { SetValue(AutoReverseProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.AutoReverse dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.AutoReverse dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoReverseProperty =
            GridDependencyProperty.Register("AutoReverse", typeof(bool), typeof(GridUpDownColumn), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a string that specifies how to format the bounded value in GridUpDownColumn.
        /// </summary>
        /// <value>
        /// A string that specifies how to format the bound value in GridUpDownColumn. The default value is string.Empty.
        /// </value>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.FormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.FormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatStringProperty =
            GridDependencyProperty.Register("FormatString", typeof(string), typeof(GridUpDownColumn), new GridPropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that decides whether the user can parse decimal or double value in GridUpDownColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.Input.Parsers"/> that specifies the parsing mode of GridUpDownColumn.
        /// The default mode is <b> Parsers.Double </b>.
        /// </value>
        public Parsers ParsingMode
        {
            get { return (Parsers)GetValue(ParsingModeProperty); }
            set { SetValue(ParsingModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.ParsingMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.ParsingMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty ParsingModeProperty =
            GridDependencyProperty.Register("ParsingMode", typeof(Parsers), typeof(GridUpDownColumn), new GridPropertyMetadata(Parsers.Double));

        /// <summary>
        /// Gets or sets a value to increment or decrement when Up and Down arrow key or mouse wheel is pressed.
        /// </summary>
        /// <value>
        /// The value to increment or decrement when Up and Down arrow key or mouse wheel is pressed. The default value is <b>1d</b>.
        /// </value>
        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.SmallChange dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.SmallChange dependency property.
        /// </remarks>
        public static readonly DependencyProperty SmallChangeProperty =
            GridDependencyProperty.Register("SmallChange", typeof(double), typeof(GridUpDownColumn), new GridPropertyMetadata(1d));

        /// <summary>
        /// Gets or sets a value to increment or decrement when PageUp and PageDown key is pressed.
        /// </summary>
        /// <value>
        /// The value to increment or decrement when PageUp and PageDown key is pressed. The default value is <b>1d</b>.
        /// </value>
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.LargeChange dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.LargeChange dependency property.
        /// </remarks>
        public static readonly DependencyProperty LargeChangeProperty =
            GridDependencyProperty.Register("LargeChange", typeof(double), typeof(GridUpDownColumn), new GridPropertyMetadata(1d));

        /// <summary>
        /// Gets or sets a <see cref="Syncfusion.UI.Xaml.Controls.SpinButtonsAlignment"/> that specifies the alignment of spin buttons in GridUpDownColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.SpinButtonsAlignment"/> that specifies the alignment of spin buttons in GridUpDownColumn.The default value is <see cref="Syncfusion.UI.Xaml.Controls.SpinButtonsAlignment.Right"/>.
        /// </value>
        public SpinButtonsAlignment SpinButtonsAlignment
        {
            get { return (SpinButtonsAlignment)GetValue(SpinButtonsAlignmentProperty); }
            set { SetValue(SpinButtonsAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.SpinButtonsAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUpDownColumn.SpinButtonsAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty SpinButtonsAlignmentProperty =
            GridDependencyProperty.Register("SpinButtonsAlignment", typeof(SpinButtonsAlignment), typeof(GridUpDownColumn), new GridPropertyMetadata(SpinButtonsAlignment.Right));

#endif

#endregion

#region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridUpDownColumn"/> class.
        /// </summary>
        public GridUpDownColumn()
        {
            CellType = "UpDown";
        }
#endregion

#region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the GridUpDownColumn.
        /// </summary> 
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.DataGrid == null)
                return;
            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;
            this.minValue = this.MinValue;
            this.maxValue = this.MaxValue;
            this.formatString = this.FormatString;
            base.UpdateBindingInfo();
        }

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/> of GridUpDownColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.DisplayBinding"/>.
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {
            if ((DisplayBinding as Binding).Converter == null)
                (DisplayBinding as Binding).Converter = new CultureFormatConverter(this);
        }
#endregion
    }

    /// <summary>
    /// Represents a column that used to display and edit boolean values and hosts ToggleSwitch as its cell content.
    /// </summary>
    public class GridToggleSwitchColumn : GridColumn
    {
#region Ctor
        public GridToggleSwitchColumn()
        {
            this.CellType = "ToggleSwitch";
        }
#endregion

#region Dependency properties               

        /// <summary>
        /// Gets or sets the text when the toggle in off state.
        /// </summary>
        /// <value>
        /// The displayed text when Toggle in off state.
        /// The default value is <b>string.Empty</b>.
        /// </value>
        public object OffContent
        {
            get { return (string)GetValue(OffContentProperty); }
            set { SetValue(OffContentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridToggleSwitchColumn.OffContent dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridToggleSwitchColumn.OffContent dependency property.
        /// </remarks>
        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register("OffContent", typeof(string), typeof(GridToggleSwitchColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the text when the toggle in on state.
        /// </summary>
        /// <value>
        /// The displayed text when Toggle in on state.
        /// The default value is <b>string.Empty</b>.
        /// </value>
        public object OnContent
        {
            get { return (object)GetValue(OnContentProperty); }
            set { SetValue(OnContentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridToggleSwitchColumn.OnContent dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridToggleSwitchColumn.OnContent dependency property.
        /// </remarks>
        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register("OnContent", typeof(object), typeof(GridToggleSwitchColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));


        /// <summary>
        /// Gets or sets the horizontal alignment for the column .
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.HorizontalAlignment"/> enumeration that specifies the horizontal alignment. The default value is <b>HorizontalAlignment.Stretch)</b>
        /// </value>
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridToggleSwitchColumn.HorizontalAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridToggleSwitchColumn.HorizontalAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            GridDependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(GridCheckBoxColumn), new GridPropertyMetadata(HorizontalAlignment.Center, OnUpdateBindingInfo));

#endregion

#region overrides

        protected internal override bool CanEndEditColumn()
        {
            return true;
        }

#endregion

    }

#endif
    /// <summary>
    /// Represents a column that displays the URI data in its cells content.
    /// </summary>
    public class GridHyperlinkColumn : GridTextColumn
    {
#region Ctor

        /// <summary>
        /// Gets or sets the horizontal alignment of the GridHyperLinkColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.HorizontalAlignment"/> enumeration that specifies the horizontal alignment of the GridHyperLinkColumn.
        /// The default is <see cref="System.Windows.HorizontalAlignment">Stretch</see>.
        /// </value>
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridHyperlinkColumn.HorizontalAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridHyperlinkColumn.HorizontalAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            GridDependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(GridHyperlinkColumn), new GridPropertyMetadata(HorizontalAlignment.Stretch, OnUpdateBindingInfo));

#region overrides
        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/> dependency property defined in GridHyperLinkColumn.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        /// <exception cref="System.NotSupportedException"/> Thrown when the <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/> is defined in GridHyperLinkColumn.
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            throw new NotSupportedException("The " + this.ToString() + " does not implement CellTemplate property");
        }
#endregion
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridHyperLinkColumn"/> class.
        /// </summary>
        public GridHyperlinkColumn()
        {
            CellType = "Hyperlink";
            Padding = new Thickness(2, 0, 2, 0);
        }
#endregion

#region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in GridHyperLinkColumn.
        /// </summary>                
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {

        }
        /// <summary>
        /// Determines whether the GridHyperLinkColumn can be editable.
        /// </summary>
        /// <returns>
        /// Returns <b>false</b> for the GridHyperLinkColumn .
        /// </returns>
        protected internal override bool CanEditCell(int rowIndex = -1)
        {
            bool isFilterRow = this.DataGrid.IsFilterRowIndex(rowIndex);
            return isFilterRow;
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            
        }

#endregion
    }
    //WPF-36114 Split the separate class for mainatain the Dispaly binding and value binding individually.

    /// <summary>
    /// Classs for the handles the display binding.
    /// </summary>
    internal class GridDisplayColumnWrapper : FrameworkElement
    {
        public GridDisplayColumnWrapper()
        {

        }

        internal string FormattedValue
        {
            get { return (string)GetValue(FormattedValueProperty); }
            set { SetValue(FormattedValueProperty, value); }
        }

        public static readonly DependencyProperty FormattedValueProperty = DependencyProperty.Register("FormattedValue", typeof(string), typeof(GridDisplayColumnWrapper), new PropertyMetadata(null));

#if WPF
        internal BindingExpressionBase ValueBindingExpression;
#endif

        public void ResetBindings()
        {
#if WPF
            ValueBindingExpression = null;
            BindingOperations.ClearBinding(this, GridDisplayColumnWrapper.FormattedValueProperty);
#else
            this.ClearValue(GridDisplayColumnWrapper.FormattedValueProperty);
          
#endif
        }
        public void SetDisplayBinding(BindingBase binding)
        {
#if WPF
            if (ValueBindingExpression != null)
            {
                this.ClearValue(FormattedValueProperty);
                ValueBindingExpression = null;
            }
            ValueBindingExpression = this.SetBinding(FormattedValueProperty, binding);
#else
            this.SetBinding(FormattedValueProperty, binding);
#endif
        }
    }

    internal class GridColumnWrapper : FrameworkElement
    {
        public GridColumnWrapper()
        {
        }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(GridColumnWrapper), new PropertyMetadata(null));

        public void SetValueBinding(BindingBase binding)
        {
#if WPF
            if (ValueBindingExpression != null)
            {
                this.ClearValue(ValueProperty);
                ValueBindingExpression = null;
            }
            ValueBindingExpression = this.SetBinding(ValueProperty, binding);
#else
            this.SetBinding(ValueProperty, binding);
#endif
        }

#if WPF
        internal BindingExpressionBase ValueBindingExpression;
#endif
        public void ResetBindings()
        {
#if WPF
            ValueBindingExpression = null;
            BindingOperations.ClearBinding(this, GridColumnWrapper.ValueProperty);
#else
            this.ClearValue(GridColumnWrapper.ValueProperty);
#endif
        }


    }
}

