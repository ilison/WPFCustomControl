#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using Syncfusion.UI.Xaml.Grid.Utility;
using System.Windows.Input;

#if WPF
using System.Windows.Controls;
using System.Windows.Data;
using Syncfusion.Windows.Shared;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.Grid;

#else
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using MaskedTextBox = SfMaskedEdit;
    using TextCompositionEventArgs = KeyRoutedEventArgs;
#endif
    /// <summary>
    ///  Renderer for <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn"/> which is used to display the masked data.
    /// </summary>
    public class TreeGridCellMaskRenderer : TreeGridVirtualizingCellRenderer<TextBlock, MaskedTextBox>
    {

        #region Override Methods

        #region Display/Edit Binding Overrides
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            //Note: Padding cannot be binded for Display UI Element, since the padding will be irregular to match Edit/Display UI Elements
            //The Column will be refreshed via Dependency CallBack.
            uiElement.Padding = dataColumn.TreeGridColumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, MaskedTextBox uiElement, object dataContext)
        {
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            InitializeEditUIElement(uiElement, dataColumn.TreeGridColumn);
            BindingExpression = uiElement.GetBindingExpression(MaskedTextBox.ValueProperty);

        }
        #endregion

        #region Display/ Edit Value Overrides
        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
                ((MaskedTextBox)CurrentCellRendererElement).Value = (string)value;
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
            return CurrentCellRendererElement.GetValue(IsInEditing ? MaskedTextBox.ValueProperty : TextBlock.TextProperty);
        }
        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Invoked when the edit element(MaskedTextBox or SfMaskedEdit) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            //WPF-23142 - When we enter the single value into Maskcolumn,
            //The value is not commited, because Binding is not updated in TextChangedEvent
            // So the ValueChanged event is trigger for update the bininding
#if WPF
            ((MaskedTextBox)sender).ValueChanged += OnValueChanged;
            ((MaskedTextBox)sender).Focus();

#else
            ((SfMaskedEdit)sender).TextChanged += OnTextChanged;
            ((SfMaskedEdit)sender).Focus(FocusState.Pointer);
#endif

            ProcessCaretIndex(sender as UIElement);
        }

        /// <summary>
        /// Invoked when the edit element(MaskedTextBox or SfMaskedEdit) is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
#if WPF
            ((MaskedTextBox)sender).ValueChanged -= OnValueChanged;           
#else
            ((SfMaskedEdit)sender).TextChanged -= OnTextChanged;           
#endif

        }
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            base.CurrentRendererValueChanged();
        }


        /// <summary>
        /// Called when [unwire edit UI element].
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        protected override void OnUnwireEditUIElement(MaskedTextBox uiElement)
        {
#if WPF
            uiElement.ValueChanged -= OnValueChanged;
#else
            uiElement.TextChanged -= OnTextChanged;
#endif
        }
        #endregion

        #region PreviewTextInput Override

#if WPF
        /// <summary>
        /// Called when text is entered in the Data Control
        /// </summary>
        /// <param name="e">KeyRoutedEventArgs</param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!HasCurrentCellState)
                return;
            PreviewInputText = e.Text;
        }
#else
        /// <summary>
        /// Called when text is entered in the Data Control
        /// </summary>
        /// <param name="e">KeyRoutedEventArgs</param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!HasCurrentCellState)
                return;
            base.OnPreviewTextInput(e);
            //Convert virtualkey to Physical key value.
            if (e.Key >= Key.Number0 && e.Key <= Key.Number9)
                PreviewInputText = (e.Key - Key.Number0).ToString();
            else if (e.Key >= Key.NumberPad0 && e.Key <= Key.NumberPad9)
                PreviewInputText = (e.Key - Key.NumberPad0).ToString();
            else
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
#if WPF
            if (!HasCurrentCellState || !IsInEditing)
                return true;
            var currentCellUiElement = (MaskedTextBox)CurrentCellRendererElement;
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        currentCellUiElement.ClearValue(MaskedTextBox.ValueProperty);
                        return true;
                    }
                case Key.F2:
                    {
                        if (!IsInEditing)
                            TreeGrid.Focus();
                        return true;
                    }

                case Key.A:
                    return SelectionHelper.CheckControlKeyPressed() && !IsInEditing;
                case Key.Left:
                    return (currentCellUiElement.SelectionStart == currentCellUiElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Right:
                    return (currentCellUiElement.CaretIndex >= currentCellUiElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Home:
                    return (currentCellUiElement.SelectionStart == currentCellUiElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.End:
                    return (currentCellUiElement.CaretIndex == currentCellUiElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());

            }
#else
            if (!HasCurrentCellState)
                return true;

            if (!IsInEditing)
            {
                ProcessPreviewTextInput(e);
                if (!(CurrentCellRendererElement is SfMaskedEdit))
                    return true;
            }

            var currentCellUiElement = (SfMaskedEdit)CurrentCellRendererElement;
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        currentCellUiElement.ClearValue(SfMaskedEdit.TextProperty);
                        return true;
                    }
            }
