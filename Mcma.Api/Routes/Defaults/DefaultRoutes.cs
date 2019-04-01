using Mcma.Api;
using Mcma.Core.Logging;
using System.Net;
using Mcma.Core;
using Mcma.Core.Utility;

namespace Mcma.Api.Routes.Defaults
{
    public class DefaultRoutes
    {
        public static DefaultRouteCollectionBuilder<T> Builder<T>(IDbTableProvider<T> dbTableProvider, string root = null) where T : McmaResource
            => new DefaultRouteCollectionBuilder<T>(dbTableProvider, root ?? typeof(T).Name.CamelCaseToKebabCase());

        public static McmaApiRouteCollection ForJobAssignments(IDbTableProvider<JobAssignment> dbTableProvider, IWorkerInvoker workerInvoker)
        {
            var defaultRoutesBuilder = Builder<JobAssignment>(dbTableProvider);

            defaultRoutesBuilder.Create(rb =>
                rb.OnCompleted((requestContext, jobAssignment) =>
                    workerInvoker.RunAsync(requestContext.WorkerLambdaFunctionName(),
                        new
                        {
                            action = "ProcessJobAssignment",
                            stageVariables = requestContext.ContextVariables,
                            jobAssignmentId = jobAssignment.Id
                        })));

            return defaultRoutesBuilder.Build();
        }
    }
}
