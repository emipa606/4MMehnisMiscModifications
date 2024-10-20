using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Mehni.Misc.Modifications;

public class Alert_NPDBlocked : Alert
{
    public Alert_NPDBlocked()
    {
        defaultExplanation = "M4_NPDNeedsSpace_Desc".Translate();
        defaultLabel = "M4_NPDNeedsSpace".Translate();
    }

    // ReSharper disable once InconsistentNaming
    private static IEnumerable<Thing> BlockedNPDs
    {
        get
        {
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                IEnumerable<Building> npdBuildings =
                    map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.NutrientPasteDispenser);
                foreach (var building in npdBuildings)
                {
                    if (!building.InteractionCell.Standable(map))
                    {
                        yield return building;
                    }
                }
            }
        }
    }

    public override AlertReport GetReport()
    {
        return AlertReport.CulpritsAre(BlockedNPDs.ToList());
    }
}