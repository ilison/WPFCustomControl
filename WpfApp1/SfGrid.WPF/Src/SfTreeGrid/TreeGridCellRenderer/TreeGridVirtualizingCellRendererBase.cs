#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Data;
using Windows.UI.Core;
using Key = Windows.System.VirtualKey;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#endif


namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using Windows.UI.Core;
#endif
    /// <summary>
    /// TreeGridVirtualizingCellRendererBase is an abstract base class for cell renderers
    /// that need live UIElement visuals displayed in a cell. You can derive from
    /// this class and provide the type of the UIElement you want to show inside cells
    /// as type parameter. The class provides strong typed virtual methods for 
    /// initializing content of the cell and arranging the cell visuals.
    /// <para/>
    /// The class manages the creation 
    /// of cells UIElement objects when the cell is scrolled into view and also 
    /// unloading of the elements. The class offers an optimization in which 
    /// elements can be recycled when <see cref="AllowRecycle"/> is set. 
    /// In this case when a cell is scrolled out of view
    /// it is moved into a recycle bin and the next time a new element is scrolled into
    /// view the element is recovered from the recycle bin and reinitialized with the
    /// new content of the cell.<para/>
    /// when the user moves the mouse over the cell or if the UIElement is needed for
    /// other reasons.<para/>
    /// After a UIElement was created the virtual methods <see cref="WireEditUIElement"/> 
    /// and <see cref="UnwireEditUIElement"/> are called to wire any event listeners.
    /// <para/>
    /// Updates to appearance and content of child elements, creation and unloading
    /// of elements will not trigger ArrangeOverride or Render calls in parent canvas.
    /// <para/>
    /// </summary>
    /// <typeparam name="D">The type of the UIElement that should be placed inside cells in display mode.</typeparam>
    /// <typeparam name="E">The type of the UIElement that should be placed inside cells in edit mode.</typeparam>

    public abstract class TreeGridVirtualizingCellRendererBase<D, E> : TreeGridCellRendererBase
        where D : FrameworkElement, new()
        where E : FrameworkElement, new()
    {
        #region Fields

        private bool allowRecycle = true;
        //AgunaCapital incident: 136598. we set readonly
        protected readonly VirtualizingCellUIElementBin<D> DisplayRecycleBin = new VirtualizingCellUIElementBin<D>();
        protected readonly VirtualizingCellUIElementBin<E> EditRecycleBin = new VirtualizingCellUIElementBin<E>();
        protected readonly VirtualizingCellUIElementBin<ContentControl> TemplateRecycleBin = new VirtualizingCellUIElementBin<ContentControl>();

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase"/> class.
        /// </summary>
        public TreeGridVirtualizingCellRendererBase()
        {

        }

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets a value that indicates whether elements can be recycled when scrolled out of view.     
        /// </summary>
        /// <value>
        /// <b>true</b> if elements can be recycled when scrolled out of view; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The elements moved into a recycle bin when a cell is scrolled out of view
        /// and the next time a new element is scrolled into
        /// view the element is recovered from the recycle bin and reinitialized with the
        /// new content of the cell. 
        /// </remarks>
        public bool AllowRecycle
        {
            get { return allowRecycle; }
            set { allowRecycle = value; }
        }

        #endregion

        #region override methods

        /// <summary>
        /// Invoked when the UIElement for cell is prepared to render it in view .
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase"/> overrides this method and
        /// creates new UIElements and wires them with the parent cells control.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding column of the element.
        /// </param>
        /// <param name="record">
        /// The corresponding Record for the element.
        /// </param>
        /// <param name="isInEdit">
        /// Specifies whether the element is editable or not.
        /// </param>
        /// <returns>
        /// Returns the new cell UIElement.
        /// </returns>
        protected internal override FrameworkElement OnPrepareUIElements(TreeDataColumnBase dataColumn, object record, bool isInEdit)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            FrameworkElement cellContainer = dataColumn.ColumnElement;
            TreeGridColumn column = dataColumn.TreeGridColumn;
            FrameworkElement cellcontent = null;

            // Create TreeGridCell only for editable columns
            // UseOnlyRendererElement for Non-Editable columns
            if (!UseOnlyRendererElement)
            {
                if (dataColumn.ColumnElement == null)
                    throw new InvalidOperationException("ColumnElement can't be null in  OnPrepareUIElements when UseOnlyRendererElement is false");
                cellContainer = dataColumn.ColumnElement;
            }
            else if (dataColumn.ColumnElement != null)
            {
                throw new InvalidOperationException("ColumnElement should be null in  OnPrepareUIElements when UseOnlyRendererElement is true");
            }

            if (this.SupportsRenderOptimization && !isInEdit)
            {
                if ((!column.hasCellTemplate && !column.hasCellTemplateSelector))
                {
                    // Cell Content will be created for Non Template cells.
                    cellcontent = CreateOrRecycleDisplayUIElement();
                    InitializeDisplayElement(dataColumn, (D)cellcontent, record);
                    WireDisplayUIElement((D)cellcontent);
                }
                else
                {
                    // We wont create Cell Content for Templated cells. 
                    // TreeGridCell is used as RendererElement with template case.
                    InitializeTemplateElement(dataColumn, (ContentControl)cellContainer, record);
                    WireTemplateUIElement((ContentControl)cellContainer);
                }
                if (cellcontent != null && !UseOnlyRendererElement)
                    (cellContainer as TreeGridCell).Content = cellcontent;
            }
            else
            {
                cellcontent = CreateOrEditRecycleUIElement();
                if (dataColumn.TreeGridColumn != null)
                    dataColumn.TreeGridColumn.IsInSuspend = true;
                InitializeEditElement(dataColumn, (E)cellcontent, record);
                if (dataColumn.TreeGridColumn != null)
                    dataColumn.TreeGridColumn.IsInSuspend = false;
                WireEditUIElement((E)cellcontent);
                // TreeGridImageColumn, TreeeGridHyperLinkColumn and TreeGridCheckBoxColumn are Noneditable columns. 
                //So content created and set to TreeGridCell.      
                if (cellcontent != null && cellContainer is TreeGridCell)
                    (cellContainer as TreeGridCell).Content = cellcontent;
            }
            return UseOnlyRendererElement ? cellcontent : cellContainer;
        }

        /// <summary>
        /// Resets the TreeGridCell's content such as ContentTemplate and ContentTemplateSelector for reuse purpose.
        /// </summary>
        /// <param name="Control">
        /// Specifies the control to reuse.
        /// </param>
        private void ResetGridCell(ContentControl Control)
        {
            Control.Content = null;
            Control.ContentTemplate = null;
            Control.ContentTemplateSelector = null;
        }

        /// <summary>
        /// Starts an edit operation on a current cell.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// Specifies the row and column index of the cell to start an edit operation.
        /// </param>
        /// <param name="cellElement">
        /// Specifies the UIElement of the cell to start an edit operation.
        /// </param>
        /// <param name="column">
        /// The corresponding column to edit the cell.
        /// </param>
        /// <param name="record">
        /// The corresponding record to edit the cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the current cell starts an editing; otherwise, <b>false</b>.
        /// </returns>

        public override bool BeginEdit(RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, TreeGridColumn column, object record)
        {
            if (!this.HasCurrentCellState)
                return false;

            if (this.SupportsRenderOptimization)
            {
                E cellcontent = null;

                if (!UseOnlyRendererElement && cellElement == null)
                    throw new Exception("Cell Element will not be get null for any case");

                var dataColumn = (cellElement as TreeGridCell).ColumnBase;

                OnUnloadUIElements(dataColumn);

                // Cell content will be null for templated case always.
                cellcontent = (column.IsTemplate && ((column as TreeGridTemplateColumn).hasEditTemplate || (column as TreeGridTemplateColumn).hasEditTemplateSelector)) ?
                                 null : CreateOrEditRecycleUIElement();

                if (dataColumn.TreeGridColumn != null)
                    dataColumn.TreeGridColumn.IsInSuspend = true;
                InitializeEditElement(dataColumn, (E)(cellcontent ?? cellElement), record);
                if (dataColumn.TreeGridColumn != null)
                    dataColumn.TreeGridColumn.IsInSuspend = false;

                WireEditUIElement((E)(cellcontent ?? cellElement));

                if (cellcontent != null)
                    (cellElement as TreeGridCell).Content = cellcontent;

                OnEnteredEditMode(dataColumn, cellcontent ?? cellElement);
            }
            else
                OnEnteredEditMode(null, this.CurrentCellRendererElement);

            return this.IsInEditing;
        }

        /// <summary>
        /// Invoked when the cell is being entered on the edit mode.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding datacolumn being entered on the edit mode.
        /// </param>
        /// <param name="currentRendererElement">
        /// The corresponding renderer element in edit mode.
        /// </param>
        protected virtual void OnEnteredEditMode(TreeDataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            this.UpdateCurrentCellState(currentRendererElement, true);
        }
        /// <summary>
        /// Ends the edit occurring on the cell.
        /// </summary>
        /// <param name="dc">
        /// The corresponding datacolumn to complete the edit operation.
        /// </param>
        /// <param name="record">
        /// The corresponding record to complete the edit operation.
        /// </param>
        /// <param name="canResetBinding">
        /// Specifies whether the binding is reset or not.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the edit ends on the cell ; otherwise, <b>false</b>.
        /// </returns>
        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            var cellRowColumnIndex = new RowColumnIndex(dc.RowIndex, dc.ColumnIndex);
            var cellElement = dc.ColumnElement;
            var column = dc.TreeGridColumn;

            if (!this.HasCurrentCellState)
                return false;
            if (!this.IsInEditing)
                return false;
            if (this.SupportsRenderOptimization)
            {
#if WPF
                E uiElement = null;
                if (!UseOnlyRendererElement && cellElement is TreeGridCell)
                    uiElement = (E)((cellElement as TreeGridCell).Content is FrameworkElement ? (cellElement as TreeGridCell).Content as FrameworkElement : cellElement);
                else
                    uiElement = cellElement as E;
                uiElement.PreviewLostKeyboardFocus -= OnLostKeyboardFocus;
#endif
                //this.IsFocused = false;
                this.SetFocus(false);
                var dataColumn = (cellElement as TreeGridCell).ColumnBase;

                OnEditingComplete(dataColumn, CurrentCellRendererElement);
                if (!UseOnlyRendererElement && cellElement == null)
                    throw new Exception("Cell Element will not be get null for any case");
                OnUnloadUIElements(dataColumn);

                OnPrepareUIElements(dataColumn, record, false);
                if (!column.hasCellTemplate && !column.hasCellTemplateSelector)
                    UpdateCurrentCellState((cellElement as TreeGridCell).Content as FrameworkElement, false);
                else
                    UpdateCurrentCellState(cellElement as FrameworkElement, false);
            }
            else
                UpdateCurrentCellState(this.CurrentCellRendererElement, false);

            return !this.IsInEditing;
        }
        /// <summary>
        /// Invoked when the editing is completed on the cell. 
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding datacolumn of the cell.
        /// </param>
        /// <param name="currentRendererElement">
        /// The corresponding renderer element of the cell.
        /// </param>
        protected virtual void OnEditingComplete(TreeDataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {

        }

        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase &lt; D,E &gt;"/> overrides this method and either removes the cell renderer visuals from the parent
        /// or hide them and reuse it later in same element depending on whether <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase&lt; D,E &gt;.AllowRecycle"/>  was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>                     
        protected override void OnUnloadUIElements(TreeDataColumnBase dataColumn)
        {
            var cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            var uiElements = dataColumn.ColumnElement;            
            if (this.HasCurrentCellState && this.IsInEditing && this.CurrentCellIndex == cellRowColumnIndex)
            {
                UnloadEditUIElement(uiElements, dataColumn);
            }
            else
            {
                if (SupportsRenderOptimization)
                    UnloadDisplayUIElement(uiElements, dataColumn);
                else
                    UnloadEditUIElement(uiElements, dataColumn);
            }
        }

        /// <summary>
        /// Invoked when the visual children of cell is arranged in view. 
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// The corresponding row and column index of the cell.
        /// </param>
        /// <param name="uiElement">
        /// The corresponding UiElement that is to be arranged
        /// </param>
        /// <param name="cellRect">
        /// The corresponding size of cell element for arranging the UIElement
        /// </param>        
        protected override void OnArrange(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Rect cellRect)
        {
            uiElement.Arrange(cellRect);
        }

        /// <summary>
        /// Invoked when the desired size for cell is measured.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// The corresponding row and column index of the cell</param>
        /// <param name="uiElement">
        /// Specifies the corresponding UiElement to measure.
        /// </param>
        /// <param name="availableSize">
        /// The available size that a parent element can allocate the cell.
        /// </param>  
        protected override void OnMeasure(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Size availableSize)
        {
            uiElement.Measure(availableSize);
        }

        /// <summary>
        /// Updates the binding of the Cell UIElement for the specified column.
        /// Implement this method to update binding when the cell UIElement is reused during horizontal scrolling.        
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the corresponding column to update binding.
        /// </param>
        /// <param name="record">
        /// The corresponding record to update binding.
        /// </param>
        /// <param name="isInEdit">
        /// Indicates the whether the cell is editable or not.
        /// </param>     
        protected internal override void OnUpdateBindingInfo(TreeDataColumnBase dataColumn, object record, bool isInEdit)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            FrameworkElement uiElement = dataColumn.ColumnElement;
            TreeGridColumn column = dataColumn.TreeGridColumn;
            FrameworkElement rendererElement = null;

            if (UseOnlyRendererElement)
                rendererElement = uiElement as FrameworkElement;
            else if (uiElement is TreeGridCell)
                rendererElement = (uiElement as TreeGridCell).Content is FrameworkElement ? (uiElement as TreeGridCell).Content as FrameworkElement : uiElement;

            if (this.SupportsRenderOptimization && !isInEdit)
            {
                if (!column.hasCellTemplate && !column.hasCellTemplateSelector)
                    OnUpdateDisplayBinding(dataColumn, (D)rendererElement, record);
                else
                    OnUpdateTemplateBinding(dataColumn, (ContentControl)rendererElement, record);
            }
            else
                OnUpdateEditBinding(dataColumn, (E)rendererElement, record);
        }
        /// <summary>
        /// Updates the tool tip for GridCell.
        /// </summary>
        /// <param name="dataColumn">Which holds GridColumn, Row Column Index and GridCell</param>
        public override void UpdateToolTip(TreeDataColumnBase dataColumn)
        {
            var uiElement = dataColumn.ColumnElement;
            var column = dataColumn.TreeGridColumn;
            if (dataColumn.IsEditing || column == null || !column.ShowToolTip)
            {
                uiElement.ClearValue(ToolTipService.ToolTipProperty);
                return;
            }
            object dataContext = dataColumn.ColumnElement.DataContext;

            var obj = ToolTipService.GetToolTip(uiElement);
            ToolTip tooltip;
            if (obj is ToolTip)
                tooltip = obj as ToolTip;
            else
                tooltip = new ToolTip();
            if (column.hasToolTipTemplate || column.hasToolTipTemplateSelector)
            {
                if (column.SetCellBoundToolTip)
                {
                    var dataContextHelper = new TreeGridDataContextHelper { Record = dataContext };
                    dataContextHelper.SetValueBinding(column.DisplayBinding, dataContext);
                    tooltip.Content = dataContextHelper;
                }
                else
                    tooltip.Content = dataContext;

                if (column.hasToolTipTemplate)
                    tooltip.ContentTemplate = column.ToolTipTemplate;
                else if (column.hasToolTipTemplateSelector)
                    tooltip.ContentTemplateSelector = column.ToolTipTemplateSelector;
            }
            else
            {
                var provider = column.TreeGrid.View.GetPropertyAccessProvider();
                if (provider != null)
                {
                    var displaytext = Convert.ToString(provider.GetFormattedValue(dataColumn.ColumnElement.DataContext, column.MappingName));
                    tooltip.Content = displaytext;
                    if (string.IsNullOrEmpty(displaytext))
                        tooltip.IsEnabled = false;
                }
            }
            //Specifies to raise tooltip opening event for the corresponding cell
            if (dataColumn.RaiseCellToolTipOpening(tooltip))
                ToolTipService.SetToolTip(uiElement, tooltip);
            else
                uiElement.ClearValue(ToolTipService.ToolTipProperty);
        }
        #endregion

        #region virtual methods

        /// <summary>
        /// Creates a new UIElement for the edit mode of cell.
        /// </summary>
        /// <returns>
        /// Returns the new UIElement for edit mode of cell. 
        /// </returns>
        protected virtual E OnCreateEditUIElement()
        {
            var uiElement = new E();
#if WPF
            Validation.SetErrorTemplate(uiElement, null);
#endif
            return uiElement;
        }
        /// <summary>
        /// Creates a new UIElement for the display mode of cell.
        /// </summary>
        /// <returns>
        /// Returns the new UIElement for display mode of cell. 
        /// </returns>
        protected virtual D OnCreateDisplayUIElement()
        {
            var uiElement = new D();
#if WPF
            Validation.SetErrorTemplate(uiElement, null);
#endif
            return uiElement;
        }

        #endregion

        #region abstract methods
        /// <summary>
        /// Invoked when the display element is initialized on the cell.
        /// </summary>
        /// <param name="dataColumn">
        ///  The dataColumn where the cell is located.
        /// </param>
        /// <param name="uiElement">
        /// The uiElement that is initialized on the display element of cell.
        /// </param>
        /// <param name="dataContext">
        /// The dataContext of the cell.
        /// </param>
        public abstract void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, D uiElement, object dataContext);

        /// <summary>
        /// Updates the binding for display element of cell in column.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding dataColumn where the cell is located.              
        /// </param>
        /// <param name="uiElement">
        /// The corresponding uiElement to update display element.
        /// </param>
        /// <param name="dataContext">
        /// The data context of the cell.
        /// </param>
        public abstract void OnUpdateDisplayBinding(TreeDataColumnBase dataColumn, D uiElement, object dataContext);

        /// <summary>
        /// Invoked when the template element is initialized on the cell.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding dataColumn where the cell is located.              
        /// </param>
        /// <param name="uiElement">
        /// The corresponding uiElement to initialize the template element.
        /// </param>
        /// <param name="dataContext">
        /// The data context of the cell.
        /// </param>
        public abstract void OnInitializeTemplateElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext);

        /// <summary>
        /// Updates the binding for template element of cell in column.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding dataColumn where the cell is located.              
        /// </param>
        /// <param name="uiElement">
        /// The corresponding uiElement to update template element.
        /// </param>
        /// <param name="dataContext">
        /// The data context of the cell.
        /// </param>
        public abstract void OnUpdateTemplateBinding(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext);

        /// <summary>
        /// Invoked when the edit element is initialized on the cell.
        /// </summary>
        /// <param name="treeColumn">
        ///  The dataColumn where the cell is located.
        /// </param>
        /// <param name="element">
        /// The element that is initialized on the edit element of cell.
        /// </param>
        /// <param name="dataContext">
        /// The dataContext of the cell.
        /// </param>
        public abstract void OnInitializeEditElement(TreeDataColumnBase treeColumn, E uiElement, object dataContext);

        /// <summary>
        /// Updates the binding for edit element of cell in column.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding dataColumn where the cell is located.              
        /// </param>
        /// <param name="element">
        /// The corresponding element to update binding of edit element.
        /// </param>
        /// <param name="dataContext">
        /// The data context of the cell.
        /// </param>
        public abstract void OnUpdateEditBinding(TreeDataColumnBase dataColumn, E element, object dataContext);

        #endregion

        #region public methods

        /// <summary>
        /// Initializes an edit element of the cell in column.
        /// </summary>
        /// <param name="treeColumn">
        /// The dataColumn where the cell is located.
        /// </param>
        /// <param name="element">
        /// The element that is initialized on the edit element of cell.
        /// </param>
        /// <param name="dataContext">
        /// The dataContext of the cell.
        /// </param>
        public void InitializeEditElement(TreeDataColumnBase treeColumn, E uiElement, object dataContext)
        {
            OnInitializeEditElement(treeColumn, uiElement, dataContext);
        }

        /// <summary>
        /// Initializes the display element of the cell in column.
        /// </summary>
        /// <param name="dataColumn">
        ///  The dataColumn where the cell is located.
        /// </param>
        /// <param name="uiElement">
        /// The uiElement that is initialized on the display element of cell.
        /// </param>
        /// <param name="dataContext">
        /// The dataContext of the cell.
        /// </param>
        public void InitializeDisplayElement(TreeDataColumnBase dataColumn, D uiElement, object dataContext)
        {
            OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
        }

        /// <summary>
        /// Invoked when the template element is initialized on the cell.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding dataColumn where the cell is located.              
        /// </param>
        /// <param name="uiElement">
        /// The corresponding uiElement to initialize the template element.
        /// </param>
        /// <param name="dataContext">
        /// The data context of the cell.
        /// </param>
        public void InitializeTemplateElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            OnInitializeTemplateElement(dataColumn, uiElement, dataContext);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Method which is return the UIElement for Cell.
        /// This method will create new element or Recycle the old element.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        internal D CreateOrRecycleDisplayUIElement()
        {
            D uiElement;
            if (AllowRecycle)
            {
                uiElement = DisplayRecycleBin.Dequeue(this);

                if (uiElement != null)
                {
                    return uiElement;
                }
            }
            uiElement = OnCreateDisplayUIElement();
            return uiElement;
        }

        internal E CreateOrEditRecycleUIElement()
        {
            E uiElement;
            if (AllowRecycle)
            {
                uiElement = EditRecycleBin.Dequeue(this);

                if (uiElement != null)
                {
                    return uiElement;
                }
            }
            uiElement = OnCreateEditUIElement();
            return uiElement;
        }

        /// <summary>
        /// To unload edit elements  
        /// </summary>
        /// <param name="uiElements"></param>
        /// <param name="column">Need column to check if column has HeaderTemplate</param> 
        private void UnloadEditUIElement(FrameworkElement uiElements, TreeDataColumnBase column)
        {
            E uiElement = null;
            if (!UseOnlyRendererElement && uiElements is TreeGridCell)
                uiElement = (E)((uiElements as TreeGridCell).Content is FrameworkElement ? (uiElements as TreeGridCell).Content as FrameworkElement : uiElements);
            else
            {
                uiElement = uiElements as E;
                column.ColumnElement = null;
            }
            if (uiElement != null)
            {
                UnwireEditUIElement(uiElement);

                if (AllowRecycle && !(uiElement is TreeGridCell))
                    EditRecycleBin.Enqueue(this, uiElement);

                if (!UseOnlyRendererElement && uiElements is TreeGridCell)
                    ResetGridCell((ContentControl)uiElements);
                else if (uiElement.Parent is Panel)
                {
                    if (column.TreeGridColumn != null && column.TreeGridColumn.hasHeaderTemplate || TreeGrid.hasHeaderTemplate)
                        ResetGridCell((ContentControl)uiElements);
                    (uiElement.Parent as Panel).Children.Remove(uiElement);
                }
            }
        }

        private void UnloadDisplayUIElement(FrameworkElement uiElements, TreeDataColumnBase column)
        {
            D uiElement = null;

            if (!(column.TreeGridColumn.hasCellTemplate || column.TreeGridColumn.hasCellTemplateSelector))
            {
                if (!UseOnlyRendererElement && uiElements is TreeGridCell)
                    uiElement = (uiElements as TreeGridCell).Content as D;
                else
                    uiElement = uiElements as D;
            }

            if (uiElement != null)
            {
                UnwireDisplayUIElement(uiElement);
                if (AllowRecycle)
                    DisplayRecycleBin.Enqueue(this, uiElement);
            }

            if (!UseOnlyRendererElement && uiElements is TreeGridCell)
                ResetGridCell(uiElements as TreeGridCell);
            else if (uiElement.Parent is Panel)
                (uiElement.Parent as Panel).Children.Remove(uiElement);
        }

        internal void WireDisplayUIElement(D uiElamant)
        {
            OnWireDisplayUIElement(uiElamant);
        }

        internal void WireTemplateUIElement(ContentControl uiElamant)
        {
            OnWireTemplateUIElement(uiElamant);
        }

        internal void UnwireDisplayUIElement(D uiElamant)
        {
            OnUnwireDisplayUIElement(uiElamant);
        }

        internal void UnwireTemplateUIElement(ContentControl uiElamant)
        {
            OnUnwireTemplateUIElement(uiElamant);
        }

        /// <summary>
        /// Wires the events associated with display UIElement of the cell.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding display UIElement to wire its events.
        /// </param>
        protected virtual void OnWireDisplayUIElement(D uiElement)
        {

        }

        /// <summary>
        /// Unwires the events associated with display UIElement of the cell.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding display UIElement to unwire its events.
        /// </param>
        protected virtual void OnUnwireDisplayUIElement(D uiElement)
        {

        }

        /// <summary>
        /// Wires the events associated with template element of the cell.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding template UIElement to wire its events.
        /// </param>
        protected virtual void OnWireTemplateUIElement(ContentControl uiElement)
        {

        }

        /// <summary>
        /// Unwires the events associated with template element of the cell.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding template UIElement to unwire its events.
        /// </param>
        protected virtual void OnUnwireTemplateUIElement(ContentControl uiElement)
        {

        }
        /// <summary>
        /// Wires the events associated with edit UIElement.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding edit UIElement to wire its events.
        /// </param>        
        internal void WireEditUIElement(E uiElement)
        {
            OnWireEditUIElement(uiElement);
            uiElement.LostFocus += OnEditElementLostFocus;
            uiElement.Loaded += OnEditElementLoaded;
            uiElement.Unloaded += OnEditElementUnloaded;            
#if WPF
            uiElement.PreviewLostKeyboardFocus += OnLostKeyboardFocus;
#endif
        }

        /// <summary>
        /// Unwires the events associated with edit UIElement.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding edit UIElement to unwire its events.
        /// </param>     
        private void UnwireEditUIElement(E uiElement)
        {
            OnUnwireEditUIElement(uiElement);
            uiElement.LostFocus -= OnEditElementLostFocus;
            uiElement.Loaded -= OnEditElementLoaded;
            uiElement.Unloaded -= OnEditElementUnloaded;
#if WPF
            uiElement.PreviewLostKeyboardFocus -= OnLostKeyboardFocus;
#endif
        }
        /// <summary>
        /// Wires the events associated with edit UIElement.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding edit UIElement to wire its events.
        /// </param>   
        protected virtual void OnWireEditUIElement(E uiElement)
        {

        }

        /// <summary>
        /// Unwires the events associated with edit UIElement.
        /// </summary>
        /// <param name="uiElement">
        /// The corresponding edit UIElement to unwire its events.
        /// </param>   
        protected virtual void OnUnwireEditUIElement(E uiElement)
        {

        }

        /// <summary>
        /// Invoked when the edit element is loaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>
        /// Invoked when the edit element is loaded on the cell in column
        /// </summary>
        /// <param name="sender">
        /// The sender that contains the corresponding edit UIElement.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs"/> that contains event data.
        /// </param>
        protected virtual void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Invoked when the edit element is unloaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>
        /// Invoked when the edit element is unloaded on the cell in column
        /// </summary>
        /// <param name="sender">
        /// The sender that contains the corresponding edit UIElement.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs"/> that contains event data.
        /// </param>       
        protected virtual void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {

        }

