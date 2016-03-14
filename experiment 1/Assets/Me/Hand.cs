using UnityEngine;
using System.Collections;

public class Hand : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //dfs(this.gameObject);
	}
    
	// Update is called once per frame
	void Update () {
        GameObject go = GameObject.FindWithTag("hand_r");
        go.transform.Rotate(0, 0, 10);
	}

    void dfs(GameObject go)
    {
        Debug.Log(go);
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        foreach (Transform t in go.transform)
        {
            dfs(t.gameObject);
        }
    }
}
