#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using Windows.Devices.Input;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.Grid.Helpers;

#endif
namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using Windows.UI.Xaml.Data;
#endif
    public class TreeGridCell : TreeGridElement, IDisposable
    {
        internal string eventErrorMessage;
        internal string attributeErrorMessage;
        internal string bindingErrorMessage;
        public TreeGridCell()
        {
            this.DefaultStyleKey = typeof(TreeGridCell);
            this.AllowDrop = true;
#if UWP
            this.CanDrag = true;
            this.DragStarting += TreeGridCell_DragStarting;
            this.DragOver += TreeGridCell_DragOver;
            this.DragLeave += TreeGridCell_DragLeave;
            this.Drop += TreeGridCell_Drop;
            this.DropCompleted += TreeGridCell_DropCompleted;
#endif
#if WPF
            this.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnErrorHandled), true);
#endif
            //this.IsTabStop = false;
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
            // WPF-33699 - To check validation when template is loaded in TreeGridCell.
            else
            {
                if (!GridHelper.GetHasError(this))
                    this.bindingErrorMessage = string.Empty;
            }
            // WPF-33863 - After deleting the cell value (if type does not support nullable value), this method will be called with ValidationStep as  ConvertedProposedValue. Since this is not updated value, error message should be removed.
            if (e.Error.RuleInError != null && e.Error.RuleInError.ValidationStep == ValidationStep.ConvertedProposedValue)
                this.bindingErrorMessage = string.Empty;
            this.ApplyValidationVisualState();
        }
#endif

#if UWP
        /// <summary>
        /// Occurs when a drag operation is initiated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TreeGridCell_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            if (this.ColumnBase == null || !this.ColumnBase.TreeGrid.AllowDraggingRows)
                args.Cancel = true;
            else
                this.ColumnBase.TreeGrid.RowDragDropController.ProcessOnDragStarting(args, new ScrollAxis.RowColumnIndex(this.ColumnBase.RowIndex, this.ColumnBase.ColumnIndex));
        }

        /// <summary>
        /// Occurs when the input system reports an underlying drag event with this element as the potential drop target.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeGridCell_DragOver(object sender, DragEventArgs e)
        {
            if (this.ColumnBase == null || !this.ColumnBase.TreeGrid.AllowDraggingRows)
                return;
            this.ColumnBase.TreeGrid.RowDragDropController.ProcessOnDragOver(e, new ScrollAxis.RowColumnIndex(this.ColumnBase.RowIndex, this.ColumnBase.ColumnIndex));
        }
        /// <summary>
        /// Occurs when the input system reports an underlying drag event with this element as the origin.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeGridCell_DragLeave(object sender, DragEventArgs e)
        {
            if (this.ColumnBase == null || !this.ColumnBase.TreeGrid.AllowDraggingRows)
                return;
            this.ColumnBase.TreeGrid.RowDragDropController.ProcessOnDragLeave(e, new ScrollAxis.RowColumnIndex(this.ColumnBase.RowIndex, this.ColumnBase.ColumnIndex));
        }
        /// <summary>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>         
        private void TreeGridCell_Drop(object sender, DragEventArgs e)
        {
            if (this.ColumnBase == null || !this.ColumnBase.TreeGrid.AllowDraggingRows)
                return;
            this.ColumnBase.TreeGrid.RowDragDropController.ProcessOnDrop(e, new ScrollAxis.RowColumnIndex(this.ColumnBase.RowIndex, this.ColumnBase.ColumnIndex));
        }
        /// <summary>
        /// Occurs when a drag-and-drop operation is ended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TreeGridCell_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            if (this.ColumnBase == null || !this.ColumnBase.TreeGrid.AllowDraggingRows)
                return;
            this.ColumnBase.TreeGrid.RowDragDropController.ProcessOnDropCompleted(args);
        }

#endif

        /// <summary>
        /// Opens the context menu at the specified position.
        /// </summary>
        /// <param name="position">The position to display context menu.</param>
        /// <b>true</b> If the context menu opened;Otherwise<b>false</b>
        /// </returns>
#if UWP
        protected virtual internal bool ShowContextMenu(Point position)
#else
        protected virtual internal bool ShowContextMenu()
