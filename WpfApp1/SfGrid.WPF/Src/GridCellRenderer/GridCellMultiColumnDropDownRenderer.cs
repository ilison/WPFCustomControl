#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
#if WinRT || UNIVERSAL
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Utility;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Utility;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    public class GridCellMultiColumnDropDownRenderer:GridVirtualizingCellRenderer<TextBlock, SfMultiColumnDropDownControl>
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellComboBoxRenderer"/> class.
        /// </summary>
        public GridCellMultiColumnDropDownRenderer()
        {
            this.IsDropDownable = true;
        }
        #endregion
        #region Override Methods
        /// <summary>
        /// Called when [create edit unique identifier element].
        /// </summary>
        /// <returns></returns>
        protected override SfMultiColumnDropDownControl OnCreateEditUIElement()
        {
            var sfMultiColumnDropDownControl = base.OnCreateEditUIElement();
#if WPF
            VisualContainer.SetWantsMouseInput(sfMultiColumnDropDownControl, true);
            //WPF-29023 - While using GridMultiColumnDropDownList column, no need to set the BorderThickness to SfMultiColumnDropDownControl.
            sfMultiColumnDropDownControl.BorderThickness = new Thickness(1);
#endif
            return sfMultiColumnDropDownControl;
        }             

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
        public override void OnInitializeEditElement(DataColumnBase dataColumn, SfMultiColumnDropDownControl uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            uiElement.ResizingThumbVisibility = (column as GridMultiColumnDropDownList).ShowResizeThumb;
            var Bind = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(SfMultiColumnDropDownControl.SelectedValueProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.TextAlignmentProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("DisplayMember"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.DisplayMemberProperty, Bind);                         
            Bind = new Binding { Path = new PropertyPath("ValueMember"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.ValueMemberProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("ItemsSource"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.ItemsSourceProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("PopUpWidth"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.PopupWidthProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("PopUpHeight"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.PopupHeightProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AllowAutoComplete"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AllowAutoCompleteProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AllowSpinOnMouseWheel"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AllowSpinOnMouseWheelProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AllowIncrementalFiltering"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AllowIncrementalFilteringProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("SearchCondition"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.SearchConditionProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AllowCasingforFilter"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AllowCaseSensitiveFilteringProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("PopUpMinHeight"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.PopupMinHeightProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("PopUpMinWidth"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.PopupMinWidthProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("PopUpMaxHeight"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.PopupMaxHeightProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("PopUpMaxWidth"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.PopupMaxWidthProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("IsTextReadOnly"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.ReadOnlyProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AllowNullInput"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AllowNullInputProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AutoGenerateColumns"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AutoGenerateColumnsProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("Columns"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.ColumnsProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("GridColumnSizer"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.GridColumnSizerProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("IsAutoPopupSize"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.IsAutoPopupSizeProperty, Bind);
            Bind = new Binding { Path = new PropertyPath("AutoGenerateColumnsMode"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(SfMultiColumnDropDownControl.AutoGenerateColumnsModeProperty, Bind);
            uiElement.SetValue(SfMultiColumnDropDownControl.PaddingProperty, column.Padding);            
            BindingExpression = uiElement.GetBindingExpression(SfMultiColumnDropDownControl.SelectedValueProperty);
        }
        #endregion

        #region Display/ Edit Value Overrides
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (!HasCurrentCellState) return base.GetControlValue();            
            return CurrentCellRendererElement.GetValue(IsInEditing ? SfMultiColumnDropDownControl.SelectedValueProperty : TextBlock.TextProperty);
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
                ((SfMultiColumnDropDownControl)CurrentCellRendererElement).SelectedItem = value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
        #endregion

        #region Wire/UnWire UIElements Overrides
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = ((SfMultiColumnDropDownControl)sender);
            uiElement.SelectionChanged += uiElement_SelectionChanged;
#if WinRT || UNIVERSAL
            if (uiElement.Editor != null)
                uiElement.Editor.Focus(FocusState.Programmatic);
            else
                uiElement.ContentControl.Focus(FocusState.Programmatic);
#else
            uiElement.Editor.Focus();  
#endif
            if (uiElement.Editor != null)
            {
                if ((DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || DataGrid.IsAddNewIndex(CurrentCellIndex.RowIndex)) && PreviewInputText == null)
                {
                    uiElement.Editor.SelectAll();
                }
                else
                {
                    var index = uiElement.Editor.Text.Length;
                    uiElement.Editor.Select(index + 1, 0);
                }
            }
            PreviewInputText = null;            
#if WPF
            ((SfMultiColumnDropDownControl)sender).PreviewMouseDown += PreviewMouseDown;     
#endif            
        }

        internal override Control GetFocusedUIElement(FrameworkElement uiElement)
        {
#if WPF
            if(uiElement is SfMultiColumnDropDownControl)
                return (uiElement as SfMultiColumnDropDownControl).Editor;
#endif
            return base.GetFocusedUIElement(uiElement);
        }
        
        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        protected override void OnUnwireEditUIElement(SfMultiColumnDropDownControl uiElement)
        {
            uiElement.SelectionChanged -= uiElement_SelectionChanged;
#if WPF
            uiElement.PreviewMouseDown -= PreviewMouseDown;          
#endif
        }
        #endregion

        #endregion
#if WPF
        void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var sfMultiColumnDropDownControl = sender as SfMultiColumnDropDownControl;
            if (sfMultiColumnDropDownControl.IsMouseCaptured && sfMultiColumnDropDownControl.IsKeyboardFocusWithin)
            {
                if (!this.DataGrid.ValidationHelper.CheckForValidation(false))                
                    sfMultiColumnDropDownControl.ReleaseMouseCapture();                    
            }
        }
#endif
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {

            if (!HasCurrentCellState || !IsInEditing)
                return true;

#if WinRT
            UIElement CurrentCellUIElement;

            if (CurrentCellRendererElement is SfMultiColumnDropDownControl)
                CurrentCellUIElement = ((SfMultiColumnDropDownControl)CurrentCellRendererElement).Editor;
            else
                CurrentCellUIElement = (TextBlock)CurrentCellRendererElement;
#else
            var CurrentCellUIElement = ((SfMultiColumnDropDownControl)CurrentCellRendererElement).Editor;
#endif

            switch (e.Key)
            {
                case Key.Escape:
                    {
                        //While pressing escape key, based on IsDropDownOpen, need to clear the editor text.
                        var control = CurrentCellRendererElement as SfMultiColumnDropDownControl;
                        if (control.IsDropDownOpen)
                            return false;

                        CurrentCellUIElement.ClearValue(TextBox.TextProperty);
                        return true;
                    }
#if WPF
                case Key.Left:
                    // WPF-25803 - Need to handle by Grid to move the current cell to previous cell.
                    if((CurrentCellRendererElement as SfMultiColumnDropDownControl).AllowAutoComplete)
                        return CurrentCellUIElement.CaretIndex == 0  && !CheckControlKeyPressed() && !CheckShiftKeyPressed();
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Right:
                    return (CurrentCellUIElement.CaretIndex >= CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.CaretIndex == CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Up:
                case Key.Down:
                    return false;
                case Key.PageUp:
                case Key.PageDown:
                case Key.Enter:
                    {
                        var multiDropDown = CurrentCellRendererElement as SfMultiColumnDropDownControl;
                        if (multiDropDown != null)
                            return !(multiDropDown).IsDropDownOpen;
                        return true;
                    }
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

        #region EndEdit

        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (SfMultiColumnDropDownControl)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(SfMultiColumnDropDownControl.SelectedValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }

        #endregion
        protected override void OnEnteredEditMode(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
#if WPF
            VisualContainer.SetWantsMouseInput(currentRendererElement, false);
#endif
            base.OnEnteredEditMode(dataColumn, currentRendererElement);
        }

        #region Private Methods
        /// <summary>
        /// Handles the SelectionChanged event of the uiElement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void uiElement_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            DataGrid.RaiseCurrentCellDropDownSelectionChangedEvent(
                new CurrentCellDropDownSelectionChangedEventArgs(DataGrid)
                {
                    RowColumnIndex = CurrentCellIndex, 
                    SelectedIndex = args.SelectedIndex, 
                    SelectedItem = args.SelectedItem
                });
        }


        #endregion
    }
}
