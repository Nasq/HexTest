using UnityEngine;
using System.Collections;

public class character : MonoBehaviour {

    // Components
    private Transform _transform;

    // Objects


    // Variables
    public float moveSpeed = 8.0f;
    public float turnSpeed = 8.0f;

    void Awake() {
        _transform = this.transform;
    }


	void Start () {
	
	}
	
    
    IEnumerable Update () {

        yield return null;
	}



}
