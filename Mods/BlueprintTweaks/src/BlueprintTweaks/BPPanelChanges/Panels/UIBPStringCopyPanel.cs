namespace BlueprintTweaks
{
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