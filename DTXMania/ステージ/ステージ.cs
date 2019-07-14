using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;
using SSTFormat.v4;

namespace DTXMania
{
    /// <summary>
    ///     ステージの抽象クラス。
    /// </summary>
    abstract class ステージ : Activity
    {
        
        // 進行と描画


        public abstract void 進行する();

        public abstract void 描画する();



        // ステージ間で共通のメソッド。


        public チップ 指定された時刻に一番近いチップを返す( double 時刻sec, ドラム入力種別 drumType )
        {
            var チップtoプロパティ = App進行描画.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理.チップtoプロパティ;

            var 一番近いチップ = (チップ) null;
            var 一番近いチップの時刻差の絶対値sec = (double) 0.0;

            for( int i = 0; i < App進行描画.演奏スコア.チップリスト.Count; i++ )
            {
                var chip = App進行描画.演奏スコア.チップリスト[ i ];

                if( チップtoプロパティ[ chip.チップ種別 ].ドラム入力種別 != drumType )
                    continue;   // 指定されたドラム入力種別ではないチップは無視。

                if( null != 一番近いチップ )
                {
                    var 今回の時刻差の絶対値sec = Math.Abs( chip.描画時刻sec - 時刻sec );

                    if( 一番近いチップの時刻差の絶対値sec < 今回の時刻差の絶対値sec )
                    {
                        // 時刻差の絶対値が前回より増えた → 前回のチップが指定時刻への再接近だった
                        break;
                    }
                }

                一番近いチップ = chip;
                一番近いチップの時刻差の絶対値sec = Math.Abs( 一番近いチップ.描画時刻sec - 時刻sec );
            }

            return 一番近いチップ;
        }

        public チップ 一番最後のチップを返す( ドラム入力種別 drumType )
        {
            var チップtoプロパティ = App進行描画.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理.チップtoプロパティ;

            // チップリストの後方から先頭に向かって検索。
            for( int i = App進行描画.演奏スコア.チップリスト.Count - 1; i >= 0; i-- )
            {
                var chip = App進行描画.演奏スコア.チップリスト[ i ];

                if( チップtoプロパティ[ chip.チップ種別 ].ドラム入力種別 == drumType )
                    return chip;    // 見つけた
            }

            return null;    // 見つからなかった
        }

        public void チップの発声を行う( チップ chip, bool ドラムサウンドを再生する )
        {
            if( chip.チップ種別 == チップ種別.背景動画 )
            {
                if( App進行描画.ユーザ管理.ログオン中のユーザ.演奏中に動画を表示する )
                {
                    #region " (A) AVI動画を再生する。"
                    //----------------
                    int AVI番号 = chip.チップサブID;

                    if( App進行描画.AVI管理.動画リスト.TryGetValue( AVI番号, out Video video ) )
                    {
                        App進行描画.サウンドタイマ.一時停止する();       // 止めても止めなくてもカクつくだろうが、止めておけば譜面は再開時にワープしない。

                        video.再生を開始する();

                        App進行描画.サウンドタイマ.再開する();
                    }
                    //----------------
                    #endregion
                    return;
                }
            }

            if( 0 == chip.チップサブID && ドラムサウンドを再生する )
            {
                #region " (B) SSTF準拠のドラムサウンドを再生する。"
                //----------------
                var ドラムチッププロパティ = App進行描画.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理.チップtoプロパティ[ chip.チップ種別 ];

                // ドラムサウンドを持つチップなら発声する。（持つかどうかはこのメソッド↓内で判定される。）
                App進行描画.ドラムサウンド.発声する( chip.チップ種別, 0, ドラムチッププロパティ.発声前消音, ドラムチッププロパティ.消音グループ種別, ( chip.音量 / (float) チップ.最大音量 ) );
                //----------------
                #endregion
            }
            else
            {
                #region " (C) WAVサウンドを再生する。"
                //----------------
                int WAV番号 = chip.チップサブID;
                var prop = App進行描画.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理.チップtoプロパティ[ chip.チップ種別 ];

                // WAVを持つチップなら発声する。（持つかどうかはこのメソッド↓内で判定される。）
                App進行描画.WAV管理.発声する( chip.チップサブID, chip.チップ種別, prop.発声前消音, prop.消音グループ種別, BGM以外も再生する: ドラムサウンドを再生する, 音量: ( chip.音量 / (float) チップ.最大音量 ) );
                //----------------
                #endregion
            }
        }
    }
}
