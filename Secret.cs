using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
namespace MySimpleFunctionWithApiGW
{
    public class Secret
    {
        public string GetSecret()

        {
            string secretName = "DT_CONNECTION_AUTH_TOKEN";
            string region = "us-east-1";
            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };
            GetSecretValueResponse response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();
            return response.SecretString;
        }
    }
}