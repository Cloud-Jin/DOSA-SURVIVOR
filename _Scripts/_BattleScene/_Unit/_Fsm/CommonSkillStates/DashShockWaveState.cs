using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;

// 대쉬 충격파 추가

namespace ProjectM.Battle._Fsm
{
    public class DashShockWaveState : SkillState, IState
    {
        public float remainDistance;
        private bool isWall;
        private int count;              // 스킬 시전횟수
        private float duration;
        private IDisposable disposable;
        private LineRenderer lineRenderer;
        private bool isAction;

        public DashShockWaveState(UnitBase unit, SkillAI data, StateMachine stateMachine)
        {
            caster = unit;
            this.stateMachine = stateMachine;
            
            if (!caster.TryGetComponent(out lineRenderer))
            {
                lineRenderer = caster.AddComponent<LineRenderer>();
            }
            
            lineRenderer.enabled = false;
            
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingLayerName = MyLayer.VisibleParticle;
            lineRenderer.material = new Material(ResourcesManager.Instance.GetResources<Shader>("Particle Premultiply Blend"));
            lineRenderer.startColor = new Color(0.87f, 0.12f, 0.12f, 0.7f);
            lineRenderer.endColor = new Color(0.87f, 0.12f, 0.12f, 0.7f);
            
            this.data = data;
            PoolManager.Instance.CreatePool(data.ObjectResource, 1);
        }
        public void Enter()
        {
            target = targetUnit.Rigid;
            rigid = caster.Rigid;
            count = data.Count;
            duration = data.DurationTime / 1000f;
            // 돌진 2번
            // 쿨타임 2초, 1.5초
            // 피해량 100%
            // 속도 30
            // 글로벌 쿨타임 5초
            isAction = false;
            castingTime = data.CastingTime / 1000f;
            DashEnter();
            Observable.FromCoroutine(Dash).Subscribe().AddTo(caster.disposables);
        }

        public void Tick()
        {
            if (!isAction)
            {
                // var isFlip = targetUnit.Rigid.position.x > rigid.position.x;
                // FlipX(isFlip);
                FlipX();
                //caster.Spriter.flipX = target.position.x > rigid.position.x;
            }
                
        }

        public void Exit()
        {
            // 연계기가 없다면
            // 글로벌 쿨타임 부여
            disposable.Dispose();
            caster.attack = caster.baseAttack;
            DashEnd();
            stateMachine.SetCoolTime(data);
        }

        void WallCollision(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                isWall = true;
            }
        }

        IEnumerator Dash()
        {
            MovePause();
            lineRenderer.enabled = true;
            count--;
            // 선딜레이

            Vector2 dirVec = Vector2.zero;
            while (castingTime > 0)
            {
                castingTime -= Time.deltaTime;
                List<Vector3> pos = new List<Vector3>();
                pos.Add(rigid.position);
                dirVec = (target.position - rigid.position).normalized;
                pos.Add(( rigid.position + dirVec * data.Range / 10f));
                lineRenderer.SetPositions(pos.ToArray());
                yield return 0;
            }
            lineRenderer.enabled = false;
            
            // Dash
            // endPosition = target.position;
            // dirVec = (endPosition - Rigid.position).normalized;
            
            var startPosition = rigid.position;
            remainDistance = 0;
            float _time = 0;
            float _ShockWaveTime = 0f;
            isWall = false;
            isAction = true;
            disposable = caster.OnCollisionEnter2DAsObservable().Subscribe(WallCollision);
            caster.attack = MyMath.CalcCoefficient(caster.baseAttack, data.DamageRatio);
            caster.PlayAnim(data.Ani);
            caster.Rigid.drag = 0f;

            var effectPool = PoolManager.Instance.GetPool(data.ObjectResource);
            MoveResume();
            while (remainDistance <= data.Range / 10f && !isWall && _time < duration)
            {
                rigid.velocity = dirVec.normalized * (data.Speed / 10f);
                remainDistance = (startPosition - rigid.position).magnitude;
                _time += Time.fixedDeltaTime;
                _ShockWaveTime += Time.fixedDeltaTime;
                if (_ShockWaveTime >= data.DamegeTime / 1000f)
                {
                    // 충격파 데미지
                    var effect = effectPool.Rent().GetComponent<Projectile>();
                    effect.transform.position = caster.BodyParts[data.Pivot].position;
                    effect.InitBuilder()
                        .SetDamage(MyMath.CalcCoefficient(caster.attack, data.TypeValue))
                        .SetPer(99)
                        .SetDuration(0.5f)
                        .SetUnit(caster)
                        .Build();

                    _ShockWaveTime = 0f;
                }
                yield return new WaitForFixedUpdate();
            }
            isAction = false;
            caster.PlayAnim($"{data.Ani}_END");
            
            if (count > 0)
            {
                castingTime = data.CountTime / 1000f;
                Observable.FromCoroutine(Dash).Subscribe().AddTo(caster.disposables);
            }
            else if (data.NextSkillId > 0)
            {
                stateMachine.SetNextState(data.NextSkillId);
            }
            else
            {
                stateMachine.SetIdelState();
            }
        }
    }
}