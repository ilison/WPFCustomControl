#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Syncfusion.UI.Xaml.Controls.Input;
#else
using System.Windows.Controls;
using System.Windows.Data;
using Syncfusion.Windows.Shared;

#endif
namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using DoubleTextBox = SfNumericTextBox;    
#endif

    public class TreeGridCellNumericRenderer : TreeGridVirtualizingCellRenderer<TextBlock, DoubleTextBox>
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
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, DoubleTextBox uiElement, object dataContext)
        {
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            InitializeEditUIElement(uiElement, dataColumn.TreeGridColumn);
#if UWP
            var wrapBind = new Binding { Path = new PropertyPath("TextWrapping"), Source = dataColumn.TreeGridColumn };
            uiElement.SetBinding(TextBox.TextWrappingProperty, wrapBind);
            uiElement.ValueChangedMode = ValueChange.OnKeyFocus;
#else
            uiElement.TextSelectionOnFocus = false;
#endif
            BindingExpression = uiElement.GetBindingExpression(DoubleTextBox.ValueProperty);
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
                ((DoubleTextBox)CurrentCellRendererElement).Value = (double)value;
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
        /// Invoked when the edit element(DoubleTextBox) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = ((DoubleTextBox)sender);
            uiElement.ValueChanged += OnValueChanged;
#if UWP
            uiElement.Focus(FocusState.Programmatic);
#else
            uiElement.Focus();
#endif
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
                double value;
                double.TryParse(PreviewInputText.ToString(), out value);
                uiElement.Value = value;
#if WPF
                var caretIndex = uiElement.Text.IndexOf(PreviewInputText.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
#else
                var caretIndex = uiElement.Text.IndexOf(PreviewInputText, StringComparison.Ordinal);
#endif
                uiElement.Select(caretIndex + 1, 0);
            }
            PreviewInputText = null;
        }

        /// <summary>
        /// Invoked when the edit element(DoubleTextBox) is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            ((DoubleTextBox)sender).ValueChanged -= OnValueChanged;          
        }

        /// <summary>
        /// Called when [unwire edit UI element].
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        /// 

        protected override void OnUnwireEditUIElement(DoubleTextBox uiElement)
        {
            uiElement.ValueChanged -= OnValueChanged;
        }

        #endregion

#if UWP
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

            if (!HasCurrentCellState)
                return true;

            if (!IsInEditing)
            {
#if UWP
                ProcessPreviewTextInput(e);
                if (!(CurrentCellRendererElement is SfNumericTextBox))
                    return true;
#else
                return true;
#endif
            }

            var CurrentCellUIElement = (DoubleTextBox)CurrentCellRendererElement;

            switch (e.Key)
            {
                case Key.Escape:
                    {
                        CurrentCellUIElement.ClearValue(DoubleTextBox.ValueProperty);
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
#if WPF
                //WPF-18339- When navigating by Up or Down key, should return here if IsScrollingOnCircle is true for that UIElement
                case Key.Up:
                case Key.Down:
                    return !CurrentCellUIElement.IsScrollingOnCircle;
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
                var CurrentCellUIElement = (DoubleTextBox)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(DoubleTextBox.ValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
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

#if UWP
        private void OnValueChanged(object sender, ValueChangedEventArgs e)
#else
        void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#endif
        {
            base.CurrentRendererValueChanged();
        }

        #endregion

        #region Private Methods      

        /// <summary>
        /// Processes the edit binding.
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        /// <param name="column">TreeGridColumn of the Editing Column</param>
        private void InitializeEditUIElement(DoubleTextBox uiElement, TreeGridColumn column)
        {
            var numericColumn = ((TreeGridNumericColumn)column);
            var bind = numericColumn.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(DoubleTextBox.ValueProperty, bind);
#if UWP
            bind = new Binding { Path = new PropertyPath("AllowNull"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.AllowNullProperty, bind);
            bind = new Binding { Path = new PropertyPath("BlockCharactersOnTextInput"), Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.BlockCharactersOnTextInputProperty, bind);
            bind = new Binding { Path = new PropertyPath("FormatString"), Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.FormatStringProperty, bind);            
#else
            bind = new Binding { Path = new PropertyPath("AllowScrollingOnCircle"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(EditorBase.IsScrollingOnCircleProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinValue"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.MinValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxValue"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.MaxValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("NumberDecimalDigits"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.NumberDecimalDigitsProperty, bind);
            bind = new Binding { Path = new PropertyPath("NumberDecimalSeparator"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.NumberDecimalSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("NumberGroupSeparator"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.NumberGroupSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("NumberGroupSizes"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.NumberGroupSizesProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowNullValue"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(EditorBase.UseNullOptionProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxValidation"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(EditorBase.MaxValidationProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinValidation"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(EditorBase.MinValidationProperty, bind);
            bind = new Binding { Path = new PropertyPath("NullValue"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.NullValueProperty, bind);
            uiElement.WatermarkTextIsVisible = true;
            bind = new Binding { Path = new PropertyPath("NullText"), Mode = BindingMode.TwoWay, Source = numericColumn };
            uiElement.SetBinding(DoubleTextBox.WatermarkTextProperty, bind);
#endif
        }
    }
        #endregion
}

