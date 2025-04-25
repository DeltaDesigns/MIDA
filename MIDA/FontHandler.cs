using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Other;
using FontFamily = System.Windows.Media.FontFamily;

namespace MIDA;

[InitializeAfter(typeof(Hash64Map))]
public class FontHandler : Strategy.StrategistSingleton<FontHandler>
{
    public ConcurrentDictionary<FontInfo, FontFamily> Fonts = new();

    public FontHandler(TigerStrategy strategy) : base(strategy)
    {
    }

    protected override void Initialise()
    {
        //return true;
        SaveAllFonts();
        LoadAllFonts();
        RegisterFonts();
    }

    protected override void Reset()
    {

    }

    private static void SaveAllFonts()
    {
        var vals = PackageResourcer.Get().GetAllHashes<S808050B7>();
        Tag<S808050B7> fontsContainer = FileResourcer.Get().GetSchemaTag<S808050B7>(vals.First());

        // Check if the font exists in the Fonts/ folder, if not extract it
        if (!Directory.Exists("fonts/"))
        {
            Directory.CreateDirectory("fonts/");
        }
        Parallel.ForEach(fontsContainer.TagData.FontParents, f =>
        {
            var ff = f.FontParent.TagData.FontFile;
            var fontName = f.FontParent.TagData.FontName.Value;
            if (!File.Exists($"fonts/{fontName}"))
            {
                using (TigerReader reader = ff.GetReader())
                {
                    var bytes = reader.ReadBytes((int)f.FontParent.TagData.FontFileSize);
                    File.WriteAllBytes($"fonts/{fontName}", bytes);
                }
            }
        });
    }

