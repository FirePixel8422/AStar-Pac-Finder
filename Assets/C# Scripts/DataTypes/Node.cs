using System;
using Unity.Mathematics;
using UnityEngine;


namespace FirePixel.PathFinding
{
    public struct Node : IEquatable<Node>, IComparable<Node>
    {
        public int gridId;
        public int parentNodeId;

        [Tooltip("Unsure if this will be a feature")]
        public float3 worldPos;

        [Tooltip("0 = not walkable, 1 = walkable")]
        public bool walkable;
        public int movementPenalty;
        public int layerId;

        public int gCost;
        public int hCost;
        public int FCost => gCost + hCost;

        public int heapIndex;


        public Node(int gridId, bool walkable, int movementPenalty, int layerId, float3 worldPos)
        {
            this.gridId = gridId;
            this.walkable = walkable;
            this.movementPenalty = movementPenalty;
            this.layerId = layerId;
            this.worldPos = worldPos;
            parentNodeId = -1;
            gCost = 0;
            hCost = 0;
            heapIndex = 0;
        }

        public void UpdateNode(int gCost, int hCost, int parentNodeId)
        {
            this.gCost = gCost;
            this.hCost = hCost;
            this.parentNodeId = parentNodeId;
        }

        #region Is equal and not equal operators

        public static bool operator == (Node a, Node b)
        {
            return a.gridId == b.gridId;
        }

        public static bool operator != (Node a, Node b)
        {
            return !(a == b);
        }

        public bool Equals(Node other)
        {
            return gridId == other.gridId;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node other)
            {
                return this == other;
            }
            return false;
        }

        #endregion


        public int CompareTo(Node nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }


        public override int GetHashCode()
        {
            return gridId.GetHashCode();
        }
    }
}