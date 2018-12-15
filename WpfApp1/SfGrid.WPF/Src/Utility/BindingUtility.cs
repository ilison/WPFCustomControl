#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
#if UWP
using Syncfusion.UI.Xaml.TreeGrid;
#endif
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Syncfusion.Data;
#else
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.TreeGrid;
#endif

namespace Syncfusion.UI.Xaml.Grid.Utility
{
#if UWP
    using PropertyDescriptorCollection = Syncfusion.Data.PropertyInfoCollection;
#endif    
    internal static class BindingUtility
    {
        public static BindingBase CreateDisplayBinding(this BindingBase source, GridColumnBase column = null)
        {
            return CreateDisplayBinding(source, true, column);
        }

        internal static BindingBase CreateDisplayBinding(this BindingBase source, bool enableErrorNotification, GridColumnBase column = null)
        {
#if WPF
            if (source is MultiBinding)
                return CreateDisplayMultiBinding(source as MultiBinding, enableErrorNotification, column);
#endif
            return CreateDisplayBindingBase(source as Binding, enableErrorNotification, column);

        }

        private static Binding CreateDisplayBindingBase(this Binding source, bool enableErrorNotification, GridColumnBase column = null)
        {
            var binding = new Binding
            {
                ConverterParameter = source.ConverterParameter,
                Mode = !column.isdisplayingbindingcreated ? source.Mode : BindingMode.OneWay,
                Path = source.Path,
                UpdateSourceTrigger = source.UpdateSourceTrigger,
#if WPF
                BindsDirectlyToSource = source.BindsDirectlyToSource,
                ConverterCulture = source.ConverterCulture,
                FallbackValue = source.FallbackValue,
                NotifyOnValidationError = enableErrorNotification,
                StringFormat = source.StringFormat,
                TargetNullValue = source.TargetNullValue,
                ValidatesOnDataErrors = enableErrorNotification,
                ValidatesOnExceptions = source.ValidatesOnExceptions,
#if !SyncfusionFramework4_0
                ValidatesOnNotifyDataErrors = enableErrorNotification,
                Delay = source.Delay,
#endif
                AsyncState = source.AsyncState,
                BindingGroupName = source.BindingGroupName,
                IsAsync = source.IsAsync,
                NotifyOnSourceUpdated = source.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = source.NotifyOnTargetUpdated,
                UpdateSourceExceptionFilter = source.UpdateSourceExceptionFilter,
                XPath = source.XPath
#endif
            };

            if (source.Converter != null)
            {
                //Should skip setting Converter when ValueBinding created by column. 
                if (!(column.ValueBinding == source && column.isvaluebindingcreated))
                    binding.Converter = source.Converter;
            }
            if (source.ElementName != null)
            {
                binding.ElementName = source.ElementName;
                return binding;
            }
            if (source.RelativeSource != null)
            {
                binding.RelativeSource = source.RelativeSource;
                return binding;
            }
            if (source.Source != null)
            {
                binding.Source = source.Source;
            }
#if WPF
            foreach (var validationRule in source.ValidationRules)
            {
                binding.ValidationRules.Add(validationRule);
            }
#endif
            return binding;
        }

