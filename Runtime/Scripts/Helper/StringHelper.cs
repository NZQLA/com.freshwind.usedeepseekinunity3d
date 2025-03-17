using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Business
{
    /// <summary>
    /// Some  helper  methods   for string .
    /// </summary>
    public static class StringHelper
    {

        public const string Empty = "";

        public static bool IsNullOrEmpty(this string self)
        {
            return self == null || string.IsNullOrEmpty(self);
        }


        /// <summary>
        /// Join the specified datas with yourself ruler .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas">all the data , it may be array / list / queue / stack / dictionary /...</param>
        /// <param name="yourToString">your ruler , convert the data to string </param>
        /// <returns></returns>
        public static string Join<T>(IEnumerable<T> datas, Func<T, string> yourToString)
        {
            if (datas == null || datas.Count() == 0)
            {
                return Empty;
            }

            if (yourToString == null)
            {
                return Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var data in datas)
            {
                sb.Append(yourToString(data));
            }

            return sb.ToString();
        }
    }
}