#if WPF
        private bool IsKeyboardFocusWithin<T>(T uiElement, T focusedElement) where T : DependencyObject
        {
            List<DependencyObject> childrens = new List<DependencyObject>();
            GridUtil.Descendant(uiElement, ref childrens);
            return childrens.Contains(focusedElement);
        }

        internal void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        { 
            var uiElement = sender as UIElement;
            if (uiElement != null)
            {
                var IsKeyboardFocusWithin = false;
                var focusedElement = e.NewFocus as DependencyObject;
                if (focusedElement != null)
                {
                    if (focusedElement == e.Source) return;
                   IsKeyboardFocusWithin = this.IsKeyboardFocusWithin(uiElement, focusedElement);
                }

                if (IsKeyboardFocusWithin)
                    return;
            }

            //WPF-24276 - Need to ensure the CurrentCellState once again after raising the ValidationEvents. 
            if (!CheckToAllowFocus(e.NewFocus, sender) && this.HasCurrentCellState)
            {
                e.Handled = true;
                if (this.CurrentCellElement is TreeGridCell && (this.CurrentCellElement as TreeGridCell).ColumnBase.TreeGridColumn.IsTemplate)
                {
                    if (FocusManagerHelper.GetFocusedUIElement(this.CurrentCellRendererElement) != null)
                        (FocusManagerHelper.GetFocusedUIElement(this.CurrentCellRendererElement)).CaptureMouse();
                }
                else
                {
                    (sender as FrameworkElement).CaptureMouse();
                }
            }
        }
