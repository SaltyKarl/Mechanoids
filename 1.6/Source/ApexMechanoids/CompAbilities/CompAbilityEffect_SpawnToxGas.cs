using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class CompProperties_AbilitySpawnToxGas : CompProperties_AbilityEffect
    {
        public float radius = 5f;
        public float gasAmount = 1f;

        public CompProperties_AbilitySpawnToxGas()
        {
            compClass = typeof(CompAbilityEffect_SpawnToxGas);
        }
    }

    public class CompAbilityEffect_SpawnToxGas : CompAbilityEffect
    {
        public new CompProperties_AbilitySpawnToxGas Props => (CompProperties_AbilitySpawnToxGas)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = parent.pawn;
            Map map = pawn.Map;
            if (map == null)
            {
                return;
            }
            IntVec3 center = pawn.Position;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(center, Props.radius, useCenter: true))
            {
                if (cell.InBounds(map))
                {
                    GasUtility.AddGas(cell, map, GasType.ToxGas, Props.gasAmount);
                }
            }
        }
    }
}
