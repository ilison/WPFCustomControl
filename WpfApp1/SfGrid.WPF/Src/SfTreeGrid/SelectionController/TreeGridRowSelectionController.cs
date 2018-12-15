#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using Syncfusion.UI.Xaml.Grid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Syncfusion.Data;
#if UWP
using Windows.UI.Xaml.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Windows.Foundation;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using System.Collections.Specialized;
    using Windows.Devices.Input;
#else
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using System.Collections.Specialized;
    
#endif

    /// <summary>
    /// Represents a class that implements the selection behavior of row in SfTreeGrid.
    /// </summary>
    public class TreeGridRowSelectionController : TreeGridBaseSelectionController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowSelectionController"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/>.
        /// </param>
        public TreeGridRowSelectionController(SfTreeGrid treeGrid)
            : base(treeGrid)
        {
            previousSelectedRows = new List<TreeGridRowInfo>();
        }

        //Behavior: To maintain the selection while selecting the rows using CTRL key.
        /// <summary>
        /// Gets or sets the collection of previous selected rows while selecting the cells using CTRL key.
        /// </summary>
        internal List<TreeGridRowInfo> previousSelectedRows;

        /// <summary>
        /// Gets or sets a value that determines whether shift selection performed in SfTreeGrid.
        /// </summary>        
        internal bool isInShiftSelection = false;

#if WPF
        /// <summary>
        /// Processes the cell selection while scrolling the SfTreeGrid using mouse wheel.
        /// </summary>
        /// <param name="mouseButtonEventArgs">
        /// An <see cref="T:System.Windows.Input.MouseButtonEventArgs"/>that contains the event data.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex where the mouse wheel interaction occurs.    
        /// </param>  
        protected override void ProcessPointerWheel(MouseWheelEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var currentCell = CurrentCellManager.CurrentCell;
            if (currentCell != null && CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowColumnIndex.RowIndex
                && currentCell.TreeGridColumn.CanAllowSpinOnMouseScroll())
            {
                TreeGrid.TreeGridPanel.SuspendManipulationScroll = currentCell.IsEditing;
            }
            else
            {
                TreeGrid.TreeGridPanel.SuspendManipulationScroll = false;
            }
        }
