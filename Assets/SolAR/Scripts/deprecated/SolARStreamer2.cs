/**
 * @copyright Copyright (c) 2021 B-com http://www.b-com.com/
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Google.Protobuf;

#if ENABLE_WINMD_SUPPORT
using SolARHololens2UnityPlugin;
using Windows.Storage;
#endif

using SolARRpc = Com.Bcom.Solar.Gprc;
    
public class SolARStreamer2 : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
SolARHololens2ResearchMode researchMode;
#endif

    [SerializeField]
    public string SolARServicesFrontEndIpAddress = "http://192.168.137.1:5001";
    private UnityEngine.TouchScreenKeyboard keyboard;

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
    Bcom.Solar.Cloud.Rpc.Pose receivedPose;

    byte[] frameTexture;
    bool isRGBsensorStarted = false;

    // DEBUG
    [SerializeField]
    TextMeshPro fpsText;
    private float hudRefreshRate = 1f;
    private float timer;
    bool debugDisplayEnabled = false;

    public enum Hl2SensorType
    {
        PV,
        RM_LEFT_LEFT,
        RM_LEFT_FRONT,
        RM_RIGHT_FRONT,
        RM_RIGHT_RIGHT,
        RM_DEPTH
    }

    /*    SolarRpcClient rpcClient = new SolarRpcClient();*/
    SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager relocAndMappingProxy = null;
    bool relocAndMappingProxyInitialized = false;

     Dictionary<Hl2SensorType, bool> enabledSensors = new Dictionary<Hl2SensorType, bool>();
    bool isLongThrow = false;

    string duration = "<not-set>";

    bool recordingEnabled = false;
    string photoVideoSensorId = "000";
    int cptPhotoVideoSensorFrame = 0;
    string leftFrontVLCSensorFilesId = "001";
    int cptLeftFrontVLCSensorFrame = 0;
    string poseFilesPrefix = "pose_";
    string timestampFileName = "timeStamps";

#if ENABLE_WINMD_SUPPORT
	StorageFolder dataFolder;
	StorageFolder photoVideoSensorImagesFolder;
	StorageFile photoVideoSensorPosesFile;
	StorageFolder leftFrontVLCSensorImagesFolder;
	StorageFile leftFrontVLCSensorPosesFile;
	StorageFile timestampsFile;
#endif
    List<string> posesPV = new List<string>();
    List<string> posesVLC = new List<string>();
    List<string> timestamps = new List<string>();

    // RPC
    bool rpcAvailable = true;

    GameObject solARCube = null;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("I am alive!");

/*        // Test connection with a ping
        Tuple<bool, string> pingResult = rpcClient.Ping();
        rpcAvailable = pingResult.Item1;

        DisplayDebug(pvText, pingResult.Item2);*/

        ipAddressText.text = SolARServicesFrontEndIpAddress;


#if ENABLE_WINMD_SUPPORT
        researchMode = new SolARHololens2ResearchMode();
#endif
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
        if (isLongThrow)
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

        enabledSensors.Add(Hl2SensorType.PV, false);
        enabledSensors.Add(Hl2SensorType.RM_LEFT_LEFT, false);
        enabledSensors.Add(Hl2SensorType.RM_LEFT_FRONT, true);
        enabledSensors.Add(Hl2SensorType.RM_RIGHT_FRONT, false);
        enabledSensors.Add(Hl2SensorType.RM_RIGHT_RIGHT, false);
        enabledSensors.Add(Hl2SensorType.RM_DEPTH, false);



#if ENABLE_WINMD_SUPPORT
		if ( recordingEnabled )
		{
			// Create folder to store images and poses
			Task task = new Task( async () =>
			{
				StorageFolder localFolder = ApplicationData.Current.LocalFolder;
				dataFolder = await localFolder.CreateFolderAsync("data", CreationCollisionOption.ReplaceExisting);
				photoVideoSensorImagesFolder = await dataFolder.CreateFolderAsync(photoVideoSensorId, CreationCollisionOption.ReplaceExisting);
				leftFrontVLCSensorImagesFolder = await dataFolder.CreateFolderAsync(leftFrontVLCSensorFilesId, CreationCollisionOption.ReplaceExisting);

				leftFrontVLCSensorPosesFile = await dataFolder.CreateFileAsync(poseFilesPrefix + leftFrontVLCSensorFilesId + ".txt", CreationCollisionOption.ReplaceExisting);

			});
			task.Start();
			task.Wait();
		}

		if ( enabledSensors[Hl2SensorType.PV] )
		{
			researchMode.EnablePv();
		}
		if ( enabledSensors[Hl2SensorType.RM_LEFT_LEFT] )
		{
			researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.LEFT_LEFT);
		}
		if ( enabledSensors[Hl2SensorType.RM_LEFT_FRONT] )
		{
			researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT);
		}
		if ( enabledSensors[Hl2SensorType.RM_RIGHT_FRONT] )
		{
			researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT);
		}
		if ( enabledSensors[Hl2SensorType.RM_RIGHT_RIGHT] )
		{
			researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.RIGHT_RIGHT);
		}
		if ( enabledSensors[Hl2SensorType.RM_DEPTH] )
		{
			researchMode.EnableDepth(isLongThrow);
		}

		researchMode.Init();
