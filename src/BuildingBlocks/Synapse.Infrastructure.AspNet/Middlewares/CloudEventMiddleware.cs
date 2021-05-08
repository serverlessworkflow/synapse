using CloudNative.CloudEvents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Synapse.Infrastructure.Middlewares
{

    /// <summary>
    /// Represents a <see cref="IMiddleware"/> used to handle <see cref="CloudEvent"/>s
    /// </summary>
    public class CloudEventsMiddleware
    {

        /// <summary>
        /// Initializes a new <see cref="CloudEventsMiddleware"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="formatter">The service used to format <see cref="CloudEvent"/>s</param>
        /// <param name="stream">The consumed <see cref="CloudEvent"/> stream</param>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the pipeline</param>
        public CloudEventsMiddleware(ILogger<CloudEventsMiddleware> logger, ICloudEventFormatter formatter, Subject<CloudEvent> stream, RequestDelegate next)
        {
            this.Logger = logger;
            this.Formatter = formatter;
            this.Stream = stream;
            this.Next = next;
        }

        /// <summary>
        /// Initializes a new <see cref="CloudEventsMiddleware"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="stream">The consumed <see cref="CloudEvent"/> stream</param>
        /// <param name="next">The next <see cref="RequestDelegate"/> in the pipeline</param>
        public CloudEventsMiddleware(ILogger<CloudEventsMiddleware> logger, Subject<CloudEvent> stream, RequestDelegate next)
            : this(logger, new JsonEventFormatter(), stream, next)
        {

        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to format <see cref="CloudEvent"/>s
        /// </summary>
        protected ICloudEventFormatter Formatter { get; }

        /// <summary>
        /// Gets the consumed <see cref="CloudEvent"/> stream
        /// </summary>
        protected Subject<CloudEvent> Stream { get; }

        /// <summary>
        /// Gets the next <see cref="RequestDelegate"/> in the pipeline
        /// </summary>
        protected RequestDelegate Next { get; }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.Request.ContentType)
                && context.Request.ContentType.StartsWith(CloudEvent.MediaType, StringComparison.InvariantCultureIgnoreCase))
                await this.ProcessCloudEventAsync(context);
            else
                await this.Next(context);
        }

        /// <summary>
        /// Processes the incoming <see cref="CloudEvent"/>
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the incoming <see cref="CloudEvent"/> to process</param>
        /// <param name="eventTypeUri">The </param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        protected virtual async Task ProcessCloudEventAsync(HttpContext context)
        {
            try
            {
                CloudEvent cloudEvent = await context.Request.ReadCloudEventAsync();
                this.Stream.OnNext(cloudEvent);
                context.Response.StatusCode = (int)HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                this.Logger.LogError("An error occured while processing an incoming cloud event: {ex}", ex.ToString());
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                this.Stream.OnError(ex);
            }
            await context.Response.CompleteAsync();
        }

    }

}
