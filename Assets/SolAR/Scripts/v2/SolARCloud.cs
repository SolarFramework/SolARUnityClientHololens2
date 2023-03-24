using System;
using System.Threading.Tasks;

using UnityEngine;

using Com.Bcom.Solar.Gprc;


namespace Com.Bcom.Solar
{
    public class SolARCloud : MonoBehaviour
    {
        #region Events
        //public Action StartFetchingFrames;
        //public Action StopFetchingFrames;

        // bool: error?, string: message
        public event Action<bool, string> OnStart;
        public event Action OnStop;
        public event Action<bool> OnReset;
        // PipelineMode: old mode, PipelineMode: new mode
        public event Action<PipelineMode, PipelineMode> OnPipelineModeChanged;
        // MappingStatus: old status, MappingStatus: new status
        public event Action<MappingStatus, MappingStatus> OnMappingStatusChanged;
        // PoseStatus: old status, PoseStatus: new status
        public event Action<RelocalizationPoseStatus, RelocalizationPoseStatus> OnPoseStatusChanged;
        public event Action<RelocAndMappingResult> OnReceivedPose;
        public event Action<LogLevel,string> OnLog;
        #endregion // Events


        #region UnityEditor
        [Tooltip("IP address of the SolAR cloud services frontend")]
        public string frontendIp = "<not-set>";

        [Tooltip("Port for SolAR cloud services frontend")]
        public int frontendBasePort = -1;

        [Tooltip("Select to preform both mapping and relocalization or only relocalization")]
        public PipelineMode pipelineMode = PipelineMode.RelocalizationAndMapping;

        [Serializable]
        public struct GrpcSettings
        {
            [Tooltip("Max amount off parallel reception of frames from device")]
            public volatile int threadSlots;
            [Tooltip("Max amount off parallel requests of reloc and/or mapping to SolAR services")]
            public volatile int networkSlots;
            [Tooltip("Select compression method of frame image buffer to save bandwidth")]
            public ImageCompression imageCompression;
        }

        [SerializeField]
        public GrpcSettings advancedGrpcSettings = new GrpcSettings()
        {
            threadSlots = 5,
            networkSlots = 5,
            imageCompression = ImageCompression.Png
        };
        #endregion //UnityEditor


        #region PrivateMembers
        private bool started = false;
        private GrpcManager grpc = null;
        private MappingStatus mappingStatus = MappingStatus.Bootstrap;
        private RelocalizationPoseStatus poseStatus = RelocalizationPoseStatus.NoPose;
        #endregion

        #region UnityMonoBehaviorLifecycle
        void Start()
        {
            LoadUserPrefs();
            //await Connect();
        }

        void OnApplicationQuit()
        {
            SaveUserPrefs();
            //await Disconnect();
        }
        #endregion // UnityMonoBehaviorLifecycle

        #region PublicMethods
        async public Task OnFrameReceived(Frame frame)
        {
            if (frame == null) { /* await grpc.SendMessage("Frame is null")*/ ;  Debug.Log("OnFrameReceived(): frame is null"); return; }

            var frames = new Frames();
            frames.Frames_.Add(frame);
            var result = await grpc.RelocalizeAndMap(frames);
        }

        async public Task OnFrameReceived(Frame frame0, Frame frame1)
        {
            if (frame0 == null || frame1 == null) return;

            var frames = new Frames();
            frames.Frames_.Add(frame0);
            frames.Frames_.Add(frame1);
            var result = await grpc.RelocalizeAndMap(frames);
        }

        private void OnReceivedPoseInternal(RelocAndMappingResult r)
        {
            try
            {
                OnReceivedPose?.Invoke(r);
            }
            catch(Exception e)
            {
                Log(LogLevel.ERROR, $"SolARCloud.OnReceivedPoseInternal(): {e.Message}");
            }


            if (r.Result.MappingStatus != mappingStatus)
            {
                var oldMappingStatus = mappingStatus;
                mappingStatus = r.Result.MappingStatus;
                OnMappingStatusChanged?.Invoke(oldMappingStatus, mappingStatus);
            }

            if (r.Result.PoseStatus != poseStatus)
            {
                var oldPoseStatus = poseStatus;
                poseStatus = r.Result.PoseStatus;
                OnPoseStatusChanged?.Invoke(oldPoseStatus, poseStatus);
            }
        }

        async public Task<bool> Connect()
        {
            if (!await Disconnect()) return false;

            grpc = BuildGrpcManager();
            var result = await grpc.RegisterClient();
            if (!result.Success)
            {
                Log(LogLevel.ERROR, "Error while registering client: " + result.ErrMessage);
                return false;
            }
            grpc.OnResultReceived += OnReceivedPoseInternal;
            return true;
        }

