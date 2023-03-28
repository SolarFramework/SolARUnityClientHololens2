/**
 * @copyright Copyright (c) 2023 B-com http://www.b-com.com/
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
using System.Threading;

using UnityEngine;

using Google.Protobuf;

using Microsoft.MixedReality.OpenXR;
#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Perception.Spatial;
using SolARHololens2UnityPlugin;
#endif

using Com.BCom.SolAR;
using Com.Bcom.Solar.Gprc;

using SolARRpc = Com.Bcom.Solar.Gprc;
using SysDiag = System.Diagnostics;

namespace Com.Bcom.Solar
{
    
    public class SolARCloudHololens2Specific : MonoBehaviour
    {
        public enum Hl2SensorType
        {
            PV,
            RM_LEFT_FRONT,
            STEREO
        }

        public event Action<LogLevel, string> OnLog;
        public event Action<bool> OnSensorStarted;
        public event Action OnSensorStopped;

        [Tooltip("Top level GO of 3D scene to apply SolAR transform onto")]
        public GameObject solarScene;
        [Tooltip("Select Hololens 2 sensors to capture images")]
        public Hl2SensorType sensorType = Hl2SensorType.RM_LEFT_FRONT;
        [Tooltip("Number of frame fetched from sensors per seconds")]
        public int framerate = 30;
        private SysDiag.Stopwatch stopWatch = new SysDiag.Stopwatch();

        private CamParameters pvDefaultParameters = new CamParameters(
            name: "PV",
            id: 0,
            type: SolARRpc.CameraType.Rgb,
            resolution: new CamResolution(width: 760, height: 428),
            intrisincs: new CamIntrinsics(fx: 592.085f, fy: 592.085f, cx: 371.296f, cy: 203.017f),
            distortion: new CamDistortion(k1: -0.0626f, k2: 0.7265f, p1: -0.006169f, p2: -0.008975f, k3: 0f));

        private CamParameters leftFrontDefaultParameters = new CamParameters(
            name: "LEFT_FRONT",
            id: 0,
            type: SolARRpc.CameraType.Gray,
            resolution: new CamResolution(width: 640, height: 480),
            intrisincs: new CamIntrinsics(fx: 366.189f, fy: 366.478f, cx: 320.0f, cy: 240.0f),
            distortion: new CamDistortion(k1: -0.0094f, k2: 0.0030f, p1: -0.0061f, p2: -0.0089f, k3: 0f));

        private CamParameters rightFrontDefaultParameters = new CamParameters(
            name: "RIGHT_FRONT",
            id: 1,
            type: SolARRpc.CameraType.Gray,
            resolution: new CamResolution(width: 640, height: 480),
            intrisincs: new CamIntrinsics(fx: 366.189f, fy: 366.478f, cx: 320.0f, cy: 240.0f),
            distortion: new CamDistortion(k1: -0.0094f, k2: 0.0030f, p1: -0.0061f, p2: -0.0089f, k3: 0f));

        private CamRectification leftFrontDefaultRectification = new CamRectification(
            rotation: new RotationMatrix {
                m00 = 0.9993278980255127f,    m01 = 0.03256781026721001f,  m02 = -0.016826163977384567f,
                m10 = -0.030504178255796432f, m11 = 0.9933525919914246f,   m12 = 0.11099620908498764f,
                m20 = 0.020329216495156288f,  m21 = -0.11040833592414856f, m22 = 0.9936783909797668f},
            projection: new ProjectionMatrix {
                m00 = 366.189453125f, m01 = 0.0f,           m02 = 309.883056640625f,  m03 = 0.0f,
                m10 = 0.0f,           m11 = 366.189453125f, m12 = 184.3231201171875f, m13 = 0.0f,
                m20 = 0.0f,           m21 = 0.0f,           m22 = 1.0f,               m23 = 0.0f,
            },
            stereoType: SolARRpc.StereoType.Vertical,
            baseline: 0.1087767705321312f);

        private CamRectification rightFrontDefaultRectification = new CamRectification(
            rotation: new RotationMatrix {
                m00 = 0.9991445541381836f,    m01 = 0.03613580763339996f,  m02 = 0.020108524709939957f,
                m10 = -0.037666063755750656f, m11 = 0.9959368109703064f,   m12 = 0.08179932087659836f,
                m20 = -0.017070936039090157f,  m21 = -0.08248675614595413f, m22 = 0.9964459538459778f
            },
            projection: new ProjectionMatrix {
                m00 = 366.189453125f, m01 = 0.0f,           m02 = 309.883056640625f,  m03 = 0.0f,
                m10 = 0.0f,           m11 = 366.189453125f, m12 = 184.3231201171875f, m13 = 39.83290481567383f,
                m20 = 0.0f,           m21 = 0.0f,           m22 = 1.0f,               m23 = 0.0f,
            },
            stereoType: SolARRpc.StereoType.Vertical,
            baseline: 0.1087767705321312f);

        // Use these object to store user modifed values
        // TODO: persistency, load from file, calibrate within app.
        [HideInInspector]
        public CamParameters pvParameters;

        [HideInInspector]
        public CamParameters leftFrontParameters;

        [HideInInspector]
        public CamParameters selectedCameraParameter;

        private SolARCloud solARCloud;
        private Thread fetchFramesThread;
        [HideInInspector]
        public  bool isRunning;

        private UnityEngine.Matrix4x4 solarSceneInitPose;
        private Quaternion receivedPoseOrientation;
        private Vector3 receivedPosePosition;
        private bool poseReceived = false;

#if ENABLE_WINMD_SUPPORT
        private SpatialCoordinateSystem spatialCoordinateSystem;
        private SolARHololens2ResearchMode researchMode;
#endif

        // Start is called before the first frame update
        void Start()
        {

            solARCloud = GetComponent<SolARCloud>();
            if (solARCloud == null)
            {
                Debug.LogError("required SolARCloud component not found");
                return;
            }

            solARCloud.OnReceivedPose += RelocAndMappingResultReceived;

            if (solarScene)
            {
                solarScene.SetActive(false);
                solarSceneInitPose = solarScene.transform.localToWorldMatrix;
            }

            //solARCloud.StartFetchingFrames += StartSensorsCapture;
            //solARCloud.StopFetchingFrames += StopSensorsCapture;


#if ENABLE_WINMD_SUPPORT

            researchMode = new SolARHololens2ResearchMode();

            spatialCoordinateSystem = PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;
            researchMode.SetSpatialCoordinateSystem(spatialCoordinateSystem);

            switch( sensorType )
            {
                case Hl2SensorType.PV:
                {
                    researchMode.EnablePv();
                    break;
                } 
                case Hl2SensorType.RM_LEFT_FRONT:
                {
                    researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT);
                    break;
                }
                case Hl2SensorType.STEREO:
                {
                    researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT);
                    researchMode.EnableVlc(SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT);
                    break;
                }
                default:
                {
                    throw new Exception("Unkown sensor type");
                }
            }

            researchMode.Init();
#endif
        }

        private void Update()
        {
            if (poseReceived)
            {
                // Apply corrected pose to scene
                if (solarScene)
                {
                    solarScene.transform.rotation = receivedPoseOrientation;
                    solarScene.transform.position = receivedPosePosition;
                    solarScene.SetActive(true);
                }
                poseReceived = false;
            }
        }

        public void SetSolARScene(GameObject scene)
        {
            solarScene = scene;
            solarSceneInitPose = scene.transform.localToWorldMatrix;
        }

        public void StartSensorsCapture()
        {
            if (fetchFramesThread == null || !fetchFramesThread.IsAlive)
            {
                fetchFramesThread = new Thread(FetchAndSendFramesThread);
                fetchFramesThread.Start();              
            }
        }

        async public void StopSensorsCapture()
        {
            isRunning = false;

            fetchFramesThread?.Join(2000);
            fetchFramesThread = null;

            await solARCloud.StopRelocAndMapping();

#if ENABLE_WINMD_SUPPORT
			researchMode.Stop();
#endif
            OnSensorStopped?.Invoke();
        }

        async private void FetchAndSendFramesThread()
        {
            bool res;
            switch (sensorType)
            {
                case Hl2SensorType.STEREO:
                    res = await solARCloud.StartRelocAndMapping(leftFrontDefaultParameters,
                                                                rightFrontDefaultParameters,
                                                                leftFrontDefaultRectification,
                                                                rightFrontDefaultRectification); break;
                case Hl2SensorType.RM_LEFT_FRONT:
                    res = await solARCloud.StartRelocAndMapping(leftFrontDefaultParameters); break;
                case Hl2SensorType.PV:
                    res = await solARCloud.StartRelocAndMapping(pvDefaultParameters); break;
                default:
                    throw new ArgumentException($"Unkown sensor type: '{sensorType}'");
            }

            if (!res) { Debug.LogError("Services not started"); ; OnSensorStarted?.Invoke(false); return; }

            Debug.Log("Services started");

            try
            {
#if ENABLE_WINMD_SUPPORT
	            researchMode.Start();
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start sensors: {e.Message}");
                OnSensorStarted?.Invoke(false); return;
            }

            Debug.Log($"Sensors started");

            await solARCloud.SolARSendMessage(
                $"Config: sensor: {sensorType}, " +
                $"threadSlots: {solARCloud.advancedGrpcSettings.threadSlots}, " +
                $"networksSlots: {solARCloud.advancedGrpcSettings.networkSlots}, " +
                $"compression: {solARCloud.advancedGrpcSettings.imageCompression};" +
                $"FPS: {framerate}");
           
            if (framerate > 0)
            {
                stopWatch.Reset();
                stopWatch.Start();
            }
            OnSensorStarted?.Invoke(true);
            int timeBetweenFrames = framerate > 0 ? 1000 / framerate : 0;

            isRunning = true;
            while (isRunning)
            {
                if (framerate > 0)
                {
                    //if (stopWatch.ElapsedMilliseconds < timeBetweenFrames) continue;
                    //stopWatch.Reset();
                    //stopWatch.Start();

                    long sleepDuration = timeBetweenFrames - stopWatch.ElapsedMilliseconds;
                    if (sleepDuration > 0) Thread.Sleep((int)sleepDuration);
                    stopWatch.Reset();
                    stopWatch.Start();
                }

                switch (sensorType)
                {
                    case Hl2SensorType.STEREO:
                        solARCloud.OnFrameReceived(GetLeftFrontVlcFrame(), GetRighFrontFrontVlcFrame()); break;
                    case Hl2SensorType.RM_LEFT_FRONT:
                        solARCloud.OnFrameReceived(GetLeftFrontVlcFrame()); break;
                    case Hl2SensorType.PV:
                        solARCloud.OnFrameReceived(getPvFrame()); break;
                    default:
                        throw new ArgumentException($"Unkown sensor type: '{sensorType}'");
                }
            }
        }

        private void RelocAndMappingResultReceived(RelocAndMappingResult result)
        {
            if (!result.Status.Success)
            {
                Log(LogLevel.ERROR, $"Received error from Relocalization and Mapping request: {result.Status.ErrMessage}");
                return;
            }

            if (result.Result.PoseStatus == RelocalizationPoseStatus.NoPose)
            {
                Log(LogLevel.DEBUG, "Received NoPose");
                return;
            }

            if (solarScene == null)
            {
                Log(LogLevel.DEBUG, "solarScene is not set, nothing to do");
            }

            var receivedPose = result.Result.Pose;

            Log(LogLevel.DEBUG, $"Pose received: {receivedPose}");
            Log(LogLevel.DEBUG, $"Mapping Status: {result.Result.MappingStatus}");
            Log(LogLevel.DEBUG, $"Pose Status: {result.Result.PoseStatus}");

            // SolAR transform * CS conversion
            // https://medium.com/comerge/what-are-the-coordinates-225f1ec0dd78
            var solarDeltaMat = new UnityEngine.Matrix4x4()
            {
                m00 = receivedPose.M11, m01 = receivedPose.M12, m02 = receivedPose.M13, m03 = receivedPose.M14,
                m10 = receivedPose.M21, m11 = receivedPose.M22, m12 = receivedPose.M23, m13 = receivedPose.M24,
                m20 = receivedPose.M31, m21 = receivedPose.M32, m22 = receivedPose.M33, m23 = receivedPose.M34,
                m30 = receivedPose.M41, m31 = receivedPose.M42, m32 = receivedPose.M43, m33 = receivedPose.M44,
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

        private Frame getPvFrame()
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
                    Log(LogLevel.ERROR, $"No value retrieved for PV buffer width and/or height (w: {width}, h: {height})");
                    return null;
                }

                uint _width = 0;
                uint _height = 0;
                ulong _timestamp = 0;
                double[] _PVtoWorldtransform = null;
                byte[] frameTexture = null;
#if ENABLE_WINMD_SUPPORT
                uint _pixelBufferSize = 0;
                float _fx = -1.0f;
                float _fy = -1.0f;
			    frameTexture = researchMode.GetPvData(
                    out _timestamp,
                    out _PVtoWorldtransform,
                    out _fx,
                    out _fy,
                    out _pixelBufferSize,
                    out _width,
                    out _height,
                    /* flip = */ solARCloud.advancedGrpcSettings.imageCompression != SolARRpc.ImageCompression.None);
                _timestamp = _timestamp / TimeSpan.TicksPerMillisecond;
