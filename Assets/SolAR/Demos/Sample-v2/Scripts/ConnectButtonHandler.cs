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

using System.Collections.Generic;

using UnityEngine;

using Microsoft.MixedReality.Toolkit.UI;


namespace Com.Bcom.Solar.Ui
{
    public class ConnectButtonHandler : MonoBehaviour
    {
        public SolARCloud solar;
        public ButtonConfigHelper buttonConfigHelper;
        public List<GameObject> enabledWhenDisconnected = new List<GameObject>();
        public List<GameObject> enabledWhenConnected = new List<GameObject>();
        private bool update = false;
        private bool error;

        void Start()
        {
            update = true;
        }

        void Update()
        {
            if (update)
            {
                buttonConfigHelper.MainLabelText = $"{(solar.Isregistered() ? "Disconnect" : "Connect")}{(error ? "\nerror" : "")}";
                foreach (var button in enabledWhenDisconnected) button.SetActive(!solar.Isregistered());
                foreach (var button in enabledWhenConnected) button.SetActive(solar.Isregistered());
                update = false;
            }
        }

        public async void ToggleConnection()
        {
            error = !(solar.Isregistered() ? await solar.Disconnect() : await solar.Connect());
            update = true;
        }
    }
}

