using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Synapse.Dashboard
{
    /// <summary>
    /// Wraps a <see cref="IBrowserFile"/> as <see cref="IFormFile"/>
    /// </summary>
    public class UploadableFile
        : IFormFile, IDisposable
    {
        ///<inheritdoc/>
        public string ContentType { get => this.Content.Headers.ContentType?.ToString() ?? ""; }

        ///<inheritdoc/>
        public string ContentDisposition { get => this.Content.Headers.ContentDisposition?.ToString() ?? ""; }

        ///<inheritdoc/>
        public IHeaderDictionary Headers { get; set; }

        ///<inheritdoc/>
        public long Length { get => this.BrowserFile.Size; }

        ///<inheritdoc/>
        public string Name { get => this.BrowserFile.Name; }

        ///<inheritdoc/>
        public string FileName { get => this.BrowserFile.Name; }

        ///<inheritdoc/>
        public Stream OpenReadStream()
        {
            return this.Content.ReadAsStream();
        }

        ///<inheritdoc/>
        public void CopyTo(Stream target)
        {
            this.Content.CopyTo(target, null, new CancellationToken());
        }

        ///<inheritdoc/>
        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.Content.CopyToAsync(target, cancellationToken);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            this.Content.Dispose();
        }

        /// <summary>
        /// The base <see cref="IBrowserFile"/>
        /// </summary>
        private IBrowserFile BrowserFile { get; set; }

        /// <summary>
        /// The <see cref="IBrowserFile"/> content
        /// </summary>
        private StreamContent Content { get; set; }

        /// <summary>
        /// Creates a new <see cref="UploadableFile"/> with the specified <see cref="IBrowserFile"/>
        /// </summary>
        /// <param name="browserFile"></param>
        public UploadableFile(IBrowserFile browserFile)
        {
            this.BrowserFile = browserFile;
            this.Content = new StreamContent(this.BrowserFile.OpenReadStream());
            this.Content.Headers.ContentType = new MediaTypeHeaderValue(this.BrowserFile.ContentType);
            this.Headers = null!;
        }
    }
}
