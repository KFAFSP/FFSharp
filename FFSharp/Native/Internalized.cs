using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace FFSharp.Native
{
    /// <summary>
    /// Template type for implementing internalization for managed wrappers of unmanaged references.
    /// </summary>
    /// <typeparam name="TUnmanaged">The unmanaged type that is wrapped.</typeparam>
    /// <typeparam name="TManaged">The managed type that wraps it.</typeparam>
    /// <remarks>
    /// This class is helpful if reference equality of the <typeparamref name="TManaged"/> type
    /// is desirable, as it manages exactly one instance per unmanaged object if all factories
    /// delegate to this type.
    /// </remarks>
    // ReSharper disable errors
    internal static class Internalized<TUnmanaged, TManaged>
        where TUnmanaged : unmanaged
        where TManaged : class
    {
        static readonly Dictionary<Ref<TUnmanaged>, WeakReference<TManaged>> _FReferences =
            new Dictionary<Ref<TUnmanaged>, WeakReference<TManaged>>();

        /// <summary>
        /// Clean up all orphaned weak references.
        /// </summary>
        public static void Clean()
        {
            bool cleaned;
            do
            {
                cleaned = false;

                foreach (var pair in _FReferences)
                {
                    if (!pair.Value.TryGetTarget(out _))
                    {
                        _FReferences.Remove(pair.Key);
                        cleaned = true;
                        break;
                    }
                }
            } while (cleaned);
        }

        /// <summary>
        /// Get the internalized managed wrapper for the specified reference.
        /// </summary>
        /// <param name="ARef">The <see cref="Ref{T}"/>.</param>
        /// <returns>The managed wrapper or <see langword="null"/> if none exists.</returns>
        [CanBeNull]
        public static TManaged For(Ref<TUnmanaged> ARef)
        {
            if (_FReferences.TryGetValue(ARef, out var weak))
            {
                if (weak.TryGetTarget(out var managed))
                {
                    return managed;
                }

                _FReferences.Remove(ARef);
            }

            return null;
        }

        /// <summary>
        /// Get or create a managed wrapper for the specified reference.
        /// </summary>
        /// <param name="ARef">The <see cref="Ref{T}"/>.</param>
        /// <param name="AFactory">The factory delegate.</param>
        /// <returns>
        /// The managed wrapper or <see langword="null"/> if <paramref name="ARef"/> is
        /// <see cref="Ref{T}.Null"/>.
        /// </returns>
        [CanBeNull]
        public static TManaged Of(
            Ref<TUnmanaged> ARef,
            [NotNull] Func<Ref<TUnmanaged>, TManaged> AFactory)
        {
            Debug.Assert(
                AFactory != null,
                "Factory is null.",
                "The provided factory delegate is null. This indicates a severe logic error in " +
                "the code."
            );

            if (ARef.IsNull)
            {
                return null;
            }

            var intern = For(ARef);
            if (intern == null)
            {
                intern = AFactory(ARef);
                _FReferences.Add(ARef, new WeakReference<TManaged>(intern));
            }

            return intern;
        }
    }
    // ReSharper restore errors
}
