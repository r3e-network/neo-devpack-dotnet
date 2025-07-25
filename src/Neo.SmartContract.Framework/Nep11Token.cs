// Copyright (C) 2015-2025 The Neo Project.
//
// Nep11Token.cs file belongs to the neo project and is free
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
    [SupportedStandards(NepStandard.Nep11)]
    [ContractPermission(Permission.Any, Method.OnNEP11Payment)]
    public abstract class Nep11Token<TokenState> : TokenContract
        where TokenState : Nep11TokenState
    {
        public delegate void OnTransferDelegate(UInt160? from, UInt160? to, BigInteger amount, ByteString tokenId);

        [DisplayName("Transfer")]
        public static event OnTransferDelegate OnTransfer = null!;

        protected const byte Prefix_TokenId = 0x02;
        protected const byte Prefix_Token = 0x03;
        protected const byte Prefix_AccountToken = 0x04;

        [Safe]
        public sealed override byte Decimals => 0;

        [Safe]
        public static UInt160 OwnerOf(ByteString tokenId)
        {
            if (tokenId.Length > 64) throw new Exception("The argument \"tokenId\" should be 64 or less bytes long.");
            var tokenMap = new StorageMap(Prefix_Token);
            var tokenKey = tokenMap[tokenId] ?? throw new Exception("The token with given \"tokenId\" does not exist.");
            TokenState token = (TokenState)StdLib.Deserialize(tokenKey);
            return token.Owner;
        }

        [Safe]
        public virtual Map<string, object> Properties(ByteString tokenId)
        {
            var tokenMap = new StorageMap(Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]!);
            return new Map<string, object>()
            {
                ["name"] = token.Name
            };
        }

        [Safe]
        public static Iterator Tokens()
        {
            var tokenMap = new StorageMap(Prefix_Token);
            return tokenMap.Find(FindOptions.KeysOnly | FindOptions.RemovePrefix);
        }

        [Safe]
        public static Iterator TokensOf(UInt160 owner)
        {
            if (!owner.IsValid)
                throw new Exception("The argument \"owner\" is invalid");
            var accountMap = new StorageMap(Prefix_AccountToken);
            return accountMap.Find(owner, FindOptions.KeysOnly | FindOptions.RemovePrefix);
        }

        public static bool Transfer(UInt160 to, ByteString tokenId, object data)
        {
            if (to is null || !to.IsValid)
                throw new Exception("The argument \"to\" is invalid.");
            var tokenMap = new StorageMap(Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]!);
            UInt160 from = token.Owner;
            if (!Runtime.CheckWitness(from)) return false;
            if (from != to)
            {
                token.Owner = to;
                tokenMap[tokenId] = StdLib.Serialize(token);
                UpdateBalance(from, tokenId, -1);
                UpdateBalance(to, tokenId, +1);
            }
            PostTransfer(from, to, tokenId, data);
            return true;
        }

        protected static ByteString NewTokenId()
        {
            return NewTokenId(Runtime.ExecutingScriptHash);
        }

        protected static ByteString NewTokenId(ByteString salt)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_TokenId };
            ByteString? id = Storage.Get(context, key);
            Storage.Put(context, key, (BigInteger)(id ?? ByteString.Empty) + 1);
            if (id is not null) salt += id;
            return CryptoLib.Sha256(salt);
        }

        protected static void Mint(ByteString tokenId, TokenState token)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            tokenMap[tokenId] = StdLib.Serialize(token);
            UpdateBalance(token.Owner, tokenId, +1);
            TotalSupply++;
            PostTransfer(null, token.Owner, tokenId, null);
        }

        protected static void Burn(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]!);
            tokenMap.Delete(tokenId);
            UpdateBalance(token.Owner, tokenId, -1);
            TotalSupply--;
            PostTransfer(token.Owner, null, tokenId, null);
        }

        protected static void UpdateBalance(UInt160 owner, ByteString tokenId, int increment)
        {
            UpdateBalance(owner, increment);
            StorageMap accountMap = new(Storage.CurrentContext, Prefix_AccountToken);
            ByteString key = owner + tokenId;
            if (increment > 0)
                accountMap.Put(key, 0);
            else
                accountMap.Delete(key);
        }

        protected static void PostTransfer(UInt160? from, UInt160? to, ByteString tokenId, object? data)
        {
            OnTransfer(from, to, 1, tokenId);
            if (to is not null && ContractManagement.GetContract(to) is not null)
                Contract.Call(to, "onNEP11Payment", CallFlags.All, from, 1, tokenId, data);
        }
    }
}
