using Verse;

namespace ApexMechanoids
{
    // Runtime comp properties for the Aegis shields. All configuration now lives in
    // ModExtension_Aegis; this exists only to bind CompAegis and is injected automatically
    // onto any ThingDef carrying that extension (see AegisCompInjector).
    public class CompProperties_Aegis : CompProperties
    {
        public CompProperties_Aegis()
        {
            compClass = typeof(CompAegis);
        }
    }
}
