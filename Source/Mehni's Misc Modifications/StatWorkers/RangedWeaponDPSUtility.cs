using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mehni.Misc.Modifications;

public static class RangedWeaponDPSUtility
{
    public static float GetDps(Thing weapon, float dist, Pawn pawn = null)
    {
        var verb = weapon.def.Verbs[0];
        var projectile = verb.defaultProjectile.projectile;
        var singleUse = typeof(Verb_ShootOneUse).IsAssignableFrom(verb.verbClass);

        var damage = getDamage(projectile, weapon);
        var cooldown = getCooldown(weapon, singleUse);
        var warmup = getWarmup(verb, pawn);
        var accuracy = getAccuracy(weapon, verb, projectile, dist, pawn);
        var burstCount = getBurstCount(verb);
        var burstShotDelay = getBurstShotDelay(verb);

        return getDps(damage, cooldown, warmup, accuracy, burstCount, burstShotDelay);
    }

    // Signature retains reference to projectile travel and explosion delay for backwards compatibility.
    private static float getDps(float damage, float cooldown, float warmup, float accuracy, int burstCount,
        float burstShotDelay)
    {
        return damage * burstCount * accuracy / (cooldown + warmup + ((burstCount - 1) * burstShotDelay));
    }

    public static string GetExplanation(Thing weapon, float dist, Pawn pawn = null)
    {
        var verb = weapon.def.Verbs[0];
        var projectile = verb.defaultProjectile.projectile;
        var singleUse = typeof(Verb_ShootOneUse).IsAssignableFrom(verb.verbClass);

        var damage = getDamage(projectile, weapon);
        var cooldown = getCooldown(weapon, singleUse);
        var warmup = getWarmup(verb, pawn);
        var accuracy = getAccuracy(weapon, verb, projectile, dist, pawn);
        var burstCount = getBurstCount(verb);
        var burstShotDelay = getBurstShotDelay(verb);
        var projectileTravelTime = getProjectileTravelTime(projectile, dist);
        var explosionDelay = getExplosionDelay(projectile);
        var bCMinOne = burstCount - 1;

        var expBuilder = new StringBuilder();

        // Damage - Is the weapon burst-fire or single-shot?
        expBuilder.AppendLine(
            burstCount > 1
                ? $"{"M4_DamagePerBurst".Translate()}: {burstCount * damage} ({burstCount} x {damage})"
                : $"{"Damage".Translate()}: {damage}");

        expBuilder.AppendLine($"{"Accuracy".Translate()}: {accuracy.ToStringPercent()}");
        expBuilder.AppendLine();

        // Cooldown and Warmup
        var singleUseText = singleUse ? " (" + "M4_WeaponSingleUse".Translate() + ")" : TaggedString.Empty;
        expBuilder.AppendLine($"{"CooldownTime".Translate()}: {cooldown:F2} s{singleUseText}");
        expBuilder.AppendLine(
            pawn != null
                ? $"{"WarmupTime".Translate()}: {warmup:F2} s ({verb.warmupTime:F2} x {pawn.GetStatValue(StatDefOf.AimingDelayFactor).ToStringPercent()})"
                : $"{"WarmupTime".Translate()}: {warmup:F2} s");

        // - delay between burst shots
        if (bCMinOne > 0)
        {
            expBuilder.AppendLine(
                $"{"M4_BurstShotDelay".Translate()}: {bCMinOne * burstShotDelay:F2} s ({bCMinOne} x {burstShotDelay:F2})");
        }

        // Total time
        var totalTime = cooldown + warmup + (bCMinOne * burstShotDelay);
        expBuilder.AppendLine($"{"M4_TotalTime".Translate()}: {totalTime:F2} s");

        // DPS result
        expBuilder.AppendLine();
        expBuilder.AppendLine($"DPS = ({burstCount * damage} x {accuracy.ToStringPercent()}) / {totalTime:F2} s");

        // Projectile 
        expBuilder.AppendLine();
        _ = expBuilder.AppendLine($"{"M4_ProjectileTravelTime".Translate()}: {projectileTravelTime:F2} s");
        if (explosionDelay > 0f)
        {
            _ = expBuilder.AppendLine($"{"M4_ProjectileExplosionDelay".Translate()}: {explosionDelay:F2} s");
        }

        return expBuilder.ToString();
    }

    private static float getDamage(ProjectileProperties projectile, Thing weapon)
    {
        return projectile.GetDamageAmount(weapon);
    }

    private static float getCooldown(Thing weapon, bool singleUse)
    {
        return singleUse ? 0f : weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown).SecondsToTicks().TicksToSeconds();
    }

    private static float getWarmup(VerbProperties verb, Pawn pawn = null)
    {
        var warmup = verb.warmupTime;
        if (pawn != null)
        {
            warmup *= pawn.GetStatValue(StatDefOf.AimingDelayFactor);
        }

        return warmup.SecondsToTicks().TicksToSeconds();
    }

    private static float getAccuracy(Thing weapon, VerbProperties verb, ProjectileProperties projectile, float dist,
        Pawn pawn = null)
    {
        var forcedMissRadius = calculateAdjustedForcedMissDist(verb.ForcedMissRadius, dist);
        var baseAimOn = verb.GetHitChanceFactor(weapon, dist);
        if (pawn != null)
        {
            baseAimOn *= ShotReport.HitFactorFromShooter(pawn, dist);
        }

        var affectedCellCount = verb.CausesExplosion ? GenRadial.NumCellsInRadius(projectile.explosionRadius) : 1;

        float accuracy;
        if (forcedMissRadius > 0.5f)
        {
            var affectableCellCount = GenRadial.NumCellsInRadius(forcedMissRadius);
            accuracy = (float)affectedCellCount / affectableCellCount;
        }
        else
        {
            var medianToWildRadius = ShootTuning.MissDistanceFromAimOnChanceCurves.Evaluate(baseAimOn, 0.5f);
            var indirectHitChance = (float)(affectedCellCount - 1) / GenRadial.NumCellsInRadius(medianToWildRadius);
            accuracy = baseAimOn + ((1f - baseAimOn) * indirectHitChance);
        }

        return Mathf.Clamp01(accuracy);
    }

    private static float calculateAdjustedForcedMissDist(float forcedMiss, float dist)
    {
        switch (dist)
        {
            case < 9f:
                return 0f;
            case < 25f:
                return forcedMiss * 0.5f;
            case < 49f:
                return forcedMiss * 0.8f;
            default:
                return forcedMiss;
        }
    }

    private static int getBurstCount(VerbProperties verb)
    {
        return verb.burstShotCount;
    }

    private static float getBurstShotDelay(VerbProperties verb)
    {
        return verb.ticksBetweenBurstShots.TicksToSeconds();
    }

    // 100 from Projectile.StartingTicksToImpact
    private static float getProjectileTravelTime(ProjectileProperties projectile, float dist)
    {
        return Mathf.RoundToInt(Math.Max(dist / (projectile.speed / 100), 1)).TicksToSeconds();
    }

    private static float getExplosionDelay(ProjectileProperties projectile)
    {
        return projectile.explosionDelay.TicksToSeconds();
    }
}