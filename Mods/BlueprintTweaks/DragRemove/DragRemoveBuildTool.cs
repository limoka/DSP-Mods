using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks
{
    public class DragRemoveBuildTool : BuildTool
    {
        public int segment => planet.aux.activeGrid?.segment ?? 200;

        public bool castTerrain;

        public bool castGround;

        public Vector3 castGroundPos = Vector3.zero;

        public Vector3 castGroundPosSnapped = Vector3.zero;

        public int castObjectId;

        public int castObjectIdDisplayed;

        public Vector3 castObjectPos;

        public bool cursorValid;

        public Vector3 cursorTarget;

        public Vector3 startGroundPosSnapped = Vector3.zero;

        public Vector3 lastGroundPosSnapped = Vector3.zero;

        public BPGratBox preSelectGratBox = BPGratBox.zero;

        public BPGratBox lastPreSelectGratBox = BPGratBox.zero;

        public BPGratBox preSelectArcBox = BPGratBox.zero;

        public HashSet<int> preSelectObjIds;

        public float divideLineRad = -3.1415927f;

        public BuildPreview[] bpPool;

        public AnimData[] animPool;

        public ComputeBuffer animBuffer;

        public int bpCursor = 1;

        private int bpPoolCapacity;

        private int[] bpRecycle;

        private int bpRecycleCursor;

        private List<int> _tmp_int_list;

        public bool isSelecting;

        private List<int> dismantleQueryObjectIds = new List<int>();
        private UIMessageBox dismantleQueryBox;
        
        private int neighborId0;
        private int neighborId1;
        private int neighborId2;
        private int neighborId3;

        public override void _OnInit()
        {
            preSelectObjIds = new HashSet<int>();
            SetDisplayPreviewCapacity(256);
        }

        public override void _OnFree()
        {
            preSelectObjIds = null;
            FreeBuildPreviews();
        }

        public override void _OnOpen()
        {
            ClearPreSelection();
            ResetBuildPreviews();
            castTerrain = false;
            castGround = false;
            castGroundPos = Vector3.zero;
            startGroundPosSnapped = lastGroundPosSnapped = castGroundPosSnapped = Vector3.zero;
            lastPreSelectGratBox = preSelectGratBox = preSelectArcBox = BPGratBox.zero;
            castObjectId = 0;
            castObjectIdDisplayed = 0;
            castObjectPos = Vector3.zero;
            cursorValid = false;
            cursorTarget = Vector3.zero;
            isSelecting = false;
        }

        public override void _OnClose()
        {
            DismantleQueryRemove();
            ClearPreSelection();
            ResetBuildPreviews();
            startGroundPosSnapped = lastGroundPosSnapped = castGroundPosSnapped = Vector3.zero;
            lastPreSelectGratBox = preSelectGratBox = preSelectArcBox = BPGratBox.zero;
            castObjectId = 0;
            castObjectIdDisplayed = 0;
            castObjectPos = Vector3.zero;
            cursorValid = false;
            cursorTarget = Vector3.zero;
        }

        public override void _OnTick(long time)
        {
            UpdateRaycast();
            Operating();
            if (active)
            {
                UpdatePreviewModels(actionBuild.model);
            }
        }

        public override bool DetermineActive()
        {
            if (actionBuild.blueprintMode != EBlueprintMode.None) return false;

            return controller.cmd.mode == -1 && actionBuild.dismantleTool.cursorType == 2;
        }

        public void UpdateRaycast()
        {
            castTerrain = false;
            castGround = false;
            castGroundPos = Vector3.zero;
            castGroundPosSnapped = Vector3.zero;
            castObjectId = 0;
            castObjectPos = Vector3.zero;
            cursorValid = false;
            cursorTarget = Vector3.zero;
            if (!VFInput.onGUI && VFInput.inScreen)
            {
                const int layerMask = 8720;
                castGround = Physics.Raycast(mouseRay, out RaycastHit raycastHit, 800f, layerMask, QueryTriggerInteraction.Collide);
                if (castGround)
                {
                    Layer layer = (Layer) raycastHit.collider.gameObject.layer;
                    castTerrain = layer == Layer.Terrain || layer == Layer.Water;
                    castGroundPos = controller.cmd.test = controller.cmd.target = raycastHit.point;
                    castGroundPosSnapped = actionBuild.planetAux.Snap(castGroundPos, castTerrain);
                    castGroundPosSnapped = castGroundPosSnapped.normalized * (planet.realRadius + 0.2f);
                    controller.cmd.test = castGroundPosSnapped;
                    Vector3 normalized = castGroundPosSnapped.normalized;
                    if (Physics.Raycast(new Ray(castGroundPosSnapped + normalized * 10f, -normalized), out raycastHit, 20f, layerMask,
                        QueryTriggerInteraction.Collide))
                    {
                        controller.cmd.test = raycastHit.point;
                    }

                    cursorTarget = castGroundPosSnapped;
                    cursorValid = true;
                }

                int castAllCount = controller.cmd.raycast.castAllCount;
                RaycastData[] castAll = controller.cmd.raycast.castAll;
                int num = 0;
                for (int i = 0; i < castAllCount; i++)
                {
                    if (castAll[i].objType == EObjectType.Entity || castAll[i].objType == EObjectType.Prebuild)
                    {
                        num = castAll[i].objType == EObjectType.Entity ? castAll[i].objId : -castAll[i].objId;
                        break;
                    }
                }

                if (num != 0)
                {
                    castObjectId = num;
                    castObjectPos = GetObjectPose(num).position;
                    cursorTarget = castObjectPos;
                    controller.cmd.test = castObjectPos;
                    castGroundPosSnapped = castGroundPos = castObjectPos;
                    castGroundPosSnapped = castGroundPosSnapped.normalized * (planet.realRadius + 0.2f);
                    controller.cmd.test = castGroundPosSnapped;
                    Vector3 normalized2 = castGroundPosSnapped.normalized;
                    if (Physics.Raycast(new Ray(castGroundPosSnapped + normalized2 * 10f, -normalized2), out raycastHit, 20f, layerMask,
                        QueryTriggerInteraction.Collide))
                    {
                        controller.cmd.test = raycastHit.point;
                    }

                    cursorTarget = castGroundPosSnapped;
                    cursorValid = true;
                }
            }

            controller.cmd.state = cursorValid ? 1 : 0;
            controller.cmd.target = cursorValid ? cursorTarget : Vector3.zero;
        }

        public void Operating()
        {
            if (!isSelecting && VFInput.blueprintCopyOperate0.onDown && cursorValid)
            {
                isSelecting = true;
                startGroundPosSnapped = castGroundPosSnapped;
                lastGroundPosSnapped = startGroundPosSnapped;
                InitPreSelectGratBox();
                VFInput.UseMouseLeft();
            }

            bool point = (castGroundPosSnapped - startGroundPosSnapped).sqrMagnitude > 0.01f;

            bool onDown = VFInput.blueprintCopyOperate0.onDown || VFInput.blueprintCopyOperate1.onDown;
            if (isSelecting && (onDown && cursorValid || VFInput.blueprintCopyOperate0.onUp && castObjectId != 0 && !point))
            {
                CheckDismantle();   
                ClearPreSelection();
                DeterminePreviews();
                isSelecting = false;
                VFInput.UseMouseLeft();
                VFInput.UseEnterConfirm();
            }
            else if (isSelecting && VFInput.rtsCancel.onUp)
            {
                isSelecting = false;
                startGroundPosSnapped = castGroundPosSnapped;
                lastGroundPosSnapped = startGroundPosSnapped;
                ClearPreSelection();
                DeterminePreviews();
            }

            bool changed = false;
            if (isSelecting)
            {
                DeterminePreSelectGratBox();
                InitDivideLine();
                if (lastPreSelectGratBox != preSelectGratBox)
                {
                    DetermineAddPreSelection();
                    lastPreSelectGratBox = preSelectGratBox;
                    changed = true;
                }
            }
            else
            {
                startGroundPosSnapped = castGroundPosSnapped;
                changed = castObjectId != castObjectIdDisplayed;
            }

            if (changed)
            {
                DeterminePreviews();
            }
        }

        public void CheckDismantle()
        {
            foreach (int objId in preSelectObjIds)
            {
                EntityData data = factory.entityPool[objId];

                PrefabDesc desc = LDB.items.Select(data.protoId).prefabDesc;

                if (BuildTool_Dismantle.showDemolishContainerQuery)
                {
                    if (objId > 0 && desc.isStorage)
                    {
                        int storageId = data.storageId;
                        if (!factory.factoryStorage.TryTakeBackItems_Storage(player.package, storageId))
                        {
                            dismantleQueryObjectIds.Clear();
                            foreach (int objId2 in preSelectObjIds)
                            {
                                dismantleQueryObjectIds.Add(objId2);
                            }

                            dismantleQueryBox = UIMessageBox.Show("拆除储物仓标题".Translate(), "拆除储物仓文字".Translate(), "否".Translate(),
                                "是".Translate(), 0, DismantleQueryCancel, DismantleQueryConfirm);
                            return;
                        }
                    }

                    if (objId > 0 && desc.isTank)
                    {
                        int tankId = data.tankId;
                        if (!factory.factoryStorage.TryTakeBackItems_Tank(player.package, tankId))
                        {
                            dismantleQueryObjectIds.Clear();
                            foreach (int objId2 in preSelectObjIds)
                            {
                                dismantleQueryObjectIds.Add(objId2);
                            }

                            dismantleQueryBox = UIMessageBox.Show("拆除储液罐标题".Translate(), "拆除储液罐文字".Translate(), "否".Translate(),
                                "是".Translate(), 0, DismantleQueryCancel, DismantleQueryConfirm);
                            return;
                        }
                    }

                    if (objId > 0 && desc.isStation)
                    {
                        int stationId = data.stationId;
                        if (factory.transport.stationPool[stationId] != null)
                        {
                            dismantleQueryObjectIds.Clear();
                            foreach (int objId2 in preSelectObjIds)
                            {
                                dismantleQueryObjectIds.Add(objId2);
                            }

                            dismantleQueryBox = UIMessageBox.Show("拆除物流站标题".Translate(), "拆除物流站文字".Translate(), "否".Translate(),
                                "是".Translate(), 0, DismantleQueryCancel, DismantleQueryConfirm);
                            return;
                        }
                    }
                }
            }
            
            DismantleAction();
        }

        public void DismantleAction()
        {
            int num = 0;
            foreach (int objId in preSelectObjIds)
            {
                if (ObjectIsBelt(objId))
                {
                    factory.ReadObjectConn(objId, 0, out bool _, out neighborId0, out int _);
                    factory.ReadObjectConn(objId, 1, out bool _, out neighborId1, out int _);
                    factory.ReadObjectConn(objId, 2, out bool _, out neighborId2, out int _);
                    factory.ReadObjectConn(objId, 3, out bool _, out neighborId3, out int _);
                    if (!ObjectIsBelt(neighborId0))
                    {
                        neighborId0 = 0;
                    }

                    if (!ObjectIsBelt(neighborId1))
                    {
                        neighborId1 = 0;
                    }

                    if (!ObjectIsBelt(neighborId2))
                    {
                        neighborId2 = 0;
                    }

                    if (!ObjectIsBelt(neighborId3))
                    {
                        neighborId3 = 0;
                    }
                }

                if (actionBuild.DoDismantleObject(objId))
                {
                    num++;
                }
            }

            if (num > 5)
            {
                VFAudio.Create("demolish-large", null, GameMain.mainPlayer.position, true, 5);
            }

            if (!actionBuild.dismantleTool.chainReaction)
            {
                int num3 = 0;
                foreach (int objId in preSelectObjIds)
                {
                    if ((objId == neighborId0 || objId == neighborId1 || objId == neighborId2 || objId == neighborId3))
                    {
                        if (ObjectIsBelt(objId))
                        {
                            factory.ReadObjectConn(objId, 0, out bool _, out neighborId0, out int _);
                            factory.ReadObjectConn(objId, 1, out bool _, out neighborId1, out int _);
                            factory.ReadObjectConn(objId, 2, out bool _, out neighborId2, out int _);
                            factory.ReadObjectConn(objId, 3, out bool _, out neighborId3, out int _);
                            if (!ObjectIsBelt(neighborId0))
                            {
                                neighborId0 = 0;
                            }

                            if (!ObjectIsBelt(neighborId1))
                            {
                                neighborId1 = 0;
                            }

                            if (!ObjectIsBelt(neighborId2))
                            {
                                neighborId2 = 0;
                            }

                            if (!ObjectIsBelt(neighborId3))
                            {
                                neighborId3 = 0;
                            }
                        }

                        if (actionBuild.DoDismantleObject(objId))
                        {
                            num3++;
                        }
                    }
                }

                if (num3 > 5)
                {
                    VFAudio.Create("demolish-large", null, GameMain.mainPlayer.position, true, 5);
                }
            }
        }

        public void DismantleQueryCancel()
        {
            dismantleQueryObjectIds.Clear();
            dismantleQueryBox = null;
        }

        public void DismantleQueryConfirm()
        {
            preSelectObjIds.Clear();
            foreach (int objId in dismantleQueryObjectIds)
            {
                preSelectObjIds.Add(objId);
            }
            DismantleAction();

            dismantleQueryBox = null;
        }
        
        public void DismantleQueryRemove()
        {
            dismantleQueryObjectIds.Clear();
            if (dismantleQueryBox != null)
            {
                dismantleQueryBox.OnButton1Click();
                dismantleQueryBox = null;
            }
        }

        private void InitPreSelectGratBox()
        {
            BlueprintUtils.GetMinimumGratBox(startGroundPosSnapped.normalized, ref preSelectGratBox);
            preSelectArcBox = preSelectGratBox;
            if (preSelectArcBox.y >= 1.5707864f)
            {
                preSelectArcBox.y = preSelectArcBox.w = 1.5707964f;
                preSelectArcBox.z = preSelectArcBox.x + 628.31854f;
            }
            else if (preSelectArcBox.y <= -1.5707864f)
            {
                preSelectArcBox.y = preSelectArcBox.w = -1.5707964f;
                preSelectArcBox.z = preSelectArcBox.x + 628.31854f;
            }

            lastPreSelectGratBox = preSelectGratBox;
        }

        public void DeterminePreSelectGratBox()
        {
            if (cursorValid)
            {
                float longitudeRad = BlueprintUtils.GetLongitudeRad(castGroundPosSnapped.normalized);
                float longitudeRad2 = BlueprintUtils.GetLongitudeRad(lastGroundPosSnapped.normalized);
                float latitudeRad = BlueprintUtils.GetLatitudeRad(castGroundPosSnapped.normalized);
                bool flag = latitudeRad >= 1.5707864f || latitudeRad <= -1.5707864f;
                float num = flag ? 0f : longitudeRad - longitudeRad2;
                num = Mathf.Repeat(num + 3.1415927f, 6.2831855f) - 3.1415927f;
                preSelectArcBox.endLongitudeRad = preSelectArcBox.endLongitudeRad + num;
                preSelectArcBox.endLatitudeRad = latitudeRad;
                preSelectGratBox = preSelectArcBox;
                preSelectGratBox.x = preSelectArcBox.x < preSelectArcBox.z ? preSelectArcBox.x : preSelectArcBox.z;
                preSelectGratBox.z = preSelectArcBox.x > preSelectArcBox.z ? preSelectArcBox.x : preSelectArcBox.z;
                if (preSelectArcBox.x < preSelectArcBox.z)
                {
                    if (preSelectGratBox.z > preSelectGratBox.x + 6.2831855f - 1E-05f - 4E-06f)
                    {
                        preSelectGratBox.z = preSelectGratBox.x + 6.2831855f - 1E-05f - 4E-06f;
                    }

                    preSelectGratBox.z = Mathf.Repeat(preSelectGratBox.z + 3.1415927f, 6.2831855f) - 3.1415927f;
                }
                else
                {
                    if (preSelectGratBox.x < preSelectGratBox.z - 6.2831855f + 1E-05f + 4E-06f)
                    {
                        preSelectGratBox.x = preSelectGratBox.z - 6.2831855f + 1E-05f + 4E-06f;
                    }

                    preSelectGratBox.x = Mathf.Repeat(preSelectGratBox.x + 3.1415927f, 6.2831855f) - 3.1415927f;
                }

                preSelectGratBox.y = preSelectArcBox.y < preSelectArcBox.w ? preSelectArcBox.y : preSelectArcBox.w;
                preSelectGratBox.w = preSelectArcBox.y > preSelectArcBox.w ? preSelectArcBox.y : preSelectArcBox.w;
                float longitude = BlueprintUtils.GetLongitudeRadPerGrid(Mathf.Abs(castGroundPosSnapped.y) < Mathf.Abs(startGroundPosSnapped.y)
                    ? castGroundPosSnapped.normalized
                    : startGroundPosSnapped.normalized) * 0.33f;
                preSelectGratBox.Extend(longitude, 0.002f);
                if (!flag)
                {
                    lastGroundPosSnapped = castGroundPosSnapped;
                }
            }
        }

        public void DetermineAddPreSelection()
        {
            preSelectObjIds.Clear();
            if (Mathf.Abs(preSelectArcBox.x - preSelectArcBox.z) < 0.01f && Mathf.Abs(preSelectArcBox.y - preSelectArcBox.w) < 0.01f && castObjectId != 0)
            {
                if (ShouldAddObject(castObjectId))
                {
                    preSelectObjIds.Add(castObjectId);
                }
            }
            else
            {
                EntityData[] entityPool = factory.entityPool;
                int entityCursor = factory.entityCursor;
                for (int i = 1; i < entityCursor; i++)
                {
                    int item = i;
                    if (entityPool[i].id == i && preSelectGratBox.InGratBox(entityPool[i].pos))
                    {
                        if (ShouldAddObject(item))
                        {
                            preSelectObjIds.Add(item);
                        }
                    }
                }

                PrebuildData[] prebuildPool = factory.prebuildPool;
                int prebuildCursor = factory.prebuildCursor;
                for (int j = 1; j < prebuildCursor; j++)
                {
                    int item2 = -j;
                    if (prebuildPool[j].id == j && preSelectGratBox.InGratBox(prebuildPool[j].pos))
                    {
                        if (ShouldAddObject(item2))
                        {
                            preSelectObjIds.Add(item2);
                        }
                    }
                }
            }

            DetermineChainPreSelection();
        }

        public bool ShouldAddObject(int objId)
        {
            EntityData data = factory.entityPool[objId];
            PrefabDesc desc = LDB.items.Select(data.protoId).prefabDesc;

            if (desc.isInserter)
            {
                return actionBuild.dismantleTool.filterInserter;
            }

            if (desc.isBelt)
            {
                return actionBuild.dismantleTool.filterBelt;
            }

            return actionBuild.dismantleTool.filterFacility;
        }

        public void DetermineChainPreSelection()
        {
            if (!VFInput._chainReaction)
            {
                return;
            }

            if (_tmp_int_list == null)
            {
                _tmp_int_list = new List<int>();
            }

            _tmp_int_list.Clear();
            foreach (int objId in preSelectObjIds)
            {
                int num = GetPrefabDesc(objId).slotPoses.Length;
                if (num > 0)
                {
                    for (int i = 0; i < num; i++)
                    {
                        factory.ReadObjectConn(objId, i, out bool _, out int num2, out int _);
                        if (num2 != 0)
                        {
                            _tmp_int_list.Add(num2);
                        }
                    }
                }
            }

            foreach (int item in _tmp_int_list)
            {
                if (ShouldAddObject(item))
                {
                    preSelectObjIds.Add(item);
                }
            }

            _tmp_int_list.Clear();
        }

        public void InitDivideLine()
        {
            divideLineRad = Mathf.Repeat(preSelectArcBox.x, 6.2831855f) - 3.1415927f;
        }


        public void ClearPreSelection()
        {
            preSelectObjIds.Clear();
            lastPreSelectGratBox = preSelectGratBox = preSelectArcBox = BPGratBox.zero;
        }

        public void DeterminePreviews()
        {
            ResetBuildPreviews();
            foreach (int objId in preSelectObjIds)
            {
                BuildPreview buildPreview = GetBuildPreview(objId);

                AddBPGPUIModel(buildPreview);
            }

            if (castObjectId != 0)
            {
                BuildPreview buildPreview3 = GetBuildPreview(castObjectId);

                AddBPGPUIModel(buildPreview3);
            }

            castObjectIdDisplayed = castObjectId;
            SyncAnimBuffer();
            planet.factoryModel.bpgpuiManager.animBuffer = animBuffer;
            planet.factoryModel.bpgpuiManager.SyncAllGPUBuffer();
        }

        public override void UpdatePreviewModels(BuildModel model)
        {
            for (int i = 1; i < bpCursor; i++)
            {
                BuildPreview buildPreview = bpPool[i];
                if (buildPreview != null && buildPreview.bpgpuiModelId > 0 && buildPreview.isConnNode)
                {
                    if (buildPreview.objId > 0)
                    {
                        factory.cargoTraffic.SetBeltSelected(factory.entityPool[buildPreview.objId].beltId);
                    }
                    else
                    {
                        uint color = (uint) buildPreview.desc.beltSpeed;
                        if (buildPreview.outputObjId == 0 || buildPreview.inputObjId == 0 || buildPreview.coverbp != null)
                        {
                            model.connRenderer.AddBlueprintBeltMajorPoint(buildPreview.lpos, buildPreview.lrot, color);
                        }
                        else
                        {
                            model.connRenderer.AddBlueprintBeltPoint(buildPreview.lpos, buildPreview.lrot, color);
                        }
                    }

                    model.connRenderer.AddXSign(buildPreview.lpos, buildPreview.lrot);
                }
            }
        }


        public void AddBPGPUIModel(BuildPreview preview)
        {
            if (preview == null || preview.bpgpuiModelId <= 0)
            {
                return;
            }

            if (!preview.needModel)
            {
                return;
            }

            ModelProto modelProto = LDB.models.Select(preview.desc.modelIndex);
            Color32 color = Configs.builtin.copyErrorColor;

            if (modelProto.RendererType == 2)
            {
                GetInserterT1T2(preview.objId, out bool flag, out bool flag2);

                if (preview.objId > 0)
                {
                    animPool[preview.bpgpuiModelId] = factory.entityAnimPool[preview.objId];
                }

                animPool[preview.bpgpuiModelId].state = (uint) ((color.r << 24) + (color.g << 16) + (color.b << 8) + color.a);
                planet.factoryModel.bpgpuiManager.AddBuildPreviewModel(preview.desc.modelIndex, out preview.bpgpuiModelInstIndex, preview.bpgpuiModelId,
                    preview.lpos, preview.lrot, preview.lpos2, preview.lrot2, flag ? 1 : 0, flag2 ? 1 : 0, false);
                return;
            }

            if (modelProto.RendererType == 3)
            {
                factory.ReadObjectConn(preview.objId, 14, out bool _, out int num, out int _);

                if (preview.objId > 0)
                {
                    animPool[preview.bpgpuiModelId] = factory.entityAnimPool[preview.objId];
                }

                animPool[preview.bpgpuiModelId].state = (uint) ((color.r << 24) + (color.g << 16) + (color.b << 8) + color.a);
                planet.factoryModel.bpgpuiManager.AddBuildPreviewModel(preview.desc.modelIndex, out preview.bpgpuiModelInstIndex, preview.bpgpuiModelId,
                    preview.lpos, preview.lrot, num != 0 ? 1U : 0U, false);
                return;
            }

            if (preview.objId > 0)
            {
                animPool[preview.bpgpuiModelId] = factory.entityAnimPool[preview.objId];
            }

            animPool[preview.bpgpuiModelId].state = (uint) ((color.r << 24) + (color.g << 16) + (color.b << 8) + color.a);
            if (preview.objId > 0 && preview.desc.isEjector)
            {
                animPool[preview.bpgpuiModelId].power = factory.factorySystem.ejectorPool[factory.entityPool[preview.objId].ejectorId].localDir.z;
            }

            planet.factoryModel.bpgpuiManager.AddBuildPreviewModel(preview.desc.modelIndex, out preview.bpgpuiModelInstIndex, preview.bpgpuiModelId,
                preview.lpos, preview.lrot, false);
        }

        public void GeneratePreviewByObjId(BuildPreview preview, int objId)
        {
            ItemProto itemProto = GetItemProto(objId);
            PrefabDesc prefabDesc = GetPrefabDesc(objId);
            if (prefabDesc == null || itemProto == null)
            {
                preview.ResetAll();
                return;
            }

            Pose objectPose = GetObjectPose(objId);
            Pose pose = prefabDesc.isInserter ? GetObjectPose2(objId) : objectPose;
            preview.item = itemProto;
            preview.desc = prefabDesc;
            preview.lpos = objectPose.position;
            preview.lrot = objectPose.rotation;
            preview.lpos2 = objectPose.position;
            preview.lrot2 = objectPose.rotation;
            preview.objId = objId;
            preview.genNearColliderArea2 = 0f;
            if (preview.desc.lodCount > 0 && preview.desc.lodMeshes != null && preview.desc.lodMeshes[0] != null)
            {
                preview.needModel = true;
            }
            else
            {
                preview.needModel = false;
            }

            preview.isConnNode = prefabDesc.isBelt;
            if (prefabDesc.isBelt)
            {
                for (int i = 0; i < 4; i++)
                {
                    factory.ReadObjectConn(objId, i, out bool flag, out int num, out int num2);
                    if (num != 0)
                    {
                        if (flag)
                        {
                            preview.outputObjId = num;
                        }
                        else if (preview.inputObjId == 0)
                        {
                            preview.inputObjId = num;
                        }
                        else
                        {
                            preview.coverbp = preview;
                        }
                    }
                }
            }

            if (prefabDesc.isInserter)
            {
                preview.lpos2 = pose.position;
                preview.lrot2 = pose.rotation;
            }
        }

        public void ResetBuildPreviews()
        {
            if (planet != null && planet.factoryModel != null && planet.factoryModel.bpgpuiManager != null)
            {
                planet.factoryModel.bpgpuiManager.Reset();
            }

            for (int i = 0; i < bpPool.Length; i++)
            {
                if (bpPool[i] != null)
                {
                    bpPool[i].ResetAll();
                }
            }

            Array.Clear(animPool, 0, bpPoolCapacity);
            Array.Clear(bpRecycle, 0, bpPoolCapacity);
            bpCursor = 1;
            bpRecycleCursor = 0;
            animBuffer.SetData(animPool);
        }

        public void FreeBuildPreviews()
        {
            if (planet != null && planet.factoryModel != null && planet.factoryModel.bpgpuiManager != null)
            {
                planet.factoryModel.bpgpuiManager.Reset();
            }

            for (int i = 0; i < bpPool.Length; i++)
            {
                if (bpPool[i] != null)
                {
                    bpPool[i].Free();
                    bpPool[i] = null;
                }
            }

            animPool = null;
            bpPool = null;
            bpCursor = 1;
            bpPoolCapacity = 0;
            bpRecycle = null;
            bpRecycleCursor = 0;
            if (animBuffer != null)
            {
                animBuffer.Release();
                animBuffer = null;
            }
        }

        private void SetDisplayPreviewCapacity(int newCapacity)
        {
            BuildPreview[] array = bpPool;
            AnimData[] sourceArray = animPool;
            bpPool = new BuildPreview[newCapacity];
            animPool = new AnimData[newCapacity];
            bpRecycle = new int[newCapacity];
            if (array != null)
            {
                Array.Copy(array, bpPool, newCapacity > bpPoolCapacity ? bpPoolCapacity : newCapacity);
                Array.Copy(sourceArray, animPool, newCapacity > bpPoolCapacity ? bpPoolCapacity : newCapacity);
            }

            bpPoolCapacity = newCapacity;
            animBuffer?.Release();
            animBuffer = new ComputeBuffer(newCapacity, 20, ComputeBufferType.Default);
        }

        public BuildPreview GetBuildPreview(int objId)
        {
            int num;
            if (bpRecycleCursor > 0)
            {
                int[] array = bpRecycle;
                num = bpRecycleCursor - 1;
                bpRecycleCursor = num;
                int num2 = array[num];
                BuildPreview buildPreview = bpPool[num2];
                if (buildPreview == null)
                {
                    buildPreview = new BuildPreview();
                    bpPool[num2] = buildPreview;
                }

                GeneratePreviewByObjId(buildPreview, objId);
                animPool[num2] = default;
                buildPreview.previewIndex = num2;
                buildPreview.bpgpuiModelId = num2;
                return buildPreview;
            }

            num = bpCursor;
            bpCursor = num + 1;
            int num3 = num;
            if (num3 == bpPoolCapacity)
            {
                SetDisplayPreviewCapacity(bpPoolCapacity * 2);
            }

            BuildPreview buildPreview2 = bpPool[num3];
            if (buildPreview2 == null)
            {
                buildPreview2 = new BuildPreview();
                bpPool[num3] = buildPreview2;
            }

            GeneratePreviewByObjId(buildPreview2, objId);
            animPool[num3] = default(AnimData);
            buildPreview2.previewIndex = num3;
            buildPreview2.bpgpuiModelId = num3;
            return buildPreview2;
        }

        public void SyncAnimBuffer()
        {
            animBuffer?.SetData(animPool);
        }

        public override void EscLogic()
        {
            bool outsideGUI = !VFInput.onGUI && VFInput.inScreen && !VFInput.inputing;
            bool escape = VFInput.escKey.onDown || VFInput.escape;
            bool rtsCancel = !VFInput._godModeMechaMove && VFInput.rtsCancel.onDown && outsideGUI;
            bool exit = rtsCancel || escape;

            if (exit)
            {
                player.SetHandItems(0, 0);
                _Close();
                actionBuild.Close();
            }

            if (escape)
            {
                VFInput.UseEscape();
            }

            if (rtsCancel)
            {
                VFInput.UseMouseRight();
            }
        }
    }
}