    private bool LoadAllFonts()
    {
        if (!Directory.Exists(@"fonts"))
            return false;

        foreach (var s in Directory.GetFiles(@"fonts/"))
        {
            var otfPath = Environment.CurrentDirectory + "/" + s;
            FontInfo fontInfo = GetFontInfo(otfPath);
            FontFamily font = new FontFamily(otfPath + $"#{fontInfo.Family}");
            Fonts.TryAdd(fontInfo, font);

            // Adds the Destiny Keys fonts as the fallback font for Haas Grot
            if (fontInfo.Family.Contains("KH Interference"))
            {
                FontFamily fontKeys = new FontFamily(otfPath + $"#{fontInfo.Family}, " +
                    $"{Environment.CurrentDirectory + $"/fonts/goliath_symbols_pc.otf#Destiny Keys"}");

                Fonts.TryAdd(new FontInfo { Family = $"{fontInfo.Family} {fontInfo.Subfamily}", Subfamily = "Keys" }, fontKeys);
            }

            if (fontInfo.Family.Contains("PP Fraktion Mono"))
            {
                FontFamily fontKeys = new FontFamily(otfPath + $"#{fontInfo.Family}, " +
                    $"{Environment.CurrentDirectory + $"/fonts/goliath_symbols_pc.otf#Destiny Keys"}");

                Fonts.TryAdd(new FontInfo { Family = $"{fontInfo.Family} {fontInfo.Subfamily}", Subfamily = "Keys" }, fontKeys);
            }
        }

        return Fonts.Count > 0;
    }

    // TODO figure out why Marathon Shapiro flat out refuses to work as a resource
    private void RegisterFonts()
    {
        foreach (var (key, value) in Fonts)
        {
            if (!Application.Current.Resources.Contains($"{key.Family} {key.Subfamily}"))
            {
                Application.Current.Resources.Add($"{key.Family} {key.Subfamily}", value);
            }
        }

        // Debug font list
        // List<string> fontList = Fonts.Select(pair => (pair.Key.Family + " " + pair.Key.Subfamily).Trim()).ToList();
        //foreach (var s in Fonts)
        //{
        //    Console.WriteLine($"Family: {s.Key.Family} | SubFamilty: {s.Key.Subfamily}");
        //}

        /*
        Family: Destiny Keys SubFamilty: Regular
        Family: KH Interference Light SubFamilty: Keys
        Family: IvyPresto Headline SubFamilty: Regular
        Family: KH Teka SubFamilty: Regular
        Family: KH Teka SubFamilty: Medium
        Family: KH Teka SubFamilty: Light
        Family: KH Teka SubFamilty: Bold
        Family: Pragmatica SubFamilty: Bold
        Family: Noto Sans SC SubFamilty: Medium
        Family: Noto Sans SC SubFamilty:
        Family: Noto Sans JP SubFamilty: Medium
        Family: Noto Sans JP SubFamilty:
        Family: Synchro Std SubFamilty:
        Family: KH Interference SubFamilty: Regular
        Family: KH Interference SubFamilty: Light
        Family: Marathon Shapiro SubFamilty: Wide 65
        Family: Destiny Symbols SubFamilty:
        Family: Shapiro SubFamilty: 65 Light Heavy Wide
        Family: PP Fraktion Mono SubFamilty: Semibold
        Family: PP Fraktion Mono SubFamilty: Medium
        Family: PP Fraktion Mono SubFamilty: Light
        Family: PP Fraktion Mono SubFamilty:
        Family: PP Fraktion Mono SubFamilty: Thin
        Family: KH Interference Regular SubFamilty: Keys
        Family: Noto Sans KR SubFamilty: Medium
        Family: Noto Sans KR SubFamilty:
        Family: Noto Sans TC SubFamilty: Medium
        Family: Noto Sans TC SubFamilty:
        */
    }

    public struct FontInfo
    {
        public string Family;
        public string Subfamily;
    }

    private FontInfo GetFontInfo(string fontPath)
    {
        FontInfo fontInfo;
        using var br = new BinaryReaderBE(new MemoryStream(File.ReadAllBytes(fontPath)));
        byte[] val = br.ReadBytes(4);
        while (Encoding.ASCII.GetString(val) != "name")
        {
            val = br.ReadBytes(4);
        }

        var nameTableRecord = br.ReadType<OtfNameTableRecord>(true);
        br.BaseStream.Seek(nameTableRecord.Offset, SeekOrigin.Begin);

        var namingTableVer0 = br.ReadType<OtfNamingTableVersion0>(true);

        List<OtfNameRecord> nameRecords = new(namingTableVer0.Count);
        for (int i = 0; i < namingTableVer0.Count; i++)
        {
            var nameRecord = br.ReadType<OtfNameRecord>(true);
            nameRecords.Add(nameRecord);
        }

        OtfNameRecord familyRecord;
        try
        {
            familyRecord = nameRecords.First(x => x.NameId == 16);
        }
        catch (InvalidOperationException e)
        {
            familyRecord = nameRecords.First(x => x.NameId == 1);
        }
        br.BaseStream.Seek(nameTableRecord.Offset + namingTableVer0.StorageOffset + familyRecord.StringOffset, SeekOrigin.Begin);
        fontInfo.Family = ReadString(br, familyRecord.Length).Trim();

        OtfNameRecord subfamilyRecord;
        try
        {
            subfamilyRecord = nameRecords.FirstOrDefault(x => x.NameId == 17);
        }
        catch (InvalidOperationException e)
        {
            subfamilyRecord = nameRecords.FirstOrDefault(x => x.NameId == 2);
        }

        br.BaseStream.Seek(nameTableRecord.Offset + namingTableVer0.StorageOffset + subfamilyRecord.StringOffset, SeekOrigin.Begin);
        fontInfo.Subfamily = ReadString(br, subfamilyRecord.Length).Trim();

        return fontInfo;
    }

    /// <summary>
    /// Glyph names are kinda interesting, could get them in the future. CCF table?
    /// </summary>
    private static List<string> GetGlyphNames(string fontPath)
    {
        throw new NotImplementedException();
    }

    private static string ReadString(BinaryReaderBE br, int length)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            char c = br.ReadChar();
            sb.Append(c);
        }

        return ConvertWideChar(sb.ToString());
    }

    private static string ConvertWideChar(string s)
    {
        if (s.Contains("\x00")) // wchar_t
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            bytes = bytes.Where(x => x != '\x00').ToArray();
            return Encoding.UTF8.GetString(bytes);
        }
        return s;
    }
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNameTableRecord
{
    public uint Length;
    public uint Offset;
    public uint Checksum;
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNamingTableVersion0
{
    public ushort StorageOffset;
    public ushort Count;
    public ushort Version;
}

[StructLayout(LayoutKind.Sequential)]
struct OtfNameRecord
{
    public ushort StringOffset;
    public ushort Length;
    public ushort NameId;
    public ushort LanguageId;
    public ushort EncodingId;
    public ushort PlatformId;
}

public class BinaryReaderBE : BinaryReader
{
    public BinaryReaderBE(Stream stream) : base(stream) { }

    public override int ReadInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public override Int16 ReadInt16()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }

    public override Int64 ReadInt64()
    {
        var data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }

    public override UInt32 ReadUInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

    public dynamic ToType(byte[] bytes, Type type)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type); }
        finally { handle.Free(); }
    }

    public T ReadType<T>(bool BE = true)
    {
        return (T)ReadType(typeof(T), BE);
    }

    public dynamic ReadType(Type type, bool BE)
    {
        var buffer = new byte[Marshal.SizeOf(type)];
        Read(buffer, 0, buffer.Length);
        if (BE)
            Array.Reverse(buffer);
        return ToType(buffer, type);
    }

}
