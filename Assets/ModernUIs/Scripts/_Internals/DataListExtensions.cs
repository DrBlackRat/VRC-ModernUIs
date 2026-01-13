using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.SDK3.Data;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class DataListExtensions
    {
        public static DataList RemoveDuplicates(DataList dataList)
        {
            DataList tempList = new DataList();

            for (int i = 0; i < dataList.Count; i++)
            {
                if (tempList.Contains(dataList[i])) continue;
                tempList.Add(dataList[i]);
            }

            return tempList;
        }
    }
}

