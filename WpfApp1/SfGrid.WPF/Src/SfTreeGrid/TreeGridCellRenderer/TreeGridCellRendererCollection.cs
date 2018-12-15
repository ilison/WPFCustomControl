#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Collections.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
    /// <summary>
    /// Represents a collection of <see cref="TreeGridCellRendererBase"/> objects in the view.
    /// </summary>
    /// <remarks>
    /// On the <see cref="Columns"/>, you access the <see cref="TreeGridCellRendererCollection"/> through the <see cref="Columns.CellRenderers"/> property.
    /// <para/>
    /// The <see cref="TreeGridCellRendererCollection"/> uses standard <see cref="Add"/> and <see cref="Remove"/>
    /// methods to manipulate the collection.
    /// Use the Contains method to determine if a specific cell type exists in the collection.
    /// </remarks>    
    public class TreeGridCellRendererCollection : Disposable, ICollection
    {
        #region Fields

        internal Dictionary<string, ITreeGridCellRenderer> content = new Dictionary<string, ITreeGridCellRenderer>();
        string cachedKey = "";
        ITreeGridCellRenderer cachedRenderer = null;
        SfTreeGrid treeGrid = null;

        #endregion

        #region Property

        /// <summary>
        /// Indexer will return the corresponding renderer for the corresponding Key value.
        /// </summary>
        /// <param name="key"></param>
        /// <value></value>
        /// <remarks></remarks>
        public ITreeGridCellRenderer this[string key]
        {
            get
            {
                if (key == cachedKey)
                {
                    cachedRenderer.TreeGrid = this.treeGrid;
                    return cachedRenderer;
                }

                cachedKey = key;
                if (!this.ContainsKey(key))
                {
                    cachedKey = key;
                    this.Add(cachedKey, cachedRenderer);
                }
                else
                    cachedRenderer = (ITreeGridCellRenderer)content[key];

                cachedRenderer.TreeGrid = this.treeGrid;
                return cachedRenderer;
            }
            set
            {
                if (this.ContainsKey(key) && content[key] != value)
                    this.Remove(key);
                this.Add(key, value);
            }
        }

        /// <summary>
        /// Gets the Renderers in the collection
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public ICollection Values
        {
            get
            {
                return this.content.Values;
            }
        }

        /// <summary>
        /// Get the Key values of Renderers
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public ICollection Keys
        {
            get
            {
                return this.content.Keys;
            }
        }

        #endregion

        #region Ctor

        public TreeGridCellRendererCollection(SfTreeGrid treeGrid)
        {
            this.treeGrid = treeGrid;
        }

        #endregion

        #region override methods

        /// <summary>
        /// Releases the unmanaged resources used by the Component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        /// <remarks>See the documentation for the <see cref="System.ComponentModel.Component"/> class and its Dispose member.</remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
                this.content = null;
                this.treeGrid = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Add the Renderes to the Renderer dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="renderer"></param>
        /// <remarks></remarks>
        public void Add(string key, ITreeGridCellRenderer renderer)
        {
            this.content.Add(key, renderer);
        }

        /// <summary>
        /// Remove the Renderer from dictionary for corresponding key vallue
        /// </summary>
        /// <param name="key"></param>
        /// <remarks></remarks>
        public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                var renderer = content[key] as ITreeGridCellRenderer;
                if (renderer != null)
                    renderer.Dispose();
                this.content.Remove(key);
            }
            cachedKey = "";
            cachedRenderer = null;
        }

        /// <summary>
        /// Checks whether the Renderer Dictionary contains the Corresponding Renderer Key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool ContainsKey(string key)
        {
            return this.content.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether the Render Dictionary contains the corresponding renderer.
        /// </summary>
        /// <param name="cellRenderer"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool ContainsValue(TreeGridCellRendererBase cellRenderer)
        {
            return this.content.ContainsValue(cellRenderer);
        }

        /// <summary>
        /// Copy the Renderer values to Array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <remarks></remarks>
        public void CopyTo(TreeGridCellRendererBase[] array, int index)
        {
            this.content.Values.CopyTo(array, index);
        }

        /// <summary>
        /// Clears the values in Renderer Dictionary.
        /// </summary>
        /// <remarks></remarks>
        public void Clear()
        {
            foreach (var obj in content.Values)
            {
                var renderer = obj as TreeGridCellRendererBase;
                if (renderer != null)
                    renderer.Dispose();
            }
            this.content.Clear();
            cachedKey = "";
            cachedRenderer = null;
        }

        #endregion

        #region ICollection

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection" /> to an
        /// <see cref="T:System.Array" />, starting at a particular <see
        /// cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="index">The zero-based index in <paramref name="array" /> at which
        /// copying begins. </param>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is
        /// the destination of the elements copied from <see
        /// cref="T:System.Collections.ICollection" />. The <see cref="T:System.Array" />
        /// must have zero-based indexing. </param>
        /// <filterpriority>2</filterpriority>
        /// <exception cref="T:System.ArgumentException"><paramref name="array" /> is
        /// multidimensional.-or- The number of elements in the source <see
        /// cref="T:System.Collections.ICollection" /> is greater than the available space
        /// from <paramref name="index" /> to the end of the destination <paramref
        /// name="array" />.-or-The type of the source <see
        /// cref="T:System.Collections.ICollection" /> cannot be cast automatically to the
        /// type of the destination <paramref name="array" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index" />
        /// is less than zero. </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="array" /> is
        /// null. </exception>
        public void CopyTo(Array array, int index)
        {
            CopyTo((TreeGridCellRendererBase[])array, index);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see
        /// cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see
        /// cref="T:System.Collections.ICollection" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public int Count
        {
            get { return this.content.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see
        /// cref="T:System.Collections.ICollection" /> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="T:System.Collections.ICollection" /> is
        /// synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see
        /// cref="T:System.Collections.ICollection" />.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see
        /// cref="T:System.Collections.ICollection" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object SyncRoot
        {
            get { return null; }
        }

        /// <summary>
        /// Returns the Enumerator for retrieving the values.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public IEnumerator GetEnumerator()
        {
            return this.content.GetEnumerator();
        }

        #endregion
    }
}
