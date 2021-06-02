using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace MasstransitCorrelationId
{
    public class MassTransitCorrelationIdPublishFilter<T> : IFilter<PublishContext<T>> where T : class
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CorrelationTokenHeader = "x-correlation-id";

        public MassTransitCorrelationIdPublishFilter(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public void Probe(ProbeContext context) { }

        public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            if (!(!StringValues.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers[CorrelationTokenHeader])
                && Guid.TryParse(_httpContextAccessor.HttpContext.Request.Headers[CorrelationTokenHeader], out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
            }

            context.ConversationId = correlationId;
            context.Headers.Set(CorrelationTokenHeader, correlationId);

            await next.Send(context);
        }
    }
}