#endif
        /// <summary>
        /// Invoked when the edit element loses its focus on the cell.
        /// </summary>
        /// <param name="sender">
        /// The sender that contains the corresponding edit UIElement.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Windows.RoutedEventArgs"/> that contains event data.
        /// </param>
#if UWP
        protected async virtual void OnEditElementLostFocus(object sender, RoutedEventArgs e)
#else
        protected virtual void OnEditElementLostFocus(object sender, RoutedEventArgs e)
#endif
        {
            //OnLostKeyboardFocus - Take care of validation in WPF
            if (this.HasCurrentCellState && this.CurrentCellRendererElement == sender)
            {
                this.isfocused = false;
            }

#if UWP
            var uiElement = sender as UIElement;
            if (uiElement != null)
            {
                var IsKeyboardFocusWithin = false;
                var focusedElement = FocusManager.GetFocusedElement() as DependencyObject;
                if (focusedElement != null)
                {
                    if (focusedElement == sender) return;
                    List<DependencyObject> childrens = new List<DependencyObject>();
                    GridUtil.Descendant(uiElement, ref childrens);
                    IsKeyboardFocusWithin = childrens.Contains(focusedElement);
                }
                if (IsKeyboardFocusWithin)
                    return;
            }

            if (HasCurrentCellState && !CheckToAllowFocus(FocusManager.GetFocusedElement(), sender))
            {
                Control element = sender as Control;

                if (FocusManagerHelper.GetFocusedUIElement(this.CurrentCellRendererElement) != null)
                    element = FocusManagerHelper.GetFocusedUIElement(this.CurrentCellRendererElement);

                await element.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    element.Focus(FocusState.Programmatic);
                });
            }
