using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ROD_core
{
    public static class EnumExtension
    {
        public static List<T> EnumToList<T>(this Enum value)
        {
            Type enumType = typeof(T);

            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");

            Array enumValArray = Enum.GetValues(enumType);

            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (long val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }


        public static List<T> ListEnumElements<T>(this Enum value)
        {
            List<T> EnumElements = new List<T>();
            List<T> semanticList = value.EnumToList<T>();
            foreach (T _semantic in semanticList)
            {

                if ((Convert.ToInt64(value) & Convert.ToInt64(_semantic)) == Convert.ToInt64(_semantic))
                {
                    EnumElements.Add(_semantic);
                }
            }
            return EnumElements;
        }
    }

    public static class RODUtiles
    {
        public static T GetNewObject<T>()
        {
            try
            {
                return (T)typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { });
            }
            catch
            {
                return default(T);
            }
        }
    }
}
