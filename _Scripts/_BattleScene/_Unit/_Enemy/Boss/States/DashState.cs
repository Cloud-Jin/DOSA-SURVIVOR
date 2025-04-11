// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UniRx;
// using UniRx.Triggers;
// using Unity.VisualScripting;
//
//
// namespace ProjectM.Battle._Fsm.Boss
// {
//     [Serializable]
//     public class DashState : SkillState, IState
//     {
//         public Vector2 endPosition;
//         public float remainDistance;
//         private bool isWall;
//         private int count;              // 스킬 시전횟수
//         private IDisposable disposable;
//         private LineRenderer lineRenderer;
//         private bool isAction;
//         public DashState(UnitBase unit, SkillAI data, StateMachine stateMachine)
//         {
//             //this.unit = unit.GetComponent<EnemyBoss>();
//             caster = unit;
//             this.stateMachine = stateMachine;
//             
//             if (!caster.TryGetComponent(out lineRenderer))
//             {
//                 lineRenderer = caster.AddComponent<LineRenderer>();
//             }
//             
//             lineRenderer.enabled = false;
//             
//             lineRenderer.startWidth = 0.5f;
//             lineRenderer.endWidth = 0.5f;
//             lineRenderer.useWorldSpace = true;
//             lineRenderer.sortingLayerName = MyLayer.VisibleParticle;
//             lineRenderer.material = new Material(ResourcesManager.Instance.GetResources<Shader>("Particle Premultiply Blend"));
//             lineRenderer.startColor = new Color(0.87f, 0.12f, 0.12f, 0.7f);
//             lineRenderer.endColor = new Color(0.87f, 0.12f, 0.12f, 0.7f);
//
//             this.data = data;
//         }
//         public void Enter()
//         {
//             target = targetUnit.Rigid;
//             rigid = caster.Rigid;
//             count = data.Count;
//             // 돌진 2번
//             // 쿨타임 2초, 1.5초
//             // 피해량 100%
//             // 속도 30
//             // 글로벌 쿨타임 5초
//             isAction = false;
//             castingTime = data.CastingTime / 1000f;
//             Observable.FromCoroutine(Dash).Subscribe();
//             disposable = caster.OnCollisionEnter2DAsObservable().Subscribe(WallCollision);
//         }
//
//         public void Tick()
//         {
//             if(!isAction)
//                 caster.Spriter.flipX = target.position.x > rigid.position.x;
//         }
//
//         public void Exit()
//         {
//             // 연계기가 없다면
//             // 글로벌 쿨타임 부여
//             disposable.Dispose();
//             caster.attack = caster.baseAttack;
//             stateMachine.SetCoolTime(data);
//         }
//
//         void WallCollision(Collision2D col)
//         {
//             if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
//             {
//                 isWall = true;
//             }
//         }
//
//         IEnumerator Dash()
//         {
//             rigid.velocity = Vector2.zero;
//             lineRenderer.enabled = true;
//             count--;
//             // 선딜레이
//
//             Vector2 dirVec = Vector2.zero;
//             rigid.constraints = RigidbodyConstraints2D.FreezeAll;
//             while (castingTime > 0)
//             {
//                 castingTime -= Time.deltaTime;
//                 List<Vector3> pos = new List<Vector3>();
//                 pos.Add(rigid.position);
//                 dirVec = (target.position - rigid.position).normalized;
//                 pos.Add(( rigid.position + dirVec * data.Range / 10f));
//                 lineRenderer.SetPositions(pos.ToArray());
//                 yield return 0;
//             }
//             lineRenderer.enabled = false;
//             #region ray
//
//             // int layerMask = 1 << LayerMask.NameToLayer("Wall");
//             // RaycastHit2D hit = Physics2D.Raycast(Rigid.position, dir, 5f, layerMask);
//             // if (hit)
//             // {
//             //     Debug.Log($"레이캐스트 {hit.transform.name} Hit point {hit.point}");
//             //     // endPosition = hit.point;
//             //     // dir = (hit.point - Rigid.position).normalized;
//             // }
//             // Debug.DrawRay(Rigid.position, dir*5f, Color.red, 3f);
//
//             #endregion
//             
//             // Dash
//             // endPosition = target.position;
//             // dirVec = (endPosition - Rigid.position).normalized;
//             
//             var startPosition = rigid.position;
//             remainDistance = 0;
//             isWall = false;
//             isAction = true;
//             rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
//             caster.attack = MyMath.CalcCoefficient(caster.baseAttack, data.DamageRatio);
//             caster.PlayAnim(data.Ani);
//             while (remainDistance <= data.Range / 10f && !isWall)
//             {
//                 Vector2 newPosition = dirVec * (data.Speed / 10f * Time.fixedDeltaTime);
//                 rigid.MovePosition(rigid.position + newPosition);
//                 remainDistance = (startPosition - rigid.position).magnitude;
//                 yield return new WaitForFixedUpdate();
//             }
//             isAction = false;
//             rigid.velocity = Vector2.zero;
//
//             if (count > 0)
//             {
//                 castingTime = data.CountTime / 1000f;
//                 Observable.FromCoroutine(Dash).Subscribe();
//             }
//             else if (data.NextSkillId > 0)
//             {
//                 stateMachine.SetNextState(data.NextSkillId);
//             }
//             else
//             {
//                 stateMachine.SetIdelState();
//             }
//         }
//     }
// }