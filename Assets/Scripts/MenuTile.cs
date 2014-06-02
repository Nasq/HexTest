using UnityEngine;
using System.Collections;

[SerializeField]
public enum MenuTileType {
    Exit,
    Editor
}


public class MenuTile : MonoBehaviour {

    Color startColor;

    [SerializeField]
    MenuTileType menuTileType;

    bool hovering = false;


    void Start() {
        startColor = renderer.material.color;
    }


    void Update() {
        if (!hovering) renderer.material.color = Color.Lerp(renderer.material.color, startColor, Time.deltaTime * 5);
    }


    void OnMouseEnter() {
        renderer.material.color = Color.gray;
        hovering = true;
        audio.Play();
    }


    void OnMouseUpAsButton() {
        audio.Play();
        switch (menuTileType) {
            case MenuTileType.Exit:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                Debug.Log("pressed quit");
                break;
            case MenuTileType.Editor:
                Debug.Log("pressed editor");
                break;
        }
    }
     

}
