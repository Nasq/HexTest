using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

public enum GameState { Game, Editor }

public enum PlayState { Play, Pause, Edit }

/// <summary>
/// Manage Game Behaviour 
/// </summary>
public class GameManager : MonoBehaviour {

    // Components
    private HexBoardManager boardManager;
    private HexInput hexInput;
    private TileList tileList;

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

    // Internal Variables
    private bool editToolTip = true;
    private GameObject myCharacter; 
    private List<Rect> uiList; // UI list of Rects, for ignoring mouse clicks.


    void Awake() {
        boardManager = GetComponent<HexBoardManager>();
        hexInput = GetComponent<HexInput>();
        tileList = GetComponent<TileList>();
    }


    void Start() {
        gameState = GameState.Editor;
        playState = PlayState.Play;

        uiList = new List<Rect>();

        boardManager.debug = true;
    }


    void Update() { }


    /// <summary>
    /// Recieve info about new board loads for gui.
    /// </summary>
    /// <param name="x">Board Width</param>
    /// <param name="y">Board Height</param>
    /// <param name="sn">Board Name</param>
    public void LoadInfo(int x, int y, string sn) {
        sizeX = x.ToString();
        sizeY = y.ToString();
        saveName = sn;
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

        if (gameState == GameState.Editor) {
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


    /// <summary>
    /// Build the GUI - Editor / Game / Sub-Menus
    /// </summary>
    void OnGUI() {
        switch (gameState) {
            case GameState.Game:
                displayGame();
                break;
            case GameState.Editor:
                displayEditor();
                break;
        }

        GUI.Label(new Rect(25, 200, 100, 20), "UI Count: " + uiList.Count);
    }

    Vector2 scrollPosition = Vector2.zero;
    string sizeX = "50";
    string sizeY = "50";
    Rect tempBox1, tempBox2;
    void displayEditorStart() {
        // Center Editor Tooltip Box
        tempBox1 = new Rect(Screen.width / 2 - 80, Screen.height / 2 - 80, 160, 160);
        GUI.Box(tempBox1, "");
        if (!uiList.Exists(x => x == tempBox1)) uiList.Add(tempBox1);

        GUI.Label(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 75, 150, 20), "Load Board");
        /*if (GUI.Button(new Rect(Screen.width / 2 - 75, Screen.height / 2 - 75, 150, 20), "Load Board")) {
            // load board
        }*/

        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/data/");
        FileInfo[] info = dir.GetFiles("*.xml");

        scrollPosition = GUI.BeginScrollView(new Rect(Screen.width / 2 - 75, Screen.height / 2 -50, 150, 100), 
            scrollPosition, new Rect(0, 0, 130, 200), false, true);

        // Make four buttons - one in each corner. The coordinate system is defined
        // by the last parameter to BeginScrollView.
        
        
        int offset = 0;
        foreach (FileInfo f in info) {
            string tempName = f.Name.Substring(0, f.Name.Length - f.Extension.Length);
            if (GUI.Button(new Rect(0, offset, 130, 22), tempName)) {
                boardManager.LoadInitialize(tempName);
            }
            offset += 25;
        }

        // End the scroll view that we began above.
        GUI.EndScrollView();


        sizeX = GUI.TextField(new Rect(Screen.width / 2 - 75, Screen.height / 2 + 55, 30, 20), sizeX, 3);
        sizeY = GUI.TextField(new Rect(Screen.width / 2 - 40, Screen.height / 2 + 55, 30, 20), sizeY, 3);

        sizeX = Regex.Replace(sizeX, @"[^0-9 ]", "");
        sizeY = Regex.Replace(sizeY, @"[^0-9 ]", "");

        if (GUI.Button(new Rect(Screen.width / 2 - 5, Screen.height / 2 + 55, 80, 20), "New Board")) {
            boardManager.DestroyBoard();
            boardManager.newBoard(int.Parse(sizeX), int.Parse(sizeY));
            saveName = "New_Board";
        }

        // Center Editor Tooltip Box
        tempBox2 = new Rect(Screen.width / 2 - 80, Screen.height / 2 + 85, 160, 20);
        if (!uiList.Exists(x => x == tempBox2)) uiList.Add(tempBox2);

        if (GUI.Button(tempBox2, "Close")) {
            editToolTip = false;
            uiList.Remove(tempBox1);
            uiList.Remove(tempBox2);
        }

    }




    void displayGame() {


    }



    string saveName = "myboard";
    void displayEditor() {
        // Right Side GUI Box 
        Rect uiBox = new Rect(Screen.width - 125, 20, 110, 280);
        GUI.Box(uiBox, "");
        if (!uiList.Exists(x => x == uiBox)) uiList.Add(uiBox);

        // Right side GUI options
        if (GUI.Button(new Rect(Screen.width - 120, 25, 100, 20), "Load / New")) {
            if (editToolTip) {
                editToolTip = false;
                uiList.Remove(tempBox1);
                uiList.Remove(tempBox2);
            } else { editToolTip = true; }
        }

        if (GUI.Button(new Rect(Screen.width - 120, 75, 100, 20), "All Flats")) {
            boardManager.SetAllTiles(HexTileType.Flat);
        }
        if (GUI.Button(new Rect(Screen.width - 120, 100, 100, 20), "All Hill")) {
            boardManager.SetAllTiles(HexTileType.Hill);
        }

        GUI.Label(new Rect(Screen.width - 120, 125, 100, 20), "Tile");
        toolTile = GUI.Toolbar(new Rect(Screen.width - 120, 150, 100, 20), toolTile, new string[] { "0", "1", "2" });

        GUI.Label(new Rect(Screen.width - 120, 175, 100, 20), "Object");
        toolObject = GUI.Toolbar(new Rect(Screen.width - 120, 200, 100, 20), toolObject, new string[] { "0", "1", "2" });


        saveName = GUI.TextField(new Rect(Screen.width - 120, 245, 100, 22), saveName, 15);
        saveName = Regex.Replace(saveName, @"[^a-zA-Z0-9_]", "");

        if (GUI.Button(new Rect(Screen.width - 120, 275, 100, 20), "Save")) {
            boardManager.SaveBoard(saveName);
        }

        // Show Editor Startup Tooltip
        if (editToolTip) { displayEditorStart(); }


    }
}
