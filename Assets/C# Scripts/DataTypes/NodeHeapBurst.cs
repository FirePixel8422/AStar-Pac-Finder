using Unity.Burst;
using Unity.Collections;

namespace FirePixel.PathFinding
{
    [BurstCompile]
    public static class NodeHeapBurst
    {
        public static void Add(ref NativeArray<Node> nodes, ref int count, ref Node node)
        {
            // Insert at end
            node.heapIndex = count;
            nodes[count] = node;

            // Bubble up by index
            SortUp(ref nodes, count);
            count++;
        }

        public static Node RemoveFirstSwapBack(ref NativeArray<Node> nodes, ref int count)
        {
            // Assume caller ensures count > 0
            Node firstNode = nodes[0];
            count--;

            if (count <= 0)
            {
                // Heap is now empty, nothing to reorder
                return firstNode;
            }

            // Move last node to root and heapify down
            Node lastNode = nodes[count];
            lastNode.heapIndex = 0;
            nodes[0] = lastNode;

            SortDown(ref nodes, count, 0);
            return firstNode;
        }

        public static bool Contains(NativeArray<Node> nodes, Node node, int count)
        {
            int id = node.gridId;
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].gridId == id)
                    return true;
            }
            return false;
        }

        private static void SortDown(ref NativeArray<Node> nodes, int count, int startIndex)
        {
            int index = startIndex;

            while (true)
            {
                int left = index * 2 + 1;
                int right = index * 2 + 2;
                int swapIndex = -1;

                if (left < count)
                {
                    swapIndex = left;
                    if (right < count && nodes[left].CompareTo(nodes[right]) < 0)
                    {
                        swapIndex = right;
                    }

                    if (nodes[index].CompareTo(nodes[swapIndex]) < 0)
                    {
                        // swap nodes[index] <-> nodes[swapIndex] and update heapIndex fields
                        Node a = nodes[index];
                        Node b = nodes[swapIndex];

                        int ai = a.heapIndex;
                        int bi = b.heapIndex;

                        a.heapIndex = bi;
                        b.heapIndex = ai;

                        nodes[index] = b;
                        nodes[swapIndex] = a;

                        index = swapIndex;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private static void SortUp(ref NativeArray<Node> nodes, int startIndex)
        {
            int index = startIndex;
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (nodes[index].CompareTo(nodes[parent]) > 0)
                {
                    Node a = nodes[index];
                    Node b = nodes[parent];

                    int ai = a.heapIndex;
                    int bi = b.heapIndex;

                    a.heapIndex = bi;
                    b.heapIndex = ai;

                    nodes[index] = b;
                    nodes[parent] = a;

                    index = parent;
                }
                else
                {
                    return;
                }
            }
        }

        public static void Clear(ref int count)
        {
            count = 0;
        }
    }
}
