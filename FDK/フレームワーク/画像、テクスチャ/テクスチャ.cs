using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace FDK
{
    /// <summary>
    ///		Direct3D の D3DTexture を使って描画する画像。
    /// </summary>
    public class テクスチャ : IDisposable
    {

        // プロパティ


        /// <summary>
        ///		0:透明～1:不透明
        /// </summary>
        public float 不透明度 { get; set; } = 1f;

        public bool 加算合成する { get; set; } = false;

        public Size2F サイズ { get; protected set; }

        

        // 生成と終了


        /// <summary>
        ///     指定した画像ファイルからテクスチャを作成する。
        /// </summary>
        public テクスチャ( VariablePath 画像ファイルパス, BindFlags bindFlags = BindFlags.ShaderResource )
        {
            this._画像ファイルパス = 画像ファイルパス;
            this.ユーザ指定サイズ = Size2F.Zero;

            this._bindFlags = bindFlags;
            this._定数バッファ = this._定数バッファを作成する();

            // テクスチャとシェーダーリソースビューを生成する。

            if( this._画像ファイルパス.変数なしパス.Nullでも空でもない() )
            {
                if( !System.IO.File.Exists( this._画像ファイルパス.変数なしパス ) )
                {
                    Log.ERROR( $"画像ファイルが存在しません。[{this._画像ファイルパス.変数付きパス}]" );
                    return;
                }

                var テクスチャリソース = FDKUtilities.CreateShaderResourceViewFromFile(
                    DXResources.Instance.D3D11Device1,
                    this._bindFlags,
                    this._画像ファイルパス );

                this._ShaderResourceView = テクスチャリソース.srv;
                this.サイズ = テクスチャリソース.viewSize;
                this.Texture = テクスチャリソース.texture;
            }
        }

        /// <summary>
        ///     指定したサイズの、空のテクスチャを作成する。
        /// </summary>
        public テクスチャ( Size2F サイズ, BindFlags bindFlags = BindFlags.ShaderResource )
        {
            this._画像ファイルパス = null;
            this.ユーザ指定サイズ = サイズ;

            this._bindFlags = bindFlags;
            this._定数バッファ = this._定数バッファを作成する();

            // テクスチャとシェーダーリソースビューを生成する。

            if( ( 0f >= this.ユーザ指定サイズ.Width ) && ( 0f >= this.ユーザ指定サイズ.Height ) )
            {
                Log.ERROR( $"テクスチャサイズが不正です。[{this.ユーザ指定サイズ}]" );
                return;
            }

            var テクスチャリソース = FDKUtilities.CreateShaderResourceView(
                DXResources.Instance.D3D11Device1,
                this._bindFlags,
                new Size2( (int) this.ユーザ指定サイズ.Width, (int) this.ユーザ指定サイズ.Height ) );

            this._ShaderResourceView = テクスチャリソース.srv;
            this.サイズ = this.ユーザ指定サイズ;
            this.Texture = テクスチャリソース.texture;
        }

        public virtual void Dispose()
        {
            this._ShaderResourceView?.Dispose();
            this._ShaderResourceView = null;

            this.Texture?.Dispose();
            this.Texture = null;

            this._定数バッファ?.Dispose();
            this._定数バッファ = null;
        }

        private SharpDX.Direct3D11.Buffer _定数バッファを作成する()
        {
            return new SharpDX.Direct3D11.Buffer(
                DXResources.Instance.D3D11Device1,
                new BufferDescription() {
                    Usage = ResourceUsage.Dynamic,              // 動的使用法
                    BindFlags = BindFlags.ConstantBuffer,       // 定数バッファ
                    CpuAccessFlags = CpuAccessFlags.Write,      // CPUから書き込む
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = SharpDX.Utilities.SizeOf<ST定数バッファの転送元データ>(),   // バッファサイズ
                    StructureByteStride = 0,
                } );
        }



        // 描画


        /// <summary>
        ///		テクスチャを描画する。
        ///	</summary>
        public void 描画する( float 左位置, float 上位置, float 不透明度0to1 = 1.0f, float X方向拡大率 = 1.0f, float Y方向拡大率 = 1.0f, RectangleF? 転送元矩形 = null )
        {
            RectangleF srcRect = 転送元矩形 ?? new RectangleF( 0, 0, this.サイズ.Width, this.サイズ.Height );

            var 変換行列 =
                Matrix.Scaling( X方向拡大率, Y方向拡大率, 0f ) *
                Matrix.Translation(
                    DXResources.Instance.画面左上dpx.X + ( 左位置 + X方向拡大率 * srcRect.Width / 2f ),
                    DXResources.Instance.画面左上dpx.Y - ( 上位置 + Y方向拡大率 * srcRect.Height / 2f ),
                    0f );

            this.描画する( 変換行列, 不透明度0to1, 転送元矩形 );
        }

        /// <summary>
        ///		テクスチャを描画する。
        ///	</summary>
        /// <param name="ワールド行列変換">テクスチャは原寸（<see cref="サイズ"/>）にスケーリングされており、その後にこのワールド行列が適用される。</param>
        /// <param name="転送元矩形">テクスチャ座標(値域0～1)で指定する。</param>
        public void 描画する( Matrix ワールド行列変換, float 不透明度0to1 = 1f, RectangleF? 転送元矩形 = null )
        {
            if( null == this.Texture )
                return;

            var d3ddc = DXResources.Instance.D3D11Device1.ImmediateContext;

            this.不透明度 = MathUtil.Clamp( 不透明度0to1, 0f, 1f );
            var srcRect = 転送元矩形 ?? new RectangleF( 0f, 0f, this.サイズ.Width, this.サイズ.Height );

            #region " 定数バッファを更新する。"
            //----------------
            {
                // 1x1のモデルサイズをテクスチャの描画矩形サイズへスケーリングする行列を前方に乗じる。
                ワールド行列変換 =
                    Matrix.Scaling( srcRect.Width, srcRect.Height, 0f ) *
                    ワールド行列変換;

                // ワールド変換行列
                ワールド行列変換.Transpose();    // 転置
                this._定数バッファの転送元データ.World = ワールド行列変換;

                // ビュー変換行列と射影変換行列
                DXResources.Instance.等倍3D平面描画用の変換行列を取得する( out Matrix 転置済みビュー行列, out Matrix 転置済み射影行列 );
                this._定数バッファの転送元データ.View = 転置済みビュー行列;
                this._定数バッファの転送元データ.Projection = 転置済み射影行列;

                // 描画元矩形（x,y,zは0～1で指定する（UV座標））
                this._定数バッファの転送元データ.TexLeft = srcRect.Left / this.サイズ.Width;
                this._定数バッファの転送元データ.TexTop = srcRect.Top / this.サイズ.Height;
                this._定数バッファの転送元データ.TexRight = srcRect.Right / this.サイズ.Width;
                this._定数バッファの転送元データ.TexBottom = srcRect.Bottom / this.サイズ.Height;

                // アルファ
                this._定数バッファの転送元データ.TexAlpha = this.不透明度;
                this._定数バッファの転送元データ.dummy1 = 0f;
                this._定数バッファの転送元データ.dummy2 = 0f;
                this._定数バッファの転送元データ.dummy3 = 0f;

                // 定数バッファへ書き込む。
                var dataBox = d3ddc.MapSubresource(
                    resourceRef: this._定数バッファ,
                    subresource: 0,
                    mapType: MapMode.WriteDiscard,
                    mapFlags: MapFlags.None );
                SharpDX.Utilities.Write( dataBox.DataPointer, ref this._定数バッファの転送元データ );
                d3ddc.UnmapSubresource( this._定数バッファ, 0 );
            }
            //----------------
            #endregion

            #region " 3Dパイプラインを設定する。"
            //----------------
            {
                // 入力アセンブラ
                d3ddc.InputAssembler.InputLayout = null;
                d3ddc.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

                // 頂点シェーダ
                d3ddc.VertexShader.Set( テクスチャ._VertexShader );
                d3ddc.VertexShader.SetConstantBuffers( 0, this._定数バッファ );

                // ハルシェーダ
                d3ddc.HullShader.Set( null );

                // ドメインシェーダ
                d3ddc.DomainShader.Set( null );

                // ジオメトリシェーダ
                d3ddc.GeometryShader.Set( null );

                // ラスタライザ
                d3ddc.Rasterizer.SetViewports( DXResources.Instance.既定のD3D11ViewPort );
                d3ddc.Rasterizer.State = テクスチャ._RasterizerState;

                // ピクセルシェーダ
                d3ddc.PixelShader.Set( テクスチャ._PixelShader );
                d3ddc.PixelShader.SetConstantBuffers( 0, this._定数バッファ );
                d3ddc.PixelShader.SetShaderResources( 0, 1, this._ShaderResourceView );
                d3ddc.PixelShader.SetSamplers( 0, 1, テクスチャ._SamplerState );

                // 出力マージャ
                d3ddc.OutputMerger.SetTargets( DXResources.Instance.既定のD3D11DepthStencilView, DXResources.Instance.既定のD3D11RenderTargetView );
                d3ddc.OutputMerger.SetBlendState(
                    ( this.加算合成する ) ? テクスチャ._BlendState加算合成 : テクスチャ._BlendState通常合成,
                    new Color4( 0f, 0f, 0f, 0f ),
                    -1 );
                d3ddc.OutputMerger.SetDepthStencilState( DXResources.Instance.既定のD3D11DepthStencilState, 0 );
            }
            //----------------
            #endregion

            // 頂点バッファとインデックスバッファを使わずに 4 つの頂点を描画する。
            d3ddc.Draw( vertexCount: 4, startVertexLocation: 0 );
        }



        // リソース


        /// <summary>
        ///     生成時に、サイズの指定があった場合、そのサイズ。
        ///     <see cref="サイズ"/> と同一とは保証しない。
        /// </summary>
        protected Size2F ユーザ指定サイズ;

        private VariablePath _画像ファイルパス = null;

        private BindFlags _bindFlags;

        protected Texture2D Texture = null;

        private SharpDX.Direct3D11.Buffer _定数バッファ = null;

        private ShaderResourceView _ShaderResourceView = null;

        private struct ST定数バッファの転送元データ
        {
            public Matrix World;      // ワールド変換行列
            public Matrix View;       // ビュー変換行列
            public Matrix Projection; // 透視変換行列

            public float TexLeft;   // 描画元矩形の左u座標(0～1)
            public float TexTop;    // 描画元矩形の上v座標(0～1)
            public float TexRight;  // 描画元矩形の右u座標(0～1)
            public float TexBottom; // 描画元矩形の下v座標(0～1)

            public float TexAlpha;  // テクスチャに乗じるアルファ値(0～1)
            public float dummy1;    // float4境界に合わせるためのダミー
            public float dummy2;    // float4境界に合わせるためのダミー
            public float dummy3;    // float4境界に合わせるためのダミー
        };
        private ST定数バッファの転送元データ _定数バッファの転送元データ;



        // 全インスタンス共通項目(static) 


        private static bool _全インスタンスで共有するリソースを作成済み = false;


        public static void 全インスタンスで共有するリソースを作成する()
        {
            if( _全インスタンスで共有するリソースを作成済み )
                return;

            _全インスタンスで共有するリソースを作成済み = true;


            var d3dDevice = DXResources.Instance.D3D11Device1;

            var シェーダコンパイルのオプション =
                ShaderFlags.Debug |
                ShaderFlags.SkipOptimization |
                ShaderFlags.EnableStrictness |
                ShaderFlags.PackMatrixColumnMajor;

            #region " 頂点シェーダを生成する。"
            //----------------
            {
                // シェーダコードをコンパイルする。
                using( var code = ShaderBytecode.Compile(
                    Properties.Resources.テクスチャ用シェーダコード,
                    "VS", "vs_5_0", シェーダコンパイルのオプション ) )
                {
                    // 頂点シェーダを生成する。
                    テクスチャ._VertexShader = new VertexShader( d3dDevice, code );
                }
            }
            //----------------
            #endregion

            #region " ピクセルシェーダを生成する。"
            //----------------
            {
                // シェーダコードをコンパイルする。
                using( var code = ShaderBytecode.Compile(
                    Properties.Resources.テクスチャ用シェーダコード,
                    "PS", "ps_5_0", シェーダコンパイルのオプション ) )
                {
                    // ピクセルシェーダを作成する。
                    テクスチャ._PixelShader = new PixelShader( d3dDevice, code );
                }
            }
            //----------------
            #endregion

            #region " ブレンドステート通常版を生成する。"
            //----------------
            {
                var BlendStateNorm = new BlendStateDescription() {
                    AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
                    IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
                };
                BlendStateNorm.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
                BlendStateNorm.RenderTarget[ 0 ].RenderTargetWriteMask = ColorWriteMaskFlags.All;        // RGBA の書き込みマスク。

                // アルファ値のブレンディング設定 ... 特になし
                BlendStateNorm.RenderTarget[ 0 ].SourceAlphaBlend = BlendOption.One;
                BlendStateNorm.RenderTarget[ 0 ].DestinationAlphaBlend = BlendOption.Zero;
                BlendStateNorm.RenderTarget[ 0 ].AlphaBlendOperation = BlendOperation.Add;

                // 色値のブレンディング設定 ... アルファ強度に応じた透明合成（テクスチャのアルファ値は、テクスチャのアルファ×ピクセルシェーダでの全体アルファとする（HLSL参照））
                BlendStateNorm.RenderTarget[ 0 ].SourceBlend = BlendOption.SourceAlpha;
                BlendStateNorm.RenderTarget[ 0 ].DestinationBlend = BlendOption.InverseSourceAlpha;
                BlendStateNorm.RenderTarget[ 0 ].BlendOperation = BlendOperation.Add;

                // ブレンドステートを作成する。
                テクスチャ._BlendState通常合成 = new BlendState( d3dDevice, BlendStateNorm );
            }
            //----------------
            #endregion

            #region " ブレンドステート加算合成版を生成する。"
            //----------------
            {
                var BlendStateAdd = new BlendStateDescription() {
                    AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
                    IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
                };
                BlendStateAdd.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
                BlendStateAdd.RenderTarget[ 0 ].RenderTargetWriteMask = ColorWriteMaskFlags.All;        // RGBA の書き込みマスク。

                // アルファ値のブレンディング設定 ... 特になし
                BlendStateAdd.RenderTarget[ 0 ].SourceAlphaBlend = BlendOption.One;
                BlendStateAdd.RenderTarget[ 0 ].DestinationAlphaBlend = BlendOption.Zero;
                BlendStateAdd.RenderTarget[ 0 ].AlphaBlendOperation = BlendOperation.Add;

                // 色値のブレンディング設定 ... 加算合成
                BlendStateAdd.RenderTarget[ 0 ].SourceBlend = BlendOption.SourceAlpha;
                BlendStateAdd.RenderTarget[ 0 ].DestinationBlend = BlendOption.One;
                BlendStateAdd.RenderTarget[ 0 ].BlendOperation = BlendOperation.Add;

                // ブレンドステートを作成する。
                テクスチャ._BlendState加算合成 = new BlendState( d3dDevice, BlendStateAdd );
            }
            //----------------
            #endregion

            #region " ラスタライザステートを生成する。"
            //----------------
            {
                var RSDesc = new RasterizerStateDescription() {
                    FillMode = FillMode.Solid,   // 普通に描画する
                    CullMode = CullMode.None,    // 両面を描画する
                    IsFrontCounterClockwise = false,    // 時計回りが表面
                    DepthBias = 0,
                    DepthBiasClamp = 0,
                    SlopeScaledDepthBias = 0,
                    IsDepthClipEnabled = true,
                    IsScissorEnabled = false,
                    IsMultisampleEnabled = false,
                    IsAntialiasedLineEnabled = false,
                };

                テクスチャ._RasterizerState = new RasterizerState( d3dDevice, RSDesc );
            }
            //----------------
            #endregion

            #region " サンプラーステートを生成する。"
            //----------------
            {
                var descSampler = new SamplerStateDescription() {
                    Filter = Filter.Anisotropic,
                    AddressU = TextureAddressMode.Border,
                    AddressV = TextureAddressMode.Border,
                    AddressW = TextureAddressMode.Border,
                    MipLodBias = 0.0f,
                    MaximumAnisotropy = 2,
                    ComparisonFunction = Comparison.Never,
                    BorderColor = new RawColor4( 0f, 0f, 0f, 0f ),
                    MinimumLod = float.MinValue,
                    MaximumLod = float.MaxValue,
                };

                テクスチャ._SamplerState = new SamplerState( d3dDevice, descSampler );
            }
            //----------------
            #endregion
        }

        public static void 全インスタンスで共有するリソースを解放する()
        {
            if( !_全インスタンスで共有するリソースを作成済み )
                return;

            テクスチャ._SamplerState?.Dispose();
            テクスチャ._SamplerState = null;

            テクスチャ._RasterizerState?.Dispose();
            テクスチャ._RasterizerState = null;

            テクスチャ._BlendState加算合成?.Dispose();
            テクスチャ._BlendState加算合成 = null;

            テクスチャ._BlendState通常合成?.Dispose();
            テクスチャ._BlendState通常合成 = null;

            テクスチャ._PixelShader?.Dispose();
            テクスチャ._PixelShader = null;

            テクスチャ._VertexShader?.Dispose();
            テクスチャ._VertexShader = null;

            _全インスタンスで共有するリソースを作成済み = false;
        }


        private static VertexShader _VertexShader = null;

        private static PixelShader _PixelShader = null;

        private static BlendState _BlendState通常合成 = null;

        private static BlendState _BlendState加算合成 = null;

        private static RasterizerState _RasterizerState = null;

        private static SamplerState _SamplerState = null;
    }
}
