using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DirectInput;
using FDK;
using DTXMania.曲;
using DTXMania.設定;
using DTXMania.アイキャッチ;
using DTXMania.ステージ.演奏;

namespace DTXMania.ステージ.結果
{
    class 結果ステージ : ステージ
    {
        public enum フェーズ
        {
            表示,
            フェードアウト,
            確定,
        }

        public フェーズ 現在のフェーズ { get; protected set; }


        // 外部依存アクション; ステージ管理クラスで接続。

        internal Func<成績> 結果を取得する = null;

        internal Action BGMを停止する = null;


        public 結果ステージ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._背景 = new 舞台画像() );
                this.子Activityを追加する( this._曲名パネル = new 画像( @"$(System)images\結果\曲名パネル.png" ) );
                this.子Activityを追加する( this._曲名画像 = new 文字列画像() {
                    フォント名 = "HGMaruGothicMPRO",
                    フォントサイズpt = 40f,
                    フォント幅 = FontWeight.Regular,
                    フォントスタイル = FontStyle.Normal,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 6f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                } );
                this.子Activityを追加する( this._サブタイトル画像 = new 文字列画像() {
                    フォント名 = "HGMaruGothicMPRO",
                    フォントサイズpt = 25f,
                    フォント幅 = FontWeight.Regular,
                    フォントスタイル = FontStyle.Normal,
                    描画効果 = 文字列画像.効果.縁取り,
                    縁のサイズdpx = 5f,
                    前景色 = Color4.Black,
                    背景色 = Color4.White,
                } );
                this.子Activityを追加する( this._演奏パラメータ結果 = new 演奏パラメータ結果() );
                this.子Activityを追加する( this._ランク = new ランク() );
                this.子Activityを追加する( this._システム情報 = new システム情報() );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 選択曲 = App.曲ツリー.フォーカス曲ノード;
                Debug.Assert( null != 選択曲 );

                this._結果 = this.結果を取得する();

                App.システムサウンド.再生する( システムサウンド種別.ステージクリア );

                // 成績をDBに記録。
                if( !( App.ユーザ管理.ログオン中のユーザ.AutoPlayがすべてONである ) )    // ただし全AUTOなら記録しない。
                    曲DB.成績を追加または更新する( this._結果, App.ユーザ管理.ログオン中のユーザ.ユーザID, 選択曲.曲ファイルハッシュ );

                this._曲名画像.表示文字列 = 選択曲.タイトル;
                this._サブタイトル画像.表示文字列 = 選択曲.サブタイトル;

