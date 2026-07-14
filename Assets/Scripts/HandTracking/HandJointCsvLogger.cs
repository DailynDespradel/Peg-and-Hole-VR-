using System;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Hands;

/// <summary>
/// Logs the 3D positions of all tracked hand joints to a CSV file at a fixed sampling interval.
/// A new CSV file is created each time the application starts.
/// </summary>
public class HandJointCsvLogger : MonoBehaviour
{
    [SerializeField] private float sampleInterval = 0.1f;    // Time (in seconds) between consecutive samples.

    private XRHandSubsystem handSubsystem; // Reference to the active XR Hand Tracking subsystem.
    private StreamWriter csvWriter; // Stream used to write data to the CSV file.
    private float sampleTimer; // Timer used to enforce the sampling interval

    private void Start()
    {
        // Find the currently running XR Hand subsystem.
        handSubsystem = GetHandSubsystem();

        if (handSubsystem == null)
        {
            Debug.LogError("[HandJointCsvLogger] XRHandSubsystem was not found");
            enabled = false;
            return;
        }

        string directory = Path.Combine(Application.dataPath, "Data");
        Directory.CreateDirectory(directory);

        string fileName = "HandJointLog_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";
        string filePath = Path.Combine(directory, fileName);

        // Find the currently running XR Hand subsystem.
        csvWriter = new StreamWriter(filePath, append: false);

        //HEADER 
        csvWriter.WriteLine("TimeSec,Hand,JointIndex,PositionX,PositionY,PositionZ,TrackingState");

        Debug.Log("[HandJointCsvLogger] Logging hand joint data to: " + filePath);
    }

    private void Update()
    {
        // Do nothing if logging is unavailable.
        if (csvWriter == null || handSubsystem == null)
            return;
        // Accumulate elapse time 
        sampleTimer += Time.deltaTime;

        if (sampleTimer < sampleInterval)
            return;
        
        //reset timer 
        sampleTimer = 0f;

        LogHand("Left", handSubsystem.leftHand);
        LogHand("Right", handSubsystem.rightHand);

        csvWriter.Flush();
    }

    /// <summary>
    /// Logs the position of every tracked joint for a given hand.
    /// </summary>
    /// <param name="handName">Name written to the CSV ("Left" or "Right").</param>
    /// <param name="hand">XRHand object to log.</param>
    private void LogHand(string handName, XRHand hand)
    {
        if (!hand.isTracked)
            return;

        foreach (XRHandJointID jointID in Enum.GetValues(typeof(XRHandJointID)))
        {
            if (jointID == XRHandJointID.Invalid || jointID == XRHandJointID.EndMarker)
                continue;
            
            //Retrieve the joints
            XRHandJoint joint = hand.GetJoint(jointID);
            
            //Only log joints that have a valid pose 
            if (joint.TryGetPose(out Pose pose))
            {
                Vector3 position = pose.position;

                csvWriter.WriteLine($"{Time.time:F3},{handName},{jointID},{position.x:F6},{position.y:F6},{position.z:F6},{joint.trackingState}");
            }
        }
    }

    /// <summary>
    /// Finds the active XR Hand subsystem.
    /// </summary>
    /// <returns>The running XRHandSubsystem, or the first available subsystem if none are running.</returns>
    private XRHandSubsystem GetHandSubsystem()
    {
        //retrive all XR hand subsystems resgistered in the project 
        var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);

        //return the first subsystem that is currently running 
        foreach (XRHandSubsystem subsystem in subsystems)
        {
            if (subsystem.running)
            {
                return subsystem;
            }
        }
        return subsystems.Count > 0 ? subsystems[0] : null;
    }

    private void OnDestroy()
    {
        //close the CSV file when the object is destroyed 
        if (csvWriter != null)
        {
            csvWriter.Flush();
            csvWriter.Close();
            csvWriter = null;
        }
        
        // refresh the unity asset database so the new CSV appears immendiately in the Editor 
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

}
