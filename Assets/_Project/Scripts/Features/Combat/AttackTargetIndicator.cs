using UnityEngine;

namespace Shogun.Features.Combat
{
    public class AttackTargetIndicator : MonoBehaviour
    {
        public enum State { Hidden, AttackReady, ComboReady }

        private static readonly Color AttackReadyTint = new Color(1.00f, 0.35f, 0.35f);
        private static readonly Color ComboReadyTint = new Color(1.00f, 0.50f, 0.20f);

        private const float TintPulseMin = 0.10f;
        private const float TintPulseMax = 0.50f;
        private const float TintPulseHz = 2.2f;

        private const float LabelOffsetY = 2.7f;
        private const float LabelScale = 0.075f;
        private const float LabelShadowOffsetX = 0.10f;
        private const float LabelShadowOffsetY = -0.08f;
        private const int LabelFontSize = 96;
        private const float BurstDurationDefault = 0.42f;
        private const float BurstScaleOvershoot = 0.58f;
        private const float BurstLift = 0.24f;
        private const float BurstBobHz = 4.5f;
        private const float BurstBobAmplitude = 0.12f;

        private SpriteRenderer mainRenderer;
        private Color originalColor;
        private TextMesh comboLabel;
        private TextMesh comboLabelShadow;
        private Transform comboLabelTransform;
        private Vector3 comboLabelBaseLocalPosition;
        private State currentState = State.Hidden;
        private float comboBurstStartedAt = -999f;
        private float comboBurstDuration = BurstDurationDefault;
        private bool comboBurstActive;

        void Awake()
        {
            mainRenderer = GetComponentInChildren<SpriteRenderer>();
            if (mainRenderer != null)
                originalColor = mainRenderer.color;

            BuildComboLabel();
        }

        void Update()
        {
            UpdateTint();
            UpdateComboLabel();
        }

        public void ShowAttackReady()
        {
            currentState = State.AttackReady;
            if (!comboBurstActive)
                SetLabelVisible(false);
        }

        public void ShowComboReady(int comboCount = 2)
        {
            currentState = State.ComboReady;
            ApplyComboLabelText(comboCount);
            SetLabelVisible(true);
            ResetComboLabelPose();
        }

        public void PlayComboBurst(int comboCount, float duration = BurstDurationDefault)
        {
            ApplyComboLabelText(comboCount);
            comboBurstDuration = Mathf.Max(0.12f, duration);
            comboBurstStartedAt = Time.time;
            comboBurstActive = true;
            SetLabelVisible(true);
            AnimateComboBurst(0f);
        }

        public void Hide()
        {
            currentState = State.Hidden;
            comboBurstActive = false;
            SetLabelVisible(false);

            if (mainRenderer != null)
                mainRenderer.color = originalColor;
        }

        private void UpdateTint()
        {
            if (mainRenderer == null)
                return;

            if (currentState == State.Hidden)
            {
                mainRenderer.color = originalColor;
                return;
            }

            float pulse = (Mathf.Sin(Time.time * TintPulseHz * Mathf.PI * 2f) + 1f) * 0.5f;
            float tintBlend = Mathf.Lerp(TintPulseMin, TintPulseMax, pulse);
            Color tint = currentState == State.ComboReady ? ComboReadyTint : AttackReadyTint;
            mainRenderer.color = Color.Lerp(Color.white, tint, tintBlend);
        }

        private void UpdateComboLabel()
        {
            if (comboLabelTransform == null || !comboLabelTransform.gameObject.activeSelf)
                return;

            if (comboBurstActive)
            {
                float elapsed = Time.time - comboBurstStartedAt;
                if (elapsed >= comboBurstDuration)
                {
                    comboBurstActive = false;
                    if (currentState == State.ComboReady)
                        ResetComboLabelPose();
                    else
                        SetLabelVisible(false);
                    return;
                }

                AnimateComboBurst(elapsed / comboBurstDuration);
                return;
            }

            if (currentState == State.ComboReady)
                ResetComboLabelPose();
            else
                SetLabelVisible(false);
        }

        private void ApplyComboLabelText(int comboCount)
        {
            string labelText = $"\u00D7{Mathf.Max(2, comboCount)}!";
            if (comboLabel != null)
                comboLabel.text = labelText;
            if (comboLabelShadow != null)
                comboLabelShadow.text = labelText;
        }

        private void SetLabelVisible(bool on)
        {
            if (comboLabelTransform != null)
                comboLabelTransform.gameObject.SetActive(on);
        }

        private void ResetComboLabelPose()
        {
            if (comboLabelTransform == null)
                return;

            float scaleSignX = transform.lossyScale.x < 0f ? -1f : 1f;
            comboLabelTransform.localScale = new Vector3(LabelScale * scaleSignX, LabelScale, LabelScale);
            comboLabelTransform.localPosition = comboLabelBaseLocalPosition;
        }

        private void AnimateComboBurst(float normalizedTime)
        {
            if (comboLabelTransform == null)
                return;

            float t = Mathf.Clamp01(normalizedTime);
            float bounce = Mathf.Sin(t * Mathf.PI);
            float burstScale = 1f + bounce * Mathf.Lerp(BurstScaleOvershoot, 0.10f, t);
            float burstLift = bounce * BurstLift;
            float burstBob = Mathf.Sin(Time.time * BurstBobHz * Mathf.PI * 2f) * BurstBobAmplitude * (1f - t);
            float finalScale = LabelScale * burstScale;
            float scaleSignX = transform.lossyScale.x < 0f ? -1f : 1f;

            comboLabelTransform.localScale = new Vector3(finalScale * scaleSignX, finalScale, finalScale);
            comboLabelTransform.localPosition = comboLabelBaseLocalPosition + new Vector3(0f, burstLift + burstBob, 0f);
        }

        private void BuildComboLabel()
        {
            GameObject labelRoot = new GameObject("ComboLabel");
            labelRoot.transform.SetParent(transform, false);
            labelRoot.transform.localPosition = new Vector3(0f, LabelOffsetY, -0.1f);
            labelRoot.transform.localScale = Vector3.one * LabelScale;

            comboLabelTransform = labelRoot.transform;
            comboLabelBaseLocalPosition = comboLabelTransform.localPosition;

            comboLabel = labelRoot.AddComponent<TextMesh>();
            comboLabel.text = "\u00D72!";
            comboLabel.fontSize = LabelFontSize;
            comboLabel.fontStyle = FontStyle.Bold;
            comboLabel.alignment = TextAlignment.Center;
            comboLabel.anchor = TextAnchor.MiddleCenter;
            comboLabel.color = new Color(1f, 0.88f, 0.15f);

            GameObject shadowRoot = new GameObject("ComboLabelShadow");
            shadowRoot.transform.SetParent(labelRoot.transform, false);
            shadowRoot.transform.localPosition = new Vector3(LabelShadowOffsetX, LabelShadowOffsetY, 0.02f);

            comboLabelShadow = shadowRoot.AddComponent<TextMesh>();
            comboLabelShadow.text = comboLabel.text;
            comboLabelShadow.fontSize = LabelFontSize;
            comboLabelShadow.fontStyle = FontStyle.Bold;
            comboLabelShadow.alignment = TextAlignment.Center;
            comboLabelShadow.anchor = TextAnchor.MiddleCenter;
            comboLabelShadow.color = new Color(0.22f, 0.08f, 0f, 0.85f);

            labelRoot.SetActive(false);
        }
    }
}
