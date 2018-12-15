#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data.Extensions;
using Syncfusion.Data.Helper;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.Data;
#if UWP
using Windows.UI.Xaml.Data;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Core;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
#endif
namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
    using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
#else
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
#endif
    /// <summary>
    /// Represents a class that manages the current cell operation in SfTreeGrid.
    /// </summary>
    public class TreeGridCurrentCellManager : IDisposable
    {
        /// <summary>
        /// Gets or sets an instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> class.
        /// </summary>
        protected SfTreeGrid TreeGrid;
        internal object oldCellValue;
        internal RowColumnIndex previousRowColumnIndex;
        private bool setFocusForGrid = true;

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> class.
        /// </summary>
        /// <param name="grid">
        /// An instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> class.
        /// </param>
        public TreeGridCurrentCellManager(SfTreeGrid treeGrid)
        {
            this.TreeGrid = treeGrid;
            var rowColumnIndex = new RowColumnIndex(-1, -1);
            CurrentRowColumnIndex = rowColumnIndex;
            previousRowColumnIndex = rowColumnIndex;
        }

        /// <summary>
        /// Gets or sets the action that encapsulates the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController.SuspendUpdates"/> method.
        /// </summary>
        protected internal Action SuspendUpdates;

        /// <summary>
        /// Gets or sets the action that encapsulates the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController.ResumeUpdates"/> method.
        /// </summary>
        protected internal Action ResumeUpdates;

        private RowColumnIndex currentRowColumnIndex;

        /// <summary>
        /// Gets the current <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the row or cell that contains the current cell.
        /// </summary>
        public RowColumnIndex CurrentRowColumnIndex
        {
            get { return currentRowColumnIndex; }
            internal set { currentRowColumnIndex = value; }
        }

        /// <summary>
        /// Gets or sets the currently active cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataColumnBase"/> that represents the current cell. 
        /// Returns null if there is no currently active cell.
        /// </value>
        private TreeDataColumnBase _currentCell;
        public TreeDataColumnBase CurrentCell
        {
            get { return _currentCell; }
            set { _currentCell = value; }
        }

        /// <summary>
        /// Returns a value that indicates whether the SfTreeGrid contains the currently active cell.
        /// </summary>
        public bool HasCurrentCell
        {
            get { return CurrentCell != null && CurrentCell is TreeDataColumn; }
        }

        /// <summary>
        /// Handles the current cell selection when any of <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> performed in cell.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the mouse action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the cell.
        /// </param>        
        /// <returns>
        /// Returns <b>true</b> if the pointer operation should be handled on selection controller; otherwise, <b>false</b>.
        /// </returns>
        protected internal virtual bool HandlePointerOperation(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (TreeGrid.NavigationMode == NavigationMode.Row)
                return true;

            if (CurrentRowColumnIndex != rowColumnIndex && TreeGrid.AllowFocus(rowColumnIndex))
            {
                //When selection mode is multiple we have to perform selection, hence the MultipleSelection mode condition is added.
                bool cancelSelection =
                                        (CurrentRowColumnIndex.RowIndex == rowColumnIndex.RowIndex &&
                                        (this.TreeGrid.SelectionMode == GridSelectionMode.Single ||
                                        (!CanSelectRow(rowColumnIndex) && TreeGrid.SelectionMode != GridSelectionMode.Multiple)));

#if UWP
                var activationTrigger = ActivationTrigger.Mouse;
                if (args != null)
                    activationTrigger = SelectionHelper.ConvertPointerDeviceTypeToActivationTrigger(args.Pointer.PointerDeviceType);
                if (!ProcessCurrentCellSelection(rowColumnIndex, activationTrigger))
#else
                if (!ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Mouse))
#endif
                    return false;
                return !cancelSelection;
            }
            if (CurrentRowColumnIndex == rowColumnIndex)
            {
                //Clicking inside the edit element should not affect the selection
                if (HasCurrentCell && this.CurrentCell.IsEditing && TreeGrid.SelectionMode == GridSelectionMode.Multiple)
                    return false;
                if (CanSelectRow(rowColumnIndex) || TreeGrid.SelectionMode == GridSelectionMode.Multiple)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Processes the selection when the mouse point is tapped on the current cell. 
        /// </summary>
        /// <param name="e">
        /// Contains the data related to the tap interaction.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>    
        /// <remarks>
        /// This method invoked to begin edit the current cell when <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnTap"/>.
        /// </remarks>
        protected internal virtual void ProcessOnTapped(TappedEventArgs e, RowColumnIndex currentRowColumnIndex)
        {
            if (TreeGrid.EditTrigger == EditTrigger.OnTap && TreeGrid.AllowFocus(currentRowColumnIndex))
            {
                if (this.HasCurrentCell && !this.CurrentCell.IsEditing)
                    this.BeginEdit();
            }
            if (CurrentRowColumnIndex == currentRowColumnIndex)
            {
                ScrollInView(currentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the selection when the mouse point is double tapped on the current cell.
        /// </summary>
        /// <param name="e">
        /// Contains the data related to the double tap interaction.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>  
        /// <remarks>
        /// This method invoked to begin edit the cell when <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnDoubleTap"/>.
        /// </remarks>
        protected internal virtual void ProcessOnDoubleTapped()
        {
            if (CurrentCell == null || CurrentCell.Renderer == null)
                return;

            if (CurrentCell.Renderer.HasCurrentCellState && !CurrentCell.IsEditing)
            {
                this.BeginEdit();
            }
        }

        /// <summary>
        /// Method to raise current cell activating event and set current cell, raise current cell activated event.
        /// </summary>
        /// <param name="newRowColumnIndex">
        /// The corresponding rowcolumnindex to set the current cell.
        /// </param>
        /// <param name="activationTriggger">
        /// Indicates how the current cell is activated.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the current cell can be set on particular index; otherwise, <b>false</b>.
        /// </returns>
        protected internal virtual bool ProcessCurrentCellSelection(RowColumnIndex newRowColumnIndex, ActivationTrigger activationTriggger)
        {
            if (this.TreeGrid.NavigationMode == NavigationMode.Row)
                return false;
            var firstCellIndex = GetFirstCellIndex();
            if (newRowColumnIndex.ColumnIndex < firstCellIndex)
            {
                newRowColumnIndex.ColumnIndex = firstCellIndex;
            }
            if (newRowColumnIndex != CurrentRowColumnIndex)
            {
                if (RaiseCurrentCellActivatingEvent(CurrentRowColumnIndex, newRowColumnIndex, activationTriggger))
                    return false;
                if (activationTriggger == ActivationTrigger.Program || CurrentRowColumnIndex.RowIndex == newRowColumnIndex.RowIndex || (this.TreeGrid.SelectionMode == GridSelectionMode.Multiple && activationTriggger == ActivationTrigger.Keyboard))
                {
                    RemoveCurrentCell(CurrentRowColumnIndex);
                    SelectCurrentCell(newRowColumnIndex, activationTriggger != ActivationTrigger.Program);
                    RaiseCurrentCellActivatedEvent(newRowColumnIndex, previousRowColumnIndex, activationTriggger);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set CurrentRowColumnIndex, CurrentItem and RowHeader state.
        /// </summary>
        /// <param name="rowColumnIndex">rowColumnIndex for setting current row index.</param>
        protected internal void UpdateGridProperties(RowColumnIndex rowColumnIndex)
        {
            SetCurrentRowColumnIndex(rowColumnIndex);

            if (this.TreeGrid.NavigationMode == NavigationMode.Cell || this.TreeGrid.CurrentColumn != null)
                this.TreeGrid.CurrentColumn = this.TreeGrid.GetTreeGridColumn(rowColumnIndex.ColumnIndex);
            if (CurrentRowColumnIndex.RowIndex == previousRowColumnIndex.RowIndex)
                return;

            this.SuspendUpdates();
            if (this.TreeGrid.View != null)
            {
                if (rowColumnIndex.RowIndex >= 0)
                    this.TreeGrid.View.MoveCurrentToPosition(this.TreeGrid.ResolveToNodeIndex(rowColumnIndex.RowIndex));
                else
                    this.TreeGrid.View.MoveCurrentToPosition(-1);
            }
            else
                this.TreeGrid.CurrentItem = null;

            this.TreeGrid.UpdateRowHeaderState();
            this.ResumeUpdates();
        }

        /// <summary>
        /// Sets the current rowcolumnindex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The rowcolumnindex.
        /// </param>
        internal protected void SetCurrentRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex != currentRowColumnIndex)
                UpdatePreviousIndex();
            CurrentRowColumnIndex = rowColumnIndex;
        }

        internal void UpdatePreviousIndex()
        {
            previousRowColumnIndex = currentRowColumnIndex;
        }

        /// <summary>
        /// Initiates the edit operation on the current cell.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the current cell entering into edit mode; otherwise, <b>false</b> .
        /// </returns>  
        public bool BeginEdit()
        {
            if (!HasCurrentCell) return false;

            var currentCell = CurrentCell.ColumnElement as TreeGridCell;
            if (currentCell == null || CurrentCell.TreeGridColumn == null)
                return false;

            if (!CurrentCell.TreeGridColumn.CanEditCell() || !CurrentCell.TreeGridColumn.AllowEditing)
                return false;

            if (CurrentCell.Renderer == null || !CurrentCell.Renderer.IsEditable)
                return false;

            var dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == CurrentCell.RowIndex);
            if (dataRow == null)
                return false;

            if (!RaiseCurrentCellBeginEditEvent(new RowColumnIndex(CurrentCell.RowIndex, CurrentCell.ColumnIndex), CurrentCell.TreeGridColumn))
            {
                var isInEdit = CurrentCell.Renderer.BeginEdit(new RowColumnIndex(CurrentCell.RowIndex, CurrentCell.ColumnIndex), currentCell, CurrentCell.TreeGridColumn, currentCell.DataContext);
                if (isInEdit)
                {
                    CurrentCell.IsEditing = isInEdit;
                    dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == CurrentRowColumnIndex.RowIndex);
                    if (dataRow != null)
                    {
                        if (!TreeGrid.View.IsEditingItem || !TreeGrid.View.CurrentEditItem.Equals(dataRow.RowData))
                            TreeGrid.View.EditItem(dataRow.RowData);
                        dataRow.IsEditing = isInEdit;
                        if (CurrentCell.Renderer.CanValidate())
                        {
                            TreeGrid.ValidationHelper.SetCurrentCellValidated(!isInEdit);
                        }
                        TreeGrid.ValidationHelper.SetCurrentRowValidated(!isInEdit);
                        dataRow.ApplyRowHeaderVisualState();
                    }
                }
                return isInEdit;
            }
            return false;
        }

        /// <summary>
        /// Ends the edit operation on the current cell.
        /// </summary>
        /// <param name="canCommit">
        /// Specifies whether the value can be committed to the current cell.
        /// </param>      
        /// <returns>
        /// Returns <b>true</b> if the edit operation is ended; otherwise, <b>false</b> if validation fails, false will be returned.
        /// </returns>
        public bool EndEdit(bool canCommit = true)
        {
            //Need to validate, if CurrentCell is not validated before, when calling from Application
            if (!TreeGridValidationHelper.IsCurrentCellValidated)
            {
                return this.RaiseCellValidationAndEndEdit(canCommit);
            }

            var datarow = TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.IsEditing);

            if (!HasCurrentCell) return true;
            var currentCell = CurrentCell.ColumnElement as TreeGridCell;
            if (currentCell == null)
                return true;
            if (CurrentCell.Renderer != null && CurrentCell.Renderer.IsEditable)
            {
                if (CurrentCell.IsEditing)
                {
                    var currentDataColumn = this.CurrentCell;
                    var isInEdit = CurrentCell.Renderer.EndEdit(this.CurrentCell, currentCell.DataContext);
                    if (isInEdit)
                    {
                        if (datarow != null)
                            datarow.IsEditing = !isInEdit;
                        if (currentDataColumn != null)
                            currentDataColumn.IsEditing = !isInEdit;

                        if (canCommit && TreeGrid.HasView && TreeGrid.View.IsEditingItem)
                            TreeGrid.View.CommitEdit();
                        RaiseCurrentCellEndEditEvent(currentDataColumn != null ? new RowColumnIndex(currentDataColumn.RowIndex, currentDataColumn.ColumnIndex) : CurrentRowColumnIndex);
                        if (datarow != null)
                            (datarow as TreeDataRowBase).ApplyRowHeaderVisualState();
                    }
                }
                else if (TreeGrid.HasView && TreeGrid.View.IsEditingItem)
                {
                    if (canCommit)
                        TreeGrid.View.CommitEdit();
                }
            }
            else if (TreeGrid.HasView && TreeGrid.View.IsEditingItem)
            {
                if (canCommit)
                    TreeGrid.View.CommitEdit();
            }
            return true;
        }

        internal void SetCurrentRowIndex(int rowIndex)
        {
            UpdatePreviousIndex();
            currentRowColumnIndex.RowIndex = rowIndex;
        }
        /// <summary>
        /// Scrolls the SfTreeGrid vertically and horizontally to display a cell in view.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the rowColumnIndex of the cell to bring into view.
        /// </param>
        protected internal void ScrollInView(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex.RowIndex < 0)
                return;
            var visibleRowLines = this.TreeGrid.TreeGridPanel.ScrollRows.GetVisibleLines();
            var rowLineInfo = this.TreeGrid.TreeGridPanel.ScrollRows.GetVisibleLineAtLineIndex(rowColumnIndex.RowIndex);
            bool isScrolled = false;

            if (visibleRowLines.FirstBodyVisibleIndex < visibleRowLines.Count)
            {
                if (rowColumnIndex.RowIndex < visibleRowLines[visibleRowLines.FirstBodyVisibleIndex].LineIndex || rowColumnIndex.RowIndex > visibleRowLines[visibleRowLines.LastBodyVisibleIndex].LineIndex || (rowLineInfo != null && rowLineInfo.IsClipped))
                {
                    this.TreeGrid.TreeGridPanel.ScrollRows.ScrollInView(rowColumnIndex.RowIndex);
                    isScrolled = true;
                }
            }

            if (rowColumnIndex.ColumnIndex < 0)
                return;

            var visibleColumnLines = this.TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines();
            var columnLineInfo = this.TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(rowColumnIndex.ColumnIndex);

            if (visibleColumnLines.FirstBodyVisibleIndex < visibleColumnLines.Count)
            {
                if (rowColumnIndex.ColumnIndex < visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex].LineIndex || rowColumnIndex.ColumnIndex > visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex || (columnLineInfo != null && columnLineInfo.IsClipped))
                {
                    this.TreeGrid.TreeGridPanel.ScrollColumns.ScrollInView(rowColumnIndex.ColumnIndex);
                    isScrolled = true;
                }
            }

            if (isScrolled)
                this.TreeGrid.TreeGridPanel.InvalidateMeasureInfo();
        }

        /// <summary>
        /// Handles the current cell selection when the columns is added or removed at run time.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the collection changed action in columns collection.
        /// </param>
        protected internal virtual void HandleColumnsCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CurrentCell != null)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        ProcessColumnRemoveAndInsert(args.NewItems.Count > 0 && args.NewItems[0] != null ? (TreeGridColumn)args.NewItems[0] : null, TreeGrid.ResolveToScrollColumnIndex(args.NewStartingIndex), NotifyCollectionChangedAction.Add);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        ProcessColumnRemoveAndInsert(args.OldItems.Count > 0 && args.OldItems[0] != null ? (TreeGridColumn)args.OldItems[0] : null, TreeGrid.ResolveToScrollColumnIndex(args.OldStartingIndex), NotifyCollectionChangedAction.Remove);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        RemoveCurrentCell(CurrentRowColumnIndex);
                        SelectCurrentCell(currentRowColumnIndex);
                        break;
                }
            }
            else
            {
                // UWP-3376 - If expander cell is current cell, to reuse the expander cell, we remove current cell. So Current ColumnIndex needs to be updated here.
                bool needToChangeCurrentColumnIndex = this.CurrentCell == null && this.CurrentRowColumnIndex.RowIndex != -1 && this.CurrentRowColumnIndex.ColumnIndex != -1;
                if (needToChangeCurrentColumnIndex)
                {
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            ProcessExpanderColumnCurrentCell(args.NewItems.Count > 0 && args.NewItems[0] != null ? (TreeGridColumn)args.NewItems[0] : null, TreeGrid.ResolveToScrollColumnIndex(args.NewStartingIndex), NotifyCollectionChangedAction.Add);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            ProcessExpanderColumnCurrentCell(args.OldItems.Count > 0 && args.OldItems[0] != null ? (TreeGridColumn)args.OldItems[0] : null, TreeGrid.ResolveToScrollColumnIndex(args.OldStartingIndex), NotifyCollectionChangedAction.Remove);
                            break;
                    }
                }
            }
            if (args.Action == NotifyCollectionChangedAction.Move)
            {
#if UWP
                bool needToMove = this.CurrentCell == null && this.CurrentRowColumnIndex.RowIndex != -1 && this.CurrentRowColumnIndex.ColumnIndex != -1;
#else
                bool needToMove = this.CurrentCell == null && this.CurrentRowColumnIndex.RowIndex != -1 && this.CurrentRowColumnIndex.ColumnIndex != -1 && SfTreeGrid.suspendForColumnMove;
#endif
                if (needToMove || this.CurrentCell != null)
                {
                    this.ProcessColumnPositionChanged(args, needToMove);
                }
            }
        }

        /// <summary>
        /// Processes the current cell when column added or removed at runtime.
        /// </summary>
        /// <param name="changedColumn">
        /// Contains the corresponding changed column.
        /// </param>
        /// <param name="changedIndex">
        /// The corresponding index of the column.
        /// </param>
        /// <param name="action">
        /// Corresponding collection changed action performed on columns.
        /// </param>
        protected void ProcessColumnRemoveAndInsert(TreeGridColumn changedColumn, int changedIndex, NotifyCollectionChangedAction action)
        {
            int nextCellIndex = -1;
            if (changedColumn != null && CurrentCell.TreeGridColumn != null && changedColumn.MappingName == CurrentCell.TreeGridColumn.MappingName)
            {
                if (action == NotifyCollectionChangedAction.Remove)
                {
                    RemoveCurrentCell(CurrentRowColumnIndex);
                    nextCellIndex = this.GetFirstCellIndex();
                    SelectCurrentCell(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, nextCellIndex));
                }
#if WPF
                else if (action == NotifyCollectionChangedAction.Add)
                {
                    nextCellIndex = changedIndex;
                    RemoveCurrentCell(CurrentRowColumnIndex);
                    SelectCurrentCell(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, nextCellIndex));
                }
