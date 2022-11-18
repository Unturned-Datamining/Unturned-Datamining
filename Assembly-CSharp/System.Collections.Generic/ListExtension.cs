using System.Reflection;
using System.Reflection.Emit;

namespace System.Collections.Generic;

public static class ListExtension
{
    private static class ListInternalArrayAccessor<T>
    {
        public static Func<List<T>, T[]> Getter;

        static ListInternalArrayAccessor()
        {
            DynamicMethod dynamicMethod = new DynamicMethod("get", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(T[]), new Type[1] { typeof(List<T>) }, typeof(ListInternalArrayAccessor<T>), skipVisibility: true);
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic));
            iLGenerator.Emit(OpCodes.Ret);
            Getter = (Func<List<T>, T[]>)dynamicMethod.CreateDelegate(typeof(Func<List<T>, T[]>));
        }
    }

    public static void RemoveAtFast<T>(this List<T> list, int index)
    {
        list[index] = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
    }

    public static bool RemoveFast<T>(this List<T> list, T item)
    {
        int num = list.IndexOf(item);
        if (num < 0)
        {
            return false;
        }
        list.RemoveAtFast(num);
        return true;
    }

    public static T[] GetInternalArray<T>(this List<T> list)
    {
        return ListInternalArrayAccessor<T>.Getter(list);
    }
}
