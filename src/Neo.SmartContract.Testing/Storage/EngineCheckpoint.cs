// Copyright (C) 2015-2025 The Neo Project.
//
// EngineCheckpoint.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Extensions;
using Neo.IO;
using Neo.Persistence;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.SmartContract.Testing.Storage
{
    public class EngineCheckpoint
    {
        /// <summary>
        /// Data
        /// </summary>
        public (byte[] key, byte[] value)[] Data { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="snapshot">Snapshot</param>
        public EngineCheckpoint(DataCache snapshot)
        {
            var list = new List<(byte[], byte[])>();

            foreach (var entry in snapshot.Seek(Array.Empty<byte>(), SeekDirection.Forward))
            {
                // Store the complete storage key (including contract ID) as raw bytes
                var keyBytes = new byte[sizeof(int) + entry.Key.Key.Length];
                BinaryPrimitives.WriteInt32LittleEndian(keyBytes, entry.Key.Id);
                entry.Key.Key.CopyTo(keyBytes.AsMemory(sizeof(int)));
                
                list.Add((keyBytes, entry.Value.ToArray()));
            }

            Data = list.ToArray();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">Stream</param>
        public EngineCheckpoint(Stream stream)
        {
            var list = new List<(byte[], byte[])>();
            var buffer = new byte[sizeof(int)];

            while (stream.Read(buffer) == sizeof(int))
            {
                var length = BinaryPrimitives.ReadInt32LittleEndian(buffer);
                var key = new byte[length];

                if (stream.Read(key) != length) break;
                if (stream.Read(buffer) != sizeof(int)) break;

                length = BinaryPrimitives.ReadInt32LittleEndian(buffer);
                var data = new byte[length];

                if (stream.Read(data) != length) break;

                list.Add((key, data));
            }

            Data = list.ToArray();
        }

        /// <summary>
        /// Restore
        /// </summary>
        /// <param name="snapshot">Snapshot</param>
        public void Restore(DataCache snapshot)
        {
            // Clean snapshot

            foreach (var entry in snapshot.Seek(Array.Empty<byte>(), SeekDirection.Forward).ToArray())
            {
                snapshot.Delete(entry.Key);
            }

            // Restore

            foreach (var entry in Data)
            {
                // Reconstruct the original storage key from the raw bytes
                var contractId = BinaryPrimitives.ReadInt32LittleEndian(entry.key);
                var keyData = entry.key.AsSpan(sizeof(int)).ToArray();
                
                snapshot.Add(new StorageKey { Id = contractId, Key = keyData }, new StorageItem(entry.value));
            }
        }

        /// <summary>
        /// To Array
        /// </summary>
        /// <returns>binary data</returns>
        public byte[] ToArray()
        {
            using var ms = new MemoryStream();
            Write(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Write to Stream
        /// </summary>
        public void Write(Stream stream)
        {
            var buffer = new byte[sizeof(int)];

            foreach (var entry in Data)
            {
                BinaryPrimitives.WriteInt32LittleEndian(buffer, entry.key.Length);
                stream.Write(buffer);
                stream.Write(entry.key);

                BinaryPrimitives.WriteInt32LittleEndian(buffer, entry.value.Length);
                stream.Write(buffer);
                stream.Write(entry.value);
            }
        }
    }
}
