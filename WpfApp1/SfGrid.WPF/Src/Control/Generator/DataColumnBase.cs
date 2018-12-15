#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Utility;
using System;
using System.ComponentModel;
using Syncfusion.Data.Extensions;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#else
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Controls;
#else
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using Syncfusion.Data;
    using System.Windows.Media;
    using System.Windows.Controls;
#endif
    [ClassReference(IsReviewed = false)]
    public abstract class DataColumnBase : IColumnElement, IDisposable 
    {
        #region Fields

        IGridCellRenderer renderer;
        int rowIndex = -1;
        int columnIndex = -1;
        Visibility columnVisibility = Visibility.Visible;
        bool isEditing;
        GridColumn gridColumn = null;
        IndentColumnType indentColumnType;
        bool isCurrentCell = false;
        bool isSelectedColumn;
        private bool isdisposed = false;

        #endregion

        #region Internal Property

        internal FrameworkElement columnElement;
        internal IGridSelectionController SelectionController;
        internal bool IsIndentColumn = false;
        internal int rowSpan = 0;
        internal int columnSpan = 0;
        internal int Level = 1;
        internal bool IsEnsured;
        internal bool isUnBoundRowCell = false;
        internal GridUnBoundRowEventsArgs previousGridUnBoundRowEventsArgs = null;
        internal GridUnBoundRowEventsArgs gridUnBoundRowEventsArgs = null;
        internal bool IsExpanderColumn;
        internal bool isSpannedColumn = false;

        #region lightweight templates properties

#if WPF        
        internal Pen borderPen = new Pen();        
#endif        
#endregion

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

        #endregion

        #region Public property

        /// <summary>
        /// Gets or sets a value indicating whether the current cell selection is visible or not..
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool IsCurrentCell
        {
            get
            {
                return isCurrentCell;
            }
            set
            {
                isCurrentCell = value;
                OnIsCurrentCellChanged();
                OnPropertyChanged("IsCurrentCell");
            }
        }

        public bool IsSelectedColumn
        {
            get { return isSelectedColumn; }
            set
            {
                isSelectedColumn = value;
                OnIsSelectedColumnChanged();
                OnPropertyChanged("IsSelectedColumn");
            }
        }
                
        public bool IsEditing
        {
            get
            {
                return isEditing;
            }
            internal set
            {
                isEditing = value;
                OnPropertyChanged("IsEditing");
            }
        }

        public int ColumnIndex
        {
            get
            {
                return columnIndex;
            }
            internal set
            {
                columnIndex = value;
                OnPropertyChanged("ColumnIndex");
            }
        }

        public int RowIndex
        {
            get
            {
                return rowIndex;
            }
            internal set
            {
                rowIndex = value;
                OnPropertyChanged("RowIndex");
            }
        }

        public GridColumn GridColumn
        {
            get
            {
                return gridColumn;
            }
            internal set
            {
                gridColumn = value;
                OnPropertyChanged("GridColumn");
            }
        }

        public IndentColumnType IndentColumnType
        {
            get
            {
                return indentColumnType;
            }
            set
            {
                indentColumnType = value;
                OnIndentColumnTypeChanged();
                OnPropertyChanged("IndentColumnType");
            }
        }

        /// <summary>
        /// Flag to denote the element is newly created and its needs to be added as Children of OrientedCellsPanel.
        /// </summary>
        internal bool isnewElement = false;
        public FrameworkElement ColumnElement
        {
            get
            {
                return columnElement;
            }
            internal set
            {
                if (value != null && columnElement != value)
                    isnewElement = true;
                
                columnElement = value;
            }
        }

        /// <summary>
        /// which holds the UnBoudnRowCell proeprties.
        /// </summary>
        public GridUnBoundRowEventsArgs GridUnBoundRowEventsArgs
        {
            get
            {
                return gridUnBoundRowEventsArgs;
            }
            internal set
            {
                gridUnBoundRowEventsArgs = value;
            }
        }

        public bool IsSpannedColumn
        {
            get { return isSpannedColumn; }
        }

        #endregion

#if WPF
        public DataColumnBase()
        {            
            borderPen.Brush = Brushes.Black;
        }
#endif


        #region Property Changed

        private void OnColumnVisibilityChanged()
        {
            if (this.ColumnElement != null)
                this.ColumnElement.Visibility = this.columnVisibility;
        }

        private void OnIsCurrentCellChanged()
        {
            var columnElemennt = this.ColumnElement as GridCell;
            if (columnElemennt == null)
                return;

            columnElemennt.CurrentCellBorderVisibility = IsCurrentCell ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnIsSelectedColumnChanged()
        {
            var columnElemennt = this.ColumnElement as GridCell;
            if (columnElemennt == null)
                return;

            columnElemennt.SelectionBorderVisibility = IsSelectedColumn ? Visibility.Visible : Visibility.Collapsed;
        }

        internal void OnGridCellRegionChanged(GridCellRegion cellRegion)
        {
            var columnElemennt = this.ColumnElement as GridCell;
            if (columnElemennt == null)
                return;

            columnElemennt.GridCellRegion = cellRegion.ToString();
        }

        private void OnIndentColumnTypeChanged()
        {
            var columnElemennt = this.ColumnElement as GridIndentCell;
            if (columnElemennt == null)
                return;

            columnElemennt.ColumnType = this.IndentColumnType;
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Prepares the Column element based on the Visible column 
        /// </summary>
        /// <remarks></remarks>
        internal void InitializeColumnElement(object dataContext, bool isInEdit)
        {
            this.ColumnElement = OnInitializeColumnElement(dataContext, isInEdit);
        }

        /// <summary>
        /// Raise the PointerPressed method in the selection controller
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaisePointerPressed(MouseButtonEventArgs args)
        {
            this.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed,args), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        /// <summary>
        /// Raise the pointer released method in selection controller.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaisePointerReleased(MouseButtonEventArgs args)
        {
            this.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, args), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        /// <summary>
        /// Raise the pointer moved method in cell selection controller.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaisePointerMoved(MouseEventArgs args)
        {
            this.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Move, args), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        /// <summary>
        /// This method used to call UpdateToolTip, When mouse entered on GrdiCell.
        /// </summary>
        internal void RaisePointerEntered()
        {
            if (this.Renderer != null)
                this.Renderer.UpdateToolTip(this);
        }

#if !WinRT
        internal void RaisePointerWheel()
        {
            var columnindex = this.IsExpanderColumn ? this.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex : this.ColumnIndex;
            this.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Wheel, null), new RowColumnIndex(this.RowIndex, columnindex));
        }
#endif

        /// <summary>
        /// Raise OnTapped Method in Selection Controller
        /// </summary>
        /// <param name="e">TappedRoutedEventArgs</param>
        internal void OnTapped(TappedEventArgs e)
        {
            if (this.GridColumn != null && this.GridColumn.DataGrid != null)
            {
                var cellTappedEventArgs = new GridCellTappedEventArgs(this.ColumnElement) 
                { 
                    Column = this.GridColumn, 
                    RowColumnIndex = new RowColumnIndex(this.RowIndex, this.ColumnIndex), 
                    Record = this.ColumnElement.DataContext,
#if WPF
                    ChangedButton = e.ChangedButton
#else
                    PointerDeviceType = e.PointerDeviceType
#endif
                };
                this.GridColumn.DataGrid.RaiseCellTappedEvent(cellTappedEventArgs);
            }            
            var columnindex = this.IsExpanderColumn ? this.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex : this.ColumnIndex;
            this.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Tapped, e), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }

        /// <summary>
        /// Raise OnDoubleTapped Method in Selection Controller
        /// </summary>
        /// <param name="e">DoubleTappedRoutedEventArgs</param>
        internal void OnDoubleTapped(DoubleTappedEventArgs e)
        {
            if (this.GridColumn != null && this.GridColumn.DataGrid != null)
            {
                var cellDoubleTappedEventArgs = new GridCellDoubleTappedEventArgs(this.ColumnElement) 
                { 
                    Column = this.GridColumn, 
                    RowColumnIndex = new RowColumnIndex(this.RowIndex, this.ColumnIndex), 
                    Record = this.ColumnElement.DataContext ,
#if WPF
                    ChangedButton = e.ChangedButton
#else
                    PointerDeviceType = e.PointerDeviceType
#endif
                };
                this.GridColumn.DataGrid.RaiseCellDoubleTappedEvent(cellDoubleTappedEventArgs);
            }

            //var columnindex = this.IsExpanderColumn ? this.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex : this.ColumnIndex;
            this.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.DoubleTapped, e), new RowColumnIndex(this.RowIndex, this.ColumnIndex));
        }
        /// <summary>
        /// Specifies to raise tooltip opening event for the cell
        /// </summary>
        /// <param name="tooltip"></param>
        internal bool RaiseCellToolTipOpening(ToolTip tooltip)
        {
            if (this.GridColumn != null && this.GridColumn.DataGrid != null && this.GridColumn.DataGrid.CanCellToolTipOpening())
            {
                var cellToolTipOpeningEventArgs = new GridCellToolTipOpeningEventArgs(this.ColumnElement)
                {
                    Column = this.GridColumn,
                    RowColumnIndex = new RowColumnIndex(this.RowIndex, this.ColumnIndex),
                    Record = this.ColumnElement.DataContext,
                    ToolTip = tooltip
                };
                this.GridColumn.DataGrid.RaiseCellToolTipOpeningEvent(cellToolTipOpeningEventArgs);
            }
            return tooltip.IsEnabled;
        }
        protected void SetBindings(UIElement columnElement)
        {
            if (columnElement is GridIndentCell)
            {
                var element = columnElement as GridIndentCell;
                element.ColumnBase = this;
                element.ColumnType = this.IndentColumnType;                   
            }
            else
            {
                var cell = columnElement as GridCell;
                if (cell == null)
                    return;

                cell.ColumnBase = this;
                cell.CurrentCellBorderVisibility = this.IsCurrentCell ? Visibility.Visible : Visibility.Collapsed;
                cell.SelectionBorderVisibility = this.IsSelectedColumn ? Visibility.Visible : Visibility.Collapsed;
                cell.GridCellRegion = "NormalCell";                                                          
            }
        }

        #endregion

        #region abstract methods

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
        public abstract void UpdateCellStyle(object dataContext);

        /// <summary>
        /// Method which is update the binding and style information of the 
        /// cell when we recycle the cell for scrolling.
        /// </summary>
        /// <remarks></remarks>
        public abstract void UpdateBinding(object dataContext, bool updateCellStyle = true);

