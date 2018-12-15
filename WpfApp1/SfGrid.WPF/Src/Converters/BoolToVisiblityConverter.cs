#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Dynamic;
using Syncfusion.UI.Xaml.TreeGrid;
#if UWP
using System.Collections;
using System.Globalization;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Linq;
using System.Windows;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public class BoolToVisiblityConverter : IValueConverter
    {

#if UWP
        /// <summary>
        /// Converts the bool value to Visiblity
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter != null && parameter.Equals("InverseVisiblity") && (bool)value)
                return Visibility.Collapsed;
            else if (parameter != null && parameter.Equals("InverseVisiblity") && !(bool)value)
                return Visibility.Visible;
            else if ((bool)value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts the visibity value to bool
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;
            return false;
        }
#else
        /// <summary>
        /// Converts the bool value to Visiblity
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter != null && parameter.Equals("InverseVisiblity") && (bool)value)
                return Visibility.Collapsed;
            else if (parameter != null && parameter.Equals("InverseVisiblity") && !(bool)value)
                return Visibility.Visible;
            else if ((bool)value)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts the visibity value to bool
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;
            return false;
        }
#endif

    }

    /// <summary>
    /// Derived from IValueConverter which returns display text by reflection data based on DisplayMemberPath and SelectedValuePath
    /// </summary>
    public class DisplayMemberConverter : IValueConverter, IDisposable
    {
        public DisplayMemberConverter()
        {

        }

        /// <summary>
        /// Helper to reflect dynamic types. This maintains the cache for faster reflection
        /// </summary>
        protected DynamicHelper dynamicHelper;
        public DisplayMemberConverter(GridColumnBase column)
        {
            cachedColumn = column;
        }

        private GridColumnBase cachedColumn;

#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
#endif

        protected virtual object Convert(object selectedValue)
        {
            IEnumerable list = null;
            var displayMemberPath = string.Empty;
            var valueMemberPath = string.Empty;
            if (cachedColumn is GridComboBoxColumn)
            {
                var column = cachedColumn as GridComboBoxColumn;
                cachedColumn = column;
                list = column.itemsSource as IEnumerable;
                displayMemberPath = column.displayMemberPath;
                valueMemberPath = column.valueMemberPath;
            }

            else if (cachedColumn is TreeGridComboBoxColumn)
            {
                var column = cachedColumn as TreeGridComboBoxColumn;
                cachedColumn = column;
                list = column.ItemsSource as IEnumerable;
                displayMemberPath = column.DisplayMemberPath;
                valueMemberPath = column.SelectedValuePath;
            }

            else if (cachedColumn is GridMultiColumnDropDownList)
            {
                var column = cachedColumn as GridMultiColumnDropDownList;
                cachedColumn = column;
                list = column.itemsSource as IEnumerable;
                displayMemberPath = column.displayMemberPath;
                valueMemberPath = column.valueMemberPath;
            }

            if (selectedValue == null)
                return null;

            bool? isdynamic = null;
#if WPF
            PropertyDescriptorCollection pdc = null;
            bool? isdatarow = null;
#else
            PropertyInfoCollection pdc = null;
#endif

            if (string.IsNullOrEmpty(valueMemberPath))
            {
                if (!string.IsNullOrEmpty(displayMemberPath))
                {
                    var type = selectedValue.GetType();
                    isdynamic = isdynamic ?? DynamicHelper.CheckIsDynamicObject(type);
                    if (isdynamic == true)
                    {
                        if (dynamicHelper == null)
                            dynamicHelper = new DynamicHelper();
                        return dynamicHelper.GetValue(selectedValue, displayMemberPath);
                    }
#if WPF
                    //WPF - 35357 - Support to bind DataTable as ComboBoxColumn ItemsSource provided.
                    isdatarow = isdatarow ?? type == typeof(System.Data.DataRowView);
                    if (isdatarow == true)
                    {
                        return (selectedValue as System.Data.DataRowView)[displayMemberPath];
                    }
                    pdc = TypeDescriptor.GetProperties(type);
#else
                    pdc = new PropertyInfoCollection(type);
#endif
                    return pdc.GetValue(selectedValue, displayMemberPath);
                }
                return selectedValue;
            }
            else
            {
                if (list == null)
                    return null;

                var enumerator = list.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var type = enumerator.Current.GetType();

                    isdynamic = isdynamic ?? DynamicHelper.CheckIsDynamicObject(type);
                    if (isdynamic == true)
                    {
                        if (dynamicHelper == null)
                            dynamicHelper = new DynamicHelper();
                        if (selectedValue.Equals(dynamicHelper.GetValue(enumerator.Current, valueMemberPath)))
                        {
                            if (!string.IsNullOrEmpty(displayMemberPath))
                                return dynamicHelper.GetValue(enumerator.Current, displayMemberPath);
                            return enumerator.Current;
                        }
                        continue;
                    }
#if WPF
                    //WPF - 35357 - Support to bind DataTable as ComboBoxColumn ItemsSource provided.
                    isdatarow = isdatarow ?? type == typeof(System.Data.DataRowView);
                    if (isdatarow == true)
                    {
                        var datarowview = (enumerator.Current as System.Data.DataRowView);
                        if (selectedValue.Equals(datarowview[valueMemberPath]))
                            return datarowview[displayMemberPath];
                        continue;
                    }
                    pdc = pdc ?? TypeDescriptor.GetProperties(type);
#else
                    pdc = pdc ?? new PropertyInfoCollection(type);
#endif
                    if (selectedValue.Equals(pdc.GetValue(enumerator.Current, valueMemberPath)))
                    {
                        if (!string.IsNullOrEmpty(displayMemberPath))
                            return pdc.GetValue(enumerator.Current, displayMemberPath);
                        return enumerator.Current;
                    }
                }
            }
            return null;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }       
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                dynamicHelper.Dispose();
                dynamicHelper = null;
            }
        }
    }

    /// <summary>
    /// Represents as class derived from IValueConverter which provide customized ConvertBack method by using CanConvertBack.
    /// </summary>
    public class GridValueConverter : IValueConverter
    {

        /// <summary>
        /// Initialize the new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridValueConverter"/> class.
        /// </summary>
        public GridValueConverter()
        {

        }

        /// <summary>
        /// Determines whether the ConvertBack method is need to execute.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the specified column index is RowHeader; otherwise , <b>false</b>.
        /// </returns>
        protected internal virtual bool CanConvertBack()
        {
            return true;
        }
#if UWP   
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
#else
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
#endif
    }

    /// <summary>
    /// Derived from GridValueConverter which return formatted value for corresponding column. 
    /// </summary>
    internal class ValueBindingConverter : GridValueConverter
    {
        protected GridColumnBase cachedColumn;
        public ValueBindingConverter(GridColumnBase column)
        {
            cachedColumn = column;
        }
         protected internal override bool CanConvertBack()
        {
            return true;
        }

#if UWP
        public override object Convert(object value,Type targetType,object parameter, string language)
        {
            throw new NotImplementedException();
        }
         public override object ConvertBack(object value,Type targetType,object parameter, string language)
        {
            throw new NotImplementedException();
        }
#else
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (CanConvertBack())
            {
                if (cachedColumn is GridNumericColumn)
                {
                    var column = (cachedColumn as GridNumericColumn);
                    if (column != null)
                    {
                        if (value == null)
                            return value;

                        if (column.ParsingMode == ParseMode.Int)
                        {
                            int _columnValue;
                            if (int.TryParse(value.ToString(), out _columnValue))
                                return _columnValue;
                            return value;
                        }
                        if (column.ParsingMode == ParseMode.Decimal)
                        {
                            decimal _columnValue;
                            if (decimal.TryParse(value.ToString(), out _columnValue))
                                return _columnValue;
                            return value;
                        }
                        if ((column.ParsingMode == ParseMode.Double))
                        {
                            double _columnValue;
                            if (double.TryParse(value.ToString(), out _columnValue))
                                return _columnValue;
                            return value;
                        }
                    }
                }
                else if (cachedColumn is TreeGridNumericColumn)
                {
                    var column = (cachedColumn as TreeGridNumericColumn);
                    if (column != null)
                    {
                        if (value == null)
                            return value;

                        if (column.ParsingMode == ParseMode.Int)
                        {
                            int _columnValue;
                            if (int.TryParse(value.ToString(), out _columnValue))
                                return _columnValue;
                            return value;
                        }
                        if (column.ParsingMode == ParseMode.Decimal)
                        {
                            decimal _columnValue;
                            if (decimal.TryParse(value.ToString(), out _columnValue))
                                return _columnValue;
                            return value;
                        }
                        if ((column.ParsingMode == ParseMode.Double))
                        {
                            double _columnValue;
                            if (double.TryParse(value.ToString(), out _columnValue))
                                return _columnValue;
                            return value;
                        }
                    }

                }
            }
               
            return value;
        }
