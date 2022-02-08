using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using UnityEngine.XR;
using Varjo.XR;
using UnityEditor;

public enum GazeDataSource
{
    InputSubsystem,
    GazeAPI
}

public class EyeTrackingExample : MonoBehaviour
{
    [Header("Gaze data")]
    public GazeDataSource gazeDataSource = GazeDataSource.InputSubsystem;

    [Header("Visualization Transforms")]

    public Transform leftEyeTransform;
    public Transform rightEyeTransform;
    public Transform fixationPointTransform;

    [Header("XR camera")]
    public Camera xrCamera;

    [Header("Gaze point indicator")]
    public GameObject gazeTarget;

    [Header("Gaze ray radius")]
    public float gazeRadius = 0.01f;

    [Header("Gaze point distance if not hit anything")]
    public float floatingGazeTargetDistance = 5f;

    [Header("Gaze target offset towards viewer")]
    public float targetOffset = 0.2f;

    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    private Eyes eyes;
    private VarjoEyeTracking.GazeData gazeData;
    private Vector3 leftEyePosition;
    private Vector3 rightEyePosition;
    private Quaternion leftEyeRotation;
    private Quaternion rightEyeRotation;
    private Vector3 fixationPoint;
    private Vector3 direction;
    private Vector3 rayOrigin;
    private RaycastHit hit;
    private float distance;
    

    private string filePath;
    private StreamWriter writer;
    private Stopwatch stopwatch;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.CenterEye, devices);
        device = devices.FirstOrDefault();
    }


    void Start()
    {

        if (!device.isValid)
        {
            GetDevice();
        }

        filePath = Application.dataPath + "/CSV" + "eyeData.csv";
        writer = new StreamWriter(filePath);
        writer.WriteLine("Time,Gaze");
        //Hiding the gazetarget if gaze is not available or if the gaze calibration is not done
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            gazeTarget.SetActive(true);
        }
        else
        {
            gazeTarget.SetActive(false);
        }
        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    void Update()
    {
        gazeData = VarjoEyeTracking.GetGaze();

        if (gazeData.status != VarjoEyeTracking.GazeStatus.Invalid)
        {
            // GazeRay vectors are relative to the HMD pose so they need to be transformed to world space
            if (gazeData.leftStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
            {
                leftEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.left.origin);
                leftEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.left.forward));
            }

            if (gazeData.rightStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
            {
                rightEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.right.origin);
                rightEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.right.forward));
            }

            // Set gaze origin as raycast origin
            rayOrigin = xrCamera.transform.TransformPoint(gazeData.gaze.origin);

            // Set gaze direction as raycast direction
            direction = xrCamera.transform.TransformDirection(gazeData.gaze.forward);

            // Fixation point can be calculated using ray origin, direction and focus distance
            fixationPointTransform.position = rayOrigin + direction * gazeData.focusDistance;
        }

        if (Physics.SphereCast(rayOrigin, gazeRadius, direction, out hit))
        {
            writer.WriteLine(stopwatch.Elapsed + "," + hit.collider);
        }
        
    }
}
