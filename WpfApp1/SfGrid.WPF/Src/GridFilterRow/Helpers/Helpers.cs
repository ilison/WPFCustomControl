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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
#if UWP
using Syncfusion.UI.Xaml.Controls.Input;
#endif

namespace Syncfusion.UI.Xaml.Grid.Helpers
{
    public static class FilterRowHelpers
    {

        /// <summary>
        /// Gets the type of the filter.
        /// </summary>
        /// <param name="filterType">FilterRowCondition of the column.</param>
        /// <returns></returns>
        internal static FilterType GetFilterType(FilterRowCondition filterType)
        {
            if (filterType == FilterRowCondition.Equals)
                return FilterType.Equals;
            else if (filterType == FilterRowCondition.NotEquals)
                return FilterType.NotEquals;
            else if (filterType == FilterRowCondition.GreaterThan)
                return FilterType.GreaterThan;
            else if (filterType == FilterRowCondition.GreaterThanOrEqual)
                return FilterType.GreaterThanOrEqual;
            else if (filterType == FilterRowCondition.LessThan)
                return FilterType.LessThan;
            else if (filterType == FilterRowCondition.LessThanOrEqual)
                return FilterType.LessThanOrEqual;
            else if (filterType == FilterRowCondition.BeginsWith)
                return FilterType.StartsWith;
            else if (filterType == FilterRowCondition.EndsWith)
                return FilterType.EndsWith;
            else if (filterType == FilterRowCondition.Contains)
                return FilterType.Contains;
            else if (filterType == FilterRowCondition.Before)
                return FilterType.LessThan;
            else if (filterType == FilterRowCondition.BeforeOrEqual)
                return FilterType.LessThanOrEqual;
            else if (filterType == FilterRowCondition.After)
                return FilterType.GreaterThan;
            else if (filterType == FilterRowCondition.AfterOrEqual)
                return FilterType.GreaterThanOrEqual;
            else if (filterType == FilterRowCondition.Empty)
                return FilterType.Equals;
            else if (filterType == FilterRowCondition.NotEmpty)
                return FilterType.NotEquals;
            else if (filterType == FilterRowCondition.Null)
                return FilterType.Equals;
            else if (filterType == FilterRowCondition.NotNull)
                return FilterType.NotEquals;
            return FilterType.Equals;
        }

        /// <summary>
        /// Gets the corresponding string for the given FilterRowCondition.
        /// </summary>
        /// <param name="filterType"></param>
        /// <returns></returns>
        internal static string GetResourceWrapper(FilterRowCondition filterType)
        {
            if (filterType == FilterRowCondition.Null)
                return GridResourceWrapper.Null;
            else if (filterType == FilterRowCondition.NotNull)
                return GridResourceWrapper.NotNull;
            else if (filterType == FilterRowCondition.Empty)
                return GridResourceWrapper.Empty;
            else if (filterType == FilterRowCondition.NotEmpty)
                return GridResourceWrapper.NotEmpty;
            else if (filterType == FilterRowCondition.NotEquals)
                return GridResourceWrapper.NotEquals;
            else if (filterType == FilterRowCondition.Equals)
                return GridResourceWrapper.Equalss;
            else if (filterType == FilterRowCondition.After)
                return GridResourceWrapper.After;
            else if (filterType == FilterRowCondition.AfterOrEqual)
                return GridResourceWrapper.AfterOrEqual;
            else if (filterType == FilterRowCondition.Before)
                return GridResourceWrapper.Before;
            else if (filterType == FilterRowCondition.BeforeOrEqual)
                return GridResourceWrapper.BeforeOrEqual;
            else if (filterType == FilterRowCondition.GreaterThan)
                return GridResourceWrapper.GreaterThan;
            else if (filterType == FilterRowCondition.GreaterThanOrEqual)
                return GridResourceWrapper.GreaterThanorEqual;
            else if (filterType == FilterRowCondition.LessThan)
                return GridResourceWrapper.LessThan;
            else if (filterType == FilterRowCondition.LessThanOrEqual)
                return GridResourceWrapper.LessThanorEqual;
            else if (filterType == FilterRowCondition.BeginsWith)
                return GridResourceWrapper.BeginsWith;
            else if (filterType == FilterRowCondition.EndsWith)
                return GridResourceWrapper.EndsWith;
            else if (filterType == FilterRowCondition.Contains)
                return GridResourceWrapper.Contains;
            return GridResourceWrapper.Equalss;
        }

