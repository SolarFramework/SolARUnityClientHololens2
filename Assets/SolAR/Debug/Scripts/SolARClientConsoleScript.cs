/**
 * @copyright Copyright (c) 2022 B-com http://www.b-com.com/
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
using TMPro;
using UnityEngine;

public class SolARClientConsoleScript : MonoBehaviour
{
    public enum LogType
    {
        GRPC,
        UNITY,
        PLUGIN
    }

    public Bcom.Solar.SolArCloudHololens2 client;
    public TMPro.TextMeshPro title;
    public TMPro.TextMeshProUGUI text;
    public LogType logType = LogType.UNITY;
    private Queue<string> logsQueue = new Queue<string>();
    private int logsNbMaxElements = 10;
    private bool hasChanged = false;

    void Start()
    {
        switch (logType)
        {
            case LogType.GRPC:
                {
                    client.OnGrpcError += OnLogReceived;
                    title.text += " - gRPC";
                    break;
                }
            case LogType.UNITY:
                {
                    client.OnUnityAppError += OnLogReceived;
                    title.text += " - Unity";
                    break;
                }
            case LogType.PLUGIN:
                {
                    client.OnPluginError += OnLogReceived;
                    title.text += " - plugin";
                    break;
                }
            default:
                {
                    throw new Exception("Unkown LogType");
                }
        }
    }
    void Update()
    {
        if (hasChanged)
        {
            hasChanged = false;
            UpdateLogsText(text, logsQueue);
        }
    }

    public void OnLogReceived(string message)
    {
        AddLogToQueue(logsQueue, message);
        hasChanged = true;
    }

    private void AddLogToQueue(Queue<string> q, string m)
    {
        q.Enqueue(m);
        if (q.Count > logsNbMaxElements)
        {
            q.Dequeue();
        }
    }

    private void UpdateLogsText(TextMeshProUGUI text, Queue<string> messagesQueue)
    {
        text.text = "";
        if (messagesQueue.Count > 0)
        {
            text.text = String.Join("\n", messagesQueue.ToArray());
        }
    }

    public void ClearLogs()
    {
        text.text = "";
        logsQueue.Clear();
    }

}
