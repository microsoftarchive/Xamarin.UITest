using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xamarin.UITest.Shared.Json
{
    public class JsonTranslator
    {
        public T Deserialize<T>(string json)
        {
            var deserializationJsonConverter = new JsonCaseConverter();
            json = deserializationJsonConverter.ConvertedCaseJObject(json);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public T[] DeserializeArray<T>(JArray jArray)
        {
            return jArray.Children().Select(c => ConvertToSimpleType<T>(c)).ToArray();
        }

        public T[] DeserializeArray<T>(string json)
        {
            var jArray = JArray.Parse(json);

            return DeserializeArray<T>(jArray);
        }

        T ConvertToSimpleType<T>(JToken jToken)
        {
            if (typeof (T).Name.Equals("RawJson"))
            {
                return (T)Activator.CreateInstance(typeof(T), jToken.ToString());
            }

            if (!AllowedConversion(jToken.Type, typeof (T)))
            {
                throw new InvalidOperationException(string.Format("Actual type '{0}' not compatible with declared type '{1}'. Value: {2}", jToken.Type, typeof(T), jToken.ToString())); 
            }

            return jToken.ToObject<T>();
        }

        bool AllowedConversion(JTokenType jsonType, Type targetType)
        {
            if (targetType == typeof (object))
            {
                return true;
            }
                
            switch (jsonType)
            {
                case JTokenType.Boolean:
                    return ConversionToTargetTypeExists(typeof(bool), targetType);
 
                case JTokenType.Integer:
                    return ConversionToTargetTypeExists(typeof(int), targetType);

                case JTokenType.Float:
                    return ConversionToTargetTypeExists(typeof(float), targetType);

                case JTokenType.String:
                    return ConversionToTargetTypeExists(typeof(string), targetType);
                
                case JTokenType.Object:
                    return ConversionToTargetTypeExists(typeof(object), targetType);

                case JTokenType.Null:
                    return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;

                default:
                    return false;
            }
        }

        bool ConversionToTargetTypeExists(Type sourceType, Type targetType)
        {
            if (sourceType == targetType)
            {
                return true;
            } 

            // Try the runtime equvialent of casting to from sourceType to targetType. If sucess then the caller will be 
            // able to cast without any exceptions being thrown 
            try
            {
                Expression.Convert(Expression.Parameter(sourceType, null), targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}