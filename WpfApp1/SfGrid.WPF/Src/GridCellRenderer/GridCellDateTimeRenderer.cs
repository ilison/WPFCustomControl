#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Utility;
#if WinRT || UNIVERSAL
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Syncfusion.Windows.Shared;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using EventArgs = PointerRoutedEventArgs;
    using DateTimeEdit = SfDatePicker;
    using Xaml.Utility;
#endif
    [ClassReference(IsReviewed = false)]
    public class GridCellDateTimeRenderer:GridVirtualizingCellRenderer<TextBlock, DateTimeEdit>
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellDateTimeRenderer"/> class.
        /// </summary>
        public GridCellDateTimeRenderer()
        {
#if WinRT || UNIVERSAL
            IsFocusible = false;
#endif
        }
        #endregion 

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
            uiElement.Padding = dataColumn.GridColumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, DateTimeEdit uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;            
            InitializeEditUIElement(uiElement, column);
            BindingExpression = uiElement.GetBindingExpression(DateTimeEdit.DateTimeProperty);
#if !WinRT && !UNIVERSAL
            base.OnInitializeEditElement(dataColumn, uiElement,dataContext);
#else
            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
            uiElement.HorizontalAlignment = HorizontalAlignment.Stretch;
#endif
            uiElement.HorizontalContentAlignment = TextAlignmentToHorizontalAlignment(column.TextAlignment);
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
#if WinRT || UNIVERSAL
            return this.CurrentCellRendererElement.GetValue(IsInEditing ? SfDatePicker.ValueProperty : TextBlock.TextProperty);
#else
            return CurrentCellRendererElement.GetValue(IsInEditing ? DateTimeEdit.DateTimeProperty : TextBlock.TextProperty);
#endif
        }

#if !WinRT && !UNIVERSAL
        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
                ((DateTimeEdit) CurrentCellRendererElement).DateTime = (DateTime?) value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
#endif
        #endregion

        #region Wire/UnWire UIElement

        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="sender"></param>
#if WinRT || UNIVERSAL
        /// <summary>
        /// Invoked when the edit element(SfDatePicker) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var sfDatePicker = ((SfDatePicker)sender);
            sfDatePicker.ValueChanged += OnValueChanged;            
            // Event hooked set the Null value to SfDatePicker.Value when AllowInlineEditing is enabled and delete the value.                               
            SfTextBoxExt sfTextBoxExt = (SfTextBoxExt)GridUtil.FindDescendantChildByName(sfDatePicker, "PART_TextBoxExt");
            if (sfTextBoxExt == null || !sfDatePicker.AllowNull)
                return;
            sfTextBoxExt.TextChanged += sfTextBoxExt_TextChanged;
        }

        /// <summary>
        /// Invoked when the edit element(SfDatePicker) is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            var sfDatePicker = ((SfDatePicker)sender);
            sfDatePicker.ValueChanged -= OnValueChanged;
            // Event hooked set the Null value to SfDatePicker.Value when AllowInlineEditing is enabled and delete the value.                               
            SfTextBoxExt sfTextBoxExt = (SfTextBoxExt)GridUtil.FindDescendantChildByName(sfDatePicker, "PART_TextBoxExt");
            if (sfTextBoxExt == null || !sfDatePicker.AllowNull)
                return;
            sfTextBoxExt.TextChanged -= sfTextBoxExt_TextChanged;
        }

        private void sfTextBoxExt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var sfTextBoxExt = sender as SfTextBoxExt;
            if (string.IsNullOrEmpty(sfTextBoxExt.Text))
            {
                // Set null to value of SfDatePicker explicitly 
                CurrentCellRendererElement.SetValue(SfDatePicker.ValueProperty, null);
            }
        }

        protected override void OnUnwireEditUIElement(SfDatePicker uiElement)
        {
            uiElement.ValueChanged -= OnValueChanged;
            SfTextBoxExt sfTextBoxExt = (SfTextBoxExt)GridUtil.FindDescendantChildByName(uiElement, "PART_TextBoxExt");
            if (sfTextBoxExt == null || !uiElement.AllowNull)
                return;
            sfTextBoxExt.TextChanged -= sfTextBoxExt_TextChanged;
        }
