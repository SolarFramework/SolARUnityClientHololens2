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
using UnityEngine.UI;

using Bcom.Solar;

using SolARRpc = Com.Bcom.Solar.Gprc;

public class TrackingLostMessageManager : MonoBehaviour
{
    public SolArCloudHololens2 solArCloudHololens2;

    private bool displayTrackingLostMessage;
    private bool mappingStatusChanged;

    // Start is called before the first frame update
    void Start()
    {
        displayTrackingLostMessage = false;
        mappingStatusChanged = true;

        solArCloudHololens2.OnMappingStatusChanged += OnMappingStatusChanged;
    }

    // Update is called once per frame
    void Update()
    {
        // Manage tracking lost icon
        if (mappingStatusChanged)
        {
            gameObject.GetComponent<Text>().enabled = displayTrackingLostMessage;   

            mappingStatusChanged = false;
        }
    }

    void OnMappingStatusChanged(SolARRpc.MappingStatus mappingStatus)
    {
        if ((mappingStatus == SolARRpc.MappingStatus.TrackingLost) != displayTrackingLostMessage)
        {
            displayTrackingLostMessage = (mappingStatus == SolARRpc.MappingStatus.TrackingLost);
            mappingStatusChanged = true;
        }
    }
}
