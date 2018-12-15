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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Utility;
using System.Windows.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    public class GridCellComboBoxRenderer:GridVirtualizingCellRenderer<ContentControl,ComboBox>
    {
        #region Ctor
        public Key? lastKeyPressed;
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellComboBoxRenderer"/> class.
        /// </summary>
        public GridCellComboBoxRenderer()
        {
            this.IsDropDownable = true;
        }
        #endregion

        #region Override Methods

        #region Display/Edit Binding Overrides

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
        protected override ComboBox OnCreateEditUIElement()
        {
            var comboBox = base.OnCreateEditUIElement();
#if WPF
            VisualContainer.SetWantsMouseInput(comboBox,true);
#endif
            return comboBox;
        }

#if WPF
        public override void InitializeDisplayElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            // Overridden to call the display since we have returned without initializing when the DataGrid.UseLightweightTemplates is true
            OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
        }

        protected override void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell,  object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads  CheckBox as usual in UseLightweightTemplate true case also.
        }    
#endif

        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn,ContentControl uiElement, object dataContext)
        {            
            var column = dataColumn.GridColumn;
            var gridcolumn = column as GridComboBoxColumn;

            uiElement.SetBinding(ContentControl.ContentProperty, column.DisplayBinding);            
            uiElement.SetValue(Control.HorizontalAlignmentProperty, TextAlignmentToHorizontalAlignment(gridcolumn.TextAlignment));        
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.VerticalAlignment);
            if (gridcolumn.ItemTemplate != null)
                uiElement.ContentTemplate = gridcolumn.ItemTemplate;
            uiElement.Margin = gridcolumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, ComboBox uiElement,object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            InitializeEditBinding(uiElement, column);
            var textAlignment = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.OneWay, Source = column, Converter = new TextAlignmentToHorizontalAlignmentConverter() };
            uiElement.SetBinding(Control.HorizontalContentAlignmentProperty, textAlignment);
            var verticalAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalAlignment);
#if UWP
            uiElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
#endif
            uiElement.SetValue(Control.PaddingProperty, column.padding);
        }
        #endregion

        #region Display/Edit Value 
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (HasCurrentCellState)
                return CurrentCellRendererElement.GetValue(IsInEditing ? Selector.SelectedValueProperty : ContentControl.ContentProperty);
            return base.GetControlValue();
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
                ((ComboBox) CurrentCellRendererElement).SelectedValue = value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }

        #endregion

        #region Wire/UnWire UIElements Overrides

        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var combobox = ((ComboBox)sender);
#if WPF
            combobox.Focus();            

            TextBox comboTextBox = (TextBox)GridUtil.FindDescendantChildByType(combobox, typeof(TextBox));

            if (combobox.IsEditable)
            {
                if (this.DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || this.DataGrid.IsAddNewIndex(this.CurrentCellIndex.RowIndex))
                {
                    comboTextBox.SelectAll();
                }
                else
                {
                    comboTextBox.Select(comboTextBox.SelectedText.Length, 0);
                }
            }
#endif
#if WinRT
            combobox.Focus(FocusState.Programmatic);
#endif
            combobox.SelectionChanged += SelectionChangedinComboBox;
#if WPF                        
            combobox.PreviewMouseDown += PreviewMouseDown;           
#endif
        }     

        protected override void OnUnwireEditUIElement(ComboBox uiElement)
        {
            uiElement.SelectionChanged -= SelectionChangedinComboBox;
#if WPF
            uiElement.PreviewMouseDown -= PreviewMouseDown;
#endif
        }

#if UWP
        internal override ComboBox CreateOrEditRecycleUIElement()
        {
            if (EditRecycleBin.Count > 1)
                return base.CreateOrEditRecycleUIElement();
            else
            {
                var uiElement = OnCreateEditUIElement();
                return uiElement;
            }
        }
#endif

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
#if WPF
            var combobox = (CurrentCellRendererElement as ComboBox);

            TextBox comboTextBox=(TextBox)GridUtil.FindDescendantChildByType(combobox, typeof(TextBox));
#endif

#if WPF
            if ((CheckAltKeyPressed() && e.SystemKey == Key.Down) || ((CheckAltKeyPressed() && e.SystemKey == Key.Up) || e.Key == Key.F4))
#else
            if ((CheckAltKeyPressed() && e.Key == Key.Down) || ((CheckAltKeyPressed() && e.Key == Key.Up) || e.Key == Key.F4))
