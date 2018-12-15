#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Syncfusion.Data.Extensions;
#if WPF
using System.Windows.Data;
#else
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public static partial class CollectionViewExtensions
    {
        /// <summary>
        /// Return ValueFunc for based on corresponding  column and DataOperationMode
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <param name="column">GridColumn</param>
        /// <param name="operation">DataOperation</param>
        /// <param name="mode">DataReflectionMode</param>
        /// <param name="useBindingValue">useBindingValue</param>
        /// <returns>Func<string, object, object></returns>
        public static Func<string, object, object> GetValueFunc(this CollectionViewAdv view, GridColumn column, DataOperation operation, DataReflectionMode mode, bool useBindingValue)
        {
            if (operation == DataOperation.Grouping)
            {
                var pgd = view.GroupDescriptions.OfType<ColumnGroupDescription>().FirstOrDefault(g => g.PropertyName == column.mappingName);
                if (pgd != null && pgd.Converter != null)
                {
                    Func<string, object, object> groupConverterFunc = (columnName, record) =>
                    {
                        //The ColumnName and the PropertyReflector helps to get the value to Group the Column with the Intervals.
                        if (pgd.Converter is IColumnAccessProvider)
                        {
                            (pgd.Converter as IColumnAccessProvider).ColumnName = columnName;
                            (pgd.Converter as IColumnAccessProvider).PropertyReflector = view.GetPropertyAccessProvider();
                        }
                        return pgd.Converter.Convert(record, record.GetType(), column, CultureInfo.CurrentCulture.GetCulture());
                    };
                    return groupConverterFunc;
                }
            }

            if (column != null && column.IsUnbound)
            {
                var unboundcolumn = column as GridUnBoundColumn;
                var dataGrid = view.GetDataGrid() as SfDataGrid;
                return view.GetUnboundFunc(dataGrid, unboundcolumn);
            }

            else if (mode == DataReflectionMode.Default)
            {
                if (column != null && column.ValueBinding != null && column.UseBindingValue)
                    return view.GetBindingFunc();
#if WPF
                if (view.IsITypedListSource())
                {
                    var func = view.GetITypedListFunc(column.MappingName);
                    if (func != null)
                        return func;
                }
#endif
                if (view.IsDynamicBound)
                    return view.GetDynamicFunc();

                if (view.IsXElementBound)
                    return view.GetXElementFunc();
            }
            else if (mode == DataReflectionMode.Display)
            {
                return (propname, record) => view.GetPropertyAccessProvider().GetDisplayValue(record, propname, useBindingValue);
            }
            else if (mode == DataReflectionMode.Value)
            {
                return (propName, record) => view.GetPropertyAccessProvider().GetValue(record, propName, useBindingValue);
            }
            return null;
        }

        /// <summary>
        /// Returns ValueExpressionFunc for corresponding column and data operation
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <param name="column">Column</param>
        /// <param name="operation">DataOperation</param>
        /// <param name="mode">DataReflectionMode</param>
        /// <param name="useBindingValue">useBindingvalue</param>
        /// <returns>Expression<Func<string, object, object>></returns>
        public static System.Linq.Expressions.Expression<Func<string, object, object>> GetValueExpressionFunc(this CollectionViewAdv view, GridColumn column, DataOperation operation, DataReflectionMode mode, bool useBindingValue)
        {
            if (column != null && column.IsUnbound)
            {
                var unboundcolumn = column as GridUnBoundColumn;
                var dataGrid = view.GetDataGrid() as SfDataGrid;
                view.GetUnboundFunc(dataGrid, unboundcolumn);
                if (unboundcolumn.UnBoundFunc != null)
                    return (columnName, record) => unboundcolumn.UnBoundFunc(columnName, record);
                return null;
            }
            else if (mode == DataReflectionMode.Default)
            {
                if (column != null && column.ValueBinding != null && column.UseBindingValue)
                    return view.GetBindingExpressionFunc();
#if WPF
                if (view.IsITypedListSource())
                {
                    var func = view.GetITypedListExpressionFunc(column.MappingName);
                    if (func != null)
                        return func;
                }
#endif
                if (view.IsDynamicBound)
                    return view.GetDynamicExpressionFunc();
                else if (view.IsXElementBound)
                    return view.GetXElementAttributeFunc();
            }
            else if (mode == DataReflectionMode.Display)
            {
                return (propname, record) => view.GetPropertyAccessProvider().GetDisplayValue(record, propname, useBindingValue);
            }
            else if (mode == DataReflectionMode.Value)
            {
                return (propName, record) => view.GetPropertyAccessProvider().GetValue(record, propName, useBindingValue);
            }
            return null;
        }

        /// <summary>
        /// Returns Dynamic Func to reflect the value in dynamic collection
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <returns>Dynamic collection Func</returns>
        internal static Func<string, object, object> GetDynamicFunc(this CollectionViewAdv view)
        {
            return (propertyName, record) =>
            {
                var dynamicProvider = view.GetPropertyAccessProvider() as DynamicPropertiesProvider;
                return dynamicProvider != null ? dynamicProvider.GetValue(record, propertyName) : null;
            };
        }

        /// <summary>
        /// Returns Dynamic Expression Func to reflect the value in Dynamic collection
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <returns>Dynamic collection Expression Func</returns>
        internal static System.Linq.Expressions.Expression<Func<string, object, object>> GetDynamicExpressionFunc(this CollectionViewAdv view)
        {
            var func = view.GetDynamicFunc();
            return (propertyName, record) => func(propertyName, record);
        }

        /// <summary>
        /// Gets UnBound Func Value for sorting and grouping
        /// </summary>
        /// <param name="view">View</param>
        /// <param name="dataGrid">DataGrid</param>
        /// <param name="column">Column</param>
        /// <returns>returns Func<string, object, object></returns>
        internal static Func<string, object, object> GetUnboundFunc(this CollectionViewAdv view, SfDataGrid dataGrid, GridUnBoundColumn column)
        {
            if (column.UnBoundFunc != null)
                return column.UnBoundFunc;

            column.UnBoundFunc = (columnName, record) =>
            {
                if (dataGrid != null)
                    return dataGrid.GetUnBoundCellValue(column, record);
                else
                    return null;
            };
            return column.UnBoundFunc;
        }

        /// <summary>
        /// Returns Binding Func to reflect the value form collection
        /// </summary>
        /// <param name="view">CollectionView</param>
        /// <returns>Binding Func</returns>
        internal static Func<string, object, object> GetBindingFunc(this CollectionViewAdv view)
        {
            return (propertyName, record) =>
            {
                var provider = view.GetPropertyAccessProvider();
                return provider.GetValue(record, propertyName);
            };
        }

        /// <summary>
        /// Returns Binding Expression func to reflect the value form collection
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <returns>Binding Expression Func</returns>
        internal static System.Linq.Expressions.Expression<Func<string, object, object>> GetBindingExpressionFunc(this CollectionViewAdv view)
        {
            var func = view.GetBindingFunc();
            return (propertyName, record) => func(propertyName, record);
        }

        /// <summary>
        /// Return XElement ExpressionFunc to reflect the value form XElement collection
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <returns>XElement ExpressionFunc</returns>
        internal static System.Linq.Expressions.Expression<Func<string, object, object>> GetXElementAttributeFunc(this CollectionViewAdv view)
        {
            var func = view.GetXElementFunc();
            return (propertyName, record) => func(propertyName, record);
        }

        /// <summary>
        /// Returns XElement Func to reflect the value form XElement collection
        /// </summary>
        /// <param name="view">CollectionViewAdv</param>
        /// <returns>XElement Func </returns>
        internal static Func<string, object, object> GetXElementFunc(this CollectionViewAdv view)
        {
            return (propertyName, record) =>
            {
                var xElementAttributesProvider = view.GetPropertyAccessProvider() as XElementAttributesProvider;
                return xElementAttributesProvider != null ? xElementAttributesProvider.GetValue(record, propertyName) : null;
            };
        }

        /// <summary>
        /// Invokes to get ConvertBack value from Binding
        /// </summary>
        /// <param name="provider">Property access provider</param>
        /// <param name="binding">Binding</param>
        /// <param name="value">Value</param>
        /// <returns>formatted value</returns>
        internal static object GetConvertBackValue(this ItemPropertiesProvider provider, Binding binding, object value)
        {
#if UWP
            if (binding.Converter != null)
                return binding.Converter.ConvertBack(value, null, binding.ConverterParameter, binding.ConverterLanguage);
#else
            if (binding.Converter != null)
                return binding.Converter.ConvertBack(value, null, binding.ConverterParameter, binding.ConverterCulture);
#endif
            return value;
        }
        /// <summary>
        /// Invokes to get Formatted value from Binding
        /// </summary>
        /// <param name="provider">Property access provider</param>
        /// <param name="binding">Binding</param>
        /// <param name="value">Value</param>
        /// <returns>formatted value</returns>
        internal static object GetConvertedValue(this ItemPropertiesProvider provider, Binding binding, object value)
        {
#if UWP
            if (binding.Converter != null)
                return binding.Converter.Convert(value, null, binding.ConverterParameter, binding.ConverterLanguage);

            if (value == null && binding.TargetNullValue != DependencyProperty.UnsetValue)
                return binding.TargetNullValue;
#else
            if (binding.Converter != null)
                return binding.Converter.Convert(value, null, binding.ConverterParameter, binding.ConverterCulture);

            if (value == null && binding.TargetNullValue != DependencyProperty.UnsetValue)
                return binding.TargetNullValue;

            if (!String.IsNullOrEmpty(binding.StringFormat))
                return (value is DateTime) ? Convert.ToDateTime(value).ToString(binding.StringFormat) : string.Format(binding.StringFormat, value);
#endif
            return value;
        }

#if UWP
        /// <summary>
        /// Invokes to get Formatted value from Binding properties
        /// </summary>
        /// <param name="provider">ItemPropertiesProvider</param>
        /// <param name="converter">IValueConverter</param>
        /// <param name="parameter">Converter Parameter</param>
        /// <param name="language">Converter Language</param>
        /// <param name="targetNullValue">TargetNullValue</param>
        /// <param name="value">value</param>
        /// <returns>formatted value</returns>
        internal static object GetConvertedValue(this ItemPropertiesProvider provider, IValueConverter converter, object parameter, string language, object targetNullValue, object value)
        {
            if(converter != null)
                return converter.Convert(value, null, parameter, language);

            if (value == null && targetNullValue != DependencyProperty.UnsetValue)
                return targetNullValue;
            return value;
        }
#endif


    }
}
