using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using SharpDX;

namespace FDK32
{
    public class グラフィックデバイス
    {
        // singleton

        public static グラフィックデバイス Instance { get; protected set; } = null;

        public static void インスタンスを生成する( IntPtr hWindow, Size2F 設計画面サイズ, Size2F 物理画面サイズ, bool 深度ステンシルを使う = true )
        {
            if( null != Instance )
                throw new Exception( "インスタンスはすでに生成済みです。" );

            Instance = new グラフィックデバイス( hWindow, 設計画面サイズ, 物理画面サイズ, 深度ステンシルを使う );
            Instance.Initialize();
        }

        public static void インスタンスを解放する()
        {
            Instance?.Dispose();
            Instance = null;
        }



        protected グラフィックデバイス( IntPtr hWindow, Size2F 設計画面サイズ, Size2F 物理画面サイズ, bool 深度ステンシルを使う )
        {
            this._hWindow = hWindow;
            this._深度ステンシルを使う = 深度ステンシルを使う;
            this.設計画面サイズ = 設計画面サイズ;
            this.物理画面サイズ = 物理画面サイズ;
        }

        protected void Initialize()
        {
            this._Draw中のレンダーターゲットリスト = new List<SharpDX.Direct2D1.RenderTarget>();

            SharpDX.MediaFoundation.MediaManager.Startup();

            this._スワップチェーンに依存しないグラフィックリソースを作成する();
            this._スワップチェーンを作成する();
            this._スワップチェーンに依存するグラフィックリソースを作成する();
        }

        protected void Dispose()
        {
            if( 0 < this._Draw中のレンダーターゲットリスト.Count )
                throw new Exception( "まだ Draw 中のレンダーターゲットがあります。" );

            this._スワップチェーンに依存するグラフィックリソースを解放する();
            this._スワップチェーンを解放する();
            this._スワップチェーンに依存しないグラフィックリソースを解放する();

            SharpDX.MediaFoundation.MediaManager.Shutdown();
        }

        #region " グラフィックリソースプロパティ(1) スワップチェーンに依存しないもの "
        //----------------
        public SharpDX.DXGI.SwapChain1 SwapChain { get; private set; } = null;

        public SharpDX.DirectWrite.Factory DWriteFactory { get; private set; } = null;

        public SharpDX.Direct2D1.Factory2 D2DFactory { get; private set; } = null;

        public SharpDX.Direct2D1.Device1 D2DDevice { get; private set; } = null;

        public SharpDX.Direct2D1.DeviceContext1 D2DDeviceContext { get; private set; } = null;

        public SharpDX.DirectComposition.DesktopDevice DCompDevice { get; private set; } = null;

        public SharpDX.DirectComposition.Target DCompTarget { get; private set; } = null;

        public SharpDX.DirectComposition.Visual2 DCompVisualForSwapChain { get; private set; } = null;

        public SharpDX.WIC.ImagingFactory2 WicImagingFactory { get; private set; } = null;

        public SharpDX.MediaFoundation.DXGIDeviceManager DXGIDeviceManager { get; private set; } = null;

        public SharpDX.Direct3D11.DeviceDebug D3DDeviceDebug { get; private set; } = null;

        public FDK32.WindowsAnimation Animation { get; private set; } = null;

        public SharpDX.Direct3D11.Device D3DDevice { get; protected set; }

        public SharpDX.DXGI.Output DXGIOutput { get; protected set; }
        //----------------
        #endregion

        #region " グラフィックリソースプロパティ(2) スワップチェーンに依存するもの "
        //----------------
        public SharpDX.Direct2D1.Bitmap1 D2DRenderBitmap { get; private set; } = null;

        public SharpDX.Direct3D11.RenderTargetView D3DRenderTargetView { get; private set; } = null;

        public SharpDX.Direct3D11.Texture2D D3DDepthStencil { get; private set; } = null;

        public SharpDX.Direct3D11.DepthStencilView D3DDepthStencilView { get; private set; } = null;

        public SharpDX.Direct3D11.DepthStencilState D3DDepthStencilState { get; private set; } = null;

        public SharpDX.Mathematics.Interop.RawViewportF[] D3DViewPort { get; } = new SharpDX.Mathematics.Interop.RawViewportF[ 1 ];
        //----------------
        #endregion