#endif
            {
                var comboBox = ((ComboBox)CurrentCellRendererElement);
                comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
               
#if WinRT || UNIVERSAL
                comboBox.Focus(FocusState.Programmatic);
#endif
                return false;
            }

            switch (e.Key)
            {
                case Key.End:
                case Key.Home:
                case Key.Enter:                
                case Key.Escape:
                case Key.PageUp:
                case Key.PageDown:
                    return !((ComboBox)CurrentCellRendererElement).IsDropDownOpen;
                case Key.Down:
                case Key.Up:
#if WinRT
                    return !((ComboBox)CurrentCellRendererElement).IsDropDownOpen;
#else
                    // WPF-25803 - Up/Down Needs to be handle by UiElement itself to change the selection whether drop down open or not/ IsEditable set or not set.
                    return false;                                        
                case Key.Right:
                    return ((ComboBox)CurrentCellRendererElement).IsEditable?(comboTextBox.SelectionStart >= comboTextBox.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed()) : true;
                case Key.Left:
                    return ((ComboBox)CurrentCellRendererElement).IsEditable ? (comboTextBox.SelectionStart == comboTextBox.SelectionLength && !CheckControlKeyPressed() && !CheckShiftKeyPressed()) : true;                    
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        
        #endregion

        #region EndEdit

        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
        {
            if(canResetBinding)
            {
                var CurrentCellUIElement=(Selector)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(Selector.SelectedValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitializeEditBinding(ComboBox uiElement, GridColumn column)
        {
            var comboBoxColumn = (GridComboBoxColumn) column;
            var bind = column.ValueBinding.CreateEditBinding(comboBoxColumn.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(ComboBox.SelectedValueProperty, bind);
            
            var itemsSourceBinding = new Binding { Path = new PropertyPath("ItemsSource"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.ItemsSourceProperty, itemsSourceBinding);
#if UWP
            if (comboBoxColumn.ItemTemplate == null)
#endif
            {
                var displayMemberBinding = new Binding { Path = new PropertyPath("DisplayMemberPath"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
                uiElement.SetBinding(ComboBox.DisplayMemberPathProperty, displayMemberBinding);
            }
            var selectedValuePathBinding = new Binding { Path = new PropertyPath("SelectedValuePath"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.SelectedValuePathProperty, selectedValuePathBinding);               

#if WPF
            var staysOpenOnEditBinding = new Binding { Path = new PropertyPath("StaysOpenOnEdit"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.StaysOpenOnEditProperty, staysOpenOnEditBinding);
            var isEditableBinding = new Binding { Path = new PropertyPath("IsEditable"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.IsEditableProperty, isEditableBinding);                       
#endif
            var itemTemplateBinding = new Binding { Path = new PropertyPath("ItemTemplate"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.ItemTemplateProperty, itemTemplateBinding);

        }

#if WPF
        void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var combobox = sender as ComboBox;
            //WPF-31464 Need to check IsKeyboardFocusWithin 
            if (combobox.IsMouseCaptured && combobox.IsKeyboardFocusWithin)
            {
                if (!this.DataGrid.ValidationHelper.CheckForValidation(false))                
                {
                    //WPF-31651 - We have to release the focus of ComboBox when its closed.
                    if (!combobox.IsDropDownOpen)
                        combobox.ReleaseMouseCapture();
                }
            }
        }
#endif

        protected override void OnEnteredEditMode(DataColumnBase dataColumn,FrameworkElement currentRendererElement)
        {
#if WPF
            if ((currentRendererElement as ComboBox).StaysOpenOnEdit && (currentRendererElement as ComboBox).IsEditable)
                (currentRendererElement as ComboBox).IsDropDownOpen = true;
#endif
            base.OnEnteredEditMode(dataColumn, currentRendererElement);                       
        }
    

#if WinRT || UNIVERSAL
        private void SelectionChangedinComboBox(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
#else
        private void SelectionChangedinComboBox(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
#endif
        {
            var comboBox = (ComboBox) sender;
            DataGrid.RaiseCurrentCellDropDownSelectionChangedEvent(new CurrentCellDropDownSelectionChangedEventArgs(DataGrid)
            {
                SelectedIndex = comboBox.SelectedIndex, 
                SelectedItem = comboBox.SelectedItem, 
                RowColumnIndex = CurrentCellIndex
            });
        }
        #endregion
    }
}
