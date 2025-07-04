using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace DrBlackRat.VRC.ModernUI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ModernUIMirrorSeparator : UdonSharpBehaviour
    {
        [SerializeField] private RectTransform transform;
        [SerializeField] private Vector2 defaultPos;
        [SerializeField] private Vector2 playerPos;
        [SerializeField] private Vector2 offPos;

        private Vector2 prevSeparatorPos;
        private MirrorUIState state = MirrorUIState.Off;

        public void _UpdateSeparatorInfo(MirrorUIState newState)
        {
            state = newState;
            prevSeparatorPos = transform.anchoredPosition;
        }

        public void _AnimateSeparator(float transition)
        {
            switch (state)
            {
                case MirrorUIState.High:
                    MoveSeparator(prevSeparatorPos, defaultPos, transition);
                    break;
                case MirrorUIState.Low:
                    MoveSeparator(prevSeparatorPos, defaultPos, transition);
                    break;
                case MirrorUIState.Player:
                    MoveSeparator(prevSeparatorPos, playerPos, transition);
                    break;
                case MirrorUIState.Off:
                    MoveSeparator(prevSeparatorPos, offPos, transition);
                    break;
            }
        }

        private void MoveSeparator(Vector2 prevPos, Vector2 newPos, float transition)
        {
            transform.anchoredPosition = Vector2.LerpUnclamped(prevPos, newPos, transition);
        }
    }
}
