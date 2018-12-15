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
using Syncfusion.UI.Xaml.Grid.RowFilter;
using Syncfusion.Data;
#if WinRT || UNIVERSAL
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
using System.Windows.Resources;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using System.Globalization;
using System.Diagnostics;
#endif


namespace Syncfusion.UI.Xaml.Grid.Cells
{
   
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using Windows.UI.Core;
#endif
    /// <summary>
    /// VirtualizingCellRendererBase is an abstract base class for cell renderers
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
    [ClassReference(IsReviewed = false)]
    public abstract class GridVirtualizingCellRendererBase<D,E> : GridCellRendererBase
          where D : FrameworkElement, new()
          where E : FrameworkElement, new()
    {
        #region Fields

        private bool allowRecycle = true;
        //AgunaCapital incident: 136598. we set readonly
        protected readonly VirtualizingCellUIElementBin<D> DisplayRecycleBin = new VirtualizingCellUIElementBin<D>();
        protected readonly VirtualizingCellUIElementBin<E> EditRecycleBin = new VirtualizingCellUIElementBin<E>();
        protected readonly VirtualizingCellUIElementBin<ContentControl> TemplateRecycleBin = new VirtualizingCellUIElementBin<ContentControl>();
        private bool isdisposed = false;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase"/> class.
        /// </summary>
        public GridVirtualizingCellRendererBase()
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
        /// <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase"/> overrides this method and
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
        protected internal override FrameworkElement OnPrepareUIElements(DataColumnBase dataColumn,object record, bool isInEdit)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex,dataColumn.ColumnIndex);
            FrameworkElement cellContainer = dataColumn.ColumnElement;
            GridColumn column = dataColumn.GridColumn;
            FrameworkElement cellcontent = null;            

            // Create GridCell only for editable columns
            // UseOnlyRendererElement for Non-Editable columns
            if (!UseOnlyRendererElement && cellContainer == null)
                cellContainer = base.OnPrepareUIElements(dataColumn, record,isInEdit);

