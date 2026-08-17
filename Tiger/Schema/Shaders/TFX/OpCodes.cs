using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

public static class TfxBytecodeOp
{
    public static List<TfxData> ParseAll(DynamicArray<SUInt8> bytecode)
    {
        byte[] data = new byte[bytecode.Count];
        for (int i = 0; i < bytecode.Count; i++)
        {
            data[i] = bytecode[i].Value;
        }

        List<TfxData> opcodes = new();
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < data.Length)
                {
                    TfxData op = ReadTfxBytecodeOp(reader);
                    opcodes.Add(op);
                }
            }
        }

        return opcodes;
    }

    public static TfxData ReadTfxBytecodeOp(BinaryReader reader)
    {
        TfxData tfxData = new()
        {
            op = (TfxBytecode)reader.ReadByte(),
            data = null
        };

        try
        {
            switch (tfxData.op)
            {
                case TfxBytecode.PopOutput:
                case TfxBytecode.PushTemp:
                case TfxBytecode.PopTemp:
                case TfxBytecode.PopSamplerState:
                case TfxBytecode.PushSampler:
                case TfxBytecode.PushFromOutput:
                case TfxBytecode.PopOutputMat4:
                case TfxBytecode.PopOutputUnk:
                case TfxBytecode.PopTextureView:
                case TfxBytecode.PushGlobalChannelVector:
                case TfxBytecode.PushConstantVec4:
                case TfxBytecode.LerpConstant:
                case TfxBytecode.LerpConstantSaturated:
                case TfxBytecode.Permute:
                case TfxBytecode.PopUav:
                case TfxBytecode.Spline4Const:
                case TfxBytecode.Spline8Const:
                case TfxBytecode.Spline8ConstChain:
                case TfxBytecode.Gradient4Const:
                case TfxBytecode.Gradient8Const:
                case TfxBytecode.Unk55:
                case TfxBytecode.Unk57:
                case TfxBytecode.Unk5A:
                case TfxBytecode.Unk5E:
                    tfxData.data = new TfxData1Byte()
                    {
                        value = reader.ReadByte()
                    };
                    break;

                case TfxBytecode.PushExternInputFloat:
                case TfxBytecode.PushExternInputVec4:
                case TfxBytecode.PushExternInputMat4:
                case TfxBytecode.PushExternInputTextureView:
                case TfxBytecode.PushExternInputU32:
                case TfxBytecode.PushExternInputUav:
                case TfxBytecode.PushTexDimensions:
                case TfxBytecode.PushTexTileParams:
                case TfxBytecode.PushTexTileCount:
                    tfxData.data = new TfxData2Byte()
                    {
                        value = reader.ReadByte(),
                        value2 = reader.ReadByte()
                    };
                    break;

                case TfxBytecode.PushObjectChannelVector:
                    tfxData.data = new TfxDataUint()
                    {
                        value = reader.ReadUInt32()
                    };
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
        return tfxData;
    }

    public static string TfxToString(TfxData tfxData, DynamicArray<Vec4> constants, Material? material = null)
    {
        string output = "";
        byte index = 0;
        switch (tfxData.op)
        {
            case TfxBytecode.Permute:
                output = $"{DecodePermuteParam(((TfxData1Byte)tfxData.data).value).ToUpper()}";
                break;
            case TfxBytecode.PushConstantVec4:
                output = $"{constants[((TfxData1Byte)tfxData.data).value].Vec.ToString().Replace("Infinity", "1.#INF")}";
                break;
            case TfxBytecode.LerpConstant:
                output = $"A: {constants[((TfxData1Byte)tfxData.data).value].Vec}: B: {constants[((TfxData1Byte)tfxData.data).value + 1].Vec}";
                break;

            case TfxBytecode.Spline4Const:
                index = ((TfxData1Byte)tfxData.data).value;
                var C3 = $"{constants[index].Vec}";
                var C2 = $"{constants[index + 1].Vec}";
                var C1 = $"{constants[index + 2].Vec}";
                var C0 = $"{constants[index + 3].Vec}";
                var threshold = $"{constants[index + 4].Vec}";

                output = $"Index {index}:" +
                    $"\n\tC3: {C3}" +
                    $"\n\tC2: {C2}" +
                    $"\n\tC1: {C1}" +
                    $"\n\tC0: {C0}" +
                    $"\n\tThreshold: {threshold}";
                break;

            case TfxBytecode.Spline8Const:
                index = ((TfxData1Byte)tfxData.data).value;
                var s8_C3 = $"{constants[index].Vec}";
                var s8_C2 = $"{constants[index + 1].Vec}";
                var s8_C1 = $"{constants[index + 2].Vec}";
                var s8_C0 = $"{constants[index + 3].Vec}";
                var s8_D3 = $"{constants[index + 4].Vec}";
                var s8_D2 = $"{constants[index + 5].Vec}";
                var s8_D1 = $"{constants[index + 6].Vec}";
                var s8_D0 = $"{constants[index + 7].Vec}";
                var C_thresholds = $"{constants[index + 8].Vec}";
                var D_thresholds = $"{constants[index + 9].Vec}";

                output = $"Index {index}:" +
                    $"\n\tC3: {s8_C3}" +
                    $"\n\tC2: {s8_C2}" +
                    $"\n\tC1: {s8_C1}" +
                    $"\n\tC0: {s8_C0}" +
                    $"\n\tD3: {s8_D3}" +
                    $"\n\tD2: {s8_D2}" +
                    $"\n\tD1: {s8_D1}" +
                    $"\n\tD0: {s8_D0}" +
                    $"\n\tC_thresholds: {C_thresholds}" +
                    $"\n\tD_thresholds: {D_thresholds}";
                break;

            case TfxBytecode.Spline8ConstChain:
                output = $"Index {((TfxData1Byte)tfxData.data).value}";
                break;

            case TfxBytecode.Gradient4Const:
                index = ((TfxData1Byte)tfxData.data).value;
                var BaseColor = $"{constants[index].Vec}";
                var Cred = $"{constants[index + 1].Vec}";
                var Cgreen = $"{constants[index + 2].Vec}";
                var Cblue = $"{constants[index + 3].Vec}";
                var Calpha = $"{constants[index + 4].Vec}";
                var Cthresholds = $"{constants[index + 5].Vec}";

                output = $"Index {index}:" +
                    $"\n\tBaseColor: {BaseColor}" +
                    $"\n\tCred: {Cred}" +
                    $"\n\tCgreen: {Cgreen}" +
                    $"\n\tCblue: {Cblue}" +
                    $"\n\tCalpha: {Calpha}" +
                    $"\n\tCthresholds: {Cthresholds}";
                break;

            case TfxBytecode.Gradient8Const:
                index = ((TfxData1Byte)tfxData.data).value;
                BaseColor = $"{constants[index].Vec}";
                Cred = $"{constants[index + 1].Vec}";
                Cgreen = $"{constants[index + 2].Vec}";
                Cblue = $"{constants[index + 3].Vec}";
                Calpha = $"{constants[index + 4].Vec}";
                var Dred = $"{constants[index + 5].Vec}";
                var Dgreen = $"{constants[index + 6].Vec}";
                var Dblue = $"{constants[index + 7].Vec}";
                var Dalpha = $"{constants[index + 8].Vec}";
                Cthresholds = $"{constants[index + 9].Vec}";
                var Dthresholds = $"{constants[index + 10].Vec}";

                output = $"Index {index}:" +
                    $"\n\tBaseColor: {BaseColor}" +
                    $"\n\tCred: {Cred}" +
                    $"\n\tCgreen: {Cgreen}" +
                    $"\n\tCblue: {Cblue}" +
                    $"\n\tCalpha: {Calpha}" +
                    $"\n\tDred: {Dred}" +
                    $"\n\tDgreen: {Dgreen}" +
                    $"\n\tDblue: {Dblue}" +
                    $"\n\tDalpha: {Dalpha}" +
                    $"\n\tCthresholds: {Cthresholds}" +
                    $"\n\tDthresholds: {Dthresholds}";
                break;

            case TfxBytecode.PushExternInputFloat:
                var pFloat = ((TfxData2Byte)tfxData.data).value2;
                output = $"extern {(TfxExtern)((TfxData2Byte)tfxData.data).value}, element {pFloat} (0x{(pFloat * 4):X})";
                break;
            case TfxBytecode.PushExternInputVec4:
                var pVec = ((TfxData2Byte)tfxData.data).value2;
                output = $"extern {(TfxExtern)((TfxData2Byte)tfxData.data).value}, element {pVec} (0x{(pVec * 16):X})";
                break;
            case TfxBytecode.PushExternInputMat4:
                var pMat = ((TfxData2Byte)tfxData.data).value2;
                output = $"extern {(TfxExtern)((TfxData2Byte)tfxData.data).value}, element {pMat} (0x{(pMat * 16):X})";
                break;
            case TfxBytecode.PushExternInputTextureView:
                var pTex = ((TfxData2Byte)tfxData.data).value2;
                output = $"extern {(TfxExtern)((TfxData2Byte)tfxData.data).value}, element {pTex} (0x{(pTex * 8):X})";
                break;
            case TfxBytecode.PushExternInputU32:
                var pU32 = ((TfxData2Byte)tfxData.data).value2;
                output = $"extern {(TfxExtern)((TfxData2Byte)tfxData.data).value}, element {pU32} (0x{(pU32 * 4):X})";
                break;
            case TfxBytecode.PushExternInputUav:
                var pUav = ((TfxData2Byte)tfxData.data).value2;
                output = $"extern {(TfxExtern)((TfxData2Byte)tfxData.data).value}, element {pUav} (0x{(pUav * 8):X})";
                break;
            case TfxBytecode.PopOutput:
                output = $"slot {((TfxData1Byte)tfxData.data).value}";
                break;
            case TfxBytecode.PushFromOutput:
                output = $"element {((TfxData1Byte)tfxData.data).value}";
                break;
            case TfxBytecode.PushTemp:
            case TfxBytecode.PopTemp:
                output = $"index {((TfxData1Byte)tfxData.data).value}";
                break;
            case TfxBytecode.PopTextureView:
                var texSlot = ((TfxData1Byte)tfxData.data).value;
                output = $"Texture Slot {texSlot & 0x1F}";
                break;
            case TfxBytecode.PopSamplerState:
                var sampSlot = ((TfxData1Byte)tfxData.data).value;
                output = $"Sampler Slot {sampSlot & 0x1F}";
                break;
            case TfxBytecode.PopUav:
                output = $"value {((TfxData1Byte)tfxData.data).value}";
                break;
            case TfxBytecode.PushSampler:
                output = $"Sampler Index {((TfxData1Byte)tfxData.data).value}";
                break;
            case TfxBytecode.PushObjectChannelVector:
                var hash = new StringHash(Endian.SwapU32(((TfxDataUint)tfxData.data).value));
                output = $"hash {GlobalStrings.Get().GetString(hash)}";
                break;
            case TfxBytecode.PushGlobalChannelVector:
                index = ((TfxData1Byte)tfxData.data).value;
                output = $"index {index} {GlobalChannels.GetDefault(index)}";
                break;

            case TfxBytecode.Unk57:
            case TfxBytecode.Unk5A:
            case TfxBytecode.Unk5E:
                output = $"unk1 {((TfxData1Byte)tfxData.data).value}";
                break;

            case TfxBytecode.PushTexDimensions:
                var ptd = ((TfxData2Byte)tfxData.data);
                Texture tex = FileResourcer.Get().GetFile<Texture>(material.PSSamplers[ptd.value].Hash);

                output = $"{DecodePermuteParam(ptd.value2).ToUpper()}: " +
                    $"({tex.TagData.Width}, {tex.TagData.Height}, {tex.TagData.Depth}, {tex.TagData.ArraySize})";
                break;
            case TfxBytecode.PushTexTileParams:
                var ptt = ((TfxData2Byte)tfxData.data);
                tex = FileResourcer.Get().GetFile<Texture>(material.PSSamplers[ptt.value2].Hash);

                output = $"{DecodePermuteParam(ptt.value2).ToUpper()}: " +
                    $"{tex.TagData.TilingScaleOffset}";
                break;
            case TfxBytecode.PushTexTileCount:
                var pttc = ((TfxData2Byte)tfxData.data);
                tex = FileResourcer.Get().GetFile<Texture>(material.PSSamplers[pttc.value].Hash);

                output = $"{DecodePermuteParam(pttc.value2).ToUpper()}: " +
                    $"({tex.TagData.TileCount}, {tex.TagData.ArraySize}, 0, 0)"; break;
        }

        return output;
    }

    public static string DecodePermuteParam(byte param)
    {
        char[] dims = { 'x', 'y', 'z', 'w' };
        int s0 = (param >> 6) & 0b11;
        int s1 = (param >> 4) & 0b11;
        int s2 = (param >> 2) & 0b11;
        int s3 = param & 0b11;

        return $".{dims[s0]}{dims[s1]}{dims[s2]}{dims[s3]}";
    }
}

public enum TfxBytecode : byte
{
    Add = 0x1,
    Subtract,
    Multiply,
    Divide,
    Multiply2,
    Add2,
    IsZero,
    Min,
    Max,
    LessThan,
    Dot,
    Merge_1_3,
    Merge_2_2,
    Merge_3_1,

    Cubic = 0x0F,
    Unk10,
    Unk11,
    Unk12,
    Lerp,
    LerpSaturated,

    MultiplyAdd = 0x15,
    Clamp,
    Unk17,
    Abs,
    Sign,
    Floor,
    Ceil,
    Round,
    Frac,
    Unk1e,
    Unk1f,
    Negate,
    VecRotSin,
    VecRotCos,
    VecRotSinCos,

    PermuteAllX = 0x28,
    Permute,
    Saturate,
    Unk24,
    Unk25,
    Unk26,
    Triangle,
    Jitter,
    Wander,
    Rand,
    RandSmooth,
    Unk2c,
    Unk2d,
    TransformVec4,

    // CompareLess = ???,
    // CompareLessEqual = ???,
    // CompareGreater = ???,
    // CompareGreaterEqual = ???,
    // CompareEqual = ???,
    // CompareNotEqual = ???,
    // CompareNotZeroTernary = ???,
    Unk3B = 0x3B,
    Unk3D = 0x3D,
    Unk3F = 0x3F,
    Unk40 = 0x40,
    Unk41 = 0x41,

    PushConstantVec4 = 0x42,
    LerpConstant,
    LerpConstantSaturated,
    Spline4Const,
    Spline8Const,
    Spline8ConstChain,
    Gradient4Const,
    Gradient8Const,
    PushExternInputFloat,
    PushExternInputVec4,
    PushExternInputMat4,
    PushExternInputTextureView,
    PushExternInputU32,
    PushExternInputUav,
    Unk50,
    PushFromOutput,

    PopOutput = 0x53,
    PopOutputUnk,
    PopOutputMat4,
    PushTemp,
    PopTemp,

    Unk55 = 0x58,
    PopTextureView,
    Unk57,

    PopSamplerState = 0x5D,
    PopUav,
    Unk5A,

    PushSampler = 0x61,
    PushObjectChannelVector,
    PushGlobalChannelVector,
    Unk5E,
    Unk5F,
    Unk60,
    PushTexDimensions,
    PushTexTileParams,
    PushTexTileCount,
    Unk64,
    Unk65,
    Unk66,
    Unk67,
}

public struct TfxData
{
    public TfxBytecode op;
    public dynamic? data;
}

public struct TfxData1Byte
{
    public byte value;
}

public struct TfxData2Byte
{
    public byte value;
    public byte value2;
}

public struct TfxDataUint
{
    public uint value;
}

