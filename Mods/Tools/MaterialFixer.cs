using System;
using kremnev8;
using UnityEngine;
using UnityEngine.UI;

namespace SignalNetwork
{
    public class MaterialFixer : MonoBehaviour
    {
        private static readonly int ColorGlass = Shader.PropertyToID("_ColorGlass");
        private static readonly int Vibrancy = Shader.PropertyToID("_Vibrancy");
        private static readonly int Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int Flatten = Shader.PropertyToID("_Flatten");
        
        private void Start()
        {
            if (!Application.isEditor)
            {
                Material trsmat = Registry.CreateMaterial("UI/TranslucentImage", "trs-mat", "#00000000", null,
                    new[] {"_EMISSION"});

                trsmat.SetFloat(ColorGlass, 1f);
                trsmat.SetFloat(Vibrancy, 1.1f);
                trsmat.SetFloat(Brightness, -0.5f);
                trsmat.SetFloat(Flatten, 0.005f);

                TranslucentImage[] TranslucentImages = GetComponentsInChildren<TranslucentImage>(true);
                foreach (TranslucentImage image in TranslucentImages)
                {
                    image.material = trsmat;
                    image.vibrancy = 1.1f;
                    image.brightness = -0.5f;
                    image.flatten = 0.005f;
                    image.spriteBlending = 0.7f;
                }
            }
        }
    }
}