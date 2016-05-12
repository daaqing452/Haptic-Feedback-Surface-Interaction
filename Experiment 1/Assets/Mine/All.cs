using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;

public class All : MonoBehaviour {

    //  Game objects
    GameObject gHand;
    GameObject gIndexTop;
    GameObject gDisplay;
    GameObject gScreen;
    GameObject gClickTarget;
    GameObject gDragSource;
    GameObject gDragTarget;
    GameObject gZoomSource;
    GameObject gZoomTarget;

    //  Network
    TcpClient socket = new TcpClient();
    const string serverIP = "192.168.1.159";
    const int serverPort = 7643;
    
    //  Scene
    const float ADJUST_DELTA = 0.002f;
    const float TOUCH_DIST_Z = 0.005f;
    List<Vector3> poss = new List<Vector3>();
    string task = "drag";

    //  Task click
    int clickPos;

    //  Task drag
    const float DRAG_SOURCE_TARGET_DIST = 0.01f;
    int dragStep = 0;
    int dragTargetPos;
    DateTime dragTime;
    Vector3 dragDist;

    //  Tracking
    const int MAX_MARKER_NUM = 30;
    int frameID = -1;
    Frame now;
    Frame last;

    void Start () {
        Application.targetFrameRate = 2;
        gHand = GameObject.Find("hand_right_prefab");
        gIndexTop = GameObject.Find("index_top_r");
        gDisplay = GameObject.Find("display");
        gScreen = GameObject.Find("screen");

        gClickTarget = GameObject.Find("click target");
        gDragSource = GameObject.Find("drag source");
        gDragTarget = GameObject.Find("drag target");
        gZoomSource = GameObject.Find("zoom source");
        gZoomTarget = GameObject.Find("zoom target");

        for (int i = -2; i <= 2; i++)
            for (int j = -1; j <= 1; j++)
            {
                float x = i / 6.0f * gScreen.transform.localScale.x;
                float y = j / 3.0f * gScreen.transform.localScale.y;
                poss.Add(new Vector3(x, y, 0.0f));
            }
        
        gClickTarget.SetActive(false);
        gDragSource.SetActive(false);
        gDragTarget.SetActive(false);
        gZoomSource.SetActive(false);
        gZoomTarget.SetActive(false);
        switch (task)
        {
            case "click":
                gClickTarget.SetActive(true);
                clickPos = LocateOnScreen(gClickTarget, -1);
                break;
            case "drag":
                gDragSource.SetActive(true);
                gDragTarget.SetActive(true);
                int i = LocateOnScreen(gDragSource, -1);
                dragTargetPos = LocateOnScreen(gDragTarget, i);
                break;
            case "zoom":
                gZoomSource.SetActive(true);
                gZoomTarget.SetActive(true);
                break;
        }
    }
    
