using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ApexMechanoids
{
    public class CompAbility_SpawnSirenSongNotes : AbilityComp
    {
        private static readonly Dictionary<int, int> lastSpawnTickByCaster = new Dictionary<int, int>();
        private int ticksUntilNextSpawn;

        public CompProperties_SpawnSirenSongNotes Props => (CompProperties_SpawnSirenSongNotes)props;

        public override void CompTick()
        {
            base.CompTick();

            Pawn caster = parent.pawn;
            Map map = caster?.Map;
            if (caster == null || map == null || !caster.Spawned)
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

            if (!caster.Position.ShouldSpawnMotesAt(map))
            {
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            int casterId = caster.thingIDNumber;
            if (lastSpawnTickByCaster.TryGetValue(casterId, out int lastTick) && lastTick == currentTick)
            {
                return;
            }

            lastSpawnTickByCaster[casterId] = currentTick;

            int noteCountThisBurst = Props.noteCount;
            if (Props.extraNoteChance > 0f && Rand.Chance(Props.extraNoteChance))
            {
                noteCountThisBurst++;
            }

            for (int i = 0; i < noteCountThisBurst; i++)
            {
                Vector3 pos = caster.DrawPos + new Vector3(Rand.Range(-Props.horizontalJitter, Props.horizontalJitter), 0f, Rand.Range(Props.minVerticalOffset, Props.maxVerticalOffset));
                FleckCreationData data = FleckMaker.GetDataStatic(pos, map, Props.fleckDef, Rand.Range(Props.scaleRange.min, Props.scaleRange.max));
                data.rotation = Props.rotation;
                data.rotationRate = Rand.Range(-Props.rotationRate, Props.rotationRate);
                data.velocityAngle = Rand.Range(Props.velocityAngle.min, Props.velocityAngle.max);
                data.velocitySpeed = Rand.Range(Props.speedRange.min, Props.speedRange.max);
                map.flecks.CreateFleck(data);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksUntilNextSpawn, nameof(ticksUntilNextSpawn));
        }
    }

    public class CompProperties_SpawnSirenSongNotes : AbilityCompProperties
    {
        public CompProperties_SpawnSirenSongNotes()
        {
            compClass = typeof(CompAbility_SpawnSirenSongNotes);
        }

        public FleckDef fleckDef;
        public int noteCount = 2;
        public float extraNoteChance = 0.25f;
        public int spawnIntervalTicks = 8;
        public float horizontalJitter = 0.16f;
        public float minVerticalOffset = 0.12f;
        public float maxVerticalOffset = 0.56f;
        public FloatRange scaleRange = new FloatRange(0.32f, 0.64f);
        public FloatRange speedRange = new FloatRange(0.52f, 0.74f);
        public FloatRange velocityAngle = new FloatRange(-4f, 4f);
        public float rotation = 0f;
        public float rotationRate = 0f;
    }
}
