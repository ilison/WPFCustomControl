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
    using Windows.Foundation;
#endif
    /// <summary>
    /// Represents a class which handles the filter operation that loads the Text box in a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridFilterRowTextBoxRenderer : GridFilterRowCellRenderer<TextBlock, TextBox>
    {
        #region Display/Edit Binding Overrides
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            uiElement.Padding = ProcessUIElementPadding(dataColumn.GridColumn);
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="filterValue">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, TextBox uiElement, object dataContext)
        {
            var filterValue = dataColumn.GridColumn.FilteredFrom == FilteredFrom.FilterRow &&
                 dataColumn.GridColumn.FilterPredicates.Count > 0 ? dataColumn.GridColumn.FilterPredicates.FirstOrDefault().FilterValue : null;
            uiElement.Text = filterValue != null ? (string)filterValue : string.Empty;
            base.OnInitializeEditElement(dataColumn, uiElement, filterValue);
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

        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (!HasCurrentCellState)
                return base.GetControlValue();
            return CurrentCellRendererElement.GetValue(IsInEditing ? TextBox.TextProperty : TextBlock.TextProperty);
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState)
                return;
            if (IsInEditing)
                ((TextBox)CurrentCellRendererElement).Text = value != null ? (string)value : string.Empty;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
        #endregion

        #region Wire evnets that needs to be done.

        /// <summary>
        /// Called when [edit element loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (TextBox)sender;
            uiElement.TextChanged += OnTextChanged;

            if (!this.FilterRowCell.IsDropDownOpen)
#if WinRT || UNIVERSAL
                uiElement.Focus(FocusState.Programmatic);
            uiElement.KeyUp += OnKeyUp;
#else
                uiElement.Focus();
#endif
            if ((this.DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || this.DataGrid.IsAddNewIndex(this.CurrentCellIndex.RowIndex)) && PreviewInputText == null)
            {
                uiElement.SelectAll();
            }
            else
            {
                if (PreviewInputText == null)
                {
                    var index = uiElement.Text.Length;
                    uiElement.Select(index + 1, 0);
                    return;
                }
                uiElement.Text = PreviewInputText;
                var caretIndex = (uiElement.Text).IndexOf(PreviewInputText.ToString());
                uiElement.Select(caretIndex + 1, 0);
            }
            PreviewInputText = null;
        }


        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        protected override void OnUnwireEditUIElement(TextBox uiElement)
        {
            uiElement.TextChanged -= OnTextChanged;
#if !WPF
            uiElement.KeyUp -= OnKeyUp;
#endif
        }

        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
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
                if (!(CurrentCellRendererElement is TextBox))
                    return true;
            }
#else

            if (!IsInEditing)
                return true;
#endif

            var CurrentCellUIElement = (TextBox)CurrentCellRendererElement;
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
#if WPF
                case Key.Left:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Right:
                    return (CurrentCellUIElement.CaretIndex >= CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.CaretIndex == CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

#endregion

        #region Private Methods
        /// <summary>
        /// Computes the Padding for the Display UIElement.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        private Thickness ProcessUIElementPadding(GridColumn column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
#if WinRT || UNIVERSAL
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 3 + padTop, 5 + padRight, 5 + padBotton)
                           : new Thickness(3, 1, 6, 6);
#else
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 1 + padTop, 3 + padRight, 1 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#endif
        }
        #endregion

        #region PreviewTextInput Override

#if WinRT || UNIVERSAL
        /// <summary>
        /// Raises the <see cref="E:PreviewTextInput"></see>
        ///     event.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>

        protected override void OnPreviewTextInput(KeyEventArgs e)
        {
            base.OnPreviewTextInput(e);
            if (e.Key >= Key.Number0 && e.Key <= Key.Number9)
                PreviewInputText = (e.Key - Key.Number0).ToString();
            else if (e.Key >= Key.NumberPad0 && e.Key <= Key.NumberPad9)
                PreviewInputText = (e.Key - Key.NumberPad0).ToString();
            else if ((e.Key >= Key.A && e.Key <= Key.Z))
                PreviewInputText = e.Key.ToString();
        }
#endif

        #endregion
    }
}
