using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
///  可以通过脚本修改法线反向。
/// </summary>
public class MeshExample3 : MonoBehaviour
{
    // Start is called before the first frame update
    
    private Mesh meshV;
    void Start()
    {
        meshV = GetComponent<MeshFilter>().mesh;
        NormalBack(meshV);
    }
    private void NormalBack(Mesh IN){
        Vector3[] vertices = IN.vertices;
        Vector3[] normals = IN.normals;
        int[] Triangles = IN.triangles;
        int buffer;
        for(int i=0;i<Triangles.Length;i+=3){
            buffer = Triangles[i+1];
            Triangles[i+1]=Triangles[i+2];
            Triangles[i+2]=buffer;
        }
            // 这里数组肯定是引用类型，但是其中通过 某种手段，看起来像值类型。因此 存在一个Clear（）.
        for(int i=0;i<vertices.Length;i++){
            vertices[i] *=1.2f;
        }
        IN.triangles = Triangles;
        IN.vertices = vertices;
    }
}
