using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class Projectile_HazeToxicRepeater : Beam
    {
        private static ThingDef toxicPuddleDef;

        private static ThingDef ToxicPuddleDef
        {
            get
            {
                if (toxicPuddleDef == null)
                {
                    toxicPuddleDef = DefDatabase<ThingDef>.GetNamedSilentFail("APM_HazeToxicPuddle");
                }

                return toxicPuddleDef;
            }
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 cell = hitThing?.Position ?? Position;

            base.Impact(hitThing, blockedByShield);

            if (blockedByShield || map == null || !cell.InBounds(map))
            {
                return;
            }

            TryPlaceToxicPuddle(cell, map);
        }

        private static void TryPlaceToxicPuddle(IntVec3 cell, Map map)
        {
            ThingDef puddleDef = ToxicPuddleDef;
            if (puddleDef == null)
            {
                return;
            }

            foreach (Thing thing in cell.GetThingList(map))
            {
                if (thing is HazeToxicPuddle existingPuddle)
                {
                    existingPuddle.Refresh();
                    return;
                }
            }

            GenSpawn.Spawn(puddleDef, cell, map);
        }
    }

    public class HazeToxicPuddle : Filth
    {
        private const int LifetimeTicks = 900;
        private const int ToxicTickInterval = 120;
        private const float ToxicSeverity = 0.025f;

        private int ticksLeft = LifetimeTicks;

        public void Refresh()
        {
            ticksLeft = LifetimeTicks;
        }

        public override void Tick()
        {
            base.Tick();

            ticksLeft--;
            if (ticksLeft <= 0)
            {
                Destroy(DestroyMode.Vanish);
                return;
            }

            if (this.IsHashIntervalTick(ToxicTickInterval))
            {
                ApplyToxicBuildup();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksLeft, nameof(ticksLeft), LifetimeTicks);
        }

        private void ApplyToxicBuildup()
        {
            if (Map == null)
            {
                return;
            }

            foreach (Thing thing in Position.GetThingList(Map))
            {
                Pawn pawn = thing as Pawn;
                if (pawn == null || pawn.Dead || pawn.RaceProps.IsMechanoid)
                {
                    continue;
                }

                HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, ToxicSeverity);
            }
        }
    }
}
