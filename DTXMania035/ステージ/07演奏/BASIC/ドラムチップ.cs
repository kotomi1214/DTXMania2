using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK32;
using SSTFormat.v4;
using DTXMania.設定;

namespace DTXMania.ステージ.演奏.BASIC
{
    class ドラムチップ : Activity
    {
        public ドラムチップ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._ドラムチップ画像 = new テクスチャ( @"$(System)images\演奏\ドラムチップBASIC.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\ドラムチップBASIC.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ_ドラムチップ>( yaml );

                this._ドラムチップの矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._ドラムチップの矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }
                this._ドラムチップアニメ = new LoopCounter( 0, 200, 3 );
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        /// <returns>クリアしたらtrueを返す。</returns>
        public bool 進行描画する( double 現在の演奏時刻sec, ref int 描画開始チップ番号, チップの演奏状態 state, チップ chip, int index, double ヒット判定バーと描画との時間sec, double ヒット判定バーと発声との時間sec, double ヒット判定バーとの距離dpx )
        {
            float たて中央位置dpx = (float) ( 演奏ステージ.ヒット判定位置Ydpx + ヒット判定バーとの距離dpx );
            float 消滅割合 = 0f;

            #region " 消滅割合を算出; チップがヒット判定バーを通過したら徐々に消滅する。"
            //----------------
            const float 消滅を開始するヒット判定バーからの距離dpx = 20f;
            const float 消滅開始から完全消滅するまでの距離dpx = 70f;

            if( 消滅を開始するヒット判定バーからの距離dpx < ヒット判定バーとの距離dpx )   // 通過した
            {
                // 通過距離に応じて 0→1の消滅割合を付与する。0で完全表示、1で完全消滅、通過してなければ 0。
                消滅割合 = Math.Min( 1f, (float) ( ( ヒット判定バーとの距離dpx - 消滅を開始するヒット判定バーからの距離dpx ) / 消滅開始から完全消滅するまでの距離dpx ) );
            }
            //----------------
            #endregion

            #region " チップが描画開始チップであり、かつ、そのY座標が画面下端を超えたなら、描画開始チップ番号を更新する。"
            //----------------
            if( ( index == 描画開始チップ番号 ) &&
                ( グラフィックデバイス.Instance.設計画面サイズ.Height + 40.0 < たて中央位置dpx ) )   // +40 はチップが隠れるであろう適当なマージン。
            {
                描画開始チップ番号++;

                // 描画開始チップがチップリストの末尾に到達したら、演奏を終了する。
                if( App.演奏スコア.チップリスト.Count <= 描画開始チップ番号 )
                {
                    描画開始チップ番号 = -1;    // 演奏完了。
                    return true;                // クリア。
                }

                return false;
            }
            //----------------
            #endregion

            if( state.不可視 )
                return false;

            // BASICモードでは、チップの大きさは変化しない。
            float 大きさ0to1 = 1.0f;
        
            // チップ種別 から、表示レーン種別 と 表示チップ種別 を取得。
            var 表示レーン種別 = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ].表示レーン種別;
            var 表示チップ種別 = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ].表示チップ種別;

