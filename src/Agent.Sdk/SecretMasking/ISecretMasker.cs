using System;
using ValueEncoder = Microsoft.TeamFoundation.DistributedTask.Logging.ValueEncoder;
using ISecretMaskerVSO = Microsoft.TeamFoundation.DistributedTask.Logging.ISecretMasker;
//using Microsoft.TeamFoundation.DistributedTask.Logging;

namespace Agent.Sdk.SecretMasking;
public interface ISecretMasker : ISecretMaskerVSO
{
    new void AddRegex(String pattern);

    void AddCompiledRegex(String pattern);

    new void AddValue(String value);

    new void AddValueEncoder(ValueEncoder encoder);

    new ISecretMasker Clone();
    
    new String MaskSecrets(String input);

    new int MinSecretLength { get; set; }

    bool UseCompiledRegex { get; set; }

    new void RemoveShortSecretsFromDictionary();
}