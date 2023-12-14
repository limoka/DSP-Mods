using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIBlueprintFoundationsPanel : UIBlueprintPanel
    {
        public Text countLabel;
        public Toggle enableToggle;
        public Toggle copyColorsToggle;

        public Text colorDataLabel;
        
        private static bool ignoreEvent;

        public override int verticalSize => 75;

        public override void OnOpen()
        {
            ignoreEvent = true;
            enableToggle.isOn = BlueprintCopyExtension.isEnabled;
            copyColorsToggle.isOn = BlueprintCopyExtension.copyColors;
            ignoreEvent = false;
        }

        public override void OnUpdate()
        {
            if (!enableToggle.isOn) return;

            int reformsCount = inspector.blueprint.reforms.Length;
            countLabel.text = $"{reformsCount}  {"foundationsBPCountLabel".Translate()}";
            
            bool hasColors = inspector.blueprint.customColors != null && inspector.blueprint.customColors.Length > 0;
            colorDataLabel.text = hasColors ? "hasColorsLabel".Translate() : "";
        }

        public void OnToggleEnabled(bool value)
        {
            if (ignoreEvent) return;
            
            BlueprintCopyExtension.isEnabled = value;
            if (inspector.usage == UIBlueprintInspector.EUsage.Copy)
            {
                inspector.copyBuildTool.RefreshBlueprintData();
                inspector.copyBuildTool.DeterminePreviews();
            }

            if (inspector.usage == UIBlueprintInspector.EUsage.Paste)
            {
                inspector.pasteBuildTool.ResetStates();
            }

            inspector.Refresh(false, true, true);
        }

        public void OnToggleCopyColors(bool value)
        {
            if (ignoreEvent) return;
            BlueprintCopyExtension.copyColors = value;
        }
    }
}