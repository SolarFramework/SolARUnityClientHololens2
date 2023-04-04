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
using System.Runtime.InteropServices;

using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using SolARHololens2UnityPlugin;
using Windows.Storage;
using Windows.Perception.Spatial;
#endif

using Microsoft.MixedReality.OpenXR;

using SolARRpc = Com.Bcom.Solar.Gprc;

namespace Bcom.Solar
{
    [Obsolete]
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
            RM_LEFT_FRONT,
            STEREO
        }

        [HideInInspector]
        public Hl2SensorTypeEditor selectedSensor = Hl2SensorTypeEditor.RM_LEFT_FRONT;

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
            focalX = 366.189453125,
            focalY = 366.47808837890625,
            centerX = 320.0,
            centerY = 240.0,
            distK1 = -0.009463000111281872,
            distK2 = 0.0030129998922348022,
            distP1 = -0.0061690001748502254,
            distP2 = -0.008975000120699406,
            distK3 = 0.0
        };

        private CameraParameters rightFrontDefaultParameters = new CameraParameters()
        {
            name = "RIGHT_FRONT",
            id = 1,
            type = SolARRpc.CameraType.Gray,
            width = 640,
            height = 480,
            focalX = 366.189453125,
            focalY = 366.47808837890625,
            centerX = 320.0,
            centerY = 240.0,
            distK1 = -0.009463000111281872,
            distK2 = 0.0030129998922348022,
            distP1 = -0.0061690001748502254,
            distP2 = -0.008975000120699406,
            distK3 = 0.0
        };

        public struct RotationMatrix
        {
            public float m00;
            public float m01;
            public float m02;
            public float m10;
            public float m11;
            public float m12;
            public float m20;
            public float m21;
            public float m22;
        }
        public struct ProjectionMatrix
        {
            public float m00;
            public float m01;
            public float m02;
            public float m03;
            public float m10;
            public float m11;
            public float m12;
            public float m13;
            public float m20;
            public float m21;
            public float m22;
            public float m23;
        }

        public class CameraRectification
        {
            public RotationMatrix rotation;
            public ProjectionMatrix projection;
            public SolARRpc.StereoType stereoType;
            public float baseline;
        }

        private CameraRectification leftFrontDefaultRectification = new CameraRectification()
        {
                rotation = new RotationMatrix
                {
                    m00 = 0.9993278980255127f,
                    m01 = 0.03256781026721001f,
                    m02 = -0.016826163977384567f,
                    m10 = -0.030504178255796432f,
                    m11 = 0.9933525919914246f,
                    m12 = 0.11099620908498764f,
                    m20 = 0.020329216495156288f,
                    m21 = -0.11040833592414856f,
                    m22 = 0.9936783909797668f,
                },

                projection = new ProjectionMatrix
                {
                    m00 = 366.189453125f,
                    m01 = 0.0f,
                    m02 = 309.883056640625f,
                    m03 = 0.0f,
                    m10 = 0.0f,
                    m11 = 366.189453125f,
                    m12 = 184.3231201171875f,
                    m13 = 0.0f,
                    m20 = 0.0f,
                    m21 = 0.0f,
                    m22 = 1.0f,
                    m23 = 0.0f,
                },
                stereoType = SolARRpc.StereoType.Vertical,
                baseline = 0.1087767705321312f
        };

        private CameraRectification rightFrontDefaultRectification = new CameraRectification()
        {
            rotation = new RotationMatrix
            {
                m00 = 0.9991445541381836f,
                m01 = 0.03613580763339996f,
                m02 = 0.020108524709939957f,
                m10 = -0.037666063755750656f,
                m11 = 0.9959368109703064f,
                m12 = 0.08179932087659836f,
                m20 = -0.017070936039090157f,
                m21 = -0.08248675614595413f,
                m22 = 0.9964459538459778f,
            },

            projection = new ProjectionMatrix
            {
                m00 = 366.189453125f,
                m01 = 0.0f,
                m02 = 309.883056640625f,
                m03 = 0.0f,
                m10 = 0.0f,
                m11 = 366.189453125f,
                m12 = 184.3231201171875f,
                m13 = 39.83290481567383f,
                m20 = 0.0f,
                m21 = 0.0f,
                m22 = 1.0f,
                m23 = 0.0f,
            },
            stereoType = SolARRpc.StereoType.Vertical,
            baseline = 0.1087767705321312f
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

        private SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager relocAndMappingProxy = null;

        private Dictionary<Hl2SensorType, bool> enabledSensors = new Dictionary<Hl2SensorType, bool>();
        private bool longThrow = false;

        private Dictionary<Hl2SensorType, int> sensorIds = new Dictionary<Hl2SensorType, int>();

        public bool isLongThrow()
        {
            return longThrow;
        }

        private bool rpcAvailable = false;

        private Thread fetchFramesThread = null;
        private Thread stopFrameSendingThread = null;

        private Quaternion receivedPoseOrientation;
        private Vector3 receivedPosePosition;
        private bool poseReceived = false;

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

        public event Action<
            SolARRpc.MappingStatus /* mapping status */> OnMappingStatusChanged;

        // TODO(jmhenaff): rename toLog ?
        public event Action<string> OnGrpcError;
        public event Action<string> OnPluginError;
        public event Action<string> OnUnityAppError;

        public event Action
            <SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult>
            OnReceivedPose;

        public event Action<long> OnSentFrame;

        public event Action OnReset;

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

        private void NotifyOnMappingStatusChanged(SolARRpc.MappingStatus mappingStatus)
            => OnMappingStatusChanged?.Invoke(mappingStatus);

        private void NotifyOnPluginError(string message)
            => OnPluginError?.Invoke(message);

        private void NotifyOnUnityAppError(string message)
            => OnUnityAppError?.Invoke(message);

        private void NotifyOnReceivedPose(
            SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
            => OnReceivedPose?.Invoke(result);

        private void NotifyOnSentFrame(long nbSentFrames)
            => OnSentFrame?.Invoke(nbSentFrames);

        private void NotifyOnReset()
            => OnReset?.Invoke();

#if ENABLE_WINMD_SUPPORT
        SpatialCoordinateSystem spatialCoordinateSystem;
#endif

        public SolArCloudHololens2()
        {
            pvParameters = new CameraParameters(pvDefaultParameters);
            leftFrontParameters = new CameraParameters(leftFrontDefaultParameters);
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
                UpdateEnabledSensors(selectedSensor);
            }
            catch (Exception e)
            {
                NotifyOnUnityAppError("Error enabling sensor: " + e.Message);
                return;
            }

            // Sensor Id == -1 for disabled devices, >= 0 otherwise
            int enabledSensorId = 0;
            foreach (KeyValuePair<Hl2SensorType, bool> entry in enabledSensors)
            {
                sensorIds.Add(entry.Key, entry.Value ? enabledSensorId++ : -1);
            }
            // Special case for stereo mapping, hard coded for now
            if (selectedSensor == Hl2SensorTypeEditor.STEREO)
            {
                sensorIds[Hl2SensorType.RM_LEFT_FRONT] = 0;
                sensorIds[Hl2SensorType.RM_RIGHT_FRONT] = 1;
            }

#if ENABLE_WINMD_SUPPORT

        spatialCoordinateSystem = PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;
        researchMode.SetSpatialCoordinateSystem(spatialCoordinateSystem);

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

        void OnApplicationQuit()
        {
        //    SaveUserPrefs();

            // Stop services if currently started
            StopSensorsCapture();
        }

        public bool TestRpcConnection()
        {
            var res = relocAndMappingProxy.SendMessage("Hololens 2 requesting connection");
            if (!res.success)
            {
                NotifyOnUnityAppError("Failure in attempt to contact relocalization and mapping service:\n" + res.errMessage + "\n");
            }
            return res.success;
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
        }

        public void ToggleSensorCatpure()
        {
            // Hide/init tracking lost icon
            NotifyOnMappingStatusChanged(SolARRpc.MappingStatus.Bootstrap);

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
            if (fetchFramesThread == null || !fetchFramesThread.IsAlive)
            {
                fetchFramesThread = new Thread(FetchAndSendFramesThread);
                fetchFramesThread.Start();
            }
        }

        public void StopSensorsCapture()
        {
            if (stopFrameSendingThread == null || !stopFrameSendingThread.IsAlive)
            {
                stopFrameSendingThread = new Thread(StopSensorsCaptureThread);
                stopFrameSendingThread.Start();
            }
        }

        public void StopSensorsCaptureThread()
        {
            NotifyOnUnityAppError("StopRGBSensorCaptureEvent");

            try
            {
                sensorsStarted = false;
#if ENABLE_WINMD_SUPPORT
				researchMode.Stop();
#endif
            }
            catch (Exception e)
            {
                Error(ErrorKind.PLUGIN, "Exception occured when stopping plugin", e);
            }

            if (fetchFramesThread != null)
            {
                try
                {
                    fetchFramesThread.Join();
                }
                catch (Exception e)
                {
                    Error(ErrorKind.UNITY, "Exception occured when joining sender thread", e);
                }
                finally
                {
                    fetchFramesThread = null;
                }
            }

            try
            {
                if (relocAndMappingProxy != null)
                {
                    relocAndMappingProxy.Stop();

                    relocAndMappingProxy.UnregisterClient();                    
                }
            }
            catch (Exception e)
            {
                Error(ErrorKind.UNITY, "Exception occured when stopping", e);
            }
            finally
            {
                relocAndMappingProxy = null;
                NotifyOnStop();
            }
        }

#endregion

        private void handleDepth(
            SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
        {
            try
            {
                int width = -1;
                int height = -1;
#if ENABLE_WINMD_SUPPORT
	            width = (int)researchMode.GetDepthWidth();
	            height = (int)researchMode.GetDepthHeight();
#endif
                if (width <= 0 || height <= 0)
                {
                    Error(ErrorKind.PLUGIN, "No value retrieved for depth buffer width and/or height (w:" + width + ", h: " + height + ")");
                    return;
                }

                ulong ts = 0;
                double[] cam2WorldTransform = null;
                uint _width = 0;
                uint _height = 0;
                float _fx = -1;
                float _fy = -1;
                uint _pixelBufferSize = 0;
                ushort[] depthBufferData = null;

#if ENABLE_WINMD_SUPPORT
		        depthBufferData = researchMode.GetDepthData(out ts, out cam2WorldTransform, out _fx, out _fy, out _pixelBufferSize, out _width, out _height);
#endif
                // TODO: update native plugin to produce this SolAR timestamp format
                ts = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();

                if (depthBufferData == null)
                {
                    // Error(ErrorKind.PLUGIN, "Depth buffer is null");
                    return;
                }

                if (depthBufferData.Length == 0)
                {
                    // Error(ErrorKind.PLUGIN, "Depth buffer is empty");
                    return;
                }

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

                NotifyOnDepthFrame(depthData, depthABData, ts, cam2WorldTransform, _width,
                    _height, _fx, _fy, _pixelBufferSize);

                if (rpcAvailable)
                {
                    relocAndMappingFrameSender.AddFrame(sensorIds[Hl2SensorType.RM_DEPTH], ts, SolARRpc.ImageLayout.Grey16,
                        _width, _height, depthData, cam2WorldTransform, SolARRpc.ImageCompression.None);
                }
            }
            catch (Exception e)
            {
                Error(ErrorKind.UNITY, "Error while handling depth buffer: [" + e.GetType().Name + "]", e);
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
                       SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
        {
            try
            {
                int width = -1;
                int height = -1;
#if ENABLE_WINMD_SUPPORT
	            width = (int)researchMode.GetVlcWidth(sensorType);
	            height = (int)researchMode.GetVlcHeight(sensorType);
#endif
                if (width <= 0 || height <= 0)
                {
                    Error(ErrorKind.PLUGIN, "No value retrieved for VLC buffer width and/or height (w:" + width + ", h: " + height + ")");
                    return;
                }

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
                ts = ts / TimeSpan.TicksPerMillisecond;
#endif

                // TODO: update native plugin to produce this SolAR timestamp format
                ts = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();

                if (vclBufferData == null)
                {
                    Error(ErrorKind.PLUGIN, "VLC buffer is null");
                    return;
                }

                if (vclBufferData.Length == 0)
                {
                    Error(ErrorKind.PLUGIN, "VLC buffer is empty");
                    return;
                }

#if ENABLE_WINMD_SUPPORT
                NotifyOnVlcFrame(sensorType, vclBufferData, ts, cam2WorldTransform, _width, _height, _fx, _fy, _pixelBufferSize);
#endif

                if (rpcAvailable)
                {
#if ENABLE_WINMD_SUPPORT                                
                    relocAndMappingFrameSender.AddFrame(
                            sensorIds[ConvertVlcSensorType(sensorType)],
                            ts,
                            SolARRpc.ImageLayout.Grey8,
                            _width,
                            _height,
                            vclBufferData,
                            cam2WorldTransform,
                            advancedGrpcSettings.imageCompression);
#endif
                }
            }
            catch (Exception e)
            {
                Error(ErrorKind.UNITY, "Exception while handling VLC frame: [" + e.GetType().Name + "]", e);
            }
        }

        private void handlePv(
            SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender)
        {
            try
            {
                // update depth map texture
                int width = -1;
                int height = -1;
#if ENABLE_WINMD_SUPPORT
			    width = (int)researchMode.GetPvWidth();
			    height = (int)researchMode.GetPvHeight();
#endif
                if (width <= 0 || height <= 0)
                {
                    Error(ErrorKind.PLUGIN, "No value retrieved for PV buffer width and/or height (w:" + width + ", h: " + height + ")");
                    return;
                }

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
                _timestamp = _timestamp / TimeSpan.TicksPerMillisecond;
#endif
                if (frameTexture == null)
                {
                    // Error(ErrorKind.PLUGIN, "PV buffer is null");
                    return;
                }

                if (frameTexture.Length == 0)
                {
                    // Error(ErrorKind.PLUGIN, "PV buffer is empty");
                    return;
                }

                NotifyOnPvFrame(frameTexture, _timestamp, _PVtoWorldtransform, _width, _height, _fx, _fy, _pixelBufferSize);

                // TODO: update native plugin to produce this SolAR timestamp format
                _timestamp = (ulong)((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();

                if (rpcAvailable)
                {
                    relocAndMappingFrameSender.AddFrame(sensorIds[Hl2SensorType.PV], _timestamp,
                        SolARRpc.ImageLayout.Rgb24, _width, _height, frameTexture,
                        _PVtoWorldtransform, advancedGrpcSettings.imageCompression);
                }
            }
            catch (Exception e)
            {
                Error(ErrorKind.UNITY, "Exception while handling PV frame: [" + e.GetType().Name + "]", e);
            }
        }

        private void relocAndMappingResultReceived(SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
        {
            NotifyOnReceivedPose(result);

            if (result.resultStatus.success)
            {
                // Manage tracking lost icon
                if ((pipelineMode == SolARRpc.PipelineMode.RelocalizationAndMapping) && (sensorsStarted))
                {
                    NotifyOnMappingStatusChanged(result.relocAndMappingResult.MappingStatus);
                }

                if (result.relocAndMappingResult.PoseStatus == SolARRpc.RelocalizationPoseStatus.NoPose)
                {
                    return;
                }                

                if (solarScene != null)
                {
                    var receivedPose = result.relocAndMappingResult.Pose;

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

                    solarDeltaMatinv.m02 *= -1;
                    solarDeltaMatinv.m12 *= -1;
                    solarDeltaMatinv.m22 *= -1;
                    solarDeltaMatinv.m32 *= -1;

                    // Apply modified delta to initial pose of scene
                    var newScenePose = solarDeltaMatinv * solarSceneInitPose;

                    receivedPoseOrientation = newScenePose.ExtractRotation();
                    receivedPosePosition = newScenePose.ExtractPosition();
                    poseReceived = true;
                }
            }
            else
            {                
                
                // Manage tracking lost icon
                if ((pipelineMode == SolARRpc.PipelineMode.RelocalizationAndMapping) && (sensorsStarted))
                {
                    NotifyOnMappingStatusChanged(result.relocAndMappingResult.MappingStatus);
                }

                NotifyOnGrpcError("Error when receiving pose: " + result.resultStatus.errMessage);
/*                
                if (rpcAvailable)
                {
                    relocAndMappingProxy.SendMessage("Error when receiving pose: " +
                        result.resultStatus.errMessage);
                }
*/                
            }
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

        void FetchAndSendFramesThread()
        {
            // Wait for stop thread to complete
            if (stopFrameSendingThread != null)
            {
                stopFrameSendingThread.Join();
                stopFrameSendingThread = null;
            }

            try
            {

                // Check service URL format
                // Warning: only checks IP v4 address (only 2 colons possible, protocol and port)
                // TODO(jmhenaff): handle IP v6
                Uri uri = new Uri(frontendIp.Split(':').Length > 2 ? frontendIp : frontendIp + ":" + frontendBasePort);

                // Warning: only allows IP address, not textual domain names. This is to avoid a bug
                // with gRPC calls with addresses like "xxx.xxx.xxx." which seems to take a lot of resource
                // even after the deadline has expired and the client has been deleted in the manager.
                // TODO(jmhenaff): find a way to allow any well formed URLs without the aforementioned issue.
                System.Net.IPAddress.Parse(uri.Host);

                int _frontendBasePort = uri.Port;
                string _frontEndIp = uri.GetComponents(UriComponents.SchemeAndServer & ~UriComponents.Port, UriFormat.UriEscaped);

                // New instance to force creation of new reusable channels and clients with potentially a different address
                relocAndMappingProxy = new SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.Builder()
                                        .SetServiceAddress(_frontEndIp)
                                        .SetPortBase(_frontendBasePort)
                                        .SetClientPoolSize(advancedGrpcSettings.channelPoolSize)
                                        .UseUniquePortNumber(advancedGrpcSettings.useUniquePort)
                                        // .SetRelocAndMappingRequestIntervalMs(advancedGrpcSettings.delayBetweenFramesInMs)
                                        .Build();


                rpcAvailable = TestRpcConnection();

                if (!rpcAvailable)
                {
                    NotifyOnUnityAppError("Could not connect to RPC service");
                    throw new Exception();
                }

                var res = relocAndMappingProxy.RegisterClient();

                if (!res.success)
                {
                    Error(ErrorKind.GRPC, res.errMessage);
                    throw new Exception(res.errMessage);
                }

                res = relocAndMappingProxy.Init(pipelineMode);
                
                if (!res.success)
                {
                    Error(ErrorKind.GRPC, res.errMessage);
                    throw new Exception();
                }

                // Mono camera mode
                if ((selectedSensor == Hl2SensorTypeEditor.PV) || (selectedSensor == Hl2SensorTypeEditor.RM_LEFT_FRONT))
                {
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

                    if (!res.success)
                    {
                        Error(ErrorKind.GRPC, res.errMessage);
                        throw new Exception();
                    }
                }

                // Stereo camera mode => set rectification parameters for each camera
                if (selectedSensor == Hl2SensorTypeEditor.STEREO)
                {
                    res = relocAndMappingProxy.SetCameraParametersStereo(
                        leftFrontDefaultParameters.name,
                        leftFrontDefaultParameters.id,
                        leftFrontDefaultParameters.type,
                        leftFrontDefaultParameters.width,
                        leftFrontDefaultParameters.height,
                        new double[]
                        {
                                leftFrontDefaultParameters.focalX, 0, leftFrontDefaultParameters.centerX,
                                0, leftFrontDefaultParameters.focalY, leftFrontDefaultParameters.centerY,
                                0, 0, 1
                        },
                        (float)leftFrontDefaultParameters.distK1,
                        (float)leftFrontDefaultParameters.distK2,
                        (float)leftFrontDefaultParameters.distP1,
                        (float)leftFrontDefaultParameters.distP2,
                        (float)leftFrontDefaultParameters.distK3,
                        rightFrontDefaultParameters.name,
                        rightFrontDefaultParameters.id,
                        rightFrontDefaultParameters.type,
                        rightFrontDefaultParameters.width,
                        rightFrontDefaultParameters.height,
                        new double[]
                        {
                                rightFrontDefaultParameters.focalX, 0, rightFrontDefaultParameters.centerX,
                                0, rightFrontDefaultParameters.focalY, rightFrontDefaultParameters.centerY,
                                0, 0, 1
                        },
                        (float)rightFrontDefaultParameters.distK1,
                        (float)rightFrontDefaultParameters.distK2,
                        (float)rightFrontDefaultParameters.distP1,
                        (float)rightFrontDefaultParameters.distP2,
                        (float)rightFrontDefaultParameters.distK3);

                    if (!res.success)
                    {
                        Error(ErrorKind.GRPC, res.errMessage);
                        throw new Exception();
                    }

                    res = relocAndMappingProxy.setRectificationParameters(leftFrontDefaultRectification,
                        rightFrontDefaultRectification);

                    if (!res.success)
                    {
                        Error(ErrorKind.GRPC, res.errMessage);
                        throw new Exception(res.errMessage);
                    }
                }

                res = relocAndMappingProxy.Start();

                if (!res.success)
                {
                    Error(ErrorKind.GRPC, res.errMessage);
                    throw new Exception(res.errMessage);
                }

                try
                {
    #if ENABLE_WINMD_SUPPORT
				    researchMode.Start();
    #endif
                }
                catch (Exception e)
                {
                    Error(ErrorKind.PLUGIN, res.errMessage);
                    throw e;
                }

                sensorsStarted = true;

            }
            catch(Exception e)
            {
                return;
            }
            finally
            {
                NotifyOnStart(sensorsStarted, rpcAvailable);
            }

            try
            {
                while (sensorsStarted)
                {
#if ENABLE_WINMD_SUPPORT
		            researchMode.Update();
#endif
                    SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.FrameSender relocAndMappingFrameSender =
                        relocAndMappingProxy.BuildFrameSender(relocAndMappingResultReceived, SentFrame);

                    // Set mono or stereo mode at frame sender level
                    relocAndMappingFrameSender.setStereoMode(selectedSensor == Hl2SensorTypeEditor.STEREO);

                    // Fetch frames image buffers and poses
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

                    //Send request to SolAR services
                    var res = relocAndMappingFrameSender.RelocAndMapAsyncDrop();

                    if (!res.success)
                    {
                        Error(ErrorKind.GRPC, "RelocAndMapAsyncDrop error: " + res.errMessage);
                    }

                    Thread.Sleep(33);
                }

                // Stopped: wait for remaining frames to be processed
                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Error(ErrorKind.GRPC, "FetchAndSendFramesThread() error", e);
            }
        }
#if ENABLE_WINMD_SUPPORT
        private Hl2SensorType ConvertVlcSensorType(SolARHololens2UnityPlugin.RMSensorType pluginSensorType)
        {
            switch(pluginSensorType)
            {
                case SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT: return Hl2SensorType.RM_LEFT_FRONT;
                case SolARHololens2UnityPlugin.RMSensorType.LEFT_LEFT: return Hl2SensorType.RM_LEFT_LEFT;
                case SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT: return Hl2SensorType.RM_RIGHT_FRONT;
                case SolARHololens2UnityPlugin.RMSensorType.RIGHT_RIGHT: return Hl2SensorType.RM_RIGHT_RIGHT;
                default: throw new Exception("Cannot convert sensor type: not a valid VLC type");
            }
        }
#endif

        public void Reset()
        {
            // RelocAndMappingProxy used to be created when starting the mapping/reloc
            // (in order to take into account the latest value for service address)
            // Need to be created here if null to be able to reset before having used 
            // the service.
            //TODO(jmhenaff): handle this better
            if (relocAndMappingProxy == null)
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

                relocAndMappingProxy = new SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.Builder()
                        .SetServiceAddress(_frontEndIp)
                        .SetPortBase(_frontendBasePort)
                        .SetClientPoolSize(advancedGrpcSettings.channelPoolSize)
                        .UseUniquePortNumber(advancedGrpcSettings.useUniquePort)
                        // .SetRelocAndMappingRequestIntervalMs(advancedGrpcSettings.delayBetweenFramesInMs)
                        .Build();
            }

            relocAndMappingProxy.Reset();
            NotifyOnReset();
        }

        private void Update()
        {
            if (poseReceived)
            {
                // Apply corrected pose to scene
                solarScene.transform.rotation = receivedPoseOrientation;
                solarScene.transform.position = receivedPosePosition;
                solarScene.SetActive(true);
                poseReceived = false;
            }
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

        private void UpdateEnabledSensors(Hl2SensorTypeEditor type)
        {
            switch (type)
            {
                case Hl2SensorTypeEditor.PV:
                    {
                        enabledSensors[Hl2SensorType.PV] = true;
                        break;
                    }
                case Hl2SensorTypeEditor.RM_LEFT_FRONT:
                    {
                        enabledSensors[Hl2SensorType.RM_LEFT_FRONT] = true;
                        break;
                    }
                case Hl2SensorTypeEditor.STEREO:
                    {
                        enabledSensors[Hl2SensorType.RM_LEFT_FRONT] = true;
                        enabledSensors[Hl2SensorType.RM_RIGHT_FRONT] = true;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Unkown Hl2SensorTypeEditor value");
                    }
            }
        }
        enum ErrorKind
        {
            GRPC,
            UNITY,
            PLUGIN
        }

        private void Error(ErrorKind kind, string message, Exception e = null)
        {
            string notifyOnMessage = message + (e != null ? ": " + e.Message : "");
            string rpcMessage = notifyOnMessage + ( e != null ? "\n" + e.StackTrace : "");

            switch(kind)
            {
                case ErrorKind.GRPC: NotifyOnGrpcError(notifyOnMessage); break;
                case ErrorKind.PLUGIN: NotifyOnPluginError(notifyOnMessage); break;
                case ErrorKind.UNITY: NotifyOnUnityAppError(notifyOnMessage); break;
            }
            if (rpcAvailable)
            {
                relocAndMappingProxy.SendMessage(rpcMessage);
            }
        }
    }
}