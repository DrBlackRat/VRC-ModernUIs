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
        [SerializeField] private NameSeparatorType nameSeparatorType = NameSeparatorType.NewLine;
        [Tooltip("If the Name Separator Type is set to Custom, this will be used as the separator.")]
        [SerializeField] private string customNameSeparator = "";
        
        [Tooltip("If enabled duplicate names will be removed from the list.")]
        [SerializeField] private bool removeDuplicateNames = true;
        [Tooltip("If enabled the names will be sorted alphabetically.")]
        [SerializeField] private bool sortNames = true;
        [Tooltip("Names in this list will not be displayed.")]
        [SerializeField] private string[] namesToIgnore;
        
        [Tooltip("How to behave when the received name list is empty.\n- Nothing: Just displays an empty string.\n- Hide: Hides the section.\n- EmptyString: Displays the string provided below.")]
        [SerializeField] private EmptyBehaviour emptyBehaviour = EmptyBehaviour.EmptyString;
        [Tooltip("Text to display if the name list is empty. Only used if Empty Behaviour is set to Empty String.")]
        public string emptyString = "No names to display.";
        
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
        private string nameText;
        
        private int productCount;
        private int receivedProductInfoCount;
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
            
            SendCustomEventDelayedSeconds(nameof(_StartInitialLoading), 5f + Random.Range(0f, 5f));
        }

        public void _OnWhitelistChanged()
        {
            if (!updateOnWhitelistChange || !startedInitialGather) return;
            if (gatheringCounter > -1) return;
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

        #region Gathering Names
        public void _StartGatheringNames()
        {
            if (gatheringCounter > -1)
            {
                MUIDebug.LogWarning(LOGPrefix + $"Already gathering names on {gameObject.name}, wait before trying again!", gameObject);
                return;
            }
            
            MUIDebug.Log(LOGPrefix + $"Started gathering names for {gameObject.name}.", gameObject);
            
            receivedProductInfoCount = 0;
            gatheringCounter = productCount + whitelistCount;
            nameList.Clear();

            if (gatheringCounter > 0)
            {
                _GetNextData();
            }
            else
            {
                _FinishGatheringNames();
            }
            
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
            receivedProductInfoCount++;
            nameList.AddRange(owners.ToDataList());
        }
        #endregion

        #region Finish Gathering
        public void _FinishGatheringNames()
        {
            if (receivedProductInfoCount != productCount)
            {
                SendCustomEventDelayedSeconds(nameof(_FinishGatheringNames), 1f);
                return;
            }
            
            SendCustomEventDelayedFrames(nameof(_CleanUpNameList), 1);
            SendCustomEventDelayedFrames(nameof(_GenerateString), 2);
            SendCustomEventDelayedFrames(nameof(_FinishUpdate), 3);
        }
        public void _CleanUpNameList()
        {
            if (removeDuplicateNames)
            {
                nameList = nameList.RemoveDuplicates();
                // Only need to remove the names a maximum of once if duplicates are removed
                foreach (var nameToIgnore in namesToIgnore)
                {
                    if (string.IsNullOrEmpty(nameToIgnore))
                    {
                        continue;
                    }
                    nameList.Remove(nameToIgnore);
                }
            }
            else
            {
                // if duplicates are not removed, remove the names that are in the ignore list, even if they are in there multiple times
                foreach (var nameToIgnore in namesToIgnore)
                {
                    if (string.IsNullOrEmpty(nameToIgnore))
                    {
                        continue;
                    }
                    nameList.RemoveAll(nameToIgnore);
                }
            }
            
            if (sortNames)
            {
                nameList.Sort();
            }


        }

        public void _GenerateString()
        {
            var stringBuilder = new StringBuilder();
            
            for (int i = 0; i < nameList.Count; i++)
            {
                if (i > 0)
                {
                    switch (nameSeparatorType)
                    {
                        case NameSeparatorType.NewLine:
                            stringBuilder.Append("\n");
                            break;
                        case NameSeparatorType.Comma:
                            stringBuilder.Append(", ");
                            break;
                        case NameSeparatorType.VerticalBar:
                            stringBuilder.Append(" | ");
                            break;
                        case NameSeparatorType.Custom:
                            stringBuilder.Append(customNameSeparator);
                            break;
                    }
                } 
                stringBuilder.Append(nameList[i].String);
            }
            nameText = stringBuilder.ToString();
        }

        public void _FinishUpdate()
        {
            gameObject.SetActive(true);
            if (nameList.Count > 0 || emptyBehaviour == EmptyBehaviour.Nothing)
            {
                ApplyData(nameText, nameList.Count);
            }
            else if (emptyBehaviour == EmptyBehaviour.EmptyString)
            {
                ApplyData(emptyString, 0);
            }
            else
            {
                gameObject.SetActive(false);
            }
            
            gatheringCounter = -1;
            MUIDebug.Log(LOGPrefix + $"Finished gathering names for {gameObject.name}!", gameObject);
            
            if (autoUpdate && !autoUpdateScheduled)
            {
                autoUpdateScheduled = true;
                SendCustomEventDelayedSeconds(nameof(_StartAutoUpdate), updateInterval * 60f);
                MUIDebug.Log(LOGPrefix + $"Next update on {gameObject.name} in {updateInterval} minutes.", gameObject);
            }
        }
        #endregion
        
        private void ApplyData(string text, int counter)
        {
            foreach (var textMesh in textMeshes)
                textMesh.text = text;
            
            var counterText = counter.ToString();
            foreach (var counterTextMesh in counterTextMeshes)
                counterTextMesh.text = counterText;
        }
    }

    internal enum NameSeparatorType
    {
        NewLine,
        Comma,
        VerticalBar,
        Custom
    }
    
    internal enum EmptyBehaviour
    {
        Nothing,
        Hide,
        EmptyString,
    }
}
