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
#if DEBUG
    /// <summary>
    /// Class which is used to measure the performance of SfTreeGrid.
    /// </summary>
    public static class SfTreeGridPerformanceCounter
    {
        public static int ColumnSizer_Refresh_Count = 0;

        public static int MeasureOverride_Count = 0;

        public static int ArrangeOverride_Count = 0;
        public static void ResetCount()
        {
            ColumnSizer_Refresh_Count = 0;

            MeasureOverride_Count = 0;

            ArrangeOverride_Count = 0;
        }
    }
#endif
}
