/*
 *  Author：Qukuang
 *  Version ：1.0
 *  Date ：2019.11.27
 *  UnityVersion：2019.3a
 *  AutoDate：2019.11.27
 * Remark：当前的AStar 未经行 “二叉堆树”优化。主要问题是：当地图一大，开放列表中要寻找到F值最小的节点，花费时间较长。导致计算量过大，CPU占用过高
 *                  等我  熟练掌握数据结构时再来优化。
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Qkuang_Space.AStar
{

    public class PathFinding : MonoBehaviour
    {

        public Transform seeker, target;
        MapGrid  grid;

        private void Awake()
        {
            grid = GetComponent<MapGrid >();

        }
        private void Update()
        {
            FindPath(seeker.position, target.position);
        }

        /// <summary>
        /// AStar 算法核心 函数。
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="targetPos"></param>
        void FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);

            List<Node> openSet = new List<Node>();      // 开放列表
            HashSet<Node> closedSet = new HashSet<Node>();      // 关闭列表
            openSet.Add(startNode);


            while (openSet.Count >0)
            {
                Node currentNode = openSet[0];

                //循环：为了判断开发列表中，哪个作为 当前节点 去参加接下来的运算。
                for (int i = 1; i < openSet.Count; i++)
                {
                    // 如果后一个节点的 的F值小，（若相同，就用 h值小的） 就将后一个节点作为 当前节点。
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];

                    }
                }

                openSet.Remove(currentNode);        // 吧当前节点，从开放列表移除…… 避免了一个节点多次参加运算。
                closedSet.Add(currentNode);             //把当前节点，移入到关闭列表

                if (currentNode == targetNode)
                {
                    // 判断当前节点是否是 目标节点。如果是，回忆路径，并退出循环 ！
                    //同时，如果一直找不到，会把 开放列表中的所有节点移除完，此时自动退出 循环。
                    RetracePath(startNode, targetNode);
                    return;
                }

                foreach (Node  neighbour in grid .GetNeighbours (currentNode ))
                {
                    // 遍历当前节点的相邻点 ，如果不能走、或者 包含在关闭列表中，那么继续下一次循环
                    if (!neighbour .walkable || closedSet .Contains (neighbour))
                    {
                        continue;
                    }
                    // 计算 G H F ：
                    // 当前相邻点的G值 = 当前点的G值+ 距离（当前，当前相邻） 【注】 发现G值，其实是累加的 ，因为G表示从起点开始计算
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    // 若 当前相邻点 ，未包含到 开放列表，则包含进入，并计算 GH，
                    //第一个条件是最难理解的。如果相邻点在 已经包含进入 开放列表，那么新的G值，小于 旧G值，才从新计算G值。—— 撞墙情况下
                    if (newMovementCostToNeighbour <neighbour .gCost || ! openSet .Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);       // h值不应该每次都计算，因为不变，除非 结束位置在时刻变化。
                        neighbour.parent = currentNode;
                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }

                }
            }
        }
        /// <summary>
        /// 回忆路径，如果计算 成功，那么可以通过结束点的 父节点.父节点……的方式，回忆路径
        /// </summary>
        /// <param name="startNode">开始点</param>
        /// <param name="endNode">目标点</param>

        void RetracePath (Node startNode ,Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();         //反转一下路径，因为第一个节点是从 结束点开始
            grid.path = path;
        }

        /// <summary>
        /// 返回 A星算法 中的 特定距离 算法 计算得到的 距离
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>

        int GetDistance( Node nodeA ,Node nodeB)
        {
            int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (distX > distY)
                return 14 * distY + (distX - distY) * 10;
            return 14 * distX + (distY - distX) * 10;
        }

    }
}
