using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using FDK32;
using SSTFormat.v4;

namespace DTXMania.ステージ.演奏.EXPERT
{
    class ドラムチップ : Activity
    {
        public ドラムチップ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._ドラムチップ画像 = new テクスチャ( @"$(System)images\演奏\ドラムチップEXPERT.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\ドラムチップEXPERT.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ_ドラムチップ>( yaml );

                this._ドラムチップの矩形リスト = new Dictionary<表示チップ種別, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._ドラムチップの矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }
                this._ドラムチップアニメ = new LoopCounter( 0, 48, 10 );
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        /// <returns>クリアしたらtrueを返す。</returns>
        public bool 進行描画する( レーンフレーム frame, double 現在の演奏時刻sec, ref int 描画開始チップ番号, チップの演奏状態 state, チップ chip, int index, double ヒット判定バーと描画との時間sec, double ヒット判定バーと発声との時間sec, double ヒット判定バーとの距離dpx )
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

            float 大きさ0to1 = 1.0f;

            #region " 音量からチップの大きさを計算する。"
            //----------------
            if( chip.チップ種別 != チップ種別.Snare_Ghost )   // Ghost は対象外
            {
                // 既定音量未満は大きさを小さくするが、既定音量以上は大きさ1.0のままとする。最小は 0.3。
                大きさ0to1 = MathUtil.Clamp( chip.音量 / (float) チップ.既定音量, 0.3f, 1.0f );
            }
            //----------------
            #endregion

