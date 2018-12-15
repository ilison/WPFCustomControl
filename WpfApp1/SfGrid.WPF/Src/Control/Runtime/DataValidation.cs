#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Syncfusion.Data.Extensions;
using System.Text;
using System.Reflection;
using Syncfusion.Data;
using System.Windows;
#if WinRT ||UNIVERSAL
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides methods that support data validation.
    /// </summary>
    public static class DataValidation 
    {
#if !WinRT && !UNIVERSAL

        internal static bool ValidateRow(object dataModel)
        {
            var dataValidation = dataModel as IDataErrorInfo;
            if (dataValidation != null)
                if (string.IsNullOrEmpty(dataValidation.Error))
                    return false;
                else
                    return true;
            return false;

        }
#endif
        /// <summary>
        /// Validates the value of current cell.
        /// </summary>
        /// <param name="currentCell">
        /// The current cell that is to validated.
        /// </param>
        /// <param name="propertyName">
        /// The propertyName.
        /// </param>
        /// <param name="dataModel">
        /// The dataModel.
        /// </param>
        /// <returns>
        /// <b>true</b> if the validation is successful; otherwise, <b>false</b>.
        /// </returns>
        public static bool Validate(GridCell currentCell,string propertyName, object dataModel)
        {
            bool hasError = false;
            var itemproperties = currentCell.ColumnBase.GridColumn.DataGrid.View.GetItemProperties();
           
            // WPF-25016 Using PropertyDescriptorExtensions for WPF and PropertyInfoExtensions for WinRT, the codes are cleaned up
            if (propertyName.Contains('.'))
            {
                var propNames = propertyName.Split('.');
                propertyName = propNames[propNames.Length - 1];
                Array.Resize(ref propNames, propNames.Length - 1);
                var pName = string.Join(".", propNames);
#if WPF
                dataModel = PropertyDescriptorExtensions.GetValue(itemproperties, dataModel, pName);
#else
                dataModel = Syncfusion.Data.PropertyInfoExtensions.GetValue(itemproperties, dataModel, pName);
#endif
            }

            if (dataModel == null)
                return hasError;

#if WPF
            var dataValidation = dataModel as IDataErrorInfo;
            if (dataValidation != null)
            {
                string errormessage = dataValidation[propertyName];
                hasError = !String.IsNullOrEmpty(errormessage);
                if (hasError)
                    currentCell.bindingErrorMessage = errormessage;

                currentCell.ApplyValidationVisualState();
                return !hasError;
            }
#endif
#if !SyncfusionFramework4_0 && !SyncfusionFramework3_5 || UWP
            hasError = !ValidateINotifyDataErrorInfo(currentCell, propertyName, dataModel);
#endif
            return !hasError;
        }
        
        /// <summary>
        /// Gets the value of property.
        /// </summary>
        /// <param name="obj">
        /// The object to get property value.
        /// </param>
        /// <param name="propertyName">
        /// The name of property to get its value.
        /// </param>
        /// <returns>
        /// Returns the corresponding property value.
        /// </returns>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            foreach (var prop in propertyName.Split('.').Select(s => obj.GetType().GetProperty(s)))
            {
#if WinRT || UNIVERSAL
                if (prop != null && !prop.PropertyType.IsPrimitive())
#else
                    if (prop != null && !prop.PropertyType.IsPrimitive)
#endif

                    obj = prop.GetValue(obj, null);
            }
            return obj;
        }

#if !SyncfusionFramework4_0 && !SyncfusionFramework3_5 || UWP
        internal static bool ValidateRowINotifyDataErrorInfo(object dataModel)
        {
            var dataErrorValidation = dataModel as INotifyDataErrorInfo;
            return dataErrorValidation.HasErrors;
        }

        internal static bool ValidateINotifyDataErrorInfo(GridCell currentCell,string propertyName, object dataModel)
        {
            bool hasError = false;
            var dataErrorValidation = dataModel as INotifyDataErrorInfo;
            currentCell.bindingErrorMessage = string.Empty;
            if (dataErrorValidation != null)
            {
                var errorList = dataErrorValidation.GetErrors(propertyName);
                if (errorList != null)
                {
                    var errormessage = errorList.Cast<string>().FirstOrDefault();
                    hasError = !String.IsNullOrEmpty(errormessage);
                    if (hasError)
                        currentCell.bindingErrorMessage = errormessage;
                }
                currentCell.ApplyValidationVisualState();
            }
            return !hasError;
        }
#endif
        private static void ChangeCellErrorState(GridCell element, string propertyName, object datamodel, TextAlignment alignment, bool OnEditing)
        {
            //var dataValidation = datamodel as IDataErrorInfo;
            //if (dataValidation != null)
            //{
            //    var validator = element;
            //    if (validator != null)
            //    {
            //        string errormessage = dataValidation[propertyName];
            //        bool hasError = !String.IsNullOrEmpty(errormessage);
            //        var _args = new ValidationEventArgs
            //        {
            //            TextAlignment = alignment,
            //            ErrorMessage = errormessage,
            //            HasError = hasError
            //        };
            //        validator.Validate(_args);
            //    }
            //}
        }

        //public static void ValidateData(GridCell element, string propertyName, object datamodel,bool isInEditing)
        //{
            //if (!Validate(propertyName, datamodel))
            //{
            //    ChangeCellErrorState(element, propertyName, datamodel, alignment, isInEditing);
            //}
            //else if ((this.DataGrid.GridValidationMode != GridValidationMode.None && this.DataGrid.GridValidationMode != GridValidationMode.InEdit) || (isInEditing && this.DataGrid.GridValidationMode != GridValidationMode.None))
            //{
            //    ChangeCellErrorState(element, propertyName, datamodel, alignment, isInEditing);
            //}
      //  }

    }


}
