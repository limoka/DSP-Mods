using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks
{
    public class UIRecipesPanel : MonoBehaviour
    {
        public List<UIBlueprintRecipeItem> recipeItems = new List<UIBlueprintRecipeItem>();
        public List<int> tmpRecipeList = new List<int>();

        public int recipeCount;
        
        [HideInInspector]
        public UIBlueprintInspector inspector;
        public UIBlueprintRecipeItem prefab;
        
        [HideInInspector]
        public RectTransform panelTrs;

        public void Create(UIBlueprintInspector inspector)
        {
            this.inspector = inspector;
            
            panelTrs = (RectTransform) transform;

        }

        public void RefreshUI()
        {
            if (inspector.blueprint == null || !inspector.blueprint.isValid)return;
            
            tmpRecipeList.Clear();
            
            foreach (BlueprintBuilding blueprintBuilding in inspector.blueprint.buildings)
            {
                if (blueprintBuilding.recipeId != 0 && !tmpRecipeList.Contains(blueprintBuilding.recipeId))
                {
                    tmpRecipeList.Add(blueprintBuilding.recipeId);
                }
            }
            int num = 0;
            foreach (int recipeId in tmpRecipeList)
            {
                SetItem(num, recipeId);
                num++;
            }
            
            ClearComponentItems(num);
        }
        
        public void SetItem(int index, int recipeId)
        {
            if (index < 0)
            {
                return ;
            }
            if (index > 256)
            {
                return ;
            }
            while (index >= recipeItems.Count)
            {
                UIBlueprintRecipeItem uiblueprintComponentItem = Instantiate(prefab, prefab.transform.parent);
                uiblueprintComponentItem.Create(this);
                recipeItems.Add(uiblueprintComponentItem);
            }
            recipeItems[index].SetDisplay(index, recipeId);
            recipeItems[index].Open();
        }
        
        public void ClearComponentItems(int activeCount = 0)
        {
            for (int i = activeCount; i < recipeItems.Count; i++)
            {
                recipeItems[i].Close();
            }

            recipeCount = activeCount;
        }
        
        public void ChangeRecipe(int oldRecipe, RecipeProto newRecipe)
        {
            BlueprintTweaksPlugin.logger.LogInfo($"Changing recipe to {newRecipe.ID}");

            for (int i = 0; i < inspector.blueprint.buildings.Length; i++)
            {
                if (inspector.blueprint.buildings[i].recipeId == oldRecipe)
                {
                    inspector.blueprint.buildings[i].recipeId = newRecipe.ID;
                }
            }

            if (inspector.usage == UIBlueprintInspector.EUsage.Browser || inspector.usage == UIBlueprintInspector.EUsage.Paste)
            {
                if (inspector.usage == UIBlueprintInspector.EUsage.Paste)
                {
                    inspector.pasteBuildTool.ResetStates();
                }
            }
            else if (inspector.usage == UIBlueprintInspector.EUsage.Copy && inspector.copyBuildTool.active)
            {
            }
            inspector.Refresh(true, true, true);
        }
    }
}