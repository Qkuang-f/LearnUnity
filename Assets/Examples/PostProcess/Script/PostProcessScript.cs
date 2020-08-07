using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 这个脚本作为后处理的基类
/// </summary>
//脚本只会在编辑器下被 实例化，并执行。
[ExecuteInEditMode]
//该脚本需要 Camera组件。如果GameObject没有会添加。
[RequireComponent (typeof (Camera ))]

public class PostProcessScript : MonoBehaviour
{
    void Start()
    {
        CheckResources();
    }

    /// <summary>
    /// 检测设备是否支持
    /// </summary>
    protected void CheckResources()
    {
        bool isSupported = CheckSupport();
        if (isSupported == false)
        {
            NotSupported();
        }
    }

    /// <summary>
    /// 判断设备是否支持 后处理。
    /// </summary>
    /// <returns></returns>
    protected bool CheckSupport()
    {
        if (/*SystemInfo.supportsImageEffects == false || */SystemInfo.supports3DRenderTextures == false)
        {
            Debug.Log("当前设备不支持 后处理！");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 如果不支持时调用此函数，
    /// </summary>
    protected void NotSupported()
    {
        enabled = false;
    }

    protected Material CheckShaderAndCreateMaterial(Shader shader ,Material material)
    {
        if (shader == null)
            return null ;
        if (shader.isSupported && material && material.shader == shader)
            return material;

        if (!shader.isSupported)            // 判断此shader 能否在用户的设备上运行。
        {
            return null;
        }else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            // 不会保存这个对象。DestroyImmediate（）需要用这个函数销毁    //避免 内存泄漏。

            if (material)
            {
                return material;
            }
            else
            {
                return null;
            }
        }

    }

  
}
