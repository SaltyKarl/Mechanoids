using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class CompProperties_PlayerFactionMechPowerCell : CompProperties_MechPowerCell
    {
        public CompProperties_PlayerFactionMechPowerCell()
        {
            compClass = typeof(CompPlayerFactionMechPowerCell);
        }
    }

    public class CompPlayerFactionMechPowerCell : CompMechPowerCell
    {
        public override void CompTick()
        {
            if (parent is Pawn pawn && pawn.Faction == Faction.OfPlayer)
            {
                base.CompTick();
            }
        }
    }
}
