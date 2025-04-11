using UnityEngine;

namespace ProjectM
{
    public class TraitBridge : MonoBehaviour
    {
        public GameObject bridge;
        public bool isOn;
        public void SetBridge(bool isOn)
        {
            this.isOn = isOn;
            bridge.SetActive(isOn);
        }
    }
}