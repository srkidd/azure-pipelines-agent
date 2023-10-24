using System;
using System.ComponentModel;

namespace Agent.Sdk
{
    public interface ISecretMasker
    {
        void AddRegex(String pattern);

        void AddValue(String value);

        void AddValueEncoder(ValueEncoder encoder);

        ISecretMasker Clone();

        String MaskSecrets(String input);

        int MinSecretLength { get; set; }

        void RemoveShortSecretsFromDictionary();
    }
}
