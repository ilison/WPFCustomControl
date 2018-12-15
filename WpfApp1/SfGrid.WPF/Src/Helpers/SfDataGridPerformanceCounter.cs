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

namespace Syncfusion.UI.Xaml.Grid
{
#if DEBUG
    public static class SfDataGridPerformanceCounter
    {
        public static int DataGrid_ColumnSizer_Refresh_Count = 0;

        public static int DataGrid_MeasureOverride_Count = 0;

        public static int DataGrid_ArrangeOverride_Count = 0;

        public static int DetaisViewDataGrid_ColumnSizer_Refresh_count = 0;

        public static int DetaisViewDataGrid_MeasureOverride_Count = 0;

        public static int DetaisViewDataGrid_ArrangeOverride_Count = 0;


        public static void ResetCount()
        {
            DataGrid_ColumnSizer_Refresh_Count = 0;

            DataGrid_MeasureOverride_Count = 0;

            DataGrid_ArrangeOverride_Count = 0;

            DetaisViewDataGrid_ColumnSizer_Refresh_count = 0;

            DetaisViewDataGrid_MeasureOverride_Count = 0;

            DetaisViewDataGrid_ArrangeOverride_Count = 0;

        }
    }
#endif
}
