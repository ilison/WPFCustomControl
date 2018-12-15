#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Globalization;
using System.Windows;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using Windows.UI.Xaml;
    using Windows.Foundation;

#endif
    public class GridUnBoundCellTemplateRenderer : GridVirtualizingCellRenderer<ContentControl, ContentControl>
    {        
        #region Arrange UIElement

        /// <summary>
        /// Set focus to UIElement loaded in DataTemplate of GridCell on loading. since OnEditElementloaded will not fire for GridCell again when start editing.
        /// </summary>
        /// <param name="cellRowColumnIndex"></param>
        /// <param name="uiElement"></param>
        /// <param name="cellRect"></param>
        protected override void OnArrange(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Rect cellRect)
        {
            base.OnArrange(cellRowColumnIndex, uiElement, cellRect);
            if (CanFocus && HasCurrentCellState && IsInEditing && FocusManagerHelper.GetFocusedUIElement(CurrentCellRendererElement) != null)
            {
                SetFocus(true);
                CanFocus = false;
            }
        }
        #endregion

        #region Display/Edit Binding Overrides

        /// <summary>
        /// Method overridden to avoid binding for a content cntrol when cell template is not defined.
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <param name="uiElement"></param>
        /// <param name="dataContext"></param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
           
        }

        public override void OnInitializeTemplateElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            OnUpdateTemplateBinding(dataColumn,uiElement,dataContext);
            if (column.hasCellTemplate)
                uiElement.ContentTemplate = column.CellTemplate;
            else if (column.hasCellTemplateSelector)
                uiElement.ContentTemplateSelector = column.CellTemplateSelector;
            else if (column.DataGrid != null && column.DataGrid.hasCellTemplateSelector)
                uiElement.ContentTemplateSelector = column.DataGrid.CellTemplateSelector;
        }

        public override void OnUpdateTemplateBinding(DataColumnBase dataColumn, ContentControl uiElement,object dataContext)
        {
            var column = dataColumn.GridColumn;

            var contentValue = DataGrid.GetUnBoundCellValue(column, dataContext);

            var dataContextHelper = new DataContextHelper
            {
                Record = dataContext,
                Value = contentValue
            };
            uiElement.Content = dataContextHelper;       
        }
        
        /// <summary>
        /// Method which initialize the element with respect corresponding UnBound value
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">DataContext of the row</param>
        /// <remarks></remarks>        
        public override void OnInitializeEditElement(DataColumnBase dataColumn, ContentControl uiElement,object dataContext)
        {
            var column = dataColumn.GridColumn as GridUnBoundColumn;
            object contentValue = null;
            if (dataContext != null)
                contentValue = DataGrid.GetUnBoundCellValue(column, dataContext);
            if (column.hasEditTemplate)
                uiElement.ContentTemplate = column.EditTemplate;
            if (column.hasEditTemplateSelector)
                uiElement.ContentTemplateSelector = column.EditTemplateSelector;
            var dataContextHelper = new DataContextHelper
            {
                Record = dataContext,
                Value = contentValue
            };
            uiElement.Content = dataContextHelper;
            CanFocus = true;
        }

        protected override void OnEditingComplete(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            if (HasCurrentCellState && IsInEditing)
            {                
                    var dataContextHelper = (CurrentCellRendererElement as ContentControl).Content as DataContextHelper;
                    if (dataContextHelper != null)
                        DataGrid.RaiseQueryUnboundValue(new GridUnboundColumnEventsArgs(UnBoundActions.CommitData,dataContextHelper.Value,dataColumn.GridColumn,dataContextHelper.Record,this.DataGrid));             
            }
            base.OnEditingComplete(dataColumn, currentRendererElement);

            RaiseCurrentCellValueChangedEvent(DataGrid,dataColumn);
        }
        #endregion      

        #region Update/Ensure Editing Overrides
        public override bool CanValidate()
        {
            return false;
        }
        #endregion

        #region ShouldGridTryToHandleKeyDown
        /// <summary>
        /// Shoulds the grid try automatic handle key down.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            if (!HasCurrentCellState)
                return true;
            var columnIndex = DataGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex);            
            var column = (GridTemplateColumn)DataGrid.Columns[columnIndex];    
          
            var handleTemplatedUIElementKeyDown = FocusManagerHelper.GetWantsKeyInput(column);

            if ((column.hasEditTemplate || column.hasEditTemplateSelector))
                handleTemplatedUIElementKeyDown = false;

            switch (e.Key)
            {
                case Key.F2:
                case Key.Escape:
                {
                    e.Handled = true;
                    return true;
                }
                case Key.Tab:
                    if (!IsInEditing && !(DataGrid.GetLastDataRowIndex() == CurrentCellIndex.RowIndex && DataGrid.SelectionController.CurrentCellManager.GetLastCellIndex() == CurrentCellIndex.ColumnIndex))
                        base.SetFocus(CurrentCellRendererElement, false);
                    return true;
                case Key.Enter:
                case Key.PageUp:
                case Key.PageDown:
                    if (!IsInEditing)
                        base.SetFocus(CurrentCellRendererElement, true);
                    return true;
                case Key.Home:
                case Key.End:
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                    {
                        if (!handleTemplatedUIElementKeyDown && !IsInEditing)
                        {
                            base.SetFocus(CurrentCellRendererElement, false);
                            return true;
                        }
                        else if (handleTemplatedUIElementKeyDown)
                        {
                            return false;
                        }
                        else
                            return !IsInEditing;
                    }
            }                                            
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        
        #endregion

        public override bool CanUpdateBinding(GridColumn column)
        {
            return true;
        }        
    }
    
    /// <summary>
    /// Claas that deals with UnBound column with TexColumn Behavior.
    /// </summary>
    public class GridUnBoundCellTextBoxRenderer : GridVirtualizingCellRenderer<TextBlock, TextBox>
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
            GridColumn column = dataColumn.GridColumn;
            var cellvalue = DataGrid.GetUnBoundCellValue(column, dataContext);

            if (cellvalue == null)
                uiElement.Text = string.Empty;
            else
            {
                var type = cellvalue.GetType();
                var unboundColumn = column as GridUnBoundColumn;

                //If both format, expression used in unbound column, then only format should be applied.
                if (!string.IsNullOrEmpty(unboundColumn.Format) && !string.IsNullOrEmpty(unboundColumn.Expression))
                    uiElement.Text = String.Format((column as GridUnBoundColumn).Format, cellvalue);
                else
                    uiElement.Text = cellvalue.ToString();
            }
            uiElement.VerticalAlignment = column.verticalAlignment;
            uiElement.Padding = dataColumn.GridColumn.padding;
            uiElement.SetValue(TextBlock.TextAlignmentProperty, column.textAlignment);
            uiElement.SetValue(TextBlock.TextTrimmingProperty, column.textTrimming);
            uiElement.SetValue(TextBlock.TextWrappingProperty, column.textWrapping);           
