﻿#define DEBUG
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCut : MonoBehaviour {

	static int TempCount= 0;
	static Mesh targetMesh=null;        //目标网格
	static Vector3 hitPos;                 // 碰撞点位置
	static Vector3 cutVertivalDir;      //切割垂线
	static Vector3 camPos;              // 相机位置
	static Vector3 dir;                 //切割方向
	static Vector3 planeNormal;         //切割平面法线
	static Transform hitTarget;         //碰撞目标变换

	public static bool working=false;       // 是否开始

	static List<Vector3> tempVert1=new List<Vector3>();     //临时顶点 集合 —— 切割法线一边
	static List<Vector3> tempNormal1=new List<Vector3>();   // 临时法线 集合
	static List<int> triangles1=new List<int>();       // 临时三角面 集合

	static List<Vector3> tempVert2=new List<Vector3>();     //临时顶点 集合2
	static List<Vector3> tempNormal2=new List<Vector3>();   //……
    static List<int> triangles2=new List<int>();        //……
	static int[] triangles;         // 三角面集合

	static Dictionary<int,int> pointIndex1 = new Dictionary<int, int> ();   //关闭列表1
	static Dictionary<int,int> pointIndex2 = new Dictionary<int, int> ();       //……

	static List<Vector3> localPos=new List<Vector3>();      // 切割点临时存放：算出来的一个局部坐标系点 —— 临时存放列表 每次存两个
	static List<Vector3> allPoint = new List<Vector3> ();   //切割点永久存放：算出来的一个局部坐标系点。 每次切割clear
	static Vector3 fpos;        // 位置
	static bool fbool = false;      //在Cut（） 中第一次出现，应作为一个开关，串联多个函数。

	float scrollSpeed=25.0f;        // 卷速度
	Ray _ray;           
	static RaycastHit _hit;
	static bool colliding=false;        // 是否碰撞
	GameObject temp=null;           // 临时物体
	public Material mat;        //使用GL画线所需材质
	float angle=0;              // 切线旋转角度
	void Start(){
	}

	void Update(){
		if (working) {
			_ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (_ray, out _hit)) {
				colliding = true;
				camPos = Camera.main.transform.position;

				//切割平面由dir,cutVerticleDir两个向量唯一确定，planeNormal由dir，cutVerticleDir叉乘所得
				dir = (_hit.point - camPos).normalized;  // 计算相机到 hit 点的 方向。

				float ang = Vector3.Angle (dir, Vector3.up);
				if (ang == 90.0f)
					cutVertivalDir = Vector3.up;        
				else
					cutVertivalDir = (Vector3.Dot (Vector3.up, dir) * (-dir) + Vector3.up).normalized;      // 求得垂直向量。
				planeNormal = Vector3.Cross (dir, cutVertivalDir).normalized; // 注意左手坐标系

				if (temp != _hit.transform.gameObject)
					angle = 0f;
				temp = _hit.transform.gameObject;
				//使用鼠标滚轮旋转切割平面
				if (Input.GetAxis ("Mouse ScrollWheel") < 0) {
					angle += scrollSpeed * Time.deltaTime;
				} else if (Input.GetAxis ("Mouse ScrollWheel") > 0) {
					angle -= scrollSpeed * Time.deltaTime;
				}
                    //求旋转后的平面
				cutVertivalDir = cutVertivalDir * Mathf.Cos (angle) + planeNormal * Mathf.Sin (angle);
				planeNormal = Vector3.Cross (dir, cutVertivalDir).normalized;

				#if DEBUG
				Debug.DrawRay (_hit.point, cutVertivalDir, Color.green);
				Debug.DrawRay (_hit.point, -dir, Color.red);
				Debug.DrawRay (_hit.point, planeNormal, Color.yellow);
				#endif
				if (Input.GetKeyDown (KeyCode.Mouse0)) {
					CutMesh ();
				}

			} else
				colliding = false;
		}
	}

	//后处理渲染 用于绘制辅助线
	void OnPostRender() {
		if (!working||!colliding)
			return;

		if (!mat) {
			Debug.LogError("Please Assign a material on the inspector");
			return;
		}

		GL.PushMatrix(); 
		mat.SetPass(0);
		GL.Color(Color.yellow);
		GL.Begin(GL.LINES);
		GL.Vertex(_hit.point+cutVertivalDir*2f-dir*1f);
		GL.Vertex(_hit.point-cutVertivalDir*2f-dir*1f);
		GL.Vertex(_hit.point-dir*1f);
		GL.Vertex(_hit.point-planeNormal*0.2f-dir*1f);
		GL.End();
		GL.PopMatrix();

	}

	//无用函数
	 static void CutMesh(Mesh _targetMesh){
		
	}

	 static void CutMesh(){
		if (!GetMesh ())
			return;

//		Debug.DrawRay(hitPos,cutVertivalDir,Color.green,333f);
//		Debug.DrawRay (hitPos,-dir,Color.red,333f);
//		Debug.DrawRay (hitPos, planeNormal, Color.yellow, 333f);

			//使用前，先清空。
		tempVert1.Clear ();
		tempNormal1.Clear ();
		triangles1.Clear ();

		tempVert2.Clear ();
		tempNormal2.Clear ();
		triangles2.Clear ();

		pointIndex1.Clear ();
		pointIndex2.Clear ();

		allPoint.Clear ();

		Cut ();
		//补全截面
		GenerateSection ();

		Mesh originMesh=new Mesh(),newMesh=new Mesh();

		originMesh.vertices = tempVert1.ToArray ();
		originMesh.normals = tempNormal1.ToArray ();
		originMesh.triangles = triangles1.ToArray ();
		hitTarget.GetComponent<MeshFilter> ().mesh = originMesh;


		newMesh.vertices = tempVert2.ToArray ();
		newMesh.normals = tempNormal2.ToArray ();
		newMesh.triangles = triangles2.ToArray ();
		GameObject newObj = new GameObject ();
		newObj.transform.position = hitTarget.position;
		newObj.transform.rotation = hitTarget.rotation;
		//BoxCollider collider = newObj.AddComponent<BoxCollider> ();
		//collider.center = newMesh.bounds.center;
		//collider.size = newMesh.bounds.size;
		newObj.AddComponent<MeshFilter> ().mesh = newMesh;
		newObj.AddComponent<MeshRenderer> ();
		Material material = hitTarget.GetComponent<MeshRenderer> ().material;
		newObj.GetComponent<MeshRenderer> ().material = material;
		newObj.AddComponent<Rigidbody> ();

		Destroy (newObj,5f);

	}