    void Update()
    {
        KeyboardEvent();
        TaskClick();
        TaskDrag();
        TaskZoom();
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
        if (Input.GetKey(KeyCode.I)) gDisplay.transform.Translate(0.0f, ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.K)) gDisplay.transform.Translate(0.0f, -ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.J)) gDisplay.transform.Translate(-ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.L)) gDisplay.transform.Translate(ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.U)) gDisplay.transform.Translate(0.0f, 0.0f, ADJUST_DELTA);
        if (Input.GetKey(KeyCode.O)) gDisplay.transform.Translate(0.0f, 0.0f, -ADJUST_DELTA);

        //  adjust hand
        if (Input.GetKey(KeyCode.W)) gHand.transform.Translate(0.0f, ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.S)) gHand.transform.Translate(0.0f, -ADJUST_DELTA, 0.0f);
        if (Input.GetKey(KeyCode.A)) gHand.transform.Translate(-ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.D)) gHand.transform.Translate(ADJUST_DELTA, 0.0f, 0.0f);
        if (Input.GetKey(KeyCode.Q)) gHand.transform.Translate(0.0f, 0.0f, ADJUST_DELTA);
        if (Input.GetKey(KeyCode.E)) gHand.transform.Translate(0.0f, 0.0f, -ADJUST_DELTA);
    }

    void TaskClick()
    {
        if (!gClickTarget.activeSelf) return;
        if (IsTouch(gClickTarget, gIndexTop, 4))
        {
            Debug.Log("click " + DateTime.Now);
            clickPos = LocateOnScreen(gClickTarget, clickPos);
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
                    if (DateTime.Now - dragTime > TimeSpan.FromSeconds(0.1))
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
                int i = LocateOnScreen(gDragSource, dragTargetPos);
                dragTargetPos = LocateOnScreen(gDragTarget, i);
            }
            dragStep = 0;
            gDragSource.GetComponent<Renderer>().material.color = colorIdle;
        }
    }

    void TaskZoom()
    {

    }

    bool IsTouch(GameObject g0, GameObject g1, float b = 2)
    {
        float distX = Math.Abs(g0.transform.position.x - g1.transform.position.x) * b;
        float distY = Math.Abs(g0.transform.position.y - g1.transform.position.y) * b;
        float distZ = Math.Abs(g0.transform.position.z - g1.transform.position.z);
        return (distX < g0.transform.lossyScale.x) && (distY < g0.transform.lossyScale.y) && (distZ < TOUCH_DIST_Z);
    }

    int LocateOnScreen(GameObject g, int banned)
    {
        System.Random random = new System.Random(DateTime.Now.Millisecond);
        int i = random.Next() % poss.Count;
        while (i == banned)
        {
            i = random.Next() % poss.Count;
        }
        g.transform.localPosition = new Vector3(poss[i].x, poss[i].y, g.transform.localPosition.z);
        return i;
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

    void Register()
    {
        // init and get normal vector
        int[] arr = new int[now.n];
        for (int i = 0; i < now.n; i++) arr[i] = i;
        Vector3 nv = Vector3x.NormalVector(now.rb[1].p, now.rb[2].p, now.rb[3].p);

        // get angle and dist
        float[] angle = new float[now.n];
        float[] dist = new float[now.n];
        for (int i = 0; i < now.n; i++)
        {
            Vector3 pv = Vector3x.ProjectiveVector(nv, now.pl[i].p - now.rb[0].p);
            angle[i] = Vector3.Angle(pv, now.rb[1].p - now.rb[0].p);
            dist[i] = (now.pl[i].p - now.rb[0].p).magnitude;
        }

        // sort by finger then sort by dist per finger
        Array.Sort(arr, (int a, int b) => { return angle[a] < angle[b] ? -1 : 1; });
        for (int finger = 0; finger < 5; finger++)
        {
            int jointN = (finger == 4) ? 3 : 4;
            int[] arr2 = new int[jointN];
            for (int i = 0; i < jointN; i++) arr2[i] = arr[finger * 4 + i];
            Array.Sort(arr2, (int a, int b) => { return dist[a] < dist[b] ? -1 : 1; });
            for (int i = 0; i < jointN; i++) arr[finger * 4 + i] = arr2[i];
        }

        // renew
        List<Marker> pl2 = new List<Marker>();
        for (int i = 0; i < now.n; i++)
        {
            pl2.Add(now.pl[arr[i]]);
        }
        now.pl = pl2;
    }

    void Refering()
    {
        // matching
        NetworkFlow networkFlow = new NetworkFlow(last.n, now.n);
        float[,] weight = new float[now.n, last.n];
        for (int i = 0; i < now.n; i++)
        {
            int[] arr = new int[last.n];
            for (int j = 0; j < last.n; j++)
            {
                weight[i, j] = (now.pl[i].p - last.pl[j].p).magnitude;
                arr[j] = j;
            }
            Array.Sort(arr, (int a, int b) => { return weight[i, a] < weight[i, b] ? -1 : 1; });
            for (int j = 0; j < Math.Min(last.n, 5); j++)
            {
                int k = arr[j];
                networkFlow.AddEdge(k, i, weight[i, k]);
            }
        }
        int[] match = networkFlow.Solve();
        Frame fix = new Frame(now.rb);
        for (int i = 0; i < last.n; i++)
        {
            if (match[i] == -1)
            {
                fix.Add(last.pl[i]);
            }
            else
            {
                fix.Add(now.pl[match[i]]);
            }
        }
        now = fix;
        //now.CheckQuality(frameID);
    }
}