#endif
        /// <summary>
        /// Handles the selection for the keyboard interactions that are performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled in SfTreeGrid; otherwise, <b>false</b>.
        /// </returns>
        public override bool HandleKeyDown(KeyEventArgs args)
        {
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return args.Handled;

            if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
            {
                if (CurrentCellManager.HandleKeyDown(args))
                    ProcessKeyDown(args);
            }
            else
                ProcessKeyDown(args);
            return args.Handled;
        }

        /// <summary>
        /// Processes the row selection when the keyboard interactions that are performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the information about the key that was pressed.
        /// </param>
        /// <remarks>
        /// This method helps to customize the keyboard interaction behavior in SfTreeGrid.
        /// </remarks>      
        protected virtual void ProcessKeyDown(KeyEventArgs args)
        {
            var currentKey = args.Key;
            if (TreeGrid.FlowDirection == FlowDirection.RightToLeft)
                ChangeFlowDirectionKey(ref currentKey);

            int rowIndex, columnIndex;
            bool needToScroll = true;

            switch (currentKey)
            {
                case Key.Escape:
                    {
                        if (CurrentCellManager.ProcessKeyDown(args, CurrentCellManager.CurrentRowColumnIndex))
                            args.Handled = true;
                    }
                    break;
                case Key.F2:
                    {
                        if (CurrentCellManager.ProcessKeyDown(args, CurrentCellManager.CurrentRowColumnIndex))
                            args.Handled = true;
                    }
                    break;
                case Key.Enter:
                    if (SelectionHelper.CheckControlKeyPressed())
                    {
                        var rowColumnIndex = new RowColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                        var newRowColumnIndex = rowColumnIndex;
                        if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex, true))
                        {
                            args.Handled = true;
                            return;
                        }
                        args.Handled = true;
                        return;
                    }
                    goto case Key.Down;
                case Key.A:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.SelectAll();
                            this.LastPressedKey = Key.A;
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.Down:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                            rowIndex = this.TreeGrid.GetLastDataRowIndex();
                        else
                            rowIndex = this.GetNextRowIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        if (rowIndex < 0)
                            return;

                        var rowColumnIndex = new RowColumnIndex(rowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                        RowColumnIndex newRowColumnIndex = rowColumnIndex;

                        if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex, true))
                        {
                            args.Handled = true;
                            return;
                        }
                        var firstCellIndex = CurrentCellManager.GetFirstCellIndex();
                        if (newRowColumnIndex.ColumnIndex < firstCellIndex)
                            newRowColumnIndex.ColumnIndex = firstCellIndex;

                        if (!HandleSelection(args, this.CurrentCellManager.CurrentRowColumnIndex, newRowColumnIndex, currentKey, ref needToScroll))
                        {
                            args.Handled = true;
                            return;
                        }

                        args.Handled = true;
                    }
                    break;
                case Key.Up:
                    {
                        var firstDataRowIndex = this.TreeGrid.GetFirstDataRowIndex();
                        if (SelectionHelper.CheckControlKeyPressed())
                            rowIndex = firstDataRowIndex;
                        else
                            rowIndex = this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        var rowColumnIndex = new RowColumnIndex(rowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                        RowColumnIndex newRowColumnIndex = rowColumnIndex;
                        if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (!HandleSelection(args, this.CurrentCellManager.CurrentRowColumnIndex, newRowColumnIndex, currentKey, ref needToScroll))
                        {
                            args.Handled = true;
                            return;
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.Right:
                case Key.Left:
                    {
                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.TreeGrid.GetHeaderIndex())
                            return;

                        var rowColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex;
                        if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                        {
                            if (!CurrentCellManager.ProcessKeyDown(args, rowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }
                        }

                        if (this.SelectedRows.Count > 1 && this.TreeGrid.SelectionMode == GridSelectionMode.Extended && !SelectionHelper.CheckShiftKeyPressed())
                            this.ClearSelections(true);

                        this.LastPressedKey = currentKey;
                        args.Handled = true;
                    }
                    break;
                case Key.PageDown:
                    {
                        rowIndex = this.TreeGrid.GetNextPageIndex();
                        var firstCellIndex = this.CurrentCellManager.GetFirstCellIndex();
                        var lastRowIndex = this.TreeGrid.GetLastDataRowIndex();
                        if (rowIndex < 0)
                            return;

                        if (rowIndex > lastRowIndex || (rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && rowIndex == lastRowIndex))
                        {
                            args.Handled = true;
                            return;
                        }
                        var colIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < firstCellIndex ? firstCellIndex : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                        var rowColumnIndex = new RowColumnIndex(rowIndex, colIndex);
                        RowColumnIndex newRowColumnIndex = rowColumnIndex;

                        if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                        {
                            args.Handled = true;
                            return;
                        }
                        rowIndex = newRowColumnIndex.RowIndex;
                        if (rowIndex > lastRowIndex || rowIndex < TreeGrid.GetFirstDataRowIndex())
                        {
                            args.Handled = true;
                            return;
                        }

                        if (!HandleSelection(args, this.CurrentCellManager.CurrentRowColumnIndex, newRowColumnIndex, currentKey, ref needToScroll))
                        {
                            args.Handled = true;
                            return;
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.PageUp:
                    {
                        rowIndex = this.TreeGrid.GetPreviousPageIndex();
                        var firstCellIndex = this.CurrentCellManager.GetFirstCellIndex();
                        var firstDataRowIndex = this.TreeGrid.GetFirstDataRowIndex();
                        if (rowIndex < firstDataRowIndex && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                        {
                            args.Handled = true;
                            return;
                        }
                        var colIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < firstCellIndex ? firstCellIndex : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                        var rowColumnIndex = new RowColumnIndex(rowIndex, colIndex);
                        RowColumnIndex newRowColumnIndex = rowColumnIndex;
                        if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                        {
                            args.Handled = true;
                            return;
                        }
                        rowIndex = newRowColumnIndex.RowIndex;
                        if (rowIndex < firstDataRowIndex)
                        {
                            args.Handled = true;
                            return;
                        }

                        if (!HandleSelection(args, this.CurrentCellManager.CurrentRowColumnIndex, newRowColumnIndex, currentKey, ref needToScroll))
                        {
                            args.Handled = true;
                            return;
                        }

                        args.Handled = true;
                    }
                    break;
                case Key.Home:
                    {
                        var firstDataRowIndex = this.TreeGrid.GetFirstDataRowIndex();
                        rowIndex = SelectionHelper.CheckControlKeyPressed() ? firstDataRowIndex : this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                        if (rowIndex <= this.TreeGrid.GetHeaderIndex())
                            return;

                        if (this.TreeGrid.NavigationMode == NavigationMode.Row)
                            rowIndex = firstDataRowIndex;

                        if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                            columnIndex = CurrentCellManager.GetFirstCellIndex();
                        else
                            columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                        var rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                        var newRowColumnIndex = rowColumnIndex;
                        if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                        {
                            if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }
                            rowIndex = newRowColumnIndex.RowIndex;
                        }

                        if (!HandleSelection(args, this.CurrentCellManager.CurrentRowColumnIndex, newRowColumnIndex, currentKey, ref needToScroll))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (needToScroll && this.TreeGrid.NavigationMode == NavigationMode.Cell)
                            this.CurrentCellManager.ScrollInView(newRowColumnIndex.ColumnIndex);

                        args.Handled = true;
                    }
                    break;
                case Key.End:
                    {
                        var lastDataRowIndex = this.TreeGrid.GetLastDataRowIndex();
                        rowIndex = SelectionHelper.CheckControlKeyPressed() ? lastDataRowIndex : CurrentCellManager.CurrentRowColumnIndex.RowIndex;

                        if (this.TreeGrid.NavigationMode == NavigationMode.Row)
                            rowIndex = lastDataRowIndex;

                        if (rowIndex < 0 || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.TreeGrid.GetHeaderIndex())
                            return;
                        if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                            columnIndex = CurrentCellManager.GetLastCellIndex();
                        else
                            columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

                        if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                        {
                            if (TreeGrid.FlowDirection == FlowDirection.RightToLeft && SelectionHelper.CheckControlKeyPressed())
                                columnIndex = CurrentCellManager.GetFirstCellIndex();
                        }
                        var rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                        var newRowColumnIndex = rowColumnIndex;
                        if (TreeGrid.NavigationMode == NavigationMode.Cell)
                        {
                            if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }
                        }
                        rowIndex = newRowColumnIndex.RowIndex;
                        if (!HandleSelection(args, this.CurrentCellManager.CurrentRowColumnIndex, newRowColumnIndex, currentKey, ref needToScroll))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (needToScroll && this.TreeGrid.NavigationMode == NavigationMode.Cell)
                            this.CurrentCellManager.ScrollInView(columnIndex);

                        args.Handled = true;
                    }
                    break;
                case Key.Tab:
                    {
                        if (CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.TreeGrid.GetHeaderIndex() || this.TreeGrid.NavigationMode != NavigationMode.Cell)
                            return;
                        var lastDataRowIndex = this.TreeGrid.GetLastDataRowIndex();
                        var firstDataRowIndex = this.TreeGrid.GetFirstDataRowIndex();
                        this.LastPressedKey = Key.Tab;
                        RowColumnIndex rowColumnIndex;
                        RowColumnIndex newRowColumnIndex;

                        if (this.TreeGrid.NavigationMode == NavigationMode.Cell)
                        {
                            if (!CurrentCellManager.ProcessKeyDown(args, CurrentCellManager.CurrentRowColumnIndex))
                            {
                                if (args.Handled)
                                    return;

                                rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                                if (SelectionHelper.CheckShiftKeyPressed())
                                {
                                    if (rowIndex < 0)
                                        rowIndex = lastDataRowIndex;
                                    else
                                        rowIndex = this.GetPreviousRowIndex(rowIndex);

                                    columnIndex = CurrentCellManager.GetLastCellIndex(TreeGrid.FlowDirection);
                                    rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                                    newRowColumnIndex = rowColumnIndex;
                                    if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                                    {
                                        args.Handled = true;
                                        return;
                                    }
                                    rowIndex = newRowColumnIndex.RowIndex;
                                    if (rowIndex <= firstDataRowIndex && CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex)
                                        return;
                                }
                                else
                                {
                                    if (CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0)
                                        rowIndex = firstDataRowIndex;
                                    else
                                        rowIndex = this.GetNextRowIndex(rowIndex);

                                    columnIndex = CurrentCellManager.GetFirstCellIndex(TreeGrid.FlowDirection);

                                    rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                                    newRowColumnIndex = rowColumnIndex;
                                    if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                                    {
                                        args.Handled = true;
                                        return;
                                    }

                                    rowIndex = newRowColumnIndex.RowIndex;
                                    if (rowIndex >= lastDataRowIndex && (CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex || this.TreeGrid.TreeGridPanel.RowCount == rowIndex))
                                        return;
                                }

                                if (!CurrentCellManager.ProcessCurrentCellSelection(newRowColumnIndex, ActivationTrigger.Keyboard))
                                    return;

                                if (TreeGrid.SelectionMode != GridSelectionMode.Multiple)
                                    needToScroll = ProcessSelection(newRowColumnIndex, this.CurrentCellManager.CurrentRowColumnIndex, SelectionReason.KeyPressed);

                                this.SetPressedIndex(newRowColumnIndex);

                                columnIndex = columnIndex == this.CurrentCellManager.GetFirstCellIndex() ? (this.TreeGrid.showRowHeader ? 1 : 0) : columnIndex;

                                if (needToScroll)
                                    CurrentCellManager.ScrollInView(columnIndex);

                                this.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            }
                            args.Handled = true;
                        }
                    }
                    break;

                case Key.Space:
                    {
                        if (this.TreeGrid.SelectionMode != GridSelectionMode.Multiple || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.TreeGrid.GetHeaderIndex())
                            return;

                        var addedItems = new List<object>();
                        var removedItems = new List<object>();
                        var currentRow = this.TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                        if (currentRow != null)
                        {
                            if (currentRow.IsSelectedRow)
                            {
                                removedItems.Add(this.SelectedRows.Find(currentRow.RowIndex));
                                if (RaiseSelectionChanging(addedItems, removedItems))
                                {
                                    return;
                                }
                                RemoveSelection(removedItems);
                                this.TreeGrid.ShowRowFocusBorder(CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            }
                            else
                            {
                                addedItems.Add(GetTreeGridSelectedRow(currentRow.RowIndex));

                                if (RaiseSelectionChanging(addedItems, removedItems))
                                {
                                    return;
                                }
                                AddSelection(addedItems);
                                this.TreeGrid.HideRowFocusBorder();
                            }
                            RaiseSelectionChanged(addedItems, removedItems);
                            //This condition is added to Check and UnCheck the check box when pressing space key in CheckBoxColumn.
                            if (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is TreeGridCellCheckBoxRenderer)
                                args.Handled = false;
                            else
                                args.Handled = true;
                        }

                        break;
                    }
                case Key.C:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.TreeGrid.TreeGridCopyPaste.Copy();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.X:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.TreeGrid.TreeGridCopyPaste.Cut();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.V:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.TreeGrid.TreeGridCopyPaste.Paste();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.Insert:
                    {
                        if (SelectionHelper.CheckShiftKeyPressed())
                        {
                            this.TreeGrid.TreeGridCopyPaste.Paste();
                            args.Handled = true;
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// Method to handles selection commonly in keyboard action.
        /// </summary>
        /// <param name="args">KeyEventArgs which contains pressed key information.</param>
        /// <param name="previousCurrentCellIndex">previous current row column index.</param>
        /// <param name="newRowColumnIndex">new rowcolumn index to be selected.</param>
        /// <param name="currentKey">currently pressed key after changing it based on flow direction.</param>
        /// <param name="needToScroll">if it is true, selection is processed. Else, false.</param>
        /// <returns>Returns true if selection is handled by selection controller. Else, false.</returns>
        private bool HandleSelection(KeyEventArgs args, RowColumnIndex previousCurrentCellIndex, RowColumnIndex newRowColumnIndex, Key currentKey, ref bool needToScroll)
        {
            if (!CurrentCellManager.ProcessKeyDown(args, newRowColumnIndex))
            {
                args.Handled = true;
                return false;
            }
            if (TreeGrid.SelectionMode != GridSelectionMode.Multiple)
            {
                if (SelectionHelper.CheckShiftKeyPressed() && this.TreeGrid.SelectionMode == GridSelectionMode.Extended)
                {
                    this.ProcessShiftSelection(newRowColumnIndex, previousCurrentCellIndex, SelectionReason.KeyPressed, currentKey);
                }
                else
                    needToScroll = this.ProcessSelection(newRowColumnIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);
                if (needToScroll)
                    ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            }
            else
            {
                CurrentCellManager.UpdateGridProperties(newRowColumnIndex);
                UpdateRowFocusBorder();
                ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            }
            if (SelectionHelper.CheckShiftKeyPressed() && this.TreeGrid.SelectionMode == GridSelectionMode.Extended)
                this.LastPressedKey = currentKey;
            else
            {
                this.LastPressedKey = Key.None;
                this.SetPressedIndex(new RowColumnIndex(newRowColumnIndex.RowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
            }
            return true;
        }

        internal void UpdateRowFocusBorder()
        {
            if (TreeGrid.NavigationMode != NavigationMode.Row)
                return;
            this.TreeGrid.HideRowFocusBorder();
            this.TreeGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
        }

        /// <summary>
        /// Gets the index of the next row corresponding to the specified row index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the index of next row.
        /// </param>
        /// <returns>
        /// The index of next row; Returns , -1 when the row index is last row index.
        /// </returns>
        protected int GetNextRowIndex(int index)
        {
            int firstRowIndex = this.TreeGrid.GetFirstDataRowIndex();
            if (index < firstRowIndex)
                return firstRowIndex;
            var lastRowIndex = TreeGrid.GetLastDataRowIndex();
            if (index >= lastRowIndex)
                return lastRowIndex;
            if (index >= this.TreeGrid.TreeGridPanel.ScrollRows.LineCount)
                return -1;
            var nextIndex = this.TreeGrid.TreeGridPanel.ScrollRows.GetNextScrollLineIndex(index);
            return nextIndex;
        }

        /// <summary>
        /// Gets the index of previous row corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the previous row index.
        /// </param>
        /// <returns>
        /// Returns the index of previous row.
        /// </returns>
        protected int GetPreviousRowIndex(int index)
        {
            if (index <= this.TreeGrid.HeaderLineCount)
                return this.TreeGrid.HeaderLineCount;

            var previousIndex = this.TreeGrid.TreeGridPanel.ScrollRows.GetPreviousScrollLineIndex(index);
            return previousIndex;
        }

        /// <summary>
        /// Processes the row selection when the mouse pointer is pressed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the pressed point location.    
        /// </param>
        /// <remarks>
        /// This method will be invoked when the pointer is pressed using touch or mouse in SfTreeGrid.
        /// The selection is initialized in pointer pressed state when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowSelectionOnPointerPressed"/> set as true.
        /// </remarks>
        protected override void ProcessPointerPressed(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None || rowColumnIndex.RowIndex <= this.TreeGrid.GetHeaderIndex())
                return;
#if WPF
            if (args != null && this.SelectedRows.Contains(rowColumnIndex.RowIndex) && args.ChangedButton == MouseButton.Right)
                return;
#endif
            pressedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.TreeGrid.TreeGridPanel);
#if UWP
            pressedVerticalOffset = this.TreeGrid.TreeGridPanel.ScrollOwner != null ? this.TreeGrid.TreeGridPanel.ScrollOwner.VerticalOffset : 0;
#endif
            this.isMousePressed = true;
            if (isInShiftSelection && (!SelectionHelper.CheckShiftKeyPressed() || SelectionHelper.CheckControlKeyPressed() || this.TreeGrid.SelectionMode == GridSelectionMode.Multiple))
                isInShiftSelection = false;
            if (!SelectionHelper.CheckShiftKeyPressed() && LastPressedKey != Key.A)
                LastPressedKey = Key.None;
            if (!SelectionHelper.CheckShiftKeyPressed())
                this.SetPressedIndex(rowColumnIndex);
            if (this.TreeGrid.AllowSelectionOnPointerPressed)
                ProcessPointerSelection(args, rowColumnIndex);
        }


        /// <summary>
        /// Processes the row selection when the mouse pointer is released from SfTreeGrid. 
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse released point.
        /// </param>
        /// <remarks>
        /// The selection is initialized in pointer released state when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowSelectionPointerPressed"/> set as false.        
        /// </remarks>
        protected override void ProcessPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            this.isMousePressed = false;
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None || this.TreeGrid.AllowSelectionOnPointerPressed || rowColumnIndex.RowIndex <= this.TreeGrid.GetHeaderIndex())
                return;
#if WPF
            var pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.TreeGrid.TreeGridPanel);
#else
            //Point is calculated based on VisualContainer like WPF.
            Point pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.TreeGrid.TreeGridPanel);
            var releasedVerticalOffset = this.TreeGrid.TreeGridPanel.ScrollOwner != null ? this.TreeGrid.TreeGridPanel.ScrollOwner.VerticalOffset : 0;

            //  get the difference of pressed and released vertical offset value.
            var verticalOffsetChange = Math.Abs(releasedVerticalOffset - pressedVerticalOffset);
#endif
            var xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            var yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);

            //Here we checking the pointer pressed position and pointer released position. Because we don't select the row while manipulate scrolling.
#if UWP
            //Selection needs to be skipped if the scrolling happens in SfTreeGrid. So verticalOffsetChange check is added.
            if (xPosChange < 20 && yPosChange < 20 && verticalOffsetChange < 10)
            {
#else
            bool isValidated = TreeGridValidationHelper.IsCurrentRowValidated || TreeGridValidationHelper.IsCurrentCellValidated;
            if (xPosChange < 20 && yPosChange < 20)
            {

                if (args != null && (this.SelectedRows.Contains(rowColumnIndex.RowIndex) || !isValidated) &&
                    args.ChangedButton == MouseButton.Right)
                {
                    return;
                }
#endif
                if (this.PressedRowColumnIndex.RowIndex != rowColumnIndex.RowIndex && !SelectionHelper.CheckShiftKeyPressed())
                {
#if WPF
                    if (args == null) return;
                    if (args.ChangedButton == MouseButton.Right)
                        args.Handled = true;
#endif
                    return;
                }

                ProcessPointerSelection(args, rowColumnIndex);
            }
#if WPF
            else if (args.ChangedButton == MouseButton.Right && (!isValidated || this.PressedRowColumnIndex.RowIndex != rowColumnIndex.RowIndex))
            {
                args.Handled = true;
            }
#endif
        }
        /// <summary>
        /// Processes the row selection when mouse pointer is moved on the SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse related interaction.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse point.    
        /// </param>
        protected override void ProcessPointerMoved(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
#if WPF
            this.isMousePressed = args.LeftButton == MouseButtonState.Pressed && isMousePressed;
#else
            if (args.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
                return;

            this.isMousePressed = args.Pointer.IsInContact && isMousePressed;
#endif
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None || this.TreeGrid.SelectionMode == GridSelectionMode.Single || this.PressedRowColumnIndex.RowIndex < 0 || rowColumnIndex.RowIndex > this.TreeGrid.GetLastDataRowIndex() || rowColumnIndex.RowIndex < this.TreeGrid.GetFirstDataRowIndex() || !this.isMousePressed)
                return;

            if (this.CurrentCellManager.CurrentRowColumnIndex == rowColumnIndex && this.SelectedRows.Contains(rowColumnIndex.RowIndex))
                return;

            if (rowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex())
                return;

            //Point is calculated based on VisualContainer like WPF.
            Point pointerMovedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.TreeGrid.TreeGridPanel);


            double xPosChange = Math.Abs(pointerMovedPosition.X - pressedPosition.X);
            double yPosChange = Math.Abs(pointerMovedPosition.Y - pressedPosition.Y);

            //if (!this.TreeGrid.AutoScroller.InsideScrollBounds.Contains(pointerMovedPosition) && this.TreeGrid.AutoScroller.InsideScrollBounds.Contains(pressedPosition))
            //    return;

            if (xPosChange < 10 && yPosChange < 10)
                return;

            if (this.CurrentCellManager.CurrentCell != null && this.CurrentCellManager.CurrentCell.IsEditing)
            {
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == this.PressedRowColumnIndex.RowIndex && rowColumnIndex.ColumnIndex == this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex)
                    return;

                var newRowColumnIndex = rowColumnIndex;
                if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                {
                    args.Handled = true;
                    return;
                }
            }
        }
        /// <summary>
        /// Processes the selection when the mouse point is double tapped on the particular cell in SfTreeGrid.
        /// </summary>        
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>  
        /// <remarks>
        /// This method invoked to process selection and begin edit the cell when <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnDoubleTap"/>.
        /// </remarks>          
        protected override void ProcessOnDoubleTapped(RowColumnIndex currentRowColumnIndex)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None || this.CurrentCellManager.CurrentRowColumnIndex != currentRowColumnIndex || (this.TreeGrid.NavigationMode == NavigationMode.Row))
                return;

            if (this.TreeGrid.EditTrigger == EditTrigger.OnDoubleTap)
            {
                if (this.TreeGrid.SelectionMode != GridSelectionMode.Single)
                    this.AddSelection(new List<object>() { GetTreeGridSelectedRow(CurrentCellManager.CurrentRowColumnIndex.RowIndex) });
                CurrentCellManager.ProcessOnDoubleTapped();
            }
        }
        /// <summary>
        /// Processes the cell selection when the mouse point is tapped on the TreeGridCell. 
        /// </summary>
        /// <param name="e">
        /// Contains the data related to the tap interaction.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>    
        /// <remarks>
        /// This method invoked to process selection and begin edit the cell when <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnTap"/>.
        /// </remarks>
        protected override void ProcessOnTapped(TappedEventArgs e, RowColumnIndex currentRowColumnIndex)
        {
            if (!TreeGridValidationHelper.IsCurrentCellValidated || IsSuspended || !this.TreeGrid.AllowFocus(currentRowColumnIndex) || this.TreeGrid.SelectionMode == GridSelectionMode.None || this.TreeGrid.NavigationMode == NavigationMode.Row)
                return;
#if WPF
            Point pointerReleasedRowPosition = e == null ? new Point() : SelectionHelper.GetPointPosition(e, this.TreeGrid.TreeGridPanel);
#else
            Point pointerReleasedRowPosition = e == null ? new Point() : e.GetPosition(this.TreeGrid.TreeGridPanel);
#endif

            double xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            double yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);
            //Here we checking the pointer pressed position and pointer released position. Because we don't selec the row while manipulate scrolling.
            if (xPosChange < 20 && yPosChange < 20)
            {
                var newRowColumnIndex = currentRowColumnIndex;
                // UWP-3380 - When EditTrigger is double tap, cell gets end edited from here. To skip this, EditTrigger is checked like in SfDataGrid.
                if (TreeGrid.EditTrigger == EditTrigger.OnTap)
                {
                    if (!CanCommitAndMoveCurrentCell(currentRowColumnIndex, ref newRowColumnIndex))
                        return;
                }
                CurrentCellManager.ProcessOnTapped(e, newRowColumnIndex);
            }
            else
            {
                CurrentCellManager.ScrollInView(currentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the row selection when the pointer pressed or released interactions are performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse point.
        /// </param>
        protected internal void ProcessPointerSelection(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex.ColumnIndex < this.TreeGrid.GetFirstColumnIndex())
            {
                rowColumnIndex.ColumnIndex = this.CurrentCellManager.GetFirstCellIndex();
            }
            var newRowColumnIndex = rowColumnIndex;
            if (TreeGrid.NavigationMode == NavigationMode.Cell)
            {
                if (CurrentCellManager.CurrentRowColumnIndex != rowColumnIndex && TreeGrid.AllowFocus(rowColumnIndex))
                {
                    if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                        return;
                    if (newRowColumnIndex.RowIndex < 0)
                        return;
                }
            }
            if (!CurrentCellManager.HandlePointerOperation(args, newRowColumnIndex))
                return;
            var previousCurrentCellIndex = CurrentCellManager.CurrentRowColumnIndex;
            if (this.TreeGrid.SelectionMode == GridSelectionMode.Extended && SelectionHelper.CheckShiftKeyPressed() && this.SelectedRows.Count > 0)
            {
                if (TreeGridValidationHelper.IsCurrentCellValidated)
                    this.ProcessShiftSelection(newRowColumnIndex, previousCurrentCellIndex, SelectionReason.PointerReleased, Key.None);
            }
            else
            {
                if (TreeGridValidationHelper.IsCurrentCellValidated)
                    this.ProcessSelection(newRowColumnIndex, previousCurrentCellIndex, SelectionReason.PointerReleased);
            }
        }

        /// <summary>
        /// Processes the row selection when the rows are selected by using Shift key.
        /// </summary>
        /// <param name="newRowColumnIndex"></param>
        /// <param name="previousRowColumnIndex">previous currentcell row column index.</param>
        /// <param name="reason">Contains the reason for processing the Shift selection.</param>
        /// <param name="key">Contains the key which initiates the shift selection.</param>
        internal virtual void ProcessShiftSelection(RowColumnIndex newRowColumnIndex, RowColumnIndex previousRowColumnIndex, SelectionReason reason, Key key)
        {
            if (newRowColumnIndex == previousRowColumnIndex)
                return;
            var activationTrigger = (reason == SelectionReason.PointerPressed || reason == SelectionReason.PointerReleased) ? ActivationTrigger.Mouse : ActivationTrigger.Keyboard;
            if (newRowColumnIndex.RowIndex >= this.TreeGrid.GetFirstDataRowIndex() && previousRowColumnIndex.RowIndex < this.TreeGrid.GetFirstDataRowIndex())
            {
                this.ProcessSelection(newRowColumnIndex, previousRowColumnIndex, reason);
                this.SetPressedIndex(newRowColumnIndex);
                return;
            }
            if (LastPressedKey == Key.None && !isInShiftSelection)
                previousSelectedRows = this.SelectedRows.ToList();

            object rowData = null;
            isInShiftSelection = true;
            List<TreeGridRowInfo> addedItems = new List<TreeGridRowInfo>();
            List<TreeGridRowInfo> removedItems = new List<TreeGridRowInfo>();

            int pressedRowIndex = this.PressedRowColumnIndex.RowIndex;
            int pressedColumnIndex = this.PressedRowColumnIndex.ColumnIndex;
            bool isInLargeSelection = false;
            bool needToRemove = (newRowColumnIndex.RowIndex < previousRowColumnIndex.RowIndex && newRowColumnIndex.RowIndex >= pressedRowIndex || (newRowColumnIndex.RowIndex > previousRowColumnIndex.RowIndex && newRowColumnIndex.RowIndex <= pressedRowIndex)) && this.SelectedRows.Count > 0;
            List<object> addedItem = new List<object>();
            List<object> removedItem = new List<object>();

            switch (key)
            {
                case Key.None:
                case Key.PageDown:
                case Key.PageUp:
                case Key.Home:
                case Key.End:
                    var rItems = this.SelectedRows.ToList();
                    var aItems = new List<TreeGridRowInfo>();
                    isInLargeSelection = true;

                    if (pressedRowIndex <= newRowColumnIndex.RowIndex)
                    {
                        var lastDataRowIndex = this.TreeGrid.GetLastDataRowIndex();
                        while (pressedRowIndex <= newRowColumnIndex.RowIndex)
                        {
                            TreeNode treeNode = this.TreeGrid.GetNodeAtRowIndex(pressedRowIndex);
                            rowData = treeNode.Item;
                            TreeGridRowInfo treeGridRowInfo = new TreeGridRowInfo(pressedRowIndex, rowData, treeNode);
                            aItems.Add(treeGridRowInfo);
                            pressedRowIndex = pressedRowIndex == lastDataRowIndex ? ++pressedRowIndex : this.GetNextRowIndex(pressedRowIndex);
                        }
                    }
                    else
                    {
                        var firstDataRowIndex = this.TreeGrid.GetFirstDataRowIndex();
                        while (pressedRowIndex >= newRowColumnIndex.RowIndex)
                        {
                            TreeNode treeNode = this.TreeGrid.GetNodeAtRowIndex(pressedRowIndex);
                            rowData = treeNode.Item;
                            TreeGridRowInfo treeGridRowInfo = new TreeGridRowInfo(pressedRowIndex, rowData, treeNode);
                            aItems.Add(treeGridRowInfo);
                            pressedRowIndex = pressedRowIndex == firstDataRowIndex ? --pressedRowIndex : this.GetPreviousRowIndex(pressedRowIndex);
                        }
                    }

                    var commonItems = aItems.Intersect(rItems).ToList();
                    removedItems = rItems.Except(commonItems).ToList();
                    addedItems = aItems.Except(commonItems).ToList();
                    removedItem = removedItems.Except(previousSelectedRows).ToList<object>();
                    break;
                case Key.Down:
                case Key.Up:

                    if (SelectionHelper.CheckControlKeyPressed() && SelectionHelper.CheckShiftKeyPressed())
                    {
                        goto case Key.End;
                    }
                    else
                    {
                        int rowIndex = 0;
                        int comparerRowIndex = 0;
                        if (!needToRemove)
                        {
                            rowIndex = newRowColumnIndex.RowIndex;
                            comparerRowIndex = previousRowColumnIndex.RowIndex;
                        }
                        else
                        {
                            rowIndex = previousRowColumnIndex.RowIndex;
                            comparerRowIndex = newRowColumnIndex.RowIndex;
                        }
                        if (rowIndex < comparerRowIndex)
                        {
                            var lastDataRowIndex = this.TreeGrid.GetLastDataRowIndex();
                            while (rowIndex < comparerRowIndex)
                            {
                                TreeNode treeNode = this.TreeGrid.GetNodeAtRowIndex(rowIndex);
                                rowData = treeNode.Item;
                                TreeGridRowInfo treeGridRowInfo = new TreeGridRowInfo(rowIndex, rowData, treeNode);
                                addedItems.Add(treeGridRowInfo);
                                rowIndex = rowIndex != lastDataRowIndex ? this.GetNextRowIndex(rowIndex) : ++rowIndex;
                            }
                        }
                        else
                        {
                            var firstDataRowIndex = this.TreeGrid.GetFirstDataRowIndex();
                            while (rowIndex > comparerRowIndex)
                            {
                                TreeNode treeNode = this.TreeGrid.GetNodeAtRowIndex(rowIndex);
                                rowData = treeNode.Item;
                                TreeGridRowInfo treeGridRowInfo = new TreeGridRowInfo(rowIndex, rowData, treeNode);
                                addedItems.Add(treeGridRowInfo);
                                rowIndex = rowIndex != firstDataRowIndex ? this.GetPreviousRowIndex(rowIndex) : --rowIndex;
                            }
                        }
                    }
                    break;
            }

            addedItem = addedItems.Except(previousSelectedRows).ToList<object>();

            if (needToRemove && !isInLargeSelection)
            {
                removedItem = addedItem.ToList();
                addedItem.Clear();
            }
            // After pressing Ctrl+A, if we press shift+Down, Shift+Up
            else if (this.LastPressedKey == Key.A)
            {
                removedItems = this.SelectedRows.ToList();

                var rowInfo = this.SelectedRows.FindRowData(this.TreeGrid.GetNodeAtRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex));
                if (SelectionHelper.CheckShiftKeyPressed() && key != Key.None && rowInfo != null)
                {
                    removedItems.Remove(rowInfo);
                    removedItem = removedItems.Except(previousSelectedRows).ToList<object>();
                }
            }
            if (removedItem.Count > 0)
            {
                needToRemove = true;
            }

            if (this.RaiseSelectionChanging(addedItem, removedItem))
            {
                return;
            }
            if (newRowColumnIndex.RowIndex != previousRowColumnIndex.RowIndex)
            {
                if (TreeGrid.NavigationMode == NavigationMode.Cell)
                {
                    CurrentCellManager.UpdateCurrentCell(newRowColumnIndex, previousRowColumnIndex, activationTrigger);
                }
                else
                {
                    this.CurrentCellManager.UpdateGridProperties(newRowColumnIndex);
                }
            }
            if (needToRemove)
            {
                RemoveSelection(removedItem);
            }

            if (isInLargeSelection || !needToRemove || this.LastPressedKey == Key.A)
            {
                AddSelection(addedItem);
                if (LastPressedKey == Key.A)
                    LastPressedKey = key;
            }

            SuspendUpdates();
            this.RefreshSelectedIndexAndItem();
            ResumeUpdates();
            RaiseSelectionChanged(addedItem, removedItem);
        }

        /// <summary>
        /// Processes the row selection for the specified row index.
        /// </summary>       
        /// <param name="previousCurrentCellIndex">
        /// The previous current cell row column index.
        /// </param>
        /// <param name="reason">
        /// Contains the reason for process selection <see cref="Syncfusion.UI.Xaml.Grid.SelectionReason"/>.
        /// </param>       
        /// <returns>
        /// Returns <b>true</b> if the selection is added to corresponding row and column index; otherwise, <b>false</b>.
        /// </returns>        
        protected virtual bool ProcessSelection(RowColumnIndex rowColumnIndex, RowColumnIndex previousCurrentCellIndex, SelectionReason reason)
        {
            int rowIndex = rowColumnIndex.RowIndex;
            isInShiftSelection = false;
            bool needToRemove = this.TreeGrid.SelectionMode == GridSelectionMode.Single || (this.TreeGrid.SelectionMode == GridSelectionMode.Extended && ((!SelectionHelper.CheckControlKeyPressed() && (reason == SelectionReason.PointerReleased || reason == SelectionReason.PointerPressed)) || (reason == SelectionReason.KeyPressed && !SelectionHelper.CheckShiftKeyPressed()) || (SelectionHelper.CheckShiftKeyPressed())))
              || ((reason == SelectionReason.KeyPressed && this.TreeGrid.SelectionMode == GridSelectionMode.Multiple));
            bool needToRemoveSameRow = ((this.TreeGrid.SelectionMode == GridSelectionMode.Extended && ((SelectionHelper.CheckControlKeyPressed() && reason != SelectionReason.KeyPressed) || (reason == SelectionReason.KeyPressed && SelectionHelper.CheckShiftKeyPressed() && this.LastPressedKey != Key.Tab && this.LastPressedKey != Key.A))) || this.TreeGrid.SelectionMode == GridSelectionMode.Multiple) && this.SelectedRows.Contains(rowIndex);
            var activationTrigger = (reason == SelectionReason.PointerPressed || reason == SelectionReason.PointerReleased) ? ActivationTrigger.Mouse : ActivationTrigger.Keyboard;
            if (needToRemoveSameRow)
                needToRemoveSameRow = CheckCanRemoveSameRow();

            var data = this.TreeGrid.GetNodeAtRowIndex(rowIndex);
            if (needToRemoveSameRow && (SelectionHelper.CheckShiftKeyPressed() && this.TreeGrid.SelectionMode == GridSelectionMode.Extended && ((this.PressedRowColumnIndex.RowIndex < rowIndex && previousCurrentCellIndex.RowIndex < rowIndex) || (this.PressedRowColumnIndex.RowIndex > rowIndex && previousCurrentCellIndex.RowIndex > rowIndex))))
                return true;

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            var selectedRow = this.GetTreeGridSelectedRow(rowIndex);
            if (selectedRow != null)
                addedItems.Add(selectedRow);

            if (!needToRemoveSameRow)
            {
                var rItems = this.SelectedRows.ToList();
                removedItems = rItems.Cast<object>().ToList<object>();
            }

            if (needToRemoveSameRow)
            {
                removedItems.Clear();
                removedItems.AddRange(addedItems);

                if (this.RaiseSelectionChanging(null, removedItems))
                {
                    return false;
                }
                if (rowColumnIndex.RowIndex != previousCurrentCellIndex.RowIndex)
                {
                    if (TreeGrid.NavigationMode == NavigationMode.Cell)
                    {
                        CurrentCellManager.UpdateCurrentCell(rowColumnIndex, previousCurrentCellIndex, activationTrigger);
                    }
                    else
                    {
                        this.CurrentCellManager.UpdateGridProperties(rowColumnIndex);
                    }
                }
                this.RemoveSelection(removedItems);
                if (this.TreeGrid.SelectionMode == GridSelectionMode.Multiple)
                {
                    UpdateRowFocusBorder();
                }
                SuspendUpdates();
                this.RefreshSelectedIndexAndItem();
                ResumeUpdates();
                this.RaiseSelectionChanged(new List<object>(), removedItems);
                return true;
            }

            if (!needToRemove)
            {
                removedItems.Clear();
            }
            if (addedItems.Count > 0 || removedItems.Count > 0)
            {
                if (this.RaiseSelectionChanging(addedItems, removedItems))
                {
                    //When Cancel the Selection through the Selection Changing Event the pressed index will update when press any DataRow through MouseClick.
                    this.SetPressedIndex(previousCurrentCellIndex);
                    return false;
                }
                if (TreeGrid.NavigationMode == NavigationMode.Cell)
                {
                    // While pressing Enter key, new index also belongs to current index (in sorting cases) so rowColumnIndex==previousCurrentCellIndex check added
                    if (rowColumnIndex.RowIndex != previousCurrentCellIndex.RowIndex || rowColumnIndex == previousCurrentCellIndex)
                        CurrentCellManager.UpdateCurrentCell(rowColumnIndex, previousCurrentCellIndex, activationTrigger);
                }
                else
                {
                    this.CurrentCellManager.UpdateGridProperties(rowColumnIndex);
                }
                if (needToRemove)
                {
                    this.RemoveSelection(removedItems);
                }

                if (addedItems.Count > 0)
                    this.AddSelection(addedItems);

                if (this.PressedRowColumnIndex.RowIndex < this.TreeGrid.HeaderLineCount)
                    this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                this.previousSelectedRows.Clear();
                this.TreeGrid.HideRowFocusBorder();
                this.RaiseSelectionChanged(addedItems, removedItems);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Moves the current cell to the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current row selection should remove or not while moving the current cell.
        /// </param>     
        /// <remarks>
        /// This method is not applicable when <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NavigationMode"/> is Row.
        /// </remarks>   
        public override void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true)
        {
            if (!CurrentCellManager.CanMoveCurrentCell(rowColumnIndex))
                return;
            var newRowColumnIndex = rowColumnIndex;
            if (!CanCommitAndMoveCurrentCell(rowColumnIndex, ref newRowColumnIndex))
                return;
            if (newRowColumnIndex.RowIndex < 0)
                return;
            if (!CurrentCellManager.ProcessCurrentCellSelection(newRowColumnIndex, ActivationTrigger.Program))
                return;

            SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            if (this.SelectedRows.Contains(newRowColumnIndex.RowIndex))
                return;

            this.SuspendUpdates();

            if (needToClearSelection || this.TreeGrid.SelectionMode == GridSelectionMode.Single)
                this.RemoveSelection(this.SelectedRows.Cast<object>().ToList<object>());
            //The below condition is used to check whether the RowColumnIndex is greater than the FirstRowColumnIndex or not Because in this case we could not maintain the SelectedRows.           
            if (newRowColumnIndex.RowIndex >= this.TreeGrid.GetFirstDataRowIndex() && newRowColumnIndex.ColumnIndex >= this.TreeGrid.GetFirstColumnIndex())
            {
                var rowInfo = GetTreeGridSelectedRow(newRowColumnIndex.RowIndex);
                this.SelectedRows.Add(rowInfo);
                this.TreeGrid.ShowRowSelection(newRowColumnIndex.RowIndex, rowInfo.Node);

                if (rowInfo != null && rowInfo.Node != null)
                {
                    this.TreeGrid.SelectedItems.Add((rowInfo.Node).Item);
                }
                RefreshSelectedIndexAndItem();
            }
            this.ResumeUpdates();
            ResetFlags();
        }


        /// <summary>
        /// Clears all the selected rows in SfTreeGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Decides whether the current row selection needs to be removed when the selections are cleared.
        /// </param>
        /// <remarks>
        /// This method helps to clear the selection programmatically.
        /// </remarks>          
        public override void ClearSelections(bool exceptCurrentRow)
        {
            if (this.TreeGrid.TreeGridPanel == null)
                return;

            this.SuspendUpdates();

            //When changing the SelectionMode to Single from Multiple with editing the any row, the EditItem is still maintained in View,
            //Which throws exception when editing other cells. Hence the EndEdit is called.
            if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing || (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)))
            {
                this.CurrentCellManager.EndEdit();
                ResetSelectionHandled();
            }

            var removedItems = new List<object>();
            removedItems = this.SelectedRows.ToList<object>();

            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= this.TreeGrid.GetFirstDataRowIndex() && exceptCurrentRow)
            {
                object currentData = this.TreeGrid.CurrentItem;
                int currentRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                TreeGridRowInfo rowInfo = SelectedRows.Find(currentRowIndex);
                this.SelectedRows.Clear();
                this.TreeGrid.SelectedItems.Clear();
                this.HideAllRowSelectionBorder(exceptCurrentRow);
                var addedRowInfo = GetTreeGridSelectedRow(currentRowIndex);
                this.SelectedRows.Add(addedRowInfo);
                if (rowInfo != null)
                    removedItems.Remove(rowInfo);
                if (currentData != null)
                    this.TreeGrid.SelectedItems.Add(currentData);

                this.TreeGrid.ShowRowSelection(currentRowIndex, addedRowInfo.Node);
                this.RefreshSelectedIndexAndItem();
            }
            else
            {
                CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                this.CurrentCellManager.ResetCurrentRowColumnIndex();

                this.HideAllRowSelectionBorder(false);
                this.SelectedRows.Clear();
                this.TreeGrid.SelectedItems.Clear();
                this.TreeGrid.SelectedItem = null;
                this.TreeGrid.SelectedIndex = -1;
            }
            this.RaiseSelectionChanged(new List<object>(), removedItems);
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            this.TreeGrid.HideRowFocusBorder();
            //When changing the SelectionMode to single from Multiple the RowHeaderState has been changed, hence the below condition is added.
            ResetFlags();
            this.ResumeUpdates();
        }

        /// <summary>
        /// Method which reset the flags in the selection controller.
        /// </summary>
        private void ResetFlags()
        {
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            this.previousSelectedRows.Clear();
            this.LastPressedKey = Key.None;
            this.isInShiftSelection = false;
        }

        /// <summary>
        /// Selects all the rows in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public override void SelectAll()
        {
            if (!this.TreeGrid.HasView)
                return;
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None || this.TreeGrid.SelectionMode == GridSelectionMode.Single)
                return;

            int currentRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
            int rowIndex = this.TreeGrid.GetFirstDataRowIndex();
            var firstCellIndex = this.CurrentCellManager.GetFirstCellIndex();
            int columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < firstCellIndex ? firstCellIndex : CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            if (currentRowIndex < 0)
                currentRowIndex = rowIndex;

            CurrentCellManager.SelectCurrentCell(new RowColumnIndex(currentRowIndex, columnIndex));
            this.SuspendUpdates();

            var addedItems = this.SelectedRows.Cast<object>().ToList();

            var rowCount = this.TreeGrid.GetLastDataRowIndex();
            var rowIndexes = this.SelectedRows.GetRowIndexes();
            int nodeIndex = 0;
            TreeGridRowInfo rowInfo = null;
            foreach (var node in this.TreeGrid.View.Nodes)
            {
                rowIndex = this.TreeGrid.ResolveToRowIndex(nodeIndex);
                if (!rowIndexes.Contains(rowIndex))
                {
                    this.TreeGrid.SelectedItems.Add(node.Item);
                    rowInfo = new TreeGridRowInfo(rowIndex, node.Item, node);
                    this.SelectedRows.Add(rowInfo);
                }
                nodeIndex++;
            }

            this.ShowAllRowSelectionBorder();
            var aItems = this.SelectedRows.Except(addedItems).ToList();
            this.RefreshSelectedIndexAndItem();
            this.ResumeUpdates();
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            var args = new GridSelectionChangedEventArgs(this.TreeGrid) { AddedItems = aItems, RemovedItems = new List<object>() };
            this.TreeGrid.RaiseSelectionChangedEvent(args);
        }

        /// <summary>
        /// Hides the row selection border for all rows in SfTreeGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">        
        /// Indicates whether the selection border should be hidden except for current row.
        /// </param>
        internal virtual void HideAllRowSelectionBorder(bool exceptCurrentRow)
        {
            if (!exceptCurrentRow)
                this.TreeGrid.RowGenerator.Items.ForEach(item => { if (item.IsSelectedRow) item.IsSelectedRow = false; });
            else
                this.TreeGrid.RowGenerator.Items.ForEach(item => { if (!item.IsCurrentRow && item.IsSelectedRow) item.IsSelectedRow = false; });
        }

        /// <summary>
        /// Shows the selection border for all rows in SfTreeGrid.
        /// </summary>
        internal virtual void ShowAllRowSelectionBorder()
        {
            var FirstRowIndex = this.TreeGrid.GetFirstDataRowIndex();
            var LastRowIndex = this.TreeGrid.GetLastDataRowIndex();
            this.TreeGrid.RowGenerator.Items.ForEach(item =>
            {
                if (!item.IsSelectedRow && item.RowIndex >= FirstRowIndex && item.RowIndex <= LastRowIndex)
                    item.IsSelectedRow = true;
            });
        }

        /// <summary>
        /// Selects the rows corresponding to the specified start and end index of the row.
        /// </summary>
        /// <param name="startRowIndex">
        /// The start index of the row.
        /// </param>
        /// <param name="endRowIndex">
        /// The end index of the row.
        /// </param>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public override void SelectRows(int startRowIndex, int endRowIndex)
        {
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None || this.TreeGrid.SelectionMode == GridSelectionMode.Single)
                return;
            if (startRowIndex < 0 || endRowIndex < 0 || startRowIndex >= TreeGrid.TreeGridPanel.RowCount || endRowIndex >= TreeGrid.TreeGridPanel.RowCount)
                return;

            if (startRowIndex > endRowIndex)
            {
                var temp = startRowIndex;
                startRowIndex = endRowIndex;
                endRowIndex = temp;
            }

            var isSelectedRowsContains = this.SelectedRows.Any();

            if (!isSelectedRowsContains)
                this.CurrentCellManager.ProcessCurrentCellSelection(new RowColumnIndex(endRowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), ActivationTrigger.Program);

            this.SuspendUpdates();
            var addedItem = new List<object>();
            int rowIndex = startRowIndex;
            while (rowIndex <= endRowIndex)
            {
                object rowData = this.TreeGrid.GetNodeAtRowIndex(rowIndex).Item;
                var rowInfo = GetTreeGridSelectedRow(rowIndex);
                if (!this.SelectedRows.Contains(rowIndex))
                    this.SelectedRows.Add(rowInfo);
                this.TreeGrid.ShowRowSelection(rowIndex, rowInfo.Node);
                if (rowData != null && !this.TreeGrid.SelectedItems.Contains(rowData))
                {
                    addedItem.Add(rowData);
                    this.TreeGrid.SelectedItems.Add(rowData);
                }
                rowIndex++;
            }

            if (!isSelectedRowsContains)
            {
                this.RefreshSelectedIndexAndItem();
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Method which decides whether we can remove the selection in same row.
        /// </summary>
        /// <returns></returns>
        private bool CheckCanRemoveSameRow()
        {
            if (this.TreeGrid.SelectionMode == GridSelectionMode.Extended)
            {
#if UWP
                if ((Window.Current.CoreWindow.GetAsyncKeyState(Key.Shift).HasFlag(CoreVirtualKeyStates.Down)) && (Window.Current.CoreWindow.GetAsyncKeyState(Key.Home).HasFlag(CoreVirtualKeyStates.Down) || Window.Current.CoreWindow.GetAsyncKeyState(Key.End).HasFlag(CoreVirtualKeyStates.Down)))
#else
                if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.Home) || Keyboard.IsKeyDown(Key.End)))