// 切割 计算出 两个 临时顶点、法线、三角集合数值。
	 static void Cut(){
		 TempCount++;
		triangles = targetMesh.triangles;
		//遍历网格的每个三角面
		for (int i = 0; i < triangles.Length; i += 3) {
			
			int index1 = triangles [i], index2 = triangles [i + 1], index3 = triangles [i + 2];

				//获取三角面 三个点转换“世坐”后求“差向”，再和切割面 法向量 点乘，若小0，在法向量边。—— 点所在区域。
			float vert1 = Vector3.Dot (planeNormal, (hitTarget.TransformPoint(targetMesh.vertices [index1]) - hitPos));
			float vert2 = Vector3.Dot (planeNormal, (hitTarget.TransformPoint(targetMesh.vertices [index2]) - hitPos));
			float vert3 = Vector3.Dot (planeNormal, (hitTarget.TransformPoint(targetMesh.vertices [index3]) - hitPos));
		
			//判断分别：三个点在左（左手坐标系）、三点都在右、其他
			if (vert1 >= 0 && vert2 >= 0 && vert3 >= 0) {
				CopyVert (index1, index2, index3, ref tempVert1, ref tempNormal1, ref triangles1,ref pointIndex1);
			} else if (vert1 <= 0 && vert2 <= 0 && vert3 <= 0) {
				CopyVert (index1, index2, index3, ref tempVert2, ref tempNormal2, ref triangles2,ref pointIndex2);
			} else {

				// 三角面被切割的情况……
				localPos.Clear();
				fbool = false;
				//
				// 注意 ！ 号 ： 因此是  不共边时 切割。
				if (!((vert1 >0 && vert2 >0) || (vert1 < 0 && vert2 < 0))) {
					GetIntersection (index1, index2, vert1, vert2);
				}
			
				if (!((vert2 >0 && vert3 > 0) || (vert2 < 0 && vert3 < 0))) {

					GetIntersection (index2, index3, vert2, vert3);
				}
					
				if (!((vert3 > 0 && vert1 > 0) || (vert3 < 0 && vert1 < 0))) {
				
					GetIntersection (index3, index1, vert3, vert1);
				}
				Debug.DrawLine (hitTarget.TransformPoint(localPos [0]),hitTarget.TransformPoint(localPos [1]),Color.red,2f);

					// 做判断 共边用 AddVert 剩余一点 用AddVert2
				if (vert1 >= 0) {
					if (vert2 >= 0) {
						//1 2 左 3 右
						AddVert (index1, index2,ref tempVert1,ref tempNormal1,ref triangles1,ref pointIndex1);
						AddVert2 (index3,ref tempVert2,ref tempNormal2,ref triangles2,ref pointIndex2,false);
					} else {
						if(vert3>=0){
							//1 3 左 2 右
							AddVert (index3, index1,ref tempVert1,ref tempNormal1,ref triangles1,ref pointIndex1);
							AddVert2 (index2,ref tempVert2,ref tempNormal2,ref triangles2,ref pointIndex2,false);
						}else{
							//…… 傻掉情况多
							AddVert2 (index1,ref tempVert1,ref tempNormal1,ref triangles1,ref pointIndex1,true);
							AddVert (index2,index3,ref tempVert2,ref tempNormal2,ref triangles2,ref pointIndex2,false);
						}
					}
				} else {
					if(vert2>=0){
						if(vert3>=0){
							AddVert (index2, index3,ref tempVert1,ref tempNormal1,ref triangles1,ref pointIndex1);
							AddVert2 (index1,ref tempVert2,ref tempNormal2,ref triangles2,ref pointIndex2,true);
						}else{
							AddVert2 (index2,ref tempVert1,ref tempNormal1,ref triangles1,ref pointIndex1,false);
							AddVert (index3, index1, ref tempVert2, ref tempNormal2, ref triangles2,ref pointIndex2);
						}
					}else{
						AddVert2 (index3,ref tempVert1,ref tempNormal1,ref triangles1,ref pointIndex1,false);
						AddVert (index1, index2, ref tempVert2, ref tempNormal2, ref triangles2,ref pointIndex2);
					}
				}
			}
		}
		
	}

