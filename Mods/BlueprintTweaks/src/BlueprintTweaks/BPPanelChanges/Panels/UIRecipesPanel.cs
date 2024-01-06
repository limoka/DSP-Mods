using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintTweaks
{
    public class UIRecipesPanel : UIBlueprintPanel
    {
        public List<UIBlueprintRecipeItem> recipeItems = new List<UIBlueprintRecipeItem>();
        public List<int> tmpRecipeList = new List<int>();

        public int recipeCount;

        public UIBlueprintRecipeItem prefab;

        public static Sprite noRecipeIcon;


        private void Awake()
        {
            noRecipeIcon = Resources.Load<Sprite>("ui/textures/sprites/icons/select-recipe");
        }

        public override int verticalSize
        {
            get
            {
                if (recipeCount == 0) return 22;

                return 22 + ((recipeCount - 1) / 8 + 1) * 46;
            }
        }

        public override void OnUpdate()
        {
            tmpRecipeList.Clear();

            foreach (BlueprintBuilding blueprintBuilding in inspector.blueprint.buildings)
            {
                int currentRecipeId = blueprintBuilding.recipeId;
                if (currentRecipeId == 0)
                {
                    ItemProto proto = LDB.items.Select(blueprintBuilding.itemId);
                    if (proto.prefabDesc != null)
                    {
                        currentRecipeId = -(int)proto.prefabDesc.assemblerRecipeType;

                        if (proto.prefabDesc.isLab && 
                            blueprintBuilding.parameters.Length > 0 &&
                            blueprintBuilding.parameters[0] != 2)
                        {
                            currentRecipeId = -(int)ERecipeType.Research;
                        }
                    }
                }

                if (currentRecipeId != 0 && !tmpRecipeList.Contains(currentRecipeId))
                {
                    tmpRecipeList.Add(currentRecipeId);
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
                return;
            }

            if (index > 256)
            {
                return;
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
            var buildings = inspector.blueprint.buildings;

            if (oldRecipe < 0)
            {
                var recipeKind = (ERecipeType)(-oldRecipe);
                foreach (BlueprintBuilding building in buildings)
                {
                    if (building.recipeId != 0) continue;
                    
                    ItemProto proto = LDB.items.Select(building.itemId);
                    if (proto.prefabDesc == null) continue;
                    
                    if (proto.prefabDesc.assemblerRecipeType == recipeKind)
                    {
                        building.recipeId = newRecipe.ID;
                    }else if (proto.prefabDesc.isLab && 
                              recipeKind == ERecipeType.Research && 
                              building.parameters.Length > 0 &&
                              building.parameters[0] != 2)
                    {
                        building.recipeId = newRecipe.ID;
                        building.parameters[0] = 1;
                    }
                }
            }
            else
            {
                foreach (BlueprintBuilding building in buildings)
                {
                    if (building.recipeId == oldRecipe)
                    {
                        building.recipeId = newRecipe.ID;
                    }
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