#endif
                if (frameTexture == null)
                {
                    Log(LogLevel.WARNING, "PV buffer is null");
                    return null;
                }

                if (frameTexture.Length == 0)
                {
                    Log(LogLevel.WARNING, "PV buffer is empty");
                    return null;
                }

                return new Frame()
                {
                    SensorId = 0,
                    Timestamp = _timestamp,
                    Image = new Image
                    {
                        Layout = ImageLayout.Rgb24,
                        Width = _width,
                        Height = _height,
                        Data = ByteString.CopyFrom(frameTexture),
                        ImageCompression = solARCloud.advancedGrpcSettings.imageCompression
                    },
                    Pose = Utils.toMatrix4x4(_PVtoWorldtransform)
                };

            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while handling PV frame: {e.Message}");
                Log(LogLevel.ERROR, $"Exception while handling PV frame: {e.Message}");
                return null;
            }
        }

        private Frame GetLeftFrontVlcFrame()
        {
            return getVlcFrame(
#if ENABLE_WINMD_SUPPORT
            SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT
#endif
                );
        }

        private Frame GetRighFrontFrontVlcFrame()
        {
            return getVlcFrame(
#if ENABLE_WINMD_SUPPORT
            SolARHololens2UnityPlugin.RMSensorType.RIGHT_FRONT
#endif
                );
        }

        private Frame getVlcFrame(
#if ENABLE_WINMD_SUPPORT
            SolARHololens2UnityPlugin.RMSensorType sensorType
#endif
            )
        {
            try
            {
                int sensorId = -1;
                int width = -1;
                int height = -1;
#if ENABLE_WINMD_SUPPORT
                sensorId = sensorType == SolARHololens2UnityPlugin.RMSensorType.LEFT_FRONT ? 0 : 1; 
	            width = (int)researchMode.GetVlcWidth(sensorType);
	            height = (int)researchMode.GetVlcHeight(sensorType);
#endif
                if (width <= 0 || height <= 0)
                {
                    Log(LogLevel.ERROR, $"No value retrieved for VLC buffer width and/or height (w: {width}, h: {height})");
                    return null;
                }

                ulong ts = 0;
                double[] cam2WorldTransform = null; ;
                uint _width = 0;
                uint _height = 0;
                byte[] vclBufferData = null;

#if ENABLE_WINMD_SUPPORT
                float _fx = -1f;
                float _fy = -1f;
                uint _pixelBufferSize = 0;
			    vclBufferData = researchMode.GetVlcData(
                    sensorType,
                    out ts,
                    out cam2WorldTransform,
                    out _fx,
                    out _fy,
                    out _pixelBufferSize,
                    out _width,
                    out _height,
                    /* flip = */ solARCloud.advancedGrpcSettings.imageCompression != SolARRpc.ImageCompression.None);
                ts = ts / TimeSpan.TicksPerMillisecond;
#endif
                if (vclBufferData == null)
                {
                    Log(LogLevel.WARNING, "VLC buffer is null");
                    return null;
                }

                if (vclBufferData.Length == 0)
                {
                    Log(LogLevel.WARNING, "VLC buffer is empty");
                    return null;
                }

                return ImageUtils.ApplyCompression(new Frame() {
                    SensorId = sensorId,
                    Timestamp = ts,
                    Image = new Image
                    {
                        Layout = ImageLayout.Grey8,
                        Width = _width,
                        Height = _height,
                        Data = ByteString.CopyFrom(vclBufferData),
                        ImageCompression = solARCloud.advancedGrpcSettings.imageCompression
                    },
                    Pose = Utils.toMatrix4x4(cam2WorldTransform)
                });

            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while handling PV frame: {e.Message}");
                Log(LogLevel.ERROR, $"Exception while handling VLC frame: {e.Message}");
                return null;
            }
        }

        private void Log(LogLevel level, string message)
        {
            // solARCloud?.SolARSendMessage($"{level}: {message}");
            OnLog?.Invoke(level, message);
        }
    }
}