#else
        /// <summary>
        ///Invoked when the edit element(DateTimeEdit) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var dateTimeEdit = ((DateTimeEdit)sender);
            dateTimeEdit.TextChanged += OnTextChanged;
            dateTimeEdit.Focus();
            if ((this.DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || this.DataGrid.IsAddNewIndex(this.CurrentCellIndex.RowIndex)) && PreviewInputText == null)
            {
                dateTimeEdit.SelectAll();
                return;
            }
            else
            {
                if (PreviewInputText == null)
                {
                    var index = dateTimeEdit.Text.Length;
                    dateTimeEdit.Select(index + 1, 0);
                    return;
                }
                if (dateTimeEdit.CanEdit)
                    ((DateTimeEdit)CurrentCellRendererElement).SelectedText = PreviewInputText.ToString();
                    TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, (DateTimeEdit)CurrentCellRendererElement, PreviewInputText.ToString()));
            }
            PreviewInputText = null;
        }

        /// <summary>
        /// Invoked when the edit element(DateTimeEdit) is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            ((DateTimeEdit)sender).TextChanged -= OnTextChanged;        
        }
        protected override void OnUnwireEditUIElement(DateTimeEdit uiElement)
        {
            uiElement.TextChanged -= OnTextChanged;
        }
#endif


        #endregion

        #region ShouldHandleKeyDown
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
                if (!(CurrentCellRendererElement is SfDatePicker))
                    return true;
            }
#else
            if (!HasCurrentCellState || !IsInEditing)
                return true;
