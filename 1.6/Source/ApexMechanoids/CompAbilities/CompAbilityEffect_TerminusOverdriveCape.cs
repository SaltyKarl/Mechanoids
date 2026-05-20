using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ApexMechanoids
{
    public static class TerminusOverdriveCapeState
    {
        private static readonly Dictionary<int, int> HiddenUntilByPawnId = new Dictionary<int, int>();

        public static void MarkCapeDropped(Pawn pawn, int ticks)
        {
            if (pawn == null)
            {
                return;
            }

            HiddenUntilByPawnId[pawn.thingIDNumber] = Find.TickManager.TicksGame + ticks;
        }

        public static bool ShouldHideCape(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (pawn.health?.hediffSet?.HasHediff(ApexDefsOf.APM_Hediff_TerminusOverdrive) ?? false)
            {
                return true;
            }

            if (HiddenUntilByPawnId.TryGetValue(pawn.thingIDNumber, out int hiddenUntilTick))
            {
                if (Find.TickManager.TicksGame <= hiddenUntilTick)
                {
                    return true;
                }

                HiddenUntilByPawnId.Remove(pawn.thingIDNumber);
            }

            return false;
        }
    }

    public static class TerminusOverdriveCapeUtility
    {
        private const string TerminusBossKindDefName = "APM_Mech_Terminus_Boss";
        private const float VisualSouthOffset = 0.32f;

        public static void SpawnBurst(Pawn caster, ThingDef moteDef, int burstCount, FloatRange speedRange, FloatRange angleOffsetRange, float spawnRadius, float sideOffsetDistance, float verticalOffset)
        {
            if (caster == null || !caster.Spawned || moteDef == null)
            {
                return;
            }

            Map map = caster.Map;
            if (map == null || !caster.Position.ShouldSpawnMotesAt(map))
            {
                return;
            }

            bool isBossVariant = caster.kindDef?.defName == TerminusBossKindDefName;
            Color colorOne = isBossVariant ? Color.white : (caster.Faction?.AllegianceColor ?? caster.DrawColor);

            for (int i = 0; i < burstCount; i++)
            {
                bool throwRight = Rand.Bool;
                float moveAngle = throwRight ? 90f : 270f;
                float horizontalOffset = (throwRight ? 1f : -1f) * sideOffsetDistance;
                float horizontalJitter = Rand.Range(-spawnRadius, spawnRadius) * 1.6f;
                float verticalJitter = Rand.Range(-spawnRadius, spawnRadius) * 0.9f;
                Vector3 spawnPos = caster.DrawPos + new Vector3(horizontalOffset + horizontalJitter, 0f, VisualSouthOffset + verticalOffset + verticalJitter);

                Mote_TerminusCapeThrown mote = ThingMaker.MakeThing(moteDef) as Mote_TerminusCapeThrown;
                if (mote == null)
                {
                    continue;
                }

                GenSpawn.Spawn(mote, spawnPos.ToIntVec3(), map);
                float moveSpeed = Rand.Range(speedRange.min, speedRange.max);
                float startRotation = Rand.Range(-18f, 18f);
                float startRotationRate = Rand.Range(-18f, 18f);
                int settleAfterTicks = Rand.RangeInclusive(36, 64);
                mote.Launch(spawnPos, moveAngle, moveSpeed, startRotation, startRotationRate, colorOne, !throwRight, isBossVariant, settleAfterTicks);
            }
        }
    }

    public class CompAbility_TerminusOverdriveCapeWarmup : AbilityComp
    {
        private int ticksUntilNextSpawn;

        public CompProperties_TerminusOverdriveCapeWarmup Props => (CompProperties_TerminusOverdriveCapeWarmup)props;

        public override void CompTick()
        {
            base.CompTick();

            Pawn caster = parent.pawn;
            if (caster == null || !caster.Spawned || caster.Map == null)
            {
                return;
            }

            if (!parent.wasCastingOnPrevTick)
            {
                ticksUntilNextSpawn = 0;
                return;
            }

            if (ticksUntilNextSpawn > 0)
            {
                ticksUntilNextSpawn--;
                return;
            }

            ticksUntilNextSpawn = Props.spawnIntervalTicks;
            TerminusOverdriveCapeUtility.SpawnBurst(caster, Props.moteDef, Props.burstCount, Props.speedRange, Props.angleOffsetRange, Props.spawnRadius, Props.sideOffsetDistance, Props.verticalOffset);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilNextSpawn, nameof(ticksUntilNextSpawn));
        }
    }

    public class CompProperties_TerminusOverdriveCapeWarmup : AbilityCompProperties
    {
        public CompProperties_TerminusOverdriveCapeWarmup()
        {
            compClass = typeof(CompAbility_TerminusOverdriveCapeWarmup);
        }

        public ThingDef moteDef;
        public int spawnIntervalTicks = 10;
        public int burstCount = 1;
        public float spawnRadius = 0.18f;
        public float sideOffsetDistance = 0.45f;
        public float verticalOffset = 0.14f;
        public FloatRange speedRange = new FloatRange(0.18f, 0.3f);
        public FloatRange angleOffsetRange = new FloatRange(-28f, 28f);
    }

    public class CompAbilityEffect_TerminusOverdriveCapeBurst : CompAbilityEffect
    {
        private const int InstantHideGraceTicks = 45;

        public new CompProperties_TerminusOverdriveCapeBurst Props => (CompProperties_TerminusOverdriveCapeBurst)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            TerminusOverdriveCapeState.MarkCapeDropped(parent.pawn, InstantHideGraceTicks);
            base.Apply(target, dest);
            TerminusOverdriveCapeUtility.SpawnBurst(parent.pawn, Props.moteDef, Props.burstCount, Props.speedRange, Props.angleOffsetRange, Props.spawnRadius, Props.sideOffsetDistance, Props.verticalOffset);
        }
    }

    public class CompProperties_TerminusOverdriveCapeBurst : CompProperties_AbilityEffect
    {
        public CompProperties_TerminusOverdriveCapeBurst()
        {
            compClass = typeof(CompAbilityEffect_TerminusOverdriveCapeBurst);
        }

        public ThingDef moteDef;
        public int burstCount = 1;
        public float spawnRadius = 0.06f;
        public float sideOffsetDistance = 0f;
        public float verticalOffset = 0.06f;
        public FloatRange speedRange = new FloatRange(0.9f, 1.15f);
        public FloatRange angleOffsetRange = new FloatRange(-10f, 10f);
    }
}