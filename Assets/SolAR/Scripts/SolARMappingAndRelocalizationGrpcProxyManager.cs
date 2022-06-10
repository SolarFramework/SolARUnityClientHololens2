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

using static UnityEngine.Debug;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Google.Protobuf;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Experimental.Rendering;

using SolARMappingAndRelocalizationProxyClient =
    Com.Bcom.Solar.Gprc.SolARMappingAndRelocalizationProxy.SolARMappingAndRelocalizationProxyClient;

namespace Com.Bcom.Solar.Gprc
{

    public class SolARMappingAndRelocalizationGrpcProxyManager
    {
        private string gRpcAddress = "<not-set>";

        private Empty EMPTY = new Empty();
        private ResultStatus SUCCESS = new ResultStatus(true, "");

        private Task<ResultStatus> rpcTask = null;

        private long relocAndMappingRequestIntervalMs = -1;
        private System.Diagnostics.Stopwatch relocAndMapStopWatch = new System.Diagnostics.Stopwatch();
        private bool fastModeEnabled;

        private int gRpcDeadlineInS = 2;

        private GrpcClientPool clientPool;

        private long nbSentFrames = 0;

        public class FrameSender
        {
            private SolARMappingAndRelocalizationGrpcProxyManager manager;
            // TODO(jmhenaff): Use a Frames object to handle multiple sensor frames (e.g. to support stereo)
            private Frame frame;
            private Action<RelocAndMappingResult> poseReceivedCallback;

            private Action<long> frameSentCallback;

            public FrameSender(SolARMappingAndRelocalizationGrpcProxyManager manager,
                Action<RelocAndMappingResult> poseReceivedCallback, Action<long> frameSentCallback)
            {
                this.manager = manager;
                this.poseReceivedCallback = poseReceivedCallback;
                this.frameSentCallback = frameSentCallback;
            }

            private byte[] encodeToPNG(UInt32 width, UInt32 height, ImageLayout imLayout, byte[] originalImage)
            {
                UnityEngine.TextureFormat textureFormat;
                switch (imLayout)
                {
                    case ImageLayout.Rgb24:
                        textureFormat = TextureFormat.BGRA32; break;
                    case ImageLayout.Grey8:
                        textureFormat = TextureFormat.R8; break;
                    case ImageLayout.Grey16:
                        textureFormat = TextureFormat.R16; break;
                    default: throw new ArgumentException("Unknown ImageLayout");
                }


                // Create a texture the size of the image, using given texture format
                Texture2D tex = new Texture2D((int)width, (int)height, textureFormat, false);

                tex.hideFlags = HideFlags.HideAndDontSave;

                // Fill texture pixels with original image
                tex.LoadRawTextureData(originalImage);
                tex.Apply();

                // Flip texture before encoding
                FlipTextureVertically(tex);

                // Encode texture into PNG
                byte[] bytes = tex.EncodeToPNG();

                UnityEngine.Object.Destroy(tex);

                return bytes;
            }

            public static void FlipTextureVertically(Texture2D original)
            {
                var originalPixels = original.GetPixels();

                var newPixels = new Color[originalPixels.Length];

                var width = original.width;
                var rows = original.height;

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < rows; y++)
                    {
                        newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
                    }
                }

                original.SetPixels(newPixels);
                original.Apply();
            }

            private byte[] convertToPNG(ImageLayout imLayout,
              uint imWidth, uint imHeight, byte[] imData)
            {
                GraphicsFormat format;
                switch (imLayout)
                {
                    case ImageLayout.Rgb24: format = GraphicsFormat.B8G8R8A8_UNorm; break;
                    case ImageLayout.Grey8: format = GraphicsFormat.R8_UNorm; break;
                    case ImageLayout.Grey16: format = GraphicsFormat.R16_UNorm; break;
                    default: throw new ArgumentException("Unkown image layout");
                }

                return UnityEngine.ImageConversion.EncodeArrayToPNG(imData, format, imWidth, imHeight);
            }

