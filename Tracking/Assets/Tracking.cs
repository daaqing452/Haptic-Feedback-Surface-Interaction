using UnityEngine;
using System.Collections;

using System.IO;

public class Tracking : MonoBehaviour
{
    StreamReader reader;

    // Use this for initialization
    void Start()
    {
        reader = new StreamReader(new FileStream("Take 2016-04-15 03.02.24 PM.csv", FileMode.Open));
        string line;
        for (int i = 0; i < 7; i++)
        {
            line = reader.ReadLine();
        }
    }

	// Update is called once per frame
	void Update ()
    {
        string line = reader.ReadLine();
        string[] arr = line.Split(',');
        int j = 0;
        int bias = 2;
        //for (int i = 0; i * 3 + 22 < arr.Length && j < 22; i++)
        for (int i = 0; i * 3 + 2 < arr.Length && j < 12; i++)
        {
            GameObject sphere = GameObject.Find("Sphere (" + j.ToString() + ")");
            /*Renderer r = sphere.GetComponent<Renderer>();
            r.material.color = Color.blue;*/
            if (arr[i * 3 + bias + 0] == "") continue;
            j++;
            float x = 10 * float.Parse(arr[i * 3 + bias + 0]) - sphere.transform.position.x;
            float y = 10 * float.Parse(arr[i * 3 + bias + 1]) - sphere.transform.position.y;
            float z = 10 * float.Parse(arr[i * 3 + bias + 2]) - sphere.transform.position.z;
            sphere.transform.Translate(x, y, z, Space.World);
        }
    }
}

class Point3D
{
    float x;
    float y;
    float z;
}
