using Mlie;
using UnityEngine;
using Verse;

namespace Mehni.Misc.Modifications;

public class MehniMiscMods : Mod
{
    public static string currentVersion;

    public MehniMiscMods(ModContentPack content) : base(content)
    {
        GetSettings<MeMiMoSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "Mehni's Miscellaneous Modifications";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        GetSettings<MeMiMoSettings>().DoWindowContents(inRect);
        base.DoSettingsWindowContents(inRect);
    }
}