#if WPF            
            uiElement.SetValue(TextBlock.TextDecorationsProperty, column.textDecoration);
#endif                                    
        }

        public override void OnInitializeTemplateElement(DataColumnBase dataColumn, ContentControl uiElement,object dataContext)
        {            
            var column = dataColumn.GridColumn;
            OnUpdateTemplateBinding(dataColumn,uiElement, dataContext);
            if (column.hasCellTemplate)
                uiElement.ContentTemplate = column.CellTemplate;
            else if (column.hasCellTemplateSelector)
                uiElement.ContentTemplateSelector = column.CellTemplateSelector;
            else if (column.DataGrid != null && column.DataGrid.CellTemplateSelector != null)
                uiElement.ContentTemplateSelector = column.DataGrid.CellTemplateSelector;
        }

        public override void OnUpdateTemplateBinding(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            var contentValue = DataGrid.GetUnBoundCellValue(column, dataContext);
            var dataContextHelper = new DataContextHelper
            {
                Record = dataContext,
                Value = contentValue
            };
            uiElement.Content = dataContextHelper;            
        }

        public override void OnInitializeEditElement(DataColumnBase dataColumn, TextBox uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            object contentValue = null;

            if (dataContext != null)
                contentValue = DataGrid.GetUnBoundCellValue(column, dataContext);
                
            uiElement.Text = contentValue == null ? null : contentValue.ToString();

            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            var textAlignBind = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextAlignmentProperty, textAlignBind);
            var textWrappingBinding = new Binding { Path = new PropertyPath("TextWrapping"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextWrappingProperty, textWrappingBinding);
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
            var verticalContentAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalContentAlignment);          
         }

        /// <summary>
        /// When complete edit, we need to raise query again to provide entered value to customer.
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        ///  GridColumn - Column which is providing the information for Binding
        /// <param name="currentRendererElement">The UIElement that resides in GridUnBoundCell</param>
        protected override void OnEditingComplete(DataColumnBase dataColumn,FrameworkElement currentRendererElement)
        {
            if (HasCurrentCellState && IsInEditing)
            {                                                               
                    var txtbox = CurrentCellRendererElement as TextBox;
                    var record = CurrentCellRendererElement.DataContext;                    
                    if (record != null)
                        DataGrid.RaiseQueryUnboundValue(UnBoundActions.CommitData, txtbox.Text, DataGrid.Columns[DataGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex)], record);                
            }
            base.OnEditingComplete(dataColumn,currentRendererElement);
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
                ((TextBox) CurrentCellRendererElement).Text = (string) value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");            
        }
        #endregion        

        #region Wire evnets that needs to be done.

        /// <summary>
        /// Invoked when the edit element(TextBox) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (TextBox)sender;
            uiElement.TextChanged += OnTextChanged;
