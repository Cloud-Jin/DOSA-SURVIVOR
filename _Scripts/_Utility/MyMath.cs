using System;
using System.Collections;
using System.Collections.Generic;
using InfiniteValue;
using ProjectM.Battle;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace ProjectM
{
    public class MyMath
    {
        public static int Pick(int[] probs)
        {
            float total = 0;

            foreach (float elem in probs)
            {
                total += elem;
            }

            float randomPoint = Random.value * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                {
                    return i;
                }
                else
                {
                    randomPoint -= probs[i];
                }
            }

            return probs.Length - 1;
        }

        public static InfVal Increase(InfVal value, float increaseValue)
        {
            return value * (1 + increaseValue / (double)100);
        }
        
        public static float Increase(float value, float increaseValue)
        {
            return value * (1 + increaseValue / 100f);
        }
        
        public static InfVal Decrease(InfVal value, float decreaseValue)
        {
            return value * (1 - decreaseValue / (double)100);
        }
        
        public static float Decrease(float value, float decreaseValue)
        {
            return value * (1 - decreaseValue / 100f);
        }
        
        public static bool Compare(float value, float min, float max)
        {
            // min max 사이값인지 판별
            return value >= min && value <= max;
        }

        public static InfVal CalcDamage(InfVal playerActtack, float ratio, float incRatio )
        {
            var value = (playerActtack * (ratio / (double)100));

            var retval = Increase(value, incRatio);
            return retval;
        }
        
        public static float CalcCoefficient(float value, float coefficient)
        {
            var retval = (value * (coefficient / 100f));
            return retval;
        }
        
        public static InfVal CalcCoefficient(InfVal value, InfVal coefficient)
        {
            var retval = (value * (coefficient / (double)100));
            return retval;
        }

        
        // 타원형 구하기
        static Vector3 GetElipseEdgesPos(float x_radius, float y_radius)
        {
            Vector3 returnPos = Vector3.zero;
            Vector2 center = Vector2.zero;

            Vector2 pos = Random.insideUnitCircle.normalized;
            var v = pos - center;
            float angle = Mathf.Atan2(v.y * x_radius / y_radius, v.x) * Mathf.Rad2Deg;

            returnPos.x = center.x + Mathf.Cos(Mathf.Deg2Rad * angle) * x_radius;
            returnPos.y = center.y + Mathf.Sin(Mathf.Deg2Rad * angle) * y_radius;
            returnPos.z = 0;

            return returnPos;
        }
        
        public static Vector3 GetRandomPosition(Battle.Map map, float xRadius = 0, float yRadius = 0) 
        {
            Vector3 position = new Vector3();
            
            float f = Random.value > 0.5f ? -1f : 1f;
            if(map == Map.Infinite)
            {
                var pos = GetElipseEdgesPos(xRadius, yRadius);
                position.x = pos.x;
                position.y = pos.y;

                // float radius = 8f;
                // // position.x = Random.Range(-radius, radius);
                // float x = Random.Range(-radius, radius);
                // float y_b = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2));
                //
                // y_b *= Random.Range(0, 2) == 0 ? -1 : 1;
                //
                // float y = y_b;
                //
                // position.x = x + playerPos.x;
                // position.y = y + playerPos.y;
            }
            else if(map == Battle.Map.Vertical)
            {
                var area = new Vector2(xRadius, yRadius);
                position.x = Random.Range(-area.x, area.x);
                position.y = (area.y * f * Random.Range(0.8f, 1f));
            }
            else if (map == Battle.Map.Rect)
            {
                var area = new Vector2(xRadius, yRadius);
                if (Random.value > 0.5f)
                {
                    position.x = Random.Range(-area.x, area.x);
                    position.y = area.y * f;
                }
                else
                {
                    position.x = area.x * f;
                    position.y = Random.Range(-area.y, area.y);
                }
            }

            position.z = 0;
            
            return position;
        }
        
        public static Vector3 GetBossSpawnPosition(Transform player, Battle.Map map)
        {
            Vector3 position = new Vector3();
            
            float f = Random.value > 0.5f ? -1f : 1f;

            switch (map)
            {
                case Map.Infinite:
                    position.x = player.position.x;
                    position.y = player.position.y + 3;
                    break;
                case Map.Vertical:
                    position.x = 0;
                    position.y = player.position.y + 3;
                    break;
                case Map.Rect:
                    position = Vector3.zero;
                    break;
            }

            position.z = 0;
            
            return position;
        }
        
        public static Vector3 GetCrackSpawnPosition(Battle.Map map, Vector3 playerPos)
        {
            
            Vector3 position = new Vector3();
            
            float f = Random.value > 0.5f ? -1f : 1f;
            if(map == Map.Infinite)
            {
                float radius = 9f;
                float x = Random.Range(-radius, radius);
                float y_b = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(x, 2));

                y_b *= Random.Range(0, 2) == 0 ? -1 : 1;

                float y = y_b;

                position.x = x + playerPos.x;
                position.y = y + playerPos.y;
            }
            else if(map == Battle.Map.Vertical)
            {
                var area = new Vector2(3, 9);
                position.x = Random.Range(-area.x, area.x);
                position.y = (area.y * f) + playerPos.y;
            }
            else if (map == Battle.Map.Rect)
            {
                var area = new Vector2(6, 5.5f);
                {
                    position.x = area.x * f;
                    position.y = Random.Range(-area.y, area.y);
                }
            }

            position.z = 0;
            
            return position;
        }
        
        public static Vector3 RandomCirclePoint(float radius,int minAngle = 0, int maxAngle = 360)
        {
            int ran = Random.Range(minAngle, maxAngle) + 90; //랜덤으로 0~360도
            float x = Mathf.Cos(ran*Mathf.Deg2Rad) * radius; // 정해진 위치에서 radius만큼 떨어진 원형 랜덤 방향으로 생성
            float y = Mathf.Sin(ran*Mathf.Deg2Rad) * radius; // 정해진 위치에서 radius만큼 떨어진 원형 랜덤 방향으로 생성
            Vector3 pos = new Vector3(x, y, 0);
            return pos;
        }
        
        public static Vector3 RandomDonut(float radius, float f = 0.8f)
        {
            int ran = Random.Range(0, 360); //랜덤으로 0~360도
            float x = Mathf.Cos(ran * Mathf.Deg2Rad) * radius * Random.Range(f, 1f); // 정해진 위치에서 radius만큼 떨어진 원형 랜덤 방향으로 생성
            float y = Mathf.Sin(ran * Mathf.Deg2Rad) * radius * Random.Range(f, 1f); // 정해진 위치에서 radius만큼 떨어진 원형 랜덤 방향으로 생성
            Vector3 pos = new Vector3(x, y, 0);
            return pos;
        }
        
        public static Vector3 CalcDonut(float radius, float angle, float f = 0.8f)
        {
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius * Random.Range(f, 1f); // 정해진 위치에서 radius만큼 떨어진 원형 방향으로 생성
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius * Random.Range(f, 1f); // 정해진 위치에서 radius만큼 떨어진 원형 방향으로 생성
            Vector3 pos = new Vector3(x, y, 0);
            return pos;
        }
        
        public static Vector3 Lerp(Vector3 p1, Vector3 p2, Vector3 p3, float value)
        {
            Vector3 a = Vector3.Lerp(p1, p2, value);
            Vector3 b = Vector3.Lerp(p2, p3, value);

            Vector3 d = Vector3.Lerp(a, b, value);
            return d;
        }
        public static Vector2 GetCircleSize(float value)
        {
            return (Random.insideUnitCircle * value) * 0.5f;
        }

        /// <summary>
        /// B to A Vector
        /// </summary>
        /// <param name="b">target</param>
        
        public static Vector2 GetDirection(Vector2 a, Vector2 b)
        {
            return (b - a).normalized;
        }

        public static List<float> CalcAngleCount(int angle, int count)
        {
            // 부채꼴 앵글 구하기
            List<float> angles = new List<float>();
            
            if (count > 2)
            {
                var angleValue = (float)angle / (count - 1);
                var firstAngle = (angle / 2f) * -1;
                for (int i = 0; i < count; i++)
                {
                    angles.Add(firstAngle + angleValue * i);
                }
            }
            else if(count == 2)
            {
                angles.Add((angle / 2f) * -1);
                angles.Add((angle / 2f) * 1);
            }
            else
            {
                angles.Add(0);
            }
            
            return angles;
        }

        /// <summary>
        /// N분율 
        /// </summary>
        /// <param name="per">N분율</param>
        /// <param name="ratio">확률값</param>
        /// <returns></returns>
        public static bool RandomPer(int per, int ratio)
        {
            return (Random.Range(0, per) < ratio);
        }
        
        
        // Get Angle -180 ~ 180
        public static float GetAngle(Vector3 vStart, Vector3 vEnd)
        {
            Vector3 v = vEnd - vStart;

            return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        }
        
        // Degree 계산
        public static float GetDegree(Vector3 vStart, Vector3 vEnd, Vector3 dir)
        {
            var interV = vEnd - vStart;
            float dot = Vector3.Dot(interV.normalized, dir);
            float theta = Mathf.Acos(dot);
            float degree = Mathf.Rad2Deg * theta;

            return degree;
        }
        
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length != 6)
            {
                return Color.white;
            }

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, 255);
        }
    }
}