using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.終了
{
    class 終了ステージ : ステージ
    {
        public enum フェーズ
        {
            開始,
            表示中,
            開始音終了待ち,
            確定,
        }

        public フェーズ 現在のフェーズ { get; protected set; }


        public 終了ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._背景画像 = new 画像( @"$(System)images\終了\終了画面.jpg" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.システムサウンド.再生する( 設定.システムサウンド種別.終了ステージ_開始音 );

                this.現在のフェーズ = フェーズ.開始;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.システムサウンド.停止する( 設定.システムサウンド種別.終了ステージ_開始音 );
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            switch( this.現在のフェーズ )
            {
                case フェーズ.開始:
                    this._カウンタ = new Counter( 0, 1, 値をひとつ増加させるのにかける時間ms: 1000 );
                    this.現在のフェーズ = フェーズ.表示中;
                    break;

                case フェーズ.表示中:
                    {
                        this._背景画像.描画する( dc );

                        if( this._カウンタ.終了値に達した )
                            this.現在のフェーズ = フェーズ.開始音終了待ち;
                    }
                    break;

                case フェーズ.開始音終了待ち:
                    {
                        this._背景画像.描画する( dc );

                        if( !App.システムサウンド.再生中( 設定.システムサウンド種別.終了ステージ_開始音 ) )
                            this.現在のフェーズ = フェーズ.確定; // 再生が終わったのでフェーズ遷移。
                    }
                    break;

                case フェーズ.確定:
                    break;
            }
        }


        private 画像 _背景画像 = null;

        private Counter _カウンタ = null;
    }
}
