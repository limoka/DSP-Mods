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

    public class UIBPComponentPanel : UIBlueprintPanel
    {
        public override int verticalSize
        {
            get
            {
                if (inspector.componentCount == 0) return 22;
                
                return 22 + ((inspector.componentCount - 1) / 8 + 1) * 46;
            }
        }

        public override void Create(UIBlueprintInspector inspector)
        {
            base.Create(inspector);
            panelTrs = inspector.group2;
        }
    }
    
    public class UIBPStringCopyPanel: UIBlueprintPanel
    {
        public override int verticalSize => 110;
        
        public override void Create(UIBlueprintInspector inspector)
        {
            base.Create(inspector);
            panelTrs = inspector.group3;
        }
    }
}