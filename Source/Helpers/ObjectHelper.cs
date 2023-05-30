using System.Reflection;

namespace TrayToolkit.Helpers
{
    public static class ObjectHelper
    {
        /// <summary>
        /// Returns value of objects's private field
        /// </summary>
        public static T GetPrivateFieldValue<T>(this object c, string fieldName)
        {
            return (T)c.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c);
        }
    }
}
