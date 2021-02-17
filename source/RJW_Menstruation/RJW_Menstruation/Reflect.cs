using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using RimWorld;

namespace RJW_Menstruation
{
    public static class Reflector
    {
        public static object GetMemberValue(this Type type, string name)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo fieldInfo = type?.GetField(name, flags);
            return fieldInfo?.GetValue(null);
        }

        public static object GetMemberValue(this object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo fieldInfo = obj?.GetType().GetField(name, flags);
            return fieldInfo?.GetValue(obj);
        }

        public static void SetMemberValue(this Type type, string name, object value)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo fieldInfo = type?.GetField(name, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(null, value);
            }
        }

        public static void SetMemberValue(this object obj, string name, object value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo fieldInfo = obj?.GetType().GetField(name, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }


    }
}
