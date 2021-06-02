using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace MasstransitCorrelationId
{
    public class MassTransitCorrelationIdSendFilter<T> : IFilter<SendContext<T>> where T : class
    {
        private const string CorrelationTokenHeader = "x-correlation-id";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MassTransitCorrelationIdSendFilter(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public void Probe(ProbeContext context) { }

        public async Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
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
