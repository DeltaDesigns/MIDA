using System.Diagnostics;
using Tiger.Exporters;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace Tiger.Schema;

public class Terrain : Tag<STerrain>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.TerrainPatch;
    public Terrain(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene scene, string saveDirectory, ulong? identifier = null)
    {
        var _config = ConfigSubsystem.Get();
        if (_config.GetSingleFolderMapAssetsEnabled())
            saveDirectory = Path.Join(_config.GetExportSavePath(), $"Maps/Assets/");

        // Uses triangle strip + only using first set of vertices and indices
        Dictionary<StaticPart, Material> parts = new Dictionary<StaticPart, Material>();
        List<Texture> dyeMaps = new List<Texture>();

        int terrainTextureIndex = 14;
        Texture lastValidEntry = null;
        for (int i = 0; i < _tag.MeshGroups.Count; i++)
        {
            var meshGroup = _tag.MeshGroups[i];
            // Check if the current Dyemap is null

            if (meshGroup.Dyemap == null)
            {
                if (lastValidEntry != null)
                {
                    // Use the last valid Dyemap for any invalid
                    scene.ExternalTextures.Add(lastValidEntry);
                    dyeMaps.Add(lastValidEntry);
                }
                else // Use the first valid dyemap if it gets to this point
                {
                    var firstValidDyemap = _tag.MeshGroups.FirstOrDefault(x => x.Dyemap != null).Dyemap;
                    if (firstValidDyemap != null)
                    {
                        scene.ExternalTextures.Add(firstValidDyemap);
                        dyeMaps.Add(firstValidDyemap);
                    }
                }
            }
            else
            {
                // Update lastValidEntry with the current Dyemap
                lastValidEntry = meshGroup.Dyemap;
                scene.ExternalTextures.Add(meshGroup.Dyemap);
                dyeMaps.Add(meshGroup.Dyemap);
            }

            foreach (var partEntry in _tag.StaticParts.Where(x => x.GroupIndex == i))
            {
                // MainGeom0 LOD0, GripStock0 LOD1, Stickers0 LOD2?
                if (partEntry.DetailLevel == ELodCategory.Unk0)
                {
                    if (partEntry.Material != null && partEntry.Material.Vertex.Shader != null)
                    {
                        var part = MakePart(partEntry);

                        scene.Materials.Add(new ExportMaterial(partEntry.Material, true));
                        part.Material = partEntry.Material;

                        TransformPositions(part);
                        TransformTexcoords(part);
                        TransformVertexColors(part);

                        if (_config.GetS2ShaderExportEnabled())
                            Source2Handler.SaveVMAT($"{saveDirectory}", $"{part.Material.Hash}", part.Material, dyeMaps);

                        parts.TryAdd(part, partEntry.Material);
                    }
                }
            }

            scene.AddTerrain($"{Hash}", parts.Keys.ToList(), identifier, i);

            if (_config.GetS2VMDLExportEnabled())
                Source2Handler.SaveTerrainVMDL($"{Hash}_{i}", saveDirectory, parts.Keys.ToList());

            parts.Clear();
        }

        scene.AddStaticInstance(Hash, Vector3.One, Vector4.Zero, Vector3.Zero);
        for (int i = 0; i < dyeMaps.Count; i++)
        {
            scene.AddTerrainDyemap(Hash, dyeMaps[i].Hash);
        }
    }

    public StaticPart MakePart(STerrainPart entry)
    {
        StaticPart part = new(entry);
        part.GroupIndex = entry.GroupIndex;
        part.Indices = _tag.Indices1.GetIndexData(PrimitiveType.TriangleStrip, entry.IndexOffset, entry.IndexCount);
        part.VertexLayoutIndex = 22;

        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in part.Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        part.VertexIndices = uniqueVertexIndices.ToList();

        _tag.Vertices1.ReadVertexDataFromLayout(part, uniqueVertexIndices, 0);
        _tag.Vertices2?.ReadVertexDataFromLayout(part, uniqueVertexIndices, 1);

        return part;
    }

    public void TransformPositions(StaticPart part)
    {
        Debug.Assert(part.VertexPositions.Count == part.VertexNormals.Count);
        for (int i = 0; i < part.VertexPositions.Count; i++)
        {
            //The "standard" terrain vertex shader from hlsl
            System.Numerics.Vector4 r0, r1, r2 = new();
            System.Numerics.Vector4 v0 = new(part.VertexPositions[i].X, part.VertexPositions[i].Y, part.VertexPositions[i].Z, part.VertexPositions[i].W);
            System.Numerics.Vector4 v1 = new(part.VertexNormals[i].X, part.VertexNormals[i].Y, part.VertexNormals[i].Z, part.VertexNormals[i].W);

            //r0 = cb11.transform + v0;
            r0.X = _tag.Unk30.X + v0.X;
            r0.Y = _tag.Unk30.Y + v0.Y;
            r0.Z = _tag.Unk30.Z + v0.Z;
            r0.W = _tag.Unk30.W + v0.W;

            r0.Z = r0.W * 65536 + r0.Z;

            //r0.xyw = float3(0.015625, 0.015625, 0.000122070313) * r0.xyz;
            r0.X = 0.015625f * r0.X;
            r0.Y = 0.015625f * r0.Y;
            r0.W = 0.000122070313f * r0.Z;

            //r1.xyz = float3(0,1,0) * v1.yzx;
            r1.X = 0 * v1.Y;
            r1.Y = 1 * v1.Z;
            r1.Z = 0 * v1.X;

            //r1.xyz = v1.zxy * float3(0,0,1) + -r1.xyz;
            r1.X = v1.Z * 0 + -r1.X;
            r1.Y = v1.X * 0 + -r1.Y;
            r1.Z = v1.Y * 1 + -r1.Z;

            //r0.z = dot(r1.yz, r1.yz);
            r0.Z = System.Numerics.Vector2.Dot(new(r1.Y, r1.Z), new(r1.Y, r1.Z));

            //r0.z = rsqrt(r0.z);
            r0.Z = MathF.ReciprocalSqrtEstimate(r0.Z);

            //r1.xyz = r1.xyz * r0.zzz;
            r1.X = r1.X * r0.Z;
            r1.Y = r1.Y * r0.Z;
            r1.Z = r1.Z * r0.Z;

            //r2.xyz = v1.zxy * r1.yzx;
            r2.X = v1.Z * r1.Y;
            r2.Y = v1.X * r1.Z;
            r2.Z = v1.Y * r1.X;

            //r2.xyz = v1.yzx * r1.zxy + -r2.xyz;
            r2.X = v1.Y * r1.Z + -r2.X;
            r2.Y = v1.Z * r1.X + -r2.Y;
            r2.Z = v1.X * r1.Y + -r2.Z;

            //r0.z = dot(r2.xyz, r2.xyz);
            //r0.z = rsqrt(r0.z);
            r0.Z = System.Numerics.Vector3.Dot(new(r2.X, r2.Y, r2.Z), new(r2.X, r2.Y, r2.Z));
            r0.Z = MathF.ReciprocalSqrtEstimate(r0.Z);

            part.VertexPositions[i] = new Vector4(r0.X, r0.Y, r0.Z * r0.W, r0.W);
        }
    }

    public void TransformTexcoords(StaticPart part)
    {
        for (int i = 0; i < part.VertexTexcoords0.Count; i++)
        {
            part.VertexTexcoords0[i] = new Vector2(
            part.VertexTexcoords0[i].X * _tag.MeshGroups[part.GroupIndex].Unk20.X + _tag.MeshGroups[part.GroupIndex].Unk20.Z,
            part.VertexTexcoords0[i].Y * -_tag.MeshGroups[part.GroupIndex].Unk20.Y + 1 - _tag.MeshGroups[part.GroupIndex].Unk20.W);
        }
    }

    public void TransformVertexColors(StaticPart part)
    {
        //Helper for dyemap assignment
        float alpha = part.GroupIndex / 15.0f;
        for (int i = 0; i < part.VertexPositions.Count; i++)
        {
            part.VertexColours.Add(new Vector4(0.0f, 0.0f, 0.0f, alpha));
        }
    }
}

