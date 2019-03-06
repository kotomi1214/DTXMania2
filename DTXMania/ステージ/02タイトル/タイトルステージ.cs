using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using DTXMania.アイキャッチ;

namespace DTXMania.ステージ.タイトル
{
    class タイトルステージ : ステージ
    {
        public enum フェーズ
        {
            表示,
            フェードアウト,
            確定,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; }


        public タイトルステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._舞台画像 = new 舞台画像() );
                this.子Activityを追加する( this._タイトルロゴ = new 画像( @"$(System)images\タイトルロゴ.png" ) );
                this.子Activityを追加する( this._パッドを叩いてください = new 文字列画像() { 表示文字列 = "パッドを叩いてください", フォントサイズpt = 40f, 描画効果 = 文字列画像.効果.縁取り } );
                this.子Activityを追加する( this._システム情報 = new システム情報() );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._帯ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, new Color4( 0f, 0f, 0f, 0.8f ) );

                App.システムサウンド.再生する( 設定.システムサウンド種別.タイトルステージ_開始音 );
                App.システムサウンド.再生する( 設定.システムサウンド種別.タイトルステージ_ループBGM, ループ再生する: true );

                this.現在のフェーズ = フェーズ.表示;

                base.On活性化();
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.システムサウンド.停止する( 設定.システムサウンド種別.タイトルステージ_開始音 );
                App.システムサウンド.停止する( 設定.システムサウンド種別.タイトルステージ_ループBGM );
                //App.システムサウンド.停止する( 設定.システムサウンド種別.タイトルステージ_確定音 );  --> ならしっぱなしでいい

                this._帯ブラシ?.Dispose();
                this._帯ブラシ = null;

                base.On非活性化();
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            App.入力管理.すべての入力デバイスをポーリングする();

            this._システム情報.VPSをカウントする();
            this._システム情報.FPSをカウントしプロパティを更新する();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:
                    #region " タイトル画面を表示し、入力の判定を行う。"
                    //----------------
                    {
                        // 進行描画

                        this._舞台画像.進行描画する( dc );

                        this._タイトルロゴ.描画する(
                            dc,
                            ( グラフィックデバイス.Instance.設計画面サイズ.Width - this._タイトルロゴ.サイズ.Width ) / 2f,
                            ( グラフィックデバイス.Instance.設計画面サイズ.Height - this._タイトルロゴ.サイズ.Height ) / 2f - 100f );

                        this._帯メッセージを描画する( dc );

                        this._システム情報.描画する( dc );

                        // 入力

                        if( App.入力管理.確定キーが入力された() )
                        {
                            #region " 確定 "
                            //----------------
                            App.システムサウンド.再生する( 設定.システムサウンド種別.タイトルステージ_確定音 );
                            App.ステージ管理.アイキャッチを選択しクローズする( nameof( シャッター ) );
                            this.現在のフェーズ = フェーズ.フェードアウト;
                            //----------------
                            #endregion
                        }
                        else if( App.入力管理.キャンセルキーが入力された() )
                        {
                            #region " キャンセル "
                            //----------------
                            this.現在のフェーズ = フェーズ.キャンセル;
                            //----------------
                            #endregion
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.フェードアウト:
                    #region " アイキャッチを使って、タイトル画面をフェードアウトする。"
                    //----------------
                    {
                        // 画面描画

                        this._舞台画像.進行描画する( dc );

                        this._タイトルロゴ.描画する(
                            dc,
                            ( グラフィックデバイス.Instance.設計画面サイズ.Width - this._タイトルロゴ.サイズ.Width ) / 2f,
                            ( グラフィックデバイス.Instance.設計画面サイズ.Height - this._タイトルロゴ.サイズ.Height ) / 2f - 100f );

                        this._帯メッセージを描画する( dc );


                        // アイキャッチ描画

                        App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                        if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.クローズ完了 )
                        {
                            this.現在のフェーズ = フェーズ.確定;
                        }


                        // システム情報描画

                        this._システム情報.描画する( dc );
                    }
                    //----------------
                    #endregion
                    break;

                case フェーズ.確定:
                case フェーズ.キャンセル:
                    break;
            }
        }


        private 舞台画像 _舞台画像 = null;

        private 画像 _タイトルロゴ = null;

        private Brush _帯ブラシ = null;

        private 文字列画像 _パッドを叩いてください = null;

        private システム情報 _システム情報 = null;


        private void _帯メッセージを描画する( DeviceContext1 dc )
        {
            var 領域 = new RectangleF( 0f, 800f, グラフィックデバイス.Instance.設計画面サイズ.Width, 80f );

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                dc.FillRectangle( 領域, this._帯ブラシ );
            } );

            this._パッドを叩いてください.描画する( dc, 720f, 810f );
        }
    }
}
