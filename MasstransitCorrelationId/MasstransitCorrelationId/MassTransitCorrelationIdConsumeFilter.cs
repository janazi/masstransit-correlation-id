using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace NeonModelo.Api.Middlewares
{
    public class MassTransitCorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private const string CORRELATION_TOKEN_HEADER = "x-correlation-id";
        private readonly IHttpContextAccessor httpContextAccessor;

        public MassTransitCorrelationIdConsumeFilter(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public void Probe(ProbeContext context) { }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            if (!(!StringValues.IsNullOrEmpty(context.ConversationId.ToString())
                && Guid.TryParse(context.ConversationId.ToString(), out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
            }

            httpContextAccessor.HttpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Request.Headers.Add(CORRELATION_TOKEN_HEADER,
                correlationId.ToString());

            await next.Send(context);
        }
    }
}
