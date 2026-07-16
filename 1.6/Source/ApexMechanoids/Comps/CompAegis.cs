using RimWorld;
using System.Linq;
using Verse;

namespace ApexMechanoids
{
    public class CompAegis : ThingComp
    {
        // Whether shields are currently damaged/missing and being tracked for regeneration.
        // Scribed under the legacy key "shieldsDamaged" for save compatibility.
        private bool shieldsNeedRepair = false;
        private int ticksSinceDamage = 0;
        private int ticksSinceRegen = 0;
        private const int CompTickRareInterval = 250;

        public CompProperties_Aegis Props => (CompProperties_Aegis)props;

        private int RegenerationDelayTicks => (int)(Props.regenerationDelaySeconds * 60f);
        private int RegenerationIntervalTicks => (int)(Props.regenerationIntervalSeconds * 60f);

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref shieldsNeedRepair, "shieldsDamaged", false);
            Scribe_Values.Look(ref ticksSinceDamage, "ticksSinceDamage", 0);
            Scribe_Values.Look(ref ticksSinceRegen, "ticksSinceRegen", 0);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            if (totalDamageDealt <= 0f)
                return;

            Pawn pawn = parent as Pawn;
            if (pawn == null)
                return;

            if (AnyShieldsMissingOrDamaged(pawn))
            {
                shieldsNeedRepair = true;
                // Require a fresh peace period before regeneration may resume, but do NOT
                // reset ticksSinceRegen: occasional chip damage during regeneration must not
                // stall progress indefinitely (it previously prevented regen from ever advancing).
                ticksSinceDamage = 0;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();

            Pawn pawn = parent as Pawn;
            if (pawn == null)
                return;

            // Shields fully intact (possibly healed by other means): stop tracking and reset timers.
            if (!AnyShieldsMissingOrDamaged(pawn))
            {
                shieldsNeedRepair = false;
                ticksSinceDamage = 0;
                ticksSinceRegen = 0;
                return;
            }

            shieldsNeedRepair = true;
            ticksSinceDamage += CompTickRareInterval;
            ticksSinceRegen += CompTickRareInterval;

            // Wait for the post-damage delay, then regenerate at most one step per interval.
            if (ticksSinceDamage >= RegenerationDelayTicks && ticksSinceRegen >= RegenerationIntervalTicks)
            {
                if (RegenerateOneShieldStep(pawn))
                {
                    // All shields restored.
                    shieldsNeedRepair = false;
                    ticksSinceDamage = 0;
                    ticksSinceRegen = 0;
                }
                else
                {
                    // One step done; more remain. Only the interval throttle resets,
                    // so subsequent parts are fixed every interval without re-waiting the full delay.
                    ticksSinceRegen = 0;
                }
            }
        }

        /// <summary>
        /// Performs a single regeneration step: rebuilds every missing shield as a fresh injury
        /// and heals every injured shield by a fixed HP amount. Returns true once every shield
        /// is intact.
        /// </summary>
        private bool RegenerateOneShieldStep(Pawn pawn)
        {
            bool allIntact = true;

            foreach (BodyPartRecord shieldPart in pawn.RaceProps.body.AllParts)
            {
                if (shieldPart.def != ApexDefsOf.APM_AegisShield)
                    continue;

                if (pawn.health.hediffSet.PartIsMissing(shieldPart))
                {
                    RebuildMissingShield(pawn, shieldPart);
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                    allIntact = false;
                }
                else if (pawn.health.hediffSet.GetInjuredParts().Contains(shieldPart))
                {
                    HealShieldInjury(pawn, shieldPart);
                    FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
                    if (pawn.health.hediffSet.GetInjuredParts().Contains(shieldPart))
                        allIntact = false;
                }
            }

            return allIntact;
        }

        private void RebuildMissingShield(Pawn pawn, BodyPartRecord part)
        {
            // Restore the missing part, then add a full-severity injury so the HP-based regen
            // heals it back up gradually (one step at a time) rather than instantly.
            Hediff missing = pawn.health.hediffSet.GetMissingPartFor(part);
            if (missing != null)
            {
                pawn.health.RemoveHediff(missing);
            }

            float maxHP = part.def.GetMaxHealth(pawn);
            Hediff_Injury injury = HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, part) as Hediff_Injury;
            if (injury != null)
            {
                injury.Severity = maxHP;
                pawn.health.AddHediff(injury, part);
            }
        }

        private void HealShieldInjury(Pawn pawn, BodyPartRecord part)
        {
            Hediff_Injury injury = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .FirstOrDefault(h => h.Part == part);

            // Heal a fixed amount of HP per step so regeneration is gradual and HP-based.
            injury?.Heal(Props.regenerationHPPerStep);
        }

        private bool AnyShieldsMissingOrDamaged(Pawn pawn)
        {
            foreach (BodyPartRecord part in pawn.RaceProps.body.AllParts)
            {
                if (part.def != ApexDefsOf.APM_AegisShield)
                    continue;

                if (pawn.health.hediffSet.PartIsMissing(part))
                    return true;

                if (pawn.health.hediffSet.GetInjuredParts().Contains(part))
                    return true;
            }

            return false;
        }
    }
}
