using Mcma.Api.Routes.Defaults;
using Mcma.Aws.DynamoDb;
using Mcma.Core;

namespace Mcma.Aws.Api
{
    public static class AwsDefaultRoutes
    {
        public static DefaultRouteCollectionBuilder<T> WithDynamoDb<T>() where T : McmaResource
            => DefaultRoutes.Builder<T>(new DynamoDbTableProvider<T>());
    }
}