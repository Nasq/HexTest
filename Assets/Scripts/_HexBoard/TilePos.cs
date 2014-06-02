using UnityEngine;

/// <summary>
/// Tile position in the board and Hover behaviour.
/// </summary>
public class TilePos : MonoBehaviour {

    /// <summary>
    /// Position of this tile on the board.
    /// </summary>
    public Point2 arrayPos;
    
    private Color startColor;
    private bool hovering = false;


    void Start() {
        startColor = renderer.material.color;
    }


    void Update() {
        if(!hovering) renderer.material.color = Color.Lerp(renderer.material.color, startColor, Time.deltaTime * 5);
    }


    void OnMouseEnter() {
        renderer.material.color = Color.gray;
        hovering = true;
    }


    void OnMouseExit() {
        hovering = false;
    }
    
}
