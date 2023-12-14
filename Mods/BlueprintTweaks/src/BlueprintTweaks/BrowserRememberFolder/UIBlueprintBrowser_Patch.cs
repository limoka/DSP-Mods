using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace BlueprintTweaks.BrowserRememberFolder
{
    [HarmonyPatch]
    public static class UIBlueprintBrowser_Patch
    {

        [HarmonyPatch(typeof(UIBlueprintBrowser), nameof(UIBlueprintBrowser.SetCurrentDirectory))]
        [HarmonyPostfix]
        public static void OnOpenDirectory(UIBlueprintBrowser __instance, string fullpath)
        {
            __instance.openPath = fullpath;
        }

        [HarmonyPatch(typeof(UIBlueprintBrowser), nameof(UIBlueprintBrowser._OnOpen))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DontClearOpenPath(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldstr),
                    new CodeMatch(OpCodes.Stfld)
                )
                .SetOpcodeAndAdvance(OpCodes.Nop)
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
                .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop));

            return matcher.InstructionEnumeration();
        }
    }
}