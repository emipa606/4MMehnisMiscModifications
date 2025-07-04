using RimWorld;
using Verse;

namespace Mehni.Misc.Modifications;

public class StatWorker_RangedWeaponDPSPawn : StatWorker
{
    private float Dist
    {
        get
        {
            if (stat == MeMiMo_StatDefOf.RangedWeapon_TouchDPSPawn)
            {
                return ShootTuning.DistTouch;
            }

            if (stat == MeMiMo_StatDefOf.RangedWeapon_ShortDPSPawn)
            {
                return ShootTuning.DistShort;
            }

            if (stat == MeMiMo_StatDefOf.RangedWeapon_MediumDPSPawn)
            {
                return ShootTuning.DistMedium;
            }

            return stat == MeMiMo_StatDefOf.RangedWeapon_LongDPSPawn ? ShootTuning.DistLong : 0f;
        }
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return MeMiMoSettings.displayRangedDPS &&
               req.Thing is Pawn { equipment.Primary: not null } pawn &&
               pawn.equipment.Primary.def.IsRangedWeapon &&
               pawn.equipment.Primary.def.Verbs[0].defaultProjectile?.projectile != null;
    }

    public override bool IsDisabledFor(Thing thing)
    {
        return base.IsDisabledFor(thing) || StatDefOf.ShootingAccuracyPawn.Worker.IsDisabledFor(thing);
    }

    public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense,
        StatRequest optionalReq, bool finalized = true)
    {
        return value.ToStringByStyle(stat.toStringStyle, numberSense);
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return getRangedDamagePerSecond(req);
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        var pawn = req.Thing as Pawn;
        Thing weapon = pawn?.equipment.Primary;
        return RangedWeaponDPSUtility.GetExplanation(weapon, Dist, pawn);
    }

    private float getRangedDamagePerSecond(StatRequest req)
    {
        var pawn = req.Thing as Pawn;
        Thing weapon = pawn?.equipment?.Primary;

        if (weapon == null || !weapon.def.IsRangedWeapon)
        {
            return 0f;
        }

        return RangedWeaponDPSUtility.GetDps(weapon, Dist, pawn);
    }
}