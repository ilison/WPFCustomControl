#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Globalization;
using System.Windows;
#if WPF
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a control that print previewing the SfDataGrid control.
    /// </summary>
    public class GridPrintPreviewControl : Control, IDisposable
    {

        #region Fields

        private PrintManagerBase printManager;
        private bool isWired;
        private bool isdisposed = false;

        #endregion

        #region UIElements

        private PrintPreviewAreaControl PartPrintPreviewAreaControl;
        private TextBox Part_TextBox;
        private Slider PartZoomSlider;
        private Button PartZoomInButton;
        private Button PartZoomOutButton;

        #endregion

        #region Ctor

        internal GridPrintPreviewControl(SfDataGrid dataGrid, PrintManagerBase printManager)
        {
            DefaultStyleKey = typeof(GridPrintPreviewControl);
            this.printManager = printManager ?? new GridPrintManager(dataGrid);
            Loaded += OnGridPrintPreviewControlLoaded;
        }

        #endregion

        #region Internal Methods

        internal void Print()
        {
            printManager.Print();
        }

        #endregion

        #region Override
        /// <summary>
        /// Builds the visual tree for the print preview control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PartPrintPreviewAreaControl = GetTemplateChild("PART_PrintPreviewAreaControl") as PrintPreviewAreaControl;
            Part_TextBox = GetTemplateChild("PART_TextBox") as TextBox;
            PartZoomSlider = GetTemplateChild("PART_ZoomSlider") as Slider;
            PartZoomInButton = GetTemplateChild("PART_MinusZoomButton") as Button;
            PartZoomOutButton = GetTemplateChild("PART_PlusZoomButton") as Button;
            Part_TextBox.ToolTip = new ToolTip();
            WireEvents();
        }

        #endregion

        #region Wire/UnWire Events
        /// <summary>
        /// Wires the events associated with the print preview control.
        /// </summary>
        private void WireEvents()
        {

            if (PartPrintPreviewAreaControl != null)
                PartPrintPreviewAreaControl.PrintManagerBase = printManager;

            if (Part_TextBox != null)
            {
                Part_TextBox.LostFocus += OnPartTextBoxLostFocus;
                Part_TextBox.KeyDown += Part_TextBox_KeyDown; //WPF-24012
                Part_TextBox.MouseMove += Part_TextBox_MouseMove;
            }

            if(PartZoomInButton != null)
                PartZoomInButton.Click += OnZoomInClicked;

            if (PartZoomOutButton != null)
                PartZoomOutButton.Click += OnZoomOutClicked;

            //WPF-24012
            if(PartZoomSlider !=null)
                PartZoomSlider.ValueChanged += PartZoomSlider_ValueChanged;

            Unloaded += OnGridPrintPreviewControlUnloaded;

            isWired = true;
        }     

        /// <summary>
        /// set tool tip content when mouse hover the  textbox
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">MouseEventArgs</param>
        private void Part_TextBox_MouseMove(object sender, MouseEventArgs e)
        {
            var textBox = (sender as TextBox);
            //WPF-24012 below code is to show tool tip when mouse hover the  text box
            if (textBox.IsMouseOver && (textBox.ToolTip as ToolTip).IsOpen && !textBox.IsFocused)
            {
                (Part_TextBox.ToolTip as ToolTip).Content = "Current Page";
            }
        }

        /// <summary>
        /// clear invalidate border and close tool tip while  zoom value changed.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">RoutedPropertyChangedEventArgs<double></param>
        private void PartZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ClearInvalidBorderandToolTip(Part_TextBox);
        }

        /// <summary>
        /// Validate and set tool tip if the value is invalid / incorrect/ out of range
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">System.Windows.Input.KeyEventArgs</param>
        private void Part_TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int pageIndex;
            var textBox = (sender as TextBox);
            var toolTip = (textBox.ToolTip as ToolTip);
            // below code is to handle enter key pressed in text box
            if (e.Key == Key.Return)
            {
                if (int.TryParse(textBox.Text, out pageIndex) && pageIndex > 0 && pageIndex <= printManager.pageCount)
                {
                    PartPrintPreviewAreaControl.PageIndex = pageIndex;
                    ClearInvalidBorderandToolTip(textBox);
                }
                else if (textBox.Text.Equals("") || textBox.Text == "0")
                    textBox.Text = Convert.ToString(PartPrintPreviewAreaControl.PageIndex); 
            }
            else if(e.Key==Key.OemMinus)
            {
                toolTip.Content = "Entry must be positive value ";
                ShowInvalidBorderandToolTip(textBox);
                e.Handled = true;
            }
            //this code is to handle whether the input is valid or not
            else if((e.Key >= Key.D0) && (e.Key <= Key.D9))
            {
                var str = String.IsNullOrEmpty((textBox.SelectedText)) ? textBox.Text : (textBox.Text).Replace(textBox.SelectedText, "");
                var isParse = int.TryParse(str.Trim() + Convert.ToString(e.Key - Key.D0), out pageIndex);
                if (isParse)
                {                    
                    if (pageIndex < 1 || pageIndex > printManager.pageCount)
                    {
                        if (pageIndex > printManager.pageCount)
                            toolTip.Content = "Entry must be less than or equal to " + printManager.pageCount;
                        else if (pageIndex < 1)
                            toolTip.Content = "Entry must be greater than or equal to 1";

                        ShowInvalidBorderandToolTip(textBox);                       
                        e.Handled = true;
                    }
                    else
                    {
                        pageIndex = (Convert.ToInt32((str.Trim() + Convert.ToString(e.Key - Key.D0))));
                        if (pageIndex > 0 && pageIndex <= printManager.pageCount)
                            ClearInvalidBorderandToolTip(textBox);
                    }
                }                                
            }
            else if(!IsNumberOrControlKey(e.Key))
            {
                toolTip.Content = "Entry must be a Number";
                textBox.Text = Convert.ToString(PartPrintPreviewAreaControl.PageIndex);
                ShowInvalidBorderandToolTip(textBox);             
                e.Handled = true;
            }
        }

        /// <summary>
        /// find whether the key entry of textbox is number / control keys or not.
        /// </summary>
        /// <param name="inKey">Key</param>
        /// <returns>bool</returns>
        private bool IsNumberOrControlKey(Key inKey)
        {
            if (inKey == Key.Delete || inKey == Key.Back || inKey == Key.Tab || inKey == Key.Return || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt) || Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                return true;

            if (inKey < Key.D0 || inKey > Key.D9)
            {
                if (inKey < Key.NumPad0 || inKey > Key.NumPad9)
                {
                    return false;
                }
            }               
            return true;
        }

        /// <summary>
        /// Show invalidate border to textbox and open the tool tip
        /// </summary>
        /// <param name="textBox">TextBox</param>
        private void ShowInvalidBorderandToolTip(TextBox textBox)
        {
            //to set Border brush as Red if page index is out of range or invalid
            textBox.BorderBrush = Brushes.Red;
            (textBox.ToolTip as ToolTip).PlacementRectangle = textBox.GetRectFromCharacterIndex(textBox.Text.Length);
            if (!(textBox.ToolTip as ToolTip).IsOpen)
                (textBox.ToolTip as ToolTip).IsOpen = true;
        }

        /// <summary>
        /// Clear invalidate border and tool tip 
        /// </summary>
        /// <param name="textBox"></param>
        private void ClearInvalidBorderandToolTip(TextBox textBox)
        {
            if ((textBox.ToolTip as ToolTip).IsOpen)
                (textBox.ToolTip as ToolTip).IsOpen = false;

            //to clear the Red border if page index is with in the range
            if (textBox.BorderBrush == Brushes.Red)
                textBox.ClearValue(Border.BorderBrushProperty);
        }

        private void UnWireEvents()
        {
            if (Part_TextBox != null)
            {
                Part_TextBox.LostFocus -= OnPartTextBoxLostFocus;
                Part_TextBox.KeyDown -= Part_TextBox_KeyDown; //WPF-24012
                Part_TextBox.MouseMove -= Part_TextBox_MouseMove;
            }

            if (PartZoomInButton != null)
                PartZoomInButton.Click -= OnZoomInClicked;

            if (PartZoomOutButton != null)
                PartZoomOutButton.Click -= OnZoomOutClicked;

            //WPF-24012
            if (PartZoomSlider != null)
                PartZoomSlider.ValueChanged -= PartZoomSlider_ValueChanged;

            isWired = false;
        }

        #endregion

        #region Events

        void OnGridPrintPreviewControlLoaded(object sender, RoutedEventArgs e)
        {
            if(!isWired)
                WireEvents();
        }

        void OnGridPrintPreviewControlUnloaded(object sender, RoutedEventArgs e)
        {
            if (isWired)
                UnWireEvents();
            Unloaded -= OnGridPrintPreviewControlUnloaded;
        }

        private void OnPartTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            int pageIndex;
            if (int.TryParse(tb.Text, out pageIndex) && pageIndex > 0 && pageIndex <= printManager.pageCount)
            {
                PartPrintPreviewAreaControl.PageIndex = pageIndex;
                ClearInvalidBorderandToolTip(tb);
            }
            else
                tb.ClearValue(TextBox.TextProperty);
        }

        private void OnZoomInClicked(object obj, RoutedEventArgs routedEventArgs)
        {
            if (PartZoomSlider.Value - 10 < PartZoomSlider.Minimum)
                PartZoomSlider.Value = PartZoomSlider.Minimum;
            else
                PartZoomSlider.Value -= 10;
        }

        private void OnZoomOutClicked(object obj, RoutedEventArgs routedEventArgs)
        {
            if (PartZoomSlider.Value + 10 > PartZoomSlider.MaxHeight)
                PartZoomSlider.Value = PartZoomSlider.MaxHeight;
            else
                PartZoomSlider.Value += 10;
        }

        #endregion

        #region Dispose
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridPrintPreviewControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridPrintPreviewControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                printManager = null;
                PartPrintPreviewAreaControl = null;
            }
            isdisposed = true;
        }

        #endregion
    }

}
