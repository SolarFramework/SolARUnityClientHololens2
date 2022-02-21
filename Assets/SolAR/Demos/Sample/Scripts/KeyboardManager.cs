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

public class KeyboardManager : MonoBehaviour
{
    public SolArCloudHololens2 solArCloudHololens2;
    public TextMeshPro ipText;

    private TouchScreenKeyboard keyboard;

    void Start()
    {
        ipText.text = solArCloudHololens2.frontendIp;
        ipText.GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        if (keyboard != null)
        {
            if (keyboard.status == TouchScreenKeyboard.Status.Visible)
            {
                ipText.GetComponent<MeshRenderer>().enabled = true;
                solArCloudHololens2.frontendIp = keyboard.text;
            }
            else
            {
                ipText.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        ipText.text = solArCloudHololens2.frontendIp;
    }

    public void OpenKeyboardForIp()
    {
        keyboard = TouchScreenKeyboard.Open(solArCloudHololens2.frontendIp, TouchScreenKeyboardType.URL);
    }

}
