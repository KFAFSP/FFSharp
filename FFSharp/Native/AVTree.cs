using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

using Whetstone.Contracts;

using Unsafe = FFmpeg.AutoGen;

namespace FFSharp.Native
{
    /// <summary>
    /// Provides methods that operate on <see cref="Unsafe.AVTreeNode"/>.
    /// </summary>
    // ReSharper disable errors
    internal static unsafe class AVTree
    {
        /// <summary>
        /// Allocate a new <see cref="Unsafe.AVTreeNode"/>.
        /// </summary>
        /// <returns><see cref="Result{T}"/> with the new <see cref="Unsafe.AVTreeNode"/>.</returns>
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVTreeNode>> Alloc()
        {
            Fixed<Unsafe.AVTreeNode> node = Unsafe.ffmpeg.av_tree_node_alloc();
            if (node.IsNull) return new BadAllocationException(typeof(Unsafe.AVTreeNode));

            return node;
        }

        /// <summary>
        /// Find an item in the tree.
        /// </summary>
        /// <param name="ARoot">The root <see cref="Unsafe.AVTreeNode"/>.</param>
        /// <param name="AKey">The key to search for.</param>
        /// <param name="ACompare">The compare delegate.</param>
        /// <param name="APrevious">Will be set to the previous element in the tree.</param>
        /// <param name="ANext">Will be set to the next element in the tree.</param>
        /// <returns>The item that was found.</returns>
        [Pure]
        public static Fixed Find(
            Fixed<Unsafe.AVTreeNode> ARoot,
            Fixed AKey,
            [NotNull] [InstantHandle] Func<Fixed, Fixed, int> ACompare,
            out Fixed APrevious,
            out Fixed ANext
        )
        {
            // No null-check necessary.

            Debug.Assert(
                ACompare != null,
                "Compare is null.",
                "This indicates a contract violation."
            );

            Unsafe.av_tree_find_cmp make_compare_delegate() => (X, Y) => ACompare(X, Y);

            Unsafe.void_ptrArray2 next = new Unsafe.void_ptrArray2();
            var result = Unsafe.ffmpeg.av_tree_find(ARoot, AKey, make_compare_delegate(), ref next);

            APrevious = next[0];
            ANext = next[1];
            return result;
        }

        /// <summary>
        /// Free a <see cref="Unsafe.AVTreeNode"/>.
        /// </summary>
        /// <param name="ARoot">The <see cref="Unsafe.AVTreeNode"/>.</param>
        public static void Free(Fixed<Unsafe.AVTreeNode> ARoot)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.av_tree_destroy(ARoot);
        }

        /// <summary>
        /// Insert or remove items in the tree.
        /// </summary>
        /// <param name="ARoot">The root <see cref="Unsafe.AVTreeNode"/>.</param>
        /// <param name="AKey">The key to remove/insert.</param>
        /// <param name="ACompare">The compare delegate.</param>
        /// <param name="ANext">
        /// To remove, <see langword="null"/>.
        /// To insert, an allocated <see cref="Unsafe.AVTreeNode"/>.
        /// </param>
        /// <returns><see langword="null"/> or <paramref name="AKey"/> if no insertion or
        /// removal was performed; otherwise the already present value for <paramref name="AKey"/>.
        /// </returns>
        /// <remarks>
        /// If no intended insertion was performed <paramref name="ANext"/> is not reset to
        /// <see langword="null"/> and must be free'd by the caller again.
        /// </remarks>
        [MustUseReturnValue]
        public static Fixed Insert(
            Movable<Unsafe.AVTreeNode> ARoot,
            Fixed AKey,
            [NotNull] [InstantHandle] Func<Fixed, Fixed, int> ACompare,
            Movable<Unsafe.AVTreeNode> ANext
        )
        {
            // No present-check necessary.

            Debug.Assert(
                !ARoot.IsNull,
                "Root is null.",
                "This indicates a contract violation."
            );

            Unsafe.av_tree_insert_cmp make_compare_delegate() => (X, Y) => ACompare(X, Y);

            Fixed result = Unsafe.ffmpeg.av_tree_insert(
                ARoot,
                AKey,
                make_compare_delegate(),
                ANext
            );
            return result;
        }

