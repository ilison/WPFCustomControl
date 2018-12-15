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
using System.Windows;
using System.Windows.Input;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if WPF
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
#else
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Input;
using Windows.UI.Xaml.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides the functionality for the column chooser operation in SfDataGrid.
    /// </summary>
    /// <remarks></remarks>
    public interface IColumnChooser
    {
        /// <summary>
        /// Adds the specified column to the children of column chooser window.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to add children for the column chooser window.
        /// </param>        
        void AddChild(GridColumn column);

        /// <summary>
        /// Removes the specified child column from the column chooser window.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to remove child from the column chooser window.
        /// </param>        
        void RemoveChild(GridColumn column);

        /// <summary>
        /// Gets the display rectangle for column chooser window.
        /// </summary>
        /// <returns>
        /// The display rectangle for column chooser window.
        /// </returns>        
        Rect GetControlRect();
    }

#if !WPF
    public class ColumnChooser : Control, IColumnChooser
    {
        #region Public Field
        /// <summary>
        /// Gets or sets ColumnChooser Popup window.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Popup Popup { get; set; }
        #endregion

        /// <summary>
        /// Gets DataGrid for ColumnChooser.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        protected SfDataGrid DataGrid { get; private set; }

        #region Fields
        Border closeButtonBorder;
        Point pointerPressedPoint;
        bool isPointerPressed;
        StackPanel chooserPanel;
        List<GridColumn> intialChildren = new List<GridColumn>();
        #endregion
        
        #region Constructor
        public ColumnChooser(SfDataGrid dataGrid)
        {
            this.DefaultStyleKey = typeof(ColumnChooser);
            this.Popup = new Popup();
            Popup.Child = this;
            this.DataGrid = dataGrid;
        }
        #endregion

        #region Dependency properties

        /// <summary>
        /// Gets or sets Title for ColumnChooser Popup Window.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ColumnChooser), new PropertyMetadata(GridResourceWrapper.ColumnChooserTitle));

        /// <summary>
        /// Gets or sets WaterMarkText for Empty Column Chooser.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string WaterMarkText
        {
            get { return (string)GetValue(WaterMarkTextProperty); }
            set { SetValue(WaterMarkTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WaterMarkText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WaterMarkTextProperty =
            DependencyProperty.Register("WaterMarkText", typeof(string), typeof(ColumnChooser), new PropertyMetadata(GridResourceWrapper.ColumnChooserWaterMark));
        
        /// <summary>
        /// Gets or sets Visibility of WaterMarkText for Empty Column Chooser.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public Visibility WaterMarkTextVisibility
        {
            get { return (Visibility)GetValue(WaterMarkTextVisibilityProperty); }
            set { SetValue(WaterMarkTextVisibilityProperty, value); }
        }

        public static readonly DependencyProperty WaterMarkTextVisibilityProperty =
            DependencyProperty.Register("WaterMarkTextVisibility", typeof(Visibility), typeof(ColumnChooser), new PropertyMetadata(Visibility.Visible));



        /// <summary>
        /// Gets or sets TitleBar background.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public SolidColorBrush TitleBarBackground
        {
            get { return (SolidColorBrush)GetValue(TitleBarBackgroundProperty); }
            set { SetValue(TitleBarBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarBackgroundProperty =
            DependencyProperty.Register("TitleBarBackground", typeof(SolidColorBrush), typeof(ColumnChooser), new PropertyMetadata(null));



        /// <summary>
        /// Gets or sets TitleBar Foreground.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public SolidColorBrush TitleBarForeground
        {
            get { return (SolidColorBrush)GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarForegroundProperty =
            DependencyProperty.Register("TitleBarForeground", typeof(SolidColorBrush), typeof(ColumnChooser), new PropertyMetadata(null));


        /// <summary>
        /// Gets or sets Text Alignment for the Title.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public TextAlignment TitleTextAlignment
        {
            get { return (TextAlignment)GetValue(TitleTextAlignmentProperty); }
            set { SetValue(TitleTextAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleTextAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleTextAlignmentProperty =
            DependencyProperty.Register("TitleTextAlignment", typeof(TextAlignment), typeof(ColumnChooser), new PropertyMetadata(null));

        

        #endregion

        #region Private methods

        private void OnCloseButtonPressed(object sender, PointerRoutedEventArgs e)
        {
            isPointerPressed = false;
            (this.Parent as Popup).IsOpen = false;
        }

        #endregion

        #region Overrides

        protected override void OnApplyTemplate()
        {
            //this.Title = GridResourceWrapper.ColumnChooserTitle;
            //this.WaterMarkText = GridResourceWrapper.ColumnChooserWaterMark;
            chooserPanel = this.GetTemplateChild("PART_ChooserPanel") as StackPanel;
            closeButtonBorder = this.GetTemplateChild("PART_CloseBorder") as Border;
            closeButtonBorder.PointerPressed += OnCloseButtonPressed;
            this.DataGrid.Columns.ForEach(col =>
            {
                if (col.IsHidden)
                    intialChildren.Add(col);
            });
            intialChildren.ForEach(child => AddChild(child));
            intialChildren.Clear();
            base.OnApplyTemplate();
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            PointerPoint pp = e.GetCurrentPoint(null);

            double deltaV = pp.Position.Y;
            double deltaH = pp.Position.X;
            if (isPointerPressed && pp.Properties.IsLeftButtonPressed && deltaH != pointerPressedPoint.X && deltaV != pointerPressedPoint.Y)
            {
                this.CapturePointer(e.Pointer);
                (this.Parent as Popup).HorizontalOffset = deltaH - pointerPressedPoint.X;
                (this.Parent as Popup).VerticalOffset = deltaV - pointerPressedPoint.Y;
                e.Handled = true;
            }

        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            pointerPressedPoint.X = -1;
            pointerPressedPoint.Y = -1;
            isPointerPressed = false;
            this.ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            isPointerPressed = true;
            pointerPressedPoint = e.GetCurrentPoint(this).Position;
            e.Handled = true;
        }

        #endregion

        #region Virtual methods
        /// <summary>
        /// Adds the Child for the column chooser whenever the column gets hide
        /// </summary>
        /// <param name="column"></param>
        /// <remarks></remarks>
        public virtual void AddChild(GridColumn column)
        {
            if (chooserPanel == null)
            {
                intialChildren.Add(column);
                return;
            }
            if (this.chooserPanel.Children.ToList<ColumnChooserItem>().All(item => (item as ColumnChooserItem).Column.MappingName != column.MappingName) && this.DataGrid.View != null)
            {
                var chooserItem = new ColumnChooserItem(column);
                chooserItem.Controller = this.DataGrid.GridColumnDragDropController;
                chooserItem.ColumnName = column.HeaderText;
                this.chooserPanel.Children.Add(chooserItem);
            }
            if (this.chooserPanel.Children.Count == 0)
                this.WaterMarkTextVisibility = Visibility.Visible;
            else
                this.WaterMarkTextVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Remove the Child for the column chooser whenever the column gets Unhidden
        /// </summary>
        /// <param name="column"></param>
        /// <remarks></remarks>
        public virtual void RemoveChild(GridColumn column)
        {
            if (this.chooserPanel != null && this.chooserPanel.Children.Count > 0)
            {
                var element = this.chooserPanel.Children.ToList<ColumnChooserItem>().FirstOrDefault(item => (item as ColumnChooserItem).Column.MappingName == column.MappingName);
                if (element != null)
                    this.chooserPanel.Children.Remove(element);
            }
            if (this.chooserPanel != null && this.chooserPanel.Children.Count == 0)
                this.WaterMarkTextVisibility = Visibility.Visible;
            else
                this.WaterMarkTextVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Returns the Rect of the ColumnChooserControl
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual Rect GetControlRect()
        {
            var rect = this.GetControlRect(this.DataGrid);
            if (this.DataGrid.FlowDirection == FlowDirection.RightToLeft)
                rect.X -= this.ActualWidth;
            return rect;
        }

        /// <summary>
        /// Shows the ColumnChooser Window
        /// </summary>
        /// <remarks></remarks>
        public void ShowColumnChooser()
        {
            this.Popup.IsOpen = true;
        }
        #endregion
    }
#endif


    /// <summary>
    /// Represents a control that specifies the item for the column chooser window.
    /// </summary>    
    public class ColumnChooserItem : Control
    {

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.ColumnChooserItem"/> class.
        /// </summary>
        /// <param name="column">
        /// The column.
        /// </param>
        public ColumnChooserItem(GridColumn column)
        {
            DefaultStyleKey = typeof(ColumnChooserItem);
            Column = column;
            ColumnName = column.HeaderText != null ? column.HeaderText : column.MappingName;
#if !WPF
            this.ManipulationMode = ManipulationModes.None;
#endif
        }
        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the column for the column chooser item.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> for the column chooser item.
        /// </value>        
        public GridColumn Column { get; set; }

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> which controls the column drag-and-drop operation in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        public GridColumnDragDropController Controller { get; set; }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// Gets or sets name of the column for the ColumnChooserItem.
        /// </summary>
        /// <value>
        /// A string that specifies the name of column. The default value is <c>string.Empty</c>.
        /// </value>        
        public string ColumnName
        {
            get { return (string)GetValue(ColumnNameProperty); }
            set { SetValue(ColumnNameProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.ColumnChooserItem.ColumnName dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.ColumnChooserItem.ColumnName dependency property.
        /// </remarks>    
        public static readonly DependencyProperty ColumnNameProperty =
            DependencyProperty.Register("ColumnName", typeof(string), typeof(ColumnChooserItem), new PropertyMetadata(string.Empty));
        #endregion

        #region Overrides
#if WPF
        /// <summary>
        /// Raises the <see cref="System.Windows.MouseLeave"/> event.
        /// </summary>
        /// <param name="e">
        /// Contains the event data.
        /// </param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            base.OnMouseLeave(e);
        }
#else
        protected override void OnPointerExited(PointerRoutedEventArgs e)
         {
            VisualStateManager.GoToState(this,"Normal",true);
 	        base.OnPointerExited(e);
          }
#endif

#if !WPF
        protected override void OnPointerMoved(PointerRoutedEventArgs e)
#else
        /// <summary>
        /// Raises the <see cref="System.Windows.MouseLeave"/> event.
        /// </summary>
        /// <param name="e">
        /// Contains the event data.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
#endif
        {
#if WPF
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                (this.Controller as GridColumnChooserController).Show((this.Controller as GridColumnChooserController).dataGrid.Columns.IndexOf(Column), e);
                VisualStateManager.GoToState(this, "Normal", true);
                return;
            }
            if (this.Controller.isDragState)
                VisualStateManager.GoToState(this, "Normal", true);
            else
                VisualStateManager.GoToState(this, "MouseOver", true);
            base.OnMouseMove(e);
#else
            if(e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                (this.Controller as GridColumnChooserController).Show((this.Controller as GridColumnChooserController).dataGrid.Columns.IndexOf(Column),e);
                VisualStateManager.GoToState(this, "Normal", true);
                return;
            }
            VisualStateManager.GoToState(this, "PointerOver", true);
            base.OnPointerMoved(e);
#endif
        }
        #endregion

    }
}