#if WPF
        // Render border for GridIndentCells of DataRow, UnboundRow,Caption summary row, caption covered row, table summary row, table summary covered row
        /// <summary>
        /// Invoked when the visual children of cells is render in view.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders.</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>        
        /// <param name="uiElement">The corresponding UiElement that is rendered.</param>
        /// <param name="hasClip">True to render border based on IndentColumnType</param>
        internal void OnRender(DrawingContext dc, Rect cellRect, GridCell uiElement, bool hasClip)
        {
            if (uiElement == null || uiElement is GridHeaderIndentCell || uiElement is GridRowHeaderCell || uiElement is GridRowHeaderIndentCell)
                return;

            var borderThickness = uiElement.BorderThickness;
            cellRect.X = cellRect.X - (borderThickness.Right / 2);
            cellRect.Y = cellRect.Y - (borderThickness.Bottom / 2);
            var borderBrush = uiElement.BorderBrush;
            // Render border for Fixed_NormalCell and Fixed_LastCell only when the row is having Fixed_RowCaption border state.
            if (hasClip)
            {
                if (uiElement.GridCellRegion != "LastColumnCell")
                {
                    // Fixed_NormalCell
                    if (uiElement is GridIndentCell)
                    {
                        if (this.IndentColumnType == IndentColumnType.AfterExpander)
                            this.RenderBorder(dc, this.borderPen, cellRect, borderBrush, borderThickness, false, true, false, true); // Renders Top, Bottom borders.                                                                                
                    }
                }
                else                
                    // Fixed_LastCell
                    this.RenderBorder(dc, this.borderPen, cellRect, borderBrush, borderThickness, false, true, true, true);                
            }
            switch (this.IndentColumnType)
            {
                case IndentColumnType.AfterExpander:
                    this.RenderBorder(dc, this.borderPen, cellRect, borderBrush, borderThickness, false, false, false, true);// Bottom border.
                    break;
                case IndentColumnType.BeforeExpander:
                case IndentColumnType.InDataRow:
                case IndentColumnType.InSummaryRow:
                    this.RenderBorder(dc, this.borderPen, cellRect, borderBrush, borderThickness, false, false, true, false);// Right border.
                    break;
                case IndentColumnType.InExpanderCollapsed:
                    this.RenderBorder(dc, this.borderPen, cellRect, borderBrush, borderThickness, false, false, false, true);// Bottom border.
                    break;
                case IndentColumnType.InExpanderExpanded:
                case IndentColumnType.InTableSummaryRow:
                    break;
                case IndentColumnType.InLastGroupRow:
                case IndentColumnType.InAddNewRow:
                case IndentColumnType.InFilterRow:
                case IndentColumnType.InUnBoundRow:
                    this.RenderBorder(dc, this.borderPen, cellRect, borderBrush, borderThickness, false, false, true, true); // Renders Right, Bottom border.
                    break;
            }                      
        }

        /// <summary>
        /// Render border for cells.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders.</param>
        /// <param name="pen">The corresponding DataColumnBase pen to draw the cell borders.</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="borderBrush">The corresponding border brush of UIElement</param>
        /// <param name="borderThickness">The corresponding border thickness of UIElement</param>
        /// <param name="left">True to draw the left side border of UIElement.</param>
        /// <param name="top">True to draw the top side border of UIElement.</param>
        /// <param name="right">True to draw the right side border of UIElement.</param>
        /// <param name="bottom">True to draw the bottom side border of UIElement.</param>
        internal void RenderBorder(DrawingContext dc, Pen pen, Rect cellRect, Brush borderBrush, Thickness borderThickness, bool left, bool top, bool right, bool bottom)
        {
            pen.Brush = borderBrush;
            if (left)
            {
                if (borderThickness.Left == 0)
                    return;

                pen.Thickness = borderThickness.Left;
                dc.DrawLine(pen, cellRect.TopLeft, cellRect.BottomLeft); // Left border.
            }
            if (top)
            {
                if (borderThickness.Top == 0)
                    return;

                pen.Thickness = borderThickness.Top;
                dc.DrawLine(pen, cellRect.TopLeft, cellRect.TopRight); // Top Border.
            }
            if (right)
            {
                if (borderThickness.Right == 0)
                    return;

                pen.Thickness = borderThickness.Right;
                dc.DrawLine(pen, cellRect.TopRight, cellRect.BottomRight); // Right border.
            }
            if (bottom)
            {
                if (borderThickness.Bottom == 0)
                    return;

                pen.Thickness = borderThickness.Bottom;
                dc.DrawLine(pen, cellRect.BottomLeft, cellRect.BottomRight); // Bottom border.
            }
        }

