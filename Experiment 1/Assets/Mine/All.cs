using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class All : MonoBehaviour {

    TcpClient socket = new TcpClient();
    const string serverIP = "192.168.1.159";
    const int serverPort = 7643;

    const float adjustDelta = 0.001f;

    // Use this for initialization
    void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        ButtonClick();
        GameObject indexTop = GameObject.Find("index_top_r");
        Debug.Log(indexTop.transform.position.z);
    }

    void ButtonClick()
    {
        //  Connect to server
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (socket.Available == 0)
            {
                Debug.Log("Network connecting ...");
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
        }

        //  adjust screen
        if (Input.GetKey(KeyCode.W))
        {
            GameObject screen = GameObject.Find("Screen Kernel");
            screen.transform.Translate(0.0f, adjustDelta, 0.0f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            GameObject screen = GameObject.Find("Screen Kernel");
            screen.transform.Translate(0.0f, -adjustDelta, 0.0f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            GameObject screen = GameObject.Find("Screen Kernel");
            screen.transform.Translate(-adjustDelta, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            GameObject screen = GameObject.Find("Screen Kernel");
            screen.transform.Translate(adjustDelta, 0.0f, 0.0f);
        }

        //  adjust hand
        if (Input.GetKey(KeyCode.T))
        {
            GameObject hand = GameObject.Find("hand_right_prefab");
            hand.transform.Translate(0.0f, adjustDelta, 0.0f);
        }
        if (Input.GetKey(KeyCode.G))
        {
            GameObject hand = GameObject.Find("hand_right_prefab");
            hand.transform.Translate(0.0f, -adjustDelta, 0.0f);
        }
        if (Input.GetKey(KeyCode.F))
        {
            GameObject hand = GameObject.Find("hand_right_prefab");
            hand.transform.Translate(-adjustDelta, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.H))
        {
            GameObject hand = GameObject.Find("hand_right_prefab");
            hand.transform.Translate(adjustDelta, 0.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.R))
        {
            GameObject hand = GameObject.Find("hand_right_prefab");
            hand.transform.Translate(0.0f, 0.0f, -adjustDelta);
        }
        if (Input.GetKey(KeyCode.Y))
        {
            GameObject hand = GameObject.Find("hand_right_prefab");
            hand.transform.Translate(0.0f, 0.0f, adjustDelta);
        }
    }

    void ReceiveThread()
    {
        Debug.Log("Network receiving");
        StreamReader streamReader = new StreamReader(socket.GetStream());
        while (true)
        {
            string line = streamReader.ReadLine();
            if (line == null) break;
            string[] arr = line.Split(' ');
        }
        Debug.Log("Network disconnect");
    }
}
