#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
#if UWP
using Syncfusion.UI.Xaml.ScrollAxis;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Grid.Helpers;
#else
using System.Windows;
using System.Windows.Controls;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows.Data;
using System.Windows.Input;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using EventArgs = PointerRoutedEventArgs;
    using Helpers;
#endif

    /// <summary>
    /// TreeGridVirtualizingCellRenderer is an abstract base class for cell renderers
    /// that need live UIElement visuals displayed in a cell. You can derive from
    /// this class and provide the type of the UIElement you want to show inside cells
    /// as type parameter. The class provides strong typed virtual methods for 
    /// initializing content of the cell and arranging the cell visuals. See 
    /// <see>
    ///     <cref>TreeGridVirtualizingCellRendererBase{T}</cref>
    /// </see>
    ///     for more details.
    /// <para/>
    /// The idea behind this class is to provide a place where we can 
    /// add general code that should be shared for all cell renderers in the tree derived
    /// from TreeGridVirtualizingCellRendererBase. While this class does at
    /// the moment not add meaningful functionality to TreeGridVirtualizingCellRendererBase
    /// we created this extra layer of inheritance to make it easy to share 
    /// code for the TreeGridVirtualizingCellRendererBase base class between grid and
    /// common assemblies and keep grid control specific code
    /// out of the base class. It is currently not possible with C# to the base class as 
    /// template type parameter.
    /// </summary>    
    public abstract class TreeGridVirtualizingCellRenderer<D, E> : TreeGridVirtualizingCellRendererBase<D, E>
        where D : FrameworkElement, new()
        where E : FrameworkElement, new()
    {
        #region Property
        public Type EditorType { get; set; }
        #endregion

        #region Ctor

        protected TreeGridVirtualizingCellRenderer()
        {
            this.EditorType = typeof(E);
        }

        #endregion

        #region Private Methods
#if UWP
        internal void ProcessPreviewTextInput(KeyEventArgs e)
        {
            if ((!char.IsLetterOrDigit(e.Key.ToString(), 0) || !TreeGrid.AllowEditing || TreeGrid.NavigationMode != NavigationMode.Cell) || SelectionHelper.CheckControlKeyPressed() || (!(e.Key >= Key.A && e.Key <= Key.Z) && !(e.Key >= Key.Number0 && e.Key <= Key.Number9) && !(e.Key >= Key.NumberPad0 && e.Key <= Key.NumberPad9)))
                return;
            if (TreeGrid.SelectionController.CurrentCellManager.BeginEdit())
                PreviewTextInput(e);
        }
#endif
        #endregion
        #region Protected Methods

        /// <summary>
        /// Texts the alignment to horizontal alignment.
        /// </summary>
        /// <param name="textAlignment">The text alignment.</param>
        /// <returns></returns>
        protected HorizontalAlignment TextAlignmentToHorizontalAlignment(TextAlignment textAlignment)
        {
            HorizontalAlignment horizontalAlignment;

            switch (textAlignment)
            {
                case TextAlignment.Right:
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;

                case TextAlignment.Center:
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;

                case TextAlignment.Justify:
                    horizontalAlignment = HorizontalAlignment.Stretch;
                    break;
                default:
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
            return horizontalAlignment;
        }
        #endregion

        #region Virtual Methods
        protected virtual void CurrentRendererValueChanged()
        {
            var column = (TreeGrid.Columns[TreeGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex)]);
            TreeGrid.RaiseCurrentCellValueChangedEvent(new TreeGridCurrentCellValueChangedEventArgs(TreeGrid) { RowColumnIndex = CurrentCellIndex, Record = CurrentCellRendererElement.DataContext, Column = column });
            if (column.UpdateTrigger != UpdateSourceTrigger.PropertyChanged || BindingExpression == null)
                return;
            if (BindingExpression.DataItem == null)
                return;
            object oldValue = null;
            if (TreeGrid.View != null)
                oldValue = TreeGrid.View.GetPropertyAccessProvider()
                             .GetValue(BindingExpression.DataItem, BindingExpression.ParentBinding.Path.Path);


            BindingExpression.UpdateSource();
            object Text = null;
            if (TreeGrid.View != null)
                Text = TreeGrid.View.GetPropertyAccessProvider()
                          .GetValue(BindingExpression.DataItem, BindingExpression.ParentBinding.Path.Path);

            object newValue;
            string errorMessage;
            var treeNode = TreeGrid.View.Nodes.GetNode(BindingExpression.DataItem);
            if (!TreeGrid.ValidationHelper.RaiseCurrentCellValidatingEvent(oldValue, Text, column, out newValue, CurrentCellIndex, CurrentCellElement, out errorMessage, BindingExpression.DataItem, treeNode))
                return;
            if (!ReferenceEquals(newValue, Text))
                SetControlValue(newValue);

            if (TreeGrid.GridValidationMode != GridValidationMode.None)
                TreeGrid.ValidationHelper.ValidateColumn(BindingExpression.DataItem, BindingExpression.ParentBinding.Path.Path, (TreeGridCell)CurrentCellElement, CurrentCellIndex);

            TreeGrid.ValidationHelper.RaiseCurrentCellValidatedEvent(oldValue, Text, column, errorMessage, BindingExpression.DataItem, treeNode);
        }
        #endregion


        #region Override Methods
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Up:
                case Key.Down:
                case Key.F2:
                case Key.Escape:
                case Key.PageDown:
                case Key.PageUp:

                case Key.Delete:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.Tab:
                //While pressing the Space key the WholeRow have to be select when the CurrentCell is in the DataRow.
                //Hence the Space key operation is missing in this Class.
                case Key.Space:
                    {
                        //This code is removed for selection issue, when pressing any Down arrow key in Child gird, e.Handled is set to true
                        //hence the Down operation is continued in parent grid. So that, the below code is removed.
                        //e.Handled = true;
                        return true;
                    }
                case Key.C:
                case Key.V:
                case Key.X:
                case Key.A:
                    return !IsInEditing;
            }
            return false;
        }

        /// <summary>
        /// Initialize the binding for TreeGridCell by Columns's CellTemplate and CellTemplateSelector.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain TreeGridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeTemplateElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;
            if (column.hasCellTemplate)
                uiElement.ContentTemplate = column.CellTemplate;
            else if (column.hasCellTemplateSelector)
                uiElement.ContentTemplateSelector = column.CellTemplateSelector;
            OnUpdateTemplateBinding(dataColumn, uiElement, dataContext);
        }

        /// <summary>
        ///  Updates the binding for the TreeGridCell by Column's CellTemplate and CellTemplateSelector.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain TreeGridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnUpdateTemplateBinding(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;

            if (column.SetCellBoundValue)
            {
                var dataContextHelper = new TreeGridDataContextHelper { Record = dataContext, DataRow = dataColumn.DataRow };
                dataContextHelper.SetValueBinding(column.DisplayBinding, dataContext);
                uiElement.Content = dataContextHelper;
            }
            else
                uiElement.SetBinding(ContentControl.ContentProperty, new Binding());
        }

        /// <summary>
        /// Initialize the binding for display element of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain TreeGridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, D uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;

            uiElement.SetBinding(TextBlock.TextProperty, column.DisplayBinding);

            uiElement.SetValue(TextBlock.TextAlignmentProperty, column.textAlignment);
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.verticalAlignment);

            var textColumnBase = column as TreeGridTextColumnBase;
            if (textColumnBase == null)
                return;

            uiElement.SetValue(TextBlock.TextTrimmingProperty, textColumnBase.textTrimming);
            uiElement.SetValue(TextBlock.TextWrappingProperty, textColumnBase.textWrapping);
