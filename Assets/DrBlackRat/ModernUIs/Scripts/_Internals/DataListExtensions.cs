using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.SDK3.Data;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class DataListExtensions
    {
        /// <summary>
        /// Generates a copy of the provided <see cref="DataList"/> and removes duplicate entries.
        /// </summary>
        /// <param name="dataList">DataList of which to remove duplicate entries from.</param>
        /// <returns>A copy of the provided <see cref="DataList"/> without duplicate entries.</returns>
        public static DataList RemoveDuplicates(this DataList dataList)
        {
            DataList tempList = new DataList();

            for (int i = 0; i < dataList.Count; i++)
            {
                if (tempList.Contains(dataList[i])) continue;
                tempList.Add(dataList[i]);
            }

            return tempList;
        }
        
        #region To Datalist
        /// <summary>
        /// Converts an array of elements into a <see cref="DataList"/> by wrapping each element in a <see cref="DataToken"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The array to convert.</param>
        /// <returns>
        /// A new <see cref="DataList"/> containing a <see cref="DataToken"/> for each element in the <paramref name="array"/>.
        /// If the array is <c>null</c>, an empty <see cref="DataList"/> is returned.
        /// </returns>
        public static DataList ToDataList<T>(this T[] array)
        {
            var dataList = new DataList();
            
            if (array == null)
                return dataList;
            
            foreach (var element in array)
            {
                var token = new DataToken(element);
                dataList.Add(token);
            }
            return dataList;
        }
        
        /// <summary>
        /// Converts an array of strings into a <see cref="DataList"/> by wrapping each string in a <see cref="DataToken"/>.
        /// </summary>
        /// <param name="array">The string array to convert.</param>
        /// <returns>
        /// A new <see cref="DataList"/> containing a <see cref="DataToken"/> for each string in the <paramref name="array"/>.
        /// If the array is <c>null</c>, an empty <see cref="DataList"/> is returned.
        /// </returns>
        public static DataList ToDataList(this string[] array)
        {
            var dataList = new DataList();
            
            if (array == null)
                return dataList;
            foreach (var element in array)
            {
                var token = new DataToken(element);
                dataList.Add(token);
            }
            return dataList;
        }
        
        #endregion

        #region To Array
        /// <summary>
        /// Converts a <see cref="DataList"/> containing string entries into a <see cref="string"/> array.
        /// </summary>
        /// <param name="dataList">The data list to convert.</param>
        /// <returns>
        /// A string array containing all elements from the data list, or <c>null</c> if <paramref name="dataList"/> is <c>null</c>.
        /// </returns>
        public static string[] ToArray(this DataList dataList)
        {
            if (dataList == null) 
                return new string[0];

            string[] tempArray = new string[dataList.Count];
            for (int i = 0; i < dataList.Count; i++)
            {
                tempArray[i] = dataList[i].String;
            }

            return tempArray;
        }
        #endregion

    }
}

