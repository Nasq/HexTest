using UnityEngine;
using System.Collections;

/// <summary>
/// Rotate vertically around world position.
/// </summary>
public class Rotate : MonoBehaviour {

    // Components
    private Transform _transform;

    // Set-Up
    public float speed = 20f;


	void Awake () {
        _transform = transform;
	}
	

	void Update () {
        _transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);
	}
}
