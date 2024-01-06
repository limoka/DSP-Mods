using System;
using System.Text;
using BepInEx.Configuration;
using CommonAPI.Systems;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIKeybindHintsPanel : UIBlueprintPanel
    {
        public ConfigEntry<bool> isCollapsed;
        public int currentSize = 22;

        public Text collapseText;
        public Text hintText;
        
        public override int verticalSize => currentSize;

        private void Awake()
        {
            isCollapsed = BlueprintTweaksPlugin.configFile.Bind(
                "Runtime", 
                "KeyBindHints_IsCollapsed", 
                true, 
                "This is a runtime option. You don't need to change it here, it can be altered ingame!");
        }

        public override void OnOpen()
        {
            OnUpdate();
        }

        public override void OnUpdate()
        {
            currentSize = 22;

            string hintCollapseText = "KeyHintsCollapseText".Translate();
            string triangle = isCollapsed.Value ? "▶" : "▼";

            collapseText.text = $"{triangle} {hintCollapseText}";
            
            if (isCollapsed.Value)
            {
                hintText.text = "";
                return;
            }

            StringBuilder sb = new StringBuilder();
            
            foreach (PressKeyBind keyBind in BlueprintTweaksPlugin.modKeybinds)
            {
                CombineKey key = keyBind.defaultBind.key;
                
                if (!VFInput.override_keys[keyBind.defaultBind.id].IsNull())
                {
                    key = VFInput.override_keys[keyBind.defaultBind.id];
                }

                string actionTerm = $"KEY{keyBind.defaultBind.name}";

                sb.Append(string.Format("KeyHintsPress".Translate(), key.ToString(), actionTerm.Translate()));
                sb.Append('\n');
                currentSize += 22;
            }

            hintText.text = sb.ToString();
        }


        public void OnCollapseClicked()
        {
            isCollapsed.Value = !isCollapsed.Value;
            inspector.Refresh(true, true, true);
        }
    }
}