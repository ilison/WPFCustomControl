#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
#if WPF
using System.Windows.Data;
#endif
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
#if WinRT || UNIVERSAL
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
#if WPF
using System.Data;
#endif
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that implements <see cref="Syncfusion.Data.IPropertyAccessProvider"/> to Get / Set value on the underlying object which is used by <see cref="Syncfusion.Data.CollectionViewAdv"/>.
    /// </summary>
    public class GridItemPropertiesProvider : ItemPropertiesProvider
    {
        private SfDataGrid dataGrid;
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridItemPropertiesProvider"/> class.
        /// </summary>
        /// <param name="view">
        /// The corresponding view.
        /// </param>
        /// <param name="_dataGrid">
        /// The SfDataGrid.
        /// </param>
        public GridItemPropertiesProvider(ICollectionViewAdv view, SfDataGrid _dataGrid)
            : base(view)
        {
            dataGrid = _dataGrid;
        }

        private GridColumn cachedcolumn;
        private bool isdisposed = false;
        internal List<GridColumn> columns;
#if UWP
        private bool usePLINQ;
        internal List<string> displayBindingList;
        internal List<IValueConverter> bindingConverter;
        internal List<object> converterParameter;
        internal List<string> converterLanguage;
        internal List<object> targetNullValue;
#endif

        public override void OnBeginReflect()
        {
            if (columns != null)
                columns.Clear();
            columns = this.dataGrid.Columns.ToList();

#if UWP
            usePLINQ = this.dataGrid.UsePLINQ;
            if (usePLINQ)
            {
                displayBindingList = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).Path.Path : string.Empty).ToList();
                bindingConverter = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).Converter : null).ToList();
                converterParameter = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).ConverterParameter : null).ToList();
                converterLanguage = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).ConverterLanguage : string.Empty).ToList();
                targetNullValue = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).TargetNullValue : null).ToList();
            }
