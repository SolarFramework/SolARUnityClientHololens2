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

using System;
using System.Linq;

using UnityEngine;

namespace Com.Bcom.Solar.Ui
{
    public class LogConsoleHandler : MonoBehaviour
    {

        public SolARCloud solarCommon;
        public SolARCloudHololens2Specific solarHololens2;
        public TMPro.TextMeshPro title;
        public TMPro.TextMeshProUGUI text;

        private LogConsole console;
        private bool update;

        void Start()
        {
            console = new LogConsole();
            solarCommon.OnLog += OnLogImpl;
            solarHololens2.OnLog += OnLogImpl;
            title.text = "Console";
        }

        void Update()
        {
            if (!update) return;
            text.text = String.Join("\n", console.logs.Reverse().ToArray()) ;
            update = false;
        }

        private void OnLogImpl(LogLevel level, string message)
        {
            console?.Log(level, message);
            update = true;
        }

        public void Clear()
        {
            console.Clear();
            update = true;
        }
    }
}