#endif

        // TODO(jmhenaff): remove as soon as proto is updated to support multiple frames
        int cptEnabled = 0;
        foreach (KeyValuePair<Hl2SensorType, bool> entry in enabledSensors)
        {
            if (entry.Value)
            {
                cptEnabled++;
            }

            if (cptEnabled > 1)
            {
                receivedPoseText.text =
                    "Error: more than one sensor activated. Not currently supported by MappingAndRelocatlization service\n" +
                    "Connection to service is disabled.\n";
                rpcAvailable = false;
                break;
            }
        }

        var res = relocAndMappingProxy.SendMessage("Hololens 2 requesting connection");
        if (!res.success)
        {
            receivedPoseText.text +=
                "Failure in attempt to contact relocalization and mapping service:\n" + res.errMessage + "\n";
            rpcAvailable = false;
        }


        solARCube = GameObject.FindWithTag("SolAR_Object");
        //solARCube.SetActive(false);
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

    }

    private void DisplayDebug(TextMeshPro textMp, string message)
    {
        textMp.text = message;
    }

    public void EnterIp()
    {
        keyboard = TouchScreenKeyboard.Open(SolARServicesFrontEndIpAddress, TouchScreenKeyboardType.URL);
    }

    #region Button Events

    public void StartRGBSensorCaptureEvent()
    {

        if (!isRGBsensorStarted)
        {
            DisplayDebug(pvText, "Sensors started (" + (rpcAvailable ? "Ping OK)" : "Ping NOK)"));

            try
            {
                // rpcClient.gRpcAddress = SolARServicesFrontEndIpAddress;
                relocAndMappingProxy  = new SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.Builder()
                                    .SetServiceAddress(SolARServicesFrontEndIpAddress)
                                    .Build();

                if (!relocAndMappingProxyInitialized)
                {
                    // TODO(jmhenaff): update plugin to get these hard coded values
                    var res = relocAndMappingProxy.Init();
                    relocAndMappingProxyInitialized = res.success;
                    if (!relocAndMappingProxyInitialized)
                    {
                        pvText.text += res.errMessage;
                    }

                    if (enabledSensors[Hl2SensorType.RM_LEFT_FRONT])
                    {
                        res = relocAndMappingProxy.SetCameraParameters(
                            "LEFT_FRONT",
                            0,
                            SolARRpc.CameraType.Gray,
                            640,
                            480,
                            new double[]{
                                366.189452,          0, 320,
                                         0, 366.478090, 240,
                                         0,          0, 1},
                            -0.009463f,
                            0.003013f,
                            -0.006169f,
                            -0.008975f,
                            0f);
                    }
                    else if (enabledSensors[Hl2SensorType.PV])
                    {
                        res = relocAndMappingProxy.SetCameraParameters(
                            "PV",
                            0,
                            SolARRpc.CameraType.Rgb,
                            760,
                            428,
                            new double[]{
                                592.085,       0, 371.296,
                                      0, 592.085, 203.017,
                                      0,       0,       1},
                            -0.0626f,
                            0.7265f,
                            -0.006169f,
                            -0.008975f,
                            0f);
                    }
                    else if (enabledSensors[Hl2SensorType.RM_LEFT_LEFT]
                                || enabledSensors[Hl2SensorType.RM_RIGHT_FRONT]
                                || enabledSensors[Hl2SensorType.RM_RIGHT_RIGHT]
                                || enabledSensors[Hl2SensorType.RM_DEPTH])
                    {
                        pvText.text += "Error: only left front and PV cameras are supported for now";
                        relocAndMappingProxyInitialized = false;
                        rpcAvailable = false;
                        return;
                    }
                    else
                    {
                        pvText.text += "Warning: no sensors are enabled";
                        relocAndMappingProxyInitialized = false;
                        rpcAvailable = false;
                        return;
                    }

                    relocAndMappingProxyInitialized = res.success;
                    if (!relocAndMappingProxyInitialized)
                    {
                        pvText.text += res.errMessage;
                    }
                }

                if (relocAndMappingProxyInitialized)
                {
                    relocAndMappingProxy.Start();
                }
                

#if ENABLE_WINMD_SUPPORT
				researchMode.Start();
#endif
                isRGBsensorStarted = true;
            }
            catch (Exception e)
            {
                DisplayDebug(pvText, "Exception occured when starting : " + e.Message);
            }
        }
        else
        {
            DisplayDebug(pvText, "StopRGBSensorCaptureEvent");

            if (relocAndMappingProxyInitialized)
            {
                relocAndMappingProxy.Stop();
            }

            try
            {
#if ENABLE_WINMD_SUPPORT
				researchMode.Stop();
#endif
                isRGBsensorStarted = false;
            }
            catch (Exception e)
            {
                Debug.Log("Exception occured when stopping : " + e.Message);
            }
        }
    }

    #endregion

    private string d2str(double d)
    {
        return String.Format("{0:0.00000}", d);
    }

    // Normalize depth from its ushort value to a [0..255] byte for better
    // vizualization
    // Inspired from SensorVisualization Sample of HoloLens2ForCV
    // https://github.com/microsoft/HoloLens2ForCV/tree/main/Samples/SensorVisualization
    private byte convertDepthPixel(ushort depthValue, bool isLongThrow)
    {
        float normalizedValue = 0.0f;
        int maxDepth = isLongThrow ? 4000 : 1000;

        if (depthValue >= maxDepth)
        {
            normalizedValue = 1.0f;
        }
        else
        {
            normalizedValue = (float)(depthValue) / (float)(maxDepth);
        }

        return (byte)(normalizedValue * 255);
    }

    private void handleDepth(SolarRpcClient.FrameSender frameSender,
        SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
    {

        int width = -1;
        int height = -1;
#if ENABLE_WINMD_SUPPORT
	    width = (int)researchMode.GetDepthWidth();
	    height = (int)researchMode.GetDepthHeight();
#endif

        if ((width != 0) && (height != 0))
        {
            try
            {
                UInt64 ts = 0;
                double[] cam2WorldTransform = null;
                UInt32 _width = 0;
                UInt32 _height = 0;
                float _fx;
                float _fy;
                UInt32 _pixelBufferSize;

                Debug.Log("Get depth buffer data");
                ushort[] depthBufferData = null; ;
#if ENABLE_WINMD_SUPPORT
			depthBufferData = researchMode.GetDepthData(out ts, out cam2WorldTransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height);
#endif
                Debug.Log("Depth buffer is retrieved");
                if (depthBufferData != null)
                {
                    Debug.Log("Retrieved depth buffer is not null");
                    if (depthBufferData.Length > 0)
                    {
                        Debug.Log("Retrieved depth buffer size is not zero");

                        // TODO(jmhenaff): optimize
                        int depthPixelCount = width * height;

                        // Arrays for visualization (ushort depth value normalized to byte) otherwise it's too dark
                        byte[] depthData = new byte[depthPixelCount];
                        byte[] depthABData = new byte[depthPixelCount];

                        // Arrays of data to send via gRPC, with original values for depth and expected layout (u16)
                        byte[] depthDataGrpc = new byte[depthPixelCount * 2];
                        byte[] depthABDataGrpc = new byte[depthPixelCount * 2];

                        // depthAText.text += "Fill buffers \n";
                        for (int i = 0; i < depthPixelCount; i++)
                        {
                            depthDataGrpc[2 * i] = (byte)(depthBufferData[i] >> 8);
                            depthDataGrpc[2 * i + 1] = (byte)(depthBufferData[i]);
                            depthABDataGrpc[2 * i] = (byte)(depthBufferData[depthPixelCount + i] >> 8);
                            depthABDataGrpc[2 * i + 1] = (byte)(depthBufferData[depthPixelCount + i]);

                            depthData[i] = convertDepthPixel(depthBufferData[i], isLongThrow);
                            depthABData[i] = convertDepthPixel(depthBufferData[depthPixelCount + i], isLongThrow);
                        }

                        // Send 2 frames
                        // frameSender.addFrame( /* depth sensor id */ 5, ts, width, height, Bcom.Solar.Cloud.Rpc.ImageLayout.Grey16, depthDataGrpc, cam2WorldTransform);
                        // frameSender.addFrame( /* depthAB sensor id */ 6, ts, width, height, Bcom.Solar.Cloud.Rpc.ImageLayout.Grey16, depthABDataGrpc, cam2WorldTransform);


                        depthMaterial.mainTexture = depthTexture;
                        depthTexture.LoadRawTextureData(depthData);
                        depthTexture.Apply();
                        // GetComponent<Renderer>().material.mainTexture = depthTexture;

                        depthABMaterial.mainTexture = depthABTexture;
                        depthABTexture.LoadRawTextureData(depthABData);
                        depthABTexture.Apply();
                        // GetComponent<Renderer>().material.mainTexture = depthABTexture;

                        //                  if (recordingEnabled)
                        //					{
                        //						byte[] bytes = vlcTexture.EncodeToJPG();
                        //						//Create file
                        //						StorageFile frameImageFile;
                        //						Task task = new Task(async () =>
                        //						{
                        //							frameImageFile = await leftFrontVLCSensorImagesFolder.CreateFileAsync(cptLeftFrontVLCSensorFrame.ToString("D8") + ".jpg");
                        //							System.IO.File.WriteAllBytes(frameImageFile.Path, bytes);
                        //							cptLeftFrontVLCSensorFrame++;
                        //
                        //						});
                        //						task.Start();
                        //						task.Wait();
                        //					}

                        DisplayDebug(depthText, "w: " + width + "\n" + "h: " + height + "\n" +
                                "VLC2World = [\n" +
                                "    " + d2str(cam2WorldTransform[0]) + "," + d2str(cam2WorldTransform[1]) + "," + d2str(cam2WorldTransform[2]) + "," + d2str(cam2WorldTransform[3]) + "\n" +
                                "    " + d2str(cam2WorldTransform[4]) + "," + d2str(cam2WorldTransform[5]) + "," + d2str(cam2WorldTransform[6]) + "," + d2str(cam2WorldTransform[7]) + "\n" +
                                "    " + d2str(cam2WorldTransform[8]) + "," + d2str(cam2WorldTransform[9]) + "," + d2str(cam2WorldTransform[10]) + "," + d2str(cam2WorldTransform[11]) + "\n" +
                                "    " + d2str(cam2WorldTransform[12]) + "," + d2str(cam2WorldTransform[13]) + "," + d2str(cam2WorldTransform[14]) + "," + d2str(cam2WorldTransform[15]) + "\n" +
                                "]");
                    }
                }
                else
                {
                    Debug.Log("Depth buffer is empty");
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception occured when retrieving depth buffer: " + e.Message);
            }
        }
    }

#if ENABLE_WINMD_SUPPORT
private int GetRmSensorIdForRpc(SolARHololens2UnityPlugin.RMSensorType sensorType)
{
	switch(sensorType)
	{
	case SolARHololens2UnityPlugin.RMSensorType.LEFT_LEFT: return 1;
	case SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT: return 2;
	case SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT: return 3;
	case SolARHololens2UnityPlugin.RMSensorType.RIGHT_RIGHT: return 4;
	default: throw new Exception();
	}
}
#endif

    private void handleVlc(
#if ENABLE_WINMD_SUPPORT
	SolARHololens2UnityPlugin.RMSensorType sensorType,
#endif
    GameObject vlcPlane,
    TextMeshPro vlcText,
    Material vlcMaterial,
    Texture2D vlcTexture,
    SolarRpcClient.FrameSender frameSender,
    SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender
    )
    {

        int width = -1;
        int height = -1;
#if ENABLE_WINMD_SUPPORT
	    width = (int)researchMode.GetVlcWidth(sensorType);
	    height = (int)researchMode.GetVlcHeight(sensorType);
#endif

        if ((width != 0) && (height != 0))
        {
            try
            {
                UInt64 ts = 0;
                double[] cam2WorldTransform = null; ;
                UInt32 _width = 0;
                UInt32 _height = 0;
                float _fx;
                float _fy;
                UInt32 _pixelBufferSize;

                Debug.Log("Get VLC buffer data");
                byte[] vclBufferData = null;
#if ENABLE_WINMD_SUPPORT
			    vclBufferData = researchMode.GetVlcData(sensorType, out ts, out cam2WorldTransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height, /* flip = */ false);
#endif
                Debug.Log("VLC buffer is retrieved");
                if (vclBufferData != null)
                {
#if ENABLE_WINMD_SUPPORT
				// frameSender.addFrame(GetRmSensorIdForRpc(sensorType), ts, width, height, Bcom.Solar.Cloud.Rpc.ImageLayout.Grey8, vclBufferData, cam2WorldTransform);
#endif
                    if (rpcAvailable)
                    {
                        relocAndMappingFrameSender.SetFrame(
                             /* sensor id PV */ 0,
                             ts,
                             SolARRpc.ImageLayout.Grey8,
                             _width,
                             _height,
                             vclBufferData,
                             cam2WorldTransform,
                             /* compression = */ false);
                    }


                    Debug.Log("Retrieved VLC buffer is not null");
                    if (vclBufferData.Length > 0)
                    {

                        Debug.Log("Retrieved VLC buffer size is not zero");
                        vlcMaterial.mainTexture = vlcTexture;
                        vlcTexture.LoadRawTextureData(vclBufferData);
                        vlcTexture.Apply();
                        // GetComponent<Renderer>().material.mainTexture = vlcTexture;

                        /*						if (recordingEnabled)
                                            {
                                                byte[] bytes = vlcTexture.EncodeToJPG();
                                                //Create file
                                                StorageFile frameImageFile;
                                                Task task = new Task(async () =>
                                                {
                                                    frameImageFile = await leftFrontVLCSensorImagesFolder.CreateFileAsync(cptLeftFrontVLCSensorFrame.ToString("D8") + ".jpg");
                                                    System.IO.File.WriteAllBytes(frameImageFile.Path, bytes);
                                                    cptLeftFrontVLCSensorFrame++;

                                                });
                                                task.Start();
                                                task.Wait();
                                            }*/

                        DisplayDebug(vlcText, "rpc duration: " + duration + "\n" +
                                "w: " + width + "\n" + "h: " + height + "\n" +
                                "VLC2World = [\n" +
                                "    " + d2str(cam2WorldTransform[0]) + "," + d2str(cam2WorldTransform[1]) + "," + d2str(cam2WorldTransform[2]) + "," + d2str(cam2WorldTransform[3]) + "\n" +
                                "    " + d2str(cam2WorldTransform[4]) + "," + d2str(cam2WorldTransform[5]) + "," + d2str(cam2WorldTransform[6]) + "," + d2str(cam2WorldTransform[7]) + "\n" +
                                "    " + d2str(cam2WorldTransform[8]) + "," + d2str(cam2WorldTransform[9]) + "," + d2str(cam2WorldTransform[10]) + "," + d2str(cam2WorldTransform[11]) + "\n" +
                                "    " + d2str(cam2WorldTransform[12]) + "," + d2str(cam2WorldTransform[13]) + "," + d2str(cam2WorldTransform[14]) + "," + d2str(cam2WorldTransform[15]) + "\n" +
                                "]");
                    }
                    else
                    {
                        Debug.Log("VLC buffer is empty");
                    }
                }
                else
                {
                    Debug.Log("VLC buffer is null");
                }
            }
            catch (Exception e)
            {
                // duration =  "ex: " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace;
                // Debug.Log("Exception occured when retrieving VLC buffer: " + e.Message);
                if (rpcAvailable)
                {
                    // rpcClient.SendMessage("ex sending VLC: " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
                    relocAndMappingProxy.SendMessage("ex sending VLC: " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace);
                }
            }
        }
    }

    private void handlePv(SolarRpcClient.FrameSender frameSender,
        SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
    {
        // update depth map texture
        int width = -1;
        int height = -1;

#if ENABLE_WINMD_SUPPORT
			width = (int)researchMode.GetPvWidth();
			height = (int)researchMode.GetPvHeight();
#endif

        if ((width != 0) && (height != 0))
        {

            UInt32 _width = 0;
            UInt32 _height = 0;
            UInt32 _pixelBufferSize = 0;
            float _fx = -1.0f;
            float _fy = -1.0f;
            UInt64 _timestamp = 0;
            double[] _PVtoWorldtransform = null;

#if ENABLE_WINMD_SUPPORT
				frameTexture = researchMode.GetPvData(out _timestamp, out _PVtoWorldtransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height, /* flip = */ false);
#endif
            Matrix4x4 m = new Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m[i, j] = (float)_PVtoWorldtransform[i * 4 + j];
                }
            }

            DisplayDebug(pvText, "rpc duration: " + duration + "\n" +
                                    "width:" + _width + "\n"
                                + "height:" + _height + "\n"
                                + "timestamp:" + _timestamp + "\n"
                                + "fx:" + _fx + "\n"
                                + "fy:" + _fy + "\n" +
                                "PV2World = [\n" +
                                "    " + d2str(_PVtoWorldtransform[0]) + "," + d2str(_PVtoWorldtransform[1]) + "," + d2str(_PVtoWorldtransform[2]) + "," + d2str(_PVtoWorldtransform[3]) + "\n" +
                                "    " + d2str(_PVtoWorldtransform[4]) + "," + d2str(_PVtoWorldtransform[5]) + "," + d2str(_PVtoWorldtransform[6]) + "," + d2str(_PVtoWorldtransform[7]) + "\n" +
                                "    " + d2str(_PVtoWorldtransform[8]) + "," + d2str(_PVtoWorldtransform[9]) + "," + d2str(_PVtoWorldtransform[10]) + "," + d2str(_PVtoWorldtransform[11]) + "\n" +
                                "    " + d2str(_PVtoWorldtransform[12]) + "," + d2str(_PVtoWorldtransform[13]) + "," + d2str(_PVtoWorldtransform[14]) + "," + d2str(_PVtoWorldtransform[15]) + "\n" +
                                "]");

            if (frameTexture != null)
            {
                if (frameTexture.Length > 0)
                {
                    // frameSender.addFrame( /* sensor id PV */ 0, _timestamp, width, height, Bcom.Solar.Cloud.Rpc.ImageLayout.Rgb24, frameTexture, _PVtoWorldtransform);
                    if (rpcAvailable)
                    {
                        relocAndMappingFrameSender.SetFrame(
                            /* sensor id PV */ 0,
                            _timestamp,
                            SolARRpc.ImageLayout.Rgb24,
                            _width,
                            _height,
                            frameTexture,
                            _PVtoWorldtransform,
                            /* compression = */ false);
                    }


                    pvMaterial.mainTexture = pvTexture;
                    pvTexture.LoadRawTextureData(frameTexture);
                    pvTexture.Apply();
                    //GetComponent<Renderer>().material.mainTexture = pvTexture;

                    /*						if ( recordingEnabled )
                                            {
                                                // Save image
                                                byte[] bytes = pvTexture.EncodeToJPG();
                                                //Create file
                                                StorageFile frameImageFile;
                                                Task task = new Task( async () =>
                                                {
                                                    frameImageFile = await photoVideoSensorImagesFolder.CreateFileAsync(cptPhotoVideoSensorFrame.ToString("D8") + ".jpg");
                                                    System.IO.File.WriteAllBytes(frameImageFile.Path, bytes);
                                                    cptPhotoVideoSensorFrame++;

                                                });
                                                task.Start();
                                                task.Wait();
                                                // Add pose
                                                string pose = String.Join(" ", new List<double>(_PVtoWorldtransform));
                                                posesPV.Add(pose);
                                                // Add timestamp
                                                timestamps.Add(""+_timestamp);
                                            }*/
                }
            }
        }
    }

    void poseReceived(Bcom.Solar.Cloud.Rpc.Pose pose)
    {
        receivedPose = pose;
        receivedPoseText.text =
            receivedPose.Mat.M11 + ", " + receivedPose.Mat.M12 + ", " + receivedPose.Mat.M13 + ", " + receivedPose.Mat.M14 + "\n" +
            receivedPose.Mat.M11 + ", " + receivedPose.Mat.M12 + ", " + receivedPose.Mat.M13 + ", " + receivedPose.Mat.M14 + "\n" +
            receivedPose.Mat.M11 + ", " + receivedPose.Mat.M12 + ", " + receivedPose.Mat.M13 + ", " + receivedPose.Mat.M14 + "\n" +
            receivedPose.Mat.M11 + ", " + receivedPose.Mat.M12 + ", " + receivedPose.Mat.M13 + ", " + receivedPose.Mat.M14;

    }

    private void relocAndMappingResultReceived(SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
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

             if (solARCube != null)
            {
                //solARCube.SetActive(true);
                Matrix4x4 mat = new Matrix4x4()
                {
                    m00 = receivedPose.M11,
                    m01 = -receivedPose.M12,
                    m02 = -receivedPose.M13,
                    m03 = receivedPose.M14,
                    m10 = receivedPose.M21,
                    m11 = -receivedPose.M22,
                    m12 = -receivedPose.M23,
                    m13 = receivedPose.M24,
                    m20 = receivedPose.M31,
                    m21 = -receivedPose.M32,
                    m22 = -receivedPose.M33,
                    m23 = receivedPose.M43,
                    m30 = receivedPose.M41,
                    m31 = -receivedPose.M42,
                    m32 = -receivedPose.M43,
                    m33 = receivedPose.M44,
                };
                solARCube.transform.rotation = mat.ExtractRotation();
                solARCube.transform.position = mat.ExtractPosition();
            }
        }
        else
        {
            receivedPoseText.text =
                "Error: " + result.resultStatus.errMessage;
        }
    }

    private void LateUpdate()
    {
        // FPS
        if (Time.unscaledTime > timer)
        {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
            timer = Time.unscaledTime + hudRefreshRate;
        }

        SolarRpcClient.FrameSender frameSender = null; // rpcClient.BuildFrameSender(poseReceived);

        SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender =
            relocAndMappingProxy.BuildFrameSender(relocAndMappingResultReceived, null);

        if (keyboard != null && keyboard.active)
        {
            ipAddressText.text = keyboard.text;
            SolARServicesFrontEndIpAddress = ipAddressText.text;
        }

#if ENABLE_WINMD_SUPPORT
		researchMode.Update();
#endif

        if (isRGBsensorStarted)
        {
            if (enabledSensors[Hl2SensorType.PV])
            {
                handlePv(frameSender, relocAndMappingFrameSender);
            }

            if (enabledSensors[Hl2SensorType.RM_LEFT_LEFT])
            {
                handleVlc(
#if ENABLE_WINMD_SUPPORT
					SolARHololens2UnityPlugin.RMSensorType.LEFT_LEFT,
#endif
                vlcLeftLeftPlane,
                vlcLeftLeftText,
                vlcLeftLeftMaterial,
                vlcLeftLeftTexture,
                frameSender,
                relocAndMappingFrameSender);
            }

            if (enabledSensors[Hl2SensorType.RM_LEFT_FRONT])
            {
                handleVlc(
#if ENABLE_WINMD_SUPPORT
					SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT,
#endif
                vlcLeftFrontPlane,
                vlcLeftFrontText,
                vlcLeftFrontMaterial,
                vlcLeftFrontTexture,
                frameSender,
                relocAndMappingFrameSender);
            }

            if (enabledSensors[Hl2SensorType.RM_RIGHT_FRONT])
            {
                handleVlc(
#if ENABLE_WINMD_SUPPORT
					SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT,
#endif
                vlcRightFrontPlane,
                vlcRightFrontText,
                vlcRightFrontMaterial,
                vlcRightFrontTexture,
                frameSender,
                relocAndMappingFrameSender);
            }

            if (enabledSensors[Hl2SensorType.RM_RIGHT_RIGHT])
            {
                handleVlc(
#if ENABLE_WINMD_SUPPORT
					SolARHololens2UnityPlugin.RMSensorType.RIGHT_RIGHT,
#endif
                vlcRightRightPlane,
                vlcRightRightText,
                vlcRightRightMaterial,
                vlcRightRightTexture,
                frameSender,
                relocAndMappingFrameSender);
            }

            if (enabledSensors[Hl2SensorType.RM_DEPTH])
            {
                handleDepth(frameSender,
                    relocAndMappingFrameSender);
            }
        }

        if (rpcAvailable && relocAndMappingProxyInitialized)
        {
 //           frameSender.SendAsyncDropFrames();
            var res = relocAndMappingFrameSender.RelocAndMapAsyncDrop();
            if (!res.success)
            {
                receivedPoseText.text = "RelocAndMapAsyncDrop error: " + res.errMessage;
            }

            // frameSender.sendToSolar();
            // await frameSender.SendFramesAsync2();
            /*			if (sw == null)
                        {
                            sw = new System.Diagnostics.Stopwatch();
                            sw.Start();
                        }
                        else if ( sw.ElapsedMilliseconds > 33)
                        {
                            frameSender.SendFramesAsync2();
                            sw.Stop();
                            sw.Reset();
                            sw.Start();
                        }*/
        }
    }
}
