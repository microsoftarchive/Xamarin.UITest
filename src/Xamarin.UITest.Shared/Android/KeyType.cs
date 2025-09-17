using System;
using Xamarin.UITest.Shared.Extensions;

namespace Xamarin.UITest.Shared.Android
{
    public class KeyType
	{
        public static readonly KeyType RSA = new KeyType("withRSA");
        public static readonly KeyType DSA = new KeyType("withDSA");
        public static readonly KeyType EC = new KeyType("withECDSA");

        static readonly KeyType[] _knownTypes = { RSA, DSA, EC };

        readonly string _signingPostfix;

        KeyType(string signingPostFix) 
        {
            _signingPostfix = signingPostFix;
        }

        public bool SameType(string sigAlgo)
        {
            return sigAlgo.EndsWithIgnoreCase(_signingPostfix);
        }

        public string SigningAlgorithmWithMaxCompatability()
        {
            return string.Format("SHA1{0}", _signingPostfix);
        }

        public static KeyType FromSigningAlgorithm(string sigAlgo)
        {
            foreach (KeyType kt in _knownTypes)
            {
                if (kt.SameType(sigAlgo))
                {
                    return kt;
                }
            }   
           throw new Exception(string.Format("Unknown signature algorithm, unable to determine key type: {0}", sigAlgo));
        }
	}
}