
using System.Collections.Generic;
using Mcma.Api;

namespace Mcma.Aws.Api
{
    public class ApiGatewayRequestContext : McmaApiRequestContext, IStageVariableProvider
    {
        public ApiGatewayRequestContext(McmaApiRequest request, IDictionary<string, string> contextVariables)
            : base(request, contextVariables)
        {
        }

        public IDictionary<string, string> StageVariables => ContextVariables;
    }
}