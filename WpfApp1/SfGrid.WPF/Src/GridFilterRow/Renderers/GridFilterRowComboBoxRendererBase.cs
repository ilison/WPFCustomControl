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
    /// Represents a class which handles the filter operation that loads the ComboBoxAdv in a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridFilterRowComboBoxRendererBase : GridFilterRowCellRenderer<ContentControl, ComboBoxAdv>
    {
        internal ItemPropertiesProvider provider;
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFilterRowComboBoxRendererBase"/> class.
        /// </summary>
        public GridFilterRowComboBoxRendererBase()
        {
            this.IsDropDownable = true;
        }
        #endregion

        #region Override Methods

        #region Display/Edit Binding Overrides

        /// <summary>
        /// Method that invoked when the DisplayElement is loaded.
        /// </summary>
        /// <returns></returns>
        protected override ContentControl OnCreateDisplayUIElement()
        {
            var contentControl = base.OnCreateDisplayUIElement();
#if WPF
            contentControl.FocusVisualStyle = null;
#endif
            return contentControl;
        }
        /// <summary>
        /// Called when [create edit unique identifier element].
        /// </summary>
        /// <returns></returns>
        protected override ComboBoxAdv OnCreateEditUIElement()
        {
            var comboBox = base.OnCreateEditUIElement();
#if WPF
            VisualContainer.SetWantsMouseInput(comboBox, true);
#endif
            return comboBox;
        }

        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            var textBind = new Binding { Path = new PropertyPath("FilterRowText"), Mode = BindingMode.TwoWay, Source = column, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            uiElement.SetBinding(ContentControl.ContentProperty, textBind);
            var verticalAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalAlignmentProperty, verticalAlignment);
            uiElement.Margin = ProcessUIElementPadding(column);
            uiElement.HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, ComboBoxAdv uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            InitializeEditBinding(uiElement, dataColumn);
            var textAlignment = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.OneWay, Source = column, Converter = new TextAlignmentToHorizontalAlignmentConverter() };
            uiElement.SetBinding(Control.HorizontalContentAlignmentProperty, textAlignment);
            var verticalAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalAlignment);
            uiElement.HorizontalContentAlignment = HorizontalAlignment.Left;
#if WINDOWS_UAP
            uiElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
#endif
        }

        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Called when [edit element loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var combobox = ((ComboBoxAdv)sender);
#if WinRT
            combobox.Focus(FocusState.Programmatic);
#else
            combobox.Focus();
#endif
            combobox.KeyDown += OnComboboxKeyDown;
            combobox.SelectionChanged += OnComboBoxSelectionChanged;
        }

        /// <summary>
        /// UnWire the wired events.
        /// </summary>
        /// <param name="uiElement"></param>
        protected override void OnUnwireEditUIElement(ComboBoxAdv uiElement)
        {
            uiElement.SelectionChanged -= OnComboBoxSelectionChanged;
            uiElement.KeyDown -= OnComboboxKeyDown;
        }

#endregion

        #region ShouldGridHandleKeyDown

            /// <summary>
            /// Let Renderer decide whether the parent grid should be allowed to handle keys and prevent
            /// the key event from being handled by the visual UIElement for this renderer. If this method
            /// returns true the parent grid will handle arrow keys and set the Handled flag in the event
            /// data. Keys that the grid does not handle will be ignored and be routed to the UIElement
            /// for this renderer.
            /// </summary>
            /// <param name="e">A <see cref="KeyEventArgs" /> object.</param>
            /// <returns>
            /// True if the parent grid should be allowed to handle keys; false otherwise.
            /// </returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            if (!HasCurrentCellState || !IsInEditing)
                return true;

            var comboBox = (CurrentCellRendererElement as ComboBoxAdv);

#if WPF
            if ((CheckAltKeyPressed() && e.SystemKey == Key.Down) || ((CheckAltKeyPressed() && e.SystemKey == Key.Up) || e.Key == Key.F4))
#else
            if ((CheckAltKeyPressed() && e.Key == Key.Down) || ((CheckAltKeyPressed() && e.Key == Key.Up) || e.Key == Key.F4))
