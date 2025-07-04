using Verse;

namespace Mehni.Misc.Modifications;

internal class TimeAssignmentExtension : DefModExtension
{
    public static readonly TimeAssignmentExtension defaultValues = new();

    public float globalWorkSpeedFactor = 1f;
}