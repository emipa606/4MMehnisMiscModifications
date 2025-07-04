using RimWorld;
using UnityEngine;
using Verse;

namespace Mehni.Misc.Modifications;

public class MeMiMoSettings : ModSettings
{
    // BigAnimalMigrations
    public static bool bigAnimalMigrations;

    // BigManhunterPacks
    public static bool enableLargePacks = true;

    // RerollingPawns
    public static bool enableTutorialStyleRolling = true;

    // DeathMessages
    public static bool deathMessagesForAnimals = true;

    // WorkAssignmentMatters
    public static bool workAssignmentMatters;

    // DisplayRangedWeaponDPS
    public static bool displayRangedDPS = true;

    // ThingFilterInfoCards
    public static bool thingFilterInfoCards = true;

    //#region HideDisfigurement
    //public static bool apparelHidesDisfigurement = true;
    //#endregion

    //[TweakValue("AAAMehniMiscMods")]
    private static readonly float yPos = 43f;

    // Value to modify when adding new settings, pushing the scrollview down.
    //[TweakValue("AAAMehniMiscMods", max: 500f)]
    private static readonly float moreOptionsRecty = 210f;

    //[TweakValue("AAAMehniMiscMods")]
    private static readonly float widthFiddler = 9f;

    //value to modify when adding more settings to the scrollview.
    //[TweakValue("AAAMehniMiscMods", 0, 1200f)]
    private static readonly float viewHeight = 620f;

    //Value where the rect stops.
    //[TweakValue("AAAMehniMiscMods", 0, 1200f)]
    private static readonly float yMax = 620f;

    //Do not touch.
    //[TweakValue("AAAMehniMiscMods", 0, 1200f)]
    private static readonly float height = 640f;

    private static Vector2 scrollVector2;

    public static bool modifyAutoUndrafter;
    public static bool whenGunsAreFiring = true;
    public static int extendUndraftTimeBy = 5000;
    public static bool allowAutoUndraftAtLowMood = true;
    public static string dontExtendWhenMoodAt = "1";

    public static readonly string[] mentalBreakRiskKeys =
    [
        "M4_MinorBreakRisk", "M4_MajorBreakRisk", "M4_ExtremeBreakRisk"
    ];

    public static bool variableRaidRetreat;
    public static FloatRange retreatDefeatRange = new(0.5f, 0.5f);

    public static bool guardingPredatorsDeferHuntingTameDesignatedAnimals = true;
    public static int animalInteractionHourLimit = 20;

    public static bool iAmAModder;

    public static bool chooseItemStuff = true;
    public static string stuffDefName = "";
    public static bool forceItemQuality = true;
    public static int forcedItemQuality = 2;