#endif
            {
                if (!comboBox.IsDropDownOpen)
                    comboBox.IsDropDownOpen = true;
#if WinRT || UNIVERSAL
                comboBox.Focus(FocusState.Programmatic);
#endif
                return false;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    if (!CheckControlKeyPressed() && IsInEditing)
                    {
                        this.SetFocus(true);
                        return false;
                    }
                    return true;
                case Key.Escape:
                    if (comboBox.IsDropDownOpen)
                    {
                        comboBox.IsDropDownOpen = false;
                        this.SetFocus(true);
                        e.Handled = true;
                        return false;
                    }
                    return true;
                case Key.Home:
                case Key.End:
                case Key.Down:
                case Key.Up:
                case Key.PageUp:
                case Key.PageDown:
#if WinRT || UNIVERSAL
                    return !((ComboBox)CurrentCellRendererElement).IsDropDownOpen;
#endif
#if WPF
                    return !comboBox.IsDropDownOpen;
#endif
                case Key.Tab:
                    if (comboBox.IsDropDownOpen)
                    {
                        //WPF-29214 Need to check SelectedItem when the user doesnt select anything, need to skip the 
                        //filtering operation.
                        if (!comboBox.AllowMultiSelect && comboBox.SelectedItem == null)
                            return true;

                        var filterValues = comboBox.AllowMultiSelect
                            ? comboBox.SelectedItems.Cast<object>().ToList()
                            : new List<object>() { comboBox.SelectedItem };
                        var toatalItems = comboBox.Items.SourceCollection.Cast<object>().ToList();
                        this.ProcessMultipleFilters(filterValues, toatalItems);
                        comboBox.IsDropDownOpen = false;
                        return true;
                    }
                    break;
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

        #endregion

        #endregion

        #region Public Methods

        ///<summary>
        /// This method is used to get the FormattedString for FilterElement, it returns the Actual Value with formatted text.
        /// </summary>
        /// <param name="item"></param>
        public string GetFormattedString(object item)
        {
            var Column = this.FilterRowCell.DataColumn.GridColumn;
            if (item != null && item is FilterRowElement)
            {
                var filterElement = item as FilterRowElement;
                if (filterElement.Record != null)
                    return FilterHelpers.GetFormattedString(Column, provider, filterElement.Record, true);

                return FilterHelpers.GetFormattedString(Column, provider, filterElement.ActualValue, false);
            }
            return FilterHelpers.GetFormattedString(Column, provider, item, false);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Enven handler when selection is changed in ComboBoxAdv.
        /// </summary>
        /// <param name="sender">The ComboBoxAdv control.</param>
        /// <param name="e">Arguments that denotes the selection changes.</param>
        protected virtual void OnComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {                  
            var comboBox = (ComboBoxAdv)sender;
            this.IsValueChanged = true;
            if ((comboBox.AllowMultiSelect && !comboBox.EnableOKCancel &&
                this.DataGrid.FilterRowPosition != FilterRowPosition.Bottom) || HasCurrentCellState)
            {
                var filterValues = comboBox.AllowMultiSelect
                    ? comboBox.SelectedItems.Cast<object>().ToList()
                    : new List<object>() { comboBox.SelectedItem };
                var toatalItems = comboBox.Items.SourceCollection.Cast<object>().ToList();
                this.ProcessMultipleFilters(filterValues, toatalItems);
            }
        }

        /// <summary>
        /// Enven handler when Key is pressed in ComboBoxAdv.
        /// </summary>
        /// <param name="sender">The ComboBoxAdv control.</param>
        /// <param name="e">Arguments that returns the keydown informations.</param>
        protected virtual void OnComboboxKeyDown(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBoxAdv;
#if WPF
            if ((CheckAltKeyPressed() && e.SystemKey == Key.Down) || ((CheckAltKeyPressed() && e.SystemKey == Key.Up) || e.Key == Key.F4))
#else
            if ((CheckAltKeyPressed() && e.Key == Key.Down) || ((CheckAltKeyPressed() && e.Key == Key.Up) || e.Key == Key.F4))
#endif
            {
                if (!comboBox.IsDropDownOpen)
                    comboBox.IsDropDownOpen = true;
#if WinRT || UNIVERSAL
                comboBox.Focus(FocusState.Programmatic);
#endif
            }
        }

        #endregion

        #region Private Methods

        private List<FilterRowElement> GetItemsSource(GridColumn column)
        {
            IEnumerable<RecordValuePair> itemsSource;
            IEnumerable<object> items = null;
            List<FilterRowElement> itemsCollection;
            var provider = this.DataGrid.View.GetPropertyAccessProvider();
            var displayTextFilter = column.ColumnFilter == ColumnFilter.DisplayText;
            if (column.isDisplayMultiBinding && displayTextFilter && !column.UseBindingValue)
                throw new InvalidOperationException("Set UseBindingValue to true to reflect the property value as GridColumn.DisplayBinding is MultiBinding");

            if (this.DataGrid.View is Syncfusion.Data.PagedCollectionView)
            {
                items = (this.DataGrid.View as Syncfusion.Data.PagedCollectionView).GetInternalList().Cast<object>().ToList();
            }
            else if (this.DataGrid.View is Syncfusion.Data.VirtualizingCollectionView)
            {
                var virtualItems = (column.DataGrid.View as VirtualizingCollectionView).GetInternalSource();
                if (virtualItems != null)
                    items = virtualItems.Cast<object>();
                else
                    itemsSource = null;
            }
            else
                items = column.DataGrid.View.SourceCollection.ToList<object>();

            itemsSource = items.Select(x =>
            {
                if (column.IsUnbound)
                {
                    var value = column.DataGrid.GetUnBoundCellValue(column, x);
                    return new RecordValuePair(value, x);
                }
                else if (displayTextFilter && column.isDisplayMultiBinding)
                    return new RecordValuePair(provider.GetFormattedValue(x, column.MappingName), x);
                else
                    return new RecordValuePair(provider.GetValue(x, column.MappingName), x);
            }).Distinct(new RecordValueEqualityComparer<RecordValuePair>());

            itemsCollection = itemsSource.Select(item => new FilterRowElement
            {
                ActualValue = item.Value,
                Record = column.isDisplayMultiBinding || (isSupportDisplayTextFiltering(column) && !this.DataGrid.View.IsLegacyDataTable) ? item.Record : null,
                FormattedString = this.GetFormattedString
            }).ToList();

            var gridView = this.DataGrid.View;
            if (!column.AllowBlankFilters)
                itemsCollection = itemsCollection.Where(x => x.ActualValue != null).ToList();

            if (displayTextFilter)
                itemsCollection = itemsCollection.Distinct(new FilterControlEqualityComparer<FilterRowElement>()).ToList();

            itemsCollection.Sort(new FilterElementAscendingOrder());
            return itemsCollection;
        }

        /// <summary>
        /// Initialize the ComboBoxAdv with required fields.
        /// </summary>
        /// <param name="uiElement">The ComboBoxAdv that loaded in Edit mode.</param>
        /// <param name="dataColumn">The DataColumn that loads the ComboBoxAdv.</param>
        protected virtual void InitializeEditBinding(ComboBoxAdv uiElement, DataColumnBase dataColumn)
        {
            var comboBoxColumn = dataColumn.GridColumn as GridComboBoxColumn;
            if (this.DataGrid != null && this.DataGrid.View != null)
                provider = this.DataGrid.View.GetPropertyAccessProvider();
            var itemSource = this.GetItemsSource(dataColumn.GridColumn);
            if (uiElement.DisplayMemberPath == string.Empty)
                uiElement.DisplayMemberPath = "DisplayText";
            uiElement.ItemsSource = itemSource;
            uiElement.SelectedItems = null;

        }

        private static Thickness ProcessUIElementPadding(GridColumn column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
#if WinRT || UNIVERSAL
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(7.5 + padLeft, padTop, 2 + padRight,  padBotton)
                           : new Thickness(7.5, 1, 2, 0);
#else
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(1 + padLeft, 0 + padTop, 1 + padRight, 0 + padBotton)
                           : new Thickness(1, 0, 1, 0);
#endif
        }

        #endregion
    }
}
