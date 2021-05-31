﻿using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace NeonModelo.Api.Middlewares
{
    public class MassTransitCorrelationIdPublishFilter<T> : IFilter<PublishContext<T>> where T : class
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private const string CORRELATION_TOKEN_HEADER = "x-correlation-id";

        public MassTransitCorrelationIdPublishFilter(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        public void Probe(ProbeContext context) { }

        public async Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
        {
            if (!(!StringValues.IsNullOrEmpty(httpContextAccessor.HttpContext.Request.Headers[CORRELATION_TOKEN_HEADER])
                && Guid.TryParse(httpContextAccessor.HttpContext.Request.Headers[CORRELATION_TOKEN_HEADER], out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
            }

            context.ConversationId = correlationId;
            context.Headers.Set(CORRELATION_TOKEN_HEADER, correlationId);

            await next.Send(context);
        }
    }
}