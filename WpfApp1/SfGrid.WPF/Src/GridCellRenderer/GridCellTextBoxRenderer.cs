#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Globalization;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Utility;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    [ClassReference(IsReviewed = false)]
    public class GridCellTextBoxRenderer : GridVirtualizingCellRenderer<TextBlock,TextBox>
    {
        #region Override Methods

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
            base.OnInitializeDisplayElement(dataColumn, uiElement,dataContext);
            uiElement.Padding = dataColumn.GridColumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, TextBox uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            var bind = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(TextBox.TextProperty, bind);
            BindingExpression = uiElement.GetBindingExpression(TextBox.TextProperty);
#if !WPF
            var Bind = new Binding { Path = new PropertyPath("IsSpellCheckEnabled"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.IsSpellCheckEnabledProperty, Bind);
#endif            
            
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
                ((TextBox) CurrentCellRendererElement).Text = (string) value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Invoked when the edit element(TextBox) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (TextBox)sender;
            uiElement.TextChanged += OnTextChanged;
#if WinRT || UNIVERSAL
            uiElement.Focus(FocusState.Programmatic);
#else
            uiElement.Focus();
#endif
            if ((this.DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || this.DataGrid.IsAddNewIndex(this.CurrentCellIndex.RowIndex)) && PreviewInputText==null)
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
        /// Invoked when the edit element(TextBox) is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).TextChanged -= OnTextChanged;
        }

        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        protected override void OnUnwireEditUIElement(TextBox uiElement)
        {
            uiElement.TextChanged -= OnTextChanged;
        }

        #endregion

        #region PreviewTextInput Override

        /// <summary>
        /// Raises the <see cref="E:PreviewTextInput"></see>
        ///     event.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
#if WinRT || UNIVERSAL
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
#if WinRT || UNIVERSAL
            if (!HasCurrentCellState)
                return true;
            if (!IsInEditing)
            {
                ProcessPreviewTextInput(e);
                if (!(CurrentCellRendererElement is TextBox))
                    return true;
            }
#else
            if (!HasCurrentCellState || !IsInEditing)
                return true;
#endif

            var CurrentCellUIElement = (TextBox) CurrentCellRendererElement;
            switch (e.Key)
            {
                case Key.Escape:
                {
                    CurrentCellUIElement.ClearValue(TextBox.TextProperty);
                    return true;
                }
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

        #region EndEdit

        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
        {
            var CurrentCellUIElement = (TextBox)CurrentCellRendererElement;
            if (canResetBinding)
                CurrentCellUIElement.ClearValue(TextBox.TextProperty);
            return base.EndEdit(dc, record, canResetBinding);
        }

        #endregion

        #endregion

        #region EventHandlers

        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            base.CurrentRendererValueChanged();
        }

        #endregion
    }
}
