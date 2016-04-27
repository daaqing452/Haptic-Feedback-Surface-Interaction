using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
	
	public float speed;
	private Rigidbody rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void FixedUpdate() {
		Vector3 movement = new Vector3 (Input.GetAxis ("Horizontal"), 0.0f, Input.GetAxis ("Vertical"));
		rb.AddForce (movement * speed);
	}
}
