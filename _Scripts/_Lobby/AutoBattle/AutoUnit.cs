using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DamageNumbersPro;
using InfiniteValue;
using UnityEngine;

namespace ProjectM.AutoBattle
{
    public class AutoUnit : MonoBehaviour
    {
        public enum AutoState
        {
            Idle,
            Move,
            Attack,
            Dead
        }
        
        public bool isLive;
        public InfVal attack;
        public float moveSpeed;
        public Vector2 returnPos;
        public AutoUnit target;
        public AutoBattleSkillAI _battleSkillAI;
        public Dictionary<string, Transform> BodyParts;
        public Action DeadAction;
        public AnimatorPlayer AnimatorPlayer => animPlayer;
        public SpriteRenderer Spriter => spriter;

        public int unitId;
        protected Collider2D coll;
        protected AnimatorPlayer animPlayer;
        protected SpriteRenderer spriter;
        
        private IState[] _states;
        private AutoState _state;
        
        public AutoState State
        {
            get => _state;
            set
            {
                _states[(int)_state].Exit();
                _state = value;
                _states[(int)_state].Enter();
            }
        }

        protected virtual void Awake()
        {
           SetUnit();
        }

        public void SetUnit()
        {
            _states = new IState[Enum.GetValues(typeof(AutoState)).Length];
            _states[(int)AutoState.Idle] = new IdleState(this);
            _states[(int)AutoState.Move] = new MoveState(this);
            _states[(int)AutoState.Attack] = new AttackState(this);
            _states[(int)AutoState.Dead] = new DeadState(this);

            isLive = true;
            moveSpeed = TableDataManager.Instance.data.AutoBattleConfig.Single(t => t.Index == 17).Value;
            
            var parts = transform.Find("Parts");
            if (parts == null) 
                parts = transform.GetChild(0).Find("Parts");
            if (parts)
            {
                BodyParts = new Dictionary<string, Transform>();
                BodyParts.Add("Parts", parts);
      
                var body = parts.Find("Body");
                BodyParts.Add("Body",body);
                
                body.TryGetComponent(out spriter);
                body.TryGetComponent(out animPlayer);
                
                BodyParts.Add("Weapon_Left", body.Find("Weapon_Left"));
                BodyParts.Add("Weapon_Middle", body.Find("Weapon_Middle"));
                BodyParts.Add("Weapon_Right", body.Find("Weapon_Right"));
                BodyParts.Add("Weapon_Pivot", body.Find("Weapon_Pivot"));
            }
            
            coll = GetComponentInChildren<Collider2D>();
            coll.isTrigger = true;
        }

        public virtual void Init(Monster stat)
        {
            // 능력치등 설정
            
        }

        private void Update()
        {
            _states?[(int)_state].Stay();
        }

        public void SetSkillData(AutoBattleSkillAI skillAI)
        {
            _battleSkillAI = skillAI;
        }

        public void SetRetrunPos(Vector2 pos)
        {
            this.returnPos = pos;
        }
        
        public void DamageHitUI(InfVal value, HitType hitType, Vector3 position)
        {
            if(hitType == HitType.None) return;
            
            // 데미지 폰트 출력
            GameObject numberPrefab = null;
            string resourcesName = String.Empty;
            switch (hitType)
            {
                case HitType.Normal:
                    resourcesName = "Damage Number1";
                    break;
                // case HitType.Fatal:
                //     resourcesName = "Damage Number2";
                //     break;
                // case HitType.HyperFatal:
                //     resourcesName = "Damage Number3";
                //     break;
            }
            numberPrefab = ResourcesManager.Instance.GetResources<GameObject>(resourcesName);
            //<DamageNumber>
            DamageNumber damageNumber = numberPrefab.GetComponent<DamageNumber>().Spawn(position);
            damageNumber.topText = value.ToParseString();
        }
    }

}