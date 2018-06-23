using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp
{
    /// <summary>
    /// Base class for conformly implementing the <see cref="IDisposable"/> interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This base class handles the <see cref="IsDisposed"/> state of the instance and provides
    /// methods to uniformly deal with it (see <see cref="ThrowIfDisposed()"/>).
    /// </para>
    /// <para>
    /// Any deriving class that adds dispose tasks should overwrite <see cref="Dispose(bool)"/> and
    /// implement it as one would normally do:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Never call <see cref="Dispose(bool)"/> directly; only ever call <see cref="Dispose()"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <see cref="Dispose(bool)"/> method is guaranteed to be called only once.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <see cref="Dispose(bool)"/> implementation MUST NEVER throw.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If the parameter of <see cref="Dispose(bool)"/> is <c>false</c>, managed attributes of the
    /// instance may or may not have been disposed already and shall be avoided.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    [PublicAPI]
    public abstract class Disposable : IDisposable
    {
        /// <summary>
        /// Create a new <see cref="Disposable"/> instance.
        /// </summary>
        /// <param name="ADisposed">If <c>true</c>, the instance starts out disposed.</param>
        protected Disposable(bool ADisposed = false)
        {
            IsDisposed = ADisposed;
        }

        /// <summary>
        /// Finalize this instance.
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
            IsDisposed = true;
        }

        /// <summary>
        /// Dispose of this instance by freeing all unmanaged and optionally all managed resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see cref="Disposable"/> for details on implementation.
        /// </para>
        /// <para>
        /// Never throw in any implementation of <see cref="Dispose(bool)"/>!
        /// Never call <see cref="Dispose(bool)"/> directly; only ever call <see cref="Dispose()"/>.
        /// </para>
        /// </remarks>
        /// <param name="ADisposing">
        /// If <c>false</c>, managed attributes of this instance may or may not have been disposed
        /// already and shall be avoided.
        /// </param>
        protected virtual void Dispose(bool ADisposing) { }

        #region IDisposable
        /// <inheritdoc />
        /* Note: We do this correctly, but relieve Dispose(bool) of the check to make overriding
                 even simpler - it wont have to do anything if there is nothing to do, and might
                 as well not even be implemented.
         */
        [
            System.Diagnostics.CodeAnalysis.SuppressMessage(
                "Microsoft.Design",
                "CA1063:ImplementIDisposableCorrectly"
            )
        ]
        public void Dispose()
        {
            // Multiple calls to Dispose() SHALL NOT throw.
            if (IsDisposed) return;

            Dispose(true);

            // The finalizer SHALL NOT be invoked.
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }
        #endregion

        /// <summary>
        /// Throw an <see cref="ObjectDisposedException"/> if this instance is disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance is disposed.</exception>
        public void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
        /// <summary>
        /// Assert that this instance is not disposed.
        /// </summary>
        /// <remarks>
        /// This method is conditionally available only in DEBUG builds.
        /// </remarks>
        [Conditional("DEBUG")]
        public void AssertNotDisposed()
        {
            Debug.Assert(
                !IsDisposed,
                "Instance is disposed.",
                "An operation was attempted on a disposed instance. This indicates a severe " +
                "logic error."
            );
        }

        /// <summary>
        /// Get a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }
    }
}