//补全切面
// 漏洞：只是和图面体，
	static void GenerateSection(){
		Debug.Log ("all point count:"+allPoint.Count);
		Vector3 center=0.5f*(allPoint[0]+allPoint[allPoint.Count/2]);
		Vector3 normal = hitTarget.InverseTransformDirection (planeNormal);

		tempVert1.Add (center);
		tempNormal1.Add (-normal);

		tempVert2.Add (center);
		tempNormal2.Add (normal);


		for (int i = 0; i < allPoint.Count; i+=2) {

			tempVert1.Add (allPoint[i]);
			tempVert1.Add (allPoint[i+1]);
			tempNormal1.Add (-normal);
			tempNormal1.Add (-normal);

			Vector3 vector_1 = allPoint [i] - center;
			Vector3 vector_2 = allPoint [i + 1] - center;
			Vector3 cross_vector = Vector3.Cross (vector_1, vector_2);
				//判断三角法线
			if (Vector3.Dot(normal,cross_vector)<0) {
				triangles1.Add (tempVert1.LastIndexOf (center));
				triangles1.Add (tempVert1.Count-2);
				triangles1.Add (tempVert1.Count-1);
			} else {
				triangles1.Add (tempVert1.LastIndexOf (center));
				triangles1.Add (tempVert1.Count-1);
				triangles1.Add (tempVert1.Count-2);
			}

			tempVert2.Add (allPoint[i]);
			tempVert2.Add (allPoint[i+1]);
			tempNormal2.Add (normal);
			tempNormal2.Add (normal);
				// 前面已经判断了三角法线，那么这次就不用判断三角法线了。
			if (Vector3.Dot(normal,cross_vector)>0) {
				triangles2.Add (tempVert2.LastIndexOf (center));
				triangles2.Add (tempVert2.Count-2);
				triangles2.Add (tempVert2.Count-1);
			} else {
				triangles2.Add (tempVert2.LastIndexOf (center));
				triangles2.Add (tempVert2.Count-1);
				triangles2.Add (tempVert2.Count-2);
			}
		}
	
	}


