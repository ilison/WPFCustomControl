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
using System.Threading.Tasks;
using Syncfusion.UI.Xaml.Grid;

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{

    /// <summary>
    /// Represents a class that populates the items in ComboBoxAdv in GridFilterRowComboBoxRenderer.
    /// </summary>
    public class FilterRowElement : INotifyPropertyChanged, IFilterElement
    {
        private object _actualValue;
        private string _displayText;
        private Func<object, string> _formattedString;

        /// <summary>
        /// Initialize a new instance of <see cref="Syncfusion.UI.Xaml.Grid.RowFilter.FilterRowElement"/> class.
        /// </summary>
        public FilterRowElement()
        {
            this._displayText = string.Empty;
        }

        /// <summary>
        /// Gets or sets the actual value.
        /// </summary>
        /// <value>The actual value.</value>
        public object ActualValue
        {
            get { return _actualValue; }
            set { _actualValue = value; }
        }

        /// <summary>
        /// Gets or sets the formatted value to display and filter.
        /// </summary>
        /// <value>The name.</value>
        public string DisplayText
        {
            get
            {
                if (this.FormattedString != null)
                    return this.FormattedString(this);
                return _displayText;
            }
            set { _displayText = value; }
        }

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
            }
        }

        /// <summary>
        /// Gets or sets record to get display text when using ColumnFilter is DisplayText. Otherwise null.
        /// </summary>
        internal object Record { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
