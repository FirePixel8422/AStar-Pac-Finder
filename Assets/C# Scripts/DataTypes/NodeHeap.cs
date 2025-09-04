using Unity.Collections;


namespace FirePixel.PathFinding
{
    public struct NodeHeap
    {
        private NativeArray<Node> nodes;
        public int Count { get; private set; }

        public NodeHeap(int maxHeapSize, Allocator allocator)
        {
            nodes = new NativeArray<Node>(maxHeapSize, allocator);
            Count = 0;
        }

        public void Add(Node node)
        {
            node.heapIndex = Count;
            nodes[Count] = node;
            SortUp(node);
            Count++;
        }

        public Node RemoveFirstSwapBack()
        {
            Node firstNode = nodes[0];
            Count--;
            
            // Set first node to last node
            Node lastNode = nodes[Count];
            lastNode.heapIndex = 0;
            nodes[0] = lastNode;

            SortDown(nodes[0]);
            return firstNode;
        }

        public bool Contains(Node node)
        {
            return nodes[node.heapIndex] == node;
        }

        private void SortDown(Node node)
        {
            while (true)
            {
                int childIndexLeft = node.heapIndex * 2 + 1;
                int childIndexRight = node.heapIndex * 2 + 2;
                int swapIndex;

                if (childIndexLeft < Count)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < Count)
                    {
                        if (nodes[childIndexLeft].CompareTo(nodes[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (node.CompareTo(nodes[swapIndex]) < 0)
                    {
                        Swap(node, nodes[swapIndex]);
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

        private void SortUp(Node node)
        {
            int parentGridPos = (node.heapIndex - 1) / 2;

            while (true)
            {
                Node parentItem = nodes[parentGridPos];
                if (node.CompareTo(parentItem) > 0)
                {
                    Swap(node, parentItem);
                }
                else
                {
                    break;
                }

                parentGridPos = (node.heapIndex - 1) / 2;
            }
        }

        private void Swap(Node nodeA, Node nodeB)
        {
            int nodeAIndex = nodeA.heapIndex;
            nodeA.heapIndex = nodeB.heapIndex;
            nodeB.heapIndex = nodeAIndex;

            nodes[nodeA.heapIndex] = nodeB;
            nodes[nodeB.heapIndex] = nodeA;
        }

        public void Clear()
        {
            Count = 0;
        }


        public void DisposeIfCreated()
        {
            nodes.DisposeIfCreated();
        }
    }
}