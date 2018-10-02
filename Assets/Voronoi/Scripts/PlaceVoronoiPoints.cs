﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class PlaceVoronoiPoints : MonoBehaviour {
    //prep
    int colorCount = 24;
    Color[] colors = null;
    TransferDataModel[] data;

    // input
    [Header("Colorchord Config")]
    public int ColorchordUdpInputPort;
    UDPColorchordReceiver receiver;

    // LEDS
    [Header("LED Output")]
    public int LEDOutPort;
    public int ledCount;
    UdpClient LEDDataChannel;

    // Shader
    [Header("Shader Output")]
    public float minRadius;
    public float maxRadius;
    public float circles;
    public bool ShowDebugSpheres = false;
    public GameObject PointPrefab = null;
    MeshRenderer mr = null;
    float twoTimesPi = 2 * Mathf.PI;

    IPEndPoint LEDdestination;

    // Use this for initialization
    void Start() {
        if (colors == null || colors.Length == 0) {
            colors = new Color[colorCount];

            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Color.HSVToRGB(i / (float)colorCount, 1, 0.7f);
            }
        }

        receiver = new UDPColorchordReceiver(ColorchordUdpInputPort, colorCount);
        receiver.Start();

        LEDdestination = new IPEndPoint(IPAddress.Loopback, LEDOutPort);
        LEDDataChannel = new UdpClient(LEDOutPort+1);

        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {
        DataPreperator();
        SendLedData();
        SendShaderData();
    }

    void DataPreperator() {
        float amplitudesSum, amplitudesMax, h, s, v;
        float[] rawData = receiver.latestData;

        amplitudesSum = 0;
        amplitudesMax = 0;
        data = new TransferDataModel[rawData.Length];

        for (int i = 0; i < rawData.Length; i++) {
            amplitudesSum += rawData[i];
            amplitudesMax = Mathf.Max(amplitudesMax, rawData[i]);
        }

        for (int i = 0; i < rawData.Length; i++) {
            Color.RGBToHSV(colors[i], out h, out s, out v);
            v = Mathf.Pow((rawData[i] / amplitudesMax), 3.0f); // highest amp in this frame gets value 1
            rawData[i] /= amplitudesSum;

            data[i] = new TransferDataModel(h, s, v, rawData[i]);
        }
    }

    void SendLedData() {
        byte[] outData = new byte[ledCount * 3]; //RGB for every Led in 0..255
        int ledsUsed = 0;
        int leds;
        for (int i = 0; i < data.Length; i++) {
            if (i < data.Length - 1) {
                leds = (int)Mathf.Round(data[i].width * ledCount);
            }
            else {
                leds = ledCount - ledsUsed;
            }
            for (int j = 0; j < leds; j++) {
                
                outData[3 * (ledsUsed + j)] = (byte)(255 * data[i].color.r);
                outData[3 * (ledsUsed + j) + 1] = (byte)(255 * data[i].color.g);
                outData[3 * (ledsUsed + j) + 2] = (byte)(255 * data[i].color.b);

            }
            ledsUsed += leds;
        }

        LEDDataChannel.SendAsync(outData,outData.Length, LEDdestination);
    }

    void SendShaderData() {
        float[] voronoiPoints = new float[data.Length * 5];
        Vector3 coords = new Vector3();
        float usedCirclePart = 0;
        float radius, partOfCircle;

        if (ShowDebugSpheres) {
            for (int i = 0; i < transform.childCount; i++) {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < data.Length; i++) {
            partOfCircle = usedCirclePart + (0.5f * data[i].width);
            radius = minRadius * (1 - partOfCircle) + maxRadius * (partOfCircle);
            coords = new Vector3(Mathf.Cos(partOfCircle * circles * twoTimesPi) * radius, Mathf.Sin(partOfCircle * circles * twoTimesPi) * radius, 0);
            usedCirclePart += data[i].width;

            voronoiPoints[5 * i] = coords.x;
            voronoiPoints[5 * i + 1] = coords.y;
            voronoiPoints[5 * i + 2] = data[i].color.r;
            voronoiPoints[5 * i + 3] = data[i].color.g;
            voronoiPoints[5 * i + 4] = data[i].color.b;

            if (ShowDebugSpheres) {
                var Point = Instantiate<GameObject>(PointPrefab, transform);
                Point.transform.localPosition = coords;
                Point.GetComponent<MeshRenderer>().material.color = new Color(voronoiPoints[5 * i + 2], voronoiPoints[5 * i + 3], voronoiPoints[5 * i + 4], 1);
            }
        }

        mr.material.SetFloatArray("voronoiPoints", voronoiPoints);
        mr.material.SetInt("_NUMOFPOINTS", data.Length);
    }

    private void OnDestroy() {
        receiver.Stop();
        LEDDataChannel.Close();
    }
}