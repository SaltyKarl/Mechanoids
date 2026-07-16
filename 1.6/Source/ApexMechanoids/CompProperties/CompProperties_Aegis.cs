using Verse;

namespace ApexMechanoids
{
    public class CompProperties_Aegis : CompProperties
    {
        // Minimum time in seconds of peace (no damage taken) before regeneration may begin.
        public int regenerationDelaySeconds = 20;

        // How often to regenerate in seconds (converted to ticks internally)
        public int regenerationIntervalSeconds = 5;

        // How much shield HP to restore per regeneration step.
        public float regenerationHPPerStep = 2f;

        public CompProperties_Aegis()
        {
            compClass = typeof(CompAegis);
        }
    }
}