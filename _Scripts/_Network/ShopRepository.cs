using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Phoenix;

// 상품관련 
// 패키지, 인앱, 이벤트상품 등.

namespace ProjectM
{
    public partial class APIRepository
    {
        public static void RequestIap(Dictionary<string, object> payload, Action<CommonApiResultData> onResponse, Action<Reply> onFail)
        {
            Action<CommonApiResultData> resp = data =>
            {
                
                data.SetData();
            };

            resp += onResponse;

            Net.PushUserChannel("iap", payload, resp, true, onFail);
        }

        public static void RequestShopBuy(Dictionary<string, object> payload, Action<CommonApiResultData> onResponse, Action<Reply> onFail)
        {
            Action<CommonApiResultData> resp = data =>
            {
                data.SetData();
            };

            resp += onResponse;

            Net.PushUserChannel("shop_buy", payload, resp, true, onFail);
        }

        public static void RequestAdBuff(Dictionary<string, object> payload, Action<AdBuffData> onResponse, Action<Reply> onFail)
        {
            Action<AdBuffData> resp = data =>
            {
                if (data.ad_Buff != null)
                    UserDataManager.Instance.payInfo.AddADBuff(data.ad_Buff);
                
                if (data.ad_buffs.IsNullOrEmpty() == false)
                    UserDataManager.Instance.payInfo.AddADBuff(data.ad_buffs);
                
                SubscriptionAdBuff();
            };

            resp += onResponse;
            
            Net.PushUserChannel("ad_buff_active", payload, resp, true, onFail);
        }
    }
}