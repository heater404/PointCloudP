using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class RecvFPS : MonoBehaviour
{
    Communication comm;
    float fixTime = 0;
    UInt32 updateFPS = 0;
    Text fpsText;
    // Start is called before the first frame update
    void Start()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
        fpsText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        updateFPS++;
    }

    private void FixedUpdate()
    {
        fixTime += Time.fixedDeltaTime;
        if (fixTime >= 1)//UpdataCnt等于50的时候条件不成立,0-49的时候条件成立(1s)
        {
            fpsText.text = $"FPS:{comm.RecvDepthCnt}";//物理50帧(1s)收到的帧数和 /{updateFPS}
            Interlocked.Exchange(ref comm.RecvDepthCnt, 0);
            fixTime = 0;
            updateFPS = 0;
        }
    }
}
