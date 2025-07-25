using Neo.Cryptography.ECC;
using Neo.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract.Testing;

public abstract class Contract_IndexOrRange(Neo.SmartContract.Testing.SmartContractInitialize initialize) : Neo.SmartContract.Testing.SmartContract(initialize), IContractInfo
{
    #region Compiled data

    public static Neo.SmartContract.Manifest.ContractManifest Manifest => Neo.SmartContract.Manifest.ContractManifest.Parse(@"{""name"":""Contract_IndexOrRange"",""groups"":[],""features"":{},""supportedstandards"":[],""abi"":{""methods"":[{""name"":""testMain"",""parameters"":[],""returntype"":""Void"",""offset"":0,""safe"":false}],""events"":[]},""permissions"":[{""contract"":""0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0"",""methods"":[""itoa""]}],""trusts"":[],""extra"":{""Version"":""1.0.1"",""nef"":{""optimization"":""All""}}}");

    /// <summary>
    /// Optimization: "All"
    /// </summary>
    public static Neo.SmartContract.NefFile Nef => Convert.FromBase64String(@"TkVGM1Rlc3RpbmdFbmdpbmUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHA7znO4OTpJcbCoGp54UQN2G/OrARpdG9hAQABDwAA/a4BVxQADAoBAgMEBQYHCAkK2zBwaErKUBBRS5+McWgTUBBRS5+McmhKylASUUufjHNoFVATUUufjHRoSspQSsqdnVFLn4x1aErKE59QEFFLn4x2aErKFJ9QE1FLn4x3B2hKyp2dUErKFJ9RS5+MdwhoEM53CWnKNwAAQc/nR5ZqyjcAAEHP50eWa8o3AABBz+dHlmzKNwAAQc/nR5ZtyjcAAEHP50eWbso3AABBz+dHlm8HyjcAAEHP50eWbwjKNwAAQc/nR5ZvCTcAAEHP50eWDAkxMjM0NTY3ODl3Cm8KSspQEFFLn4zbKHcLbwoTUBBRS5+M2yh3DG8KSspQElFLn4zbKHcNbwoVUBNRS5+M2yh3Dm8KSspQSsqdnVFLn4zbKHcPbwpKyhOfUBBRS5+M2yh3EG8KSsoUn1ATUUufjNsodxFvCkrKnZ1QSsoUn1FLn4zbKHcSbwoQzncTbwvbKEHP50eWbwzbKEHP50eWbw3bKEHP50eWbw7bKEHP50eWbw/bKEHP50eWbxDbKEHP50eWbxHbKEHP50eWbxLbKEHP50eWbxPbKEHP50eWQKCEoxg=").AsSerializable<Neo.SmartContract.NefFile>();

    #endregion

    #region Unsafe methods

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VxQADAoBAgMEBQYHCAkK2zBwaErKUBBRS5+McWgTUBBRS5+McmhKylASUUufjHNoFVATUUufjHRoSspQSsqdnVFLn4x1aErKE59QEFFLn4x2aErKFJ9QE1FLn4x3B2hKyp2dUErKFJ9RS5+MdwhoEM53CWnKNwAAQc/nR5ZqyjcAAEHP50eWa8o3AABBz+dHlmzKNwAAQc/nR5ZtyjcAAEHP50eWbso3AABBz+dHlm8HyjcAAEHP50eWbwjKNwAAQc/nR5ZvCTcAAEHP50eWDAkxMjM0NTY3ODl3Cm8KSspQEFFLn4zbKHcLbwoTUBBRS5+M2yh3DG8KSspQElFLn4zbKHcNbwoVUBNRS5+M2yh3Dm8KSspQSsqdnVFLn4zbKHcPbwpKyhOfUBBRS5+M2yh3EG8KSsoUn1ATUUufjNsodxFvCkrKnZ1QSsoUn1FLn4zbKHcSbwoQzncTbwvbKEHP50eWbwzbKEHP50eWbw3bKEHP50eWbw7bKEHP50eWbw/bKEHP50eWbxDbKEHP50eWbxHbKEHP50eWbxLbKEHP50eWbxPbKEHP50eWQA==
    /// INITSLOT 1400 [64 datoshi]
    /// PUSHDATA1 0102030405060708090A [8 datoshi]
    /// CONVERT 30 'Buffer' [8192 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH3 [1 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC2 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC3 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH5 [1 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH3 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// SWAP [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// DEC [4 datoshi]
    /// DEC [4 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH3 [1 datoshi]
    /// SUB [8 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC6 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH4 [1 datoshi]
    /// SUB [8 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH3 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC 07 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// DEC [4 datoshi]
    /// DEC [4 datoshi]
    /// SWAP [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH4 [1 datoshi]
    /// SUB [8 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC 08 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC 09 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC3 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC4 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC5 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC6 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 07 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 08 [2 datoshi]
    /// SIZE [4 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 09 [2 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// PUSHDATA1 313233343536373839 '123456789' [8 datoshi]
    /// STLOC 0A [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 0B [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// PUSH3 [1 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 0C [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 0D [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// PUSH5 [1 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH3 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 0E [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// SWAP [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// DEC [4 datoshi]
    /// DEC [4 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 0F [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH3 [1 datoshi]
    /// SUB [8 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 10 [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH4 [1 datoshi]
    /// SUB [8 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH3 [1 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 11 [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// DEC [4 datoshi]
    /// DEC [4 datoshi]
    /// SWAP [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH4 [1 datoshi]
    /// SUB [8 datoshi]
    /// ROT [2 datoshi]
    /// OVER [2 datoshi]
    /// SUB [8 datoshi]
    /// SUBSTR [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC 12 [2 datoshi]
    /// LDLOC 0A [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC 13 [2 datoshi]
    /// LDLOC 0B [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 0C [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 0D [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 0E [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 0F [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 10 [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 11 [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 12 [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC 13 [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("testMain")]
    public abstract void TestMain();

    #endregion
}
