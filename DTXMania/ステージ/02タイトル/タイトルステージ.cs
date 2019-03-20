using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania
{
    class タイトルステージ : ステージ
    {
        public enum フェーズ
        {
            表示,
            フェードアウト,
            完了,
            キャンセル,
        }
        public フェーズ 現在のフェーズ { get; protected set; }



        // 生成と終了


        public タイトルステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._舞台画像 = new 舞台画像();
                this._タイトルロゴ = new 画像( @"$(System)images\タイトルロゴ.png" );
                this._パッドを叩いてください = new 文字列画像() { 表示文字列 = "パッドを叩いてください", フォントサイズpt = 40f, 描画効果 = 文字列画像.効果.縁取り };
                this._システム情報 = new システム情報();

                this._帯ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.既定のD2D1DeviceContext, new Color4( 0f, 0f, 0f, 0.8f ) );
                
                App進行描画.システムサウンド.再生する( システムサウンド種別.タイトルステージ_開始音 );
                App進行描画.システムサウンド.再生する( システムサウンド種別.タイトルステージ_ループBGM, ループ再生する: true );

                this.現在のフェーズ = フェーズ.表示;
            }
        }

        public override void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App進行描画.システムサウンド.停止する( システムサウンド種別.タイトルステージ_開始音 );
                App進行描画.システムサウンド.停止する( システムサウンド種別.タイトルステージ_ループBGM );
                //App進行描画.システムサウンド.停止する( システムサウンド種別.タイトルステージ_確定音 );  --> ならしっぱなしでいい

                this._帯ブラシ?.Dispose();
                this._帯ブラシ = null;
            }
        }



        // 活性化と非活性化


        public override void 活性化する()
        {
        }

        public override void 非活性化する()
        {
        }



        // 進行と描画


        public override void 進行する()
        {
            App進行描画.入力管理.すべての入力デバイスをポーリングする();

            this._システム情報.FPSをカウントしプロパティを更新する();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:
                    #region " 入力判定 "
                    //----------------
                    if( App進行描画.入力管理.確定キーが入力された() )
                    {
                        #region " 確定 "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.タイトルステージ_確定音 );
                        App進行描画.アイキャッチ管理.アイキャッチを選択しクローズする( nameof( シャッター ) );

                        this.現在のフェーズ = フェーズ.フェードアウト;
                        //----------------
                        #endregion
                    }
                    else if( App進行描画.入力管理.キャンセルキーが入力された() )
                    {
                        #region " キャンセル "
                        //----------------
                        this.現在のフェーズ = フェーズ.キャンセル;
                        //----------------
                        #endregion
                    }
                    //----------------
                    #endregion
                    break;
            }
        }

        public override void 描画する()
        {
            this._システム情報.VPSをカウントする();

            var dc = グラフィックデバイス.Instance.既定のD2D1DeviceContext;

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:
                    #region " タイトル画面を表示する。"
                    //----------------
                    this._舞台画像.進行描画する( dc );
                    this._タイトルロゴ.描画する(
                        dc,
                        ( グラフィックデバイス.Instance.設計画面サイズ.Width - this._タイトルロゴ.サイズ.Width ) / 2f,
                        ( グラフィックデバイス.Instance.設計画面サイズ.Height - this._タイトルロゴ.サイズ.Height ) / 2f - 100f );
                    this._帯メッセージを描画する( dc );
                    //----------------
                    #endregion
                    break;

                case フェーズ.フェードアウト:
                    #region " タイトル画面を表示する。"
                    //----------------
                    this._舞台画像.進行描画する( dc );
                    this._タイトルロゴ.描画する(
                        dc,
                        ( グラフィックデバイス.Instance.設計画面サイズ.Width - this._タイトルロゴ.サイズ.Width ) / 2f,
                        ( グラフィックデバイス.Instance.設計画面サイズ.Height - this._タイトルロゴ.サイズ.Height ) / 2f - 100f );
                    this._帯メッセージを描画する( dc );
                    //----------------
                    #endregion
                    #region " アイキャッチを描画する。"
                    //----------------
                    App進行描画.アイキャッチ管理.現在のアイキャッチ.進行描画する( グラフィックデバイス.Instance.既定のD2D1DeviceContext );

                    if( App進行描画.アイキャッチ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
                    {
                        this.現在のフェーズ = フェーズ.完了;
                    }
                    //----------------
                    #endregion
                    break;
            }

            this._システム情報.描画する( dc );
        }



        // private


        private 舞台画像 _舞台画像 = null;

        private 画像 _タイトルロゴ = null;

        private Brush _帯ブラシ = null;

        private 文字列画像 _パッドを叩いてください = null;

        private システム情報 _システム情報 = null;


        private void _帯メッセージを描画する( DeviceContext dc )
        {
            var 領域 = new RectangleF( 0f, 800f, グラフィックデバイス.Instance.設計画面サイズ.Width, 80f );

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                dc.FillRectangle( 領域, this._帯ブラシ );
            } );

            this._パッドを叩いてください.描画する( dc, 720f, 810f );
        }
    }
}
