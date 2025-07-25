using Neo.Cryptography.ECC;
using Neo.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract.Testing;

public abstract class Contract_Foreach(Neo.SmartContract.Testing.SmartContractInitialize initialize) : Neo.SmartContract.Testing.SmartContract(initialize), IContractInfo
{
    #region Compiled data

    public static Neo.SmartContract.Manifest.ContractManifest Manifest => Neo.SmartContract.Manifest.ContractManifest.Parse(@"{""name"":""Contract_Foreach"",""groups"":[],""features"":{},""supportedstandards"":[],""abi"":{""methods"":[{""name"":""intForeach"",""parameters"":[],""returntype"":""Integer"",""offset"":0,""safe"":false},{""name"":""stringForeach"",""parameters"":[],""returntype"":""String"",""offset"":84,""safe"":false},{""name"":""byteStringEmpty"",""parameters"":[],""returntype"":""Integer"",""offset"":136,""safe"":false},{""name"":""byteStringForeach"",""parameters"":[],""returntype"":""ByteArray"",""offset"":145,""safe"":false},{""name"":""structForeach"",""parameters"":[],""returntype"":""Map"",""offset"":201,""safe"":false},{""name"":""byteArrayForeach"",""parameters"":[],""returntype"":""Array"",""offset"":297,""safe"":false},{""name"":""uInt160Foreach"",""parameters"":[],""returntype"":""Array"",""offset"":335,""safe"":false},{""name"":""uInt256Foreach"",""parameters"":[],""returntype"":""Array"",""offset"":412,""safe"":false},{""name"":""eCPointForeach"",""parameters"":[],""returntype"":""Array"",""offset"":513,""safe"":false},{""name"":""bigIntegerForeach"",""parameters"":[],""returntype"":""Array"",""offset"":642,""safe"":false},{""name"":""objectArrayForeach"",""parameters"":[],""returntype"":""Array"",""offset"":697,""safe"":false},{""name"":""intForeachBreak"",""parameters"":[{""name"":""breakIndex"",""type"":""Integer""}],""returntype"":""Integer"",""offset"":744,""safe"":false},{""name"":""testContinue"",""parameters"":[],""returntype"":""Integer"",""offset"":898,""safe"":false},{""name"":""intForloop"",""parameters"":[],""returntype"":""Integer"",""offset"":1000,""safe"":false},{""name"":""testIteratorForEach"",""parameters"":[],""returntype"":""Void"",""offset"":1127,""safe"":false},{""name"":""testForEachVariable"",""parameters"":[],""returntype"":""Void"",""offset"":1189,""safe"":false},{""name"":""testDo"",""parameters"":[],""returntype"":""Void"",""offset"":1235,""safe"":false},{""name"":""testWhile"",""parameters"":[],""returntype"":""Void"",""offset"":1306,""safe"":false}],""events"":[]},""permissions"":[{""contract"":""0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0"",""methods"":[""itoa""]}],""trusts"":[],""extra"":{""Version"":""1.0.1"",""nef"":{""optimization"":""All""}}}");

