using RimWorld;
using Verse;
using Verse.AI;

namespace ApexMechanoids
{
    public class ThinkNode_ConditionalPinpointAlliesInCombat : ThinkNode_Conditional
    {
        public float allySearchRadius = 20f;
        public float auraRadius = 9.9f;
        public int minAlliesInCombat = 1;

        public override bool Satisfied(Pawn pawn)
        {
            if (pawn?.Map == null || pawn.Faction == null)
            {
                return false;
            }

            int found = 0;
            for (int i = 0; i < pawn.Map.mapPawns.AllPawnsSpawned.Count; i++)
            {
                Pawn other = pawn.Map.mapPawns.AllPawnsSpawned[i];
                if (!ShouldCountAlly(pawn, other))
                {
                    continue;
                }

                found++;
                if (found >= minAlliesInCombat)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ShouldCountAlly(Pawn pawn, Pawn other)
        {
            if (other == null || other == pawn || other.Dead || other.Downed || !other.Spawned)
            {
                return false;
            }

            if (other.Faction != pawn.Faction || !other.RaceProps.IsMechanoid)
            {
                return false;
            }

            if (!other.Position.InHorDistOf(pawn.Position, allySearchRadius))
            {
                return false;
            }

            if (!other.Position.InHorDistOf(pawn.Position, auraRadius))
            {
                return false;
            }

            return IsEngagedInCombat(other);
        }

        private bool IsEngagedInCombat(Pawn pawn)
        {
            Thing enemyTarget = pawn.mindState?.enemyTarget;
            if (enemyTarget != null && enemyTarget.Spawned && !enemyTarget.Destroyed && enemyTarget.HostileTo(pawn))
            {
                return true;
            }

            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.AttackTarget),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                18f,
                thing => thing.HostileTo(pawn)) != null;
        }
    }
}
