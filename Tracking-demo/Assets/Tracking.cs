#define GET_FRAME_FROM_FILE

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class Tracking : MonoBehaviour
{
    const int MARKER_NUM = 30;
    const int MARKER_NUM_EXPECT = 19;
    const float MATCH_THRESHOLD = 1e-2f;

    StreamReader reader;
    Frame now;
    Frame last;
    int frameID = -1;

    void Start()
    {
#if GET_FRAME_FROM_FILE
        reader = new StreamReader(new FileStream("Take 2016-04-18 04.34.30 PM.csv", FileMode.Open));
        for (int i = 0; i < 7; i++)
        {
            reader.ReadLine();
        }
#endif
    }

    void Update ()
    {
        frameID++;
        if (frameID > 3637) return;
        if (!GetFrame()) return;
        if (frameID == 0 || frameID - last.rb[0].t > 10)
        {
            if (now.n == MARKER_NUM_EXPECT)
            {
                Debug.Log("register");
                Register();
            }
            else
            {
                Debug.Log("giveup");
                return;
            }
        }
        else
        {
            Debug.Log("refering");
            Refering();
        }
        last = now;
        Draw();
    }

    bool GetFrame()
    {
#if GET_FRAME_FROM_FILE
        string line = reader.ReadLine();
#endif
        string[] lineArray = line.Split(',');
        now = new Frame();

        /* get rb */
        now.rb[0] = new Marker(GetVector3FromArray(lineArray, 6), frameID);
        for (int i = 1; i <= 3; i++)
        {
            now.rb[i] = new Marker(GetVector3FromArray(lineArray, 6 + i * 4), frameID);
        }
        if (now.rb[0].p == Vector3x.nan)
        {
            Debug.Log("rigidbody miss!");
            return false;
        }

        /* get other markers */
        for (int i = 22; i < lineArray.Length; i += 3)
        {
            Vector3 p = GetVector3FromArray(lineArray, i);
            if (p == Vector3x.nan) continue;
            bool inRbs = false;
            for (int j = 1; j <= 3; j++)
            {
                if (Vector3x.Equal(p, now.rb[j].p)) inRbs = true;
            }
            if (inRbs) continue;
            now.Add(new Marker(p, frameID));
        }
        Debug.Log("frameID: " + frameID + ", marker: " + now.n);
        return true;
    }
    Vector3 GetVector3FromArray(string[] arr, int idx)
    {
        Vector3 p = Vector3x.nan;
        if (arr[idx] != "")
        {
            p = new Vector3();
            p.x = float.Parse(arr[idx + 0]);
            p.y = float.Parse(arr[idx + 1]);
            p.z = float.Parse(arr[idx + 2]);
        }
        return p;
    }

    void Draw()
    {
        DrawMarker(0, now.rb[0].p, Color.black);
        for (int i = 1; i <= 3; i++)
        {
            DrawMarker(i, now.rb[i].p, Color.gray);
        }
        for (int i = 0; i < now.n; i++)
        {
            Color nowColor = Color.white;
            switch (i / 4)
            {
                case 0:
                    nowColor = Color.red;
                    break;
                case 1:
                    nowColor = Color.yellow;
                    break;
                case 2:
                    nowColor = Color.green;
                    break;
                case 3:
                    nowColor = Color.blue;
                    break;
            }
            DrawMarker(i + 4, now.pl[i].p, nowColor);
        }
        for (int i = now.n + 4; i < MARKER_NUM; i++)
        {
            DrawMarker(i, new Vector3(0, 0, 0), Color.white);
        }
    }
    void DrawMarker(int id, Vector3 p, Color color)
    {
        GameObject sphere = GameObject.Find("Sphere (" + id.ToString() + ")");
        Renderer r = sphere.GetComponent<Renderer>();
        r.material.color = color;
        float x = p.x * 10 - sphere.transform.position.x;
        float y = p.y * 10 - sphere.transform.position.y;
        float z = p.z * 10 - sphere.transform.position.z;
        sphere.transform.Translate(x, y, z, Space.World);
    }

    void Register()
    {
        /* init and get normal vector */
        int[] arr = new int[now.n];
        for (int i = 0; i < now.n; i++) arr[i] = i;
        Vector3 nv = Vector3x.NormalVector(now.rb[1].p, now.rb[2].p, now.rb[3].p);

        /* get angle and dist */
        float[] angle = new float[now.n];
        float[] dist = new float[now.n];
        for (int i = 0; i < now.n; i++)
        {
            Vector3 pv = Vector3x.ProjectiveVector(nv, now.pl[i].p - now.rb[0].p);
            angle[i] = Vector3.Angle(pv, now.rb[1].p - now.rb[0].p);
            dist[i] = (now.pl[i].p - now.rb[0].p).magnitude;
        }

        /* sort by finger then sort by dist per finger */
        Array.Sort(arr, (int a, int b) => { return angle[a] < angle[b] ? -1 : 1; });
        for (int finger = 0; finger < 5; finger++)
        {
            int jointN = (finger == 4) ? 3 : 4;
            int[] arr2 = new int[jointN];
            for (int i = 0; i < jointN; i++) arr2[i] = arr[finger * 4 + i];
            Array.Sort(arr2, (int a, int b) => { return dist[a] < dist[b] ? -1 : 1; });
            for (int i = 0; i < jointN; i++) arr[finger * 4 + i] = arr2[i];
        }

        /* renew */
        List<Marker> pl2 = new List<Marker>();
        for (int i = 0; i < now.n; i++)
        {
            pl2.Add(now.pl[arr[i]]);
        }
        now.pl = pl2;
    }

    void Refering()
    {
        /* matching */
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
                //if (weight[i, k] > MATCH_THRESHOLD) break;
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