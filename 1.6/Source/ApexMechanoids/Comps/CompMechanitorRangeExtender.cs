using UnityEngine;
using Verse;

namespace ApexMechanoids
{
    public class CompProperties_MechanitorRangeExtender : Verse.CompProperties
    {
        public float maxRange;
        public float minRange;

        public CompProperties_MechanitorRangeExtender() => compClass = typeof(CompMechanitorRangeExtender);
    }

    public class CompMechanitorRangeExtender : ThingComp
    {
        public CompProperties_MechanitorRangeExtender Props => (CompProperties_MechanitorRangeExtender)props;
        private Pawn Pawn => parent as Pawn;

        public float currentRange;

        public float SquaredDistance => GetEffectiveSquaredDistance();

        private float GetEffectiveSquaredDistance()
        {
            float range = GetEffectiveRange();
            if (range <= 0f) return 0f;
            return range * range;
        }

        public float GetEffectiveRange()
        {
            Pawn pawn = Pawn;
            Pawn overseer = pawn?.GetOverseer();
            if (overseer == null)
            {
                currentRange = 0f;
                return 0f;
            }

            currentRange = overseer.MapHeld == pawn.MapHeld ? Props.maxRange : Props.minRange;
            return currentRange;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            Pawn pawn = Pawn;
            if (pawn == null || !pawn.Drafted) return;
            float range = GetEffectiveRange();
            if (range > 0f)
            {
                GenDraw.DrawRadiusRing(parent.Position, range, Color.cyan);
            }
        }
    }
}
