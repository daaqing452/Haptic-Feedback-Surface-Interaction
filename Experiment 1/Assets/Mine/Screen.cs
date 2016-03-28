using UnityEngine;
using System.Collections;
using System;

public class Screen : MonoBehaviour {

    GameObject target;

    // Use this for initialization
    void Start () {
        target = GameObject.FindGameObjectWithTag("target");
	}
	
	// Update is called once per frame
	void Update () {
        System.Random r = new System.Random();
        float x = r.Next(80) - 40;
        float y = r.Next(80) - 40;
        target.transform.localPosition = new Vector3(x / 100, y / 100, -0.6f);
	}
}
