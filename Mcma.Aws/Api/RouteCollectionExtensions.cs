using Mcma.Api.Routes;

namespace Mcma.Aws.Api
{
    public static class RouteCollectionExtensions
    {
        public static ApiGatewayApiController ToApiGatewayController(this McmaApiRouteCollection routes)
            => new ApiGatewayApiController(routes);
    }
}