        #region " 物理画面、設計画面とその変換に関するプロパティ "
        //----------------
        public Size2F 設計画面サイズ { get; protected set; } = Size2F.Empty;
        public Size2F 物理画面サイズ { get; protected set; } = Size2F.Empty;
        // ↑ int より float での利用が多いので、Size や Size2 ではなく Size2F を使う。
        //  （int 同士ということを忘れて、割り算しておかしくなるケースも多発したので。）

        public float 拡大率DPXtoPX横 => ( this.物理画面サイズ.Width / this.設計画面サイズ.Width );
        public float 拡大率DPXtoPX縦 => ( this.物理画面サイズ.Height / this.設計画面サイズ.Height );
        public float 拡大率PXtoDPX横 => ( this.設計画面サイズ.Width / this.物理画面サイズ.Width );
        public float 拡大率PXtoDPX縦 => ( this.設計画面サイズ.Height / this.物理画面サイズ.Height );
        public Matrix3x2 拡大行列DPXtoPX => Matrix3x2.Scaling( this.拡大率DPXtoPX横, this.拡大率DPXtoPX縦 );
        public Matrix3x2 拡大行列PXtoDPX => Matrix3x2.Scaling( this.拡大率PXtoDPX横, this.拡大率PXtoDPX縦 );

        /// <summary>
        ///     3D視点で見る画面左上の座標。
        /// </summary>
        public Vector3 画面左上dpx => new Vector3( -this.設計画面サイズ.Width / 2f, +this.設計画面サイズ.Height / 2f, 0f );
        //----------------
        #endregion

        #region " 3D変換用プロパティ "
        //----------------
        public float 視野角deg { get; set; } = 45f;

        public Matrix ビュー変換行列
        {
            get
            {
                var カメラの位置 = new Vector3( 0f, 0f, ( -2f * this._dz( this.設計画面サイズ.Height, this.視野角deg ) ) );
                var カメラの注視点 = new Vector3( 0f, 0f, 0f );
                var カメラの上方向 = new Vector3( 0f, 1f, 0f );

                var mat = Matrix.LookAtLH( カメラの位置, カメラの注視点, カメラの上方向 );

                mat.Transpose();  // 転置

                return mat;
            }
        }

        public Matrix 射影変換行列
        {
            get
            {
                float dz = this._dz( this.設計画面サイズ.Height, this.視野角deg );

                var mat = Matrix.PerspectiveFovLH(
                    MathUtil.DegreesToRadians( 視野角deg ),
                    設計画面サイズ.Width / 設計画面サイズ.Height,   // アスペクト比
                    -dz,                                            // 前方投影面までの距離
                    dz );                                           // 後方投影面までの距離

                mat.Transpose();  // 転置

                return mat;
            }
        }
        //----------------
        #endregion

        /// <summary>
        ///		現在時刻から、DirectComposition Engine による次のフレーム表示時刻までの間隔[秒]を返す。
        /// </summary>
        /// <remarks>
        ///		この時刻の仕様と使い方については、以下を参照。
        ///		Architecture and components - MSDN
        ///		https://msdn.microsoft.com/en-us/library/windows/desktop/hh437350.aspx
        /// </remarks>
        public double 次のDComp表示までの残り時間sec
        {
            get
            {
                var fs = this.DCompDevice.FrameStatistics;
                return ( fs.NextEstimatedFrameTime - fs.CurrentTime ) / fs.TimeFrequency;
            }
        }

        /// <summary>
        ///		指定したD2Dデバイスコンテキストに対して描画処理を実行する。
        /// </summary>
        /// <remarks>
        ///		描画処理は、D2Dデバイスコンテキストの BeginDraw() と EndDraw() の間で行われることが保証される。
        ///		描画処理中に例外が発生しても EndDraw() の呼び出しが確実に保証される。
        /// </remarks>
        /// <param name="dc">D2Dデバイスコンテキスト。</param>
        /// <param name="描画処理">BeginDraw() と EndDraw() の間で行う処理。</param>
        public void D2DBatchDraw( SharpDX.Direct2D1.DeviceContext1 dc, Action 描画処理 )
        {
            // リストになかったらこの dc を使うのは初回なので、BeginDraw/EndDraw() の呼び出しを行う。
            // もしリストに登録されていたら、この dc は他の誰かが BeginDraw して EndDraw してない状態
            // （D2DBatcDraw() の最中に D2DBatchDraw() が呼び出されている状態）なので、これらを呼び出してはならない。
            bool BeginとEndを行う = !( this._Draw中のレンダーターゲットリスト.Contains( dc ) );
            try
            {
                if( BeginとEndを行う )
                {
                    this._Draw中のレンダーターゲットリスト.Add( dc );     // Begin したらリストに追加。
                    dc.BeginDraw();
                }

                var trans = dc.Transform;
                var blend = dc.PrimitiveBlend;
                try
                {
                    描画処理();
                }
                finally
                {
                    // 描画が終わるたびに元に戻す。
                    dc.Transform = trans;
                    dc.PrimitiveBlend = blend;
                }
            }
            finally
            {
                if( BeginとEndを行う )
                {
                    dc.EndDraw();
                    this._Draw中のレンダーターゲットリスト.Remove( dc );  // End したらリストから削除。
                }
            }
        }

