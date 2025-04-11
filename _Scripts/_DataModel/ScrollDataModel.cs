using InfiniteValue;
using UnityEngine;

namespace ProjectM
{
    public enum Numerals
    {
        Comma,
        KMGT,
    }
    public class ScrollDataModel
    {
        public int Index;                   // 테이블 Idx
        public Numerals GoodsType;         // 재화표기
        public int IconType;                // 재화 0 , 장비타입 1~10
        public InfVal Count;
        public int Grade;
        public Sprite IconSprite;
        public Sprite BackgroundSprite;
        public int RewardType;
        public int bonusIconType;
    }
}