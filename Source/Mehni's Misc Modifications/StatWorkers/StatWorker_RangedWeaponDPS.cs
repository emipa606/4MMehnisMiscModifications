using RimWorld;
using Verse;

namespace Mehni.Misc.Modifications;

public class StatWorker_RangedWeaponDPS : StatWorker
{
    private float Dist
    {
        get
        {
            if (stat == MeMiMo_StatDefOf.RangedWeapon_TouchDPS)
            {
                return ShootTuning.DistTouch;
            }

            if (stat == MeMiMo_StatDefOf.RangedWeapon_ShortDPS)
            {
                return ShootTuning.DistShort;
            }

            if (stat == MeMiMo_StatDefOf.RangedWeapon_MediumDPS)
            {
                return ShootTuning.DistMedium;
            }

            return stat == MeMiMo_StatDefOf.RangedWeapon_LongDPS ? ShootTuning.DistLong : 0f;
        }
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return MeMiMoSettings.displayRangedDPS &&
               req.Def is ThingDef { IsRangedWeapon: true } def && def.Verbs?[0]?.defaultProjectile != null &&
               def.Verbs[0].defaultProjectile.projectile.damageDef.harmsHealth;
    }

    public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq, bool finalized)
    {
        return value.ToStringByStyle(stat.toStringStyle, numberSense);
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return GetRangedDamagePerSecond(req);
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        var weapon = GetThingFromReq(req);

        var explanation = RangedWeaponDPSUtility.GetExplanation(weapon, Dist);

        if (req.Thing == null)
        {
            weapon.Destroy();
        }

        return explanation;
    }

    private float GetRangedDamagePerSecond(StatRequest req)
    {
        var weapon = GetThingFromReq(req);

        var DPS = RangedWeaponDPSUtility.GetDPS(weapon, Dist);

        if (req.Thing == null)
        {
            weapon.Destroy();
        }

        return DPS;
    }

    private Thing GetThingFromReq(StatRequest req)
    {
        var def = req.Def as ThingDef;
        return req.Thing ??
               ThingMaker.MakeThing(def, def is { MadeFromStuff: true } ? GenStuff.DefaultStuffFor(def) : null);
    }
}