        /// <summary>
        ///		指定したレンダーターゲットに対して描画処理を実行する。
        /// </summary>
        /// <remarks>
        ///		描画処理は、レンダーターゲットの BeginDraw() と EndDraw() の間で行われることが保証される。
        ///		描画処理中に例外が発生しても EndDraw() の呼び出しが確実に保証される。
        /// </remarks>
        /// <param name="rt">レンダリングターゲット。</param>
        /// <param name="描画処理">BeginDraw() と EndDraw() の間で行う処理。</param>
        public void D2DBatchDraw( SharpDX.Direct2D1.RenderTarget rt, Action 描画処理 )
        {
            // リストになかったらこの rt を使うのは初回なので、BeginDraw/EndDraw() の呼び出しを行う。
            // もしリストに登録されていたら、この rt は他の誰かが BeginDraw して EndDraw してない状態
            // （D2DBatcDraw() の最中に D2DBatchDraw() が呼び出されている状態）なので、これらを呼び出してはならない。
            bool BeginとEndを行う = !( this._Draw中のレンダーターゲットリスト.Contains( rt ) );
            try
            {
                if( BeginとEndを行う )
                {
                    this._Draw中のレンダーターゲットリスト.Add( rt );     // Begin したらリストに追加。
                    rt.BeginDraw();
                }

                var trans = rt.Transform;
                try
                {
                    描画処理();
                }
                finally
                {
                    // 描画が終わるたびに元に戻す。
                    rt.Transform = trans;
                }
            }
            finally
            {
                if( BeginとEndを行う )
                {
                    rt.EndDraw();
                    this._Draw中のレンダーターゲットリスト.Remove( rt );  // End したらリストから削除。
                }
            }
        }

        private List<SharpDX.Direct2D1.RenderTarget> _Draw中のレンダーターゲットリスト = null;


        /// <summary>
        ///		バックバッファ（スワップチェーン）のサイズを変更する。
        /// </summary>
        /// <param name="newSize">新しいサイズ。</param>
        public void サイズを変更する( Size newSize )
        {
            // (1) 依存リソースを解放。
            this._スワップチェーンに依存するグラフィックリソースを解放する();

            // (2) バックバッファのサイズを変更。
            this.SwapChain.ResizeBuffers(
                0,                                  // 現在のバッファ数を維持
                newSize.Width,                      // 新しいサイズ
                newSize.Height,                     //
                SharpDX.DXGI.Format.Unknown,        // 現在のフォーマットを維持
                SharpDX.DXGI.SwapChainFlags.None );

            this.物理画面サイズ = new Size2F( newSize.Width, newSize.Height );

            // (3) 依存リソースを作成。
            this._スワップチェーンに依存するグラフィックリソースを作成する();
        }

        /// <summary>
        ///		バックバッファに対応するウィンドウのハンドル。
        ///		コンストラクタで指定する。
        /// </summary>
        private IntPtr _hWindow;

        private bool _深度ステンシルを使う = true;


