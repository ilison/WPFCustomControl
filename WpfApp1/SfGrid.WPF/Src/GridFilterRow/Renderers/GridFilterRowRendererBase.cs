#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data;
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
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections;
using Syncfusion.Windows.Tools.Controls;
#endif

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{

    /// <summary>
    /// The GridFilterRowCellRenderer is a abstract class to load UIEelents in FilterRowCell.
    /// </summary>
    /// <typeparam name="D"></typeparam>
    /// <typeparam name="E"></typeparam>
    [ClassReference(IsReviewed = false)]
    public abstract class GridFilterRowCellRenderer<D, E> : GridVirtualizingCellRenderer<D, E>, IGridFilterRowRenderer
        where D : FrameworkElement, new()
        where E : FrameworkElement, new()
    {
        #region Field

        protected bool IsValueChanged;

        #endregion

        /// <summary>
        /// Gets or sets the corresponding GridFilterRowCell.
        /// </summary>
        protected IGridFilterRowCell FilterRowCell
        {
            get
            {
                if (this.HasCurrentCellState)
                    return this.CurrentCellElement as GridFilterRowCell;
                return null;
            }
        }

        /// <summary>
        /// Creates the FilterRowCell.
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement OnPrepareUIElements()
        {
            var gridfilterRowCell = this.DataGrid.RowGenerator.GetGridCell<GridFilterRowCell>() as GridFilterRowCell;
            return gridfilterRowCell;
        }
#if WPF

        // WPF-34216 - We did n't support lightweight template for GridFilterRowCell.
        protected override void OnRenderCell(System.Windows.Media.DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {
            
        }
#endif

        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, D uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            var textBind = new Binding { Path = new PropertyPath("FilterRowText"), Mode = BindingMode.TwoWay, Source = column, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            uiElement.SetBinding(TextBlock.TextProperty, textBind);
            var verticalAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalAlignmentProperty, verticalAlignment);

            uiElement.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="filterValue">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, E uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            uiElement.SetValue(TextBox.TextAlignmentProperty, TextAlignment.Left);
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
            var verticalContentAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalContentAlignment);
        }

        /// <summary>
        /// Starts an edit operation on a current cell.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// Specifies the row and column index of the cell to start an edit operation.
        /// </param>
        /// <param name="cellElement">
        /// Specifies the UIElement of the cell to start an edit operation.
        /// </param>
        /// <param name="column">
        /// The corresponding column to edit the cell.
        /// </param>
        /// <param name="record">
        /// The corresponding record to edit the cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the current cell starts an editing; otherwise, <b>false</b>.
        /// </returns>
        public override bool BeginEdit(ScrollAxis.RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, GridColumn column, object record)
        {
            this.IsValueChanged = false;
            return base.BeginEdit(cellRowColumnIndex, cellElement, column, record);
        }

        /// <summary>
        /// Method which is used to Initialize the custom style for FilterRow cell
        /// by using FilterRowCellStyle property.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// <param name="record"></param>
        /// <remarks></remarks>
        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement as GridFilterRowCell;
            var column = dataColumn.GridColumn;
            if(cell == null || column == null)
                return;
            Style style = null;
            if (!column.hasFilterRowCellStyle)
            {
                if (cell.ReadLocalValue(GridHeaderCellControl.StyleProperty) != DependencyProperty.UnsetValue)
                    cell.ClearValue(GridHeaderCellControl.StyleProperty);
            }
            else if (column.hasFilterRowCellStyle)
                style = column.FilterRowCellStyle;

            if (style != null)
                cell.Style = style;
            else
                cell.ClearValue(GridHeaderCellControl.StyleProperty);
        }

        /// <summary>
        /// Process the filtering when the FilterRowCondition is changed in corresponding column.
        /// </summary>
        /// <param name="filterRowCondition">The new FilterRowCondition that have been changed.</param>
        public virtual void OnFilterRowConditionChanged(string filterRowCondition)
        {
            if (filterRowCondition == FilterRowHelpers.GetResourceWrapper(this.FilterRowCell.DataColumn.GridColumn.FilterRowCondition))
                return;

            this.FilterRowCell.DataColumn.GridColumn.UpdateFilterType(FilterRowHelpers.GetFilterRowCondition(filterRowCondition));
            object filterValue;
            if (this.IsInEditing)
                filterValue = this.GetControlValue();
            else
                filterValue = this.FilterRowCell.DataColumn.GridColumn.FilterPredicates.Count > 0 && this.FilterRowCell.DataColumn.GridColumn.FilteredFrom == FilteredFrom.FilterRow
                    ? this.FilterRowCell.DataColumn.GridColumn.FilterPredicates[0].FilterValue : null;
            var filterType = this.FilterRowCell.DataColumn.GridColumn.FilterRowCondition;
            this.ProcessSingleFilter(filterValue);
            this.SetFocus(true);
            if (this.IsInEditing && (filterType == FilterRowCondition.Empty || filterType == FilterRowCondition.NotEmpty || filterType == FilterRowCondition.NotNull
            || filterType == FilterRowCondition.Null))
            {                
                this.SetControlValue(null);
                this.DataGrid.SelectionController.CurrentCellManager.EndEdit();
            }
            else if (!this.IsInEditing)
#if WPF
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                {
#endif
                    this.DataGrid.SelectionController.CurrentCellManager.BeginEdit();
#if WPF
                }), DispatcherPriority.ApplicationIdle);
