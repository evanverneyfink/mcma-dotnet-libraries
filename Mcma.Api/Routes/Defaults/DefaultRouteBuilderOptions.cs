using System.Collections.Generic;

namespace Mcma.Api.Routes.Defaults
{
    public class DefaultRouteBuilderOptions<TResource>
    {
        internal DefaultRouteBuilderOptions()
        {
        }

        public DefaultRouteBuilder<IEnumerable<TResource>> Query { get; internal set; }

        public DefaultRouteBuilder<TResource> Create { get; internal set; }

        public DefaultRouteBuilder<TResource> Get { get; internal set; }

        public DefaultRouteBuilder<TResource> Update { get; internal set; }

        public DefaultRouteBuilder<TResource> Delete { get; internal set; }
    }
}