#endif
        {
            if (ColumnBase == null || ColumnBase.TreeGrid == null)
                return false;

            var treeGrid = ColumnBase.TreeGrid;
            TreeGridContextMenuEventArgs args = null;
            var menuinfo = new TreeGridNodeContextMenuInfo() { TreeNode = ColumnBase.DataRow.Node, TreeGrid = treeGrid };
#if WPF
            ContextMenu contextMenu = null;
#else
            MenuFlyout contextMenu = null;
#endif
            if (this is TreeGridExpanderCell)
            {
                if (treeGrid.ExpanderContextMenu != null)
                {
                    contextMenu = treeGrid.ExpanderContextMenu;
                    args = new TreeGridContextMenuEventArgs(contextMenu, menuinfo, new RowColumnIndex(ColumnBase.RowIndex,
                        this.ColumnBase.ColumnIndex), ContextMenuType.ExpanderCell, this);
                }
            }
            else if (this is TreeGridCell)
            {
                if (treeGrid.RecordContextMenu != null)
                {
                    contextMenu = treeGrid.RecordContextMenu;
                    args = new TreeGridContextMenuEventArgs(contextMenu, menuinfo, new RowColumnIndex(ColumnBase.RowIndex,
                        this.ColumnBase.ColumnIndex), ContextMenuType.RecordCell, this);
                }
            }
            if (args != null && !treeGrid.RaiseTreeGridContextMenuEvent(args))
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

#if WPF
        private bool isPreviewMouseDown;
#endif
        public TreeDataColumnBase ColumnBase { get; internal set; }

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            UpdateCurrentState(false);
            this.ApplyValidationVisualState(false);
#if WPF
            UnwireEvent();
            WireEvent();
#endif
        }
#if WPF
        /// <summary>
        /// Wire TreeGrid cell events
        /// </summary>
        internal void WireEvent()
        {
            this.ContextMenuOpening += OnContextMenuOpening;
        }

        /// <summary>
        /// Unwire TreeGrid cell events
        /// </summary>
        internal void UnwireEvent()
        {
            this.ContextMenuOpening -= OnContextMenuOpening;
        }

        /// <summary>
        /// Occurs when any context menu on the element is opened.
        /// </summary>
        /// <param name="sender">The sender which contains SfTreeGrid</param>
        /// <param name="e">Context menu event arguments</param>
        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ShowContextMenu())
                e.Handled = true;
        }
#endif
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
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(TreeGridCell), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets a value indicating whether TreeGridCell has error or not.
        /// </summary>
        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(eventErrorMessage) || !string.IsNullOrEmpty(attributeErrorMessage) || !string.IsNullOrEmpty(bindingErrorMessage);
            }
        }

        /// <summary>
        /// Updates the Visual State of the TreeGridCell based on the cell validation applied on SfTreeGrid.
        /// </summary>
        protected internal virtual void ApplyValidationVisualState(bool canApplyDefaultState = true)
        {
            ErrorMessage = string.Empty;
            if (this.HasError)
            {
                if (!string.IsNullOrEmpty(eventErrorMessage))
                    ErrorMessage = eventErrorMessage;
                if (!string.IsNullOrEmpty(attributeErrorMessage))
                    ErrorMessage = !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage + string.Format("\n") + attributeErrorMessage : attributeErrorMessage;
                if (!string.IsNullOrEmpty(bindingErrorMessage))
                    ErrorMessage = !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage + string.Format("\n") + bindingErrorMessage : bindingErrorMessage;

                VisualStateManager.GoToState(this, "HasError", true);
            }
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "NoError", true);
        }

        internal void SetError(string errorMessage, bool isAttributeError)
        {
            if (isAttributeError)
                attributeErrorMessage = errorMessage;
            else
                eventErrorMessage = errorMessage;

            ApplyValidationVisualState();
        }

        internal void RemoveError(bool isAttributeError)
        {
            if (isAttributeError)
                attributeErrorMessage = string.Empty;
            else
                eventErrorMessage = string.Empty;
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
            eventErrorMessage = string.Empty;
            ApplyValidationVisualState();
        }