#if WinRT
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

        #region Public override methods
        public override bool CanUpdateBinding(GridColumn column)
        {
            return true;
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

    /// <summary>
    /// Class taht deals with UnboundRow template cell
    /// </summary>
    public class GridUnBoundRowCellTemplateRenderer : GridUnBoundRowCellRenderer<ContentControl, ContentControl>
    {        
        #region Arrange UIElement

        /// <summary>
        /// Set focus to UIElement loaded in DataTemplate of GridCell on loading. since OnEditElementloaded will not fire for GridCell again when start editing.
        /// </summary>
        /// <param name="cellRowColumnIndex"></param>
        /// <param name="uiElement"></param>
        /// <param name="cellRect"></param>
        protected override void OnArrange(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Rect cellRect)
        {
            base.OnArrange(cellRowColumnIndex, uiElement, cellRect);
            if (CanFocus && HasCurrentCellState && IsInEditing && FocusManagerHelper.GetFocusedUIElement(CurrentCellRendererElement) != null)
            {
                SetFocus(true);
                CanFocus = false;
            }
        }
        #endregion

        #region Display/Edit Binding Overrides

        /// <summary>
        /// Method overridden to avoid binding for a content cntrol when cell template is not defined in GridUnBoundRowEventArgs.
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <param name="uiElement"></param>
        /// <param name="dataContext"></param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            
        }

        /// <summary>
        /// Method which initialize the element with respect corresponding UnBound value
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// <param name="dataContext"></param>
        /// GridColumn - Column which is providing the information for Binding
        /// <remarks></remarks>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {            
            object contentValue = null;
            contentValue = (dataColumn.GridUnBoundRowEventsArgs != null && dataColumn.GridUnBoundRowEventsArgs.Value != null) ?
                            dataColumn.GridUnBoundRowEventsArgs.Value.ToString() :
                            string.Empty;

            CanFocus = true;
            uiElement.ContentTemplate = dataColumn.GridUnBoundRowEventsArgs != null && 
                                        dataColumn.GridUnBoundRowEventsArgs.hasEditTemplate ?
                                        dataColumn.GridUnBoundRowEventsArgs.EditTemplate : null;

            var dataContextHelper = new DataContextHelper
            {
                Record = dataColumn.GridUnBoundRowEventsArgs,
                Value = contentValue
            };
            uiElement.Content = dataContextHelper;            
        }

        protected override void OnEditingComplete(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            if (HasCurrentCellState && IsInEditing)
            {
                var dataContextHelper = (CurrentCellRendererElement as ContentControl).Content as DataContextHelper;
                if (dataContextHelper != null)
                {
                    var args = new GridUnBoundRowEventsArgs(dataColumn.GridUnBoundRowEventsArgs.GridUnboundRow, UnBoundActions.CommitData, dataContextHelper.Value, dataColumn.GridColumn, dataColumn.GridUnBoundRowEventsArgs.CellType, this, new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex))
                    {
                        CellTemplate = dataColumn.GridUnBoundRowEventsArgs.CellTemplate,
                        EditTemplate = dataColumn.GridUnBoundRowEventsArgs.EditTemplate
                    };
                    dataColumn.GridUnBoundRowEventsArgs.Value = args.Value;
                    DataGrid.RaiseQueryUnBoundRow(args);                    
                }                              
            }
            base.OnEditingComplete(dataColumn,currentRendererElement);
            RaiseCurrentCellValueChangedEvent(DataGrid,dataColumn);
        }
        #endregion

        #region Display/Edit Value Overrides
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (HasCurrentCellState)
            {
                var contentControl = CurrentCellRendererElement as ContentControl;               
                if (contentControl != null && !(contentControl.Content is FrameworkElement))
                    return CurrentCellRendererElement.GetValue(ContentControl.ContentProperty);              
            }
            return base.GetControlValue();
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (HasCurrentCellState)
            {
                var contentControl = CurrentCellRendererElement as ContentControl;               
                if (contentControl != null && !(contentControl.Content is FrameworkElement))
                    contentControl.Content = value;               
            }
        }
        #endregion                   

        #region ShouldGridTryToHandleKeyDown
        /// <summary>
        /// Shoulds the grid try automatic handle key down.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            if (!HasCurrentCellState)
                return true;
            var columnIndex = DataGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex);
            var dataColumn = (CurrentCellElement as GridCell).ColumnBase;            

            var handleTemplatedUIElementKeyDown = FocusManagerHelper.GetWantsKeyInput(dataColumn.GridColumn);

            if ((dataColumn.GridUnBoundRowEventsArgs != null && dataColumn.GridUnBoundRowEventsArgs.EditTemplate != null))
                handleTemplatedUIElementKeyDown = false;

            switch (e.Key)
            {
                case Key.F2:
                case Key.Escape:
                    {
                        e.Handled = true;
                        return true;
                    }
                case Key.Tab:
                    if (!IsInEditing && !(DataGrid.GetLastDataRowIndex() == CurrentCellIndex.RowIndex && DataGrid.SelectionController.CurrentCellManager.GetLastCellIndex() == CurrentCellIndex.ColumnIndex))
                        base.SetFocus(CurrentCellRendererElement, false);
                    return true;
                case Key.Enter:
                case Key.PageUp:
                case Key.PageDown:
                    if (!IsInEditing)
                        base.SetFocus(CurrentCellRendererElement, true);
                    return true;
                case Key.Home:
                case Key.End:
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                    {
                        if (!handleTemplatedUIElementKeyDown && !IsInEditing)
                        {
                            base.SetFocus(CurrentCellRendererElement, false);
                            return true;
                        }
                        else if (handleTemplatedUIElementKeyDown)
                        {
                            return false;
                        }
                        else
                            return !IsInEditing;
                    }
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }        
        #endregion
    }

    /// <summary>
    /// Class that deals with UnBoundRow textcolumn cell.
    /// </summary>
    public class GridUnBoundRowCellTextBoxRenderer : GridUnBoundRowCellRenderer<TextBlock, TextBox>
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
            var column = dataColumn.GridColumn;
            var cellvalue = dataColumn.GridUnBoundRowEventsArgs != null && dataColumn.GridUnBoundRowEventsArgs.Value != null ? dataColumn.GridUnBoundRowEventsArgs.Value.ToString() : string.Empty;
            uiElement.Text = cellvalue;            
            uiElement.SetValue(TextBlock.TextAlignmentProperty, column.textAlignment);
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.verticalAlignment);
            uiElement.VerticalAlignment = VerticalAlignment.Center;
            uiElement.Padding = ProcessUIElementPadding(column);
            uiElement.SetValue(TextBlock.TextTrimmingProperty, column.textTrimming);
            uiElement.SetValue(TextBlock.TextWrappingProperty, column.textWrapping);
