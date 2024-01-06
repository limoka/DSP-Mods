using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIBlueprintRecipeItem : MonoBehaviour
    {
        public int position
        {
            get => Mathf.RoundToInt(rectTrans.anchoredPosition.x / 46f) + Mathf.RoundToInt(-rectTrans.anchoredPosition.y / 46f) * 8;
            set => rectTrans.anchoredPosition = new Vector2(value % 8 * 46, -(value / 8 * 46));
        }
        
        [HideInInspector]
        public UIRecipesPanel panel;

        public RectTransform rectTrans;

        public UIButton button;

        public Image iconImage;

        private int recipeId;

        private ERecipeType recipeFilter;

        private const int kMargin = 0;

        private const int kWidth = 46;

        private const int kHeight = 46;

        private const int kColCount = 8;


        public void Create(UIRecipesPanel panel)
        {
            this.panel = panel;
            button.onClick += OnClick;
        }
        

        public void Free()
        {
            button.onClick -= OnClick;
            recipeId = 0;
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
            recipeId = 0;
            gameObject.SetActive(false);
        }


        public int SetDisplay(int newIndex, int recipeID)
        {
            position = newIndex;
            if (recipeId != recipeID)
            {
                recipeId = recipeID;
                button.data = recipeId;
                RecipeProto proto = LDB.recipes.Select(recipeID);
                
                button.tips.itemId = 0;
                button.tips.tipTitle = "";
                button.tips.tipText = "";
                
                if (proto != null)
                {
                    button.tips.itemId = proto.Results[0];

                    recipeFilter = proto.Type;
                    iconImage.sprite = proto.iconSprite;
                    iconImage.color = Color.white;
                }
                else if (recipeID < 0)
                {
                    recipeFilter = (ERecipeType)(-recipeID);

                    string recipeDesc = $"{recipeFilter}RecipeKind".Translate();
                    button.tips.tipTitle = string.Format("SelectEmptyRecipeTitle".Translate(), recipeDesc);
                    button.tips.tipText = string.Format("SelectEmptyRecipeText".Translate(), recipeDesc);
                    
                    iconImage.sprite = UIRecipesPanel.noRecipeIcon;
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
            
            UIRecipePicker.Close();
            UIRecipePicker.Popup(pos, proto =>
            {
                if (proto != null)
                    panel.ChangeRecipe(recipeId, proto);
            }, recipeFilter);
        }
    }
}