#if UWP       
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (ColumnBase != null)
            {
                this.ColumnBase.RaisePointerPressed(e);
                //WRT-4919 - We have set the OSK(On Screen Keyboard) as a Numeric for EditElement in the Renderer,
                //OSK is changed from  Numeric to AlphaNumeric while scrolling or click on to other cell, 
                //because focus is changed from EditElement to TreeGridCell, So focus is didn't set to TreeGridCell 
                //when the cell is in edit mode.
                if (ColumnBase.Renderer != null && ColumnBase.Renderer.TreeGrid != null
                    && (!ColumnBase.Renderer.TreeGrid.SelectionController.CurrentCellManager.HasCurrentCell || !ColumnBase.Renderer.TreeGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing))
                    this.Focus(FocusState.Programmatic);
                if (!ColumnBase.TreeGrid.AllowDraggingRows)
                    e.Handled = true;
            }
            base.OnPointerPressed(e);
        }
        /// <summary>
        /// Method override to set tooltip for the TreeGridCell
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPointerEntered(MouseButtonEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerEntered();
            base.OnPointerEntered(e);
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

        /// <summary>
        /// When long press on SfTreeGrid Cell, Context menu appears for the selected cell. 
        /// We are using this event for context menu support in Record cell and Expander cell.
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
        /// When Right click the SfDataGrid Cell, Context menu appears for the selected cell. 
        /// We are using this event for context menu support in Record cell and Expander cell.
        /// </summary>
        /// <param name="e">Right tapped event arguments</param>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            if (ShowContextMenu(position))
                e.Handled = true;
            base.OnRightTapped(e);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.OnTapped(e);
            base.OnTapped(e);
        }
#else
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
        /// <summary>
        ///  Method override to set tooltip for TreeGridCell.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (ColumnBase != null)
                this.ColumnBase.RaisePointerEntered();
            base.OnMouseEnter(e);
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            isPreviewMouseDown = true;
            ProcessMouseDown(e);
            base.OnPreviewMouseDown(e);
        }

        private void ProcessMouseDown(MouseButtonEventArgs e)
        {
            if (VisualContainer.GetWantsMouseInput(e.OriginalSource as DependencyObject, this) == false)
            {
                if (!this.IsKeyboardFocusWithin)
                {
                    this.Focus();
                    e.Handled = !TreeGridValidationHelper.IsCurrentCellValidated;
                }
            }

            if (ColumnBase != null)
                ColumnBase.RaisePointerPressed(e);
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
#endif

        internal bool IsCurrentCell
        {
            get { return (bool)GetValue(IsCurrentCellProperty); }
            set { SetValue(IsCurrentCellProperty, value); }
        }

        /// <summary>
        /// Dependency registration for CurrentCellBorderVisiblity.
        /// </summary>
        /// <remarks></remarks>
        internal static readonly DependencyProperty IsCurrentCellProperty =
            DependencyProperty.Register("IsCurrentCell", typeof(bool), typeof(TreeGridCell), new PropertyMetadata(false,
                (obj, args) =>
                {
                    var cell = obj as TreeGridCell;
                    if (cell == null)
                        return;
                    cell.UpdateCurrentState();
                }));

        private void UpdateCurrentState(bool canApplyDefaultState = true)
        {
            if (this.IsCurrentCell)
                VisualStateManager.GoToState(this, "Current", false);
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "Regular", false);
        }

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
            DependencyProperty.Register("CurrentCellBorderBrush", typeof(Brush), typeof(TreeGridCell), new PropertyMetadata(new SolidColorBrush(Colors.Black)));


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
            DependencyProperty.Register("CurrentCellBorderThickness", typeof(Thickness), typeof(TreeGridCell), new PropertyMetadata(new Thickness(2)));


        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCell"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCell"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ColumnBase = null;
#if WPF
                this.RemoveHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnErrorHandled));
                UnwireEvent();
#else
                this.DragStarting -= TreeGridCell_DragStarting;
                this.DragOver -= TreeGridCell_DragOver;
                this.DragLeave -= TreeGridCell_DragLeave;
                this.Drop -= TreeGridCell_Drop;
                this.DropCompleted -= TreeGridCell_DropCompleted;
#endif
            }
        }
    }
}