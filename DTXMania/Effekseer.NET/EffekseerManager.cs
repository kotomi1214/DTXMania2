using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDK;

namespace DTXMania
{
    class EffekseerManager : IDisposable
    {
        public EffekseerNET.Manager Manager { get; protected set; } = null;

        public EffekseerNET.Renderer Renderer { get; protected set; } = null;

        /// <summary>
        ///     視点の位置。
        /// </summary>
        public SharpDX.Vector3 Position
        {
            get => this._Position;
            set
            {
                this._Position = value;
                this.Renderer.SetCameraMatrix( SharpDX.Matrix.LookAtRH( this._Position, new SharpDX.Vector3( 0f, 0f, 0f ), new SharpDX.Vector3( 0f, 1f, 0f ) ) );
            }
        }



        // 生成と終了


        public EffekseerManager()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 描画用インスタンスを生成する。
                this.Renderer = EffekseerNET.Renderer.Create(
                    DXResources.Instance.D3D11Device1,
                    DXResources.Instance.D3D11Device1.ImmediateContext,
                    squareMaxCount: 2000,
                    depthFunc: SharpDX.Direct3D11.Comparison.Less );

                // エフェクト管理用インスタンスを生成する。
                this.Manager = EffekseerNET.Manager.Create( instance_max: 2000, autoFlip: true );

                // 描画用インスタンスから描画機能を設定する。
                this.Manager.SetSpriteRenderer( this.Renderer.CreateSpriteRenderer() );
                this.Manager.SetRibbonRenderer( this.Renderer.CreateRibbonRenderer() );
                this.Manager.SetRingRenderer( this.Renderer.CreateRingRenderer() );
                this.Manager.SetTrackRenderer( this.Renderer.CreateTrackRenderer() );
                this.Manager.SetModelRenderer( this.Renderer.CreateModelRenderer() );

                // 描画用インスタンスからテクスチャの読込機能を設定する。
                // 独自拡張可能、現在はファイルから読み込んでいる。
                this.Manager.SetTextureLoader( this.Renderer.CreateTextureLoader() );
                this.Manager.SetModelLoader( this.Renderer.CreateModelLoader() );

                // 視点位置を確定する。
                this.Position = new SharpDX.Vector3( 10f, 5f, 20f );

                // 投影行列を設定する。
                this.Renderer.SetProjectionMatrix( SharpDX.Matrix.PerspectiveFovRH( 90.0f / 180.0f * SharpDX.MathUtil.Pi, (float) DXResources.Instance.設計画面サイズ.Width / (float) DXResources.Instance.設計画面サイズ.Height, 1.0f, 50.0f ) );

                // カメラ行列を設定する。
                this.Position = SharpDX.Vector3.Zero;
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 全エフェクトを停止する。
                this.Manager.StopAllEffects();

                // 先にエフェクト管理用インスタンスを破棄する。
                this.Manager?.Destroy();

                // 次に描画用インスタンスを破棄する。
                this.Renderer?.Destroy();
            }
        }



        // 進行と描画


        public void 進行描画する()
        {
            // エフェクトの更新処理を行う。
            this.Manager.Update( deltaFrame: 1f );

            // エフェクトの描画開始処理を行う。
            this.Renderer.BeginRendering();

            // エフェクトの描画を行う。
            this.Manager.Draw();

            // エフェクトの描画終了処理を行う。
            this.Renderer.EndRendering();
        }



        // ローカル


        private SharpDX.Vector3 _Position = SharpDX.Vector3.Zero;
    }
}
