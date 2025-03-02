// Copyright (C) 2015-2024 The Neo Project.
//
// RefHandling.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

extern alias scfx;

using Microsoft.CodeAnalysis;
using Neo.VM;
using System.Collections.Generic;

namespace Neo.Compiler
{
    internal partial class MethodConvert
    {
        private readonly Dictionary<ISymbol, byte> _refVariables = new(SymbolEqualityComparer.Default);
        private bool _hasRefReturn;

        /// <summary>
        /// Adds a ref local variable to the method's scope.
        /// </summary>
        /// <param name="symbol">The ILocalSymbol representing the ref local variable to be added.</param>
        /// <returns>The index of the newly added ref local variable.</returns>
        private byte AddRefLocalVariable(ILocalSymbol symbol)
        {
            byte index = AddLocalVariable(symbol);
            _refVariables.Add(symbol, index);
            return index;
        }

        /// <summary>
        /// Checks if a local variable is a ref variable.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <returns>True if the variable is a ref variable, false otherwise.</returns>
        private bool IsRefVariable(ISymbol symbol)
        {
            return _refVariables.ContainsKey(symbol);
        }

        /// <summary>
        /// Loads the address of a ref variable onto the evaluation stack.
        /// </summary>
        /// <param name="symbol">The symbol of the ref variable.</param>
        /// <returns>An instruction representing the load address operation.</returns>
        private Instruction LdRefAddr(ISymbol symbol)
        {
            if (!_refVariables.TryGetValue(symbol, out byte index))
                throw new CompilationException(symbol, DiagnosticId.SyntaxNotSupported, $"Symbol {symbol} is not a ref variable.");

            // For ref variables, we need to load the address instead of the value
            // In Neo VM, we can use LDLOCA for local variables
            return AccessSlot(OpCode.LDLOCA, index);
        }

        /// <summary>
        /// Loads the value of a ref variable onto the evaluation stack.
        /// </summary>
        /// <param name="symbol">The symbol of the ref variable.</param>
        /// <returns>An instruction representing the load value operation.</returns>
        private Instruction LdRefValue(ISymbol symbol)
        {
            if (!_refVariables.TryGetValue(symbol, out byte index))
                throw new CompilationException(symbol, DiagnosticId.SyntaxNotSupported, $"Symbol {symbol} is not a ref variable.");

            // For ref variables, we need to load the value
            // In Neo VM, we can use LDLOC for local variables
            return AccessSlot(OpCode.LDLOC, index);
        }

        /// <summary>
        /// Stores a value to a ref variable.
        /// </summary>
        /// <param name="symbol">The symbol of the ref variable.</param>
        /// <returns>An instruction representing the store operation.</returns>
        private Instruction StRefValue(ISymbol symbol)
        {
            if (!_refVariables.TryGetValue(symbol, out byte index))
                throw new CompilationException(symbol, DiagnosticId.SyntaxNotSupported, $"Symbol {symbol} is not a ref variable.");

            // For ref variables, we need to store the value
            // In Neo VM, we can use STLOC for local variables
            return AccessSlot(OpCode.STLOC, index);
        }

        /// <summary>
        /// Sets the method as having a ref return.
        /// </summary>
        private void SetRefReturn()
        {
            _hasRefReturn = true;
        }

        /// <summary>
        /// Checks if the method has a ref return.
        /// </summary>
        /// <returns>True if the method has a ref return, false otherwise.</returns>
        private bool HasRefReturn()
        {
            return _hasRefReturn;
        }
    }
}
