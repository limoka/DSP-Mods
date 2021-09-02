using System.Collections.Generic;

namespace BlueprintTweaks
{
    public class UIStationPanel : UIBlueprintPanel
    {
        public List<UIBlueprintStationItem> items = new List<UIBlueprintStationItem>();

        public int recipeCount;
        public UIBlueprintStationItem prefab;

        public override int verticalSize
        {
            get
            {
                if (recipeCount == 0) return 22;

                return 22 + ((recipeCount - 1) / 2 + 1) * 46;
            }
        }

        public override void OnUpdate()
        {
            int num = 0;

            foreach (BlueprintBuilding blueprintBuilding in inspector.blueprint.buildings)
            {
                int protoId = blueprintBuilding.itemId;
                ItemProto proto = LDB.items.Select(protoId);
                if (!proto.prefabDesc.isStation || proto.prefabDesc.isCollectStation) continue;

                int[] parameters = blueprintBuilding.parameters;
                if (parameters != null && parameters.Length >= 2048)
                {
                    for (int i = 0; i < proto.prefabDesc.stationMaxItemKinds; i++)
                    {
                        if (parameters[i * 6] > 0)
                        {
                            int itemId = parameters[i * 6];
                            ELogisticStorage localLogic = (ELogisticStorage) parameters[i * 6 + 1];
                            ELogisticStorage remoteLogic = (ELogisticStorage) parameters[i * 6 + 2];
                            StationData data = new StationData()
                            {
                                itemId = itemId,
                                local = localLogic,
                                remote = remoteLogic,
                                slotId = i
                            };
                            SetItem(num++, blueprintBuilding.index, proto.prefabDesc.isStellarStation, data);
                        }
                    }
                }
            }

            ClearComponentItems(num);
        }

        public void SetItem(int index, int stationId, bool isStellar, StationData data)
        {
            if (index < 0)
            {
                return;
            }

            if (index > 256)
            {
                return;
            }

            while (index >= items.Count)
            {
                UIBlueprintStationItem uiblueprintComponentItem = Instantiate(prefab, prefab.transform.parent);
                uiblueprintComponentItem.Create(this);
                items.Add(uiblueprintComponentItem);
            }

            items[index].SetStation(stationId, isStellar);
            items[index].SetDisplay(index, data);
            items[index].Open();
        }

        public void ClearComponentItems(int activeCount = 0)
        {
            for (int i = activeCount; i < items.Count; i++)
            {
                items[i].Close();
            }

            recipeCount = activeCount;
        }

        public void ChangeItem(int stationId, int slotId, ItemProto newItem)
        {
            if (stationId < 0 || stationId >= inspector.blueprint.buildings.Length) return;

            BlueprintBuilding building = inspector.blueprint.buildings[stationId];

            int protoId = building.itemId;
            ItemProto proto = LDB.items.Select(protoId);
            if (!proto.prefabDesc.isStation || proto.prefabDesc.isCollectStation) return;

            int[] parameters = building.parameters;
            if (parameters == null || parameters.Length < 2048) return;
            
            if (parameters[slotId * 6] > 0)
            {
                parameters[slotId * 6] = newItem.ID;
            }

            if (inspector.usage == UIBlueprintInspector.EUsage.Paste)
            {
                inspector.pasteBuildTool.ResetStates();
            }

            inspector.Refresh(true, true, true);
        }
    }
}