#endif

    }

    public class CultureFormatConverter : GridValueConverter
    {
        private GridColumnBase cachedColumn;
#if WPF
        internal IFormatProvider formatProvider;
#else
        internal string formatProvider;
#endif
        public CultureFormatConverter(GridColumnBase column)
        {
            cachedColumn = column;
            formatProvider = GetFormatProvider(cachedColumn);
        }

        /// <summary>
        /// It return true if need to convert back the value otherwise false
        /// </summary>
        /// <returns>bool</returns>
        protected internal override bool CanConvertBack()
        {
            if (cachedColumn is GridDateTimeColumn)
                return true;

            return false;
        }
#if WPF
        /// <summary>
        /// Invokes to convert actual value into formatted value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">convert parameter</param>
        /// <param name="CultureInfo">culture Info</param>
        /// <returns>object</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string formatValue = string.Empty;
            decimal columnValue;
            if (value == null || DBNull.Value == value)
            {                                                                                      
                if (cachedColumn is GridTimeSpanColumn)
                {
                    var column = (cachedColumn as GridTimeSpanColumn);
                    TimeSpan _columnValue;
                    if (column.AllowNull && column.NullText != string.Empty)
                        return column.NullText;
                    else if (column.AllowNull && column.NullText == string.Empty && column.minValue == System.TimeSpan.MinValue
                        || (column.AllowNull && column.NullText == string.Empty && column.minValue != System.TimeSpan.MinValue))
                        return null;

                    TimeSpan.TryParse(column.minValue != System.TimeSpan.MinValue &&  !column.AllowNull? column.minValue.ToString() : new TimeSpan(0, 0, 0, 0, 0).ToString(), out _columnValue);

                    return GridCellTimeSpanRenderer.DisplayText(_columnValue, column.format);                    
                }
                else if (cachedColumn is GridDateTimeColumn)
                {
                    var column = cachedColumn as GridDateTimeColumn;
                    if (column.AllowNullValue && column.NullValue == null)
                        return column.NullText;

                    var datetime = DateTime.Now;
                    if (column.AllowNullValue && column.NullValue != null)
                        datetime = column.NullValue.Value;

                    if (column.maxDateTime != System.DateTime.MaxValue && datetime > column.maxDateTime)
                        datetime = column.maxDateTime;

                    if (column.minDateTime != System.DateTime.MinValue && datetime < column.minDateTime)
                        datetime = column.minDateTime;

                    return FilterHelpers.DateTimeFormatString(column, datetime);
                }
                else if (cachedColumn is GridEditorColumn)
                {
                    var column = cachedColumn as GridEditorColumn;
                    if (column.AllowNullValue && column.NullValue != null)
                    {
                        decimal.TryParse(column.NullValue.ToString(), out columnValue);
                        return ConvertToFormat(column,columnValue);
                    }
                    else if (column.AllowNullValue && column.NullText != string.Empty)
                        return column.NullText;
                }                   
                    return null;
            }           
            var _value = value.ToString();

            if (cachedColumn is GridCurrencyColumn)
            {
                decimal.TryParse(value.ToString(), out columnValue);
                formatValue = ConvertToFormat(cachedColumn as GridCurrencyColumn, columnValue);
            }
            else if (cachedColumn is GridPercentColumn)
            {
                decimal.TryParse(value.ToString(), out columnValue);
                formatValue = ConvertToFormat(cachedColumn as GridPercentColumn, columnValue);
            }
            else if (cachedColumn is GridNumericColumn)
            {
                decimal.TryParse(value.ToString(), out columnValue);
                formatValue = ConvertToFormat(cachedColumn as GridNumericColumn, columnValue);
            }
            else if (cachedColumn is GridDateTimeColumn)
            {
                DateTime _columnValue;
                var column = (GridDateTimeColumn)cachedColumn;
                bool canParse = true;
                if (value is DateTime)
                    _columnValue = (DateTime)value;
                else
                {
                    // WPF-23652 if _value cannot be parsed to DateTime,current DateTime will be set 
                    if (!DateTime.TryParse(_value, this.formatProvider,
                                        DateTimeStyles.AdjustToUniversal, out _columnValue))
                    {
                        _columnValue = DateTime.Now;
                        canParse = false;
                    }
                }

                if (canParse)
                {
                    if (_columnValue < column.minDateTime)
                        _columnValue = column.minDateTime;
                    if (_columnValue > column.maxDateTime)
                        _columnValue = column.maxDateTime;
                }
                return FilterHelpers.DateTimeFormatString(column, _columnValue);
            }
            else if (cachedColumn is GridMaskColumn)
            {
                var column = cachedColumn as GridMaskColumn;
                Decimal.TryParse(_value, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out columnValue);
                if (column.IsNumeric)
                    return columnValue.ToString(column.Mask, NumberFormatInfo.CurrentInfo);
                else
                {                   
                    var dateSeparator = string.IsNullOrEmpty(column.DateSeparator) ? DateTimeFormatInfo.CurrentInfo.DateSeparator : column.DateSeparator;
                    var timeSeparator = string.IsNullOrEmpty(column.TimeSeparator) ? DateTimeFormatInfo.CurrentInfo.TimeSeparator : column.TimeSeparator;
                    var decimalSeparator = string.IsNullOrEmpty(column.DecimalSeparator) ? NumberFormatInfo.CurrentInfo.NumberDecimalSeparator : column.DecimalSeparator;
                    string maskValue = MaskedEditorModel.GetMaskedText(column.Mask, _value,
                                                             dateSeparator,
                                                             timeSeparator,
                                                             decimalSeparator,
                                                             NumberFormatInfo.CurrentInfo.NumberGroupSeparator, 
                                                             column.PromptChar,
                                                             NumberFormatInfo.CurrentInfo.CurrencySymbol);

                    if (_value == string.Empty)
                        return string.Empty;
                    else
                        return maskValue;
                }
            }
            else if (cachedColumn is GridTimeSpanColumn)
            {
                TimeSpan _columnValue;
                var column = cachedColumn as GridTimeSpanColumn;
                TimeSpan.TryParse(_value, out _columnValue);
                if (_columnValue < column.minValue)
                    _columnValue = column.minValue;
                if (_columnValue > column.maxValue)
                    _columnValue = column.maxValue;
                return GridCellTimeSpanRenderer.DisplayText(_columnValue, column.format);
            }
            return formatValue;
        }

        /// <summary>
        /// Invokes to Return formatted string for corresponding column 
        /// </summary>
        /// <param name="column">GridEditorColumn</param>
        /// <param name="columnValue">column value</param>
        /// <returns>foramtted string</returns>
        internal string ConvertToFormat(GridEditorColumn column, decimal columnValue)
        {
            if (columnValue < column.minValue)
                columnValue = column.minValue;
            if (columnValue > column.maxValue)
                columnValue = column.maxValue;

            if (column is GridCurrencyColumn)
                return columnValue.ToString("C" , this.formatProvider);      
            else if(column is GridPercentColumn)
            {
                var percentColumn = column as GridPercentColumn;
                if (percentColumn.percentEditMode == PercentEditMode.DoubleMode)
                    columnValue /= 100;
                return columnValue.ToString("P", this.formatProvider);
            }
            else if(column is GridNumericColumn)
                return columnValue.ToString("N", this.formatProvider);

            return null;
        }

        /// <summary>
        ///  Invokes to convert back the converted value into actual value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="CultureInfo">culture Info</param>
        /// <returns>object</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (CanConvertBack())
            {
                //If Custom DateTime format is used in GridDateTimeColumn, then we need to  convert back from
                //its custom pattern to default pattern to identify the value is valid.
                if (cachedColumn is GridDateTimeColumn)
                {
                    DateTime _columnValue;
                    var column = (GridDateTimeColumn)cachedColumn;
                    if (!string.IsNullOrEmpty(column.CustomPattern) && DateTime.TryParseExact(value.ToString(),
                        column.CustomPattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out _columnValue))
                        return _columnValue;

                    return value;
                }
            }

            return null;
        }
