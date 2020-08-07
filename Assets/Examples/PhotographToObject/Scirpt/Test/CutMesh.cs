using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OneTriangleData{
    //public bool isNormal;// 该三角是否为法线边
    // 一个三角形数据类
    public Vector3 vertP1;      //三角形的顶点位置 建议自生坐标系
    public Vector3 vertP2;
    public Vector3 vertP3;
    public int index1;          // 该顶点 位于旧集合列表中的索引 或者 切割顶点集合列表索引
    public int index2;
    public int index3;

}
/// <summary>
/// 自己的Cut mesh 工具类。 没有完成 切面的 缝合。因为：切面缝合的算法 面对 凹面体 并不适用。普通的 物体可以。
/// </summary>
public class CutMesh : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform r_cutPlan;         //剪切平面：以XZ平面为切面，Y为切割法线。
    public GameObject targetObj;        //目标物体
    public GameObject beCutObj;         //切割后，另外一个物体

    private Mesh targetMesh;    // 目标网格
    private Mesh cutMesh;       // 切割后 物体网格
    private int[] targetTriangles;      //就网格三角集合临时存储。
    
    private OneTriangleData triangleDataBuffe = new OneTriangleData();
    private List<Vector3> tempVert1=new List<Vector3>();     //临时顶点 集合 —— 切割法线一边
	private List<Vector3> tempNormal1=new List<Vector3>();   // 临时法线 集合
	private  List<int> triangles1=new List<int>();       // 临时三角面 集合

	private List<Vector3> tempVert2=new List<Vector3>();     //临时顶点 集合2
	private List<Vector3> tempNormal2=new List<Vector3>();   //……
    private List<int> triangles2=new List<int>();        //……

    private Dictionary<int,int> pointIndex1 = new Dictionary<int, int> ();   //旧顶点关闭列表1 —— 法线边
	private  Dictionary<int,int> pointIndex2 = new Dictionary<int, int> ();       //…… k-v ：旧顶点索引-临时列表索引

    private Dictionary<int,int> otherPointIndex1 = new Dictionary<int, int> ();   //切割点集合关闭列表1 —— 法线边
	private  Dictionary<int,int> otherPointIndex2 = new Dictionary<int, int> ();       //……k-v ：切割顶点索引-临时列表索引
	static List<Vector3> allPoint = new List<Vector3> ();   //切割点永久存放：算出来的一个局部坐标系点。 每次切割clear
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            GetMesh();
            MainCutMesh();
            targetMesh = new Mesh();
            cutMesh = new Mesh();

            targetMesh.vertices = tempVert2.ToArray();
            targetMesh.normals = tempNormal2.ToArray();
            targetMesh.triangles = triangles2.ToArray();

            cutMesh.vertices = tempVert1.ToArray();
            cutMesh.normals = tempNormal1.ToArray();
            cutMesh.triangles = triangles1.ToArray();
            targetObj.GetComponent<MeshFilter>().mesh = targetMesh;
            beCutObj.GetComponent <MeshFilter>().mesh = cutMesh;
        }
    }

    private void MainCutMesh(){
        tempVert1.Clear ();
		tempNormal1.Clear ();
		triangles1.Clear ();

		tempVert2.Clear ();
		tempNormal2.Clear ();
		triangles2.Clear ();

		pointIndex1.Clear ();
		pointIndex2.Clear ();

        otherPointIndex1.Clear();
        otherPointIndex2.Clear();

		allPoint.Clear ();

        Cut();
    }

    /// <summary>
    /// 获取要切割的网格。
    /// </summary>
    private void GetMesh(){
        targetMesh = targetObj.GetComponent<MeshFilter>().mesh;
    }
    /// <summary>
    /// 切割网格
    /// </summary>
    private void Cut(){
        targetTriangles = targetMesh.triangles;
        //遍历所有三角形
        for(int i=0;i<targetTriangles.Length;i+=3){

            int index1 = targetTriangles [i], index2 = targetTriangles [i + 1], index3 = targetTriangles [i + 2];
            Vector3 vertP1 = targetMesh.vertices [index1];
            Vector3 vertP2 = targetMesh.vertices [index2];
            Vector3 vertP3 = targetMesh.vertices [index3];
            float vert1 = Vector3.Dot (r_cutPlan.up, (targetObj.transform.TransformPoint(targetMesh.vertices [index1]) - r_cutPlan.transform.position));
			float vert2 = Vector3.Dot (r_cutPlan.up, (targetObj.transform.TransformPoint(targetMesh.vertices [index2]) - r_cutPlan.transform.position));
			float vert3 = Vector3.Dot (r_cutPlan.up, (targetObj.transform.TransformPoint(targetMesh.vertices [index3]) - r_cutPlan.transform.position));
            if (vert1 >= 0 && vert2 >= 0 && vert3 >= 0) {
				AddTriangToTempList (index1, index2, index3,  tempVert1,  tempNormal1,  triangles1, pointIndex1);
			} else if (vert1 <= 0 && vert2 <= 0 && vert3 <= 0) {
				AddTriangToTempList (index1, index2, index3,  tempVert2,  tempNormal2,  triangles2, pointIndex2);
			} else {
                // 切割三角面情况
                triangleDataBuffe.vertP1 = vertP1;
                triangleDataBuffe.vertP2 = vertP2;
                triangleDataBuffe.vertP3 = vertP3;
                triangleDataBuffe.index1 = index1;
                triangleDataBuffe.index2 = index2;
                triangleDataBuffe.index3 = index3;

                if(vert1*vert2*vert3==0){
                    //切到顶点…… 这种情况 忽略。
                    CutInVert(vert1,vert2,vert3,triangleDataBuffe);
                }else{
                    //未切到顶点
                    CutNoVert(vert1,vert2,vert3,triangleDataBuffe);
                }

            }

        }
    }

    //切割到顶点时：
    private void CutInVert(float vert1,float vert2,float vert3,OneTriangleData buffe){
        Vector3 cutPos ;
        //1 表示法线边
        int cutIndex,leftIndex1,leftIndex2,rightIndex1,rightIndex2;
        if(vert1 ==0 ){         //切割到顶点1
            cutPos = CutLine(buffe.vertP2,buffe.vertP3);
  
            if(vert2>0){
                leftIndex1=buffe.index1;
                rightIndex1=buffe.index2;
                leftIndex2=buffe.index3;
                rightIndex2=buffe.index1;
            }else{
                leftIndex2=buffe.index1;
                rightIndex2=buffe.index2;
                leftIndex1=buffe.index3;
                rightIndex1=buffe.index1;
            }
        }else if(vert2 == 0){       //切割到顶点2
            cutPos = CutLine(buffe.vertP1,buffe.vertP3);
            
            if(vert1>0){
                leftIndex1=buffe.index1;
                rightIndex1=buffe.index2;
                leftIndex2=buffe.index2;
                rightIndex2=buffe.index3;
            }else{
                leftIndex2=buffe.index1;
                rightIndex2=buffe.index2;
                leftIndex1=buffe.index2;
                rightIndex1=buffe.index3;
            }

        }else{                      ////切割到顶点3
            cutPos = CutLine(buffe.vertP2,buffe.vertP1);
            
            if(vert1>0){
                leftIndex1=buffe.index3;
                rightIndex1=buffe.index1;
                leftIndex2=buffe.index2;
                rightIndex2=buffe.index3;
            }else{
                leftIndex2=buffe.index3;
                rightIndex2=buffe.index1;
                leftIndex1=buffe.index2;
                rightIndex1=buffe.index3;
            }
        }
        allPoint.Add(cutPos);
        cutIndex = allPoint.Count-1;
        AddTriangToTempList2(leftIndex1,rightIndex1,cutIndex,tempVert1,tempNormal1,triangles1,pointIndex1,otherPointIndex1);
        AddTriangToTempList2(leftIndex2,rightIndex2,cutIndex,tempVert2,tempNormal2,triangles2,pointIndex2,otherPointIndex2);
    }

    //切割 之 未切割到顶点

    private void CutNoVert(float vert1,float vert2,float vert3 ,OneTriangleData buffe){
        bool isSingleVertOnCutNormal;       //单顶点，是否在切割法线边
        Vector3 leftCutPos,rightCutPos;
        int leftCutIndex,rightCutIndex;
        int singleVertIndex,leftVertIndex,rightVertIndex;
        // 四边形切：从 单顶点的左顶点 切。
        if(vert2*vert3>0){              //23共边，1独
            leftCutPos = CutLine(buffe.vertP1,buffe.vertP2);
            rightCutPos = CutLine(buffe.vertP1,buffe.vertP3);
            singleVertIndex = buffe.index1;
            leftVertIndex = buffe.index2;
            rightVertIndex = buffe.index3;

            if(vert1>0){
                isSingleVertOnCutNormal = true;
            }else{
                isSingleVertOnCutNormal = false;
            }
        }else if(vert1*vert3>0){        //13共 ，2独
            leftCutPos = CutLine(buffe.vertP2,buffe.vertP3);
            rightCutPos = CutLine(buffe.vertP2,buffe.vertP1);
            singleVertIndex = buffe.index2;
            leftVertIndex = buffe.index3;
            rightVertIndex = buffe.index1;

            if(vert2>0){
                isSingleVertOnCutNormal = true;
            }else{
                isSingleVertOnCutNormal = false;
            }
        }else{                          //12g共，3独
            leftCutPos = CutLine(buffe.vertP3,buffe.vertP1);
            rightCutPos = CutLine(buffe.vertP3,buffe.vertP2);
            singleVertIndex = buffe.index3;
            leftVertIndex = buffe.index1;
            rightVertIndex = buffe.index2;

            if(vert3>0){
                isSingleVertOnCutNormal = true;
            }else{
                isSingleVertOnCutNormal = false;
            }
        }

        allPoint.Add(leftCutPos);
        leftCutIndex = allPoint.Count-1;
        allPoint.Add(rightCutPos);
        rightCutIndex = allPoint.Count-1;

        if(isSingleVertOnCutNormal){
            AddTriangToTempList1(singleVertIndex,leftCutIndex,rightCutIndex,tempVert1,tempNormal1,triangles1,pointIndex1,otherPointIndex1);
            AddTriangToTempList1(leftVertIndex,rightCutIndex,leftCutIndex,tempVert2,tempNormal2,triangles2,pointIndex2,otherPointIndex2);
            AddTriangToTempList2(leftVertIndex,rightVertIndex,rightCutIndex,tempVert2,tempNormal2,triangles2,pointIndex2,otherPointIndex2);
        }else{
            AddTriangToTempList1(singleVertIndex,leftCutIndex,rightCutIndex,tempVert2,tempNormal2,triangles2,pointIndex2,otherPointIndex2);
             AddTriangToTempList1(leftVertIndex,rightCutIndex,leftCutIndex,tempVert1,tempNormal1,triangles1,pointIndex1,otherPointIndex1);
            AddTriangToTempList2(leftVertIndex,rightVertIndex,rightCutIndex,tempVert1,tempNormal1,triangles1,pointIndex1,otherPointIndex1);
        }

    }
    // 一个旧顶点索引输入 旧顶点列表索引、切割顶点集合索引 左右。
    private void AddTriangToTempList1(int index,int indexCutVertLeft,int indexCutVertRight,List<Vector3> tempVert, List<Vector3> tempNormal, List<int> triangles, Dictionary<int,int> pointIndex,Dictionary<int,int> otherPointIndex){
        if(!pointIndex.ContainsKey(index)){
            tempVert.Add (targetMesh.vertices [index]);	//不存在索引时，插入对应索引得值。——1
			tempNormal.Add (targetMesh.normals [index]);
			pointIndex.Add (index,tempVert.Count-1);		//这里是关键
        }

        if(!otherPointIndex.ContainsKey(indexCutVertLeft)){
            tempVert.Add(allPoint[indexCutVertLeft]);
            tempNormal.Add (targetMesh.normals [index]);
            otherPointIndex.Add(indexCutVertLeft,tempVert.Count-1);
        }

        if(!otherPointIndex.ContainsKey(indexCutVertRight)){
            tempVert.Add(allPoint[indexCutVertRight]);
            tempNormal.Add (targetMesh.normals [index]);
            otherPointIndex.Add(indexCutVertRight,tempVert.Count-1);
        }

		triangles.Add (pointIndex[index]);
        triangles.Add (otherPointIndex[indexCutVertLeft]);					//通过 关闭列表，找到新顶点列表索引。
		triangles.Add (otherPointIndex[indexCutVertRight]);

    }
    //两个就顶点索引输入。剩下的顶点索引来自于 切割点集合。
    private void AddTriangToTempList2(int indexLeft,int indexRight,int indexCutVert,List<Vector3> tempVert, List<Vector3> tempNormal, List<int> triangles, Dictionary<int,int> pointIndex,Dictionary<int,int> otherPointIndex){
        if(!pointIndex.ContainsKey(indexLeft)){
            tempVert.Add (targetMesh.vertices [indexLeft]);	//不存在索引时，插入对应索引得值。——1
			tempNormal.Add (targetMesh.normals [indexLeft]);
			pointIndex.Add (indexLeft,tempVert.Count-1);		//这里是关键
        }

        if(!pointIndex.ContainsKey(indexRight)){
            tempVert.Add (targetMesh.vertices [indexRight]);	//不存在索引时，插入对应索引得值。——1
			tempNormal.Add (targetMesh.normals [indexRight]);
			pointIndex.Add (indexRight,tempVert.Count-1);		//这里是关键
        }

        if(!otherPointIndex.ContainsKey(indexCutVert)){
            tempVert.Add(allPoint[indexCutVert]);
            tempNormal.Add(targetMesh.normals [indexLeft]);
            otherPointIndex.Add(indexCutVert,tempVert.Count-1);
        }

        triangles.Add (otherPointIndex[indexCutVert]);					//通过 关闭列表，找到新顶点列表索引。
		triangles.Add (pointIndex[indexLeft]);
		triangles.Add (pointIndex[indexRight]);

    }

    // 三个旧顶点索引输入
    private void AddTriangToTempList(int index1,int index2,int index3, List<Vector3> tempVert, List<Vector3> tempNormal, List<int> triangles, Dictionary<int,int> pointIndex){
        if (!pointIndex.ContainsKey (index1)) {
			tempVert.Add (targetMesh.vertices [index1]);	//不存在索引时，插入对应索引得值。——1
			tempNormal.Add (targetMesh.normals [index1]);
			pointIndex.Add (index1,tempVert.Count-1);		//这里是关键
		}
		if (!pointIndex.ContainsKey (index2)) {
			tempVert.Add (targetMesh.vertices [index2]);	//不存在索引时，插入对应索引得值。——2
			tempNormal.Add (targetMesh.normals [index2]);
			pointIndex.Add (index2,tempVert.Count-1);

		}
		if (!pointIndex.ContainsKey (index3)) {
			tempVert.Add (targetMesh.vertices [index3]);	//不存在索引时，插入对应索引得值。——3
			tempNormal.Add (targetMesh.normals [index3]);
			pointIndex.Add (index3,tempVert.Count-1);

		}
	
		triangles.Add (pointIndex[index1]);					//通过 关闭列表，找到新顶点列表索引。
		triangles.Add (pointIndex[index2]);
		triangles.Add (pointIndex[index3]);
    }

    // 切割线段 函数； 返回切割点自身坐标系位置：参数 自生坐标系位置。
    private Vector3 CutLine(Vector3 pos1,Vector3 pos2){
        pos1 = targetObj.transform.TransformPoint(pos1);
        pos2 = targetObj.transform.TransformPoint(pos2);
        Vector3 lineDir = pos2-pos1;
        float scele =Vector3.Dot(r_cutPlan.transform.position-pos1,r_cutPlan.up)/Vector3.Dot(lineDir,r_cutPlan.up); 
        Vector3 value_ = pos1+lineDir*scele;
        return targetObj.transform.InverseTransformPoint(value_);
    }
}
