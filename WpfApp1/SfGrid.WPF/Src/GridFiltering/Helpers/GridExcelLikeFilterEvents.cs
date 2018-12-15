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
using Syncfusion.Data;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.Data.Extensions;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using Syncfusion.Windows.Shared;
using System.Windows.Data;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    #region OKButtonClick

    public delegate void OkButtonClickEventHandler(object sender, OkButtonClikEventArgs e);

    public class EventArgsExt : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgsExt"/> class.
        /// </summary>
        public EventArgsExt()
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.EventArgsExt"/> class.
        /// </summary>
        /// <param name="originalSender">
        /// The original reporting sender that raised the event.
        /// </param>
        protected EventArgsExt(object originalSource)
        {
            OriginalSource = originalSource;
        }

        /// <summary>
        /// Gets the original reporting source that raised the event.
        /// </summary>
        public object OriginalSource { get; private set; }
    }
    /// <summary>
    /// Provides data for OkButtonClikEvent in <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/>.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class OkButtonClikEventArgs : EventArgsExt
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OkButtonClikEventArgs"/> class.
        /// </summary>
        public OkButtonClikEventArgs()
        {
        }

        public OkButtonClikEventArgs(object source) :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the record.
        /// </summary>
        /// <value>The record.</value>
        public IEnumerable<FilterElement> UnCheckedElements
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the edited values.
        /// </summary>
        /// <value>The edited values.</value>
        public IEnumerable<FilterElement> CheckedElements
        {
            get;
            internal set;
        }
        public object FilterValue
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the filter value1.
        /// </summary>
        /// <value>The filter value1.</value>
        public object FilterValue1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the filter value2.
        /// </summary>
        /// <value>The filter value2.</value>
        public object FilterValue2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the filter type1.
        /// </summary>
        /// <value>The filter type1.</value>
        public object FilterType1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the filter type2.
        /// </summary>
        /// <value>The filter type2.</value>
        public object FilterType2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the predicate.
        /// </summary>
        /// <value>The type of the predicate.</value>
        public object PredicateType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ColumnType.
        /// </summary>
        /// <value>The type of the predicate.</value>
        public AdvancedFilterType ColumnType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ColumnType.
        /// </summary>
        /// <value>The type of the predicate.</value>
        public bool IsCaseSensitive1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ColumnType.
        /// </summary>
        /// <value>The type of the predicate.</value>
        public bool IsCaseSensitive2
        {
            get;
            set;
        }

    }

    #endregion

    #region PopupOpened

    public delegate void PopupOpenedEventHandler(object sender, PopupOpenedEventArgs e);

    [ClassReference(IsReviewed = false)]
    /// <summary>
    /// Provides data for PopupOpened event in <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/>.
    /// </summary>
    public class PopupOpenedEventArgs : EventArgsExt
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupOpenedEventArgs"/> class.
        /// </summary>
        public PopupOpenedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupOpenedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        public PopupOpenedEventArgs(RoutedEvent routedEvent, object source) :
            base()
        {
        }


        /// <summary>
        /// Gets or sets the record.
        /// </summary>
        /// <value>The record.</value>
        public IEnumerable<FilterElement> ItemsSource
        {
            get;
            set;
        }

    }
    #endregion

    #region OnFilterElementPropertyChanged

    public delegate void OnFilterElementPropertyChangedEventHandler(object sender, OnFilterElementPropertyChangedEventArgs e);

    [ClassReference(IsReviewed = false)]
    /// <summary>
    /// Provides data for FilterElementPropertyChanged event in <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/>.
    /// </summary>
    public class OnFilterElementPropertyChangedEventArgs : EventArgsExt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnFilterElementPropertyChangedEventArgs"/> class.
        /// </summary>
        public OnFilterElementPropertyChangedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnFilterElementPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        public OnFilterElementPropertyChangedEventArgs(RoutedEvent routedEvent, object source) :
            base()
        {
        }


        /// <summary>
        /// Gets or sets the filter element.
        /// </summary>
        /// <value>The filter element.</value>
        public FilterElement FilterElement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the select all checked.
        /// </summary>
        /// <value>The select all checked.</value>
        public Nullable<bool> SelectAllChecked
        {
            get;
            set;
        }
    }
    #endregion

    #region SelectAllCheckBoxChecked

    public delegate void SelectAllCheckBoxCheckedEventHandler(object sender, SelectAllCheckBoxCheckedEventArgs e);

    [ClassReference(IsReviewed = false)]
    /// <summary>
    /// Provides data for SelectAllCheckBoxChecked event in <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/>.
    /// </summary>
    public class SelectAllCheckBoxCheckedEventArgs : EventArgsExt
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAllCheckBoxCheckedEventArgs"/> class.
        /// </summary>
        public SelectAllCheckBoxCheckedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAllCheckBoxCheckedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        public SelectAllCheckBoxCheckedEventArgs(RoutedEvent routedEvent, object source) :
            base()
        {
        }


        /// <summary>
        /// Gets or sets the filter elements.
        /// </summary>
        /// <value>The filter elements.</value>
        public List<FilterElement> FilterElements
        {
            get;
            set;
        }
    }

    #endregion

    #region SelectAllCheckBoxUnChecked
    public delegate void SelectAllCheckBoxUnCheckedEventHandler(object sender, SelectAllCheckBoxUnCheckedEventArgs e);

    [ClassReference(IsReviewed = false)]
    /// <summary>
    /// Provides data for SelectAllCheckBoxUnChecked event in <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/>.
    /// </summary>
    public class SelectAllCheckBoxUnCheckedEventArgs : EventArgsExt
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAllCheckBoxUnCheckedEventArgs"/> class.
        /// </summary>
        public SelectAllCheckBoxUnCheckedEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAllCheckBoxUnCheckedEventArgs"/> class.
        /// </summary>
        /// <param name="routedEvent">The routed event identifier for this instance of the <see cref="T:System.Windows.RoutedEventArgs"/> class.</param>
        /// <param name="source">An alternate source that will be reported when the event is handled. This pre-populates the <see cref="P:System.Windows.RoutedEventArgs.Source"/> property.</param>
        public SelectAllCheckBoxUnCheckedEventArgs(RoutedEvent routedEvent, object source) :
            base()
        {
        }

        /// <summary>
        /// Gets or sets the filter elements.
        /// </summary>
        /// <value>The filter elements.</value>
        public List<FilterElement> FilterElements
        {
            get;
            set;
        }
    }
    #endregion

    internal static class FilterHelpers
    {
        /// <summary>
        /// Gets the type of the predicate.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns></returns>
        internal static PredicateType GetPredicateType(String filterType)
        {
            PredicateType predicate = (PredicateType)Enum.Parse(typeof(PredicateType), filterType, true);
            return predicate;
        }

        /// <summary>
        /// Gets the type of the filter.
        /// </summary>
        /// <param name="filterType">Type of the filter.</param>
        /// <returns></returns>
        internal static FilterType GetFilterType(String filterType)
        {
            if (filterType == GridResourceWrapper.Equalss)
                return FilterType.Equals;
            else if (filterType == GridResourceWrapper.NotEquals)
                return FilterType.NotEquals;
            else if (filterType == GridResourceWrapper.GreaterThan)
                return FilterType.GreaterThan;
            else if (filterType == GridResourceWrapper.GreaterThanorEqual)
                return FilterType.GreaterThanOrEqual;
            else if (filterType == GridResourceWrapper.LessThan)
                return FilterType.LessThan;
            else if (filterType == GridResourceWrapper.LessThanorEqual)
                return FilterType.LessThanOrEqual;
            else if (filterType == GridResourceWrapper.BeginsWith)
                return FilterType.StartsWith;
            else if (filterType == GridResourceWrapper.EndsWith)
                return FilterType.EndsWith;
            else if (filterType == GridResourceWrapper.Contains)
                return FilterType.Contains;
            else if (filterType == GridResourceWrapper.NotContains)
                return FilterType.Contains;
            else if (filterType == GridResourceWrapper.Before)
                return FilterType.LessThan;
            else if (filterType == GridResourceWrapper.BeforeOrEqual)
                return FilterType.LessThanOrEqual;
            else if (filterType == GridResourceWrapper.After)
                return FilterType.GreaterThan;
            else if (filterType == GridResourceWrapper.AfterOrEqual)
                return FilterType.GreaterThanOrEqual;
            else if (filterType == GridResourceWrapper.Empty)
                return FilterType.Equals;
            else if (filterType == GridResourceWrapper.NotEmpty)
                return FilterType.NotEquals;
            else if (filterType == GridResourceWrapper.Null)
                return FilterType.Equals;
            else if (filterType == GridResourceWrapper.NotNull)
                return FilterType.NotEquals;
            return FilterType.Equals;
        }

#if WPF
        internal static string DateTimeFormatString(GridDateTimeColumn column, DateTime columnValue)
        {
            switch (column.pattern)
            {
                case DateTimePattern.ShortDate:
                    return columnValue.ToString("d", column.dateTimeFormat);
                case DateTimePattern.LongDate:
                    return columnValue.ToString("D", column.dateTimeFormat);
                case DateTimePattern.LongTime:
                    return columnValue.ToString("T", column.dateTimeFormat);
                case DateTimePattern.ShortTime:
                    return columnValue.ToString("t", column.dateTimeFormat);
                case DateTimePattern.FullDateTime:
                    return columnValue.ToString("F", column.dateTimeFormat);
                case DateTimePattern.RFC1123:
                    return columnValue.ToString("R", column.dateTimeFormat);
                case DateTimePattern.SortableDateTime:
                    return columnValue.ToString("s", column.dateTimeFormat);
                case DateTimePattern.UniversalSortableDateTime:
                    return columnValue.ToString("u", column.dateTimeFormat);
                case DateTimePattern.YearMonth:
                    return columnValue.ToString("Y", column.dateTimeFormat);
                case DateTimePattern.MonthDay:
                    return columnValue.ToString("M", column.dateTimeFormat);
                case DateTimePattern.CustomPattern:
                    return columnValue.ToString(column.customPattern, column.dateTimeFormat);
                default:
                    return columnValue.ToString("MMMM", column.dateTimeFormat);
            }
        }

        internal static object DateTimeFormatString(TreeGridDateTimeColumn column, DateTime columnValue)
        {
            switch (column.Pattern)
            {
                case DateTimePattern.ShortDate:
                    return columnValue.ToString("d", column.DateTimeFormat);
                case DateTimePattern.LongDate:
                    return columnValue.ToString("D", column.DateTimeFormat);
                case DateTimePattern.LongTime:
                    return columnValue.ToString("T", column.DateTimeFormat);
                case DateTimePattern.ShortTime:
                    return columnValue.ToString("t", column.DateTimeFormat);
                case DateTimePattern.FullDateTime:
                    return columnValue.ToString("F", column.DateTimeFormat);
                case DateTimePattern.RFC1123:
                    return columnValue.ToString("R", column.DateTimeFormat);
                case DateTimePattern.SortableDateTime:
                    return columnValue.ToString("s", column.DateTimeFormat);
                case DateTimePattern.UniversalSortableDateTime:
                    return columnValue.ToString("u", column.DateTimeFormat);
                case DateTimePattern.YearMonth:
                    return columnValue.ToString("Y", column.DateTimeFormat);
                case DateTimePattern.MonthDay:
                    return columnValue.ToString("M", column.DateTimeFormat);
                case DateTimePattern.CustomPattern:
                    return columnValue.ToString(column.CustomPattern, column.DateTimeFormat);
                default:
                    return columnValue.ToString("MMMM", column.DateTimeFormat);
            }
        }
#endif

        /// <summary>
        /// Gets the AdvancedFilterType of the particular column
        /// </summary>
        /// <param name="DataGrid"></param>
        /// <param name="Column"></param>
        /// <returns></returns>
        internal static AdvancedFilterType GetAdvancedFilterType(GridColumn column)
        {
            if (column.IsUnbound)
                return AdvancedFilterType.TextFilter;

            if (column.FilterBehavior == FilterBehavior.StringTyped)
                return AdvancedFilterType.TextFilter;

            if (column.DataGrid == null || column.DataGrid.View == null)
                return AdvancedFilterType.TextFilter;

            Type columnType;

            if (column.DataGrid.View.IsDynamicBound)
            {
                columnType = column.ColumnMemberType;
                if (columnType == null)
                    return AdvancedFilterType.TextFilter;
            }
            else
            {
                var pdc = column.DataGrid.View.GetItemProperties();
                if (pdc == null)
                    return AdvancedFilterType.TextFilter;
                var pd = pdc.GetPropertyDescriptor(column.MappingName);
                if (pd == null)
                    return AdvancedFilterType.TextFilter;
                columnType = pd.PropertyType;
            }
            if (columnType == typeof(int) || columnType == typeof(Double) || columnType == typeof(Decimal) || columnType == typeof(int?) || columnType == typeof(double?) || columnType == typeof(decimal?) || columnType == typeof(long) || columnType == typeof(long?) || columnType == typeof(uint) || columnType == typeof(uint?) || columnType == typeof(byte) || columnType == typeof(byte?) || columnType == typeof(float)
               || columnType == typeof(float?) || columnType == typeof(sbyte) || columnType == typeof(sbyte?) || columnType == typeof(ulong) || columnType == typeof(ulong?) || columnType == typeof(short) || columnType == typeof(short?) || columnType == typeof(ushort) || columnType == typeof(ushort?))
                return AdvancedFilterType.NumberFilter;
            else if (columnType == typeof(DateTime) || columnType == typeof(DateTime?) || columnType == typeof(TimeSpan) || columnType == typeof(TimeSpan?))
                return AdvancedFilterType.DateFilter;
            else
                return AdvancedFilterType.TextFilter;
        }

        internal static string GetResourceWrapper(FilterType filterType, object FilterValue)
        {
            if (filterType == FilterType.Equals && FilterValue == null)
                return GridResourceWrapper.Null;
            else if (filterType == FilterType.NotEquals && FilterValue == null)
                return GridResourceWrapper.NotNull;
            else if (filterType == FilterType.Equals && FilterValue.Equals(string.Empty))
                return GridResourceWrapper.Empty;
            else if (filterType == FilterType.NotEquals && FilterValue.Equals(string.Empty))
                return GridResourceWrapper.NotEmpty;
            else if (filterType == FilterType.NotEquals)
                return GridResourceWrapper.NotEquals;
            else if (filterType == FilterType.Equals)
                return GridResourceWrapper.Equalss;
            if (FilterValue != null && !string.IsNullOrEmpty(FilterValue.ToString()))
            {
                if (TypeConverterHelper.CanConvert(typeof(DateTime), FilterValue.ToString()))
                {
                    if (filterType == FilterType.GreaterThan)
                        return GridResourceWrapper.After;
                    else if (filterType == FilterType.GreaterThanOrEqual)
                        return GridResourceWrapper.AfterOrEqual;
                    else if (filterType == FilterType.LessThan)
                        return GridResourceWrapper.Before;
                    else if (filterType == FilterType.LessThanOrEqual)
                        return GridResourceWrapper.BeforeOrEqual;
                }
            }
            if (filterType == FilterType.GreaterThan)
                return GridResourceWrapper.GreaterThan;
            else if (filterType == FilterType.GreaterThanOrEqual)
                return GridResourceWrapper.GreaterThanorEqual;
            else if (filterType == FilterType.LessThan)
                return GridResourceWrapper.LessThan;
            else if (filterType == FilterType.LessThanOrEqual)
                return GridResourceWrapper.LessThanorEqual;
            else if (filterType == FilterType.StartsWith)
                return GridResourceWrapper.BeginsWith;
            else if (filterType == FilterType.EndsWith)
                return GridResourceWrapper.EndsWith;
            else if (filterType == FilterType.Contains)
                return GridResourceWrapper.Contains;
            return GridResourceWrapper.Equalss;
        }

        /// <summary>
        /// Invokes to get formatted string for corresponding value.
        /// </summary>
        /// <param name="column">GridColumn</param>
        /// <param name="item">item is either Value or Record</param>
        /// <param name="isRecord">true if item is record otherwise false</param>
        /// <returns>formatted text</returns>
        internal static string GetFormattedString(GridColumn column, ItemPropertiesProvider provider, object item, bool isRecord)
        {
            var displayBinding = column.DisplayBinding as Binding;
            if (isRecord)
            {
                var displayText = column.isDisplayMultiBinding ? provider.GetFormattedValue(item, column.MappingName) : column.DataGrid.View.GetPropertyAccessProvider().GetDisplayValue(item, column.mappingName, column.useBindingValue);
                if (displayText != null)
                    return displayText.ToString();
				//WPF-34213 - isdisplayingbindingcreated condition added.
                else if (displayText == null && column.isdisplayingbindingcreated) //displayBinding.Converter != null && (displayBinding.Converter is GridValueConverter || displayBinding.Converter is DisplayMemberConverter))
                    return GridResourceWrapper.Blanks;
            }

            var value = provider.GetConvertedValue(displayBinding, item);
            //While item is null we need to return the Blanks text
            if (item != null)
                return value.ToString();
            return GridResourceWrapper.Blanks;
        }
    }
}
