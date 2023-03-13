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

using TMPro;
using Bcom.Solar;

using Microsoft.MixedReality.Toolkit.Experimental.UI;

namespace Com.Bcom.Solar.Ui
{
    public class IpKeyboardHandler : MonoBehaviour
    {
        public SolARCloud solar;

        public GameObject mRKeyboardPreview;
        public MixedRealityKeyboardPreview mRKeyboardPreviewScript;

        private TouchScreenKeyboard keyboard;

        void Start()
        {
            mRKeyboardPreviewScript.Text = solar.frontendIp;
            mRKeyboardPreview.SetActive(false);
        }

        void Update()
        {
            if (keyboard != null)
            {
                if (keyboard.status == TouchScreenKeyboard.Status.Visible)
                {
                    mRKeyboardPreview.SetActive(true);
                    solar.frontendIp = keyboard.text;
                }
                else if (mRKeyboardPreview.activeInHierarchy)
                {
                    mRKeyboardPreview.SetActive(false);
                    solar.Connect();
                }
            }
            mRKeyboardPreviewScript.Text = solar.frontendIp;
        }

        public void OpenKeyboardForIp()
        {
            keyboard = TouchScreenKeyboard.Open(solar.frontendIp, TouchScreenKeyboardType.URL);
        }
    }
}