#if WPF            
            uiElement.SetValue(TextBlock.TextDecorationsProperty, column.textDecoration);
#endif
        }

        public override void OnInitializeEditElement(DataColumnBase dataColumn, TextBox uiElement, object dataContext)
        {
            var column = dataColumn.GridColumn;
            object contentValue = null;

            contentValue = (dataColumn.GridUnBoundRowEventsArgs != null && dataColumn.GridUnBoundRowEventsArgs.Value != null) ?
                                    dataColumn.GridUnBoundRowEventsArgs.Value.ToString() :
                                string.Empty;                   

            uiElement.Text = contentValue.ToString();

            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            var textAlignBind = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextAlignmentProperty, textAlignBind);
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;

            var verticalContentAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalContentAlignment);
#if WPF
            if (DataGrid.useDrawing)            
                uiElement.SetValue(Control.BorderThicknessProperty, new Thickness(0));                            
#endif
            if (!(column is GridTextColumnBase))
                return;

            var textWrappingBinding = new Binding { Path = new PropertyPath("TextWrapping"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextWrappingProperty, textWrappingBinding);
        }

        /// <summary>
        /// When complete edit, we need to raise query again to provide entered value to customer.
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// <param name="currentRendererElement">The UIElement that was loaded in edit mdoe</param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        ///  GridColumn - Column which is providing the information for Binding
        protected override void OnEditingComplete(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            if (HasCurrentCellState && IsInEditing)
            {                
                var data = (CurrentCellRendererElement as TextBox).Text;
                var args = new GridUnBoundRowEventsArgs(dataColumn.GridUnBoundRowEventsArgs.GridUnboundRow, UnBoundActions.CommitData, data, dataColumn.GridColumn, dataColumn.GridUnBoundRowEventsArgs.CellType, this.DataGrid, new RowColumnIndex(dataColumn.RowIndex,dataColumn.ColumnIndex))
                {
                    CellTemplate = dataColumn.GridUnBoundRowEventsArgs.CellTemplate,
                    EditTemplate = dataColumn.GridUnBoundRowEventsArgs.EditTemplate
                };
                dataColumn.GridUnBoundRowEventsArgs.Value = args.Value;
                DataGrid.RaiseQueryUnBoundRow(args);                               
            }
            base.OnEditingComplete(dataColumn,currentRendererElement);
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

        #region Wire evnets that needs to be done.

        /// <summary>
        /// Invoked when the edit element(TextBox) is loaded on the cell in column
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (TextBox)sender;
            uiElement.TextChanged += OnTextChanged;
#if WinRT
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