#endif
            }
            else
            {
                if (changedIndex <= this.CurrentRowColumnIndex.ColumnIndex)
                    nextCellIndex = action == NotifyCollectionChangedAction.Add ? CurrentRowColumnIndex.ColumnIndex + 1 : CurrentRowColumnIndex.ColumnIndex - 1;
            }
            if (nextCellIndex != -1)
                this.SetCurrentColumnIndex(nextCellIndex);
        }

        /// <summary>
        /// Processs the expander cell current cell when column position is changed.
        /// </summary>
        /// <param name="changedColumn">changed column.</param>
        /// <param name="changedIndex">corresponding column index.</param>
        /// <param name="action">action.</param>
        internal void ProcessExpanderColumnCurrentCell(TreeGridColumn changedColumn, int changedIndex, NotifyCollectionChangedAction action)
        {
            int nextCellIndex = -1;
            if (changedIndex <= this.CurrentRowColumnIndex.ColumnIndex)
                nextCellIndex = action == NotifyCollectionChangedAction.Add ? CurrentRowColumnIndex.ColumnIndex + 1 : CurrentRowColumnIndex.ColumnIndex - 1;

            if (nextCellIndex != -1)
                this.SetCurrentColumnIndex(nextCellIndex);
        }

        private void ProcessColumnPositionChanged(NotifyCollectionChangedEventArgs args, bool needToMove)
        {
            int nextCellIndex = -1;

            int oldColumnIndex = TreeGrid.ResolveToScrollColumnIndex(args.OldStartingIndex);

            if (oldColumnIndex > CurrentRowColumnIndex.ColumnIndex && TreeGrid.ResolveToScrollColumnIndex(args.NewStartingIndex) <= CurrentRowColumnIndex.ColumnIndex)
                nextCellIndex = CurrentRowColumnIndex.ColumnIndex + 1;
            else if (TreeGrid.ResolveToScrollColumnIndex(args.NewStartingIndex) >= CurrentRowColumnIndex.ColumnIndex && oldColumnIndex < CurrentRowColumnIndex.ColumnIndex)
                nextCellIndex = CurrentRowColumnIndex.ColumnIndex - 1;
            else if (needToMove || ((TreeGridColumn)args.OldItems[0]).MappingName == CurrentCell.TreeGridColumn.MappingName)
            {
                nextCellIndex = TreeGrid.ResolveToScrollColumnIndex(args.NewStartingIndex);
                RemoveCurrentCell(CurrentRowColumnIndex);
                SelectCurrentCell(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, nextCellIndex));
            }
            if (nextCellIndex != -1)
                SetCurrentColumnIndex(nextCellIndex);
        }

        internal void SetCurrentColumnIndex(int columnIndex)
        {
            UpdatePreviousIndex();
            currentRowColumnIndex.ColumnIndex = columnIndex;

            if (!this.HasCurrentCell)
                return;

            // Update CurrentCellIndex of TreeGridCellRendererBase.
            if (this.CurrentCell.Renderer != null && (this.CurrentCell.Renderer as TreeGridCellRendererBase).CurrentCellIndex.ColumnIndex != columnIndex)
                this.CurrentCell.Renderer.SetCurrentCellState(CurrentRowColumnIndex, this.CurrentCell.ColumnElement, this.CurrentCell.IsEditing, false, this.CurrentCell.TreeGridColumn, this.CurrentCell);
        }
        /// <summary>
        /// Removes the current cell based on the specified row and column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to remove the current cell.
        /// </param>
        /// <remarks>
        /// This method helps to remove the current cell corresponding to the specified row and column index.
        /// </remarks>
        protected internal void RemoveCurrentCell(RowColumnIndex rowColumnIndex)
        {
            if (CurrentCell == null)
                return;
            CurrentCell.IsCurrentCell = false;
            if (CurrentCell.Renderer == null)
                return;
            var dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.IsEditing);
            if (CurrentCell.IsEditing)
            {
                CurrentCell.Renderer.EndEdit(CurrentCell, CurrentCell.ColumnElement.DataContext, true);
                if (TreeGrid.View != null)
                    TreeGrid.View.CommitEdit();
                CurrentCell.IsEditing = false;
                dataRow.IsEditing = false;
                oldCellValue = null;
            }
            CurrentCell.Renderer.ResetCurrentCellState();
            CurrentCell = null;
        }
        internal void ResetCurrentRowColumnIndex()
        {
            var rowColumnIndex = new RowColumnIndex(-1, -1);
            UpdateGridProperties(rowColumnIndex);
            this.TreeGrid.CurrentItem = null;
            this.previousRowColumnIndex = rowColumnIndex;
        }

        /// <summary>
        /// Determines whether the selection can be processed for row corresponding to the specified row and column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index to perform row selection.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the row selection can be processed; otherwise, <b>false</b>.
        /// </returns>
        internal bool CanSelectRow(RowColumnIndex rowColumnIndex)
        {
            return (TreeGrid.SelectionMode == GridSelectionMode.Extended && (SelectionHelper.CheckControlKeyPressed() || (this.TreeGrid.SelectionController.SelectedRows.Count > 1 && !SelectionHelper.CheckShiftKeyPressed())))
                || !this.TreeGrid.SelectionController.SelectedRows.Any(item => item.RowIndex == rowColumnIndex.RowIndex);
        }

        internal bool CanMoveCurrentCell(RowColumnIndex rowColumnIndex)
        {
            if (TreeGrid.NavigationMode == NavigationMode.Row || TreeGrid.SelectionMode == GridSelectionMode.None || !TreeGrid.AllowFocus(rowColumnIndex) || CurrentRowColumnIndex == rowColumnIndex)
                return false;

            //The Below condition is used to cancel the MoveCurrentCell process when the value RowIndex value is Greater than the LastRowIndex value or the ColumnIndex value is Greater than the LastCell Index value.
            if ((rowColumnIndex.RowIndex > -1 && rowColumnIndex.RowIndex < this.TreeGrid.GetFirstDataRowIndex())
                || (rowColumnIndex.ColumnIndex > -1 && rowColumnIndex.ColumnIndex < this.TreeGrid.GetFirstColumnIndex())
                || rowColumnIndex.ColumnIndex > this.TreeGrid.GetLastColumnIndex() || rowColumnIndex.RowIndex > this.TreeGrid.GetLastDataRowIndex())
                throw new InvalidOperationException("InValid RowColumnIndex");
            if (!TreeGrid.ValidationHelper.RaiseCellValidate(CurrentRowColumnIndex) || CurrentRowColumnIndex.RowIndex != rowColumnIndex.RowIndex && !TreeGrid.ValidationHelper.RaiseRowValidate(CurrentRowColumnIndex))
                return false;
            return true;
        }
        /// <summary>
        /// Commits the value in the underlying object while calling end edit method.
        /// </summary>
        /// <param name="isNewValue">if it true, currently edited value(renderer value) will be set to the underlying object.
        /// Else, old cell value will be set.</param>
        internal void CommitCellValue(bool isNewValue)
        {
            var dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == this.CurrentRowColumnIndex.RowIndex);
            if (dataRow == null || (!dataRow.IsEditing && !TreeGrid.HasView)) return;

            var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.ColumnIndex == this.CurrentRowColumnIndex.ColumnIndex);
            if (dataColumn == null || !dataColumn.IsEditing) return;
            object cellValue;
            if (isNewValue)
                cellValue = dataColumn.Renderer.GetControlValue();
            else
                cellValue = oldCellValue;
            if (dataColumn.TreeGridColumn.IsTemplate)
                return;
            if (!dataColumn.TreeGridColumn.isvaluebindingcreated && dataColumn.TreeGridColumn.ValueBinding != null)
                cellValue = TreeGridPropertiesProvider.GetConvertBackValue(dataColumn.TreeGridColumn.ValueBinding as Binding, cellValue);
            (TreeGrid.View.propertyAccessProvider as TreeGridPropertiesProvider).CommitValue(dataRow.RowData, dataColumn.TreeGridColumn.MappingName, cellValue);          
        }
        /// <summary>
        /// Raises Cell Validation event and EndEdit.
        /// </summary>
        /// <param name="canCommit">
        /// Specifies whether the value can be committed to the current cell.
        /// </param>      
        /// Returns <b>true</b> if the current cell validation is successful and the cell is allowed for end edit operation;
        /// otherwise, <b>false</b>.
        /// </returns> 
        internal protected bool RaiseCellValidationAndEndEdit(bool canCommit = false)
        {
            if (this.HasCurrentCell && (this.CurrentCell.IsEditing || this.CurrentCell.Renderer is TreeGridCellCheckBoxRenderer))
            {
                if (!(this.CurrentCell.Renderer is TreeGridCellCheckBoxRenderer) && !TreeGrid.ValidationHelper.RaiseCellValidate(CurrentRowColumnIndex))
                    return false;

                EndEdit(canCommit);
            }
            return true;
        }

        /// <summary>
        /// Check cell and row validation and end edit.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the current cell validation is successful and the cell is allowed for end edit operation;
        /// otherwise, <b>false</b>.
        /// </returns>       
        internal bool CheckValidationAndEndEdit()
        {
            if (!TreeGrid.ValidationHelper.CheckForValidation(true))
                return false;

            if (this.HasCurrentCell && (this.CurrentCell.IsEditing || this.CurrentCell.Renderer is TreeGridCellCheckBoxRenderer
                || (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)))
                EndEdit();
            return true;
        }


        /// <summary>
        /// Set current cell at the specified row column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to set the current cell.
        /// </param>
        /// <param name="setFocus">
        /// Decides whether the focus is set to current cell.
        /// </param>
        protected internal void SelectCurrentCell(RowColumnIndex rowColumnIndex, bool setFocus = true)
        {
            var dataColumn = this.TreeGrid.GetTreeDataColumnBase(rowColumnIndex);
            this.setFocusForGrid = setFocus;

            if (dataColumn != null && !(rowColumnIndex.RowIndex < 0) && this.TreeGrid.NavigationMode == NavigationMode.Cell)
            {
                dataColumn.IsCurrentCell = true;
                if (dataColumn.Renderer != null)
                {
                    dataColumn.Renderer.SetCurrentCellState(rowColumnIndex, dataColumn.ColumnElement, dataColumn.IsEditing, setFocusForGrid, dataColumn.TreeGridColumn, dataColumn);
                    SetOldCellValue(dataColumn, rowColumnIndex);
                    setFocusForGrid = false;
                }
                this.CurrentCell = dataColumn;
            }

            if (dataColumn == null && setFocus)

#if UWP
                this.TreeGrid.Focus(FocusState.Programmatic);
#else
                this.TreeGrid.Focus();
#endif
            this.UpdateGridProperties(rowColumnIndex);

        }

        /// <summary>
        /// Set old cell value while selecting the current cell.
        /// </summary>
        /// <param name="dataColumn">corresponding dataColumn.</param>
        /// <param name="rowColumnIndex">specified row column index to get the cell value and set oldCellValue.</param>
        private void SetOldCellValue(TreeDataColumnBase dataColumn, RowColumnIndex rowColumnIndex)
        {
            var dataRow = this.TreeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowColumnIndex.RowIndex);
            if (dataRow != null && dataRow.RowType == TreeRowType.DefaultRow && dataColumn.TreeGridColumn != null)
            {
                if (dataColumn.TreeGridColumn.MappingName != null)
                    oldCellValue = this.TreeGrid.View.GetPropertyAccessProvider().GetValue(dataRow.RowData, dataColumn.TreeGridColumn.MappingName, true);
            }
        }

        /// <summary>
        /// Set current cell state and old cell value.
        /// </summary>
        /// <param name="column">specified data column base.</param>
        /// <param name="setFocus">whether need to set focus or not.</param>
        internal void SetCurrentColumnBase(TreeDataColumnBase column, bool setFocus)
        {
            CurrentCell = column;
            if (CurrentCell.Renderer != null && !CurrentCell.Renderer.HasCurrentCellState && !CurrentCell.IsEditing)
            {
                CurrentCell.Renderer.SetCurrentCellState(TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex, CurrentCell.ColumnElement, CurrentCell.IsEditing, setFocusForGrid, CurrentCell.TreeGridColumn, column);
                SetOldCellValue(column, CurrentRowColumnIndex);
            }
            setFocusForGrid = false;
        }

        /// <summary>
        /// Handles the selection for the keyboard interactions that are performed current cell.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled by selection controller; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool HandleKeyDown(KeyEventArgs args)
        {
            if (!HasCurrentCell || CurrentCell.Renderer == null)
                return true;
            if (!CurrentCell.Renderer.ShouldGridTryToHandleKeyDown(args))
            {
#if UWP
                if (char.IsLetterOrDigit(args.Key.ToString(), 0) && this.TreeGrid.AllowEditing)
                {
                    if (SelectionHelper.CheckControlKeyPressed())
                        return false;
                    if (!(args.Key >= Key.A && args.Key <= Key.Z) && !(args.Key >= Key.Number0 && args.Key <= Key.Number9) && !(args.Key >= Key.NumberPad0 && args.Key <= Key.NumberPad9))
                    {
                        if (args.Key == Key.Up || args.Key == Key.Down || args.Key == Key.Home || args.Key == Key.End || args.Key == Key.Left || args.Key == Key.Right)
                            args.Handled = true;  // Fix for Scrolling happens when press down or up key in Edited cell.
                        return false;
                    }
                    if (CurrentCell.Renderer != null && CurrentCell.TreeGridColumn != null)// && !(CurrentCell.TreeGridColumn.IsTemplate))
                    {
                        if (!CurrentCell.IsEditing)
                        {
                            if (BeginEdit())
                                CurrentCell.Renderer.PreviewTextInput(args);
                        }
                    }
                }
#endif
                return false;
            }

            return true;
        }


        /// <summary>
        /// Handles the selection when the key navigation is processed on the current cell.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index where the key navigation occurs.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the navigation should be processed by selection controller; otherwise , <b>false</b>.
        /// </returns>
        /// <remarks>
        /// Override this method , to customize navigation behavior of current cell in SfTreeGrid.
        /// </remarks>
        protected internal virtual bool ProcessKeyDown(KeyEventArgs args, RowColumnIndex rowColumnIndex)
        {
            Key currentKey = args.Key;
            if (TreeGrid.FlowDirection == FlowDirection.RightToLeft)
                (this.TreeGrid.SelectionController as TreeGridBaseSelectionController).ChangeFlowDirectionKey(ref currentKey);
            int rowIndex, columnIndex;
            switch (currentKey)
            {
                case Key.Home:
                case Key.End:
                case Key.Enter:
                case Key.Up:
                case Key.Down:
                case Key.PageDown:
                case Key.PageUp:
                    {
                        if (rowColumnIndex != this.CurrentRowColumnIndex)
                        {
                            if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                            {
                                if (currentKey == Key.Home || currentKey == Key.End)
                                {
                                    if (rowColumnIndex.ColumnIndex >= this.TreeGrid.GetFirstColumnIndex() && !this.TreeGrid.AllowFocus(rowColumnIndex))
                                        return false;
                                    if (ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                                    {
                                        // WPF-34083 - If SelectionMode is Multiple, CurrentRowColumnIndex is updated in ProcessCurrentCellSelection method itself. To find whether current cell is moved in same row, condition check based on previousRowColumnIndex is added here.
                                        if (rowColumnIndex.RowIndex == CurrentRowColumnIndex.RowIndex && previousRowColumnIndex.RowIndex == CurrentRowColumnIndex.RowIndex)
                                        {
                                            this.ScrollInView(CurrentRowColumnIndex.ColumnIndex);
                                            return false;
                                        }
                                    }

                                }
                                else if ((rowColumnIndex.ColumnIndex >= this.TreeGrid.GetFirstColumnIndex() && !this.TreeGrid.AllowFocus(rowColumnIndex)) || !ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                                    return false;
                            }
                            return true;
                        }
                        return false;
                    }
                case Key.F2:
                    {
                        if (CurrentCell != null && CurrentCell.IsEditing)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() || !RaiseCellValidationAndEndEdit())
                                return false;
                        }
                        else if (CurrentCell != null)
                        {
                            BeginEdit();
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.Right:
                    {
                        if (!RaiseCellValidationAndEndEdit())
                            return false;
                        rowIndex = CurrentRowColumnIndex.RowIndex;
                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = GetLastCellIndex();
                        else
                            columnIndex = this.GetNextCellIndex();

                        if (CurrentRowColumnIndex.ColumnIndex == columnIndex)
                            return false;
                        rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                        if (!HandleCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                            return false;
                        args.Handled = true;
                    }
                    break;
                case Key.Left:
                    {
                        if (!RaiseCellValidationAndEndEdit())
                            return false;
                        rowIndex = CurrentRowColumnIndex.RowIndex;
                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = GetFirstCellIndex();
                        else
                            columnIndex = this.GetPreviousCellIndex();

                        if (CurrentRowColumnIndex.ColumnIndex == columnIndex)
                        {
                            if (columnIndex == this.GetFirstCellIndex())
                                this.ScrollInView(columnIndex);
                            return false;
                        }
                        rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                        if (!HandleCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                            return false;
                    }
                    break;
                case Key.Escape:
                    if (TreeGrid.NavigationMode == NavigationMode.Cell)
                    {
                        TreeGrid.ValidationHelper.ResetValidations(false);
                        if (HasCurrentCell && CurrentCell.IsEditing)
                        {
                            CommitCellValue(false);
                            //Validate current cell and update validation messages when press Esc key
                            UpdateValidationMessagesForCurrentCell();
                            EndEdit(false);
                            args.Handled = true;
                        }
                        else
                        {
                            if (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)
                            {
                                TreeGrid.View.CancelEdit();
                                TreeGrid.ValidationHelper.SetCurrentCellValidated(true);
                                TreeGrid.ValidationHelper.SetCurrentRowValidated(true);
                                args.Handled = true;
                            }
                        }
                        if (TreeGridValidationHelper.IsCurrentCellValidated && TreeGridValidationHelper.IsCurrentRowValidated)
                        {
                            var dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == CurrentRowColumnIndex.RowIndex);
                            this.TreeGrid.ValidationHelper.RemoveError(dataRow, false);
                        }
                        else if (TreeGridValidationHelper.IsCurrentCellValidated && HasCurrentCell && (this.CurrentCell.ColumnElement is TreeGridCell))
                            (this.CurrentCell.ColumnElement as TreeGridCell).RemoveError(false);

                    }
                    break;
                case Key.Tab:
                    {
                        var previousCellIndex = GetPreviousCellIndex();
                        var firstCellIndex = GetFirstCellIndex();
                        var nextCellIndex = GetNextCellIndex();
                        var lastCellIndex = GetLastCellIndex();
                        rowIndex = CurrentRowColumnIndex.RowIndex;
                        var getPrevious = (SelectionHelper.CheckShiftKeyPressed() && TreeGrid.FlowDirection == FlowDirection.LeftToRight) || (!SelectionHelper.CheckShiftKeyPressed() && TreeGrid.FlowDirection == FlowDirection.RightToLeft);
                        if (!RaiseCellValidationAndEndEdit(false))
                        {
                            args.Handled = true;
                            return false;
                        }
                        if (getPrevious)
                            columnIndex = previousCellIndex;
                        else
                            columnIndex = nextCellIndex;
                        if ((columnIndex == firstCellIndex && CurrentRowColumnIndex.ColumnIndex == columnIndex) || (columnIndex == lastCellIndex && CurrentRowColumnIndex.ColumnIndex == columnIndex))
                            return false;

                        rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                        if (!HandleCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                        {
                            args.Handled = true;
                            return false;
                        }
                        args.Handled = true;
                    }
                    break;
            }
            return args.Handled;
        }

        /// <summary>
        /// Validate current cell and update validation messages when press Esc key
        /// </summary>
        private void UpdateValidationMessagesForCurrentCell()
        {
            var dataRow = TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentRowColumnIndex.RowIndex && currentRowColumnIndex.RowIndex != -1);
            if (dataRow == null)
                return;
            if (CurrentCell == null)
                return;
            if (CurrentCell.TreeGridColumn.GridValidationMode != GridValidationMode.None)
            {
                this.TreeGrid.ValidationHelper.ValidateColumn(dataRow.RowData, CurrentCell.TreeGridColumn.MappingName, (CurrentCell.ColumnElement as TreeGridCell), currentRowColumnIndex);
            }
        }
        /// <summary>
        /// Method to set the current cell selection commonly in keyboard action.
        /// </summary>
        /// <param name="rowColumnIndex">rowColumnIndex to set the current cell.</param>
        /// <param name="activationTrigger">activationTrigger for setting current cell.</param>
        /// <returns>Returns <b>true</b> if the current cell is set to particular row column index.; otherwise, <b>false</b>.</returns>
        private bool HandleCurrentCellSelection(RowColumnIndex rowColumnIndex, ActivationTrigger activationTrigger)
        {
            if (rowColumnIndex.ColumnIndex != -1)
            {
                if (ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                {
                    this.ScrollInView(CurrentRowColumnIndex.ColumnIndex);
                }
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Set current cell and raise CurrentCellActivated event.
        /// </summary>
        /// <param name="rowColumnIndex">rowColumnIndex.</param>
        /// <param name="previousRowColumnIndex">previousRowColumnIndex.</param>
        /// <param name="activationTrigger">activationTrigger.</param>
        internal void UpdateCurrentCell(RowColumnIndex rowColumnIndex, RowColumnIndex previousRowColumnIndex, ActivationTrigger activationTrigger)
        {
            RemoveCurrentCell(previousRowColumnIndex);
            SelectCurrentCell(rowColumnIndex);
            RaiseCurrentCellActivatedEvent(rowColumnIndex, previousRowColumnIndex, activationTrigger);
        }

        /// <summary>
        /// Scrolls the specified column index in to view from the right direction to the SfTreeGrid.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to scroll the column into view.
        /// </param>  
        /// <remarks>
        /// This method helps to scroll the column into view if it is not present in the view area of SfTreeGrid.
        /// </remarks>
        public void ScrollInViewFromRight(int columnIndex)
        {
            ScrollInView(columnIndex);
        }

        /// <summary>
        /// Scrolls the specified column index in to view.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to scroll the column into view.
        /// </param>  
        /// <remarks>
        /// This method helps to scroll the column into view if it is not present in the view area of SfTreeGrid.
        /// </remarks>
        internal void ScrollInView(int columnIndex)
        {
            VisibleLinesCollection lineCollection = TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines();
            VisibleLineInfo lineInfo = TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);
            int firstBodyVisibleIndex = -1;
            if (lineCollection.FirstBodyVisibleIndex < lineCollection.Count)
                firstBodyVisibleIndex = lineCollection[lineCollection.FirstBodyVisibleIndex].LineIndex;

            //Here we checking whether the column is not in visible or clipped
            if (columnIndex >= 0 && (firstBodyVisibleIndex >= 0 && columnIndex < firstBodyVisibleIndex) || columnIndex > TreeGrid.TreeGridPanel.ScrollColumns.LastBodyVisibleLineIndex || lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                TreeGrid.TreeGridPanel.ScrollColumns.ScrollInView(columnIndex);
                TreeGrid.TreeGridPanel.InvalidateMeasureInfo();
            }
        }

        /// <summary>
        /// Scrolls the specified column index in to view from the left direction to the SfTreeGrid.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to scroll the column into view.
        /// </param>  
        /// <remarks>
        /// This method helps to scroll the column into view when the column is not present in the view area of SfTreeGrid.
        /// </remarks>
        public void ScrollInViewFromLeft(int columnIndex)
        {
            ScrollInView(columnIndex);
        }

        /// <summary>
        /// Gets the index of first focused cell in SfTreeGrid for the specified flow direction.
        /// </summary>
        /// <param name="direction">
        /// Contains the direction to get first cell index in SfTreeGrid.
        /// </param>
        /// <returns>
        /// The first cell index in SfTreeGrid for the specified flow direction.
        /// </returns>
        protected internal int GetFirstCellIndex(FlowDirection direction = FlowDirection.LeftToRight)
        {
            if (direction == FlowDirection.RightToLeft)
                return this.GetLastCellIndex();
            int firstColumnIndex = TreeGrid.Columns.IndexOf(TreeGrid.Columns.FirstOrDefault(col => (col.AllowFocus && (!col.IsHidden || (col.IsHidden && col == TreeGrid.TreeGridColumnSizer.GetExpanderColumn())) && col.ActualWidth != 0d)));
            firstColumnIndex = TreeGrid.ResolveToScrollColumnIndex(firstColumnIndex);
            return firstColumnIndex;
        }
        /// <summary>
        /// Gets the index of the last focused cell corresponding to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The corresponding direction of the cell to get its index.
        /// </param>
        /// <returns>
        /// Returns the corresponding index of last focused cell.
        /// </returns>
        protected internal int GetLastCellIndex(FlowDirection direction = FlowDirection.LeftToRight)
        {
            if (direction == FlowDirection.RightToLeft)
                return this.GetFirstCellIndex();
            int lastIndex = TreeGrid.Columns.IndexOf(TreeGrid.Columns.LastOrDefault(col => (col.AllowFocus && (!col.IsHidden || (col.IsHidden && col == TreeGrid.TreeGridColumnSizer.GetExpanderColumn())) && col.ActualWidth != 0d)));
            lastIndex = TreeGrid.ResolveToScrollColumnIndex(lastIndex);
            return lastIndex;
        }


        /// <summary>
        /// Gets the index of the next focused cell corresponding to the specified direction.
        /// </summary>
        /// <param name="flowdirection">
        /// The corresponding direction to get the index of next focused cell.
        /// </param>
        /// <returns>
        /// Returns the index of next focused cell.
        /// </returns>
        protected internal int GetNextCellIndex(FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return this.GetPreviousCellIndex();
            int nextCellIndex = CurrentRowColumnIndex.ColumnIndex;
            int lastCellIndex = TreeGrid.GetLastColumnIndex();

            TreeGridColumn column = this.GetNextFocusableTreeGridColumn(nextCellIndex + 1, MoveDirection.Right);
            if (column != null)
            {
                nextCellIndex = TreeGrid.ResolveToScrollColumnIndex(TreeGrid.Columns.IndexOf(column));
            }

            nextCellIndex = (nextCellIndex > lastCellIndex) ? lastCellIndex : nextCellIndex;

            return nextCellIndex;
        }

        /// <summary>
        /// Gets the next focused grid column for the specified column index and direction.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to get the next focused column.
        /// </param>
        /// <param name="direction">
        /// Specifies flow direction to get the next focused column.
        /// </param>
        /// <returns>
        /// The next focused <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn"/> for specified column index and direction. 
        /// Returns <b>null</b>, if the specified column index is last column.
        /// </returns>
        internal TreeGridColumn GetNextFocusableTreeGridColumn(int columnIndex, MoveDirection direction)
        {
            var resolvedIndex = TreeGrid.ResolveToGridVisibleColumnIndex(columnIndex);
            if (resolvedIndex < 0 || resolvedIndex >= TreeGrid.Columns.Count)
                return null;

            var gridColumn = TreeGrid.Columns[resolvedIndex];

            if (gridColumn == null || !gridColumn.AllowFocus || gridColumn.ActualWidth == 0.0 || double.IsNaN(gridColumn.ActualWidth))
            {
                gridColumn = this.GetNextFocusableTreeGridColumn(direction == MoveDirection.Right ? columnIndex + 1 : columnIndex - 1, direction);
            }
            return gridColumn;
        }

        /// <summary>
        /// Gets the index of previous cell corresponding to the specified flow direction.
        /// </summary>
        /// <param name="flowdirection">
        /// The corresponding direction to get previous cell index.
        /// </param>
        /// <returns>
        /// Returns the index of previous cell.
        /// </returns>
        protected internal int GetPreviousCellIndex(FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return this.GetNextCellIndex();
            int previousCellIndex = CurrentRowColumnIndex.ColumnIndex;

            TreeGridColumn column = GetNextFocusableTreeGridColumn(previousCellIndex - 1, MoveDirection.Left);
            if (column != null)
            {
                previousCellIndex = TreeGrid.ResolveToScrollColumnIndex(TreeGrid.Columns.IndexOf(column));
            }
            return previousCellIndex;
        }


        #region Events & Event Raising Methods

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellActivating"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="previousRowColumnIndex">
        /// The rowcolumnindex of the precious active cell.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The rowcolumnindex of the currently active cell.                
        /// </param>
        /// <param name="activationTrigger">
        /// The activation trigger for current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellActivating"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseCurrentCellActivatingEvent(RowColumnIndex previousRowColumnIndex, RowColumnIndex currentRowColumnIndex, ActivationTrigger activationTrigger)
        {
            if (previousRowColumnIndex.RowIndex < 0 || previousRowColumnIndex.ColumnIndex < 0)
                previousRowColumnIndex = new RowColumnIndex(0, 0);

            var args = new CurrentCellActivatingEventArgs(TreeGrid)
            {
                PreviousRowColumnIndex = previousRowColumnIndex,
                CurrentRowColumnIndex = currentRowColumnIndex,
                ActivationTrigger = activationTrigger
            };
            return TreeGrid.RaiseCurrentCellActivatingEvent(args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellActivated"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="previousRowColumnIndex">
        /// The rowcolumnindex of the precious active cell.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The rowcolumnindex of the currently active cell.                
        /// </param>
        /// <param name="activationTrigger">
        /// The activation trigger for current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellActivated"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        internal protected void RaiseCurrentCellActivatedEvent(RowColumnIndex rowColumnIndex, RowColumnIndex previousRowColumnIndex, ActivationTrigger activationTrigger)
        {
            if (previousRowColumnIndex.RowIndex < 0 || previousRowColumnIndex.ColumnIndex < 0)
                previousRowColumnIndex = new RowColumnIndex(0, 0);

            var e = new CurrentCellActivatedEventArgs(TreeGrid)
            {
                CurrentRowColumnIndex = rowColumnIndex,
                PreviousRowColumnIndex = previousRowColumnIndex,
                ActivationTrigger = activationTrigger
            };
            TreeGrid.RaiseCurrentCellActivatedEvent(e);
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellEndEdit"/> event.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The rowcolumnindex of the current cell to raise the event.
        /// </param>
        protected void RaiseCurrentCellEndEditEvent(RowColumnIndex rowColumnIndex)
        {
            var args = new CurrentCellEndEditEventArgs(TreeGrid)
            {
                RowColumnIndex = rowColumnIndex
            };
            TreeGrid.RaiseCurrentCellEndEditEvent(args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellBeginEdit"/> event.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex of the current cell.
        /// </param>
        /// <param name="gridColumn">
        /// The corresponding column that contains the current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellBeginEdit"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseCurrentCellBeginEditEvent(RowColumnIndex rowColumnIndex, TreeGridColumn gridColumn)
        {
            var args = new TreeGridCurrentCellBeginEditEventArgs(TreeGrid)
            {
                RowColumnIndex = rowColumnIndex,
                Column = gridColumn
            };
            return TreeGrid.RaiseCurrentCellBeginEditEvent(args);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            this.TreeGrid = null;
        }
        #endregion
    }
}
