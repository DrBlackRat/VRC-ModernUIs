using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class MUIDebug
    {
        private const string logPrefix = "[<color=#cfa463>ModernUIs</color>]";
        
        // Normal
        public static void Log(object message, Object context = null)
        {
            Debug.Log($"{logPrefix} {message}", context);
        }
        public static void LogWarning(object message, Object context = null)
        {
            Debug.LogWarning($"{logPrefix} {message}", context);
        }
        public static void LogError(object message, Object context = null)
        {
            Debug.LogError($"{logPrefix} {message}", context);
        }
    }
}

