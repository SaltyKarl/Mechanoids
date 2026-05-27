using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ApexMechanoids
{
    public class JobGiver_PinpointSupportPosition : ThinkNode_JobGiver
    {
        public float anchorSearchRadius = 28f;
        public float supportOffsetDistance = 3.2f;
        public float supportCellSearchRadius = 4.9f;
        public float minEnemyDistance = 8.2f;
        public float fallbackRetreatDistance = 6.5f;
        public float auraRadius = 9.9f;
        public float retreatEnemyDistance = 6.5f;
        public float soloThreatRadius = 30f;
        public float repositionScoreTolerance = 30f;
        public float soloPreferredDistance = 15f;
        public int holdPositionTicksMin = 180;
        public int holdPositionTicksMax = 320;

        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Map == null || pawn.Downed || pawn.Dead)
            {
                return null;
            }

            List<Pawn> supportAllies = FindSupportAllies(pawn, requireCombat: true);
            if (supportAllies.Count == 0)
            {
                supportAllies = FindSupportAllies(pawn, requireCombat: false);
            }

            Thing hostile = FindHostile(pawn, supportAllies.Count > 0 ? 45f : soloThreatRadius);
            if (hostile == null)
            {
                if (supportAllies.Count == 0)
                {
                    return TryMakeSoloWanderJob(pawn);
                }

                return MakeHoldJob();
            }

            IntVec3 targetCell;
            CellEvaluation bestCell = default;
            bool enemyTooClose = pawn.Position.DistanceTo(hostile.Position) <= retreatEnemyDistance;

            if (supportAllies.Count > 0)
            {
                if (!TryFindOptimalSupportCell(pawn, hostile, supportAllies, enemyTooClose, out targetCell, out bestCell))
                {
                    if (!TryFindFallbackRetreatCell(pawn, hostile, out targetCell, out bestCell))
                    {
                        return MakeHoldJob();
                    }
                }
            }
            else if (!TryFindSoloKiteCell(pawn, hostile, out targetCell, out bestCell))
            {
                return MakeHoldJob();
            }

            if (ShouldHoldCurrentPosition(pawn, hostile, supportAllies, bestCell))
            {
                return MakeHoldJob();
            }

            Job moveJob = JobMaker.MakeJob(JobDefOf.Goto, targetCell);
            moveJob.expiryInterval = Rand.RangeInclusive(180, 300);
            moveJob.checkOverrideOnExpire = true;
            moveJob.collideWithPawns = true;
            moveJob.locomotionUrgency = LocomotionUrgency.Jog;
            return moveJob;
        }

        private Job MakeHoldJob()
        {
            Job holdJob = JobMaker.MakeJob(JobDefOf.Wait_Combat);
            holdJob.expiryInterval = Rand.RangeInclusive(holdPositionTicksMin, holdPositionTicksMax);
            holdJob.checkOverrideOnExpire = true;
            return holdJob;
        }

        private Job TryMakeSoloWanderJob(Pawn pawn)
        {
            IntVec3 wanderCell = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, 10);
            if (!wanderCell.IsValid || wanderCell == pawn.Position)
            {
                return MakeHoldJob();
            }

            Job wanderJob = JobMaker.MakeJob(JobDefOf.Goto, wanderCell);
            wanderJob.expiryInterval = Rand.RangeInclusive(180, 300);
            wanderJob.checkOverrideOnExpire = true;
            wanderJob.locomotionUrgency = LocomotionUrgency.Walk;
            return wanderJob;
        }

        private bool ShouldHoldCurrentPosition(Pawn pawn, Thing hostile, List<Pawn> supportAllies, CellEvaluation bestCell)
        {
            if (!EvaluateCell(pawn, pawn.Position, hostile, supportAllies, out CellEvaluation currentCell))
            {
                return false;
            }

            if (currentCell.enemyDistance < retreatEnemyDistance)
            {
                return false;
            }

            if (supportAllies.Count > 0)
            {
                if (currentCell.coveredAllies < bestCell.coveredAllies)
                {
                    return false;
                }

                if (currentCell.uncoveredAllies > bestCell.uncoveredAllies)
                {
                    return false;
                }

                if (currentCell.coveredAllies == bestCell.coveredAllies
                    && currentCell.uncoveredAllies == bestCell.uncoveredAllies
                    && currentCell.outsideAuraDistanceSum <= bestCell.outsideAuraDistanceSum + 0.75f
                    && currentCell.enemyDistance >= bestCell.enemyDistance - 0.75f)
                {
                    return true;
                }

                return currentCell.coveredAllies == bestCell.coveredAllies
                    && currentCell.uncoveredAllies == bestCell.uncoveredAllies
                    && currentCell.score <= bestCell.score + repositionScoreTolerance;
            }
            else if (currentCell.enemyDistance >= soloPreferredDistance)
            {
                return currentCell.score <= bestCell.score + repositionScoreTolerance * 0.6f;
            }

            return currentCell.score <= bestCell.score + repositionScoreTolerance;
        }

        private Thing FindHostile(Pawn pawn, float radius)
        {
            Thing hostile = pawn.mindState?.enemyTarget;
            if (IsValidHostile(pawn, hostile) && hostile.Position.InHorDistOf(pawn.Position, radius))
            {
                return hostile;
            }

            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.AttackTarget),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                radius,
                thing => IsValidHostile(pawn, thing));
        }

        private bool IsValidHostile(Pawn pawn, Thing thing)
        {
            return thing != null
                && thing.Spawned
                && !thing.Destroyed
                && thing.HostileTo(pawn)
                && pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly);
        }

        private List<Pawn> FindSupportAllies(Pawn pawn, bool requireCombat)
        {
            List<Pawn> allies = new List<Pawn>();
            List<Pawn> pawns = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn other = pawns[i];
                if (other == pawn || other.Downed || other.Dead || !other.RaceProps.IsMechanoid)
                {
                    continue;
                }

                if (!other.Position.InHorDistOf(pawn.Position, anchorSearchRadius))
                {
                    continue;
                }

                if (requireCombat && !IsEngagedInCombat(other))
                {
                    continue;
                }

                allies.Add(other);
            }

            return allies;
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

        private bool TryFindOptimalSupportCell(Pawn pawn, Thing hostile, List<Pawn> supportAllies, bool enemyTooClose, out IntVec3 bestCell, out CellEvaluation bestEval)
        {
            Vector3 groupCenter = GetGroupCenter(supportAllies);
            Vector3 retreatDirection = groupCenter - hostile.DrawPos;
            retreatDirection.y = 0f;
            if (retreatDirection.sqrMagnitude < 0.001f)
            {
                retreatDirection = pawn.DrawPos - hostile.DrawPos;
                retreatDirection.y = 0f;
            }

            if (retreatDirection.sqrMagnitude < 0.001f)
            {
                retreatDirection = Vector3.back;
            }

            retreatDirection.Normalize();
            float desiredOffset = enemyTooClose ? Mathf.Max(fallbackRetreatDistance, supportOffsetDistance + 2f) : supportOffsetDistance;
            IntVec3 desiredCell = (groupCenter + retreatDirection * desiredOffset).ToIntVec3();
            return TryFindBestCellNear(pawn, hostile, supportAllies, desiredCell, out bestCell, out bestEval);
        }

        private Vector3 GetGroupCenter(List<Pawn> supportAllies)
        {
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < supportAllies.Count; i++)
            {
                sum += supportAllies[i].DrawPos;
            }

            return sum / supportAllies.Count;
        }

        private bool TryFindFallbackRetreatCell(Pawn pawn, Thing hostile, out IntVec3 bestCell, out CellEvaluation bestEval)
        {
            Vector3 direction = pawn.DrawPos - hostile.DrawPos;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.back;
            }

            direction.Normalize();
            IntVec3 desiredCell = (pawn.DrawPos + direction * fallbackRetreatDistance).ToIntVec3();
            return TryFindBestCellNear(pawn, hostile, new List<Pawn>(), desiredCell, out bestCell, out bestEval);
        }

        private bool TryFindSoloKiteCell(Pawn pawn, Thing hostile, out IntVec3 bestCell, out CellEvaluation bestEval)
        {
            Vector3 direction = pawn.DrawPos - hostile.DrawPos;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.back;
            }

            direction.Normalize();
            Vector3 sideways = new Vector3(-direction.z, 0f, direction.x);
            float sideSign = pawn.thingIDNumber % 2 == 0 ? 1f : -1f;
            Vector3 desiredPos = pawn.DrawPos + direction * fallbackRetreatDistance + sideways * sideSign * 2.5f;
            IntVec3 desiredCell = desiredPos.ToIntVec3();
            return TryFindBestCellNear(pawn, hostile, new List<Pawn>(), desiredCell, out bestCell, out bestEval);
        }

        private bool TryFindBestCellNear(Pawn pawn, Thing hostile, List<Pawn> supportAllies, IntVec3 desiredCell, out IntVec3 bestCell, out CellEvaluation bestEval)
        {
            bestCell = IntVec3.Invalid;
            bestEval = default;
            bool found = false;
            bool supportMode = supportAllies.Count > 0;

            foreach (IntVec3 cell in GenRadial.RadialCellsAround(desiredCell, supportCellSearchRadius, true))
            {
                if (!EvaluateCell(pawn, cell, hostile, supportAllies, out CellEvaluation eval))
                {
                    continue;
                }

                if (!found || IsBetterCell(eval, bestEval, supportMode))
                {
                    found = true;
                    bestEval = eval;
                    bestCell = cell;
                }
            }

            return found;
        }

        private bool IsBetterCell(CellEvaluation candidate, CellEvaluation current, bool supportMode)
        {
            if (supportMode)
            {
                if (candidate.coveredAllies != current.coveredAllies)
                {
                    return candidate.coveredAllies > current.coveredAllies;
                }

                if (candidate.uncoveredAllies != current.uncoveredAllies)
                {
                    return candidate.uncoveredAllies < current.uncoveredAllies;
                }

                if (!Mathf.Approximately(candidate.outsideAuraDistanceSum, current.outsideAuraDistanceSum))
                {
                    return candidate.outsideAuraDistanceSum < current.outsideAuraDistanceSum;
                }
            }

            return candidate.score < current.score;
        }

        private bool EvaluateCell(Pawn pawn, IntVec3 cell, Thing hostile, List<Pawn> supportAllies, out CellEvaluation eval)
        {
            eval = default;

            if (!cell.InBounds(pawn.Map) || !cell.Standable(pawn.Map))
            {
                return false;
            }

            if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly))
            {
                return false;
            }

            float enemyDistance = cell.DistanceTo(hostile.Position);
            if (enemyDistance < minEnemyDistance)
            {
                return false;
            }

            int coveredAllies = 0;
            float allyDistanceSum = 0f;
            for (int i = 0; i < supportAllies.Count; i++)
            {
                Pawn ally = supportAllies[i];
                float distance = cell.DistanceTo(ally.Position);
                if (distance <= auraRadius)
                {
                    coveredAllies++;
                    allyDistanceSum += distance;
                }
            }

            float movementPenalty = pawn.Position.DistanceToSquared(cell) * 0.22f;
            bool soloMode = supportAllies.Count == 0;
            float cohesionPenalty = coveredAllies > 0 ? allyDistanceSum * 2.2f : (soloMode ? 0f : 35f);
            int uncoveredAllies = 0;
            float outsideAuraDistanceSum = 0f;
            for (int i = 0; i < supportAllies.Count; i++)
            {
                float distance = cell.DistanceTo(supportAllies[i].Position);
                if (distance > auraRadius)
                {
                    uncoveredAllies++;
                    outsideAuraDistanceSum += distance - auraRadius;
                }
            }

            float uncoveredPenalty = soloMode ? 0f : uncoveredAllies * 260f + outsideAuraDistanceSum * 24f;
            float coverageBonus = coveredAllies * 240f;
            float safetyBonus = enemyDistance * (soloMode ? 9f : 8f);
            float soloDistancePenalty = soloMode ? Mathf.Abs(enemyDistance - soloPreferredDistance) * 11f : 0f;
            float score = movementPenalty + cohesionPenalty + uncoveredPenalty + soloDistancePenalty - coverageBonus - safetyBonus;

            eval = new CellEvaluation
            {
                cell = cell,
                score = score,
                coveredAllies = coveredAllies,
                uncoveredAllies = uncoveredAllies,
                outsideAuraDistanceSum = outsideAuraDistanceSum,
                enemyDistance = enemyDistance
            };
            return true;
        }

        private struct CellEvaluation
        {
            public IntVec3 cell;
            public float score;
            public int coveredAllies;
            public int uncoveredAllies;
            public float outsideAuraDistanceSum;
            public float enemyDistance;
        }
    }
}
