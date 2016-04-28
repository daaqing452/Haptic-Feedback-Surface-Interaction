using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;

public class All : MonoBehaviour {

    //  Game objects
    GameObject gHand;
    GameObject gIndexTop;
    GameObject gDisplay;
    GameObject gScreen;
    GameObject gPointingTarget;

    //  Network
    TcpClient socket = new TcpClient();
    const string serverIP = "192.168.1.159";
    const int serverPort = 7643;

    //  ButtonClick
    const float adjustDelta = 0.001f;

    //  Task
    bool pointingTargetTouching = false;

    void Start () {
        gHand = GameObject.Find("hand_right_prefab");
        gIndexTop = GameObject.Find("index_top_r");
        gDisplay = GameObject.Find("display");
        gScreen = GameObject.Find("screen");
        gPointingTarget = GameObject.Find("pointing target");
    }
    
    void Update()
    {
        ButtonClick();
        TaskPointing();
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
        if (Input.GetKey(KeyCode.W)) gDisplay.transform.Translate(0.0f, adjustDelta, 0.0f);
        if (Input.GetKey(KeyCode.S)) gDisplay.transform.Translate(0.0f, -adjustDelta, 0.0f);
        if (Input.GetKey(KeyCode.A)) gDisplay.transform.Translate(-adjustDelta, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.D)) gDisplay.transform.Translate(adjustDelta, 0.0f, 0.0f);

        //  adjust hand
        if (Input.GetKey(KeyCode.T)) gHand.transform.Translate(0.0f, adjustDelta, 0.0f);
        if (Input.GetKey(KeyCode.G)) gHand.transform.Translate(0.0f, -adjustDelta, 0.0f);
        if (Input.GetKey(KeyCode.F)) gHand.transform.Translate(-adjustDelta, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.H)) gHand.transform.Translate(adjustDelta, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.R)) gHand.transform.Translate(0.0f, 0.0f, -adjustDelta);
        if (Input.GetKey(KeyCode.Y)) gHand.transform.Translate(0.0f, 0.0f, adjustDelta);
    }

    void TaskPointing()
    {
        if (gPointingTarget.activeSelf && (gIndexTop.transform.position - gPointingTarget.transform.position).magnitude < 5e-3 && !pointingTargetTouching)
        {
            //Debug.Log("touch " + DateTime.Now);
            pointingTargetTouching = true;
            System.Random random = new System.Random();
            float x = (random.Next(100) - 50) / 110.0f;
            float y = (random.Next(100) - 50) / 110.0f;
            gPointingTarget.transform.localPosition = new Vector3(x * gScreen.transform.localScale.x, y * gScreen.transform.localScale.y, gPointingTarget.transform.localPosition.z);
        }
        else
        {
            pointingTargetTouching = false;
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
