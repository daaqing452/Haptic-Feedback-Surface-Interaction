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
    GameObject gClickTarget;
    GameObject gDragSource;
    GameObject gDragTarget;

    //  Network
    TcpClient socket = new TcpClient();
    const string serverIP = "192.168.1.159";
    const int serverPort = 7643;

    //  constant
    const float ADJUST_DELTA = 0.001f;
    const float TOUCH_DIST_Z = 0.005f;
    const float DRAG_SOURCE_TARGET_DIST = 0.01f;

    //  Task
    int clickStep = 0;
    int dragStep = 0;
    DateTime dragTime;
    Vector3 dragDist;

    void Start () {
        gHand = GameObject.Find("hand_right_prefab");
        gIndexTop = GameObject.Find("index_top_r");
        gDisplay = GameObject.Find("display");
        gScreen = GameObject.Find("screen");
        gClickTarget = GameObject.Find("click target");
        gDragSource = GameObject.Find("drag source");
        gDragTarget = GameObject.Find("drag target");
    }
    
    void Update()
    {
        KeyboardEvent();
        TaskClick();
        TaskDrag();
    }

    void KeyboardEvent()
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
        if (Input.GetKey(KeyCode.W)) gDisplay.transform.Translate(0.0f, ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.S)) gDisplay.transform.Translate(0.0f, -ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.A)) gDisplay.transform.Translate(-ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.D)) gDisplay.transform.Translate(ADJUST_DELTA, 0.0f, 0.0f);

        //  adjust hand
        if (Input.GetKey(KeyCode.T)) gHand.transform.Translate(0.0f, ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.G)) gHand.transform.Translate(0.0f, -ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.F)) gHand.transform.Translate(-ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.H)) gHand.transform.Translate(ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.R)) gHand.transform.Translate(0.0f, 0.0f, -ADJUST_DELTA);
        if (Input.GetKey(KeyCode.Y)) gHand.transform.Translate(0.0f, 0.0f, ADJUST_DELTA);
    }

    void TaskClick()
    {
        if (!gClickTarget.activeSelf) return;
        if (IsTouch(gClickTarget, gIndexTop, 4))
        {
            clickStep = 1;
            Debug.Log("click " + DateTime.Now);
            System.Random random = new System.Random();
            RandomPosition(gClickTarget);
        }
        else
        {
            clickStep = 0;
        }
    }

    void TaskDrag()
    {
        if (!gDragSource.activeSelf) return;
        Color colorIdle = new Color(1.00f, 0.58f, 0.58f);
        Color colorDraging = new Color(1.00f, 0.16f, 0.16f);
        if (IsTouch(gDragSource, gIndexTop))
        {
            switch (dragStep)
            {
                case 0:
                    dragStep = 1;
                    dragTime = DateTime.Now;
                    break;
                case 1:
                    if (DateTime.Now - dragTime > TimeSpan.FromSeconds(0.5))
                    {
                        dragStep = 2;
                        Debug.Log("drag source " + dragTime);
                        gDragSource.GetComponent<Renderer>().material.color = colorDraging;
                        dragDist = gDragSource.transform.position - gIndexTop.transform.position;
                    }
                    break;
                case 2:
                    float x = gIndexTop.transform.position.x + dragDist.x;
                    float y = gIndexTop.transform.position.y + dragDist.y;
                    float z = gDragSource.transform.position.z;
                    gDragSource.transform.position = new Vector3(x, y, z);
                    break;
            }
        }
        else
        {
            if ((gDragSource.transform.position - gDragTarget.transform.position).magnitude < DRAG_SOURCE_TARGET_DIST)
            {
                RandomPosition(gDragSource, 0.8f);
                RandomPosition(gDragTarget, 0.8f);
            }
            dragStep = 0;
            gDragSource.GetComponent<Renderer>().material.color = colorIdle;
        }
    }

    bool IsTouch(GameObject g0, GameObject g1, float b = 2)
    {
        float distX = Math.Abs(g0.transform.position.x - g1.transform.position.x) * b;
        float distY = Math.Abs(g0.transform.position.y - g1.transform.position.y) * b;
        float distZ = Math.Abs(g0.transform.position.z - g1.transform.position.z);
        return (distX < g0.transform.lossyScale.x) && (distY < g0.transform.lossyScale.y) && (distZ < TOUCH_DIST_Z);
    }

    void RandomPosition(GameObject g, float k = 0.9f)
    {
        System.Random random = new System.Random(DateTime.Now.Millisecond);
        float x = (random.Next(100) - 50) / 100.0f * k * gScreen.transform.localScale.x;
        float y = (random.Next(100) - 50) / 100.0f * k * gScreen.transform.localScale.y;
        Debug.Log(x + " " + y);
        float z = g.transform.localPosition.z;
        g.transform.localPosition = new Vector3(x, y, z);
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