        private void _スワップチェーンに依存しないグラフィックリソースを作成する()
        {
            this.DXGIDeviceManager = new SharpDX.MediaFoundation.DXGIDeviceManager();
#if DEBUG
            this.D2DFactory = new SharpDX.Direct2D1.Factory2( SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.Information );
#else
			this.D2DFactory = new SharpDX.Direct2D1.Factory2( SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.None );
#endif
            this.DWriteFactory = new SharpDX.DirectWrite.Factory( SharpDX.DirectWrite.FactoryType.Shared );

            this.WicImagingFactory = new SharpDX.WIC.ImagingFactory2();

            // D3Dデバイスを作成する。
            this.D3DDevice = new SharpDX.Direct3D11.Device(
                SharpDX.Direct3D.DriverType.Hardware,
#if DEBUG
                // D3D11 Debugメッセージは、Visual Studio のプロジェクトプロパティで「ネイティブコードのデバッグを有効にする」を ON にしないと表示されない。
                // なお、デバッグを有効にしてアプリケーションを実行すると、速度が大幅に低下する。
                SharpDX.Direct3D11.DeviceCreationFlags.Debug |
#endif
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
                new SharpDX.Direct3D.FeatureLevel[] {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                } );

            using( var dxgiDevice = this.D3DDevice.QueryInterface<SharpDX.DXGI.Device1>() )
            {
                #region " D3DDevice が ID3D11VideoDevice を実装してないならエラー。（Windows8以降のPCで実装されている。）"
                //----------------
                using( var videoDevice = this.D3DDevice.QueryInterfaceOrNull<SharpDX.Direct3D11.VideoDevice>() )
                {
                    if( null == videoDevice )
                        throw new Exception( "Direct3D11デバイスが、ID3D11VideoDevice をサポートしていません。" );
                }
                //----------------
                #endregion

                #region " マルチスレッドモードを ON に設定する。DXVAを使う場合は必須。"
                //----------------
                using( var multithread = this.D3DDevice.QueryInterfaceOrNull<SharpDX.Direct3D.DeviceMultithread>() )
                {
                    if( null == multithread )
                        throw new Exception( "Direct3D11デバイスが、ID3D10Multithread をサポートしていません。" );

                    multithread.SetMultithreadProtected( true );
                }
                //----------------
                #endregion

                // DXGIDevice のレイテンシ設定。
                dxgiDevice.MaximumFrameLatency = 1;

                // Debug フラグが立ってないなら null 。
                this.D3DDeviceDebug = this.D3DDevice.QueryInterfaceOrNull<SharpDX.Direct3D11.DeviceDebug>();

                // D2Dデバイスを作成する。
                this.D2DDevice = new SharpDX.Direct2D1.Device1( this.D2DFactory, dxgiDevice );

                // 既定のD2Dデバイスコンテキストを作成する。
                this.D2DDeviceContext = new SharpDX.Direct2D1.DeviceContext1( this.D2DDevice, SharpDX.Direct2D1.DeviceContextOptions.EnableMultithreadedOptimizations ) {
                    TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale,  // Grayscale がすべての Windows ストアアプリで推奨される。らしい。
                };

                // DirectCompositionデバイスを作成する。
                this.DCompDevice = new SharpDX.DirectComposition.DesktopDevice( this.D2DDevice );

                // スワップチェーン用のVisualを作成する。
                this.DCompVisualForSwapChain = new SharpDX.DirectComposition.Visual2( this.DCompDevice );

                // コンポジションターゲットを作成し、Visualツリーのルートにスワップチェーン用Visualを設定する。
                this.DCompTarget = SharpDX.DirectComposition.Target.FromHwnd( this.DCompDevice, this._hWindow, topmost: true );
                this.DCompTarget.Root = this.DCompVisualForSwapChain;

                // DXGIデバイスマネージャに D3Dデバイスを登録する。MediaFoundationで必須。
                this.DXGIDeviceManager.ResetDevice( this.D3DDevice );

                // 既定のDXGI出力を取得する。
                using( var dxgiAdapter = dxgiDevice.Adapter )
                {
                    this.DXGIOutput = dxgiAdapter.Outputs[ 0 ];
                }
            }

            テクスチャ.全インスタンスで共有するリソースを作成する();

            this.Animation = new WindowsAnimation();
        }
        private void _スワップチェーンに依存しないグラフィックリソースを解放する()
        {
            this.Animation?.Dispose();
            this.Animation = null;

            テクスチャ.全インスタンスで共有するリソースを解放する();

            this.DXGIOutput?.Dispose();
            this.DXGIOutput = null;

            this.DCompTarget.Root = null;

            this.DCompTarget?.Dispose();
            this.DCompTarget = null;

            this.DCompVisualForSwapChain?.Dispose();
            this.DCompVisualForSwapChain = null;

            this.DCompDevice?.Dispose();
            this.DCompDevice = null;

            this.D2DDeviceContext?.Dispose();
            this.D2DDeviceContext = null;

            this.D2DDevice?.Dispose();
            this.D2DDevice = null;

            this.WicImagingFactory?.Dispose();
            this.WicImagingFactory = null;

            this.DWriteFactory?.Dispose();
            this.DWriteFactory = null;

            this.D2DFactory?.Dispose();
            this.D2DFactory = null;

            this.DXGIDeviceManager?.Dispose();
            this.DXGIDeviceManager = null;
#if DEBUG
            this.D3DDeviceDebug?.ReportLiveDeviceObjects( SharpDX.Direct3D11.ReportingLevel.Detail );
#endif
            this.D3DDeviceDebug?.Dispose();
            this.D3DDeviceDebug = null;
       }

