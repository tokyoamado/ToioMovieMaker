using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using toio;

public class LmmakerScript : MonoBehaviour
{
    public int numCubes = 2;
    public float samplingTime = 0.25f;
    public float margin = 1.05f;
    public ConnectType connectType;
    public GameObject[] toggles;

    int mode = 0;  // 0:reset, 1:record, 2:play, 3:pause
    int[] phase;
    List<Vector2>[] pos;

    float intervalTime;
    float elapsedTime = 0f;
    int durationTime;

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
        for(int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i].GetComponent<Toggle>();
            toggle.isOn = false;
        }
        for(int i = 0; i < numCubes; i++)
        {
            pos[i] = new List<Vector2>();
            phase[i] = 0;
            Toggle toggle = toggles[i].GetComponent<Toggle>();
            toggle.isOn = true;
        }
    }

    void Update()
    {
        intervalTime = samplingTime;
        durationTime = (int)(samplingTime * 1000f * margin);
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= intervalTime && cm.synced)
        {
            elapsedTime = 0.0f;
            for(int i = 0; i < cm.handles.Count; i++)
            {
                Toggle toggle = toggles[i].GetComponent<Toggle>();
                switch(mode)
                {
                    case 0:  // reset
                        phase[i] = 0;
                        if(pos[i].Count > 0)
                            cm.handles[i].Move2Target(pos[i][0], 50, durationTime, 8).Exec();
                        break;
                    case 1:  // record
                        if(toggle.isOn)
                            pos[i].Add(cm.handles[i].cube.pos);
                        else {
                            if(phase[i] < pos[i].Count - 1) {
                                phase[i]++;
                                cm.handles[i].Move2Target(pos[i][phase[i]], 50, durationTime, 8).Exec();
                            }
                        }
                        break;
                    case 2:  // play
                        if(toggle.isOn && phase[i] < pos[i].Count - 1)
                        {
                            phase[i]++;
                            cm.handles[i].Move2Target(pos[i][phase[i]], 50, durationTime, 8).Exec();
                        }
                        break;
                    case 3:  // pause
                        cm.handles[i].Stop();
                        break;
                }
            }
        }
    }

    public void Rec() { mode = 1; }
    public void Play() { mode = 2; }
    public void Reset() { mode = 0; }
    public void Pause() { mode = 3; }
    public void Clear()
    {
        mode = 0;
        for(int i = 0; i < numCubes; i++) {
            Toggle toggle = toggles[i].GetComponent<Toggle>();
            if(!toggle.isOn) continue;
            pos[i].Clear();
        }
    }

    public void onToggleChange(int i) {
        if(i < numCubes) {
            Toggle toggle = toggles[i].GetComponent<Toggle>();
            cm.handles[i].RotateByDeg(30, 20).Exec();
        }
    }
}
