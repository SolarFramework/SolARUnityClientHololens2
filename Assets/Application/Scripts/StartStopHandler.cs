/**
 * @copyright Copyright (c) 2021-2023 B-com http://www.b-com.com/
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

namespace Com.Bcom.Solar.Ui
{
    public class StartStopHandler : MonoBehaviour
    {
        public ButtonConfigHelper startStopButtonConfigHelper;
        public SolARCloudHololens2Specific solar;

        private string label;
        private bool labelChanged = false;

        // Start is called before the first frame update
        void Start()
        {
            solar.OnSensorStarted += OnStart;
            solar.OnSensorStopped += OnStop;
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
            if (solar.isRunning)
                solar.StopSensorsCapture();
            else
                solar.StartSensorsCapture();
        }

        private void OnStart(bool isOk)
        {
            label = isOk ? "Stop" : "Start\nError";
            labelChanged = true;
        }

        private void OnStop()
        {
            label = "Start";
            labelChanged = true;
        }
    }
}

