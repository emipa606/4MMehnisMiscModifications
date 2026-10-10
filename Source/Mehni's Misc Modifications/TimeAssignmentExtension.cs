using Verse;

namespace Mehni.Misc.Modifications;

internal class TimeAssignmentExtension : DefModExtension
{
    public static readonly TimeAssignmentExtension defaultValues = new();

    public readonly float globalWorkSpeedFactor = 1f;
}