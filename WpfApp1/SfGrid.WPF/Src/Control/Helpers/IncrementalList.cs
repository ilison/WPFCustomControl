#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml.Data;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// The list that implements ISupportIncrementalLoading to load the data for SfDataGrid incrementally.
    /// </summary>
    /// <typeparam name="T">The type of data object </typeparam>
    public class IncrementalList<T> : IList<T>, ISupportIncrementalLoading, INotifyCollectionChanged
    {
        #region Members

        bool isBusy = false;
        List<T> InternalList;
#if WinRT || UNIVERSAL
        Func<CancellationToken, uint, int, Task<IList<T>>> LoadMoreItems;
#else
        Action<uint, int> LoadMoreItems;
#endif

        #endregion

        #region Public Members

        public int MaxItemCount { get; set; }

        #endregion

        #region Ctor

#if WinRT || UNIVERSAL
        public IncrementalList(Func<CancellationToken, uint, int, Task<IList<T>>> loadeMoreItemsFunc)
#else
        public IncrementalList(Action<uint, int> loadeMoreItemsFunc)        
#endif
        { 
            InternalList = new List<T>();
            LoadMoreItems = loadeMoreItemsFunc;
        }


        #endregion

        #region ISupportIncreamentaLoading Members

        /// <summary>
        /// Gets a sentinel value that supports incremental loading implementations.
        /// </summary>
        public bool HasMoreItems
        {
            get { return InternalList.Count < MaxItemCount; }
        }

#if WinRT || UNIVERSAL
        IAsyncOperation<LoadMoreItemsResult> result;
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            try
            {
                if (isBusy)
                    return result;
                isBusy = true;
                result=AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
                return result;
            }
            finally
            {
                
            }

        }
#else
        /// <summary>
        /// Initializes incremental loading from the view.
        /// </summary>
        /// <param name="count">Specifies the number of items to load.</param>        
#if !SyncfusionFramework4_0
        public async Task LoadMoreItemsAsync(uint count)
#else
        public Task LoadMoreItemsAsync(uint count)
#endif
        {
            if (isBusy)
#if !SyncfusionFramework4_0
                return;
#else
                return null;
#endif
            isBusy = true;
            var baseIndex = this.InternalList.Count;
#if !SyncfusionFramework4_0
            await Task.Run(
            () =>
            {
#endif
                LoadMoreItems(count, baseIndex);
#if !SyncfusionFramework4_0
            }
            );
            return;
#else            
             return null;
#endif
        }
#endif

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return InternalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            InternalList.Insert(index, item);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public void RemoveAt(int index)
        {
            if (index < -1 || index > this.InternalList.Count)
                throw new ArgumentOutOfRangeException();
            var removedItem = InternalList[index];
            InternalList.RemoveAt(index);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        public T this[int index]
        {
            get
            {
                return InternalList[index];
            }
            set
            {
                var oldValue = InternalList[index];
                InternalList[index] = value;
#if !WPF
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue));
#else
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,value,oldValue,index));
#endif
            }
        }

        public void Add(T item)
        {
            InternalList.Add(item);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            InternalList.Clear();
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return InternalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InternalList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return InternalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            var index= this.InternalList.IndexOf(item);
            var result = InternalList.Remove(item);
            if (result)
            {
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (InternalList as IEnumerable).GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChaged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, args);
        }

        #endregion

        #region Private Methods
        
      

        void NotifyOfInsertedItems(int baseIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.InternalList[i + baseIndex], i + baseIndex);
                RaiseCollectionChanged(args);
            }
        }

#if !WinRT && !UNIVERSAL

        /// <summary>
        /// Adds the data objects incrementally. 
        /// </summary>
        /// <param name="items">Specifies the collection of data items to be added incrementally </param>
        public void LoadItems(IEnumerable<T> items)
        {
            if (items != null)
            {
                int baseIndex = InternalList.Count;
                InternalList.AddRange(items);
                NotifyOfInsertedItems(baseIndex, items.Count());
                isBusy = false;
            }
        }

#else
        async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {                
                var baseIndex = this.InternalList.Count;
                var items = await LoadMoreItems(c, count, baseIndex);
                if (items != null)
                {
                    InternalList.AddRange(items);
                    NotifyOfInsertedItems(baseIndex, items.Count);
                }
                return new LoadMoreItemsResult() { Count =items==null ? (uint)0: (uint)items.Count };
            }
            finally
            {
                isBusy = false;
            }
        }
#endif

        #endregion
    }
}