        private void _スワップチェーンを作成する()
        {
            var swapChainDesc = new SharpDX.DXGI.SwapChainDescription1() {
                BufferCount = 2,
                Width = (int) this.物理画面サイズ.Width,
                Height = (int) this.物理画面サイズ.Height,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,    // D2D をサポートするなら B8G8R8A8 を使う必要がある。
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore,      // Premultiplied にすると、ウィンドウの背景（デスクトップ画像）と加算合成される（意味ない）
                Stereo = false,
                SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ), // マルチサンプリングは使わない。
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,    // SwapChainForComposition での必須条件。
                Scaling = SharpDX.DXGI.Scaling.Stretch,                 // SwapChainForComposition での必須条件。
                Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                Flags = SharpDX.DXGI.SwapChainFlags.None,

                // https://msdn.microsoft.com/en-us/library/windows/desktop/bb174579.aspx
                // > You cannot call SetFullscreenState on a swap chain that you created with IDXGIFactory2::CreateSwapChainForComposition.
                // よって、以下のフラグは使用禁止。
                //Flags = SharpDX.DXGI.SwapChainFlags.AllowModeSwitch,
            };

            using( var dxgiDevice = this.D3DDevice.QueryInterface<SharpDX.DXGI.Device1>() )
            using( var dxgiAdapter = dxgiDevice.Adapter )
            using( var dxgiFactory = dxgiAdapter.GetParent<SharpDX.DXGI.Factory2>() )
            {
                this.SwapChain = new SharpDX.DXGI.SwapChain1( dxgiFactory, this.D3DDevice, ref swapChainDesc );   // IDXGIFactory2::CreateSwapChainForComposition

                // 標準機能である PrintScreen と Alt+Enter は使わない。
                dxgiFactory.MakeWindowAssociation(
                    this._hWindow,
                    SharpDX.DXGI.WindowAssociationFlags.IgnoreAll
                    //SharpDX.DXGI.WindowAssociationFlags.IgnorePrintScreen |
                    //SharpDX.DXGI.WindowAssociationFlags.IgnoreAltEnter
                    );
            }

            // DirectComposition 関連
            this.DCompVisualForSwapChain.Content = this.SwapChain;
            this.DCompDevice.Commit();
        }
        private void _スワップチェーンを解放する()
        {
            //this._SwapChain.SetFullscreenState( false, null );
            // --> このクラスでは「全画面」を使わない（代わりに「最大化」を使う）ので不要。

            this.SwapChain?.Dispose();
            this.SwapChain = null;
        }

