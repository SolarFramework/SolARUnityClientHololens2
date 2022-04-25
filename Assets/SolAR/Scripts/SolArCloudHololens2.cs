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
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using SolARHololens2UnityPlugin;
using Windows.Storage;
#endif

using SolARRpc = Com.Bcom.Solar.Gprc;

namespace Bcom.Solar
{
    public class SolArCloudHololens2 : MonoBehaviour
    {

        enum Hl2SensorType
        {
            PV,
            RM_LEFT_LEFT,
            RM_LEFT_FRONT,
            RM_RIGHT_FRONT,
            RM_RIGHT_RIGHT,
            RM_DEPTH
        }


#if ENABLE_WINMD_SUPPORT
SolARHololens2ResearchMode researchMode;
#endif

        [Tooltip("IP address of the SolAR cloud services frontend")]
        public string frontendIp = "<not-set>";
        [Tooltip("Base port for SolAR cloud services frontend.\n" +
            "If unique port option is disabled, a new port will be created for each\n" +
            "gRPC channels, starting from this one, and incremented by 1")]
        public int frontendBasePort = -1;
        [SerializeField]
        GameObject solarScene = null;
        Matrix4x4 solarSceneInitPose;

        [Tooltip("Allow the pipeline to only perform mapping or enable relocalization as well")]
        public SolARRpc.PipelineMode pipelineMode = SolARRpc.PipelineMode.RelocalizationAndMapping;

        [Serializable]
        public struct GrpcSettings
        {
            [Tooltip("Size of gRPC channel pool")]
            public int channelPoolSize;
            [Tooltip("Size of gRPC channel pool")]
            public bool useUniquePort;
            [Tooltip("Delay in ms before sending each frame")]
            public int delayBetweenFramesInMs;
            [Tooltip("Select compression method of frame image buffer to save bandwidth")]
            public SolARRpc.ImageCompression imageCompression;
        }

        [SerializeField]
        private GrpcSettings advancedGrpcSettings = new GrpcSettings()
        {
            channelPoolSize = 6,
            useUniquePort = false,
            delayBetweenFramesInMs = 20,
            imageCompression = SolARRpc.ImageCompression.None
        };



        // Only show currently supported sensors
        // When other sensors are supported, e.g for multi-frame,
        // this will be converted to multiple selection UI i.o. dropdown list
        public enum Hl2SensorTypeEditor
        {
            PV,
            RM_LEFT_FRONT
        }

        [HideInInspector] public Hl2SensorTypeEditor selectedSensor = Hl2SensorTypeEditor.RM_LEFT_FRONT;

        [Serializable]
        public struct CameraParameters
        {
            public string name;
            public uint id;
            public SolARRpc.CameraType type;
            public uint width;
            public uint height;
            public double focalX;
            public double focalY;
            public double centerX;
            public double centerY;
            public double distK1;
            public double distK2;
            public double distP1;
            public double distP2;
            public double distK3;

            public CameraParameters(CameraParameters other)
            {
                name = other.name;
                id = other.id;
                type = other.type;
                width = other.width;
                height = other.height;
                focalX = other.focalX;
                focalY = other.focalY;
                centerX = other.centerX;
                centerY = other.centerY;
                distK1 = other.distK1;
                distK2 = other.distK2;
                distP1 = other.distP1;
                distP2 = other.distP2;
                distK3 = other.distK3;
            }
        }

        private CameraParameters pvDefaultParameters = new CameraParameters()
        {
            name = "PV",
            id = 0,
            type = SolARRpc.CameraType.Rgb,
            width = 760,
            height = 428,
            focalX = 592.085,
            focalY = 592.085,
            centerX = 371.296,
            centerY = 203.017,
            distK1 = -0.0626,
            distK2 = 0.7265,
            distP1 = -0.006169,
            distP2 = -0.008975,
            distK3 = 0
        };

