using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class StationData
    {
        public int slotId;
        public int itemId;
        public ELogisticStorage local;
        public ELogisticStorage remote;
    }
    
    public class UIBlueprintStationItem : MonoBehaviour
    {
        public int position
        {
            get => Mathf.RoundToInt(rectTrans.anchoredPosition.x / 184f) + Mathf.RoundToInt(-rectTrans.anchoredPosition.y / 46f) * 2;
            set => rectTrans.anchoredPosition = new Vector2(value % 2 * 184, -(value / 2 * 46));
        }
        
        [HideInInspector]
        public UIStationPanel panel;

        public RectTransform rectTrans;

        public UIButton button;

        public Image iconImage;
        
        public Text localText;
        public Text remoteText;
        
        private int stationId;
        private bool isStellar;

        private StationData currentItem;


        public void Create(UIStationPanel panel)
        {
            this.panel = panel;
            button.onClick += OnClick;
        }
        

        public void Free()
        {
            button.onClick -= OnClick;
            stationId = 0;
            button.tips.itemId = 0;
            button.data = 0;
            iconImage.sprite = null;
            iconImage.color = Color.clear;
        }
        
        public void Open()
        {
            gameObject.SetActive(true);
        }


        public void Close()
        {
            button.tips.itemId = 0;
            button.data = 0;
            stationId = 0;
            currentItem = null;
            gameObject.SetActive(false);
        }
        
        private static string GetLogisticText(ELogisticStorage logic, bool isStellar, bool careStellar)
        {
            switch (careStellar)
            {
                case true when !isStellar:
                    switch (logic)
                    {
                        //local stellar
                        case ELogisticStorage.Supply:
                            return "本地供应".Translate();
                        case ELogisticStorage.Demand:
                            return "本地需求".Translate();
                        default:
                            return "本地仓储".Translate();
                    }
                case true:
                    switch (logic)
                    {
                        //remote stellar
                        case ELogisticStorage.Supply:
                            return "星际供应".Translate();
                        case ELogisticStorage.Demand:
                            return "星际需求".Translate();
                        default:
                            return "星际仓储".Translate();
                    }
                default:
                    switch (logic)
                    {
                        //pls
                        case ELogisticStorage.Supply:
                            return "供应".Translate();
                        case ELogisticStorage.Demand:
                            return "需求".Translate();
                        default:
                            return "仓储".Translate();
                    }
            }
        }


        public void SetStation(int newStationId, bool isStellar)
        {
            stationId = newStationId;
            this.isStellar = isStellar;
        }
        
        public void SetDisplay(int newIndex, StationData newItem)
        {
            position = newIndex;
            if (currentItem == null || currentItem.slotId != newItem.slotId || currentItem.itemId != newItem.itemId)
            {
                currentItem = newItem;

                localText.text = GetLogisticText(currentItem.local, false, isStellar);
                remoteText.text = isStellar ? GetLogisticText(currentItem.remote, true, true) : "";

                ItemProto proto = LDB.items.Select(currentItem.itemId);
                
                if (proto != null)
                {
                    button.tips.itemId = currentItem.itemId;
                    iconImage.sprite = proto.iconSprite;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                }
            }

        }


        private void OnClick(int obj)
        {
            VFAudio.Create("ui-click-0", null, Vector3.zero, true, 1);

            Vector2 pos =  new Vector2(-300, 238);
            
            UIItemPicker.Close();
            UIItemPicker.Popup(pos, proto =>
            {
                if (proto != null)
                    panel.ChangeItem(stationId, currentItem.slotId, proto);
            });
        }
    }
}