#if WPF
            uiElement.SetValue(TextBlock.TextDecorationsProperty, textColumnBase.textDecoration);            
#endif
        }

        /// <summary>
        /// Updates the binding for display element of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain TreeGridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the data context of the particular row</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnUpdateDisplayBinding(TreeDataColumnBase dataColumn, D uiElement, object dataContext)
        {
            OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
        }


        /// <summary>
        /// Initialize the binding for editor control of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain TreeGridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the data context of the particular row</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, E uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;
            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.OneWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            var textAlignBind = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextAlignmentProperty, textAlignBind);
            var textWrappingBinding = new Binding { Path = new PropertyPath("TextWrapping"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextWrappingProperty, textWrappingBinding);
            var verticalContentAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalContentAlignment);

#if WPF
            var textDecorations = new Binding { Path = new PropertyPath("TextDecorations"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextDecorationsProperty, textDecorations);
#endif
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;

        }

        /// <summary>
        /// Updates the binding for editor control of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain TreeGridColumn, RowColumnIndex</param>
        /// <param name="element">Specifies the data context of the particular row</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnUpdateEditBinding(TreeDataColumnBase dataColumn, E element, object dataContext)
        {
            OnInitializeEditElement(dataColumn, element, dataContext);
        }
        #endregion
    }

}
