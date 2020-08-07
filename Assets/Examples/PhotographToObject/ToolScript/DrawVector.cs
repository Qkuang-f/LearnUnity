using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawVector : MonoBehaviour
{
    // Start is called before the first frame update
    //private  bool isWork;
    public  Color r_color ;
    public Transform r_EndPos;
    private void OnDrawGizmos() {
        // if(!isWork)
        // return;
        Gizmos.color=r_color;
        Gizmos.DrawLine(transform.position,r_EndPos.position);
        
    }
    void Start()
    {  
        // if(r_StartPos==null || r_EndPos == null)
        // return;
        // isWork = true;
    }

    private void OnEnable() {
        //isWork = false;
    }
}
