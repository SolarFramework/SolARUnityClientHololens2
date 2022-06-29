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

using UnityEngine;

using Microsoft.MixedReality.Toolkit.UI;
using Bcom.Solar;

public class StartStopButtonAppearanceHandler : MonoBehaviour
{
    public ButtonConfigHelper startStopButtonConfigHelper;
    public SolArCloudHololens2 solArCloudHololens2;

    private string label;
    private bool labelChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        solArCloudHololens2.OnStart += OnStart;
        solArCloudHololens2.OnStop += OnStop;
    }

    void Update()
    {
        if (labelChanged)
        {
            startStopButtonConfigHelper.MainLabelText = label;
            labelChanged = false;
        }
    }

    public void ToggleSensorCatpure()
    {
        solArCloudHololens2.ToggleSensorCatpure();
    }

    private void OnStart(bool sensorsStarted, bool gRpcOk)
    {
        label = sensorsStarted ? "stop" : "start\nerror sensors";
        label += gRpcOk ? "" : "\nerror gRPC";
        labelChanged = true;
    }

    private void OnStop()
    {
        label = "Start";
        labelChanged = true;
    }
}
