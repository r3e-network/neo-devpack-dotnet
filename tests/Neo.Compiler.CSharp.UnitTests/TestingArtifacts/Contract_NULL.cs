using Neo.Cryptography.ECC;
using Neo.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract.Testing;

public abstract class Contract_NULL(Neo.SmartContract.Testing.SmartContractInitialize initialize) : Neo.SmartContract.Testing.SmartContract(initialize), IContractInfo
{
    #region Compiled data

    public static Neo.SmartContract.Manifest.ContractManifest Manifest => Neo.SmartContract.Manifest.ContractManifest.Parse(@"{""name"":""Contract_NULL"",""groups"":[],""features"":{},""supportedstandards"":[],""abi"":{""methods"":[{""name"":""isNull"",""parameters"":[{""name"":""value"",""type"":""Any""}],""returntype"":""Boolean"",""offset"":0,""safe"":false},{""name"":""equalNullA"",""parameters"":[{""name"":""value"",""type"":""Any""}],""returntype"":""Boolean"",""offset"":8,""safe"":false},{""name"":""equalNullB"",""parameters"":[{""name"":""value"",""type"":""Any""}],""returntype"":""Boolean"",""offset"":15,""safe"":false},{""name"":""equalNotNullA"",""parameters"":[{""name"":""value"",""type"":""Any""}],""returntype"":""Boolean"",""offset"":21,""safe"":false},{""name"":""equalNotNullB"",""parameters"":[{""name"":""value"",""type"":""Any""}],""returntype"":""Boolean"",""offset"":28,""safe"":false},{""name"":""nullCoalescing"",""parameters"":[{""name"":""code"",""type"":""String""}],""returntype"":""String"",""offset"":35,""safe"":false},{""name"":""nullCollation"",""parameters"":[{""name"":""code"",""type"":""String""}],""returntype"":""String"",""offset"":49,""safe"":false},{""name"":""nullPropertyGT"",""parameters"":[{""name"":""a"",""type"":""String""}],""returntype"":""Boolean"",""offset"":68,""safe"":false},{""name"":""nullPropertyLT"",""parameters"":[{""name"":""a"",""type"":""String""}],""returntype"":""Boolean"",""offset"":80,""safe"":false},{""name"":""nullPropertyGE"",""parameters"":[{""name"":""a"",""type"":""String""}],""returntype"":""Boolean"",""offset"":92,""safe"":false},{""name"":""nullPropertyLE"",""parameters"":[{""name"":""a"",""type"":""String""}],""returntype"":""Boolean"",""offset"":104,""safe"":false},{""name"":""nullProperty"",""parameters"":[{""name"":""a"",""type"":""String""}],""returntype"":""Boolean"",""offset"":116,""safe"":false},{""name"":""ifNull"",""parameters"":[{""name"":""obj"",""type"":""Any""}],""returntype"":""Boolean"",""offset"":128,""safe"":false},{""name"":""nullCollationAndCollation"",""parameters"":[{""name"":""code"",""type"":""String""}],""returntype"":""Any"",""offset"":138,""safe"":false},{""name"":""nullCollationAndCollation2"",""parameters"":[{""name"":""code"",""type"":""String""}],""returntype"":""Any"",""offset"":167,""safe"":false},{""name"":""nullType"",""parameters"":[],""returntype"":""Void"",""offset"":208,""safe"":false},{""name"":""nullCoalescingAssignment"",""parameters"":[{""name"":""nullableArg"",""type"":""Integer""}],""returntype"":""Void"",""offset"":221,""safe"":false},{""name"":""staticNullableCoalesceAssignment"",""parameters"":[],""returntype"":""Void"",""offset"":486,""safe"":false},{""name"":""_initialize"",""parameters"":[],""returntype"":""Void"",""offset"":653,""safe"":false}],""events"":[]},""permissions"":[],""trusts"":[],""extra"":{""Version"":""1.0.1"",""nef"":{""optimization"":""All""}}}");