/// <summary>
/// Terrain data resource.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80808563", 0x20)]
public struct SMapTerrainResource
{
    [SchemaField(0x10)]
    public ulong Identifier;
    [NoLoad]
    public Terrain Terrain;
    public Tag<SOcclusionBounds> TerrainBounds;
}

/// <summary>
/// Terrain _tag.
/// </summary>
[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "80808567", 0xB0)]
public struct STerrain
{
    public long FileSize;
    [SchemaField(0x10)]
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;

    [SchemaField(0x50, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<SMeshGroup> MeshGroups;

    public VertexBuffer Vertices1;
    public VertexBuffer Vertices2;
    public IndexBuffer Indices1;
    public Material Unk6C;
    public Material Unk70;

    [SchemaField(0x78, TigerStrategy.MARATHON_ALPHA)]
    public DynamicArray<STerrainPart> StaticParts;
    public VertexBuffer Vertices3;
    public VertexBuffer Vertices4;
    public IndexBuffer Indices2;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080856C", 0x60)]
public struct SMeshGroup
{
    //Location?
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public uint Unk30;
    public uint Unk34;
    public uint Unk38;
    public uint Unk3C;
    public uint Unk40;
    public uint Unk44;
    public uint Unk48;
    public uint Unk4C;

    [SchemaField(0x50, TigerStrategy.MARATHON_ALPHA)]
    public Texture Dyemap;
}

[SchemaStruct(TigerStrategy.MARATHON_ALPHA, "8080856A", 0x0C)]
public struct STerrainPart
{
    public Material Material;
    public uint IndexOffset;
    public ushort IndexCount;
    public byte GroupIndex;
    public ELodCategory DetailLevel;
}
