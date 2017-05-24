//The random string generator class can be intialized to use different random souce such as RNGCrypto and
//basic Random generator etc. The id can be composed of any character set such as alphanumeric,
//alphabetic, or numeric characters. The id can be of any length. The chance of collision will be
//based on the number of ids already generated, the length of the id, and the size of the character
//set.

//The larger the number of ids already generated, the more the probablity of collision. The larger
//the size of the character set, the less the probability of collision. The longer the length of the
//id, the less the probability of collision.

//The probility of collision can be estimated based on an equation derived from the famous Birthday
//problem. The equation is: P(n) = 1 - e^(-n^2 / (2*x)), where n is the number of ids to be generated,
//x is number of all distinct values for the ids, and P(n) is the probability of collision.

//For example, if the number of ids already generated is 1 million, the character set size is 62,
//and the id length is 8, the probability of collision of the next newly generated id would be 0.23%

//Depending on the number of ids we plan to generate, the length of character set and length of id should
//be selected to guarantee a low probability of collision.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Functions.IdGenerator
{
    public class RandomStringGenerator
    {
        public enum RandomSource { RNGCrypto, ImprovedRNGCrypto, BasicRandom };

        public const string alphanumericCharacters =
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                    "abcdefghijklmnopqrstuvwxyz" +
                    "0123456789";

        public const string alphabeticCharacters =
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                    "abcdefghijklmnopqrstuvwxyz";

        public const string numericCharacters = "0123456789";

        private RandomSource _source;
        Random rand = new Random();

        public RandomStringGenerator(RandomSource source = RandomSource.RNGCrypto)
        {
            _source = source;
        }

        public string GetAlphanumericId(int length)
        {
            return GetRandomString(length, alphanumericCharacters);
        }

        public string GetAlphabeticId(int length)
        {
            return GetRandomString(length, alphabeticCharacters);
        }

        public string GetNumericId(int length)
        {
            return GetRandomString(length, numericCharacters);
        }

        public byte[] PopulateRandomBytes(int length)
        {
            var bytes = new byte[length * 8];
            if (_source == RandomSource.RNGCrypto)
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetBytes(bytes);
            }
            else if (_source == RandomSource.ImprovedRNGCrypto)
            {
                byte[] data = new byte[1];
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                crypto.GetNonZeroBytes(bytes);
            }
            else if (_source == RandomSource.BasicRandom)
            {
                rand.NextBytes(bytes);
            }
            return bytes;
        }

        public string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", "length");
            if (length > int.MaxValue / 8)
                throw new ArgumentException("length is too big", "length");
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");
            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", "characterSet");

            var bytes = PopulateRandomBytes(length);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }
            return new string(result);
        }
    }
}
