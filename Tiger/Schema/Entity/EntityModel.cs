using System.Diagnostics;
using Arithmic;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

public class EntityModel : Tag<SEntityModel>
{
    public EntityModel(FileHash hash) : base(hash)
    {
    }

    public Vector4 RotationOffset = Vector4.Quaternion;
    public Vector4 TranslationOffset = Vector4.Zero;
    public int AttachmentBoneIndex = -1;
    public ExportDetailLevel DetailLevel;

    /*
     * We need the parent resource to get access to the external materials
     */
    public List<DynamicMeshPart> Load(ExportDetailLevel detailLevel, EntityComponent parentResource, bool hasSkeleton = false)
    {
        DetailLevel = detailLevel;
        Dictionary<int, Dictionary<int, S808087D1>> dynamicParts = GetPartsOfDetailLevel();
        List<DynamicMeshPart> parts = GenerateParts(dynamicParts, parentResource, hasSkeleton);
        return parts;
    }

    /// <summary>
    /// There are two flags that we use as selection criteria.
    /// First is LodCategory, second is DetailGroup.
    /// DetailGroup groups together objects that belong to the same geometry representation.
    /// LodCategory is a scale 0-A (usually 0,4,7,9) that determines how detailed (0 is highest).
    /// The criteria for selection for highest detail is:
    /// - detail level closest to 0 within the whole group.
    /// - OR parts that have a material right there as I'm still unsure about external material table stuff AND same detail level.
    /// </summary>
    /// <param name="detailLevel">The desired level of detail to get parts for.</param>
    /// <returns></returns>
    private Dictionary<int, Dictionary<int, S808087D1>> GetPartsOfDetailLevel()
    {
        Dictionary<int, Dictionary<int, S808087D1>> parts = new();

        using TigerReader reader = GetReader();

        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            int partIndex = 0;
            parts.Add(meshIndex, new Dictionary<int, S808087D1>());
            for (int i = 0; i < mesh.Parts.Count; i++)
            {
                S808087D1 part = mesh.Parts[reader, i];
                //Console.WriteLine($"{i}--------------");
                //Console.WriteLine($"Material {part.Material?.FileHash}");
                //Console.WriteLine($"VariantShaderIndex {part.VariantShaderIndex}");
                //Console.WriteLine($"PrimitiveType {part.PrimitiveType}");
                //Console.WriteLine($"IndexOffset {part.IndexOffset}");
                //Console.WriteLine($"IndexCount {part.IndexCount}");
                //Console.WriteLine($"Unk10 {part.Unk10}");
                //Console.WriteLine($"ExternalIdentifier {part.ExternalIdentifier}");
                //Console.WriteLine($"Unk16 {part.Unk16}");
                //Console.WriteLine($"FlagsD1 {part.FlagsD1}");
                //Console.WriteLine($"GearDyeChangeColorIndex {part.GearDyeChangeColorIndex}");
                //Console.WriteLine($"LodCategory {part.LodCategory}");

                switch (DetailLevel)
                {
                    case ExportDetailLevel.MostDetailed:
                        if (part.DetailLevel.IsHighestLevel())
                            parts[meshIndex].Add(partIndex, part);
                        break;

                    case ExportDetailLevel.LeastDetailed:
                        if (!part.DetailLevel.IsHighestLevel())
                            parts[meshIndex].Add(partIndex, part);
                        break;

                    default:
                        parts[meshIndex].Add(partIndex, part);
                        break;
                }

                partIndex++;
            }

            meshIndex++;
        }

        return parts;
    }

    private List<DynamicMeshPart> GenerateParts(Dictionary<int, Dictionary<int, S808087D1>> dynamicParts, EntityComponent parentResource, bool hasSkeleton = false)
    {
        var _strategy = Strategy.CurrentStrategy;

        List<DynamicMeshPart> parts = new();
        List<int> exportPartRange = new();
        if (_tag.Meshes.Count == 0) return parts;
        int meshIndex = 0;
        foreach (SEntityModelMesh mesh in _tag.Meshes.Enumerate(GetReader()))
        {
            exportPartRange = GetExportRanges(mesh);
            foreach ((int i, S808087D1 part) in dynamicParts[meshIndex])
            {
                if (!exportPartRange.Contains(i) && DetailLevel != ExportDetailLevel.AllLevels)
                    continue;

                DynamicMeshPart dynamicMeshPart = new(part, parentResource)
                {
                    Index = i,
                    MeshIndex = meshIndex,
                    GroupIndex = part.ExternalIdentifier,
                    DetailLevel = part.DetailLevel,
                    bAlphaClip = (part.Flags & 0x8) != 0,
                    GearDyeChangeColorIndex = part.GearDyeChangeColorIndex,
                    HasSkeleton = hasSkeleton,
                    VertexLayoutIndex = mesh.GetInputLayoutForStage(0),
                    RenderStage = (TfxRenderStage)Array.IndexOf(mesh.PartRangePerRenderStage, (short)i)
                };

                //We only care about the vertex shader for now for mesh data
                //But if theres also no pixel shader then theres no point in adding it
                if (DetailLevel != ExportDetailLevel.AllLevels &&
                    (dynamicMeshPart.Material is null
                    || dynamicMeshPart.Material.Vertex.Shader is null
                    || dynamicMeshPart.Material.Pixel.Shader is null))
                    continue;

                if (dynamicMeshPart.Material is not null)
                    dynamicMeshPart.Material.RenderStage = dynamicMeshPart.RenderStage;

                dynamicMeshPart.GetAllData(mesh, _tag);
                parts.Add(dynamicMeshPart);
            }

            meshIndex++;
        }

        return parts;
    }

    public static List<int> GetExportRanges(SEntityModelMesh mesh)
    {
        List<int> exportPartRange = new();

        foreach (TfxRenderStage stage in Globals.Get().GetExportStages())
        {
            var range = mesh.GetRangeForStage((int)stage);
            if (!(range.Start.Value < range.End.Value))
                continue;

            for (int i = range.Start.Value; i < range.End.Value; i++)
                exportPartRange.Add(i);
        }

        return exportPartRange;
    }
}

