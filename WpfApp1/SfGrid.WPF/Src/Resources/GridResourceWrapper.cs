#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#if WinRT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that retrieve and localize the culture-specific resources defined by the user. 
    /// </summary>
    public static class GridResourceWrapper
    {
#if WPF
        /// <summary>
        /// Sets the resources based on the specified assembly localize SfDataGrid. 
        /// </summary>
        /// <param name="assembly">
        /// An assembly to set the resources for localizing the SfDataGrid. 
        /// </param>        
        public static void SetResources(Assembly assembly)
        {
            var _namespace = assembly.FullName.Split(new char[] { ',' })[0];
            SR.SetResources(assembly, _namespace);
        }
        /// <summary>
        /// Sets the resources based on specified assembly and namespace to perform the localization. 
        /// </summary>
        /// <param name="assembly">
        /// An assembly to set the resources
        /// </param>
        /// <param name="_namespace">
        /// The namespace of the resources. 
        /// </param>
        public static void SetResources(Assembly assembly, string _namespace)
        {
            SR.SetResources(assembly, _namespace);
        }
#endif
        /// <summary>
        /// Gets the resource value for SelectAll resource name. 
        /// </summary>
        public static string SelectAll
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SelectAll"); }
        }

        /// <summary>
        /// Gets the resource value for AND resource name. 
        /// </summary>
        public static string AND
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "AND"); }
        }

        /// <summary>
        /// Gets the resource value for OR resource name. 
        /// </summary>
        public static string OR
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "OR"); }
        }

        /// <summary>
        /// Gets the resource value for AdvancedFiltersButtonText resource name. 
        /// </summary>
        public static string AdvancedFiltersButtonText
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "AdvancedFiltersButtonText"); }
        }

        /// <summary>
        /// Gets the resource value for ShowRowsWhere resource name. 
        /// </summary>
        public static string ShowRowsWhere
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "ShowRowsWhere"); }
        }

        /// <summary>
        /// Gets the resource value for Blanks resource name. 
        /// </summary>
        public static string Blanks
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Blanks"); }
        }

        /// <summary>
        /// Gets the resource value for Cancel resource name. 
        /// </summary>
        public static string Cancel
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Cancel"); }
        }

        /// <summary>
        /// Gets the resource value for Done resource name. 
        /// </summary>
        public static string Done
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Done"); }
        }

        /// <summary>
        /// Gets the resource value for ClearFilter resource name. 
        /// </summary>
        public static string ClearFilter
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "ClearFilter"); }
        }

        /// <summary>
        /// Gets the resource value for NoMatches resource name. 
        /// </summary>
        public static string NoMatches
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NoMatches"); }
        }

        /// <summary>
        /// Gets the resource value for OK resource name. 
        /// </summary>
        public static string OK
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "OK"); }
        }

        /// <summary>
        /// Gets the resource value for Search resource name. 
        /// </summary>
        public static string Search
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Search"); }
        }

        /// <summary>
        /// Gets the resource value for SortNumberAscending resource name. 
        /// </summary>
        public static string SortNumberAscending
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SortNumberAscending"); }
        }

        /// <summary>
        /// Gets the resource value for SortNumberDescending resource name. 
        /// </summary>
        public static string SortNumberDescending
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SortNumberDescending"); }
        }

        /// <summary>
        /// Gets the resource value for SortDateAscending resource name. 
        /// </summary>
        public static string SortDateAscending
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SortDateAscending"); }
        }

        /// <summary>
        /// Gets the resource value for SortDateDescending resource name. 
        /// </summary>
        public static string SortDateDescending
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SortDateDescending"); }
        }

        /// <summary>
        /// Gets the resource value for SortStringAscending resource name. 
        /// </summary>
        public static string SortStringAscending
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SortStringAscending"); }
        }

        /// <summary>
        /// Gets the resource value for SortStringDescending resource name. 
        /// </summary>
        public static string SortStringDescending
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SortStringDescending"); }
        }

        /// <summary>
        /// Gets the resource value for NoItems resource name. 
        /// </summary>
        public static string NoItems
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NoItems"); }
        }

        /// <summary>
        /// Gets the resource value for RowErrorMessage resource name. 
        /// </summary>
        public static string RowErrorMessage
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "RowErrorMessage"); }
        }

        /// <summary>
        /// Gets the resource value for ColumnChooserTitle resource name. 
        /// </summary>
        public static string ColumnChooserTitle
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "ColumnChooserTitle"); }
        }

        /// <summary>
        /// Gets the resource value for ColumnChooserWaterMark resource name. 
        /// </summary>
        public static string ColumnChooserWaterMark
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "ColumnChooserWaterMark"); }
        }

        /// <summary>
        /// Gets the resource value for AddNewRowText resource name. 
        /// </summary>
        public static string AddNewRowText
        {
            get { 
#if WinRT
                 if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                 return "Click here to add new row";
#endif
                    return SR.GetString(CultureInfo.CurrentUICulture, "AddNewRowText");
            }
        }

        /// <summary>
        /// Gets the resource value for GroupDropAreaText resource name. 
        /// </summary>
        public static string GroupDropAreaText
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "GroupDropAreaText"); }
        }

        /// <summary>
        /// Gets the resource value for PrintPreview resource name. 
        /// </summary>
        public static string PrintPreview
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "PrintPreview"); }
        }

        /// <summary>
        /// Gets the resource value for Print resource name. 
        /// </summary>
        public static string Print
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Print"); }
        }

        /// <summary>
        /// Gets the resource value for NoScaling resource name. 
        /// </summary>
        public static string NoScaling
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NoScaling"); }
        }
        /// <summary>
        /// Gets the resource value for FitGridOnOnePage resource name. 
        /// </summary>
        public static string FitGridOnOnePage
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "FitGridOnOnePage"); }
        }   
        /// <summary>
        /// Gets the resource value for FitAllColumnsOnOnePage resource name. 
        /// </summary>
        public static string FitAllColumnsOnOnePage
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "FitAllColumnsOnOnePage"); }
        }
        /// <summary>
        /// Gets the resource value for FitAllRowsOnOnePage resource name. 
        /// </summary>
        public static string FitAllRowsOnOnePage
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "FitAllRowsOnOnePage"); }
        }
        /// <summary>
        /// Gets the resource value for PortraitOrientation resource name. 
        /// </summary>
        public static string PortraitOrientation
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "PortraitOrientation"); }
        }
        /// <summary>
        /// Gets the resource value for LandScapeOrientation resource name. 
        /// </summary>
        public static string LandScapeOrientation
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "LandScapeOrientation"); }
        }
        /// <summary>
        /// Gets the resource value for CustomPageSizes resource name. 
        /// </summary>
        public static string CustomPageSizes
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "CustomPageSizes"); }
        }
        /// <summary>
        /// Gets the resource value for Normal resource name. 
        /// </summary>
        public static string Normal
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Normal"); }
        }
        /// <summary>
        /// Gets the resource value for Narrow resource name. 
        /// </summary>
        public static string Narrow
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Narrow"); }
        }
        /// <summary>
        /// Gets the resource value for Moderate resource name. 
        /// </summary>
        public static string Moderate
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Moderate"); }
        }
        /// <summary>
        /// Gets the resource value for Wide resource name. 
        /// </summary>
        public static string Wide
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Wide"); }
        }
        /// <summary>
        /// Gets the resource value for CustomMargin resource name. 
        /// </summary>
        public static string CustomMargin
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "CustomMargin"); }
        }
        /// <summary>
        /// Gets the resource value for CustomMargins resource name. 
        /// </summary>
        public static string CustomMargins
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "CustomMargins"); }
        }
        /// <summary>
        /// Gets the resource value for Left resource name. 
        /// </summary>
        public static string Left
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Left"); }
        }
        /// <summary>
        /// Gets the resource value for Right resource name. 
        /// </summary>
        public static string Right
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Right"); }
        }
        /// <summary>
        /// Gets the resource value for Top resource name. 
        /// </summary>
        public static string Top
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Top"); }
        }
        /// <summary>
        /// Gets the resource value for Bottom resource name. 
        /// </summary>
        public static string Bottom
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Bottom"); }
        }
        /// <summary>
        /// Gets the resource value for Width resource name. 
        /// </summary>
        public static string Width
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Width"); }
        }
        /// <summary>
        /// Gets the resource value for Height resource name. 
        /// </summary>
        public static string Height
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Height"); }
        }        
   
        /// <summary>
        /// Gets the resource value for CustomSize resource name.
        /// </summary>
        public static string CustomSize
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "CustomSize"); }
        }
        /// <summary>
        /// Gets the resource value for Equalss resource name. 
        /// </summary>
        public static string Equalss
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Equalss"); }
        }

        /// <summary>
        /// Gets the resource value for NotEquals resource name. 
        /// </summary>
        public static string NotEquals
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NotEquals"); }
        }

        /// <summary>
        /// Gets the resource value for BeginsWith resource name. 
        /// </summary>
        public static string BeginsWith
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "BeginsWith"); }
        }

        /// <summary>
        /// Gets the resource value for EndsWith resource name. 
        /// </summary>
        public static string EndsWith
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "EndsWith"); }
        }

        /// <summary>
        /// Gets the resource value for Contains resource name. 
        /// </summary>
        public static string Contains
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Contains"); }
        }

        /// <summary>
        /// Gets the resource value for NotContains resource name. 
        /// </summary>
        public static string NotContains
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NotContains"); }
        }

        /// <summary>
        /// Gets the resource value for Empty resource name. 
        /// </summary>
        public static string Empty
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Empty"); }
        }

        /// <summary>
        /// Gets the resource value for NotEmpty resource name. 
        /// </summary>
        public static string NotEmpty
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NotEmpty"); }
        }

        /// <summary>
        /// Gets the resource value for Null resource name. 
        /// </summary>
        public static string Null
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Null"); }
        }

        /// <summary>
        /// Gets the resource value for NotNull resource name. 
        /// </summary>
        public static string NotNull
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NotNull"); }
        }

        /// <summary>
        /// Gets the resource value for LessThanorEqual resource name. 
        /// </summary>
        public static string LessThanorEqual
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "LessThanorEqual"); }
        }

        /// <summary>
        /// Gets the resource value for LessThan resource name. 
        /// </summary>
        public static string LessThan
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "LessThan"); }
        }

        /// <summary>
        /// Gets the resource value for GreaterThan resource name. 
        /// </summary>
        public static string GreaterThan
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "GreaterThan"); }
        }

        /// <summary>
        /// Gets the resource value for GreaterThanorEqual resource name. 
        /// </summary>
        public static string GreaterThanorEqual
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "GreaterThanorEqual"); }
        }

        /// <summary>
        /// Gets the resource value for Before resource name. 
        /// </summary>
        public static string Before
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Before"); }
        }

        /// <summary>
        /// Gets the resource value for BeforeOrEqual resource name. 
        /// </summary>
        public static string BeforeOrEqual
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "BeforeOrEqual"); }
        }

        /// <summary>
        /// Gets the resource value for After resource name. 
        /// </summary>
        public static string After
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "After"); }
        }

        /// <summary>
        /// Gets the resource value for AfterOrEqual resource name. 
        /// </summary>
        public static string AfterOrEqual
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "AfterOrEqual"); }
        }

        /// <summary>
        /// Gets the resource value for EnterValidFilterValue resource name. 
        /// </summary>
        public static string EnterValidFilterValue
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "EnterValidFilterValue"); }
        }

        /// <summary>
        /// Gets the resource value for TextFilters resource name. 
        /// </summary>
        public static string TextFilters
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "TextFilters"); }
        }

        /// <summary>
        /// Gets the resource value for NumberFilters resource name. 
        /// </summary>
        public static string NumberFilters
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "NumberFilters"); }
        }

        /// <summary>
        /// Gets the resource value for DateFilters resource name. 
        /// </summary>
        public static string DateFilters
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "DateFilters"); }
        }