#endif
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        #endregion

        #region EndEdit

        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (MaskedTextBox)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(MaskedTextBox.ValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }
        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when value of MaskColumn is changed 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"> instance containing the event data </param>
        void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            base.CurrentRendererValueChanged();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the edit binding.
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        /// <param name="column">Grid Column</param>
        private void InitializeEditUIElement(MaskedTextBox uiElement, TreeGridColumn column)
        {
            var maskedColumn = ((TreeGridMaskColumn)column);
            var bind = maskedColumn.ValueBinding.CreateEditBinding(maskedColumn.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(MaskedTextBox.ValueProperty, bind);
#if UWP
            bind = new Binding { Path = new PropertyPath("Mask"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(SfMaskedEdit.MaskProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaskType"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(SfMaskedEdit.MaskTypeProperty, bind);
            bind = new Binding { Path = new PropertyPath("PromptChar"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(SfMaskedEdit.PromptCharProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaskFormat"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(SfMaskedEdit.ValueMaskFormatProperty, bind);
            bind = new Binding { Path = new PropertyPath("ValidationMode"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(SfMaskedEdit.ValidationModeProperty, bind);
            bind = new Binding { Path = new PropertyPath("KeyboardType"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(SfMaskedEdit.KeyboardTypeProperty, bind);            
            uiElement.Watermark = false;    
#endif

#if WPF          
            bind = new Binding { Path = new PropertyPath("IsNumeric"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.IsNumericProperty, bind);
            bind = new Binding { Path = new PropertyPath("DateSeparator"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.DateSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("DecimalSeparator"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.DecimalSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("TimeSeparator"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.TimeSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("PromptChar"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.PromptCharProperty, bind);
            bind = new Binding { Path = new PropertyPath("Mask"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.MaskProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaskFormat"), Mode = BindingMode.TwoWay, Source = maskedColumn };
            uiElement.SetBinding(MaskedTextBox.TextMaskFormatProperty, bind);
            uiElement.WatermarkTextIsVisible = false;
#endif

        }

     
        /// <summary>
        /// Processes the index of the caret.
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        private void ProcessCaretIndex(UIElement uiElement)
        {
            var editor = uiElement as MaskedTextBox;
            if (editor != null)
            {
                if (this.TreeGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll && string.IsNullOrEmpty(PreviewInputText))
                {
                    editor.SelectAll();
                }
                else
                {
                    if (string.Equals(PreviewInputText, string.Empty) || PreviewInputText == null)
                    {
                        var caretIndex = editor.Text.Length;
                        editor.Select(caretIndex, 0);
                    }
                    if (!string.IsNullOrEmpty(PreviewInputText))
                    {
#if WPF

                        editor.Value = MaskedEditorModel.GetMaskedText(editor.Mask, PreviewInputText, editor.DateSeparator, editor.TimeSeparator, editor.DecimalSeparator, editor.NumberGroupSeparator, ' ', editor.CurrencySymbol);
#else
                        //editor.Text = SfMaskedEditorModel.GetMaskedText(editor.Mask, editor.MaskType, PreviewInputText, editor.PromptChar, editor.ValueMaskFormat, editor.Culture);
#endif
                        var caretIndex = (editor.Text).IndexOf(PreviewInputText.ToString());
                        editor.Select(caretIndex + 1, 0);
                    }
                }
            }
            PreviewInputText = string.Empty;
        }


        #endregion
    }
}
