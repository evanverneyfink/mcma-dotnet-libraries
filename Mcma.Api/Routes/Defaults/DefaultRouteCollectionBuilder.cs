using System;
using System.Linq;
using Mcma.Core.Serialization;
using Mcma.Core;
using System.Collections.Generic;
using System.Net.Http;

namespace Mcma.Api.Routes.Defaults
{
    public class DefaultRouteCollectionBuilder<TResource> where TResource : McmaResource
    {
        internal DefaultRouteCollectionBuilder(IDbTableProvider<TResource> dbTableProvider, string root)
        {
            if (!root.StartsWith("/"))
                root = "/" + root;

            Routes = new DefaultRouteBuilderOptions<TResource>
            {
                Query = QueryRouteBuilder(dbTableProvider, root),
                Create = CreateRouteBuilder(dbTableProvider, root),
                Get = GetRouteBuilder(dbTableProvider, root),
                Update = UpdateRouteBuilder(dbTableProvider, root),
                Delete = DeleteRouteBuilder(dbTableProvider, root)
            };
        }

        private DefaultRouteBuilderOptions<TResource> Routes { get; }

        public McmaApiRouteCollection Build()
            => new McmaApiRouteCollection(Routes.Query.Build(), Routes.Create.Build(), Routes.Get.Build(), Routes.Update.Build(), Routes.Delete.Build());

        public DefaultRouteCollectionBuilder<TResource> Query(Action<DefaultRouteBuilder<IEnumerable<TResource>>> configure)
            => ConfigureRoute(Routes.Query, configure);

        public DefaultRouteCollectionBuilder<TResource> Create(Action<DefaultRouteBuilder<TResource>> configure)
            => ConfigureRoute(Routes.Create, configure);

        public DefaultRouteCollectionBuilder<TResource> Get(Action<DefaultRouteBuilder<TResource>> configure)
            => ConfigureRoute(Routes.Get, configure);

        public DefaultRouteCollectionBuilder<TResource> Update(Action<DefaultRouteBuilder<TResource>> configure)
            => ConfigureRoute(Routes.Update, configure);

        public DefaultRouteCollectionBuilder<TResource> Delete(Action<DefaultRouteBuilder<TResource>> configure)
            => ConfigureRoute(Routes.Delete, configure);

        private DefaultRouteCollectionBuilder<TResource> ConfigureRoute<TResult>(
            DefaultRouteBuilder<TResult> routeBuilder,
            Action<DefaultRouteBuilder<TResult>> configure)
        {
            configure(routeBuilder);
            return this;
        }

        private static DefaultRouteBuilder<IEnumerable<TResource>> QueryRouteBuilder(IDbTableProvider<TResource> dbTableProvider, string root) => 
            new DefaultRouteBuilder<IEnumerable<TResource>>(
                HttpMethod.Get,
                root, 
                new DefaultRouteHandlerBuilder<IEnumerable<TResource>>(
                    (onStarted, onCompleted) =>
                        async requestContext =>
                        {
                            // invoke the start handler, if any
                            if (onStarted != null)
                                await onStarted.Invoke(requestContext);

                            // get all resources from the table, applying in-memory filtering using the query string (if any)
                            var resources =
                                await dbTableProvider
                                    .Table(requestContext.TableName())
                                    .QueryAsync(
                                        requestContext.Request.QueryStringParameters.Any()
                                            ? Filters.InMemoryTextValues<TResource>(requestContext.Request.QueryStringParameters)
                                            : null);

                            // invoke the completion handler with the results
                            if (onCompleted != null)
                                await onCompleted(requestContext, resources);

                            // return the results as JSON in the body of the response
                            // NOTE: This will never return a 404 - just an empty collection
                            requestContext.ResourceIfFound(resources);
                        }));