    /// <summary>
    /// Optimization: "All"
    /// </summary>
    public static Neo.SmartContract.NefFile Nef => Convert.FromBase64String(@"TkVGM1Rlc3RpbmdFbmdpbmUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHA7znO4OTpJcbCoGp54UQN2G/OrARpdG9hAQABDwAA/WMFVwYAFBMSERTAcBBxaEpyynMQdCI7amzOdWltnkoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9xbJx0bGswxWlAVwYADANoaWoMA2RlZgwDYWJjE8BwDABxaEpyynMQdCIPamzOdWlti9socWycdGxrMPFpQFcBAAwAcGjKQFcGAAwADAAMA2hpagwDZGVmDANhYmMVwHAMAHFoSnLKcxB0Ig9qbM51aW2L2yhxbJx0bGsw8WlAVwgAEAsSv3AMBXRlc3QxSmgQUdBFEUpoEVHQRRALEr9xDAV0ZXN0MkppEFHQRRJKaRFR0EVpaBLAcshzakp0ynUQdiIXbG7OdwdvBxHOSm8HEM5rU9BFbpx2bm0w6WtAVwYADAMBChHbMHDCcWhKcspzEHQiDGpsznVpbc9snHRsazD0aUBXBgAMFAAAAAAAAAAAAAAAAAAAAAAAAAAADBQAAAAAAAAAAAAAAAAAAAAAAAAAABLAcMJxaEpyynMQdCIMamzOdWltz2ycdGxrMPRpQFcGAAwgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEsBwwnFoSnLKcxB0IgxqbM51aW3PbJx0bGsw9GlAVwYADCECRwDbLpDZ8CxPn8hiq6ypJyX5W0/dzI1/+lOGk+z0Y6nbKErYJAlKygAhKAM6DCECRwDbLpDZ8CxPn8hiq6ypJyX5W0/dzI1/+lOGk+z0Y6nbKErYJAlKygAhKAM6EsBwwnFoSnLKcxB0IgxqbM51aW3PbJx0bGsw9GlAVwYAAwAAZKeztuANAgDKmjsCQEIPAAEQJxTAcMJxaEpyynMQdCIMamzOdWltz2ycdGxrMPRpQFcGAAB7DAR0ZXN0DAIBAtswE8BwwnFoSnLKcxB0IgxqbM51aW3PbJx0bGsw9GlAVwYBFBMSERTAcBBxPIkAAAAAAAAAaEpyynMQdCJzamzOdXhKnUoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ+AELYmBD1AaW2eSgIAAACALgQiCkoC////fzIeA/////8AAAAAkUoC////fzIMAwAAAAABAAAAn3FsnHRsazCNPQVyPQJpQFcGABUUExIRFcBwEHE7VABoSnLKcxB0IkRqbM51bRKiEJcmBCI0aW2eSgIAAACALgQiCkoC////fzIeA/////8AAAAAkUoC////fzIMAwAAAAABAAAAn3FsnHRsazC8PQVyPQJpQFcDABQTEhEUwHAQcRByImlpaGrOnkoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9xakqcSgIAAACALgQiCkoC////fzIeA/////8AAAAAkUoC////fzIMAwAAAAABAAAAn3JFamjKtSSVaUBXAwATQZv2Z84TEYhOEFHQUBLAwUVB3zC4mnBocSIRaUHzVL8dcmrbKEHP50eWaUGcCO2cJOtAEr8SvxLAQFcFADT2SnDKcRByIh5oas7BRXN0azcAAAwCOiCLbIvbKEHP50eWapxyamkw4kBXAQAQcGg3AABBz+dHlmhKnEoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9wRWgVtSTBQFcBABBwaBW1JkBoNwAAQc/nR5ZoSpxKAgAAAIAuBCIKSgL///9/Mh4D/////wAAAACRSgL///9/MgwDAAAAAAEAAACfcEUiv0CEj7kK").AsSerializable<Neo.SmartContract.NefFile>();

    #endregion