    /// <summary>
    /// Optimization: "All"
    /// </summary>
    public static Neo.SmartContract.NefFile Nef => Convert.FromBase64String(@"TkVGM1Rlc3RpbmdFbmdpbmUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP2UAlcBAXhwaNhAVwABC3iXQFcAAXjYQFcAAQt4mEBXAAF42KpAVwEBeErYJAUREoxwaEBXAQF4StgmCkUMBWxpbnV4cGhAVwABeErYJAPKELdAVwABeErYJAPKELVAVwABeErYJAPKELhAVwABeErYJAPKELZAVwABeErYJAPKEJhAVwABeCYECEAJQFcBAUGb9mfOcHhoQZJd6DFK2CYKRQwBe9sw2yhAVwEBQZv2Z85wDAMxMTF4aEHmPxiEeGhBkl3oMUrYJgpFDAF72zDbKEBXAQALcGhK2CQDQEVAVwEBeNgkBXgiBRFKgEULeBLAcGgQzhGXOQtKaBBR0EVoEc7YOWhKEc5K2CQFRiIRRWgQzhMSCxAVwE5QEVHQRQsRwGg1nwAAAEVoEc4Qzmg1pgAAAEVoEM4QlzloEEtLztgkBc4iC2gRzhHOSlRT0EVoEM4QlzloEc4RS0vO2CQFziIHEUpUU9BFaBHOEc4RlzloEc4RS0vO2CQFziIHEkpUU9BFaBHOEc4RlzloEc4RS0vOSlRTnEoQLgQiCEoB/wAyBgH/AJHQRWgRzhHOEpc5CxHASmgRUdBFaBHOEM7YOUBXAAJ4Ec5K2CYKRXh5TlARUdBAVwACeBBLS87YJATOQHlKVFPQQFjYJAVYIgULSmBFWAiXOQtgWNgkBVgiBQlKYAmXOVlK2CYGRQtKYUVZEJc5C2FZStgmBkURSmERlzlZSpxKAgAAAIAuBCIKSgL///9/Mh4D/////wAAAACRSgL///9/MgwDAAAAAAEAAACfYUVZEpc5WZ1KAgAAAIAuBCIKSgL///9/Mh4D/////wAAAACRSgL///9/MgwDAAAAAAEAAACfYVkRlzlAVgIIYBBhQBH1/Ns=").AsSerializable<Neo.SmartContract.NefFile>();

    #endregion