#endif
        }

        /// <summary>
        /// Gets the property value that is reflected from the specified record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the property value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get the value.
        /// </param>
        /// <returns>
        /// Returns the property value for the specified record and property name.
        /// </returns>
        public override object GetValue(object record, string propName)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);
                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

                    if (cachedcolumn.ValueBinding != null && cachedcolumn.useBindingValue)
                    {
                        cachedcolumn.ColumnWrapper.DataContext = record;
#if WPF
                        if (cachedcolumn.ColumnWrapper.ValueBindingExpression.Status == BindingStatus.Unattached)
                            cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
#endif
                        return cachedcolumn.ColumnWrapper.Value;
                    }
                }
            }
            return base.GetValue(record, propName);
        }

        /// <summary>
        /// Gets the property value from the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the property value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get the value.
        /// </param>
        /// <param name="useBindingValue">
        /// Specifies whether the property value is reflected either from record or ValueBinding.
        /// </param>
        /// <returns>
        /// Returns the property value for the specified record and property name.
        /// </returns>
        public override object GetValue(object record, string propName, bool useBindingValue)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);
                
                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

                    if (cachedcolumn.ValueBinding != null)
                    {
                        if (useBindingValue)
                        {
                            cachedcolumn.ColumnWrapper.DataContext = record;
#if WPF
                            if (cachedcolumn.ColumnWrapper.ValueBindingExpression.Status == BindingStatus.Unattached)
                                cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
#endif
                            return cachedcolumn.ColumnWrapper.Value;
                        }
                        else
                        {
                            if (cachedcolumn.isValueMultiBinding)
                                throw new Exception("Set UseBindingValue to true to reflect the property value as GridColumn.ValueBinding is MultiBinding");

                            var valueBinding = cachedcolumn.ValueBinding as Binding;
                            var value = string.IsNullOrEmpty(valueBinding.Path.Path) ? record : base.GetValue(record, valueBinding.Path.Path);
                            return this.GetConvertedValue(valueBinding, value);
                        }
                    }
                }
            }
            return base.GetValue(record, propName);
        }

        /// <summary>
        /// Sets the value to particular property in a record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to set the value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to set the value.
        /// </param>
        /// <param name="value">
        /// The corresponding value to set particular property.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the value is set the particular property in the specified record.
        /// </returns>
        public override bool SetValue(object record, string propName, object value)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.MappingName.Equals(propName))
                    cachedcolumn = dataGrid.Columns.FirstOrDefault(col => col.MappingName == propName);

                if (cachedcolumn != null)
                    return SetValue(record, propName, value, cachedcolumn.useBindingValue);
            }
            return base.SetValue(record, propName, value);
        }

        /// <summary>
        /// Sets the value to particular property in a record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to set the value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to set the value.
        /// </param>
        /// <param name="value">
        /// The corresponding value to set particular property.
        /// </param>
        /// <param name="useBindingValue">
        /// Specifies the whether the property value is set to record or ValueBinding.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the value is set the particular property in the specified record.
        /// </returns>
        public override bool SetValue(object record, string propName, object value, bool useBindingValue)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return false;

                    if (cachedcolumn.ValueBinding != null && useBindingValue)
                    {
                        cachedcolumn.ColumnWrapper.DataContext = record;
                        cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
                        cachedcolumn.ColumnWrapper.Value = value;
                        return true;
                    }
                }
            }
            return base.SetValue(record, propName, value);
        }

        /// <summary>
        /// Gets the formatted value of particular property for the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get formatted value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get formatted value.
        /// </param>
        /// <returns>
        /// Returns the formatted value of particular property based on the specified record and property name.
        /// </returns>
        public override object GetFormattedValue(object record, string propName)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                    return this.GetDisplayValue(record, propName, cachedcolumn.UseBindingValue);
            }
            return base.GetFormattedValue(record, propName);
        }

        /// <summary>
        ///  Gets the Display value of particular property for the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get formatted value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get formatted value.
        /// </param>
        /// <param name="canusebindingreflection">
        /// If true then use Binding reflection to get the display value
        /// </param>
        /// <returns>
        /// Returns the Display value of particular property based on the specified record and property name.
        /// </returns>
        public override object GetDisplayValue(object record, string propName, bool canusebindingreflection)
        {
            if (dataGrid == null)
                return base.GetValue(record, propName);

            if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

            if (cachedcolumn.IsUnbound)
                return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

            if (canusebindingreflection)
            {
                if (cachedcolumn != null && cachedcolumn.DisplayBinding != null)
                {
                    cachedcolumn.DisplayColumnWrapper.DataContext = record;
                    cachedcolumn.DisplayColumnWrapper.SetDisplayBinding(cachedcolumn.DisplayBinding);
                    return cachedcolumn.DisplayColumnWrapper.FormattedValue == null ? string.Empty : cachedcolumn.DisplayColumnWrapper.FormattedValue.ToString();
                }
            }
            else if (cachedcolumn.DisplayBinding != null)
            {
                if (cachedcolumn.isDisplayMultiBinding)
                    throw new Exception("Set UseBindingValue to true to reflect the property value as GridColumn.DisplayBinding is MultiBinding");

                var displayBinding = cachedcolumn.DisplayBinding as Binding;
#if UWP
                if (this.usePLINQ)
                {
                    var index = columns.IndexOf(cachedcolumn);
                    var path = displayBindingList[index];
                    var _value = string.IsNullOrEmpty(path) ? record : base.GetValue(record, path);
                    return this.GetConvertedValue(bindingConverter[index], converterParameter[index], converterLanguage[index], targetNullValue[index], _value);
                }
#endif
                var value = string.IsNullOrEmpty(displayBinding.Path.Path) ? record : base.GetValue(record, displayBinding.Path.Path);
                return this.GetConvertedValue(displayBinding, value);
            }
            return base.GetValue(record, propName);
        }

        internal void SetDataGrid(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridItemPropertiesProvider"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                this.dataGrid = null;
                if (columns != null)
                    columns.Clear();
                columns = null;
#if UWP
                if (displayBindingList != null)
                {
                    displayBindingList.Clear();
                    displayBindingList = null;
                }
                if (bindingConverter != null)
                {
                    bindingConverter.Clear();
                    bindingConverter = null;
                }
                if (converterParameter != null)
                {
                    converterParameter.Clear();
                    converterParameter = null;
                }
                if (converterLanguage != null)
                {
                    converterLanguage.Clear();
                    converterLanguage = null;
                }
                if (targetNullValue != null)
                {
                    targetNullValue.Clear();
                    targetNullValue = null;
                }
#endif
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }

    }

    /// <summary>
    /// Represents a class that implements <see cref="Syncfusion.Data.DynamicPropertiesProvider"/> to Get / Set value on the underlying  dynamic data object which is used by <see cref="Syncfusion.Data.CollectionViewAdv"/>.
    /// </summary>
    public class GridDynamicPropertiesProvider : DynamicPropertiesProvider
    {
        private SfDataGrid dataGrid;
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridDynamicPropertiesProvider"/> class.
        /// </summary>
        /// <param name="view">
        /// The corresponding view.
        /// </param>
        /// <param name="_dataGrid">
        /// The SfDataGrid.
        /// </param>
        public GridDynamicPropertiesProvider(ICollectionViewAdv view, SfDataGrid _dataGrid)
            : base(view)
        {
            dataGrid = _dataGrid;
        }

        private GridColumn cachedcolumn;
        private bool isdisposed = false;
        internal List<GridColumn> columns;
#if UWP
        private bool usePLINQ;
        internal List<string> displayBindingList;
        internal List<IValueConverter> bindingConverter;
        internal List<object> converterParameter;
        internal List<string> converterLanguage;
        internal List<object> targetNullValue;
#endif
        public override void OnBeginReflect()
        {
            if (columns != null)
                columns.Clear();
            columns = this.dataGrid.Columns.ToList();

#if UWP
            usePLINQ = this.dataGrid.UsePLINQ;
            if (usePLINQ)
            {
                displayBindingList = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).Path.Path : string.Empty).ToList();
                bindingConverter = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).Converter : null).ToList();
                converterParameter = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).ConverterParameter : null).ToList();
                converterLanguage = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).ConverterLanguage : null).ToList();
                targetNullValue = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).TargetNullValue : null).ToList();
            }
