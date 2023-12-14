using CommonAPI.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIHintItem : MonoBehaviour
    {
        public int position
        {
            get => Mathf.RoundToInt(rectTrans.anchoredPosition.x / 46f) + Mathf.RoundToInt(-rectTrans.anchoredPosition.y / 46f) * 8;
            set => rectTrans.anchoredPosition = new Vector2(value % 8 * 46, -(value / 8 * 46));
        }
        
        [HideInInspector]
        public UIHintsPanel panel;

        public RectTransform rectTrans;

        public UIButton button;

        public Image iconImage;
        public Text countText;

        private HintData currentHint;

        private const int kMargin = 0;

        private const int kWidth = 46;

        private const int kHeight = 46;

        private const int kColCount = 8;


        public void Create(UIHintsPanel panel)
        {
            this.panel = panel;
            button.onClick += OnClick;
        }
        

        public void Free()
        {
            button.onClick -= OnClick;
            currentHint = default;
            button.tips.itemId = 0;
            button.data = 0;
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            countText.text = "";
        }
        
        public void Open()
        {
            gameObject.SetActive(true);
        }


        public void Close()
        {
            button.tips.itemId = 0;
            button.data = 0;
            currentHint = default;
            gameObject.SetActive(false);
        }


        public int SetDisplay(int newIndex, HintData newHint)
        {
            position = newIndex;
            if (!Equals(currentHint, newHint))
            {
                currentHint = newHint;
                button.data = currentHint.signalId;
                Sprite sprite = LDB.signals.IconSprite(currentHint.signalId);

                if (sprite != null)
                {
                    button.tips.itemId = currentHint.signalId;
                    iconImage.sprite = sprite;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                }

                countText.text = newHint.value != 0 ? newHint.value.ToString() : "";
            }

            return (newIndex / 8 + 1) * 46;
        }


        private void OnClick(int obj)
        {
            VFAudio.Create("ui-click-0", null, Vector3.zero, true, 1);

            Vector2 pos =  new Vector2(-300, 238);

            UISignalPicker.Close();
            UINumberPickerExtension.Popup(pos, (signalId, value) =>
            {
                if (value < 0) value = 0;
                
                panel.ChangeHint(currentHint.signalId, new HintData(signalId, value));
            }, currentHint.value, currentHint.signalId);
            
        }
    }
}