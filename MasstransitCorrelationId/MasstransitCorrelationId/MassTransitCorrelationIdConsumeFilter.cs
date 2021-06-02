using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace MasstransitCorrelationId
{
    public class MassTransitCorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private const string CorrelationTokenHeader = "x-correlation-id";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MassTransitCorrelationIdConsumeFilter(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }
        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            if (!(!StringValues.IsNullOrEmpty(context.ConversationId.ToString())
                && Guid.TryParse(context.ConversationId.ToString(), out var correlationId)))
            {
                correlationId = Guid.NewGuid();
            }

            _httpContextAccessor.HttpContext = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Request.Headers.Add(CorrelationTokenHeader,
                correlationId.ToString());

            await next.Send(context);
        }
    }
}
