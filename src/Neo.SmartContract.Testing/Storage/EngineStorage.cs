// Copyright (C) 2015-2025 The Neo Project.
//
// EngineStorage.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Json;
using Neo.Persistence;
using System;
using System.Buffers.Binary;
using System.Linq;

namespace Neo.SmartContract.Testing.Storage
{
    /// <summary>
    /// TestStorage centralizes the storage management of our TestEngine
    /// </summary>
    public class EngineStorage
    {
        // Key to check if native contracts are initialized, by default: ContractManagement.Prefix_NextAvailableId
        private static readonly StorageKey _initKey = new() { Id = Neo.SmartContract.Native.NativeContract.ContractManagement.Id, Key = new byte[] { 15 } };

        /// <summary>
        /// Store
        /// </summary>
        public IStore Store { get; }

        /// <summary>
        /// Snapshot
        /// </summary>
        public DataCache Snapshot { get; private set; }

        /// <summary>
        /// Return true if native contract are initialized
        /// </summary>
        public bool IsInitialized => Snapshot.Contains(_initKey);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="store">Store</param>
        public EngineStorage(IStore store) : this(store, new StoreCache(store.GetSnapshot())) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="store">Store</param>
        /// <param name="snapshotCache">Snapshot cache</param>
        internal EngineStorage(IStore store, DataCache snapshotCache)
        {
            Store = store;
            Snapshot = snapshotCache;
        }

        /// <summary>
        /// Commit
        /// </summary>
        public void Commit()
        {
            Snapshot.Commit();
        }

        /// <summary>
        /// Rollback
        /// </summary>
        public void Rollback()
        {
            if (Snapshot is IDisposable sp)
            {
                sp.Dispose();
            }
            Snapshot = new StoreCache(Store.GetSnapshot());
        }

        /// <summary>
        /// Get storage checkpoint
        /// </summary>
        /// <returns>EngineCheckpoint</returns>
        public EngineCheckpoint Checkpoint() => new(Snapshot);

        /// <summary>
        /// Restore
        /// </summary>
        /// <param name="checkpoint">Checkpoint</param>
        public void Restore(EngineCheckpoint checkpoint)
        {
            checkpoint.Restore(Snapshot);
        }

        /// <summary>
        /// Import data from json, expected data (in base64):
        /// - "key"     : "value"
        /// - "prefix"  : { "key":"value" }
        /// </summary>
        /// <param name="json">Json Object</param>
        public void Import(string json)
        {
            if (JToken.Parse(json) is not JObject jo)
            {
                throw new FormatException("The json is not a valid JObject");
            }

            Import(jo);
        }

        /// <summary>
        /// Import data from json, expected data (in base64):
        /// - "key"     : "value"
        /// - "prefix"  : { "key":"value" }
        /// </summary>
        /// <param name="json">Json Object</param>
        public void Import(JObject json)
        {
            foreach (var entry in json.Properties)
            {
                if (entry.Value is JString str)
                {
                    // "key":"value" in base64

                    Store.Put(Convert.FromBase64String(entry.Key), Convert.FromBase64String(str.Value));
                }
                else if (entry.Value is JObject obj)
                {
                    // "prefix": { "key":"value" }  in base64

                    byte[] prefix = Convert.FromBase64String(entry.Key);

                    foreach (var subEntry in obj.Properties)
                    {
                        if (subEntry.Value is JString subStr)
                        {
                            var keyBytes = prefix.Concat(Convert.FromBase64String(subEntry.Key)).ToArray();
                            Store.Put(keyBytes, Convert.FromBase64String(subStr.Value));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Export data to json
        /// </summary>
        public JObject Export()
        {
            JObject ret = new();

            foreach (var entry in Store.Find([], SeekDirection.Forward))
            {
                // Export raw key-value pairs from the store
                // "key":"value" in base64
                ret[Convert.ToBase64String(entry.Key)] = Convert.ToBase64String(entry.Value);
            }

            return ret;
        }
    }
}
