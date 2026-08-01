using UnityEngine;
using Verse;

namespace ApexMechanoids
{
    [StaticConstructorOnStartup]
    public class Gizmo_ShieldHP : Gizmo
    {
        public CompAegis comp;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.55f, 0.85f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.15f, 0.15f, 0.15f));

        public Gizmo_ShieldHP()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect inner = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);

            Text.Font = GameFont.Tiny;

            Rect labelRect = inner;
            labelRect.height = inner.height / 2f;
            Widgets.Label(labelRect, "APM_ShieldIntegrity".Translate());

            Rect barRect = inner;
            barRect.yMin = inner.y + inner.height / 2f;
            Widgets.FillableBar(barRect, comp.ShieldHPPercent, FullShieldBarTex, EmptyShieldBarTex, doBorder: false);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(barRect, Mathf.RoundToInt(comp.CurShieldHP) + " / " + Mathf.RoundToInt(comp.MaxShieldHP));
            Text.Anchor = TextAnchor.UpperLeft;

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
