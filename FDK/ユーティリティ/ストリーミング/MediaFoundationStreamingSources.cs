using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CSCore;
using SharpDX;
using SharpDX.MediaFoundation;

namespace FDK
{
    public class MediaFoundationStreamingSources : IDisposable
    {
        /// <remarks>
        ///     Video ストリームがなければ null。
        /// </remarks>
        public IVideoSource VideoSource
            => this._VideoSource;

        /// <remarks>
        ///     Audio ストリームがなければ null。
        /// </remarks>
        public IWaveSource WaveSource
            => this._WaveSource;

        public bool ループ再生する { get; set; } = false;


        public static MediaFoundationStreamingSources CreateFromFile( DXGIDeviceManager dxgiDeviceManager, VariablePath ファイルパス, WaveFormat soundDeviceFormat )
        {
            var sources = new MediaFoundationStreamingSources();

            #region " ファイルから SourceReaderEx を生成する。"
            //----------------
            using( var ビデオ属性 = new MediaAttributes() )
            {
                // DXVAに対応しているGPUの場合には、それをデコードに利用するよう指定する。
                ビデオ属性.Set( SourceReaderAttributeKeys.D3DManager, dxgiDeviceManager );

                // 追加のビデオプロセッシングを有効にする。
                ビデオ属性.Set( SourceReaderAttributeKeys.EnableAdvancedVideoProcessing, true );  // 真偽値が bool だったり

                // 追加のビデオプロセッシングを有効にしたら、こちらは無効に。
                ビデオ属性.Set( SinkWriterAttributeKeys.ReadwriteDisableConverters, 0 );           // int だったり

                // 属性を使って、SourceReaderEx を生成。
                using( var sourceReader = new SourceReader( ファイルパス.変数なしパス, ビデオ属性 ) )    // パスは URI 扱い
                    sources._SourceReaderEx = sourceReader.QueryInterface<SourceReaderEx>();
            }
            //----------------
            #endregion

            #region " WaveFormat を生成。"
            //----------------
            sources._Audioのフォーマット = new WaveFormat(
                soundDeviceFormat.SampleRate,
                32,
                soundDeviceFormat.Channels,
                AudioEncoding.IeeeFloat );
            //----------------
            #endregion

            sources._SourceReaderEx生成後の初期化();

            return sources;
        }

        public static MediaFoundationStreamingSources CreateFromニコ動( string user_id, string password, string video_id, WaveFormat soundDeviceFormat )
        {
            var sources = new MediaFoundationStreamingSources();

            #region " ニコ動から SourceReaderEx を生成する。"
            //----------------
            if( null == _HttpClient )
                _HttpClient = new HttpClient();

            // ログインする。
            var content = new FormUrlEncodedContent( new Dictionary<string, string> {
                    { "mail", user_id },
                    { "password", password },
                    { "next_url", string.Empty },
                } );
            using( var responseLogin = _HttpClient.PostAsync( "https://secure.nicovideo.jp/secure/login?site=niconico", content ).Result )
            {
            }

            // 動画ページにアクセスする。（getflvより前に）
            var responseWatch = _HttpClient.GetStringAsync( $"http://www.nicovideo.jp/watch/{video_id}" ).Result;

            // 動画情報を取得する。
            var responseGetFlv = _HttpClient.GetStringAsync( $"http://flapi.nicovideo.jp/api/getflv/{video_id}" ).Result;
            var flvmap = HttpUtility.ParseQueryString( responseGetFlv );
            var flvurl = flvmap[ "url" ];

            // 動画の長さを取得する。
            ulong 長さbyte = 0;
            string contentType = "";
            using( var requestMovie = new HttpRequestMessage( HttpMethod.Get, flvurl ) )
            using( var responseMovie = _HttpClient.SendAsync( requestMovie, HttpCompletionOption.ResponseHeadersRead ).Result )
            {
                長さbyte = (ulong) ( responseMovie.Content.Headers.ContentLength );
                contentType = responseMovie.Content.Headers.ContentType.MediaType;
            }

            // IMFByteStream を生成する。
            sources._ByteStream = new ByteStream( IntPtr.Zero );
            sources._HttpRandomAccessStream = new HttpRandomAccessStream( _HttpClient, 長さbyte, flvurl );
            sources._unkHttpRandomAccessStream = new ComObject( Marshal.GetIUnknownForObject( sources._HttpRandomAccessStream ) );
            MediaFactory.CreateMFByteStreamOnStreamEx( sources._unkHttpRandomAccessStream, sources._ByteStream );
            using( var 属性 = sources._ByteStream.QueryInterfaceOrNull<MediaAttributes>() )
            {
                // content-type を設定する。
                属性.Set( ByteStreamAttributeKeys.ContentType, contentType );
            }

            // SourceResolver で IMFByteStream から MediaSouce を取得する。
            using( var sourceResolver = new SourceResolver() )
            using( var unkMediaSource = sourceResolver.CreateObjectFromStream( sources._ByteStream, null, SourceResolverFlags.MediaSource ) )
            {
                sources._MediaSource = unkMediaSource.QueryInterface<MediaSource>();

                // MediaSource から SourceReaderEx を生成する。
                using( var 属性 = new MediaAttributes() )
                {
                    // DXVAに対応しているGPUの場合には、それをデコードに利用するよう指定する。
                    属性.Set( SourceReaderAttributeKeys.D3DManager, DXResources.Instance.MFDXGIDeviceManager );

                    // 追加のビデオプロセッシングを有効にする。
                    属性.Set( SourceReaderAttributeKeys.EnableAdvancedVideoProcessing, true );  // 真偽値が bool だったり

                    // 追加のビデオプロセッシングを有効にしたら、こちらは無効に。
                    属性.Set( SinkWriterAttributeKeys.ReadwriteDisableConverters, 0 );           // int だったり

                    // 属性を使って、SourceReaderEx を生成。
                    using( var sourceReader = new SourceReader( sources._MediaSource, 属性 ) )
                    {
                        sources._SourceReaderEx = sourceReader.QueryInterfaceOrNull<SourceReaderEx>();
                    }
                }
            }
            //----------------
            #endregion

            #region " WaveFormat を生成。"
            //----------------
            sources._Audioのフォーマット = new WaveFormat(
                soundDeviceFormat.SampleRate,
                32,
                soundDeviceFormat.Channels,
                AudioEncoding.IeeeFloat );
            //----------------
            #endregion

            sources._SourceReaderEx生成後の初期化();

            return sources;
        }

