using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WebDiskApplication.Areas.WebDisk.Manage.Utils
{
    public class RandomUtil
    {
        private static System.Random random = new System.Random(); //?

        public static int RandomInt(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        public static string RandomChar(int length, bool isUpper)
        {
            StringBuilder builder = new StringBuilder();

            char ch;

            for (int i = 1; i <= length; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))); //이게 무슨 의미일까?
                builder.Append(ch);
            }

            if (isUpper) return builder.ToString().ToUpper();
            return builder.ToString();
        }



        public static string MixedIntChar(int length, bool isUpper)
        {
            int rnum = 0;
            int i, j;
            string ranStr = null;

            for (i = 1; i <= length; i++)
            {
                /*
                 아스키코드에서
                 48부터 57이 숫자 0부터 9
                 65부터 90이 대문자 A부터 Z
                 97부터 122까지가 소순자 a부터 z
                 */

                for (j = 48; j <= 122; j++)
                {
                    rnum = random.Next(48, 123); //48과 122사이의 랜덤 숫자 추출
                    if (rnum >= 48 && rnum <= 122 && rnum != 92 &&(rnum <= 57 || rnum >= 65) && (rnum >= 97 || rnum <= 122))
                    {
                        //조건을 만족하는 숫자가 나오면 가장 가까운 반복문 탈출
                        break;
                    }

                }

                ranStr += Convert.ToChar(rnum);


            }

            if (isUpper) ranStr = ranStr.ToUpper(); else ranStr = ranStr.ToLower();

            return ranStr;
        }
    }
}