//拷贝顶点 完整的 三角面 三个顶点传入。
/// <summary>
/// 三角面、顶点、法线拷贝
/// </summary>
/// <param name="index1">旧顶点列表索引1</param>
/// <param name="index2">旧顶点列表索引2</param>
/// <param name="index3">旧顶点列表索引3</param>
/// <param name="tempVert">临时（新）顶点列表</param>
/// <param name="tempNormal">新法线列表</param>
/// <param name="triangles">新三角列表</param>
/// <param name="pointIndex">关闭列表</param>
	static void CopyVert(int index1,int index2,int index3,ref List<Vector3> tempVert,ref List<Vector3> tempNormal,ref List<int> triangles,ref Dictionary<int,int> pointIndex){

			//是顶点索引否存在 ： 字典作为 一个 旧顶点 索引 的关闭列表。(键值对：旧顶点列表索引 —— 新顶点列表索引)
			//用于判断点是否拷贝过
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
		

//获取切割点。这里求出一个 自身坐标系下 表示的未知 点。 并存入了 localPos allPoint之中。 
// 调用只在 三角面被 切割时，两边点传入。 ————猜测求的就是 切割点。
	static void GetIntersection(int index1,int index2,float vert1,float  vert2){
		Vector3 p;
		Vector3 lineDir;

		if (vert1 > 0 || vert1 < 0) {
			p = targetMesh.vertices [index1];
			lineDir = targetMesh.vertices [index2] - targetMesh.vertices [index1];
		} else if (vert2 > 0 || vert2 < 0) {
			p = targetMesh.vertices [index2];
			lineDir = targetMesh.vertices [index1] - targetMesh.vertices [index2];
		} else {
			// 点连线平行于 切割法线
			p = Vector3.zero;
			lineDir = targetMesh.vertices [index2] - targetMesh.vertices [index1];
		}
			//现在不迷了， 手动确定一个 “第一个”点。
			//三点中必有一个点在 法线边。 fpos 确定 第一个切点是在 哪个边上。
		if (vert1 > 0&&!fbool) {
			fpos = targetMesh.vertices [index1];
			fbool = true;
		} else if(vert2 > 0&&!fbool){
			fpos = targetMesh.vertices [index2];
			fbool = true;
		}
		// 这里求出一个 自身坐标系下 表示的未知 点。 并存入了 localPos allPoint之中。

		Vector3 intersection;
		intersection=hitTarget.TransformPoint(p)+hitTarget.TransformDirection(lineDir).normalized*((Vector3.Dot(hitPos,planeNormal)-Vector3.Dot(hitTarget.TransformPoint(p),planeNormal))/Vector3.Dot(hitTarget.TransformDirection(lineDir).normalized,planeNormal));

		Vector3 localIntersection = hitTarget.InverseTransformPoint (intersection);
		localPos.Add (localIntersection);
		allPoint.Add (localIntersection);
	}

// 切割三角面， 顶点转移 —— 一个顶点一边 雷同于CopyVert（）
	static void AddVert2(int index,ref List<Vector3> tempVert,ref List<Vector3> tempNormal,ref List<int> triangles,ref Dictionary<int,int> pointIndex,bool b){

		if (!pointIndex.ContainsKey (index)) {
			tempVert.Add (targetMesh.vertices [index]);
			tempNormal.Add (targetMesh.normals[index]);
			pointIndex.Add (index, tempVert.Count - 1);

			}


		tempVert.Add (localPos[0]);
		tempVert.Add (localPos[1]);

		tempNormal.Add (targetMesh.normals [index]);
		tempNormal.Add (targetMesh.normals [index]);

		triangles.Add (pointIndex[index]);

		if (b) {
			triangles.Add (tempVert.Count - 2);
			triangles.Add (tempVert.Count - 1);
		} else {
			triangles.Add (tempVert.Count - 1);
			triangles.Add (tempVert.Count - 2);
		}
	}

// 切割三角面 顶点转移 —— 两个顶点一边 —— 雷同于CopyVert（）
	public static void AddVert(int index1,int index2,ref List<Vector3> tempVert,ref List<Vector3> tempNormal,ref List<int> triangles,ref Dictionary<int,int> pointIndex,bool b=true){
		
		if (!pointIndex.ContainsKey (index1)) {
			tempVert.Add (targetMesh.vertices [index1]);
			tempNormal.Add (targetMesh.normals[index1]);
			pointIndex.Add (index1,tempVert.Count-1);
		}
		if (!pointIndex.ContainsKey (index2)) {
			tempVert.Add (targetMesh.vertices [index2]);
			tempNormal.Add (targetMesh.normals[index2]);
			pointIndex.Add (index2,tempVert.Count-1);
		}
		tempVert.Add (localPos[0]);
		tempVert.Add (localPos[1]);

		tempNormal.Add (targetMesh.normals [index1]);
		tempNormal.Add (targetMesh.normals [index2]);

		triangles.Add (pointIndex[index1]);
		triangles.Add (pointIndex[index2]);
		triangles.Add (tempVert.Count-2); //添加了两个点，取第一个点的索引

			// 这里 本函数 拥有四个点，需要形成两个三角面。
			// 如何不交叉 是关键。  这里的做法有点 垃圾了。
		if (fpos == targetMesh.vertices [index1]) {
			triangles.Add (pointIndex [index2]);
			triangles.Add (tempVert.Count - 1);
			triangles.Add (tempVert.Count - 2);
      
		} else if (fpos == targetMesh.vertices [index2]) {
			triangles.Add (pointIndex [index1]);
			triangles.Add (tempVert.Count - 2);
			triangles.Add (tempVert.Count - 1);

		} else {
			if (!b) {
				triangles.Add (pointIndex [index2]);
				triangles.Add (tempVert.Count - 1);
				triangles.Add (tempVert.Count - 2);
			} else {
				triangles.Add (pointIndex [index1]);
				triangles.Add (tempVert.Count - 2);
				triangles.Add (tempVert.Count - 1);
			}
		}

	}

// 有个bug ：如果物体有Colliding 无Mesh……
/// <summary>
/// bool 返回是否检测到物体，并将物体的 mesh、 hit 位置、hit Trans 赋值。
/// </summary>
/// <returns></returns>
	static bool GetMesh(){
		if (!colliding)
			return false;
		targetMesh = _hit.transform.GetComponent<MeshFilter> ().mesh;

		hitPos = _hit.point;

		hitTarget = _hit.transform;
		return true;
	}
}