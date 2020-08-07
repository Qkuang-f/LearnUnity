using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessSaturationAndContrast : PostProcessScript
{
    public Shader briSatConShader;
    private Material briSatConMaterial;
    public  Material material
    {
        get
        {
            briSatConMaterial = CheckShaderAndCreateMaterial(briSatConShader, briSatConMaterial);
            return briSatConMaterial;
        }
    }
    [Range (0f,3.0f)]
    public float brightness = 1.0f;
    [Range(0f, 3.0f)]

    public float saturation = 1.0f;
    [Range(0f, 3.0f)]

    public float contrast = 1.0f;

    /// <summary>
    /// 手动卸载资源，避免内存泄漏。
    /// </summary>
    private void OnDisable()
    {
        DestroyImmediate(briSatConMaterial);
        print("销毁材质");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            material.SetFloat("_Brightnees", brightness);
            material.SetFloat("_Saturation", saturation);
            material.SetFloat("_Contarast", contrast);

            Graphics.Blit(source, destination, material);

        }else
        {
            //如果没有材质，就原样输出。
            Graphics.Blit(source, destination);

        }
    }


}
