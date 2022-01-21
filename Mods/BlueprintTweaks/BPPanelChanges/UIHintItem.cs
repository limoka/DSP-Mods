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

        private int signalId;

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
            signalId = 0;
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
            signalId = 0;
            gameObject.SetActive(false);
        }


        public int SetDisplay(int newIndex, int signalId)
        {
            position = newIndex;
            if (this.signalId != signalId)
            {
                this.signalId = signalId;
                button.data = this.signalId;
                Sprite sprite = LDB.signals.IconSprite(signalId);

                if (sprite != null)
                {
                    button.tips.itemId = signalId;
                    iconImage.sprite = sprite;
                    iconImage.color = Color.white;
                }
                else
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                }
            }

            return (newIndex / 8 + 1) * 46;
        }


        private void OnClick(int obj)
        {
            VFAudio.Create("ui-click-0", null, Vector3.zero, true, 1);

            Vector2 pos =  new Vector2(-300, 238);
            
            UISignalPicker.Close();
            UISignalPicker.Popup(pos, newSignal =>
            {
                panel.ChangeHint(signalId, newSignal);
            });
        }
    }
}