using RimWorld;
using Verse;

namespace ApexMechanoids
{
    public class CompProperties_AbilityRevealInvisibility : CompProperties_AbilityEffect
    {
        public bool instant = false;
        public CompProperties_AbilityRevealInvisibility() => compClass = typeof(CompAbilityRevealInvisibility);
    }

    public class CompAbilityRevealInvisibility : CompAbilityEffect
    {
        public new CompProperties_AbilityRevealInvisibility Props => (CompProperties_AbilityRevealInvisibility)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Pawn != null && !target.Pawn.Dead)
            {
                target.Pawn.GetInvisibilityComp()?.BecomeVisible(Props.instant);
            }
        }
    }
}