#else
        /// <summary>
        /// Invokes to convert actual value into formatted value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">convert parameter</param>
        /// <param name="language">language</param>
        /// <returns>object</returns>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            string formatValue = string.Empty;
            Double columnValue;
            if (value == null)
            {
                if (cachedColumn is GridDateTimeColumn)
                {
                    var column = cachedColumn as GridDateTimeColumn;
                    if (column.AllowNullValue)
                        return column.WaterMark;
                }
                else if (cachedColumn is GridNumericColumn)
                {
                    var column = cachedColumn as GridNumericColumn;
                    if (column.AllowNullInput)
                        return column.WaterMark;
                }
                return null;
            }

            var _value = value.ToString();
            if (cachedColumn is GridNumericColumn)
            {
                Decimal columnvalue;
                var column = cachedColumn as GridNumericColumn;
                if (column.parsingMode == Parsers.Double)
                {
                    Double.TryParse(_value, out columnValue);
                    if (string.Equals(this.formatProvider, "P") && column.percentDisplayMode == PercentDisplayMode.Value)
                        columnValue /= 100;
                    formatValue = columnValue.ToString(this.formatProvider, CultureInfo.CurrentUICulture);
                }
                else
                {
                    Decimal.TryParse(_value, out columnvalue);
                    if (string.Equals(this.formatProvider, "P") && column.percentDisplayMode == PercentDisplayMode.Value)
                        columnvalue /= 100;
                    formatValue = columnvalue.ToString(this.formatProvider, CultureInfo.CurrentUICulture);
                }
            }

            if (cachedColumn is GridDateTimeColumn)
            {
                DateTime _columnValue;
                var column = (GridDateTimeColumn)cachedColumn;
                DateTime.TryParse(_value, CultureInfo.CurrentUICulture, DateTimeStyles.AdjustToUniversal, out _columnValue);

                if (_columnValue < column.minDateTime)
                    _columnValue = column.minDateTime;
                if (_columnValue > column.maxDateTime)
                    _columnValue = column.maxDateTime;

                return formatValue = _columnValue.ToString(this.formatProvider, CultureInfo.CurrentUICulture);
            }
            else if (cachedColumn is GridUpDownColumn)
            {
                var column = cachedColumn as GridUpDownColumn;

                double _columnValue = double.MinValue;
                Double.TryParse(_value, NumberStyles.Float, CultureInfo.CurrentUICulture, out _columnValue);

                if (_columnValue < column.minValue)
                    _columnValue = column.minValue;
                if (_columnValue > column.maxValue)
                    _columnValue = column.maxValue;

                var numericNumberFormatInfo = new NumberFormatInfo
                {
                    NumberDecimalDigits = column.numberDecimalDigits
                };
                return _columnValue.ToString("N", numericNumberFormatInfo);
            }

            return formatValue;
        }

        /// <summary>
        ///  Invokes to convert back the converted value into actual value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="language">language</param>
        /// <returns>object</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (this.CanConvertBack())
            {
                //If Custom DateTime format is used in GridDateTimeColumn, then we need to  convert back from
                //its custom pattern to default pattern to identify the value is valid.
                if (cachedColumn is GridDateTimeColumn)
                {
                    DateTime _columnValue;
                    var column = (GridDateTimeColumn)cachedColumn;
                    if (!string.IsNullOrEmpty(column.FormatString) && DateTime.TryParseExact(value.ToString(),
                        column.FormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out _columnValue))
                        return _columnValue;

                    return value;
                }
            }

            throw new NotImplementedException();
        }