        private CameraParameters leftFrontDefaultParameters = new CameraParameters()
        {
            name = "LEFT_FRONT",
            id = 0,
            type = SolARRpc.CameraType.Gray,
            width = 640,
            height = 480,
            focalX = 366.189452,
            focalY = 366.478090,
            centerX = 320,
            centerY = 240,
            distK1 = -0.009463,
            distK2 = 0.003013,
            distP1 = -0.006169,
            distP2 = -0.008975,
            distK3 = 0
        };

        // Use these object to store user modifed values
        // TODO: persistency, load from file, calibrate within app.
        [HideInInspector]
        public CameraParameters pvParameters;

        [HideInInspector]
        public CameraParameters leftFrontParameters;

        [HideInInspector]
        public CameraParameters selectedCameraParameter;

        private bool sensorsStarted = false;

        SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager relocAndMappingProxy = null;
        bool relocAndMappingProxyInitialized = false;

        Dictionary<Hl2SensorType, bool> enabledSensors = new Dictionary<Hl2SensorType, bool>();
        private bool longThrow = false;

        // Manual transform from sensor Camera to Unity viewpoint
        Matrix4x4 camToEyesMatrix;

        public bool isLongThrow()
        {
            return longThrow;
        }

        private bool rpcAvailable = false;

        // *******************************************
        // Callbacks 
        // *******************************************

        public event Action<
            byte[] /* depthData */,
            byte[] /* depthABData */,
            ulong /* timestamp */,
            double[] /* cam2WorldTransform */,
            uint /* width */,
            uint /* height */,
            float /* fx */,
            float /* fy */,
            uint /* pixelBufferSize */> OnDepthFrame;

        public event Action<
#if ENABLE_WINMD_SUPPORT
	    SolARHololens2UnityPlugin.RMSensorType /* sensorType */,
#endif
        byte[] /* vlcData */,
            ulong /* timestamp */,
            double[] /* cam2WorldTransform */,
            uint /* width */,
            uint /* height */,
            float /* fx */,
            float /* fy */,
            uint /* pixelBufferSize */> OnVlcFrame;

        public event Action<
            byte[] /* pvData */,
            ulong /* timestamp */,
            double[] /* cam2WorldTransform */,
            uint /* width */,
            uint /* height */,
            float /* fx */,
            float /* fy */,
            uint /* pixelBufferSize */> OnPvFrame;

        public event Action<
            bool /* sensorsStarted */,
            bool /* gRpcOk */> OnStart;
        public event Action OnStop;

        public event Action<
            SolARRpc.PipelineMode /* old mode */,
            SolARRpc.PipelineMode /* new mode */> OnPipelineModeChanged;

        // TODO(jmhenaff): rename toLog ?
        public event Action<string> OnGrpcError;
        public event Action<string> OnPluginError;
        public event Action<string> OnUnityAppError;

        public event Action
            <SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult>
            OnReceivedPose;

        public event Action<long> OnSentFrame;

        private void NotifyOnDepthFrame(
            byte[] depthData,
            byte[] depthABData,
            ulong ts,
            double[] cam2WorldTransform,
            uint width,
            uint height,
            float fx,
            float fy,
            uint pixelBufferSize
        ) => OnDepthFrame?.Invoke(depthData, depthABData, ts, cam2WorldTransform,
            width, height, fx, fy, pixelBufferSize);

        private void NotifyOnVlcFrame(
#if ENABLE_WINMD_SUPPORT
	    SolARHololens2UnityPlugin.RMSensorType sensorType,
#endif
    byte[] vlcData,
        ulong ts,
        double[] cam2WorldTransform,
        uint width,
        uint height,
        float fx,
        float fy,
        uint pixelBufferSize
    ) => OnVlcFrame?.Invoke(
#if ENABLE_WINMD_SUPPORT
	    sensorType,
#endif
        vlcData, ts, cam2WorldTransform,
                width, height, fx, fy, pixelBufferSize);

