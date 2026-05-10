using RimWorld;
using System.Security.Cryptography;
using Verse;

namespace ApexMechanoids
{
    [DefOf]
    public static class ApexDefsOf
    {
		public static ThingDef APM_Mech_Tinker;
		public static ThingDef APM_Mech_Frostivus;
        public static BodyPartGroupDef APM_LeftAegisShield;
        public static BodyPartGroupDef APM_RightAegisShield;
        public static PawnKindDef APM_Mech_Aegis;
		public static BodyPartDef APM_AegisShield;
        //public static JobDef APM_RepairAegisShields;
        public static HediffDef APM_Hediff_Unity;
        public static HediffDef APM_DuelWinner;
        public static HediffDef APM_DuelDraw;
        public static HediffDef APM_InDuel;
        public static FleckDef ArcLargeEMP_B;
        public static FleckDef APM_AirPuffGreen;
        public static ThingDef APM_PawnFlyer_Hooked;
        public static ThingDef APM_Projectile_Hook;
        public static ThingDef APM_Mote_HookRope;
        public static StatDef MechGestationSpeedFactor; //no prefix, so its easier for other mods to not collide with ours
        public static ThingDef APM_MechCommandCasket;

        public static HediffDef RemoteRepairerImplant;  //from Biotech
        public static HediffDef RemoteShielderImplant;  //from Biotech
        public static HediffDef APM_Hediff_Devoured;
        public static SoundDef ShieldMech_Complete;     //from Biotech
        public static SoundDef ShieldMech_Start;        //from Biotech



        static ApexDefsOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ApexDefsOf));
        }
    }
}
