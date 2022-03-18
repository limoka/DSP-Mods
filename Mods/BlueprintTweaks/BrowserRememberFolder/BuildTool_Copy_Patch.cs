using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using HarmonyLib;

namespace BlueprintTweaks.BrowserRememberFolder
{
    [HarmonyPatch]
    public static class BuildTool_Copy_Patch
    {
        [HarmonyPatch(typeof(BuildTool_BlueprintCopy), nameof(BuildTool_BlueprintCopy._OnOpen))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DontClearOpenPath(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr))

                .SetInstruction(Transpilers.EmitDelegate<Func<string>>(() =>
                {
                    string currentPath = UIRoot.instance.uiGame.blueprintBrowser.openPath;
                    
                    return BlueprintData.GenerateNewFileName(currentPath);
                }));

            return matcher.InstructionEnumeration();
        }
    }
}