        private void _SourceReaderEx生成後の初期化()
        {
            Debug.Assert( null != this._SourceReaderEx );

            #region " ストリームを列挙して、Video と Audio の実ストリーム番号を調べる。"
            //----------------
            try
            {
                for( int index = 0; true; index++ ) // 例外が発生するまでループ
                {
                    using( var mediaType = this._SourceReaderEx.GetCurrentMediaType( index ) )
                    {
                        if( mediaType.MajorType == MediaTypeGuids.Video )
                        {
                            this._Videoのストリーム番号 = index;
                        }
                        else if( mediaType.MajorType == MediaTypeGuids.Audio )
                        {
                            this._Audioのストリーム番号 = index;
                        }
                    }
                }
            }
            catch( SharpDXException e )
            {
                if( e.ResultCode == SharpDX.MediaFoundation.ResultCode.InvalidStreamNumber )
                {
                    // 列挙完了
                }
                else
                {
                    throw;
                }
            }
            //----------------
            #endregion

            this._SourceReaderEx.SetStreamSelection( SourceReaderIndex.AllStreams, false );

            this._長さsec = FDKUtilities.変換_100ns単位からsec単位へ(
                this._SourceReaderEx.GetPresentationAttribute( SourceReaderIndex.MediaSource, PresentationDescriptionAttributeKeys.Duration ) );

            if( 0 <= this._Videoのストリーム番号 )
            {
                #region " Video の初期化 "
                //----------------
                this._SourceReaderEx.SetStreamSelection( this._Videoのストリーム番号, true );

                // 部分メディアタイプを設定する。
                using( var videoMediaType = new MediaType() )
                {
                    // フォーマットは ARGB32 で固定とする。（SourceReaderEx を使わない場合、H264 では ARGB32 が選べないので注意。）
                    videoMediaType.Set( MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video );
                    videoMediaType.Set( MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Argb32 );

                    // 部分メディアタイプを SourceReaderEx にセットする。SourceReaderEx は、必要なデコーダをロードするだろう。
                    this._SourceReaderEx.SetCurrentMediaType( this._Videoのストリーム番号, videoMediaType );
                }

                // 完成されたメディアタイプを取得する。
                this._VideoのMediaType = this._SourceReaderEx.GetCurrentMediaType( this._Videoのストリーム番号 );

                // ビデオのフレームサイズを取得する。
                long packedFrameSize = this._VideoのMediaType.Get( MediaTypeAttributeKeys.FrameSize ); // 動画の途中でのサイズ変更には対応しない。
                this._Videoのフレームサイズ = new Size2F( ( packedFrameSize >> 32 ) & 0xFFFFFFFF, ( packedFrameSize ) & 0xFFFFFFFF );
                //----------------
                #endregion

                this._VideoSource = new StreamingVideoSource( this._Videoのフレームサイズ );
            }
            if( 0 <= this._Audioのストリーム番号 )
            {
                #region " Audio の初期化 "
                //----------------
                this._SourceReaderEx.SetStreamSelection( this._Audioのストリーム番号, true );

                // 部分メディアタイプを設定する。
                var wf = SharpDX.Multimedia.WaveFormat.CreateIeeeFloatWaveFormat( this._Audioのフォーマット.SampleRate, this._Audioのフォーマット.Channels );
                MediaFactory.CreateAudioMediaType( ref wf, out AudioMediaType audioMediaType );

                // 作成した部分メディアタイプを SourceReader にセットする。必要なデコーダが見つからなかったら、ここで例外が発生する。
                using( audioMediaType )
                    this._SourceReaderEx.SetCurrentMediaType( this._Audioのストリーム番号, audioMediaType );

                // 完成されたメディアタイプを取得する。
                this._AudioのMediaType = this._SourceReaderEx.GetCurrentMediaType( this._Audioのストリーム番号 );

                // フォーマットを取得する（念のため）。
                var _CscoreAudioMediaType = new CSCore.MediaFoundation.MFMediaType( this._AudioのMediaType.NativePointer );  // ネイティブポインタの共有で生成したのでDispose不要。
                this._Audioのフォーマット = _CscoreAudioMediaType.ToWaveFormat( CSCore.MediaFoundation.MFWaveFormatExConvertFlags.Normal );
                _CscoreAudioMediaType = null;
                //----------------
                #endregion

                this._WaveSource = new StreamingWaveSource( this._Audioのフォーマット );
            }

            #region " デコード開始。"
            //----------------
            this._デコードキャンセル = new CancellationTokenSource();
            this._デコード起動完了通知 = new ManualResetEvent( false );
            this._デコードタスク = Task.Factory.StartNew( this._デコードタスクエントリ, this._デコードキャンセル.Token );
            this._デコード起動完了通知.WaitOne();
            //----------------
            #endregion

            //Thread.Sleep( 500 );    // デコードデータが蓄積されるまで待機（手抜き
        }