#endif

        /// <summary>
        /// Returns IFormatProvider for GridColumn
        /// </summary>
        /// <param name="column">GridColumnBase</param>
        /// <returns>IFormatProvider</returns>
#if WPF
        private IFormatProvider GetFormatProvider(GridColumnBase column)
#else
        private string GetFormatProvider(GridColumnBase column)
#endif
        {
#if WPF
            if (column is GridCurrencyColumn)
            {
                var currencyColumn = cachedColumn as GridCurrencyColumn;
                return FormatConverterHelper.GetCurrencyColumnFormatProvider(currencyColumn);
            }
            else if (column is GridPercentColumn)
            {
                var percentColumn = cachedColumn as GridPercentColumn;
                return FormatConverterHelper.GetPercentColumnFormatProvider(percentColumn);
            }
            else if (column is GridNumericColumn)
            {
                var numericColumn = cachedColumn as GridNumericColumn;
                return FormatConverterHelper.GetNumericColumnFormatProvider(numericColumn);
            }
            else if (column is GridDateTimeColumn)
            {
                var datetimeColumn = cachedColumn as GridDateTimeColumn;
                return datetimeColumn.dateTimeFormat;
            }
            return null;
#else
            if (cachedColumn is GridNumericColumn)
                return (cachedColumn as GridNumericColumn).FormatString;
            if (cachedColumn is GridDateTimeColumn)
                return (cachedColumn as GridDateTimeColumn).FormatString;

            return string.Empty;
#endif
        }
       
        /// <summary>
        /// Invokes to update FormatProvider field in CultureFormatConverter
        /// </summary>
        internal void UpdateFormatProvider()
        {
            this.formatProvider = GetFormatProvider(cachedColumn);
        }
    }

    internal class TreeGridCultureFormatConverter : GridValueConverter
    {
        private GridColumnBase cachedColumn;

#if WPF
        internal IFormatProvider formatProvider;
#else
        internal string formatProvider;
#endif

        public TreeGridCultureFormatConverter(GridColumnBase column)
        {
            cachedColumn = column;
            formatProvider = GetFormatProvider(cachedColumn);
        }

        /// <summary>
        /// It return true if need to convert back the value otherwise false
        /// </summary>
        /// <returns>bool</returns>
        protected internal override bool CanConvertBack()
        {
            return false;
        }
#if WPF
        /// <summary>
        /// Invokes to convert actual value into formatted value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">convert parameter</param>
        /// <param name="CultureInfo">culture Info</param>
        /// <returns>object</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string formatValue = string.Empty;
            decimal columnValue;
            if (value == null || DBNull.Value == value)
            {               
                if (cachedColumn is TreeGridDateTimeColumn)
                {
                    var column = cachedColumn as TreeGridDateTimeColumn;
                    if (column.AllowNullValue && column.NullValue == null)
                        return column.NullText;

                    var datetime = DateTime.Now;
                    if (column.AllowNullValue && column.NullValue != null)
                        datetime = column.NullValue.Value;

                    if (column.MaxDateTime != System.DateTime.MaxValue && datetime > column.MaxDateTime)
                        datetime = column.MaxDateTime;

                    if (column.MinDateTime != System.DateTime.MinValue && datetime < column.MinDateTime)
                        datetime = column.MinDateTime;

                    return FilterHelpers.DateTimeFormatString(column, datetime);
                }
                else if (cachedColumn is TreeGridEditorColumn)
                {
                    var column = cachedColumn as TreeGridEditorColumn;
                    if (column.AllowNullValue && column.NullValue != null)
                    {
                        decimal.TryParse(column.NullValue.ToString(), out columnValue);
                        return ConvertToFormat(column, columnValue);
                    }
                    else if (column.AllowNullValue && column.NullText != string.Empty)
                        return column.NullText;
                }
                return null;
            }
            var _value = value.ToString();
            
            if (cachedColumn is TreeGridCurrencyColumn)
            {
                var column = cachedColumn as TreeGridCurrencyColumn;
                decimal.TryParse(value.ToString(), out columnValue);

                if (columnValue < column.MinValue)
                    columnValue = column.MinValue;
                if (columnValue > column.MaxValue)
                    columnValue = column.MaxValue;
                formatValue = ConvertToFormat(column, columnValue);
            }
            else if (cachedColumn is TreeGridPercentColumn)
            {
                var column = cachedColumn as TreeGridPercentColumn;
                decimal.TryParse(value.ToString(), out columnValue);

                if (columnValue < column.MinValue)
                    columnValue = column.MinValue;
                if (columnValue > column.MaxValue)
                    columnValue = column.MaxValue;
                formatValue = ConvertToFormat(column, columnValue);
            }
            else if (cachedColumn is TreeGridNumericColumn)
            {
                var column = cachedColumn as TreeGridNumericColumn;
                decimal.TryParse(value.ToString(), out columnValue);

                if (columnValue < column.MinValue)
                    columnValue = column.MinValue;
                if (columnValue > column.MaxValue)
                    columnValue = column.MaxValue;
                formatValue = ConvertToFormat(column, columnValue);
            }           
            else if (cachedColumn is TreeGridDateTimeColumn)
            {
                DateTime _columnValue;
                var column = (TreeGridDateTimeColumn)cachedColumn;
                bool canParse = true;
                if (value is DateTime)
                    _columnValue = (DateTime)value;
                else
                {
                    // WPF-23652 if _value cannot be parsed to DateTime,current DateTime will be set 
                    if (!DateTime.TryParse(_value, (cachedColumn as TreeGridDateTimeColumn).DateTimeFormat,
                                        DateTimeStyles.AdjustToUniversal, out _columnValue))
                    {
                        _columnValue = DateTime.Now;
                        canParse = false;
                    }
                }

                if (canParse)
                {
                    if (_columnValue < column.MinDateTime)
                        _columnValue = column.MinDateTime;
                    if (_columnValue > column.MaxDateTime)
                        _columnValue = column.MaxDateTime;
                }
                return FilterHelpers.DateTimeFormatString(column, _columnValue);
            }
            else if (cachedColumn is TreeGridMaskColumn)
            {
                var column = cachedColumn as TreeGridMaskColumn;
                Decimal.TryParse(_value, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out columnValue);
                if (column.IsNumeric)
                    return columnValue.ToString(column.Mask, NumberFormatInfo.CurrentInfo);
                else
                {
                    var dateSeparator = string.IsNullOrEmpty(column.DateSeparator) ? DateTimeFormatInfo.CurrentInfo.DateSeparator : column.DateSeparator;
                    var timeSeparator = string.IsNullOrEmpty(column.TimeSeparator) ? DateTimeFormatInfo.CurrentInfo.TimeSeparator : column.TimeSeparator;
                    var decimalSeparator = string.IsNullOrEmpty(column.DecimalSeparator) ? NumberFormatInfo.CurrentInfo.NumberDecimalSeparator : column.DecimalSeparator;

                    string maskValue = MaskedEditorModel.GetMaskedText(column.Mask, _value,
                                                             dateSeparator,
                                                             timeSeparator,
                                                             decimalSeparator,
                                                             NumberFormatInfo.CurrentInfo.NumberGroupSeparator,
                                                             column.PromptChar,
                                                             NumberFormatInfo.CurrentInfo.CurrencySymbol);

                    if (_value == string.Empty)
                        return string.Empty;
                    else
                        return maskValue;
                }
            }
            return formatValue;
        }
        private string ConvertToFormat(TreeGridEditorColumn column, decimal columnValue)
        {
            if (column is TreeGridCurrencyColumn)
            {
                var currencyColumn = cachedColumn as TreeGridCurrencyColumn;
                var currencyNumberFormatInfo = new NumberFormatInfo
                {
                    CurrencyDecimalDigits = currencyColumn.CurrencyDecimalDigits,
                    CurrencyDecimalSeparator = currencyColumn.CurrencyDecimalSeparator,
                    CurrencyGroupSeparator = currencyColumn.CurrencyGroupSeparator,
                    CurrencyNegativePattern = currencyColumn.CurrencyNegativePattern,
                    CurrencyPositivePattern = currencyColumn.CurrencyPositivePattern,
                    CurrencySymbol = currencyColumn.CurrencySymbol,
                    CurrencyGroupSizes = currencyColumn.CurrencyGroupSizes.ToArray(),
                };
                return columnValue.ToString("C", currencyNumberFormatInfo);
            }
            else if (column is TreeGridNumericColumn)
            {
                var numericColumn = cachedColumn as TreeGridNumericColumn;
                var numericNumberFormatInfo = new NumberFormatInfo
                {
                    NumberDecimalDigits = numericColumn.NumberDecimalDigits,
                    NumberDecimalSeparator = numericColumn.NumberDecimalSeparator,
                    NumberGroupSeparator = numericColumn.NumberGroupSeparator,
                    NumberNegativePattern = numericColumn.NumberNegativePattern,
                    NumberGroupSizes = numericColumn.NumberGroupSizes.ToArray(),
                };
                return columnValue.ToString("N", numericNumberFormatInfo);
            }
            else if (column is TreeGridPercentColumn)
            {
                var percentColumn = cachedColumn as TreeGridPercentColumn;

                if (percentColumn.PercentEditMode == PercentEditMode.DoubleMode)
                    columnValue /= 100;

                var percentFormatInfo = new NumberFormatInfo
                {
                    PercentDecimalDigits = percentColumn.PercentDecimalDigits,
                    PercentDecimalSeparator = percentColumn.PercentDecimalSeparator,
                    PercentGroupSeparator = percentColumn.PercentGroupSeparator,
                    PercentNegativePattern = percentColumn.PercentNegativePattern,
                    PercentPositivePattern = percentColumn.PercentPositivePattern,
                    PercentSymbol = percentColumn.PercentSymbol,
                    PercentGroupSizes = percentColumn.PercentGroupSizes.ToArray(),
                };
                return columnValue.ToString("P", percentFormatInfo);
            }
            return null;
        }       

        /// <summary>
        ///  Invokes to convert back the converted value into actual value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="CultureInfo">culture Info</param>
        /// <returns>object</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
