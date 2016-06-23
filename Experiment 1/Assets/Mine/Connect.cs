//#define CONNECT

using UnityEngine;
using System.Collections;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

public class Connect : MonoBehaviour
{
    TcpClient socket;
    string serverIP = "192.168.1.159";
    int serverPort = 7643;

    object mutex = new object();
    Vector3 rbp = new Vector3(0.0f, 0.0f, 0.0f);
    Vector4 rbq = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    Vector3 rbpDefault = new Vector3(0.26f, 0.40f, 0.14f);

    // Use this for initialization
    void Start ()
    {
        socket = new TcpClient();
        try
        {
            socket.Connect(serverIP, serverPort);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        if (socket.Available != 0)
        {
            Thread receiveThread = new Thread(ReceiveThread);
            receiveThread.Start();
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        GameObject lowerarm1 = GameObject.Find("lowerarm1_r");
        lowerarm1.transform.localPosition = new Vector3(rbp.x, rbp.y, rbp.z) - rbpDefault;
        lowerarm1.transform.localRotation = new Quaternion(rbq.x, rbq.y, rbq.z, rbq.w);

        //GameObject index3 = GameObject.FindWithTag("pointing");
        //Debug.Log(index3.transform.position.x + " " + index3.transform.position.y + " " + index3.transform.position.z);
        //Debug.Log(rbp.x + " " + rbp.y + " " + rbp.z);
    }

    void ReceiveThread()
    {
        Debug.Log("receiving");
        StreamReader streamReader = new StreamReader(socket.GetStream());
        while (true)
        {
            string line = streamReader.ReadLine();
            if (line == null) break;
            string[] arr = line.Split(' ');
            switch (arr[0])
            {
                case "framestart":
                    break;
                case "frameend":
                    break;
                case "rbp":
                    rbp = new Vector3(-float.Parse(arr[3]), -float.Parse(arr[2]), float.Parse(arr[1]));
                    break;
                case "rbq":
                    rbq = new Vector4(float.Parse(arr[3]), float.Parse(arr[2]), -float.Parse(arr[1]), float.Parse(arr[4]));
                    break;
                default:
                    break;
            }
            //Debug.Log(line);
            //Debug.Log(rbp.x + " " + rbp.y + " " + rbp.z);
        }
        Debug.Log("disconnect");
    }
}
