using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using Arithmic;


// TODO: Find where these indexes actually go?
// Would be nice if bungie stopped changing these every season :)
public static class DestinyDamageType
{
    public static DestinyDamageTypeEnum GetDamageType(int index)
    {
        switch (index)
        {
            default:
                Log.Warning($"Unknown DestinyDamageTypeEnum {index}");
                return DestinyDamageTypeEnum.None;
        }
    }
}

public enum DestinyDamageTypeEnum : int
{
    None = -1,
    [Description("Kinetic")]
    Kinetic,
    [Description(" Arc")]
    Arc,
    [Description(" Solar")]
    Solar,
    [Description(" Void")]
    Void,
    [Description(" Stasis")]
    Stasis,
    [Description(" Strand")]
    Strand
}

public enum MarathonTierType
{
    None = -1,
    Standard = 0,
    Enhanced = 1,
    Deluxe = 2,
    Superior = 3,
    Prestige = 4,
    Contraband = 5, // unsure if highest
}

// https://bungie-net.github.io/multi/schema_Destiny-DestinyUnlockValueUIStyle.html#schema_Destiny-DestinyUnlockValueUIStyle
// Pls update your api docs bungo, most dont match up
public enum DestinyUnlockValueUIStyle
{
    Automatic = 0,
    Checkbox = 1,
    DateTime = 2,
    Fraction = 3,
    Integer = 5,
    Percentage = 6,
    TimeDuration = 7,
    GreenPips = 9,
    RedPips = 10,
    Hidden = 11,
    RawFloat = 13,
}

public static class MarathonTierTypeColor
{
    private static readonly Dictionary<MarathonTierType, Color> Colors = new Dictionary<MarathonTierType, Color>
    {
        { MarathonTierType.None, Color.FromArgb(255, 56, 56, 56) },
        { MarathonTierType.Standard, Color.FromArgb(255, 200, 203, 210) },
        { MarathonTierType.Enhanced, Color.FromArgb(255, 1, 244, 134) },
        { MarathonTierType.Deluxe, Color.FromArgb(255, 74, 170, 255) },
        { MarathonTierType.Superior, Color.FromArgb(255, 195, 50, 255) },
        { MarathonTierType.Prestige, Color.FromArgb(255, 255, 100, 0) }, // Unknown atm
        { MarathonTierType.Contraband, Color.FromArgb(255, 200, 0, 0) } // Unknown atm
    };

    public static Color GetColor(this MarathonTierType tierType)
    {
        if (Colors.ContainsKey(tierType))
            return Colors[tierType];
        else
            return Colors[MarathonTierType.None];
    }
}
