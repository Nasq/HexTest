using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI() {
        GUI.Label(new Rect(50, Screen.height - 200, 150, 30), "Menu Option 1");
        GUI.Label(new Rect(50, Screen.height - 160, 150, 30), "Menu Option 2");
        GUI.Label(new Rect(50, Screen.height - 120, 150, 30), "Menu Option 3");

        if (GUI.Button(new Rect(50, Screen.height - 80, 150, 30), "Level Editor")) {
            Application.LoadLevel("Editor");
        }
    }
}
