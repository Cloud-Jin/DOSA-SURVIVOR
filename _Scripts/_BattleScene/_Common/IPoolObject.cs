using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectM.Battle
{
    public interface IPoolObject
    {
        public ObjectPooling<ObjectBase> Pool { get; set; }

        public virtual void ReleaseObject(ObjectBase unit)
        {
            Pool.Return(unit);
        }
    }
}