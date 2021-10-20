using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varjo.XR;

public class EyeTracking : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VarjoEyeTracking.RequestGazeCalibration();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