    public void DoWindowContents(Rect wrect)
    {
        var options = new Listing_Standard();
        var defaultColor = GUI.color;
        options.Begin(wrect);

        var titleRect = options.GetRect(30f);
        Text.Font = GameFont.Medium;
        GUI.color = Color.yellow;
        Widgets.Label(titleRect, "M4_GreySettingsIgnored".Translate());
        GUI.color = defaultColor;
        Text.Font = GameFont.Small;
        if (Widgets.ButtonText(titleRect.RightPart(0.25f), "M4_Reset".Translate()))
        {
            Reset();
        }

        options.Gap();

        options.ColumnWidth = (wrect.width / 2) - widthFiddler;

        // Left column

        options.CheckboxLabeled("M4_NotifyDeadAnimals".Translate(), ref deathMessagesForAnimals,
            "M4_NotifyDeadAnimals_Desc".Translate());
        options.GapLine();

        options.CheckboxLabeled("M4_TimetableAssignmentMatters".Translate(), ref workAssignmentMatters,
            "M4_TimetableAssignmentMatters_Desc".Translate());
        options.GapLine();

        options.CheckboxLabeled("M4_DisplayRangedDPS".Translate(), ref displayRangedDPS,
            "M4_DisplayRangedDPS_Desc".Translate());
        options.GapLine();

        //options.CheckboxLabeled("M4_BetterHostileReadouts".Translate(), ref betterHostileReadouts, "M4_BetterHostileReadouts_Desc".Translate());
        //options.GapLine();

        options.SliderLabeled(
            "M4_AnimalInteractionHourLimit".Translate(),
            ref animalInteractionHourLimit,
            animalInteractionHourLimit + "h",
            0f, 24f,
            "M4_AnimalInteractionHourLimit_Desc".Translate()
        );

        // Right column

        options.NewColumn();
        options.Gap(yPos);

        options.CheckboxLabeled("M4_SettingBigAnimalMigrations".Translate(), ref bigAnimalMigrations,
            "M4_SettingBigAnimalMigrations_Desc".Translate());
        options.GapLine();

        options.CheckboxLabeled("M4_SettingEnableLargePacks".Translate(), ref enableLargePacks,
            "M4_SettingLargePack_Desc".Translate());
        options.GapLine();

        options.CheckboxLabeled("M4_TutorialStyleRolling".Translate(), ref enableTutorialStyleRolling,
            "M4_TutorialStyleRolling_Desc".Translate());
        options.GapLine();

        options.CheckboxLabeled("M4_ThingFilterInfoCards".Translate(), ref thingFilterInfoCards,
            "M4_ThingFilterInfoCards_Desc".Translate());

        options.Gap();
        options.End();

        var gapline = new Listing_Standard();
        var gapliRect = new Rect(wrect.x, wrect.y + moreOptionsRecty - 35f, wrect.width, wrect.height);
        gapline.Begin(gapliRect);
        gapline.GapLine();
        gapline.End();

        var moreOptions = new Listing_Standard();
        var moreOptionsRect = wrect;
        moreOptionsRect.y = (moreOptionsRecty + 20f) / 2;
        moreOptionsRect.height = height / 2;
        moreOptionsRect.yMax = yMax;

        var viewRect = new Rect(0, 0, wrect.width - 18f, viewHeight);
        viewRect.width -= 18f;

        moreOptions.Begin(moreOptionsRect);

        Widgets.BeginScrollView(moreOptionsRect, ref scrollVector2, viewRect);
        moreOptionsRect.height = 100000f;
        moreOptionsRect.width -= 20f;
        moreOptions.Begin(moreOptionsRect.AtZero());

        moreOptions.CheckboxLabeled("M4_GuardingPredatorsDontHuntTameDesignatedPawns".Translate(),
            ref guardingPredatorsDeferHuntingTameDesignatedAnimals,
            "M4_GuardingPredatorsDontHuntTameDesignatedPawns_Desc".Translate());
        moreOptions.GapLine();

        if (!modifyAutoUndrafter)
        {
            GUI.color = Color.grey;
        }

        moreOptions.CheckboxLabeled("M4_SettingModifyAutoUndrafter".Translate(), ref modifyAutoUndrafter,
            "M4_SettingModifyAutoUndrafter_Desc".Translate());
        if (modifyAutoUndrafter)
        {
            moreOptions.SliderLabeled(
                "M4_SettingExtendUndraftTimeBy".Translate(), ref extendUndraftTimeBy,
                extendUndraftTimeBy.ToStringTicksToPeriod(),
                0, GenDate.TicksPerDay, "M4_SettingExtendUndraftTimeBy_Desc".Translate()
            );
            moreOptions.CheckboxLabeled("M4_SettingWithGunsBlazing".Translate(), ref whenGunsAreFiring,
                "M4_SettingGunsBlazing_Desc".Translate());
            moreOptions.CheckboxLabeled("M4_SettingLowMoodUndraft".Translate(), ref allowAutoUndraftAtLowMood,
                "M4_SettingLowMoodUndraft_Desc".Translate());
            GUI.color = defaultColor;
            if (!modifyAutoUndrafter || !allowAutoUndraftAtLowMood)
            {
                GUI.color = Color.grey;
            }

            moreOptions.AddLabeledRadioList(string.Empty, mentalBreakRiskKeys, ref dontExtendWhenMoodAt);
        }

        GUI.color = defaultColor;
        moreOptions.GapLine();

        moreOptions.CheckboxLabeled("M4_SettingVariableRaidRetreat".Translate(), ref variableRaidRetreat,
            "M4_SettingVariableRaid_Desc".Translate());
        if (!variableRaidRetreat)
        {
            GUI.color = Color.grey;
        }

        moreOptions.Gap(2);
        moreOptions.FloatRange("M4_SettingRetreatAtPercentageDefeated".Translate(), ref retreatDefeatRange, 0f, 1f,
            "M4_SettingRandomRaidRetreat_Desc".Translate(
                retreatDefeatRange.min.ToStringByStyle(ToStringStyle.PercentZero),
                retreatDefeatRange.max.ToStringByStyle(ToStringStyle.PercentZero)
            ), ToStringStyle.PercentZero);
        moreOptions.GapLine();

        moreOptions.CheckboxLabeled("M4_ImAModder".Translate(), ref iAmAModder, "M4_ImAModderTT".Translate());
        if (iAmAModder)
        {
            moreOptions.Label("M4_DevInfo".Translate());
            moreOptions.CheckboxLabeled("M4_ChooseStuff".Translate(), ref chooseItemStuff);
            moreOptions.Gap();
            moreOptions.AddLabeledTextField("M4_StuffName".Translate(), ref stuffDefName);
            moreOptions.Gap();
            moreOptions.CheckboxLabeled("M4_ForceQuality".Translate(), ref forceItemQuality);
            moreOptions.Gap();
            moreOptions.SliderLabeled("M4_ItemQuality".Translate(), ref forcedItemQuality,
                ((QualityCategory)forcedItemQuality).ToString(), 0, 6);
            moreOptions.GapLine();
        }

        if (MehniMiscMods.currentVersion != null)
        {
            moreOptions.Gap();
            GUI.contentColor = Color.gray;
            moreOptions.Label("M4_CurrentModVersion".Translate(MehniMiscMods.currentVersion));
            GUI.contentColor = Color.white;
        }

        Widgets.EndScrollView();
        moreOptions.End();
        moreOptions.End();
    }

