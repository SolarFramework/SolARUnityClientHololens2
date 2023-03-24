using System;
using UnityEngine;


namespace Com.Bcom.Solar
{
    static class Extensions
    {
        public static LogType ToUnityLogType(this LogLevel logLevel)
        {
            switch(logLevel)
            {
                case LogLevel.ERROR: return LogType.Error;
                case LogLevel.WARNING: return LogType.Warning;
                case LogLevel.INFO:
                case LogLevel.DEBUG: return LogType.Log;
                default: throw new ArgumentException("Unkown LogLevel value");
            }
        }
    }
}
