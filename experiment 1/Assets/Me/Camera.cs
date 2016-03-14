using UnityEngine;
using System.Collections;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class Camera : MonoBehaviour
{
    public TcpClient client;
    public string serverIP = "127.0.0.1";
    public int serverPort = 7643;

	// Use this for initialization
	void Start ()
    {
        client = new TcpClient();
        client.Connect(IPAddress.Parse(serverIP), serverPort);
        Thread receiveThread = new Thread(ReceiveThread);
        receiveThread.Start();
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {

    }

    void ReceiveThread()
    {
        Debug.Log("receiving");
        StreamReader streamReader = new StreamReader(client.GetStream());
        while (true)
        {
            string line = streamReader.ReadLine();
            if (line == null) break;
            Debug.Log(line);
        }
        Debug.Log("disconnect");
    }
}
