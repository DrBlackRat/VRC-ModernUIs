using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrBlackRat.VRC.ModernUIs
{
    public class Note : MonoBehaviour
    {
        [TextArea(1, 20)]
        [SerializeField] private string note;
    }
}