            // チップ種別 から、表示レーン種別 と 表示チップ種別 を取得。
            var 表示レーン種別 = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ].表示レーン種別;
            var 表示チップ種別 = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ].表示チップ種別;

            if( ( 表示レーン種別 != 表示レーン種別.Unknown ) &&   // Unknwon ならチップを表示しない。
                ( 表示チップ種別 != 表示チップ種別.Unknown ) )    //
            {
                #region " チップを描画する。"
                //----------------
                switch( chip.チップ種別 )
                {
                    case チップ種別.LeftCrash:
                        this._単画チップを１つ描画する( 表示レーン種別.LeftCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.LeftCymbal ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.HiHat_Close:
                        this._アニメチップを１つ描画する( 表示レーン種別.HiHat, this._ドラムチップの矩形リスト[ 表示チップ種別.HiHat ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.HiHat_HalfOpen:
                        this._アニメチップを１つ描画する( 表示レーン種別.HiHat, this._ドラムチップの矩形リスト[ 表示チップ種別.HiHat ], たて中央位置dpx, 大きさ0to1 );
                        this._単画チップを１つ描画する( 表示レーン種別.Foot, this._ドラムチップの矩形リスト[ 表示チップ種別.HiHat_HalfOpen ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.HiHat_Open:
                        this._アニメチップを１つ描画する( 表示レーン種別.HiHat, this._ドラムチップの矩形リスト[ 表示チップ種別.HiHat ], たて中央位置dpx, 大きさ0to1 );
                        this._単画チップを１つ描画する( 表示レーン種別.Foot, this._ドラムチップの矩形リスト[ 表示チップ種別.HiHat_Open ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.HiHat_Foot:
                        this._単画チップを１つ描画する( 表示レーン種別.Foot, this._ドラムチップの矩形リスト[ 表示チップ種別.Foot ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.Snare:
                        this._アニメチップを１つ描画する( 表示レーン種別.Snare, this._ドラムチップの矩形リスト[ 表示チップ種別.Snare ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Snare_ClosedRim:
                        this._単画チップを１つ描画する( 表示レーン種別.Snare, this._ドラムチップの矩形リスト[ 表示チップ種別.Snare_ClosedRim ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.Snare_OpenRim:
                        this._単画チップを１つ描画する( 表示レーン種別.Snare, this._ドラムチップの矩形リスト[ 表示チップ種別.Snare_OpenRim ], たて中央位置dpx, 大きさ0to1 );
                        //this._単画チップを１つ描画する( 表示レーン種別.Snare, this._ドラムチップの矩形リスト[ 表示チップ種別.Snare ], たて中央位置dpx, 大きさ0to1 );
                        // → ないほうがいいかも。
                        break;

                    case チップ種別.Snare_Ghost:
                        this._単画チップを１つ描画する( 表示レーン種別.Snare, this._ドラムチップの矩形リスト[ 表示チップ種別.Snare_Ghost ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.Bass:
                        this._アニメチップを１つ描画する( 表示レーン種別.Bass, this._ドラムチップの矩形リスト[ 表示チップ種別.Bass ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.LeftBass:
                        this._アニメチップを１つ描画する( 表示レーン種別.Bass, this._ドラムチップの矩形リスト[ 表示チップ種別.Bass ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Tom1:
                        this._アニメチップを１つ描画する( 表示レーン種別.Tom1, this._ドラムチップの矩形リスト[ 表示チップ種別.Tom1 ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Tom1_Rim:
                        this._単画チップを１つ描画する( 表示レーン種別.Tom1, this._ドラムチップの矩形リスト[ 表示チップ種別.Tom1_Rim ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.Tom2:
                        this._アニメチップを１つ描画する( 表示レーン種別.Tom2, this._ドラムチップの矩形リスト[ 表示チップ種別.Tom2 ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Tom2_Rim:
                        this._単画チップを１つ描画する( 表示レーン種別.Tom2, this._ドラムチップの矩形リスト[ 表示チップ種別.Tom2_Rim ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.Tom3:
                        this._アニメチップを１つ描画する( 表示レーン種別.Tom3, this._ドラムチップの矩形リスト[ 表示チップ種別.Tom3 ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Tom3_Rim:
                        this._単画チップを１つ描画する( 表示レーン種別.Tom3, this._ドラムチップの矩形リスト[ 表示チップ種別.Tom3_Rim ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.RightCrash:
                        this._単画チップを１つ描画する( 表示レーン種別.RightCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.RightCymbal ], たて中央位置dpx
                            , 大きさ0to1 );
                        break;

                    case チップ種別.China:
                        if( App.ユーザ管理.ログオン中のユーザ.表示レーンの左右.Chinaは左 )
                            this._単画チップを１つ描画する( 表示レーン種別.LeftCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.LeftChina ], たて中央位置dpx, 大きさ0to1 );
                        else
                            this._単画チップを１つ描画する( 表示レーン種別.RightCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.RightChina ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Ride:
                        if( App.ユーザ管理.ログオン中のユーザ.表示レーンの左右.Rideは左 )
                            this._単画チップを１つ描画する( 表示レーン種別.LeftCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.LeftRide ], たて中央位置dpx, 大きさ0to1 );
                        else
                            this._単画チップを１つ描画する( 表示レーン種別.RightCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.RightRide ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Ride_Cup:
                        if( App.ユーザ管理.ログオン中のユーザ.表示レーンの左右.Rideは左 )
                            this._単画チップを１つ描画する( 表示レーン種別.LeftCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.LeftRide_Cup ], たて中央位置dpx, 大きさ0to1 );
                        else
                            this._単画チップを１つ描画する( 表示レーン種別.RightCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.RightRide_Cup ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.Splash:
                        if( App.ユーザ管理.ログオン中のユーザ.表示レーンの左右.Splashは左 )
                            this._単画チップを１つ描画する( 表示レーン種別.LeftCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.LeftSplash ], たて中央位置dpx, 大きさ0to1 );
                        else
                            this._単画チップを１つ描画する( 表示レーン種別.RightCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.RightSplash ], たて中央位置dpx, 大きさ0to1 );
                        break;

                    case チップ種別.LeftCymbal_Mute:
                        this._単画チップを１つ描画する( 表示レーン種別.LeftCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.LeftCymbal_Mute ], たて中央位置dpx, 1.0f );
                        break;

                    case チップ種別.RightCymbal_Mute:
                        this._単画チップを１つ描画する( 表示レーン種別.RightCymbal, this._ドラムチップの矩形リスト[ 表示チップ種別.RightCymbal_Mute ], たて中央位置dpx, 1.0f );
                        break;
                }
                //----------------
                #endregion
            }

            return false;
        }


        private テクスチャ _ドラムチップ画像 = null;

        private Dictionary<表示チップ種別, RectangleF> _ドラムチップの矩形リスト = null;

        private LoopCounter _ドラムチップアニメ = null;

        private const float _チップの最終調整倍率 = 1.2f;


        private void _単画チップを１つ描画する( 表示レーン種別 lane, RectangleF 転送元矩形, float 上位置, float 大きさ0to1 )
        {
            float X倍率 = 1f;
            float Y倍率 = 大きさ0to1;

            if( lane == 表示レーン種別.LeftCymbal || lane == 表示レーン種別.RightCymbal )
            {
                // シンバルレーンは大きさの変化をより少なく、さらにX倍率もY倍率と同じにする。
                X倍率 = MathUtil.Clamp( 大きさ0to1 * 2f, 0f, 1f );
                Y倍率 = MathUtil.Clamp( 大きさ0to1 * 2f, 0f, 1f );
            }

            X倍率 *= _チップの最終調整倍率;
            Y倍率 *= _チップの最終調整倍率;

            this._ドラムチップ画像.描画する(
                左位置: レーンフレーム.レーン中央位置X[ lane ] - ( 転送元矩形.Width * X倍率 / 2f ),
                上位置: 上位置 - ( 転送元矩形.Height * Y倍率 / 2f ),
                転送元矩形: 転送元矩形,
                X方向拡大率: X倍率,
                Y方向拡大率: Y倍率 );
        }

        private void _アニメチップを１つ描画する( 表示レーン種別 lane, RectangleF 転送元矩形, float Y, float 大きさ0to1 )
        {
            float X倍率 = 1f * _チップの最終調整倍率;
            float Y倍率 = 大きさ0to1 * _チップの最終調整倍率;

            if( lane == 表示レーン種別.Bass )
                Y倍率 *= 1.2f;    // Bass は縦方向に少し大きめに。

            const float チップ1枚の高さ = 18f;

            転送元矩形.Offset( 0f, this._ドラムチップアニメ.現在値 * 15f );   // 下端3pxは下のチップと共有する前提のデザインなので、18f-3f = 15f。
            転送元矩形.Height = チップ1枚の高さ;

            this._ドラムチップ画像.描画する(
                左位置: レーンフレーム.レーン中央位置X[ lane ] - ( 転送元矩形.Width * X倍率 / 2f ),
                上位置: Y - ( チップ1枚の高さ * Y倍率 / 2f ),
                転送元矩形: 転送元矩形,
                X方向拡大率: X倍率,
                Y方向拡大率: Y倍率 );
        }


        private class YAMLマップ_ドラムチップ
        {
            public Dictionary<表示チップ種別, float[]> 矩形リスト { get; set; }
        }
    }
}
