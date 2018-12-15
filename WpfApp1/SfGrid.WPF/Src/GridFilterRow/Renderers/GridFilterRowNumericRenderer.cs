#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using System.Windows.Input;
using System.Globalization;
using Syncfusion.UI.Xaml.Grid.Helpers;
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
using Syncfusion.Windows.Shared;
using System.Windows.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using DoubleTextBox = SfNumericTextBox;
    using EventArgs = PointerRoutedEventArgs;
    using DateTimeEdit = SfDatePicker;
    using Windows.UI.Xaml;
#endif
    /// <summary>
    /// Represents a class which handles the filter operation that loads the Numeric text box in a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridFilterRowNumericRenderer : GridFilterRowCellRenderer<TextBlock, DoubleTextBox>
    {
        #region Override Methods

        #region Display/Edit Binding Overrides

        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            //Note: Padding cannot be binded for Display UI Element, since the padding will be irregular to match Edit/Display UI Elements
            //The Column will be refreshed via Dependency CallBack.
            uiElement.Padding = ProcessUIElementPadding(dataColumn.GridColumn);
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, DoubleTextBox uiElement, object dataContext)
        {
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            InitializeEditUIElement(uiElement, dataColumn.GridColumn);
#if WinRT ||UNIVERSAL
            uiElement.ValueChangedMode = ValueChange.OnKeyFocus;
#else
            uiElement.TextSelectionOnFocus = false;
#endif
        }

        /// <summary>
        /// When complete edit, we need to raise query again to provide entered value to customer.
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// <param name="currentRendererElement">The UIElement that was loaded in edit mdoe</param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// GridColumn - Column which is providing the information for Binding
        protected override void OnEditingComplete(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            base.OnEditingComplete(dataColumn, currentRendererElement);
            if (HasCurrentCellState && this.IsValueChanged && !this.FilterRowCell.DataColumn.GridColumn.ImmediateUpdateColumnFilter)
                this.ProcessSingleFilter(GetControlValue());
        }

        #endregion

        #region Display/Edit Value Overrides
        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
            {
                double? filterValue;
                if (value == null)
                    filterValue = null;
                else
                    filterValue = Convert.ToDouble(value);
                ((DoubleTextBox)CurrentCellRendererElement).Value = filterValue;
            }
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }

        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (!HasCurrentCellState)
                return base.GetControlValue();
            return CurrentCellRendererElement.GetValue(IsInEditing ? DoubleTextBox.ValueProperty : TextBlock.TextProperty);
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
            var uiElement = ((DoubleTextBox)sender);
            uiElement.ValueChanged += OnValueChanged;
#if WinRT || UNIVERSAL
            uiElement.KeyUp += OnKeyUp;
#endif
            if (!this.FilterRowCell.IsDropDownOpen)
#if WinRT || UNIVERSAL
                uiElement.Focus(FocusState.Programmatic);
#else
                uiElement.Focus();
#endif

            if ((this.DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || this.DataGrid.IsAddNewIndex(this.CurrentCellIndex.RowIndex)) && PreviewInputText == null)
            {
                uiElement.SelectAll();
            }
            else
            {
#if WinRT || UNIVERSAL
                if (PreviewInputText == null)
#else
                if (PreviewInputText == null || char.IsLetter(PreviewInputText.ToString(), 0))
#endif
                {
                    var index = uiElement.Text.Length;
                    uiElement.Select(index + 1, 0);
                    return;
                }
                double value;
                double.TryParse(PreviewInputText.ToString(), out value);
                uiElement.Value = value;
                var caretIndex = uiElement.Text.IndexOf(PreviewInputText, StringComparison.Ordinal);
                uiElement.Select(caretIndex + 1, 0);
            }
            PreviewInputText = null;
        }

        /// <summary>
        /// Called when [unwire edit UI element].
        /// </summary>
        /// <param name="uiElement">The UI element.</param>       
        protected override void OnUnwireEditUIElement(DoubleTextBox uiElement)
        {
            uiElement.ValueChanged -= OnValueChanged;
#if WinRT
            uiElement.KeyUp -= OnKeyUp;
#endif
        }

        #endregion

        #region PreviewTextInput Override
        /// <summary>
        /// Called when text is entered in the Data Control
        /// </summary>
        /// <param name="e">KeyRoutedEventArgs</param>
#if WinRT || UNIVERSAL
        protected override void OnPreviewTextInput(KeyEventArgs e)
        {
            if (e.Key >= Key.Number0 && e.Key <= Key.Number9)
                PreviewInputText = (e.Key - Key.Number0).ToString();
            else if (e.Key >= Key.NumberPad0 && e.Key <= Key.NumberPad9)
                PreviewInputText = (e.Key - Key.NumberPad0).ToString();
            else
                PreviewInputText = 0.ToString();
        }
#endif
        #endregion

        #region ShouldGridTryToHandleKeyDown
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
#if WPF
            if ((CheckAltKeyPressed() && e.SystemKey == Key.Down) || (CheckAltKeyPressed() && e.SystemKey == Key.Up))
#else
            if ((CheckAltKeyPressed() && e.Key == Key.Down) || (CheckAltKeyPressed() && e.Key == Key.Up))