public class DynamicMeshPart : MeshPart
{
    public List<VertexWeight> VertexWeights = new List<VertexWeight>();

    // used for single-pass skin buffer, where we want to find the position vec from a global index
    public Dictionary<uint, int> VertexIndexMap = new Dictionary<uint, int>();

    public List<Vector4> VertexColourSlots = new List<Vector4>();
    public bool bAlphaClip;
    public bool HasSkeleton;
    public byte GearDyeChangeColorIndex = 0xFF;

    public TfxRenderStage RenderStage;

    public DynamicMeshPart(S808087D1 part, EntityComponent parentResource) : base()
    {
        IndexOffset = part.IndexOffset;
        IndexCount = part.IndexCount;
        PrimitiveType = (PrimitiveType)part.PrimitiveType;
        if (part.VariantShaderIndex == -1)
            Material = part.Material;
        else
            Material = GetMaterialFromExternalMaterial(part.VariantShaderIndex, parentResource);
    }

    public DynamicMeshPart() : base()
    {
    }

    public void GetAllData(SEntityModelMesh mesh, SEntityModel model)
    {
        VertexScale = model.ModelScale.ToVec3();
        VertexOffset = model.ModelTranslation.ToVec3();

        Indices = mesh.Indices.GetIndexData(PrimitiveType, IndexOffset, IndexCount);

        // Get unique vertex indices we need to get data for
        HashSet<uint> uniqueVertexIndices = new HashSet<uint>();
        foreach (UIntVector3 index in Indices)
        {
            uniqueVertexIndices.Add(index.X);
            uniqueVertexIndices.Add(index.Y);
            uniqueVertexIndices.Add(index.Z);
        }
        VertexIndices = uniqueVertexIndices.ToList();

        for (int i = 0; i < VertexIndices.Count; i++)
        {
            VertexIndexMap.Add(VertexIndices[i], i);
        }

        //Dictionary<uint, int> lookup = new Dictionary<uint, int>();
        //for (int i = 0; i < VertexIndices.Count; i++)
        //{
        //    lookup[VertexIndices[i]] = i;
        //}

        //Log.Debug($"Reading vertex buffers {mesh.Vertices1.Hash}/{mesh.Vertices1.TagData.Stride} and {mesh.Vertices2?.Hash}/{mesh.Vertices2?.TagData.Stride}");
        mesh.Vertices1.ReadVertexDataFromLayout(this, uniqueVertexIndices, 0);
        mesh.Vertices2?.ReadVertexDataFromLayout(this, uniqueVertexIndices, 1);

        if (mesh.OldWeights != null)
            mesh.OldWeights.ReadVertexData(this, uniqueVertexIndices, 2); // bufferIndex 2 is used for D1, shouldnt affect D2 I hope

        if (mesh.VertexColour != null)
            mesh.VertexColour.ReadVertexData(this, uniqueVertexIndices);

        if (mesh.SinglePassSkinningBuffer != null)
            mesh.SinglePassSkinningBuffer.ReadVertexData(this, uniqueVertexIndices);

        Debug.Assert(VertexPositions.Count == VertexTexcoords0.Count && VertexPositions.Count == VertexNormals.Count);

        TransformPositions(model);
        TransformTexcoords(mesh, model);
    }

    private void TransformTexcoords(SEntityModelMesh mesh, SEntityModel header)
    {
        Vector2 texcoordScale = header.TexcoordScale;
        Vector2 texcoordTranslation = header.TexcoordTranslation;
        float yOffset = 0f;//5f / 3f; // idfk

        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            VertexTexcoords0[i] = new Vector2(
                tx.X * texcoordScale.X + texcoordTranslation.X,
                tx.Y * texcoordScale.Y + texcoordTranslation.Y
            );
        }

