using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SSTFormat.v4;
using FDK;

namespace DTXMania.曲読み込み
{
    class 曲読み込みステージ : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            完了,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; } = フェーズ.完了;


        
        // 生成と終了


        public 曲読み込みステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public override void OnDispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                base.OnDispose();
            }
        }



        // 活性化と非活性化


        public override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._舞台画像 = new 舞台画像();
                this._注意文 = new 画像( @"$(System)images\曲読み込み\ご注意ください.png" );
                this._曲名画像 = new 文字列画像() {
                    フォント名 = "HGMaruGothicMPRO",
                    フォントサイズpt = 70f,
                    フォント幅 = FontWeight.Regular,
                    フォントスタイル = FontStyle.Normal,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 10f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                };
                this._サブタイトル画像 = new 文字列画像() {
                    フォント名 = "HGMaruGothicMPRO",
                    フォントサイズpt = 45f,
                    フォント幅 = FontWeight.Regular,
                    フォントスタイル = FontStyle.Normal,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 7f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                };
                this._プレビュー画像 = new プレビュー画像();
                this._難易度 = new 難易度();


                var 選択曲 = App進行描画.曲ツリー.フォーカス曲ノード;

                this._曲名画像.表示文字列 = 選択曲.タイトル;
                this._サブタイトル画像.表示文字列 = 選択曲.サブタイトル;

                App進行描画.システムサウンド.再生する( システムサウンド種別.曲読み込みステージ_開始音 );
                App進行描画.システムサウンド.再生する( システムサウンド種別.曲読み込みステージ_ループBGM, ループ再生する: true );

                this._舞台画像.ぼかしと縮小を適用する( 0.0 );
                App進行描画.アイキャッチ管理.現在のアイキャッチ.オープンする();

                this.現在のフェーズ = フェーズ.フェードイン;

                base.On活性化();
            }
        }

        public override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                //App進行描画.システムサウンド.停止する( システムサウンド種別.曲読み込みステージ_開始音 ); --> なりっぱなしでいい
                App進行描画.システムサウンド.停止する( システムサウンド種別.曲読み込みステージ_ループBGM );

                this._難易度?.Dispose();
                this._プレビュー画像?.Dispose();
                this._サブタイトル画像?.Dispose();
                this._曲名画像?.Dispose();
                this._注意文?.Dispose();
                this._舞台画像?.Dispose();

                base.On非活性化();
            }
        }



        // 進行と描画


        public override void 進行する()
        {
        }

        public override void 描画する()
        {
            var dc = グラフィックデバイス.Instance.既定のD2D1DeviceContext;
            dc.Transform = グラフィックデバイス.Instance.拡大行列DPXtoPX;

            this._舞台画像.進行描画する( dc );
            this._注意文.描画する( dc, 0f, 760f );
            this._プレビュー画像.描画する( dc );
            this._難易度.描画する( dc );
            this._曲名を描画する( dc );
            this._サブタイトルを描画する( dc );

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    App進行描画.アイキャッチ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App進行描画.アイキャッチ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.オープン完了 )
                        this.現在のフェーズ = フェーズ.表示;

                    break;

                case フェーズ.表示:
                    this._スコアを読み込む();

                    App進行描画.入力管理.すべての入力デバイスをポーリングする();  // 先行入力があったらここでキャンセル

                    this.現在のフェーズ = フェーズ.完了;
                    break;

                case フェーズ.完了:
                case フェーズ.キャンセル:
                    break;
            }
        }

        private void _曲名を描画する( DeviceContext dc )
        {
            var 表示位置dpx = new Vector2( 782f, 409f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = グラフィックデバイス.Instance.設計画面サイズ.Width - 表示位置dpx.X;

            this._曲名画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._曲名画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._曲名画像.画像サイズdpx.Width );
        }

        private void _サブタイトルを描画する( DeviceContext dc )
        {
            var 表示位置dpx = new Vector2( 782f, 520f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = グラフィックデバイス.Instance.設計画面サイズ.Width - 表示位置dpx.X;

            this._サブタイトル画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._サブタイトル画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._サブタイトル画像.画像サイズdpx.Width );
        }



        // スコアの読み込み


        private void _スコアを読み込む()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 曲ファイルを読み込む。

                var 選択曲 = App進行描画.曲ツリー.フォーカス曲ノード;
                var 選択曲ファイルの絶対パス = 選択曲.曲ファイルの絶対パス;

                App進行描画.演奏スコア = スコア.ファイルから生成する( 選択曲ファイルの絶対パス.変数なしパス );


                // 全チップの発声時刻を修正する。

                foreach( var chip in App進行描画.演奏スコア.チップリスト )
                {
                    chip.発声時刻sec /= App進行描画.ユーザ管理.ログオン中のユーザ.再生速度;
                    chip.描画時刻sec /= App進行描画.ユーザ管理.ログオン中のユーザ.再生速度;

                    chip.発声時刻sec -= App進行描画.サウンドデバイス.再生遅延sec;
                }

                // 完了。

                Log.Info( $"曲ファイルを読み込みました。" );
                Log.Info( $"曲名: {App進行描画.演奏スコア.曲名}" );
            }
        }



        // private


        private 舞台画像 _舞台画像 = null;

        private 画像 _注意文 = null;

        private 文字列画像 _曲名画像 = null;

        private 文字列画像 _サブタイトル画像 = null;

        private プレビュー画像 _プレビュー画像 = null;

        private 難易度 _難易度 = null;
    }
}
