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
using System.ComponentModel;
using System.Threading;

namespace Syncfusion.UI.Xaml.Grid
{
    internal static class BusyIndicatorHelper
    {
        internal static void RunWork(this SfDataGrid dataGrid, Action work, bool runAsync = true)
        {
            if (!runAsync)
            {
                work.Invoke();
                return;
            }
            RunWorkProgress(dataGrid, work);
        }
#if !SyncfusionFramework4_0
        internal static async void RunWorkProgress(SfDataGrid dataGrid, Action work)
        {
            dataGrid.SetBusyState("Busy");
            await Task.Delay(50);
            try
            {
#if WPF
                await dataGrid.Dispatcher.InvokeAsync(() =>
                {
#endif
                    work.Invoke();
                    dataGrid.SetBusyState("Normal");
#if WPF
                });
#endif
            }
            catch(ArgumentException)
            {
#if DEBUG
                throw new InvalidOperationException("Not able to perform data operation for current action. Try setting GridColumn.UseBindingValue or contact support");
#endif
            }
        }
#else
        internal static void RunWorkProgress(SfDataGrid dataGrid, Action work)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (o, ea) =>
            {
                Thread.Sleep(50);
                dataGrid.Dispatcher.Invoke((Action)(() =>
                {
                    work.Invoke();
                }));

            };
            worker.RunWorkerCompleted += (o, args) =>
            {
                if (dataGrid != null)
                    dataGrid.SetBusyState("Normal");
            };            
            worker.RunWorkerAsync();
            dataGrid.SetBusyState("Busy");
        }
#endif

    }
}
