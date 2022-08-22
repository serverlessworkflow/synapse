using Microsoft.Extensions.Hosting;
using System.Dynamic;
using System.Runtime.InteropServices;

namespace Synapse.Worker.Services
{

    /// <summary>
    /// Represents a file-based implementation of the <see cref="ISecretManager"/> interface
    /// </summary>
    public class FileBasedSecretManager
        : BackgroundService, ISecretManager
    {

        /// <summary>
        /// Gets the default directory for file-based secrets
        /// </summary>
        public static string DefaultSecretDirectory
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "secrets");
                else
                    return "/run/secrets/synapse";
            }
        }

        /// <summary>
        /// Initializes a new <see cref="FileBasedSecretManager"/>
        /// </summary>
        /// <param name="logger">The service used to perform logging</param>
        /// <param name="serializerProvider">The service used to provide <see cref="ISerializer"/>s</param>
        public FileBasedSecretManager(ILogger<FileBasedSecretManager> logger, ISerializerProvider serializerProvider)
        {
            this.Logger = logger;
            this.SerializerProvider = serializerProvider;
        }

        /// <summary>
        /// Gets the service used to perform logging
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the service used to provide <see cref="ISerializer"/>s
        /// </summary>
        protected ISerializerProvider SerializerProvider { get; }

        /// <summary>
        /// Gets an <see cref="ExpandoObject"/> containing the key/value mappings of all loaded secrets
        /// </summary>
        protected Dictionary<string, object> Secrets { get; } = new();

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var directory = new DirectoryInfo(DefaultSecretDirectory); //todo: replace with options-based values
            if (!directory.Exists)
                directory.Create();
            foreach (var file in directory.GetFiles())
            {
                using var stream = file.OpenRead();
                var mediaTypeName = MimeTypes.GetMimeType(file.Name);
                var serializer = this.SerializerProvider.GetSerializerFor(mediaTypeName);
                if (serializer == null)
                {
                    this.Logger.LogWarning("Skipped loading secret '{secretFile}': failed to find a serializer for the specified media type '{mediaType}'", file.Name, mediaTypeName);
                    continue;
                }
                try
                {
                    var secret = await serializer.DeserializeAsync<Neuroglia.Serialization.DynamicObject>(stream, stoppingToken);
                    this.Secrets.Add(file.Name, secret);
                }
                catch(Exception ex)
                {
                    this.Logger.LogWarning("Skipped loading secret '{secretFile}': an exception occured while deserializing the secret object: {ex}", file.Name, ex.ToString());
                    continue;
                }
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IDictionary<string, object>> GetSecretsAsync(CancellationToken cancellationToken)
        {
            return await Task.FromResult(this.Secrets);
        }

    }

}
