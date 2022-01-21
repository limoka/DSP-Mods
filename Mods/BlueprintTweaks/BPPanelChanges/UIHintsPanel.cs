using System.Collections.Generic;

namespace BlueprintTweaks
{
    public class UIHintsPanel : UIBlueprintPanel
    {
        public List<UIHintItem> hintsItems = new List<UIHintItem>();
        public List<int> tmpHintList = new List<int>();

        public int hintsCount;

        public UIHintItem prefab;

        public override int verticalSize
        {
            get
            {
                if (hintsCount == 0) return 22;

                return 22 + ((hintsCount - 1) / 8 + 1) * 46;
            }
        }

        public override void OnUpdate()
        {
            tmpHintList.Clear();

            foreach (BlueprintBuilding blueprintBuilding in inspector.blueprint.buildings)
            {
                int protoId = blueprintBuilding.itemId;
                ItemProto proto = LDB.items.Select(protoId);

                if (proto.prefabDesc.isBelt)
                {
                    if (blueprintBuilding.parameters != null && blueprintBuilding.parameters.Length == 1)
                    {
                        tmpHintList.Add(blueprintBuilding.parameters[0]);
                    }
                }
            }

            int num = 0;
            foreach (int itemId in tmpHintList)
            {
                SetItem(num, itemId);
                num++;
            }

            ClearComponentItems(num);
        }

        public void SetItem(int index, int itemId)
        {
            if (index < 0)
            {
                return;
            }

            if (index > 256)
            {
                return;
            }

            while (index >= hintsItems.Count)
            {
                UIHintItem uiblueprintComponentItem = Instantiate(prefab, prefab.transform.parent);
                uiblueprintComponentItem.Create(this);
                hintsItems.Add(uiblueprintComponentItem);
            }

            hintsItems[index].SetDisplay(index, itemId);
            hintsItems[index].Open();
        }

        public void ClearComponentItems(int activeCount = 0)
        {
            for (int i = activeCount; i < hintsItems.Count; i++)
            {
                hintsItems[i].Close();
            }

            hintsCount = activeCount;
        }

        public void ChangeHint(int oldSignalId, int newSignal)
        {

            foreach (BlueprintBuilding blueprintBuilding in inspector.blueprint.buildings)
            {
                int protoId = blueprintBuilding.itemId;
                ItemProto proto = LDB.items.Select(protoId);

                if (!proto.prefabDesc.isBelt) continue;
                if (blueprintBuilding.parameters == null ) continue;
                if (blueprintBuilding.parameters.Length != 1) continue;
                
                if (blueprintBuilding.parameters[0] == oldSignalId)
                {
                    blueprintBuilding.parameters[0] = newSignal;
                }
            }


            if (inspector.usage == UIBlueprintInspector.EUsage.Paste)
            {
                inspector.pasteBuildTool.ResetStates();
            }

            inspector.Refresh(true, true, true);
        }
    }
}