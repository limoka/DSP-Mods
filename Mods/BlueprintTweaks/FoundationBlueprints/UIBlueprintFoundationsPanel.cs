using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIBlueprintFoundationsPanel : UIBlueprintPanel
    {
        public Text countLabel;
        public Toggle enableToggle;

        private static bool ignoreEvent;

        public override int verticalSize => 45;

        public override void OnOpen()
        {
            ignoreEvent = true;
            enableToggle.isOn = BlueprintCopyExtension.isEnabled;
            ignoreEvent = false;
        }

        public override void OnUpdate()
        {
            if (!enableToggle.isOn) return;

            int reformsCount = inspector.blueprint.reforms.Length;
            countLabel.text = $"{reformsCount}  {"foundationsBPCountLabel".Translate()}";
        }

        public void OnToggleChanged(bool value)
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
    }
}