using Amazon.Lambda.Core;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;
using Dynatrace.OpenTelemetry;
using Dynatrace.OpenTelemetry.Instrumentation.AwsLambda;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MySimpleFunctionWithApiGW
{

    public class Function
    {
        private static readonly TracerProvider? tracerProvider;
        static Function() {
            DynatraceSetup.InitializeLogging();
            var secret = new Secret().GetSecret();
            Environment.SetEnvironmentVariable("DT_CONNECTION_AUTH_TOKEN", secret); //Get this from a secret storage
            tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddDynatrace()
                .AddHttpClientInstrumentation()
                // Configures AWS Lambda invocations tracing
                .AddAWSLambdaConfigurations(c => c.DisableAwsXRayContextExtraction = true)
                // Instrumentation for creation of span (Activity) representing AWS SDK call.
                // Can be omitted if there are no outgoing AWS SDK calls to other AWS Lambdas and/or calls to AWS services like DynamoDB and SQS.
                .AddAWSInstrumentation(c => c.SuppressDownstreamInstrumentation = true)
                // Adds injection of Dynatrace-specific context information in certain SDK calls (e.g. Lambda Invoke).
                // Can be omitted if there are no outgoing calls to other Lambdas via the AWS Lambda SDK.
                .AddDynatraceAwsSdkInjection()
                .Build();
            LambdaLogger.Log($"Initializing Setup\n");
        }
        public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            return AWSLambdaWrapper.Trace(tracerProvider, FunctionHandlerInternal, request, context);
        }
        private APIGatewayHttpApiV2ProxyResponse FunctionHandlerInternal(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = OutgoingHttp(),
            };
        }
        private readonly HttpClient httpClient = new HttpClient();
        public string OutgoingHttp()
        {
            _ = httpClient.GetAsync("https://aws.amazon.com").Result;
            LambdaLogger.Log($"Initializing Setup\n");
            return "OK";
        }
        
    }
}