class Vector3x
{
    public static Vector3 nan = Vector3.one * 1e20f;

    public static bool Equal(Vector3 a, Vector3 b)
    {
        if ((a - b).magnitude < 4e-3) return true;
        return false;
    }

    public static Vector3 NormalVector(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a);
    }

    public static Vector3 ProjectiveVector(Vector3 nv, Vector3 a)
    {
        float len = Vector3.Dot(nv.normalized, a);
        return a - nv.normalized * len;
    }
}

class Marker
{
    public Vector3 p;
    public int t;

    public Marker(Vector3 p, int t)
    {
        this.p = p;
        this.t = t;
    }
}

class Frame
{
    public Marker[] rb;
    public List<Marker> pl;
    public int n;

    public Frame(Marker[] rb = null, List<Marker> pl = null)
    {
        this.rb = (rb == null) ? (new Marker[4]) : rb;
        this.pl = (pl == null) ? (new List<Marker>()) : pl;
        n = this.pl.Count;
    }

    public void Add(Marker p)
    {
        pl.Add(p);
        n++;
    }

    public void CheckQuality(int t)
    {
        for (int i = 0; i < n; i++)
        {
            Marker marker = pl[i];
            if (t - marker.t > 10) Debug.Log("low quality (" + i + "): " + (t - marker.t));
        }
    }
}

class NetworkFlow
{
    const int INF = 0xfffffff;
    List<int>[] edge;
    List<int> ev;
    List<int> ec;
    List<float> eq;
    int n;
    int nLast;
    int s;
    int t;
    public NetworkFlow(int nLast, int nNow)
    {
        this.nLast = nLast;
        n = nLast + nNow + 2;
        s = n - 2;
        t = n - 1;
        edge = new List<int>[n];
        for (int i = 0; i < n; i++) edge[i] = new List<int>();
        ev = new List<int>();
        ec = new List<int>();
        eq = new List<float>();
        for (int i = 0; i < nLast; i++) AddEdge(s, i, 1, 0);
        for (int i = 0; i < nNow; i++) AddEdge(nLast + i, t, 1, 0);
    }

    public void AddEdge(int u, int v, float q)
    {
        AddEdge(u, nLast + v, 1, q);
    }
    public void AddEdge(int u, int v, int c, float q)
    {
        edge[u].Add(ev.Count);
        ev.Add(v);
        ec.Add(c);
        eq.Add(q);
        edge[v].Add(ev.Count);
        ev.Add(u);
        ec.Add(0);
        eq.Add(-q);
    }

    public int[] Solve()
    {
        while (true)
        {
            float[] dist = new float[n];
            bool[] visit = new bool[n];
            int[] pref = new int[n];
            for (int i = 0; i < n; i++) dist[i] = INF;
            dist[s] = 0;
            visit[s] = true;
            List<int> que = new List<int>();
            que.Add(s);
            for (int quefi = 0; quefi < que.Count; quefi++)
            {
                int u = que[quefi];
                for (int i = 0; i < edge[u].Count; i++)
                {
                    int j = edge[u][i];
                    if (ec[j] <= 0) continue;
                    int v = ev[j];
                    float q = eq[j];
                    if (dist[v] <= dist[u] + q) continue;
                    dist[v] = dist[u] + q;
                    pref[v] = j;
                    if (visit[v]) continue;
                    visit[v] = true;
                    que.Add(v);
                }
                visit[u] = false;
            }
            if (dist[t] >= INF) break;
            for (int i = t; i != s; i = ev[pref[i] ^ 1])
            {
                ec[pref[i]] -= 1;
                ec[pref[i] ^ 1] += 1;
            }
        }
        int[] match = new int[nLast];
        for (int u = 0; u < nLast; u++)
        {
            match[u] = -1;
            for (int i = 0; i < edge[u].Count; i++)
            {
                int j = edge[u][i];
                if (ec[j] == 0 && ev[j] < s)
                {
                    match[u] = ev[j] - nLast;
                    break;
                }
            }
        }
        return match;
    }
}