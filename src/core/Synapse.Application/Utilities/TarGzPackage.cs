using System.IO.Compression;
using System.Text;

namespace Synapse
{

    /// <summary>
    /// Defines helpers for .tar.gz packages
    /// </summary>
    /// <remarks>Code taken from <see href="https://gist.github.com/ForeverZer0/a2cd292bd2f3b5e114956c00bb6e872b"/> under license ALTERNATIVE B - Public Domain (www.unlicense.org)</remarks>
    public static class TarGzPackage
    {

        /// <summary>
        /// Extracts the specified .tar.gz file to the specified output directory
        /// </summary>
        /// <param name="filePath">The path to the .tar.gz file to extract</param>
        /// <param name="outputDirectory">The directory to extract the specified file to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
        public static async Task ExtractToDirectoryAsync(string filePath, string outputDirectory, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if(string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentNullException(nameof(outputDirectory));
            if(!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var file = new FileInfo(filePath);
            if (!file.Exists)
                throw new FileNotFoundException($"Failed to find the specified .tar.gz file", filePath);
            using var stream = file.OpenRead();
            await ExtractToDirectoryAsync(stream, outputDirectory, cancellationToken);
        }

        /// <summary>
        /// Extracts the specified .tar.gz <see cref="Stream"/> to the specified output directory
        /// </summary>
        /// <param name="stream">The the .tar.gz <see cref="Stream"/> to extract</param>
        /// <param name="outputDirectory">The directory to extract the specified <see cref="Stream"/> to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
        /// <returns>A new awaitable <see cref="Task"/></returns>
		public static async Task ExtractToDirectoryAsync(Stream stream, string outputDirectory, CancellationToken cancellationToken = default)
        {
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new ArgumentNullException(nameof(outputDirectory));
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            const int chunk = 4096;
            using var memoryStream = new MemoryStream();
            int read;
            var buffer = new byte[chunk];
            while ((read = await gzip.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, read, cancellationToken);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            await ExtractTarToDirectoryAsync(memoryStream, outputDirectory, cancellationToken);
        }

        private static async Task ExtractTarToDirectoryAsync(Stream stream, string outputDirectory, CancellationToken cancellationToken)
        {
            var inBuffer = new byte[100];
            while (true)
            {
                await stream.ReadAsync(inBuffer, 0, 100, cancellationToken);
                var name = Encoding.ASCII.GetString(inBuffer).Trim('\0');
                if (string.IsNullOrWhiteSpace(name))
                    break;
                stream.Seek(24, SeekOrigin.Current);
                await stream.ReadAsync(inBuffer, 0, 12, cancellationToken);
                var size = Convert.ToInt64(Encoding.UTF8.GetString(inBuffer, 0, 12).Trim('\0').Trim(), 8);
                stream.Seek(376L, SeekOrigin.Current);
                var output = Path.Combine(outputDirectory, name);
                if (!Directory.Exists(Path.GetDirectoryName(output)))
                    Directory.CreateDirectory(Path.GetDirectoryName(output)!);
                if (!name.EndsWith("/", StringComparison.InvariantCulture))
                {
                    using (var outputFileStream = File.Open(output, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var outBuffer = new byte[size];
                        await stream.ReadAsync(outBuffer, 0, outBuffer.Length, cancellationToken);
                        await outputFileStream.WriteAsync(outBuffer, 0, outBuffer.Length, cancellationToken);
                    }
                }
                var pos = stream.Position;
                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;
                stream.Seek(offset, SeekOrigin.Current);
            }
        }

    }

}