        // インスタンスの直接生成禁止
        protected MediaFoundationStreamingSources()
        {
        }

        public void Dispose()
        {
            #region " デコードタスクが稼働してたら停止する。"
            //----------------
            if( ( null != this._デコードタスク ) && !( this._デコードタスク.IsCompleted ) )
            {
                this._デコードキャンセル.Cancel();
                this._VideoSource?.Cancel();
                this._WaveSource?.Cancel();

                this._デコードタスク.Wait( 5000 ); // 最大 5000ms 待つ
                this._デコードタスク = null;
            }
            //----------------
            #endregion

            this._デコードキャンセル?.Dispose();
            this._デコードキャンセル = null;

            this._WaveSource?.Dispose();
            this._WaveSource = null;

            this._VideoSource?.Dispose();
            this._VideoSource = null;

            this._AudioのMediaType?.Dispose();
            this._AudioのMediaType = null;

            this._VideoのMediaType?.Dispose();
            this._VideoのMediaType = null;

            this._SourceReaderEx?.Dispose();
            this._SourceReaderEx = null;

            this._MediaSource?.Dispose();
            this._MediaSource = null;

            this._unkHttpRandomAccessStream?.Dispose();
            this._unkHttpRandomAccessStream = null;

            this._HttpRandomAccessStream?.Dispose();
            this._HttpRandomAccessStream = null;

            this._ByteStream?.Dispose();
            this._ByteStream = null;

            //_HttpClient?.Dispose();   --> アプリ全体でひとつのインスタンスを使いまわすので、解放しない。
            //_HttpClient = null;
        }

        /// <summary>
        ///     ビデオのデコードをキャンセルし、オーディオのデコードのみ続ける。
        /// </summary>
		public void ビデオをキャンセルする()
		{
			this._オーディオのみ.Set();
			this._VideoSource.Cancel(); // ビデオのみキャンセル
		}


