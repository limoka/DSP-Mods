using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks.FactoryUndo
{
    public class UndoAddon : UndoBuild
    {
        public UndoAddon(PlayerUndo undoData, int objectId) : base(undoData, objectId) { }
        private UndoAddon(PlayerUndo undoData, IEnumerable<int> objectIds) : base(undoData, objectIds) { }

        public override bool Redo(PlanetFactory factory, PlayerAction_Build actionBuild)
        {
            if (previews == null) return false;
            if (previews.Count != 1) return false;
            
            BuildTool_Addon pathAddon = actionBuild.addonTool;

            pathAddon.InitTool();
            pathAddon.handbp = previews[0];
            bool condition;
            
            using (UndoManager.IgnoreAllEvents.On())
            {
                pathAddon.ActiveColliders();
                if (pathAddon.handPrefabDesc.addonType == EAddonType.Belt)
                {
                    pathAddon.FindPotentialBelt();
                    pathAddon.FindPotentialBeltStrict();
                }else if (pathAddon.handPrefabDesc.addonType == EAddonType.Storage)
                {
                    int count = factory.planet.physics.nearColliderLogic.GetBuildingsInAreaNonAlloc(previews[0].lpos, 2f, tmpIds, false);
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int objId = tmpIds[i];
                            Pose objPos = actionBuild.noneTool.GetObjectPose(objId);
                            PrefabDesc desc = actionBuild.noneTool.GetPrefabDesc(objId);
                            if (desc != null && desc.isStorage)
                            {
                                float num2 = desc.colliders[0].pos.y + desc.colliders[0].ext.y;
                                Vector3 ourPos = (objPos.position.magnitude + num2) * objPos.position.normalized;
                                if ((previews[0].lpos - ourPos).magnitude < 0.1f)
                                {
                                    pathAddon.handbp.inputObjId = objId;
                                    pathAddon.castObjectId = objId;
                                    pathAddon.handbp.inputFromSlot = 13;
                                    pathAddon.handbp.inputToSlot = 0;
                                    break;
                                }
                            }
                        }
                    }
                }

                condition = pathAddon.CheckBuildConditions();

                if (condition)
                {
                    pathAddon.CreatePrebuilds();
                }

                objectIds.Clear();
                objectIds.Add(pathAddon.handbp.objId);

                if (objectIds.Count > 0)
                {
                    if (!undoData.notifyBuildListeners.Contains(this))
                        undoData.notifyBuildListeners.Add(this);
                    if (!undoData.notifyDismantleListeners.Contains(this))
                        undoData.notifyDismantleListeners.Add(this);
                }
            }
                
            previews.Clear();
            previews = null;
            return condition;
        }
    }
}