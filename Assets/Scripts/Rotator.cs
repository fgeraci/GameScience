using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    private Transform Object;

	// Use this for initialization
	void Awake () {
        Object = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        Object.Rotate(Vector3.up, 0.5f);
        Object.Rotate(Vector3.right, 0.5f);
    }
}