#endif
        }

        /// <summary>
        /// Process filtering operation with the given filter value for the particular column.
        /// </summary>
        /// <param name="filterValue">Value that want to be filter in particular column.</param>
        public virtual void ProcessSingleFilter(object filterValue)
        {
            if (this.FilterRowCell.DataColumn.GridColumn == null)
                return;

            var filterPredicates = this.GetFilterPredicates(filterValue);
            var _filterText = this.GetFilterText(filterPredicates);
            this.ApplyFilters(filterPredicates, _filterText);
            this.IsValueChanged = false;
        }

        /// <summary>
        /// Process filtering operation with the given filter values in the particular column.
        /// </summary>
        /// <param name="filterValues">The list of Values that want to be filter in particular column.</param>
        /// <param name="totalItems">The list of items which is loaded in ComboBox.</param>
        public virtual void ProcessMultipleFilters(List<object> filterValues, List<object> totalItems)
        {
            if (this.FilterRowCell.DataColumn.GridColumn == null || totalItems == null || filterValues == null)
                return;

            var filterPredicates = this.GetMultiSelectFilterPredicates(filterValues.Cast<FilterRowElement>().ToList(), totalItems.Cast<FilterRowElement>().ToList());
            string _filterText = this.GetFilterText(filterPredicates);
            this.ApplyFilters(filterPredicates,_filterText);
            this.IsValueChanged = false;
        }

        /// <summary>
        /// Gets the FilterText that want to be displayed in particular FilterRow Cell.
        /// </summary>
        /// <param name="filterValues">The list of FilterPredicates that have been created to the particular column.</param>
        public virtual string GetFilterText(List<FilterPredicate> filterPredicates)
        {
            string _filterText = string.Empty;

            if (filterPredicates.Count > 0)
            {
#if WPF
                if(this.EditorType == typeof(ComboBoxAdv) && this.HasCurrentCellState)
                {
                    var comboBox = (ComboBoxAdv) this.CurrentCellRendererElement;
                    if (comboBox.AllowMultiSelect)
                    {
                        var selectedItems = ((IList) comboBox.SelectedItems).Cast<FilterRowElement>().ToList();
                        foreach (var item in selectedItems)
                        {
                            _filterText += item.DisplayText;
                            if (selectedItems.IndexOf(item) < selectedItems.Count - 1)
                                _filterText += comboBox.SelectedValueDelimiter;
                        }
                    }
                    else
                        _filterText = (comboBox.SelectedItem as FilterRowElement).DisplayText;
                    return _filterText;
                }
#endif
                var filterRowCond = this.FilterRowCell.DataColumn.GridColumn.FilterRowCondition;
                if (filterRowCond == FilterRowCondition.Null || filterRowCond == FilterRowCondition.NotNull
                    || filterRowCond == FilterRowCondition.Empty || filterRowCond == FilterRowCondition.NotEmpty)
                    _filterText = FilterRowHelpers.GetResourceWrapper(this.FilterRowCell.DataColumn.GridColumn.FilterRowCondition);
                else
                {
                    var filterValue = filterPredicates[0].FilterValue;
                    if (filterValue != null && filterValue.ToString() != string.Empty)
                        _filterText = this.FilterRowCell.DataColumn.GridColumn.GetFormatedFilterText(filterValue);
                }
            }

            return _filterText;
        }

        /// <summary>
        /// Apply the filter to the corresponding column with given FilterPredicates.
        /// </summary>
        /// <param name="filterPredicates">The list of FilterPredicate that want to be apply in a particular column.</param>
        /// <param name="filterText">The text that want to displayed in FilterRowCell of a particular column.</param>
        protected void ApplyFilters(List<FilterPredicate> filterPredicates, string filterText)
        {
            if (filterPredicates != null && filterPredicates.Count > 0)
            {
                this.FilterRowCell.DataColumn.GridColumn.FilteredFrom = FilteredFrom.FilterRow;
                this.DataGrid.GridModel.FilterColumn(this.FilterRowCell.DataColumn.GridColumn, filterPredicates);
                this.FilterRowCell.DataColumn.GridColumn.SetFilterRowText(filterText);
                this.DataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(false)));
            }
            else if (this.FilterRowCell.DataColumn.GridColumn.FilteredFrom == FilteredFrom.FilterRow)
            {
                this.FilterRowCell.DataColumn.GridColumn.FilteredFrom = FilteredFrom.None;                
                this.DataGrid.GridModel.FilterColumn(this.FilterRowCell.DataColumn.GridColumn, new List<FilterPredicate>());
                this.DataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(false)));
            }
        }

        /// <summary>
        /// Gets the list of FilterPredicates for the corresponding column.
        /// </summary>
        /// <param name="filterValue">Value that want to be filter in corresponding column.</param>
        /// <returns>
        /// Returns the list of FilterPredicates that have been generated for the particular column.
        /// </returns>
        protected List<FilterPredicate> GetFilterPredicates(object filterValue)
        {
            var column = this.FilterRowCell.DataColumn.GridColumn;
            if (column == null)
                return null;

            var filterPredicates = new List<FilterPredicate>();
            if (column.FilterRowCondition == FilterRowCondition.NotNull || column.FilterRowCondition == FilterRowCondition.Null)
                filterValue = null;
            else if (column.FilterRowCondition == FilterRowCondition.Empty || column.FilterRowCondition == FilterRowCondition.NotEmpty)
                filterValue = string.Empty;
            else if ((filterValue == null || filterValue.ToString() == string.Empty))
                return filterPredicates;

            var filterMode = isSupportDisplayTextFiltering(column) || (column.GetType() != typeof(GridTextColumn) && column.GetFilterBehavior() == FilterBehavior.StringTyped) ? ColumnFilter.DisplayText : ColumnFilter.Value;
            var filterPredicate = new FilterPredicate()
            {
                FilterBehavior = column.GetFilterBehavior(),
                FilterType = FilterRowHelpers.GetFilterType(column.FilterRowCondition),
                FilterValue = filterValue,
                IsCaseSensitive = column.IsCaseSensitiveFilterRow,
                PredicateType = PredicateType.And ,
                FilterMode = filterMode
            };
            filterPredicates.Add(filterPredicate);
            return filterPredicates;
        }

        /// <summary>
        /// invokes to return either column support display text based fitlering
        /// </summary>
        /// <param name="column">GridColumn</param>
        /// <returns> if column support display Text based filtering true, otehrwise false</returns>
        internal bool isSupportDisplayTextFiltering(GridColumn column)
        {
            if (column.ColumnFilter==ColumnFilter.DisplayText && ( column.FilterRowEditorType == "TextBox" || column.FilterRowEditorType == "ComboBox" || column.FilterRowEditorType == "MultiSelectComboBox"))
                return true;

            return false;
        }

        /// <summary>
        /// Gets the list of FilterPredicates for the corresponding column.
        /// </summary>
        /// <param name="filterValues">The list of values that want to be filter in the particular column.</param>
        /// <param name="totalItems">The list of items which is loaded in ComboBox control.</param>
        /// <returns>
        /// Returns the list of FilterPredicates that have been generated for the particular column.
        /// </returns>
        protected List<FilterPredicate> GetMultiSelectFilterPredicates(List<FilterRowElement> filterValues, List<FilterRowElement> totalItems)
        {
            if (filterValues == null || totalItems == null)
                return null;
            var column = this.FilterRowCell.DataColumn.GridColumn;
            
            int selectedCount = filterValues.Count();
            int unSelectedCount = totalItems.Count - selectedCount;
            List<FilterPredicate> filterPredicates = new List<FilterPredicate>();
            //WPF-29214 Removed the UnSelectedCount condition because when there is no any selected values we need to clear
            //the filter.
            if (selectedCount > unSelectedCount && unSelectedCount != 0)
            {
                filterPredicates = totalItems.Where(x => !filterValues.Contains(x)).Select(x => new FilterPredicate()
                {
                    FilterBehavior = FilterBehavior.StronglyTyped,
                    FilterType = FilterType.NotEquals,
                    FilterValue = isSupportDisplayTextFiltering(column) && (x.DisplayText != GridResourceWrapper.Blanks) ? x.DisplayText : x.ActualValue,
                    IsCaseSensitive = true,
                    PredicateType = PredicateType.And,
                    FilterMode = isSupportDisplayTextFiltering(column) ? ColumnFilter.DisplayText : ColumnFilter.Value
                }).ToList<FilterPredicate>();
            }
            else if (selectedCount != totalItems.Count && selectedCount != 0)
            {
                filterPredicates = filterValues.Select(x => new FilterPredicate()
                {
                    FilterBehavior = FilterBehavior.StronglyTyped,
                    FilterType = FilterType.Equals,
                    FilterValue = isSupportDisplayTextFiltering(column) && (x.DisplayText != GridResourceWrapper.Blanks) ? x.DisplayText : x.ActualValue,
                    IsCaseSensitive = true,
                    PredicateType = PredicateType.Or,
                    FilterMode = isSupportDisplayTextFiltering(column) ? ColumnFilter.DisplayText : ColumnFilter.Value
                }).ToList<FilterPredicate>();

                if (filterPredicates != null && filterPredicates.Count > 0)
                    filterPredicates[0].PredicateType = PredicateType.And;
            }
            return filterPredicates;
        }
        /// <summary>
        /// Invoked when filter cleared programmatically to update the cell            
        /// </summary>
        /// <param>The <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridCellRendererBase.SupportsRenderOptimization"/> Override this method to update the View when SupportRendererOptimization is false.</param>
        /// <param name="dataColumn">DataColumn of FilterRowCell.</param>
        protected internal virtual void OnClearFilter(DataColumnBase dataColumn)
        {
            //WPF-37891 While loading edit elemnt alone in FilterRow we need to unload and reload the Edit element.
            if (!this.SupportsRenderOptimization)
            {
                UnloadUIElements(dataColumn);
                OnPrepareUIElements(dataColumn, null, false);

                //While using the below way OnWiredEvents is not working because we have hook the events in Renderer loaded event.                
                //var editElement = (dataColumn.ColumnElement as GridFilterRowCell).Content;                
                //OnUnwireEditUIElement((E)editElement);
                //OnInitializeEditElement(dataColumn, (E)editElement, null);
                //OnWireEditUIElement((E)editElement);
            }
        }
        void IGridFilterRowRenderer.ClearFilter(DataColumnBase dataColumn)
        {
            OnClearFilter(dataColumn);
        }
    }
}
