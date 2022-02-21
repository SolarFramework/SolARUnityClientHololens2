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

using static UnityEngine.Debug;

using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using System.Net.Http;
using Google.Protobuf;

using System;
using System.Threading.Tasks;
using Bcom.Solar.Cloud.Rpc;

public class SolarRpcClient
{

    public string gRpcAddress = "http://192.168.137.1:5001";

    private string result;
    public string Result
    {
        get => result;
    }

    private bool newPoseCorrectionAvailable = false;
    private Bcom.Solar.Cloud.Rpc.Pose poseCorrection;

    private GrpcChannel channel = null;
    private SolARCloudProxy.SolARCloudProxyClient client = null;
    private Task<string> rpcTask = null;
    private ulong index = 0;

    public class FrameSender
    {
        private Bcom.Solar.Cloud.Rpc.Frames frames = new Bcom.Solar.Cloud.Rpc.Frames();
        private SolarRpcClient manager = null;

        private Action<Bcom.Solar.Cloud.Rpc.Pose> poseReceivedCallback;

        public FrameSender(SolarRpcClient manager, Action<Bcom.Solar.Cloud.Rpc.Pose> poseReceivedCallback)
        {
            this.manager = manager;
            this.poseReceivedCallback = poseReceivedCallback;
        }


        public void addFrame(int sensorId, double timestamp, int width, int height, Bcom.Solar.Cloud.Rpc.ImageLayout layout, byte[] image, double[] pose)
        {

            var frame = new Bcom.Solar.Cloud.Rpc.Frame
            {
                SensorId = sensorId,
                Timestamp = timestamp,
                Image = new Bcom.Solar.Cloud.Rpc.Image
                {
                    Layout = layout,
                    Width = width,
                    Height = height,
                    Data = ByteString.CopyFrom(image)
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
                    M44 = (float)pose[15],
                }
            };
            frames.Frames_.Add(frame);
        }

        public void SendAsyncDropFrames()
        {
            if (manager.rpcTask == null || manager.rpcTask.IsCompleted)
            {
                if (manager.rpcTask != null)
                {
                    if (manager.rpcTask.Exception != null)
                    {
                        manager.ResetChannel();
                        manager.result = "Canceled or faulted";
                    }
                    else
                    {
                        try
                        {
                            manager.result = manager.rpcTask.Result;
                        }
                        catch (Exception e)
                        {
                            // Should not happen thanks to check of rpcTask.Exception
                            manager.result = "ex2: " + e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace;
                            // Debug.Log("Exception occured when retrieving rpc response (VLC): " + e.Message);
                        }
                    }

                }
                manager.rpcTask = SendFramesAsync();
            }
        }

        public async Task<string> SendFramesAsync()
        {
            if (frames.Frames_.Count == 0)
            {
                return "empty";
            }

            String result = "";
            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                sw.Start();

                var client = manager.GetGrpcClient();

                Bcom.Solar.Cloud.Rpc.Pose response = null;

                manager.result = "client.Send(rpcFrames)) (count: " + frames.Frames_.Count + ")";
                frames.Index = manager.index++;
                await Task.Run(() => response = client.SendFrames(frames));
                sw.Stop();

                poseReceivedCallback(response);

                return sw.Elapsed.ToString();
            }
            catch (Grpc.Core.RpcException e)
            {
                LogError("SendFramesAsync() - An exception occured:" + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "");
                manager.ResetChannel();
                result = "testSendImages(): " + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "";
            }

            return result;
        }

        /*
                public async Task<string> SendFramesAsync2()
                {
                    if (frames.Frames_.Count == 0)
                    {
                        return "empty";
                    }

                    String result = "";
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                        sw.Start();

                        var client = manager.GetGrpcClient();

                        Bcom.Solar.Cloud.Rpc.Pose response = null;

                        manager.result = "client.Send(rpcFrames)) (count: " + frames.Frames_.Count + ")";
                        frames.Index = manager.index++;
                        response = await client.SendFramesAsync(frames, deadline: DateTime.UtcNow.AddSeconds(5));
                        sw.Stop();

                        return sw.Elapsed.ToString();
                    }
                    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.DeadlineExceeded)
                    {
                        result = "Timeout";
                    }
                    catch (Grpc.Core.RpcException e)
                    {
                        // Debug.Log("gRPC: exception! :" + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "");
                        manager.ResetChannel();
                        result = "testSendImages(): " + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "";
                    }

                    return result;
                }

                public string sendToSolar()
                {
                    if (frames.Frames_.Count == 0)
                    {
                        return "empty";
                    }

                    String result = "";
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                        sw.Start();
                        var client = manager.GetGrpcClient();
                        Bcom.Solar.Cloud.Rpc.Pose response = null;
                        frames.Index = manager.index++;
                        response = client.SendFrames(frames);
                        sw.Stop();

                        return sw.Elapsed.ToString();
                    }
                    catch (Grpc.Core.RpcException e)
                    {
                        // Debug.Log("gRPC: exception! :" + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "");
                        manager.ResetChannel();
                        result = "testSendImages(): " + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "";
                    }

                    return result;
                }
        */
    }

    public FrameSender BuildFrameSender(Action<Bcom.Solar.Cloud.Rpc.Pose> poseReceivedCallback)
    {
        return new FrameSender(this, poseReceivedCallback);
    }

    public Tuple<bool, string> Ping()
    {
        String result = "";
        try
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            var client = GetGrpcClient();

            for (int i = 0; i < 10; i++)
            {
                sw.Reset();
                sw.Start();
                client.Ping(new Empty());
                sw.Stop();
                result = sw.Elapsed.ToString();
                SendMessage(result);
            }
            return new Tuple<bool, string>(true, "Ping OK (" + result + "ms)");

        }
        catch (Grpc.Core.RpcException e)
        {
            // Debug.Log("gRPC: exception! :" + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "");
            ResetChannel();
            result = "Ping(): " + e.Message + "(status: " + e.Status.Detail + ", code: " + e.StatusCode + "";
            //SendMessage(result);
            return new Tuple<bool, string>(false, "Ping failed (" + result + ")");
        }
    }

    public void SendMessage(string message)
    {
        GetGrpcClient().SendMessage(new Bcom.Solar.Cloud.Rpc.Message { Message_ = message });
    }

    private GrpcChannel GetGrpcChannel()
    {
        if (channel == null)
        {
            // channel = GrpcChannel.ForAddress(gRpcAddress);
            channel = GrpcChannel.ForAddress(gRpcAddress, new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler())
                {
                    HttpVersion = new Version("1.1")
                }
            });
        }

        return channel;
    }

    private SolARCloudProxy.SolARCloudProxyClient GetGrpcClient()
    {
        if (client == null)
        {
            client = new Bcom.Solar.Cloud.Rpc.SolARCloudProxy.SolARCloudProxyClient(GetGrpcChannel());
        }
        return client;
    }

    private void ResetChannel()
    {
        channel.Dispose();
        channel = null;
        client = null;
    }
}
