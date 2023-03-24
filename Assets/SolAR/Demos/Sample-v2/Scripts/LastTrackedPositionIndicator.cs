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

using Bcom.Solar;

using SolARRpc = Com.Bcom.Solar.Gprc;

namespace Com.Bcom.Solar.Ui
{
    public class LastTrackedPositionIndicator : MonoBehaviour
    {
        public SolARCloud solar;

        private bool displayLastTrackedPos = false;
        private bool updateIndicator = false;

        void Start()
        {
            solar.OnMappingStatusChanged += OnMappingStatusChanged;
            SetEnableAllComponentsButThis(false);
        }

        void Update()
        {
            if (updateIndicator)
            {
                SetEnableAllComponentsButThis(displayLastTrackedPos);
                transform.position = Camera.main.transform.position;
                updateIndicator = false;
            }
        }

        void OnMappingStatusChanged(SolARRpc.MappingStatus oldMappingStatus, SolARRpc.MappingStatus newMappingStatus)
        {
            displayLastTrackedPos = newMappingStatus == SolARRpc.MappingStatus.TrackingLost;
            updateIndicator = true;
        }

        private void SetEnableAllComponentsButThis(bool enable)
        {
            foreach (var c in GetComponents<MonoBehaviour>())
                if (c != this) c.enabled = enabled;
        }
    }
}
