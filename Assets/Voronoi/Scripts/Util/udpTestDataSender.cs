using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class udpTestDataSender : MonoBehaviour {
    UdpClient client;
    float[] data;
    bool running;
    // Use this for initialization
    void Start () {
        running = true;
        data = new float[12];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0.1f;
        }
        client = new UdpClient(5512);
        StartCoroutine("StepChangeAmplitudes");
    }
	
	// Update is called once per frame
	void Update () {
        byte[] outArr = new byte[data.Length * 4];

        Buffer.BlockCopy(data, 0, outArr, 0, outArr.Length);

        client.Send(outArr, outArr.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 5518));
	}

    IEnumerator StepChangeAmplitudes()
    {
        while (running)
        {
            // pick random amp
            int ampIndex = UnityEngine.Random.Range(0, 12);
            int duration = UnityEngine.Random.Range(10, 30);
            // random end value
            float endValue = UnityEngine.Random.value;
            float startValue = data[ampIndex];
            // loop from current to end value
            for (int i = 0; i < duration; i++)
            {
                if (!running) break;
                float samplePosition = i / (float)duration;
                data[ampIndex] = startValue * (1 - samplePosition) + endValue * samplePosition;
                yield return null;
            }
        }
    }

    private void OnDestroy()
    {
        running = false;
    }
}
