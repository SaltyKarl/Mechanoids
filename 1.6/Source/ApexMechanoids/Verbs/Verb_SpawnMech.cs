using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ApexMechanoids
{
    public class Verb_SpawnMech : Verb_Shoot
    {
        public DefModExtension_MechPack modExtension => EquipmentSource.def.GetModExtension<DefModExtension_MechPack>();

        public CompApparelReloadable comp
        {
            get
            {
                return EquipmentSource.TryGetComp<CompApparelReloadable>();
            }
        }

        public List<Pawn> spawnedThing = new List<Pawn>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref spawnedThing, "spawnedThing",LookMode.Reference);
        }
        public override bool TryCastShot()
        {
            if (comp != null)
            {
                try
                {
                    if (comp.remainingCharges > 0)
                    {
                        comp.UsedOnce();
                        IReadOnlyList<Pawn> list = spawnedThing.ToList();
                        foreach (var item in list)
                        {
                            if (item.Dead || item.DestroyedOrNull())
                            {
                                spawnedThing.Remove(item);
                            }
                        }
                        if (spawnedThing.Count > 2)
                        {                            
                            Pawn pawn = spawnedThing.FirstOrDefault();
                            pawn.Kill(new DamageInfo(DamageDefOf.ElectricalBurn,99999f,2f,instigator:Caster));
                            spawnedThing.Remove(pawn);
                        }
                        Pawn spawnedOne = PawnGenerator.GeneratePawn(modExtension.spawnedKind);
                        spawnedOne.SetFaction(Caster.Faction);
                        GenSpawn.Spawn(spawnedOne, CurrentTarget.Cell, Caster.MapHeld);
                        CompUnity compUnity = spawnedOne.TryGetComp<CompUnity>();
                        if (compUnity != null)
                        {
                            compUnity.ForceUpdateNow();
                        }
                        spawnedThing.Add(spawnedOne);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[ApexMechanoids] Error in Verb_SpawnMech.TryCastShot {ex}");
                }
            }
            return false;
        }

        public override bool Available()
        {
            if (comp != null)
            {
                return comp.CanBeUsed(out var _);
            }
            return base.Available();
        }
    }

    public class DefModExtension_MechPack : DefModExtension
    {
        public int maxNum = 2;

        public PawnKindDef spawnedKind;

        public float resourceConsumed = 1;
    }
}
