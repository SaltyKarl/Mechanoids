using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class CompProperties_PinpointProximityDetector : CompProperties
    {
        public float radius = 19.9f;
        public int intervalTicks = 30;
        public bool instant;
        public string effecterDefName;

        public CompProperties_PinpointProximityDetector()
        {
            compClass = typeof(CompPinpointProximityDetector);
        }
    }

    public class CompPinpointProximityDetector : ThingComp
    {
        public CompProperties_PinpointProximityDetector Props => (CompProperties_PinpointProximityDetector)props;

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.IsHashIntervalTick(Props.intervalTicks))
            {
                return;
            }

            Pawn pinpoint = parent as Pawn;
            Map map = parent.Map;
            if (pinpoint == null || map == null || pinpoint.Dead || !pinpoint.Spawned)
            {
                return;
            }

            bool detectedAny = false;
            float radiusSquared = Props.radius * Props.radius;
            var pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn other = pawns[i];
                if (other == null || other == pinpoint || other.Dead || !other.Spawned)
                {
                    continue;
                }

                if ((other.Position - pinpoint.Position).LengthHorizontalSquared > radiusSquared)
                {
                    continue;
                }

                HediffComp_Invisibility invisibility = other.GetInvisibilityComp();
                if (invisibility == null || invisibility.PsychologicallyVisible)
                {
                    continue;
                }

                invisibility.BecomeVisible(Props.instant);
                detectedAny = true;
            }

            if (detectedAny)
            {
                SpawnDetectionEffect(pinpoint.PositionHeld, map);
            }
        }

        private void SpawnDetectionEffect(IntVec3 position, Map map)
        {
            if (map == null || string.IsNullOrEmpty(Props.effecterDefName))
            {
                return;
            }

            EffecterDef effecterDef = DefDatabase<EffecterDef>.GetNamedSilentFail(Props.effecterDefName);
            if (effecterDef == null)
            {
                return;
            }

            Effecter effecter = effecterDef.Spawn(position, map);
            effecter?.Cleanup();
        }
    }
}
