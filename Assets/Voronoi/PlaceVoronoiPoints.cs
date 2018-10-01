using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataModel
{
    public float notePosition;
    public float amplitude;

    public DataModel(float notePosition, float amplitude)
    {
        this.notePosition = notePosition;
        this.amplitude = amplitude;
    }
}

public class DataModelBins
{
    public int index;
    public float amplitude;

    public DataModelBins(int index, float amplitude)
    {
        this.index = index;
        this.amplitude = amplitude;
    }
}

public class PlaceVoronoiPoints : MonoBehaviour {
    public float fullCircles = 3;
    int pointCount = 24;
    public float minRadius = 0.2f;
    public float maxRadius = 0.5f;
    public float epsilon = 0.5f;
    public int topBins = 5;
    public int port = 5518;

    public Color[] colors = null;

    float[] rawdata;
    float scope;
    public bool ShowDebugSpheres = false;
    public GameObject PointPrefab = null;
    float coiunter = 0;
    private bool running = true;
    MeshRenderer mr = null;
    private UDPColorchordReceiver receiver;

    // Use this for initialization
    void Start() {
        if (colors == null || colors.Length == 0)
        {
            colors = new Color[pointCount];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.HSVToRGB(i / (float)pointCount, 1, 0.7f);
            }
        }

        receiver = new UDPColorchordReceiver(port);
        receiver.Start();

        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rawdata = receiver.latestData;

        scope = 2 * Mathf.PI * fullCircles;

        placePointsFoldedBinsTop5();
    }

    void placePointsFoldedBinsTop5()
    {
        float amplitudesSum = 0, amplitudesMax = 0;
        float[] voronoiPoints = new float[rawdata.Length * 5];
        Vector3 coords = new Vector3();
        float usedCirclePart = 0;
        float h, s, v;
        List<DataModelBins> data = new List<DataModelBins>(rawdata.Length);



        if (ShowDebugSpheres)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < rawdata.Length; i++)
        {
            if (rawdata[i] > epsilon || rawdata[i] < -epsilon) // != 0
                data.Add(new DataModelBins(i, rawdata[i]));
        }

        int bins = Math.Min(topBins, data.Count);
        amplitudesSum = 0;
        for (int i = 0; i < bins; i++)
        {
            amplitudesSum += data[i].amplitude;
            amplitudesMax = Mathf.Max(amplitudesMax, data[i].amplitude);
        }
        usedCirclePart = 0;
        for (int i = 0; i < bins; i++)
        {
            data[i].amplitude /= amplitudesSum;

            coords = calcAmplitudesToCoords(usedCirclePart + (0.5f * data[i].amplitude));
            usedCirclePart += data[i].amplitude;

            Color.RGBToHSV(colors[data[i].index], out h, out s, out v);
            v = rawdata[data[i].index] / amplitudesMax; // highest amp in this frame gets value 1
            v = Mathf.Pow(v, 3.0f);
            Color col = Color.HSVToRGB(h, s, v);

            voronoiPoints[5 * i] = coords.x;
            voronoiPoints[5 * i + 1] = coords.y;
            voronoiPoints[5 * i + 2] = col.r;
            voronoiPoints[5 * i + 3] = col.g;
            voronoiPoints[5 * i + 4] = col.b;

            if (ShowDebugSpheres)
            {
                var Point = Instantiate<GameObject>(PointPrefab, transform);
                Point.transform.localPosition = coords;
                Point.GetComponent<MeshRenderer>().material.color = new Color(voronoiPoints[5 * i + 2], voronoiPoints[5 * i + 3], voronoiPoints[5 * i + 4], 1);
            }
        }
        mr.material.SetFloatArray("voronoiPoints", voronoiPoints);
        mr.material.SetInt("_NUMOFPOINTS", bins);
    }

    Vector3 calcAmplitudesToCoords(float partOfCircle){
        float radius = minRadius * (1 - partOfCircle) + maxRadius * (partOfCircle);
        return new Vector3(Mathf.Cos(partOfCircle * scope) * radius, Mathf.Sin(partOfCircle * scope) * radius, 0);
    }

    private void OnDestroy()
    {
        receiver.Stop();
    }
}
