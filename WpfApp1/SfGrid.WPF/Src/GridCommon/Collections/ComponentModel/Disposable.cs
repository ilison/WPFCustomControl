#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using System;

namespace Syncfusion.UI.Xaml.Collections.ComponentModel
{
    /// <summary>
    /// This is a base class for Disposable. It implements the IDisposable interface
    /// as suggested in the .NET documentation using the Disposable pattern but it does not
    /// implement a finalizer. If you need finalization you need to derive from Disposable
    /// or add a finalizer to your derived class and manually call Dispose from the Finalizer.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class NonFinalizeDisposable : IDisposable
    {
        /// <overload>
        /// Releases all resources used by the Component.
        /// </overload>
        /// <summary>
        /// Releases all resources used by the Component.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        /// <remarks>See the documentation for the <see cref="System.ComponentModel.Component"/> class and its Dispose member.</remarks>
        protected virtual void Dispose(bool disposing)
        {
        }
    }

    /// <summary>
    /// This class provides a base class that implements the IDisposable interface
    /// as suggested in the .NET documentation using the Disposable pattern.
    /// </summary>
    /// <remarks>If you derive from this class, you only need to override the protected
    /// Dispose method and check the disposing parameter.</remarks>
    [ClassReference(IsReviewed = false)]
    public class Disposable : NonFinalizeDisposable
    {
        /// <summary>
        /// <see cref="Object.Finalize"/>.<para/>
        /// In C# and C++, finalizers are expressed using destructor syntax.
        /// </summary>
        ~Disposable()
        {
            this.Dispose(false);
        }
    }



    /// <summary>
    /// This class provides a base class that implements the IDisposable interface
    /// as suggested in the .NET documentation using the Disposable pattern. After the
    /// object was disposed a <see cref="Disposed"/> event is raised.
    /// </summary>
    /// <remarks>If you derive from this class, you only need to override the protected
    /// Dispose method and check the disposing parameter.</remarks>
    [ClassReference(IsReviewed = false)]
    public class DisposableWithEvent : IDisposable
    {
        #region Fields
        bool inDispose;
        bool isDisposed;
        #endregion

        #region Properties
        /// <summary>
        /// Allows to detect if object is disposed or not. True indicates object is disposed,
        /// otherwise indicates object is still alive and ready for use.
        /// </summary>
        protected bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        /// <summary>
        /// Returns True if object is executing the <see cref="Dispose"/> method call.
        /// </summary>
        public bool IsDisposing
        {
            get
            {
                return inDispose;
            }
        }
        #endregion

        #region Dispose Method
        /// <overload>
        /// Releases all resources used by the Component.
        /// </overload>
        /// <summary>
        /// Releases all resources used by the Component.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed || inDispose)
                return;

            inDispose = true;
            Dispose(true);
            inDispose = false;                      
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        /// <remarks>See the documentation for the <see cref="System.ComponentModel.Component"/> class and its Dispose member.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
                OnDisposed(EventArgs.Empty);

            isDisposed = true;
        }
        #endregion

        #region Disposed Event
        /// <summary>
        /// Occurs after the object was disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
                Disposed(this, e);
        }
        #endregion
    }

    /// <summary>
    /// Provides classes and interfaces that are used to dispose the control and process the Batch updates in SfDataGrid.
    /// </summary>
    class NamespaceDoc
    { }
}
