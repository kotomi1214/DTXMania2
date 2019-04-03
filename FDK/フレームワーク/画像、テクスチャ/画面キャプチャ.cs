using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace FDK
{
    public class 画面キャプチャ
    {
        // インスタンス生成禁止。
        private 画面キャプチャ()
        {
        }

        /// <summary>
        ///		現在のバックバッファの内容を Bitmap に複写して返す。
        ///		すべての描画が終わったあと、Present() する前に呼び出すこと。
        /// </summary>
        /// <returns>Bitmap。使用後は解放すること。</returns>
        public static Bitmap 取得する()
        {
            Debug.Assert( null != グラフィックデバイス.Instance.D3D11Device1, "D3DDevice が取得されていません。" );

            // バックバッファの情報を取得する。
            var backBufferDesc = new Texture2DDescription();
            using( var backBuffer = グラフィックデバイス.Instance.DXGISwapChain1.GetBackBuffer<Texture2D>( 0 ) )
            {
                backBufferDesc = backBuffer.Description;
            }

            // CPUがアクセス可能な Texture2D バッファをGPU上に作成する。
            var captureTexture = new Texture2D(
                グラフィックデバイス.Instance.D3D11Device1,
                new Texture2DDescription() {
                    ArraySize = 1,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,   // CPUからアクセスできる。
                    Format = backBufferDesc.Format,
                    Height = backBufferDesc.Height,
                    Width = backBufferDesc.Width,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription( 1, 0 ),
                    Usage = ResourceUsage.Staging,  // GPU から CPU への copy ができる。
                } );

            // RenderTarget から Texture2D バッファに、GPU上で画像データをコピーする。
            using( var resource = グラフィックデバイス.Instance.既定のD3D11RenderTargetView.Resource )
            {
                グラフィックデバイス.Instance.D3D11Device1.ImmediateContext.CopyResource( resource, captureTexture );
            }

            // Texture2D の本体（DXGIサーフェス）から Bitmap を生成する。
            var bitmap = (Bitmap) null;
            using( var dxgiSurface = captureTexture.QueryInterface<Surface>() )
            {
                var dataRect = dxgiSurface.Map( SharpDX.DXGI.MapFlags.Read, out DataStream dataStream );

                bitmap = new Bitmap(
                    グラフィックデバイス.Instance.既定のD2D1DeviceContext,
                    new Size2( captureTexture.Description.Width, captureTexture.Description.Height ),
                    new DataPointer( dataStream.DataPointer, (int) dataStream.Length ),
                    dataRect.Pitch,
                    new BitmapProperties(
                        new PixelFormat( Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore ),
                        グラフィックデバイス.Instance.既定のD2D1DeviceContext.DotsPerInch.Width,
                        グラフィックデバイス.Instance.既定のD2D1DeviceContext.DotsPerInch.Width ) );

                dxgiSurface.Unmap();
            }

            return bitmap;
        }
    }
}
