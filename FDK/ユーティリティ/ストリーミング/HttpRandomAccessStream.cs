using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace FDK
{
    /// <summary>
    ///     実際には読み取りしかできない <see cref="Windows.Storage.Streams.IRandomAccessStream"/> 。
    /// </summary>
    /// <remarks>
    ///     出展：http://garicchi.com/?p=18860
    ///     Windowsランタイムを using するので、
    ///     (1) .csproj にターゲットプラットフォームのバージョンを追加し、（例: "<TargetPlatformVersion>10.0</TargetPlatformVersion>"）
    ///     (2) 参照に  Windows.winmd を追加すること。（例: "C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.15063.0\Windows.winmd"）
    /// </remarks>
    class HttpRandomAccessStream : IRandomAccessStream
    {
        public bool CanRead => true;    // Read 可
        public bool CanWrite => false;  // Write 不可
        public ulong Position
            => _requestedPosition;
        public ulong Size
        {
            get
                => _size;
            set
                => throw new NotSupportedException();
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync( IBuffer buffer )
            => throw new NotSupportedException();
        public IAsyncOperation<bool> FlushAsync()
            => throw new NotSupportedException();
        public IInputStream GetInputStreamAt( ulong position )
            => throw new NotSupportedException();
        public IOutputStream GetOutputStreamAt( ulong position )
            => throw new NotSupportedException();
        public IRandomAccessStream CloneStream()
            => throw new NotSupportedException();

        public HttpRandomAccessStream( HttpClient client, ulong streamSize, string requestedUrl )
        {
            this._client = client ?? throw new ArgumentNullException();
            this._size = streamSize;
            this._requestedUri = new Uri( requestedUrl );
        }
        private async Task<Stream> GetStreamWithRange( Uri uri, ulong start, ulong? end )
        {
            var testHeader1 = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.3; WOW64; Trident/7.0; .NET4.0E; .NET4.0C; .NET CLR 3.5.30729; .NET CLR 2.0.50727; .NET CLR 3.0.30729; InfoPath.3)";

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Get, uri );

                request.Headers.Range = new RangeHeaderValue( (long?) start, (long?) end );
                request.Headers.UserAgent.ParseAdd( testHeader1 );

                var response = await _client.SendAsync( request );

                return await response.Content.ReadAsStreamAsync();
            }
            catch( WebException ex )
            {
                var message = ex.Message;
                throw;
            }
            catch( InvalidOperationException ex )
            {
                var message = ex.Message;
                throw;
            }
            catch( ArgumentNullException ex )
            {
                var message = ex.Message;
                throw;
            }
        }
        public void Dispose()
        {
            this._stream?.Dispose();
            this._stream = null;
        }
        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync( IBuffer buffer, uint count, InputStreamOptions options )
        {
            return AsyncInfo.Run<IBuffer, uint>( async ( cancellationToken, progress ) => {
                progress.Report( 0 );
                var netStream = await GetStreamWithRange( _requestedUri, _requestedPosition, _requestedPosition + count );
                _stream = netStream.AsInputStream();
                return await _stream.ReadAsync( buffer, count, options ).AsTask( cancellationToken, progress );
            } );
        }
        public void Seek( ulong position )
        {
            _stream?.Dispose();
            _requestedPosition = position;
            _stream = null;
        }

        private ulong _size;
        private IInputStream _stream;
        private ulong _requestedPosition;
        private Uri _requestedUri = null;
        private readonly HttpClient _client; // 解放不要
    }
}
