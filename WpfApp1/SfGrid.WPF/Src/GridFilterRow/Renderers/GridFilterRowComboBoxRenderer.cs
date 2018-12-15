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
using System.Windows.Input;
using System.Linq.Expressions;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Data;
using Windows.UI.Core;
using Key = Windows.System.VirtualKey;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Syncfusion.UI.Xaml.Controls.Input;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Threading;
using Syncfusion.Windows.Tools.Controls;
using System.Collections.ObjectModel;
#endif

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
#if WPF
    
    /// <summary>
    /// Represents a class which handles the filter operation that loads the ComboBox in a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridFilterRowComboBoxRenderer : GridFilterRowComboBoxRendererBase
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellComboBoxRenderer"/> class.
        /// </summary>
        public GridFilterRowComboBoxRenderer():base()
        { }

        #endregion

        #region Override Methods

        /// <summary>
        /// Initialize the ComboBoxAdv with required fields.
        /// </summary>
        /// <param name="uiElement">The ComboBoxAdv that loaded in Edit mode.</param>
        /// <param name="dataColumn">The DataColumn that loads the ComboBoxAdv.</param>
        protected override void InitializeEditBinding(ComboBoxAdv uiElement, DataColumnBase dataColumn)
        {
            base.InitializeEditBinding(uiElement, dataColumn);
            if (dataColumn.GridColumn.FilteredFrom == FilteredFrom.FilterRow && dataColumn.GridColumn.FilterPredicates.Count > 0)
            {

                var itemsCollection = uiElement.ItemsSource.ToList<FilterRowElement>();
                if (itemsCollection != null)
                {
                    var predicate = dataColumn.GridColumn.FilterPredicates.FirstOrDefault();
                    var selItem = itemsCollection.FirstOrDefault(item =>
                                dataColumn.GridColumn.ColumnFilter == Data.ColumnFilter.DisplayText
                                    ? item.DisplayText.Equals(predicate.FilterValue)
                                    : (item.ActualValue != null ? item.ActualValue.Equals(predicate.FilterValue) 
                                    : item.ActualValue == predicate.FilterValue));
                    uiElement.SelectedItem = selItem;
                }
            }
            uiElement.IsEditable = false;
            uiElement.AllowMultiSelect = false;
            uiElement.AllowSelectAll = false;
            uiElement.EnableOKCancel = false;
        }

        #endregion
    }
#endif
}
