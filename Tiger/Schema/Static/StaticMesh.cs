using Arithmic;
using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace Tiger.Schema;

public enum ExportDetailLevel
{
    MostDetailed,
    LeastDetailed,
    AllLevels
}

/// <summary>
/// A processed form of RawMeshPart that is ready to be exported.
/// </summary>
public class MeshPart
{
    public int Index;
    public uint IndexOffset;
    public uint IndexCount;
    public PrimitiveType PrimitiveType;
    public ELodCategory DetailLevel;
    public List<UIntVector3> Indices = new List<UIntVector3>();
    public List<uint> VertexIndices = new List<uint>();
    public List<Vector4> VertexPositions = new List<Vector4>();
    public List<Vector2> VertexTexcoords0 = new List<Vector2>();
    public List<Vector2> VertexTexcoords1 = new List<Vector2>();
    public List<Vector4> VertexNormals = new List<Vector4>();
    public List<Vector4> VertexTangents = new List<Vector4>();
    public List<Vector4> VertexColours = new List<Vector4>();
    public List<Vector4> VertexAO = new List<Vector4>();
    public Dictionary<int, List<Vector4>> VertexExtraData = new(); //TEXCOORD#, extra data
    public Material? Material;
    public int GroupIndex = 0;
    public int VertexLayoutIndex = -1;
    public int MaxVertexColorIndex = -1;
    public bool Collision = true;

    /// <summary>
    /// Creates an instance of a specified type, derived from MeshPart, using data from the provided index and vertex buffers and other data.
    /// </summary>
    /// <typeparam name="T">
    /// The type of MeshPart to create. Must be derived from MeshPart.
    /// </typeparam>
    /// <param name="ib">The mesh index buffer.</param>
    /// <param name="vb">The mesh vertex buffer.</param>
    /// <param name="mat">The material to assign to the created mesh part.</param>
    /// <param name="primitiveType">The type of primitives used by the mesh.</param>
    /// <param name="layoutIndex">The layout index to assign for vertex data lookup.</param>
    /// <param name="indexCount">The number of indices to read from the index buffer.</param>
    /// <param name="indexOffset">The offset in the index buffer where the indices start.</param>
    /// <returns>
    /// A new instance of type <typeparamref name="T"/> initialized with the provided data.
    /// </returns>
    public static T CreateFromBuffers<T>(
    IndexBuffer ib,
    VertexBuffer vb,
    Material mat,
    PrimitiveType primitiveType,
    int layoutIndex,
    uint indexCount,
    uint indexOffset,
    TfxRenderStage renderStage = TfxRenderStage.GenerateGbuffer) where T : MeshPart, new()
    {
        T part = new T();

        if (mat is not null)
        {
            part.Material = mat;
            part.Material.RenderStage = renderStage;
        }

        part.Indices = ib.GetIndexData(primitiveType, indexOffset, indexCount);
        part.VertexLayoutIndex = layoutIndex;
        part.IndexCount = indexCount;
        part.IndexOffset = indexOffset;
        if (part is DynamicMeshPart)
            (part as DynamicMeshPart).RenderStage = renderStage;

        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in part.Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        part.VertexIndices = uniqueVertexIndices.ToList();

        Log.Debug($"Reading vertex buffers {vb.Hash}/{vb.TagData.Stride}");
        vb.ReadVertexDataFromLayout(part, uniqueVertexIndices, 0);

        return part;
    }

    public void TransformPosition(Vector4 offset, Vector4 scale)
    {
        for (int i = 0; i < VertexPositions.Count; i++)
        {
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * scale.X + offset.X,
                VertexPositions[i].Y * scale.Y + offset.Y,
                VertexPositions[i].Z * scale.Z + offset.Z,
                VertexPositions[i].W
            );
        }
    }

    public void TransformTexcoord(Vector2 offset, Vector2 scale)
    {
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * scale.X + offset.X,
                1 - (tx.Y * scale.Y + offset.Y)
            );
        }
    }
}

public struct VertexWeight
{
    public IntVector4 WeightValues;
    public IntVector4 WeightIndices;
}

public class StaticMesh : Tag<SStaticMesh>
{
    public StaticMesh(FileHash hash) : base(hash) { }

    public void SaveMaterialsFromParts(ExporterScene scene, List<StaticPart> parts)
    {
        foreach (var part in parts)
        {
            if (part.Material == null)
            {
                continue;
            }
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }

    public List<StaticPart> Load(ExportDetailLevel detailLevel)
    {
        List<StaticPart> decalParts = LoadDecals(detailLevel);
        var mainParts = _tag.StaticData.Load(detailLevel, _tag);
        mainParts.AddRange(decalParts);
        return mainParts;
    }

    public Task<List<StaticPart>> LoadAsync(ExportDetailLevel detailLevel)
    {
        return Task.Run(() => Load(detailLevel));
    }

    private List<StaticPart> LoadDecals(ExportDetailLevel detailLevel)
    {
        List<StaticPart> parts = new List<StaticPart>();
        foreach (var decalPartEntry in _tag.Decals)
        {
            if (!Globals.Get().GetExportStages().Contains((TfxRenderStage)decalPartEntry.GetRenderStage()))
                continue;

            if (detailLevel == ExportDetailLevel.MostDetailed && !decalPartEntry.DetailLevel.IsHighestLevel())
                continue;

            if (detailLevel == ExportDetailLevel.LeastDetailed && decalPartEntry.DetailLevel.IsHighestLevel())
                continue;

            StaticPart part = new StaticPart(decalPartEntry);
            part.GetDecalData(decalPartEntry, _tag);
            if (decalPartEntry.Material is not null)
            {
                part.Material = decalPartEntry.Material;
                part.Material.RenderStage = (TfxRenderStage)decalPartEntry.GetRenderStage();
            }
            parts.Add(part);
        }

        return parts;
    }
}
