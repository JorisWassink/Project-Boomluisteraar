using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TreeClimb
{
    [RequireComponent(typeof(Button))]
    public class PlatformerTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public PlayerController player;
        public bool moveLeft;
        readonly HashSet<int> pointers = new HashSet<int>();
        public void OnPointerDown(PointerEventData data)
        {
            if (!GetComponent<Button>().IsInteractable()) return;
            pointers.Add(data.pointerId);
            Apply();
        }
        public void OnPointerUp(PointerEventData data) { pointers.Remove(data.pointerId); Apply(); }
        public void OnPointerExit(PointerEventData data) { pointers.Remove(data.pointerId); Apply(); }
        void OnDisable() { pointers.Clear(); Apply(); }
        void OnApplicationFocus(bool focused) { if (!focused) { pointers.Clear(); Apply(); } }
        void OnApplicationPause(bool paused) { if (paused) { pointers.Clear(); Apply(); } }
        void Apply()
        {
            if (player == null) return;
            if (moveLeft) player.SetTouchLeft(pointers.Count > 0);
            else player.SetTouchRight(pointers.Count > 0);
        }
    }
}
