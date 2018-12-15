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
using Syncfusion.Data.Extensions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the Search operation which is performed in the DataGrid.
    /// </summary>
    /// <remarks>The SearchHelper provides a flexible way to search the text in the DataGrid. </remarks>
    public class SearchHelper : DependencyObject, INotifyDependencyPropertyChanged, IDisposable
    {
        #region Fields

        /// <summary>
        /// Gets the Datagrid
        /// </summary>
        protected SfDataGrid DataGrid;

        /// <summary>
        /// Gets the provider to reflect the cell value.
        /// </summary>
        protected internal IPropertyAccessProvider Provider = null;

        private RowColumnIndex _currentRowColumnIndex;
        internal bool isSuspended;
        private bool isdisposed = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the RowColumnIndex of the cell that match with search text when <see cref=”Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHElper.FindNext”/> or <see cref=”Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHElper.FindPrevious”/> methods are called.
        /// </summary>
        public RowColumnIndex CurrentRowColumnIndex
        {
            get { return _currentRowColumnIndex; }
            set { _currentRowColumnIndex = value; }
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets a value that enables case-sensitive string comparison during search in SfDataGrid.        /// </summary>
        /// <value><b>true</b> if the case sensitive search enabled; otherwise, <b>false</b>. The default value is <b>false</b>.</value>
        public bool AllowCaseSensitiveSearch
        {
            get { return (bool)GetValue(AllowCaseSensitiveSearchProperty); }
            set { SetValue(AllowCaseSensitiveSearchProperty, value); }
        }
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.AllowCaseSensitiveSearch dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.AllowCaseSensitiveSearch dependency property.
        /// </remarks> 
        public static readonly DependencyProperty AllowCaseSensitiveSearchProperty =
            GridDependencyProperty.Register("AllowCaseSensitiveSearch", typeof(bool), typeof(SearchHelper), new GridPropertyMetadata(false, OnAllowCaseSensitiveSearchChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether to enable filter based on search text.
        /// </summary>
        /// <value><b>true</b> if the DataGrid is filtered when Search is called. Otherwise, <b>false</b>. The default value is <b>false</b>.</value>
        public bool AllowFiltering
        {
            get { return (bool)GetValue(AllowFilteringProperty); }
            set { SetValue(AllowFilteringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.AllowFiltering dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.AllowFiltering dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowFilteringProperty =
            GridDependencyProperty.Register("AllowFiltering", typeof(bool), typeof(SearchHelper), new GridPropertyMetadata(false, OnAllowSearchFilterChanged));

        /// <summary>
        /// Gets or sets a brush that highlights the search text of a cell during FindNext and FindPrevious method calls.
        /// </summary>
        /// <value>The background color of the SearchText in the GridCell when FindNext or FindPrevious is called.</value>
        [Cloneable(false)]
        public Brush SearchHighlightBrush
        {
            get { return (Brush)GetValue(SearchHighlightBrushProperty); }
            set { SetValue(SearchHighlightBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.SearchHighlightBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.SearchHighlightBrush dependency property.
        /// </remarks>
        public static readonly DependencyProperty SearchHighlightBrushProperty =
            GridDependencyProperty.Register("SearchHighlightBrush", typeof(Brush), typeof(SearchHelper), new GridPropertyMetadata(new SolidColorBrush(Colors.LightSalmon), OnSearchHighlightBrushChanged));

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.XAML.Grid.SearchType"/> that denotes how to compare cell values with search text. 
        /// </summary>
        /// <value>One of the enumeration specifying the SearchType. The default value is <b>Syncfusion.UI.XAML.Grid.SearchType.Contains</b></value>
        public SearchType SearchType
        {
            get { return (SearchType)GetValue(SearchTypeProperty); }
            set { SetValue(SearchTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.SearchType dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.SearchType dependency property.
        /// </remarks>
        public static readonly DependencyProperty SearchTypeProperty =
            GridDependencyProperty.Register("SearchType", typeof(SearchType), typeof(SearchHelper), new GridPropertyMetadata(SearchType.Contains));

        /// <summary>
        /// Gets or sets a brush to highlight search text in SfDataGrid.
        /// </summary>
        /// <value>The background color of the SearchText in the GridCell. The default value is Yellow.</value>
        [Cloneable(false)]
        public Brush SearchBrush
        {
            get { return (Brush)GetValue(SearchBrushProperty); }
            set { SetValue(SearchBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.SearchBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SearchHelper.SearchBrush dependency property.
        /// </remarks>
        public static readonly DependencyProperty SearchBrushProperty =
            GridDependencyProperty.Register("SearchBrush", typeof(Brush), typeof(SearchHelper), new GridPropertyMetadata(new SolidColorBrush(Colors.Yellow), OnSearchBrushchanged));

        /// <summary>
        /// Gets the text which is used to search the DataGrid.
        /// </summary>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        public static readonly DependencyProperty SearchTextProperty =
            GridDependencyProperty.Register("SearchText", typeof(string), typeof(SearchHelper), new GridPropertyMetadata(String.Empty, OnSearchTextChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether to highlight the search text or not.
        /// </summary>
        /// <value>
        /// <b>true</b>, if the search text is highlighted; otherwise,<b>false</b>. The default value is <b>true</b>.
        /// </value>
        public bool CanHighlightSearchText
        {
            get { return (bool)GetValue(CanHighlightSearchTextProperty); }
            set { SetValue(CanHighlightSearchTextProperty, value); }
        }

        public static readonly DependencyProperty CanHighlightSearchTextProperty =
            GridDependencyProperty.Register("CanHighlightSearchText", typeof(bool), typeof(SearchHelper), new GridPropertyMetadata(true));

        #endregion

        #region CallBack Methods

        /// <summary>
        /// Invokes when the SearchText has been changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/> Contains the event data.</param>
        private static void OnSearchTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var searchHelper = obj as SearchHelper;
            if (searchHelper.DataGrid == null || searchHelper.isSuspended)
                return;

            if (searchHelper.DataGrid is DetailsViewDataGrid)
                searchHelper.Search(searchHelper.SearchText);
            else
                throw new InvalidOperationException("User SearchHelper.Search method to search. SearchText property can't be set from outside");
        }

        /// <summary>
        /// Invokes when the AllowSearchFilter value has been changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/>Contains the event data.</param>
        private static void OnAllowSearchFilterChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var searchHelper = obj as SearchHelper;
            if (!searchHelper.DataGrid.HasView)
                return;

            if (!(bool)args.NewValue)
                searchHelper.DataGrid.View.Filter = null;
            else            
                searchHelper.DataGrid.View.Filter = searchHelper.FilterRecords;          
            searchHelper.DataGrid.View.RefreshFilter();
        }

        /// <summary>
        /// Invokes when the AllowCaseSensitiveSearch value has been changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/>Contains the event data.</param>
        private static void OnAllowCaseSensitiveSearchChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var SearchHelper = obj as SearchHelper;
            if (!string.IsNullOrEmpty(SearchHelper.SearchText))
                SearchHelper.Search(SearchHelper.SearchText);
        }

        /// <summary>
        /// Invokes when the AllowSearchFilter value has been changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/>contains the event data.</param>
        private static void OnSearchHighlightBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var SearchHelper = obj as SearchHelper;
            if (!string.IsNullOrEmpty(SearchHelper.SearchText))
                SearchHelper.Search(SearchHelper.SearchText);
        }

        /// <summary>
        /// Invokes when the SearchBrush value has been changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">The <see cref="DependencyPropertyChangedEventArgs"/>contains the event data.</param>
        private static void OnSearchBrushchanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var SearchHelper = obj as SearchHelper;
            if (!string.IsNullOrEmpty(SearchHelper.SearchText))
                SearchHelper.Search(SearchHelper.SearchText);
        }

        #endregion

        #region ctor

        /// <summary>
        /// Invokes when new instances has been created.
        /// </summary>
        /// <param name="sfDataGrid"></param>
        public SearchHelper(SfDataGrid sfDataGrid)
        {
            this.DataGrid = sfDataGrid;
            this.SearchText = string.Empty;
            this.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Temporarily suspends the updates for the search operation in SfDataGrid.
        /// </summary>
        protected internal void SuspendUpdates()
        {
            this.isSuspended = true;
        }

        /// <summary>
        /// Resumes usual search operation in SfDataGrid
        /// </summary>
        protected internal void ResumeUpdates()
        {
            this.isSuspended = false;
        }

        #region Search Methods

        /// <summary>
        /// Initiates the Search operation in SfDataGrid based on passed text.
        /// </summary>
        /// <param name="text">Specifies the text to be Search.</param>
        public virtual void Search(string text)
        {
            if (DataGrid.useDrawing)            
                throw new NotSupportedException("Search is not supported with Light weight templates rendering.");            

            DataGrid.RunWork(new Action(() =>
                {
                    string previouewSearchText = SearchText;

                    this.isSuspended = true;
                    SearchText = text;
                    this.isSuspended = false;

                    if (!this.DataGrid.HasView)
                        return;

                    Provider = this.DataGrid.View.GetPropertyAccessProvider();
                    this.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);

                    if (string.IsNullOrEmpty(SearchText))
                    {
                        this.ClearSearch();
                        return;
                    }

                    if (string.IsNullOrEmpty(previouewSearchText) && string.IsNullOrEmpty(SearchText))
                        return;

                    if (this.DataGrid.SearchHelper.AllowFiltering == true)
                    {
                        this.DataGrid.View.Filter = this.FilterRecords;
                        this.DataGrid.View.RefreshFilter();
                    }
                    if (this.CanHighlightSearchText)
                    {
                        for (int i = 0; i < this.DataGrid.RowGenerator.Items.Count; i++)
                        {
                            var dataRow = this.DataGrid.RowGenerator.Items[i];
                            if (dataRow.RowIndex > -1)
                                this.SearchRow(dataRow);
                        }
                    }
                }), DataGrid.ShowBusyIndicator);
        }

        private void SearchByText(string text)
        {
            
        }

        /// <summary>
        /// Searches the cells in the row to match with the SearchText. 
        /// </summary>
        /// <param name="row">Specifies the DataRow to search.</param>
        protected internal virtual void SearchRow(DataRowBase row)
        {
            if (row is DataRow && row.RowData != null)
            {
                //Except record rows other rows returned.
                if (row.RowData is NodeEntry)
                    return;

                if (row.RowType != RowType.DefaultRow)
                    return;

                foreach (var column in row.VisibleColumns)
                    SearchCell(column, row.RowData, false);
            }
        }

        /// <summary>
        /// Looks for the SearchText in the TextBlocks of the cell and invokes ApplyInline to set the background color for the search text in TextBlock.
        /// </summary>
        /// <returns>Returns true if the brush value is applied to the TextBlock inlines.</returns>
        /// <param name="column">Denotes the GridColumn associate with cell.</param>
        /// <param name="record">Denotes the data object associated with cell. </param>
        /// <param name="fromFindNext">Denotes whether to apply SearchBrush and SearchHighlightBrush. </param>
        protected internal virtual bool SearchCell(DataColumnBase column, object record, bool ApplySearchHighlightBrush)
        {
            if (column == null)
                return true;

            if (column.GridColumn == null || !this.DataGrid.HasView)
                return false;

            if (Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();

            var data = Provider.GetFormattedValue(record, column.GridColumn.MappingName);
            var content = (column.columnElement as ContentControl).Content;

            if (!(content is TextBlock))
                return false;

            var textBlock = content as TextBlock;

            if (MatchSearchText(column.GridColumn, record))
                return this.ApplyInline(column, data, ApplySearchHighlightBrush);
            else
            {
                ClearSearchCell(column, record);
                return false;
            }
        }

        /// <summary>
        /// Sets the background color for search text in TextBlock.
        /// </summary>
        /// <param name="column">Denotes the column being searched. </param>
        /// <param name="data">Gets the corresponding cell value.</param>
        /// <param name="ApplySearchBrush">Denotes whether to set the background for the SearchText based on SearchBrush or SearchHiglightBrush.</param>
        /// <returns>Returns true, if background changed for the search text in TextBlock. otherwise false.</returns>
        protected virtual bool ApplyInline(DataColumnBase column, object data, bool ApplySearchHighlightBrush)
        {
			//WPF-28260 Issue 4 & 5 - If we search text with MetaCharaters, the character that  denotes all the characters, 
            //so we have to add Escape sequence before the MetaCharaters. 
            var tempSearchText = SearchText;
            String[] metaCharacters = { "\\", "^", "$", "{", "}", "[", "]", "(", ")", ".", "*", "+", "?", "|", "<", ">", "-", "&" };
            if (metaCharacters.Any(tempSearchText.Contains))
            {
                for (int i = 0; i < metaCharacters.Length; i++)
                {
                    if (tempSearchText.Contains(metaCharacters[i]))
                        tempSearchText = tempSearchText.Replace(metaCharacters[i], "\\" + metaCharacters[i]);
                }
            }

            string[] substrings;
            Regex regex;
            if (this.SearchType == SearchType.StartsWith)
            {
                if (!AllowCaseSensitiveSearch)
                    regex = new Regex("^(" + tempSearchText + ")", RegexOptions.IgnoreCase);
                else
                    regex = new Regex("^(" + tempSearchText + ")", RegexOptions.None);
            }
            else if (this.SearchType == SearchType.EndsWith)
            {
                if (!AllowCaseSensitiveSearch)
                    regex = new Regex("(" + tempSearchText + ")$", RegexOptions.IgnoreCase);
                else
                    regex = new Regex("(" + tempSearchText + ")$", RegexOptions.None);
            }
            else
            {
                if (!this.AllowCaseSensitiveSearch)
                    regex = new Regex("(" + tempSearchText + ")", RegexOptions.IgnoreCase);
                else
                    regex = new Regex("(" + tempSearchText + ")", RegexOptions.None);
            }

            //get all the words from the 'content'
            var textBlock = (column.columnElement as ContentControl).Content as TextBlock;
            textBlock.Inlines.Clear();
            substrings = regex.Split(data.ToString());
            bool success = false;
            foreach (var item in substrings)
            {
                if (regex.Match(item).Success)
                {
                    //create a 'Run' and add it to the TextBlock
                    Run run = new Run(item);
                    if (ApplySearchHighlightBrush || column.ColumnIndex == CurrentRowColumnIndex.ColumnIndex && column.RowIndex == CurrentRowColumnIndex.RowIndex)
                        run.Background = this.SearchHighlightBrush;
                    else
                        run.Background = this.SearchBrush;
                    if (column.GridColumn is GridHyperlinkColumn)
                        textBlock.Inlines.Add(new Hyperlink(run));
                    else
                        textBlock.Inlines.Add(run);
                    success = true;
                }
                else
                {
                    if (column.GridColumn is GridHyperlinkColumn)
                        textBlock.Inlines.Add(new Hyperlink(new Run(item)));
                    else
                        textBlock.Inlines.Add(item);
                }
            }
            return success;
        }

        /// <summary>
        /// Checks whether the cell display text with the SearchText.
        /// </summary>
        /// <param name="column">Denotes the column being searched. </param>
        /// <param name="record"> Denotes the data object being searched </param>
        /// <returns>Returns true, if cell display text match with search text. Otherwise false.</returns>
        protected virtual bool MatchSearchText(GridColumn column, object record)
        {
            if (!this.DataGrid.HasView || string.IsNullOrEmpty(SearchText))
                return false;

            var cellvalue = Provider.GetFormattedValue(record, column.MappingName);
            if (this.SearchType == SearchType.Contains)
            {
                if (!AllowCaseSensitiveSearch)
                    return cellvalue != null && cellvalue.ToString().ToLower().Contains(SearchText.ToString().ToLower());
                else
                    return cellvalue != null && cellvalue.ToString().Contains(SearchText.ToString());
            }
            else if (this.SearchType == SearchType.StartsWith)
            {
                if (!AllowCaseSensitiveSearch)
                    return cellvalue != null && cellvalue.ToString().ToLower().StartsWith(SearchText.ToString().ToLower());
                else                
                    return cellvalue != null && cellvalue.ToString().StartsWith(SearchText.ToString());
            }
            else
            {
                if (!AllowCaseSensitiveSearch)
                    return cellvalue != null && cellvalue.ToString().ToLower().EndsWith(SearchText.ToString().ToLower());
                else
                    return cellvalue != null && cellvalue.ToString().EndsWith(SearchText.ToString());
            }			
        }

        #endregion

        #region ClearMethods

        /// <summary>
        /// Clears the text highlighting in the searched TextBlock’s of SfDataGrid. 
        /// </summary>
        public virtual void ClearSearch()
        {
            this.isSuspended = true;
            SearchText = String.Empty;
            this.isSuspended = false;

            if (!this.DataGrid.HasView)
                return;

            this.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
            if (this.DataGrid is DetailsViewDataGrid)
            {
                var parentGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
                while (parentGrid != null)
                {
                    parentGrid.SearchHelper.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
                    parentGrid = parentGrid.NotifyListener != null ? parentGrid.NotifyListener.GetParentDataGrid() : null;
                }
            }

            if (this.CanHighlightSearchText)
            {
                for (int i = 0; i < this.DataGrid.RowGenerator.Items.Count; i++)
                {
                    var clearDataRow = this.DataGrid.RowGenerator.Items[i];
                    ClearSearchRow(clearDataRow);
                }
            }

            if (this.AllowFiltering == true)
            {
                this.DataGrid.View.Filter = this.FilterRecords;
                this.DataGrid.View.RefreshFilter();
            }
        }

        /// <summary>
        /// Clears the highlighting in the searched TextBlock’s of DataRow.
        /// </summary>
        /// <param name="row"> Specifies the corresponding DataRow.</param>
        protected internal virtual void ClearSearchRow(DataRowBase row)
        {
            if ((row is DataRow) && row.RowData != null)
            {
                //Except record rows other rows returned.
                if (row.RowData is NodeEntry)
                    return;

                foreach (var column in row.VisibleColumns)
                    ClearSearchCell(column, row.RowData);
            }
        }

        /// <summary>
        /// Clears highlighting in the specified GridCell.
        /// </summary>
        /// <param name="column">Denotes the column associated with cell. </param>
        /// <param name="record">Denotes the data object associated with cell. </param>
        protected internal virtual void ClearSearchCell(DataColumnBase column, object record)
        {
            if (column.RowIndex > 0 && column.GridColumn != null && this.DataGrid.HasView)
            {
                var Content = (column.columnElement as ContentControl).Content;
                var textBlock = (Content as TextBlock);
                if (textBlock != null && textBlock.Inlines.Count > 1)
                {
                    textBlock.Inlines.Clear();
                    column.UpdateBinding(record, false);
                }
            }
        }

        #endregion

        #region FindNext Methods

        /// <summary>
        /// Finds and highlights, the next cell match with search text and updates CurrentRowColumnIndex. 
        /// </summary>
        /// <param name="text">Specifies the text to be searched.</param>
        public virtual bool FindNext(string text)
        {
            if (this.DataGrid.IsSourceDataGrid)
            {
                var parentGrid = this.DataGrid.GetTopLevelParentDataGrid();
                if (parentGrid == null)
                    return false;
                return parentGrid.SearchHelper.ProcessParentGridDetailsViewNextIndex(this.DataGrid);
            }

            if (!this.DataGrid.HasView)
                return false;

            bool isSearched = false;

            this.isSuspended = true;
            SearchText = text;
            this.isSuspended = false;

            if (this.Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();

            if (String.IsNullOrEmpty(SearchText))
                this.ClearSearch();

            int firstColumnIndex = DataGrid.GetFirstColumnIndex();
            var previousRowColumnIndex = CurrentRowColumnIndex;
            int columnIndex = CurrentRowColumnIndex.ColumnIndex < 0 ? firstColumnIndex : this.DataGrid.SearchHelper.GetNextColumnIndex(CurrentRowColumnIndex.ColumnIndex);
            int rowIndex = CurrentRowColumnIndex.RowIndex != -1 ? CurrentRowColumnIndex.RowIndex : DataGrid.GetFirstDataRowIndex();
            rowIndex = CurrentRowColumnIndex.ColumnIndex == DataGrid.GetLastColumnIndex() && !(DataGrid.IsInDetailsViewIndex(CurrentRowColumnIndex.RowIndex)) ? this.DataGrid.SearchHelper.GetNextDataRowIndex(rowIndex) : rowIndex;
            var currentIndex = this.CurrentRowColumnIndex.RowIndex;
            while (rowIndex >= 0)
            {
                if (!this.DataGrid.IsInDetailsViewIndex(rowIndex) && SetNextRowColumnIndex(new RowColumnIndex(rowIndex, columnIndex))) 
                {
                    isSearched = true;
                    if (!(this.DataGrid is DetailsViewDataGrid))
                    {                                                
                        VisibleLineInfo columnLineInfo = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.ColumnIndex);
                        VisibleLineInfo rowLineLineInfo = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.RowIndex);                                                
                        if(columnLineInfo == null || rowLineLineInfo == null)
                            this.DataGrid.ScrollInView(CurrentRowColumnIndex);
                    }
                    break;
                }

                rowIndex = this.DataGrid.SearchHelper.GetNextDataRowIndex(rowIndex);
                columnIndex = firstColumnIndex;
            }

            if (rowIndex >= 0 && this.CanHighlightSearchText)
            {
                ClearHighlightedText(previousRowColumnIndex);
                var datarow = this.DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == CurrentRowColumnIndex.RowIndex);
                if (datarow != null)
                {
                    var dataColumn = datarow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == CurrentRowColumnIndex.ColumnIndex);
                    return SearchCell(dataColumn, datarow.RowData, true);
                }
            }
            return isSearched;
        }

        /// <summary>
        /// Searches the next row index match with search text in DetailsViewDataGrid.
        /// </summary>
        /// <param name="sfDataGrid">The SfDataGrid associated with all DetailsViewDataGrid in that level.</param>
        /// <returns>Returns true, if the search text matched in DetailsViewDataGrid, otherwise false.</returns>
        protected bool ProcessDetailsViewNextIndex(SfDataGrid sfDataGrid, string relationName)
        {
            var rowIndex = this.CurrentRowColumnIndex.RowIndex;
            do
            {
                if (this.DataGrid.IsInDetailsViewIndex(rowIndex))
                {
                    if (ProcessDetailsViewFindNext(rowIndex, relationName, sfDataGrid))
                    {
                        if (this.CurrentRowColumnIndex.RowIndex > 0 && rowIndex != this.CurrentRowColumnIndex.RowIndex && this.DataGrid.IsInDetailsViewIndex(this.CurrentRowColumnIndex.RowIndex)) 
                        {
                            var detailsViewGrid = this.DataGrid.GetDetailsViewGridInView(this.CurrentRowColumnIndex.RowIndex);
                            var currentRowIndex = detailsViewGrid.SearchHelper.CurrentRowColumnIndex;
                            detailsViewGrid.SearchHelper.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
                            if (detailsViewGrid.SearchHelper.CanHighlightSearchText)
                                detailsViewGrid.SearchHelper.ClearHighlightedText(currentRowIndex);
                        }
                        CurrentRowColumnIndex = new RowColumnIndex(rowIndex, this.DataGrid.GetFirstColumnIndex());
                        var searchgrid = this.DataGrid.GetParentDataGrid();
                        var childGrid = this.DataGrid;
                        while (searchgrid != null)
                        {
                            int rowindex = searchgrid.GetGridDetailsViewRowIndex(childGrid as DetailsViewDataGrid);
                            if (rowindex != searchgrid.SearchHelper.CurrentRowColumnIndex.RowIndex)
                                searchgrid.SearchHelper.ClearDetailsViewDataGrid();
                            searchgrid.SearchHelper.CurrentRowColumnIndex = new RowColumnIndex(rowindex, this.DataGrid.GetFirstColumnIndex());
                            childGrid = searchgrid;
                            searchgrid = searchgrid.GetParentDataGrid();
                        }
                        var parentGrid = this.DataGrid.GetTopLevelParentDataGrid();
                        if (parentGrid != null)
                            parentGrid.SearchHelper.ParentGridScrollInView(parentGrid.SearchHelper.CurrentRowColumnIndex.RowIndex);
                        return true;
                    }
                }
                rowIndex = this.GetNextDataRowIndex(rowIndex);
            } while (rowIndex >= 0);

            return false;
        }

        /// <summary>
        /// Method which helps to scroll the particular row into view in DetailsViewDataGrid.
        /// </summary>
        /// <param name="rowIndex"> The row index to bring into view.</param>
        protected void ParentGridScrollInView(int rowIndex)
        {
            if (rowIndex < 0)
                return;
            int firstBodyVisibleIndex = -1;
            var SelectionController = this.DataGrid.SelectionController as GridBaseSelectionController;
            VisibleLineInfo lineInfo = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(rowIndex);
            if (lineInfo == null)
            {
                var visibleLines = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLines();
                if (visibleLines.FirstBodyVisibleIndex < visibleLines.Count)
                    firstBodyVisibleIndex = visibleLines[visibleLines.FirstBodyVisibleIndex].LineIndex;
            }
            var isInOutOfView = rowIndex > this.DataGrid.VisualContainer.ScrollRows.LastBodyVisibleLineIndex + 1 || (firstBodyVisibleIndex >= 0 && rowIndex < firstBodyVisibleIndex - 1);
            if (isInOutOfView || lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                var detailsViewGrid = this.GetSearchDetailsViewGrid(this.DataGrid);
                if (this.DataGrid.IsInDetailsViewIndex(rowIndex) && detailsViewGrid != null && !(this.DataGrid is DetailsViewDataGrid))
                {
                    var delta = this.DataGrid.VisualContainer.ScrollRows.Distances.GetCumulatedDistanceAt(rowIndex);
                    int count = 0;
                    var searchGrid = detailsViewGrid;
                    double headerExtent = this.DataGrid.VisualContainer.ScrollRows.HeaderExtent;
                    while (searchGrid != null)
                    {
                        detailsViewGrid = searchGrid;
                        delta += detailsViewGrid.VisualContainer.ScrollRows.Distances.GetCumulatedDistanceAt(detailsViewGrid.SearchHelper.CurrentRowColumnIndex.RowIndex);
                        headerExtent += detailsViewGrid.VisualContainer.ScrollRows.HeaderExtent;
                        searchGrid = this.GetSearchDetailsViewGrid(detailsViewGrid);
                        count++;
                    }
                    double topPaddingValue = 0;
                    double bottomPaddingValue = 0;
                    if (count > 0)
                    {
                        topPaddingValue += (this.DataGrid.DetailsViewPadding.Top + detailsViewGrid.BorderThickness.Top) * count;
                        bottomPaddingValue += (this.DataGrid.DetailsViewPadding.Bottom + 1) * count;
                    }

                    delta += topPaddingValue + bottomPaddingValue + detailsViewGrid.VisualContainer.ScrollRows.GetLineSize(detailsViewGrid.SearchHelper.CurrentRowColumnIndex.RowIndex);
                    if ((delta - (topPaddingValue + headerExtent)) > this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value)
                    {
                        delta = delta - this.DataGrid.VisualContainer.ScrollRows.ScrollBar.LargeChange - this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value;
                        if (delta > 0)
                            this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value += delta;
                    }
                    else
                    {
                        delta -= (topPaddingValue + headerExtent);
                        this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value = delta;
                    }
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    return;
                }

                if (!(this.DataGrid is DetailsViewDataGrid))
                {
                    this.DataGrid.VisualContainer.ScrollRows.ScrollInView(rowIndex);
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();

                    if (isInOutOfView && this.DataGrid.SelectedDetailsViewGrid != null)
                        this.ParentGridScrollInView(rowIndex);
                }
            }
        }

        /// <summary>
        /// Searches and highlights, the next cell that match with search text. 
        /// </summary>
        /// <param name="rowIndex"> The row index to find the next DetailsViewDataGrid</param>
        /// <returns>Returns true, if the search text matches with any cell in DetailsViewDataGrid. otherwise false</returns>
        protected bool ProcessDetailsViewFindNext(int rowIndex,string relationalColumn, SfDataGrid sourceDataGrid = null)
        {
            var record = this.DataGrid.GetRecordAtRowIndex(rowIndex);
            var ChildSource = this.DataGrid.DetailsViewManager.GetChildSource(record, relationalColumn);
            bool Success = false;
            foreach (var item in ChildSource)
            {
                if (sourceDataGrid != null)
                {
                    int colindex = sourceDataGrid.ResolveToGridVisibleColumnIndex(sourceDataGrid.GetFirstColumnIndex());
                    for (int i = colindex; i < sourceDataGrid.Columns.Count; i++)
                    {
                        var column = sourceDataGrid.Columns[i];

                        if (column != null && column.ActualWidth != 0.0 && !column.IsHidden)
                        {
#if SyncfusionFramework4_0
                            var cellValue = item.GetType().GetProperty(column.MappingName).GetValue(item, null);
#else
                            var cellValue = item.GetType().GetProperty(column.MappingName).GetValue(item);
#endif
                            if (cellValue.ToString().ToLower().Contains(sourceDataGrid.SearchHelper.SearchText.ToLower()))
                            {
                                Success = true;
                                break;
                            }
                        }
                    }
                }
                if (Success)
                    break;
            }
            if (!Success)
                return false;

            var DetailsViewGrid = this.DataGrid.GetDetailsViewGridInView(rowIndex);
            if (DetailsViewGrid == null || (!DetailsViewGrid.NotifyListener.SourceDataGrid.Equals(sourceDataGrid)))
                return false;

            if (!string.IsNullOrEmpty(DetailsViewGrid.SearchHelper.SearchText))
            {
                if (DetailsViewGrid.SearchHelper.FindNext(DetailsViewGrid.SearchHelper.SearchText))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Process the FindNext operation to the next corresponding DetailsViewDataGrid.
        /// </summary>
        /// <param name="sourceDataGrid">SourceDataGrid</param>
        /// <returns></returns>
        private bool ProcessParentGridDetailsViewNextIndex(SfDataGrid sourceDataGrid)
        {
            String relationName = this.GetRelationName(sourceDataGrid);

            if (relationName != string.Empty)
                return this.ProcessDetailsViewNextIndex(sourceDataGrid, relationName);
            int rowIndex = this.CurrentRowColumnIndex.RowIndex;
            do
            {
                if (this.DataGrid.IsInDetailsViewIndex(rowIndex))
                {
                    var detailsviewGrid = this.DataGrid.GetDetailsViewGridInView(rowIndex);
                    bool available = false;
                    if (detailsviewGrid.DetailsViewManager.HasDetailsView)
                        available = detailsviewGrid.SearchHelper.ProcessParentGridDetailsViewNextIndex(sourceDataGrid);
                    if (available)
                        return true;
                }
                rowIndex = this.GetNextDataRowIndex(rowIndex);
            } while (rowIndex >= 0);
            return false;
        }

        /// <summary>
        /// Sets the next CurrentRowColumnIndex when the SearchText matches the corresponding cell value.
        /// </summary>
        /// <param name="rowIndex">The rowIndex</param>
        /// <param name="columnIndex">The ColumnIndex</param>
        /// <returns>Return true if CurrentRowColumnIndex is changed otherwise false.</returns>
        private bool SetNextRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            var record = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            int colIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            for (int i = colIndex; i < this.DataGrid.Columns.Count; i++)
            {
                var column = this.DataGrid.Columns[i];
                if (column != null && column.ActualWidth != 0.0 && !column.IsHidden)
                {
                    if (MatchSearchText(column, record))
                    {
                        rowColumnIndex.ColumnIndex = DataGrid.ResolveToScrollColumnIndex(i);
                        this.CurrentRowColumnIndex = new RowColumnIndex(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex);
                        this.HorizontalScrollinView(rowColumnIndex.ColumnIndex);
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region FindPrevious Methods

        /// <summary>
        /// Find and highlights, the previous cell match with search text and updates CurrentRowColumnIndex. 
        /// </summary>
        /// <param name="text"> Specifies the text to be searched.</param>
        /// <returns>Returns true, if any of the previous cell match with search text. Otherwise false.</returns>
        public virtual bool FindPrevious(string text)
        {
            if (this.DataGrid.IsSourceDataGrid)
            {
                var parentGrid = this.DataGrid.GetTopLevelParentDataGrid();
                if (parentGrid == null)
                    return false;
                return parentGrid.SearchHelper.ProcessParentGridDetailsViewPreviousIndex(this.DataGrid);
            }

            if (!this.DataGrid.HasView)
                return false;

            bool isSearched = false;

            this.isSuspended = true;
            SearchText = text;
            this.isSuspended = false;

            if (this.Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();

            if (string.IsNullOrEmpty(SearchText))
                this.ClearSearch();

            int lastColumnIndex = DataGrid.GetLastColumnIndex();
            var previousRowColumnIndex = CurrentRowColumnIndex;
            int columnIndex = CurrentRowColumnIndex.ColumnIndex < 0 ? lastColumnIndex : this.DataGrid.SearchHelper.GetPreviousColumnIndex(CurrentRowColumnIndex.ColumnIndex);
            int rowIndex = CurrentRowColumnIndex.RowIndex != -1 ? CurrentRowColumnIndex.RowIndex : DataGrid.GetLastDataRowIndex();
            rowIndex = CurrentRowColumnIndex.ColumnIndex == this.DataGrid.GetFirstColumnIndex() && !(DataGrid.IsInDetailsViewIndex(CurrentRowColumnIndex.RowIndex)) ? this.DataGrid.SearchHelper.GetPreviousDataRowIndex(rowIndex) : rowIndex;
            var currentIndex = this.CurrentRowColumnIndex.RowIndex;
            while (rowIndex >= 0)
            {
                if (!this.DataGrid.IsInDetailsViewIndex(rowIndex) && SetPreviousRowColumnIndex(new RowColumnIndex(rowIndex, columnIndex))) 
                {
                    isSearched = true;
                    var dataRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
                    if (!(this.DataGrid is DetailsViewDataGrid))
                    {
                        VisibleLineInfo columnLineInfo = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.ColumnIndex);
                        VisibleLineInfo rowLineLineInfo = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.RowIndex);
                        if (columnLineInfo == null || rowLineLineInfo == null)
                            this.DataGrid.ScrollInView(CurrentRowColumnIndex);
                    }
                    break;
                }
                rowIndex = this.DataGrid.SearchHelper.GetPreviousDataRowIndex(rowIndex);
                columnIndex = lastColumnIndex;
            }

            if (rowIndex >= 0 && this.CanHighlightSearchText)
            {
                ClearHighlightedText(previousRowColumnIndex);
                var datarow = this.DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == CurrentRowColumnIndex.RowIndex);
                if (datarow != null)
                {
                    var dataColumn = datarow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == CurrentRowColumnIndex.ColumnIndex);
                    return SearchCell(dataColumn, datarow.RowData, true);
                }
            }
            return isSearched;
        }


        /// <summary>
        /// Searches the previous row index match with search text in DetailsViewDataGrid.
        /// </summary>
        /// <param name="sfDataGrid">The SfDataGrid associated with all DetailsViewDataGrid in that level.</param>
        /// <returns>Returns true, if the search text matched in DetailsViewDataGrid, otherwise false.</returns>
        protected bool ProcessDetailsViewPreviousIndex(SfDataGrid sfDataGrid, string relationName)
        {
            var rowIndex = this.CurrentRowColumnIndex.RowIndex < 0 ? this.DataGrid.GetLastDataRowIndex() : this.CurrentRowColumnIndex.RowIndex;
            do
            {
                if (this.DataGrid.IsInDetailsViewIndex(rowIndex))
                {
                    if (ProcessDetailsViewFindPrevious(rowIndex, relationName, sfDataGrid))
                    {
                        if (this.CurrentRowColumnIndex.RowIndex > 0 && rowIndex != this.CurrentRowColumnIndex.RowIndex && this.DataGrid.IsInDetailsViewIndex(this.CurrentRowColumnIndex.RowIndex)) 
                        {
                            var detailsViewGrid = this.DataGrid.GetDetailsViewGridInView(this.CurrentRowColumnIndex.RowIndex);
                            var currentRowIndex = detailsViewGrid.SearchHelper.CurrentRowColumnIndex;
                            detailsViewGrid.SearchHelper.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
                            if (detailsViewGrid.SearchHelper.CanHighlightSearchText)
                                detailsViewGrid.SearchHelper.ClearHighlightedText(currentRowIndex);
                        }
                        CurrentRowColumnIndex = new RowColumnIndex(rowIndex, this.DataGrid.GetFirstColumnIndex());
                        var searchgrid = this.DataGrid.GetParentDataGrid();
                        var childGrid = this.DataGrid;
                        while (searchgrid != null)
                        {
                            int rowindex = searchgrid.GetGridDetailsViewRowIndex(childGrid as DetailsViewDataGrid);
                            if (rowindex != searchgrid.SearchHelper.CurrentRowColumnIndex.RowIndex)
                                searchgrid.SearchHelper.ClearDetailsViewDataGrid();
                            searchgrid.SearchHelper.CurrentRowColumnIndex = new RowColumnIndex(rowindex, this.DataGrid.GetFirstColumnIndex());
                            childGrid = searchgrid;
                            searchgrid = searchgrid.GetParentDataGrid();
                        }
                        var parentGrid = this.DataGrid.GetTopLevelParentDataGrid();
                        if (parentGrid != null)
                            parentGrid.SearchHelper.ParentGridScrollInView(parentGrid.SearchHelper.CurrentRowColumnIndex.RowIndex);
                        return true;
                    }
                }
                rowIndex = this.GetPreviousDataRowIndex(rowIndex);
            } while (rowIndex >= 0);

            return false;
        }

        /// <summary>
        /// Searches and highlights, the previous cell that match with search text. 
        /// </summary>
        /// <param name="rowIndex"> The row index to find the next DetailsViewDataGrid</param>
        /// <returns>Returns true, if the search text matches with any cell in DetailsViewDataGrid. otherwise false</returns>
        protected bool ProcessDetailsViewFindPrevious(int rowIndex,String relationName ,SfDataGrid sourceDataGrid = null)
        {
            var record = this.DataGrid.GetRecordAtRowIndex(rowIndex);
            var ChildSource = this.DataGrid.DetailsViewManager.GetChildSource(record, relationName);
            bool Success = false;
            foreach (var item in ChildSource)
            {
                if (sourceDataGrid != null)
                {
                    int colindex = sourceDataGrid.ResolveToGridVisibleColumnIndex(sourceDataGrid.GetFirstColumnIndex());
                    for (int i = colindex; i < sourceDataGrid.Columns.Count; i++)
                    {
                        var column = sourceDataGrid.Columns[i];

                        if (column != null && column.ActualWidth != 0.0 && !column.IsHidden)
                        {
#if SyncfusionFramework4_0
                            var cellValue = item.GetType().GetProperty(column.MappingName).GetValue(item, null);
#else
                            var cellValue = item.GetType().GetProperty(column.MappingName).GetValue(item);
#endif
                            if (cellValue.ToString().Contains(SearchText))
                            {
                                Success = true;
                                break;
                            }
                        }
                    }
                }
                if (Success)
                    break;
            }
            if (!Success)
                return false;

            var DetailsViewGrid = this.DataGrid.GetDetailsViewGridInView(rowIndex);
            if (DetailsViewGrid == null || (!DetailsViewGrid.NotifyListener.SourceDataGrid.Equals(sourceDataGrid)))
                return false;
            if (!string.IsNullOrEmpty(DetailsViewGrid.SearchHelper.SearchText)) 
            {
                if (DetailsViewGrid.SearchHelper.FindPrevious(DetailsViewGrid.SearchHelper.SearchText))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Process the FindPrevious operation to the previous corresponding DetailsViewDataGrid.
        /// </summary>
        /// <param name="sourceDataGrid">SourceDataGrid</param>
        /// <returns></returns>
        private bool ProcessParentGridDetailsViewPreviousIndex(SfDataGrid sourceDataGrid)
        {
            String relationName = this.GetRelationName(sourceDataGrid);

            if (relationName != string.Empty)
                return this.ProcessDetailsViewPreviousIndex(sourceDataGrid, relationName);
            int rowIndex = this.CurrentRowColumnIndex.RowIndex < 0 ? this.DataGrid.GetLastDataRowIndex() : this.CurrentRowColumnIndex.RowIndex;
            do
            {
                if (this.DataGrid.IsInDetailsViewIndex(rowIndex))
                {
                    var detailsviewGrid = this.DataGrid.GetDetailsViewGridInView(rowIndex);
                    bool available = false;
                    if (detailsviewGrid.DetailsViewManager.HasDetailsView)
                        available = detailsviewGrid.SearchHelper.ProcessParentGridDetailsViewPreviousIndex(sourceDataGrid);
                    if (available)
                        return true;
                }
                rowIndex = this.GetPreviousDataRowIndex(rowIndex);
            } while (rowIndex >= 0);
            return false;
        }

        /// <summary>
        /// Sets the previous CurrentRowColumnIndex when the SearchText matches the corresponding cell value.
        /// </summary>
        /// <param name="rowIndex">The rowIndex</param>
        /// <param name="columnIndex">The ColumnIndex</param>
        /// <returns>Return true if CurrentRowColumnIndex is changed otherwise false.</returns>
        private bool SetPreviousRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            var record = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            int colIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            for (int i = colIndex; i >= 0; i--)
            {
                var column = this.DataGrid.Columns[i];
                if (column != null && column.ActualWidth != 0.0 && !column.IsHidden)
                {
                    if (MatchSearchText(column, record))
                    {
                        rowColumnIndex.ColumnIndex = DataGrid.ResolveToScrollColumnIndex(i);
                        this.CurrentRowColumnIndex = new RowColumnIndex(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex);
                        this.HorizontalScrollinView(rowColumnIndex.ColumnIndex);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Clears the highlighted text in the corresponding RowColumnIndex.
        /// </summary>
        /// <param name="previousRowColumnIndex">The previousRowColumnIndex</param>
        private void ClearHighlightedText(RowColumnIndex previousRowColumnIndex)
        {
            if (previousRowColumnIndex.RowIndex > 0)
            {
                var dataRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == previousRowColumnIndex.RowIndex);
                if (dataRow != null)
                {
                    var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == previousRowColumnIndex.ColumnIndex);
                    SearchCell(dataColumn, dataRow.RowData, false);
                }
            }
        }

        /// <summary>
        /// Scrolls the columns horizontally when the column is clipped.
        /// </summary>
        /// <param name="ColumnIndex"> The column index to scroll the column.</param>
        protected void HorizontalScrollinView(int columnIndex)
        {
            VisibleLineInfo lineInfo = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);

            if (lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                this.DataGrid.VisualContainer.ScrollColumns.ScrollInView(columnIndex);
                lineInfo = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);
                if (lineInfo != null && this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                    DetailsViewHelper.ScrollInViewAllDetailsViewParent(this.DataGrid);
                this.DataGrid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        /// <summary>
        /// Clears the HighlightedText in DetailsViewGrid.
        /// </summary>
        private void ClearDetailsViewDataGrid()
        {
            if (this.CurrentRowColumnIndex.RowIndex < 0)
                return;

            var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(this.CurrentRowColumnIndex.RowIndex);
            if (detailsViewGrid.IsInDetailsViewIndex(detailsViewGrid.SearchHelper.CurrentRowColumnIndex.RowIndex))
                detailsViewGrid.SearchHelper.ClearDetailsViewDataGrid();
            detailsViewGrid.SearchHelper.CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
            if (detailsViewGrid.SearchHelper.CanHighlightSearchText)
                detailsViewGrid.SearchHelper.ClearHighlightedText(this.CurrentRowColumnIndex);
        }

        /// <summary>
        /// Returns the relationName when the DetailsViewDefinition is matched with the SourceDataGrid.
        /// </summary>
        /// <param name="sourceDataGrid">SourceDataGrid</param>
        /// <returns></returns>
        private string GetRelationName(SfDataGrid sourceDataGrid)
        {
            String relationName = string.Empty;
            for (int i = 0; i < this.DataGrid.DetailsViewDefinition.Count; i++)
            {
                if ((this.DataGrid.DetailsViewDefinition[i] as GridViewDefinition).DataGrid.Equals(sourceDataGrid))
                    relationName = (this.DataGrid.DetailsViewDefinition[i] as GridViewDefinition).RelationalColumn;
            }
            return relationName;
        }

        /// <summary>
        /// Returns the DetailsViewGrid when the CurrentRowIndex is DetailsViewIndex.
        /// </summary>
        /// <param name="parentGrid">parentGrid</param>
        /// <returns>Returns the DetailsviewDataGrid</returns>
        private SfDataGrid GetSearchDetailsViewGrid(SfDataGrid parentGrid)
        {
            var detailsViewGrid = parentGrid.GetDetailsViewGrid(parentGrid.SearchHelper.CurrentRowColumnIndex.RowIndex);
            if (detailsViewGrid != null && parentGrid.SearchHelper.CurrentRowColumnIndex.RowIndex > -1)
                return detailsViewGrid;
            else
                return null;
        }

        #endregion

        #endregion

        #region HelperMethods

        /// <summary>
        /// Returns previous rowIndex value when navigating through the FindPrevious operation.
        /// </summary>
        /// <param name="RowIndex">The RowIndex.</param>
        internal int GetPreviousDataRowIndex(int RowIndex)
        {
            if (RowIndex < this.DataGrid.GetFirstDataRowIndex())
                return -1;
            if (RowIndex > this.DataGrid.GetLastDataRowIndex())
                return this.DataGrid.GetLastDataRowIndex();

            int rowIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(RowIndex);
            NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
            if ((nodeEntry != null && nodeEntry.IsRecords) || nodeEntry is NestedRecordEntry)
                return rowIndex;
            else
                return this.DataGrid.SearchHelper.GetPreviousDataRowIndex(rowIndex);
        }

        /// <summary>
        /// Returns next rowIndex value when navigating through the FindNext operation.
        /// </summary>
        /// <param name="RowIndex">The RowIndex</param>
        internal int GetNextDataRowIndex(int RowIndex)
        {
            if (RowIndex > this.DataGrid.GetLastDataRowIndex())
                return -1;
            if (RowIndex < this.DataGrid.GetFirstDataRowIndex())
                return this.DataGrid.GetFirstDataRowIndex();
            int rowIndex = this.DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(RowIndex);
            NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
            if ((nodeEntry != null && nodeEntry.IsRecords) || nodeEntry is NestedRecordEntry)
                return rowIndex;
            else if (rowIndex >= 0)
                return this.DataGrid.SearchHelper.GetNextDataRowIndex(rowIndex);
            return rowIndex;
        }

        
        /// <summary>
        /// Returns previous columnIndex value when navigating through FindPrevious operation.
        /// </summary>
        /// <param name="columnIndex">The ColumnIndex.</param>
        internal int GetPreviousColumnIndex(int columnIndex)
        {
            MoveDirection direction = MoveDirection.Left;
            var resolvedIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(columnIndex);
            if (resolvedIndex >= this.DataGrid.Columns.Count)
                return this.DataGrid.GetLastColumnIndex();

            var gridColumn = this.DataGrid.Columns[resolvedIndex];

            if (gridColumn != null && gridColumn.ActualWidth != 0.0 && !gridColumn.IsHidden)
            {
                gridColumn = this.DataGrid.SelectionController.CurrentCellManager.GetNextFocusGridColumn(direction == MoveDirection.Right ? columnIndex + 1 : columnIndex - 1, direction);
                if (gridColumn != null)
                    return this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(gridColumn));
                else
                    return this.DataGrid.GetLastColumnIndex();
            }
            return resolvedIndex;
        }
       
        /// <summary>
        /// Returns next columnIndex value when navigating through the FindNext operation.
        /// </summary>
        /// <param name="columnIndex">The columnIndex.</param>
        internal int GetNextColumnIndex(int columnIndex)
        {
            MoveDirection direction = MoveDirection.Right;
            var resolvedIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(columnIndex);
            if (resolvedIndex >= this.DataGrid.Columns.Count)
                return this.DataGrid.GetFirstColumnIndex();

            var gridColumn = this.DataGrid.Columns[resolvedIndex];

            if (gridColumn != null && gridColumn.ActualWidth != 0.0 && !gridColumn.IsHidden)
            {
                gridColumn = this.DataGrid.SelectionController.CurrentCellManager.GetNextFocusGridColumn(direction == MoveDirection.Right ? columnIndex + 1 : columnIndex - 1, direction);
                if (gridColumn != null)
                    return this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(gridColumn));
                else
                    return this.DataGrid.GetFirstColumnIndex();
            }
            return resolvedIndex;
        }

        /// <summary>
        /// Checks the record is in the SearchCellInfo Collection.
        /// </summary>
        /// <param name="record">The record</param>
        private GridSearchCellInfo FindRecord(List<GridSearchCellInfo> SearchedRowInfo, object record)
        {
            if (SearchedRowInfo.Count > 0 && record != null)
            {
                var searchedRow = SearchedRowInfo.FirstOrDefault(item => item.Record.GetHashCode() == record.GetHashCode());
                if (searchedRow != null && searchedRow.Record == record)
                    return searchedRow;
                else if (searchedRow == null)
                    return SearchedRowInfo.FirstOrDefault(item => item.Record == record);
            }
            return null;
        }

        /// <summary>
        /// Returns whether row match with search text to filter SfDataGrid based on search text. 
        /// </summary>
        /// <param name="dataRow">Denotes the data object .</param>
        protected internal virtual bool FilterRecords(object dataRow)
        {
            if (string.IsNullOrEmpty(SearchText))
                return true;

            if (this.Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();

            int colindex = this.DataGrid.ResolveToGridVisibleColumnIndex(this.DataGrid.GetFirstColumnIndex());
            for (int i = colindex; i < this.DataGrid.Columns.Count; i++)
            {
                var column = this.DataGrid.Columns[i];
                if (column != null && column.ActualWidth != 0.0 && !column.IsHidden)
                {
                    if (MatchSearchText(column, dataRow))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Search and returns the collection of GridSearchInfo. 
        /// </summary>
        /// <returns>Returns list of GridSearchCellInfo.</returns>
        public List<GridSearchCellInfo> GetSearchRecords()
        {
            if (String.IsNullOrEmpty(SearchText))
                return null;
            List<GridSearchCellInfo> searchCellInfo = new List<GridSearchCellInfo>();
            int colindex = this.DataGrid.ResolveToGridVisibleColumnIndex(this.DataGrid.GetFirstColumnIndex());
            foreach (var record in this.DataGrid.View.Records)
            {
                for (int i = colindex; i < this.DataGrid.Columns.Count; i++)
                {
                    var column = this.DataGrid.Columns[i];
                    if (column != null && column.ActualWidth != 0.0 && !column.IsHidden)
                    {
                        if (MatchSearchText(column, record.Data))
                        {
                            var SearchCellInfo = FindRecord(searchCellInfo, record);
                            if (SearchCellInfo != null)
                            {
                                if (!SearchCellInfo.HasColumn(column))
                                    SearchCellInfo.ColumnCollection.Add(column);
                            }
                            else
                            {
                                SearchCellInfo = new GridSearchCellInfo() { Record = record };
                                SearchCellInfo.ColumnCollection.Add(column);
                                searchCellInfo.Add(SearchCellInfo);
                            }
                        }
                    }
                }
            }

            return searchCellInfo;
        }

        #endregion

        /// <summary>
        /// Releases the allocated resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SearchHelper"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the allocated resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SearchHelper"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                if (this.DataGrid != null)
                    this.DataGrid = null;
            }
            isdisposed = true;
        }


        public void OnDependencyPropertyChanged(string propertyName, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataGrid == null) return;
            // Need to get NotifyListener from SourceDataGrid and call NotifyPropertyChanged method
            if (this.DataGrid.NotifyListener != null)
                this.DataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyPropertyChanged(this, propertyName, e, datagrid => datagrid.SearchHelper, this.DataGrid, typeof(SearchHelper));
        }

    }
}
