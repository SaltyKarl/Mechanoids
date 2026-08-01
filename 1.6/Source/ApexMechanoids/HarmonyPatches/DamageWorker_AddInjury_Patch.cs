using Verse;

namespace ApexMechanoids
{
    internal static class DamageWorker_AddInjury_Patch
    {
        public static bool pickShield;
        public static BodyPartGroupDef whichShield;
        public static BodyPartDef whichShieldPart;

        [HarmonyLib.HarmonyPatch(typeof(DamageWorker_AddInjury), "GetExactPartFromDamageInfo")]
        internal static class GetExactPartFromDamageInfo
        {
            private static void Prefix(DamageInfo dinfo, Pawn pawn)
            {
                ModExtension_Aegis ext = pawn?.def?.GetModExtension<ModExtension_Aegis>();
                if (ext == null || ext.shieldPart == null || !(dinfo.Instigator is Pawn pawn2))
                {
                    return;
                }

                var angle = (pawn2.DrawPos - pawn.DrawPos).AngleFlat();
                var rot = Pawn_RotationTracker.RotFromAngleBiased(angle);

                if (rot == pawn.Rotation)
                {
                    TryPickShieldForFrontAttack(pawn, ext);
                }
                else if (IsSideAttack(rot, pawn.Rotation) && Rand.Chance(ext.sideDamageChance))
                {
                    TryPickShieldForSideAttack(pawn, ext, rot);
                }
            }

            private static void TryPickShieldForFrontAttack(Pawn pawn, ModExtension_Aegis ext)
            {
                bool checkRightFirst = Rand.Chance(0.5f);
                var firstShield = checkRightFirst ? ext.rightShieldGroup : ext.leftShieldGroup;
                var secondShield = checkRightFirst ? ext.leftShieldGroup : ext.rightShieldGroup;

                if (TryPickShield(pawn, ext, firstShield))
                {
                    return;
                }

                TryPickShield(pawn, ext, secondShield);
            }

            private static void TryPickShieldForSideAttack(Pawn pawn, ModExtension_Aegis ext, Rot4 attackRot)
            {
                bool isRightSide = IsRightSideAttack(attackRot, pawn.Rotation);
                var shieldGroup = isRightSide ? ext.rightShieldGroup : ext.leftShieldGroup;
                TryPickShield(pawn, ext, shieldGroup);
            }

            private static bool TryPickShield(Pawn pawn, ModExtension_Aegis ext, BodyPartGroupDef shieldGroup)
            {
                if (shieldGroup == null)
                {
                    return false;
                }

                var targetBodyPart = Utils.GetNonMissingBodyPart(pawn, ext.shieldPart, shieldGroup);
                if (targetBodyPart != null)
                {
                    whichShield = shieldGroup;
                    whichShieldPart = ext.shieldPart;
                    pickShield = true;
                    return true;
                }

                return false;
            }

            private static bool IsSideAttack(Rot4 attackRot, Rot4 pawnRot)
            {
                int rotDiff = (attackRot.AsInt - pawnRot.AsInt + 4) % 4;
                return rotDiff == 1 || rotDiff == 3;
            }

            private static bool IsRightSideAttack(Rot4 attackRot, Rot4 pawnRot)
            {
                int rotDiff = (attackRot.AsInt - pawnRot.AsInt + 4) % 4;
                return rotDiff == 1;
            }

            private static void Postfix()
            {
                pickShield = false;
            }
        }
    }
}
