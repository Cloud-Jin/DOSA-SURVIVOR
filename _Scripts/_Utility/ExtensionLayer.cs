using UnityEngine;

namespace ProjectM
{
    public static class ExtensionLayer
    {
        public static bool Contanis(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}