    #region Unsafe methods

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABC3iYQA==
    /// INITSLOT 0001 [64 datoshi]
    /// PUSHNULL [1 datoshi]
    /// LDARG0 [2 datoshi]
    /// NOTEQUAL [32 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("equalNotNullA")]
    public abstract bool? EqualNotNullA(object? value = null);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeNiqQA==
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// ISNULL [2 datoshi]
    /// NOT [4 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("equalNotNullB")]
    public abstract bool? EqualNotNullB(object? value = null);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABC3iXQA==
    /// INITSLOT 0001 [64 datoshi]
    /// PUSHNULL [1 datoshi]
    /// LDARG0 [2 datoshi]
    /// EQUAL [32 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("equalNullA")]
    public abstract bool? EqualNullA(object? value = null);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeNhA
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// ISNULL [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("equalNullB")]
    public abstract bool? EqualNullB(object? value = null);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeCYECEAJQA==
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// JMPIFNOT 04 [2 datoshi]
    /// PUSHT [1 datoshi]
    /// RET [0 datoshi]
    /// PUSHF [1 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("ifNull")]
    public abstract bool? IfNull(object? obj = null);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEBeHBo2EA=
    /// INITSLOT 0101 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// ISNULL [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("isNull")]
    public abstract bool? IsNull(object? value = null);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEBeErYJAUREoxwaEA=
    /// INITSLOT 0101 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// SUBSTR [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullCoalescing")]
    public abstract string? NullCoalescing(string? code);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEBeNgkBXgiBRFKgEULeBLAcGgQzhGXOQtKaBBR0EVoEc7YOWhKEc5K2CQFRiIRRWgQzhMSCxAVwE5QEVHQRQsRwGg1nwAAAEVoEc4Qzmg1pgAAAEVoEM4QlzloEEtLztgkBc4iC2gRzhHOSlRT0EVoEM4QlzloEc4RS0vO2CQFziIHEUpUU9BFaBHOEc4RlzloEc4RS0vO2CQFziIHEkpUU9BFaBHOEc4RlzloEc4RS0vOSlRTnEoQLgQiCEoB/wAyBgH/AJHQRWgRzhHOEpc5CxHASmgRUdBFaBHOEM7YOUA=
    /// INITSLOT 0101 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// LDARG0 [2 datoshi]
    /// JMP 05 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// DUP [2 datoshi]
    /// STARG0 [2 datoshi]
    /// DROP [2 datoshi]
    /// PUSHNULL [1 datoshi]
    /// LDARG0 [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// PACK [2048 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// ISNULL [2 datoshi]
    /// ASSERT [1 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// NIP [2 datoshi]
    /// JMP 11 [2 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH3 [1 datoshi]
    /// PUSH2 [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// PUSH0 [1 datoshi]
    /// PUSH5 [1 datoshi]
    /// PACK [2048 datoshi]
    /// TUCK [2 datoshi]
    /// SWAP [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// PUSHNULL [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// PACK [2048 datoshi]
    /// LDLOC0 [2 datoshi]
    /// CALL_L 9F000000 [512 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// LDLOC0 [2 datoshi]
    /// CALL_L A6000000 [512 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// OVER [2 datoshi]
    /// OVER [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// JMP 0B [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// DUP [2 datoshi]
    /// REVERSE4 [2 datoshi]
    /// REVERSE3 [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// OVER [2 datoshi]
    /// OVER [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// JMP 07 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// DUP [2 datoshi]
    /// REVERSE4 [2 datoshi]
    /// REVERSE3 [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// OVER [2 datoshi]
    /// OVER [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// JMP 07 [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// DUP [2 datoshi]
    /// REVERSE4 [2 datoshi]
    /// REVERSE3 [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// OVER [2 datoshi]
    /// OVER [2 datoshi]
    /// PICKITEM [64 datoshi]
    /// DUP [2 datoshi]
    /// REVERSE4 [2 datoshi]
    /// REVERSE3 [2 datoshi]
    /// INC [4 datoshi]
    /// DUP [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// JMPGE 04 [2 datoshi]
    /// JMP 08 [2 datoshi]
    /// DUP [2 datoshi]
    /// PUSHINT16 FF00 [1 datoshi]
    /// JMPLE 06 [2 datoshi]
    /// PUSHINT16 FF00 [1 datoshi]
    /// AND [8 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH2 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// PUSH1 [1 datoshi]
    /// PACK [2048 datoshi]
    /// DUP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// ROT [2 datoshi]
    /// SETITEM [8192 datoshi]
    /// DROP [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// PUSH0 [1 datoshi]
    /// PICKITEM [64 datoshi]
    /// ISNULL [2 datoshi]
    /// ASSERT [1 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullCoalescingAssignment")]
    public abstract void NullCoalescingAssignment(BigInteger? nullableArg);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEBeErYJgpFDAVsaW51eHBoQA==
    /// INITSLOT 0101 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIFNOT 0A [2 datoshi]
    /// DROP [2 datoshi]
    /// PUSHDATA1 6C696E7578 'linux' [8 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullCollation")]
    public abstract string? NullCollation(string? code);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEBQZv2Z85weGhBkl3oMUrYJgpFDAF72zDbKEA=
    /// INITSLOT 0101 [64 datoshi]
    /// SYSCALL 9BF667CE 'System.Storage.GetContext' [16 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDARG0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// SYSCALL 925DE831 'System.Storage.Get' [32768 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIFNOT 0A [2 datoshi]
    /// DROP [2 datoshi]
    /// PUSHDATA1 7B '{' [8 datoshi]
    /// CONVERT 30 'Buffer' [8192 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullCollationAndCollation")]
    public abstract object? NullCollationAndCollation(string? code);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEBQZv2Z85wDAMxMTF4aEHmPxiEeGhBkl3oMUrYJgpFDAF72zDbKEA=
    /// INITSLOT 0101 [64 datoshi]
    /// SYSCALL 9BF667CE 'System.Storage.GetContext' [16 datoshi]
    /// STLOC0 [2 datoshi]
    /// PUSHDATA1 313131 '111' [8 datoshi]
    /// LDARG0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// SYSCALL E63F1884 'System.Storage.Put' [32768 datoshi]
    /// LDARG0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// SYSCALL 925DE831 'System.Storage.Get' [32768 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIFNOT 0A [2 datoshi]
    /// DROP [2 datoshi]
    /// PUSHDATA1 7B '{' [8 datoshi]
    /// CONVERT 30 'Buffer' [8192 datoshi]
    /// CONVERT 28 'ByteString' [8192 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullCollationAndCollation2")]
    public abstract object? NullCollationAndCollation2(string? code);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeErYJAPKEJhA
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 03 [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH0 [1 datoshi]
    /// NOTEQUAL [32 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullProperty")]
    public abstract bool? NullProperty(string? a);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeErYJAPKELhA
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 03 [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH0 [1 datoshi]
    /// GE [8 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullPropertyGE")]
    public abstract bool? NullPropertyGE(string? a);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeErYJAPKELdA
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 03 [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH0 [1 datoshi]
    /// GT [8 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullPropertyGT")]
    public abstract bool? NullPropertyGT(string? a);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeErYJAPKELZA
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 03 [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH0 [1 datoshi]
    /// LE [8 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullPropertyLE")]
    public abstract bool? NullPropertyLE(string? a);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwABeErYJAPKELVA
    /// INITSLOT 0001 [64 datoshi]
    /// LDARG0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 03 [2 datoshi]
    /// SIZE [4 datoshi]
    /// PUSH0 [1 datoshi]
    /// LT [8 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullPropertyLT")]
    public abstract bool? NullPropertyLT(string? a);

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: VwEAC3BoStgkA0BFQA==
    /// INITSLOT 0100 [64 datoshi]
    /// PUSHNULL [1 datoshi]
    /// STLOC0 [2 datoshi]
    /// LDLOC0 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 03 [2 datoshi]
    /// RET [0 datoshi]
    /// DROP [2 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("nullType")]
    public abstract void NullType();

    /// <summary>
    /// Unsafe method
    /// </summary>
    /// <remarks>
    /// Script: WNgkBVgiBQtKYEVYCJc5C2BY2CQFWCIFCUpgCZc5WUrYJgZFC0phRVkQlzkLYVlK2CYGRRFKYRGXOVlKnEoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9hRVkSlzlZnUoCAAAAgC4EIgpKAv///38yHgP/////AAAAAJFKAv///38yDAMAAAAAAQAAAJ9hWRGXOUA=
    /// LDSFLD0 [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// LDSFLD0 [2 datoshi]
    /// JMP 05 [2 datoshi]
    /// PUSHNULL [1 datoshi]
    /// DUP [2 datoshi]
    /// STSFLD0 [2 datoshi]
    /// DROP [2 datoshi]
    /// LDSFLD0 [2 datoshi]
    /// PUSHT [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// STSFLD0 [2 datoshi]
    /// LDSFLD0 [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIF 05 [2 datoshi]
    /// LDSFLD0 [2 datoshi]
    /// JMP 05 [2 datoshi]
    /// PUSHF [1 datoshi]
    /// DUP [2 datoshi]
    /// STSFLD0 [2 datoshi]
    /// PUSHF [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDSFLD1 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIFNOT 06 [2 datoshi]
    /// DROP [2 datoshi]
    /// PUSHNULL [1 datoshi]
    /// DUP [2 datoshi]
    /// STSFLD1 [2 datoshi]
    /// DROP [2 datoshi]
    /// LDSFLD1 [2 datoshi]
    /// PUSH0 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// PUSHNULL [1 datoshi]
    /// STSFLD1 [2 datoshi]
    /// LDSFLD1 [2 datoshi]
    /// DUP [2 datoshi]
    /// ISNULL [2 datoshi]
    /// JMPIFNOT 06 [2 datoshi]
    /// DROP [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// DUP [2 datoshi]
    /// STSFLD1 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDSFLD1 [2 datoshi]
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
    /// STSFLD1 [2 datoshi]
    /// DROP [2 datoshi]
    /// LDSFLD1 [2 datoshi]
    /// PUSH2 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// LDSFLD1 [2 datoshi]
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
    /// STSFLD1 [2 datoshi]
    /// LDSFLD1 [2 datoshi]
    /// PUSH1 [1 datoshi]
    /// EQUAL [32 datoshi]
    /// ASSERT [1 datoshi]
    /// RET [0 datoshi]
    /// </remarks>
    [DisplayName("staticNullableCoalesceAssignment")]
    public abstract void StaticNullableCoalesceAssignment();

    #endregion
}
