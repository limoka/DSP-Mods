namespace BlueprintTweaks
{
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
}