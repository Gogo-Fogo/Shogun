using UnityEngine;

namespace Shogun.Features.Combat
{
    public class DragMultiTargetIndicator : MonoBehaviour
    {
        private const float LabelOffsetY = 3.15f;
        private const float LabelScale = 0.072f;
        private const float LabelShadowOffsetX = 0.08f;
        private const float LabelShadowOffsetY = -0.08f;
        private const int LabelFontSize = 84;

        private Transform labelRoot;
        private TextMesh mainLabel;
        private TextMesh shadowLabel;
        private Vector3 baseLocalPosition;

        void Awake()
        {
            BuildLabel();
            Hide();
        }

        void LateUpdate()
        {
            if (labelRoot == null || !labelRoot.gameObject.activeSelf)
                return;

            float scaleSignX = transform.lossyScale.x < 0f ? -1f : 1f;
            labelRoot.localScale = new Vector3(LabelScale * scaleSignX, LabelScale, LabelScale);
            labelRoot.localPosition = baseLocalPosition;
        }

        public void Show(int targetCount)
        {
            if (labelRoot == null)
                return;

            int normalizedCount = Mathf.Max(2, targetCount);
            string labelText = $"X{normalizedCount}!";
            if (mainLabel != null)
                mainLabel.text = labelText;
            if (shadowLabel != null)
                shadowLabel.text = labelText;

            labelRoot.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (labelRoot != null)
                labelRoot.gameObject.SetActive(false);
        }

        private void BuildLabel()
        {
            GameObject root = new GameObject("DragMultiTargetLabel");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, LabelOffsetY, -0.1f);
            root.transform.localScale = Vector3.one * LabelScale;
            labelRoot = root.transform;
            baseLocalPosition = labelRoot.localPosition;

            mainLabel = root.AddComponent<TextMesh>();
            mainLabel.text = "X2!";
            mainLabel.fontSize = LabelFontSize;
            mainLabel.fontStyle = FontStyle.Bold;
            mainLabel.alignment = TextAlignment.Center;
            mainLabel.anchor = TextAnchor.MiddleCenter;
            mainLabel.color = new Color(0.64f, 1f, 0.96f);

            GameObject shadow = new GameObject("DragMultiTargetLabelShadow");
            shadow.transform.SetParent(root.transform, false);
            shadow.transform.localPosition = new Vector3(LabelShadowOffsetX, LabelShadowOffsetY, 0.02f);

            shadowLabel = shadow.AddComponent<TextMesh>();
            shadowLabel.text = mainLabel.text;
            shadowLabel.fontSize = LabelFontSize;
            shadowLabel.fontStyle = FontStyle.Bold;
            shadowLabel.alignment = TextAlignment.Center;
            shadowLabel.anchor = TextAnchor.MiddleCenter;
            shadowLabel.color = new Color(0.04f, 0.18f, 0.24f, 0.88f);
        }
    }
}