        private static DefaultRouteBuilder<TResource> CreateRouteBuilder(IDbTableProvider<TResource> dbTableProvider, string root) => 
            new DefaultRouteBuilder<TResource>(
                HttpMethod.Post,
                root,
                new DefaultRouteHandlerBuilder<TResource>(
                    (onStarted, onCompleted) =>
                        async requestContext =>
                        {
                        // invoke the start handler, if any
                        if (onStarted != null)
                            await onStarted.Invoke(requestContext);

                        // ensure the body is set
                        if (requestContext.IsBadRequestDueToMissingBody(out TResource resource))
                            return;

                        // initialize the new resource with an ID
                        resource.OnCreate($"{requestContext.PublicUrl()}/{root}/{Guid.NewGuid()}");

                        // put the new object into the table
                        await dbTableProvider.Table(requestContext.TableName()).PutAsync(resource.Id, resource);

                        // invoke the completion handler (if any) with the newly-created resource
                        onCompleted?.Invoke(requestContext, resource);

                        // return a Created status with the id of the resource
                        requestContext.ResourceCreated(resource);
                    }));

        private static DefaultRouteBuilder<TResource> GetRouteBuilder(IDbTableProvider<TResource> dbTableProvider, string root) =>
            new DefaultRouteBuilder<TResource>(
                HttpMethod.Get,
                root,
                new DefaultRouteHandlerBuilder<TResource>(
                    (onStarted, onCompleted) =>
                        async requestContext =>
                        {
                            // invoke the start handler, if any
                            onStarted?.Invoke(requestContext);

                            // get the resource from the database
                            var resource =
                                await dbTableProvider.Table(requestContext.TableName()).GetAsync(requestContext.PublicUrl() + requestContext.Request.Path);

                            // invoke the completion handler, if any
                            onCompleted?.Invoke(requestContext, resource);

                            // return the resource as json, if found; otherwise, this will return a 404
                            requestContext.ResourceIfFound(resource);
                        }));

        private static DefaultRouteBuilder<TResource> UpdateRouteBuilder(IDbTableProvider<TResource> dbTableProvider, string root) =>
            new DefaultRouteBuilder<TResource>(
                HttpMethod.Put,
                root + "/{id}",
                new DefaultRouteHandlerBuilder<TResource>(
                    (onStarted, onCompleted) =>
                        async requestContext =>
                        {
                            // invoke the start handler, if any
                            onStarted?.Invoke(requestContext);

                            // ensure the body is set
                            if (requestContext.IsBadRequestDueToMissingBody(out TResource resource))
                                return;
                            
                            // set properties for upsert
                            resource.OnUpsert(requestContext.PublicUrl() + requestContext.Request.Path);

                            // upsert the resource
                            await dbTableProvider.Table(requestContext.TableName()).PutAsync(resource.Id, resource);

                            // invoke the completion handler, if any
                            onCompleted?.Invoke(requestContext, resource);

                            // return the new or updated resource as json
                            requestContext.Response.JsonBody = resource.ToMcmaJson();
                        }));

        private static DefaultRouteBuilder<TResource> DeleteRouteBuilder(IDbTableProvider<TResource> dbTableProvider, string root) =>
            new DefaultRouteBuilder<TResource>(
                HttpMethod.Delete,
                root + "/{id}",
                new DefaultRouteHandlerBuilder<TResource>(
                    (onStarted, onCompleted) =>
                        async requestContext =>
                        {
                            // invoke the start handler, if any
                            onStarted?.Invoke(requestContext);

                            // get the table for the resource
                            var table = dbTableProvider.Table(requestContext.TableName());

                            // build id from the root public url and the path
                            var id = requestContext.PublicUrl() + requestContext.Request.Path;

                            // get the resource from the db
                            var resource = await table.GetAsync(id);

                            // if the resource doesn't exist, return a 404
                            if (!requestContext.ResourceIfFound(resource, false))
                                return;

                            // delete the resource from the db
                            await table.DeleteAsync(id);

                            // invoke the completion handler, if any
                            onCompleted?.Invoke(requestContext, resource);
                        }));
    }
}