            public void SetFrame(int sensorId, ulong timestamp, ImageLayout imLayout,
                uint imWidth, uint imHeight, byte[] imData, double[] pose, ImageCompression imageCompression)
            {
                frame = new Frame
                {
                    SensorId = sensorId,
                    Timestamp = timestamp,
                    Image = new Image
                    {
                        Layout = imLayout,
                        Width = imWidth,
                        Height = imHeight,
                        Data = ByteString.CopyFrom(imData),
                        ImageCompression = imageCompression
                    },
                    Pose = new Matrix4x4
                    {
                        M11 = (float)pose[0],
                        M12 = (float)pose[1],
                        M13 = (float)pose[2],
                        M14 = (float)pose[3],

                        M21 = (float)pose[4],
                        M22 = (float)pose[5],
                        M23 = (float)pose[6],
                        M24 = (float)pose[7],

                        M31 = (float)pose[8],
                        M32 = (float)pose[9],
                        M33 = (float)pose[10],
                        M34 = (float)pose[11],

                        M41 = (float)pose[12],
                        M42 = (float)pose[13],
                        M43 = (float)pose[14],
                        M44 = (float)pose[15]
                    }
                };
            }

            public ResultStatus RelocalizeAndMap()
            {
                try
                {
                    if (frame != null)
                    {
                        poseReceivedCallback(manager.RelocalizeAndMap(frame));
                    }
                }
                catch (Exception e)
                {
                    poseReceivedCallback(new RelocAndMappingResult(
                        new ResultStatus(false, "Reloc and map threw an exception: " + e.Message),
                        null));
                }
                return manager.SUCCESS;
            }

            // TODO(jmhenaff): use void return type ? (all could be handled via the callback method)
            public ResultStatus RelocAndMapAsyncDrop()
            {
                //manager.SendMessage("*********************");
                //manager.SendMessage("manager.rpcTask: " + manager.rpcTask);
                //if (manager.rpcTask != null)
                //{
                //    manager.SendMessage("manager.rpcTask.IsCompleted: " + manager.rpcTask.IsCompleted);
                //}
                //manager.SendMessage("stopWatch.IsRunning: " + manager.relocAndMapStopWatch.IsRunning);
                //manager.SendMessage("stopWatch.ElapsedMilliseconds: " + manager.relocAndMapStopWatch.ElapsedMilliseconds);
                //manager.SendMessage("TOTAL: " + (manager.rpcTask == null || manager.rpcTask.IsCompleted || !manager.relocAndMapStopWatch.IsRunning || manager.relocAndMapStopWatch.ElapsedMilliseconds > manager.relocAndMappingRequestIntervalMs));
                //manager.SendMessage("*********************");

                if (manager.rpcTask == null
                    || manager.rpcTask.IsCompleted
                    || (manager.fastModeEnabled && (!manager.relocAndMapStopWatch.IsRunning
                                                    || manager.relocAndMapStopWatch.ElapsedMilliseconds > manager.relocAndMappingRequestIntervalMs)))
                {
                    if (manager.fastModeEnabled)
                    {
                        if (manager.relocAndMapStopWatch.IsRunning)
                        {
                            manager.relocAndMapStopWatch.Reset();
                        }
                        manager.relocAndMapStopWatch.Start();
                    }

                    if (manager.rpcTask != null && manager.rpcTask.IsCompleted)
                    {
                        if (manager.rpcTask.Exception != null)
                        {
                            manager.rpcTask = null;
                            return new ResultStatus(false, "Reloc and mapping async task threw an exception");
                        }
                    }

                    manager.rpcTask = RelocAndMapAsyncDropInternal();

                    frameSentCallback(++manager.nbSentFrames);
                }

                return manager.SUCCESS;
            }

