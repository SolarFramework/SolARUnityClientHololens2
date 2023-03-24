using System;
using System.Collections;
using System.Collections.Generic;
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


