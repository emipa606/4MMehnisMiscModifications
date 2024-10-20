using System;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mehni.Misc.Modifications;

public static class RangedWeaponDPSUtility
{
    public static float GetDPS(Thing weapon, float dist, Pawn pawn = null)
    {
        var verb = weapon.def.Verbs[0];
        var projectile = verb.defaultProjectile.projectile;
        var singleUse = typeof(Verb_ShootOneUse).IsAssignableFrom(verb.verbClass);

        var damage = GetDamage(projectile, weapon);
        var cooldown = GetCooldown(weapon, singleUse);
        var warmup = GetWarmup(verb, pawn);
        var accuracy = GetAccuracy(weapon, verb, projectile, dist, pawn);
        var burstCount = GetBurstCount(verb);
        var burstShotDelay = GetBurstShotDelay(verb);

        return GetDPS(damage, cooldown, warmup, accuracy, burstCount, burstShotDelay);
    }

    // Signature retains reference to projectile travel and explosion delay for backwards compatibility.
    public static float GetDPS(float damage, float cooldown, float warmup, float accuracy, int burstCount,
        float burstShotDelay)
    {
        return damage * burstCount * accuracy / (cooldown + warmup + ((burstCount - 1) * burstShotDelay));
    }

    public static string GetExplanation(Thing weapon, float dist, Pawn pawn = null)
    {
        var verb = weapon.def.Verbs[0];
        var projectile = verb.defaultProjectile.projectile;
        var singleUse = typeof(Verb_ShootOneUse).IsAssignableFrom(verb.verbClass);

        var damage = GetDamage(projectile, weapon);
        var cooldown = GetCooldown(weapon, singleUse);
        var warmup = GetWarmup(verb, pawn);
        var accuracy = GetAccuracy(weapon, verb, projectile, dist, pawn);
        var burstCount = GetBurstCount(verb);
        var burstShotDelay = GetBurstShotDelay(verb);
        var projectileTravelTime = GetProjectileTravelTime(projectile, dist);
        var explosionDelay = GetExplosionDelay(projectile);
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

    private static float GetDamage(ProjectileProperties projectile, Thing weapon)
    {
        return projectile.GetDamageAmount(weapon);
    }

    private static float GetCooldown(Thing weapon, bool singleUse)
    {
        return singleUse ? 0f : weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown).SecondsToTicks().TicksToSeconds();
    }

    private static float GetWarmup(VerbProperties verb, Pawn pawn = null)
    {
        var warmup = verb.warmupTime;
        if (pawn != null)
        {
            warmup *= pawn.GetStatValue(StatDefOf.AimingDelayFactor);
        }

        return warmup.SecondsToTicks().TicksToSeconds();
    }

    private static float GetAccuracy(Thing weapon, VerbProperties verb, ProjectileProperties projectile, float dist,
        Pawn pawn = null)
    {
        var forcedMissRadius = CalculateAdjustedForcedMissDist(verb.ForcedMissRadius, dist);
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

    private static float CalculateAdjustedForcedMissDist(float forcedMiss, float dist)
    {
        if (dist < 9f)
        {
            return 0f;
        }

        if (dist < 25f)
        {
            return forcedMiss * 0.5f;
        }

        if (dist < 49f)
        {
            return forcedMiss * 0.8f;
        }

        return forcedMiss;
    }

    private static int GetBurstCount(VerbProperties verb)
    {
        return verb.burstShotCount;
    }

    private static float GetBurstShotDelay(VerbProperties verb)
    {
        return verb.ticksBetweenBurstShots.TicksToSeconds();
    }

    // 100 from Projectile.StartingTicksToImpact
    private static float GetProjectileTravelTime(ProjectileProperties projectile, float dist)
    {
        return Mathf.RoundToInt(Math.Max(dist / (projectile.speed / 100), 1)).TicksToSeconds();
    }

    private static float GetExplosionDelay(ProjectileProperties projectile)
    {
        return projectile.explosionDelay.TicksToSeconds();
    }
}