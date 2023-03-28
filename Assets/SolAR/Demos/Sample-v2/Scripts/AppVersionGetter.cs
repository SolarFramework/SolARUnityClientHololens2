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

using UnityEngine;

using TMPro;

using hl2sensor = Com.Bcom.Solar.SolARCloudHololens2Specific.Hl2SensorType;

namespace Com.Bcom.Solar.Ui
{
    public class AppVersionGetter : MonoBehaviour
    {
        public SolARCloud solar;
        public SolARCloudHololens2Specific hololens2;

        void Start()
        {
            gameObject.GetComponent<TextMeshPro>().text = $"v.{Application.version} {toString(hololens2.sensorType)}";
        }

        private static string toString(hl2sensor t)
        {
            switch(t)
            {
                case hl2sensor.PV: return "(RGB front camera)";
                case hl2sensor.RM_LEFT_FRONT: return "(left front camera)";
                case hl2sensor.STEREO: return "(left front + right front cameras)";
                default: return "(Unkown sensor)";
            }
        }
    }
}


