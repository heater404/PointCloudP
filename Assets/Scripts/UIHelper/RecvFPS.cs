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
        if (fixTime >= 1)//UpdataCnt����50��ʱ������������,0-49��ʱ����������(1s)
        {
            fpsText.text = $"FPS:{comm.RecvDepthCnt}";//����50֡(1s)�յ���֡���� /{updateFPS}
            Interlocked.Exchange(ref comm.RecvDepthCnt, 0);
            fixTime = 0;
            updateFPS = 0;
        }
    }
}
