#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
#if UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
    /// <summary>
    /// Represents a class that implements a cache of UIElement of the given type parameter T. 
    /// It is used by the TreeGridVirtualizingCellRendererBase renderer to recycle UIElement 
    /// for cells that were scrolled out of view and delay unloading of UIElements.
    /// This reduces the number of times the UIElement needs to be created or unloaded ,
    /// instead only the contents of the UIElement will be reinitialized with cell contents. 
    /// <para/>
    /// </summary>
    /// <typeparam name="T">Type of the UIElement to be recycled </typeparam>    
    public class VirtualizingCellUIElementBin<T> : Dictionary<TreeGridCellRendererBase, Queue<WeakReference>>
        where T : UIElement
    {
        #region Property

        /// <summary>
        /// Gets the <see cref="System.Collections.Generic.Queue{WeakReference}"/> for the specified Renderer.
        /// </summary>
        /// <value></value>
        public new Queue<WeakReference> this[TreeGridCellRendererBase renderer]
        {
            get
            {
                if (ContainsKey(renderer))
                    return base[renderer];

                Queue<WeakReference> queue = base[renderer] = new Queue<WeakReference>();
                return queue;
            }
        }

        #endregion

        #region Public methods        
        /// <summary>
        /// Enqueues the UI element to the specified renderer.
        /// </summary>
        /// <param name="renderer">
        /// The corresponding renderer to enqueue the UIElement in it.
        /// </param>
        /// <param name="uiElement">
        /// The corresponding UIElement to perform enqueue operation.
        /// </param>
        public void Enqueue(TreeGridCellRendererBase renderer, T uiElement)
        {
            this[renderer].Enqueue(new WeakReference(uiElement));
            uiElement.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Dequeues an UIElement from the specified renderer.
        /// </summary>
        /// <param name="renderer">
        /// Specifies the corresponding renderer to dequeue the UIElement
        /// </param>
        /// <returns>
        /// Returns the UIelement.
        /// </returns>        
        public T Dequeue(TreeGridCellRendererBase renderer)
        {
            if (!ContainsKey(renderer))
                return default(T);

            Queue<WeakReference> queue = base[renderer];
            if (queue.Count == 0)
                return default(T);

            var el = queue.Dequeue().Target as T;

            if (el == null)
                return default(T);
            
            el.Visibility = Visibility.Visible;
            
            return el;
        }

        #endregion
    }
}
