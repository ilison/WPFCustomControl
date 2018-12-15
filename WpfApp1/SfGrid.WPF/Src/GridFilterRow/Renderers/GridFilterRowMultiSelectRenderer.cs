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
    /// <summary>
    /// Represents a class which handles the filter operation that loads the MultiSelectComboBox in a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
   public class GridFilterRowMultiSelectRenderer:GridFilterRowComboBoxRendererBase
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellComboBoxRenderer"/> class.
        /// </summary>
        public GridFilterRowMultiSelectRenderer()
            : base()
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
            ObservableCollection<object> selItems = new ObservableCollection<object>();
            if (dataColumn.GridColumn.FilteredFrom == FilteredFrom.FilterRow && dataColumn.GridColumn.FilterPredicates.Count > 0)
            {
                var itemsCollection = uiElement.ItemsSource.ToList<FilterRowElement>();
                if (itemsCollection != null)
                {
                    itemsCollection.ForEach(element =>
                    {
                        bool needToAdd = false;
                        foreach (var item in dataColumn.GridColumn.FilterPredicates)
                        {
                            var predicate = item as FilterPredicate;
                            bool isEqual = dataColumn.GridColumn.ColumnFilter == Data.ColumnFilter.DisplayText
                                               ? element.DisplayText.Equals(predicate.FilterValue) : (element.ActualValue != null 
                                               ? element.ActualValue.Equals(predicate.FilterValue) : element.ActualValue == predicate.FilterValue);
                            needToAdd = (isEqual && predicate.FilterType == Data.FilterType.Equals) || (predicate.FilterType == Data.FilterType.NotEquals && !isEqual);
                            if (isEqual) break;
                        }
                        if (needToAdd)
                            selItems.Add(element);
                    });
                }
            }

            if (selItems.Count > 0)
                uiElement.SelectedItems = selItems;
            uiElement.IsEditable = false;
            uiElement.AllowMultiSelect = true;
            uiElement.AllowSelectAll = true;
            uiElement.EnableOKCancel = !dataColumn.GridColumn.ImmediateUpdateColumnFilter;
        }

        #endregion
    }
}
