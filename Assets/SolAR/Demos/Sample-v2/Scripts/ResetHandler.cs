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
using Com.Bcom.Solar.Gprc;

namespace Com.Bcom.Solar.Ui
{
    public class ResetHandler : MonoBehaviour
    {
        public ButtonConfigHelper buttonConfigHelper;
        public SolARCloud solar;

        void Start()
        {
            solar.OnPipelineModeChanged += OnPipelineModeChanged;
            solar.OnConnected += OnConnected;
            solar.OnDisconnected += OnDisconnected;
            buttonConfigHelper.gameObject.SetActive(ShouldEnable());
        }

        public async void Reset()
        {
            await solar.SolARReset();
        }

        private void OnPipelineModeChanged(PipelineMode oldMode, PipelineMode newMode)
        {
            buttonConfigHelper.gameObject.SetActive(ShouldEnable());
        }

        private void OnConnected()
        {
            buttonConfigHelper.gameObject.SetActive(ShouldEnable());
        }

        private void OnDisconnected()
        {
            buttonConfigHelper.gameObject.SetActive(ShouldEnable());
        }

        private bool ShouldEnable()
        {
            return solar.Isregistered() && solar.pipelineMode == PipelineMode.RelocalizationAndMapping;
        }
    }
}
