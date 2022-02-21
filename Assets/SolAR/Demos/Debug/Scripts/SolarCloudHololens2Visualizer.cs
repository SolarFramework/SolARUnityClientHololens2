/**
 * @copyright Copyright (c) 2021-2022 B-com http://www.b-com.com/
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using SolARRpc = Com.Bcom.Solar.Gprc;
using Bcom.Solar;

public class SolarCloudHololens2Visualizer : MonoBehaviour
{
    [SerializeField]
    public SolArCloudHololens2 solArCloudHololens2;

    // PV 
    [SerializeField]
    GameObject pvPlane;
    [SerializeField]
    TextMeshPro pvText;
    private Material pvMaterial = null;
    private Texture2D pvTexture = null;

    // VLC
    [SerializeField]
    GameObject vlcLeftLeftPlane;
    [SerializeField]
    TextMeshPro vlcLeftLeftText;
    private Material vlcLeftLeftMaterial = null;
    private Texture2D vlcLeftLeftTexture = null;

    [SerializeField]
    GameObject vlcLeftFrontPlane;
    [SerializeField]
    TextMeshPro vlcLeftFrontText;
    private Material vlcLeftFrontMaterial = null;
    private Texture2D vlcLeftFrontTexture = null;

    [SerializeField]
    GameObject vlcRightFrontPlane;
    [SerializeField]
    TextMeshPro vlcRightFrontText;
    private Material vlcRightFrontMaterial = null;
    private Texture2D vlcRightFrontTexture = null;

    [SerializeField]
    GameObject vlcRightRightPlane;
    [SerializeField]
    TextMeshPro vlcRightRightText;
    private Material vlcRightRightMaterial = null;
    private Texture2D vlcRightRightTexture = null;

    // DEPTH
    [SerializeField]
    GameObject depthPlane;
    [SerializeField]
    TextMeshPro depthText;
    private Material depthMaterial = null;
    private Texture2D depthTexture = null;

    [SerializeField]
    GameObject depthABPlane;
    [SerializeField]
    TextMeshPro depthABText;
    private Material depthABMaterial = null;
    private Texture2D depthABTexture = null;


    [SerializeField]
    TextMeshPro ipAddressText;

    [SerializeField]
    TextMeshPro receivedPoseText;

    private int logsNbMaxElements = 10;
    public TextMeshPro unityLogs;
    private Queue<string> unityLogsQueue = new Queue<string>();
    public TextMeshPro gRpcLogs;
    private Queue<string> gRpcLogsQueue = new Queue<string>();
    public TextMeshPro pluginLogs;
    private Queue<string> pluginLogsQueue = new Queue<string>();

    public TextMeshPro nbSentFramesText;

    private UnityEngine.TouchScreenKeyboard keyboard;

    bool debugDisplayEnabled = false;

    private FpsCounter fpsCounter = new FpsCounter();

    public void OnDepthFrame(
        byte[] depthData,
        byte[] depthABData,
        UInt64 ts,
        double[] cam2WorldTransform,
        UInt32 width,
        UInt32 height,
        float fx,
        float fy,
        UInt32 pixelBufferSize
        )
    {
        depthTexture.LoadRawTextureData(depthData);
        depthTexture.Apply();

        depthABTexture.LoadRawTextureData(depthABData);
        depthABTexture.Apply();

        depthText.text =
            "w: " + width + "\n" + "h: " + height + "\n" +
            "VLC2World = [\n" +
            "    " + d2str(cam2WorldTransform[0]) + "," + d2str(cam2WorldTransform[1]) + "," + d2str(cam2WorldTransform[2]) + "," + d2str(cam2WorldTransform[3]) + "\n" +
            "    " + d2str(cam2WorldTransform[4]) + "," + d2str(cam2WorldTransform[5]) + "," + d2str(cam2WorldTransform[6]) + "," + d2str(cam2WorldTransform[7]) + "\n" +
            "    " + d2str(cam2WorldTransform[8]) + "," + d2str(cam2WorldTransform[9]) + "," + d2str(cam2WorldTransform[10]) + "," + d2str(cam2WorldTransform[11]) + "\n" +
            "    " + d2str(cam2WorldTransform[12]) + "," + d2str(cam2WorldTransform[13]) + "," + d2str(cam2WorldTransform[14]) + "," + d2str(cam2WorldTransform[15]) + "\n" +
            "]";
    }

    public void OnVlcFrame(
#if ENABLE_WINMD_SUPPORT
	    SolARHololens2UnityPlugin.RMSensorType sensorType,
#endif
        byte[] vlcData,
        UInt64 ts,
        double[] cam2WorldTransform,
        UInt32 width,
        UInt32 height,
        float fx,
        float fy,
        UInt32 pixelBufferSize
    )
    {
        Texture2D vlcTexture = null;
        TextMeshPro vlcText = null;

#if ENABLE_WINMD_SUPPORT
        switch (sensorType)
        {
            case SolARHololens2UnityPlugin.RMSensorType.LEFT_LEFT:
            {
                vlcTexture = vlcLeftLeftTexture;
                vlcText = vlcLeftLeftText;
                break;
            }
            case SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT:
            {
                vlcTexture = vlcLeftFrontTexture;
                vlcText = vlcLeftFrontText;
                break;
            }
            case SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT:
            {
                vlcTexture = vlcRightFrontTexture;
                vlcText = vlcRightFrontText;
                break;
            }
            case SolARHololens2UnityPlugin.RMSensorType.RIGHT_RIGHT:
            {
                vlcTexture = vlcRightRightTexture;
                vlcText = vlcRightRightText;
                break;
            }
            default:
            {
                throw new ArgumentException("Unkown VLC sensor type");
            }
        }
#endif

        vlcTexture.LoadRawTextureData(vlcData);
        vlcTexture.Apply();

        vlcText.text = 
            "w: " + width + "\n" + "h: " + height + "\n" +
            "VLC2World = [\n" +
            "    " + d2str(cam2WorldTransform[0]) + "," + d2str(cam2WorldTransform[1]) + "," + d2str(cam2WorldTransform[2]) + "," + d2str(cam2WorldTransform[3]) + "\n" +
            "    " + d2str(cam2WorldTransform[4]) + "," + d2str(cam2WorldTransform[5]) + "," + d2str(cam2WorldTransform[6]) + "," + d2str(cam2WorldTransform[7]) + "\n" +
            "    " + d2str(cam2WorldTransform[8]) + "," + d2str(cam2WorldTransform[9]) + "," + d2str(cam2WorldTransform[10]) + "," + d2str(cam2WorldTransform[11]) + "\n" +
            "    " + d2str(cam2WorldTransform[12]) + "," + d2str(cam2WorldTransform[13]) + "," + d2str(cam2WorldTransform[14]) + "," + d2str(cam2WorldTransform[15]) + "\n" +
            "]";

    }

    public void OnPvFrame(
        byte[] pvData,
        UInt64 ts,
        double[] cam2WorldTransform,
        UInt32 width,
        UInt32 height,
        float fx,
        float fy,
        UInt32 pixelBufferSize)
    {
        pvTexture.LoadRawTextureData(pvData);
        pvTexture.Apply();

        pvText.text = "width:" + width + "\n" +
                      "height:" + height + "\n" +
                      "timestamp:" + ts + "\n" +
                      "fx:" + fx + "\n" +
                      "fy:" + fy + "\n" +
                      "PV2World = [\n" +
                      "    " + d2str(cam2WorldTransform[0]) + "," + d2str(cam2WorldTransform[1]) + "," + d2str(cam2WorldTransform[2]) + "," + d2str(cam2WorldTransform[3]) + "\n" +
                      "    " + d2str(cam2WorldTransform[4]) + "," + d2str(cam2WorldTransform[5]) + "," + d2str(cam2WorldTransform[6]) + "," + d2str(cam2WorldTransform[7]) + "\n" +
                      "    " + d2str(cam2WorldTransform[8]) + "," + d2str(cam2WorldTransform[9]) + "," + d2str(cam2WorldTransform[10]) + "," + d2str(cam2WorldTransform[11]) + "\n" +
                      "    " + d2str(cam2WorldTransform[12]) + "," + d2str(cam2WorldTransform[13]) + "," + d2str(cam2WorldTransform[14]) + "," + d2str(cam2WorldTransform[15]) + "\n" +
                      "]";
    }

    public void OnReceivedPose(
        SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
    {
        var receivedPose = result.relocAndMappingResult.Pose;
        if (result.resultStatus.success)
        {
            receivedPoseText.text =
                "Pose status: \n" +
                "confidence: " + result.relocAndMappingResult.Confidence + "\n" +
                receivedPose.M11 + ", " + receivedPose.M12 + ", " + receivedPose.M13 + ", " + receivedPose.M14 + "\n" +
                receivedPose.M21 + ", " + receivedPose.M22 + ", " + receivedPose.M23 + ", " + receivedPose.M24 + "\n" +
                receivedPose.M31 + ", " + receivedPose.M32 + ", " + receivedPose.M33 + ", " + receivedPose.M34 + "\n" +
                receivedPose.M41 + ", " + receivedPose.M42 + ", " + receivedPose.M43 + ", " + receivedPose.M44;
        }
        else
        {
            receivedPoseText.text =
                "Error: " + result.resultStatus.errMessage;
        }
    }

    private class FpsCounter
    {
        private long refreshPeriodInMs = 1000;
        private long nbUpdateCallsSinceLastUpdate = 0;
        private float lastFpsValue = 0;

        private System.Diagnostics.Stopwatch stopWatch =
            new System.Diagnostics.Stopwatch();

        public void Stop()
        {
            stopWatch.Stop();
            Reset();
            lastFpsValue = 0;
        }

        private void Reset()
        {
            stopWatch.Restart();
            nbUpdateCallsSinceLastUpdate = 0;
        }

        public float Update()
        {
            if (!stopWatch.IsRunning)
            {
                stopWatch.Start();
            }

            ++nbUpdateCallsSinceLastUpdate;
            var elapsedTimeSinceLastUpdate = stopWatch.ElapsedMilliseconds;
            
            if (elapsedTimeSinceLastUpdate >= refreshPeriodInMs)
            {
                lastFpsValue = 1000.0f * nbUpdateCallsSinceLastUpdate / elapsedTimeSinceLastUpdate;
                Reset();
            }

            return lastFpsValue;
        }
    }

    public void OnSentFrame(long nbSentFrames)
    {
        nbSentFramesText.text = nbSentFrames + " (" + fpsCounter.Update() + " FPS)";
    }

    public void ToggleDebugDisplay()
    {
        debugDisplayEnabled = !debugDisplayEnabled;

        pvText.enabled = debugDisplayEnabled;
        vlcLeftLeftText.enabled = debugDisplayEnabled;
        vlcLeftFrontText.enabled = debugDisplayEnabled;
        vlcRightFrontText.enabled = debugDisplayEnabled;
        vlcRightRightText.enabled = debugDisplayEnabled;
        receivedPoseText.enabled = debugDisplayEnabled;

        gRpcLogs.enabled = debugDisplayEnabled;
        unityLogs.enabled = debugDisplayEnabled;
        pluginLogs.enabled = debugDisplayEnabled;
    }

    // Start is called before the first frame update
    void Start()
    {
        pvMaterial = pvPlane.GetComponent<MeshRenderer>().material;
        pvTexture = new Texture2D(760, 428, TextureFormat.BGRA32, false);
        pvMaterial.mainTexture = pvTexture;

        vlcLeftLeftMaterial = vlcLeftLeftPlane.GetComponent<MeshRenderer>().material;
        vlcLeftLeftTexture = new Texture2D(640, 480, TextureFormat.R8, false);
        vlcLeftLeftMaterial.mainTexture = vlcLeftLeftTexture;

        vlcLeftFrontMaterial = vlcLeftFrontPlane.GetComponent<MeshRenderer>().material;
        vlcLeftFrontTexture = new Texture2D(640, 480, TextureFormat.R8, false);
        vlcLeftFrontMaterial.mainTexture = vlcLeftFrontTexture;

        vlcRightFrontMaterial = vlcRightFrontPlane.GetComponent<MeshRenderer>().material;
        vlcRightFrontTexture = new Texture2D(640, 480, TextureFormat.R8, false);
        vlcRightFrontMaterial.mainTexture = vlcRightFrontTexture;

        vlcRightRightMaterial = vlcRightRightPlane.GetComponent<MeshRenderer>().material;
        vlcRightRightTexture = new Texture2D(640, 480, TextureFormat.R8, false);
        vlcRightRightMaterial.mainTexture = vlcRightRightTexture;

        // https://arxiv.org/pdf/2008.11239.pdf
        int depthW, depthH;
        // TODO(jmhenaff): check null + error message somewhere
        if (solArCloudHololens2.isLongThrow())
        {
            depthW = 320;
            depthH = 288;
        }
        else
        {
            depthW = 512;
            depthH = 512;
        }

        depthMaterial = depthPlane.GetComponent<MeshRenderer>().material;
        depthTexture = new Texture2D(depthW, depthH, TextureFormat.R8, false);
        depthMaterial.mainTexture = depthTexture;

        depthABMaterial = depthABPlane.GetComponent<MeshRenderer>().material;
        depthABTexture = new Texture2D(depthW, depthH, TextureFormat.R8, false);
        depthABMaterial.mainTexture = depthABTexture;

        ToggleDebugDisplay();

        solArCloudHololens2.OnDepthFrame += OnDepthFrame;
        solArCloudHololens2.OnVlcFrame += OnVlcFrame;
        solArCloudHololens2.OnPvFrame += OnPvFrame;
        solArCloudHololens2.OnGrpcError += OnGrpcError;
        solArCloudHololens2.OnPluginError += OnPluginError;
        solArCloudHololens2.OnUnityAppError += OnUnityAppError;
        solArCloudHololens2.OnReceivedPose += OnReceivedPose;
        solArCloudHololens2.OnSentFrame += OnSentFrame;

        if (solArCloudHololens2 != null)
        {
            ipAddressText.text = solArCloudHololens2.frontendIp;
        }
        else
        {
            ipAddressText.text = "SolArCloudHololens2 object not set";
        }
        
    }

    private void OnGrpcError(string message)
    {
        AddLogToQueue(gRpcLogsQueue, message);
        UpdateLogsText(gRpcLogs, gRpcLogsQueue);
    }

    private void OnPluginError(string message)
    {
        AddLogToQueue(pluginLogsQueue, message);
        UpdateLogsText(pluginLogs, pluginLogsQueue);
    }

    private void OnUnityAppError(string message)
    {
        AddLogToQueue(unityLogsQueue, message);
        UpdateLogsText(unityLogs, unityLogsQueue);
    }

    private void AddLogToQueue(Queue<string> q, string m)
    {
        q.Enqueue(m);
        if (q.Count > logsNbMaxElements)
        {
            q.Dequeue();
        }
    }

    private void UpdateLogsText(TextMeshPro text, Queue<string> messagesQueue)
    {
        text.text = "";
        if (messagesQueue.Count > 0)
        {
            text.text = String.Join("\n", messagesQueue.ToArray());
        }        
    }

    private void ClearLogs()
    {
        gRpcLogs.text = "";
        gRpcLogsQueue.Clear();
        unityLogs.text = "";
        gRpcLogsQueue.Clear();
        pluginLogs.text = "";
        gRpcLogsQueue.Clear();
    }

    private string d2str(double d)
    {
        return String.Format("{0:0.00000}", d);
    }

    private void Update()
    {
        // UpdateLogsText();
        if (keyboard != null && keyboard.active)
        {           
            solArCloudHololens2.frontendIp = keyboard.text;
            ipAddressText.text = solArCloudHololens2.frontendIp;
        }
    }

    public void EnterIp()
    {
        keyboard = TouchScreenKeyboard.Open(solArCloudHololens2.frontendIp, TouchScreenKeyboardType.URL);
    }
}
