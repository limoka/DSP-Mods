namespace BlueprintTweaks
{
    public class UIBPReformsPanel : UIBlueprintPanel
    {
        public override int verticalSize
        {
            get
            {
                if (inspector.blueprint.reformData.reformCount == 0) return 0;
                
                return 68;
            }
        }

        public override void Create(UIBlueprintInspector inspector)
        {
            base.Create(inspector);
            panelTrs = inspector.group4;
        }
    }
}