            if (this.SupportsRenderOptimization && !isInEdit)
            {
                if ((!column.hasCellTemplate && !column.hasCellTemplateSelector) || this.DataGrid.IsFilterRowIndex(cellRowColumnIndex.RowIndex))
                {
                    // Cell Content will be created for Non Template cells.
                    cellcontent = CreateOrRecycleDisplayUIElement();
                    InitializeDisplayElement(dataColumn, (D)cellcontent, record);
                    WireDisplayUIElement((D)cellcontent);                    
                }
                else
                {                    
                    // We wont create Cell Content for Templated cells. 
                    // GridCell is used as RendererElement with template case.
                    InitializeTemplateElement(dataColumn, (ContentControl)cellContainer,record);
                    WireTemplateUIElement((ContentControl)cellContainer);                  
                }
                if (cellcontent != null)                
                    (cellContainer as GridCell).Content = cellcontent;                
            }
            else
            {
                cellcontent = CreateOrEditRecycleUIElement();
                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = true;
                InitializeEditElement(dataColumn,(E)cellcontent, record);
                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = false;

                WireEditUIElement((E)cellcontent);

                // GridImageColumn, GridHyperLinkColumn and GridCheckBoxColumn are Noneditable columns. 
                //So content created and set to GridCell.      
                if (cellcontent != null && cellContainer is GridCell)
                    (cellContainer as GridCell).Content = cellcontent;
            }
            return UseOnlyRendererElement ?cellcontent : cellContainer;           
        }
        
        /// <summary>
        /// Resets the GridCell's content such as ContentTemplate and ContentTemplateSelector for reuse purpose.
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
        public override bool BeginEdit(RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, GridColumn column, object record)
        {
            if (!this.HasCurrentCellState)
                return false;

            if (this.SupportsRenderOptimization)
            {
                E cellcontent = null;                

                if (!UseOnlyRendererElement && cellElement == null)
                    throw new Exception("Cell Element will not be get null for any case");

                var dataColumn = (cellElement as GridCell).ColumnBase;
                
                OnUnloadUIElements(dataColumn);

                // Cell content will be null for templated case always.
                cellcontent = !this.DataGrid.IsFilterRowIndex(cellRowColumnIndex.RowIndex) && (column.IsTemplate && ((column as GridTemplateColumn).hasEditTemplate || (column as GridTemplateColumn).hasEditTemplateSelector)) ?
                               null : CreateOrEditRecycleUIElement();

                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = true;
                InitializeEditElement(dataColumn,(E)(cellcontent ?? cellElement),record);
                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = false;
                
                WireEditUIElement((E)(cellcontent ?? cellElement));

                if (cellcontent != null)
                    (cellElement as GridCell).Content = cellcontent;
                   
                OnEnteredEditMode(dataColumn, cellcontent ?? cellElement);
            }
            else            
                OnEnteredEditMode(null,this.CurrentCellRendererElement);
            
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
        protected virtual void OnEnteredEditMode(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
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
        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
        {
            var cellRowColumnIndex = new RowColumnIndex(dc.RowIndex, dc.ColumnIndex);
            var cellElement = dc.ColumnElement;
            var column = dc.GridColumn;

            if (!this.HasCurrentCellState)
                return false;
            if (!this.IsInEditing)
                return false;
            if (this.SupportsRenderOptimization)
            {
#if WPF
                E uiElement = null;
                if (!UseOnlyRendererElement && cellElement is GridCell)
                    uiElement = (E)((cellElement as GridCell).Content is FrameworkElement ? (cellElement as GridCell).Content as FrameworkElement : cellElement);
                else
                    uiElement = cellElement as E;
                uiElement.PreviewLostKeyboardFocus -= OnLostKeyboardFocus;
#endif
                //this.IsFocused = false;
                this.SetFocus(false);
                var dataColumn = (cellElement as GridCell).ColumnBase;

                OnEditingComplete(dataColumn, CurrentCellRendererElement);
                if (!UseOnlyRendererElement && cellElement == null)
                    throw new Exception("Cell Element will not be get null for any case");
                OnUnloadUIElements(dataColumn);

                OnPrepareUIElements(dataColumn, record, false);

                if (!column.hasCellTemplate && !column.hasCellTemplateSelector)
                    UpdateCurrentCellState((cellElement as GridCell).Content as FrameworkElement, false);
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
        protected virtual void OnEditingComplete(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            
        }

        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase &lt; D,E &gt;"/> overrides this method and either removes the cell renderer visuals from the parent
        /// or hide them and reuse it later in same element depending on whether <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase&lt; D,E &gt;.AllowRecycle"/>  was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>                     
        protected override void OnUnloadUIElements(DataColumnBase dataColumn)
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

#if WPF

        /// <summary>
        /// Invoked when the visual children of cells is render in view.
        /// </summary>
        /// <param name="dataColumnBase">The DataColumnBase that provides the cells details.</param>
        /// <param name="dc">The corresponding drawing context to draw the cell borders, cell content, cell background</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="dataContext">The corresponding record to draw a text for GridCell</param>
        protected override void OnRenderCell(DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {
            // Render cell background.
            var gridCell = dataColumnBase.ColumnElement as GridCell;
            if (gridCell == null)
                return;
       
            VisibleLineInfo visibleColumnInfo = null;
            if(dataColumnBase.ColumnElement.Clip != null)
                visibleColumnInfo = DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumnBase.ColumnIndex);
            var needClip = false;
            RectangleGeometry clipGeometry = null;
            if (visibleColumnInfo != null)
            {
                var coveredCell = dataColumnBase.IsSpannedColumn ? DataGrid.CoveredCells.GetCoveredCell(dataColumnBase.RowIndex, dataColumnBase.ColumnIndex, dataColumnBase.GridColumn, dataColumnBase.ColumnElement.DataContext) : null;
                var y = coveredCell != null ? (coveredCell.Top != dataColumnBase.RowIndex ? dataColumnBase.ColumnElement.Clip.Bounds.Y + cellRect.Y : cellRect.Y) : cellRect.Y;
                var x = coveredCell != null ? (coveredCell.Left != dataColumnBase.ColumnIndex ? dataColumnBase.ColumnElement.Clip.Bounds.X + cellRect.X : visibleColumnInfo.ClippedOrigin) : visibleColumnInfo.ClippedOrigin;
                var clippedSize = dataColumnBase.ColumnElement is GridCaptionSummaryCell || dataColumnBase.ColumnElement is GridGroupSummaryCell || dataColumnBase.ColumnElement is GridTableSummaryCell ? cellRect.Width : (dataColumnBase.IsSpannedColumn ? cellRect.Width : visibleColumnInfo.ClippedSize);
                var clipRect = new Rect(x, y, clippedSize, cellRect.Height);
                clipGeometry = new RectangleGeometry(clipRect);                
                clipGeometry.Freeze();
                dc.PushClip(clipGeometry);
                needClip = true;
            }
            var background = gridCell.Background;
            dc.DrawRectangle(background, null, cellRect);         

            // Render CurrentCell background and border.                                                    
            OnRenderCurrentCell(dc, cellRect, clipGeometry, dataColumnBase, gridCell);
            // Render Cell border.
            OnRenderCellBorder(dc, cellRect, clipGeometry, dataColumnBase, gridCell);
            // Render cell value.
            OnRenderContent(dc, cellRect, clipGeometry, dataColumnBase, gridCell, dataContext);
            if (needClip)
                dc.Pop();
        }

        protected override void OnRenderCellBorder(DrawingContext dc, Rect cellRect, Geometry clipGeometry  ,DataColumnBase dataColumnBase, GridCell gridCell)        
        {
            if (dataColumnBase.IsEditing)
                return;

            var borderBursh = gridCell.BorderBrush;
            var borderThickness = gridCell.BorderThickness;
            var needClip = false;
            if (clipGeometry != null)
            {
                clipGeometry.Freeze();                    
                dc.PushClip(clipGeometry);
                needClip = true;
            }                      
            cellRect.Y = cellRect.Y - (borderThickness.Bottom / 2);
            switch (gridCell.GridCellRegion)
            {
                case "NormalCell":
                case "FrozenColumnCell":
                    cellRect.X = cellRect.X - (borderThickness.Right / 2);
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen,cellRect, borderBursh, borderThickness, false, false, true, true);// Renders Right, Bottom borders.                    
                    break;
                case "FooterColumnCell":
                    cellRect.X = cellRect.X + (borderThickness.Right / 2);
                    cellRect.Width -= borderThickness.Right;
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, true, false, true, true);// Renders Left, Right, Bottom borders.                    
                    break;
                case "BeforeFooterColumnCell":
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, false, false, true);//Renders Bottom border.
                    break;
                case "Fixed_LastCell":
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, true, true, true);// Renders Top, Right, Bottom borders.                                 
                    break;
                case "Fixed_NormalCell":
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, true, false, true);// Renders Top, Bottom borders.                                                       
                    break;
                default:
                    cellRect.X = cellRect.X - (borderThickness.Right / 2);
                    dataColumnBase.RenderBorder(dc, dataColumnBase.borderPen, cellRect, borderBursh, borderThickness, false, false, true, true); // Renders right, bottom borders              
                    break;
            }   
            if (needClip)
                dc.Pop();                 
        }

        protected override void OnRenderContent(DrawingContext dc, Rect cellRect,Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            if (dataColumnBase.IsEditing || dataColumnBase.GridColumn.hasCellTemplate || dataColumnBase.GridColumn.hasCellTemplateSelector || (DataGrid.hasCellTemplateSelector && dataColumnBase.GridColumn.IsTemplate))
                return;

            if (DataGrid.Provider == null || DataGrid.View == null)
                return;          

            // WPF-34216 - dataContext can be null when rendering AddNewRow
            dynamic value = dataColumnBase.GridUnBoundRowEventsArgs != null? dataColumnBase.GridUnBoundRowEventsArgs.Value : dataContext != null ?
                DataGrid.Provider.GetDisplayValue(dataContext, dataColumnBase.GridColumn.MappingName, dataColumnBase.GridColumn.UseBindingValue) :null ;

            string cellvalue = value != null ? value.ToString() : string.Empty;

            if (string.IsNullOrWhiteSpace(cellvalue))
                return;

             if (dataColumnBase.GridColumn.textWrapping != TextWrapping.NoWrap || cellvalue.Contains('\n'))
                DrawFormattedText(dc, cellRect,clipGeometry, dataColumnBase, gridCell, cellvalue);
             else
                DrawGlyphs(dc, cellRect, clipGeometry, dataColumnBase, gridCell,cellvalue);
        }

        protected virtual void DrawFormattedText(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, string cellvalue)
        {                                                       
            var contentrect = AddBorderMargins(cellRect, dataColumnBase.GridColumn.padding);
            var needClip = false;
            if (clipGeometry != null)
            {
                clipGeometry.Freeze();
                dc.PushClip(clipGeometry);
                needClip = true;
            }                        
            var formattedText = new FormattedText(cellvalue, CultureInfo.CurrentCulture, DataGrid.FlowDirection, dataColumnBase.GridColumn.GetTypeface(dataColumnBase.GridColumn,gridCell), gridCell.FontSize, gridCell.Foreground);                        
            formattedText.SetTextDecorations(dataColumnBase.GridColumn.textDecoration);
            formattedText.SetForegroundBrush(gridCell.Foreground);          
            if (contentrect.Width > 0 && contentrect.Height > 0)
            {
                formattedText.MaxTextWidth = contentrect.Width;
                formattedText.MaxTextHeight = contentrect.Height;
                formattedText.TextAlignment = dataColumnBase.GridColumn.textAlignment;
                formattedText.Trimming = dataColumnBase.GridColumn.textTrimming;
                if (formattedText.Trimming != TextTrimming.None)
                    formattedText.MaxLineCount = 1;
            }

            double yOffset = (contentrect.Height - formattedText.Height);
            switch (dataColumnBase.GridColumn.verticalAlignment)
            {
                case VerticalAlignment.Center:
                    contentrect.Y += (yOffset / 2);
                    break;

                case VerticalAlignment.Top:
                    contentrect.Y -= 1;
                    break;

                case VerticalAlignment.Bottom:
                    contentrect.Y += yOffset;
                    break;
            }      
            dc.DrawText(formattedText, contentrect.TopLeft);
            if (needClip)
                dc.Pop();   
        }

        protected virtual void DrawGlyphs(DrawingContext dc, Rect cellRect,Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell,string cellvalue)
        {
            GlyphTypeface glyphTypeface;
            if (!dataColumnBase.GridColumn.GetTypeface(dataColumnBase.GridColumn, gridCell).TryGetGlyphTypeface(out glyphTypeface))
            {
                DrawFormattedText(dc, cellRect,clipGeometry, dataColumnBase, gridCell, cellvalue);
                return;
            }            

            ushort[] glyphIndexes = new ushort[cellvalue.Length];
            double[] advanceWidths = new double[cellvalue.Length];            
            double totalWidth = 0;

            var lineHeight = GetLineHeightValue(gridCell);
            for (int n = 0; n < cellvalue.Length; n++)
            {
                ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[cellvalue[n]];
                glyphIndexes[n] = glyphIndex;

                double width = glyphTypeface.AdvanceWidths[glyphIndex] * gridCell.FontSize;
                advanceWidths[n] = width;
                
                totalWidth += width;
            }

            var contentrect = AddBorderMargins(cellRect, dataColumnBase.GridColumn.padding);            
            var xOffset = contentrect.Width - totalWidth;

            double xPos = 0, yPos = 0;
            switch (dataColumnBase.GridColumn.verticalAlignment)
            {                                   
                case VerticalAlignment.Top:
                    yPos = contentrect.Y + gridCell.FontSize;
                    break;

                case VerticalAlignment.Bottom:
                    yPos = contentrect.Height - (dataColumnBase.GridColumn.padding.Top + dataColumnBase.GridColumn.padding.Bottom);                    
                    break;
                default:
                    yPos = (contentrect.Bottom / 2) + (gridCell.FontSize / 2);
                    break;
            }           
            switch (dataColumnBase.GridColumn.textAlignment)
            {
                case TextAlignment.Left:
                    xPos = contentrect.X;
                    break;

                case TextAlignment.Right:
                    xPos = contentrect.Right - totalWidth;
                    break;

                default:
                    xPos = contentrect.X + (xOffset > 0 ? (xOffset /2) : 0);
                    break;
            }
            
            var needClip = false;
            if (clipGeometry != null)
            {             
                clipGeometry.Freeze();
                dc.PushClip(clipGeometry);
                needClip = true;
            }
            else if(totalWidth > contentrect.Width || lineHeight > contentrect.Height)
            {
                var rectgeometry = new RectangleGeometry(cellRect);
                rectgeometry.Freeze();
                dc.PushClip(rectgeometry);
                needClip = true;
            }                     
            var origin = new Point(xPos, yPos);  
          
            GlyphRun gr = new GlyphRun(
             glyphTypeface,
             0,       // Bi-directional nesting level
             false,   // isSideways
             gridCell.FontSize,      // pt size
             glyphIndexes,   // glyphIndices
             origin,     // baselineOrigin
             advanceWidths,  // advanceWidths
             null,    // glyphOffsets
             null,    // characters
             null,    // deviceFontName
             null,    // clusterMap
             null,    // caretStops
             null);   // xmlLanguage                        
            dc.DrawGlyphRun(gridCell.Foreground, gr);
            if (needClip)
                dc.Pop();   
        }


        private double GetLineHeightValue(Control control)
        {
            var _scale = 300.0;
            var _maxSizeInt = 0x3ffffffe;
            var _minSizeInt = 0x00000001;
            var _maxSize = ((double)_maxSizeInt) / _scale; // = 3,579,139.40 pixels 
            var _minSize = ((double)_minSizeInt) / _scale;
#if WPF
            double lineHeight = control.FontFamily.LineSpacing * control.FontSize;
#else
            double lineHeight = control.FontSize;
#endif
            return Math.Max(_minSize, Math.Min(_maxSize, lineHeight));
        }
      
        private Rect AddBorderMargins(Rect cellRect, Thickness thickness)
        {
            if (cellRect.IsEmpty)
                return Rect.Empty;
            var x = cellRect.X + thickness.Left;
            var y = cellRect.Y + thickness.Top;
            var width = cellRect.Width - (thickness.Right + thickness.Left);
            var height = cellRect.Height - (thickness.Bottom + thickness.Top);
            var textRect = new Rect(x, y, width < 0 ? 0 : width, height < 0 ? 0 : height);
            return textRect;            
        }

        protected override void OnRenderCurrentCell(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell)
        {            
            if (!dataColumnBase.IsCurrentCell && !dataColumnBase.IsSelectedColumn)
                return;          

            var needClip = false;                   
            var borderThickness = gridCell.CurrentCellBorderThickness;
            var backgrounBrush = gridCell.CellSelectionBrush;
            if (clipGeometry != null)
            {
                clipGeometry.Freeze();
                dc.PushClip(clipGeometry);
                needClip = true;
            }   
            var widthAdjustValue = cellRect.Width - (borderThickness.Right / 2);
            cellRect.Width = widthAdjustValue >  0  ?  widthAdjustValue : cellRect.Width;
            var heightAdjustValue = cellRect.Height - (borderThickness.Bottom / 2);
            cellRect.Height = heightAdjustValue > 0 ? heightAdjustValue : cellRect.Height;
            if (dataColumnBase.IsCurrentCell)
            {                
                var renderCurrentCellBackground = DataGrid.SelectionUnit == GridSelectionUnit.Cell ? dataColumnBase.IsSelectedColumn : false;
                dc.DrawRectangle(renderCurrentCellBackground ? backgrounBrush : null, dataColumnBase.IsCurrentCell ? new Pen(DataGrid.CurrentCellBorderBrush, borderThickness.Right) : null, cellRect);
            }
            else if (dataColumnBase.IsSelectedColumn)
                dc.DrawRectangle(backgrounBrush, null, cellRect);            
                            
            if (!needClip)
                return;
            dc.Pop();
        }
        
