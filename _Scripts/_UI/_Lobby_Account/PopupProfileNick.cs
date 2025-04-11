using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Doozy.Runtime.UIManager.Components;
using InfiniteValue;
using Newtonsoft.Json.Linq;
using TMPro;
using UniRx;
using UnityEngine;

namespace ProjectM
{
    public class PopupProfileNick : Popup
    {
        public override PopupName ID { get; set; } = PopupName.Lobby_ProfileNick;
        public TMP_InputField inputNick;
        public TMP_Text placeholder;
        public UIButton closeBtn, postBtn;
        public GameObject freeIcon, diaIcon;
        public TMP_Text diaCost;
        private bool enough;
        private bool isFree;
        private string changeName;

        private int minChar = 2;
        private int maxByte = 24;
        
        public bool isWhiteText { get; set; }   // 공백
        public bool isSpecialText { get; set; } // 특문
        public bool isMatchText { get; set; }   // 금칙어
        public bool isIncludeText { get; set; } // 금칙어
        public int currency { get; set; }   // 재화
        protected override void Init()
        {
            closeBtn.AddEvent(Hide);
            postBtn.AddEvent(OnClickPost);
            placeholder.SetText(UserDataManager.Instance.userInfo.GetPlayerData().name);
            inputNick.onEndEdit.AddListener(_name =>
            {
                // 금칙어 체크.
                // deleteBtn.interactable = string.Equals(playerName.Trim(), _name.Trim());
                changeName = _name;
                EndChackName(changeName);
            });

            inputNick.onValueChanged.AddListener(_name =>
            {
                var _byte = Encoding.UTF8.GetBytes(_name).Length;
                if (_byte >= maxByte)
                {
                    inputNick.text = changeName;
                }
                else
                {
                    changeName = _name;
                }
            });

            currency = TableDataManager.Instance.GetConfig(11).Value;
            
            var count = InfVal.Parse(TableDataManager.Instance.GetConfig(12).Value.ToString());
            diaCost.SetText(count.ToGoodsString());
            UserDataManager.Instance.currencyInfo.SubscribeItemRx((CurrencyType)currency, ItemRxAction(count)).AddTo(this);
        }

        void OnClickPost()
        {
            if (isSpecialText || isWhiteText || isIncludeText || isMatchText)
            {
                var message = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                message.InitBuilder()
                    .SetMessage("Nickname_Change_Error_04".Locale())
                    .Build();
                return;
            }

            if (!enough && !isFree)
            {
                var _name = TableDataManager.Instance.data.GoodsType.Single(t => t.TypeID == currency).Name.Locale();
                var message = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                message.InitBuilder()
                    .SetMessage("Not_Enough_Price".Locale(_name))
                    .Build();
                return;
            }
            
            
            
            var payload = new Dictionary<string, object>
            {
                { "name", changeName},
            };
            APIRepository.RequestNickChange(payload, o =>
            {
                Hide();
            }, (r) =>
            {
                var reason = r.Response.Unbox<JObject>()["reason"].ToString();
                var message = UIManager.Instance.Get(PopupName.Common_Alarm) as Alarm;
                message.InitBuilder()
                    .SetMessage(reason.Locale())
                    .Build();
            });
        }
        
        private Action<InfVal> ItemRxAction(InfVal needItemCount)
        {
            return (i) =>
            {
                var data = UserDataManager.Instance.userInfo.GetPlayerData().name;
                if (data.Contains('_'))
                {
                    // Free
                    isFree = true;
                    freeIcon.SetActive(isFree);
                    diaIcon.SetActive(!isFree);
                    return;
                }
                

                isFree = false;
                enough = UserDataManager.Instance.currencyInfo.ValidGoods(i, needItemCount);
                
                freeIcon.SetActive(isFree);
                diaIcon.SetActive(!isFree);
            };
        }

        void EndChackName(string changeName)
        {
            //최소 글자수 2자
            if (changeName.Length < minChar)
            {
                // 글자수 모자름.
                postBtn.interactable = false;
                return;
            }

            postBtn.interactable = true;

            isSpecialText = CheckingSpecialText(this.changeName);
            // Debug.Log($"특문 {isSpecialText}");
            
            isWhiteText = CheckingEmpty(this.changeName);
            // Debug.Log($"공백 {isWhiteText}");
            isMatchText = CheckingMatch(this.changeName);
            isIncludeText = CheckingInclude(this.changeName);

        }

        public bool CheckingSpecialText(string txt)
        {
            string str = @"[~!@\#$%^&*\()\=+|\\/:;?""<>']";
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(str);
            return rex.IsMatch(txt);
        }
        
        public bool CheckingEmpty(string txt)
        {
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex("\\s");
            return rex.IsMatch(txt);
        }

        public bool CheckingMatch(string txt)
        {
            return TableDataManager.Instance._matchList.Any(t => string.Equals(t, txt));
        }
        
        public bool CheckingInclude(string txt)
        {
            return false;
            // return TableDataManager.Instance._includeList.Any(txt.Contains);
        }
        
        // public bool CheckingLn(string txt)
        // {
        //     string str = @"[a-zA-Z0-9가-힇ㄱ-ㅎㅏ-ㅣぁ-ゔァ-ヴー々〆〤一-龥]";
        //     System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(str);
        //     return rex.IsMatch(txt);
        // }
    }
}