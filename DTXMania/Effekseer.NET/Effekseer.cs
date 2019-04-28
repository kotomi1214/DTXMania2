using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using FDK;

namespace DTXMania
{
    class Effekseer : IDisposable
    {
        public EffekseerRendererDX11NET.Manager Manager { get; protected set; }


        public Effekseer( SharpDX.Direct3D11.Device d3dDevice )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 描画用インスタンスの生成
                this._Renderer = EffekseerRendererDX11NET.Renderer.Create( d3dDevice, d3dDevice.ImmediateContext, 2000, SharpDX.Direct3D11.Comparison.Less );

                // エフェクト管理用インスタンスの生成
                this.Manager = EffekseerRendererDX11NET.Manager.Create( 2000, true );

                // 描画用インスタンスから描画機能を設定
                this.Manager.SetSpriteRenderer( this._Renderer.CreateSpriteRenderer() );
                this.Manager.SetRibbonRenderer( this._Renderer.CreateRibbonRenderer() );
                this.Manager.SetRingRenderer( this._Renderer.CreateRingRenderer() );
                this.Manager.SetTrackRenderer( this._Renderer.CreateTrackRenderer() );
                this.Manager.SetModelRenderer( this._Renderer.CreateModelRenderer() );

                // 描画用インスタンスからテクスチャの読込機能を設定
                // 独自拡張可能、現在はファイルから読み込んでいる。
                this.Manager.SetTextureLoader( this._Renderer.CreateTextureLoader() );
                this.Manager.SetModelLoader( this._Renderer.CreateModelLoader() );

                // カメラ行列を設定（右手系）
                var position = new SharpDX.Vector3( 10f, 5f, 20f );
                this._Renderer.SetCameraMatrix( SharpDX.Matrix.LookAtRH( position, new SharpDX.Vector3( 0f, 0f, 0f ), new SharpDX.Vector3( 0f, 1f, 0f ) ) );

                // 投影行列を設定（右手系）
                this._Renderer.SetProjectionMatrix( SharpDX.Matrix.PerspectiveFovRH( 90.0f / 180.0f * MathUtil.Pi, グラフィックデバイス.Instance.設計画面サイズ.Width / グラフィックデバイス.Instance.設計画面サイズ.Height, 1.0f, 50.0f ) );
            }
        }

        public void Draw()
        {
            //this._Renderer.BeginRendering();  --> DX9 で必要？
            this.Manager.Draw();
            //this._Renderer.EndRendering();
        }

        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 先にエフェクト管理用インスタンスを破棄
                this.Manager.Destroy();

                // 次に描画用インスタンスを破棄
                this._Renderer.Destroy();
            }
        }


        private EffekseerRendererDX11NET.Renderer _Renderer;
    }
}
