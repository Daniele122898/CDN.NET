using System;
using System.ComponentModel.DataAnnotations;

namespace CDN.NET.Backend.Helpers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AtLeastOnePropertyAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // Use reflection to get properties of object
            var typeInfo = value.GetType();
            var propertyInfos = typeInfo.GetProperties();

            foreach (var property in propertyInfos)
            {
                if (property.GetValue(value, null) != null)
                {
                    // At least one property is not null
                    return true;
                }
            }
            // We have not returned true yet, thus all properties are null
            return false;
        }
    }
}