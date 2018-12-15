#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Provides methods that support data validation.
    /// </summary>
    internal static class TreeGridDataValidation
    {
#if WPF 
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
        internal static bool Validate(TreeGridCell currentCell, string propertyName, object dataModel)
        {
            bool hasError = false;
            var itemproperties = currentCell.ColumnBase.TreeGridColumn.TreeGrid.View.GetItemProperties();

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
                dataModel = PropertyInfoExtensions.GetValue(itemproperties, dataModel, pName);
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
#if !SyncfusionFramework4_0 || UWP
            hasError = !ValidateINotifyDataErrorInfo(currentCell, propertyName, dataModel);
#endif
            return !hasError;
        }

      

#if !SyncfusionFramework4_0 || UWP
        internal static bool ValidateRowINotifyDataErrorInfo(object dataModel)
        {
            var dataErrorValidation = dataModel as INotifyDataErrorInfo;
            return dataErrorValidation.HasErrors;
        }

        internal static bool ValidateINotifyDataErrorInfo(TreeGridCell currentCell, string propertyName, object dataModel)
        {
            bool hasError = false;
            var dataErrorValidation = dataModel as INotifyDataErrorInfo;
            currentCell.bindingErrorMessage = string.Empty;
            if (dataErrorValidation != null)
            {
                var errorList = dataErrorValidation.GetErrors(propertyName);
                if (errorList != null)
                {
                    var errorMessage = errorList.Cast<string>().FirstOrDefault();
                    hasError = !String.IsNullOrEmpty(errorMessage);
                    if (hasError)
                        currentCell.bindingErrorMessage = errorMessage;
                }
                currentCell.ApplyValidationVisualState();
            }
            return !hasError;
        }
#endif       
    }

}
