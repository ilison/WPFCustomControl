#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.ComponentModel;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
#else
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;      
#endif
    public abstract class TreeDataColumnBase : ITreeDataColumnElement, INotifyPropertyChanged, IDisposable
    {
        private int columnIndex = -1;

        private Visibility columnVisibility = Visibility.Visible;
        private FrameworkElement columnElement;

        internal bool IsEnsured;

        private TreeGridColumn treeGridColumn = null;
        private TreeColumnType _columnType;

        protected internal TreeDataRowBase DataRow { get; set; }

        internal SfTreeGrid TreeGrid
        {
            get { return DataRow.TreeGrid; }
        }

        public TreeColumnType ColumnType
        {
            get { return _columnType; }
            set { _columnType = value; }
        }

        internal Visibility ColumnVisibility
        {
            get
            {
                return columnVisibility;
            }
            set
            {
                columnVisibility = value;
                OnColumnVisibilityChanged();
            }
        }

        public int ColumnIndex
        {
            get { return columnIndex; }
            internal set { columnIndex = value; }
        }

        public int RowIndex
        {
            get { return DataRow.RowIndex; }
            internal set { DataRow.RowIndex = value; }
        }

        bool isCurrentCell = false;
        /// <summary>
        /// Gets or sets a value indicating whether the current cell selection is visible or not..
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool IsCurrentCell
        {
            get { return isCurrentCell; }
            set
            {
                isCurrentCell = value;
                OnIsCurrentCellChanged();
                RaisePropertyChanged("IsCurrentCell");
            }
        }

        public TreeGridColumn TreeGridColumn
        {
            get { return treeGridColumn; }
            internal set
            {
                treeGridColumn = value;
                RaisePropertyChanged("TreeGridColumn");
            }
        }

        bool isEditing = false;
        public bool IsEditing
        {
            get { return isEditing; }
            internal set
            {
                isEditing = value;
                RaisePropertyChanged("IsEditing");
            }
        }

        private ITreeGridCellRenderer renderer;
        public ITreeGridCellRenderer Renderer
        {
            get { return renderer; }
            internal set { renderer = value; }
        }

        public FrameworkElement ColumnElement
        {
            get { return columnElement; }
            internal set
            {
                columnElement = value;
            }
        }

        private void OnColumnVisibilityChanged()
        {
            if (this.ColumnElement != null)
                this.ColumnElement.Visibility = this.columnVisibility;
        }

        private void OnIsCurrentCellChanged()
        {
            var columnElemennt = this.ColumnElement as TreeGridCell;
            if (columnElemennt == null)
                return;

            columnElemennt.IsCurrentCell = IsCurrentCell;
        }


        #region Selection
        /// <summary>
        /// Raise the PointerPressed method in the selection controller
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaisePointerPressed(MouseButtonEventArgs args)
        {
            this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, args), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        /// <summary>
        /// Raise the pointer released method in selection controller.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaisePointerReleased(MouseButtonEventArgs args)
        {
            this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, args), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }
        /// <summary>
        /// Specifies to raise tooltip opening event for the cell
        /// </summary>
        /// <param name="tooltip"></param>
        internal bool RaiseCellToolTipOpening(ToolTip tooltip)
        {
            if (this.TreeGridColumn != null && this.TreeGridColumn.TreeGrid != null && this.TreeGridColumn.TreeGrid.CanCellToolTipOpening())
            {
                var cellToolTipOpeningEventArgs = new TreeGridCellToolTipOpeningEventArgs(this.ColumnElement)
                {
                    Column = this.TreeGridColumn,
                    RowColumnIndex = new RowColumnIndex(this.RowIndex, this.ColumnIndex),
                    Record = this.ColumnElement.DataContext,
                    Node = this.DataRow.Node,
                    ToolTip = tooltip
                };
                this.TreeGridColumn.TreeGrid.RaiseCellToolTipOpeningEvent(cellToolTipOpeningEventArgs);
            }
            return tooltip.IsEnabled;
        }
        /// <summary>
        /// Specifies to set tooltip for the TreeGridCell
        /// </summary>
        internal void RaisePointerEntered()
        {
            if (this.Renderer != null)
                this.Renderer.UpdateToolTip(this);
        }
        /// <summary>
        /// Raise the pointer moved method in cell selection controller.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaisePointerMoved(MouseEventArgs args)
        {
            this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Move, args), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

#if WPF
        internal void RaisePointerWheel()
        {            
            this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Wheel, null), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }
