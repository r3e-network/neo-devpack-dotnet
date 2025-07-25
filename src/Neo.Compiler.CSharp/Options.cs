// Copyright (C) 2015-2025 The Neo Project.
//
// Options.cs file belongs to the neo project and is free
// software distributed under the MIT software license, see the
// accompanying file LICENSE in the main directory of the
// repository or http://www.opensource.org/licenses/mit-license.php
// for more details.
//
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using System;

namespace Neo.Compiler
{
    public class Options : CompilationOptions
    {
        [Flags]
        public enum GenerateArtifactsKind : byte
        {
            None = 0,
            Source = 1,
            Library = 2,

            All = Source | Library
        }

        public string? Output { get; set; }
        public bool Assembly { get; set; }
        public GenerateArtifactsKind GenerateArtifacts { get; set; } = GenerateArtifactsKind.None;
        public bool SecurityAnalysis { get; set; } = false;
        public bool GenerateContractInterface { get; set; } = false;
        public bool GeneratePlugin { get; set; } = false;
        public bool GenerateWebGui { get; set; } = false;
        public bool DeployWebGui { get; set; } = false;
        public string? WebGuiServiceUrl { get; set; }
        public string? ContractAddress { get; set; }
        public string? Network { get; set; } = "testnet";
        public string? DeployerAddress { get; set; }
    }
}