        private static BindingMode CreateBindingForAddNewRow(GridColumn column)
        {
            PropertyDescriptorCollection itemproperties = (column.DataGrid != null && column.DataGrid.View != null)
               ? column.DataGrid.View.GetItemProperties()
               : null;

            if (itemproperties == null)
                return BindingMode.TwoWay;

            var propDesc = itemproperties.GetPropertyDescriptor(column.MappingName);
            if (propDesc != null)
            {
#if WPF
                if(propDesc.IsReadOnly)
#else
                if(propDesc.SetMethod == null || !propDesc.SetMethod.IsPublic)
#endif
                    return BindingMode.OneWay;
            }
            else if (propDesc == null)
            {
                if (column.AllowEditing && column.CanEditCell())
                    return BindingMode.TwoWay;
                return BindingMode.OneWay;
            }                      
            return BindingMode.TwoWay;
        }
        public static BindingBase CreateEditBinding(this BindingBase source,GridColumnBase column)
        {
            return CreateEditBinding(source, true, column);
        }
        internal static BindingBase CreateEditBinding(this BindingBase source, bool enableErrorNotification,GridColumnBase column)
        {
#if WPF
            if (source is MultiBinding)
                return CreateEditMultBinding(source as MultiBinding, enableErrorNotification, column);
#endif
            return CreateEditBindingBase(source as Binding, enableErrorNotification, column);

        }
        private static Binding CreateEditBindingBase(this Binding source, bool enableErrorNotification,GridColumnBase column)
        {
            //WPF - 18160 - If AllowEditing of a column is True, mode can be determined by allowediting and 
            //CanEditCell() which return true for all columns other than GridImageColumn and GridHyperLinkColumn
            bool canBindTwoWay = column.CanEditCell();
            UpdateSourceTrigger updateSourceTrigger = column.isvaluebindingcreated ? column.UpdateTrigger : source.UpdateSourceTrigger;
            var binding = new Binding
            {
                Converter = (source.Converter is CultureFormatConverter || source.Converter is DisplayMemberConverter) ? null : source.Converter,
                ConverterParameter = (source.Converter is CultureFormatConverter || source.Converter is DisplayMemberConverter) ? null : source.ConverterParameter,
                Path = source.Path,
                UpdateSourceTrigger =
                    updateSourceTrigger == UpdateSourceTrigger.PropertyChanged
                        ? UpdateSourceTrigger.Explicit
                        : updateSourceTrigger,
#if WPF
                BindsDirectlyToSource = source.BindsDirectlyToSource,
                ConverterCulture = source.ConverterCulture,
                FallbackValue = source.FallbackValue,
                NotifyOnValidationError = enableErrorNotification,
                StringFormat = source.StringFormat,
                TargetNullValue = source.TargetNullValue,
                ValidatesOnDataErrors = enableErrorNotification,
                ValidatesOnExceptions = source.ValidatesOnExceptions,
#if !SyncfusionFramework4_0
				ValidatesOnNotifyDataErrors = enableErrorNotification,
                Delay = source.Delay,
#endif
                AsyncState = source.AsyncState,
                BindingGroupName = source.BindingGroupName,
                IsAsync = source.IsAsync,
                NotifyOnSourceUpdated = source.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = source.NotifyOnTargetUpdated,
                UpdateSourceExceptionFilter = source.UpdateSourceExceptionFilter,
                XPath = source.XPath
#endif
            };

            if (column.GridBase != null)
            {                
                if (column.GridBase is SfDataGrid)
                {                    
                    binding.Mode = (column.GridBase as SfDataGrid).SelectionController.CurrentCellManager.IsAddNewRow ? 
                                   CreateBindingForAddNewRow(column as GridColumn) : 
                                   (!column.isvaluebindingcreated ? source.Mode : 
                                   SetBindingMode(column, source.Path));
                }
                else
                    binding.Mode = !column.isvaluebindingcreated ? source.Mode : SetBindingMode(column, source.Path);                
            }

            if (source.ElementName != null)
            {
                binding.ElementName = source.ElementName;
                return binding;
            }
            if (source.RelativeSource != null)
            {
                binding.RelativeSource = source.RelativeSource;
                return binding;
            }
            if (source.Source != null)
            {
                binding.Source = source.Source;
            }
#if WPF
            foreach (var validationRule in source.ValidationRules)
            {
                binding.ValidationRules.Add(validationRule);
            }
#endif
            return binding;
        }

        private static BindingMode SetBindingMode(GridColumnBase column, PropertyPath path)
        {
            var isModeSet = false;
            var bindingMode = BindingMode.TwoWay;                                    
            PropertyDescriptorCollection itemproperties = null;
            if (column.GridBase != null)
            {
                if (column.GridBase is SfTreeGrid)
                {
                    var SfTreeGrid = (column.GridBase as SfTreeGrid);
                    if (SfTreeGrid.View != null)
                        itemproperties = SfTreeGrid.View.GetItemProperties();
                }
                else
                {
                    var sfDataGrid = (column.GridBase as SfDataGrid);
                    if (sfDataGrid.View != null)
                        itemproperties = sfDataGrid.View.GetItemProperties();
                }
            }

            if (itemproperties != null)
            {
                var propDesc = itemproperties.GetPropertyDescriptor(column.MappingName);
                if (propDesc != null)
                {
#if WPF
                    bindingMode = propDesc.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
#else
                    bindingMode = (propDesc.SetMethod == null || !propDesc.SetMethod.IsPublic) ? BindingMode.OneWay : BindingMode.TwoWay;
#endif
                    isModeSet = true;
                }
            }

            //While implementing the IDataErroInfo, will generate Error read only property. Based on this property created a column when AutoGenerateColumn is True.
            //so that mode will be reset based on AllowEditing property of that Error column.

            if (!isModeSet)
            {
                if (column.AllowEditing && column.CanEditCell())
                    bindingMode = BindingMode.TwoWay;
                else
                    bindingMode = BindingMode.OneWay;
                isModeSet = true;
            }
            return bindingMode;
        }