        private void NotifyOnPvFrame(
        byte[] pvData,
        ulong ts,
        double[] cam2WorldTransform,
        uint width,
        uint height,
        float fx,
        float fy,
        uint pixelBufferSize
        ) => OnPvFrame?.Invoke(pvData, ts, cam2WorldTransform,
            width, height, fx, fy, pixelBufferSize);

        private void NotifyOnStart(bool sensorsStarted, bool gRpcOk)
            => OnStart?.Invoke(sensorsStarted, gRpcOk);
        private void NotifyOnStop()
            => OnStop?.Invoke();

        private void NotifyOnPipelineModeChanged(SolARRpc.PipelineMode oldMode, SolARRpc.PipelineMode newMode)
            => OnPipelineModeChanged?.Invoke(oldMode, newMode);

        private void NotifyOnGrpcError(string message)
            => OnGrpcError?.Invoke(message);

        private void NotifyOnPluginError(string message)
            => OnPluginError?.Invoke(message);

        private void NotifyOnUnityAppError(string message)
            => OnUnityAppError?.Invoke(message);

        private void NotifyOnReceivedPose(
            SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
            => OnReceivedPose?.Invoke(result);

        private void NotifyOnSentFrame(long nbSentFrames)
            => OnSentFrame?.Invoke(nbSentFrames);

        SolArCloudHololens2()
        {
            pvParameters = new CameraParameters(pvDefaultParameters);
            leftFrontParameters = new CameraParameters(leftFrontDefaultParameters);

            // Apply modified delta to initial pose of scene
            camToEyesMatrix = Matrix4x4.identity;
            if (selectedSensor == Hl2SensorTypeEditor.PV)
            {
                camToEyesMatrix.m23 = -0.10f;
            }
            else
            {
                // VLC LEFT LEFT
                // camToEyesMatrix.m03 = -0.05f;
                camToEyesMatrix.m23 = -0.10f;
            }

        }

        // Start is called before the first frame update
        void Start()
        {
            frontendIp = PlayerPrefs.GetString("SolARCloudServicesAddress", frontendIp);

#if ENABLE_WINMD_SUPPORT
        researchMode = new SolARHololens2ResearchMode();
#endif

            enabledSensors.Add(Hl2SensorType.PV, false);
            enabledSensors.Add(Hl2SensorType.RM_LEFT_LEFT, false);
            enabledSensors.Add(Hl2SensorType.RM_LEFT_FRONT, false);
            enabledSensors.Add(Hl2SensorType.RM_RIGHT_FRONT, false);
            enabledSensors.Add(Hl2SensorType.RM_RIGHT_RIGHT, false);
            enabledSensors.Add(Hl2SensorType.RM_DEPTH, false);

            try
            {
                enabledSensors[toHl2SensorType(selectedSensor)] = true;
            }
            catch (Exception e)
            {
                NotifyOnUnityAppError("Error enabling sensor: " + e.Message);
            }

#if ENABLE_WINMD_SUPPORT

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
			researchMode.EnableDepth(longThrow);
		}

		researchMode.Init();
#endif

            if (solarScene == null)
            {
                solarScene = GameObject.FindWithTag("SolAR_Object");
            }

            if (solarScene == null)
            {
                NotifyOnUnityAppError("SolAR GameObject is not set");
            }
            else
            {
                solarSceneInitPose = solarScene.transform.localToWorldMatrix;
                solarScene.SetActive(false);
            }
        }

        public void SaveUserPrefs()
        {
            PlayerPrefs.SetString("SolARCloudServicesAddress", frontendIp);
            PlayerPrefs.Save();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                SaveUserPrefs();
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveUserPrefs();
            }
        }

        //void OnApplicationQuit()
        //{
        //    SaveUserPrefs();
        //}

        public bool TestRpcConnection()
        {
            var res = relocAndMappingProxy.SendMessage("Hololens 2 requesting connection");
            rpcAvailable = res.success;
            if (!res.success)
            {
                NotifyOnUnityAppError("Failure in attempt to contact relocalization and mapping service:\n" + res.errMessage + "\n");
            }
            return rpcAvailable;
        }

