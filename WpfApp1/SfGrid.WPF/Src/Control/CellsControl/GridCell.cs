#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if WinRT || UNIVERSAL
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using Windows.Devices.Input;
using Windows.Foundation;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Data;
using Syncfusion.UI.Xaml.Utility;
#endif
namespace Syncfusion.UI.Xaml.Grid
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Syncfusion.UI.Xaml.Grid.Cells;
#endif

    [ClassReference(IsReviewed = false)]
#if WPF
    public class GridCell : GridElement, IDisposable
#else
    public class GridCell : ContentControl, IDisposable
#endif
    {
        #region Fields

        internal string roweventErrorMessage;
        internal string celleventErrorMessage;
        internal string attributeErrorMessage;
        internal string bindingErrorMessage;
        /// <summary>
        /// Whether to decide begin editing for GridCell
        /// </summary>
        internal bool IsEditable = true;
        private bool isdisposed = false;
#if WPF
        private bool isPreviewMouseDown;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ColumnBase details of the GridCell.
        /// </summary>
        public DataColumnBase ColumnBase { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether GridCell has error or not.
        /// </summary>
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(celleventErrorMessage) || !string.IsNullOrEmpty(attributeErrorMessage) || !string.IsNullOrEmpty(bindingErrorMessage) || !string.IsNullOrEmpty(roweventErrorMessage);
            }
        }

        private string gridCellRegion = "NormalCell";
        internal string GridCellRegion
        {
            get
            {
                return gridCellRegion;
            }
            set
            {
                if (gridCellRegion != value)
                    this.ApplyGridCellVisualStates(value);
                gridCellRegion = value;
            }
        }

        #endregion

        #region Dependency Region

        /// <summary>
        /// Get or sets the CurrentCellBorder visibility which is bind to CurrentCell Border visibility property.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Visibility CurrentCellBorderVisibility
        {
            get { return (Visibility)GetValue(CurrentCellBorderVisibilityProperty); }
            set { SetValue(CurrentCellBorderVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets cell error message which is displayed in cell error indicator's ToolTip.
        /// </summary>
        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(GridCell), new PropertyMetadata(string.Empty));



        /// <summary>
        /// Dependency registration for CurrentCellBorderVisiblity.
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty CurrentCellBorderVisibilityProperty =
            DependencyProperty.Register("CurrentCellBorderVisibility", typeof(Visibility), typeof(GridCell), new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets or sets a thickness for CurrentCell border
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Thickness CurrentCellBorderThickness
        {
            get { return (Thickness)GetValue(CurrentCellBorderThicknessProperty); }
            set { SetValue(CurrentCellBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Dependency registration for CurrentCellBorderThickness.
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty CurrentCellBorderThicknessProperty =
            DependencyProperty.Register("CurrentCellBorderThickness", typeof(Thickness), typeof(GridCell), new PropertyMetadata(new Thickness(2)));

        /// <summary>
        /// Gets or sets Brush for CurrnetCell border.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Brush CurrentCellBorderBrush
        {
            get { return (Brush)GetValue(CurrentCellBorderBrushProperty); }
            set { SetValue(CurrentCellBorderBrushProperty, value); }
        }

        /// <summary>
        /// Dependency registration for CurrnetCell border brush
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty CurrentCellBorderBrushProperty =
            DependencyProperty.Register("CurrentCellBorderBrush", typeof(Brush), typeof(GridCell), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public Visibility SelectionBorderVisibility
        {
            get { return (Visibility)GetValue(SelectionBorderVisibilityProperty); }
            set { SetValue(SelectionBorderVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectionBorderVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectionBorderVisibilityProperty =
            DependencyProperty.Register("SelectionBorderVisibility", typeof(Visibility), typeof(GridCell), new PropertyMetadata(Visibility.Collapsed, OnSelectionBorderVisiblityChanged));



        private static void OnSelectionBorderVisiblityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var gridCell = obj as GridCell;

            // If SelectionBorderVisiblity is Visible, bind GridCell Foreground with SelectionForegroundBrush
            if (args.NewValue.Equals(Visibility.Visible))
            {
                if (gridCell.SelectionForegroundBrush != SfDataGrid.GridSelectionForgroundBrush)
                    gridCell.Foreground = gridCell.SelectionForegroundBrush;
                else if (SfDataGrid.GridSelectionForgroundBrush != gridCell.ColumnBase.Renderer.DataGrid.SelectionForegroundBrush)
                    gridCell.Foreground = gridCell.ColumnBase.Renderer.DataGrid.SelectionForegroundBrush;
#if UWP
                else if (ForegroundProperty.GetMetadata(typeof(FrameworkElement)).DefaultValue == gridCell.Foreground)
                    gridCell.Foreground = SfDataGrid.GridSelectionForgroundBrush;
#endif
            }
            else
                gridCell.ClearValue(GridCell.ForegroundProperty);
        }

        public Brush CellSelectionBrush
        {
            get { return (Brush)GetValue(CellSelectionBrushProperty); }
            set { SetValue(CellSelectionBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CellSelectionBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellSelectionBrushProperty =
            DependencyProperty.Register("CellSelectionBrush", typeof(Brush), typeof(GridCell), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));


        /// <summary>
        /// Gets or sets the value for SelectionForegroundBrush
        /// </summary>
        public Brush SelectionForegroundBrush
        {
            get { return (Brush)GetValue(SelectionForegroundBrushProperty); }
            set { SetValue(SelectionForegroundBrushProperty, value); }
        }

        /// <summary>
        /// Dependency Registration for SelectionForegroundBrush
        /// </summary>
        public static readonly DependencyProperty SelectionForegroundBrushProperty =
            GridDependencyProperty.Register("SelectionForegroundBrush", typeof(Brush), typeof(GridCell), new GridPropertyMetadata(SfDataGrid.GridSelectionForgroundBrush, OnSelectionForegroundBrushPropertyChanged));

        private static void OnSelectionForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gridCell = d as GridCell;
            if (gridCell == null)
                return;
            if (gridCell.SelectionBorderVisibility == Visibility.Visible)
                gridCell.Foreground = gridCell.SelectionForegroundBrush;
        }

#if WPF
        /// <summary>
        /// Gets or sets the value that indicates whether to load light weight template for GridCells to improve loading and scrolling performance.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridCell.UseDrawing"/> enumeration that specifies how the grid cells are rendered.
        /// The default value is false./>.
        /// </value>               
        public bool UseDrawing
        {
            get { return (bool)GetValue(UseDrawingProperty); }
            set { SetValue(UseDrawingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridCell.UseDrawing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridCell.UseDrawing dependency property.
        /// </remarks>         
        public static readonly DependencyProperty UseDrawingProperty =
            DependencyProperty.Register("UseDrawing", typeof(bool), typeof(GridCell), new PropertyMetadata(false));
#endif
#endregion

		#region Ctor

        public GridCell()
        {
            this.DefaultStyleKey = typeof(GridCell);
#if WPF
            this.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnErrorHandled), true);
#endif
        }

# if WPF
        private void OnErrorHandled(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added)
                this.bindingErrorMessage = e.Error.ErrorContent.ToString();
            else if (this.Content is DependencyObject)
            {
                if (!GridHelper.GetHasError(this.Content as DependencyObject))
                    this.bindingErrorMessage = string.Empty;
            }
            else
            {
                // WPF-33699 - To check validation when template is loaded in GridCell.               
                if (!GridHelper.GetHasError(this))
                    this.bindingErrorMessage = string.Empty;
            }
            // WPF-33863 - After deleting the cell value (if type does not support nullable value), this method will be called with ValidationStep as  ConvertedProposedValue. Since this is not updated value, error message should be removed.
            if (e.Error.RuleInError != null && e.Error.RuleInError.ValidationStep == ValidationStep.ConvertedProposedValue)
                this.bindingErrorMessage = string.Empty;
            this.ApplyValidationVisualState();
        }

#endif
        #endregion

        #region Overrides

#if WPF
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();

            this.ApplyValidationVisualState(false);
            this.ApplyGridCellVisualStates(this.GridCellRegion, false);
#if WPF
            UnwireEvent();
            WireEvent();
#endif
        }
#if WPF
        /// <summary>
        /// Wire ContextMenuOpeningEvent
        /// </summary>
        internal void WireEvent()
        {
            this.ContextMenuOpening += OnContextMenuOpening;
        }

        /// <summary>
        /// Unwire ContextMenuOpeningEvent
        /// </summary>
        internal void UnwireEvent()
        {
            this.ContextMenuOpening -= OnContextMenuOpening;
        }
#endif
        /// <summary>
        /// Opens the context menu at the specified position.
        /// </summary>
        /// <param name="position">The position to display context menu.</param>
        /// <b>true</b> If the context menu opened;Otherwise<b>false</b>
        /// </returns>
#if UWP
        protected internal virtual bool ShowContextMenu(Point position)
#else
        protected internal virtual bool ShowContextMenu()
#endif
        {
            if (ColumnBase == null || ColumnBase.Renderer == null || ColumnBase.Renderer.DataGrid == null)
                return false;
#if WPF
            ContextMenu contextMenu = null;
#else
            MenuFlyout contextMenu = null;
#endif
            var dataGrid = ColumnBase.Renderer.DataGrid;
            GridContextMenuEventArgs args = null;
            var rowColIndex = new RowColumnIndex(ColumnBase.RowIndex, this.ColumnBase.ColumnIndex);

            var menuinfo = new GridRecordContextMenuInfo() { Record = this.DataContext };

            if (dataGrid is DetailsViewDataGrid)
            {
                menuinfo.DataGrid = dataGrid;
                menuinfo.SourceDataGrid = (dataGrid as DetailsViewDataGrid).GetTopLevelParentDataGrid();
            }
            else
                menuinfo.DataGrid = dataGrid;

            if (this is GridCaptionSummaryCell)
            {
                if (dataGrid.GroupCaptionContextMenu != null)
                {
                    contextMenu = dataGrid.GroupCaptionContextMenu;
                    args = new GridContextMenuEventArgs(contextMenu, menuinfo, rowColIndex, ContextMenuType.GroupCaption, dataGrid);
                }
            }
            else if (this is GridGroupSummaryCell)
            {
                if (dataGrid.GroupSummaryContextMenu != null)
                {
                    contextMenu = dataGrid.GroupSummaryContextMenu;
                    args = new GridContextMenuEventArgs(contextMenu, menuinfo, rowColIndex, ContextMenuType.GroupSummary, dataGrid);
                }
            }
            else if (this is GridTableSummaryCell)
            {
                if (dataGrid.TableSummaryContextMenu != null)
                {
                    contextMenu = dataGrid.TableSummaryContextMenu;
                    args = new GridContextMenuEventArgs(contextMenu, menuinfo, rowColIndex, ContextMenuType.TableSummary, dataGrid);
                }
            }
            else if (dataGrid.RecordContextMenu != null && !ColumnBase.IsIndentColumn &&
                !(this is GridIndentCell) && !(this is GridRowHeaderIndentCell))
            {
                contextMenu = dataGrid.RecordContextMenu;
                args = new GridContextMenuEventArgs(contextMenu, menuinfo, rowColIndex, ContextMenuType.RecordCell, dataGrid);
            }

            if (args != null && !ColumnBase.Renderer.DataGrid.RaiseGridContextMenuEvent(args))
            {
#if WPF
                if (args.ContextMenuInfo != null)
                    contextMenu.DataContext = args.ContextMenuInfo;
                contextMenu.PlacementTarget = this;
                contextMenu.IsOpen = true;
#else
                if (args.ContextMenuInfo != null)
                    foreach (var item in args.ContextMenu.Items)
                        item.DataContext = args.ContextMenuInfo;
                args.ContextMenu.ShowAt(this, position);
#endif
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the Visual State of the GridCell based on the cell validation applied on SfDataGrid.
        /// </summary>
        protected internal virtual void ApplyValidationVisualState(bool canApplyDefaultState = true)
        {
            ErrorMessage = string.Empty;
            if (this.HasError)
            {
                if (!string.IsNullOrEmpty(bindingErrorMessage))
                    ErrorMessage = !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage + string.Format("\n") + bindingErrorMessage : bindingErrorMessage;
                else if (!string.IsNullOrEmpty(attributeErrorMessage))
                    ErrorMessage = !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage + string.Format("\n") + attributeErrorMessage : attributeErrorMessage;
                else if (!string.IsNullOrEmpty(celleventErrorMessage))
                    ErrorMessage = celleventErrorMessage;
                else if (!string.IsNullOrEmpty(roweventErrorMessage))
                    ErrorMessage = roweventErrorMessage;

                VisualStateManager.GoToState(this, "HasError", true);
            }
            else if (canApplyDefaultState)
            {
                VisualStateManager.GoToState(this, "NoError", true);
            }
        }

        /// <summary>
        /// Invoked when ColumnBase properties are changed.
        /// </summary>
        /// <param name="propertyName">The name of the property which is changed in ColumnBase.</param>
        protected internal virtual void OnDataColumnPropertyChanged(string propertyName)
        { }

        /// <summary>
        /// Invoked when columns are reused in horizontal scrolling.
        /// </summary>
        protected internal virtual void OnColumnChanged()
        {

        }

        public void ApplyGridCellVisualStates(string cellRegion, bool canApplyDefaultState = true)
        {
            if (canApplyDefaultState)
                VisualStateManager.GoToState(this, cellRegion, true);
            else if (!cellRegion.Equals("NormalCell"))
                VisualStateManager.GoToState(this, cellRegion, true);
        }

#if UWP
        /// <summary>
        /// Method override to set tooltip for GridCell.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerEntered(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerEntered();
            base.OnPointerEntered(e);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (ColumnBase != null)
            {
                this.ColumnBase.RaisePointerPressed(e);
                //WRT-4919 - We have set the OSK(On Screen Keyboard) as a Numeric for EditElement in the Renderer,
                //OSK is changed from  Numeric to AlphaNumeric while scrolling or click on to other cell, 
                //because focus is changed from EditElement to GridCell, So focus is didn't set to GridCell 
                //when the cell is in edit mode.
                if (ColumnBase.Renderer != null && ColumnBase.Renderer.DataGrid != null
                    && (!ColumnBase.Renderer.DataGrid.SelectionController.CurrentCellManager.HasCurrentCell || !ColumnBase.Renderer.DataGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing))
                    this.Focus(FocusState.Programmatic);
            }
            e.Handled = true;
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerReleased(e);
            base.OnPointerReleased(e);
        }

        protected override void OnPointerMoved(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerMoved(e);
            base.OnPointerMoved(e);
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.OnDoubleTapped(e);
            base.OnDoubleTapped(e);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.OnTapped(e);
            base.OnTapped(e);
        }

        /// <summary>
        /// When long press on SfDataGrid Cell, Context menu appears for the selected cell. 
        /// We are using this event for context menu support in Record cell, Caption Summary cell, Table summary cell and Group summary cells.
        /// </summary>
        /// <param name="e">Holding event arguments</param>
        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                var position = e.GetPosition(this);
                if (ShowContextMenu(position))
                    e.Handled = true;
            }
            base.OnHolding(e);
        }

        /// <summary>
        /// When Right click the SfDataGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Record cell, Caption Summary cell, Table summary cell and Group summary cells.
        /// </summary>
        /// <param name="e">Right tapped event arguments</param>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            if (ShowContextMenu(position))
                e.Handled = true;
            base.OnRightTapped(e);
        }
#else
        /// <summary>
        /// Occurs when any context menu on the element is opened.
        /// </summary>
        /// <param name="sender">The sender which contains SfDataGrid</param>
        /// <param name="e">Context menu event arguments</param>
        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ShowContextMenu())
                e.Handled = true;
        }


        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            isPreviewMouseDown = true;
            ProcessMouseDown(e);
            base.OnPreviewMouseDown(e);
        }
        /// <summary>
        /// When click the pop up and select the row, the OnPreviewMouseDown() method is not hit and the row is not selected because the Scrollviewer handled the OnPreviewMouseDown method. So here we written the OnMouseDown() method to overcome this issue.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!isPreviewMouseDown)
                ProcessMouseDown(e);
            isPreviewMouseDown = false;
            base.OnMouseDown(e);
        }

        private void ProcessMouseDown(MouseButtonEventArgs e)
        {
            if (VisualContainer.GetWantsMouseInput(e.OriginalSource as DependencyObject, this) == false)
            {
                if (!this.IsKeyboardFocusWithin)
                {
                    //if (ColumnBase != null && ColumnBase is DataColumn && ColumnBase.GridColumn != null && ColumnBase.GridColumn.AllowFocus)
                    this.Focus();
                    e.Handled = !ValidationHelper.IsCurrentCellValidated;
                }
            }

            if (ColumnBase != null)
                ColumnBase.RaisePointerPressed(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                ColumnBase.RaisePointerReleased(e);
            base.OnPreviewMouseUp(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.OnTapped(e);
            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                ColumnBase.OnDoubleTapped(e);
            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Method override to set tooltip for GridCell.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerEntered();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerMoved(e);
            base.OnMouseMove(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (ColumnBase != null)
                ColumnBase.RaisePointerWheel();
            base.OnPreviewMouseWheel(e);
        }
#endif

        #endregion

        #region Internal Method

        protected internal virtual bool CanSelectCurrentCell()
        {
            return true;
        }

        /// <summary>
        /// Resets the cell validation or attribute validation error message of the GridCell.
        /// </summary>
        /// <param name="errorMessage">
        /// Specifies the error message which is displayed in cell error indicator's ToolTip based on cell validation.        
        /// </param>
        /// <param name="isAttributeError">
        /// true, if need to reset attribute validation error message. Otherwise, false which resets the cell validation error message.
        /// </param>  
        internal void SetCellValidationError(string errorMessage, bool isAttributeError)
        {
            if (isAttributeError)
                attributeErrorMessage = errorMessage;
            else
                celleventErrorMessage = errorMessage;
            ApplyValidationVisualState();
        }

        /// <summary>
        /// Resets the row validation error message of the GridCell.
        /// </summary>
        /// <param name="errorMessage">
        /// Specifies the error message of which is displayed in cell error indicator's ToolTip based on row validation.     
        /// </param>
        internal void SetRowValidationError(string errorMessage)
        {
            roweventErrorMessage = errorMessage;
            ApplyValidationVisualState();
        }

        /// <summary>
        /// Resets the cell validation or attribute validation error message of the GridCell.
        /// </summary>
        /// <param name="isAttributeError">
        /// true, if need to reset attribute validation error message. Otherwise, false which resets the cell validation error message.
        /// </param> 
        internal void RemoveCellValidationError(bool isAttributeError)
        {
            if (isAttributeError)
                attributeErrorMessage = string.Empty;
            else
                celleventErrorMessage = string.Empty;
            ApplyValidationVisualState();
        }

        /// <summary>
        /// Resets the row validation error message of the GridCell.
        /// </summary>
        internal void RemoveRowValidationError()
        {
            roweventErrorMessage = string.Empty;
            ApplyValidationVisualState();
        }

        internal void RemoveError()
        {
            attributeErrorMessage = string.Empty;
            bindingErrorMessage = string.Empty;
            ApplyValidationVisualState();
        }

        internal void RemoveAll()
        {
            attributeErrorMessage = string.Empty;
            bindingErrorMessage = string.Empty;
            celleventErrorMessage = string.Empty;
            roweventErrorMessage = string.Empty;
            ApplyValidationVisualState();
        }


        #endregion

        #region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridCell"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
                this.ColumnBase = null;
            this.GridCellRegion = "NormalCell";
#if WPF
            BindingOperations.ClearBinding(this, CurrentCellBorderVisibilityProperty);
            BindingOperations.ClearBinding(this, SelectionBorderVisibilityProperty);
            BindingOperations.ClearBinding(this, ContextMenuProperty);
            this.RemoveHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnErrorHandled));
            UnwireEvent();
#endif
            isdisposed = true;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridCell"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            //While the OnCreateAutomationPeer is created from the any client based accessibity tool we have to return
            //our inbuild Automation peer else return the base class
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new SfGridCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
    }


    [ClassReference(IsReviewed = false)]
    public class GridGroupSummaryCell : GridCell
    {

        #region Ctor

        public GridGroupSummaryCell()
        {
            this.DefaultStyleKey = typeof(GridGroupSummaryCell);
            this.IsTabStop = false;
            IsEditable = false;
        }

        #endregion

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            this.ApplyGridCellVisualStates(this.GridCellRegion, false);
        }

#if UWP
        protected override void OnPointerPressed(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerPressed(e);
        }
#endif


        #endregion

#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridSummaryCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif

    }


    [ClassReference(IsReviewed = false)]
    public class GridCaptionSummaryCell : GridCell
    {
        #region Ctor

        public GridCaptionSummaryCell()
        {
            this.DefaultStyleKey = typeof(GridCaptionSummaryCell);
            this.IsTabStop = false;
            IsEditable = false;
        }

        #endregion

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            this.ApplyGridCellVisualStates(this.GridCellRegion, false);
        }

#if UWP
        protected override void OnPointerPressed(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerPressed(e);
        }
#endif
        #endregion
#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridSummaryCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif

    }

    [ClassReference(IsReviewed = false)]
    public class GridIndentCell : GridCell
    {
        #region Dependency Region

        public IndentColumnType ColumnType
        {
            get { return (IndentColumnType)GetValue(ColumnTypeProperty); }
            set { SetValue(ColumnTypeProperty, value); }
        }

        public static readonly DependencyProperty ColumnTypeProperty =
            DependencyProperty.Register("ColumnType", typeof(IndentColumnType), typeof(GridIndentCell), new PropertyMetadata(IndentColumnType.InDataRow, OnColumnTypeChanged));

        #endregion

        #region Dependency CallBack

        private static void OnColumnTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var indentCell = obj as GridIndentCell;
            if (indentCell != null) indentCell.ApplyIndentVisualState((IndentColumnType)args.NewValue);
        }

        #endregion

        #region Ctor

        public GridIndentCell()
        {
            this.DefaultStyleKey = typeof(GridIndentCell);
            this.IsTabStop = false;
        }

        #endregion

        #region Overrides

#if UWP
        protected override void OnPointerReleased(MouseButtonEventArgs e)
        {
            if (CheckToHandleCell(e))
                e.Handled = true;
            else
                base.OnPointerReleased(e);
        }

        protected override void OnPointerPressed(MouseButtonEventArgs e)
        {
            if (CheckToHandleCell(e))
                e.Handled = true;
            else if (ColumnBase != null)
                this.ColumnBase.RaisePointerPressed(e);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
#else
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (CheckToHandleCell(e))
                e.Handled = true;
            else
                base.OnPreviewMouseUp(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (CheckToHandleCell(e))
                e.Handled = true;
            else
                base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

#endif

#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridIndentCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
        protected internal override bool CanSelectCurrentCell()
        {
            return false;
        }


#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            this.ApplyIndentVisualState(this.ColumnType);
        }

        #endregion

        #region Private Methods

        internal void ApplyIndentVisualState(IndentColumnType columnType)
        {
            switch (columnType)
            {
                case IndentColumnType.AfterExpander:
                    VisualStateManager.GoToState(this, "After_Expander", true);
                    break;
                case IndentColumnType.BeforeExpander:
                    VisualStateManager.GoToState(this, "Before_Expander", true);
                    break;
                case IndentColumnType.InExpanderCollapsed:
                    VisualStateManager.GoToState(this, "Expander_Collapsed", true);
                    break;
                case IndentColumnType.InExpanderExpanded:
                    VisualStateManager.GoToState(this, "Expander_Expanded", true);
                    break;
                case IndentColumnType.InLastGroupRow:
                    VisualStateManager.GoToState(this, "Last_GroupRow", true);
                    break;
                case IndentColumnType.InSummaryRow:
                    VisualStateManager.GoToState(this, "SummaryRow", true);
                    break;
                case IndentColumnType.InTableSummaryRow:
                    VisualStateManager.GoToState(this, "TableSummaryRow", true);
                    break;
                case IndentColumnType.InDataRow:
                    VisualStateManager.GoToState(this, "DataRow", true);
                    break;
                case IndentColumnType.InUnBoundRow:
                    VisualStateManager.GoToState(this, "UnBoundRow", true);
                    break;
                case IndentColumnType.InFilterRow:
                    VisualStateManager.GoToState(this, "FilterRow", true);
                    break;
                case IndentColumnType.InAddNewRow:
                    var status = VisualStateManager.GoToState(this, "AddNewRow", true);
                    break;
            }
        }

        private bool CheckToHandleCell(MouseButtonEventArgs e)
        {
            return this.ColumnType == IndentColumnType.InTableSummaryRow;
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class GridHeaderIndentCell : GridIndentCell
    {
        #region Ctor

        public GridHeaderIndentCell()
        {
            this.DefaultStyleKey = typeof(GridHeaderIndentCell);
            this.IsTabStop = false;
        }

        #endregion
#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridIndentCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
    }

    [ClassReference(IsReviewed = false)]
    public class GridTableSummaryCell : GridCell
    {
        #region Ctor

        public GridTableSummaryCell()
        {
            this.DefaultStyleKey = typeof(GridTableSummaryCell);
            this.IsTabStop = false;
            IsEditable = false;
        }

        #endregion

        #region Overrides

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            this.ApplyGridCellVisualStates(this.GridCellRegion, false);
        }

#if UWP
        protected override void OnPointerPressed(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerReleased(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
#else
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Released)
                e.Handled = true;
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Pressed)
                e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.RightButton != MouseButtonState.Released)
                e.Handled = true;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
#endif

        protected internal override bool CanSelectCurrentCell()
        {
            return false;
        }

        #endregion

#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridSummaryCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif

    }

    public class GridUnBoundRowCell : GridCell
    {
        #region Ctr
        public GridUnBoundRowCell()
        {
            base.DefaultStyleKey = typeof(GridUnBoundRowCell);
        }
        #endregion

        #region overrides
#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            this.ApplyGridCellVisualStates(this.GridCellRegion, false);
        }
#if WPF
        #region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridUnboundRowCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
        #endregion
    }
}