    #region Unsafe methods

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYAAwAAZKeztuANAgDKmjsCQEIPAAEQJxTAcMJxaEpyynMQdCIMamzOdWltz2ycdGxrMPRpQA==
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHINT64 000064A7B3B6E00D [1 datoshi]
    /// PUSHINT32 00CA9A3B [1 datoshi]
    /// PUSHINT32 40420F00 [1 datoshi]
    /// PUSHINT16 1027 [1 datoshi]
    /// PUSH4 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// NEWARRAY0 [16 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0C [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// APPEND [8192 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F4 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("bigIntegerForeach")]
    public abstract IList<object>? BigIntegerForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYADAMBChHbMHDCcWhKcspzEHQiDGpsznVpbc9snHRsazD0aUA=
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHDATA1 010A11 [8 datoshi]
    /// CONVERT 30 'Buffer' [8192 datoshi]
    /// STLOC0 [2 datoshi]
    /// NEWARRAY0 [16 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0C [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// APPEND [8192 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F4 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("byteArrayForeach")]
    public abstract IList<object>? ByteArrayForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEADABwaMpA
    /// INITSLOT 0100 [64 datoshi]
    /// PUSHDATA1 [8 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// SIZE [4 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("byteStringEmpty")]
    public abstract BigInteger? ByteStringEmpty();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYADAAMAAwDaGlqDANkZWYMA2FiYxXAcAwAcWhKcspzEHQiD2psznVpbYvbKHFsnHRsazDxaUA=
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHDATA1 [8 datoshi]
    /// PUSHDATA1 [8 datoshi]
    /// PUSHDATA1 68696A 'hij' [8 datoshi]
    /// PUSHDATA1 646566 'def' [8 datoshi]
    /// PUSHDATA1 616263 'abc' [8 datoshi]
    /// PUSH5 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSHDATA1 [8 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0F [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// CAT [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F1 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("byteStringForeach")]
    public abstract byte[]? ByteStringForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYADCECRwDbLpDZ8CxPn8hiq6ypJyX5W0/dzI1/+lOGk+z0Y6nbKErYJAlKygAhKAM6DCECRwDbLpDZ8CxPn8hiq6ypJyX5W0/dzI1/+lOGk+z0Y6nbKErYJAlKygAhKAM6EsBwwnFoSnLKcxB0IgxqbM51aW3PbJx0bGsw9GlA
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHDATA1 024700DB2E90D9F02C4F9FC862ABACA92725F95B4FDDCC8D7FFA538693ECF463A9 [8 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 09 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSHINT8 21 [1 datoshi]
    /// JMPEQ 03 [2 datoshi]
    /// THROW [512 datoshi]
    /// PUSHDATA1 024700DB2E90D9F02C4F9FC862ABACA92725F95B4FDDCC8D7FFA538693ECF463A9 [8 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 09 [2 datoshi]
    /// DUP [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSHINT8 21 [1 datoshi]
    /// JMPEQ 03 [2 datoshi]
    /// THROW [512 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// NEWARRAY0 [16 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0C [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// APPEND [8192 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F4 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("eCPointForeach")]
    public abstract IList<object>? ECPointForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYAFBMSERTAcBBxaEpyynMQdCI7amzOdWltnkoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9xbJx0bGswxWlA
    /// INITSLOT 0600 [64 datoshi]
    /// PUSH4 [1 datoshi]
    /// PUSH3 [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// PUSH4 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 3B [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// ADD [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT C5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("intForeach")]
    public abstract BigInteger? IntForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYBFBMSERTAcBBxPIkAAAAAAAAAaEpyynMQdCJzamzOdXhKnUoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ+AELYmBD1AaW2eSgIAAACALgQiCkoC////fzIeA/////8AAAAAkUoC////fzIMAwAAAAABAAAAn3FsnHRsazCNPQVyPQJpQA==
    /// INITSLOT 0601 [64 datoshi]
    /// PUSH4 [1 datoshi]
    /// PUSH3 [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// PUSH4 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC1 [2 datoshi]
    /// TRY_L 8900000000000000 [4 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 73 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// DEC [4 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STARG0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// LE [8 datoshi]
    /// JMPIFNOT 04 [2 datoshi]
    /// ENDTRY 40 [4 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// ADD [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT 8D [2 datoshi]
    /// ENDTRY 05 [4 datoshi]
    /// STLOC2 [2 datoshi]
    /// ENDTRY 02 [4 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("intForeachBreak")]
    public abstract BigInteger? IntForeachBreak(BigInteger? breakIndex);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwMAFBMSERTAcBBxEHIiaWloas6eSgIAAACALgQiCkoC////fzIeA/////8AAAAAkUoC////fzIMAwAAAAABAAAAn3FqSpxKAgAAAIAuBCIKSgL///9/Mh4D/////wAAAACRSgL///9/MgwDAAAAAAEAAACfckVqaMq1JJVpQA==
    /// INITSLOT 0300 [64 datoshi]
    /// PUSH4 [1 datoshi]
    /// PUSH3 [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// PUSH4 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC1 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC2 [2 datoshi]
    /// JMP 69 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// ADD [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// DUP [2 datoshi]
    /// INC [4 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC2 [2 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// SIZE [4 datoshi]
    /// LT [8 datoshi]
    /// JMPIF 95 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("intForloop")]
    public abstract BigInteger? IntForloop();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYAAHsMBHRlc3QMAgEC2zATwHDCcWhKcspzEHQiDGpsznVpbc9snHRsazD0aUA=
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHINT8 7B [1 datoshi]
    /// PUSHDATA1 74657374 'test' [8 datoshi]
    /// PUSHDATA1 0102 [8 datoshi]
    /// CONVERT 30 'Buffer' [8192 datoshi]
    /// PUSH3 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// NEWARRAY0 [16 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0C [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// APPEND [8192 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F4 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("objectArrayForeach")]
    public abstract IList<object>? ObjectArrayForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYADANoaWoMA2RlZgwDYWJjE8BwDABxaEpyynMQdCIPamzOdWlti9socWycdGxrMPFpQA==
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHDATA1 68696A 'hij' [8 datoshi]
    /// PUSHDATA1 646566 'def' [8 datoshi]
    /// PUSHDATA1 616263 'abc' [8 datoshi]
    /// PUSH3 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSHDATA1 [8 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0F [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// CAT [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F1 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("stringForeach")]
    public abstract string? StringForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwgAEAsSv3AMBXRlc3QxSmgQUdBFEUpoEVHQRRALEr9xDAV0ZXN0MkppEFHQRRJKaRFR0EVpaBLAcshzakp0ynUQdiIXbG7OdwdvBxHOSm8HEM5rU9BFbpx2bm0w6WtA
    /// INITSLOT 0800 [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACKSTRUCT [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSHDATA1 7465737431 'test1' [8 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACKSTRUCT [2048 datoshi]
    /// STLOC1 [2 datoshi]
    /// PUSHDATA1 7465737432 'test2' [8 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC2 [2 datoshi]
    /// NEWMAP [8 datoshi]
    /// STLOC3 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC4 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC5 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC6 [2 datoshi]
    /// JMP 17 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC6 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC 07 [2 datoshi]
    /// LDLOC 07 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC 07 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// LDLOC3 [2 datoshi]
    /// REVERSE3 [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC6 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC6 [2 datoshi]
    /// LDLOC6 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// JMPLT E9 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("structForeach")]
    public abstract IDictionary<object, object>? StructForeach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYAFRQTEhEVwHAQcTtUAGhKcspzEHQiRGpsznVtEqIQlyYEIjRpbZ5KAgAAAIAuBCIKSgL///9/Mh4D/////wAAAACRSgL///9/MgwDAAAAAAEAAACfcWycdGxrMLw9BXI9AmlA
    /// INITSLOT 0600 [64 datoshi]
    /// PUSH5 [1 datoshi]
    /// PUSH4 [1 datoshi]
    /// PUSH3 [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// PUSH5 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC1 [2 datoshi]
    /// TRY 5400 [4 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 44 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// MOD [8 datoshi]
    /// PUSH0 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// JMPIFNOT 04 [2 datoshi]
    /// JMP 34 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// ADD [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT BC [2 datoshi]
    /// ENDTRY 05 [4 datoshi]
    /// STLOC2 [2 datoshi]
    /// ENDTRY 02 [4 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("testContinue")]
    public abstract BigInteger? TestContinue();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEAEHBoNwAAQc/nR5ZoSpxKAgAAAIAuBCIKSgL///9/Mh4D/////wAAAACRSgL///9/MgwDAAAAAAEAAACfcEVoFbUkwUA=
    /// INITSLOT 0100 [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// INC [4 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC0 [2 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH5 [1 datoshi]
    /// LT [8 datoshi]
    /// JMPIF C1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("testDo")]
    public abstract void TestDo();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwUANPZKcMpxEHIiHmhqzsFFc3RrNwAADAI6IItsi9soQc/nR5ZqnHJqaTDiQA==
    /// INITSLOT 0500 [64 datoshi]
    /// CALL F6 [512 datoshi]
    /// DUP [2 datoshi]
    /// STLOC0 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC1 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC2 [2 datoshi]
    /// JMP 1E [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// UNPACK [2048 datoshi]
    /// DROP [2 datoshi]
    /// STLOC3 [2 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// PUSHDATA1 3A20 [8 datoshi]
    /// CAT [2048 datoshi]
    /// LDLOC4 [2 datoshi]
    /// CAT [2048 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC2 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC2 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// JMPLT E2 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("testForEachVariable")]
    public abstract void TestForEachVariable();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwMAE0Gb9mfOExGIThBR0FASwMFFQd8wuJpwaHEiEWlB81S/HXJq2yhBz+dHlmlBnAjtnCTrQA==
    /// INITSLOT 0300 [64 datoshi]
    /// PUSH3 [1 datoshi]
    /// SYSCALL 9BF667CE 'System.Storage.GetContext' [16 datoshi]
    /// PUSH3 [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// NEWBUFFER [256 datoshi]
    /// TUCK [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACK [2048 datoshi]
    /// UNPACK [2048 datoshi]
    /// DROP [2 datoshi]
    /// SYSCALL DF30B89A 'System.Storage.Find' [32768 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// STLOC1 [2 datoshi]
    /// JMP 11 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// SYSCALL F354BF1D 'System.Iterator.Value' [16 datoshi]
    /// STLOC2 [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC1 [2 datoshi]
    /// SYSCALL 9C08ED9C 'System.Iterator.Next' [32768 datoshi]
    /// JMPIF EB [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("testIteratorForEach")]
    public abstract void TestIteratorForEach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEAEHBoFbUmQGg3AABBz+dHlmhKnEoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9wRSK/QA==
    /// INITSLOT 0100 [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH5 [1 datoshi]
    /// LT [8 datoshi]
    /// JMPIFNOT 40 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// CALLT 0000 [32768 datoshi]
    /// SYSCALL CFE74796 'System.Runtime.Log' [32768 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// INC [4 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 00000080 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 0A [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 1E [2 datoshi]
    /// PUSHINT64 FFFFFFFF00000000 [1 datoshi]
    /// AND [8 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT32 FFFFFF7F [1 datoshi]
    /// JMPLE 0C [2 datoshi]
    /// PUSHINT64 0000000001000000 [1 datoshi]
    /// SUB [8 datoshi]
    /// STLOC0 [2 datoshi]
    /// DROP [2 datoshi]
    /// JMP BF [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("testWhile")]
    public abstract void TestWhile();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYADBQAAAAAAAAAAAAAAAAAAAAAAAAAAAwUAAAAAAAAAAAAAAAAAAAAAAAAAAASwHDCcWhKcspzEHQiDGpsznVpbc9snHRsazD0aUA=
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHDATA1 0000000000000000000000000000000000000000 [8 datoshi]
    /// PUSHDATA1 0000000000000000000000000000000000000000 [8 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// NEWARRAY0 [16 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0C [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// APPEND [8192 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F4 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("uInt160Foreach")]
    public abstract IList<object>? UInt160Foreach();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwYADCAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAwgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAASwHDCcWhKcspzEHQiDGpsznVpbc9snHRsazD0aUA=
    /// INITSLOT 0600 [64 datoshi]
    /// PUSHDATA1 0000000000000000000000000000000000000000000000000000000000000000 [8 datoshi]
    /// PUSHDATA1 0000000000000000000000000000000000000000000000000000000000000000 [8 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// NEWARRAY0 [16 datoshi]
    /// STLOC1 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// STLOC2 [2 datoshi]
    /// SIZE [4 datoshi]
    /// STLOC3 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// STLOC4 [2 datoshi]
    /// JMP 0C [2 datoshi]
    /// LDLOC2 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// STLOC5 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// LDLOC5 [2 datoshi]
    /// APPEND [8192 datoshi]
    /// LDLOC4 [2 datoshi]
    /// INC [4 datoshi]
    /// STLOC4 [2 datoshi]
    /// LDLOC4 [2 datoshi]
    /// LDLOC3 [2 datoshi]
    /// JMPLT F4 [2 datoshi]
    /// LDLOC1 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("uInt256Foreach")]
    public abstract IList<object>? UInt256Foreach();

    #endregion
}
