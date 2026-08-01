using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ApexMechanoids
{
    // Injects CompAegis onto every ThingDef that carries a ModExtension_Aegis, so the shield
    // behaviour is applied through the mod extension rather than an explicit <comps> entry.
    [StaticConstructorOnStartup]
    public static class AegisCompInjector
    {
        static AegisCompInjector()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!def.HasModExtension<ModExtension_Aegis>())
                {
                    continue;
                }

                if (def.comps == null)
                {
                    def.comps = new List<CompProperties>();
                }

                if (!def.comps.Any(c => c is CompProperties_Aegis))
                {
                    def.comps.Add(new CompProperties_Aegis());
                }
            }
        }
    }
}
