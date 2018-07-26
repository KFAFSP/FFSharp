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
    // ReSharper disable errors
    internal static unsafe class AVTree
    {
        [MustUseReturnValue]
        public static Result<Fixed<Unsafe.AVTreeNode>> Alloc()
        {
            Fixed<Unsafe.AVTreeNode> node = Unsafe.ffmpeg.av_tree_node_alloc();
            if (node.IsNull) return new BadAllocationException(typeof(Unsafe.AVTreeNode));

            return node;
        }

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

        public static void Free(Fixed<Unsafe.AVTreeNode> ARoot)
        {
            // No null-check necessary.

            Unsafe.ffmpeg.av_tree_destroy(ARoot);
        }

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

            return result.IsNull || result == AKey;
        }

        public static bool RemoveItem(
            Movable<Unsafe.AVTreeNode> ARoot,
            Fixed AKey,
            [NotNull] [InstantHandle] Func<Fixed, Fixed, int> ACompare
        ) => Insert(ARoot, AKey, ACompare, null).Or(AKey) == AKey;

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