#endif
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
        protected internal override void OnUpdateBindingInfo(DataColumnBase dataColumn,object record, bool isInEdit)
        {            
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex) ;
            FrameworkElement uiElement = dataColumn.ColumnElement;
            GridColumn column = dataColumn.GridColumn;
            FrameworkElement rendererElement = null;

            if (UseOnlyRendererElement)
                rendererElement = uiElement as FrameworkElement;
            else if (uiElement is GridCell)
                rendererElement = (uiElement as GridCell).Content is FrameworkElement ? (uiElement as GridCell).Content as FrameworkElement : uiElement;            

            if (this.SupportsRenderOptimization && !isInEdit)
            {
                if ((!column.hasCellTemplate && !column.hasCellTemplateSelector) || this.DataGrid.IsFilterRowIndex(cellRowColumnIndex.RowIndex))
                {
#if WPF
                    if (DataGrid.useDrawing)
                        return;
#endif
                    OnUpdateDisplayBinding(dataColumn, (D)rendererElement, record);
                }
                else
                    OnUpdateTemplateBinding(dataColumn, (ContentControl)rendererElement, record);
            }
            else
                OnUpdateEditBinding(dataColumn, (E)rendererElement,record);
        }


        /// <summary>
        /// Updates the style for the particular column.
        /// Implement this method to update style when the cell UIElement is reused during scrolling.          
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the corresponding column to update style.
        /// </param>   
        protected sealed override void OnUpdateStyleInfo(DataColumnBase dataColumn, object dataContext)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            FrameworkElement uiElement = dataColumn.ColumnElement;
            GridColumn column = dataColumn.GridColumn;

            if (uiElement.Visibility == Visibility.Collapsed) return;          
            this.InitializeCellStyle(dataColumn, dataContext);            
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
        public abstract void OnInitializeDisplayElement(DataColumnBase dataColumn, D uiElement, object dataContext);

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
        public abstract void OnUpdateDisplayBinding(DataColumnBase dataColumn, D uiElement, object dataContext);

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
        public abstract void OnInitializeTemplateElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext);

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
        public abstract void OnUpdateTemplateBinding(DataColumnBase dataColumn, ContentControl uiElement, object dataContext);

        /// <summary>
        /// Invoked when the edit element is initialized on the cell.
        /// </summary>
        /// <param name="dataColumn">
        ///  The dataColumn where the cell is located.
        /// </param>
        /// <param name="element">
        /// The element that is initialized on the edit element of cell.
        /// </param>
        /// <param name="dataContext">
        /// The dataContext of the cell.
        /// </param>
        public abstract void OnInitializeEditElement(DataColumnBase dataColumn, E uiElement,object dataContext);

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
        public abstract void OnUpdateEditBinding(DataColumnBase dataColumn, E element,object dataContext);

        #endregion

        #region public methods

        /// <summary>
        /// Initializes an edit element of the cell in column.
        /// </summary>
        /// <param name="dataColumn">
        /// The dataColumn where the cell is located.
        /// </param>
        /// <param name="element">
        /// The element that is initialized on the edit element of cell.
        /// </param>
        /// <param name="dataContext">
        /// The dataContext of the cell.
        /// </param>
        public void InitializeEditElement(DataColumnBase dataColumn, E uiElement, object dataContext)
        {
            OnInitializeEditElement(dataColumn, uiElement, dataContext);
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
        public virtual void InitializeDisplayElement(DataColumnBase dataColumn, D uiElement,object dataContext)
        {
#if WPF
            if (DataGrid.useDrawing)
                return;
#endif
            OnInitializeDisplayElement(dataColumn, uiElement,dataContext);
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
        public void InitializeTemplateElement(DataColumnBase dataColumn,ContentControl uiElement, object dataContext)
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

        internal virtual E CreateOrEditRecycleUIElement()
        {
            E uiElement;
            if (AllowRecycle)
            {
#if UWP
                //UWP-4815 Because of recycling, when entering edit mode LostFocus event get called for currently edited cell with NewValue as nulll.
                //This issue happening only when OnTab and editing the same column in by clicking the mouse. 
                //So, the EditRecycleBin is dequeued one item behind for UWP platform alone.
                if (EditRecycleBin.Count > 1)
                {
                    uiElement = EditRecycleBin.Dequeue(this);
                    if (uiElement != null)
                        return uiElement;
                }
#else
                uiElement = EditRecycleBin.Dequeue(this);

                if (uiElement != null)
                    return uiElement;
#endif
            }
            uiElement = OnCreateEditUIElement();
            return uiElement;
        }

        /// <summary>
        /// To unload edit elemnets  
        /// </summary>
        /// <param name="uiElements"></param>
        /// <param name="column">Need column to check if column has HeaderTemplate</param> 
        private void UnloadEditUIElement(FrameworkElement uiElements, DataColumnBase column)
        {
            E uiElement = null;
            if (!UseOnlyRendererElement && uiElements is GridCell)
                uiElement = (E)((uiElements as GridCell).Content is FrameworkElement ? (uiElements as GridCell).Content as FrameworkElement : uiElements);
            else
            {
                uiElement = uiElements as E;
                column.columnElement = null;
            }

            if (uiElement != null)
            {
                UnwireEditUIElement(uiElement);
                if (AllowRecycle && !(uiElement is GridCell))
                    EditRecycleBin.Enqueue(this, uiElement);

                if (!UseOnlyRendererElement && uiElements is GridCell)
                    ResetGridCell((ContentControl)uiElements);
                else if (uiElement.Parent is Panel)
                {
                    if (column.GridColumn != null && column.GridColumn.hasHeaderTemplate || (DataGrid != null && DataGrid.hasHeaderTemplate))
                        ResetGridCell((ContentControl)uiElements);
                    (uiElement.Parent as Panel).Children.Remove(uiElement);
                }
            }
        }

        private void UnloadDisplayUIElement(FrameworkElement uiElements, DataColumnBase column)
        {
            D uiElement = null;           
            if (!(column.GridColumn.hasCellTemplate || column.GridColumn.hasCellTemplateSelector))
            {
                if (!UseOnlyRendererElement && uiElements is GridCell)
                    uiElement = (uiElements as GridCell).Content as D;
                else
                    uiElement = uiElements as D;
            }

            if (uiElement != null)
            {
                UnwireDisplayUIElement(uiElement);
                if (AllowRecycle)
                    DisplayRecycleBin.Enqueue(this, uiElement);
            }

            if (!UseOnlyRendererElement && uiElements is GridCell)
                ResetGridCell(uiElements as GridCell);
            else if (uiElement.Parent is Panel)
                (uiElement.Parent as Panel).Children.Remove(uiElement);           
        }

        internal void WireDisplayUIElement(D uiElamant)
        {
#if WPF
            if(DataGrid.useDrawing)
                return;
#endif
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
            ValidationHelper.SetFocusSetBack(false);
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
                if (this.CurrentCellElement is GridCell && (this.CurrentCellElement as GridCell).ColumnBase.GridColumn.IsTemplate)
                {
                    if (FocusManagerHelper.GetFocusedUIElement(this.CurrentCellRendererElement) != null)
                        (FocusManagerHelper.GetFocusedUIElement(this.CurrentCellRendererElement)).CaptureMouse();
                }
                else
                {
                    (sender as FrameworkElement).CaptureMouse();
                }
                ValidationHelper.SetFocusSetBack(true);
            }
            GridHeaderCellControl.isFilterToggleButtonPressed = false;
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
#if WinRT  || UNIVERSAL
        protected async virtual void OnEditElementLostFocus(object sender, RoutedEventArgs e)
#else
        protected virtual void OnEditElementLostFocus(object sender, RoutedEventArgs e)
#endif
        {
#if WPF
            //OnLostKeyboardFocus - Take care of validation in WPF
            if (this.HasCurrentCellState && this.CurrentCellRendererElement == sender)
            {
                this.isfocused = false;
                ValidationHelper.SetFocusSetBack(false);                
            }
            GridHeaderCellControl.isFilterToggleButtonPressed = false;
#else
            if(!GridHeaderCellControl.isFilterToggleButtonPressed)
                ValidationHelper.SetFocusSetBack(false);     

            if (this.HasCurrentCellState && this.CurrentCellRendererElement == sender)
                this.isfocused = false;

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
            ValidationHelper.SetFocusSetBack(true); 
            }
            GridHeaderCellControl.isFilterToggleButtonPressed = false;
#endif
        }

        private bool CheckToAllowFocus(object element, object sender)
        {
            //WPF-24276 - If there is no CurrentCell where need to return.
            if (this.DataGrid == null || !this.HasCurrentCellState)
                return true;         

            if((element is GridCell && sender.Equals(((element as GridCell).Content))))
                return true;

            //WPF-24276 - After Raising CellValidation we have to check once again CurrentCellState because customer
            //can made any changes while committing the value in any property.
            if ((this is GridCellTemplateRenderer || this is GridUnBoundCellTemplateRenderer || this.DataGrid.ValidationHelper.RaiseCellValidate(this.CurrentCellIndex, this, true)) && this.HasCurrentCellState)
            {
                if (!(element is GridCell))
                    return this.DataGrid.ValidationHelper.RaiseRowValidate(this.CurrentCellIndex);
                else if ((element as GridCell).ColumnBase != null && (element as GridCell).ColumnBase.RowIndex != this.CurrentCellIndex.RowIndex)
                    return this.DataGrid.ValidationHelper.RaiseRowValidate(this.CurrentCellIndex);

            }
            else
                return false;
            return true;
        }

        /// <summary>
        /// Initializes the custom style for cell when the corresponding API's and Selectors are used.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding DataColumn Which holds GridColumn, RowColumnIndex and GridCell to initialize cell style.
        /// </param>
        /// <param name="record">
        /// The corresponding record to initialize cell style.
        /// </param>        
        protected virtual void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            this.SetCellStyle(dataColumn,record);
        }
        
        /// <summary>
        /// Method which is used to set the Custom style for Cell.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// <param name="record"></param>
        /// <remarks></remarks>
        private void SetCellStyle(DataColumnBase dataColumn,object record)
        {
            var cell = dataColumn.ColumnElement;
            var column = dataColumn.GridColumn;
          
            if (column == null)
                return;
            
            var gridCell = cell as GridCell;
            if (gridCell == null) return;
            if (!column.hasCellStyleSelector && !column.hasCellStyle && !DataGrid.hasCellStyle && !DataGrid.hasCellStyleSelector)
            {
                if (gridCell.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue)
                    gridCell.ClearValue(FrameworkElement.StyleProperty);
                return;
            }
        
            Style newStyle = null;
            Style style = null;

            if (column.hasCellStyleSelector && column.hasCellStyle)
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
            else if (DataGrid.hasCellStyleSelector && DataGrid.hasCellStyle)
            {
                newStyle = DataGrid.CellStyleSelector.SelectStyle(record, cell);
                style = newStyle ?? DataGrid.CellStyle;             
            }
            else if (DataGrid.hasCellStyleSelector)
            {
                style = DataGrid.CellStyleSelector.SelectStyle(record, cell);
            }
            else if (DataGrid.hasCellStyle)
            {
                style = DataGrid.CellStyle;
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
        /// Releases all resources used by the <see cref="T:Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase"/> class.
        /// </summary>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                if (this.DisplayRecycleBin != null)
                {
                    this.DisplayRecycleBin.Clear();
                    // this.DisplayRecycleBin = null; //AgunaCapital incident: 136598
                }
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }

        [Obsolete]
        private ContentControl CreateOrRecycleTemplateUIElement()
        {
            ContentControl uiElement;
            if (AllowRecycle)
            {
                uiElement = TemplateRecycleBin.Dequeue(this);
                if (uiElement != null)
                {
                    return uiElement;
                }
            }
            uiElement = OnCreateTemplateUIElement();
            return uiElement;
        }

        [Obsolete]
        /// <summary>
        /// Creates a new template UIElement for the cell.
        /// </summary>
        /// <returns>
        /// Returns the new template UIElement for the cell. 
        /// </returns>
        protected virtual ContentControl OnCreateTemplateUIElement()
        {
            var uiElement = new ContentControl();
#if WPF
            Validation.SetErrorTemplate(uiElement, null);
#endif
            return uiElement;
        }
    }
}
