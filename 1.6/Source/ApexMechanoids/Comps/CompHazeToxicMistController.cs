using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class CompProperties_HazeToxicMistController : CompProperties
    {
        public AbilityDef abilityDef;
        public int checkIntervalTicks = 300;
        public bool autoCastForPlayer = false;
        public float threatRadius = 4.9f;
        public int minHostilesToTrigger = 1;
        public bool requireFleshThreat = true;
        public bool blockIfAlliesInRadius = true;

        public CompProperties_HazeToxicMistController()
        {
            compClass = typeof(CompHazeToxicMistController);
        }
    }

    public class CompHazeToxicMistController : ThingComp
    {
        public CompProperties_HazeToxicMistController Props => (CompProperties_HazeToxicMistController)props;

        private Pawn Pawn => parent as Pawn;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            EnsureAbility();
        }

        public override void CompTick()
        {
            base.CompTick();

            Pawn pawn = Pawn;
            if (pawn == null || !pawn.Spawned || pawn.Dead || pawn.Downed || pawn.Map == null)
            {
                return;
            }

            if (!Props.autoCastForPlayer && pawn.Faction == Faction.OfPlayer)
            {
                return;
            }

            int interval = Props.checkIntervalTicks > 0 ? Props.checkIntervalTicks : 300;
            if (!pawn.IsHashIntervalTick(interval))
            {
                return;
            }

            EnsureAbility();

            if (!ShouldAutoCast(pawn))
            {
                return;
            }

            Ability ability = pawn.abilities?.GetAbility(Props.abilityDef);
            if (ability == null || !ability.CanCast)
            {
                return;
            }

            if (pawn.CurJobDef == ability.def.jobDef)
            {
                return;
            }

            LocalTargetInfo selfTarget = new LocalTargetInfo(pawn);
            if (!ability.CanApplyOn(selfTarget))
            {
                return;
            }

            ability.QueueCastingJob(selfTarget, selfTarget);
        }

        private bool ShouldAutoCast(Pawn pawn)
        {
            if (pawn?.Map == null)
            {
                return false;
            }

            float radius = Props.threatRadius > 0f ? Props.threatRadius : 4.9f;
            int requiredHostiles = Props.minHostilesToTrigger > 0 ? Props.minHostilesToTrigger : 1;
            int hostiles = 0;
            var pawns = pawn.Map.mapPawns.AllPawnsSpawned;

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn other = pawns[i];
                if (other == null || other == pawn || other.Dead || other.Downed || !other.Spawned)
                {
                    continue;
                }

                if (other.Position.DistanceTo(pawn.Position) > radius)
                {
                    continue;
                }

                if (!other.HostileTo(pawn))
                {
                    if (Props.blockIfAlliesInRadius && other.Faction == pawn.Faction)
                    {
                        return false;
                    }

                    continue;
                }

                if (Props.requireFleshThreat && !(other.RaceProps?.IsFlesh ?? false))
                {
                    continue;
                }

                hostiles++;
                if (hostiles >= requiredHostiles)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureAbility()
        {
            Pawn pawn = Pawn;
            if (pawn?.abilities == null || Props.abilityDef == null)
            {
                return;
            }

            if (pawn.abilities.GetAbility(Props.abilityDef) == null)
            {
                pawn.abilities.GainAbility(Props.abilityDef);
            }
        }
    }
}