#else
        /// <summary>
        /// Invokes to convert actual value into formatted value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">convert parameter</param>
        /// <param name="language">language</param>
        /// <returns>object</returns>
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            string formatValue = string.Empty;
            Double columnValue;
            if (value == null)
            {
                if (cachedColumn is TreeGridDateTimeColumn)
                {
                    var column = cachedColumn as TreeGridDateTimeColumn;
                    if (column.AllowNullValue)
                        return column.WaterMark;
                }                
                else if (cachedColumn is TreeGridNumericColumn)
                {
                    var column = cachedColumn as TreeGridNumericColumn;
                    if (column.AllowNull)
                        return column.WaterMark;
                }
                return null;
            }

            var _value = value.ToString();
            if (cachedColumn is TreeGridNumericColumn)
            {
                Decimal columnvalue;
                var column = cachedColumn as TreeGridNumericColumn;
                if (column.ParsingMode == Parsers.Double)
                {
                    Double.TryParse(_value, out columnValue);
                    if (string.Equals(this.formatProvider, "P") && column.PercentDisplayMode == PercentDisplayMode.Value)
                        columnValue /= 100;
                    formatValue = columnValue.ToString(this.formatProvider, CultureInfo.CurrentUICulture);
                }
                else
                {
                    Decimal.TryParse(_value, out columnvalue);
                    if (string.Equals(this.formatProvider, "P") && column.PercentDisplayMode == PercentDisplayMode.Value)
                        columnvalue /= 100;
                    formatValue = columnvalue.ToString(this.formatProvider, CultureInfo.CurrentUICulture);
                }
            }

            if (cachedColumn is TreeGridDateTimeColumn)
            {
                DateTime _columnValue;
                var column = (TreeGridDateTimeColumn)cachedColumn;
                DateTime.TryParse(_value, CultureInfo.CurrentUICulture, DateTimeStyles.AdjustToUniversal, out _columnValue);

                if (_columnValue < column.MinDate)
                    _columnValue = column.MinDate;
                if (_columnValue > column.MaxDate)
                    _columnValue = column.MaxDate;

                return formatValue = _columnValue.ToString(this.formatProvider, CultureInfo.CurrentUICulture);
            }
            return formatValue;
        }

        /// <summary>
        ///  Invokes to convert back the converted value into actual value
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">target type</param>
        /// <param name="parameter">converter parameter</param>
        /// <param name="language">language</param>
        /// <returns>object</returns>
        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (this.CanConvertBack())
            {
                //If Custom DateTime format is used in GridDateTimeColumn, then we need to  convert back from
                //its custom pattern to default pattern to identify the value is valid.
                if (cachedColumn is TreeGridDateTimeColumn)
                {
                    DateTime _columnValue;
                    var column = (TreeGridDateTimeColumn)cachedColumn;
                    if (!string.IsNullOrEmpty(column.FormatString) && DateTime.TryParseExact(value.ToString(),
                        column.FormatString, CultureInfo.InvariantCulture, DateTimeStyles.None, out _columnValue))
                        return _columnValue;

                    return value;
                }
            }

            throw new NotImplementedException();
        }