        /// <summary>
        /// Gets the type of the RowFilter.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns></returns>
        internal static FilterRowCondition GetFilterRowCondition(String filterType)
        {
            if (filterType == GridResourceWrapper.Equalss)
                return FilterRowCondition.Equals;
            else if (filterType == GridResourceWrapper.NotEquals)
                return FilterRowCondition.NotEquals;
            else if (filterType == GridResourceWrapper.GreaterThan)
                return FilterRowCondition.GreaterThan;
            else if (filterType == GridResourceWrapper.GreaterThanorEqual)
                return FilterRowCondition.GreaterThanOrEqual;
            else if (filterType == GridResourceWrapper.LessThan)
                return FilterRowCondition.LessThan;
            else if (filterType == GridResourceWrapper.LessThanorEqual)
                return FilterRowCondition.LessThanOrEqual;
            else if (filterType == GridResourceWrapper.BeginsWith)
                return FilterRowCondition.BeginsWith;
            else if (filterType == GridResourceWrapper.EndsWith)
                return FilterRowCondition.EndsWith;
            else if (filterType == GridResourceWrapper.Contains)
                return FilterRowCondition.Contains;
            else if (filterType == GridResourceWrapper.Before)
                return FilterRowCondition.Before;
            else if (filterType == GridResourceWrapper.BeforeOrEqual)
                return FilterRowCondition.BeforeOrEqual;
            else if (filterType == GridResourceWrapper.After)
                return FilterRowCondition.After;
            else if (filterType == GridResourceWrapper.AfterOrEqual)
                return FilterRowCondition.AfterOrEqual;
            else if (filterType == GridResourceWrapper.Empty)
                return FilterRowCondition.Empty;
            else if (filterType == GridResourceWrapper.NotEmpty)
                return FilterRowCondition.NotEmpty;
            else if (filterType == GridResourceWrapper.Null)
                return FilterRowCondition.Null;
            else if (filterType == GridResourceWrapper.NotNull)
                return FilterRowCondition.NotNull;
            return FilterRowCondition.Equals;
        }

        /// <summary>
        /// Gets the corresponding FilterRowCondition for the given FilterType.
        /// </summary>
        /// <param name="filterType"></param>
        /// <returns></returns>
        internal static FilterRowCondition GetFilterRowCondition(FilterType filterType)
        {
            if (filterType == FilterType.Equals)
                return FilterRowCondition.Equals;
            else if (filterType == FilterType.NotEquals)
                return FilterRowCondition.NotEquals;
            else if (filterType == FilterType.GreaterThan)
                return FilterRowCondition.GreaterThan;
            else if (filterType == FilterType.GreaterThanOrEqual)
                return FilterRowCondition.GreaterThanOrEqual;
            else if (filterType == FilterType.LessThan)
                return FilterRowCondition.LessThan;
            else if (filterType == FilterType.LessThanOrEqual)
                return FilterRowCondition.LessThanOrEqual;
            else if (filterType == FilterType.StartsWith)
                return FilterRowCondition.BeginsWith;
            else if (filterType == FilterType.EndsWith)
                return FilterRowCondition.EndsWith;
            else if (filterType == FilterType.Contains)
                return FilterRowCondition.Contains;
            return FilterRowCondition.Equals;
        }


        /// <summary>
        /// Converts the given filterValue to corresponding string formats.
        /// </summary>
        /// <param name="filterValue">Value that want to convert to corresponding type format.</param>
        /// <returns>Returns the converted text.</returns>
        internal static string GetFormatedFilterText(this GridColumn gridColumn, object filterValue)
        {
            string filterText = string.Empty;
            string filterRowType = gridColumn.GetRowFilterType();
           
            if (filterRowType == "Numeric")
            {
#if WPF
                decimal columnValue;
                decimal.TryParse(filterValue.ToString(), out columnValue);
                int decimalDigits = 0;
                if (gridColumn is GridNumericColumn)
                    decimalDigits = (gridColumn as GridNumericColumn).NumberDecimalDigits;
                else if (gridColumn is GridPercentColumn)
                    decimalDigits = (gridColumn as GridPercentColumn).PercentDecimalDigits;
                else if (gridColumn is GridCurrencyColumn)
                    decimalDigits = (gridColumn as GridCurrencyColumn).CurrencyDecimalDigits;
                var numericNumberFormatInfo = new NumberFormatInfo
                {
                    NumberDecimalDigits = decimalDigits,
                    NumberGroupSizes = new int[0]
                };
                filterText = columnValue.ToString("N", numericNumberFormatInfo);
#else
                Double columnValue;
                if (gridColumn is GridNumericColumn)
                {
                    Decimal columnvalue;
                    var column = gridColumn as GridNumericColumn;
                    if (column.ParsingMode == Parsers.Double)
                    {
                        Double.TryParse(filterValue.ToString(), out columnValue);
                        if (string.Equals(column.FormatString, "P"))
                            columnValue /= 100;
                        return columnValue.ToString(column.FormatString, CultureInfo.CurrentUICulture);
                    }
                    else
                    {
                        Decimal.TryParse(filterValue.ToString(), out columnvalue);
                        if (string.Equals(column.FormatString, "P"))
                            columnvalue /= 100;
                        return columnvalue.ToString(column.FormatString, CultureInfo.CurrentUICulture);
                    }
                }
                filterText = filterValue.ToString();
#endif
            }
#if WPF
            else if (filterRowType == "DateTime" && gridColumn is GridDateTimeColumn)
            {
                DateTime _columnValue;
                var column = gridColumn as GridDateTimeColumn;
                if (filterValue is DateTime)
                    _columnValue = (DateTime)filterValue;
                else
                    DateTime.TryParse(filterValue.ToString(), column.DateTimeFormat,
                                        DateTimeStyles.AdjustToUniversal, out _columnValue);
                filterText = FilterHelpers.DateTimeFormatString(column, _columnValue);
            }
#endif
            else
                filterText = filterValue.ToString();

            return filterText;
        }
    }
}
