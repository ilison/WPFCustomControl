#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Windows.Shared;
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Input;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.Grid;

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
    /// <summary>
    ///  Renderer for <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn"/> which is used to display the currency value.
    /// </summary>
    public class TreeGridCellCurrencyRenderer : TreeGridVirtualizingCellRenderer<TextBlock, CurrencyTextBox>
    {
        #region Override Methods

        #region Display/Edit Binding Overrides
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            //Note: Padding cannot be binded for Display UI Element, since the padding will be irregular to match Edit/Display UI Elements
            //The Column will be refreshed via Dependency CallBack.
            uiElement.Padding = ProcessUIElementPadding(dataColumn.TreeGridColumn);
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, CurrencyTextBox uiElement, object dataContext)
        {
            InitializeEditUIElement(uiElement, dataColumn.TreeGridColumn);
            BindingExpression = uiElement.GetBindingExpression(CurrencyTextBox.ValueProperty);
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
        }

        #endregion

        #region Display/Edit Value Overrides
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (!HasCurrentCellState)
                return base.GetControlValue();
            return CurrentCellRendererElement.GetValue(IsInEditing ? CurrencyTextBox.ValueProperty : TextBlock.TextProperty);
        }
        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
                ((CurrencyTextBox)CurrentCellRendererElement).Value = (decimal?)value;
            else
                throw new InvalidOperationException("Value cannot be Set for Unloaded Editor");
        }

        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Invoked when the edit element(CurrencyTextBox) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (CurrencyTextBox)sender;
            uiElement.ValueChanged += OnValueChanged;
            uiElement.Focus();
            if (this.TreeGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll && PreviewInputText == null)
            {
                uiElement.SelectAll();
            }
            else
            {
                if (PreviewInputText == null || char.IsLetter(PreviewInputText.ToString(), 0))
                {
                    var index = uiElement.Text.Length;
                    uiElement.Select(index + 1, 0);
                    return;
                }
                decimal value;
                decimal.TryParse(PreviewInputText.ToString(), out value);
                uiElement.Value = value;
                var caretIndex = (uiElement.Text).IndexOf(PreviewInputText.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                uiElement.Select(caretIndex + 1, 0);
            }
            PreviewInputText = null;
        }

        /// <summary>
        /// Invoked when the edit element(CurrencyTextBox) is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            ((CurrencyTextBox)sender).ValueChanged -= OnValueChanged;           
        }

        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        protected override void OnUnwireEditUIElement(CurrencyTextBox uiElement)
        {
            uiElement.ValueChanged -= OnValueChanged;
        }

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
            if (!HasCurrentCellState || !IsInEditing)
                return true;

            var CurrentCellUIElement = (CurrencyTextBox)CurrentCellRendererElement;
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        CurrentCellUIElement.ClearValue(CurrencyTextBox.ValueProperty);
                        return true;
                    }
                case Key.Left:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Right:
                    return (CurrentCellUIElement.SelectionStart >= CurrentCellUIElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                //WPF-18339- When navigating by Up or Down key, should return here if IsScrollingOnCircle is true for that UIElement
                case Key.Up:
                case Key.Down:
                    return !CurrentCellUIElement.IsScrollingOnCircle;
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

        #endregion

        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (CurrencyTextBox)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(CurrencyTextBox.ValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            base.CurrentRendererValueChanged();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Computes the Padding for the Display UIElement.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <remarks>Padding Cannot be applied at RunTime, as Currently We dont haven't constructed Padding via Binding Expression</remarks>
        /// <returns></returns>
        private static Thickness ProcessUIElementPadding(TreeGridColumn column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(TreeGridColumn.PaddingProperty);
#if WPF
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 2 + padTop, 3 + padRight, 5 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(2 + padLeft, 2 +padTop, 3 + padRight, 2+ padBotton)
                           : new Thickness(2, 2, 3, 2);
#endif
        }

        /// <summary>
        /// Processes the edit binding.
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        /// <param name="gridColumn">Currency Column</param>
        private void InitializeEditUIElement(EditorBase uiElement, TreeGridColumn gridColumn)
        {
            uiElement.TextSelectionOnFocus = false;
            var currencyColumn = (TreeGridCurrencyColumn)gridColumn;
            var bind = currencyColumn.ValueBinding.CreateEditBinding(currencyColumn.GridValidationMode != GridValidationMode.None, gridColumn);
            uiElement.SetBinding(CurrencyTextBox.ValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowScrollingOnCircle"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(EditorBase.IsScrollingOnCircleProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinValue"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.MinValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxValue"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.MaxValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencyDecimalDigits"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencyDecimalDigitsProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencyGroupSeparator"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencyGroupSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencySymbol"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencySymbolProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencyDecimalSeparator"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencyDecimalSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencyGroupSizes"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencyGroupSizesProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencyPositivePattern"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencyPositivePatternProperty, bind);
            bind = new Binding { Path = new PropertyPath("CurrencyNegativePattern"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.CurrencyNegativePatternProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxValidation"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(EditorBase.MaxValidationProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinValidation"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(EditorBase.MinValidationProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowNullValue"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(EditorBase.UseNullOptionProperty, bind);
            bind = new Binding { Path = new PropertyPath("NullValue"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.NullValueProperty, bind);
            uiElement.WatermarkTextIsVisible = true;
            bind = new Binding { Path = new PropertyPath("NullText"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.WatermarkTextProperty, bind);
#if WPF
            bind = new Binding { Path = new PropertyPath("TextDecorations"), Mode = BindingMode.TwoWay, Source = currencyColumn };
            uiElement.SetBinding(CurrencyTextBox.TextDecorationsProperty, bind);
#endif
        }
        #endregion
    }
}