    public void Reset()
    {
        bigAnimalMigrations = false;
        modifyAutoUndrafter = false;
        whenGunsAreFiring = true;
        extendUndraftTimeBy = 5000;
        allowAutoUndraftAtLowMood = true;
        dontExtendWhenMoodAt = "1";
        enableLargePacks = true;
        variableRaidRetreat = false;
        retreatDefeatRange = new FloatRange(0.5f, 0.5f);
        enableTutorialStyleRolling = true;
        deathMessagesForAnimals = true;
        guardingPredatorsDeferHuntingTameDesignatedAnimals = true;
        animalInteractionHourLimit = 20;
        workAssignmentMatters = false;
        iAmAModder = false;
        // betterHostileReadouts = true;
        displayRangedDPS = true;
        thingFilterInfoCards = true;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref bigAnimalMigrations, "bigAnimalMigrations");
        Scribe_Values.Look(ref modifyAutoUndrafter, "modifyAutoUndrafter");
        Scribe_Values.Look(ref whenGunsAreFiring, "whenGunsAreFiring", true);
        Scribe_Values.Look(ref extendUndraftTimeBy, "extendUndraftTimeBy", 5000);
        Scribe_Values.Look(ref allowAutoUndraftAtLowMood, "allowAutoUndraftAtLowMood", true);
        Scribe_Values.Look(ref dontExtendWhenMoodAt, "dontExtendWhenMoodAt", "1");
        Scribe_Values.Look(ref enableLargePacks, "enableLargePacks", true);
        Scribe_Values.Look(ref variableRaidRetreat, "variableRaidRetreat");
        Scribe_Values.Look(ref retreatDefeatRange, "retreatDefeatRange", new FloatRange(0.5f, 0.5f));
        Scribe_Values.Look(ref enableTutorialStyleRolling, "tutorialStyleRolling", true);
        Scribe_Values.Look(ref deathMessagesForAnimals, "deathMessageForAnimals", true);
        Scribe_Values.Look(ref guardingPredatorsDeferHuntingTameDesignatedAnimals,
            "guardingPredatorsDeferHuntingTameDesignatedAnimals", true);
        Scribe_Values.Look(ref animalInteractionHourLimit, "animalInteractionHourLimit", 20);
        Scribe_Values.Look(ref workAssignmentMatters, "workAssignmentMatters");
        Scribe_Values.Look(ref iAmAModder, "iAmAModder");
        // Scribe_Values.Look(ref betterHostileReadouts, "betterHostileReadouts", true);
        Scribe_Values.Look(ref displayRangedDPS, "displayRangedDPS", true);
        Scribe_Values.Look(ref thingFilterInfoCards, "thingFilterInfoCards", true);
    }

    // public static bool betterHostileReadouts = true;
}