#endif
        }

        /// <summary>
        /// Gets the property value that is reflected from the specified record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the property value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get the value.
        /// </param>
        /// <returns>
        /// Returns the property value for the specified record and property name.
        /// </returns>
        public override object GetValue(object record, string propName)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

                    if (cachedcolumn.ValueBinding != null && cachedcolumn.useBindingValue)
                    {
                        cachedcolumn.ColumnWrapper.DataContext = record;
#if WPF
                        if (cachedcolumn.ColumnWrapper.ValueBindingExpression.Status == BindingStatus.Unattached)
                            cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
#endif
                        return cachedcolumn.ColumnWrapper.Value;
                    }
                }
            }
            return base.GetValue(record, propName);
        }

        /// <summary>
        /// Gets the property value from the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the property value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get the value.
        /// </param>
        /// <param name="useBindingValue">
        /// Specifies whether the property value is reflected either from record or ValueBinding.
        /// </param>
        /// <returns>
        /// Returns the property value for the specified record and property name.
        /// </returns>
        public override object GetValue(object record, string propName, bool useBindingValue)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

                    if (cachedcolumn.ValueBinding != null)
                    {
                        if (useBindingValue)
                        {
                            cachedcolumn.ColumnWrapper.DataContext = record;
#if WPF
                            if (cachedcolumn.ColumnWrapper.ValueBindingExpression.Status == BindingStatus.Unattached)
                                cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
#endif
                            return cachedcolumn.ColumnWrapper.Value;
                        }
                        else
                        {
                            if (cachedcolumn.isValueMultiBinding)
                                throw new Exception("Set UseBindingValue to true to reflect the property value as GridColumn.ValueBinding is MultiBinding");

                            var valueBinding = cachedcolumn.ValueBinding as Binding;
                            var value = string.IsNullOrEmpty(valueBinding.Path.Path) ? record : base.GetValue(record, valueBinding.Path.Path);
                            return this.GetConvertedValue(valueBinding, value);
                        }
                    }
                }
            }
            return base.GetValue(record, propName, useBindingValue);
        }

        /// <summary>
        /// Sets the value to particular property in a record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to set the value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to set the value.
        /// </param>
        /// <param name="value">
        /// The corresponding value to set particular property.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the value is set the particular property in the specified record.
        /// </returns>
        public override bool SetValue(object record, string propName, object value)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                    return SetValue(record, propName, value, cachedcolumn.useBindingValue);
            }
            return base.SetValue(record, propName, value);
        }

        /// <summary>
        /// Sets the value to particular property in a record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to set the value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to set the value.
        /// </param>
        /// <param name="value">
        /// The corresponding value to set particular property.
        /// </param>
        /// <param name="useBindingValue">
        /// Specifies the whether the property value is set to record or ValueBinding.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the value is set the particular property in the specified record.
        /// </returns>
        public override bool SetValue(object record, string propName, object value, bool useBindingValue)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return false;

                    if (cachedcolumn.ValueBinding != null && useBindingValue)
                    {
                        cachedcolumn.ColumnWrapper.DataContext = record;
                        cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
                        cachedcolumn.ColumnWrapper.Value = value;
                        return true;
                    }
                }
            }
            return base.SetValue(record, propName, value);
        }

        /// <summary>
        /// Gets the formatted value of particular property for the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get formatted value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get formatted value.
        /// </param>
        /// <returns>
        /// Returns the formatted value of particular property based on the specified record and property name.
        /// </returns>
        public override object GetFormattedValue(object record, string propName)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                    return this.GetDisplayValue(record, propName, cachedcolumn.UseBindingValue);
            }
            return base.GetFormattedValue(record, propName);
        }

        /// <summary>
        ///  Gets the Display value of particular property for the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get formatted value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get formatted value.
        /// </param>
        /// <param name="canusebindingreflection">
        /// If true then use Binding reflection to get the display value
        /// </param>
        /// <returns>
        /// Returns the Display value of particular property based on the specified record and property name.
        /// </returns>
        public override object GetDisplayValue(object record, string propName, bool canusebindingreflection)
        {
            if (dataGrid == null)
                return base.GetValue(record, propName);

            if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

            if (cachedcolumn.IsUnbound)
                return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

            if (canusebindingreflection)
            {
                if (cachedcolumn != null && cachedcolumn.DisplayBinding != null)
                {
                    cachedcolumn.DisplayColumnWrapper.DataContext = record;
                    cachedcolumn.DisplayColumnWrapper.SetDisplayBinding(cachedcolumn.DisplayBinding);
                    return cachedcolumn.DisplayColumnWrapper.FormattedValue == null ? string.Empty : cachedcolumn.DisplayColumnWrapper.FormattedValue.ToString();
                }
            }
            else if (cachedcolumn.DisplayBinding != null)
            {
                if (cachedcolumn.isDisplayMultiBinding)
                    throw new Exception("Set UseBindingValue to true to reflect the property value as GridColumn.DisplayBinding is MultiBinding");

                var displayBinding = cachedcolumn.DisplayBinding as Binding;
#if UWP
                if (this.usePLINQ)
                {
                    var index = columns.IndexOf(cachedcolumn);
                    var path = displayBindingList[index];
                    var _value = string.IsNullOrEmpty(path) ? record : base.GetValue(record, path);
                    return this.GetConvertedValue(bindingConverter[index], converterParameter[index], converterLanguage[index], targetNullValue[index], _value);
                }
#endif
                var value = string.IsNullOrEmpty(displayBinding.Path.Path) ? record : base.GetValue(record, displayBinding.Path.Path);
                return this.GetConvertedValue(displayBinding, value);
            }
            return base.GetValue(record, propName);
        }

        internal void SetDataGrid(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridXElementAttributesProvider"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                this.dataGrid = null;
                if (this.columns != null)
                    this.columns.Clear();
                this.columns = null;
#if UWP
                if (displayBindingList != null)
                {
                    displayBindingList.Clear();
                    displayBindingList = null;
                }
                if (bindingConverter != null)
                {
                    bindingConverter.Clear();
                    bindingConverter = null;
                }
                if (converterParameter != null)
                {
                    converterParameter.Clear();
                    converterParameter = null;
                }
                if (converterLanguage != null)
                {
                    converterLanguage.Clear();
                    converterLanguage = null;
                }
                if (targetNullValue != null)
                {
                    targetNullValue.Clear();
                    targetNullValue = null;
                }
#endif
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }
    }
    
    /// <summary>
    /// Represents a class that implements <see cref="Syncfusion.Data.XElementAttributesProvider"/> to Get / Set value on the underlying object which is used by <see cref="Syncfusion.Data.CollectionViewAdv"/>.
    /// </summary>
    public class GridXElementAttributesProvider : XElementAttributesProvider
    {
        private SfDataGrid dataGrid;
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridXElementAttributesProvider"/> class.
        /// </summary>
        /// <param name="view">
        /// The corresponding view.
        /// </param>
        /// <param name="_dataGrid">
        /// The SfDataGrid.
        /// </param>
        public GridXElementAttributesProvider(ICollectionViewAdv view, SfDataGrid _dataGrid)
            : base(view)
        {
            dataGrid = _dataGrid;
        }

        private GridColumn cachedcolumn;
        private bool isdisposed = false;
        internal List<GridColumn> columns;
#if UWP
        private bool usePLINQ;
        internal List<string> displayBindingList;
        internal List<IValueConverter> bindingConverter;
        internal List<object> converterParameter;
        internal List<string> converterLanguage;
        internal List<object> targetNullValue;
#endif
        public override void OnBeginReflect()
        {
            if (columns != null)
                columns.Clear();
            columns = this.dataGrid.Columns.ToList();
#if UWP
            usePLINQ = this.dataGrid.UsePLINQ;
            if (usePLINQ)
            {
                displayBindingList = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).Path.Path : string.Empty).ToList();
                bindingConverter = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).Converter : null).ToList();
                converterParameter = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).ConverterParameter : null).ToList();
                converterLanguage = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).ConverterLanguage : null).ToList();
                targetNullValue = this.dataGrid.Columns.Select(x => (x.DisplayBinding is Binding) ? (x.DisplayBinding as Binding).TargetNullValue : null).ToList();
            }
