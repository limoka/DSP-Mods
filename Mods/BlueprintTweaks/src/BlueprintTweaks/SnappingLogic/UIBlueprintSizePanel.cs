using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintTweaks
{
    public class UIBlueprintSizePanel : UIBlueprintPanel
    {
        public InputField longField;
        public InputField latField;

        public Transform anchorTrans;
        private List<UIButton> anchorButtons;

        private bool inited;
        

        public override int verticalSize => 136;

        public override void OnUpdate()
        {
            longField.text = inspector.blueprint.dragBoxSize_x.ToString();
            latField.text = inspector.blueprint.dragBoxSize_y.ToString();
            if (anchorButtons != null)
            {
                foreach (UIButton button in anchorButtons)
                {
                    if (inspector.blueprint.areas.Length > 1)
                    {
                        button.button.interactable = button.data < 3;
                    }
                    else if (inspector.blueprint.areas[0].height == 1)
                    {
                        button.button.interactable = (button.data > 5 && button.data < 15) || button.data == 2;
                    }else if (inspector.blueprint.areas[0].width == 1)
                    {
                        button.button.interactable = button.data > 15 || button.data == 2;
                    }
                    else
                    {
                        button.button.interactable = button.data < 5;
                    }

                    if (button.button.interactable)
                    {
                        int actualData = GetCorrectAnchorType(button.data);
                        button.highlighted = inspector.blueprint.anchorType == actualData;
                    }
                    else
                    {
                        button.highlighted = false;
                    }
                    
                }
            }
        }

        private static int GetCorrectAnchorType(int value)
        {
            if (value > 15)
            {
                value -= 20;
            }else if (value > 5)
            {
                value -= 10;
            }

            return value;
        }

        private void OnEnable()
        {
            if (inited) return;

            anchorButtons = new List<UIButton>(anchorTrans.childCount);
            foreach (Transform trans in anchorTrans)
            {
                UIButton button = trans.GetComponent<UIButton>();
                button.onClick += SetAnchor;
                anchorButtons.Add(button);
            }

            longField.textComponent.gameObject.SetActive(false);
            latField.textComponent.gameObject.SetActive(false);
            Invoke(nameof(FixInvisible), 0.2f);
        }

        private void FixInvisible()
        {
            longField.textComponent.gameObject.SetActive(true);
            latField.textComponent.gameObject.SetActive(true);
            inited = true;
        }

        public void SetLongtitude(string value)
        {
            int longSize = int.Parse(value);
            inspector.blueprint.dragBoxSize_x = longSize;
        }
        
        public void SetLatitude(string value)
        {
            int latSize = int.Parse(value);
            inspector.blueprint.dragBoxSize_y = latSize;
        }

        public void SetAnchor(int type)
        {
            type = GetCorrectAnchorType(type);
            inspector.blueprint.anchorType = type;

            if (inspector.usage == UIBlueprintInspector.EUsage.Paste)
            {
                inspector.pasteBuildTool.anchorType = type;
                inspector.pasteBuildTool.ResetStates();
            }

            inspector.Refresh(true, true, true);
        }
    }
}