            private async Task<ResultStatus> RelocAndMapAsyncDropInternal()
            {
                RelocAndMappingResult result = null;
                await Task.Run(() =>
                    {
                        try
                        {
                            if (frame != null)
                            {
                                result = manager.RelocalizeAndMap(frame);
                            }
                            else
                            {
                                result = new RelocAndMappingResult(
                                    new ResultStatus(false, "Given frame is null"),
                                    null);
                            }
                        }
                        catch(Exception e)
                        {
                            result = new RelocAndMappingResult(
                                new ResultStatus(false, "Reloc and map asyn task threw an exception: " + e.Message),
                                null);
                        }
                    }
                );

                try
                {
                    poseReceivedCallback(result);
                    return manager.SUCCESS;
                }
                catch (Exception e)
                {
                    return new ResultStatus(false, "Reloc and map asyn task threw an exception: " +
                        e.Message + "\n"
                        + e.StackTrace);
                }
            }

        }

        // Pool of gRPC channel as suggested here:
        // https://grpc.io/docs/guides/performance/#general
        // Hopefully this will allow to address issues encountered in fastMode with only
        // one channel where all message were sent.
        // This pool also guaranties that only one channel will only be used by a call at a time.
        private class GrpcClientPool
        {
            private class ClientInfo
            {
                public GrpcChannel channel = null;
                public bool available = false;
                public string address = "<not-set>";
            }

            private Dictionary<SolARMappingAndRelocalizationProxyClient, ClientInfo> pool =
                new Dictionary<SolARMappingAndRelocalizationProxyClient, ClientInfo>();

            private object lockObject = new object();
            public GrpcClientPool(int size, string serviceAddress, int portBase, bool useUniquePortNumber)
            {
                for (int i = 0; i < size; i++)
                {
                    int port = useUniquePortNumber ? portBase : portBase + i;
                    AddClient(serviceAddress + ":" + port);
                }
            }

            public SolARMappingAndRelocalizationProxyClient GetClient()
            {
                lock (lockObject)
                {
                    foreach (KeyValuePair<SolARMappingAndRelocalizationProxyClient, ClientInfo> entry in pool)
                    {
                        if (entry.Value.available)
                        {
                            entry.Value.available = false;
                            return entry.Key;
                        }
                    }
                    return null;
                }
            }

            public bool ReleaseClient(SolARMappingAndRelocalizationProxyClient client)
            {
                lock (lockObject)
                {
                    if (!pool.ContainsKey(client))
                    {
                        return false;
                    }
                    pool[client].available = true;
                    return true;
                }
            }

            public void DeleteClient(SolARMappingAndRelocalizationProxyClient client)
            {
                lock (lockObject)
                {
                    if (client != null && pool.ContainsKey(client))
                    {
                        ClientInfo clientInfo = pool[client];
                        clientInfo.channel.Dispose();
                        pool.Remove(client);
                        AddClient(clientInfo.address);
                    }
                }
            }

            private SolARMappingAndRelocalizationProxyClient AddClient(string channelAddress)
            {
                var newChannel = GrpcChannel.ForAddress(
                    channelAddress,
                    new GrpcChannelOptions
                    {
                        HttpHandler = new GrpcWebHandler(new HttpClientHandler())
                        {
                            HttpVersion = new Version("1.1")
                        }
                    });

                var client = new SolARMappingAndRelocalizationProxyClient(newChannel);

                pool.Add(
                    new SolARMappingAndRelocalizationProxyClient(newChannel),
                    new ClientInfo()
                    {
                        channel = newChannel,
                        available = true,
                        address = channelAddress
                    });

                return client;
            }
        }

        public class ResultStatus
        { 
            public bool success = true;
            public string errMessage = "";

            public ResultStatus(bool success, string errMessage)
            {
                this.success = success;
                this.errMessage = errMessage;
            }
        }

        public class RelocAndMappingResult
        {
            public ResultStatus resultStatus;
            public RelocalizationResult relocAndMappingResult;

            public RelocAndMappingResult(ResultStatus resultStatus, RelocalizationResult relocAndMappingResult)
            {
                this.resultStatus = resultStatus;
                this.relocAndMappingResult = relocAndMappingResult;
            }
        }

        public class Builder
        {
            private string serviceAddress = "";
            private int portBase = 5000;
            private bool fastModeEnabled = true;
            private int clientPoolSize = 6;
            private bool useUniquePortNumber = false;
            private int relocAndMappingRequestIntervalMs = 0;

            public Builder SetServiceAddress(string serviceAddress)
            {
                this.serviceAddress = serviceAddress;
                return this;
            }
            public Builder SetPortBase(int portBase)
            {
                this.portBase = portBase;
                return this;
            }

            public Builder UseUniquePortNumber(bool useUniquePortNumber)
            {
                this.useUniquePortNumber = useUniquePortNumber;
                return this;
            }

            public Builder EnableFastMode()
            {
                fastModeEnabled = true;
                return this;
            }
            public Builder DisableFastMode()
            {
                fastModeEnabled = false;
                return this;
            }

            public Builder SetClientPoolSize(int size)
            {
                clientPoolSize = size;
                return this;
            }

            public Builder SetRelocAndMappingRequestIntervalMs(int relocAndMappingRequestIntervalMs)
            {
                this.relocAndMappingRequestIntervalMs = relocAndMappingRequestIntervalMs;
                return this;
            }

            public SolARMappingAndRelocalizationGrpcProxyManager Build()
            {
                if (serviceAddress == "")
                {
                    return null;
                }

                return new SolARMappingAndRelocalizationGrpcProxyManager(serviceAddress, portBase, useUniquePortNumber,
                fastModeEnabled, clientPoolSize, relocAndMappingRequestIntervalMs);
            }
        }

        private SolARMappingAndRelocalizationGrpcProxyManager(string serviceAddress, int portBase,
            bool useUniquePortNumber, bool fastModeEnabled, int clientPoolSize, int relocAndMappingRequestIntervalMs)
        {
            this.gRpcAddress = serviceAddress; // remove?
            this.fastModeEnabled = fastModeEnabled;
            this.clientPool = new GrpcClientPool(clientPoolSize, serviceAddress, portBase, useUniquePortNumber);
            this.relocAndMappingRequestIntervalMs = relocAndMappingRequestIntervalMs;
        }

        public ResultStatus Init()
        {
            return Init(PipelineMode.RelocalizationAndMapping);
        }

        public ResultStatus Init(PipelineMode pipelineMode)
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call Init(): no gRPC client available");
                }

                client.Init(new PipelineModeValue()
                            {
                                PipelineMode = pipelineMode
                            },
                            deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::init(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);  
            }

            return SUCCESS;
        }

        public ResultStatus Start()
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call Start(): no gRPC client available");
                }

                client.Start(EMPTY,
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::Start(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
            }

            return SUCCESS;
        }

        public ResultStatus Stop()
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call Stop(): no gRPC client available");
                }

                client.Stop(EMPTY,
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::Stop(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
            }

            return SUCCESS;
        }

        public ResultStatus SetCameraParameters(string camName, uint camId, CameraType camType,
            uint width, uint height, double[] intrisincs, float distortion_k1, float distortion_k2,
            float distortion_p1, float distortion_p2, float distortion_k3)
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call SetCameraParameters(): no gRPC client available");
                }

                client.SetCameraParameters(new CameraParameters
                {
                    Name = camName,
                    Id = camId,
                    CameraType = camType,
                    Width = width,
                    Height = height,
                    Intrinsics = new Matrix3x3
                    {
                        M11 = (float)intrisincs[0],
                        M12 = (float)intrisincs[1],
                        M13 = (float)intrisincs[2],

                        M21 = (float)intrisincs[3],
                        M22 = (float)intrisincs[4],
                        M23 = (float)intrisincs[5],

                        M31 = (float)intrisincs[6],
                        M32 = (float)intrisincs[7],
                        M33 = (float)intrisincs[8]
                    },
                    Distortion = new CameraDistortion
                    {
                        K1 = distortion_k1,
                        K2 = distortion_k2,
                        P1 = distortion_p1,
                        P2 = distortion_p2,
                        K3 = distortion_k3
                    }
                },
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::Stop(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
            }

            return SUCCESS;
        }

        public RelocAndMappingResult RelocalizeAndMap(int sensorId, ulong timestamp, ImageLayout imLayout,
            uint imWidth, uint imHeight, byte[] imData, float[] pose)
        {

            return RelocalizeAndMap(new Frame
            {
                SensorId = sensorId,
                Timestamp = timestamp,
                Image = new Image
                    {
                        Layout = imLayout,
                        Width = imWidth,
                        Height = imHeight,
                        Data = ByteString.CopyFrom(imData)
                    },
                Pose = new Matrix4x4
                    {
                        M11 = (float)pose[0],
                        M12 = (float)pose[1],
                        M13 = (float)pose[2],
                        M14 = (float)pose[3],

                        M21 = (float)pose[4],
                        M22 = (float)pose[5],
                        M23 = (float)pose[6],
                        M24 = (float)pose[7],

                        M31 = (float)pose[8],
                        M32 = (float)pose[9],
                        M33 = (float)pose[10],
                        M34 = (float)pose[11],

                        M41 = (float)pose[12],
                        M42 = (float)pose[13],
                        M43 = (float)pose[14],
                        M44 = (float)pose[15]
                    }
            }
            );
        }

        private byte[] applyCompression(ImageLayout imLayout, uint imWidth, uint imHeight, byte[] imData, ImageCompression imageCompression)
        {
            GraphicsFormat format;
            switch (imLayout)
            {
                case ImageLayout.Rgb24: format = GraphicsFormat.B8G8R8A8_UNorm; break;
                case ImageLayout.Grey8: format = GraphicsFormat.R8_UNorm; break;
                case ImageLayout.Grey16: format = GraphicsFormat.R16_UNorm; break;
                default: throw new ArgumentException("Unkown image layout");
            }

            switch(imageCompression)
            {
                case ImageCompression.Png: return UnityEngine.ImageConversion.EncodeArrayToPNG(imData, format, imWidth, imHeight);
                case ImageCompression.Jpg: return UnityEngine.ImageConversion.EncodeArrayToJPG(imData, format, imWidth, imHeight);
                case ImageCompression.None: throw new ArgumentException("None should not be used here"); // return imData;
                default: throw new ArgumentException("Unknown image compression");

            }           
        }

        private RelocAndMappingResult RelocalizeAndMap(Frame frame)
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    var resStatus = makeErrorResult("Cannot call RelocalizeAndMap(): no gRPC client available");
                    return new RelocAndMappingResult(resStatus, null);
                }

                switch (frame.Image.ImageCompression)
                {
                    case ImageCompression.None:
                        {
                            // Do nothing
                            break;
                        }
                    case ImageCompression.Png:
                    case ImageCompression.Jpg:
                        {
                            frame.Image.Data = ByteString.CopyFrom(
                                applyCompression(
                                    frame.Image.Layout, 
                                    frame.Image.Width,
                                    frame.Image.Height,
                                    frame.Image.Data.ToByteArray(),
                                    frame.Image.ImageCompression));
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("Unkown Image compression kind");
                        }
                }

                // System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                // stopWatch.Start();
                var result = client.RelocalizeAndMap(frame,
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));
                // stopWatch.Stop();
                // SendMessage("RelocalizeAndMap duration: " + stopWatch.ElapsedMilliseconds);

                clientPool.ReleaseClient(client);

                return new RelocAndMappingResult(SUCCESS, result);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                var resStatus = makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::RelocalizeAndMap(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
                return new RelocAndMappingResult(resStatus, null);
            }
        }

        public ResultStatus Get3dTransform()
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call Get3dTransform(): no gRPC client available");
                }


                client.Get3DTransform(EMPTY,
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::Get3DTransform(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
            }

            return SUCCESS;
        }


        public ResultStatus Reset()
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call Reset(): no gRPC client available");
                }

                client.Reset(
                    EMPTY,
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::Reset(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
            }
            catch (Exception e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::Reset(): Error: "
                    + e.Message);
            }

            return SUCCESS;

        }

        public ResultStatus SendMessage(string message)
        {
            SolARMappingAndRelocalizationProxyClient client = null;
            try
            {
                client = clientPool.GetClient();
                if (client == null)
                {
                    return new ResultStatus(false, "Cannot call SendMessage(): no gRPC client available");
                }


                client.SendMessage(
                    new Message { Message_ = message},
                    deadline: DateTime.UtcNow.AddSeconds(gRpcDeadlineInS));

                clientPool.ReleaseClient(client);
            }
            catch (Grpc.Core.RpcException e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::SendMessage(): Error: "
                    + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode);
            }
            catch(Exception e)
            {
                clientPool.DeleteClient(client);
                return makeErrorResult("SolARMappingAndRelocalizationGrpcProxyManager::SendMessage(): Error: "
                    + e.Message);
            }

            return SUCCESS;
        }

        public FrameSender BuildFrameSender(Action<RelocAndMappingResult> poseReceivedCallback,
            Action<long> sentFrameCallback)
        {
            return new FrameSender(this, poseReceivedCallback, sentFrameCallback);
        }

        private ResultStatus makeErrorResult(string message)
        {
            LogError(message);
            return new ResultStatus(false, message);
        }

    }
} // namespace Com.Bcom.Solar.Gprc

