/*
 * Author ： Qkuang
 * Version ：1.0
 * Date ：2019.11.27
 * UnityVersion：2019.3a
 * AutoDate：2019.11.27
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Qkuang_Space
{
    namespace AStar
    {
        /// <summary>
        /// AStart 算法节点类
        /// </summary>
        public class Node 
        {
            public bool walkable;           //是否可以行走
            public Vector3 worldPosition;
            /// <summary>
            /// 每个节点 在整个棋盘中的位置，整形
            /// </summary>
            public int gridX;   
            public int gridY;

            public int gCost;               //G、H值、 父节点
            public int hCost;
            public Node parent;

            public int fCost { get => gCost + hCost; }

            public Node (bool _walkable,Vector3 _worldPos,int _gridX,int _gridY)
            {
                this.walkable = _walkable;
                this.worldPosition = _worldPos;
                gridX = _gridX;
                gridY = _gridY;
            }

        
        }
    }

}
