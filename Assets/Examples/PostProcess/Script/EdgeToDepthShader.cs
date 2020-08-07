using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeToDepthShader : PostProcessScript
{
    public Shader edgeByDepthShader;
    public  Material edgeDepthMaterial;

    [Range (0.0f,1.0f)]
    public float edgesOnly = 0.0f;                                  //边缘强度 _ 只剩下边。
    public Color edgecolor = Color.black;                       // 边颜色
    public Color backgroundColor = Color.white;             // 背景颜色 用于只看描边时 观察用。
    public float sampleDistance = 1.0f;                             //边的粗细
    public float sensitivityDepth = 1.0f;                           // 深度 敏感度 ，用于倍数 深度结果。
    public float sensitivityNormals = 1.0f;                            // 法线 敏感度，用于倍数 法线结果。
    public float normalThresholdValue;                              // 法线阈值     ,这里是把法线差值，分量相加来判断。
    public float depthThresholdValue;                                 // 深度阈值，用于区分边和物体。

     void OnEnable()
    {
        // 摄像机输出：深度法线 
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
    }
    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (edgeDepthMaterial != null)
        {
            edgeDepthMaterial.SetFloat("_EdgeOnly", edgesOnly);
            edgeDepthMaterial.SetColor("_EdgeColor", edgecolor);
            edgeDepthMaterial.SetColor("_BackgroundColor", backgroundColor);
            edgeDepthMaterial.SetFloat("_SampleDistance", sampleDistance);
            edgeDepthMaterial.SetVector("_Sensitivity", new Vector4(sensitivityNormals, sensitivityDepth, normalThresholdValue, depthThresholdValue));
            Graphics.Blit(source, destination, edgeDepthMaterial);
        }else
        {
            Graphics.Blit(source, destination);
        }
    }
}
