using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class HediffCompProperties_PinpointCombatCommandAura : HediffCompProperties
    {
        public float range = 9.9f;
        public int tickInterval = 30;
        public int buffRefreshTicks = 90;
        public HediffDef buffHediff;
        public bool includeMechanoids = true;
        public bool includeNonMechPawns = true;

        public HediffCompProperties_PinpointCombatCommandAura()
        {
            compClass = typeof(HediffComp_PinpointCombatCommandAura);
        }
    }

    public class HediffComp_PinpointCombatCommandAura : HediffComp
    {
        public HediffCompProperties_PinpointCombatCommandAura Props => (HediffCompProperties_PinpointCombatCommandAura)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn == null || !Pawn.Spawned || Pawn.MapHeld == null)
            {
                return;
            }

            if (!Pawn.IsHashIntervalTick(Props.tickInterval))
            {
                return;
            }

            for (int i = 0; i < Pawn.MapHeld.mapPawns.AllPawnsSpawned.Count; i++)
            {
                Pawn other = Pawn.MapHeld.mapPawns.AllPawnsSpawned[i];
                if (!ShouldAffect(other))
                {
                    continue;
                }

                ApplyOrRefreshBuff(other);
            }
        }

        private bool ShouldAffect(Pawn other)
        {
            if (other == null || other == Pawn || other.Dead || other.Destroyed || !other.Spawned)
            {
                return false;
            }

            if (other.Faction != Pawn.Faction)
            {
                return false;
            }

            if (!other.Position.InHorDistOf(Pawn.Position, Props.range))
            {
                return false;
            }

            if (other.RaceProps.IsMechanoid)
            {
                return Props.includeMechanoids;
            }

            return Props.includeNonMechPawns;
        }

        private void ApplyOrRefreshBuff(Pawn pawn)
        {
            Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(Props.buffHediff);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(Props.buffHediff, pawn);
                pawn.health.AddHediff(hediff);
            }

            HediffComp_Disappears disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (disappears != null)
            {
                disappears.ticksToDisappear = Props.buffRefreshTicks;
            }
        }
    }
}
