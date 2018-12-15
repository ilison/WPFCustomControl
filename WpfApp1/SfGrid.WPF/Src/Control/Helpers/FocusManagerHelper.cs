#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides a set of static methods, attached property for determining and setting focus scopes and for setting the focused element within the scope for GridCell.
    /// </summary>
    public class FocusManagerHelper
    {
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.FocusManagerHelper.FocusedElement dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.FocusManagerHelper.FocusedElement dependency property.
        /// </remarks>  
        public static readonly DependencyProperty FocusedElementProperty =
            DependencyProperty.RegisterAttached("FocusedElement", typeof (bool), typeof (FocusManagerHelper), null);

        /// <summary>
        /// Sets logical focus on the specified element inside template of GridCell.
        /// </summary>
        /// <param name="element">
        /// The element to give logical focus to.
        /// </param>
        /// <param name="value">
        /// <b>true</b> if element has focus scope; otherwise, <b>false</b>.     
        /// </param>
        public static void SetFocusedElement(UIElement element, bool value)
        {
            element.SetValue(FocusedElementProperty, value);
        }

        /// <summary>
        /// Determines whether the specified element with logical focus.
        /// </summary>
        /// <param name="element">
        /// The element to determine with logical focus.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the element has logical focus; otherwise, <b>false</b>.
        /// </returns>
        public static bool GetFocusedElement(UIElement element)
        {
            return (bool) element.GetValue(FocusedElementProperty);
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.FocusManagerHelper.WantsKeyInput dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.FocusManagerHelper.WantsKeyInput dependency property.
        /// </remarks>  
        public static readonly DependencyProperty WantsKeyInputProperty =
            DependencyProperty.RegisterAttached("WantsKeyInput", typeof(bool), typeof(FocusManagerHelper), new PropertyMetadata(false));

        /// <summary>
        /// Provides the keyboard access to the UIElement loaded inside <b>CellTemplate</b> of Gridcolumn.
        /// </summary>
        /// <param name="column">
        /// The column which has CellTemplate to give keyboard access.
        /// </param>
        /// <param name="value">
        /// <b>true</b> if the keyboard interaction is enabled the UIElement for the specified column; otherwise, <b>false</b>.
        /// </param>
        /// <remarks>
        /// The keyboard interaction can be set when the column that has CellTemplate.
        /// </remarks>
        public static void SetWantsKeyInput(GridColumnBase column, bool value)
        {
            column.SetValue(WantsKeyInputProperty, value);
        }

        /// <summary>
        /// Determines whether the keyboard interaction is enabled for the specified column.
        /// </summary>
        /// <param name="column">
        /// The column to determine whether the keyboard interaction is enabled.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the keyboard interaction is enabled in the column; otherwise, <b>false</b>.
        /// </returns>
        public static bool GetWantsKeyInput(GridColumnBase column)
        {
            return (bool)column.GetValue(WantsKeyInputProperty);
        }


        /// <summary>
        /// Gets the control with logical focus within the template of GridColumn.
        /// </summary>
        /// <param name="obj">
        /// The element with logical focus in the specified focus scope.
        /// </param>
        /// <returns>
        /// The element in the specified focus scope with logical focus.
        /// </returns>
        public static Control GetFocusedUIElement(UIElement obj)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var element = VisualTreeHelper.GetChild(obj, i) as UIElement;
                if (element == null) 
                    continue;
                var control = GetFocusedElement(element);
                if (control)
                    return element as Control;
                var current = GetFocusedUIElement(element);
                if (current != null)
                    return current;
            }
            return null;
        }
    }    
}
