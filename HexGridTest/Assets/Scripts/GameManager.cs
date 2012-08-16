using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState {
    Menu,
    Game,
    Editor
}

public enum PlayState {
    Play,
    Pause,
    Edit
}
/// <summary>
/// Manage Game Behaviour 
/// </summary>
public class GameManager : MonoBehaviour {

    // Components
    private HexBoardManager boardManager;
    private HexInput hexInput;


    // Prefabs
    public GameObject characterPrefab; // for testing only, move to somewhere else


    // Set-up Variables
    public LayerMask layer;


    // GAME STATES
    GameState gameState;
    PlayState playState;


    // Tool States
    private int toolTile;
    private int toolObject;


    private GameObject myCharacter; // for testing only, move to somewhere else

    // UI list of Rects, for ignoring mouse clicks.
    List<Rect> uiList;

    void Awake() {
        boardManager = GetComponent<HexBoardManager>();
        hexInput = GetComponent<HexInput>();
    }


	void Start () {
        gameState = GameState.Editor;
        playState = PlayState.Play;

        uiList = new List<Rect>();

        boardManager.debug = true;
	}
	

	void Update () {
	
	}


    public void LeftClick(Vector2 mousePos) {
        // Return if clicking on UI box.
        foreach (Rect r in uiList) { if (r.Contains(mousePos)) return; }
        if (gameState == GameState.Editor) {
            Debug.Log("Left Click (" + Time.deltaTime + ")");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit, 50, layer)) {
                Debug.Log("Ray is Cast");
                if (hit.transform.gameObject.tag == "hexagonFlat") {
                    Point2 tileGridPos = hit.transform.gameObject.GetComponent<TilePos>().arrayPos;
                    Debug.Log("Tile Found: getPath()");
                    boardManager.getPath(new Point2(0, 0), tileGridPos);
                }
            } else { Debug.Log("Left Click: No Hit!"); }
        }
    }


    public void RightClick(Vector2 mousePos) {
        // Return if clicking on UI box.
        foreach (Rect r in uiList) { if (r.Contains(mousePos)) return; }

        if(gameState == GameState.Editor){
            Debug.Log("Right Click (" + Time.deltaTime + ")");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit, 50, layer)) {
                Debug.Log("Ray is Cast");
                if (hit.transform.gameObject.tag == "hexagonFlat" || hit.transform.gameObject.tag == "hexagonHill") {
                    Point2 tileGridPos = hit.transform.gameObject.GetComponent<TilePos>().arrayPos;
                    Debug.Log("Tile Found: toggleTile()");
                    boardManager.ToggleTile(tileGridPos);
                }
            } else { Debug.Log("Right Click: No Hit!"); }
        }
    }


    public void MiddleClick(Vector2 mousePos) {
    }


    void OnGUI() {
        switch (gameState) {
            case GameState.Menu: 
                displayMenu(); 
                break;
            case GameState.Game:
                displayGame();
                break;
            case GameState.Editor:
                displayEditor();
                break;
        }

    }

    void displayMenu() {

        GUI.Label(new Rect(25, 25, 100, 20), "Level Editor");
        GUI.Box(new Rect(20, 45, 110, 105), "");
        
        if (GUI.Button(new Rect(25, 50, 100, 20), "Load Board")) {
            // load board
        }

        GUI.TextField(new Rect(25, 100, 45, 20), "50", 4);
        GUI.TextField(new Rect(80, 100, 45, 20), "50", 4);
        if (GUI.Button(new Rect(25, 125, 100, 20), "New Board")) {
            // new board
        }

    }

    void displayGame() {


    }

    


    void displayEditor() {
        /* Right Side GUI Box */
        Rect uiBox = new Rect(Screen.width - 125, 20, 110, 205);
        GUI.Box(uiBox, "");
        if (!uiList.Exists(x => x == uiBox)) uiList.Add(uiBox);

        if (GUI.Button(new Rect(Screen.width - 120, 25, 100, 20), "All Flat")) {
            boardManager.SetAllTiles(HexTileType.Flat);
        }
        if (GUI.Button(new Rect(Screen.width - 120, 50, 100, 20), "All Hill")) {
            boardManager.SetAllTiles(HexTileType.Hill);
        }

        GUI.Label(new Rect(Screen.width - 120, 75, 100, 20), "Tile");
        toolTile = GUI.Toolbar(new Rect(Screen.width - 120, 100, 100, 20), toolTile, new string[] { "0", "1", "2" });

        GUI.Label(new Rect(Screen.width - 120, 125, 100, 20), "Object");
        toolObject = GUI.Toolbar(new Rect(Screen.width - 120, 150, 100, 20), toolObject, new string[] { "0", "1", "2" });

        if (GUI.Button(new Rect(Screen.width - 120, 200, 100, 20), "Save")) {
            boardManager.SaveBoard("myBoard");
        }

    }
}
