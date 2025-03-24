using System;
using System.Linq.Expressions;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs
{
    public static class ArrayExtensions
    {
        public static T[] Add<T>(this T[] array, T item)
        {
            T[] newArray = new T[array == null ? 1 : array.Length + 1];
            if (array != null) Array.Copy(array, newArray, array.Length);
            newArray[newArray.Length - 1] = item;
            return newArray;
        }

        public static T[] RemoveAt<T>(this T[] array, int index)
        {
            T[] newArray = new T[array.Length - 1];
            Array.Copy(array, newArray, index);
            Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
            return newArray;
        }
    
        public static T[] Remove<T>(this T[] array, T item)
        {
            int index = Array.IndexOf(array, item);
            if (index == -1)
            {
                return array;
            }
            T[] newArray = new T[array.Length - 1];
            Array.Copy(array, newArray, index);
            Array.Copy(array, index + 1, newArray, index, newArray.Length - index);
            return newArray;
        }
        
        public static T[] Combine<T>(params T[][] arrays)
        {
            int length = 0;
            foreach (var array in arrays)
            {
                length += array.Length;
            }
            T[] newArray = new T[length];
            int index = 0;
            foreach (var array in arrays)
            {
                Array.Copy(array, 0, newArray, index, array.Length);
                index += array.Length;
            }
            return newArray; 
        }
    }
}

