using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckObjInView : MonoBehaviour
{
    // Start is called before the first frame update
    bool isRendering;
    float currentTime;
    float lastTime;

/// <summary>
/// 当物体被渲染时，调用该函数—— 周期回调 （不论是否为Scene相机）
/// </summary>
     private void OnWillRenderObject() {
        if(Camera.current.tag == "MainCamera"){
            print("主相机渲染");
        }else if(Camera .current .tag == "TwoCamera"){
            print("次相机渲染");
        }
    }

   
    void Start()
    {
        
    }

    public bool IsInView(Vector3 worlPos){
        Transform cameraTransform = Camera.main.transform;
        Vector2 viewPos = Camera.main .WorldToViewportPoint(worlPos);
        Vector3 dir = (worlPos - cameraTransform.position).normalized;
        float dot = Vector3.Dot(cameraTransform.forward,dir);
        if(dot > 0&&viewPos.x >=0 &&viewPos.x <= 1 && viewPos.y >=0 && viewPos.y <=1){
            return true;

        }else {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Vector2 vec2 = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
        // if(IsInView(transform.position)){
        //     Debug.Log("在范围内");
        // }else{
        //     Debug.Log("不在范围内");
        // }
    }
}
