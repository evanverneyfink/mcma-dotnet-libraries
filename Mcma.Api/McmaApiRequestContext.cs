using System.Collections.Generic;

namespace Mcma.Api
{
    public class McmaApiRequestContext
    {
        public McmaApiRequestContext(McmaApiRequest request, IDictionary<string, string> contextVariables)
        {
            Request = request;
            ContextVariables = contextVariables;
        }

        public McmaApiRequest Request { get; }

        public IDictionary<string, string> ContextVariables { get; }

        public McmaApiResponse Response { get; } = new McmaApiResponse();
    }
}
