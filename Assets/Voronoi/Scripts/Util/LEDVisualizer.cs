using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class LEDVisualizer : MonoBehaviour {
    public int LEDCount;
    public int LEDInputPort;
    public GameObject LEDPrefab =null;
    public Camera spectator = null;
    MeshRenderer[] leds;
    UdpClient ledColorInput;
    IPEndPoint iPEndPoint;
    bool running = true;

    byte[] inputBuffer;
    readonly object bufferLock = new object();

	// Use this for initialization
	void Start () {
        ledColorInput = new UdpClient(LEDInputPort);
        iPEndPoint = new IPEndPoint(IPAddress.Loopback, LEDInputPort);

        ledColorInput.BeginReceive(new AsyncCallback(receiveData), null);

        Debug.Log(bytes2Color(0xFF, 0x00, 0xAA).ToString());

        inputBuffer = new byte[3 * LEDCount];

        leds = new MeshRenderer[LEDCount];
        float half_fieldOfView_horizontal = spectator.fieldOfView * (Screen.width / (float)Screen.height) * 0.5f;
        float distance_to_cam = (0.1f * LEDCount * 0.5f) / Mathf.Atan(half_fieldOfView_horizontal * Mathf.Deg2Rad);
        spectator.farClipPlane = distance_to_cam * 1.2f;

        for (int i = 0; i < LEDCount; i++) {
            GameObject led = Instantiate<GameObject>(LEDPrefab, transform);
            led.transform.localPosition = new Vector3(-0.1f * LEDCount / 2.0f + 0.1f * i, 0, distance_to_cam);
            leds[i] = led.GetComponent<MeshRenderer>();
        }
	}
	
    void receiveData(IAsyncResult res) {
        byte[] arr = ledColorInput.EndReceive(res, ref iPEndPoint);
        Debug.Log("received " + arr.Length);
        if(arr.Length == LEDCount * 3) {
            lock(bufferLock) {
                inputBuffer = (byte[])arr.Clone();
            }
        }
        else {
            Debug.LogError("LED data received was of wrong length: " + arr.Length + ", should have been: " + LEDCount * 3);
        }

        if(running) ledColorInput.BeginReceive(new AsyncCallback(receiveData), null);
    }

	// Update is called once per frame
	void Update () {
        byte[] ledColors;
        Color col;
        lock (bufferLock) {
            ledColors = (byte[])inputBuffer.Clone();
        }
        for (int i = 0; i < LEDCount; i++) {
            col = new Color32(ledColors[i * 3], ledColors[i * 3 + 1], ledColors[i * 3 + 2], 0xFF);
            leds[i].material.color = col;
        }
	}

    Color bytes2Color(byte _r, byte _g, byte _b) {
        float r = 0, g = 0, b = 0;
        r = _r / 255.0f;
        g = _g / 255.0f;
        b = _b / 255.0f;
        return new Color(r,g,b);
    }

    private void OnDestroy() {
        running = false;
    }
}
