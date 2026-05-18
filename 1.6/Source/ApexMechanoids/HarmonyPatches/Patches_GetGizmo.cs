using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace ApexMechanoids
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetGizmos")]
    public static class AddOnlyOverseerResurrectionGizmo
    {
        [HarmonyPostfix]
        public static void GetGizmos(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {

            if (__instance?.CurJob != null)
            {
                Job job = __instance?.CurJob;

                if (job.def == ApexDefsOf.APM_RemoteControlUplink)
                {
                    Thing buildingThing = job.GetTarget(TargetIndex.A).Thing;
                    if (buildingThing != null)
                    {
                        CompRemoteMechCasketAbilities comp = buildingThing.TryGetComp<CompRemoteMechCasketAbilities>();
                        if (comp != null)
                        {
                            List<Gizmo> list = __result.ToList();

                            foreach (Gizmo gizmo in comp.GetGizmos())
                            {
                                if (gizmo != null)
                                {
                                    list.Add(gizmo);
                                }
                            }
                            __result = list;
                        }
                    }
                }
            }
        }
    }
}