                this._黒マスクブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, new Color4( Color3.Black, 0.75f ) );
                this._プレビュー枠ブラシ = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, new Color4( 0xFF209292 ) );

                this.現在のフェーズ = フェーズ.表示;
                this._初めての進行描画 = true;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.システムサウンド.停止する( システムサウンド種別.ステージクリア );

                this._結果 = null;

                this._黒マスクブラシ?.Dispose();
                this._黒マスクブラシ = null;

                this._プレビュー枠ブラシ?.Dispose();
                this._プレビュー枠ブラシ = null;

                this.BGMを停止する();

                App.WAV管理?.Dispose();
                App.WAV管理 = null;
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            // 進行描画

            if( this._初めての進行描画 )
            {
                this._背景.ぼかしと縮小を適用する( 0.0 );    // 即時適用
                this._初めての進行描画 = false;
            }

            this._システム情報.VPSをカウントする();
            this._システム情報.FPSをカウントしプロパティを更新する();

            this._背景.進行描画する( dc );
            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                dc.FillRectangle( new RectangleF( 0f, 36f, グラフィックデバイス.Instance.設計画面サイズ.Width, グラフィックデバイス.Instance.設計画面サイズ.Height - 72f ), this._黒マスクブラシ );
            } );
            this._プレビュー画像を描画する( dc );
            this._曲名パネル.描画する( dc, 660f, 796f );
            this._曲名を描画する( dc );
            this._サブタイトルを描画する( dc );
            this._演奏パラメータ結果.描画する( dc, 1317f, 716f, this._結果 );
            this._ランク.進行描画する( this._結果.ランク );

            // 入力

            App.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:
                    if( App.入力管理.確定キーが入力された() )
                    {
                        #region " 確定キー　→　フェーズアウトへ "
                        //----------------
                        App.システムサウンド.再生する( システムサウンド種別.取消音 );    // 確定だけど取消音
                        App.ステージ管理.アイキャッチを選択しクローズする( nameof( シャッター ) );
                        this.現在のフェーズ = フェーズ.フェードアウト;
                        //----------------
                        #endregion
                    }
                    else if( App.ユーザ管理.ログオン中のユーザ.ドラムの音を発声する )
                    {
                        #region " その他　→　空うちサウンドを再生 "
                        //----------------
                        // すべての押下入力について……
                        foreach( var 入力 in App.入力管理.ポーリング結果.Where( ( e ) => e.InputEvent.押された ) )
                        {
                            var 押下入力に対応するすべてのドラムチッププロパティのリスト
                                = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理.チップtoプロパティ.Where( ( kvp ) => ( kvp.Value.ドラム入力種別 == 入力.Type ) );

                            foreach( var kvp in 押下入力に対応するすべてのドラムチッププロパティのリスト )
                            {
                                var ドラムチッププロパティ = kvp.Value;

                                if( 0 < App.演奏スコア.空打ちチップマップ.Count )
                                {
                                    #region " (A) 空うちチップマップが存在する場合（DTX他の場合）"
                                    //----------------
                                    int zz = App.演奏スコア.空打ちチップマップ[ ドラムチッププロパティ.レーン種別 ];  // WAVのzz番号。登録されていなければ 0

                                    if( 0 != zz )
                                    {
                                        // (A-a) 空打ちチップの指定があるなら、それを発声する。
                                        App.WAV管理.発声する( zz, ドラムチッププロパティ.チップ種別, ドラムチッププロパティ.発声前消音, ドラムチッププロパティ.消音グループ種別, BGM以外も再生する: true );
                                    }
                                    else
                                    {
                                        // (A-b) 空打ちチップの指定がないなら、入力に対応する一番最後のチップを検索し、それを発声する。

                                        var chip = this.一番最後のチップを返す( 入力.Type );

                                        if( null != chip )
                                        {
                                            this.チップの発声を行う( chip, true );
                                            break;  // 複数のチップが該当する場合でも、最初のチップの発声のみ行う。
                                        }
                                    }
                                    //----------------
                                    #endregion
                                }
                                else
                                {
                                    #region " (B) 空うちチップマップ未使用の場合（SSTFの場合）"
                                    //----------------
                                    App.ドラムサウンド.発声する( ドラムチッププロパティ.チップ種別, 0, ドラムチッププロパティ.発声前消音, ドラムチッププロパティ.消音グループ種別 );
                                    //----------------
                                    #endregion
                                }
                            }
                        }
                        //----------------
                        #endregion
                    }
                    break;

                case フェーズ.フェードアウト:
                    App.ステージ管理.現在のアイキャッチ.進行描画する( dc );

                    if( App.ステージ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.アイキャッチ.フェーズ.クローズ完了 )
                    {
                        this.現在のフェーズ = フェーズ.確定;
                    }
                    break;

                case フェーズ.確定:
                    break;
            }

            this._システム情報.描画する( dc );
        }


        private bool _初めての進行描画 = true;

        private システム情報 _システム情報 = null;

        private 成績 _結果 = null;

        private 舞台画像 _背景 = null;

        private 画像 _曲名パネル = null;

        private 文字列画像 _曲名画像 = null;

        private 文字列画像 _サブタイトル画像 = null;

        private 演奏パラメータ結果 _演奏パラメータ結果 = null;

        private ランク _ランク = null;

        private SolidColorBrush _黒マスクブラシ = null;

        private SolidColorBrush _プレビュー枠ブラシ = null;

        private readonly Vector3 _プレビュー画像表示位置dpx = new Vector3( 668f, 194f, 0f );

        private readonly Vector3 _プレビュー画像表示サイズdpx = new Vector3( 574f, 574f, 0f );


        private void _プレビュー画像を描画する( DeviceContext1 dc )
        {
            var 選択曲 = App.曲ツリー.フォーカス曲ノード;
            Debug.Assert( null != 選択曲 );

            var preimage = 選択曲.ノード画像 ?? Node.既定のノード画像;
            Debug.Assert( null != preimage );

            // 枠

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                const float 枠の太さdpx = 5f;
                dc.FillRectangle(
                    new RectangleF(
                        this._プレビュー画像表示位置dpx.X - 枠の太さdpx,
                        this._プレビュー画像表示位置dpx.Y - 枠の太さdpx,
                        this._プレビュー画像表示サイズdpx.X + 枠の太さdpx * 2f,
                        this._プレビュー画像表示サイズdpx.Y + 枠の太さdpx * 2f ),
                    this._プレビュー枠ブラシ );
            } );

            // テクスチャは画面中央が (0,0,0) で、Xは右がプラス方向, Yは上がプラス方向, Zは奥がプラス方向+。

            var 変換行列 =
                Matrix.Scaling(
                    this._プレビュー画像表示サイズdpx.X / preimage.サイズ.Width,
                    this._プレビュー画像表示サイズdpx.Y / preimage.サイズ.Height,
                    0f ) *
                Matrix.Translation(
                    グラフィックデバイス.Instance.画面左上dpx.X + this._プレビュー画像表示位置dpx.X + this._プレビュー画像表示サイズdpx.X / 2f,
                    グラフィックデバイス.Instance.画面左上dpx.Y - this._プレビュー画像表示位置dpx.Y - this._プレビュー画像表示サイズdpx.Y / 2f,
                    0f );

            preimage.描画する( 変換行列 );
        }

        private void _曲名を描画する( DeviceContext1 dc )
        {
            var 表示位置dpx = new Vector2( 690f, 820f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = 545f;

            this._曲名画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._曲名画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._曲名画像.画像サイズdpx.Width );
        }

        private void _サブタイトルを描画する( DeviceContext1 dc )
        {
            var 表示位置dpx = new Vector2( 690f, 820f + 60f );

            // 拡大率を計算して描画する。
            float 最大幅dpx = 545f;

            this._サブタイトル画像.描画する(
                dc,
                表示位置dpx.X,
                表示位置dpx.Y,
                X方向拡大率: ( this._サブタイトル画像.画像サイズdpx.Width <= 最大幅dpx ) ? 1f : 最大幅dpx / this._サブタイトル画像.画像サイズdpx.Width );
        }
    }
}
