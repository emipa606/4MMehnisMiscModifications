using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Mehni.Misc.Modifications;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
    internal static readonly IntRange AnimalsCount = new IntRange(30, 50);

    //[TweakValue("AAAAMehniMiscMods", 0f, 1f)]
    //private static readonly float resizeHeart = 0.50f;

    //private static readonly float offsetPosition = 0.62f;

    private static bool returnvalue;

    private static readonly Texture2D BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond");
    private static readonly Texture2D BondBrokenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/BondBroken");

    //[TweakValue("AAAMehniMiscMods", -50f, 50f)]
    private static readonly float widthOffset = -19f;

    //[TweakValue("AAAMehniMiscMods", -50f, 50f)]
    private static readonly float xOffset = -21f;

    //[TweakValue("AAAMehniMiscMods", -50f, 50f)]
    private static readonly float yOffset = -2.25f;

    static HarmonyPatches()
    {
        var harmony = new Harmony("Mehni.RimWorld.4M.Main");
#if DEBUG
        Harmony.DEBUG = true;
#endif
        harmony.Patch(
            AccessTools.Method(
                typeof(IncidentWorker_HerdMigration),
                "GenerateAnimals"
            ),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(BigHerds_Transpiler)));

        harmony.Patch(
            AccessTools.Method(
                typeof(AutoUndrafter),
                "ShouldAutoUndraft"
            ),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(StayWhereIPutYou_Postfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Lord),
                nameof(Lord.SetJob)
            ),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(FleeTrigger_PostFix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(AggressiveAnimalIncidentUtility),
                nameof(AggressiveAnimalIncidentUtility.AnimalWeight)
            ),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(BigManhunterPackFix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(PlantProperties),
                "SpecialDisplayStats"
            ),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DisplayYieldInfo)));

        harmony.Patch(
            AccessTools.Method(
                typeof(StartingPawnUtility),
                "NewGeneratedStartingPawn"
            ),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(NewGeneratedStartingPawns_Transpiler)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Dialog_AssignBuildingOwner),
                nameof(Dialog_AssignBuildingOwner.DoWindowContents)
            ),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoWindowContents_Transpiler)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Pawn_HealthTracker),
                nameof(Pawn_HealthTracker.NotifyPlayerOfKilled)
            ),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(NotifyPlayerOfKilledAnimal_Prefix)));

        //harmony.Patch(AccessTools.Method(typeof(Building_Turret), "OnAttackedTarget"), null, null,
        //    new HarmonyMethod(typeof(HarmonyPatches), nameof(OnAttackedTarget_Transpiler)));

        harmony.Patch(
            AccessTools.Method(
                typeof(FoodUtility),
                nameof(FoodUtility.GetPreyScoreFor)
            ),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GetPreyScoreFor_Postfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(WorkGiver_InteractAnimal),
                nameof(WorkGiver_InteractAnimal.CanInteractWithAnimal),
                [
                    typeof(Pawn), typeof(Pawn), typeof(string).MakeByRefType(), typeof(bool), typeof(bool),
                    typeof(bool), typeof(bool)
                ]
            ),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CanInteractWithAnimal_Postfix)));

        harmony.Patch(
            AccessTools.PropertyGetter(
                typeof(Dialog_MessageBox),
                "InteractionDelayExpired"
            ),
            postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(YesImAModderStopAskingMe)));

        harmony.Patch(
            AccessTools.Method(
                typeof(DebugThingPlaceHelper),
                nameof(DebugThingPlaceHelper.DebugSpawn)
            ),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(TranspileDebugSpawn)));

        //harmony.Patch(AccessTools.Method(typeof(RelationsUtility), nameof(RelationsUtility.IsDisfigured)), null,
        //    new HarmonyMethod(typeof(HarmonyPatches), nameof(IsDisfigured_Postfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Page_ConfigureStartingPawns),
                "DoNext"
            ),
            new HarmonyMethod(typeof(HarmonyPatches), nameof(ConfigureStartingPawnsDoNextPrefix)));

        //harmony.Patch(AccessTools.Method(typeof(IncidentWorker_RefugeeChased), "TryExecuteWorker"),
        //    transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(TryExecuteWorker_Transpiler)));

        //harmony.Patch(
        //    original: AccessTools.Method(typeof(PawnUIOverlay), "DrawPawnGUIOverlay"),
        //    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawPawnGUIOverlay_Postfix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Listing_TreeThingFilter),
                "DoThingDef"
            ),
            transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoThingDef_Transpiler)));
    }

    public static bool NoNonviolent => TutorSystem.TutorialMode || MeMiMoSettings.enableTutorialStyleRolling;

    public static IEnumerable<CodeInstruction> BigHerds_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        if (MeMiMoSettings.bigAnimalMigrations)
        {
            var instructionList = instructions.ToList();
            var useMoreAnimals = AccessTools.Field(typeof(HarmonyPatches), nameof(AnimalsCount));

            foreach (var codeInstruction in instructionList)
            {
                if (codeInstruction.opcode == OpCodes.Ldsfld)
                {
                    codeInstruction.operand = useMoreAnimals;
                }
                //uses AnimalsCount in 4M instead of default.

                yield return codeInstruction;
            }
        }
        else
        {
            foreach (var item in instructions)
            {
                yield return item;
            }
        }
    }

    private static void FleeTrigger_PostFix(Lord __instance, ref LordJob lordJob)
    {
        if (!MeMiMoSettings.variableRaidRetreat ||
            __instance.faction == null ||
            __instance.faction.IsPlayer ||
            !__instance.faction.def.autoFlee ||
            __instance.faction.neverFlee ||
            !lordJob.AddFleeToil)
        {
            return;
        }

        var randomRetreatvalue = Rand.RangeSeeded(
            MeMiMoSettings.retreatDefeatRange.min,
            MeMiMoSettings.retreatDefeatRange.max,
            __instance.loadID);

        foreach (var transition in lordJob.lord.Graph.transitions)
        {
            if (transition.target is not LordToil_PanicFlee)
            {
                continue;
            }

            for (var i = 0; i < transition.triggers.Count; i++)
            {
                if (transition.triggers[i] is Trigger_FractionPawnsLost)
                {
                    transition.triggers[i] =
                        new Trigger_FractionPawnsLost(randomRetreatvalue);
                }
            }
        }
    }

    private static bool BigManhunterPackFix(PawnKindDef animal, float points, ref float __result)
    {
        //6000 is based on the Manhunter results table in the devtools. At around 6~7k points, there's only one or two critters dangerous enough.
        if (!MeMiMoSettings.enableLargePacks || !(points >= 6000))
        {
            return true;
        }

        if (animal.combatPower > 89)
        {
            __result = 1f;
        }

        return false;
    }

    //Thanks to XeoNovaDan
    public static void DisplayYieldInfo(PlantProperties __instance, ref IEnumerable<StatDrawEntry> __result)
    {
        if (__instance.isStump)
        {
            return;
        }

        var harvestedThingDef = Traverse.Create(__instance).Field("harvestedThingDef").GetValue<ThingDef>();
        var harvestYield = Traverse.Create(__instance).Field("harvestYield").GetValue<float>();

        if (harvestedThingDef == null)
        {
            return;
        }

        var harvestedThingDefLabel = harvestedThingDef.label;

        var extendedYieldInfo = string.Format("M4_HarvestYieldThingDetailInit".Translate(), harvestedThingDefLabel) +
                                "\n\n";
        var thingMarketValue = harvestedThingDef.GetStatValueAbstract(StatDefOf.MarketValue);
        extendedYieldInfo += StatDefOf.MarketValue.label.CapitalizeFirst() + ": " + thingMarketValue;
        if (harvestedThingDef.IsNutritionGivingIngestible)
        {
            var thingNutrition = harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition);
            var thingNutritionType = harvestedThingDef.ingestible.foodType;
            IDictionary<FoodTypeFlags, string> nutritionTypeToReportString = new Dictionary<FoodTypeFlags, string>
            {
                { FoodTypeFlags.VegetableOrFruit, "FoodTypeFlags_VegetableOrFruit" },
                { FoodTypeFlags.Meat, "FoodTypeFlags_Meat" }, { FoodTypeFlags.Seed, "FoodTypeFlags_Seed" }
            };
            string nutritionTypeReportString =
                nutritionTypeToReportString.TryGetValue(thingNutritionType, out nutritionTypeReportString)
                    ? nutritionTypeReportString
                    : "StatsReport_OtherStats";
            extendedYieldInfo += "\n" + StatDefOf.Nutrition.label.CapitalizeFirst() + ": " + thingNutrition +
                                 " (" + nutritionTypeReportString.Translate() + ")";
        }

        if (!(harvestYield > 0))
        {
            return;
        }

        var statDrawEntry = new StatDrawEntry(StatCategoryDefOf.Basics, "M4_HarvestYieldThing".Translate(),
            harvestedThingDef.label.CapitalizeFirst(), extendedYieldInfo, 0);
        __result = __result.AddItem(statDrawEntry);
    }

    private static bool NotifyPlayerOfKilledAnimal_Prefix(Pawn ___pawn)
    {
        return !___pawn.RaceProps.Animal || MeMiMoSettings.deathMessagesForAnimals;
    }


    private static void StayWhereIPutYou_Postfix(ref bool __result, int ___lastNonWaitingTick, Pawn ___pawn)
    {
        if (!MeMiMoSettings.modifyAutoUndrafter)
        {
            return;
        }

        if (!__result)
        {
            return;
        }

        if (!MentalBreakHelper(___pawn))
        {
            // As of RW 1.4: private const int UndraftDelay = 10000;
            __result =
                Find.TickManager.TicksGame - ___lastNonWaitingTick >= 10000 + MeMiMoSettings.extendUndraftTimeBy
                && GunsFiringHelper();
        }
    }

    private static bool GunsFiringHelper()
    {
        if (!MeMiMoSettings.whenGunsAreFiring)
        {
            return true;
        }

        return !Find.SoundRoot.oneShotManager.PlayingOneShots.Any(s
            => s.subDef.parentDef == DefOf_M4.BulletImpact_Flesh
               || s.subDef.parentDef == DefOf_M4.BulletImpact_Metal
               || s.subDef.parentDef == DefOf_M4.BulletImpact_Wood
               || s.subDef.parentDef == SoundDefOf.BulletImpact_Ground);
    }

    private static bool MentalBreakHelper(Pawn pawn)
    {
        if (!MeMiMoSettings.allowAutoUndraftAtLowMood)
        {
            return false;
        }

        switch (MeMiMoSettings.dontExtendWhenMoodAt)
        {
            case "0":
                return pawn.mindState.mentalBreaker.BreakMinorIsImminent ||
                       pawn.mindState.mentalBreaker.BreakMajorIsImminent ||
                       pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
            case "1":
                return pawn.mindState.mentalBreaker.BreakMajorIsImminent ||
                       pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
            case "2":
                return pawn.mindState.mentalBreaker.BreakExtremeIsImminent;
        }

        return false;
    }

    public static IEnumerable<CodeInstruction>
        NewGeneratedStartingPawns_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        List<CodeInstruction> Li_inst = [..instructions];
        var idx_insert = -1;
        var label_pawn_eq_null = il.DefineLabel();
        for (var i = 0; i < Li_inst.Count - 1; i++)
        {
            if (Li_inst[i].opcode != OpCodes.Ldnull || Li_inst[i + 1].opcode != OpCodes.Stloc_1)
            {
                continue;
            }

            idx_insert = i;
            Li_inst[i].labels.Add(label_pawn_eq_null);
            break;
        }

        if (idx_insert == -1)
        {
            return instructions;
        }

        List<CodeInstruction> Li_instInsert =
        [
            new CodeInstruction(
                OpCodes.Call,
                AccessTools.PropertyGetter(typeof(HarmonyPatches), nameof(NoNonviolent))
            ),

            new CodeInstruction(
                OpCodes.Brfalse_S,
                label_pawn_eq_null
            ),
            // request.MustBeCapableOfViolence = true;

            new CodeInstruction(
                OpCodes.Ldloca_S,
                0
            ), // &request

            new CodeInstruction(
                OpCodes.Ldc_I4_1
            ),

            new CodeInstruction(
                OpCodes.Call,
                AccessTools.PropertySetter(typeof(PawnGenerationRequest),
                    nameof(PawnGenerationRequest.MustBeCapableOfViolence))
            )
        ];

        // if(NoNonviolent)
        // request.MustBeCapableOfViolence = true;

        Li_inst.InsertRange(idx_insert, Li_instInsert);
        return Li_inst;
    }

    public static bool ConfigureStartingPawnsDoNextPrefix(Page_ConfigureStartingPawns __instance, int ___curPawnIndex)
    {
        var runMe = AccessTools.Method(typeof(Page_ConfigureStartingPawns), "DoNext");

        if (___curPawnIndex < Find.GameInitData.startingPawnCount)
        {
            return true;
        }

        var selectedPawn = Find.GameInitData.startingAndOptionalPawns[___curPawnIndex];
        Find.WindowStack.Add(
            Dialog_MessageBox.CreateConfirmation(
                "M4_HaveNotDraggedColonist".Translate(selectedPawn.LabelCap),
                () =>
                {
                    returnvalue = true;
                    runMe.Invoke(__instance, []);
                }
            )
        );

        return returnvalue;
    }

    public static IEnumerable<CodeInstruction>
        DoWindowContents_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        var label = il.DefineLabel();
        var widgetsLabel = AccessTools.Method(
            typeof(Widgets),
            nameof(Widgets.Label),
            [typeof(Rect), typeof(string)]
        );
        var thingIcon = AccessTools.Method(
            typeof(Widgets),
            nameof(Widgets.ThingIcon),
            [typeof(Rect), typeof(Thing), typeof(float), typeof(Rot4?), typeof(bool)]
        );
        var ldc_r4_35f = new CodeInstruction(OpCodes.Ldc_R4, 35f);

        var Li_inst = instructions.ToList();

        var f_rectAggregator = true;
        bool f_NewCol_AssignedPawnsForReading = true, f_Heart_AssignedPawnsForReading = true;
        bool f_NewCol_AssigningCandidates = true, f_Heart_AssigningCandidates = true;
        var Start_NewCol_AssignedPawnsForReading = -1;
        var Start_NewCol_AssigningCandidates = 1;

        // We want to inject the heart painting codes at 2 locations:
        // 1. foreach assignable.AssignedPawnsForReading
        // 2. foreach assignable.AssigningCandidates
        for (var i = 0; i < Li_inst.Count; i++)
        {
            // After "rectAggregator.NewCol(24f);",
            // insert another "rectAggregator.NewCol(35f);"
            if (f_rectAggregator && i >= 4 && i < Li_inst.Count - 3
                && Li_inst[i].opcode == OpCodes.Pop
                && Li_inst[i - 2].opcode == OpCodes.Ldc_I4_1
                && Li_inst[i + 3].opcode == OpCodes.Ldc_I4_1
               )
            {
                yield return Li_inst[i];
                Li_inst[i - 3] = ldc_r4_35f;
                foreach (var inst in Li_inst.GetRange(i - 4, 5))
                {
                    yield return inst;
                }

                f_rectAggregator = false;
            }

            // Record the range of instructions that call the parameters
            // "rectDivider2.NewCol(24f), item4"

            else if (f_NewCol_AssignedPawnsForReading && i >= 5 && i < Li_inst.Count - 6
                     && Li_inst[i].opcode == OpCodes.Ldloc_S
                     && Li_inst[i + 6].Calls(thingIcon)
                    )
            {
                yield return Li_inst[i];
                Start_NewCol_AssignedPawnsForReading = i - 5;

                f_NewCol_AssignedPawnsForReading = false;
            }

            // Record the range of instructions that call the parameters
            // "rectDivider2.NewCol(24f), pawn"
            else if (f_NewCol_AssigningCandidates && i >= 5 && i < Li_inst.Count - 7
                     && Li_inst[i].opcode == OpCodes.Ldloc_S
                     && Li_inst[i + 7].Calls(thingIcon)
                    )
            {
                yield return Li_inst[i];
                Start_NewCol_AssigningCandidates = i - 5;

                f_NewCol_AssigningCandidates = false;
            }

            // insert instructions that call the parameters
            // before "Widgets.Label(rectDivider2, tmpPawnName[item4]);"
            else if (f_Heart_AssignedPawnsForReading && i < Li_inst.Count - 6
                                                     && Li_inst[i + 3].opcode == OpCodes.Ldsfld
                                                     && Li_inst[i + 6].Calls(widgetsLabel)
                    )
            {
                yield return Li_inst[i];
                Li_inst[Start_NewCol_AssignedPawnsForReading + 1] = ldc_r4_35f;
                foreach (var inst in
                         InsertDrawHeart(
                             label,
                             Li_inst.GetRange(Start_NewCol_AssignedPawnsForReading, 6)
                         )
                        )
                {
                    yield return inst;
                }

                f_Heart_AssignedPawnsForReading = false;
            }

            // insert instructions that call parameters
            // before "Widgets.Label(rectDivider3, tmpPawnName[pawn]);"
            else if (f_Heart_AssigningCandidates && i < Li_inst.Count - 7
                                                 && Li_inst[i + 3].opcode == OpCodes.Ldsfld
                                                 && Li_inst[i + 7].Calls(widgetsLabel)
                    )
            {
                yield return Li_inst[i];
                Li_inst[Start_NewCol_AssigningCandidates + 1] = ldc_r4_35f;
                foreach (var inst in
                         InsertDrawHeart(
                             label,
                             Li_inst.GetRange(Start_NewCol_AssigningCandidates, 7)
                         )
                        )
                {
                    yield return inst;
                }

                f_Heart_AssigningCandidates = false;
            }

            else
            {
                if (Li_inst[i].Calls(
                        AccessTools.Method(typeof(Widgets), nameof(Widgets.EndScrollView))
                    ))
                {
                    yield return Li_inst[i].WithLabels(label);
                }
                else
                {
                    yield return Li_inst[i];
                }
            }
        }
    }

    private static IEnumerable<CodeInstruction>
        InsertDrawHeart(Label label, List<CodeInstruction> inst_loadParams)
    {
        // if(ShowHeart(rectDivider2.NewCol(24f), pawn))
        //     return;
        foreach (var inst in inst_loadParams)
        {
            yield return inst;
        }

        yield return new CodeInstruction(
            OpCodes.Call,
            AccessTools.Method(typeof(HarmonyPatches), nameof(ShowHeart))
        );
        yield return new CodeInstruction(
            OpCodes.Brtrue,
            label
        );
    }

    private static bool ShowHeart(Rect rect, Pawn pawn)
    {
        Texture2D iconFor;
        if (pawn is not { IsColonist: true })
        {
            return false;
        }

        var directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(pawn, false);
        if (directPawnRelation == null || directPawnRelation.otherPawn == null)
        {
            iconFor = null;
        }

        else if (!directPawnRelation.otherPawn.IsColonist || directPawnRelation.otherPawn.IsWorldPawn() ||
                 !directPawnRelation.otherPawn.relations.everSeenByPlayer)
        {
            iconFor = null;
        }

        else if (pawn.ownership?.OwnedBed != null &&
                 pawn.ownership?.OwnedBed == directPawnRelation.otherPawn.ownership?.OwnedBed)
        {
            iconFor = BondIcon;
        }

        else
        {
            iconFor = BondBrokenIcon;
        }

        if (iconFor == null)
        {
            return false;
        }

        TooltipHandler.TipRegion(rect, directPawnRelation.otherPawn.LabelCap);

        // if it's a broken heart - allow them to click on the broken heart to assign the missing partner to the bed
        if (iconFor == BondBrokenIcon)
        {
            if (!Widgets.ButtonImage(rect, iconFor, Color.white, Color.red))
            {
                return false;
            }

            if (pawn.ownership?.OwnedBed?.SleepingSlotsCount >= 2)
            {
                pawn.ownership.OwnedBed.GetComp<CompAssignableToPawn>()
                    .TryAssignPawn(directPawnRelation.otherPawn);
                Text.Anchor = TextAnchor.UpperLeft; // A warning will popup without this line
                return true;
            }

            Messages.Message("M4_LoverWarning".Translate(pawn.LabelCap), MessageTypeDefOf.NeutralEvent, false);
        }
        else
        {
            GUI.DrawTexture(rect, iconFor);
        }

        return false;
    }

    //Courtesy XND

    // 'Totally didn't almost forget to actually copypaste the testing code' edition
    public static void GetPreyScoreFor_Postfix(Pawn predator, Pawn prey, ref float __result)
    {
        if (predator.Faction == Faction.OfPlayer
            && MeMiMoSettings.guardingPredatorsDeferHuntingTameDesignatedAnimals
            && predator.training.HasLearned(TrainableDefOf.Obedience)
            && prey.Map.designationManager.DesignationOn(prey, DesignationDefOf.Tame) != null)
        {
            __result -= 35f;
        }
    }

    public static void CanInteractWithAnimal_Postfix(Pawn animal, bool canInteractWhileSleeping, ref bool __result)
    {
        var hourInteger =
            GenDate.HourInteger(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(animal.MapHeld.Tile).x);
        if (canInteractWhileSleeping || hourInteger < MeMiMoSettings.animalInteractionHourLimit || !__result)
        {
            return;
        }

        JobFailReason.Is("M4_CantInteractAnimalWillFallAsleepSoon".Translate());
        __result = false;
    }

    //#region HideDisfigurement
    //public static void IsDisfigured_Postfix(ref bool __result, Pawn pawn)
    //{
    //    if (MeMiMoSettings.apparelHidesDisfigurement && Find.TickManager.TicksGame % 200 == 0 && __result)
    //    {
    //        List<Apparel> wornApparel = pawn.apparel.WornApparel;
    //        List<Hediff> disfiguringHediffs = pawn.health.hediffSet.hediffs.Where(h => h.Part.def.beautyRelated).ToList();
    //        List<bool?> eachHediffCovered = new List<bool?>();
    //        // Stairway to heaven
    //        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
    //        {
    //            foreach (BodyPartGroupDef hediffGroup in hediff.Part.groups)
    //                foreach (Apparel apparel in wornApparel)
    //                {
    //                    foreach (BodyPartGroupDef apparelGroup in apparel.def.apparel.bodyPartGroups)
    //                        if (apparelGroup == hediffGroup)
    //                        {
    //                            eachHediffCovered.Add(true);
    //                            goto NextHediff;
    //                        }
    //                    eachHediffCovered.Add(false);
    //                }
    //            NextHediff:;
    //        }
    //        __result = eachHediffCovered.First(b => false) == null;
    //    }
    //}
    //#endregion

    private static void YesImAModderStopAskingMe(ref bool __result)
    {
        if (MeMiMoSettings.iAmAModder)
        {
            __result = true;
        }

        Debug.unityLogger.logEnabled = false;
    }

    public static IEnumerable<CodeInstruction> TranspileDebugSpawn(IEnumerable<CodeInstruction> instructions)
    {
        var instructionList = instructions.ToList();

        var randomStuffFor = AccessTools.Method(typeof(GenStuff), nameof(GenStuff.RandomStuffFor));
        var getStuffDefFromSettings = AccessTools.Method(typeof(HarmonyPatches), nameof(GetStuffDefFromSettings));

        var generateQualityRandomEqualChance = AccessTools.Method(typeof(QualityUtility),
            nameof(QualityUtility.GenerateQualityRandomEqualChance));
        var generateQualityFromSettings =
            AccessTools.Method(typeof(HarmonyPatches), nameof(GenerateQualityFromSettings));

        foreach (var instruction in instructionList)
        {
            if (instruction.opcode == OpCodes.Call)
            {
                if (instruction.Calls(randomStuffFor))
                {
                    instruction.operand = getStuffDefFromSettings;
                }
                else if (instruction.Calls(generateQualityRandomEqualChance))
                {
                    instruction.operand = generateQualityFromSettings;
                }
            }

            yield return instruction;
        }
    }

    public static ThingDef GetStuffDefFromSettings(ThingDef def)
    {
        if (MeMiMoSettings.iAmAModder && def.MadeFromStuff && MeMiMoSettings.chooseItemStuff &&
            MeMiMoSettings.stuffDefName != "" && DefDatabase<ThingDef>.GetNamed(MeMiMoSettings.stuffDefName) != null)
        {
            return DefDatabase<ThingDef>.GetNamed(MeMiMoSettings.stuffDefName);
        }

        return GenStuff.RandomStuffFor(def);
    }

    public static QualityCategory GenerateQualityFromSettings()
    {
        if (!MeMiMoSettings.iAmAModder || !MeMiMoSettings.forceItemQuality)
        {
            return QualityUtility.GenerateQualityRandomEqualChance();
        }

        return MeMiMoSettings.forcedItemQuality switch
        {
            0 => QualityCategory.Awful,
            1 => QualityCategory.Poor,
            3 => QualityCategory.Good,
            4 => QualityCategory.Excellent,
            5 => QualityCategory.Masterwork,
            6 => QualityCategory.Legendary,
            _ => QualityCategory.Normal
        };
    }

    // Thanks Mehni... :sadwinnie:
    /*
    public static IEnumerable<CodeInstruction> TryExecuteWorker_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> instructionList = instructions.ToList();

        MethodInfo alteredHostileFactionPawnReadout = AccessTools.Method(typeof(HarmonyPatches), nameof(AlteredHostileFactionPawnReadout));

        for (int i = 0; i < instructionList.Count; i++)
        {
            CodeInstruction instruction = instructionList[i];

            if (instruction.LoadsField(AccessTools.Field(typeof(FactionDef), "pawnsPlural")))
            {
                yield return new CodeInstruction(OpCodes.Ldloc_S, 5);

                instruction.opcode = OpCodes.Call;
                instruction.operand = alteredHostileFactionPawnReadout;
            }

            yield return instruction;
        }
    }

    private static string AlteredHostileFactionPawnReadout(FactionDef faction, IEnumerable<PawnKindDef> pawnKinds)
    {
        // Warning: nested ternaries ahead
        int pawnCount = pawnKinds.Count();
        return ((MeMiMoSettings.betterHostileReadouts) ? pawnCount.ToString() + " " + ((pawnCount > 1) ? faction.pawnsPlural : faction.pawnSingular) : faction.pawnsPlural);
    }

    public static void DrawPawnGUIOverlay_Postfix(Pawn ___pawn)
    {
        // First two checks are just to prevent duplicates
        if (MeMiMoSettings.betterHostileReadouts && !___pawn.RaceProps.Humanlike && ___pawn.Faction != Faction.OfPlayer && ___pawn.HostileTo(Faction.OfPlayer))
            GenMapUI.DrawPawnLabel(___pawn, GenMapUI.LabelDrawPosFor(___pawn, -0.6f), font: GameFont.Tiny);
    }
    */

    public static IEnumerable<CodeInstruction> DoThingDef_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionList = instructions.ToList();
        var lWidthCalls = 0;
        var done = false;

        foreach (var codeInstruction in instructionList)
        {
            var instruction = codeInstruction;

            if (instruction.Calls(AccessTools.Property(typeof(Listing_Tree), "LabelWidth").GetGetMethod(true)))
            {
                lWidthCalls++;
            }

            if (lWidthCalls != 3 &&
                instruction.Calls(AccessTools.Property(typeof(Listing_Tree), "LabelWidth").GetGetMethod(true)))
            {
                yield return instruction;
                instruction = new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(HarmonyPatches), nameof(AdjustedWidth)));
            }

            // Wasn't able to get Widgets.Checkbox since AccessTools.Method returned null even with the correct overload params
            if (!done && instruction.Calls(AccessTools.Method(typeof(Widgets), nameof(Widgets.Checkbox),
                [
                    typeof(Vector2), typeof(bool).MakeByRefType(), typeof(float), typeof(bool), typeof(bool),
                    typeof(Texture2D), typeof(Texture2D)
                ])))
            {
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.Property(typeof(Listing_Tree), "LabelWidth").GetGetMethod(true));
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld,
                    AccessTools.Field(typeof(Listing_TreeThingFilter), "curY"));
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                instruction = new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(HarmonyPatches), nameof(DoInfoCardButton)));
                done = true;
            }

            yield return instruction;
        }
    }

    private static float AdjustedWidth(float width)
    {
        return width + (MeMiMoSettings.thingFilterInfoCards ? widthOffset : 0f);
    }


    private static void DoInfoCardButton(float x, float y, ThingDef def)
    {
        if (MeMiMoSettings.thingFilterInfoCards)
        {
            Widgets.InfoCardButton(x + xOffset, y + yOffset, def);
        }
    }
}