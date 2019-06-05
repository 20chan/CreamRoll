using System.Reflection;

namespace CreamRoll.Helpers {
    public static class MethodInvokeHelper {
        public static T Invoke<T>(this MethodBase method, object instance, params object[] parameters) {
            return (T)method.Invoke(instance, parameters);
        }
    }
}