        private void _スワップチェーンに依存するグラフィックリソースを作成する()
        {
            using( var backbufferTexture2D = this.SwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>( 0 ) ) // D3D 用
            using( var backbufferSurface = this.SwapChain.GetBackBuffer<SharpDX.DXGI.Surface>( 0 ) )           // D2D 用
            {
                // ※正確には、「スワップチェーン」というより、「スワップチェーンが持つバックバッファ」に依存するリソース。

                // D3D 関連

                #region " バックバッファに対するD3Dレンダーターゲットビューを作成する。"
                //----------------
                this.D3DRenderTargetView = new SharpDX.Direct3D11.RenderTargetView( this.D3DDevice, backbufferTexture2D );
                //----------------
                #endregion

                #region " バックバッファに対する深度ステンシル、深度ステンシルビュー、深度ステンシルステートを作成する。"
                //----------------
                var depthStencilDesc = new SharpDX.Direct3D11.Texture2DDescription() {
                    Width = backbufferTexture2D.Description.Width,
                    Height = backbufferTexture2D.Description.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = SharpDX.DXGI.Format.D32_Float, // Depthのみのフォーマット
                    SampleDescription = backbufferTexture2D.Description.SampleDescription,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,    // CPUからはアクセスしない
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                };
                this.D3DDepthStencil = new SharpDX.Direct3D11.Texture2D( this.D3DDevice, depthStencilDesc );

                var depthStencilViewDesc = new SharpDX.Direct3D11.DepthStencilViewDescription() {
                    Format = depthStencilDesc.Format,
                    Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D,
                    Flags = SharpDX.Direct3D11.DepthStencilViewFlags.None,
                    Texture2D = new SharpDX.Direct3D11.DepthStencilViewDescription.Texture2DResource() {
                        MipSlice = 0,
                    },
                };
                this.D3DDepthStencilView = new SharpDX.Direct3D11.DepthStencilView( this.D3DDevice, this.D3DDepthStencil, depthStencilViewDesc );

                var depthSencilStateDesc = new SharpDX.Direct3D11.DepthStencilStateDescription() {
                    IsDepthEnabled = this._深度ステンシルを使う,                // 深度テストあり？
                    DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask.All,     // 書き込む
                    DepthComparison = SharpDX.Direct3D11.Comparison.Less,       // 手前の物体を描画
                    IsStencilEnabled = false,                                   // ステンシルテストなし。
                    StencilReadMask = 0,
                    StencilWriteMask = 0,
                    // 面が表を向いている場合のステンシル・テストの設定
                    FrontFace = new SharpDX.Direct3D11.DepthStencilOperationDescription() {
                        FailOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
                        DepthFailOperation = SharpDX.Direct3D11.StencilOperation.Keep,  // 維持
                        PassOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
                        Comparison = SharpDX.Direct3D11.Comparison.Never,               // 常に失敗
                    },
                    // 面が裏を向いている場合のステンシル・テストの設定
                    BackFace = new SharpDX.Direct3D11.DepthStencilOperationDescription() {
                        FailOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
                        DepthFailOperation = SharpDX.Direct3D11.StencilOperation.Keep,  // 維持
                        PassOperation = SharpDX.Direct3D11.StencilOperation.Keep,       // 維持
                        Comparison = SharpDX.Direct3D11.Comparison.Always,              // 常に成功
                    },
                };
                this.D3DDepthStencilState = new SharpDX.Direct3D11.DepthStencilState( this.D3DDevice, depthSencilStateDesc );
                //----------------
                #endregion

                #region " バックバッファに対するビューポートを作成する。"
                //----------------
                this.D3DViewPort[ 0 ] = new SharpDX.Mathematics.Interop.RawViewportF() {
                    X = 0.0f,
                    Y = 0.0f,
                    Width = (float) backbufferTexture2D.Description.Width,
                    Height = (float) backbufferTexture2D.Description.Height,
                    MinDepth = 0.0f,
                    MaxDepth = 1.0f,
                };
                //----------------
                #endregion

                // D2D 関連

                #region " バックバッファとメモリを共有する、既定のD2Dレンダーターゲットビットマップを作成する。"
                //----------------
                this.D2DRenderBitmap = new SharpDX.Direct2D1.Bitmap1(   // このビットマップは、
                    this.D2DDeviceContext,
                    backbufferSurface,                                  // このDXGIサーフェスとメモリを共有する。
                    new SharpDX.Direct2D1.BitmapProperties1() {
                        PixelFormat = new SharpDX.Direct2D1.PixelFormat( backbufferSurface.Description.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied ),
                        BitmapOptions = SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw,
                    } );

                this.D2DDeviceContext.Target = this.D2DRenderBitmap;
                //----------------
                #endregion
            }
        }
        private void _スワップチェーンに依存するグラフィックリソースを解放する()
        {
            if( null != this.D3DDevice )
            {
                this.D3DDevice.ImmediateContext.ClearState();
                this.D3DDevice.ImmediateContext.OutputMerger.ResetTargets();
            }

            this.DCompVisualForSwapChain.Content = null;
            this.D2DDeviceContext.Target = null;

            this.D2DRenderBitmap?.Dispose();
            this.D2DRenderBitmap = null;

            this.D3DDepthStencilState?.Dispose();
            this.D3DDepthStencilState = null;

            this.D3DDepthStencilView?.Dispose();
            this.D3DDepthStencilView = null;

            this.D3DDepthStencil?.Dispose();
            this.D3DDepthStencil = null;

            this.D3DRenderTargetView?.Dispose();
            this.D3DRenderTargetView = null;
        }

        /// <summary>
        ///		ビューが Z=0 の位置に置かれるとき、ビューの高さと視野角から、カメラの Z 位置を計算して返す。
        /// </summary>
        private float _dz( float 高さ, float 視野角deg )
            => (float) ( 高さ / ( 4.0 * Math.Tan( MathUtil.DegreesToRadians( 視野角deg / 2.0f ) ) ) );
    }
}