            if( ( 表示レーン種別 != 表示レーン種別.Unknown ) &&   // Unknwon ならチップを表示しない。
                ( 表示チップ種別 != 表示チップ種別.Unknown ) )    //
            {
                var 左端位置dpx = レーンフレーム.領域.Left + レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[ 表示レーン種別 ];
                var 中央位置Xdpx = 左端位置dpx + レーンフレーム.現在のレーン配置.表示レーンの幅dpx[ 表示レーン種別 ] / 2f;

                #region " チップ背景（あれば）を描画する。"
                //----------------
                {
                    var 矩形 = this._ドラムチップの矩形リスト[ 表示チップ種別.ToString() + "_back" ];

                    if( ( null != 矩形 ) && ( ( 0 < 矩形.Width && 0 < 矩形.Height ) ) )
                    {
                        var 矩形中央 = new Vector2( 矩形.Width / 2f, 矩形.Height / 2f );
                        var アニメ割合 = this._ドラムチップアニメ.現在値の割合;   // 0→1のループ

                        var 変換行列 = ( 0 >= 消滅割合 ) ? Matrix.Identity : Matrix.Scaling( 1f - 消滅割合, 1f, 0f );

                        // 変換(1) 拡大縮小、回転
                        // → 現在は、どの表示チップ種別の背景がどのアニメーションを行うかは、コード内で名指しする（固定）。
                        switch( 表示チップ種別 )
                        {
                            case 表示チップ種別.LeftCymbal:
                            case 表示チップ種別.RightCymbal:
                            case 表示チップ種別.HiHat:
                            case 表示チップ種別.HiHat_Open:
                            case 表示チップ種別.HiHat_HalfOpen:
                            case 表示チップ種別.Foot:
                            case 表示チップ種別.LeftBass:
                            case 表示チップ種別.Tom3:
                            case 表示チップ種別.Tom3_Rim:
                            case 表示チップ種別.LeftRide:
                            case 表示チップ種別.RightRide:
                            case 表示チップ種別.LeftRide_Cup:
                            case 表示チップ種別.RightRide_Cup:
                            case 表示チップ種別.LeftChina:
                            case 表示チップ種別.RightChina:
                            case 表示チップ種別.LeftSplash:
                            case 表示チップ種別.RightSplash:
                                #region " 縦横に伸び縮み "
                                //----------------
                                {
                                    float v = (float) ( Math.Sin( 2 * Math.PI * アニメ割合 ) * 0.2 );    // -0.2～0.2 の振動

                                    変換行列 = 変換行列 * Matrix.Scaling( (float) ( 1 + v ), (float) ( 1 - v ) * 1.0f, 0f );       // チップ背景は大きさを変えない
                                }
                                //----------------
                                #endregion
                                break;

                            case 表示チップ種別.Bass:
                                #region " 左右にゆらゆら回転 "
                                //----------------
                                {
                                    float r = (float) ( Math.Sin( 2 * Math.PI * アニメ割合 ) * 0.2 );    // -0.2～0.2 の振動
                                    変換行列 = 変換行列 *
                                        Matrix.Scaling( 1f, 1f, 0f ) * // チップ背景は大きさを変えない
                                        Matrix.RotationZ( (float) ( r * Math.PI ) );
                                }
                                //----------------
                                #endregion
                                break;
                        }

                        // 変換(2) 移動
                        変換行列 = 変換行列 *
                            Matrix.Translation(
                                グラフィックデバイス.Instance.画面左上dpx.X + 中央位置Xdpx,
                                グラフィックデバイス.Instance.画面左上dpx.Y - たて中央位置dpx,
                                0f );

                        // 描画。
                        //if( 表示チップ種別 != 表示チップ種別.HiHat &&         // 暫定処置：これらでは背景画像を表示しない 
                        //    表示チップ種別 != 表示チップ種別.LeftRide &&      //
                        //    表示チップ種別 != 表示チップ種別.RightRide &&     //
                        //    表示チップ種別 != 表示チップ種別.LeftRide_Cup &&  // 
                        //    表示チップ種別 != 表示チップ種別.RightRide_Cup )
                        {
                            this._ドラムチップ画像.描画する(
                                変換行列,
                                転送元矩形: 矩形 );
                        }
                    }
                }
                //----------------
                #endregion

                #region " チップ本体を描画する。"
                //----------------
                {
                    var 矩形 = this._ドラムチップの矩形リスト[ 表示チップ種別.ToString() ];

                    if( ( null != 矩形 ) && ( ( 0 < 矩形.Width && 0 < 矩形.Height ) ) )
                    {
                        var sx = ( 0.6f + ( 0.4f * 大きさ0to1 ) ) * ( ( 0 >= 消滅割合 ) ? 1f : 1f - 消滅割合 );
                        var sy = 大きさ0to1;

                        var 変換行列 =
                            Matrix.Scaling( sx, sy, 0f ) *
                            Matrix.Translation(
                                グラフィックデバイス.Instance.画面左上dpx.X + 中央位置Xdpx,
                                グラフィックデバイス.Instance.画面左上dpx.Y - たて中央位置dpx,
                                0f );

                        this._ドラムチップ画像.描画する(
                            変換行列,
                            転送元矩形: 矩形 );
                    }
                }
                //----------------
                #endregion
            }

            return false;
        }


        private テクスチャ _ドラムチップ画像 = null;

        private Dictionary<string, RectangleF> _ドラムチップの矩形リスト = null;

        private LoopCounter _ドラムチップアニメ = null;


        private class YAMLマップ_ドラムチップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
