using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Toolkit;

namespace ProjectM.Battle
{
    public class ObjectPooling<T> : ObjectPool<T> where T : ObjectBase
    {
        private readonly T prefab;
        private readonly Transform parentTransform;
        public List<ObjectBase> objects = new List<ObjectBase>();
        public ObjectPooling(Transform parent, T prefab)
        {
            this.parentTransform = parent;
            this.prefab = prefab;
        }
        protected override T CreateInstance()
        {
            var obj = GameObject.Instantiate(prefab);
            obj.transform.SetParent(parentTransform);
            obj.GetComponent<IPoolObject>().Pool = this as ObjectPooling<ObjectBase>;
            
            return obj;
        }

        protected override void OnBeforeRent(T instance)
        {
            base.OnBeforeRent(instance);
            objects.Add(instance);
        }

        protected override void OnBeforeReturn(T instance)
        {
            base.OnBeforeReturn(instance);
            objects.Remove(instance);
        }
    }
}