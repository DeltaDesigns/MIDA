// using MessageBox = System.Windows.Forms.MessageBox;
using System.Collections.Concurrent;
using System.Diagnostics;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Static;

namespace Tiger;

// Really should just use unit tests but i cant be asked to set that up, im lazy

public class TestsSubsystem : Subsystem<TestsSubsystem>
{
    public TestsSubsystem()
    {
    }

    protected internal override bool Initialise()
    {
        return true;
    }

    [SchemaTest("Analyze All Static Mesh Detail Levels")]
    public void AnalyzeAllStaticMeshDetailLevels()
    {
        Log.Info("Getting all Static Mesh Hashes...");
        var staticMeshHashes = PackageResourcer.Get().GetAllHashes<StaticMesh>();
        Log.Info($"Got {staticMeshHashes.Count} Hashes.");

        ConcurrentDictionary<string, Dictionary<ELodCategory, int>> allMeshLodData = new();

        Log.Info($"Loading Static Meshes...");
        Parallel.ForEach(staticMeshHashes, hash =>
        {
            StaticMesh mesh = FileResourcer.Get().GetFile<StaticMesh>(hash);
            List<StaticPart> allParts = mesh.Load(ExportDetailLevel.AllLevels);

            var lodCounts = allParts.GroupBy(p => p.DetailLevel)
                                   .ToDictionary(g => g.Key, g => g.Count());

            allMeshLodData.TryAdd(hash.ToString(), lodCounts);
        });

        List<string> outputLines = new List<string>();
        foreach (var meshData in allMeshLodData)
        {
            outputLines.Add($"Mesh: {meshData.Key}");
            foreach (var lodData in meshData.Value)
            {
                outputLines.Add($"  LOD Category: {lodData.Key} ({(byte)lodData.Key & 1}), Part Count: {lodData.Value}");
            }
        }

        // Make sure the Tests folder exists
        string testsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests");
        Directory.CreateDirectory(testsFolder);

        // Write output to a file
        string outputPath = Path.Combine(testsFolder, "AllStaticMeshLODs.txt");
        File.WriteAllLines(outputPath, outputLines);

        Log.Info($"Analysis complete. Output written to {outputPath}");

        // Assert that we found LOD data  
        Debug.Assert(allMeshLodData.Count > 0, "No static meshes found");
    }

    [SchemaTest("Analyze All Entity Model Detail Levels")]
    public void AnalyzeAllEntityModelDetailLevels()
    {
        Log.Info("Getting all Entity Model Hashes...");
        var entModelHashes = PackageResourcer.Get().GetAllHashes<EntityModel>();
        Log.Info($"Got {entModelHashes.Count} Hashes.");

        ConcurrentDictionary<string, Dictionary<ELodCategory, int>> allMeshLodData = new();

        Log.Info($"Loading Entity Meshes...");
        Parallel.ForEach(entModelHashes, hash =>
        {
            EntityModel mesh = FileResourcer.Get().GetFile<EntityModel>(hash);
            List<DynamicMeshPart> allParts = mesh.Load(ExportDetailLevel.AllLevels, null);

            var lodCounts = allParts.GroupBy(p => p.DetailLevel)
                                   .ToDictionary(g => g.Key, g => g.Count());

            allMeshLodData.TryAdd(hash.ToString(), lodCounts);
        });

        List<string> outputLines = new List<string>();
        foreach (var meshData in allMeshLodData)
        {
            outputLines.Add($"Mesh: {meshData.Key}");
            foreach (var lodData in meshData.Value)
            {
                outputLines.Add($"  LOD Category: {lodData.Key} ({(byte)lodData.Key & 1}), Part Count: {lodData.Value}");
            }
        }

        // Make sure the Tests folder exists
        string testsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests");
        Directory.CreateDirectory(testsFolder);

        // Write output to a file
        string outputPath = Path.Combine(testsFolder, "AllEntityModelLODs.txt");
        File.WriteAllLines(outputPath, outputLines);

        Log.Info($"Analysis complete. Output written to {outputPath}");

        // Assert that we found LOD data  
        Debug.Assert(allMeshLodData.Count > 0, "No static meshes found");
    }

    [SchemaTest("Analyze All Terrain Detail Levels")]
    public void AnalyzeAllTerrainDetailLevels()
    {
        Log.Info("Getting all Terrain Hashes...");
        var terrainHashes = PackageResourcer.Get().GetAllHashes<Terrain>();
        Log.Info($"Got {terrainHashes.Count} Hashes.");

        ConcurrentDictionary<string, Dictionary<ELodCategory, int>> allMeshLodData = new();

        Log.Info($"Loading Entity Meshes...");
        Parallel.ForEach(terrainHashes, hash =>
        {
            Terrain mesh = FileResourcer.Get().GetFile<Terrain>(hash);
            List<STerrainPart> allParts = mesh.TagData.StaticParts;

            var lodCounts = allParts.GroupBy(p => p.DetailLevel)
                                   .ToDictionary(g => g.Key, g => g.Count());

            allMeshLodData.TryAdd(hash.ToString(), lodCounts);
        });

        List<string> outputLines = new List<string>();
        foreach (var meshData in allMeshLodData)
        {
            outputLines.Add($"Mesh: {meshData.Key}");
            foreach (var lodData in meshData.Value)
            {
                outputLines.Add($"  LOD Category: {lodData.Key} ({(int)lodData.Key}), Part Count: {lodData.Value}");
            }
        }

        // Make sure the Tests folder exists
        string testsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests");
        Directory.CreateDirectory(testsFolder);

        // Write output to a file
        string outputPath = Path.Combine(testsFolder, "AllTerrainLODs.txt");
        File.WriteAllLines(outputPath, outputLines);

        Log.Info($"Analysis complete. Output written to {outputPath}");

        // Assert that we found LOD data  
        Debug.Assert(allMeshLodData.Count > 0, "No static meshes found");
    }

}
