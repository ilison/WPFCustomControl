#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using System.ComponentModel;
using Syncfusion.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Controls.Input;
using Syncfusion.UI.Xaml.Controls.Data;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a control that provides advanced filter options to filter the data.
    /// </summary>
#if UWP
    public class AdvancedFilterControl : ContentControl, IDisposable, INotifyPropertyChanged, IDataValidation
#else
    public class AdvancedFilterControl : ContentControl, IDisposable, INotifyPropertyChanged, IDataErrorInfo
#endif

    {
        #region private fields

        private string filterType1;
        private string filterType2;
        private object filterValue1;
        private object filterValue2;
        private bool isdisposed = false;
#if UWP
        bool datapickervisibility = true;
#endif
#if WPF
        bool casingbuttonvisibility = true;
#endif

        private object datefilterValue1;
        private object datefilterValue2;

        private object filterSelectedItem1;
        private object filterSelectedItem2;
        private ObservableCollection<FilterElement> comboSource;
        private bool? isORChecked = true;
        #endregion

        #region internal fields
        internal bool isCaseSensitive1 = false;
        internal bool isCaseSensitive2 = false;
        internal bool propertyChangedfromsettingControlValues = false;
        internal GridFilterControl gridFilterCtrl = null;
        #endregion

        #region CLR Properties

        /// <summary>
        /// Gets or sets a value that indicates the FilterType1 in AdvancedFilterControl.
        /// </summary>
        /// <value>
        /// A string that specifies FilterType1.
        /// </value> 
        public string FilterType1
        {
            get { return filterType1; }
            set
            {
                filterType1 = value;
                if (FilterType1 != null && (FilterType1.ToString() == GridResourceWrapper.Null || FilterType1.ToString() == GridResourceWrapper.NotNull
                    || FilterType1.ToString() == GridResourceWrapper.Empty || FilterType1.ToString() == GridResourceWrapper.NotEmpty))
                {
                    FilterValue1 = null;
                    FilterSelectedItem1 = null;
                }
                OnPropertyChanged("FilterType1");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the FilterType2 in AdvancedFilterControl.
        /// </summary>
        /// <value>
        /// A string that specifies FilterType2.
        /// </value> 
        public string FilterType2
        {
            get { return filterType2; }
            set
            {
                filterType2 = value;
                if (FilterType2 != null && (FilterType2.ToString() == GridResourceWrapper.Null || FilterType2.ToString() == GridResourceWrapper.NotNull
                    || FilterType2.ToString() == GridResourceWrapper.Empty || FilterType2.ToString() == GridResourceWrapper.NotEmpty))
                {
                    FilterValue2 = null;
                    FilterSelectedItem2 = null;
                }
                OnPropertyChanged("FilterType2");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the DateFilterValue1 in AdvancedFilterControl.
        /// </summary>
        /// <value>
        /// An object that specifies DateFilterValue1.
        /// </value> 
        public object DateFilterValue1
        {
            get { return datefilterValue1; }
            set
            {
                datefilterValue1 = value;
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    if (this.gridFilterCtrl != null)
                        FilterValue1 = this.gridFilterCtrl.GetFormattedString(value);
                }
                else
                    FilterValue1 = value;
                OnPropertyChanged("DateFilterValue1");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the DateFilterValue2 in AdvancedFilterControl.
        /// </summary>
        /// <value>
        /// An object that specifies DateFilterValue2.
        /// </value> 
        public object DateFilterValue2
        {
            get { return datefilterValue2; }
            set
            {

                datefilterValue2 = value;
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    if (this.gridFilterCtrl != null)
                        FilterValue2 = this.gridFilterCtrl.GetFormattedString(value);
                }
                else
                    FilterValue2 = value;
                OnPropertyChanged("DateFilterValue2");

            }
        }
        /// <summary>
        /// Gets or sets the Text of first Filter UIelement (UIElememt which support editing)
        /// </summary>
        public object FilterValue1
        {
            get { return filterValue1; }
            set
            {
                if (FilterValue1 != value)
                {
                    if ((FilterValue1 != null && !FilterValue1.Equals(value)) || FilterValue1 == null)
                    {
                        filterValue1 = value;
                        OnPropertyChanged("FilterValue1");
                    }
                    SetOkButtonState(filterValue1, FilterValue2);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Text of second Filter UIelement (UIElememt which support editing)
        /// </summary>
        public object FilterValue2
        {
            get { return filterValue2; }
            set
            {
                if (FilterValue2 != value)
                {
                    if ((FilterValue2 != null && !FilterValue2.Equals(value)) || FilterValue2 == null)
                    {
                        filterValue2 = value;
                        OnPropertyChanged("FilterValue2");
                    }
                    SetOkButtonState(FilterValue1, filterValue2);
                }
            }
        }
        /// <summary>
        /// Gets or sets the Text of first filter combo box selected item 
        /// </summary>
        public object FilterSelectedItem1
        {
            get { return filterSelectedItem1; }
            set
            {
                if (filterSelectedItem1 != value)
                {
                    filterSelectedItem1 = value;
                    OnPropertyChanged("FilterSelectedItem1");
                    SetOkButtonState(filterSelectedItem1, FilterSelectedItem2);
                }
            }
        }
        /// <summary>
        /// Gets or sets the Text of second filter combo box selected item 
        /// </summary>
        public object FilterSelectedItem2
        {
            get { return filterSelectedItem2; }
            set
            {
                if (filterSelectedItem2 != value)
                {
                    filterSelectedItem2 = value;
                    OnPropertyChanged("FilterSelectedItem2");
                    SetOkButtonState(filterSelectedItem2, FilterSelectedItem1);
                }
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="Syncfusion.UI.Xaml.Grid.FilterElement"/>.
        /// </summary>
        /// <value>
        /// The collection of FilterElement.
        /// </value>
        public ObservableCollection<FilterElement> ComboItemsSource
        {
            get
            {
                return comboSource;
            }
            set
            {
                comboSource = value;
                OnPropertyChanged("ComboItemsSource");
            }
        }
#if UWP
        public bool DatePickerVisibility
        {
            get
            {
                return datapickervisibility;
            }
            set
            {
                datapickervisibility = value;
                OnPropertyChanged("DatePickerVisibility");
            }
        }
#endif
#if WPF
        /// <summary>
        /// Gets or sets a value indicating whether CasingButton should be visible or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if CasingButton is visible; otherwise,<b>false</b>. The default value is <b>true</b>. 
        /// </value>
        public bool CasingButtonVisibility
        {
            get
            {
                return casingbuttonvisibility;
            }
            set
            {
                casingbuttonvisibility = value;
                OnPropertyChanged("CasingButtonVisibility");
            }
        }
#endif
        //WRT-5505 while opening a filter popup(Advanced filter only) facing an issue like "failed to assign to property 'system.windows.controls.primitives.togglebutton.checked'" in all the themes
        // since the type of ToggleButton.IsChecked is NullableBool Type.Hence this type has been changed as Nullable Bool type.

        /// <summary>
        /// Gets or sets a value indicating whether OR in radio button is checked or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if OR in radio button is checked; otherwise,<b>false</b>. The default value is <b>true</b>. 
        /// </value>
        public bool? IsORChecked
        {
            get { return isORChecked; }
            set { isORChecked = value; OnPropertyChanged("IsORChecked"); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether first CaseSensitive button is clicked or not 
        /// </summary>
        /// <value>
        /// <b>true</b> if CaseSensitive button is clicked; otherwise,<b>false</b>. The default value is <b>false</b>. 
        /// </value>
        public bool IsCaseSensitive1
        {
            get { return isCaseSensitive1; }
            set
            {
                isCaseSensitive1 = value;
                RefreshCasingButton1State();
                OnPropertyChanged("IsCaseSensitive1");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether second CaseSensitive button is clicked or not 
        /// </summary>
        /// <value>
        /// <b>true</b> if CaseSensitive button is clicked; otherwise,<b>false</b>. The default value is <b>false</b>. 
        /// </value>
        public bool IsCaseSensitive2
        {
            get { return isCaseSensitive2; }
            set
            {
                isCaseSensitive2 = value;
                RefreshCasingButton2State();
                OnPropertyChanged("IsCaseSensitive2");
            }
        }

        #endregion

        #region Dependency Properties

        #region CanGenerateUniqueItems
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl.CanGenerateUniqueItems dependency property.
        /// </summary>       
        public static readonly DependencyProperty CanGenerateUniqueItemsProperty = DependencyProperty.Register(
          "CanGenerateUniqueItems", typeof(bool), typeof(AdvancedFilterControl), new PropertyMetadata(true, OnCanGenerateUniqueItemsChanged));
        private static void OnCanGenerateUniqueItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#if UWP
            var advancedFilterControl = d as AdvancedFilterControl;
            if ((bool)e.NewValue == false)
                advancedFilterControl.DatePickerVisibility = false;
            else
                advancedFilterControl.DatePickerVisibility = true;           
#endif
        }

        /// <summary>
        /// Gets or sets a value indicating whether all the unique items in the column are loaded or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if ComboBox is loaded in Advanced filter; <b>false</b> if TextBox is loaded that allows you to manually enter text for filtering. 
        /// The default value is <b>true</b>.
        /// </value>        
        public bool CanGenerateUniqueItems
        {
            get { return (bool)this.GetValue(AdvancedFilterControl.CanGenerateUniqueItemsProperty); }
            set { this.SetValue(AdvancedFilterControl.CanGenerateUniqueItemsProperty, value); }
        }
        #endregion

        #region FilterTypeComboItems
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl.FilterTypeComboItems dependency property.
        /// </summary>  
        public static readonly DependencyProperty FilterTypeComboItemsProperty = DependencyProperty.Register(
          "FilterTypeComboItems", typeof(object), typeof(AdvancedFilterControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the ItemsSource for Filter type ComboBox.
        /// </summary>        
        public object FilterTypeComboItems
        {
            get { return (object)this.GetValue(AdvancedFilterControl.FilterTypeComboItemsProperty); }
            set { this.SetValue(AdvancedFilterControl.FilterTypeComboItemsProperty, value); }
        }
        #endregion

        #endregion

        #region Ctor
        public AdvancedFilterControl()
        {
            this.DefaultStyleKey = typeof(AdvancedFilterControl);
        }

        #endregion

        #region UIElements

        ToggleButton CasingButton1;
        ToggleButton CasingButton2;
#if WPF
        DatePicker datePicker1;
        DatePicker datePicker2;
        ComboBox MenuComboBox1;
        ComboBox MenuComboBox2;
#else
        SfDatePicker datePicker1;
        SfDatePicker datePicker2;
        SfComboBox MenuComboBox1;
        SfComboBox MenuComboBox2;
        TextBox textBox1;
        TextBox textBox2;
#endif
        RadioButton radioButton1;
        RadioButton radioButton2;

        #endregion

        #region Overrides

        /// <summary>
        /// Builds the visual tree for the AdvancedFilterControl when a new template is applied.
        /// </summary>
#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            UnWireEvents();
            base.OnApplyTemplate();
            CasingButton1 = this.GetTemplateChild("PART_CasingButton1") as ToggleButton;
            CasingButton2 = this.GetTemplateChild("PART_CasingButton2") as ToggleButton;
#if WPF
            MenuComboBox1 = this.GetTemplateChild("PART_MenuComboBox1") as ComboBox;
            MenuComboBox2 = this.GetTemplateChild("PART_MenuComboBox2") as ComboBox;
            datePicker1 = this.GetTemplateChild("PART_DatePicker1") as DatePicker;
            datePicker2 = this.GetTemplateChild("PART_DatePicker2") as DatePicker;
#else
            MenuComboBox1 = this.GetTemplateChild("PART_MenuComboBox1") as SfComboBox;
            MenuComboBox2 = this.GetTemplateChild("PART_MenuComboBox2") as SfComboBox;
            datePicker1 = this.GetTemplateChild("PART_DatePicker1") as SfDatePicker;
            datePicker2 = this.GetTemplateChild("PART_DatePicker2") as SfDatePicker;
            textBox1 = this.GetTemplateChild("PART_TextBox1") as TextBox;
            textBox2 = this.GetTemplateChild("PART_TextBox2") as TextBox;
#endif
            radioButton1 = this.GetTemplateChild("PART_RadioButton1") as RadioButton;
            radioButton2 = this.GetTemplateChild("PART_RadioButton2") as RadioButton;

            WireEvents();
            GenerateFilterTypeComboItems();
            RefreshCasingButton1State();
            RefreshCasingButton2State();
        }
#if WPF
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (datePicker1 != null && datePicker1.IsDropDownOpen)
                datePicker1.IsDropDownOpen = false;
            if (datePicker2 != null && datePicker2.IsDropDownOpen)
                datePicker2.IsDropDownOpen = false;
        }
#endif

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.AdvancedFilterControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnWireEvents();
            if (isDisposing)
            {
                this.gridFilterCtrl = null;
                if (this.ComboItemsSource != null)
                {
                    this.ComboItemsSource.Clear();
                    this.ComboItemsSource = null;
                }
                this.FilterTypeComboItems = null;
            }
            isdisposed = true;
        }

        #endregion

        #region Methods
        private void ApplyImmediateFilters()
        {
            if (this.gridFilterCtrl == null || !this.gridFilterCtrl.ImmediateUpdateColumnFilter) return;
            var error1 = this["FilterValue1"];
            var error2 = this["FilterValue2"];
            if (!string.IsNullOrEmpty(error1) || !string.IsNullOrEmpty(error2))
                return;
            this.gridFilterCtrl.InvokeFilter();
        }
        /// <summary>
        /// Get the first filter value.
        /// </summary>
        /// <returns>Filter value.</returns>
        public virtual object GetFirstFilterValue()
        {
            return this.GetFilterValue(FilterValue1, DateFilterValue1, FilterSelectedItem1);
        }

        /// <summary>
        /// Get the second filter value.
        /// </summary>
        /// <returns>Filter value.</returns>
        public virtual object GetSecondFilterValue()
        {
            return this.GetFilterValue(FilterValue2, DateFilterValue2, FilterSelectedItem2);        
        }
        
        /// <summary>
        /// Invokes to get Filter value 
        /// </summary>
        /// <param name="filterValue">FilterVValue</param>
        /// <param name="dateFilterValue">DateFilterValue</param>
        /// <param name="filterSelectedItem"> FilterSelectedItem</param>
        /// <returns></returns>
        private object GetFilterValue(object filterValue, object dateFilterValue, object filterSelectedItem)
        {
            if (this.gridFilterCtrl == null)
                return filterValue;

            if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter && this.ColumnDataType != typeof(object))
#if WPF
                return filterValue;
#else
            {
                if (!this.CanGenerateUniqueItems)
                    return filterValue;
                if (filterSelectedItem != null && filterSelectedItem is FilterElement)
                    return (filterSelectedItem as FilterElement).DisplayText;
            }
#endif
            else
            {
#if UWP
                if (filterSelectedItem != null && this.CanGenerateUniqueItems)
#else
                if (filterSelectedItem is FilterElement &&
                    (filterSelectedItem as FilterElement).DisplayText.Equals(filterValue))
#endif
                    return GetFilterElementValue(filterSelectedItem);
                else
                {
                    if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter)
                    {
#if WPF
                        //WPF-30566 if filtervalue1 is equales of selected date time's formatted string
                        //then filter value is corresponding underling date type instead of string type.
                        if (CanGenerateUniqueItems && dateFilterValue != null)
                        {
                            if (this.gridFilterCtrl.GetFormattedString(dateFilterValue).Equals(FilterValue1))
                                return dateFilterValue;
                            else
                                return filterValue;
                        }
#else
                        //In UWP combo box is not editable so no need to check formatted text simply return DateFitlerValue1
                        if (CanGenerateUniqueItems && dateFilterValue != null)
                            return dateFilterValue;
#endif
                        else
                        {
                            // when CanGenerateUniqueItems false, entered values are need to convert as DateTime if possible                          
                            if (filterValue != null && !filterValue.Equals(string.Empty))
                            {
                                var binding = (this.gridFilterCtrl.Column.DisplayBinding as Binding);

                                //in CanGenerateUniqueItems False  with  custom DateTime  Format, while change  date either  using TextBox or DateTimePicker,
                                //must value is convert back to its default DateTime Format for to check  whether the value is valid or not
                                if (binding != null && binding.Converter != null && (binding.Converter is GridValueConverter) && (binding.Converter as GridValueConverter).CanConvertBack())
                                {
#if UWP
                                    return binding.Converter.ConvertBack(filterValue, null, binding.ConverterParameter, binding.ConverterLanguage);
#else
                                    return binding.Converter.ConvertBack(filterValue, null, binding.ConverterParameter, binding.ConverterCulture);
#endif
                                }
                                else if (TypeConverterHelper.CanConvert(typeof(DateTime?), filterValue.ToString()))
                                    return DateTime.Parse(filterValue.ToString());
                            }
                            return filterValue;
                        }
                    }
                    return filterValue;
                }
            }
#if UWP
            return filterValue;
#endif
        }

        internal void GenerateFilterTypeComboItems()
        {
            ObservableCollection<String> items = new ObservableCollection<string>();
            if (this.gridFilterCtrl != null)
            {
                if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter)
                {
                    VisualStateManager.GoToState(this, "TextFilter", true);
                    items.Add(GridResourceWrapper.Equalss);
                    items.Add(GridResourceWrapper.NotEquals);
#if UWP
                if (!this.CanGenerateUniqueItems)
                {
#endif
                    items.Add(GridResourceWrapper.BeginsWith);
                    items.Add(GridResourceWrapper.EndsWith);
                    items.Add(GridResourceWrapper.Contains);
#if UWP
                }
#endif
                    items.Add(GridResourceWrapper.Empty);
                    items.Add(GridResourceWrapper.NotEmpty);
                    if (this.gridFilterCtrl.Column != null && this.gridFilterCtrl.Column.AllowBlankFilters)
                    {
                        items.Add(GridResourceWrapper.Null);
                        items.Add(GridResourceWrapper.NotNull);
                    }
                }
                else if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.NumberFilter)
                {
                    VisualStateManager.GoToState(this, "NumberFilter", true);
                    items.Add(GridResourceWrapper.Equalss);
                    items.Add(GridResourceWrapper.NotEquals);
                    if (this.gridFilterCtrl.Column != null && this.gridFilterCtrl.Column.AllowBlankFilters)
                    {
                        items.Add(GridResourceWrapper.Null);
                        items.Add(GridResourceWrapper.NotNull);
                    }
                    items.Add(GridResourceWrapper.LessThan);
                    items.Add(GridResourceWrapper.LessThanorEqual);
                    items.Add(GridResourceWrapper.GreaterThan);
                    items.Add(GridResourceWrapper.GreaterThanorEqual);
                }
                else if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter)
                {
                    VisualStateManager.GoToState(this, "DateFilter", true);
                    items.Add(GridResourceWrapper.Equalss);
                    items.Add(GridResourceWrapper.NotEquals);
                    items.Add(GridResourceWrapper.Before);
                    items.Add(GridResourceWrapper.BeforeOrEqual);
                    items.Add(GridResourceWrapper.After);
                    items.Add(GridResourceWrapper.AfterOrEqual);
                    if (this.gridFilterCtrl.Column != null && this.gridFilterCtrl.Column.AllowBlankFilters)
                    {
                        items.Add(GridResourceWrapper.Null);
                        items.Add(GridResourceWrapper.NotNull);
                    }
                }
            }
            else
            {
                items.Add(GridResourceWrapper.Null);
                items.Add(GridResourceWrapper.NotNull);
            }
            if (FilterTypeComboItems == null)
                FilterTypeComboItems = items;
            else
            {
                var hasblankfilters = (FilterTypeComboItems as ObservableCollection<string>).Any(s => s.Equals(GridResourceWrapper.Null));
                if (this.gridFilterCtrl != null && this.gridFilterCtrl.Column != null)
                {
                    if (!this.gridFilterCtrl.Column.AllowBlankFilters && hasblankfilters)
                    {
                        (FilterTypeComboItems as ObservableCollection<string>).Remove(GridResourceWrapper.Null);
                        (FilterTypeComboItems as ObservableCollection<string>).Remove(GridResourceWrapper.NotNull);
                    }
                    else if (this.gridFilterCtrl.Column.AllowBlankFilters && !hasblankfilters)
                    {
                        (FilterTypeComboItems as ObservableCollection<string>).Add(GridResourceWrapper.Null);
                        (FilterTypeComboItems as ObservableCollection<string>).Add(GridResourceWrapper.NotNull);
                    }
                }
            }
        }

        internal void MaintainAPIChanges()
        {
            if (this.gridFilterCtrl.AdvancedFilterStyle != null)
                this.Style = this.gridFilterCtrl.AdvancedFilterStyle;
        }

        private object GetFilterElementValue(object filtervalue)
        {
            if (filtervalue != null)
            {
                if (filtervalue is FilterElement)
                    return (filtervalue as FilterElement).ActualValue;
                return filtervalue;
            }
            else return null;
        }

        private object GetFilterElementDisplayValue(object filtervalue)
        {
            if (filtervalue == null || comboSource == null)
                return null;
#if WPF
            var value = comboSource.FirstOrDefault(element => element != null && element.ActualValue != null && element.ActualValue.Equals(filtervalue));
            if (value != null)
                return value.DisplayText;
#else
            if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter)
            {
                var value = comboSource.FirstOrDefault(element => element != null && element.ActualValue != null && element.DisplayText.Equals(filtervalue));
                if (value != null)
                    return value;
            }
            else
            {
                object value = null;
                if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter && this.gridFilterCtrl.Column.ColumnFilter == ColumnFilter.DisplayText)
                    value = comboSource.FirstOrDefault(element => element != null && element.DisplayText != null && element.DisplayText.Equals(filtervalue));
                else
                    value = comboSource.FirstOrDefault(element => element != null && element.ActualValue != null && element.ActualValue.Equals(filtervalue));
                // For Win RT datepicker issue filterElement is not supported(cast exception)
                if (value != null && this.CanGenerateUniqueItems)
                    return value;
            }
#endif
            return filtervalue;
        }

        internal void SetAdvancedFilterControlValues(ObservableCollection<FilterPredicate> fp)
        {
            propertyChangedfromsettingControlValues = true;
            if (comboSource == null)
            {
                if (fp.Count == 1)
                {
                    FilterType2 = GridResourceWrapper.Equalss;
                    FilterValue2 = null;
                    DateFilterValue2 = null;
                }
                propertyChangedfromsettingControlValues = false;
                return;
            }
#if WPF
            string emptyStringValue = string.Empty;
            if (this.gridFilterCtrl.Column is GridMaskColumn)
            {
                var column = this.gridFilterCtrl.Column as GridMaskColumn;
                emptyStringValue = MaskedEditorModel.GetMaskedText(column.Mask, string.Empty,
                    column.DateSeparator,
                    column.TimeSeparator,
                    column.DecimalSeparator,
                    NumberFormatInfo.CurrentInfo.NumberGroupSeparator,
                    column.PromptChar,
                    NumberFormatInfo.CurrentInfo.CurrencySymbol);
            }
#endif
            if (fp.Count > 0)
            {
#if WPF
                if (this.gridFilterCtrl != null &&
                    this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter)
#else
                 if (this.gridFilterCtrl != null &&
                    this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter && !CanGenerateUniqueItems)
#endif
                {
                    if (this.gridFilterCtrl != null && fp[0].FilterValue != null)
                        FilterValue1 = this.gridFilterCtrl.GetFormattedString(fp[0].FilterValue);
                }
                else
                    FilterValue1 = GetFilterElementDisplayValue(fp[0].FilterValue);
#if UWP
                FilterSelectedItem1 = FilterValue1;
                if (FilterValue1 is FilterElement)
                    FilterValue1 = (FilterValue1 as FilterElement).DisplayText;
#endif
                FilterType1 = FilterHelpers.GetResourceWrapper(fp[0].FilterType, FilterValue1);
#if WPF
                if (this.gridFilterCtrl.Column is GridMaskColumn && FilterValue1 != null)
                {
                    if (FilterValue1.Equals(emptyStringValue))
                        FilterType1 = GridResourceWrapper.Empty;
                }
#endif
                IsCaseSensitive1 = fp[0].IsCaseSensitive;
                if (fp.Count == 1)
                {
                    FilterType2 = GridResourceWrapper.Equalss;
                    FilterValue2 = null;
                    DateFilterValue2 = null;
                }
            }
            if (fp.Count == 2)
            {
#if WPF
                if (this.gridFilterCtrl != null &&
                    this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter)
#else
                if (this.gridFilterCtrl != null &&
                   this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter && !CanGenerateUniqueItems)
#endif
                {
                    if (this.gridFilterCtrl != null && fp[1].FilterValue != null)
                        FilterValue2 = this.gridFilterCtrl.GetFormattedString(fp[1].FilterValue);
                }
                else
                    FilterValue2 = GetFilterElementDisplayValue(fp[1].FilterValue);
#if UWP
                FilterSelectedItem2 = FilterValue2;
                if (FilterValue2 is FilterElement)
                    FilterValue2 = (FilterValue2 as FilterElement).DisplayText;
#endif
                FilterType2 = FilterHelpers.GetResourceWrapper(fp[1].FilterType, FilterValue2);
#if WPF
                if (this.gridFilterCtrl.Column is GridMaskColumn && FilterValue2 != null)
                {
                    if (FilterValue2.Equals(emptyStringValue))
                        FilterType2 = GridResourceWrapper.Empty;
                }
#endif
                IsCaseSensitive2 = fp[1].IsCaseSensitive;
                isORChecked = fp[1].PredicateType == PredicateType.Or ? true : false;
            }
            propertyChangedfromsettingControlValues = false;
        }

        internal void ResetAdvancedFilterControlValues()
        {
            propertyChangedfromsettingControlValues = true;
            FilterValue1 = null;
            FilterValue2 = null;

            DateFilterValue1 = null;
            DateFilterValue2 = null;

            FilterSelectedItem1 = null;
            FilterSelectedItem2 = null;
            FilterType1 = GridResourceWrapper.Equalss;
            FilterType2 = GridResourceWrapper.Equalss;
            IsORChecked = true;
            if (CasingButton1 != null)
            {
                IsCaseSensitive1 = false;
            }
            if (CasingButton2 != null)
            {
                IsCaseSensitive2 = false;
            }
            propertyChangedfromsettingControlValues = false;
        }

        #endregion

        #region Events
       
        private void RefreshCasingButton1State()
        {
            if (CasingButton1 == null)
                return;

            if (IsCaseSensitive1)
                VisualStateManager.GoToState(CasingButton1, "CaseSensitive", true);
            else
                VisualStateManager.GoToState(CasingButton1, "NotCaseSensitive", true);
        }

       
        private void RefreshCasingButton2State()
        {
            if (CasingButton2 == null)
                return;

            if (IsCaseSensitive2)
                VisualStateManager.GoToState(CasingButton2, "CaseSensitive", true);
            else
                VisualStateManager.GoToState(CasingButton2, "NotCaseSensitive", true);
        }

#if UWP
        void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.gridFilterCtrl.ImmediateUpdateColumnFilter)
            {
                if (textBox.Equals(textBox1))
                    FilterValue1 = textBox.Text;
                else
                    FilterValue2 = textBox.Text;
            }
            string error = string.Empty;
            object text = textBox.Text;
            var binding = (this.gridFilterCtrl.Column.DisplayBinding as Binding);
            //in CanGenerateUniqueItems False  with  custom DateTime  Format, while change  date either  using TextBox or DateTimePicker,
            //must value is convert back to its default DateTime Format for to check  whether the value is valid or not
            if (this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter && binding != null 
                    && binding.Converter != null && (binding.Converter is GridValueConverter) && (binding.Converter as GridValueConverter).CanConvertBack())
                text = binding.Converter.ConvertBack(text, null, binding.ConverterParameter, binding.ConverterLanguage);

            if (!IsValidFilterValue(text))
                error = GridResourceWrapper.EnterValidFilterValue;
            if (textBox.Equals(textBox1))
                ErrorMessage1 = error;
            else
                ErrorMessage2 = error;
            if (!string.IsNullOrEmpty(error))
                VisualStateManager.GoToState(textBox, "HasError", true);
            else
            {
                VisualStateManager.GoToState(textBox, "NoError", true);
                if (textBox.Equals(textBox1) && (textBox.Text).Length <= 1)
                    SetOkButtonState(textBox.Text, FilterValue2);
                else if ((textBox.Text).Length <= 1)
                    SetOkButtonState(FilterValue1, textBox.Text);
            }
        }

#endif
        void OnRadioButtonClick(object sender, RoutedEventArgs e)
        {
            ApplyImmediateFilters();
        }
#if WPF
        private void OnDatePickerLostFocus(object sender, RoutedEventArgs e)
        {
            var datepicker = sender as DatePicker;
            if (datepicker != null && !datepicker.IsKeyboardFocusWithin)
                datepicker.IsDropDownOpen = false;
        }
        void OnDatePickerMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
#endif

#if WPF
        void OnMenuComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
#else
        void OnMenuComboBoxSelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
#endif
        {
#if UWP
            var comboBox = sender as SfComboBox;
#else
            var comboBox = sender as ComboBox;
#endif
            //if filter has any values or filter type is particular null or empty type then only enable the ok button otherwise disable it WPF-17915
            if (ComboItemsSource != null && comboBox.SelectedValue != null && gridFilterCtrl.OkButton != null)
            {
                gridFilterCtrl.OkButton.IsEnabled = (IsFilterHasvalues() || ((IsNullOrEmptyFilterType(FilterType1) || IsNullOrEmptyFilterType(FilterType2)))) ? true : false; 
            }

            bool cancelFiltering = this.gridFilterCtrl.Column.DataGrid.FilterRowPosition != FilterRowPosition.None
                                   && this.gridFilterCtrl.Column.FilteredFrom == FilteredFrom.FilterRow
                                   && ((comboBox == this.MenuComboBox1 && this.FilterSelectedItem1 == null) ||
                                    (comboBox == this.MenuComboBox2 && this.FilterSelectedItem2 == null));
            // WRT-4458 - while loading AdvancedFilterControl, menu combo box selction is changed. But ApplyImmediateFilters should not be called as it clears the previously applied filters(if any). 
            //so RemovedItems count is checked whether selected item is set firt time
            if (comboBox.SelectedValue != null && !propertyChangedfromsettingControlValues && !cancelFiltering
                && (e.RemovedItems != null && e.RemovedItems.Count > 0))
                ApplyImmediateFilters();
        }
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (gridFilterCtrl != null && gridFilterCtrl.IsInSuspend)
                return;
#if WPF
            if ((propertyName == "FilterValue1" || propertyName == "FilterValue2") && !propertyChangedfromsettingControlValues)
                ApplyImmediateFilters();
#endif
#if UWP
            if ((propertyName == "FilterSelectedItem1" || propertyName == "FilterSelectedItem2") && !propertyChangedfromsettingControlValues)
                ApplyImmediateFilters();

            if (!CanGenerateUniqueItems && (propertyName == "FilterValue1" || propertyName == "FilterValue2") && !propertyChangedfromsettingControlValues)
                ApplyImmediateFilters();
#endif
            if (PropertyChanged != null)
            {
                if (propertyName == "ComboItemsSource")
                {
                    propertyChangedfromsettingControlValues = true;
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    propertyChangedfromsettingControlValues = false;
                }
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region WireEvents

        private void WireEvents()
        {
            this.Loaded += AdvancedFilterControl_Loaded;
            if (MenuComboBox1 != null) MenuComboBox1.SelectionChanged += OnMenuComboBoxSelectionChanged;
            if (MenuComboBox2 != null) MenuComboBox2.SelectionChanged += OnMenuComboBoxSelectionChanged;
#if WPF
            if (datePicker1 != null)
            {
                datePicker1.LostFocus += OnDatePickerLostFocus;
                datePicker1.MouseDown += OnDatePickerMouseDown;
            }
            if (datePicker2 != null)
            {
                datePicker2.LostFocus += OnDatePickerLostFocus;
                datePicker2.MouseDown += OnDatePickerMouseDown;

            }
#endif
            if (radioButton1 != null) radioButton1.Click += OnRadioButtonClick;
            if (radioButton2 != null) radioButton2.Click += OnRadioButtonClick;
#if UWP
            if (textBox1 != null)
                textBox1.TextChanged += OnTextBoxTextChanged;
            if (textBox2 != null)
                textBox2.TextChanged += OnTextBoxTextChanged;
#endif
        }

        void AdvancedFilterControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.RefreshCasingButton1State();
            this.RefreshCasingButton2State();
        }


        #endregion

        #region UnwireEvents
        private void UnWireEvents()
        {
            this.Loaded -= AdvancedFilterControl_Loaded;
            if (MenuComboBox1 != null) MenuComboBox1.SelectionChanged -= OnMenuComboBoxSelectionChanged;
            if (MenuComboBox2 != null) MenuComboBox2.SelectionChanged -= OnMenuComboBoxSelectionChanged;
#if WPF
            if (datePicker1 != null)
            {
                datePicker1.LostFocus -= OnDatePickerLostFocus;
                datePicker1.MouseDown -= OnDatePickerMouseDown;
            }
            if (datePicker2 != null)
            {
                datePicker2.LostFocus -= OnDatePickerLostFocus;
                datePicker2.MouseDown -= OnDatePickerMouseDown;
            }
#endif
            if (radioButton1 != null) radioButton1.Click -= OnRadioButtonClick;
            if (radioButton2 != null) radioButton2.Click -= OnRadioButtonClick;
#if UWP
            if (textBox1 != null)
                textBox1.TextChanged -= OnTextBoxTextChanged;
            if (textBox2 != null)
                textBox2.TextChanged -= OnTextBoxTextChanged;
#endif
        }
        #endregion
        #endregion

        #region IDataErrorInfo

        internal Type ColumnDataType;
        public string Error
        {
            get { return null; }
        }
#if UWP
        private string errorMessage1 = string.Empty;
        public string ErrorMessage1
        {
            get
            {
                return errorMessage1;
            }
            set
            {
                errorMessage1 = value;
                OnPropertyChanged("ErrorMessage1");
            }
        }

        private string errorMessage2 = string.Empty;
        public string ErrorMessage2
        {
            get
            {
                return errorMessage2;
            }
            set
            {
                errorMessage2 = value;
                OnPropertyChanged("ErrorMessage2");
            }
        }
#endif
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                object value = null;
                if (columnName == "FilterValue1")
                {
                    value = GetFirstFilterValue();
                    if (!IsValidFilterValue(value))
                    {
#if UWP
                        ErrorMessage1 = GridResourceWrapper.EnterValidFilterValue;
#endif
                        return GridResourceWrapper.EnterValidFilterValue;
                    }
                }
                else if (columnName == "FilterValue2")
                {
                    value = GetSecondFilterValue();
                    if (!IsValidFilterValue(value))
                    {
#if UWP
                        ErrorMessage2 = GridResourceWrapper.EnterValidFilterValue;
#endif
                        return GridResourceWrapper.EnterValidFilterValue;
                    }
                }

                return result;
            }
        }

        private bool IsValidFilterValue(object value)
        {
            if (ColumnDataType == typeof(object) || this.gridFilterCtrl != null && this.gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter)
                return true;
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
                return TypeConverterHelper.CanConvert(ColumnDataType, value.ToString());
            return true;
        }

        /// <summary>
        /// Returns true if filter type is null,notnull,empty or not empty
        /// </summary>
        /// <param name="filtertype"></param>
        /// <returns>bool</returns>
        internal bool IsNullOrEmptyFilterType(object type)
        {
            string filtertype = string.Empty;
            if (type == null)
                return false;
            else
                filtertype = type.ToString();
            if (filtertype != null && (filtertype == GridResourceWrapper.Null || filtertype == GridResourceWrapper.NotNull
                    || filtertype == GridResourceWrapper.Empty || filtertype == GridResourceWrapper.NotEmpty))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if any one filter has values
        /// </summary>
        /// <returns>bool</returns>
        internal bool IsFilterHasvalues()
        {
            var firstvalue = GetFirstFilterValue();
            var secondvalue = GetSecondFilterValue();
            if ((firstvalue == null||string.IsNullOrEmpty(firstvalue.ToString())) && (secondvalue == null||string.IsNullOrEmpty(secondvalue.ToString())))
                return false;
           
            return true;
        }

        /// <summary>
        /// Enable or Disable Advance filter OK Button
        /// </summary>
        /// <param name="filtervalue1"></param>
        /// <param name="filtervalue2"></param>
        /// <returns>void</returns>
        internal void SetOkButtonState( object filtervalue1,object filtervalue2)
        {
            //WRT-4569 While disposing grid, we set gridfilterctrl as null. we need to check gridFilterctrl as null or not.            
            if (this.gridFilterCtrl == null)
                return;

            //WRT-4382 Use case of this condition is ok button is enabled when filter has values. 
            //so filter ok button is set to enable if the filter type has NUll,NOTNULL,EMPTY,NOTEMPTY types 
            //or any one of the filter combo box has values.
            if (((filtervalue1 != null && !string.IsNullOrEmpty(filtervalue1.ToString())) || (filtervalue2 != null && !string.IsNullOrEmpty(filtervalue2.ToString()))) || (this.IsNullOrEmptyFilterType(FilterType1) || this.IsNullOrEmptyFilterType(FilterType2)) && this.gridFilterCtrl.OkButton != null)
            {
                this.gridFilterCtrl.OkButton.IsEnabled = true;
            }
            else if ((((filtervalue1 == null || string.IsNullOrEmpty(filtervalue1.ToString())) && (filtervalue2 == null || string.IsNullOrEmpty(filtervalue2.ToString())))) || (this.IsNullOrEmptyFilterType(FilterType1) || this.IsNullOrEmptyFilterType(FilterType2)) && this.gridFilterCtrl.OkButton != null)
            {
                this.gridFilterCtrl.OkButton.IsEnabled = false;
            }
        }
        #endregion
    }
}
