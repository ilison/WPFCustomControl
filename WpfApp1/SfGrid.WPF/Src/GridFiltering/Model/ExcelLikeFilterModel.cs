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
using System.Text;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides the common properties of FilterElement that have been used in FilterRow and AdvancedFiltering in SfDataGrid.     
    /// </summary>    
    public interface IFilterElement
    {
        /// <summary>
        /// Gets or sets the formatted string.
        ///  Encapsulates a GridFilterControl.GetFormattedString method to convert the ActualValue with formatting in GridFilterElement
        /// </summary>
        /// <value>The formatted string.</value>
        Func<object, string> FormattedString { get; set; }

        /// <summary>
        /// Gets or sets the formatted value to display and filter.
        /// </summary>
        /// <value>The name.</value>
        string DisplayText { get; set; }

        /// <summary>
        /// Gets or sets the actual value.
        /// </summary>
        /// <value>The actual value.</value>
        object ActualValue { get; set; }

    }

    [ClassReference(IsReviewed = false)]
    /// <summary>
    /// Class which is used to provide information about filter value
    /// </summary>
    public class FilterElement : INotifyPropertyChanged, IFilterElement
    {
        private string _DisplayText;

        private Func<object, string> _formattedString;
        /// <summary>
        /// Gets or sets the formatted string.
        ///  Encapsulates a GridFilterControl.GetFormattedString method to convert the ActualValue with formatting in GridFilterElement
        /// </summary>
        /// <value>The formatted string.</value>    
        public Func<object, string> FormattedString
        {
            get
            {
                return _formattedString;
            }
            set
            {
                _formattedString = value;
                if (_formattedString != null)
                    _hasDisplayText = false;
            }
        }

        /// <summary>
        /// Gets or sets record to get display text when using MultiBinding. Otherwise null.
        /// </summary>
        internal object Record { get; set; }

        bool _hasDisplayText = false;

        /// <summary>
        /// Gets or sets the formatted value to display and filter.
        /// </summary>
        /// <value>The name.</value>
        public string DisplayText
        {
            get
            {
                if (!_hasDisplayText && this.FormattedString != null)
                {
                    _hasDisplayText = true;
                    _DisplayText = this.FormattedString(this);
                }
                return _DisplayText;
            }
            set
            {
                _DisplayText = value;
                _hasDisplayText = true;
            }
        }

        private object _actualValue;

        /// <summary>
        /// Gets or sets the actual value.
        /// </summary>
        /// <value>The actual value.</value>
        public object ActualValue
        {
            get
            {
                return _actualValue;
            }
            set
            {
                _actualValue = value;
                _hasDisplayText = false;
            }
        }

        private bool _isSelected;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    /// <summary>
    /// To order the FilterElement collection as ascending
    /// </summary>
    [ClassReference(IsReviewed = false)]
    internal class FilterElementAscendingOrder : IComparer<IFilterElement>
    {
        public int Compare(IFilterElement x, IFilterElement y)
        {
#if UWP
            if (x.ActualValue == null || y.ActualValue == null || x.ActualValue is Nullable || y.ActualValue is Nullable)
#else
            if (x.ActualValue == null || y.ActualValue == null || x.ActualValue is DBNull || y.ActualValue is DBNull)
#endif
            {
#if UWP
                if ((x.ActualValue == null && y.ActualValue == null) ||
                    (x.ActualValue is Nullable && y.ActualValue is Nullable))
#else
                if ((x.ActualValue == null && y.ActualValue == null) ||
                    (x.ActualValue is DBNull && y.ActualValue is DBNull))
#endif
                    return 0;

#if UWP
                if (x.ActualValue == null || x.ActualValue is Nullable)
#else
                if (x.ActualValue == null || x.ActualValue is DBNull)
#endif
                    return -1;
                return 1;
            }

            if (x.ActualValue is string)
            {
                return String.Compare(x.ActualValue.ToString(), y.ActualValue.ToString());
            }
            if (x.ActualValue is double)
            {
                if ((double)x.ActualValue < (double)y.ActualValue)
                    return -1;
                return (double)x.ActualValue > (double)y.ActualValue ? 1 : 0;
            }
            if (x.ActualValue is decimal)
            {
                if ((decimal)x.ActualValue < (decimal)y.ActualValue)
                    return -1;
                return (decimal)x.ActualValue > (decimal)y.ActualValue ? 1 : 0;
            }
            if (x.ActualValue is DateTime)
            {
                if ((DateTime)x.ActualValue < (DateTime)y.ActualValue)
                    return -1;
                return (DateTime)x.ActualValue > (DateTime)y.ActualValue ? 1 : 0;
            }
            // WPF-37123 Sorting not working inside the FilterPopupControl of GridTimeSpanColumn
            if (x.ActualValue is TimeSpan)
            {
                if ((TimeSpan)x.ActualValue < (TimeSpan)y.ActualValue)
                    return -1;
                return (TimeSpan)x.ActualValue > (TimeSpan)y.ActualValue ? 1 : 0;
            }
            if (x.ActualValue.GetHashCode() < y.ActualValue.GetHashCode())
                return -1;
            return x.ActualValue.GetHashCode() > y.ActualValue.GetHashCode() ? 1 : 0;
        }
    }

    internal class RecordValuePair
    {
        public RecordValuePair(object value, object record)
        {
            Value = value;
            Record = record;
        }

        public object Record { get; set; }
        public object Value { get; set; }
    }

    internal class RecordValueEqualityComparer<T> : IEqualityComparer<T> where T : RecordValuePair
    {
        public bool Equals(T x, T y)
        {
            if (x.Value == y.Value)
                return true;
            if (x.Value == null || y.Value == null)
                return false;
            return x.Value.Equals(y.Value);
        }

        public int GetHashCode(T obj)
        {
            if (obj.Value == null)
                return -1;
            return obj.Value.GetHashCode();
        }
    }
	
	    internal class FilterControlEqualityComparer<T> : IEqualityComparer<T> where T : IFilterElement
    {
        public bool Equals(T x, T y)
        {
            return x.DisplayText.Equals(y.DisplayText);
        }

        public int GetHashCode(T obj)
        {
            return obj.DisplayText.GetHashCode();
        }
    }
}
