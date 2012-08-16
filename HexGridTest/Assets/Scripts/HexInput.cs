using UnityEngine;
using System.Collections;

public class HexInput : MonoBehaviour {

    // Components
    private GameManager gameManager;


    void Awake() {
        gameManager = GetComponent<GameManager>();
    }

	
	void Update () {

        if (Input.GetMouseButtonDown(0)) {
            gameManager.LeftClick(GetMousePos());
        }
        if (Input.GetMouseButtonDown(1)) {
            gameManager.RightClick(GetMousePos());
        }
        if (Input.GetMouseButtonDown(2)) {
            gameManager.MiddleClick(GetMousePos());
        }
	}


    /// <summary>
    /// Get mouse position on screen with height reversed to match GUI.
    /// </summary>
    /// <returns>Mouse Position</returns>
    public Vector2 GetMousePos() {
        return new Vector2(Input.mousePosition.x, -(Input.mousePosition.y - Screen.height));
    }

}
