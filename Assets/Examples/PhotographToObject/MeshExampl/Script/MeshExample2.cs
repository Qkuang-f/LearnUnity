using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExample2 : MonoBehaviour
{
    // Start is called before the first frame update
    public float vlaue_;
    void Update() {

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int i = 0;
        while (i < vertices.Length) {
            vertices[i] += (normals[i] * Mathf.Sin(Time.time))*vlaue_;
            i++;
        }

        mesh.vertices = vertices;

    }

}