        /// <summary>
        /// Insert an item into the tree.
        /// </summary>
        /// <param name="ARoot">The root <see cref="Unsafe.AVTreeNode"/>.</param>
        /// <param name="AKey">The item to insert.</param>
        /// <param name="ACompare">The compare delegate.</param>
        /// <returns><see cref="Result{T}"/> wrapping whether an item was inserted.</returns>
        [MustUseReturnValue]
        public static Result<bool> InsertItem(
            Movable<Unsafe.AVTreeNode> ARoot,
            Fixed AKey,
            [NotNull] [InstantHandle] Func<Fixed, Fixed, int> ACompare
        )
        {
            var alloc = Alloc();
            if (!alloc.IsSuccess)
            {
                return alloc.Error;
            }

            Unsafe.AVTreeNode* ins = alloc.Value;
            var result = Insert(ARoot, AKey, ACompare, &ins);

            if (ins != null)
            {
                Free(ins);
            }

            return result.Or(AKey) == AKey;
        }

        /// <summary>
        /// Remove an item from the tree.
        /// </summary>
        /// <param name="ARoot">The root <see cref="Unsafe.AVTreeNode"/>.</param>
        /// <param name="AKey">The item to remove.</param>
        /// <param name="ACompare">The compare delegate.</param>
        /// <returns>
        /// <see langword="true"/> if an item was removed; otherwise <see langword="false"/>.
        /// </returns>
        public static bool RemoveItem(
            Movable<Unsafe.AVTreeNode> ARoot,
            Fixed AKey,
            [NotNull] [InstantHandle] Func<Fixed, Fixed, int> ACompare
        ) => Insert(ARoot, AKey, ACompare, null).Or(AKey) == AKey;

        /// <summary>
        /// Visit nodes in the tree.
        /// </summary>
        /// <param name="ARoot">The root <see cref="Unsafe.AVTreeNode"/>.</param>
        /// <param name="AVisitor">The visitor delegate.</param>
        /// <param name="ACompare">The compare delegate.</param>
        /// <remarks>
        /// <para>
        /// If <paramref name="ACompare"/> is <see langword="null"/>, all nodes will be visited.
        /// </para>
        /// <para>
        /// <paramref name="ACompare"/> shall return:
        /// <list type="bullet">
        /// <item><description>
        /// &lt; 0, if the evaluated item is leftwards outside the range.
        /// </description></item>
        /// <item><description>
        /// = 0, if the evaluated item is inside the range.
        /// </description></item>
        /// <item><description>
        /// &gt; 0, if the evaluated item is rightwards outside the range.
        /// </description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public static void Visit(
            Fixed<Unsafe.AVTreeNode> ARoot,
            [NotNull] [InstantHandle] Action<Fixed> AVisitor,
            [CanBeNull] [InstantHandle] Func<Fixed, int> ACompare = null
        )
        {
            // No null-check necessary.

            Debug.Assert(
                AVisitor != null,
                "Visitor is null.",
                "This indicates a contract violation."
            );

            Unsafe.av_tree_enumerate_cmp make_compare_delegate()
            {
                if (ACompare == null) return null;

                return (opaque, elem) => ACompare(elem);
            }

            Unsafe.av_tree_enumerate_enu make_visitor_delegate() => (opaque, elem) =>
            {
                AVisitor(elem);
                return 0;
            };

            Unsafe.ffmpeg.av_tree_enumerate(
                ARoot,
                null,
                make_compare_delegate(),
                make_visitor_delegate()
            );
        }
    }
    // ReSharper restore errors
}