#endif
            {
                this.FilterRowCell.OpenFilterOptionPopup();
                return false;
            }
            else if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
                return !this.FilterRowCell.IsDropDownOpen;
            else if (e.Key == Key.Enter && this.FilterRowCell.IsDropDownOpen)
                return false;

            bool isDropDownOpen = this.FilterRowCell.IsDropDownOpen;
            if (isDropDownOpen)
                this.FilterRowCell.CloseFilterOptionPopup();
#if WinRT || UNIVERSAL
            if (!IsInEditing)
            {
                ProcessPreviewTextInput(e);
                if (!(CurrentCellRendererElement is SfNumericTextBox))
                    return true;
            }
#else
            if (!IsInEditing)
                return true;
#endif
#if !WinRT && !UNIVERSAL
            var CurrentCellUIElement = (DoubleTextBox)CurrentCellRendererElement;
#else
            var CurrentCellUIElement = (SfNumericTextBox)CurrentCellRendererElement;
#endif
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        this.IsValueChanged = false;
                        return !isDropDownOpen;
                    }
                case Key.Enter:
                    if (!CheckControlKeyPressed() && IsInEditing)
                    {
                        this.ProcessSingleFilter(this.GetControlValue());
                        this.SetFocus(true);
                        e.Handled = true;
                        return false;
                    }
                    return true;
                case Key.Left:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Right:
                    return (CurrentCellUIElement.SelectionStart >= CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

#endregion

#endregion

        #region Event Handlers

        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="d">its the editor here</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
#if WinRT || UNIVERSAL
        private void OnValueChanged(object sender, ValueChangedEventArgs e)
#else
        void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#endif
        {
            if (!HasCurrentCellState)
                return;

            this.IsValueChanged = true;
            if (this.FilterRowCell.DataColumn.GridColumn.ImmediateUpdateColumnFilter)
            {
                this.ProcessSingleFilter(GetControlValue());
                this.SetFocus(true);
            }
        }
#if !WPF
        /// <summary>
        /// Invokes when pressing any key pressed in TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!HasCurrentCellState)
                return;

            if ((CheckAltKeyPressed() && e.Key == Key.Down) || (CheckAltKeyPressed() && e.Key == Key.Up))
            {
                if (!this.IsInEditing)
                    this.DataGrid.SelectionController.CurrentCellManager.BeginEdit();
                this.FilterRowCell.OpenFilterOptionPopup();
                e.Handled = true;
            }
        }
#endif
        #endregion

        #region Private Methods

        /// <summary>
        /// Computes the Padding for the Display UIElement.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        private static Thickness ProcessUIElementPadding(GridColumn column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
#if WinRT || UNIVERSAL
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft,  padTop, 2 + padRight,padBotton)
                           : new Thickness(3, 1, 3, 0);
#else
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#endif
        }

        /// <summary>
        /// Processes the edit binding.
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        /// <param name="column">GridColumn of the Editing Column</param>
        private void InitializeEditUIElement(DoubleTextBox uiElement, GridColumn numericColumn)
        {
#if WinRT || UNIVERSAL

            if (numericColumn is GridNumericColumn)
            {
                var bind = new Binding { Path = new PropertyPath("FormatString"), Source = numericColumn };
                uiElement.SetBinding(DoubleTextBox.FormatStringProperty, bind);
                bind = new Binding { Path = new PropertyPath("ParsingMode"), Source = numericColumn };
                uiElement.SetBinding(DoubleTextBox.ParsingModeProperty, bind);
            }
            else
            {
                uiElement.FormatString = string.Empty;
                uiElement.ParsingMode = Parsers.Double;
            }
            uiElement.AllowNull = true;
            uiElement.BlockCharactersOnTextInput = true;
#else
            uiElement.UseNullOption = true;
            Binding bind = null;
            if (numericColumn is GridPercentColumn)
                bind = new Binding { Path = new PropertyPath("PercentDecimalDigits"), Mode = BindingMode.TwoWay, Source = numericColumn };
            else if (numericColumn is GridCurrencyColumn)
                bind = new Binding { Path = new PropertyPath("CurrencyDecimalDigits"), Mode = BindingMode.TwoWay, Source = numericColumn };
            else if (numericColumn is GridNumericColumn)
                bind = new Binding { Path = new PropertyPath("NumberDecimalDigits"), Mode = BindingMode.TwoWay, Source = numericColumn };
            if (bind != null)
                uiElement.SetBinding(DoubleTextBox.NumberDecimalDigitsProperty, bind);
            else
                uiElement.NumberDecimalDigits = 0;

            uiElement.NumberGroupSizes = new Int32Collection { 0 };
            uiElement.NullValue = null;
#endif
            var filterValue = numericColumn.FilteredFrom == FilteredFrom.FilterRow &&
                              numericColumn.FilterPredicates.Count > 0 ? numericColumn.FilterPredicates.FirstOrDefault().FilterValue : null;
            if (filterValue == null || filterValue.ToString() == string.Empty)
                uiElement.Value = null;
            else
                uiElement.Value = Convert.ToDouble(filterValue);
        }
        #endregion
    }
}
