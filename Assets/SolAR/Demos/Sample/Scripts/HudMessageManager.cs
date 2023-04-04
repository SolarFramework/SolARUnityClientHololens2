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

using Bcom.Solar;

using SolARRpc = Com.Bcom.Solar.Gprc;

public class HudMessageManager : MonoBehaviour
{
    public SolArCloudHololens2 solArCloudHololens2;

    private bool updateText;

    private string mappingStatus;
    private string poseStatus;

    // Start is called before the first frame update
    void Start()
    {
        updateText = false;

        solArCloudHololens2.OnReceivedPose += OnReceivedPose;
        solArCloudHololens2.OnStop += OnStop;
    }

    // Update is called once per frame
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
                        t.color = new Color32( 255, 0, 0, 255);
                    else if ((mappingStatus == "Bootstrap") || (mappingStatus == "LoopClosure"))
                        t.color = new Color32(240, 120, 20, 255);
                    else
                        t.color = new Color32(95, 210, 80, 255);

                }
                else
                {
                    t.text = "";
                }

            }
        }
    }

    void OnReceivedPose(SolARRpc.SolARMappingAndRelocalizationGrpcProxyManager.RelocAndMappingResult result)
    {
        lock(this)
        {
            switch (result.relocAndMappingResult.MappingStatus)
            {
                case SolARRpc.MappingStatus.Bootstrap: mappingStatus = "Bootstrap"; break;
                case SolARRpc.MappingStatus.LoopClosure: mappingStatus = "LoopClosure"; break;
                case SolARRpc.MappingStatus.Mapping: mappingStatus = "Mapping"; break;
                case SolARRpc.MappingStatus.TrackingLost: mappingStatus = "TrackingLost"; break;
                default: throw new System.Exception("Unkown mapping status");
            }
            switch(result.relocAndMappingResult.PoseStatus)
            {
                case SolARRpc.RelocalizationPoseStatus.LatestPose: poseStatus = "LastestPose"; break;
                case SolARRpc.RelocalizationPoseStatus.NewPose: poseStatus = "NewPose"; break;
                case SolARRpc.RelocalizationPoseStatus.NoPose: poseStatus = "NoPose"; break;
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
