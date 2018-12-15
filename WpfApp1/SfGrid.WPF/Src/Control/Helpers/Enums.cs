#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using System;
namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Defines the constants that specify the possible row region in SfDataGrid.    
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum RowRegion
    {
        /// <summary>
        /// Specifies the header region which is frozen at the top of the view.
        /// </summary>
        Header,
        /// <summary>
        /// Specifies the footer region which is frozen at the bottom of the view.
        /// </summary>
        Footer,
        /// <summary>
        /// Specifies the body region which is scrollable in view.
        /// </summary>
        Body
    }

    /// <summary>
    /// Used to specify the type of Search to the SearchHelper while searching.
    /// </summary>
    public enum SearchType
    {
        /// <summary>
        /// Search the Cell which contains the SearchText.
        /// </summary>
        Contains,
        /// <summary>
        /// Search the Cell which starts with the SearchText.
        /// </summary>
        StartsWith,
        /// <summary>
        /// Search the Cell which ends with the SearchText.
        /// </summary>
        EndsWith
    }

    /// <summary>
    /// Defines the constants that specify the orientation for auto scrolling.
    /// </summary>
    public enum AutoScrollOrientation
    {
        /// <summary>
        /// No orientation for auto scrolling. 
        /// </summary>
        None = 0,

        /// <summary>
        /// Only horizontal orientation for auto scrolling.
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// Only vertical orientation for auto scrolling.
        /// </summary>
        Vertical = 2,

        /// <summary>
        /// Both horizontal and vertical orientation for auto scrolling.
        /// </summary>
        Both = 3,
    }

    /// <summary>
    /// Defines the constants that specify the mode of auto-generated columns in SfDataGrid. 
    /// </summary>
    public enum AutoGenerateColumnsMode
    {
        /// <summary>
        /// Generates the columns based on the properties defined in the underlying data object and explicit column definition.
        /// </summary>
        Reset,
        /// <summary>
        /// Generates the columns based on the properties defined in the underlying data object, when the SfDataGrid doesn’t have an explicit column definition.
        /// </summary>
        RetainOld,
        /// <summary>
        /// Generates the columns based on the properties defined in the underlying data object. 
        /// </summary>
        ResetAll,
        /// <summary>
        /// Generates the columns based on the explicit column definition in SfDataGrid.
        /// </summary>
        None,
        /// <summary>
        /// Generates the columns and retains data operation based on the properties defined in underlying data object.
        /// </summary>
        SmartReset
    }

    /// <summary>
    /// Defines the constants that specify the clipboard action in SfDataGrid.
    /// </summary>
    public enum ClipBoardAction
    {
        /// <summary>
        /// Specifies the copy operation is performed in SfDataGrid.
        /// </summary>
        Copy,

        /// <summary>
        /// Specifies the cut operation is performed in SfDataGrid.
        /// </summary>
        Cut,

        /// <summary>
        /// Specifies the paste operation is performed in SfDataGrid.
        /// </summary>
        Paste
    }

    /// <summary>
    /// Defines the constants that specify whether the validation is enabled either at edit or view mode in SfDataGrid.
    /// </summary>
    public enum GridValidationMode
    {
#if WPF
        /// <summary>
        /// Display error icon & tips and also doesn’t allows the users to commit the invalid data without allowing users to edit other cells.
        /// </summary>
        InEdit,
#endif
        /// <summary>
        /// Displays error icons and tips alone.
        /// </summary>
        InView,

        /// <summary>
        /// Disables built-in validation support.
        /// </summary>
        None,
    }

    /// <summary>
    /// Defines the constants that specify the possible copy option in SfDataGrid.    
    /// </summary>
    [Flags]
    public enum GridCopyOption
    {
        /// <summary>
        /// Specifies the copy operation is disabled.
        /// </summary>
        None = 1,

        /// <summary>
        /// Specifies the values of selected cells/rows is copied from SfDataGrid to the clipboard.
        /// </summary>
        CopyData = 2,

        /// <summary>
        /// Specifies the values of selected cells/rows is cut from SfDataGrid to the clipboard.
        /// </summary>
        CutData = 4,

        /// <summary>
        /// Specifies the values of selected cells/rows along with its format is copy/cut from SfDataGrid to the clipboard.
        /// </summary>
        IncludeFormat = 8,

        /// <summary>
        /// Specifies the values of selected cells/rows along with its column header values is copy/cut from SfDataGrid to the clipboard.        
        /// </summary>
        IncludeHeaders = 16,

        /// <summary>
        /// Specifies the values of selected rows along with hidden column values is copy/cut from SfDataGrid to the clipboard.                
        /// </summary>
        /// <remarks>
        /// You can't able to copy the hidden column values when SelectionUnit as Cell.
        /// </remarks>
        IncludeHiddenColumn = 32
    }

    /// <summary>
    /// Defines the constants that specify the possible paste option in SfDataGrid.
    /// </summary>
    [Flags]
    public enum GridPasteOption
    {
        /// <summary>
        /// Specifies the paste support is disabled.
        /// </summary>
        None = 1,

        /// <summary>
        /// Specifies the clipboard copied content is pasted to SfDataGrid.
        /// </summary>
        PasteData = 2,

        /// <summary>
        /// Specifies the clipboard copied content is pasted to SfDataGrid except the first line.        
        /// </summary>
        ExcludeFirstLine = 4,

        /// <summary>
        /// Specifies the clipboard copied content with hidden column values is pasted to SfDataGrid.        
        /// </summary>
        IncludeHiddenColumn = 8
    }

    /// <summary>
    /// Defines the constants that specify whether the possible sorting action in SfDataGrid.
    /// </summary>
    public enum SortClickAction
    {
        /// <summary>
        /// Specifies the column is being sorted at single click.
        /// </summary>
        SingleClick,

        /// <summary>
        /// Specifies the column is being sorted at double click.
        /// </summary>
        DoubleClick
    }
    /// <summary>
    /// Defines the constants that specify whether the current cell is being activated either by an input device or programmatically.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum ActivationTrigger
    {
        /// <summary>
        /// Specifies the current cell is being activated by using mouse.
        /// </summary>
        Mouse,

        /// <summary>
        /// Specifies the current cell is being activated by using touch.
        /// </summary>
        Touch,

        /// <summary>
        /// Specifies the current cell is being activated by using pen.
        /// </summary>
        Pen,

        /// <summary>
        /// Specifies the current cell is being activated by using keyboard.
        /// </summary>
        Keyboard,

        /// <summary>
        /// Specifies the current cell is being activated by programmatically.
        /// </summary>
        Program
    }

    /// <summary>
    /// Defines the constants that specifies the mouse action that triggers the editing.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum EditTrigger
    {
        /// <summary>
        /// Editing begins when press the mouse at single click.
        /// </summary>
        OnTap,
        /// <summary>
        /// Editing begins when press the mouse at double click.
        /// </summary>
        OnDoubleTap
    }

    #region Selection

    /// <summary>
    /// Defines the constants that specify whether the single or multiple item selections are supported by SfDataGrid control.
    /// </summary>
    /// <remarks></remarks>
    [ClassReference(IsReviewed = false)]
    public enum GridSelectionMode
    {
        /// <summary>
        /// No items can be selected.
        /// </summary>
        /// <remarks></remarks>
        None,

        /// <summary>
        /// Only one item can be selected at a time.
        /// </summary>        
        Single,

        /// <summary>
        /// Multiple items can be selected at the same time.
        /// </summary>        
        Multiple,

        /// <summary>
        /// Multiple items can be selected, by using the SHIFT, CTRL, and
        /// arrow keys to make selections
        /// </summary>        
        Extended
    }

    /// <summary>
    /// Defines the constants that specify whether the FilterRow positioned at top or bottom of body region in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum FilterRowPosition
    {
        /// <summary>
        /// No FilterRow will be placed. 
        /// </summary>
        None,
        /// <summary>
        /// Specifies that FilterRow will be placed at top of the body region.
        /// </summary>
        Top,
        /// <summary>
        /// Specifies that FilterRow will be placed at top of the body region in frozen state.
        /// </summary>
        FixedTop,
        /// <summary>
        /// Specifies that FilterRow will be placed at bottom of the body region.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Defines the constants that specify the possible FilterRow constraints in SfDataGrid.
    /// </summary>
    public enum FilterRowCondition
    {
        /// <summary>
        /// Performs LessThan operation.
        /// </summary>
        LessThan,

        /// <summary>
        /// Performs LessThan Or Equal operation.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Checks for Greater Than or Equal on the operands.
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Checks for Greater Than on the operands.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Checks Equals on the operands.
        /// </summary>
        Equals,

        /// <summary>
        /// Checks for Not Equals on the operands.
        /// </summary>
        NotEquals,

        /// <summary>
        /// Checks for StartsWith on the string operands.
        /// </summary>
        BeginsWith,

        /// <summary>
        /// Checks for EndsWith on the string operands.
        /// </summary>
        EndsWith,

        /// <summary>
        /// Checks for Contains on the string operands.
        /// </summary>
        Contains,

        /// <summary>
        /// Checks for Before date on the operands.
        /// </summary>
        Before,

        /// <summary>
        /// Checks for Before or Equal date on the operands.
        /// </summary>
        BeforeOrEqual,

        /// <summary>
        /// Checks for After date on the operands.
        /// </summary>
        After,

        /// <summary>
        /// Checks for After or Equal date on the operands.
        /// </summary>
        AfterOrEqual,

        /// <summary>
        /// Checks the Null values on the operands.
        /// </summary>
        Null,

        /// <summary>
        /// Checks the Not Null values on the operands.
        /// </summary>
        NotNull,

        /// <summary>
        /// Checks the Empty values on the string operands.
        /// </summary>
        Empty,

        /// <summary>
        /// Checks the Non Empty values on the string operands.
        /// </summary>
        NotEmpty
    }


    /// <summary>
    /// Defines the constants that specify the possible type of filter mode enabled in a column of SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum FilterMode
    {
        /// <summary>
        /// Specifies the checkbox filter is enabled.
        /// </summary>
        CheckboxFilter,

        /// <summary>
        /// Specifies the advanced filter is enabled.
        /// </summary>
        AdvancedFilter,
        /// <summary>
        /// Specifies both checkbox and advanced filter is enabled.
        /// </summary>
        Both
    }
    /// <summary>
    /// Defines the constants that specify whether the column is filtered from Checkbox or Advanced filter.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum FilteredFrom
    {
        /// <summary>
        /// Specifies the column is being filtered from Checkbox filter.
        /// </summary>
        CheckboxFilter,

        /// <summary>
        /// Specifies the column is being filtered from Advanced filter.
        /// </summary>
        AdvancedFilter,

        /// <summary>
        /// Specifies the column is being filtered from FilterRow filter.
        /// </summary>
        FilterRow,

        /// <summary>
        /// Specifies the column is being filtered from neither Checkbox nor Advanced filter control.
        /// </summary>
        None
    }

    /// <summary>
    /// Defines the constants that specify the possible Advanced filter type in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum AdvancedFilterType
    {
        /// <summary>
        /// Specifies the TextFilter can be loaded when the column contains string value.
        /// </summary>
        TextFilter,

        /// <summary>
        /// Specifies the NumberFilter can be loaded when the column contains numeric value.
        /// </summary>
        NumberFilter,

        /// <summary>
        /// Specifies the DateFilter can be loaded when the column contains date time value.
        /// </summary>
        DateFilter
    }

    /// <summary>
    /// Defines the constants that specify the type of pointer operation is being performed in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum PointerOperation
    {
        /// <summary>
        /// Specifies the mouse pointer is being pressed.
        /// </summary>
        Pressed,

        /// <summary>
        /// Specifies the mouse pointer is being released.
        /// </summary>
        Released,

        /// <summary>
        /// Specifies the mouse pointer is being tapped.
        /// </summary>
        Tapped,

        /// <summary>
        /// Specifies the mouse pointer is being double tapped.
        /// </summary>
        DoubleTapped,

        /// <summary>
        /// Specifies the mouse pointer is being moved.
        /// </summary>
        Move,
#if !WinRT
        /// <summary>
        /// Specifies the mouse pointer is being scrolled in SfDataGrid.
        /// </summary>
        Wheel
#endif
    }

    /// <summary>
    /// Defines the constants that specify the possible grid operations in SfDataGrid.
    /// </summary>
    public enum GridOperation
    {
        /// <summary>
        /// Specifies the column is being sorted in SfDataGrid.
        /// </summary>
        Sorting,

        /// <summary>
        /// Specifies the column is being filtered in SfDataGrid.
        /// </summary>
        Filtering,

        /// <summary>
        /// Specifies the column is being grouped in SfDataGrid.
        /// </summary>
        Grouping,

        /// <summary>
        /// Specifies the paste operation is performed in SfDataGrid.
        /// </summary>
        Paste,

        /// <summary>
        /// Specifies when the page is navigated in SfDataGrid.
        /// </summary>
        Paging,

        /// <summary>
        /// Specifies the AddNewRow related operation is being performed in SfDataGrid.
        /// </summary>
        AddNewRow,

        /// <summary>
        /// Specifies the FilterRow related operation is being performed in SfDataGrid.
        /// </summary>
        FilterRow,

        /// <summary>
        /// Specifies the table summary related operation is being processed in SfDataGrid.
        /// </summary>
        TableSummary,

        /// <summary>
        /// The filter popup is being opened in the column.
        /// </summary>
        FilterPopupOpening,

        /// <summary>
        /// The RowHeader state is being changed in SfDataGrid. 
        /// </summary>
        RowHeaderChanged,

        /// <summary>
        /// Specifies the UnBoundRow related operation is being processed in SfDataGrid.
        /// </summary>
        UnBoundDataRow,

        /// <summary>
        /// Specifies the StackedHeaderRow related operation is being processed in SfDataGrid.
        /// </summary>
        StackedHeaderRow
    }

    #endregion
    /// <summary>
    /// Defines the constants that specify how the elements in a SfDataGrid are sized.
    /// </summary>    
    [ClassReference(IsReviewed = false)]
    public enum GridLengthUnitType
    {
        /// <summary>
        /// No sizing. The DefaultLineSize will be set for the columns.
        /// </summary>
        None = 0,

        /// <summary>
        /// The size is based on the contents of both the cells and the column header.
        /// </summary>
        Auto,

        /// <summary>
        /// The size is based on the contents of both the cells and the column header with last column fill by default. The column to be filled can be any column.
        /// </summary>
        AutoWithLastColumnFill,

        /// <summary>
        /// The size is based on the contents of both the cells and the column header with last column auto fill. The column to be filled can be any column.
        /// </summary>
        AutoLastColumnFill,

        /// <summary>
        /// The size is based on the contents of the cells.
        /// </summary>
        SizeToCells,

        /// <summary>
        /// The size is based on the contents of the column header.
        /// </summary>
        SizeToHeader,

        /// <summary>
        /// The size is a weighted proportion of available space.
        /// </summary>
        Star,

    }

    /// <summary>
    /// Defines the constants that specify whether the column width or row height need to be calculated based on the cell content.
    /// </summary>
    /// <remarks></remarks>
    [ClassReference(IsReviewed = false)]
    public enum GridQueryBounds
    {
        /// <summary>
        /// The row height can be calculated based on cell content. 
        /// </summary>
        Height,

        /// <summary>
        /// The column width can be calculated based on cell content. 
        /// </summary>
        Width
    }

    /// <summary>
    /// Defines the constants that specify the possible expression error in GridUnBoundColumn.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum ExpressionError
    {
        /// <summary>
        /// No expression error .
        /// </summary>
        None,

        /// <summary>
        /// Specifies that error will be occurred when the right quotation is missed in expression.
        /// </summary>
        MissingRightQuote,

        /// <summary>
        /// Specifies that error will be occurred when the expression has mismatched parentheses.
        /// </summary>
        MismatchedParentheses,

        /// <summary>
        /// Specifies that error will be occurred when the expression has compared between mismatched types.
        /// </summary>
        CannotCompareDifferentTypes,

        /// <summary>
        /// Specifies that error will be occurred when the expression contains unknown operator.
        /// </summary>
        UnknownOperator,

        /// <summary>
        /// Specifies that error will be occurred when the expression has an invalid formula.
        /// </summary>
        NotAValidFormula,

        /// <summary>
        /// Specifies that will be raised when the expression has unknown exception .
        /// </summary>
        ExceptionRaised
    }

    /// <summary>
    /// Defines the constants that specify the possible row type in SfDataGrid.
    /// </summary>    
    [ClassReference(IsReviewed = false)]
    public enum RowType
    {
        /// <summary>
        /// Specifies the SpannedDataRow that displays the group caption summary value in corresponding columns
        /// </summary>
        CaptionRow,

        /// <summary>
        /// Specifies the SpannedDataRow that displays the group caption summary value in row.
        /// </summary>
        CaptionCoveredRow,

        /// <summary>
        /// Specifies the SpannedDataRow that displays the group summary value in corresponding columns
        /// </summary>
        SummaryRow,

        /// <summary>
        /// Specifies the SpannedDataRow that displays the group summary value in row.
        /// </summary>
        SummaryCoveredRow,

        /// <summary>
        /// Specifies the SpannedDataRow that displays the table summary value in corresponding columns.
        /// </summary>
        TableSummaryRow,

        /// <summary>
        /// Specifies the SpannedDataRow that displays the table summary value in row.
        /// </summary>
        TableSummaryCoveredRow,

        /// <summary>
        /// Specifies the DataRow that displays the row data.
        /// </summary>
        DefaultRow,

        /// <summary>
        /// Specifies the DataRow that displays the header text.
        /// </summary>
        HeaderRow,

        /// <summary>
        /// Specifies the DataRow that is used to add a new row.
        /// </summary>
        AddNewRow,

        /// <summary>
        /// Specifies the DataRow that is used to filter the value from corresponding column.
        /// </summary>
        FilterRow,

        /// <summary>
        /// Specifies the UnBoundRow that is used to display the unbound data.
        /// </summary>
        UnBoundRow,

        /// <summary>
        /// Specifies the DetailsViewDataRow that is used to display the DetailsViewDataGrid.
        /// </summary>
        DetailsViewRow,

        /// <summary>
        /// Specifies the StackedHeaderRow that displays the stacked header text.
        /// </summary>
        StackedHeaderRow
    }

    /// <summary>
    /// Defines the constants that specify the possible cell region in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum GridCellRegion
    {
        /// <summary>
        /// Specifies the normal cell arranged in DataRow and placed at the body region of SfDataGrid.
        /// </summary>
        NormalCell,

        /// <summary>
        /// Specifies the frozen column cell arranged in frozen row and placed at the header region of SfDataGrid.
        /// </summary>
        FrozenColumnCell,

        /// <summary>
        /// Specifies the last column cell placed at the body region of SfDataGrid.
        /// </summary>
        LastColumnCell,

        /// <summary>
        /// Specifies the footer column cell region arranged in footer row and placed at the footer region of SfDataGrid.
        /// </summary>
        FooterColumnCell,

        /// <summary>
        /// Specifies the before footer column cell region placed at the body region of SfDataGrid.
        /// </summary>
        BeforeFooterColumnCell,

        /// <summary>
        /// Specifies the table summary cell arranged in TableSummaryRow of SfDataGrid.
        /// </summary>
        TableSummaryCell
    }

    /// <summary>
    /// Defines the constants that specify the type of IndentColumn in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum IndentColumnType
    {
        /// <summary>
        /// Specifies that indent cell will be placed before expander.
        /// </summary>
        BeforeExpander,
        /// <summary>
        /// Specifies that indent cell will be placed after expander.
        /// </summary>
        AfterExpander,
        /// <summary>
        /// Specifies that indent cell will be placed on the group expander.
        /// </summary>
        InExpanderExpanded,
        /// <summary>
        /// Specifies that indent cell will be placed on the group collapsed button.
        /// </summary>
        InExpanderCollapsed,
        /// <summary>
        /// Specifies that indent cell will be placed on the last group row.
        /// </summary>
        InLastGroupRow,
        /// <summary>
        /// Specifies that indent cell will be placed before the summary row.
        /// </summary>
        InSummaryRow,
        /// <summary>
        /// Specifies that indent cell will be placed before the table summary row.
        /// </summary>
        InTableSummaryRow,
        /// <summary>
        /// Specifies that indent cell will be placed before the data row.
        /// </summary>
        InDataRow,
        /// <summary>
        /// Specifies that indent cell will be placed before the AddNewRow.
        /// </summary>
        InAddNewRow,
        /// <summary>
        /// Specifies that indent cell will be placed before the Unbound Row.
        /// </summary>
        InUnBoundRow,
        /// <summary>
        /// Specifies that indent cell will be placed before the FilterRow.
        /// </summary>
        InFilterRow
    }
    /// <summary>
    /// Defines the constants that specify the reason for selection in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum SelectionReason
    {
        /// <summary>
        /// Processes the selection when key is pressed.
        /// </summary>
        KeyPressed,

        /// <summary>
        /// Processes the selection when the mouse pointer is pressed.
        /// </summary>
        PointerPressed,

        /// <summary>
        /// Processes the selection when the mouse pointer is moved.
        /// </summary>
        PointerMoved,

        /// <summary>
        /// Processes the selection when the mouse pointer is released.
        /// </summary>
        PointerReleased,

        /// <summary>
        /// Processes the selection when the SelectedItems is changed.
        /// </summary>
        SelectedItemsChanged,

        /// <summary>
        /// Processes the selection when the collection is changed.
        /// </summary>
        CollectionChanged,

        /// <summary>
        /// Processes the selection when the SelectedIndex is changed.
        /// </summary>
        SelectedIndexChanged,

        /// <summary>
        /// Processes the selection when the grid operations performed.
        /// </summary>
        GridOperations,

        /// <summary>
        /// Processes the selection when filtering is applied.
        /// </summary>
        FilterApplied,

        /// <summary>
        /// Processes the selection when the current cell is moved.
        /// </summary>
        MovingCurrentCell
    }

    /// <summary>
    /// Defines the constants that specify the possible reasons for collection changes in SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum CollectionChangedReason
    {
        /// <summary>
        /// Specifies the record collection change.
        /// </summary>
        RecordCollectionChanged,

        /// <summary>
        /// Specifies the source collection change.
        /// </summary>
        SourceCollectionChanged,

        /// <summary>
        /// Specifies the SelectedItems collection change.
        /// </summary>
        SelectedItemsCollection,

        /// <summary>
        /// Specifies the columns collection change.
        /// </summary>
        ColumnsCollection,

        /// <summary>
        /// Specifies the collection change during column reordering.
        /// </summary>
        DataReorder
    }

    /// <summary>
    /// Defines the constants that specify whether the cell or row navigation performed in SfDataGrid. 
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum NavigationMode
    {
        /// <summary>
        /// Specifies the cell navigation in SfDataGrid.
        /// </summary>
        Cell,

        /// <summary>
        /// Specifies the row navigation in SfDataGrid.
        /// </summary>
        Row
    }

    /// <summary>
    /// Defines the constants that specify the moving direction of current cell.
    /// </summary>
    public enum MoveDirection
    {
        /// <summary>
        /// Specifies the current cell is navigated at right direction.
        /// </summary>
        Right,
        /// <summary>
        /// Specifies the current cell is navigated at left direction.
        /// </summary>
        Left
    }


    /// <summary>
    /// Defines the constants that specify the type of shortcut menu in SfDataGrid.
    /// </summary>
    public enum ContextMenuType
    {
        /// <summary>
        /// Specifies the shortcut menu that will be opened at record cells.
        /// </summary>
        RecordCell,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at header cells.
        /// </summary>
        Header,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at GroupDropAreaItem.
        /// </summary>
        GroupDropAreaItem,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at GroupSummaryRow.
        /// </summary>
        GroupSummary,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at CaptionSummaryRow.
        /// </summary>
        GroupCaption,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at TableSummaryRow.
        /// </summary>
        TableSummary,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at GroupDropArea.
        /// </summary>
        GroupDropArea
    }

    /// <summary>
    /// Defines the constants that specify whether the Unbound Row positioned at top or bottom of body region in SfDataGrid.
    /// </summary>
    public enum UnBoundRowsPosition
    {
        /// <summary>
        /// No Unbound Row will be placed. 
        /// </summary>
        None,
        /// <summary>
        /// Specifies that Unbound Row will be placed at top of the body region.
        /// </summary>
        Top,
        /// <summary>
        /// Specifies that Unbound Row will be placed at bottom of the body region.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Defines the constants that specify whether the AddNewRow positioned at top or bottom of body region in SfDataGrid.
    /// </summary>
    public enum AddNewRowPosition
    {
        /// <summary>
        /// No AddNewRow will be placed. 
        /// </summary>
        None,
        /// <summary>
        /// Specifies that AddNewRow will be placed at top of the body region.
        /// </summary>
        Top,
        /// <summary>
        /// Specifies that AddNewRow will be placed at top of the body region in frozen state.
        /// </summary>
        FixedTop,
        /// <summary>
        /// Specifies that AddNewRow will be placed at bottom of the body region.
        /// </summary>
        Bottom
    }
    
    /// <summary>
    /// Defines the constants that specify the different scaling options during print.
    /// </summary>
    public enum PrintScaleOptions
    {
        /// <summary>
        /// No scaling will be used.
        /// </summary>
        NoScaling,

        /// <summary>
        /// Specifies the content will fit with in the view on one page.
        /// </summary>
        FitViewonOnePage,

        /// <summary>
        /// Specifies all the columns will fit on one page.
        /// </summary>
        FitAllColumnsonOnePage,

        /// <summary>
        /// Specifies all the rows will fit on one page.
        /// </summary>
        FitAllRowsonOnePage
    }

    /// <summary>
    /// Defines the constants that indicates the position of cursor and selection in edit element when entering edit mode.
    /// </summary>
    public enum EditorSelectionBehavior
    {
        /// <summary>
        /// Selects all content in the edit element.
        /// </summary>
        SelectAll,

        /// <summary>
        /// Moves the cursor to the last position in the edit element.
        /// </summary>
        MoveLast
    }

    /// <summary>
    /// Defines the constants that specify the possible region in SfDataGrid.
    /// </summary>
    public enum GridRegion
    {
        /// <summary>
        /// Specifies the GroupDropArea region and placed at the top of SfDataGrid.
        /// </summary>
        GroupDropArea,

        /// <summary>
        /// Specifies the header row part and placed at header region of the SfDataGrid.
        /// </summary>
        Header,

        /// <summary>
        /// The body region of the SfDataGrid.
        /// </summary>
        Grid,

        /// <summary>
        /// Specifies the column chooser window of SfDataGrid.
        /// </summary>
        ColumnChooser,

        /// <summary>
        /// No region.
        /// </summary>
        None
    }

    /// <summary>
    /// Defines the constants that specify whether the TableSummaryRow is positioned at either top or bottom of SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum TableSummaryRowPosition
    {
        /// <summary>
        /// Specifies the TableSummaryRow is positioned at bottom.
        /// </summary>
        Bottom,

        /// <summary>
        /// Specifies the TableSummaryRow is positioned at top.
        /// </summary>
        Top
    }

    /// <summary>
    /// Defines the constants that specify whether the cell or row or any unit are used for selection in a SfDataGrid.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum GridSelectionUnit
    {
        /// <summary>
        /// Only rows are selectable. Clicking a cell selects the row.
        /// </summary>
        Row,

        /// <summary>
        /// Only cells are selectable.
        /// </summary>
        Cell,

        /// <summary>
        /// Both Cells and rows are selectable.
        /// </summary>
        Any
    }

    /// <summary>
    /// Defines the constants that specify possible search conditions.
    /// </summary>
    public enum SearchCondition
    {
        /// <summary>
        /// Determines whether the beginning of this string instance matches a specified string.
        /// </summary>
        StartsWith,

        /// <summary>
        ///Determines whether this instance and another specified String object have the same value.
        /// </summary>
        Equals,

        /// <summary>
        /// Determines whether the specified substring occurs within this string.
        /// </summary>
        Contains
    }

    /// <summary>
    /// Defines the constants that specify the mode to measure the width and height of the cell based on its content.
    /// </summary>
    public enum AutoFitMode
    {
        /// <summary>
        /// The cell size is calculated by calculating and comparing the size of each cell.
        /// </summary>
        Default,

        /// <summary>
        /// The cell size is calculated by longest string of the cell content. 
        /// </summary>
        SmartFit
    }

#if WPF
    /// <summary>
    /// Defines the constants that used to parse different numeric types (int, double, decimal) in GridNumericColumn and TreeGridNumericColumn.
    /// By default, NumericColumn treats all the value types as double and return double.
    /// </summary>
    public enum ParseMode
    {
        /// <summary>
        /// Parse the value as int type.
        /// </summary>
        Int,

        /// <summary>
        /// Parse the value as Double type.
        /// </summary>
        Double,

        /// <summary>
        ///Parse the value as Decimal type.
        /// </summary>
        Decimal
    }

    /// <summary>
    /// Defines the constants that used to manage the different scrolling mode by DataContext binding.
    /// </summary>
    public enum ScrollMode
    {
        /// <summary>
        /// Scroll the rows without animation by synchronous DataContext binding.
        /// </summary>
        None,

        /// <summary>
        /// Scroll the rows with animation by Asynchronous DataContext binding.
        /// </summary>
        Async
    }

    [Flags]
    public enum UseDrawing
    {
        /// <summary>
        /// Renders grid cells by drawing cells border, content, background with ContentPresenter alone in ControlTemplate.
        /// </summary>
        Default = 0,       

        ///// <summary>
        ///// DataGrid rendered by Lightweight mode and also support validation.
        ///// </summary>
        //SupportValidation = 1,

        ///// <summary>
        ///// DataGrid rendered by Lightweight mode and also support search operation.
        ///// </summary>
        //SupportSearch = 2,
    }
#endif
}