#endif
        }
        private bool CheckToAllowFocus(object element, object sender)
        {
            if (this.TreeGrid == null || !this.HasCurrentCellState)
                return true;

            if ((element is TreeGridCell && sender.Equals(((element as TreeGridCell).Content))))
                return true;
            if ((this is TreeGridCellTemplateRenderer || this.TreeGrid.ValidationHelper.RaiseCellValidate(this.CurrentCellIndex, this, true)) && this.HasCurrentCellState)
            {
                if (!(element is TreeGridCell))
                    return this.TreeGrid.ValidationHelper.RaiseRowValidate(this.CurrentCellIndex);
                else if ((element as TreeGridCell).ColumnBase != null && (element as TreeGridCell).ColumnBase.RowIndex != this.CurrentCellIndex.RowIndex)
                    return this.TreeGrid.ValidationHelper.RaiseRowValidate(this.CurrentCellIndex);
            }
            else
                return false;
            return true;
        }

        /// <summary>
        /// Initializes the custom style for cell when the corresponding API's and Selectors are used.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding DataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell to initialize cell style.
        /// </param>
        /// <param name="record">
        /// The corresponding record to initialize cell style.
        /// </param>        
        protected virtual void InitializeCellStyle(TreeDataColumnBase dataColumn, object record)
        {
            this.SetCellStyle(dataColumn, record);
        }

        /// <summary>
        /// Updates the style for the particular column.
        /// Implement this method to update style when the cell UIElement is reused during scrolling.          
        /// </summary>
        /// <param name="treeDataColumn">
        /// Specifies the corresponding column to update style.
        /// </param>  
        protected override void OnUpdateStyleInfo(TreeDataColumn treeDataColumn)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(treeDataColumn.RowIndex, treeDataColumn.ColumnIndex);
            FrameworkElement uiElement = treeDataColumn.ColumnElement;
            TreeGridColumn column = treeDataColumn.TreeGridColumn;

            if (uiElement.Visibility == Visibility.Collapsed) return;
            var record = (uiElement as FrameworkElement) != null ? (uiElement as FrameworkElement).DataContext : null;
            if (record == null && treeDataColumn.DataRow != null)
                record = treeDataColumn.DataRow.RowData;
            this.InitializeCellStyle(treeDataColumn, record);
        }

        private void SetCellStyle(TreeDataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var column = dataColumn.TreeGridColumn;
            Style newStyle = null;
            if (column == null)
                return;

            var gridCell = cell as TreeGridCell;
            if (gridCell == null) return;
            if (gridCell is TreeGridExpanderCell)
            {
                Style expanderstyle = null;

                if (TreeGrid.hasExpanderCellStyleSelector && TreeGrid.hasExpanderCellStyle)
                {
                    newStyle = TreeGrid.ExpanderCellStyleSelector.SelectStyle(record, cell);
                    expanderstyle = newStyle ?? TreeGrid.ExpanderCellStyle;                 
                }
                else if (TreeGrid.hasExpanderCellStyleSelector)
                {
                    expanderstyle = TreeGrid.ExpanderCellStyleSelector.SelectStyle(record, cell);
                }
                else if (TreeGrid.hasExpanderCellStyle)
                {
                    expanderstyle = TreeGrid.ExpanderCellStyle;
                }
                if (expanderstyle != null)
                    gridCell.Style = expanderstyle;
                else
                    gridCell.ClearValue(FrameworkElement.StyleProperty);
                return;
            }
            Style style = null;
            if (!column.hasCellStyleSelector && !column.hasCellStyle && !TreeGrid.hasCellStyle && !TreeGrid.hasCellStyleSelector)
            {
                if (gridCell.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue)
                    gridCell.ClearValue(FrameworkElement.StyleProperty);
                return;
            }
            else if (column.hasCellStyleSelector && column.hasCellStyle)
            {
                newStyle = column.CellStyleSelector.SelectStyle(record, cell);
                style = newStyle ?? column.CellStyle;             
            }
            else if (column.hasCellStyleSelector)
            {
                style = column.CellStyleSelector.SelectStyle(record, cell);
            }
            else if (column.hasCellStyle)
            {
                style = column.CellStyle;
            }
            else if (TreeGrid.hasCellStyleSelector && TreeGrid.hasCellStyle)
            {
                newStyle = TreeGrid.CellStyleSelector.SelectStyle(record, cell);
                style = newStyle ?? TreeGrid.CellStyle;             
            }
            else if (TreeGrid.hasCellStyleSelector)
            {
                style = TreeGrid.CellStyleSelector.SelectStyle(record, cell);
            }
            else if (TreeGrid.hasCellStyle)
            {
                style = TreeGrid.CellStyle;
            }

            if (style != null)
                gridCell.Style = style;
            else
                gridCell.ClearValue(FrameworkElement.StyleProperty);
        }
        #endregion
        /// <summary>
        /// Clears the recycle bin.
        /// </summary>
        public override void ClearRecycleBin()
        {
            DisplayRecycleBin.Clear();
            EditRecycleBin.Clear();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase"/> class.
        /// </summary> 
        protected override void Dispose(bool isDisposing)
        {
            if (this.DisplayRecycleBin != null)
            {
                this.DisplayRecycleBin.Clear();
                // this.DisplayRecycleBin = null; //AgunaCapital incident: 136598
            }
            base.Dispose(isDisposing);
        }

    }
}
