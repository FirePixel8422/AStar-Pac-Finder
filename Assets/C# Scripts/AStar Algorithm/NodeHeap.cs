using UnityEngine;
using System.Collections;

namespace FirePixel.PathFinding
{
    public struct NodeHeap
    {
        public Node[] items;
        public int Count { get; private set; }

        public NodeHeap(int maxHeapSize)
        {
            items = new Node[maxHeapSize];
            Count = 0;
        }

        public void Add(Node item)
        {
            item.heapIndex = Count;
            items[Count] = item;
            SortUp(item);
            Count++;
        }

        public Node RemoveFirst()
        {
            Node firstItem = items[0];
            Count--;
            items[0] = items[Count];
            items[0].heapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        public void UpdateItem(Node item)
        {
            SortUp(item);
        }

        public bool Contains(Node item)
        {
            return Equals(items[item.heapIndex], item);
        }

        private void SortDown(Node item)
        {
            while (true)
            {
                int childIndexLeft = item.heapIndex * 2 + 1;
                int childIndexRight = item.heapIndex * 2 + 2;
                int swapIndex;

                if (childIndexLeft < Count)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < Count)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(items[swapIndex]) < 0)
                    {
                        Swap(item, items[swapIndex]);
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

        private void SortUp(Node item)
        {
            int parentGridPos = (item.heapIndex - 1) / 2;

            while (true)
            {
                Node parentItem = items[parentGridPos];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentGridPos = (item.heapIndex - 1) / 2;
            }
        }

        private void Swap(Node itemA, Node itemB)
        {
            items[itemA.heapIndex] = itemB;
            items[itemB.heapIndex] = itemA;
            int itemAIndex = itemA.heapIndex;
            itemA.heapIndex = itemB.heapIndex;
            itemB.heapIndex = itemAIndex;
        }
    }
}