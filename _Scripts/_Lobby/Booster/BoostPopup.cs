using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Components;
using UnityEngine;

namespace ProjectM
{
    public class BoostPopup : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Boost;

        private List<ADBuffType> _adBuffTypeList;

        public List<BoostSlot> boostSlotList;
        [Space(20)]
        public UIButton closeBtn;
        
        protected override void Init()
        {
        }

        private void Start()
        {
            uiPopup.OnShowCallback.Event.AddListener(SetData);
            
            closeBtn.AddEvent(Hide);
            
            _adBuffTypeList = TableDataManager.Instance.data.ADBuffType.OrderBy(t => t.ADBuffOrder).ToList();
        }

        public void SetData()
        {
            boostSlotList.ForEach(b => b.SetActive(false));
            
            int buffCount = boostSlotList.Count <= _adBuffTypeList.Count ? boostSlotList.Count : _adBuffTypeList.Count;

            for (int i = 0; i < buffCount; ++i)
            {
                boostSlotList[i].SetActive(true);
                boostSlotList[i].SetData(_adBuffTypeList[i]);
            }
        }
    }
}