#endif

#endregion

#region IElement
        public FrameworkElement Element
        {
            get
            {
                return this.columnElement;
            }
        }

        public int Index
        {
            get
            {
                return this.columnIndex;
            }
        }
        public IGridCellRenderer Renderer
        {
            get { return renderer; }
            internal set { renderer = value; }
        }

        public int RowSpan
        {
            get { return this.rowSpan; }
            internal set { this.rowSpan = value; }
        }

        public int ColumnSpan
        {
            get { return this.columnSpan; }                
            internal set { this.columnSpan = value; }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var gridcell = this.ColumnElement as GridCell;
            if (gridcell != null)
                gridcell.OnDataColumnPropertyChanged(propertyName);
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IComparable
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
        #endregion


        #endregion

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DataColumnBase"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DataColumnBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                if (this.renderer != null)
                {
                    this.renderer.UnloadUIElements(this);
                    this.renderer = null;
                }

                if (this.ColumnElement is IDisposable)
                    (this.ColumnElement as IDisposable).Dispose();

#if WPF
                if (this.ColumnElement!=null)
                    this.ColumnElement.CommandBindings.Clear();
#endif
                this.ColumnElement = null;
                this.SelectionController = null;
                this.gridColumn = null;
            }
            isdisposed = true;
        }

        private GridCellRegion gridCellRegion;

        /// <summary>
        /// Gets or Sets the GridCellRegion value.
        /// </summary>
        public GridCellRegion GridCellRegion
        {
            get
            {
                return gridCellRegion;
            }
            set
            {
                gridCellRegion = value;
                OnPropertyChanged("GridCellRegion");
            }
        }

    }
}
