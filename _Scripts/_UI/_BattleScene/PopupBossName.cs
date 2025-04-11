using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectM.Battle
{
    public class PopupBossName : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Battle_BossName;
        public override PopupType PopupType => PopupType.Alarm;

        public GameObject[] goGroup;
        public TMP_Text[] title;
        public Image[] HpBar;
        [HideInInspector] public List<FloatReactiveProperty> Hp = new List<FloatReactiveProperty>();
        protected override void Init()
        {
            ParticleImageUnscaled();
            goGroup.ForEach(t => t.SetActive(false));
            Hp.Add(new FloatReactiveProperty(1));
            Hp.Add(new FloatReactiveProperty(1));
        
            // Hp = new FloatReactiveProperty(1);
            // BattleManager.Instance.BossSpawn.Subscribe(t =>
            // {
            //     var order = BattleManager.Instance.tbCurrentBoss.Order;
            //     var bossName = BattleManager.Instance.BossMonster[order -1].Name;
            //     title.SetText(LocaleManager.GetLocale(bossName));
            //   
            //     Hp.Subscribe(v =>
            //     {
            //         HpBar.fillAmount = v;
            //     }).AddTo(this);
            // }).AddTo(this);

            BattleManager.Instance.BossDespawn.Subscribe(t =>
            {
                Hide();
            }).AddTo(this);
        }

        public FloatReactiveProperty SetData(int i, string bossName)
        {
            int idx = i;
            goGroup[idx].SetActive(true);
            title[idx].SetText(bossName.Locale());
            Hp[idx].Subscribe(v =>
            {
                HpBar[idx].fillAmount = v;
                if (v <= 0)
                    goGroup[idx].SetActive(false);
            }).AddTo(this);
            return Hp[idx];
        }
    }
}