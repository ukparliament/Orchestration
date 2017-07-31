using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Functions
{
    public static class DeserializerHelper
    {
        public static string GetText(this XElement value)
        {
            if ((value != null) && (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value) == false))
                return value.Value;
            else
                return null;
        }

        public static string GetText(this JValue value)
        {
            if ((value != null) && (value.Type == JTokenType.String) &&
                (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value.ToString()) == false))
                return value.Value.ToString();
            else
                return null;
        }

        public static DateTimeOffset? GetDate(this XElement value)
        {
            if ((value != null) && (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value) == false) &&
                (DateTimeOffset.TryParse(value.Value.ToString(), out DateTimeOffset dt)))
                return dt;
            else
                return null;
        }

        public static DateTimeOffset? GetDate(this JValue value)
        {
            if ((value != null) && (value.Type == JTokenType.Date) &&
                (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value.ToString()) == false) &&
                (DateTimeOffset.TryParse(value.Value.ToString(), out DateTimeOffset dt)))
                return dt;
            else
                return null;
        }

        public static IEnumerable<string> GiveMeSingleTextValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            else
                return new string[] { value };
        }

        public static IEnumerable<DateTimeOffset> GiveMeSingleDateValue(DateTimeOffset? value)
        {
            if (value.HasValue == false)
                return null;
            else
                return new DateTimeOffset[] { value.Value };
        }

        public static IEnumerable<int> GiveMeSingleIntegerValue(int? value)
        {
            if (value.HasValue == false)
                return null;
            else
                return new int[] { value.Value };
        }
    }
}
