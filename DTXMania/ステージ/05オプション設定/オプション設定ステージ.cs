﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.オプション設定
{
    class オプション設定ステージ : ステージ
    {
        public enum フェーズ
        {
            フェードイン,
            表示,
            入力割り当て,
			曲読み込みフォルダ割り当て,
			再起動,
            再起動待ち,
			フェードアウト,
            完了,
            キャンセル,
        }

        public フェーズ 現在のフェーズ { get; protected set; } = フェーズ.完了;



        // 生成と終了


        public オプション設定ステージ()
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
                this._パネルリスト = new パネルリスト();
                this._システム情報 = new システム情報();

                this.現在のフェーズ = フェーズ.フェードイン;


                // パネルフォルダツリーを構築する。

                var user = App進行描画.ユーザ管理.ログオン中のユーザ;
                this._ルートパネルフォルダ = new パネル_フォルダ( "Root", null );

                // ツリー構築・オプションパート（ユーザ別）

                #region "「自動演奏」フォルダ"
                //----------------
                {
                    var 自動演奏フォルダ = new パネル_フォルダ(

                        パネル名:
                            "自動演奏",

                        親パネル:
                            this._ルートパネルフォルダ,

                        値の変更処理:
                            ( panel ) => {
                                this._パネルリスト.子のパネルを選択する();
                                this._パネルリスト.フェードインを開始する();
                            } );

                    this._ルートパネルフォルダ.子パネルリスト.Add( 自動演奏フォルダ );

                    // 子フォルダツリーの構築

                    #region "「すべてON/OFF」パネル "
                    //----------------
                    自動演奏フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "すべてON/OFF",

                            値の変更処理:
                                ( panel ) => {
                                    bool 設定値 = !( this._パネル_自動演奏_ONOFFトグルリスト[ 0 ].ONである );  // 最初の項目値の反対にそろえる
                                    foreach( var typePanel in this._パネル_自動演奏_ONOFFトグルリスト )
                                    {
                                        if( typePanel.ONである != 設定値 )    // 設定値と異なるなら
                                            typePanel.確定キーが入力された(); // ON/OFF反転
                                    }
                                }
                        ) );
                    //----------------
                    #endregion
                    #region " 各パッドのON/OFFパネル "
                    //----------------
                    this._パネル_自動演奏_ONOFFトグルリスト = new List<パネル_ONOFFトグル>();

                    foreach( AutoPlay種別 apType in Enum.GetValues( typeof( AutoPlay種別 ) ) )
                    {
                        if( apType == AutoPlay種別.Unknown )
                            continue;

                        var typePanel = new パネル_ONOFFトグル(

                            パネル名:
                                apType.ToString(),

                            初期状態はON:
                                ( user.AutoPlay[ apType ] ),

                            値の変更処理:
                                ( panel ) => {
                                    user.AutoPlay[ apType ] = ( (パネル_ONOFFトグル) panel ).ONである;
                                }
                        );

                        自動演奏フォルダ.子パネルリスト.Add( typePanel );

                        this._パネル_自動演奏_ONOFFトグルリスト.Add( typePanel );
                    }
                    //----------------
                    #endregion
                    #region "「設定完了（戻る）」システムボタン
                    //----------------
                    自動演奏フォルダ.子パネルリスト.Add(

                        new パネル_システムボタン(

                            パネル名:
                                "設定完了（戻る）",

                            値の変更処理:
                                ( panel ) => {
                                    this._パネルリスト.親のパネルを選択する();
                                    this._パネルリスト.フェードインを開始する();
                                }
                        ) );
                    //----------------
                    #endregion

                    自動演奏フォルダ.子パネルリスト.SelectFirst();
                }
                //----------------
                #endregion

                #region "「画面モード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "画面モード",

                        選択肢初期値リスト:
                            new[] { "ウィンドウ", "全画面" },

                        初期選択肢番号:
                            ( App進行描画.システム設定.全画面モードである ) ? 1 : 0,

                        値の変更処理:
                            ( panel ) => {
                                App進行描画.システム設定.全画面モードである = ( 1 == ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 );
                                App進行描画.Instance.AppForm.画面モード = App進行描画.システム設定.全画面モードである ? 画面モード.全画面 : 画面モード.ウィンドウ;
                            }
                    ) );
                //----------------
                #endregion
                #region "「譜面スピード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_倍率(

                        パネル名:
                            "譜面スピード",

                        初期値:
                            user.譜面スクロール速度,

                        最小倍率:
                            0.5,

                        最大倍率:
                            8.0,

                        増減量:
                            0.5,

                        値の変更処理:
                            ( panel ) => {
                                user.譜面スクロール速度 = ( (パネル_倍率) panel ).現在値;
                            }
                        ) );
                //----------------
                #endregion
                #region "「演奏中の壁紙表示」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "演奏中の壁紙表示",

                        初期状態はON:
                            user.スコア指定の背景画像を表示する,

                        値の変更処理:
                            ( panel ) => {
                                user.スコア指定の背景画像を表示する = ( (パネル_ONOFFトグル)panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「演奏中の動画表示」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "演奏中の動画表示",

                        初期状態はON:
                            user.演奏中に動画を表示する,

                        値の変更処理:
                            ( panel ) => {
                                user.演奏中に動画を表示する = ( (パネル_ONOFFトグル)panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「演奏中の動画サイズ」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "演奏中の動画サイズ",

                        選択肢初期値リスト:
                            new[] { "全画面", "中央寄せ" },

                        初期選択肢番号:
                            (int)user.動画の表示サイズ,

                        値の変更処理:
                            ( panel ) => {
                                user.動画の表示サイズ = (動画の表示サイズ)( (パネル_文字列リスト)panel ).現在選択されている選択肢の番号;
                            }
                    ) );
                //----------------
                #endregion
                #region "「シンバルフリー」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "シンバルフリー",

                        初期状態はON:
                            user.シンバルフリーモードである,

                        値の変更処理:
                            ( panel ) => {
                                user.シンバルフリーモードである = ( (パネル_ONOFFトグル) panel ).ONである;
                                user.ドラムチッププロパティ管理.反映する( ( user.シンバルフリーモードである ) ? 入力グループプリセット種別.シンバルフリー : 入力グループプリセット種別.基本形 );
                            }
                    ) );
                //----------------
                #endregion
                #region "「Rideの表示位置」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "Rideの表示位置",

                        選択肢初期値リスト:
                            new[] { "左", "右" },

                        初期選択肢番号:
                            ( user.表示レーンの左右.Rideは左 ) ? 0 : 1,

                        値の変更処理:
                            ( panel ) => {
                                user.表示レーンの左右 = new 表示レーンの左右() {
                                    Rideは左 = ( ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 == 0 ),
                                    Chinaは左 = user.表示レーンの左右.Chinaは左,
                                    Splashは左 = user.表示レーンの左右.Splashは左,
                                };
                            }
                    ) );
                //----------------
                #endregion
                #region "「Chinaの表示位置」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "Chinaの表示位置",

                        選択肢初期値リスト:
                            new[] { "左", "右" },

                        初期選択肢番号:
                            ( user.表示レーンの左右.Chinaは左 ) ? 0 : 1,

                        値の変更処理:
                            ( panel ) => {
                                user.表示レーンの左右 = new 表示レーンの左右() {
                                    Rideは左 = user.表示レーンの左右.Rideは左,
                                    Chinaは左 = ( ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 == 0 ),
                                    Splashは左 = user.表示レーンの左右.Splashは左,
                                };
                            }
                    ) );
                //----------------
                #endregion
                #region "「Splashの表示位置」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "Splashの表示位置",

                        選択肢初期値リスト:
                            new[] { "左", "右" },

                        初期選択肢番号:
                            ( user.表示レーンの左右.Splashは左 ) ? 0 : 1,

                        値の変更処理:
                            ( panel ) => {
                                user.表示レーンの左右 = new 表示レーンの左右() {
                                    Rideは左 = user.表示レーンの左右.Rideは左,
                                    Chinaは左 = user.表示レーンの左右.Splashは左,
                                    Splashは左 = ( ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号 == 0 ),
                                };
                            }
                    ) );
                //----------------
                #endregion
                #region "「ドラムサウンド」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "ドラムサウンド",

                        初期状態はON:
                            user.ドラムの音を発声する,

                        値の変更処理:
                            ( panel ) => {
                                user.ドラムの音を発声する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「レーンの透明度」数値ボックス "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_整数(

                        パネル名:
                            "レーンの透明度",

                        最小値:
                            0,

                        最大値:
                            100,

                        初期値:
                            user.レーンの透明度,

                        増加減単位値:
                            5,

                        単位:
                            "%",

                        値の変更処理:
                            ( panel ) => {
                                user.レーンの透明度 = ( (パネル_整数) panel ).現在の値;
                            }
                    ) );
                //----------------
                #endregion
                #region "「演奏スピード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_倍率(

                        パネル名:
                            "演奏スピード",

                        初期値:
                            user.再生速度,

                        最小倍率:
                            0.1,

                        最大倍率:
                            2.0,

                        増減量:
                            0.1,

                        値の変更処理:
                            ( panel ) => {
                                user.再生速度 = ( (パネル_倍率) panel ).現在値;

                                // キャッシュをすべて削除するために、世代を２つ進める。
                                App進行描画.WAVキャッシュレンタル.世代を進める();
                                App進行描画.WAVキャッシュレンタル.世代を進める();
                            }
                        ) );
                //----------------
                #endregion
                #region "「小節線と拍線の表示」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "小節線と拍線の表示",

                        初期状態はON:
                            user.演奏中に小節線と拍線を表示する,

                        値の変更処理:
                            ( panel ) => {
                                user.演奏中に小節線と拍線を表示する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「小節番号の表示」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "小節番号の表示",

                        初期状態はON:
                            user.演奏中に小節番号を表示する,

                        値の変更処理:
                            ( panel ) => {
                                user.演奏中に小節番号を表示する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「判定FAST/SLOW値の表示」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "判定FAST/SLOWの表示",

                        初期状態はON:
                            user.演奏中に判定FastSlowを表示する,

                        値の変更処理:
                            ( panel ) => {
                                user.演奏中に判定FastSlowを表示する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「音量によるサイズ変化」ON/OFFトグル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_ONOFFトグル(

                        パネル名:
                            "音量によるサイズ変化",

                        初期状態はON:
                            user.音量に応じてチップサイズを変更する,

                        値の変更処理:
                            ( panel ) => {
                                user.音量に応じてチップサイズを変更する = ( (パネル_ONOFFトグル) panel ).ONである;
                            }
                    ) );
                //----------------
                #endregion
                #region "「ダークモード」リスト "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_文字列リスト(

                        パネル名:
                            "ダークモード",

                        選択肢初期値リスト:
                            new[] { "OFF", "HALF", "FULL" },

                        初期選択肢番号:
                            (int)user.ダーク,

                        値の変更処理:
                            ( panel ) => {
                                user.ダーク = (ダーク種別) ( (パネル_文字列リスト) panel ).現在選択されている選択肢の番号;
                            }
                    ) );
                //----------------
                #endregion

                // ツリー構築・システム設定パート（全ユーザ共通）

                #region "「判定位置調整」数値ボックス "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_整数(

                        パネル名:
                            "判定位置調整",

                        最小値:
                            -99,

                        最大値:
                            +99,

                        初期値:
                            App進行描画.システム設定.判定位置調整ms,

                        増加減単位値:
                            1,

                        単位:
                            "ms",

                        値の変更処理:
                            ( panel ) => {
                                App進行描画.システム設定.判定位置調整ms = ( (パネル_整数) panel ).現在の値;
                            }
                    ) );
                //----------------
                #endregion
                #region "「入力割り当て」パネル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル(

                        パネル名:
                            "入力割り当て",

                        値の変更処理:
                            ( panel ) => {
                                this.現在のフェーズ = フェーズ.入力割り当て;
                            },

                        ヘッダ色:
                            パネル.ヘッダ色種別.赤
                    ) );
                //----------------
                #endregion
                #region "「曲読み込みフォルダ」パネル "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル(

                        パネル名:
                            "曲読み込みフォルダ",

                        値の変更処理:
                            ( panel ) => {
                                this.現在のフェーズ = フェーズ.曲読み込みフォルダ割り当て;
                            },

                        ヘッダ色:
                            パネル.ヘッダ色種別.赤
                    ) );
                //----------------
                #endregion

                #region "「初期化」フォルダ "
                //----------------
                {
                    var 初期化フォルダ = new パネル_フォルダ(

                        パネル名:
                            "初期化",

                        親パネル:
                            this._ルートパネルフォルダ,

                        値の変更処理:
                            ( panel ) => {
                                this._パネルリスト.子のパネルを選択する();
                                this._パネルリスト.フェードインを開始する();
                            } );

                    this._ルートパネルフォルダ.子パネルリスト.Add( 初期化フォルダ );

                    // 子フォルダツリーの構築

                    #region "「戻る」システムボタン "
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル_システムボタン(

                            パネル名:
                                "戻る",

                            値の変更処理:
                                ( panel ) => {
                                    this._パネルリスト.親のパネルを選択する();
                                    this._パネルリスト.フェードインを開始する();
                                }
                        ) );
                    //----------------
                    #endregion

                    #region "「設定を初期化」パネル"
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "設定を初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    this._システム設定を初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion
                    #region "「曲DBを初期化」パネル"
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "曲DBを初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    this._曲データベースを初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion
                    #region "「ユーザDBを初期化」パネル"
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "ユーザDBを初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    this._ユーザデータベースを初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion
                    #region "「成績DBを初期化」パネル "
                    //----------------
                    初期化フォルダ.子パネルリスト.Add(

                        new パネル(

                            パネル名:
                                "成績DBを初期化",

                            値の変更処理:
                                new Action<パネル>( ( panel ) => {
                                    this._成績データベースを初期化する();
                                    this.現在のフェーズ = フェーズ.再起動;
                                } )
                        ) );
                    //----------------
                    #endregion

                    初期化フォルダ.子パネルリスト.SelectFirst();
                }
                //----------------
                #endregion

                #region "「設定完了」システムボタン "
                //----------------
                this._ルートパネルフォルダ.子パネルリスト.Add(

                    new パネル_システムボタン(

                        パネル名:
                            "設定完了",

                        値の変更処理:
                            ( panel ) => {
                                this._パネルリスト.フェードアウトを開始する();
                                App進行描画.アイキャッチ管理.アイキャッチを選択しクローズする( nameof( シャッター ) );
                                this.現在のフェーズ = フェーズ.フェードアウト;
                            }
                    ) );
                //----------------
                #endregion


                // 最後のパネルを選択。
                this._ルートパネルフォルダ.子パネルリスト.SelectLast();

                // ルートパネルフォルダを最初のツリーとして表示する。
                this._パネルリスト.パネルリストを登録する( this._ルートパネルフォルダ );


                App進行描画.システムサウンド.再生する( システムサウンド種別.オプション設定ステージ_開始音 );

                this._舞台画像.ぼかしと縮小を適用する( 0.5 );

                base.On活性化();
            }
        }

        public override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App進行描画.システム設定.保存する();
                App進行描画.ユーザ管理.ログオン中のユーザ.保存する();

                this._ルートパネルフォルダ = null;

                this._システム情報?.Dispose();
                this._パネルリスト?.Dispose();
                this._舞台画像?.Dispose();

                base.On非活性化();
            }
        }



        // 進行と描画


        public override void 進行する()
        {
            this._システム情報.FPSをカウントしプロパティを更新する();

            App進行描画.入力管理.すべての入力デバイスをポーリングする();

            switch( this.現在のフェーズ )
            {
                case フェーズ.表示:

                    if( App進行描画.入力管理.キャンセルキーが入力された() )
                    {
                        #region " キャンセル "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.取消音 );

                        if( null == this._パネルリスト.現在のパネルフォルダ.親パネル )
                        {
                            // (A) 親ツリーがないならステージをフェードアウトフェーズへ。

                            this._パネルリスト.フェードアウトを開始する();

                            App進行描画.アイキャッチ管理.アイキャッチを選択しクローズする( nameof( 半回転黒フェード ) );

                            this.現在のフェーズ = フェーズ.フェードアウト;
                        }
                        else
                        {
                            // (B) 親ツリーがあるなら親ツリーへ戻る。

                            this._パネルリスト.親のパネルを選択する();

                            this._パネルリスト.フェードインを開始する();
                        }
                        //----------------
                        #endregion
                    }
                    else if( App進行描画.入力管理.上移動キーが入力された() )
                    {
                        #region " 上移動 "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.カーソル移動音 );

                        this._パネルリスト.前のパネルを選択する();
                        //----------------
                        #endregion
                    }
                    else if( App進行描画.入力管理.下移動キーが入力された() )
                    {
                        #region " 下移動 "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.カーソル移動音 );

                        this._パネルリスト.次のパネルを選択する();
                        //----------------
                        #endregion
                    }
                    else if( App進行描画.入力管理.左移動キーが入力された() )
                    {
                        #region " 左移動 "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.変更音 );

                        this._パネルリスト.現在選択中のパネル.左移動キーが入力された();
                        //----------------
                        #endregion
                    }
                    else if( App進行描画.入力管理.右移動キーが入力された() )
                    {
                        #region " 右移動 "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.変更音 );

                        this._パネルリスト.現在選択中のパネル.右移動キーが入力された();
                        //----------------
                        #endregion
                    }
                    else if( App進行描画.入力管理.確定キーが入力された() )
                    {
                        #region " 確定 "
                        //----------------
                        App進行描画.システムサウンド.再生する( システムサウンド種別.変更音 );

                        this._パネルリスト.現在選択中のパネル.確定キーが入力された();
                        //----------------
                        #endregion
                    }
                    break;
            }


            // 画面モードが外部（F11キーなど）で変更されている場合には、それを「画面モード」パネルにも反映する。

            var 画面モード項目 = this._ルートパネルフォルダ.子パネルリスト.Find( ( p ) => ( p.パネル名 == "画面モード" ) ) as パネル_文字列リスト;
            int 選択肢 = ( App進行描画.システム設定.全画面モードである ) ? 1 : 0; // 0:ウィンドウ, 1:全画面

            if( null != 画面モード項目 && 画面モード項目.現在選択されている選択肢の番号 != 選択肢 )
            {
                画面モード項目.現在選択されている選択肢の番号 = 選択肢;
            }
        }

        public override void 描画する()
        {
            this._システム情報.VPSをカウントする();

            var dc = DXResources.Instance.既定のD2D1DeviceContext;
            dc.Transform = DXResources.Instance.拡大行列DPXtoPX;

            this._舞台画像.進行描画する( dc );
            this._パネルリスト.進行描画する( dc, 613f, 0f );

            switch( this.現在のフェーズ )
            {
                case フェーズ.フェードイン:
                    this._パネルリスト.フェードインを開始する();
                    this.現在のフェーズ = フェーズ.表示;
                    break;

                case フェーズ.入力割り当て:
                    {
                        var 完了通知 = new ManualResetEvent( false );

                        App進行描画.Instance.AppForm.BeginInvoke( new Action( () => {

                            using( var dlg = new 入力割り当てダイアログ() )
                            {
                                Cursor.Show();  // いったんマウスカーソル表示

                                dlg.表示する();

                                if( App進行描画.Instance.AppForm.画面モード == 画面モード.全画面 )
                                    Cursor.Hide();  // 全画面ならマウスカーソルを消す。
                            }

                            完了通知.Set();

                        } ) );

                        完了通知.WaitOne();
                    }
                    this._パネルリスト.フェードインを開始する();
                    this.現在のフェーズ = フェーズ.表示;
                    break;

                case フェーズ.曲読み込みフォルダ割り当て:
                    {
                        var 完了通知 = new ManualResetEvent( false );

                        App進行描画.Instance.AppForm.BeginInvoke( new Action( () => {

                            using( var dlg = new 曲読み込みフォルダ割り当てダイアログ( App進行描画.システム設定.曲検索フォルダ ) )
                            {
                                Cursor.Show();  // いったんマウスカーソル表示

                                if( dlg.ShowDialog() == DialogResult.OK &&
                                    dlg.新しい曲検索フォルダリストを取得する( out List<VariablePath> 新フォルダリスト ) )
                                {
                                    // 曲検索フォルダを新しいリストに更新。
                                    App進行描画.システム設定.曲検索フォルダ.Clear();
                                    App進行描画.システム設定.曲検索フォルダ.AddRange( 新フォルダリスト );
                                    App進行描画.システム設定.保存する();

                                    // 再起動へ。
                                    this.現在のフェーズ = フェーズ.再起動;
                                }
                                else
                                {
                                    this.現在のフェーズ = フェーズ.表示;
                                }

                                if( App進行描画.Instance.AppForm.画面モード == 画面モード.全画面 )
                                    Cursor.Hide();  // 全画面ならマウスカーソルを消す。
                            }

                            完了通知.Set();

                        } ) );

                        完了通知.WaitOne();
                    }
                    break;

                case フェーズ.再起動:
                    App進行描画.Instance.AppForm.BeginInvoke( new Action( () => {
                        App進行描画.Instance.AppForm.再起動する();
                    } ) );
                    this.現在のフェーズ = フェーズ.再起動待ち;
                    break;

                case フェーズ.再起動待ち:
                    break;

                case フェーズ.フェードアウト:
                    App進行描画.アイキャッチ管理.現在のアイキャッチ.進行描画する( dc );
                    if( App進行描画.アイキャッチ管理.現在のアイキャッチ.現在のフェーズ == アイキャッチ.フェーズ.クローズ完了 )
                        this.現在のフェーズ = フェーズ.完了;
                    break;
            }

            this._システム情報.描画する( dc );
        }



        // private


        private 舞台画像 _舞台画像 = null;

        private パネルリスト _パネルリスト = null;

        private パネル_フォルダ _ルートパネルフォルダ = null;

        // 以下、コード内で参照が必要になるパネルのホルダ。
        private List<パネル_ONOFFトグル> _パネル_自動演奏_ONOFFトグルリスト = null;

        private システム情報 _システム情報 = null;



        // 初期化


        private void _システム設定を初期化する()
        {
            // ファイルを削除する。

            var vpath = システム設定.システム設定ファイルパス;

            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"システム設定ファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }


            // 再生成する。

            App進行描画.システム設定 = システム設定.読み込む(); // ファイルがない場合、新規に作られる
        }

        private void _曲データベースを初期化する()
        {
            // 利用者を終了。

            App進行描画.曲ツリー?.Dispose();


            // ファイルを削除する。

            var vpath = SongDB.曲DBファイルパス;
            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"曲データベースファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }


            // 再構築は各自で。
        }

        private void _ユーザデータベースを初期化する()
        {
            // 利用者を終了する。

            App進行描画.ユーザ管理?.Dispose();


            // ファイルを削除する。

            var vpath = UserDB.DBファイルパス;
            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"ユーザデータベースファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }


            // 再生成する。

            App進行描画.ユーザ管理を再構築する();
            App進行描画.ユーザ管理.ユーザリスト.SelectItem( ( user ) => ( user.ユーザID == "AutoPlayer" ) );  // ひとまずAutoPlayerを選択。
        }

        private void _成績データベースを初期化する()
        {
            // ファイルを削除する。

            var vpath = RecordDB.DBファイルパス;
            try
            {
                File.Delete( vpath.変数なしパス );  // ファイルがない場合には例外は出ない
            }
            catch( Exception e )
            {
                Log.ERROR( $"成績データベースファイルの削除に失敗しました。[{vpath.変数付きパス}][{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
            }
        }
    }
}