        public static Binding CreateDisplayBinding(this string mappingName)
        {
            var binding = new Binding
            {
                Path = new PropertyPath(mappingName),
                Mode = BindingMode.OneWay,
#if WPF
                NotifyOnValidationError = true,
                ValidatesOnDataErrors = true,
#if !SyncfusionFramework4_0
                    ValidatesOnNotifyDataErrors = true,
#endif
#endif
            };
            return binding;
        }

        public static Binding CreateEditableBinding(this string mappingName)
        {
            return CreateEditableBinding(mappingName, null, true);
        }

        internal static Binding CreateEditableBinding(this string mappingName, GridColumnBase column, bool enableErrorNotification)
        {
            bool canBindTwoWay = column != null ? column.CanEditCell() : true;
            var binding = new Binding
            {
                Path = new PropertyPath(mappingName),
                Mode = canBindTwoWay ? BindingMode.TwoWay : BindingMode.OneWay,
#if WPF
                NotifyOnValidationError = enableErrorNotification,
                ValidatesOnDataErrors = enableErrorNotification,
#if !SyncfusionFramework4_0
				ValidatesOnNotifyDataErrors = enableErrorNotification
#endif
#endif
            };
            return binding;
        }

        public static BindingBase CreateBinding(this BindingBase binding, object source)
        {
#if WPF
            if (binding is MultiBinding)
                return CreateMultiBinding(binding as MultiBinding, source);
#endif
            return CreateBindingBase(binding as Binding, source);
        }

        internal static Binding CreateBindingBase(this Binding binding, object source)
        {
            var bind = new Binding
            {
                Converter = binding.Converter,
                ConverterParameter = binding.ConverterParameter,
                Mode = binding.Mode,
                Path = binding.Path,
                Source = source,
                UpdateSourceTrigger = binding.UpdateSourceTrigger,
#if WPF
                BindsDirectlyToSource = binding.BindsDirectlyToSource,
                ConverterCulture = binding.ConverterCulture,
                FallbackValue = binding.FallbackValue,
                NotifyOnValidationError = binding.NotifyOnValidationError,
                StringFormat = binding.StringFormat,
                TargetNullValue = binding.TargetNullValue,
                ValidatesOnDataErrors = binding.ValidatesOnDataErrors,
                ValidatesOnExceptions = binding.ValidatesOnExceptions,
#if !SyncfusionFramework4_0
                ValidatesOnNotifyDataErrors = binding.ValidatesOnNotifyDataErrors,
                Delay = binding.Delay,
#endif
                AsyncState = binding.AsyncState,
                BindingGroupName = binding.BindingGroupName,
                IsAsync = binding.IsAsync,
                NotifyOnSourceUpdated = binding.NotifyOnSourceUpdated,
                NotifyOnTargetUpdated = binding.NotifyOnTargetUpdated,
                UpdateSourceExceptionFilter = binding.UpdateSourceExceptionFilter,
                XPath = binding.XPath
#endif
            };
#if WPF
            foreach (var validationRule in binding.ValidationRules)
            {
                bind.ValidationRules.Add(validationRule);
            }
#endif
            return bind;
        }

#if WPF
        private static MultiBinding CreateDisplayMultiBinding(this MultiBinding bindingBaseSource, bool enableErrorNotification, GridColumnBase column = null)
        {
            var multiBinding = new MultiBinding();
            foreach (var binding in bindingBaseSource.Bindings)
            {
                var displayMutiBinding = CreateDisplayBindingBase(binding as Binding, enableErrorNotification, column);
                multiBinding.Bindings.Add(displayMutiBinding);
            }

            multiBinding.Converter = bindingBaseSource.Converter;
            multiBinding.ConverterCulture = bindingBaseSource.ConverterCulture;
            multiBinding.ConverterParameter = bindingBaseSource.ConverterParameter;
            multiBinding.FallbackValue = bindingBaseSource.FallbackValue;
            multiBinding.Mode = bindingBaseSource.Mode;
            multiBinding.NotifyOnSourceUpdated = bindingBaseSource.NotifyOnSourceUpdated;
            multiBinding.NotifyOnTargetUpdated = bindingBaseSource.NotifyOnTargetUpdated;
            multiBinding.NotifyOnValidationError = bindingBaseSource.NotifyOnValidationError;
            multiBinding.StringFormat = bindingBaseSource.StringFormat;
            multiBinding.TargetNullValue = bindingBaseSource.TargetNullValue;
            multiBinding.UpdateSourceExceptionFilter = bindingBaseSource.UpdateSourceExceptionFilter;
            multiBinding.UpdateSourceTrigger = bindingBaseSource.UpdateSourceTrigger;
            multiBinding.ValidatesOnDataErrors = bindingBaseSource.ValidatesOnDataErrors;
            multiBinding.ValidatesOnExceptions = bindingBaseSource.ValidatesOnExceptions;
#if !SyncfusionFramework4_0
            multiBinding.ValidatesOnNotifyDataErrors = bindingBaseSource.ValidatesOnNotifyDataErrors;
#endif
            return multiBinding;

        }

