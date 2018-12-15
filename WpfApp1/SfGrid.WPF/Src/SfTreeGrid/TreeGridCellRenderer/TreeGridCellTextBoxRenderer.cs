#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.Grid.Utility;
using System.Windows;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid;
#if WPF
using System.Windows.Controls;
using System.Windows.Input;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#endif



namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{   
#if UWP
    using Key = VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif

    public class TreeGridCellTextBoxRenderer : TreeGridVirtualizingCellRenderer<TextBlock, TextBox>
    {
        #region Display/Edit Value Overrides

        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            uiElement.Padding = dataColumn.TreeGridColumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, TextBox uiElement, object dataContext)
        {
            var column = dataColumn.TreeGridColumn;
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            var bind = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(TextBox.TextProperty, bind);
            BindingExpression = uiElement.GetBindingExpression(TextBox.TextProperty);
#if UWP
            var Bind = new Binding { Path = new PropertyPath("IsSpellCheckEnabled"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.IsSpellCheckEnabledProperty, Bind);
#endif

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
                ((TextBox)CurrentCellRendererElement).Text = (string)value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Invoked when the edit element(TextBox) is loaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (TextBox)sender;
            uiElement.TextChanged += OnTextChanged;
#if WPF
            uiElement.Focus();
#else
            uiElement.Focus(FocusState.Programmatic);
#endif
            if (this.TreeGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll && PreviewInputText == null)
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
        /// Invoked when the edit element(TextBox) is unloaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>
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
#if UWP
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
#if UWP
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

            var CurrentCellUIElement = (TextBox)CurrentCellRendererElement;
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        CurrentCellUIElement.ClearValue(TextBox.TextProperty);
                        return true;
                    }
#if WPF
                case Key.Left:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Right:
                    return (CurrentCellUIElement.CaretIndex >= CurrentCellUIElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.CaretIndex == CurrentCellUIElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

        #endregion

        #region EndEdit

        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (TextBox)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(TextBox.TextProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }

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