#endif
                    return false;
            }
            return true;
        }

        internal virtual void RefreshSelectedIndexAndItem()
        {
            this.TreeGrid.SelectedIndex = this.SelectedRows.Count > 0 ? this.TreeGrid.ResolveToNodeIndex(this.SelectedRows[0].RowIndex) : -1;
            this.TreeGrid.SelectedItem = this.TreeGrid.SelectedItems.Count > 0 ? this.TreeGrid.SelectedItems[0] : null;
        }

        internal virtual void ResetSelectedRows(bool handleSelection = false, bool canFocusGrid = true)
        {
            this.RefreshSelectedItems();
            this.UpdateCurrentRowIndex(canFocusGrid);
            isSelectionHandled = handleSelection;
        }

        /// <summary>
        /// Refreshes the selected index, selected item, selected items and current item of SfTreeGrid.
        /// </summary>
        public void RefreshSelection()
        {
            ResetSelectedRows();
        }

        /// <summary>
        /// Adds the row selection for the specified list of items.
        /// </summary>
        /// <param name="addedItems">
        /// The corresponding list of items to add the selection.
        /// </param>       
        internal void AddSelection(List<object> addedItems)
        {
            this.SuspendUpdates();
            if (addedItems != null)
            {
                addedItems.ForEach(item =>
                {
                    var rowInfo = (TreeGridRowInfo)item;
                    if (item != null && !this.SelectedRows.Contains(rowInfo))
                    {
                        this.SelectedRows.Add(rowInfo);
                        this.TreeGrid.ShowRowSelection(rowInfo.RowIndex, rowInfo.Node);
                        if (rowInfo.RowData != null && !this.TreeGrid.SelectedItems.Contains(rowInfo.RowData))
                            this.TreeGrid.SelectedItems.Add(rowInfo.RowData);
                    }
                });
            }
            RefreshSelectedIndexAndItem();
            this.ResumeUpdates();
        }


        /// <summary>
        /// Removes the selection for the specified list of items.
        /// </summary>
        /// <param name="rowIndex">
        /// The rowIndex.
        /// </param>
        /// <param name="removedItems">
        /// The corresponding list of items to remove the selection. 
        /// </param>        
        /// <param name="needToSetCurrentCell">
        /// Indicates whether need to set current cell.
        /// </param>
        internal virtual void RemoveSelection(List<object> removedItems, bool needToSetCurrentCell = false)
        {
            this.SuspendUpdates();
            removedItems.ForEach(item =>
            {
                var rowInfo = (TreeGridRowInfo)item;
                if (this.SelectedRows.Remove(rowInfo))
                {
                    this.TreeGrid.HideRowSelection(rowInfo.RowIndex, rowInfo.Node);
                    if (rowInfo.RowData != null)
                        this.TreeGrid.SelectedItems.Remove(rowInfo.RowData);
                }

            });

            if (needToSetCurrentCell && TreeGrid.NavigationMode == NavigationMode.Cell)
            {
                int index = -1;
                if (SelectedRows.Any(r => r.RowIndex != -1))
                {
                    index = this.SelectedRows.LastOrDefault(r => r.RowIndex != -1).RowIndex;
                }
                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(index, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Processes the row selection when the column is sorted in SfTreeGrid.
        /// </summary>
        /// <param name="sortcolumnHandle">
        /// Contains information related to the sorting action.
        /// </param>        
        /// <remarks>
        /// This method refreshes the selection while sorting the column in SfTreeGrid.
        /// </remarks>
        protected override void ProcessOnSortChanged(SortColumnChangedHandle sortcolumnHandle)
        {
            this.RefreshSelectedItems();
            this.SuspendUpdates();
            UpdateCurrentRowIndex();
            if (sortcolumnHandle.ScrollToCurrentItem)
                this.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            this.ResumeUpdates();
        }

        /// <summary>
        /// Refreshes the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItems"/> collection in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// This method refresh the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItems"/> collection when the grid related operations performed.
        /// </remarks>
        protected internal virtual void RefreshSelectedItems()
        {
            if (!this.TreeGrid.HasView)
                return;

            this.SuspendUpdates();
            int index = this.TreeGrid.SelectedItems.Count - 1;
            while (index >= 0)
            {
                object item = this.TreeGrid.SelectedItems[index];
                bool isAvailable = false;
                var treeNode = this.TreeGrid.View.Nodes.GetNode(item);
                if (treeNode != null)
                {
                    isAvailable = true;
                }
                if (!isAvailable)
                {
                    if (this.CurrentCellManager.CurrentCell != null && this.CurrentCellManager.CurrentCell.IsEditing)
                    {
                        this.CurrentCellManager.EndEdit();
                        this.ResetSelectionHandled();
                    }
                    this.TreeGrid.SelectedItems.Remove(item);
                }
                index--;
            }
            var removedRows = this.SelectedRows.ToList<TreeGridRowInfo>();
            this.SelectedRows.Clear();
            index = 0;
            while (index < this.TreeGrid.SelectedItems.Count)
            {
                this.SelectedRows.Add(this.GetTreeGridSelectedRow(this.TreeGrid.ResolveToRowIndex(this.TreeGrid.SelectedItems[index])));
                index++;
            }
            RefreshSelectedIndexAndItem();
            this.ResumeUpdates();
        }


        /// <summary>
        /// Updates the current row index based on <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem"/> property value changes.
        /// </summary>       
        /// <param name="canFocusGrid">
        /// Indicates the SfTreeGrid can be focused after the current row index is updated.
        /// </param>
        protected internal void UpdateCurrentRowIndex(bool canFocusGrid = true)
        {
            if (!this.TreeGrid.HasView)
            {
                if (this.CurrentCellManager.CurrentCell != null)
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                this.CurrentCellManager.ResetCurrentRowColumnIndex();
                return;
            }

            var rowIndex = -1;
            if (this.TreeGrid.CurrentItem != null)
            {
                var node = this.TreeGrid.View.Nodes.GetNode(this.TreeGrid.CurrentItem);
                if (node != null)
                {
                    rowIndex = this.TreeGrid.ResolveToRowIndex(TreeGrid.CurrentItem);
                }
            }
            if (rowIndex == -1 && this.SelectedRows.Any())
            {
                rowIndex = this.SelectedRows.LastOrDefault().RowIndex;
            }

            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex != rowIndex ||
                (this.TreeGrid.HasView && this.TreeGrid.View.CurrentItem != null && this.TreeGrid.CurrentItem != null &&
                 this.TreeGrid.CurrentItem != ((TreeNode)this.TreeGrid.View.CurrentItem).Item))
            {
                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                var rowColumnIndex = new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                if (rowIndex != -1)
                {
                    CurrentCellManager.SelectCurrentCell(rowColumnIndex, canFocusGrid);
                }
                else
                    this.CurrentCellManager.UpdateGridProperties(rowColumnIndex);
            }
            else if (canFocusGrid)
            {
#if UWP
                this.TreeGrid.Focus(FocusState.Programmatic);
#else
                this.TreeGrid.Focus();
#endif
            }
        }

        /// <summary>
        /// Handles the selection when the SelectedItems, Columns and DataSource property collection changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for collection changes.
        /// </param>
        /// <param name="reason">
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCollectionChangedReason"/> contains reason for the collection changes.
        /// </param>
        /// <remarks>
        /// This method is called when the collection changes on SelectedItems, Columns and DataSource properties in SfTreeGrid.
        /// </remarks>
        public override void HandleCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e, TreeGridCollectionChangedReason reason)
        {
            switch (reason)
            {
                case TreeGridCollectionChangedReason.SelectedItemsCollection:
                    ProcessSelectedItemsChanged(e);
                    break;
                case TreeGridCollectionChangedReason.ColumnsCollection:
                    CurrentCellManager.HandleColumnsCollectionChanged(e);
                    break;

                default:
                    ProcessSourceCollectionChanged(e, reason);
                    break;
            }
        }

        /// <summary>
        /// Handles the selection when the node is expanded or collapsed in SfTreeGrid.
        /// </summary>
        /// <param name="index">
        /// The corresponding index of the node.
        /// </param>
        /// <param name="count">
        /// The number of rows that are collapsed or expanded.
        /// </param>
        /// <param name="isExpanded">
        /// Specifies whether the node is expanded or not.
        /// </param>
        /// <remarks>
        /// This method is invoked when the node is expanded or collapsed.
        /// </remarks>
        public override void HandleNodeExpandCollapse(int index, int count, bool isExpanded)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            if (isExpanded)
                ProcessNodeExpanded(index, count);
            else
                ProcessNodeCollapsed(index, count);
        }

        /// <summary>
        /// Processes the selection when the node is expanded/collapsed in SfTreeGrid.
        /// </summary>
        /// <param name="NodeOperation">
        /// Contains the value for collection changes during expanding/collapsing action.
        /// </param>
        /// <remarks>
        /// This method refreshes the selection while expanding/collapsing the node in SfTreeGrid.
        /// </remarks>
        protected internal override void HandleNodeOperations(NodeOperation NodeOpeation)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;

            var removedItems = new List<object>();
            int columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            this.RefreshSelectedItems();
            UpdateCurrentRowIndex();
            this.CurrentCellManager.ScrollInView(new RowColumnIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex, columnIndex));
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }

        /// <summary>
        /// Processes the row selection when the node is expanded in SfTreeGrid.
        /// </summary>
        /// <param name="insertIndex">
        /// The corresponding index of the node that is expanded in to view.
        /// </param>
        /// <param name="count">
        /// inserted rows count.
        /// </param>        
        protected virtual void ProcessNodeExpanded(int insertIndex, int count)
        {
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > insertIndex)
            {
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > insertIndex)
                    this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex + count);
                this.RefreshSelectedItems();
                if (this.PressedRowColumnIndex.RowIndex > (insertIndex - 1))
                    this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex + count, this.PressedRowColumnIndex.ColumnIndex));
            }
            else
            {
                this.RefreshSelectedItems();
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (insertIndex - 1))
                    this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex + count);
            }
        }

        /// <summary>
        /// Processes the row selection when the node is collapsed from view.
        /// </summary>
        /// <param name="removeAtIndex">
        /// The corresponding index of the node that is collapsed from the view.
        /// </param>
        /// <param name="count">
        /// removed rows count.
        /// </param>   
        protected virtual void ProcessNodeCollapsed(int removeAtIndex, int count)
        {
            this.RefreshSelectedItems();
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1) &&
               this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < (removeAtIndex + count))
                this.UpdateCurrentRowIndex();
            else if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1 + count))
                this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex - count);
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedMode"/> property changes.
        /// </param>
        protected override void ProcessSelectionModeChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            if (handle.NewValue == null)
                return;

            this.SuspendUpdates();
            if ((GridSelectionMode)handle.OldValue == GridSelectionMode.Multiple)
                this.TreeGrid.HideRowFocusBorder();
            switch ((GridSelectionMode)handle.NewValue)
            {
                case GridSelectionMode.None:
                    this.ClearSelections(false);
                    this.TreeGrid.UpdateRowHeaderState();
                    this.TreeGrid.RowGenerator.ResetSelection();
                    break;
                case GridSelectionMode.Single:
                    if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= 0)
                    {
                        this.ClearSelections(true);
                    }
                    else
                        this.ClearSelections(false);
                    break;
                case GridSelectionMode.Extended:
                    {
                        var row = TreeGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                        if (row != null && !row.IsSelectedRow)
                        {
                            row.IsSelectedRow = true;
                            var rowInfo = GetTreeGridSelectedRow(row.RowIndex);
                            if (rowInfo != null)
                            {
                                this.SelectedRows.Add(rowInfo);
                                if (row.RowData != null)
                                    this.TreeGrid.SelectedItems.Add(row.RowData);
                            }
                        }
                    }
                    break;
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Do validation and commit edit if the cell is in editing.
        /// </summary>
        /// <param name="rowColumnIndex">current rowColumnIndex</param>
        /// <param name="newRowColumnIndex">new rowcolumnindex to which current cell needs to be set.</param>        
        /// <returns>true if current cell can be moved after committing. Else, false.</returns>
        internal bool CanCommitAndMoveCurrentCell(RowColumnIndex rowColumnIndex, ref RowColumnIndex newRowColumnIndex, bool forceCommit = false)
        {
            if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing ||
                                     (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)))
            {
                object data = null;
                var treeNode = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);
                if (treeNode != null)
                    data = treeNode.Item;
                if ((rowColumnIndex.RowIndex != CurrentCellManager.CurrentRowColumnIndex.RowIndex) || forceCommit)
                {
                    if (!CurrentCellManager.CheckValidationAndEndEdit())
                        return false;
                }
                else
                {
                    if (!CurrentCellManager.RaiseCellValidationAndEndEdit())
                        return false;
                }

                if (isSelectionHandled)
                {
                    ResetSelectionHandled();
                    return false;
                }
                if (data != null)
                    newRowColumnIndex.RowIndex = this.TreeGrid.ResolveToRowIndex(data);
                return true;
            }
            return true;
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NavigationMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NavigationMode"/> property value changes.
        /// </param>
        protected override void ProcessNavigationModeChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            this.ClearSelections(false);
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem"/> property value changes.
        /// </param>
        protected internal override void ProcessCurrentItemChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;

            var rowIndex = this.TreeGrid.ResolveToRowIndex(handle.NewValue);
            if (rowIndex < this.TreeGrid.GetHeaderIndex())
                return;

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            var oldCurrentCellColumnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() ? CurrentCellManager.GetFirstCellIndex() : CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            this.CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, oldCurrentCellColumnIndex), false);
            if (this.TreeGrid.SelectionMode == GridSelectionMode.Single)
            {
                if (SelectedRows.Count > 0)
                    removedItems.Add(this.SelectedRows.FirstOrDefault());

                RemoveSelection(removedItems);
                addedItems.Add(this.GetTreeGridSelectedRow(handle.NewValue));

                AddSelection(addedItems);
                this.CurrentCellManager.SetCurrentRowIndex(rowIndex);
                this.RaiseSelectionChanged(addedItems, removedItems);
            }
            else if (this.TreeGrid.NavigationMode == NavigationMode.Row)
            {
                UpdateRowFocusBorder();
            }
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }
        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedIndex"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedIndex"/> property value changes.
        /// </param>
        protected internal override void ProcessSelectedIndexChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            var newValue = (int)handle.NewValue;
            var oldValue = (int)handle.OldValue;

            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;

            if (this.TreeGrid.View.Nodes.Count == 0)
                throw new InvalidOperationException("SelectedIndex " + TreeGrid.SelectedIndex.ToString() + " doesn't fall with in expected range");

            if (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)
            {
                this.CurrentCellManager.EndEdit();
                ResetSelectionHandled();
            }

            int rowIndex = this.TreeGrid.ResolveToRowIndex(newValue);

            if (rowIndex == -1 || !this.SelectedRows.Contains(rowIndex))
            {
                this.SuspendUpdates();
                var node = this.TreeGrid.GetNodeAtRowIndex(rowIndex);

                var addedItems = new List<object>();
                var removedItems = new List<object>();

                var currentColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, currentColumnIndex), false);

                //Selection should be maintain when the SelectionMode is Multiple, hence added the condition to check the SelectionMode.
                // If SelectedIndex is -1, we should remove selection even though selection mode in multiple.
                if (this.SelectedRows.Count > 0 && (this.TreeGrid.SelectionMode != GridSelectionMode.Multiple || newValue == -1))
                {
                    removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                    this.RemoveSelection(removedItems);
                }

                var rowInfo = this.GetTreeGridSelectedRow(rowIndex);
                if (rowInfo != null)
                {
                    this.SelectedRows.Add(rowInfo);
                    addedItems.Add(rowInfo);
                }
                object selectedItem = null;
                if (node != null)
                    selectedItem = node.Item;

                if (selectedItem != null)
                    this.TreeGrid.SelectedItems.Add(selectedItem);
                if (TreeGrid.SelectionMode == GridSelectionMode.Multiple)
                    this.TreeGrid.HideRowFocusBorder();
                this.TreeGrid.ShowRowSelection(rowIndex, rowInfo != null ? rowInfo.Node : null);
                this.TreeGrid.SelectedItem = selectedItem;

                this.ResumeUpdates();
                this.RaiseSelectionChanged(addedItems, removedItems);
                SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            }
        }
        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItem"/> property value changes.
        /// </param>
        protected internal override void ProcessSelectedItemChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            if (IsSuspended || (handle.NewValue == null && handle.OldValue == null) || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;

            if (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)
            {
                this.CurrentCellManager.EndEdit();
                ResetSelectionHandled();
            }

            int rowIndex = this.TreeGrid.ResolveToRowIndex(handle.NewValue);

            if (rowIndex < this.TreeGrid.GetHeaderIndex() && handle.NewValue != null)
                return;

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            if (handle.NewValue == null)
            {
                if (this.SelectedRows.Count > 0)
                {
                    this.SuspendUpdates();
                    removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                    CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                    this.RemoveSelection(removedItems);
                    this.RefreshSelectedIndexAndItem();
                    this.RaiseSelectionChanged(new List<object>(), removedItems);
                    this.ResumeUpdates();
                    SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
                }
                return;
            }

            if (!this.TreeGrid.SelectedItems.Contains(handle.NewValue))
            {
                this.TreeGrid.HideRowFocusBorder();

                var currentColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, currentColumnIndex), false);

                this.SuspendUpdates();
                //Selection should be maintain when the SelectionMode is Multiple, hence added the condition to check the SelectionMode.
                if (this.SelectedRows.Count > 0 && this.TreeGrid.SelectionMode != GridSelectionMode.Multiple)
                {
                    removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                    this.RemoveSelection(removedItems);
                }

                var rowInfo = GetTreeGridSelectedRow(rowIndex);
                this.SelectedRows.Add(rowInfo);
                this.TreeGrid.ShowRowSelection(rowIndex, rowInfo.Node);
                this.TreeGrid.SelectedItems.Add(handle.NewValue);
                this.TreeGrid.SelectedIndex = this.TreeGrid.ResolveToNodeIndex(rowIndex);
                addedItems.Add(rowInfo);
                this.ResumeUpdates();
                this.RaiseSelectionChanged(addedItems, removedItems);
                SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ItemsSource"/> property collection changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for source collection changes.
        /// </param>
        /// <param name="reason">
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCollectionChangedReason"/> contains reason for the source collection changes.
        /// </param>
        protected virtual void ProcessSourceCollectionChanged(NotifyCollectionChangedEventArgs e, TreeGridCollectionChangedReason reason)
        {
            if (IsSuspended)
                return;
            this.SuspendUpdates();
            var args = e as NotifyCollectionChangedEventArgsExt;
            var focusGrid = args == null ? true : !args.IsProgrammatic;
            // If filtering is applied programmatically, we should not set focus to grid.
            ResetSelectedRows(canFocusGrid: focusGrid);
            this.ResumeUpdates();
        }
        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItems"/> property value changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for SelectedItems collection changes.
        /// </param>
        protected virtual void ProcessSelectedItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            this.SuspendUpdates();
            var addedItems = new List<object>();
            var removedItems = new List<object>();
            //Clear the EditItem when its having, by calling EndEdit method
            if (this.TreeGrid.HasView && this.TreeGrid.View.IsEditingItem)
            {
                this.CurrentCellManager.EndEdit();
                ResetSelectionHandled();
            }
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (this.TreeGrid.SelectionMode == GridSelectionMode.Single && this.TreeGrid.SelectedItems.Count > 1)
                            throw new InvalidOperationException("Cannot able to Add more than one item in SelectedItems collection when SelectionMode is 'Single'");
                        foreach (var item in e.NewItems)
                        {
                            var rowInfo = GetTreeGridSelectedRow(this.TreeGrid.ResolveToRowIndex(item));
                            if (rowInfo != null)
                            {
                                addedItems.Add(rowInfo);
                                this.SelectedRows.Add(rowInfo);
                                this.TreeGrid.ShowRowSelection(rowInfo.RowIndex, rowInfo.Node);
                            }
                        }
                        if (addedItems.Count > 0)
                        {
                            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                            int columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(((TreeGridRowInfo)addedItems.LastOrDefault()).RowIndex, columnIndex), false);
                        }
                        this.RefreshSelectedIndexAndItem();
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int rowIndex = this.TreeGrid.ResolveToRowIndex(e.OldItems[0]);
                        if (rowIndex == -1)
                            return;
                        removedItems.Add(this.SelectedRows.FindRowData(e.OldItems[0]));
                        this.RemoveSelection(removedItems);
                        if (TreeGrid.SelectionMode == GridSelectionMode.Single)
                        {
                            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                            this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, -1));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        removedItems = this.TreeGrid.SelectedItems.ToList();
                        this.CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                        this.SelectedRows.Clear();
                        HideAllRowSelectionBorder(false);
                        this.TreeGrid.SelectedItem = null;
                        this.TreeGrid.SelectedIndex = -1;
                        this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                    }
                    break;
            }
            if (this.TreeGrid.SelectionMode == GridSelectionMode.Multiple)
                this.TreeGrid.HideRowFocusBorder();
            this.ResumeUpdates();
            SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            this.RaiseSelectionChanged(addedItems, removedItems);
        }
    }
}