#endif
        }

        /// <summary>
        /// Sets the value to particular property in a record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to set the value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to set the value.
        /// </param>
        /// <param name="value">
        /// The corresponding value to set particular property.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the value is set the particular property in the specified record.
        /// </returns>
        public override bool SetValue(object record, string propName, object value)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.MappingName.Equals(propName))
                    cachedcolumn = dataGrid.Columns.FirstOrDefault(col => col.MappingName == propName);

                if (cachedcolumn != null)
                    return SetValue(record, propName, value, cachedcolumn.useBindingValue);
            }
            return base.SetValue(record, propName, value);
        }

        /// <summary>
        /// Sets the value to particular property in a record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to set the value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to set the value.
        /// </param>
        /// <param name="value">
        /// The corresponding value to set particular property.
        /// </param>
        /// <param name="useBindingValue">
        /// Specifies the whether the property value is set to record or ValueBinding.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the value is set the particular property in the specified record.
        /// </returns>
        public override bool SetValue(object record, string propName, object value, bool useBindingValue)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.MappingName.Equals(propName))
                    cachedcolumn = dataGrid.Columns.FirstOrDefault(col => col.MappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return false;

                    if (cachedcolumn.ValueBinding != null && useBindingValue)
                    {
                        cachedcolumn.ColumnWrapper.DataContext = record;
                        cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
                        cachedcolumn.ColumnWrapper.Value = value;
                        return true;
                    }
                }
            }
            return base.SetValue(record, propName, value);
        }

        /// <summary>
        /// Gets the property value that is reflected from the specified record.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the property value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get the value.
        /// </param>
        /// <returns>
        /// Returns the property value for the specified record and property name.
        /// </returns>
        public override object GetValue(object record, string propName)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

                    if (cachedcolumn.ValueBinding != null && cachedcolumn.useBindingValue)
                    {
                        cachedcolumn.ColumnWrapper.DataContext = record;
#if WPF
                        if (cachedcolumn.ColumnWrapper.ValueBindingExpression.Status == BindingStatus.Unattached)
                            cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
#endif
                        return cachedcolumn.ColumnWrapper.Value;
                    }
                }
            }
            return base.GetValue(record, propName);
        }

        /// <summary>
        /// Gets the property value from the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the property value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get the value.
        /// </param>
        /// <param name="useBindingValue">
        /// Specifies whether the property value is reflected either from record or ValueBinding.
        /// </param>
        /// <returns>
        /// Returns the property value for the specified record and property name.
        /// </returns>
        public override object GetValue(object record, string propName, bool useBindingValue)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);
                if (cachedcolumn != null)
                {
                    if (cachedcolumn.IsUnbound)
                        return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

                    if (cachedcolumn.ValueBinding != null)
                    {
                        if (useBindingValue)
                        {
                            cachedcolumn.ColumnWrapper.DataContext = record;
#if WPF
                            if (cachedcolumn.ColumnWrapper.ValueBindingExpression.Status == BindingStatus.Unattached)
                                cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
#endif
                            return cachedcolumn.ColumnWrapper.Value;
                        }
                        else
                        {
                            if (cachedcolumn.isValueMultiBinding)
                                throw new Exception("Set UseBindingValue to true to reflect the property value as GridColumn.ValueBinding is MultiBinding");

                            var valueBinding = cachedcolumn.ValueBinding as Binding;
                            var value = string.IsNullOrEmpty(valueBinding.Path.Path) ? record : base.GetValue(record, valueBinding.Path.Path);
                            return this.GetConvertedValue(valueBinding, value);
                        }
                    }
                }
            }
            return base.GetValue(record, propName);
        }

        /// <summary>
        /// Gets the formatted value of particular property for the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get formatted value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get formatted value.
        /// </param>
        /// <returns>
        /// Returns the formatted value of particular property based on the specified record and property name.
        /// </returns>
        public override object GetFormattedValue(object record, string propName)
        {
            if (dataGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                    return this.GetDisplayValue(record, propName, cachedcolumn.UseBindingValue);
            }
            return base.GetFormattedValue(record, propName);
        }

        /// <summary>
        ///  Gets the Display value of particular property for the specified record and property name.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get formatted value.
        /// </param>
        /// <param name="propName">
        /// The corresponding property name to get formatted value.
        /// </param>
        /// <param name="canusebindingreflection">
        /// If true then use Binding reflection to get the display value
        /// </param>
        /// <returns>
        /// Returns the Display value of particular property based on the specified record and property name.
        /// </returns>
        public override object GetDisplayValue(object record, string propName, bool canusebindingreflection)
        {
            if (dataGrid == null)
                return base.GetValue(record, propName);

            if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                cachedcolumn = columns != null ? columns.FirstOrDefault(col => col.mappingName == propName) : dataGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

            if (cachedcolumn.IsUnbound)
                return dataGrid.GetUnBoundCellValue(cachedcolumn, record);

            if (canusebindingreflection)
            {
                if (cachedcolumn != null && cachedcolumn.DisplayBinding != null)
                {
                    cachedcolumn.DisplayColumnWrapper.DataContext = record;
                    cachedcolumn.DisplayColumnWrapper.SetDisplayBinding(cachedcolumn.DisplayBinding);
                    return cachedcolumn.DisplayColumnWrapper.FormattedValue == null ? string.Empty : cachedcolumn.DisplayColumnWrapper.FormattedValue.ToString();
                }
            }
            else if (cachedcolumn.DisplayBinding != null)
            {
                if (cachedcolumn.isDisplayMultiBinding)
                    throw new Exception("Set UseBindingValue to true to reflect the property value as GridColumn.DisplayBinding is MultiBinding");

                var displayBinding = cachedcolumn.DisplayBinding as Binding;
#if UWP
                if (this.usePLINQ)
                {
                    var index = columns.IndexOf(cachedcolumn);
                    var path = displayBindingList[index];
                    var _value = string.IsNullOrEmpty(path) ? record : base.GetValue(record, path);
                    return this.GetConvertedValue(bindingConverter[index], converterParameter[index], converterLanguage[index], targetNullValue[index], _value);
                }
#endif
                var value = string.IsNullOrEmpty(displayBinding.Path.Path) ? record : base.GetValue(record, displayBinding.Path.Path);
                return this.GetConvertedValue(displayBinding, value);
            }
            return base.GetValue(record, propName);
        }

        internal void SetDataGrid(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridXElementAttributesProvider"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                this.dataGrid = null;
                if (this.columns != null)
                {
                    columns.Clear();
                    columns = null;
                }
#if UWP
                if (displayBindingList != null)
                {
                    displayBindingList.Clear();
                    displayBindingList = null;
                }
                if (bindingConverter != null)
                {
                    bindingConverter.Clear();
                    bindingConverter = null;
                }
                if (converterParameter != null)
                {
                    converterParameter.Clear();
                    converterParameter = null;
                }
                if (converterLanguage != null)
                {
                    converterLanguage.Clear();
                    converterLanguage = null;
                }
                if (targetNullValue != null)
                {
                    targetNullValue.Clear();
                    targetNullValue = null;
                }
#endif
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }
    }
}