        private SourceReaderEx _SourceReaderEx = null;
        private double _長さsec = 0.0;
        private int _Videoのストリーム番号 = -1;
        private int _Audioのストリーム番号 = -1;
        private MediaType _VideoのMediaType = null;
        private Size2F _Videoのフレームサイズ = Size2F.Zero;
        private WaveFormat _Audioのフォーマット = new WaveFormat();
        private MediaType _AudioのMediaType = null;

        private StreamingVideoSource _VideoSource = null;
        private StreamingWaveSource _WaveSource = null;

        // HTTPストリーム関連

        private static HttpClient _HttpClient = null;       // static にして使いまわす
        private ByteStream _ByteStream = null;
        private HttpRandomAccessStream _HttpRandomAccessStream = null;
        private ComObject _unkHttpRandomAccessStream = null;
        private MediaSource _MediaSource = null;


        // デコード関連

        private Task _デコードタスク = null;
        private CancellationTokenSource _デコードキャンセル = null;
        private ManualResetEvent _デコード起動完了通知 = null;
        /// <summary>
        ///     これが Set されていると、Video のデコードを行わず、Sound のデコードのみ行う。
        /// </summary>
        private ManualResetEventSlim _オーディオのみ = new ManualResetEventSlim( false );

        private void _デコードタスクエントリ()
        {
            Log.現在のスレッドに名前をつける( "ストリーミングデコード" );

            this._デコード起動完了通知.Set();

            while( true )
            {
                if( this._デコードキャンセル.Token.IsCancellationRequested )
                {
                    Log.Info( $"キャンセル通知を受信しました。" );
                    break;  // 終了
                }

                // (1) 次のサンプルをひとつデコードする。

                var サンプル = this._SourceReaderEx.ReadSample(
                    SourceReaderIndex.AnyStream,
                    SourceReaderControlFlags.None,
                    out int ストリーム番号,
                    out var ストリームフラグ,
                    out long サンプルの表示時刻100ns );

                if( ストリームフラグ.HasFlag( SourceReaderFlags.Endofstream ) )
                {
                    Log.Info( $"ストリームが終了しました。" );
                    if( this.ループ再生する )
                    {
                        Log.Info( $"ループ再生します。" );
                        this._SourceReaderEx.SetCurrentPosition( 0 );
                        continue;
                    }
                    else
                    {
                        //this._VideoSource?.Cancel();	--> ストリームが終了しても再生はまだ終了してないのでキャンセルしてはならない
                        //this._WaveSource?.Cancel();
                        break;
                    }
                }
                if( ストリームフラグ.HasFlag( SourceReaderFlags.Error ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.AllEffectsremoved ) ||
                    //ストリームフラグ.HasFlag( SourceReaderFlags.Currentmediatypechanged ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.Nativemediatypechanged ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.Newstream ) ||
                    ストリームフラグ.HasFlag( SourceReaderFlags.StreamTick ) )
                {
                    Log.ERROR( $"デコード中にエラーが発生または未対応の状態変化が発生しました。" );
                    break;
                }


                // (2) サンプルをそれぞれのストリームに書き込む。

				if( ストリーム番号 == this._Videoのストリーム番号 )
				{
					if( this._オーディオのみ.IsSet )
					{
						サンプル.Dispose();
					}
					else
					{
                        // ビデオフレームを生成してキューに格納。キューがいっぱいならブロックする。

                        this._VideoSource?.Write( new VideoFrame() {
							Sample = サンプル,
							Bitmap = this._VideoSource.サンプルからビットマップを取得する( サンプル ),
							表示時刻100ns = サンプルの表示時刻100ns,
						} );
					}
				}
				else if( ストリーム番号 == this._Audioのストリーム番号 )
				{
                    // サンプルをロックし、オーディオデータへのポインタを取得して、オーディオデータをキューに書き込む。

                    using( var mediaBuffer = サンプル.ConvertToContiguousBuffer() )
					{
						var audioData = mediaBuffer.Lock( out _, out int cbCurrentLength );
						try
						{
							var dstData = new byte[ cbCurrentLength ];
							Marshal.Copy( audioData, dstData, 0, cbCurrentLength );

                            this._WaveSource.Write( dstData, 0, cbCurrentLength );  // キューはいっぱいにならない前提なので常にブロックしない。
                        }
						finally
						{
							mediaBuffer.Unlock();
						}
					}
				}
            }

            Log.Info( $"タスクを終了します。" );
        }
    }
}
