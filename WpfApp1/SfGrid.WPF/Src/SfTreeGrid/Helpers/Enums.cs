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
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Defines the constants that specify the possible row type in SfTreeGrid.
    /// </summary>   
    public enum TreeRowType
    {
        DefaultRow,
        HeaderRow,
    }

    /// <summary>
    /// Defines the constants that specify the type of shortcut menu in SfTreeGrid.
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
        HeaderCell,

        /// <summary>
        /// Specifies the shortcut menu that will be opened at expander cells.
        /// </summary>
        ExpanderCell,
    }

    /// <summary>
    /// Defined the constants that specify the possible column type in SfTreeGrid.
    /// </summary>
    public enum TreeColumnType
    {
        DefaultColumn,
        ExpanderColumn,
        ColumnHeader,
        RowHeader
    }

    /// <summary>
    /// Enumerates possible expand modes while loading.
    /// </summary>
    public enum AutoExpandMode
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// Root nodes expanded.
        /// </summary>
        RootNodesExpanded,
        /// <summary>
        /// All nodes expanded.
        /// </summary>
        AllNodesExpanded,
    }

    /// <summary>    
    /// Defines the constants that specify how the data updates during data manipulation operation.
    /// </summary>      
    public enum LiveNodeUpdateMode
    {
        /// <summary>
        /// No data updates performed.
        /// </summary>
        Default,

        /// <summary>
        /// Sorting will be updated.
        /// </summary>
        AllowDataShaping
    }


    /// <summary>
    /// Defines the constants that specify how the columns in a SfTreeGrid are sized.
    /// </summary>       
    public enum TreeColumnSizer
    {
        /// <summary>
        /// No sizing. The DefaultLineSize will be set for the columns.
        /// </summary>
        None,

        /// <summary>
        /// The size is a weighted proportion of available space.
        /// </summary>
        Star,

        /// <summary>
        /// The size is based on the contents of both the cells and the column header.
        /// </summary>
        Auto,

        /// <summary>
        /// The size is based on the contents of both the cells and the column header with last column fill by default. The column to be filled can be any column.
        /// </summary>
        FillColumn,

        /// <summary>
        /// The size is based on the contents of both the cells and the column header with last column auto fill. The column to be filled can be any column.
        /// </summary>
        AutoFillColumn,

        /// <summary>
        /// The size is based on the contents of the cells.
        /// </summary>
        SizeToCells,

        /// <summary>
        /// The size is based on the contents of the column header.
        /// </summary>
        SizeToHeader,

    }


    /// <summary>
    /// Defines the constants that specify the possible region in SfTreeGrid.
    /// </summary>
    public enum TreeGridRegion
    {
        /// <summary>
        /// Specifies the header row part and placed at header region of the SfTreeGrid.
        /// </summary>
        Header,

        /// <summary>
        /// The body region of the SfTreeGrid.
        /// </summary>
        Grid,

        /// <summary>
        /// No region.
        /// </summary>
        None
    }
    /// <summary>
    /// Defines the constants that specify the possible grid operations in SfTreeGrid.
    /// </summary>
    public enum TreeGridOperation
    {
        /// <summary>
        /// Specifies the column is being sorted in SfTreeGrid.
        /// </summary>
        Sorting,

        /// <summary>
        /// The RowHeader state is being changed in SfTreeGrid. 
        /// </summary>
        RowHeaderChanged,
    }


    /// <summary>
    /// Defines the constants that specify how nodes should be filtered in SfTreeGrid.
    /// </summary>
    public enum FilterLevel
    {
        /// <summary>
        /// Filtering will be applied only in root nodes.
        /// </summary>
        Root,


        /// <summary>
        /// Filtering will be applied on root nodes, if root nodes matches filter criteria, it will get displayed in View. If root node passes in filter criteria, then child nodes will be filtered and so on.
        /// </summary>
        All,

       /// <summary>
       /// If a node matches the filter condition, all its parent nodes are also displayed, even though parent node not matches the filter condition.
       /// </summary>
       Extended,
    }

    /// <summary>
    /// Defines the constants that specify the possible reasons for collection changes in SfTreeGrid.
    /// </summary>    
    public enum TreeGridCollectionChangedReason
    {
        /// <summary>
        /// Specifies the node collection change.
        /// </summary>
        NodeCollectionChanged,

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
    }

    /// <summary>
    /// Defines the constants that specify whether nodes are expanded/collapsed  in SfTreeGrid.
    /// </summary>
    public enum NodeOperation
    {
        /// <summary>
        /// Specifies  all nodes are expanded
        /// </summary>
        NodesExpanded,

        /// <summary>
        /// Specifies all nodes are collapsed
        /// </summary>
        NodesCollapsed,
    }

    /// <summary>
    /// Defines the constants that specify how view should be refreshed while calling BeginInit, EndInit and Defer refresh method.
    /// </summary>
    public enum TreeViewRefreshMode
    {
        None,
        /// <summary>
        /// Specifies that nodes will be recreated. 
        /// </summary>
        DeferRefresh,
        /// <summary>
        /// Specifies that nodes will be rearranged based on sorting order. 
        /// </summary>
        NodeRefresh
    }

    /// <summary>
    /// Defines the constants that specify how nodes should be arranged while changing ChildPropertyName and ParentPropertyName in Self Relational mode.
    /// </summary> 
    [Flags]
    public enum SelfRelationUpdateMode
    {

        /// <summary>
        /// Denotes nodes are not rearranged based on ChildProperty when child property gets changed while editing and property change.
        /// </summary>
        None = 1,

        /// <summary>
        /// Denotes whether to move the node based on ChildProperty when child property gets changed while editing.
        /// </summary>
        MoveOnEdit = 2,

        /// <summary>
        /// Denotes whether to expand the parent nodes (if not expanded) 
        ///when node is moved from another parent due to change of ChildProperty in editing.
        ///It is applicable for ChildPropertyChange only
        /// </summary>
        MoveAndExpandOnEdit = 4,

        /// <summary>
        /// Denotes whether to move the node based on ChildProperty when child property gets changed on property change.
        /// </summary>
        MoveOnPropertyChange = 8,

        /// <summary>
        /// ScrollToUpdatedItem is applicable for MoveAndExpandOnEdit only.
        /// </summary>
        ScrollToUpdatedItem = 32
    }

    /// <summary>
    /// Defines the constants which specify the drop position in row while drag and drop nodes in SfTreeGrid.
    /// </summary>
    public enum DropPosition
    {
        /// <summary>
        /// Specifies the node can not be dropped at this position. 
        /// </summary>
        None,

        /// <summary>
        /// Specifies the dragging node will be dropped above the current row in TreeGrid.
        /// </summary>
        DropAbove,

        /// <summary>
        /// Specifies the dragging node will be dropped below the current row in TreeGrid.
        /// </summary>
        DropBelow,

        /// <summary>
        /// Specifies the dragging node will be dropped as a child node of the current node in TreeGrid.
        /// </summary>
        DropAsChild,

        /// <summary>
        /// Specifies the dragging node will be dropped outside of the TreeGrid.
        /// </summary>
        Default,
    }

    /// <summary>
    /// Defines the constants which specify the selection and background indentation in row, when first column is expander column.
    /// </summary>
    public enum RowIndentMode
    {
        /// <summary>
        /// Specifies that the selection and row background will be appeared on the entire row.
        /// </summary>
        Default,

        /// <summary>
        /// Specifies that the selection and row background will be appeared after expander when first column is expander column.
        /// </summary>
        Level,
    }

    /// <summary>
    /// Defines the constants which specify how selection should be processed when ShowCheckBox is true in SfTreeGrid.
    /// </summary>
    public enum CheckBoxSelectionMode
    {
        /// <summary>
        /// Specifies that checking/unchecking the checkbox should not affect the selection.
        /// </summary>
        Default,

        /// <summary>
        /// Specifies that user can select/unselect only by checking/unchecking the checkbox. Row can not be selected by using pointer operation. Key navigation, Editing and programmatic selection will not be supported. 
        /// </summary>
        SelectOnCheck,

        /// <summary>
        /// Specifies that user can select by checking checkbox and also selecting/unselecting the row will check/uncheck the checkbox.
        /// </summary>
        SynchronizeSelection,
    }

    /// <summary>
    /// Defines the constants which specify in which case recursive checking should be applied when <see cref="SfTreeGrid.EnableRecursiveChecking"/> is <Value>True</Value>.
    /// </summary>
    public enum RecursiveCheckingMode
    {
        /// <summary>
        /// Specifies that recursive checking will be applied while checking CheckBox and also changing CheckBoxMappingName.
        /// </summary>
        Default,

        /// <summary>
        /// Specifies that recursive checking will be applied only while checking CheckBox and it will not be applied if CheckBoxMappingName is changed.
        /// </summary>
        OnCheck,
    }
}
