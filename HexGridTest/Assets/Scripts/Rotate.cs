using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

    // Components
    private Transform _transform;

    // Set-Up
    public float speed = 20f;

	// Use this for initialization
	void Awake () {
        _transform = transform;
	}
	
	// Update is called once per frame
	void Update () {

        _transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);

	}
}
