using UnityEngine;

namespace BlueprintTweaks
{
    public abstract class UIBlueprintPanel : MonoBehaviour
    {
        public abstract int verticalSize { get; }

        [HideInInspector]
        public UIBlueprintInspector inspector;
        
        [HideInInspector]
        public RectTransform panelTrs;
        
        public virtual void Create(UIBlueprintInspector inspector)
        {
            this.inspector = inspector;
            panelTrs = (RectTransform) transform;
        }

        public void RefreshUI()
        {
            if (inspector.blueprint == null || !inspector.blueprint.isValid) return;
            OnUpdate();
        }

        public virtual void OnOpen()
        {
            
        }

        public virtual void OnUpdate()
        {
            
        }
    }
}