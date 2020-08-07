using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Material mat;
    public Vector3[] vectList;
    // // 相机的图片效果渲染 函数。用于后处理—— 需要挂在相机上才能调用
    // private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        
    // }

    //相机的 渲染函数，绘制一些图形渲染（由深度测试）—— 需要挂在相机上才能调用
    private void OnPostRender() {
        if (!mat) {
			Debug.LogError("Please Assign a material on the inspector");
			return;
		}
		GL.PushMatrix(); //将 相机使用的 矩阵保存下来
        //GL.LoadOrtho();   //加载其他种类相机矩阵：正交投影矩阵、透视投影矩阵（需要自己构建矩阵值）……其他种类
        // Ortho 矩阵，是归一化矩阵正交矩阵。右上角：（1，1）.
       //不写矩阵加载，使用 和相机当前设置的 “正交、透视” 有关。
		mat.SetPass(0);     // 设置本次GL 绘制的passs 通道。由于不能设置法线，因此注意使用 非光照 Shader 材质。
		//GL.Color(Color.yellow);     // 这里设置GL 颜色，但是没看到作用。
		GL.Begin(GL.TRIANGLES);     //GL 绘制的图形：三角、四边、线段……

        // 设置顶点。 注意：三角面 的三角法线 遵循左手坐标系。
		foreach (var item in vectList)
        {
            GL.Vertex(item);
        }
		GL.End();                  //GL 图形绘制 结束，可以进行下个GL 图形绘制。
		GL.PopMatrix();//将 相机使用的 矩阵 从保存中恢复

    }
    
}