#endif

        /// <summary>
        /// Returns IFormatProvider for GridColumn
        /// </summary>
        /// <param name="column">GridColumnBase</param>
        /// <returns>IFormatProvider</returns>
#if WPF
        private IFormatProvider GetFormatProvider(GridColumnBase column)
#else
        private string GetFormatProvider(GridColumnBase column)
#endif
        {
#if WPF
            if (column is TreeGridCurrencyColumn)
            {
                var currencyColumn = cachedColumn as TreeGridCurrencyColumn;
                return FormatConverterHelper.GetCurrencyColumnFormatProvider(currencyColumn);
            }
            else if (column is TreeGridPercentColumn)
            {
                var percentColumn = cachedColumn as TreeGridPercentColumn;
                return FormatConverterHelper.GetPercentColumnFormatProvider(percentColumn);
            }
            else if (column is TreeGridNumericColumn)
            {
                var numericColumn = cachedColumn as TreeGridNumericColumn;
                return FormatConverterHelper.GetNumericColumnFormatProvider(numericColumn);
            }
            else if (column is TreeGridDateTimeColumn)
            {
                var datetimeColumn = cachedColumn as TreeGridDateTimeColumn;
                return datetimeColumn.dateTimeFormat;
            }
            return null;
#else
            if (cachedColumn is TreeGridNumericColumn)
                return (cachedColumn as TreeGridNumericColumn).FormatString;
            if (cachedColumn is TreeGridDateTimeColumn)
                return (cachedColumn as TreeGridDateTimeColumn).FormatString;

            return string.Empty;
#endif
        }

        /// <summary>
        /// Invokes to update FormatProvider field in CultureFormatConverter
        /// </summary>
        internal void UpdateFormatProvider()
        {
            this.formatProvider = GetFormatProvider(cachedColumn);
        }
    }

    public class TextAlignmentToHorizontalAlignmentConverter : IValueConverter
    {
#if WPF
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object Convert(object value, Type targetType, object parameter, string language)
#endif
        {
            var textAlignment = value is TextAlignment ? (TextAlignment)value : TextAlignment.Left;
            HorizontalAlignment horizontalAlignment;
            switch (textAlignment)
            {
                case TextAlignment.Right:
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;

                case TextAlignment.Center:
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;

                case TextAlignment.Justify:
                    horizontalAlignment = HorizontalAlignment.Stretch;
                    break;
                default:
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
            return horizontalAlignment;
        }

#if WPF
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#endif
        {
            return null;
        }
    }

    /// <summary>
    /// Internal helper class to get FormatProvider for GridEditorColumns
    /// </summary>
    internal static class FormatConverterHelper
    {
#if WPF
        /// <summary>
        /// Returns IFormatprovider for GridPercentColumn
        /// </summary>
        /// <param name="percentColumn">GridPercentColumn</param>
        /// <returns>IFormatProvider</returns>
        internal static IFormatProvider GetPercentColumnFormatProvider(GridPercentColumn percentColumn)
        {
            return new NumberFormatInfo
            {
                PercentDecimalDigits = percentColumn.PercentDecimalDigits,
                PercentDecimalSeparator = percentColumn.PercentDecimalSeparator,
                PercentGroupSeparator = percentColumn.PercentGroupSeparator,
                PercentNegativePattern = percentColumn.PercentNegativePattern,
                PercentPositivePattern = percentColumn.PercentPositivePattern,
                PercentSymbol = percentColumn.PercentSymbol,
                PercentGroupSizes = percentColumn.PercentGroupSizes.ToArray(),
            };
        }

        /// <summary>
        /// Returns IFormatProvider for GridCurrencyColumn
        /// </summary>
        /// <param name="currencyColumn">GridCurrencyColumn</param>
        /// <returns>IFormatProvider</returns>
        internal static IFormatProvider GetCurrencyColumnFormatProvider(GridCurrencyColumn currencyColumn)
        {
            return new NumberFormatInfo
                {
                    CurrencyDecimalDigits = currencyColumn.CurrencyDecimalDigits,
                    CurrencyDecimalSeparator = currencyColumn.CurrencyDecimalSeparator,
                    CurrencyGroupSeparator = currencyColumn.CurrencyGroupSeparator,
                    CurrencyNegativePattern = currencyColumn.CurrencyNegativePattern,
                    CurrencyPositivePattern = currencyColumn.CurrencyPositivePattern,
                    CurrencySymbol = currencyColumn.CurrencySymbol,
                    CurrencyGroupSizes = currencyColumn.CurrencyGroupSizes.ToArray(),
                };
        }
#endif
        /// <summary>
        /// Returns format Provider for GridNumericColumn
        /// </summary>
        /// <param name="numericColumn">GridNumericColumn</param>
        /// <returns>formatted string</returns>
#if WPF
        internal static IFormatProvider GetNumericColumnFormatProvider(GridNumericColumn numericColumn)
#else
        internal static string GetNumericColumnFormatProvider(GridNumericColumn numericColumn)
#endif
        {
#if WPF
            return new NumberFormatInfo
            {

                NumberDecimalDigits = numericColumn.NumberDecimalDigits,
                NumberDecimalSeparator = numericColumn.NumberDecimalSeparator,
                NumberGroupSeparator = numericColumn.NumberGroupSeparator,
                NumberNegativePattern = numericColumn.NumberNegativePattern,
                NumberGroupSizes = numericColumn.NumberGroupSizes.ToArray(),

            };
#else
            return numericColumn.formatString;
#endif
        }


#if WPF
        /// <summary>
        /// Returns IFormatprovider for GridPercentColumn
        /// </summary>
        /// <param name="percentColumn">GridPercentColumn</param>
        /// <returns>IFormatProvider</returns>
        internal static IFormatProvider GetPercentColumnFormatProvider(TreeGridPercentColumn percentColumn)
        {
            return new NumberFormatInfo
            {
                PercentDecimalDigits = percentColumn.PercentDecimalDigits,
                PercentDecimalSeparator = percentColumn.PercentDecimalSeparator,
                PercentGroupSeparator = percentColumn.PercentGroupSeparator,
                PercentNegativePattern = percentColumn.PercentNegativePattern,
                PercentPositivePattern = percentColumn.PercentPositivePattern,
                PercentSymbol = percentColumn.PercentSymbol,
                PercentGroupSizes = percentColumn.PercentGroupSizes.ToArray(),
            };
        }

        /// <summary>
        /// Returns IFormatProvider for GridCurrencyColumn
        /// </summary>
        /// <param name="currencyColumn">GridCurrencyColumn</param>
        /// <returns>IFormatProvider</returns>
        internal static IFormatProvider GetCurrencyColumnFormatProvider(TreeGridCurrencyColumn currencyColumn)
        {
            return new NumberFormatInfo
            {
                CurrencyDecimalDigits = currencyColumn.CurrencyDecimalDigits,
                CurrencyDecimalSeparator = currencyColumn.CurrencyDecimalSeparator,
                CurrencyGroupSeparator = currencyColumn.CurrencyGroupSeparator,
                CurrencyNegativePattern = currencyColumn.CurrencyNegativePattern,
                CurrencyPositivePattern = currencyColumn.CurrencyPositivePattern,
                CurrencySymbol = currencyColumn.CurrencySymbol,
                CurrencyGroupSizes = currencyColumn.CurrencyGroupSizes.ToArray(),
            };
        }
#endif
        /// <summary>
        /// Returns format Provider for GridNumericColumn
        /// </summary>
        /// <param name="numericColumn">GridNumericColumn</param>
        /// <returns>formatted string</returns>
#if WPF
        internal static IFormatProvider GetNumericColumnFormatProvider(TreeGridNumericColumn numericColumn)
#else
        internal static string GetNumericColumnFormatProvider(TreeGridNumericColumn numericColumn)
#endif
        {
#if WPF
            return new NumberFormatInfo
            {

                NumberDecimalDigits = numericColumn.NumberDecimalDigits,
                NumberDecimalSeparator = numericColumn.NumberDecimalSeparator,
                NumberGroupSeparator = numericColumn.NumberGroupSeparator,
                NumberNegativePattern = numericColumn.NumberNegativePattern,
                NumberGroupSizes = numericColumn.NumberGroupSizes.ToArray(),

            };
#else
            return numericColumn.formatString;
#endif
        }

    }

    }
