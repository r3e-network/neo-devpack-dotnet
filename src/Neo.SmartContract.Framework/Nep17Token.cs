// Copyright (C) 2015-2025 The Neo Project.
//
// Nep17Token.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract.Framework
{
    [SupportedStandards(NepStandard.Nep17)]
    [ContractPermission(Permission.Any, Method.OnNEP17Payment)]
    public abstract class Nep17Token : TokenContract
    {
        public delegate void OnTransferDelegate(UInt160? from, UInt160? to, BigInteger amount);

        [DisplayName("Transfer")]
        public static event OnTransferDelegate OnTransfer = null!;

        public static bool Transfer(UInt160 from, UInt160 to, BigInteger amount, object data)
        {
            if (!from.IsValid)
                throw new Exception("The argument \"from\" is invalid.");
            if (to is null || !to.IsValid)
                throw new Exception("The argument \"to\" is invalid.");
            if (amount < 0)
                throw new Exception("The amount must be a positive number.");
            if (!Runtime.CheckWitness(from)) return false;
            if (amount != 0)
            {
                if (!UpdateBalance(from, -amount))
                    return false;
                UpdateBalance(to, +amount);
            }
            PostTransfer(from, to, amount, data);
            return true;
        }

        protected static void Mint(UInt160 account, BigInteger amount)
        {
            if (amount.Sign < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount.IsZero) return;
            UpdateBalance(account, +amount);
            TotalSupply += amount;
            PostTransfer(null, account, amount, null);
        }

        protected static void Burn(UInt160 account, BigInteger amount)
        {
            if (amount.Sign < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount.IsZero) return;
            if (!UpdateBalance(account, -amount))
                throw new InvalidOperationException();
            TotalSupply -= amount;
            PostTransfer(account, null, amount, null);
        }

        protected static void PostTransfer(UInt160? from, UInt160? to, BigInteger amount, object? data)
        {
            OnTransfer(from, to, amount);
            if (to is not null && ContractManagement.GetContract(to) is not null)
                Contract.Call(to, Method.OnNEP17Payment, CallFlags.All, from, amount, data);
        }
    }
}
