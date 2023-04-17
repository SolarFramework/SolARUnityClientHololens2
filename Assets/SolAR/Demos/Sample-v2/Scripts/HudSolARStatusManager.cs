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
using UnityEngine.UI;

using Com.Bcom.Solar.Gprc;

namespace Com.Bcom.Solar.Ui
{
    public class HudSolARStatusManager : MonoBehaviour
    {
        public SolARCloud solar;

        private bool updateText;

        private string mappingStatus;
        private string poseStatus;

        void Start()
        {
            updateText = false;
            gameObject.GetComponent<Text>().text = "";

            solar.OnReceivedPose += OnReceivedPose;
            solar.OnStop += OnStop;
        }

        void Update()
        {
            if (updateText)
            {
                updateText = false;
                Text t = gameObject.GetComponent<Text>();
                lock (this)
                {
                    if (mappingStatus != "")
                    {
                        t.text = "Mapping Status: " + mappingStatus;
                        t.text += "\nPose Status: " + poseStatus;

                        if (mappingStatus == "TrackingLost")
                            t.color = new Color32(255, 0, 0, 255);
                        else
                            t.color = new Color32(32, 145, 16, 255);

                    }
                    else
                    {
                        t.text = "";
                    }

                }
            }
        }

        void OnReceivedPose(RelocAndMappingResult result)
        {
            lock (this)
            {
                if (solar.pipelineMode == PipelineMode.RelocalizationOnly)
                {
                    mappingStatus = "N/A";
                }
                else
                {
                    switch (result.Result.MappingStatus)
                    {
                        case MappingStatus.Bootstrap: mappingStatus = "Bootstrap"; break;
                        case MappingStatus.LoopClosure: mappingStatus = "LoopClosure"; break;
                        case MappingStatus.Mapping: mappingStatus = "Mapping"; break;
                        case MappingStatus.TrackingLost: mappingStatus = "TrackingLost"; break;
                        default: throw new System.Exception("Unkown mapping status");
                    }
                }
                switch (result.Result.PoseStatus)
                {
                    case RelocalizationPoseStatus.LatestPose: poseStatus = "LatestPose"; break;
                    case RelocalizationPoseStatus.NewPose: poseStatus = "NewPose"; break;
                    case RelocalizationPoseStatus.NoPose: poseStatus = "NoPose"; break;
                    default: throw new System.Exception("Unkown pose status");
                }
                updateText = true;
            }
        }

        void OnStop()
        {
            mappingStatus = "";
            poseStatus = "";

            updateText = true;
        }

    }
}


