#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using Syncfusion.UI.Xaml.Grid.Utility;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Diagnostics;
using System.Globalization;
#if UWP
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using System.Collections;
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
using Syncfusion.Windows.Shared;
using System.Windows.Media;
using System.Collections;
using System.Windows.Controls;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Represents the collection of TreeGridColumn.
    /// </summary>
#if WPF

    public class TreeGridColumns : FreezableCollection<TreeGridColumn>, IDisposable
#else
    public class TreeGridColumns : ObservableCollection<TreeGridColumn>, IDisposable
#endif
    {

        // flag used to suspend/resume the UI update in Grid Column collection changed in TreeGrid.
        internal bool suspendUpdate = false;

#if WPF
        /// <summary>
        /// Initializes a new instance of columns class.
        /// </summary>
        /// <returns>Returns the new instance of column collection.</returns>
        protected override Freezable CreateInstanceCore()
        {
            return new TreeGridColumns();
        }
#endif
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the Columns class. 
        /// </summary>        
        public TreeGridColumns()
        {

        }

        #endregion

        #region Property
        /// <summary>
        /// Gets or sets the column at the specified mapping name .
        /// </summary>
        /// <param name="mappingName">
        /// The mapping name of the column.
        /// </param>
        /// <returns>
        /// Returns the column with corresponding to its mapping name.
        /// </returns>
        public TreeGridColumn this[string mappingName]
        {
            get
            {
                var column = this.FirstOrDefault(col => col.MappingName == mappingName);
                return column;
            }
        }

        #endregion
        /// <summary>
        /// Suspends the UI refresh when the columns are being added or removed.
        /// </summary>
        public void Suspend()
        {
            this.suspendUpdate = true;
        }

        /// <summary>
        /// Resumes the UI refresh when the columns are being added or removed.
        /// </summary>
        /// <remarks>
        /// Update columns by calling <see cref="Syncfusion.UI.Xaml.TreeGrid.Helpers.TreeGridHelper.RefreshColumns"/> method when the column updates are resumed.
        /// </remarks>
        public void Resume()
        {
            this.suspendUpdate = false;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumns"/> class.
        /// </summary>     
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumns"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }
                this.Clear();
            }
        }
    }

    internal delegate void ColumnPropertyChanged(TreeGridColumn column, string property);

    /// <summary>
    /// Provides the base functionalities for all the column types in SfTreeGrid.
    /// </summary>

#if WPF
    public abstract class TreeGridColumn : GridColumnBase, IDisposable
#else
    public abstract class TreeGridColumn : GridColumnBase
#endif
    {
        #region Fields
        internal ColumnPropertyChanged ColumnPropertyChanged;

        internal double ExtendedWidth = Double.NaN;
        #endregion

#if !WPF
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal string formatString;
#endif

        #region CLR Property
        /// <summary>
        /// Gets the cell type of the column which denotes renderer associated with column.
        /// </summary>
        /// <value>
        /// A string that represents the cell type of the column.
        /// </value>
        public string CellType
        {
            get;
            internal set;
        }

        #endregion

        #region Dependency Property
        /// <summary>
        /// Gets or sets the value that indicates how the column width is determined.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer"/> enumeration that adjust the column
        /// width.
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.None"/>.
        /// </value>      
        public TreeColumnSizer ColumnSizer
        {
            get { return (TreeColumnSizer)GetValue(ColumnSizerProperty); }
            set { SetValue(ColumnSizerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.ColumnSizer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.ColumnSizer dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ColumnSizerProperty =
            DependencyProperty.Register("ColumnSizer", typeof(TreeColumnSizer), typeof(TreeGridColumn), new PropertyMetadata(TreeColumnSizer.None, OnTreeGridColumnSizerChanged));

        /// <summary>
        /// Dependency call back for TreeGridColumnSizer property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnTreeGridColumnSizerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as TreeGridColumn);
            if (column != null && column.ColumnPropertyChanged != null)
                column.ColumnPropertyChanged(column, "ColumnSizer");
        }

        protected override void OnCellTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnCellTemplateSelectorChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellTemplateSelector");
        }

        internal override void OnIsHiddenChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "IsHidden");
        }

        internal override void OnMaximumWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "MaximumWidth");
        }

        internal override void OnMinimumWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "MinimumWidth");
        }

        internal override void OnColumnPropertyChanged(string property)
        {
            if (ColumnPropertyChanged != null)
                ColumnPropertyChanged(this, property);
        }
#if WPF
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn"/> class.
        /// </summary>
        /// <returns>
        /// Returns the new instance of column in SfTreeGrid.
        /// </returns>
        protected override Freezable CreateInstanceCore()
        {
            if (this is TreeGridTextColumn)
                return new TreeGridTextColumn();
            else if (this is TreeGridCheckBoxColumn)
                return new TreeGridCheckBoxColumn();
            else if (this is TreeGridTemplateColumn)
                return new TreeGridTemplateColumn();
            else if (this is TreeGridComboBoxColumn)
                return new TreeGridComboBoxColumn();
            else if (this is TreeGridCurrencyColumn)
                return new TreeGridCurrencyColumn();
            else if (this is TreeGridDateTimeColumn)
                return new TreeGridDateTimeColumn();
            else if (this is TreeGridHyperlinkColumn)
                return new TreeGridHyperlinkColumn();
            else if (this is TreeGridMaskColumn)
                return new TreeGridMaskColumn();
            else if (this is TreeGridNumericColumn)
                return new TreeGridNumericColumn();
            else if (this is TreeGridPercentColumn)
                return new TreeGridPercentColumn();

            throw new NotImplementedException();
        }

