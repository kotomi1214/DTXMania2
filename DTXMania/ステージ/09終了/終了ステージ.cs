using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.終了
{
    class 終了ステージ : ステージ
    {
        public enum フェーズ
        {
            開始,
            表示中,
            開始音終了待ち,
            完了,
        }

        public フェーズ 現在のフェーズ { get; protected set; } = フェーズ.完了;



        // 生成と終了


        public 終了ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public override void OnDispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( this.活性化済み )
                    this.非活性化する();

                base.OnDispose();
            }
        }



        // 活性化と非活性化


        public override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._背景画像 = new 画像( @"$(System)Images\終了\終了画面.jpg" );

                this.現在のフェーズ = フェーズ.開始;

                base.On活性化();
            }
        }

        public override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._背景画像?.Dispose();

                this.現在のフェーズ = フェーズ.完了;

                base.On非活性化();
            }
        }



        // 進行と描画


        public override void 進行する()
        {
            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
                    App進行描画.システムサウンド.再生する( システムサウンド種別.終了ステージ_開始音 );
                    this._カウンタ = new Counter( 0, 1, 値をひとつ増加させるのにかける時間ms: 1000 );
                    this.現在のフェーズ = フェーズ.表示中;
                    break;

                case フェーズ.表示中:
                    if( this._カウンタ.終了値に達した )
                        this.現在のフェーズ = フェーズ.開始音終了待ち;
                    break;

                case フェーズ.開始音終了待ち:
                    if( !App進行描画.システムサウンド.再生中( システムサウンド種別.終了ステージ_開始音 ) )
                        this.現在のフェーズ = フェーズ.完了; // 再生が終わったらフェーズ遷移。
                    break;
            }
        }

        public override void 描画する()
        {
            var dc = DXResources.Instance.既定のD2D1DeviceContext;
            dc.Transform = DXResources.Instance.拡大行列DPXtoPX;

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示中:
                case フェーズ.開始音終了待ち:
                    this._背景画像?.描画する( dc );
                    break;
            }
        }



        // private


        private 画像 _背景画像 = null;

        private Counter _カウンタ = null;
    }
}