        private static BindingBase CreateEditMultBinding(BindingBase bindingSource, bool enableErrorNotification, GridColumnBase column)
        {
            var multiBinding = new MultiBinding();
            var bindSource = bindingSource as MultiBinding;
            foreach (var binding in bindSource.Bindings)
            {
                var editMultiBinding = CreateEditBindingBase(binding as Binding,
                    enableErrorNotification, column);
                multiBinding.Bindings.Add(editMultiBinding);
            }
            multiBinding.Converter = bindSource.Converter;
            multiBinding.ConverterCulture = bindSource.ConverterCulture;
            multiBinding.ConverterParameter = bindSource.ConverterParameter;
            multiBinding.FallbackValue = bindSource.FallbackValue;
            multiBinding.Mode = bindSource.Mode;
            multiBinding.NotifyOnSourceUpdated = bindSource.NotifyOnSourceUpdated;
            multiBinding.NotifyOnTargetUpdated = bindSource.NotifyOnTargetUpdated;
            multiBinding.NotifyOnValidationError = bindSource.NotifyOnValidationError;
            multiBinding.StringFormat = bindSource.StringFormat;
            multiBinding.TargetNullValue = bindSource.TargetNullValue;
            multiBinding.UpdateSourceExceptionFilter = bindSource.UpdateSourceExceptionFilter;
            multiBinding.UpdateSourceTrigger = bindSource.UpdateSourceTrigger;
            multiBinding.ValidatesOnDataErrors = bindSource.ValidatesOnDataErrors;
            multiBinding.ValidatesOnExceptions = bindSource.ValidatesOnExceptions;
#if !SyncfusionFramework4_0
            multiBinding.ValidatesOnNotifyDataErrors = bindSource.ValidatesOnNotifyDataErrors;
#endif
            return multiBinding;
        }

        private static MultiBinding CreateMultiBinding(this MultiBinding bindingSource, object source)
        {

            var newMultiBinding = new MultiBinding();
            foreach (var multiBind in bindingSource.Bindings)
            {
                var multiBinding = CreateBindingBase(multiBind as Binding, source);
                newMultiBinding.Bindings.Add(multiBinding);
            }
            newMultiBinding.Converter = bindingSource.Converter;
            newMultiBinding.ConverterCulture = bindingSource.ConverterCulture;
            newMultiBinding.ConverterParameter = bindingSource.ConverterParameter;
            newMultiBinding.FallbackValue = bindingSource.FallbackValue;
            newMultiBinding.Mode = bindingSource.Mode;
            newMultiBinding.NotifyOnSourceUpdated = bindingSource.NotifyOnSourceUpdated;
            newMultiBinding.NotifyOnTargetUpdated = bindingSource.NotifyOnTargetUpdated;
            newMultiBinding.NotifyOnValidationError = bindingSource.NotifyOnValidationError;
            newMultiBinding.StringFormat = bindingSource.StringFormat;
            newMultiBinding.TargetNullValue = bindingSource.TargetNullValue;
            newMultiBinding.UpdateSourceExceptionFilter = bindingSource.UpdateSourceExceptionFilter;
            newMultiBinding.UpdateSourceTrigger = bindingSource.UpdateSourceTrigger;
            newMultiBinding.ValidatesOnDataErrors = bindingSource.ValidatesOnDataErrors;
            newMultiBinding.ValidatesOnExceptions = bindingSource.ValidatesOnExceptions;
#if !SyncfusionFramework4_0
            newMultiBinding.ValidatesOnNotifyDataErrors = bindingSource.ValidatesOnNotifyDataErrors;
#endif
            return newMultiBinding;
        }
#endif

    }
    /// <summary>
    /// Provides classes for creating the DisplayBinding and EditBinding for GridColumn in SfDataGrid.
    /// </summary>
    class NamespaceDoc
    { }
}
