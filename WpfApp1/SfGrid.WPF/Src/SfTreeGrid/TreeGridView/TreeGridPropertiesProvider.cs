#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Dynamic;
using Syncfusion.Data.Helper;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;

#if UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
#else
using System.Windows.Data;
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Implements <see cref="IPropertyAccessProvider"/> to Get / Set value on the underlying object which is used by <see cref="CollectionViewAdv"/>.
    /// </summary>
    public class TreeGridPropertiesProvider : IPropertyAccessProvider, IDisposable
    {
        protected TreeGridView View { get; set; }
        protected SfTreeGrid TreeGrid { get; set; }

        private Dictionary<String, PropertyAccessor> itemaccessor = new Dictionary<String, PropertyAccessor>();

        internal TreeGridColumn cachedcolumn;
        public TreeGridPropertiesProvider(SfTreeGrid treeGrid, TreeGridView view)
        {
            this.View = view;
            this.TreeGrid = treeGrid;
            var itemproperties = view.GetItemProperties();

            if (itemproperties == null)
                return;
            foreach (var item in itemproperties)
            {
#if WPF
                var name = (item as PropertyDescriptor).DisplayName;
                var propertyinfo = (item as PropertyDescriptor).ComponentType.GetProperty(name);
#else
                var name = item.Key;
                var propertyinfo = item.Value;
#endif
                if (propertyinfo != null)
                {
                    PropertyAccessor accessor = new PropertyAccessor(propertyinfo);
                    itemaccessor.Add(name, accessor);
                }
            }
        }

        #region IPropertyValue Members
        /// <summary>
        /// Invokes to get Formatted value from Binding
        /// </summary>
        /// <param name="provider">Property access provider</param>
        /// <param name="binding">Binding</param>
        /// <param name="value">Value</param>
        /// <returns>formatted value</returns>
        internal object GetConvertedValue(Binding binding, object value)
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
        /// <summary>
        /// Invokes to get ConvertBack value from Binding
        /// </summary>
        /// <param name="binding">Binding</param>
        /// <param name="value">Value</param>
        /// <returns>FallBack value</returns>
        internal static object GetConvertBackValue(Binding binding, object value)
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
        /// Gets the value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="property">Name of the prop.</param>
        /// <returns></returns>
        /// 
        public virtual object GetValue(object record, string property)
        {   
            if (!(View is TreeGridSelfRelationalView))
                return TreeGridHelper.GetValue(record, property);
            var itemproperties = View.GetItemProperties();

            if (itemproperties == null || record == null)
                return null;

            if (itemaccessor.ContainsKey(property))
                return itemaccessor[property].GetValue(record);

            var tempitempproperties = itemproperties;
            var propertyNameList = property.Split('.');

#if WPF
            return PropertyDescriptorExtensions.GetComplexPropertyValue(propertyNameList, tempitempproperties, record);
#else
            return PropertyInfoExtensions.GetComplexPropertyValue(propertyNameList, tempitempproperties, record);
#endif
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="propName">Name of the prop.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual bool SetValue(object record, string propName, object value)
        {            
            if (!(View is TreeGridSelfRelationalView))
                return TreeGridHelper.SetValue(record, propName, value);

            var itemproperties = View.GetItemProperties();
            if (itemproperties == null)
                return false;

            if (itemaccessor.ContainsKey(propName))
            {
                itemaccessor[propName].SetValue(record, value);
                return true;
            }

            var tempitempproperties = itemproperties;
            var propertyNameList = propName.Split('.');
#if WPF
            return PropertyDescriptorExtensions.SetComplexPropertyValue(propertyNameList, tempitempproperties, record, value);
#else
            return PropertyInfoExtensions.SetComplexPropertyValue(propertyNameList, tempitempproperties, record, value);
#endif
        }

        internal void CommitValue(object record, string propName, object value)
        {
#if UWP
            PropertyInfoCollection descriptor = new PropertyInfoCollection(record.GetType());
#else
            PropertyDescriptorCollection descriptor = TypeDescriptor.GetProperties(record.GetType());
#endif
            var propertyinfo = descriptor.GetPropertyDescriptor(propName);
            object convertedValue = null;
            if (propertyinfo == null)
            {
                if (View.IsDynamicBound)
                {
                    var type = (value == null) ? null : value.GetType();
                    convertedValue = ValueConvert.ChangeType(value, type, null);
                    if (convertedValue != null || (NullableHelperInternal.IsNullableType(type) && convertedValue == null))
                        SetValue(record, propName, convertedValue);
                }
                return;
            }

            convertedValue = ValueConvert.ChangeType(value, propertyinfo.PropertyType, null);
            if (value != null || (NullableHelperInternal.IsNullableType(propertyinfo.PropertyType) && value == null))
                SetValue(record, propName, convertedValue);
        }


        /// <summary>
        /// Gets the Formatted value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="propName">Name of the prop.</param>
        /// <returns></returns>
        /// 
        public virtual object GetFormattedValue(object record, string propName)
        {
            if (this.View != null && this.TreeGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = TreeGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null && cachedcolumn.DisplayBinding != null)
                {
                    cachedcolumn.DisplayColumnWrapper.DataContext = record;
                    cachedcolumn.DisplayColumnWrapper.SetDisplayBinding(cachedcolumn.DisplayBinding);
                    return cachedcolumn.DisplayColumnWrapper.FormattedValue == null ? string.Empty : cachedcolumn.DisplayColumnWrapper.FormattedValue.ToString();
                }
            }
            return GetValue(record, propName);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="propName">Name of the prop.</param>
        /// <param name="useBindingValue">If true,then use binding value</param>
        /// <returns></returns>
        public virtual object GetValue(object record, string propName, bool useBindingValue)
        {
            if (useBindingValue && this.View != null && this.TreeGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = TreeGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null && cachedcolumn.ValueBinding != null)
                {
                    cachedcolumn.ColumnWrapper.DataContext = record;
                    cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
                    return cachedcolumn.ColumnWrapper.Value;
                }
            }
            return GetValue(record, propName);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="propName">Name of the prop.</param>
        /// <param name="value">value</param>
        /// <param name="useBindingValue">If true,then use binding value</param>
        /// <returns></returns>
        public virtual bool SetValue(object record, string propName, object value, bool useBindingValue)
        {
            if (useBindingValue && TreeGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = TreeGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null && cachedcolumn.ValueBinding != null)
                {
                    cachedcolumn.ColumnWrapper.DataContext = record;
                    cachedcolumn.ColumnWrapper.SetValueBinding(cachedcolumn.ValueBinding);
                    cachedcolumn.ColumnWrapper.Value = value;
                    return true;
                }
            }
            return SetValue(record, propName, value);
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPropertiesProvider"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPropertiesProvider"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            this.cachedcolumn = null;
            if (this.itemaccessor != null)
                this.itemaccessor.Clear();
            this.itemaccessor = null;
            this.TreeGrid = null;
            this.View = null;
        }

        #endregion
    }

    /// <summary>
    /// Represents a class to Get / Set value of the underlying  dynamic data object which is used by <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView"/>.
    /// </summary>
    public class TreeGridDynamicPropertiesProvider : TreeGridPropertiesProvider, IDisposable
    {

        internal DynamicHelper dynamicHelper;
        private bool isdisposed = false;

        public TreeGridDynamicPropertiesProvider(SfTreeGrid treeGrid, TreeGridView view) : base(treeGrid, view)
        {
            this.View = view;
            this.dynamicHelper = new DynamicHelper();
        }

        /// <summary>
        /// Get value from dynamic record
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="propName">property name</param>
        /// <returns>value</returns>
        internal static object GetDynamicValue(object record, string propName)
        {
            return new DynamicHelper().GetValue(record, propName);
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
            return this.dynamicHelper.GetValue(record, propName);
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
            if (TreeGrid != null)
            {
                if (cachedcolumn == null || !cachedcolumn.mappingName.Equals(propName))
                    cachedcolumn = TreeGrid.Columns.FirstOrDefault(col => col.mappingName == propName);

                if (cachedcolumn != null)
                {
                    if (cachedcolumn.ValueBinding != null && useBindingValue)
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
            try
            {
                this.dynamicHelper.SetValue(record, propName, value);
            }
            catch
            {
                return false;
            }
            return true;
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
            if (!useBindingValue)
                this.SetValue(record, propName, value);
            return base.SetValue(record, propName, value, useBindingValue);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.Data.DynamicPropertiesProvider"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;

            if (isDisposing)
            {
                this.dynamicHelper.Dispose();
                this.TreeGrid = null;
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }
    }
}
