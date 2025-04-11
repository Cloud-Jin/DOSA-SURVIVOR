using System;
using Best.HTTP.Request.Settings;
using DG.Tweening;
using DTT.AreaOfEffectRegions;
using UniRx;

namespace ProjectM.Battle
{
    public class ArcIndicator : IndicatorBase
    {
        private ArcRegion _arcRegion;
        private float _arc;
        private float _angle;
        private float _radius;

        public override void Awake()
        {
            base.Awake();
            _arcRegion = GetComponent<ArcRegion>();
        }

        public ArcIndicator InitBuilder()
        {
            _arc = 120;
            _angle = 0;
            _radius = 1;
            return this;
        }

        public ArcIndicator SetArc(float arc)
        {
            _arc = arc;
            return this;
        }
        
        public ArcIndicator SetAngle(float angle)
        {
            _angle = angle;
            return this;
        }
        
        public ArcIndicator SetRadius(float radius)
        {
            _radius = radius;
            return this;
        }
        
        public ArcIndicator SetDuration(float time)
        {
            _arcRegion.FillProgress = 0;
            DOTween.To(() => _arcRegion.FillProgress, value => _arcRegion.FillProgress = value, 1, time)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    UniRx.Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(t =>
                    {
                        ReturnPool();
                    }).AddTo(this);
                });

            return this;
        }
        
        public ArcIndicator SetPool(ObjectPooling<ObjectBase> Pool)
        {
            this.Pool = Pool;
            return this;
        }
        
        public void Build()
        {
            _arcRegion.Angle = _angle;
            _arcRegion.Radius = _radius;
            _arcRegion.Arc = _arc;
        }
    }
}