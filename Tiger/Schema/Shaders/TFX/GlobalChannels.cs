using Tiger.Schema;

namespace Tiger;

public static class GlobalChannels
{
    private static Vector4[] Channels = null;

    public static Vector4 Get(int index)
    {
        if (Channels == null)
            Fill();

        return Channels[index];
    }

    public static Vector4[] Fill()
    {
        Channels = new Vector4[256];

        for (int i = 0; i < Channels.Length; i++)
        {
            Channels[i] = Vector4.One;
        }


        Channels[10] = Vector4.One;
        Channels[25] = new Vector4(40.0f);
        Channels[26] = new Vector4(0.90f); // Atmos intensity but a channel?
        Channels[27] = Vector4.One; // specular tint intensity
        Channels[28] = Vector4.One; // specular tint
        Channels[31] = Vector4.One; // diffuse tint 1
        Channels[32] = Vector4.One; // diffuse tint 1 intensity
        Channels[33] = Vector4.One; // diffuse tint 2
        Channels[34] = Vector4.One; // diffuse tint 2 intensity
        Channels[35] = new Vector4(0.55f);
        Channels[37] = new Vector4(500000.0f, 0.0f, 0.0f, 0.0f); // Fog start
        Channels[40] = Vector4.Zero;
        Channels[41] = new Vector4(50.0f, 0.0f, 0.0f, 0.0f); // Fog falloff
        Channels[43] = Vector4.Zero;
        Channels[82] = Vector4.Zero;
        Channels[83] = Vector4.Zero;
        Channels[84] = Vector4.One;
        Channels[93] = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
        Channels[97] = Vector4.Zero;
        Channels[98] = Vector4.Zero;
        Channels[100] = Vector4.Zero; //new Vector4(0.41105f, 0.71309f, 0.56793f, 0.56793f);
        Channels[102] = Vector4.One; // Seems like sun angle
        Channels[113] = Vector4.Zero;
        Channels[127] = Vector4.Zero;
        Channels[131] = new Vector4(0.0f, 0.5f, 0.3f, 0.0f); // Seems related to line lights

        return Channels;
    }
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "4EB68080", 0x70)]
public struct S4EB68080
{
    [SchemaField(0x0, TigerStrategy.MARATHON_ALPHA)]
    public TigerHash Unk00; // Assuming name
    public int Unk04;

    [SchemaField(0x14, TigerStrategy.MARATHON_ALPHA)]
    public float Unk14;
    public float Unk18;
    public short Unk1C;
    public short Unk1E;

    [SchemaField(0x28, TigerStrategy.MARATHON_ALPHA)]
    public int ChannelIndex;

    [SchemaField(0x30, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SUint8> UnkBytecode;
    public DynamicArray<Vec4> Values;
}

//[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "D3938080", 0x50)]
//[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "DF918080", 0x38)]
//[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "DD918080", 0x38)]
//public struct SDD918080
//{
//    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
//    [SchemaField(0x0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
//    public TigerHash Unk00; // Assuming name

//    [SchemaField(0x38, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
//    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
//    public DynamicArray<SE9948080> Unk20;
//}

//[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "FB938080", 0x4)]
//[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "E9948080", 0x4)]
//public struct SE9948080
//{
//    public short Unk00;
//    public short Unk02;
//}