        async public Task<bool> Disconnect()
        {
            try
            {
                if (grpc != null)
                {
                    var res = await grpc.UnRegisterClient();
                    if (!res.Success)
                    {
                        Log(LogLevel.ERROR, "Error while registering client: " + res.ErrMessage);
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                grpc = null;
            }
        }

        public bool TogglePipelineMode()
        {
            if (started)
            {
                Log(LogLevel.ERROR, "Pipeline mode cannot be changed while running");
                return false;
            }

            PipelineMode oldMode = pipelineMode;
            switch (pipelineMode)
            {
                case PipelineMode.RelocalizationAndMapping:
                    {
                        pipelineMode = PipelineMode.RelocalizationOnly;
                        break;
                    }
                case PipelineMode.RelocalizationOnly:
                    {
                        pipelineMode = PipelineMode.RelocalizationAndMapping;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Unkown pipeline mode was selected");
                    }
            }

            OnPipelineModeChanged?.Invoke(oldMode, pipelineMode);

            return true;
        }

        //async private Task<bool> SolARRegisterClient()
        //{
        //    Debug.Log($"SolARRegisterClient()");
        //    var result = await grpc.RegisterClient();
        //    Debug.Log($"SolARRegisterClient() Done ({result.Success}, {result.ErrMessage})");
        //    if (!result.Success) Debug.LogError($"SolARRegisterClient(): {result.ErrMessage}");
        //    return result.Success;
        //}

        //async private Task<bool> SolARUnRegisterClient()
        //{
        //    Debug.Log($"SolARUnRegisterClient()");
        //    var result = await grpc.UnRegisterClient();
        //    Debug.Log($"SolARUnRegisterClient() Done ({result.Success}, {result.ErrMessage})");
        //    if (!result.Success) Debug.LogError($"SolARUnRegisterClient(): {result.ErrMessage}");
        //    return result.Success;
        //}

        async private Task<bool> SolARInit()
        {
            Debug.Log($"SolARInit()");
            var result = await grpc.Init();
            Debug.Log($"SolARInit() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Debug.LogError($"SolARInit(): {result.ErrMessage}");
            return result.Success;
        }

        async private Task<bool> SolARStart()
        {
            Debug.Log($"SolARStart()");
            var result = await grpc.Start();
            Debug.Log($"SolARStart() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARStart(): {result.ErrMessage}");
            OnStart?.Invoke(result.Success, result.ErrMessage);
            return result.Success;
        }

        async public Task<bool> SolARStop()
        {
            Debug.Log($"SolARStop()");
            var result = await grpc.Stop();
            Debug.Log($"SolARStop() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARStop(): {result.ErrMessage}");
            OnStop?.Invoke();
            return result.Success;
        }

        async public Task<bool> SolARReset()
        {
            Debug.Log($"SolARReset()");
            var result = await grpc.Reset();
            Debug.Log($"SolARReset() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARReset(): {result.ErrMessage}");
            OnReset?.Invoke(result.Success);
            return result.Success;
        }

        async public Task<bool> SolARSendMessage(string message)
        {
            Debug.Log($"SolARSendMessage()");
            var result = await grpc.SendMessage(message);
            Debug.Log($"SolARSendMessage() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARSendMessage(): {result.ErrMessage}");
            return result.Success;
        }


        async public Task<RelocAndMappingResult> SolARGet3dTransform()
        {
            var result = await grpc.Get3dTransform();
            return result;
        }

        async private Task<bool> SolARSetCamParameters(CamParameters cp)
        {
            Debug.Log($"SolARSetCamParameters()");
            var result = await grpc.SetCameraParameters(cp);
            Debug.Log($"SolARSetCamParameters() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARSetCamParameters(): {result.ErrMessage}");
            return result.Success;           
        }

        async private Task<bool> SolARSetCamParametersStereo(CamParameters cp1, CamParameters cp2)
        {
            Debug.Log($"SolARSetCamParametersStereo()");
            var result = await grpc.SetCameraParametersStereo(cp1, cp2);
            Debug.Log($"SolARSetCamParametersStereo() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARSetCamParametersStereo(): {result.ErrMessage}");
            return result.Success;
        }

        async private Task<bool> SolARSetRectificationParameters(CamRectification cr1, CamRectification cr2)
        {
            Debug.Log($"SolARSetRectificationParameters()");
            var result = await grpc.SetRectificationParameters(cr1, cr2);
            Debug.Log($"SolARSetRectificationParameters() Done ({result.Success}, {result.ErrMessage})");
            if (!result.Success) Console.WriteLine($"SolARSetRectificationParameters(): {result.ErrMessage}");
            return result.Success;
        }

        async public Task<bool> StartRelocAndMapping(CamParameters cp)
        {
            // if (!await SolARRegisterClient()) return false;
            if (!await SolARInit()) return false;
            if (!await SolARSetCamParameters(cp)) return false;
            if (!await SolARStart()) return false;
            // StartFetchingFrames?.Invoke();
            return true;
        }

        async public Task<bool> StartRelocAndMapping(CamParameters cp1, CamParameters cp2, CamRectification cr1, CamRectification cr2)
        {
            // if (!await SolARRegisterClient()) return false;
            if (!await SolARInit()) return false;
            if (!await SolARSetCamParametersStereo(cp1, cp2)) return false;
            if (!await SolARSetRectificationParameters(cr1, cr2)) return false;
            if (!await SolARStart()) return false;
            // StartFetchingFrames?.Invoke();
            return true;
        }

        async public Task<bool> StopRelocAndMapping()
        {
            bool res = true;
            res = res && await SolARStop();
            // res = res && await SolARUnRegisterClient();
            // StopFetchingFrames?.Invoke();
            return res;
        }

        public bool Isregistered()
        {
            return grpc != null && grpc.IsRegistered();
        }

        #endregion // PublicMethods

        #region PrivateMethods

        private void Log(LogLevel level, string message) => OnLog?.Invoke(level, message);

        private GrpcManager BuildGrpcManager()
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
            return new GrpcManager(_frontEndIp, _frontendBasePort, advancedGrpcSettings.threadSlots, advancedGrpcSettings.networkSlots);
        }

        private void LoadUserPrefs()
        {
            frontendIp = PlayerPrefs.GetString("SolARCloudServicesAddress", frontendIp);
        }

        private async void SaveUserPrefs()
        {
            PlayerPrefs.SetString("SolARCloudServicesAddress", frontendIp);
            PlayerPrefs.Save();
            await Connect();
        }

        #endregion
    }
}