        // Detail UVs
        //TODO: Make outputs match what renderdoc says they actually are
        if (mesh.SinglePassSkinningBuffer != null)
        {
            try
            {
                var stride = mesh.SinglePassSkinningBuffer.TagData.Stride;
                using TigerReader handle = mesh.SinglePassSkinningBuffer.GetReferenceReader();

                for (int i = 0; i < VertexPositions.Count; i++)
                {
                    int normW = (int)(32767.0996f * VertexNormals[i].W);
                    uint index = (uint)normW >> 3;
                    index = index & 4095;

                    handle.Seek(index * stride, SeekOrigin.Begin);
                    float UVX = (float)handle.ReadHalf();
                    float UVY = (float)handle.ReadHalf();

                    var tx = VertexTexcoords0[i];
                    var tx1 = new Vector2(tx.X * UVX, ((tx.Y * UVY) * -1) - 0.65); // idfk whats going wrong here
                    VertexTexcoords1.Add(tx1);
                    //Console.WriteLine($"({i}) {mesh.SinglePassSkinningBuffer.Hash} {index} ({(index * 0x4):X}): XY ({tx.X}, {tx.Y}) ZW ({tx1.X}, {tx1.Y})");
                }
            }
            catch (Exception e)
            {
                Log.Error($"{mesh.SinglePassSkinningBuffer.Hash}: {e.Message}");
            }
        }
        else
        {
            yOffset = 0f;
            VertexTexcoords1 = VertexTexcoords0.Select(tx1 => new Vector2(tx1.X * 5, (1 - tx1.Y) * 5)).ToList();
        }

        // Flip Y axis, fix detail UV offset
        for (int i = 0; i < VertexTexcoords0.Count; i++)
        {
            var tx = VertexTexcoords0[i];
            var tx1 = VertexTexcoords1[i];
            VertexTexcoords0[i] = new Vector2(tx.X, 1f - tx.Y);
        }
    }

    private void TransformPositions(SEntityModel header)
    {
        Vector4 modelScale = header.ModelScale;
        Vector4 modelTranslation = header.ModelTranslation;

        for (int i = 0; i < VertexPositions.Count; i++)
        {
            VertexPositions[i] = new Vector4(
                VertexPositions[i].X * modelScale.X + modelTranslation.X,
                VertexPositions[i].Y * modelScale.Y + modelTranslation.Y,
                VertexPositions[i].Z * modelScale.Z + modelTranslation.Z,
                VertexPositions[i].W
            );
        }
    }

    private Material? GetMaterialFromExternalMaterial(short variantShaderIndex, EntityComponent parentResource)
    {
        if (parentResource is null)
            return null;

        using TigerReader reader = parentResource.GetReader();

        var map = parentResource is EntityPhysicsModelParent ?
            ((S80808655)parentResource.GetUnk18(reader)).ExternalMaterialsMap :
            ((S80808678)parentResource.GetUnk18(reader)).ExternalMaterialsMap;

        var mats = parentResource is EntityPhysicsModelParent ?
            ((S80808655)parentResource.GetUnk18(reader)).ExternalMaterials :
            ((S80808678)parentResource.GetUnk18(reader)).ExternalMaterials;

        if (map.Count == 0 || mats.Count == 0)
            return null;

        if (variantShaderIndex >= map.Count)
            return null; // todo this is actually wrong ig...

        SExternalMaterialMapEntry mapEntry = map[reader, variantShaderIndex];
        int permutationCount = map
            .Enumerate(reader)
            .Where(m => m.Unk08 == 0)
            .Select(m => (int?)m.MaterialCount)
            .FirstOrDefault() ?? 1;

        if (permutationCount <= 0)
            permutationCount = 1;

        int permutationIndex = 0;

        //int permutationIndex = permutationCount - 1;
        // TODO, permutation selection
        //if (parentResource is EntityModelParent parent && parent.MaterialPermutations is not null)
        //{
        //    permutationIndex = parent.MaterialPermutations.OverrideIndex != -1
        //        ? parent.MaterialPermutations.OverrideIndex : parent.MaterialPermutations.CalculatePermutationIndex() ?? 0;
        //}
        return mats[reader, mapEntry.MaterialStartIndex + (permutationIndex % mapEntry.MaterialCount)].Material;
    }

    public static void AddVertexColourSlotInfo(DynamicMeshPart dynamicPart, short w)
    {
        Vector4 vc = Vector4.Zero;
        switch (w & 0x7)
        {
            case 0:
                vc.X = 0.333f;
                break;
            case 1:
                vc.X = 0.666f;
                break;
            case 2:
                vc.X = 0.999f;
                break;
            case 3:
                vc.Y = 0.333f;
                break;
            case 4:
                vc.Y = 0.666f;
                break;
            case 5:
                vc.Y = 0.999f;
                break;
        }

        if (dynamicPart.bAlphaClip)
        {
            vc.Z = 0.25f;
        }

        dynamicPart.VertexColourSlots.Add(vc);
    }
}
