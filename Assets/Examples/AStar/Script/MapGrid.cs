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

namespace Qkuang_Space.AStar
{
    public class MapGrid : MonoBehaviour
    {

        public LayerMask unwalkableMask;
        public Vector2 gridWoldSize;        //全图 大小
        public float nodeRadius;            // 每个格子的半径
        Node[,] grid;

        float nodeDiameter;         //每个格子的直径
        int gridSizeX, gridSizeY;   // 格子的数量

        public List<Node> path;         // 存储获得的最短路径，的节点集合。
        private void Start()
        {
            nodeDiameter = nodeRadius * 2;
            gridSizeX = Mathf.RoundToInt(gridWoldSize.x / nodeDiameter);
            gridSizeY = Mathf.RoundToInt(gridWoldSize.y / nodeDiameter);
            CreateGrid();
        }

        /// <summary>
        /// 创建所有节点
        /// </summary>
        void CreateGrid()
        {
            grid = new Node[gridSizeX, gridSizeY];
            // 求左下角的位置 。
            Vector3 worldBottomLeft = transform.position - Vector3.right * gridWoldSize.x / 2 - Vector3.forward * gridWoldSize.y / 2;
            for (int x = 0;x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    //计算每个格子的位置  （注意：第一个个格子位置肯定不是 左下角，所以需要一个半径
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                    //假设目标位置有个球，返回和 指定“层”是否相交。
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    grid[x, y] = new Node(walkable, worldPoint,x ,y );
                }
            }
        }
        /// <summary>
        /// 传入 Player的位置，返回对应位置的 格子
        /// </summary>
        /// <param name="worldposition">Player位置</param>
        /// <returns></returns>
        public Node NodeFromWorldPoint(Vector3 worldposition)
        {
            //对传入的 位置经行坐标转换，世界转自身系。
            worldposition = transform.InverseTransformPoint(worldposition);
            float percentX = (worldposition.x + gridWoldSize.x / 2) / gridWoldSize.x;
            // 注意 这里是Z 对应 Y。 —— 归一化
            float percentY = (worldposition.z + gridWoldSize.y / 2) / gridWoldSize.y;
            // 防止 索引超出范围
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

            return grid[x, y];
        }

        /// <summary>
        /// 绘制节点
        /// </summary>
        private void OnDrawGizmos()
        {
            
            Gizmos.DrawWireCube(transform.position, new Vector3 (gridWoldSize.x,1,gridWoldSize.y ));

            if (grid!= null)
            {
               // Node playerNode = NodeFromWorldPoint(player.position);
                foreach (Node n in grid)
                {
                    //修改Gizmo 的颜色 根据是否 可以 走
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    if (path != null)
                        if (path.Contains(n))
                            Gizmos.color = Color.yellow ;
                    //减0.1f ，避免紧挨着。
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
        }
        /// <summary>
        /// 传入 一个节点，返回相邻的节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List <Node > GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x =-1;x <=1;x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                        neighbours.Add(grid[checkX, checkY]);
                }
            }

                    return neighbours;
        }

    }

}