        public bool isRpcAvailable()
        {
            return rpcAvailable;
        }

        #region Button Events

        public void TogglePipelineMode()
        {
            if (sensorsStarted)
            {
                NotifyOnUnityAppError("Pipeline mode cannot be changed while running");
                return;
            }

            SolARRpc.PipelineMode oldMode = pipelineMode;
            switch (pipelineMode)
            {
                case SolARRpc.PipelineMode.RelocalizationAndMapping:
                    {
                        pipelineMode = SolARRpc.PipelineMode.RelocalizationOnly;
                        break;
                    }
                case SolARRpc.PipelineMode.RelocalizationOnly:
                    {
                        pipelineMode = SolARRpc.PipelineMode.RelocalizationAndMapping;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Unkown pipeline mode was selected");
                    }
            }

            NotifyOnPipelineModeChanged(oldMode, pipelineMode);

            relocAndMappingProxyInitialized = false;
        }

        public void ToggleSensorCatpure()
        {
            if (!sensorsStarted)
            {
                StartSensorsCapture();
            }
            else
            {
                StopSensorsCapture();
            }
        }

        public void debugLogSelectedCameraParameters(string mess)
        {
            relocAndMappingProxy.SendMessage(mess + ":\n" +
                selectedCameraParameter.name + "\n" +
                selectedCameraParameter.id + "\n" +
                selectedCameraParameter.type + "\n" +
                selectedCameraParameter.width + "\n" +
                selectedCameraParameter.height + "\n" +
                selectedCameraParameter.focalX + "\n" +
                selectedCameraParameter.focalY + "\n" +
                selectedCameraParameter.centerX + "\n" +
                selectedCameraParameter.centerY + "\n" +
                (float)selectedCameraParameter.distK1 + "\n" +
                (float)selectedCameraParameter.distK2 + "\n" +
                (float)selectedCameraParameter.distP1 + "\n" +
                (float)selectedCameraParameter.distP2 + "\n" +
                (float)selectedCameraParameter.distK3 + "\n");
        }

        public void StartSensorsCapture()
        {
            StartCoroutine(FetchAndSendFrames());
        }

        // Needed ? (if LV, can the Task be gc'd before ending its work ?
        private Task stopServiceTask;

        public void StopSensorsCapture()
        {
            NotifyOnUnityAppError("StopRGBSensorCaptureEvent");

            try
            {
#if ENABLE_WINMD_SUPPORT
				researchMode.Stop();
#endif
                sensorsStarted = false;

                // Use Task to not freeze UI due to Sleep()
                stopServiceTask = Task.Factory.StartNew(() =>
                {
                    // Wait for remaining frames to be processed
                    // TODO: improve sync, use some kind of join()
                    Thread.Sleep(500);

                    if (relocAndMappingProxyInitialized)
                    {
                        relocAndMappingProxy.Stop();
                    }
                });
            }
            catch (Exception e)
            {
                NotifyOnUnityAppError("Exception occured when stopping : " + e.Message);
                if (rpcAvailable)
                {
                    relocAndMappingProxy.SendMessage("Exception occured when stopping : " + e.Message
                        + "\n" + e.StackTrace);
                }
            }

            // Delay before sending stop request (wait for pending requests to be sent)
            new WaitForSeconds(0.5f);

            if (relocAndMappingProxyInitialized)
            {
                relocAndMappingProxy.Stop();
            }

            NotifyOnStop();

            StopCoroutine(FetchAndSendFrames());
        }

        #endregion

