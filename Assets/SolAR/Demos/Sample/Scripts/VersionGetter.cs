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

using UnityEngine;

using TMPro;
using Bcom.Solar;

public class VersionGetter : MonoBehaviour
{
    public SolArCloudHololens2 solArCloudHololens2;

    void Start()
    {
        if (solArCloudHololens2.selectedSensor == SolArCloudHololens2.Hl2SensorTypeEditor.PV)
        {
            gameObject.GetComponent<TextMeshPro>().text = "v." + Application.version + " (RGB front camera)";
        }
        else if (solArCloudHololens2.selectedSensor == SolArCloudHololens2.Hl2SensorTypeEditor.RM_LEFT_FRONT)
        {
            gameObject.GetComponent<TextMeshPro>().text = "v." + Application.version + " (left front camera)";
        }
        else if (solArCloudHololens2.selectedSensor == SolArCloudHololens2.Hl2SensorTypeEditor.STEREO)
        {
            gameObject.GetComponent<TextMeshPro>().text = "v." + Application.version + " (left front + right front cameras)";
        }
        else
        {
            gameObject.GetComponent<TextMeshPro>().text = "v." + Application.version;
        }
    }
}
