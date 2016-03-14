using UnityEngine;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Network : MonoBehaviour {

	public TcpClient client;
	public StreamReader sr;

	float X, Y, Z;

	// Use this for initialization
	void Start () {	
		client = new TcpClient ();
		client.Connect (IPAddress.Parse ("127.0.0.1"), 9898);
		Thread receiveThread = new Thread(Receive);
		receiveThread.Start();
		X = Y = Z = 0;
	}

	void Receive() {
		Debug.Log("In receive");
		sr = new StreamReader (client.GetStream ());
		while (true)
		{
			string str = sr.ReadLine();
			if (str == null) break;
			string[] strArray = str.Split(' ');
			X = float.Parse(strArray[0]) * 3;
			Y = float.Parse(strArray[1]) * 3;
			Z = float.Parse(strArray[2]) * 3;
			Debug.Log("Receive " + X + " " + Y + " " + Z);
		}
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.position = new Vector3 (X, Y, Z);
	}
}
