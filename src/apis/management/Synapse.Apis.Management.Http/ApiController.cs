namespace Synapse.Apis.Management.Http
{

    /// <summary>
    /// Represents the base class for all the application's <see cref="ControllerBase"/> implementations
    /// </summary>
    public abstract class ApiController
        : ControllerBase
    {

        /// <summary>
        /// Initializes a new <see cref="ApiController"/>
        /// </summary>
        /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
        /// <param name="mediator">The service used to mediate calls</param>
        /// <param name="mapper">The service used to map objects</param>
        protected ApiController(ILoggerFactory loggerFactory, IMediator mediator, IMapper mapper)
        {
            this.Logger = loggerFactory.CreateLogger(this.GetType());
            this.Mediator = mediator;
            this.Mapper = mapper;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to mediate calls
        /// </summary>
        protected IMediator Mediator { get; }

        /// <summary>
        /// Gets the service used to map objects
        /// </summary>
        protected IMapper Mapper { get; }

    }

}
