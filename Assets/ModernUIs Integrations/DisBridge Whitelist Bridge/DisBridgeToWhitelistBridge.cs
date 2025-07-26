
using UdonSharp;
using UdonVR.DisBridge;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using DrBlackRat.VRC.ModernUIs.Whitelist;
using DrBlackRat.VRC.ModernUIs.Helpers;
using DrBlackRat.VRC.ModernUIs.Helpers;
using VRC.SDK3.Data;
using System.Collections;
using System.Collections.Generic;
using UdonVR.DisBridge.Editors;
using UnityEditor;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Integrations
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(BasicWhitelist))]
    public class DisBridgeToWhitelistBridge : DisBridgePlugin
    {
        private BasicWhitelist whitelist;
        
        private void Start()
        {
            whitelist = gameObject.GetComponent<BasicWhitelist>();
            disBridge.AddPlugin(this);
        }

        public override void _UVR_UserJoined(VRCPlayerApi _player)
        {
            whitelist._AddUser(_player.displayName);
        }
        
        public override void _UVR_UserLeft(VRCPlayerApi _player)
        {
            whitelist._RemoveUser(_player.displayName);
        }
    }
    
    #if !COMPILER_UDONSHARP
    [CustomEditor(typeof(DisBridgeToWhitelistBridge))]
    public class DisBridgeToWhitelistBridge_Editor : Editor
    {
        DisBridgeToWhitelistBridge myScript;

        private void OnEnable()
        {
            myScript = (DisBridgeToWhitelistBridge)target;
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(myScript, $"Undo on {myScript.name}");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorFunctions.DrawPluginManagerCheck(myScript, false,true);
                EditorFunctions.DrawPermissions(myScript, true);
                //EditorFunctions.DrawRoleContainerSelector(myScript);
                //EditorFunctions.DrawStaffSupporterSelector(myScript);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(myScript);
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
