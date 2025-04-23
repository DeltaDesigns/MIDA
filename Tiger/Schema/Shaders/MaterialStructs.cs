namespace Tiger.Schema;

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "808031D8", 0x3D0)]
public struct SMaterial // Errm Ackchyually its called "technique" 🤓
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA)]
    public ScopeBits UsedScopes;

    //public ScopeBits CompatibleScopes; // Not really important, but they are there after each UsedScopes

    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public StateSelection RenderStates;

    [SchemaField(0x58, TigerStrategy.MARATHON_ALPHA)]
    public DynamicStruct<SMaterialShader> Vertex;

    [SchemaField(0x278, TigerStrategy.MARATHON_ALPHA)]
    public DynamicStruct<SMaterialShader> Pixel;

    //[SchemaField(0x348, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Unsure, everything else has 6 shader stages, D1 has 7? (Doesnt matter anyways)
    //[SchemaField(0x368, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    //[SchemaField(0x328, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    //[SchemaField(0x340, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    //public DynamicStruct<SMaterialShader> Compute;

    public dynamic GetScopeBits()
    {
        return UsedScopes;
    }
}

[NonSchemaStruct(TigerStrategy.MARATHON_ALPHA, 0x90)]
public struct SMaterialShader
{
    [SchemaField(0x0, TigerStrategy.MARATHON_ALPHA)]
    public ShaderBytecode Shader;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<STextureTag> Textures;

    [SchemaField(0x20, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SUint8> TFX_Bytecode;
    public DynamicArray<Vec4> TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTag> Samplers;
    public DynamicArray<Vec4> CBuffers; // Fallback if Vector4Container doesn't exist, I guess..?

    [SchemaField(0x64, TigerStrategy.MARATHON_ALPHA)]
    public int Unk64;

    [SchemaField(0x68, TigerStrategy.MARATHON_ALPHA)] // unsure
    public int BufferSlot;
    public FileHash Vector4Container;

    public IEnumerable<STextureTag> EnumerateTextures()
    {
        foreach (STextureTag texture in Textures)
        {
            yield return texture;
        }
    }

    public IEnumerable<DirectXSampler> EnumerateSamplers()
    {
        foreach (SDirectXSamplerTag sampler in Samplers)
        {
            yield return sampler.GetSampler();
        }
    }

    public List<Vector4> GetCBuffer0()
    {
        List<Vector4> data = new();
        if (Vector4Container.IsValid())
        {
            data = GetVec4Container();
        }
        else
        {
            foreach (var vec in CBuffers)
            {
                data.Add(vec.Vec);
            }
        }
        return data;
    }

    public List<Vector4> GetVec4Container()
    {
        List<Vector4> data = new();
        TigerFile container = new(Vector4Container.GetReferenceHash());
        byte[] containerData = container.GetData();

        for (int i = 0; i < containerData.Length / 16; i++)
        {
            data.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }

        return data;
    }

    public TfxBytecodeInterpreter GetBytecode()
    {
        return new TfxBytecodeInterpreter(TfxBytecodeOp.ParseAll(TFX_Bytecode));
    }
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "C6868080", 0x18)]
public struct STextureTag
{
    public uint TextureIndex;

    [SchemaField(0x8, TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public Texture Texture;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "3F018080", 0x10)]
public struct SDirectXSamplerTag
{
    [SchemaField(TigerStrategy.MARATHON_ALPHA, Tag64 = true)]
    public DirectXSampler Sampler;

    public DirectXSampler GetSampler()
    {
        return Sampler;
    }
}

[SchemaStruct("80800090", 0x10)]
public struct Vec4
{
    public Vector4 Vec;
}

[Flags]
public enum ScopeBits : ulong
{
    FRAME = 1UL << 0,
    VIEW = 1UL << 1,
    RIGID_MODEL = 1UL << 2,
    EDITOR_MESH = 1UL << 3,
    EDITOR_TERRAIN = 1UL << 4,
    CUI_VIEW = 1UL << 5,
    CUI_OBJECT = 1UL << 6,
    SKINNING = 1UL << 7,
    SPEEDTREE = 1UL << 8,
    CHUNK_MODEL = 1UL << 9,
    DECAL = 1UL << 10,
    INSTANCES = 1UL << 11,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1UL << 12,
    TRANSPARENT = 1UL << 13,
    TRANSPARENT_ADVANCED = 1UL << 14,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1UL << 15,
    TERRAIN = 1UL << 16,
    POSTPROCESS = 1UL << 17,
    CUI_BITMAP = 1UL << 18,
    CUI_STANDARD = 1UL << 19,
    UI_FONT = 1UL << 20,
    CUI_HUD = 1UL << 21,
    PARTICLE_TRANSFORMS = 1UL << 22,
    PARTICLE_LOCATION_METADATA = 1UL << 23,
    CUBEMAP_VOLUME = 1UL << 24,
    GEAR_PLATED_TEXTURES = 1UL << 25,
    GEAR_DYE_0 = 1UL << 26,
    GEAR_DYE_1 = 1UL << 27,
    GEAR_DYE_2 = 1UL << 28,
    GEAR_DYE_DECAL = 1UL << 29,
    GENERIC_ARRAY = 1UL << 30,
    GEAR_DYE_SKIN = 1UL << 31,
    GEAR_DYE_LIPS = 1UL << 32,
    GEAR_DYE_HAIR = 1UL << 33,
    GEAR_DYE_FACIAL_LAYER_0_MASK = 1UL << 34,
    GEAR_DYE_FACIAL_LAYER_0_MATERIAL = 1UL << 35,
    GEAR_DYE_FACIAL_LAYER_1_MASK = 1UL << 36,
    GEAR_DYE_FACIAL_LAYER_1_MATERIAL = 1UL << 37,
    PLAYER_CENTERED_CASCADED_GRID = 1UL << 38,
    GEAR_DYE_012 = 1UL << 39,
    COLOR_GRADING_UBERSHADER = 1UL << 40,
}