        private void handleDepth(
            SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
        {

            int width = -1;
            int height = -1;
#if ENABLE_WINMD_SUPPORT
	    width = (int)researchMode.GetDepthWidth();
	    height = (int)researchMode.GetDepthHeight();
#endif

            if (width != 0 && height != 0)
            {
                try
                {
                    ulong ts = 0;
                    double[] cam2WorldTransform = null;
                    uint _width = 0;
                    uint _height = 0;
                    float _fx = -1;
                    float _fy = -1;
                    uint _pixelBufferSize = 0;

                    ushort[] depthBufferData = null; ;
#if ENABLE_WINMD_SUPPORT
			depthBufferData = researchMode.GetDepthData(out ts, out cam2WorldTransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height);
#endif
                    if (depthBufferData != null)
                    {
                        if (depthBufferData.Length > 0)
                        {
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
                                depthDataGrpc[2 * i + 1] = (byte)depthBufferData[i];
                                depthABDataGrpc[2 * i] = (byte)(depthBufferData[depthPixelCount + i] >> 8);
                                depthABDataGrpc[2 * i + 1] = (byte)depthBufferData[depthPixelCount + i];

                                depthData[i] = convertDepthPixel(depthBufferData[i], longThrow);
                                depthABData[i] = convertDepthPixel(depthBufferData[depthPixelCount + i], longThrow);
                            }

                            // TODO(jmhenaff): use addFrame() when multi frame is made available (send Depth and DepthAB
                            if (rpcAvailable)
                            {
                                relocAndMappingFrameSender.SetFrame(0, ts, SolARRpc.ImageLayout.Grey16,
                                    _width, _height, depthData, cam2WorldTransform, SolARRpc.ImageCompression.None);
                            }
                            NotifyOnDepthFrame(depthData, depthABData, ts, cam2WorldTransform, _width,
                                _height, _fx, _fy, _pixelBufferSize);
                        }
                    }
                }
                catch (Exception e)
                {
                    NotifyOnUnityAppError("Error while handling depth buffer: [" + e.GetType().Name + "] " +
                        e.Message);

                    if (rpcAvailable)
                    {
                        relocAndMappingProxy.SendMessage("Error while handling depth buffer: [" +
                            e.GetType().Name + "] " + e.Message + "\n" + e.StackTrace);
                    }
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
    SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender
        )
        {

            int width = -1;
            int height = -1;
#if ENABLE_WINMD_SUPPORT
	    width = (int)researchMode.GetVlcWidth(sensorType);
	    height = (int)researchMode.GetVlcHeight(sensorType);
#endif

            if (width != 0 && height != 0)
            {
                try
                {
                    ulong ts = 0;
                    double[] cam2WorldTransform = null; ;
                    uint _width = 0;
                    uint _height = 0;
                    float _fx = -1f;
                    float _fy = -1f;
                    uint _pixelBufferSize = 0;

                    byte[] vclBufferData = null;

#if ENABLE_WINMD_SUPPORT
			    vclBufferData = researchMode.GetVlcData(sensorType, out ts, out cam2WorldTransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height, /* flip = */ advancedGrpcSettings.imageCompression != SolARRpc.ImageCompression.None);
#endif
                    if (vclBufferData != null)
                    {
                        if (vclBufferData.Length > 0)
                        {
#if ENABLE_WINMD_SUPPORT
                        NotifyOnVlcFrame(sensorType, vclBufferData, ts, cam2WorldTransform, _width, _height, _fx, _fy, _pixelBufferSize);
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
                                     advancedGrpcSettings.imageCompression);
                            }
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
                    NotifyOnUnityAppError("Exception while handling VLC frame: [" + e.GetType().Name + "] " +
                        e.Message);

                    if (rpcAvailable)
                    {
                        relocAndMappingProxy.SendMessage("Exception while handling VLC frame: [" +
                            e.GetType().Name + "] " + e.Message + "\n" + e.StackTrace);
                    }
                }
            }
        }

        private void handlePv(
            SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
        {
            // update depth map texture
            int width = -1;
            int height = -1;

#if ENABLE_WINMD_SUPPORT
			width = (int)researchMode.GetPvWidth();
			height = (int)researchMode.GetPvHeight();
#endif
            if (width != 0 && height != 0)
            {
                try
                {
                    uint _width = 0;
                    uint _height = 0;
                    uint _pixelBufferSize = 0;
                    float _fx = -1.0f;
                    float _fy = -1.0f;
                    ulong _timestamp = 0;
                    double[] _PVtoWorldtransform = null;
                    byte[] frameTexture = null;
#if ENABLE_WINMD_SUPPORT
			    frameTexture = researchMode.GetPvData(out _timestamp, out _PVtoWorldtransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height, /* flip = */ advancedGrpcSettings.imageCompression != SolARRpc.ImageCompression.None);
#endif
                    if (frameTexture != null)
                    {
                        if (frameTexture.Length > 0)
                        {
                            NotifyOnPvFrame(frameTexture, _timestamp, _PVtoWorldtransform, _width, _height,
                                _fx, _fy, _pixelBufferSize);
                            if (rpcAvailable)
                            {
                                relocAndMappingFrameSender.SetFrame(/* sensor id PV */ 0, _timestamp,
                                    SolARRpc.ImageLayout.Rgb24, _width, _height, frameTexture,
                                    _PVtoWorldtransform, advancedGrpcSettings.imageCompression);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    NotifyOnUnityAppError("Exception while handling PV frame: [" + e.GetType().Name + "] " +
                        e.Message);
                    if (rpcAvailable)
                    {
                        relocAndMappingProxy.SendMessage("Exception while handling PV frame: [" +
                            e.GetType().Name + "] " + e.Message + "\n" + e.StackTrace);
                    }
                }
            }
        }

        // https://github.com/PimDeWitte/UnityMainThreadDispatcher
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        //private void relocAndMappingResultReceived(SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
        //{
        //    lock (_executionQueue)
        //    {
        //        _executionQueue.Enqueue(() => {
        //            StartCoroutine(relocAndMappingResultReceivedOnMainThread(result));
        //        });
        //    }
        //}

        // private System.Collections.IEnumerator relocAndMappingResultReceivedOnMainThread(SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
        private void relocAndMappingResultReceived(SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
        {
            var receivedPose = result.relocAndMappingResult.Pose;

            if (result.resultStatus.success)
            {
                if (result.relocAndMappingResult.PoseStatus == SolARRpc.RelocalizationPoseStatus.NoPose)
                {
                    return; //yield return  null;
                }

                NotifyOnReceivedPose(result);

                if (solarScene != null)
                {
                    // SolAR transform * CS conversion
                    // https://medium.com/comerge/what-are-the-coordinates-225f1ec0dd78
                    Matrix4x4 solarDeltaMat = new Matrix4x4()
                    {
                        m00 = receivedPose.M11,
                        m01 = receivedPose.M12,
                        m02 = receivedPose.M13,
                        m03 = receivedPose.M14,
                        m10 = receivedPose.M21,
                        m11 = receivedPose.M22,
                        m12 = receivedPose.M23,
                        m13 = receivedPose.M24,
                        m20 = receivedPose.M31,
                        m21 = receivedPose.M32,
                        m22 = receivedPose.M33,
                        m23 = receivedPose.M34,
                        m30 = receivedPose.M41,
                        m31 = receivedPose.M42,
                        m32 = receivedPose.M43,
                        m33 = receivedPose.M44,
                    };

                    // Take inverse because SolARDelta is HololensOrigin->Marker, we want the inverse of that
                    var solarDeltaMatinv = solarDeltaMat.inverse;

                    // Inverse third line
                    solarDeltaMatinv.m20 *= -1;
                    solarDeltaMatinv.m21 *= -1;
                    solarDeltaMatinv.m22 *= -1;
                    solarDeltaMatinv.m23 *= -1;

                    // Apply modified delta to initial pose of scene
                    var newScenePose = camToEyesMatrix * solarDeltaMatinv * solarSceneInitPose;

                    // Apply corrected pose to scene
                    solarScene.transform.rotation = newScenePose.ExtractRotation();
                    solarScene.transform.position = newScenePose.ExtractPosition();

                    solarScene.SetActive(true);
                }
            }
            else
            {
                NotifyOnGrpcError("Error when receiving pose: " + result.resultStatus.errMessage);
                if (rpcAvailable)
                {
                    relocAndMappingProxy.SendMessage("Error when receiving pose: " +
                        result.resultStatus.errMessage);
                }
            }

            // yield return null;
        }

        private void SentFrame(long nbSentFrames)
        {
            NotifyOnSentFrame(nbSentFrames);
        }

        private string toString(Matrix4x4 m)
        {
            return m.m00 + ", " + m.m01 + ", " + m.m02 + ", " + m.m03 + "\n" +
                   m.m10 + ", " + m.m11 + ", " + m.m12 + ", " + m.m13 + "\n" +
                   m.m20 + ", " + m.m21 + ", " + m.m22 + ", " + m.m23 + "\n" +
                   m.m30 + ", " + m.m31 + ", " + m.m32 + ", " + m.m33;
        }

        System.Collections.IEnumerator /*void*/ FetchAndSendFrames()
        {

            string _frontEndIp = frontendIp;
            int _frontendBasePort = frontendBasePort;

            string[] splittedString = frontendIp.Split(':');
            if (splittedString.Length > 2)
            {
                _frontEndIp = frontendIp.Substring(0, frontendIp.LastIndexOf(':'));

                if (!Int32.TryParse(frontendIp.Substring(frontendIp.LastIndexOf(':') + 1), out _frontendBasePort))
                {
                    NotifyOnUnityAppError("Ill-formed URL: '" + frontendIp + "'");
                    // best effort
                    _frontendBasePort = frontendBasePort;
                }
            }

            // New instance to force creation of new reusable channels and clients with potentially a different address
            relocAndMappingProxy = new SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.Builder()
                                    .SetServiceAddress(_frontEndIp)
                                    .SetPortBase(_frontendBasePort)
                                    .SetClientPoolSize(advancedGrpcSettings.channelPoolSize)
                                    .UseUniquePortNumber(advancedGrpcSettings.useUniquePort)
                                    .SetRelocAndMappingRequestIntervalMs(advancedGrpcSettings.delayBetweenFramesInMs)
                                    .Build();
            TestRpcConnection();
            yield return null;

            if (!rpcAvailable)
            {
                NotifyOnUnityAppError("Could not connect to RPC service");
            }

            // rpcClient.gRpcAddress = SolARServicesFrontEndIpAddress;
            // relocAndMappingProxy.gRpcAddress = SolARServicesFrontEndIpAddress;

            if (!relocAndMappingProxyInitialized)
            {
                var res = relocAndMappingProxy.Init(pipelineMode);

                yield return null;

                relocAndMappingProxyInitialized = res.success;
                if (!relocAndMappingProxyInitialized)
                {
                    NotifyOnGrpcError(res.errMessage);
                }

                // debugLogSelectedCameraParameters("Start");

                res = relocAndMappingProxy.SetCameraParameters(
                    selectedCameraParameter.name,
                    selectedCameraParameter.id,
                    selectedCameraParameter.type,
                    selectedCameraParameter.width,
                    selectedCameraParameter.height,
                    new double[]
                    {
                        selectedCameraParameter.focalX, 0, selectedCameraParameter.centerX,
                        0, selectedCameraParameter.focalY, selectedCameraParameter.centerY,
                        0, 0, 1
                    },
                    (float)selectedCameraParameter.distK1,
                    (float)selectedCameraParameter.distK2,
                    (float)selectedCameraParameter.distP1,
                    (float)selectedCameraParameter.distP2,
                    (float)selectedCameraParameter.distK3);

                yield return null;

                relocAndMappingProxyInitialized = res.success;
                if (!relocAndMappingProxyInitialized)
                {
                    NotifyOnGrpcError(res.errMessage);
                }
            }

            if (relocAndMappingProxyInitialized)
            {
                var res = relocAndMappingProxy.Start();

                yield return null;

                if (!res.success)
                {
                    NotifyOnGrpcError(res.errMessage);
                }
            }


#if ENABLE_WINMD_SUPPORT
				researchMode.Start();
#endif
            sensorsStarted = true; // TODO(jmhenaff): error handling ?

            NotifyOnStart(sensorsStarted, rpcAvailable);

            // TODO(jmhenaff): handle exception w/ coroutine
            // Comment try-catch because of yied
            //try
            //{

#if ENABLE_WINMD_SUPPORT
		    researchMode.Update();
#endif

            while (sensorsStarted)
                {
                    SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender =
                        relocAndMappingProxy.BuildFrameSender(relocAndMappingResultReceived, SentFrame);

                    if (enabledSensors[Hl2SensorType.PV])
                    {
                        handlePv(relocAndMappingFrameSender);
                    }

                    if (enabledSensors[Hl2SensorType.RM_LEFT_LEFT])
                    {
                        handleVlc(
#if ENABLE_WINMD_SUPPORT
				        SolARHololens2UnityPlugin.RMSensorType.LEFT_LEFT,
#endif
                        relocAndMappingFrameSender);
                    }

                    if (enabledSensors[Hl2SensorType.RM_LEFT_FRONT])
                    {
                        handleVlc(
#if ENABLE_WINMD_SUPPORT
					    SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT,
#endif
                        relocAndMappingFrameSender);
                    }

                    if (enabledSensors[Hl2SensorType.RM_RIGHT_FRONT])
                    {
                        handleVlc(
#if ENABLE_WINMD_SUPPORT
					    SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT,
#endif
                        relocAndMappingFrameSender);
                    }

                    if (enabledSensors[Hl2SensorType.RM_RIGHT_RIGHT])
                    {
                        handleVlc(
#if ENABLE_WINMD_SUPPORT
					    SolARHololens2UnityPlugin.RMSensorType.RIGHT_RIGHT,
#endif
                        relocAndMappingFrameSender);
                    }

                    if (enabledSensors[Hl2SensorType.RM_DEPTH])
                    {
                        handleDepth(relocAndMappingFrameSender);
                    }

                    if (rpcAvailable && relocAndMappingProxyInitialized)
                    {
                        // Sync call, not working yet
                        // relocAndMappingFrameSender.RelocalizeAndMap();

                        var res = relocAndMappingFrameSender.RelocAndMapAsyncDrop();

                        if (!res.success)
                        {
                            NotifyOnGrpcError("RelocAndMapAsyncDrop error: " + res.errMessage);
                            if (rpcAvailable)
                            {
                                relocAndMappingProxy.SendMessage("RelocAndMapAsyncDrop error: " + res.errMessage);
                            }
                        }
                    }

                    yield return new WaitForSeconds(.033f);
                    // Thread.Sleep(33);
                }

            //}
            //catch (Exception e)
            //{
            //    NotifyOnGrpcError("LateUpdate() error: " + e.Message);
            //    if (rpcAvailable)
            //    {
            //        relocAndMappingProxy.SendMessage("LateUpdate() error: " + e.Message + "\n" + e.StackTrace);
            //    }
            //}            
        }

        private void LateUpdate()
        {

        }

        public CameraParameters GetPvDefaultParameters()
        {
            return pvDefaultParameters;
        }

        public CameraParameters GetLeftFrontDefaultParameters()
        {
            return leftFrontDefaultParameters;
        }

        private string d2str(double d)
        {
            return string.Format("{0:0.00000}", d);
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
                normalizedValue = depthValue / (float)maxDepth;
            }

            return (byte)(normalizedValue * 255);
        }

        private Hl2SensorType toHl2SensorType(Hl2SensorTypeEditor type)
        {
            switch (type)
            {
                case Hl2SensorTypeEditor.PV: return Hl2SensorType.PV;
                case Hl2SensorTypeEditor.RM_LEFT_FRONT: return Hl2SensorType.RM_LEFT_FRONT;
                default: throw new ArgumentException("Cannot convert given value of Hl2SensorTypeEditor to Hl2SensorType");
            }
        }

    }
}