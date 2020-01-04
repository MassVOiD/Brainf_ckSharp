﻿using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Brainf_ckSharp.Extensions.Types;

namespace Brainf_ckSharp.Buffers
{
    /// <summary>
    /// A <see langword="class"/> with a collection of extension methods for the <see cref="UnsafeMemory{T}"/> type
    /// </summary>
    internal static class UnsafeMemoryExtensions
    {
        /// <summary>
        /// Gets a new <see cref="UnsafeMemory{T}"/> instance representing a view over the current buffer
        /// </summary>
        /// <param name="memory">The source <see cref="UnsafeMemory{T}"/> instance</param>
        /// <param name="range">The <see cref="Range"/> instance indicating how to slice the current buffer</param>
        /// <returns>A new <see cref="UnsafeMemory{T}"/> instance mapping values in the [start, end) range on the current buffer</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnsafeMemory<T> Slice<T>(in this UnsafeMemory<T> memory, in Range range) where T : unmanaged
        {
            DebugGuard.MustBeLessThan(range.Start, memory.Size, nameof(range));
            DebugGuard.MustBeLessThanOrEqualTo(range.End, memory.Size, nameof(range));

            return memory.Slice(range.Start, range.End);
        }
    }
}
