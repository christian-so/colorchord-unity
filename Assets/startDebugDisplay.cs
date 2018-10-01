using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startDebugDisplay : MonoBehaviour {
    public GameObject DebugSender = null;
	// Use this for initialization
	void Start () {
        string[] args = Environment.GetCommandLineArgs();

        foreach (var a in args)
        {
            if(a == "-debugVoronoi")
            {
                DebugSender.SetActive(true);
                break;
            }
        }
	}
}