#endif

        /// <summary>
        /// Raise OnTapped Method in Selection Controller
        /// </summary>
        /// <param name="e">TappedRoutedEventArgs</param>
        internal void OnTapped(TappedEventArgs e)
        {
            if (this.TreeGridColumn != null && this.TreeGridColumn.TreeGrid != null && this.TreeGridColumn.TreeGrid.CanCellTapped())
            {
                var cellTappedEventArgs = new TreeGridCellTappedEventArgs(this.ColumnElement)
                {
                    Column = this.TreeGridColumn,
                    RowColumnIndex = new RowColumnIndex(this.RowIndex, this.ColumnIndex),
                    Record = this.ColumnElement.DataContext,
                    Node = this.DataRow.Node,
#if WPF
                    ChangedButton = e.ChangedButton
#else
                    PointerDeviceType = e.PointerDeviceType
#endif
                };
                this.TreeGridColumn.TreeGrid.RaiseCellTappedEvent(cellTappedEventArgs);
            }
            this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Tapped, e), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        /// <summary>
        /// Raise OnDoubleTapped Method in Selection Controller
        /// </summary>
        /// <param name="e">DoubleTappedRoutedEventArgs</param>
        internal void OnDoubleTapped(DoubleTappedEventArgs e)
        {
            if (this.TreeGridColumn != null && this.TreeGridColumn.TreeGrid != null && this.TreeGridColumn.TreeGrid.CanCellDoubleTapped())
            {
                var cellDoubleTappedEventArgs = new TreeGridCellDoubleTappedEventArgs(this.ColumnElement)
                {
                    Column = this.TreeGridColumn,
                    RowColumnIndex = new RowColumnIndex(this.RowIndex, this.ColumnIndex),
                    Record = this.ColumnElement.DataContext,
                    Node = this.DataRow.Node,
#if WPF
                    ChangedButton = e.ChangedButton
#else
                    PointerDeviceType = e.PointerDeviceType
#endif
                };
                this.TreeGridColumn.TreeGrid.RaiseCellDoubleTappedEvent(cellDoubleTappedEventArgs);
            }
            this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.DoubleTapped, e), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Prepares the Column element based on the Visible column 
        /// </summary>
        /// <remarks></remarks>
        internal void InitializeColumnElement(object dataContext, bool isInEdit)
        {
            this.ColumnElement = OnInitializeColumnElement(dataContext, isInEdit);
        }

        /// <summary>
        /// Prepares the Column element based on the Visible column 
        /// </summary>
        /// <remarks></remarks>
        protected abstract FrameworkElement OnInitializeColumnElement(object dataContext, bool IsInEdit);

        /// <summary>
        /// When we scroll the Grid vertically row's will be recycled. 
        /// While recycling we need to update the style info of all the cell's in old row.
        /// This property change call back will update the style info of all the cell element when the row index changed.
        /// </summary>
        /// <remarks></remarks>
        public abstract void UpdateCellStyle();

        /// <summary>
        /// Method which is update the binding and style information of the 
        /// cell when we recycle the cell for scrolling.
        /// </summary>
        /// <remarks></remarks>
        public abstract void UpdateBinding(object dataContext, bool updateCellStyle = true);

        internal void RaiseExpandNode()
        {
            this.TreeGrid.ExpandNode(this.RowIndex);
        }

        internal void RaiseCollapseNode()
        {
            this.TreeGrid.CollapseNode(this.RowIndex);
        }

        internal void SetBindings(UIElement columnElement)
        {
            var cell = columnElement as TreeGridCell;
            if (cell == null)
                return;

            cell.ColumnBase = this;

            if (this.ColumnType == TreeColumnType.RowHeader)
                return;

            cell.IsCurrentCell = IsCurrentCell;
        }

        public FrameworkElement Element
        {
            get { return this.columnElement; }
        }

        public int Index
        {
            get { return this.columnIndex; }
        }

        public int CompareTo(object obj)
        {
            IElement thisdc = this;
            var dc = obj as IElement;
            if (dc != null)
            {
                if (thisdc.Index > dc.Index)
                    return 1;
                else if (thisdc.Index < dc.Index)
                    return -1;
                else
                    return 0;
            }
            return 0;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataColumnBase"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataColumnBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.ColumnElement is IDisposable)
                    (this.ColumnElement as IDisposable).Dispose();
                if (this.renderer != null)
                {
                    this.renderer.UnloadUIElements(this);
                    this.renderer = null;
                }
                this.DataRow = null;
                this.ColumnElement = null;
                this.treeGridColumn = null;
            }
        }
    }
}
