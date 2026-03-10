// AttackTargetIndicator.cs
// Shows that a dragged ally can reach this enemy (Naruto-Blazing style).
//
// Layer 1 — SPRITE TINT
//   mainRenderer.color pulses white → red so the whole enemy sprite flushes.
//   Subtle: tint strength stays low (TintPulseMin/Max) so it reads clearly
//   without looking garish.
//
// Layer 2 — COMBO LABEL (ComboReady only)
//   A TextMesh floating above the character shows "×2!", "×3!", etc.
//   Hidden entirely in AttackReady state.
//
// States:
//   AttackReady — dragged character can reach this enemy        (red pulse)
//   ComboReady  — attack ready AND N other allies also in range (red pulse + ×N! label)

using UnityEngine;

namespace Shogun.Features.Combat
{
    public class AttackTargetIndicator : MonoBehaviour
    {
        public enum State { Hidden, AttackReady, ComboReady }

        // ── Tint applied to the main SpriteRenderer ────────────────────
        // SpriteRenderer.color is a per-channel multiplier.
        // (1, 0.35, 0.35) keeps red at full, suppresses G/B → red flush.
        private static readonly Color AttackReadyTint = new Color(1.00f, 0.35f, 0.35f);
        private static readonly Color ComboReadyTint  = new Color(1.00f, 0.50f, 0.20f); // orange

        // Fraction of the tint blended in at each pulse extreme.
        // 0 = white (invisible tint), 1 = full tint colour.
        private const float TintPulseMin = 0.10f;  // barely there at pulse trough
        private const float TintPulseMax = 0.50f;  // noticeable but not harsh at peak

        private const float PulseHz = 2.2f;

        // ── Combo label ────────────────────────────────────────────────
        // Positioned above the character root pivot.
        // Scale is small because TextMesh units are huge by default.
        private const float LabelOffsetY  = 2.5f;   // local-space units above root
        private const float LabelScale    = 0.045f;  // compensates TextMesh's large default unit
        private const int   LabelFontSize = 72;

        // ── Runtime ────────────────────────────────────────────────────
        private SpriteRenderer mainRenderer;
        private Color          originalColor;
        private TextMesh       comboLabel;
        private State          currentState = State.Hidden;

        // ─────────────────────────────────────────────────── lifecycle

        void Awake()
        {
            mainRenderer = GetComponentInChildren<SpriteRenderer>();
            if (mainRenderer == null) return;

            originalColor = mainRenderer.color;
            BuildComboLabel();
        }

        void Update()
        {
            if (currentState == State.Hidden || mainRenderer == null) return;

            float pulse = (Mathf.Sin(Time.time * PulseHz * Mathf.PI * 2f) + 1f) * 0.5f;
            float t     = Mathf.Lerp(TintPulseMin, TintPulseMax, pulse);

            Color tint = currentState == State.ComboReady ? ComboReadyTint : AttackReadyTint;
            mainRenderer.color = Color.Lerp(Color.white, tint, t);
        }

        // ─────────────────────────────────────────────────── public API

        /// <summary>Pulse the sprite red: dragged character can reach this enemy.</summary>
        public void ShowAttackReady()
        {
            currentState = State.AttackReady;
            SetLabelVisible(false);
        }

        /// <summary>
        /// Pulse sprite orange + show "×N!" label: N combatants (including the
        /// dragged one) can reach this enemy simultaneously.
        /// </summary>
        public void ShowComboReady(int comboCount = 2)
        {
            currentState = State.ComboReady;
            if (comboLabel != null)
                comboLabel.text = $"\u00D7{Mathf.Max(2, comboCount)}!";  // ×N!
            SetLabelVisible(true);
        }

        public void Hide()
        {
            currentState = State.Hidden;
            SetLabelVisible(false);
            if (mainRenderer != null)
                mainRenderer.color = originalColor;
        }

        // ─────────────────────────────────────────────────── private

        private void SetLabelVisible(bool on)
        {
            if (comboLabel != null)
                comboLabel.gameObject.SetActive(on);
        }

        private void BuildComboLabel()
        {
            var go = new GameObject("ComboLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, LabelOffsetY, -0.1f); // slight Z forward
            go.transform.localScale    = Vector3.one * LabelScale;

            comboLabel = go.AddComponent<TextMesh>();
            comboLabel.text      = "\u00D72!";
            comboLabel.fontSize  = LabelFontSize;
            comboLabel.fontStyle = FontStyle.Bold;
            comboLabel.alignment = TextAlignment.Center;
            comboLabel.anchor    = TextAnchor.MiddleCenter;
            comboLabel.color     = new Color(1f, 0.88f, 0.15f); // bright gold

            go.SetActive(false);
        }
    }
}
