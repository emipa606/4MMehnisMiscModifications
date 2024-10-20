using RimWorld;
using Verse;

namespace Mehni.Misc.Modifications;

public class StatPart_TimeTable : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!MeMiMoSettings.workAssignmentMatters || req.Thing is not Pawn { timetable: { } timetable })
        {
            return;
        }

        var currentAssignment = timetable.CurrentAssignment;
        val *= currentAssignment.GetModExtension<TimeAssignmentExtension>() is { } tAE
            ? tAE.globalWorkSpeedFactor
            : TimeAssignmentExtension.defaultValues.globalWorkSpeedFactor;
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!MeMiMoSettings.workAssignmentMatters || req.Thing is not Pawn { timetable: { } timetable })
        {
            return null;
        }

        var currentAssignment = timetable.CurrentAssignment;
        var workSpeedFactor =
            currentAssignment.GetModExtension<TimeAssignmentExtension>() is { } tAE
                ? tAE.globalWorkSpeedFactor
                : TimeAssignmentExtension.defaultValues.globalWorkSpeedFactor;
        return "M4_StatPart_TimeTable_ExplanationString".Translate(currentAssignment.label) + ": x" +
               workSpeedFactor.ToStringPercent();
    }
}