#endif
            var CurrentCellUIElement = (DateTimeEdit)CurrentCellRendererElement;
            switch (e.Key)
            {
#if !WinRT && !UNIVERSAL
                case Key.F4:
                {
                    if (CurrentCellUIElement.IsDropDownOpen)
                        CurrentCellUIElement.ClosePopup();
                    else
                        CurrentCellUIElement.OpenPopup();
                    return true;
                }
#endif
                case Key.Escape:
                {
#if !WinRT && !UNIVERSAL
                    if (CurrentCellUIElement != null)
                    {
                        // WPF-31458 - we need to reset the old value when Esc key is pressed due to selected date is committed to underlying source instantly.
                        if (this.DataGrid.SelectionController.CurrentCellManager.oldCellValue is DateTime? && !CurrentCellUIElement.IsDropDownOpen)
                            CurrentCellUIElement.DateTime = Convert.ToDateTime(this.DataGrid.SelectionController.CurrentCellManager.oldCellValue);

                        CurrentCellUIElement.ClearValue(DateTimeEdit.DateTimeProperty);
                    }
#endif
                    //TODO: Asked Tools Team Checking DropDown is Open or Not
                    //If DropDown is Open, we need to close the DropDown and should not End Edit (return false)
                    //else return true to EndEdit
                    //Currently we are returning true to end edit the DateTime Cell to stop the Exception that arises at UpdateSource of BindingExpression
                    return true;
                }
                case Key.Up:
                case Key.Down:
                    return !IsInEditing;
#if WPF
                case Key.Left:
                    return (CurrentCellUIElement.CaretIndex <= 0 && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Right:
                    return ((CurrentCellUIElement.SelectionLength + CurrentCellUIElement.SelectionStart) >= CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == 0 && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.CaretIndex == CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
#else
                case Key.Left:
                case Key.Right:
                    return true;
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        #endregion

        #region EndEdit

        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (DateTimeEdit)CurrentCellRendererElement;
#if WinRT || UNIVERSAL 
                 CurrentCellUIElement.ClearValue(DateTimeEdit.ValueProperty);
#else
                 CurrentCellUIElement.ClearValue(DateTimeEdit.DateTimeProperty);
#endif
            }
            return base.EndEdit(dc, record, canResetBinding);
        }
        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="d">sIts the corresponding Editor here </param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
#if WinRT || UNIVERSAL
        private void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#else
        private void OnTextChanged(object sender, TextChangedEventArgs e)
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
        /// <param name="column"></param>
        private void InitializeEditUIElement(DateTimeEdit uiElement, GridColumn column)
        {
            var dateTimeColumn = (GridDateTimeColumn)column;
            var bind = dateTimeColumn.ValueBinding.CreateEditBinding(dateTimeColumn.GridValidationMode != GridValidationMode.None, column);
#if WinRT || UNIVERSAL
            //WRT-4266 Need to create the binding by using CreateEditBinding method available in BindingUtility helper
            //for Setting the Mode as Two Way for AddNewRowColumn if AllowEditing is False
            uiElement.SetBinding(DateTimeEdit.ValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("FormatString"), Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.FormatStringProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowInlineEditing"), Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.AllowInlineEditingProperty, bind);
            bind = new Binding { Path = new PropertyPath("ShowDropDownButton"), Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.ShowDropDownButtonProperty, bind);            
            bind = new Binding { Path = new PropertyPath("AllowNullValue"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.AllowNullProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinDate"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.MinDateProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxDate"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.MaxDateProperty, bind);
            bind = new Binding { Path = new PropertyPath("WaterMark"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.WatermarkProperty, bind);
            bind = new Binding { Path = new PropertyPath("AccentBrush"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.AccentBrushProperty, bind);
            bind = new Binding { Path = new PropertyPath("DropDownHeight"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.DropDownHeightProperty, bind);  
            bind = new Binding { Path = new PropertyPath("InputScope"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.InputScopeProperty, bind);
            bind = new Binding { Path = new PropertyPath("SelectorItemCount"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.SelectorItemCountProperty, bind);
            bind = new Binding { Path = new PropertyPath("SelectorItemHeight"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.SelectorItemHeightProperty, bind);
            bind = new Binding { Path = new PropertyPath("SelectorItemSpacing"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.SelectorItemSpacingProperty, bind);
            bind = new Binding { Path = new PropertyPath("SelectorItemWidth"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.SelectorItemWidthProperty, bind);
            bind = new Binding { Path = new PropertyPath("SelectorFormatString"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.SelectorFormatStringProperty, bind);
            uiElement.IsDropDownOpen = dateTimeColumn.IsDropDownOpen;
#else
            uiElement.SetBinding(DateTimeEdit.DateTimeProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinDateTime"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.MinDateTimeProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxDateTime"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.MaxDateTimeProperty, bind);
            bind = new Binding { Path = new PropertyPath("CustomPattern"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeBase.CustomPatternProperty, bind);
            bind = new Binding { Path = new PropertyPath("CanEdit"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeBase.CanEditProperty, bind);
            bind = new Binding { Path = new PropertyPath("EnableBackspaceKey"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.EnableBackspaceKeyProperty, bind);
            bind = new Binding { Path = new PropertyPath("EnableDeleteKey"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.EnableDeleteKeyProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowScrollingOnCircle"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeBase.EnableMouseWheelEditProperty, bind);
            bind = new Binding { Path = new PropertyPath("DateTimeFormat"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeBase.DateTimeFormatProperty, bind);
            bind = new Binding { Path = new PropertyPath("Pattern"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeBase.PatternProperty, bind);
            bind = new Binding { Path = new PropertyPath("EnableClassicStyle"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.EnableClassicStyleProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowNullValue"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.IsEmptyDateEnabledProperty, bind);
            bind = new Binding { Path = new PropertyPath("ShowRepeatButton"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeBase.IsVisibleRepeatButtonProperty, bind);
            bind = new Binding { Path = new PropertyPath("NullValue"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.NullValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("NullText"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.NoneDateTextProperty, bind);  
            bind = new Binding { Path = new PropertyPath("DisableDateSelection"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(DateTimeEdit.DisableDateSelectionProperty, bind);
            bind = new Binding { Path = new PropertyPath("TextDecorations"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
            uiElement.SetBinding(TimeSpanEdit.TextDecorationsProperty, bind);

            if ((column as GridDateTimeColumn).MaxDateTime != System.DateTime.MaxValue)
            {
                bind = new Binding { Path = new PropertyPath("MaxDateTime"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.MaxDateTimeProperty, bind);
            }
            else
            {
                bind = new Binding { Path = new PropertyPath("NullValue"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.NullValueProperty, bind);
            }        
#endif
        }

        #endregion
    }
}
