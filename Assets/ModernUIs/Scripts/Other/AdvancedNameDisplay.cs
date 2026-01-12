using System;
using System.Text;
using DrBlackRat.VRC.ModernUIs.Whitelist.Base;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.Economy;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Random = UnityEngine.Random;

namespace DrBlackRat.VRC.ModernUIs
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class AdvancedNameDisplay : UdonSharpBehaviour
    {
        [Tooltip("Text Mesh Pro UGUI components to display the names in.")]
        [SerializeField] private TextMeshProUGUI[] textMeshes;
        [Tooltip("Text Mesh Pro UGUI components to display the amount of users.")]
        [SerializeField] private TextMeshProUGUI[] counterTextMeshes;
        [Tooltip("How the names will be displayed, either separated by a new line, a \", \" or a \" | \"")]
        [SerializeField] private NameDisplayType nameDisplayType = NameDisplayType.NewLine;
        [Tooltip("If enabled duplicate names will be removed from the list. This has a slight performance impact.")]
        [SerializeField] private bool removeDuplicateNames = true;
        [Tooltip("If enabled the names will be sorted alphabetically. This has a slight performance impact.")]
        [SerializeField] private bool sortNames = true;
        
        [Tooltip("If enabled the display will automatically update every Update Interval minutes. Otherwise it will only update the data once on Start.")]
        [SerializeField] private bool autoUpdate = true;
        [Range(1f, 30f)] [Tooltip("Time in minutes between auto updates.")]
        [SerializeField] private float updateInterval = 10f;
        [Tooltip("If enabled the display will automatically update when the whitelist changes.")]
        [SerializeField] private bool updateOnWhitelistChange = true;
        
        [Tooltip("Udon Products to get names from. Requires \"Enable Access to Owner Names\" to be turned on for the products.")]
        [SerializeField] private UdonProduct[] udonProducts;
        [Tooltip("Whitelists to get names from. Recommended when using Disbridge or VRCLinking.")]
        [SerializeField] private WhitelistGetterBase[] whitelists;

        private DataList nameList = new DataList();
        
        private int productCount;
        private int whitelistCount;
        
        private int gatheringCounter = -1; // -1 = not gathering, > -1 = gathering
        private bool autoUpdateScheduled;

        private bool startedInitialGather;
        private const string LOGPrefix = "Advanced Name Display | ";
        
        private void Start()
        {
            productCount = udonProducts.Length;
            whitelistCount = whitelists.Length;
            
            // Set initial text
            foreach (var textMesh in textMeshes)
                textMesh.text = "Loading...";
            foreach (var counterTextMesh in counterTextMeshes)
                counterTextMesh.text = "0";

            if (updateOnWhitelistChange)
            {
                foreach (var whitelist in whitelists)
                    whitelist._SetUpConnection((IUdonEventReceiver)this, nameof(_OnWhitelistChanged));
            }
            
            SendCustomEventDelayedSeconds(nameof(_StartInitialLoading), 10f + Random.Range(0f, 5f));
        }

        public void _OnWhitelistChanged()
        {
            if (!updateOnWhitelistChange || !startedInitialGather) return;
            if (gatheringCounter > -1) return;
            Debug.LogError("WHITELIST CHANGE: RUNNING UPDATE!");
            _StartGatheringNames();
        }

        public void _StartInitialLoading()
        {
            if (startedInitialGather) return;
            startedInitialGather = true;
            _StartGatheringNames();
        }

        public void _StartAutoUpdate()
        {
            if (!autoUpdateScheduled) return;
            autoUpdateScheduled = false;
            _StartGatheringNames();
        }
        
        public void _StartGatheringNames()
        {
            if (gatheringCounter > -1)
            {
                MUIDebug.LogWarning(LOGPrefix + $"Already gathering names on {gameObject.name}, wait before trying again!", gameObject);
                return;
            }
            
            MUIDebug.Log(LOGPrefix + $"Started gathering names for {gameObject.name}.", gameObject);
            
            gatheringCounter = productCount + whitelistCount;
            nameList.Clear();
            
            _GetNextData();
        }

        public void _GetNextData()
        {
            gatheringCounter--;
            
            if (gatheringCounter < productCount)
            {
                Store.ListProductOwners((IUdonEventReceiver)this, udonProducts[gatheringCounter]);
            }
            else
            {
                var users = whitelists[gatheringCounter - productCount]._GetUsersAsList();
                nameList.AddRange(users);
            }

            if (gatheringCounter == 0)
            {
                // 1 sec delay to allow OnListProductOwners to finish
                SendCustomEventDelayedSeconds(nameof(_FinishGatheringNames), 1f);
            }
            else
            {
                SendCustomEventDelayedFrames(nameof(_GetNextData), 1);
            }
        }
        
        public override void OnListProductOwners(IProduct product, string[] owners)
        {
            nameList.AddRange(owners.ToDataList());
        }
        
        public void _FinishGatheringNames()
        {
            CleanUpNameList();
            var text = GenerateString();
            
            foreach (var textMesh in textMeshes)
                textMesh.text = text;
            
            var counterText = nameList.Count.ToString();
            foreach (var counterTextMesh in counterTextMeshes)
                counterTextMesh.text = counterText;
            
            gatheringCounter = -1;
            MUIDebug.Log(LOGPrefix + $"Finished gathering names for {gameObject.name}!", gameObject);
            
            if (autoUpdate && !autoUpdateScheduled)
            {
                autoUpdateScheduled = true;
                SendCustomEventDelayedSeconds(nameof(_StartAutoUpdate), updateInterval * 60f);
                MUIDebug.Log(LOGPrefix + $"Next update on {gameObject.name} in {updateInterval} minutes.", gameObject);
            }
        }

        private void CleanUpNameList()
        {
            if (removeDuplicateNames)
                nameList = nameList.RemoveDuplicates();
            if (sortNames)
                nameList.Sort();
        }
        
        private string GenerateString()
        {
            var stringBuilder = new StringBuilder();
            
            for (int i = 0; i < nameList.Count; i++)
            {
                if (i > 0)
                {
                    switch (nameDisplayType)
                    {
                        case NameDisplayType.NewLine:
                            stringBuilder.Append("\n");
                            break;
                        case NameDisplayType.Comma:
                            stringBuilder.Append(", ");
                            break;
                        case NameDisplayType.VerticalBar:
                            stringBuilder.Append(" | ");
                            break;
                    }
                } 
                stringBuilder.Append(nameList[i].String);
            }
            
            return stringBuilder.ToString();
        }
    }

    internal enum NameDisplayType
    {
        NewLine,
        Comma,
        VerticalBar,
    }
}
