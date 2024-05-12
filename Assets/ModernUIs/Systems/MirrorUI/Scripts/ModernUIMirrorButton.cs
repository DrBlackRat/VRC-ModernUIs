using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace DrBlackRat.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModernUIMirrorButton : UdonSharpBehaviour
    {
        [Header("Button")]
        [SerializeField] private RectTransform buttonTransform;
        [Space(10)]
        [SerializeField] private Vector2 buttonNormalSize = new Vector2(-2f, 12f);
        [SerializeField] private Vector2 buttonSelectedSize = new Vector2(0f, 14f);

        [Header("Icon")]
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform iconTransform;
        [Space(10)]
        [SerializeField] private Vector3 iconNormalPos = new Vector3(-19.5f, 0f, 0f);
        [SerializeField] private Vector3 iconSelectedPos = new Vector3(-20f, 0f, 0f);
        [SerializeField] private Vector2 iconNormalAnchor = new Vector2(-19.5f, 0f);
        [SerializeField] private Vector2 iconSelectedAnchor = new Vector2(-20f, 0f);
        [Space(10)]
        
        [Header("Text")]
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform textTransform;
        [Space(10)]
        [SerializeField] private Vector3 textNormalPos = new Vector3(3.5f, 0f, 0f);
        [SerializeField] private Vector3 textSelectedPos = new Vector3(4f, 0f, 0f);
        [SerializeField] private Vector2 textNormalAnchor = new Vector2(3.5f, 0f);
        [SerializeField] private Vector2 textSelectedAnchor = new Vector2(4f, 0f);

        public void _ApplyColor(Color color)
        {
            icon.color = color;
            text.color = color;
        }

        public void _ButtonScale(float size)
        {
            buttonTransform.sizeDelta = Vector2.Lerp(buttonNormalSize, buttonSelectedSize, size);
            
            iconTransform.localPosition = Vector3.Lerp(iconNormalPos, iconSelectedPos, size);
            iconTransform.anchoredPosition = Vector2.Lerp(iconNormalAnchor, iconSelectedAnchor, size);
            
            textTransform.localPosition = Vector3.Lerp(textNormalPos, textSelectedPos, size);
            textTransform.anchoredPosition = Vector2.Lerp(textNormalAnchor, textSelectedAnchor, size);
        }
    }
}

