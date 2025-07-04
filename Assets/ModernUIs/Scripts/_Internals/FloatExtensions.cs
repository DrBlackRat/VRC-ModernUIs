using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs.Helpers
{
    public static class FloatExtensions
    {
        /// <summary>
        /// Returns true if the absolute difference between two float values is less than or equal to the specified tolerance.
        /// </summary>
        /// <param name="a">The first float value.</param>
        /// <param name="b">The second float value.</param>
        /// <param name="tolerance">The maximum difference allowed for the values to be considered approximately equal.</param>
        /// <returns>True if the values are within the given tolerance; otherwise, false.</returns>
        public static bool Approximately(float a, float b, float tolerance)
        {
            return Mathf.Abs(a - b) <= tolerance;
        }
    }
}

