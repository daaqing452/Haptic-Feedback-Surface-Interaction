using UnityEngine;
using System.Collections;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class Connect : MonoBehaviour
{
    public TcpClient client;
    public string serverIP = "127.0.0.1";
    public int serverPort = 7643;

    public object mutex = new object();
    public float hrx, hry, hrz;

	// Use this for initialization
	void Start ()
    {
        client = new TcpClient();
        client.Connect(IPAddress.Parse(serverIP), serverPort);
        Thread receiveThread = new Thread(ReceiveThread);
        receiveThread.Start();
    }
	
	// Update is called once per frame
	void Update ()
    {
        GameObject hand = GameObject.FindWithTag("hand_r");
        //hand.transform.localPosition = new Vector3(0.1f, 0.1f, 0.1f);
    }

    void ReceiveThread()
    {
        Debug.Log("receiving");
        StreamReader streamReader = new StreamReader(client.GetStream());
        while (true)
        {
            string line = streamReader.ReadLine();
            if (line == null) break;
            string[] f = line.Split(' ');
            lock (mutex)
            {
                hrx = float.Parse(f[0]);
                hry = float.Parse(f[1]);
                hrz = float.Parse(f[2]);
            }
            Debug.Log(line);
        }
        Debug.Log("disconnect");
    }
}
