using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Functions
{
    public static class DeserializerHelper
    {
        public static int? GetNumber(this JValue value)
        {
            if ((value != null) && (((value.Type == JTokenType.Float) || (value.Type== JTokenType.Integer)) &&
                (value.Value != null) && (int.TryParse(value.Value.ToString(), out int number))))
                return number;
            else
                return null;
        }

        public static int? GetFloat(this JValue value)
        {
            if ((value != null) && (value.Type == JTokenType.Float &&
                (value.Value != null) && (int.TryParse(value.Value.ToString(), out int number))))
                return number;
            else
                return null;
        }

        public static int? GetInteger(this JValue value)
        {
            if ((value != null) && (value.Type == JTokenType.Integer &&
                (value.Value != null) && (int.TryParse(value.Value.ToString(), out int number))))
                return number;
            else
                return null;
        }

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
                (DateTimeOffset.TryParse(value.Value.ToString(), null as IFormatProvider, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset dt)))
                return dt;
            else
                return null;
        }

        public static DateTimeOffset? GetDate(this JValue value)
        {
            if ((value != null) && ((value.Type == JTokenType.Date) || (value.Type==JTokenType.String)) &&
                (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value.ToString()) == false) &&
                (DateTimeOffset.TryParse(value.Value.ToString(), null as IFormatProvider, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTimeOffset dt)))
                return dt;
            else
                return null;
        }

        public static bool? GetBoolean(this XElement value)
        {
            if ((value != null) && (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value) == false) &&
                (bool.TryParse(value.Value.ToString(), out bool bl)))
                return bl;
            else
                return null;
        }

        public static bool? GetBoolean(this JValue value)
        {
            if ((value != null) && (value.Value != null) && (string.IsNullOrWhiteSpace(value.Value.ToString()) == false) &&
                (bool.TryParse(value.Value.ToString(), out bool bl)))
                return bl;
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
