using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class MUIDebug
    {
        private const string logPrefix = "[<color=#cfa463>ModernUIs</color>]";
        
        // Normal
        public static void Log(object message)
        {
            Debug.Log($"{logPrefix} {message}");
        }
        public static void LogWarning(object message)
        {
            Debug.LogWarning($"{logPrefix} {message}");
        }
        public static void LogError(object message)
        {
            Debug.LogError($"{logPrefix} {message}");
        }
    }
}

