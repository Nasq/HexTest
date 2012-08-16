using UnityEngine;
using System.Collections;


/// <summary>
/// Minimum Heap
/// </summary>
public class MinHeap {
    private SearchNode listHead;

    /// <summary>
    /// Has Next?
    /// </summary>
    /// <returns>Boolean, Has next?</returns>
    public bool HasNext() {
        return listHead != null;
    }

    public void Add(SearchNode item) {
        if (listHead == null) {
            listHead = item;
        } else if (listHead.next == null && item.cost <= listHead.cost) {
            item.nextListElem = listHead;
            listHead = item;
        } else {
            SearchNode ptr = listHead;
            while (ptr.nextListElem != null && ptr.nextListElem.cost < item.cost)
                ptr = ptr.nextListElem;
            item.nextListElem = ptr.nextListElem;
            ptr.nextListElem = item;
        }
    }

    public SearchNode ExtractFirst() {
        SearchNode result = listHead;
        listHead = listHead.nextListElem;
        return result;
    }
}
