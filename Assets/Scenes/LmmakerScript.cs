using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using toio;

public class LmmakerScript : MonoBehaviour
{
    public int numCubes = 2;
    public ConnectType connectType;

    int mode = 0;  // 0:stop, 1:record, 2:play
    int[] phase;
    List<Vector2>[] pos;

    float intervalTime = 0.1f;
    float elapsedTime = 0.0f;

    CubeManager cm;

    async void Start()
    {
        phase = new int[numCubes];
        pos = new List<Vector2>[numCubes];
        cm = new CubeManager(connectType);
        await cm.MultiConnect(numCubes);
        foreach(var handle in cm.handles)
        {
            handle.borderRect = new RectInt(34, 34, 949, 898);
        }
        for(int i = 0; i < numCubes; i++)
        {
            pos[i] = new List<Vector2>();
            phase[i] = 0;
        }
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= intervalTime && cm.synced)
        {
            elapsedTime = 0.0f;
            for(int i = 0; i < cm.handles.Count; i++)
            {
                switch(mode)
                {
                    case 0:  // stop
                        phase[i] = 0;
                        if(pos[i].Count > 0)
                            cm.handles[i].Move2Target(pos[i][0]).Exec();
                        break;
                    case 1:  // record
                        pos[i].Add(cm.handles[i].cube.pos);
                        break;
                    case 2:  // play
                        if(phase[i] < pos[i].Count - 1)
                        {
                            phase[i]++;
                            cm.handles[i].Move2Target(pos[i][phase[i]]).Exec();
                        }
                        break;
                }
            }
        }
    }

    public void Rec() { mode = 1; }
    public void Play() { mode = 2; }
    public void Stop() { mode = 0; }
    public void Clear()
    {
        mode = 0;
        for(int i = 0; i < numCubes; i++)
            pos[i].Clear();
    }
}
