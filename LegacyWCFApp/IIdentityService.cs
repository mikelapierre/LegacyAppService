using System.ServiceModel;

namespace LegacyWCFApp
{
    [ServiceContract]
    public interface IIdentityService
    {
        [OperationContract]
        ((string key, string value)[] headers, (string key, string value)[] claims) GetHeadersAndClaims();
    }
}
