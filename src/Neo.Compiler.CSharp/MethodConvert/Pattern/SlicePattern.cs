// Copyright (C) 2015-2024 The Neo Project.
//
// SlicePattern.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

extern alias scfx;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Neo.VM;

namespace Neo.Compiler;

internal partial class MethodConvert
{
    /// <summary>
    /// Converts a slice pattern to OpCodes.
    /// </summary>
    /// <param name="model">The semantic model.</param>
    /// <param name="pattern">The slice pattern syntax.</param>
    /// <param name="localIndex">The local variable index.</param>
    /// <remarks>
    /// Slice patterns (..) are used in list patterns to match any number of elements.
    /// This implementation always returns true since a slice pattern matches any sequence.
    /// </remarks>
    /// <example>
    /// <code>
    /// int[] numbers = [1, 2, 3, 4, 5];
    /// if (numbers is [1, 2, ..]) { } // Matches arrays starting with 1, 2
    /// if (numbers is [.., 4, 5]) { } // Matches arrays ending with 4, 5
    /// if (numbers is [1, .., 5]) { } // Matches arrays starting with 1 and ending with 5
    /// </code>
    /// </example>
    private void ConvertSlicePattern(SemanticModel model, SlicePatternSyntax pattern, byte localIndex)
    {
        // Slice patterns always match, so we just push true
        Push(true);
    }
}
