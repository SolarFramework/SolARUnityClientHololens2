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

using Microsoft.MixedReality.Toolkit.UI;
using Bcom.Solar;

using SolARRpc = Com.Bcom.Solar.Gprc;

public class PipelineModeButtonHandler : MonoBehaviour
{
    public ButtonConfigHelper buttonConfigHelper;
    public SolArCloudHololens2 solArCloudHololens2;

    void Start()
    {
        buttonConfigHelper.MainLabelText = buildButtonLabel(solArCloudHololens2.pipelineMode);
        solArCloudHololens2.OnPipelineModeChanged += OnPipelineModeChanged;
    }

    void Update()
    {
        
    }
    public void TogglePipelineMode()
    {
        solArCloudHololens2.TogglePipelineMode();
    }

    private void OnPipelineModeChanged(SolARRpc.PipelineMode oldMode, SolARRpc.PipelineMode newMode)
    {
        buttonConfigHelper.MainLabelText = buildButtonLabel(newMode);
    }

    private string buildButtonLabel(SolARRpc.PipelineMode mode)
    {
        return mode == SolARRpc.PipelineMode.RelocalizationAndMapping ? "Reloc + Map" : "Reloc Only";
    }

}
