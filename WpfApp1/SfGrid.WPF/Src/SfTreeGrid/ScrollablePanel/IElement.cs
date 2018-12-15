#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using System;
using System.ComponentModel;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{ 
    public interface ITreeDataRowElement : IElement
    {
        TreeRowType RowType { get; }  
        void MeasureElement(Size size);
        void ArrangeElement(Rect rect);  
    }

    public interface ITreeDataColumnElement : IElement
    {
        ITreeGridCellRenderer Renderer { get; }
        int RowIndex { get; }		    
        void UpdateCellStyle();
    }
}
