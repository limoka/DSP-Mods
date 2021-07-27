using System;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIBlueprintSizePanel : MonoBehaviour
    {
        [HideInInspector]
        public UIBlueprintInspector inspector;

        [HideInInspector]
        public RectTransform panelTrs;
        
        public InputField longField;
        public InputField latField;

        private bool inited;
        
        public void Create(UIBlueprintInspector inspector)
        {
            this.inspector = inspector;
            panelTrs = (RectTransform) transform;
        }

        private void OnEnable()
        {
            if (inited) return;
            longField.textComponent.gameObject.SetActive(false);
            latField.textComponent.gameObject.SetActive(false);
            Invoke(nameof(FixInvisible), 0.2f);
        }

        private void FixInvisible()
        {
            longField.textComponent.gameObject.SetActive(true);
            latField.textComponent.gameObject.SetActive(true);
            inited = true;
        }

        public void UpdateFields(BlueprintData blueprint)
        {
            longField.text = blueprint.dragBoxSize_x.ToString();
            latField.text = blueprint.dragBoxSize_y.ToString();
        }

        public void SetLongtitude(string value)
        {
            int longSize = int.Parse(value);
            inspector.blueprint.dragBoxSize_x = longSize;
        }
        
        public void SetLatitude(string value)
        {
            int latSize = int.Parse(value);
            inspector.blueprint.dragBoxSize_y = latSize;
        }
    }
}