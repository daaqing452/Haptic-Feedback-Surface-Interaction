using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class VisualizeJoint : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, .007f);

		foreach (Transform child in transform){
			Gizmos.DrawLine(transform.position, child.position);
		}
	}
}
