using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ApexMechanoids
{
    public class Verb_StarfallAbility : Verb_CastAbility
    {
        private static readonly ProjectileHitFlags HitFlags = ProjectileHitFlags.IntendedTarget | ProjectileHitFlags.NonTargetPawns;

        private ThingDef ProjectileDef => DefDatabase<ThingDef>.GetNamed("APM_ArtilleryProjectile");

        private int BurstCount => verbProps.burstShotCount > 0 ? verbProps.burstShotCount : 3;

        public override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
                return false;

            IntVec3 dest = currentTarget.Cell;

            for (int i = 0; i < BurstCount; i++)
            {
                float angle = Rand.Range(0f, 360f);
                float dist = Rand.Range(0f, verbProps.forcedMissRadius > 0f ? verbProps.forcedMissRadius : 8.9f);
                IntVec3 scattered = dest + new IntVec3(
                    Mathf.RoundToInt(Mathf.Cos(angle * Mathf.Deg2Rad) * dist),
                    0,
                    Mathf.RoundToInt(Mathf.Sin(angle * Mathf.Deg2Rad) * dist)
                );
                if (!scattered.InBounds(caster.Map))
                    scattered = dest;

                Projectile proj = (Projectile)GenSpawn.Spawn(ProjectileDef, caster.Position, caster.Map);
                proj.Launch(caster, new LocalTargetInfo(scattered), new LocalTargetInfo(scattered), HitFlags);
            }

            if (CasterPawn != null)
                CasterPawn.rotationTracker.FaceCell(dest);

            SoundDef.Named("Starfall").PlayOneShot(new TargetInfo(dest, caster.Map));

            base.TryCastShot();
            return true;
        }
    }
}
