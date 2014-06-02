using UnityEngine;
using System.Collections;

/// <summary>
/// Search Node for Pathfinding.
/// </summary>
public class SearchNode {
    public Point2 position;
    public int cost;
    public int pathCost;
    public SearchNode next;
    public SearchNode nextListElem;

    /// <summary>
    /// Create new Search Node.
    /// </summary>
    /// <param name="_position">Position of the Search Node in the grid.</param>
    /// <param name="_cost">Cost to move to this Node.</param>
    /// <param name="_pathCost">Estimated cost of the path.</param>
    /// <param name="_next">Next Search Node.</param>
    public SearchNode(Point2 _position, int _cost, int _pathCost, SearchNode _next) {
        this.position = _position;
        this.cost = _cost;
        this.pathCost = _pathCost;
        this.next = _next;
    }



}