#endif
        internal override void SetValueBinding(bool internalset, BindingBase value)
        {
            base.SetValueBinding(internalset, value);
            UpdateBindingInfo();
        }

        /// <summary>
        /// Determines whether to increment and decrement the cell value in mouse wheel and up,down arrow key is pressed. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell value can be rotated using mouse wheel or up and down arrow key ; otherwise <b>false</b> .
        /// </returns>
        protected internal virtual bool CanAllowSpinOnMouseScroll()
        {
            return false;
        }

        internal override void OnWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "Width");
        }

        /// <summary>
        ///  Gets or sets a value that indicates whether the user can rearrange the columns.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can re arrange the columns; otherwise, <b>false</b>. The default value is <b>false</b> .
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowDraggingColumns"/>
        public bool AllowDragging
        {
            get
            {
                var valueColumn = this.ReadLocalValue(TreeGridColumn.AllowDraggingProperty);
                if (TreeGrid != null && valueColumn == DependencyProperty.UnsetValue)
                    return this.TreeGrid.AllowDraggingColumns;
                else
                    return (bool)GetValue(AllowDraggingProperty);

            }
            set { SetValue(AllowDraggingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.AllowDragging dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.AllowDragging dependency
        /// property.
        /// </remarks>        
        public static readonly DependencyProperty AllowDraggingProperty =
            DependencyProperty.Register("AllowDragging", typeof(bool), typeof(TreeGridColumn), new PropertyMetadata(false));


        /// <summary>
        ///  Gets or sets a value that indicates whether the user can resize the column. 
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can adjust the column width ; otherwise , <b>false</b>. The default value is <b>false</b> .
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowResizingColumns"/>
        public bool AllowResizing
        {
            get
            {
                var valueColumn = this.ReadLocalValue(TreeGridColumn.AllowResizingProperty);
                if (TreeGrid != null && valueColumn == DependencyProperty.UnsetValue)
                    return this.TreeGrid.AllowResizingColumns;
                else
                    return (bool)GetValue(AllowResizingProperty);
            }
            set { SetValue(AllowResizingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.AllowResizing dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.AllowResizing dependency
        /// property.
        /// </remarks>
        public static readonly DependencyProperty AllowResizingProperty =
            DependencyProperty.Register("AllowResizing", typeof(bool), typeof(TreeGridColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets the reference to the SfTreeGrid control.
        /// </summary>
        protected internal SfTreeGrid TreeGrid
        {
            get { return (SfTreeGrid)this.GridBase; }
            private set { this.GridBase = value; }
        }

        #endregion

        #region Callback
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnCellTemplateChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellTemplate");
        }

        internal override void OnCellStyleChanged()
        {
            base.OnCellStyleChanged();
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellStyle");
        }

        internal override void OnCellStyleSelectorChanged()
        {
            base.OnCellStyleSelectorChanged();
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "CellStyleSelector");
        }

        internal override void OnHeaderStyleChanged()
        {
            base.OnHeaderStyleChanged();
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "HeaderStyle");
        }

        internal override void OnGridValidationModeChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateValidationMode();
        }

        internal override void OnSetCellBoundValueChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnSetCellBoundValueChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "SetCellBoundValue");
        }

        internal override void OnAllowEditingChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateBindingInfo();

            if (this.TreeGrid != null)
            {
                if (this.TreeGrid.SelectionController != null)
                {
                    if (!this.AllowEditing && this.TreeGrid.SelectionController.CurrentCellManager.HasCurrentCell)
                    {
                        if (this.TreeGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                        {
                            this.TreeGrid.SelectionController.CurrentCellManager.EndEdit();
                            this.TreeGrid.SelectionController.ResetSelectionHandled();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the cell type which indicates the renderer for the column.
        /// </summary>
        /// <param name="cellType">
        /// Specifies the corresponding cell type of the column.
        /// </param>
        protected void SetCellType(string cellType)
        {
            this.CellType = cellType;
        }
        internal override void OnHeaderDataTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnHeaderDataTemplateChanged(e);
            if (this.ColumnPropertyChanged != null)
                this.ColumnPropertyChanged(this, "HeaderTemplate");
        }

        #endregion

        #region Methods

        internal void SetGrid(SfTreeGrid grid)
        {
            this.TreeGrid = grid;
            // WPF-33626 - Need to update binding based GridValidationMode.           
            this.UpdateBindingForValidation(GridValidationMode);
        }

        /// <summary>
        /// Releases all resources used by the SfTreeGrid and Columns.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.TreeGrid = null;
                this.ColumnPropertyChanged = null;
                ClearValue(ColumnSizerProperty);
            }
        }

        internal override void UpdateBindingInfo()
        {
            if (IsInSuspend)
                return;
            if (TreeGrid != null)
                TreeGrid.RowGenerator.UpdateBinding(this);
        }
        internal override void UpdateValidationMode()
        {
            this.UpdateBindingForValidation(this.GridValidationMode);

            if (this.TreeGrid == null || TreeGrid.TreeGridPanel == null)
                return;

            foreach (var item in TreeGrid.TreeGridPanel.RowGenerator.Items)
            {
                var row = item as TreeDataRowBase;
                if (row.RowType != TreeRowType.DefaultRow)
                    continue;
                var dataColumn = row.VisibleColumns.FirstOrDefault(column => column.TreeGridColumn != null && column.TreeGridColumn.MappingName == this.MappingName);
                if (dataColumn == null)
                    continue;
                if (this.GridValidationMode == GridValidationMode.None)
                {
                    (dataColumn.ColumnElement as TreeGridCell).RemoveError();
                }
                else
                {
                    TreeGrid.ValidationHelper.ValidateColumn(row.RowData, MappingName, dataColumn.ColumnElement as TreeGridCell, new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex));
                }
                //dataColumn.UpdateBinding(row.RowData);
            }
        }

        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            if (this.ValueBinding != null)
            {
                BindingBase bind = this.ValueBinding.CreateEditBinding(this.GridValidationMode != Grid.GridValidationMode.None, this);
                this.SetValueBinding(true, bind);
            }
        }

        /// <summary>
        /// Determines whether the corresponding column can receive focus.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the column is loaded with editor in its <b>CellTemplate</b>.
        /// </returns>      
        protected internal virtual bool CanFocus()
        {
            return hasCellTemplate || hasCellTemplateSelector;
        }

        internal override void SetDisplayBinding(bool internalset, BindingBase value)
        {
            base.SetDisplayBinding(internalset, value);
            UpdateBindingInfo();
        }
        #endregion
    }

    /// <summary>
    /// Provides the base implementation of text formatting in the column.
    /// </summary>
    public abstract class TreeGridTextColumnBase : TreeGridColumn
    {
        /// <summary>
        /// Gets or sets the text trimming to apply when the cell content overflows the content area. 
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.TextTrimming"/> values that specifies the text trimming behavior of cell content. The default value is <see cref="System.Windows.TextTrimming.None"/>.
        /// </value>
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridTextColumnBase.TextTrimming dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridTextColumnBase.TextTrimming dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register("TextTrimming", typeof(TextTrimming), typeof(TreeGridTextColumnBase), new PropertyMetadata(TextTrimming.None, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates how cell content should wrap the text in the column.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.TextWrapping"/> enumeration that specifies wrapping behavior of cell content. 
        /// The default value is <see cref="System.Windows.TextWrapping.NoWrap"/>.
        /// </value>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridTextColumnBase.TextWrapping dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridTextColumnBase.TextWrapping dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TreeGridTextColumnBase), new PropertyMetadata(TextWrapping.NoWrap, OnUpdateBindingInfo));

#if WPF
        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }

        public static readonly DependencyProperty TextDecorationsProperty =
            DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(TreeGridTextColumnBase), new PropertyMetadata(new TextDecorationCollection(), OnUpdateBindingInfo));
#endif
        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.DisplayBinding"/> of column.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.DisplayBinding"/> .
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {

#if WPF
            if (!isDisplayMultiBinding)
#endif
                if ((DisplayBinding as Binding).Converter == null)
                    (DisplayBinding as Binding).Converter = new TreeGridCultureFormatConverter(this);

        }

        internal override void OnUpdateBindingInfo(DependencyPropertyChangedEventArgs e)
        {
            this.textTrimming = this.TextTrimming;
            this.textWrapping = this.TextWrapping;
#if WPF
            this.textDecoration = this.TextDecorations;
#endif
            base.OnUpdateBindingInfo(e);
        }
    }

    /// <summary>
    /// Represents a column that contains template-specified cell content
    /// </summary>
    public class TreeGridTemplateColumn : TreeGridTextColumnBase
    {
        internal bool hasEditTemplate = false;
        internal bool hasEditTemplateSelector = false;


        /// <summary>
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> to load in editing mode.
        /// </summary>
        /// <value>
        /// The template that is used to display the contents of cell in a column that is in editing mode. The default is <b>null</b>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.SetCellBoundValue"/> decides whether the data context of the <b>EditTemplate</b> is based on Record or <see cref="TreeGridDataContextHelper"/> class.        
        /// By default, Record will be the DataContext for template. If SetCellBoundValue is true, <see cref="TreeGridDataContextHelper"/> will be the data context.        
        ///</remarks>
        public DataTemplate EditTemplate
        {
            get { return (DataTemplate)GetValue(EditTemplateProperty); }
            set { SetValue(EditTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridTemplateColumn.EditTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeeGrid.TreeGridTemplateColumn.EditTemplate dependency property.
        /// </remarks>
        public static readonly DependencyProperty EditTemplateProperty =
            DependencyProperty.Register("EditTemplate", typeof(DataTemplate), typeof(TreeGridTemplateColumn), new PropertyMetadata(null, OnEditTemplateChanged));

        /// <summary>
        /// Dependency call back for EditTemplate property.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.EditTemplate"/> dependency property value changed in the TreeGridTemplateColumn.
        /// </summary>
        /// <param name="d">The <c>DependencyObject</c> that contains the TreeGridTemplateColumn.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains the data for the <b>EditTemplate</b> property changes.</param>
        public static void OnEditTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as TreeGridTemplateColumn);
            if (column != null)
            {
                column.hasEditTemplate = e.NewValue != null;

                if (column.ColumnPropertyChanged != null)
                    column.ColumnPropertyChanged(column, "EditTemplate");
            }
        }


        /// <summary>        
        /// Gets or sets the  <see cref="System.Windows.DataTemplate"/>  by choosing a template based on bound data objects and data-bound element in editing mode.        
        /// </summary>
        /// <value>
        /// A custom <see cref="System.Windows.Controls.DataTemplateSelector"/> object that provides logic and returns a <see cref="System.Windows.DataTemplate"/> that is in edit mode of column. The default is null.
        /// </value>   
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridTemplateColumn.EditTemplate"/>
        public DataTemplateSelector EditTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(EditTemplateSelectorProperty); }
            set { SetValue(EditTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridTemplateColumn.EditTemplateSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridTemplateColumn.EditTemplateSelector dependency property.
        /// </remarks>        
        public static readonly DependencyProperty EditTemplateSelectorProperty =
            DependencyProperty.Register("EditTemplateSelector", typeof(DataTemplateSelector), typeof(TreeGridTemplateColumn), new PropertyMetadata(null, OnEditTemplateSelectorChanged));

        public static void OnEditTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as TreeGridTemplateColumn);
            if (column != null)
            {
                column.hasEditTemplateSelector = e.NewValue != null;

                if (column.ColumnPropertyChanged != null)
                    column.ColumnPropertyChanged(column, "EditTemplateSelector");
            }
        }


        /// <summary>
        /// Determines whether the GridTemplateColumn can receive focus.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the column is loaded with editor in its <b>CellTemplate</b>.
        /// </returns>
        protected internal override bool CanFocus()
        {
            return (this.hasCellTemplate || this.hasCellTemplateSelector || (this.TreeGrid != null && this.TreeGrid.hasCellTemplateSelector)) && !(this.hasEditTemplate || this.hasEditTemplateSelector);
        }

        /// <summary>
        /// Determines whether the cells in TreeGridTemplateColumn can be edited. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the template column has loaded with <see cref="Syncfusion.UI.Xaml.Grid.TreeGridTemplateColumn.EditTemplate"/> or <see cref="Syncfusion.UI.Xaml.Grid.TreeGridTemplateColumn.EditTemplateSelector"/>. 
        /// If the TreeGridTemplateColumn loaded with <see cref="Syncfusion.UI.Xaml.Grid.TreeGridTemplateColumn.CellTemplate"/> , returns <b>false</b>.
        /// </returns>        
        protected internal override bool CanEditCell()
        {
            return (this.hasEditTemplate || this.hasEditTemplateSelector);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridTemplateColumn"/> class.
        /// </summary>
        public TreeGridTemplateColumn()
        {
            this.CellType = "Template";
            IsTemplate = true;
        }

        #region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the TreeGridTemplateColumn.
        /// </summary>              
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            if (this.hasEditTemplate || this.hasEditTemplateSelector)
                base.UpdateBindingBasedOnAllowEditing();
        }

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.DisplayBinding"/> of TreeGridTemplateColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.DisplayBinding"/>.
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {

        }

        #endregion
    }

    /// <summary>
    /// Represents a column that  is used to display the string content in its cells and host TextBox in edit mode.
    /// </summary>
    public class TreeGridTextColumn : TreeGridTextColumnBase
    {
#if UWP
        /// <summary>
        /// Gets or sets a value that indicates whether the spell check is enabled for the TextBox in edit mode.
        /// </summary>
        public bool IsSpellCheckEnabled
        {
            get { return (bool)GetValue(IsSpellCheckEnabledProperty); }
            set { SetValue(IsSpellCheckEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridTextColumn.IsSpellCheckEnabled dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridTextColumn.IsSpellCheckEnabled dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsSpellCheckEnabledProperty =
            DependencyProperty.Register("IsSpellCheckEnabled", typeof(bool), typeof(TreeGridTextColumn), new PropertyMetadata(false));

#endif
        public TreeGridTextColumn()
        {
            this.CellType = "TextBox";
#if UWP
            this.Padding = new Thickness(3, 1, 2, 0); 
#endif
        }
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }
        protected override void SetDisplayBindingConverter()
        {

        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(TreeGridColumn.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(0 + padLeft, padTop, 0 + padRight, padBotton)
                           : new Thickness(2, 2, 2, 2);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 1 + padTop, 3 + padRight, 1 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#endif

        }

    }
    /// <summary>
    /// Represents a column that displays the URI data in its cells content.
    /// </summary>
    public class TreeGridHyperlinkColumn : TreeGridTextColumn
    {
        #region Ctor

        /// <summary>
        /// Gets or sets the horizontal alignment of the TreeGridHyperlinkColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.HorizontalAlignment"/> enumeration that specifies the horizontal alignment of the TreeGridHyperlinkColumn.
        /// The default is <see cref="System.Windows.HorizontalAlignment">Stretch</see>.
        /// </value>
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.TreeGridHyperlinkColumn.HorizontalAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.TreeGridHyperlinkColumn.HorizontalAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(TreeGridHyperlinkColumn), new PropertyMetadata(HorizontalAlignment.Stretch, OnUpdateBindingInfo));

        #region overrides
        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.CellTemplate"/> dependency property defined in GridHyperLinkColumn.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        /// <exception cref="System.NotSupportedException"/> Thrown when the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.CellTemplate"/> is defined in TreeGridHyperLinkColumn.
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            throw new NotSupportedException("The " + this.ToString() + " does not implement CellTemplate property");
        }
        #endregion
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridHyperlinkColumn"/> class.
        /// </summary>
        public TreeGridHyperlinkColumn()
        {
            CellType = "Hyperlink";
            Padding = new Thickness(2, 0, 2, 0);
        }
        #endregion

        #region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in TreeGridHyperlinkColumn.
        /// </summary>                
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {

        }
        /// <summary>
        /// Determines whether the TreeGridHyperlinkColumn can be editable.
        /// </summary>
        /// <returns>
        /// Returns <b>false</b> for the TreeGridHyperlinkColumn .
        /// </returns>
        protected internal override bool CanEditCell()
        {
            return false;
        }

        #endregion
    }
#if WPF

    /// <summary>
    /// Represents a column that displays the masked data in its cell content.
    /// </summary>
    public class TreeGridMaskColumn : TreeGridTextColumnBase
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the TreeGridMaskColumn that loads the numeric values in it.
        /// </summary>
        /// <value>
        /// <b>true</b> if the TreeGridMaskColumn loaded with numeric values; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool IsNumeric
        {
            get { return (bool)GetValue(IsNumericProperty); }
            set { SetValue(IsNumericProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.TreeGridMaskColumn.IsNumeric dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.TreeGridMaskColumn.IsNumeric dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsNumericProperty =
          DependencyProperty.Register("IsNumeric", typeof(bool), typeof(TreeGridMaskColumn), new PropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the components of date,that is, day ,month and year in TreeGridMaskColumn.
        /// </summary>
        /// <value>
        /// The string that separates the components of date,that is, day ,month and year in TreeGridMaskColumn. 
        /// </value>        
        public string DateSeparator
        {
            get { return (string)GetValue(DateSeparatorProperty); }
            set { SetValue(DateSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.TreeGridMaskColumn.DateSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.TreeGridMaskColumn.DateSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty DateSeparatorProperty =
           DependencyProperty.Register("DateSeparator", typeof(string), typeof(TreeGridMaskColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in values.
        /// </summary>
        /// <value>
        /// The string that separates groups of digits to the left of the decimal in values. 
        /// </value>
        public string DecimalSeparator
        {
            get { return (string)GetValue(DecimalSeparatorProperty); }
            set { SetValue(DecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.DecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.DecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty DecimalSeparatorProperty =
            DependencyProperty.Register("DecimalSeparator", typeof(string), typeof(TreeGridMaskColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the components of time, that is, the hour , minutes and seconds .
        /// </summary>
        /// <value>
        /// The string that separates the components of time, that is, the hour , minutes and seconds .
        /// </value>       
        public string TimeSeparator
        {
            get { return (string)GetValue(TimeSeparatorProperty); }
            set { SetValue(TimeSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.TimeSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.TimeSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty TimeSeparatorProperty =
            DependencyProperty.Register("TimeSeparator", typeof(string), typeof(TreeGridMaskColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the input mask to use at runtime.
        /// </summary>
        /// <value>
        /// A string that representing the current mask. The default value is the empty string which allows any input.
        /// </value>
        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.Mask dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.Mask dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask", typeof(string), typeof(TreeGridMaskColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format of masked input.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.MaskFormat"/> that specifies the format of masked input.
        /// The default value is <see cref="Syncfusion.Windows.Shared.MaskFormat.ExcludePromptAndLiterals"/>.
        /// </value>
        public MaskFormat MaskFormat
        {
            get { return (MaskFormat)GetValue(MaskFormatProperty); }
            set { SetValue(MaskFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.MaskFormat dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.MaskFormat dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaskFormatProperty =
            DependencyProperty.Register("MaskFormat", typeof(MaskFormat), typeof(TreeGridMaskColumn), new PropertyMetadata(MaskFormat.ExcludePromptAndLiterals));

        /// <summary>
        /// Gets or sets the character used to represent the absence of user input in TreeGridMaskColumn.
        /// </summary>
        /// <value>
        /// The character used to prompt the user for input. The default is an underscore (_).
        /// </value>
        public char PromptChar
        {
            get { return (char)GetValue(PromptCharProperty); }
            set { SetValue(PromptCharProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.PromptChar dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn.PromptChar dependency property.
        /// </remarks>
        public static readonly DependencyProperty PromptCharProperty =
            DependencyProperty.Register("PromptChar", typeof(char), typeof(TreeGridMaskColumn), new PropertyMetadata('_', OnUpdateBindingInfo));


        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridMaskColumn"/> class.
        /// </summary>
        public TreeGridMaskColumn()
        {
            CellType = "Mask";
        }
        #endregion

        #region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the TreeGridMaskColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }


        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(TreeGridColumn.PaddingProperty);
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(13 + padLeft, 7 + padTop, 13 + padRight, 5 + padBotton)
                           : new Thickness(3, 0, 3, 0);
        }
        #endregion
    }

    /// <summary>
    /// Provides the base implementation for all the editor columns in the SfTreeGrid.
    /// </summary>
    public abstract class TreeGridEditorColumn : TreeGridTextColumnBase
    {
        #region Dependency Properties
        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is changed using the mouse wheel or up and down arrow key; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>
        public bool AllowScrollingOnCircle
        {
            get { return (bool)GetValue(AllowScrollingOnCircleProperty); }
            set { SetValue(AllowScrollingOnCircleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.AllowScrollingOnCircle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.AllowScrollingOnCircle dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowScrollingOnCircleProperty =
            DependencyProperty.Register("AllowScrollingOnCircle", typeof(bool), typeof(TreeGridEditorColumn), new PropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed to the editor columns.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null value is allowed ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullValue
        {
            get { return (bool)GetValue(AllowNullValueProperty); }
            set { SetValue(AllowNullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.AllowNullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.AllowNullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullValueProperty =
            DependencyProperty.Register("AllowNullValue", typeof(bool), typeof(TreeGridEditorColumn), new PropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets an object that is displayed instead of null value if the cell value is null.
        /// </summary>          
        /// <value>
        /// An object that is displayed instead of null value in the cell.
        /// </value>
        /// <remarks>
        /// The <b>NullValue</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public object NullValue
        {
            get { return (object)GetValue(NullValueProperty); }
            set { SetValue(NullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.NullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.NullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty NullValueProperty =
            DependencyProperty.Register("NullValue", typeof(object), typeof(TreeGridEditorColumn), new PropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value is null.
        /// </summary>
        /// <value>
        /// A string that is displayed instead of null value in the cell.
        /// </value>
        /// <remarks>
        /// The <b>NullText</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public string NullText
        {
            get { return (string)GetValue(NullTextProperty); }
            set { SetValue(NullTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.NullText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.NullText dependency property.
        /// </remarks>
        public static DependencyProperty NullTextProperty =
            DependencyProperty.Register("NullText", typeof(string), typeof(TreeGridEditorColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MaxValue"/> can be validated key press or focus lost on editor in GridEditorColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.MaxValidation"/> enumeration that specifies how the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MaxValue"/> is validated. The default value is <b>MaxValidation.OnKeyPress</b>.
        /// </value>
        public MaxValidation MaxValidation
        {
            get { return (MaxValidation)GetValue(MaxValidationProperty); }
            set { SetValue(MaxValidationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MaxValidation dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MaxValidation dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValidationProperty =
            DependencyProperty.Register("MaxValidation", typeof(MaxValidation), typeof(TreeGridEditorColumn), new PropertyMetadata(MaxValidation.OnKeyPress));

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MinValue"/> can be validated key press or focus lost on editor in GridEditorColumn
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.MaxValidation"/>enumeration that specifies how the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MinValue"/> can be validated. The default value is <b>MinValidation.OnKeyPress</b>.
        /// </value>
        public MinValidation MinValidation
        {
            get { return (MinValidation)GetValue(MinValidationProperty); }
            set { SetValue(MinValidationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MinValidation dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MinValidation dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValidationProperty =
            DependencyProperty.Register("MinValidation", typeof(MinValidation), typeof(TreeGridEditorColumn), new PropertyMetadata(MinValidation.OnKeyPress));

        /// <summary>
        /// Gets or sets the minimum value constraint of the column.
        /// </summary>
        /// <value>
        /// The minimum value constraint of the column.
        /// </value>
        public decimal MinValue
        {
            get { return (decimal)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MinValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MinValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(decimal), typeof(TreeGridEditorColumn), new PropertyMetadata(decimal.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum value constraint of the column.
        /// </summary>
        /// <value>
        /// The maximum value constraint of the column.
        /// </value>
        public decimal MaxValue
        {
            get { return (decimal)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MaxValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn.MaxValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(decimal), typeof(TreeGridEditorColumn), new PropertyMetadata(decimal.MaxValue, OnUpdateBindingInfo));

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridEditorColumn"/> class.
        /// </summary>
        protected TreeGridEditorColumn()
        {
            base.TextAlignment = TextAlignment.Right;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Determines whether the cell value is changed using mouse wheel or up and down arrow key is pressed. 
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell value can be rotated using mouse wheel or up and down arrow key ; otherwise <b>false</b> .
        /// </returns>
        protected internal override bool CanAllowSpinOnMouseScroll()
        {
            return this.AllowScrollingOnCircle;
        }
        #endregion
    }
    /// <summary>
    /// Represents a column that displays the percent values in its cell content.
    /// </summary>
    public class TreeGridPercentColumn : TreeGridEditorColumn
    {
        #region Dependency Properties
        /// <summary>
        /// Gets or sets the number of decimal places to use in percent values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in percent values. 
        /// </value>
        public int PercentDecimalDigits
        {
            get { return (int)GetValue(PercentDecimalDigitsProperty); }
            set { SetValue(PercentDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentDecimalDigitsProperty =
            DependencyProperty.Register("PercentDecimalDigits", typeof(int), typeof(TreeGridPercentColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.PercentDecimalDigits, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string to use as the decimal separator in percent values.
        /// </summary>
        /// <value>
        /// The string to use as the decimal separator in percent values.
        /// </value>
        public string PercentDecimalSeparator
        {
            get { return (string)GetValue(PercentDecimalSeparatorProperty); }
            set { SetValue(PercentDecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentDecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentDecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentDecimalSeparatorProperty =
           DependencyProperty.Register("PercentDecimalSeparator", typeof(string), typeof(TreeGridPercentColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.PercentDecimalSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in percent values.
        /// </summary>
        /// <value>
        /// The string that separates groups of digits to the left of the decimal in percent values. 
        /// </value>
        public string PercentGroupSeparator
        {
            get { return (string)GetValue(PercentGroupSeparatorProperty); }
            set { SetValue(PercentGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentGroupSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentGroupSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentGroupSeparatorProperty =
            DependencyProperty.Register("PercentGroupSeparator", typeof(string), typeof(TreeGridPercentColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.PercentGroupSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the percent editor loads percent or double value being edited in TreeGridPercentColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Windows.Shared.PercentEditMode"/> that decides the type of value loads in TreeGridPercentColumn being edited.
        /// The default value is <b>PercentEditMode.DoubleMode</b>.
        /// </value>
        public PercentEditMode PercentEditMode
        {
            get { return (PercentEditMode)GetValue(PercentEditModeProperty); }
            set { SetValue(PercentEditModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentEditMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentEditMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentEditModeProperty =
            DependencyProperty.Register("PercentEditMode", typeof(PercentEditMode), typeof(TreeGridPercentColumn), new PropertyMetadata(PercentEditMode.DoubleMode, OnUpdateBindingInfo));


        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in percent values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in percent values.
        /// </value>
        public Int32Collection PercentGroupSizes
        {
            get { return (Int32Collection)GetValue(PercentGroupSizesProperty); }
            set { SetValue(PercentGroupSizesProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentGroupSizes dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentGroupSizes dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentGroupSizesProperty =
            DependencyProperty.Register("PercentGroupSizes", typeof(Int32Collection), typeof(TreeGridPercentColumn), new PropertyMetadata(new Int32Collection { 0 }, OnUpdateBindingInfo));


        /// <summary>
        /// Gets or sets the format pattern for negative values in TreeGridPercentColumn.
        /// </summary>
        /// <value>
        /// The format pattern for negative percent values in TreeGridPercentColumn. 
        /// </value>
        public int PercentNegativePattern
        {
            get { return (int)GetValue(PercentNegativePatternProperty); }
            set { SetValue(PercentNegativePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentNegativePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentNegativePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentNegativePatternProperty =
            DependencyProperty.Register("PercentNegativePattern", typeof(int), typeof(TreeGridPercentColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.PercentNegativePattern, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern for the positive values in TreeGridPercentColumn.
        /// </summary>
        /// <value>
        /// The percent positive pattern in TreeGridPercentColumn.
        /// </value>
        public int PercentPositivePattern
        {
            get { return (int)GetValue(PercentPositivePatternProperty); }
            set { SetValue(PercentPositivePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentPositivePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentPositivePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentPositivePatternProperty =
            DependencyProperty.Register("PercentPositivePattern", typeof(int), typeof(TreeGridPercentColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.PercentPositivePattern, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string to use as the percent symbol.
        /// </summary>
        /// <value>
        /// The string to use as the percent symbol.
        /// </value>
        public string PercentSymbol
        {
            get { return (string)GetValue(PercentSymbolProperty); }
            set { SetValue(PercentSymbolProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentSymbol dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn.PercentSymbol dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentSymbolProperty =
            DependencyProperty.Register("PercentSymbol", typeof(string), typeof(TreeGridPercentColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.PercentSymbol, OnUpdateBindingInfo));

        #endregion
        #region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn"/> class.
        /// </summary>
        public TreeGridPercentColumn()
        {
            CellType = "Percent";
        }
        #endregion

        #region overrides

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the TreeGridPercentColumn.
        /// </summary>       
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
             this.padding= padding != DependencyProperty.UnsetValue
                           ? new Thickness(2 + padLeft, 2 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(2, 2, 3, 2);
#endif
        }


        #endregion
    }


    /// <summary>
    /// Represents a column that displays the currency values in its cell content.
    /// </summary>
    public class TreeGridCurrencyColumn : TreeGridEditorColumn
    {
        #region Dependency Properties
        /// <summary>
        /// Gets or sets the number of decimal places to use in currency values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in currency values.The default value is 2.
        /// </value>
        public int CurrencyDecimalDigits
        {
            get { return (int)GetValue(CurrencyDecimalDigitsProperty); }
            set { SetValue(CurrencyDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyDecimalDigitsProperty =
            DependencyProperty.Register("CurrencyDecimalDigits", typeof(int), typeof(TreeGridCurrencyColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the group of digits to the left of the decimal in currency values.
        /// </summary>
        /// <value>
        /// The string that separates the group of digits to the left of the decimal .The default value is ",".
        /// </value>
        public string CurrencyGroupSeparator
        {
            get { return (string)GetValue(CurrencyGroupSeparatorProperty); }
            set { SetValue(CurrencyGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyGroupSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyGroupSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyGroupSeparatorProperty =
            DependencyProperty.Register("CurrencyGroupSeparator", typeof(string), typeof(TreeGridCurrencyColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string to use as the currency symbol.
        /// </summary>
        /// <value>
        ///  The string that is used as the currency symbol.
        /// </value>
        public string CurrencySymbol
        {
            get { return (string)GetValue(CurrencySymbolProperty); }
            set { SetValue(CurrencySymbolProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencySymbol dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencySymbol dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencySymbolProperty =
            DependencyProperty.Register("CurrencySymbol", typeof(string), typeof(TreeGridCurrencyColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencySymbol, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates the decimal part in currency values.
        /// </summary>
        /// <value>
        /// The string that separates the decimal part in currency values. 
        /// </value>
        public string CurrencyDecimalSeparator
        {
            get { return (string)GetValue(CurrencyDecimalSeparatorProperty); }
            set { SetValue(CurrencyDecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyDecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyDecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyDecimalSeparatorProperty =
            DependencyProperty.Register("CurrencyDecimalSeparator", typeof(string), typeof(TreeGridCurrencyColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in currency values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in currency values.
        /// </value>
        public Int32Collection CurrencyGroupSizes
        {
            get { return (Int32Collection)GetValue(CurrencyGroupSizesProperty); }
            set { SetValue(CurrencyGroupSizesProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyGroupSizes dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyGroupSizes dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyGroupSizesProperty =
            DependencyProperty.Register("CurrencyGroupSizes", typeof(Int32Collection), typeof(TreeGridCurrencyColumn), new PropertyMetadata(new Int32Collection { 0 }, OnUpdateBindingInfo));


        /// <summary>
        /// Gets or sets the format pattern of positive currency values.
        /// </summary>
        /// <value>
        /// The format pattern of positive currency values.
        /// </value>
        public int CurrencyPositivePattern
        {
            get { return (int)GetValue(CurrencyPositivePatternProperty); }
            set { SetValue(CurrencyPositivePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyPositivePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyPositivePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyPositivePatternProperty =
            DependencyProperty.Register("CurrencyPositivePattern", typeof(int), typeof(TreeGridCurrencyColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyPositivePattern, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern of negative currency values.
        /// </summary>
        /// <value>
        /// The format pattern of negative currency values.
        /// </value>
        public int CurrencyNegativePattern
        {
            get { return (int)GetValue(CurrencyNegativePatternProperty); }
            set { SetValue(CurrencyNegativePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyNegativePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn.CurrencyNegativePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty CurrencyNegativePatternProperty =
            DependencyProperty.Register("CurrencyNegativePattern", typeof(int), typeof(TreeGridCurrencyColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.CurrencyNegativePattern, OnUpdateBindingInfo));

        #endregion
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrencyColumn"/> class.
        /// </summary>
        public TreeGridCurrencyColumn()
        {
            CellType = "Currency";
        }
        #endregion

        #region overrides
        /// <summary>
        /// Updates the binding for the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.AllowEditing"/> property changes in TreeGridCurrencyColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }
        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 2 + padTop, 3 + padRight, 5 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(2 + padLeft, 2 +padTop, 3 + padRight, 2+ padBotton)
                           : new Thickness(2, 2, 3, 2);
#endif
        }

        #endregion
    }
    public class TreeGridNumericColumn : TreeGridEditorColumn
    {
        #region Dependency Properties
        /// <summary>
        /// Gets or sets the number of decimal places to use in numeric values.
        /// </summary>
        /// <value>
        /// The number of decimal places to use in numeric values.
        /// </value>
        public int NumberDecimalDigits
        {
            get { return (int)GetValue(NumberDecimalDigitsProperty); }
            set { SetValue(NumberDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberDecimalDigitsProperty =
            DependencyProperty.Register("NumberDecimalDigits", typeof(int), typeof(TreeGridNumericColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.NumberDecimalDigits, OnUpdateBindingInfo));
#if WPF
        /// <summary>
        /// Gets or Sets the parsing mode option for Numeric column.The purpose of the parsing mode option is to take place  the cast conversion for the Edited value at the runtime.
        /// when using the dynamic object to holds the data .
        /// </summary>
        ///<value>
        ///The Parsing mode option to use in the Numeric column.
        ///</value> 
        public ParseMode ParsingMode
        {
            get { return (ParseMode)GetValue(ParsingModeProperty); }
            set { SetValue(ParsingModeProperty, value); }
        }
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.ParsingMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.ParsingMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty ParsingModeProperty =
             DependencyProperty.Register("ParsingMode", typeof(ParseMode), typeof(TreeGridNumericColumn), new PropertyMetadata(ParseMode.Double, OnUpdateBindingInfo));
#endif
        /// <summary>
        /// Gets or sets the string to use as the decimal separator in numeric values.
        /// </summary>
        /// <value>
        /// The string to use as the decimal separator in numeric values.
        /// </value>
        public string NumberDecimalSeparator
        {
            get { return (string)GetValue(NumberDecimalSeparatorProperty); }
            set { SetValue(NumberDecimalSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberDecimalSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberDecimalSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberDecimalSeparatorProperty =
            DependencyProperty.Register("NumberDecimalSeparator", typeof(string), typeof(TreeGridNumericColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the string that separates groups of digits to the left of the decimal in numeric values.
        /// </summary>
        /// <value>
        /// The string that separates groups of digits to the left of the decimal in numeric values.
        /// </value>
        public string NumberGroupSeparator
        {
            get { return (string)GetValue(NumberGroupSeparatorProperty); }
            set { SetValue(NumberGroupSeparatorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberGroupSeparator dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberGroupSeparator dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberGroupSeparatorProperty =
            DependencyProperty.Register("NumberGroupSeparator", typeof(string), typeof(TreeGridNumericColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in numeric values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in numeric values.
        /// </value>

        /// <summary>
        /// Gets or sets the number of digits in each group to the left of the decimal in numeric values.
        /// </summary>
        /// <value>
        /// The number of digits in each group to the left of the decimal in numeric values.
        /// </value>
        public Int32Collection NumberGroupSizes
        {
            get { return (Int32Collection)GetValue(NumberGroupSizesProperty); }
            set { SetValue(NumberGroupSizesProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberGroupSizes dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberGroupSizes dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberGroupSizesProperty =
            DependencyProperty.Register("NumberGroupSizes", typeof(Int32Collection), typeof(TreeGridNumericColumn), new PropertyMetadata(new Int32Collection { 0 }, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the format pattern for negative numeric values.
        /// </summary>
        /// <value>
        /// The format pattern for negative numeric values.
        /// </value>
        public int NumberNegativePattern
        {
            get { return (int)GetValue(NumberNegativePatternProperty); }
            set { SetValue(NumberNegativePatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberNegativePattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.NumberNegativePattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty NumberNegativePatternProperty =
            DependencyProperty.Register("NumberNegativePattern", typeof(int), typeof(TreeGridNumericColumn), new PropertyMetadata(NumberFormatInfo.CurrentInfo.NumberNegativePattern, OnUpdateBindingInfo));

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn"/> class.
        /// </summary>
        public TreeGridNumericColumn()
        {
            CellType = "Numeric";
        }
        #endregion

        #region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the TreeGridNumericColumn.
        /// </summary>       
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumnBase.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft,  padTop, 2 + padRight,padBotton)
                           : new Thickness(3, 1, 3, 0);
#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#endif
        }

#if WPF
        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.ValueBinding"/> of TreeGridNumericColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.ValueBinding"/>.
        /// </remarks>
        protected override void SetValueBindingConverter()
        {
            if (!isValueMultiBinding)
                if ((ValueBinding as Binding).Converter == null)
                    (ValueBinding as Binding).Converter = new ValueBindingConverter(this);
        }
#endif

        #endregion
    }
#endif

#if UWP
    /// <summary>
    /// Represents a column that displays the numeric values in its cell content.
    /// </summary>
    public class TreeGridNumericColumn : TreeGridTextColumnBase
    {
  
    #region Dependency Properties

        /// <summary>
        /// Gets or sets a value indicating whether the characters is blocked from an user input.
        /// </summary>
        /// <value>
        /// <b>true</b> if the characters blocked; otherwise, <b>false</b>.
        /// </value>
        public bool BlockCharactersOnTextInput
        {
            get { return (bool)GetValue(BlockCharactersOnTextInputProperty); }
            set { SetValue(BlockCharactersOnTextInputProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.BlockCharactersOnTextInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.BlockCharactersOnTextInput dependency property.
        /// </remarks>
        public static readonly DependencyProperty BlockCharactersOnTextInputProperty =
            DependencyProperty.Register("BlockCharactersOnTextInput", typeof(bool), typeof(TreeGridNumericColumn), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in TreeGridNumericColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed ; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNull
        {
            get { return (bool)GetValue(AllowNullProperty); }
            set { SetValue(AllowNullProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.AllowNullInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.AllowNullInput dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullProperty =
            DependencyProperty.Register("AllowNull", typeof(bool), typeof(TreeGridNumericColumn), new PropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that specifies how to format the bound value in TreeGridNumericColumn.
        /// </summary>
        /// <value>
        /// A string that specifies how to format the bound value in TreeGridNumericColumn. The default value is string.Empty.
        /// </value>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.FormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.FormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString", typeof(string), typeof(TreeGridNumericColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that decides whether the user can parse decimal or double value in TreeGridNumericColumn.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Controls.Input.Parsers"/> that specifies the parsing mode of TreeGridNumericColumn.
        /// The default mode is <b> Parsers.Double </b>.
        /// </value>
        public Parsers ParsingMode
        {
            get { return (Parsers)GetValue(ParsingModeProperty); }
            set { SetValue(ParsingModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.ParsingMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.ParsingMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty ParsingModeProperty =
            DependencyProperty.Register("ParsingMode", typeof(Parsers), typeof(TreeGridNumericColumn), new PropertyMetadata(Parsers.Double,OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the content displayed as a watermark in TreeGridNumericColumn when its cell contains empty value.
        /// </summary>
        /// <value>
        /// The content displayed as a watermark in TreeGridNumericColumn when its cell contains empty value.
        /// The default value is string.Empty .
        /// </value>
        public object WaterMark
        {
            get { return (object)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.WaterMark dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.WaterMark dependency property.
        /// </remarks>
        public static readonly DependencyProperty WaterMarkProperty =
            DependencyProperty.Register("WaterMark", typeof(object), typeof(TreeGridNumericColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum decimal digits to restrict the decimal values.
        /// </summary>
        /// <value>
        /// The default value is <see cref="F:System.Int32.MaxValue">Int32.MaxValue</see>.
        /// </value>
        public int MaximumNumberDecimalDigits
        {
            get { return (int)GetValue(MaximumNumberDecimalDigitsProperty); }
            set { SetValue(MaximumNumberDecimalDigitsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.MaximumNumberDecimalDigits dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.MaximumNumberDecimalDigits dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaximumNumberDecimalDigitsProperty =
            DependencyProperty.Register("MaximumNumberDecimalDigits", typeof(int), typeof(TreeGridNumericColumn), new PropertyMetadata(Int32.MaxValue));

        /// <summary>
        /// Gets or sets a value which specifies how to display numeric data in percent mode.
        /// </summary>
        /// <value>
        /// The default value is <see cref="Syncfusion.UI.Xaml.Controls.Input.PercentDisplayMode">PercentDisplayMode.Compute</see>.
        /// </value>
        public PercentDisplayMode PercentDisplayMode
        {
            get { return (PercentDisplayMode)GetValue(PercentDisplayModeProperty); }
            set { SetValue(PercentDisplayModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.PercentDisplayMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn.PercentDisplayMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty PercentDisplayModeProperty =
            DependencyProperty.Register("PercentDisplayMode", typeof(PercentDisplayMode), typeof(TreeGridNumericColumn), new PropertyMetadata(PercentDisplayMode.Compute, OnUpdateBindingInfo));


        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridNumericColumn"/> class.
        /// </summary>
        public TreeGridNumericColumn()
        {
            this.CellType = "Numeric";
            base.TextAlignment = TextAlignment.Right;
        }
    #endregion
        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(TreeGridColumn.PaddingProperty);
#if UWP
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, padTop, 2 + padRight, padBotton)
                           : new Thickness(3, 1, 3, 0);
#else
            this.padding= padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 6 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#endif
        }
        /// <summary>
        /// Invokes to update the column binding information
        /// </summary>
        internal override void UpdateBindingInfo()
        {
            if (this.TreeGrid == null)
                return;

            var binding = this.DisplayBinding as Binding;
            if (binding == null)
                return;
            this.formatString = this.FormatString;
            if (binding.Converter is TreeGridCultureFormatConverter)
                (binding.Converter as TreeGridCultureFormatConverter).UpdateFormatProvider();
            base.UpdateBindingInfo();
        }

    }
#endif

    /// <summary>
    /// Represents a column that displays the date time values in its cell content.
    /// </summary>
    public class TreeGridDateTimeColumn : TreeGridTextColumnBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal DateTimeFormatInfo dateTimeFormat = DateTimeFormatInfo.CurrentInfo;

#if UWP
        #region Dependency Properties
        /// <summary>
        /// Gets or sets a value that indicates whether the editing is enabled for TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the editing is enabled; otherwise, <b>false</b>.
        /// </value>
        public bool AllowInlineEditing
        {
            get { return (bool)GetValue(AllowInlineEditingProperty); }
            set { SetValue(AllowInlineEditingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowInlineEditing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowInlineEditing dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowInlineEditingProperty =
            DependencyProperty.Register("AllowInlineEditing", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a string that specifies how to format the bounded value in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// A string that specifies how to format the bound value in TreeGridDateTimeColumn. The default value is string.Empty.
        /// </value>
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.FormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.FormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString", typeof(string), typeof(TreeGridDateTimeColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether a drop-down button control is used to adjust the date time value.
        /// </summary>
        /// <value>
        /// <b>true</b> if the drop-down button is used to adjust the date time value ; otherwise , <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool ShowDropDownButton
        {
            get { return (bool)GetValue(ShowDropDownButtonProperty); }
            set { SetValue(ShowDropDownButtonProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.ShowDropDownButton dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.ShowDropDownButton dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowDropDownButtonProperty =
            DependencyProperty.Register("ShowDropDownButton", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the minimum date and time that can be selected in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The minimum date and time that can be selected in TreeGridDateTimeColumn. 
        /// </value>
        public DateTime MinDate
        {
            get { return (DateTime)GetValue(MinDateProperty); }
            set { SetValue(MinDateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MinDate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MinDate dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinDateProperty =
            DependencyProperty.Register("MinDate", typeof(DateTime), typeof(TreeGridDateTimeColumn), new PropertyMetadata(System.DateTime.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum date and time that can be selected in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The maximum date and time that can be selected in TreeGridDateTimeColumn. 
        /// </value>
        public DateTime MaxDate
        {
            get { return (DateTime)GetValue(MaxDateProperty); }
            set { SetValue(MaxDateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MaxDate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MaxDate dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxDateProperty =
            DependencyProperty.Register("MaxDate", typeof(DateTime), typeof(TreeGridDateTimeColumn), new PropertyMetadata(System.DateTime.MaxValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the content displayed as a watermark in TreeGridDateTimeColumn when its cell contains empty value.
        /// </summary>
        /// <value>
        /// The content displayed as a watermark in TreeGridDateTimeColumn when its cell contains empty value.
        /// The default value is string.Empty .
        /// </value>
        public object WaterMark
        {
            get { return (object)GetValue(WaterMarkProperty); }
            set { SetValue(WaterMarkProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.WaterMark dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.WaterMark dependency property.
        /// </remarks>
        public static readonly DependencyProperty WaterMarkProperty =
            DependencyProperty.Register("WaterMark", typeof(object), typeof(TreeGridDateTimeColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the Accent brush for the date selector items of TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The brush for the date selector items of TreeGridDateTimeColumn. The default value is <b>SlateBlue</b>.
        /// </value>
        public Brush AccentBrush
        {
            get { return (Brush)GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AccentBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AccentBrush dependency property.
        /// </remarks>
        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register("AccentBrush", typeof(Brush), typeof(TreeGridDateTimeColumn), new PropertyMetadata(new SolidColorBrush(Colors.SlateBlue)));

        /// <summary>
        /// Gets or sets the height of the drop-down of TreeGridDateTimeColumn.      
        /// </summary>
        /// <value>
        /// The height of the drop-down of TreeGridDateTimeColumn. The default value is 400.0 .
        /// </value>        
        public double DropDownHeight
        {
            get { return (double)GetValue(DropDownHeightProperty); }
            set { SetValue(DropDownHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.DropDownHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.DropDownHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty DropDownHeightProperty =
            DependencyProperty.Register("DropDownHeight", typeof(double), typeof(TreeGridDateTimeColumn), new PropertyMetadata(400.0));

        /// <summary>
        /// Gets or sets a value that indicates whether the drop-down of TreeGridDateTimeColumn is currently open.
        /// </summary>
        /// <value>
        /// <b>true</b> if the drop-down is open; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value> 
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.IsDropDownOpen dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.IsDropDownOpen dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the input scope of the on-screen keyboard for GridDateTimeColumn.
        /// </summary> 
        /// <value>
        /// One of the <see cref="Windows.UI.Xaml.Input.InputScopeNameValue"/> that specifies the input scope for GridDateTimeColumn. The default value <see cref="Windows.UI.Xaml.Input.InputScopeNameValue.Default"/>.
        /// </value>    
        public InputScopeNameValue InputScope
        {
            get { return (InputScopeNameValue)GetValue(InputScopeProperty); }
            set { SetValue(InputScopeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.InputScope dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.InputScope dependency property.
        /// </remarks>
        public static readonly DependencyProperty InputScopeProperty =
            DependencyProperty.Register("InputScope", typeof(InputScopeNameValue), typeof(TreeGridDateTimeColumn), new PropertyMetadata(InputScopeNameValue.Default));

        /// <summary>
        /// Gets or sets the SelectorItemCount for the date selector items
        /// </summary>
        /// <value> The default value is 0 </value>
        public int SelectorItemCount
        {
            get { return (int)GetValue(SelectorItemCountProperty); }
            set { SetValue(SelectorItemCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.InputScope dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.InputScope dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemCountProperty =
            DependencyProperty.Register("SelectorItemCount", typeof(int), typeof(TreeGridDateTimeColumn), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the space for between the date, month and year items in the selector of GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The space between the items in the selector. The default value is 4. 
        /// </value>
        public double SelectorItemSpacing
        {
            get { return (double)GetValue(SelectorItemSpacingProperty); }
            set { SetValue(SelectorItemSpacingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorItemSpacing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorItemSpacing dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemSpacingProperty =
            DependencyProperty.Register("SelectorItemSpacing", typeof(double), typeof(TreeGridDateTimeColumn), new PropertyMetadata(4.0));

        /// <summary>
        /// Gets or sets the width of the date selector items in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The width of the date selector items. The default value is 80.
        /// </value>
        public double SelectorItemWidth
        {
            get { return (double)GetValue(SelectorItemWidthProperty); }
            set { SetValue(SelectorItemWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorItemWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorItemWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemWidthProperty =
            DependencyProperty.Register("SelectorItemWidth", typeof(double), typeof(TreeGridDateTimeColumn), new PropertyMetadata(80.0));

        /// <summary>
        /// Gets or sets the height of the date selector items in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// The height of the date selector items. The default value is 80.
        /// </value>
        public double SelectorItemHeight
        {
            get { return (double)GetValue(SelectorItemHeightProperty); }
            set { SetValue(SelectorItemHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorItemHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorItemHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorItemHeightProperty =
            DependencyProperty.Register("SelectorItemHeight", typeof(double), typeof(TreeGridDateTimeColumn), new PropertyMetadata(80.0));


        /// <summary>
        /// Gets or sets the SelectorFormatString for the date picker column      
        /// </summary>        
        public object SelectorFormatString
        {
            get { return (object)GetValue(SelectorFormatStringProperty); }
            set { SetValue(SelectorFormatStringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorFormatString dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.SelectorFormatString dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectorFormatStringProperty =
            DependencyProperty.Register("SelectorFormatString", typeof(object), typeof(TreeGridDateTimeColumn), new PropertyMetadata("m:d:y"));

        #endregion
#else
        #region Dependency Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the user can change the cell values using the mouse wheel or up and down arrow key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is changed using the mouse wheel or up and down arrow key; otherwise , <b>false</b>.
        /// The default value is <b> true</b>.
        /// </value>
        public bool AllowScrollingOnCircle
        {
            get { return (bool)GetValue(AllowScrollingOnCircleProperty); }
            set { SetValue(AllowScrollingOnCircleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowScrollingOnCircle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowScrollingOnCircle dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowScrollingOnCircleProperty =
            DependencyProperty.Register("AllowScrollingOnCircle", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(true, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that is displayed instead of null value if the cell value is null.
        /// </summary>          
        /// <value>
        /// A <see cref="System.DateTime"/> that is displayed instead of null value in the cells of TreeGridDateTimeColumn.
        /// </value>
        /// <remarks>
        /// The <b>NullValue</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public DateTime? NullValue
        {
            get { return (DateTime?)GetValue(NullValueProperty); }
            set { SetValue(NullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.NullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.NullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty NullValueProperty =
            DependencyProperty.Register("NullValue", typeof(DateTime?), typeof(TreeGridDateTimeColumn), new PropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a string that is displayed instead of null value if the cell value is null.
        /// </summary>
        /// <value>
        /// A string that is displayed instead of null value in the cell of TreeGridDateTimeColumn.
        /// </value>
        /// <remarks>
        /// The <b>NullText</b> is applied ,when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowNullValue"/> property is enabled.
        /// </remarks>
        public string NullText
        {
            get { return (string)GetValue(NullTextProperty); }
            set { SetValue(NullTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.NullText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.NullText dependency property.
        /// </remarks>
        public static DependencyProperty NullTextProperty =
            DependencyProperty.Register("NullText", typeof(string), typeof(TreeGridDateTimeColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the classic style is enabled on the drop-down of TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the classic style is enabled; otherwise ,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool EnableClassicStyle
        {
            get { return (bool)GetValue(EnableClassicStyleProperty); }
            set { SetValue(EnableClassicStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.EnableClassicStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.EnableClassicStyle dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableClassicStyleProperty =
            DependencyProperty.Register("EnableClassicStyle", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the date selection is disabled on the calendar pop-up of TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the date selection is disabled on the calendar pop-up; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool DisableDateSelection
        {
            get { return (bool)GetValue(DisableDateSelectionProperty); }
            set { SetValue(DisableDateSelectionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.DisableDateSelection dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.DisableDateSelection dependency property.
        /// </remarks>
        public static readonly DependencyProperty DisableDateSelectionProperty =
            DependencyProperty.Register("DisableDateSelection", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether a repeat button control is used to adjust the date and time value in GridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the repeat button control is used to adjust the date and time value; otherwise , <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool ShowRepeatButton
        {
            get { return (bool)GetValue(ShowRepeatButtonProperty); }
            set { SetValue(ShowRepeatButtonProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.ShowRepeatButton dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.ShowRepeatButton dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowRepeatButtonProperty =
            DependencyProperty.Register("ShowRepeatButton", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the format string for a date and time value.
        /// </summary>
        /// <value>
        /// The format string for a date and time value in TreeGridDateTimeColumn.The default value is <see cref="Syncfusion.Windows.Shared.DateTimePattern.ShortDate"/>.
        /// </value>
        public DateTimePattern Pattern
        {
            get { return (DateTimePattern)GetValue(PatternProperty); }
            set { SetValue(PatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.Pattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.Pattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty PatternProperty =
            DependencyProperty.Register("Pattern", typeof(DateTimePattern), typeof(TreeGridDateTimeColumn), new PropertyMetadata(DateTimePattern.ShortDate, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a <see cref="System.Globalization.DateTimeFormatInfo"/> that defines the format of date and time values.
        /// </summary>
        /// <value>
        /// A <see cref="System.Globalization.DateTimeFormatInfo"/> that defines the format of date and time values.
        /// </value>
        public DateTimeFormatInfo DateTimeFormat
        {
            get { return (DateTimeFormatInfo)GetValue(DateTimeFormatProperty); }
            set { SetValue(DateTimeFormatProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.DateTimeFormat dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.DateTimeFormat dependency property.
        /// </remarks>
        public static readonly DependencyProperty DateTimeFormatProperty =
            DependencyProperty.Register("DateTimeFormat", typeof(DateTimeFormatInfo), typeof(TreeGridDateTimeColumn), new PropertyMetadata(DateTimeFormatInfo.CurrentInfo, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the minimum date value allowed in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the minimum date value in TreeGridDateTimeColumn.
        /// </value>
        public DateTime MinDateTime
        {
            get { return (DateTime)GetValue(MinDateTimeProperty); }
            set { SetValue(MinDateTimeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MinDateTime dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MinDateTime dependency property.
        /// </remarks>
        public static readonly DependencyProperty MinDateTimeProperty =
            DependencyProperty.Register("MinDateTime", typeof(DateTime), typeof(TreeGridDateTimeColumn), new PropertyMetadata(System.DateTime.MinValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the maximum date value allowed in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the maximum date value in TreeGridDateTimeColumn.
        /// </value>
        public DateTime MaxDateTime
        {
            get { return (DateTime)GetValue(MaxDateTimeProperty); }
            set { SetValue(MaxDateTimeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MaxDateTime dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.MaxDateTime dependency property.
        /// </remarks>
        public static readonly DependencyProperty MaxDateTimeProperty =
            DependencyProperty.Register("MaxDateTime", typeof(DateTime), typeof(TreeGridDateTimeColumn), new PropertyMetadata(System.DateTime.MaxValue, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the custom pattern for date and time value.
        /// </summary>
        /// <value>
        /// The custom pattern for date and time value. The default value is string.Empty.
        /// </value>
        /// <remarks>
        /// To apply a CustomPattern, specify the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.Pattern"/> as <see cref="DateTimePattern.CustomPattern"/>.
        /// </remarks>
        public string CustomPattern
        {
            get { return (string)GetValue(CustomPatternProperty); }
            set { SetValue(CustomPatternProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.CustomPattern dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.CustomPattern dependency property.
        /// </remarks>
        public static readonly DependencyProperty CustomPatternProperty =
            DependencyProperty.Register("CustomPattern", typeof(string), typeof(TreeGridDateTimeColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that decides whether the date and time value can be edited.
        /// </summary>
        /// <value>
        /// <b>true</b> if the date and time value can be edited ; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool CanEdit
        {
            get { return (bool)GetValue(CanEditProperty); }
            set { SetValue(CanEditProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.CanEdit dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.CanEdit dependency property.
        /// </remarks>
        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether a user can delete the date and time value by using Delete key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Delete key is enabled; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The <b>EnableDeleteKey</b> worked based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.CanEdit"/> property.
        /// </remarks>
        public bool EnableBackspaceKey
        {
            get { return (bool)GetValue(EnableBackspaceKeyProperty); }
            set { SetValue(EnableBackspaceKeyProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.EnableBackspaceKey dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.EnableBackspaceKey dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableBackspaceKeyProperty =
            DependencyProperty.Register("EnableBackspaceKey", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether a user can delete the date and time value by using Delete key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Delete key is enabled; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The <b>EnableDeleteKey</b> worked based on <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.CanEdit"/> property.
        /// </remarks>
        public bool EnableDeleteKey
        {
            get { return (bool)GetValue(EnableDeleteKeyProperty); }
            set { SetValue(EnableDeleteKeyProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.EnableDeleteKey dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.EnableDeleteKey dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableDeleteKeyProperty =
            DependencyProperty.Register("EnableDeleteKey", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false));

        #endregion
#endif
        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in TreeGridDateTimeColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed in TreeGridDateTimeColumn; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullValue
        {
            get { return (bool)GetValue(AllowNullValueProperty); }
            set { SetValue(AllowNullValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowNullValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridDateTimeColumn.AllowNullValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullValueProperty =
            DependencyProperty.Register("AllowNullValue", typeof(bool), typeof(TreeGridDateTimeColumn), new PropertyMetadata(false, OnUpdateBindingInfo));

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.TreeGridDateTimeColumn"/> class.
        /// </summary>
        public TreeGridDateTimeColumn()
        {
            CellType = "DateTime";
        }

        #region overrides
        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in TreeGridDateTimeColumn.
        /// </summary>            
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(TreeGridColumn.PaddingProperty);
#if WPF
            this.padding = padding != DependencyProperty.UnsetValue
                          ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 5 + padBotton)
                          : new Thickness(3, 1, 3, 1);

#else
            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, padTop, 2 + padRight, padBotton)
                           : new Thickness(3, 1, 2, 0);
#endif

        }

        #endregion
    }
    /// <summary>
    /// Represents a column that host the ComboBox and enumeration as its cell content in edit mode.
    /// </summary>
    public class TreeGridComboBoxColumn : TreeGridColumn
    {
        /// <summary>
        /// Gets or sets the path that is used to get the SelectedValue from the SelectedItem.
        /// </summary>
        /// <value>
        /// The path that is used to get the SelectedValue from the SelectedItem.
        /// </value>
        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set { SetValue(SelectedValuePathProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.SelectedValuePath dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.SelectedValuePath dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register("SelectedValuePath", typeof(string), typeof(TreeGridComboBoxColumn), new PropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a collection used to generate the content of TreeGridComboBoxColumn.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of TreeGridComboBoxColumn. The default value is null.
        /// </value>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.ItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.ItemsSource dependency property.
        /// </remarks>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TreeGridComboBoxColumn), new PropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets the path that is used to display the visual representation of object.
        /// </summary>
        /// <value>
        /// A string that represents the path to display the visual representation of object.
        /// </value>
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.DisplayMemberPath dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.DisplayMemberPath dependency property.
        /// </remarks>        
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register("DisplayMemberPath", typeof(string), typeof(TreeGridComboBoxColumn), new PropertyMetadata(string.Empty, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether a TreeGridComboBoxColumn that opens and displays a drop-down control when a user clicks its text area .
        /// </summary>
        /// <value>
        /// <b>true</b> to keep the drop-down control open when the user clicks on the text area to start editing; otherwise , <b>false</b> . 
        /// The default value is <b>false</b>.
        /// </value>
        public bool StaysOpenOnEdit
        {
            get { return (bool)GetValue(StaysOpenOnEditProperty); }
            set { SetValue(StaysOpenOnEditProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.StaysOpenOnEdit dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.StaysOpenOnEdit dependency property.
        /// </remarks>
        public static readonly DependencyProperty StaysOpenOnEditProperty =
            DependencyProperty.Register("StaysOpenOnEdit", typeof(bool), typeof(TreeGridComboBoxColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can edit the cell value by typing through editor of TreeGridComboBoxColumn.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell value is edited by typing through the editor of TreeGridComboBoxColumn ; otherwise , <b>false</b>. 
        /// The default value is <b>false</b> .
        /// </value>
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.IsEditable dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.IsEditable dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(TreeGridComboBoxColumn), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> that is used to display each item in TreeGridComboBoxColumn.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.DataTemplate"/> that is used to display each item in TreeGridComboBoxColumn.
        /// The default value is null.
        /// </value>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.ItemTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn.ItemTemplate dependency property.
        /// </remarks>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TreeGridComboBoxColumn), new PropertyMetadata(null, OnUpdateBindingInfo));

        /// <summary>
        /// Sets the converter for the <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.DisplayBinding"/> of GridComboBoxColumn.
        /// </summary>
        /// <remarks>
        /// You can override this method to specify the converter for <see cref="Syncfusion.UI.Xaml.Grid.TreeGridColumn.DisplayBinding"/> .
        /// </remarks>
        protected override void SetDisplayBindingConverter()
        {
            if (!isDisplayMultiBinding)
                if ((DisplayBinding as Binding).Converter == null)
                    (DisplayBinding as Binding).Converter = new DisplayMemberConverter(this);

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridComboBoxColumn"/> class.
        /// </summary>
        public TreeGridComboBoxColumn()
        {
            this.CellType = "ComboBox";
            Padding = new Thickness(4, 2, 4, 2);
            IsDropDown = true;
        }

        #region overrides

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the TreeGridComboBoxColumn.
        /// </summary>           
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }      

        internal override void ProcessUIElementPadding(GridColumnBase column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(TreeGridColumn.PaddingProperty);

            this.padding = padding != DependencyProperty.UnsetValue
                           ? new Thickness(1 + padLeft, 0 + padTop, 1 + padRight, 0 + padBotton)
                           : new Thickness(1, 0, 1, 0);
        }

        #endregion
    }
    /// <summary>
    /// Represents a column that used to display and edit boolean values and hosts CheckBox as its cell content.
    /// </summary>
    public class TreeGridCheckBoxColumn : TreeGridColumn
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCheckBoxColumn"/> class.
        /// </summary>
        public TreeGridCheckBoxColumn()
        {
            this.CellType = "CheckBox";
        }

        /// <summary>
        /// Gets or sets the horizontal alignment for the column .
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.HorizontalAlignment"/> enumeration that specifies the horizontal alignment. The default value is <b>HorizontalAlignment.Stretch)</b>
        /// </value>
        public HorizontalAlignment HorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); }
            set { SetValue(HorizontalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCheckBoxColumn.HorizontalAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCheckBoxColumn.HorizontalAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(TreeGridCheckBoxColumn), new PropertyMetadata(HorizontalAlignment.Center, OnUpdateBindingInfo));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can enable the Intermediate state of the CheckBox other than the Checked and Unchecked state.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Intermediate state is enabled in TreeGridCheckBoxColumn; otherwise <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool IsThreeState
        {
            get { return (bool)GetValue(IsThreeStateProperty); }
            set { SetValue(IsThreeStateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridCheckBoxColumn.IsThreeState dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridCheckBoxColumn.IsThreeState dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsThreeStateProperty =
            DependencyProperty.Register("IsThreeState", typeof(bool), typeof(TreeGridCheckBoxColumn), new PropertyMetadata(false, OnUpdateBindingInfo));

        #region overrides

        /// <summary>
        /// Invoked when the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumn.CellTemplate"/> dependency property value changed in the TreeGridCheckBoxColumn.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains data for <b>CellTemplate</b> dependency property changes.
        /// </param>
        protected override void OnCellTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            throw new NotSupportedException("The " + this.ToString() + " does not implement CellTemplate property");
        }

        /// <summary>
        /// Updates the binding for the <b>AllowEdit</b> property changes in the TreeGridCheckBoxColumn.
        /// </summary>        
        protected internal override void UpdateBindingBasedOnAllowEditing()
        {
            base.UpdateBindingBasedOnAllowEditing();
        }

        #endregion
    }
}

