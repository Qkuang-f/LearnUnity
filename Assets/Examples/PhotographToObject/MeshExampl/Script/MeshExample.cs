using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExample : MonoBehaviour
{
    // 必要的 属性：顶点 、三角形索引
    public Vector3[] newVertices;
    public Vector3[] newNormals;
    /// <summary>
    /// 表示要 存在的三角面。其值为 顶点的索引。 从0 开始 每三个索引 表示一个三角面。 三角面的法线
    /// 是 三个点 以 左手手坐标系，叉乘结果方向为准。 每三个值，以 第一个向量 起点。
    /// </summary>
    public int[] newTriangles;
    void Start() {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.triangles = newTriangles;
    }
}
