using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Security.Cryptography;
using Verse.AI;

namespace ApexMechanoids
{
<<<<<<< Updated upstream
=======
    public static class ApexMechColors
    {
        public static readonly Color DominusColor  = new Color(230f / 255f, 130f / 255f,  40f / 255f);
        public static readonly Color CelerusColor  = new Color(160f / 255f,  60f / 255f, 230f / 255f);
        public static readonly Color TerminusColor = new Color(229f / 255f, 211f / 255f, 127f / 255f);
        public static readonly Color PlayerColor = new Color(163f / 255f, 180f / 255f, 187f / 255f);

        public static Color GetAbilityColor(Pawn caster)
        {
            string kind = caster?.kindDef?.defName;
            if (kind != null)
            {
                bool isBoss = kind.EndsWith("_Boss");
                if (kind.Contains("Dominus")) return isBoss ? DominusColor  : PlayerColor;
                if (kind.Contains("Celerus")) return isBoss ? CelerusColor  : PlayerColor;
                if (kind.Contains("Terminus")) return isBoss ? TerminusColor : PlayerColor;
                if (kind.Contains("Vassal"))  return PlayerColor;
            }
            return caster?.Faction?.AllegianceColor ?? Color.white;
        }
    }

>>>>>>> Stashed changes
    public static class Utils
    {
        public static BodyPartRecord GetNonMissingBodyPart(Pawn pawn, BodyPartDef def, BodyPartGroupDef group = null)
        {
            foreach (var notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (notMissingPart.def == def)
                {
                    if (group != null && !notMissingPart.groups.Contains(group))
                    {
                        continue;
                    }
                    return notMissingPart;
                }
            }

            return null;
        }

        public static List<BodyPartRecord> GetNonMissingBodyParts(Pawn pawn, BodyPartDef def)
        {
            List<BodyPartRecord> matchingParts = new List<BodyPartRecord>();

            foreach (var notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (notMissingPart.def == def)
                {
                    matchingParts.Add(notMissingPart);
                }
            }

            return matchingParts;
        }
        
        #region -- Logs --
        public static void LogMessage(string str) => Log.Message("<color=#9ba08c>[ApexMechanoids]</color> " + str);
        public static void LogWarning(string str) => Log.Warning("<color=#9ba08c>[ApexMechanoids]</color> " + str);
        public static void LogError(string str) => Log.Error("<color=#9ba08c>[ApexMechanoids]</color> " + str);
        public static void LogErrorOnce(string str, int key) => Log.ErrorOnce("<color=#9ba08c>[ApexMechanoids]</color> " + str, key);
        #endregion

        public static void TryDoAbility(Pawn pawn, AbilityDef abilityDef, LocalTargetInfo target)
        {
            if (!target.IsValid)
            {
                LogError("Invalid ability target: " + target.ToString());
                return;
            }
            Ability ab = pawn.abilities?.GetAbility(abilityDef);
            if (ab == null)
            {
                LogError("subAbility is null");
                return;
            }
            if (!ab.CanCast)
            {
                LogError("Can't cast subAbility");
                return;
            }
            if (!ab.verb.CanHitTarget(target))
            {
                LogError("Could not hit target: " + target + " from " + pawn.Position);
                return;
            }
            ab.QueueCastingJob(target.Pawn, target.Pawn);
        }

        public static bool IsUplinkActiveFor(Pawn mechanitor)
        {
            if (mechanitor == null || mechanitor.Dead || mechanitor.mechanitor == null)
            {
                return false;
            }

            Job curJob = mechanitor.CurJob;
            if (curJob == null || curJob.def != ApexDefsOf.APM_RemoteControlUplink)
            {
                return false;
            }

            Thing building = curJob.targetA.Thing;
            if (building == null)
            {
                return false;
            }

            CompRemoteControlUplink comp = building.TryGetComp<CompRemoteControlUplink>();
            if (comp == null || comp.ManningPawn != mechanitor)
            {
                return false;
            }

            return true;
        }
    }
}