#if WPF
        /// <summary>
        /// Gets the resource value for MatchCaseToolTip resource name. 
        /// </summary>
        public static string MatchCaseToolTip
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "MatchCaseToolTip"); }
        }

        /// <summary>
        /// Gets the resource value for SelectDateToolTip resource name. 
        /// </summary>
        public static string SelectDateToolTip
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "SelectDateToolTip"); }
        }
        
        /// <summary>
        /// Gets the resource value for Close resource name. 
        /// </summary>
        public static string Close
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "Close"); }
        }
#endif
        /// <summary>
        /// Gets the resource value for QuickPrint resource name. 
        /// </summary>
        public static string QuickPrint
        {
            get { return SR.GetString(CultureInfo.CurrentUICulture, "QuickPrint"); }
        }
    }

    public class ResourceHelper
    {

#if WPF
        public string Close { get { return GridResourceWrapper.Close; } }
        public string SelectDateToolTip { get { return GridResourceWrapper.SelectDateToolTip; } }
        public string MatchCaseToolTip { get { return GridResourceWrapper.MatchCaseToolTip; } }
#endif
        public string SelectAll { get { return GridResourceWrapper.SelectAll; } }
        public string AdvancedFilters { get { return GridResourceWrapper.AdvancedFiltersButtonText; } }
        public string AND { get { return GridResourceWrapper.AND; } }
        public string OR { get { return GridResourceWrapper.OR; } }
        public string ShowRowsWhere { get { return GridResourceWrapper.ShowRowsWhere; } }
        public string Blanks { get { return GridResourceWrapper.Blanks; } }
        public string Cancel { get { return GridResourceWrapper.Cancel; } }
        public string ClearFilter { get { return GridResourceWrapper.ClearFilter; } }
        public string NoMatches { get { return GridResourceWrapper.NoMatches; } }
        public string OK { get { return GridResourceWrapper.OK; } }
        public string RowErrorMessage { get { return GridResourceWrapper.RowErrorMessage; } }
        public string Search { get { return GridResourceWrapper.Search; } }
        public string SortNumberAscending { get { return GridResourceWrapper.SortNumberAscending; } }
        public string SortNumberDescending { get { return GridResourceWrapper.SortNumberDescending; } }
        public string SortStringAscending { get { return GridResourceWrapper.SortStringAscending; } }
        public string SortStringDescending { get { return GridResourceWrapper.SortStringDescending; } }
        public string NoItems { get { return GridResourceWrapper.NoItems; } }
        public string ColumnChooserTitle { get { return GridResourceWrapper.ColumnChooserTitle; } }
        public string ColumnChooserWaterMark { get { return GridResourceWrapper.ColumnChooserWaterMark; } }
        public string AddNewRowText { get { return GridResourceWrapper.AddNewRowText; } }
        public string GroupDropAreaText { get { return GridResourceWrapper.GroupDropAreaText; } }
        public string Print { get { return GridResourceWrapper.Print; } }
        public string NoScaling { get { return GridResourceWrapper.NoScaling; } }
        public string FitGridOnOnePage { get { return GridResourceWrapper.FitGridOnOnePage; } }
        public string FitAllColumnsOnOnePage { get { return GridResourceWrapper.FitAllColumnsOnOnePage; } }
        public string FitAllRowsOnOnePage { get { return GridResourceWrapper.FitAllRowsOnOnePage; } }
        public string PortraitOrientation { get { return GridResourceWrapper.PortraitOrientation; } }
        public string LandScapeOrientation { get { return GridResourceWrapper.LandScapeOrientation; } }
        public string CustomPageSizes { get { return GridResourceWrapper.CustomPageSizes; } }
        public string CustomSize { get { return GridResourceWrapper.CustomSize; } }       
        public string Height { get { return GridResourceWrapper.Height; } }
        public string Width { get { return GridResourceWrapper.Width; } }
        public string Left { get { return GridResourceWrapper.Left; } }
        public string Right { get { return GridResourceWrapper.Right; } }
        public string Top { get { return GridResourceWrapper.Top; } }
        public string Bottom { get { return GridResourceWrapper.Bottom; } }
        public string PrintPreview { get { return GridResourceWrapper.PrintPreview; } }
        public string Equalss { get { return GridResourceWrapper.Equalss; } }
        public string NotEquals { get { return GridResourceWrapper.NotEquals; } }
        public string BeginsWith { get { return GridResourceWrapper.BeginsWith; } }
        public string EndsWith { get { return GridResourceWrapper.EndsWith; } }
        public string Contains { get { return GridResourceWrapper.Contains; } }
        public string NotContains { get { return GridResourceWrapper.NotContains; } }
        public string Empty { get { return GridResourceWrapper.Empty; } }
        public string NotEmpty { get { return GridResourceWrapper.NotEmpty; } }
        public string Null { get { return GridResourceWrapper.Null; } }
        public string NotNull { get { return GridResourceWrapper.NotNull; } }
        public string LessThan { get { return GridResourceWrapper.LessThan; } }
        public string LessThanorEqual { get { return GridResourceWrapper.LessThanorEqual; } }
        public string GreaterThan { get { return GridResourceWrapper.GreaterThan; } }
        public string GreaterThanorEqual { get { return GridResourceWrapper.GreaterThanorEqual; } }
    }
}
