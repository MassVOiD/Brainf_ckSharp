﻿using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Brainf_ckSharp.Git.Enums;
using Brainf_ckSharp.Git.Models;
using Microsoft.Collections.Extensions;

namespace Brainf_ckSharp.Git
{
    /// <summary>
    /// A <see langword="class"/> that implements the Paul Heckel git diff algorithm for line diffs
    /// </summary>
    public static class LineDiffer
    {
        /// <summary>
        /// The reusable <see cref="DictionarySlim{TKey,TValue}"/>
        /// </summary>
        private static readonly DictionarySlim<int, DiffEntry> LinesMap = new DictionarySlim<int, DiffEntry>();

        /// <summary>
        /// Computes the line difference for a reference text and a new text
        /// </summary>
        /// <param name="oldText">The reference text to compare to</param>
        /// <param name="newText">The updated text to compare</param>
        /// <param name="separator">The separator character to use to split lines in <paramref name="oldText"/> and <paramref name="newText"/></param>
        /// <returns>A <see cref="MemoryOwner{T}"/> instance with the sequence of line modifications</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryOwner<LineModificationType> ComputeDiff(string oldText, string newText, char separator)
        {
            return ComputeDiff(oldText.AsSpan(), newText.AsSpan(), separator);
        }

        /// <summary>
        /// Computes the line difference for a reference text and a new text
        /// </summary>
        /// <param name="oldText">The reference text to compare to</param>
        /// <param name="newText">The updated text to compare</param>
        /// <param name="separator">The separator character to use to split lines in <paramref name="oldText"/> and <paramref name="newText"/></param>
        /// <returns>A <see cref="MemoryOwner{T}"/> instance with the sequence of line modifications</returns>
        [Pure]
        public static MemoryOwner<LineModificationType> ComputeDiff(ReadOnlySpan<char> oldText, ReadOnlySpan<char> newText, char separator)
        {
            // If the new text is empty, no modifications are returned
            if (newText.IsEmpty) return MemoryOwner<LineModificationType>.Allocate(0);

            int TextNumberOfLines = oldText.Count(separator) + 1;
            int newNumberOfLines = newText.Count(separator) + 1;

            object[] oldTemporaryValues = ArrayPool<object>.Shared.Rent(TextNumberOfLines);
            object[] newTemporaryValues = ArrayPool<object>.Shared.Rent(newNumberOfLines);

            DictionarySlim<int, DiffEntry> table = LinesMap;
            table.Clear();

            Pool<DiffEntry>.Reset();

            try
            {
                /* ==============
                 * First pass
                 * ==============
                 * Iterate over all the lines in the new text file.
                 * For each line, create the table entry if not present,
                 * otherwise increment the line counter. Also set the
                 * values in the temporary arrays to the table entires. */
                int i = 0;
                foreach (ReadOnlySpan<char> line in newText.Tokenize(separator))
                {
                    int hash = line.GetxxHash32Code();
                    ref DiffEntry entry = ref table.GetOrAddValueRef(hash);

                    if (entry is null)
                    {
                        entry = Pool<DiffEntry>.Rent();
                        entry.NumberOfOccurrencesInNewText = 1;
                        entry.NumberOfOccurrencesInOldText = 0;
                        entry.LineNumberInOldText = 0;
                    }
                    else
                    {
                        if (entry.NumberOfOccurrencesInNewText == 1) entry.NumberOfOccurrencesInNewText = 2;
                        else entry.NumberOfOccurrencesInNewText = int.MaxValue;
                    }

                    newTemporaryValues[i] = entry;
                    i += 1;
                }

                /* ==============
                 * Second pass
                 * ==============
                 * Same as the first pass, but acting on the old text,
                 * and the associated temporary values and table entry fields. */
                int j = 0;
                foreach (ReadOnlySpan<char> line in oldText.Tokenize(separator))
                {
                    int hash = line.GetxxHash32Code();
                    ref DiffEntry entry = ref table.GetOrAddValueRef(hash);

                    if (entry is null)
                    {
                        entry = Pool<DiffEntry>.Rent();
                        entry.NumberOfOccurrencesInNewText = 0;
                        entry.NumberOfOccurrencesInOldText = 1;
                        entry.LineNumberInOldText = 0;
                    }
                    else
                    {
                        if (entry.NumberOfOccurrencesInOldText == 0) entry.NumberOfOccurrencesInOldText = 1;
                        else if (entry.NumberOfOccurrencesInOldText == 1) entry.NumberOfOccurrencesInOldText = 2;
                        else entry.NumberOfOccurrencesInOldText = int.MaxValue;
                    }

                    entry.LineNumberInOldText = j;
                    oldTemporaryValues[j] = entry;
                    j += 1;
                }

                /* ==============
                 * Third pass
                 * ==============
                 * If a line exactly only once in both files, it means it's the same
                 * line, although it might have been moved to a different location.
                 * These are the only affected lines in this pass. */
                i = 0;
                for (; i < newNumberOfLines; i++)
                {
                    if (newTemporaryValues[i] is DiffEntry entry &&
                        entry.NumberOfOccurrencesInOldText == 1 &&
                        entry.NumberOfOccurrencesInNewText == 1)
                    {
                        int olno = entry.LineNumberInOldText;
                        newTemporaryValues[i] = olno;
                        oldTemporaryValues[olno] = i;
                    }
                }

                /* ==============
                 * Fourth pass
                 * ==============
                 * If a line doesn't have any changes, and the lines immediately
                 * adjacent to it in both files are identical, this means the
                 * line is the same line as well. This can be used to find
                 * blocks of unchanged lines across the two text versions. */
                for (i = 0; i < newNumberOfLines - 1; i++)
                {
                    if (newTemporaryValues[i] is int k &&
                        k + 1 < TextNumberOfLines &&
                        newTemporaryValues[i + 1].Equals(oldTemporaryValues[k + 1]))
                    {
                        newTemporaryValues[i + 1] = k + 1;
                        oldTemporaryValues[k + 1] = i + 1;
                    }
                }

                /* ==============
                 * Fifth pass
                 * ==============
                 * Sames as the previous step, but acting in descending order. */
                for (i = newNumberOfLines - 1; i > 0; i--)
                {
                    if (newTemporaryValues[i] is int k &&
                        k - 1 >= 0 &&
                        newTemporaryValues[i - 1].Equals(oldTemporaryValues[k - 1]))
                    {
                        newTemporaryValues[i - 1] = k - 1;
                        oldTemporaryValues[k - 1] = i - 1;
                    }
                }

                // Allocate the result array with on entry per line in the updated text
                MemoryOwner<LineModificationType> result = MemoryOwner<LineModificationType>.Allocate(newNumberOfLines);
                ref LineModificationType resultRef = ref result.GetReference();

                /* ==============
                 * Final pass
                 * ==============
                 * Each entry in the result array is set by reading data from the
                 * temporary values for the new text. If an entry is an int it
                 * means that that line was present in the old file too and in the
                 * same location. Otherwise, if a table entry is present,
                 * it means that the current line has been modified in some way. */
                for (i = 0; i < newNumberOfLines; i++)
                {
                    if (newTemporaryValues[i] is int) Unsafe.Add(ref resultRef, i) = LineModificationType.None;
                    else Unsafe.Add(ref resultRef, i) = LineModificationType.Modified;
                }

                return result;
            }
            finally
            {
                ArrayPool<object>.Shared.Return(oldTemporaryValues);
                ArrayPool<object>.Shared.Return(newTemporaryValues);
            }
        }
    }
}