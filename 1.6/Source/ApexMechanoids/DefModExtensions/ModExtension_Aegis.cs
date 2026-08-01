using Verse;

namespace ApexMechanoids
{
    // Data-only definition of the Aegis shield behaviour. Attaching this extension to a mech's
    // ThingDef (via <modExtensions>) is what turns that mech into an Aegis: the CompAegis runtime
    // comp is injected automatically (see AegisCompInjector) and the damage/repair patches key off
    // this extension instead of a hard-coded pawn kind.
    public class ModExtension_Aegis : DefModExtension
    {
        // The body part that represents a shield, and the left/right groups it belongs to.
        public BodyPartDef shieldPart;
        public BodyPartGroupDef leftShieldGroup;
        public BodyPartGroupDef rightShieldGroup;

        // Shield self-regeneration (kept intentionally very slow).
        public float regenerationDelaySeconds = 60f;
        public float regenerationIntervalSeconds = 30f;
        public float regenerationHPPerStep = 1f;

        // Chance that a side attack is absorbed by the corresponding shield.
        public float sideDamageChance = 0.2f;

        // Extra mech energy drained per shield HP restored during a repair job,
        // scaled by the mech's MechEnergyLossPerHP stat. Higher = shields cost more to repair.
        public float repairEnergyCostMultiplier = 3f;
    }
}
