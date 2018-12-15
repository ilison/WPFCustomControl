#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if WPF
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
#else
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
#endif
using Syncfusion.UI.Xaml.Grid.Utility;

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    public class TreeGridCellTemplateRenderer : TreeGridVirtualizingCellRenderer<ContentControl, ContentControl>
    {

        #region Override Methods

        #region Display/Edit Binding Overrides

        /// <summary>
        /// Method overridden to avoid binding for a content cntrol when cell template is not defined.
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <param name="uiElement"></param>
        /// <param name="dataContext"></param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {

        }
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>

        public override void OnInitializeTemplateElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;
            if (column.hasCellTemplate || column.hasCellTemplateSelector)
                base.OnInitializeTemplateElement(dataColumn, uiElement, dataContext);
            else
            {
                InitializeDisplayTemplate(uiElement, column as TreeGridTemplateColumn, dataContext);
                OnUpdateTemplateBinding(dataColumn, uiElement, dataContext);
            }
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            InitializeEditTemplate(uiElement, (TreeGridTemplateColumn)dataColumn.TreeGridColumn, dataContext);
            OnUpdateEditBinding(dataColumn, uiElement, dataContext);
            CanFocus = true;
        }

        public override void OnUpdateEditBinding(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;
            if (column.SetCellBoundValue)
            {
                var dataContextHelper = new TreeGridDataContextHelper { Record = dataContext, DataRow = dataColumn.DataRow };
                dataContextHelper.SetValueBinding(column.ValueBinding, dataContext);
                uiElement.Content = dataContextHelper;
            }
            else
                uiElement.SetBinding(ContentControl.ContentProperty, new Binding());
        }
        #endregion

        #region Arrange UIElement

        /// <summary>
        /// Set focus to UIElement loaded in DataTemplate of TreeGridCell on loading. since OnEditElementloaded will not fire for TreeGridCell again when start editing.
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

        #region Display/Edit Value Overrides

        /// <summary>
        /// Called when [entered edit mode].
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <param name="currentRendererElement">The current renderer element.</param>
        protected override void OnEnteredEditMode(TreeDataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            base.OnEnteredEditMode(dataColumn, currentRendererElement);
            isfocused = false;
        }

        public override bool CanValidate()
        {
            return false;
        }
        /// <summary>
        /// Initializes the cell style.
        /// </summary>        
        /// <param name="record">The record.</param>
        /// <param name="dataColumn">DataColumn which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element        
        /// TreeGridColumn - Column which is providing the information for Binding
        protected override void InitializeCellStyle(TreeDataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            base.InitializeCellStyle(dataColumn, record);
            var cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            var uiElement = (cell as TreeGridCell).Content as ContentControl;
            var gridColumn = dataColumn.TreeGridColumn as TreeGridTemplateColumn;
            if (uiElement != null && gridColumn != null)
            {
                if (HasCurrentCellState && CurrentCellIndex == cellRowColumnIndex && IsInEditing)
                    InitializeEditTemplate(uiElement, gridColumn, record);

                else if (gridColumn.hasCellTemplate || gridColumn.hasCellTemplateSelector || (TreeGrid != null && TreeGrid.hasCellTemplateSelector))
                    OnInitializeTemplateElement(dataColumn, uiElement, record);
            }
        }

        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        protected override void OnUnwireEditUIElement(ContentControl uiElement)
        {
            base.OnUnwireEditUIElement(uiElement);
        }


        /// <summary>
        /// Called when [unwire display unique identifier element].
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        protected override void OnUnwireDisplayUIElement(ContentControl uiElement)
        {
            base.OnUnwireDisplayUIElement(uiElement);
        }
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

            if (!HasCurrentCellState) return false;
            var columnIndex = TreeGrid.ResolveToGridVisibleColumnIndex((this.CurrentCellElement as TreeGridCell).ColumnBase.ColumnIndex);
            var column = ((TreeGridTemplateColumn)TreeGrid.Columns[columnIndex]);
            var handleTemplatedUIElementKeyDown = FocusManagerHelper.GetWantsKeyInput(column);


            if ((column.hasCellTemplate || column.hasCellTemplateSelector) && (column.hasEditTemplate || column.hasEditTemplateSelector))

                handleTemplatedUIElementKeyDown = false;
            switch (e.Key)
            {
                case Key.Space:
                    return !handleTemplatedUIElementKeyDown;
                case Key.A:
                case Key.V:
                case Key.X:
                case Key.C:
                    {
                        if (!handleTemplatedUIElementKeyDown && SelectionHelper.CheckControlKeyPressed() && !IsInEditing)
                            return true;
                        break;
                    }
                case Key.F2:
                case Key.Escape:
                    {
                        e.Handled = true;
                        return true;
                    }
                case Key.Tab:
                    // When press tab continuously  and its reach to last one, then the focus goes to grid.Again if you press upo key, focus any of the content control. to avoid that the condition added.
                    if (!IsInEditing && !(TreeGrid.GetLastDataRowIndex() == CurrentCellIndex.RowIndex && TreeGrid.SelectionController.CurrentCellManager.GetLastCellIndex() == CurrentCellIndex.ColumnIndex))
                        // If Column with CellTemplate has editor loaded, then the click on that editor will not as we click or navigate to next cell, we need to remove the focus.
                        base.SetFocus(CurrentCellRendererElement, false);
                    return true;
                case Key.Enter:
                case Key.PageUp:
                case Key.PageDown:
                    if (!IsInEditing)
                        // If Column with CellTemplate has editor loaded, then navigation of this key will set the focus.
                        base.SetFocus(CurrentCellRendererElement, true);
                    return true;
                case Key.Home:
                case Key.End:
                case Key.Down:
                case Key.Up:
                case Key.Left:
                case Key.Right:
                    // if Column has WantsKeyInput Enabled the navigation should be in editor.
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
                case Key.Delete:
                    {
                        if (handleTemplatedUIElementKeyDown)
                            return false;
                        return !IsInEditing;
                    }
            }
            return false;
        }
        #endregion

        #endregion


        #region Private Methods

        /// <summary>
        /// Applies the display data template.
        /// </summary>                
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// <param name="templateColumn">TreeGridColumn - Column which is providing the information for Binding </param> 
        /// <param name="dataContext">The data context.</param>
        private void InitializeDisplayTemplate(ContentControl uiElement, TreeGridTemplateColumn templateColumn,
                                               object dataContext)
        {
            if (templateColumn.TreeGrid != null && templateColumn.TreeGrid.CellTemplateSelector != null)
                uiElement.ContentTemplateSelector = templateColumn.TreeGrid.CellTemplateSelector;
        }


        /// <summary>
        /// Applies the edit data template.
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        /// <param name="templateColumn">The template column.</param>
        /// <param name="dataContext">The data context.</param>
        private void InitializeEditTemplate(ContentControl uiElement, TreeGridTemplateColumn templateColumn,
                                            object dataContext)
        {
            if (templateColumn.hasEditTemplate)
                uiElement.ContentTemplate = templateColumn.EditTemplate;
            else if (templateColumn.hasEditTemplateSelector)
            {
                uiElement.ContentTemplateSelector = templateColumn.EditTemplateSelector;
#if UWP
                //UWP-3838 DataTemplates are not loaded when using ContentTemplateSelector in UWP.
                //By using ContentTemplate the DataTemplates are loaded.
                uiElement.ContentTemplate = templateColumn.EditTemplateSelector.SelectTemplate(dataContext, uiElement);
#endif
            }      
        }

        protected override void OnEditingComplete(TreeDataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            base.OnEditingComplete(dataColumn, currentRendererElement);
            RaiseCurrentCellValueChangedEvent(TreeGrid, dataColumn);
        }
        #endregion
    }

    /// <summary>
    /// Represents a class which is used as DataContext for the TreeGrid with Template and Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.SetCellBoundValue TreeGridColumn value is true.
    /// </summary>
    public class TreeGridDataContextHelper : DataContextHelper
    {
        public TreeDataRowBase DataRow { get; set; }
    }
}
