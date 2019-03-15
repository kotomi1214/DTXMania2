using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;
using SSTFormat.v4;
using DTXMania.設定;
using DTXMania.データベース.ユーザ;
using DTXMania.入力;

namespace DTXMania.ステージ.演奏
{
    /// <summary>
    ///     ビュアーモードでは、SSTFファイルのみ再生可能。
    /// </summary>
    class 演奏ステージ_ビュアーモード : ステージ
    {
        public const float ヒット判定位置Ydpx = 847f;


        public enum フェーズ
        {
            表示,
            クリア,
        }

        public フェーズ 現在のフェーズ { get; protected set; }

        public 成績 成績 { get; protected set; } = null;


        public 演奏ステージ_ビュアーモード()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._背景画像 = new 画像( @"$(System)images\演奏\演奏画面.png" ) );
                this.子Activityを追加する( this._レーンフレームBASIC = new BASIC.レーンフレーム() );
                this.子Activityを追加する( this._レーンフレームEXPERT = new EXPERT.レーンフレーム() );
                this.子Activityを追加する( this._曲名パネル = new 曲名パネル() );
                this.子Activityを追加する( this._ドラムパッドBASIC = new BASIC.ドラムパッド() );
                this.子Activityを追加する( this._ヒットバーBASIC = new BASIC.ヒットバー() );
                this.子Activityを追加する( this._ドラムキットとヒットバーEXPERT = new EXPERT.ドラムキットとヒットバー() );
                this.子Activityを追加する( this._レーンフラッシュBASIC = new BASIC.レーンフラッシュ() );
                this.子Activityを追加する( this._レーンフラッシュEXPERT = new EXPERT.レーンフラッシュ() );
                this.子Activityを追加する( this._ドラムチップBASIC = new BASIC.ドラムチップ() );
                this.子Activityを追加する( this._ドラムチップEXPERT = new EXPERT.ドラムチップ() );
                this.子Activityを追加する( this._判定文字列 = new 判定文字列() );
                this.子Activityを追加する( this._チップ光 = new チップ光() );
                this.子Activityを追加する( this._左サイドクリアパネル = new 左サイドクリアパネル() );
                this.子Activityを追加する( this._右サイドクリアパネル = new 右サイドクリアパネル() );
                this.子Activityを追加する( this._判定パラメータ表示 = new 判定パラメータ表示() );
                this.子Activityを追加する( this._フェーズパネル = new フェーズパネル() );
                this.子Activityを追加する( this._コンボ表示 = new コンボ表示() );
                this.子Activityを追加する( this._カウントマップライン = new カウントマップライン() );
                this.子Activityを追加する( this._スコア表示 = new スコア表示() );
                this.子Activityを追加する( this._プレイヤー名表示 = new プレイヤー名表示() );
                this.子Activityを追加する( this._譜面スクロール速度 = new 譜面スクロール速度() );
                this.子Activityを追加する( this._達成率表示 = new 達成率表示() );
                this.子Activityを追加する( this._曲別SKILL = new 曲別SKILL() );
                this.子Activityを追加する( this._エキサイトゲージ = new エキサイトゲージ() );
                this.子Activityを追加する( this._FPS = new FPS() );
                this.子Activityを追加する( this._数字フォント中グレー48x64 = new 画像フォント(
                   @"$(System)images\数字フォント中ホワイト48x64.png",
                   @"$(System)images\数字フォント中48x64矩形リスト.yaml",
                   文字幅補正dpx: -16f,
                   不透明度: 0.3f ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._小節線色 = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, Color.White );
                this._小節線影色 = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, Color.Blue );
                this._拍線色 = new SolidColorBrush( グラフィックデバイス.Instance.D2DDeviceContext, Color.LightGray );
                this._プレイヤー名表示.名前 = App.ユーザ管理.ログオン中のユーザ.ユーザ名;
                BASIC.レーンフレーム.レーン配置を設定する( App.ユーザ管理.ログオン中のユーザ.レーン配置 );
                //this._フェードインカウンタ = new Counter( 0, 100, 10 );

                //this._演奏状態を初期化する();

                // マウスクリックイベント登録
                Program.App.MouseDown += this.App_MouseDown;

                this._ビュアーモード時にドラムサウンドを発声する = true;
                this.現在のフェーズ = フェーズ.クリア;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // マウスクリックイベント登録解除
                Program.App.MouseDown -= this.App_MouseDown;

                this._演奏状態を終了する();

                #region " 現在の譜面スクロール速度をDBに保存。"
                //----------------
                using( var userdb = new UserDB() )
                {
                    var user = userdb.Users.Where( ( r ) => ( r.Id == App.ユーザ管理.ログオン中のユーザ.ユーザID ) ).SingleOrDefault();
                    if( null != user )
                    {
                        user.ScrollSpeed = App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度;
                        userdb.DataContext.SubmitChanges();
                        Log.Info( $"現在の譜面スクロール速度({App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度})をDBに保存しました。[{user}]" );
                    }
                }
                //----------------
                #endregion

                this._拍線色?.Dispose();
                this._拍線色 = null;

                this._小節線影色?.Dispose();
                this._小節線影色 = null;

                this._小節線色?.Dispose();
                this._小節線色 = null;
            }
        }

        /// <summary>
        ///     <see cref="App.演奏スコア"/> に対して、ステージを初期化する。
        /// </summary>
        private void _演奏状態を初期化する()
        {
            // スコアに依存するデータを初期化する。

            this.成績 = new 成績();
            this.成績.スコアと設定を反映する( App.演奏スコア, App.ユーザ管理.ログオン中のユーザ );

            this._カウントマップライン.非活性化する();
            this._カウントマップライン.活性化する();

            this._描画開始チップ番号 = -1;

            this._チップの演奏状態 = new Dictionary<チップ, チップの演奏状態>();
            foreach( var chip in App.演奏スコア.チップリスト )
                this._チップの演奏状態.Add( chip, new チップの演奏状態( chip ) );


            // WAVを生成する。

            App.WAVキャッシュレンタル.世代を進める();

            App.WAV管理?.Dispose();
            App.WAV管理 = new 曲.WAV管理();

            foreach( var kvp in App.演奏スコア.WAVリスト )
            {
                var wavInfo = kvp.Value;

                var path = Path.Combine( App.演奏スコア.PATH_WAV, wavInfo.ファイルパス );
                App.WAV管理.登録する( App.サウンドデバイス, kvp.Key, path, wavInfo.多重再生する, wavInfo.BGMである );
            }


            // AVIを生成する。

            App.AVI管理?.Dispose();
            App.AVI管理 = null;

            if( App.ユーザ管理.ログオン中のユーザ.演奏中に動画を表示する )
            {
                App.AVI管理 = new 曲.AVI管理();

                foreach( var kvp in App.演奏スコア.AVIリスト )
                {
                    var path = Path.Combine( App.演奏スコア.PATH_WAV, kvp.Value );
                    App.AVI管理.登録する( kvp.Key, path, App.ユーザ管理.ログオン中のユーザ.再生速度 );
                }
            }

            // その他。

            this._一時停止中 = false;

            this._初めての進行描画 = true;
        }

        private void _演奏状態を終了する()
        {
            this._描画開始チップ番号 = -1;

            App.WAV管理?.Dispose();	
            App.WAV管理 = null;
            App.AVI管理?.Dispose();
            App.AVI管理 = null;
            this.現在のフェーズ = フェーズ.クリア;
        }

        public override void 高速進行する()
        {
            if( this._初めての進行描画 )
            {
                this._初めての進行描画 = false;
            }

            // 高速進行

            this._FPS.FPSをカウントしプロパティを更新する();

            switch( this.現在のフェーズ )
            {
                case フェーズ.クリア:
                    break;

                case フェーズ.表示:

                    // ※注:クリアや失敗の判定は、ここではなく、進行描画側で行っている。

                    double 現在の演奏時刻sec = this._演奏開始からの経過時間secを返す();


                    // AutoPlay 判定

                    #region " 自動ヒット処理。"
                    //----------------
                    this._描画範囲内のすべてのチップに対して( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) => {

                        var ユーザ設定 = App.ユーザ管理.ログオン中のユーザ;
                        var ドラムチッププロパティ = ユーザ設定.ドラムチッププロパティ管理[ chip.チップ種別 ];
                        var AutoPlay = ユーザ設定.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ];

                        bool チップはヒット済みである = this._チップの演奏状態[ chip ].ヒット済みである;
                        bool チップはまだヒットされていない = !( チップはヒット済みである );
                        bool チップはMISSエリアに達している = ( ヒット判定バーと描画との時間sec > ユーザ設定.最大ヒット距離sec[ 判定種別.OK ] );
                        bool チップは描画についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと描画との時間sec );
                        bool チップは発声についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと発声との時間sec );

                        if( チップはまだヒットされていない && チップはMISSエリアに達している )
                        {
                            #region " MISS判定。"
                            //----------------
                            if( AutoPlay && ドラムチッププロパティ.AutoPlayON_Miss判定 )
                            {
                                this._チップのヒット処理を行う(
                                    chip,
                                    判定種別.MISS,
                                    ドラムチッププロパティ.AutoPlayON_自動ヒット_再生,
                                    ドラムチッププロパティ.AutoPlayON_自動ヒット_判定,
                                    ドラムチッププロパティ.AutoPlayON_自動ヒット_非表示,
                                    ヒット判定バーと発声との時間sec );
                                return;
                            }
                            else if( !AutoPlay && ドラムチッププロパティ.AutoPlayOFF_Miss判定 )
                            {
                                this._チップのヒット処理を行う(
                                    chip,
                                    判定種別.MISS,
                                    ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_再生,
                                    ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_判定,
                                    ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_非表示,
                                    ヒット判定バーと発声との時間sec );

                                this.成績.エキサイトゲージを加算する( 判定種別.MISS ); // 手動演奏なら MISS はエキサイトゲージに反映。
                                return;
                            }
                            else
                            {
                                // 通過。
                            }
                            //----------------
                            #endregion
                        }

                        // ヒット処理(1) 発声時刻
                        if( チップは発声についてヒット判定バーを通過した )    // ヒット済みかどうかには関係ない
                        {
                            #region " 自動ヒット判定。"
                            //----------------
                            if( ( AutoPlay && ドラムチッププロパティ.AutoPlayON_自動ヒット_再生 ) ||
                                ( !AutoPlay && ドラムチッププロパティ.AutoPlayOFF_自動ヒット_再生 ) )
                            {
                                // チップの発声がまだなら発声を行う。

                                if( !( this._チップの演奏状態[ chip ].発声済みである ) )
                                {
                                    this.チップの発声を行う( chip, ( ユーザ設定.ドラムの音を発声する && this._ビュアーモード時にドラムサウンドを発声する ) );
                                    this._チップの演奏状態[ chip ].発声済みである = true;
                                }

                            }
                            //----------------
                            #endregion
                        }

                        // ヒット処理(2) 描画時刻
                        if( チップはまだヒットされていない && チップは描画についてヒット判定バーを通過した )
                        {
                            #region " 自動ヒット判定。"
                            //----------------
                            if( AutoPlay && ドラムチッププロパティ.AutoPlayON_自動ヒット )
                            {
                                this._チップのヒット処理を行う(
                                    chip,
                                    判定種別.PERFECT,   // AutoPlay 時は Perfect 扱い。
                                    ドラムチッププロパティ.AutoPlayON_自動ヒット_再生,
                                    ドラムチッププロパティ.AutoPlayON_自動ヒット_判定,
                                    ドラムチッププロパティ.AutoPlayON_自動ヒット_非表示,
                                    ヒット判定バーと発声との時間sec );

                                //this.成績.エキサイトゲージを加算する( 判定種別.PERFECT ); -> エキサイトゲージには反映しない。

                                this._ドラムキットとヒットバーEXPERT.ヒットアニメ開始( ドラムチッププロパティ.表示レーン種別 );

                                return;
                            }
                            else if( !AutoPlay && ドラムチッププロパティ.AutoPlayOFF_自動ヒット )
                            {
                                this._チップのヒット処理を行う(
                                    chip,
                                    判定種別.PERFECT,   // AutoPlay OFF でも自動ヒットする場合は Perfect 扱い。
                                    ドラムチッププロパティ.AutoPlayOFF_自動ヒット_再生,
                                    ドラムチッププロパティ.AutoPlayOFF_自動ヒット_判定,
                                    ドラムチッププロパティ.AutoPlayOFF_自動ヒット_非表示,
                                    ヒット判定バーと発声との時間sec );

                                //this.成績.エキサイトゲージを加算する( 判定種別.PERFECT ); -> エキサイトゲージには反映しない。

                                this._ドラムキットとヒットバーEXPERT.ヒットアニメ開始( ドラムチッププロパティ.表示レーン種別 );

                                return;
                            }
                            else
                            {
                                // 通過。
                            }
                            //----------------
                            #endregion
                        }

                    } );
                    //----------------
                    #endregion


                    // 入力(1) 手動演奏

                    App.入力管理.すべての入力デバイスをポーリングする( 入力履歴を記録する: false );

                    #region " ユーザヒット処理。"
                    //----------------
                    /*
                    {
                        var ヒット処理済み入力 = new List<ドラム入力イベント>(); // ヒット処理が終わった入力は、二重処理しないよう、この中に追加しておく。

                        var ユーザ設定 = App.ユーザ管理.ログオン中のユーザ;

                        #region " 描画範囲内のすべてのチップについて、対応する入力があればヒット処理を行う。"
                        //----------------
                        this._描画範囲内のすべてのチップに対して( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離 ) => {

                            // チップにヒットしている入力を探す。

                            var ドラムチッププロパティ = ユーザ設定.ドラムチッププロパティ管理[ chip.チップ種別 ];
                            var AutoPlayである = ユーザ設定.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ];

                            bool チップはヒット済みである = this._チップの演奏状態[ chip ].ヒット済みである;
                            bool チップはMISSエリアに達している = ( ヒット判定バーと描画との時間sec > ユーザ設定.最大ヒット距離sec[ 判定種別.OK ] );
                            bool チップは描画についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと描画との時間sec );
                            bool チップは発声についてヒット判定バーを通過した = ( 0 <= ヒット判定バーと発声との時間sec );

                            if( チップはヒット済みである || // ヒット済みなら何もしない。
                                AutoPlayである ||          // AutoPlay チップなので何もしない。
                                !( ドラムチッププロパティ.AutoPlayOFF_ユーザヒット ) ||   // このチップは AutoPlay OFF の時でもユーザヒットの対象ではないので何もしない。
                                !( ヒット判定バーと描画との時間sec >= -( ユーザ設定.最大ヒット距離sec[ 判定種別.OK ] ) && !( チップはMISSエリアに達している ) ) )    // チップはヒット可能エリアの外にあるので何もしない。
                                return;

                            var チップにヒットしている入力 = App.入力管理.ポーリング結果.FirstOrDefault( ( 入力 ) => {

                                if( 入力.InputEvent.離された ||                   // 押下入力じゃないなら無視。
                                    ヒット処理済み入力.Contains( 入力 ) ||         // すでに今回のターンで処理済み（＝処理済み入力リストに追加済み）なら無視。
                                    入力.Type == ドラム入力種別.HiHat_Control )    // HiHat_Control 入力はここでは無視。
                                    return false;

                                var チップの入力グループ = ドラムチッププロパティ.入力グループ種別;

                                // (A) 入力グループ種別 が Unknown の場合 → ドラム入力種別で比較
                                if( チップの入力グループ == 入力グループ種別.Unknown )
                                {
                                    return ( ドラムチッププロパティ.ドラム入力種別 == 入力.Type );
                                }
                                // (B) 入力グループ種別が Unknown ではない場合　→　入力グループ種別で比較
                                else
                                {
                                    var 入力の入力グループ = ユーザ設定.ドラムチッププロパティ管理.チップtoプロパティ.First( ( kvp ) => ( kvp.Value.ドラム入力種別 == 入力.Type ) ).Value.入力グループ種別;

                                    return ( チップの入力グループ == 入力の入力グループ );
                                }

                            } );

                            // チップにヒットした入力があった。

                            if( null != チップにヒットしている入力 )
                            {
                                #region " チップの手動ヒット処理。"
                                //----------------

                                ヒット処理済み入力.Add( チップにヒットしている入力 );    // この入力はこのチップでヒット処理した。

                                // 入力とチップとの時間差を算出。
                                double ヒット判定バーと入力との時間sec = QPCTimer.生カウント相対値を秒へ変換して返す( 現在の演奏時刻qpc - チップにヒットしている入力.InputEvent.TimeStamp );   // 常に正
                                double 入力とチップの間隔sec = Math.Abs( ヒット判定バーと入力との時間sec - ヒット判定バーと描画との時間sec );

                                // 時間差から判定を算出。
                                var 判定 =
                                    ( 入力とチップの間隔sec <= ユーザ設定.最大ヒット距離sec[ 判定種別.PERFECT ] ) ? 判定種別.PERFECT :
                                    ( 入力とチップの間隔sec <= ユーザ設定.最大ヒット距離sec[ 判定種別.GREAT ] ) ? 判定種別.GREAT :
                                    ( 入力とチップの間隔sec <= ユーザ設定.最大ヒット距離sec[ 判定種別.GOOD ] ) ? 判定種別.GOOD : 判定種別.OK;

                                // ヒット処理。
                                this._チップのヒット処理を行う(
                                    chip,
                                    判定,
                                    ( ユーザ設定.ドラムの音を発声する ) ? ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_再生 : false,
                                    ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_判定,
                                    ドラムチッププロパティ.AutoPlayOFF_ユーザヒット_非表示,
                                    ヒット判定バーと発声との時間sec );

                                // エキサイトゲージに反映する。
                                this.成績.エキサイトゲージを加算する( 判定 );

                                //----------------
                                #endregion
                            }

                        } );
                        //----------------
                        #endregion

                        #region " ヒットしてようがしてまいが起こすアクションを実行。"
                        //----------------
                        {
                            var アクション済み入力 = new List<ドラム入力イベント>();  // ヒット処理が終わった入力は、二重処理しないよう、この中に追加しておく。

                            foreach( var 入力 in App.入力管理.ポーリング結果 )
                            {
                                // 押下入力じゃないなら無視。
                                if( 入力.InputEvent.離された )
                                    continue;

                                var プロパティs = ユーザ設定.ドラムチッププロパティ管理.チップtoプロパティ.Where( ( kvp ) => ( kvp.Value.ドラム入力種別 == 入力.Type ) );

                                if( プロパティs.Count() > 0 )
                                {
                                    //for( int i = 0; i < プロパティs.Count(); i++ )
                                    int i = 0;  // １つの入力で処理するのは、１つの表示レーン種別のみ。
                                    {
                                        this._ドラムパッド.ヒットする( プロパティs.ElementAt( i ).Value.表示レーン種別 );
                                        this._レーンフラッシュ.開始する( プロパティs.ElementAt( i ).Value.表示レーン種別 );
                                    }
                                }
                            }
                        }
                        //----------------
                        #endregion

                        #region " どのチップにもヒットしなかった入力は空打ちとみなし、空打ち音を再生する。"
                        //----------------
                        if( ユーザ設定.ドラムの音を発声する )
                        {
                            foreach( var 入力 in App.入力管理.ポーリング結果 )
                            {
                                if( ヒット処理済み入力.Contains( 入力 ) ||   // ヒット済みなら無視。
                                    入力.InputEvent.離された )              // 押下じゃないなら無視。
                                    continue;

                                var プロパティs = ユーザ設定.ドラムチッププロパティ管理.チップtoプロパティ.Where( ( kvp ) => ( kvp.Value.ドラム入力種別 == 入力.Type ) );

                                for( int i = 0; i < プロパティs.Count(); i++ )
                                {
                                    var prop = プロパティs.ElementAt( i ).Value;

                                    int zz = App.演奏スコア.空打ちチップ[ prop.レーン種別 ];

                                    // (A) 空打ちチップの指定があるなら、それを発声する。
                                    if( 0 != zz )
                                        App.WAV管理.発声する( zz, prop.チップ種別, prop.発声前消音, prop.消音グループ種別 );

                                    // (B) 空打ちチップの指定がないなら、一番近いチップを検索し、それを発声する。
                                    else
                                    {
                                        var chip = this._指定された時刻に一番近いチップを返す( 現在の演奏時刻sec, 入力.Type );

                                        if( null != chip )
                                            this._チップの発声を行う( chip, 現在の演奏時刻sec );
                                    }
                                }
                            }
                        }
                        //----------------
                        #endregion

                        ヒット処理済み入力 = null;
                    }
                    */
                    //----------------
                    #endregion


                    // 入力(2) 演奏以外の操作（※演奏中なのでドラム入力は無視。）

                    if( App.入力管理.HIDKeyboard.キーが押された( 0, Keys.Escape ) )
                    {
                        /*
                        #region " ESC → 演奏中断 "
                        //----------------
                        Log.Info( "ESC キーが押されました。演奏を中断します。" );

                        this.BGMを停止する();
                        App.WAV管理.すべての発声を停止する();    // DTXでのBGMサウンドはこっちに含まれる。

                        // 進行描画スレッドへの通知フェーズを挟む。
                        this.現在のフェーズ = フェーズ.キャンセル通知;
                        //----------------
                        #endregion
                        */
                    }
                    if( App.入力管理.HIDKeyboard.キーが押された( 0, Keys.Up ) )
                    {
                        #region " 上 → 譜面スクロールを加速 "
                        //----------------
                        const double 最大倍率 = 8.0;
                        App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Min( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 + 0.5, 最大倍率 );
                        //----------------
                        #endregion
                    }
                    if( App.入力管理.HIDKeyboard.キーが押された( 0, Keys.Down ) )
                    {
                        #region " 下 → 譜面スクロールを減速 "
                        //----------------
                        const double 最小倍率 = 0.5;
                        App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 = Math.Max( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 - 0.5, 最小倍率 );
                        //----------------
                        #endregion
                    }
                    break;
            }
        }

        public override void 進行描画する( DeviceContext1 dc )
        {
            // (1) 進行描画

            if( this._初めての進行描画 )
                return; // まだ最初の高速進行が行われていない。

            switch( this.現在のフェーズ )
            {
                case フェーズ.クリア:
                    this._背景画像.描画する( dc, 0f, 0f );
                    break;

                case フェーズ.表示:
                    {
                        double 演奏時刻sec = this._演奏開始からの経過時間secを返す() + グラフィックデバイス.Instance.次のDComp表示までの残り時間sec;

                        this._譜面スクロール速度.進行する( App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 );  // チップの表示より前に進行だけ行う

                        if( App.ユーザ管理.ログオン中のユーザ.演奏中に動画を表示する )
                        {
                            #region " AVI（動画）の進行描画を行う。"
                            //----------------
                            foreach( var kvp in App.AVI管理.動画リスト )
                            {
                                int zz = kvp.Key;
                                var video = kvp.Value;

                                if( video.再生中 )
                                {
                                    // (A) 75%縮小表示
                                    {
                                        float w = グラフィックデバイス.Instance.設計画面サイズ.Width;
                                        float h = グラフィックデバイス.Instance.設計画面サイズ.Height;

                                        // (1) 画面いっぱいに描画。
                                        video.描画する( dc, new RectangleF( 0f, 0f, w, h ), 0.2f );    // 不透明度は 0.2 で暗くする。

                                        float 拡大縮小率 = 0.75f;
                                        float 上移動 = 100.0f;

                                        // (2) ちょっと縮小して描画。
                                        video.最後のフレームを再描画する( dc, new RectangleF(   // 直前に取得したフレームをそのまま描画。
                                            w * ( 1f - 拡大縮小率 ) / 2f,
                                            h * ( 1f - 拡大縮小率 ) / 2f - 上移動,
                                            w * 拡大縮小率,
                                            h * 拡大縮小率 ) );
                                    }
                                    // (B) 100%全体表示のみ --> 今は未対応
                                    {
                                        //float w = グラフィックデバイス.Instance.設計画面サイズ.Width;
                                        //float h = グラフィックデバイス.Instance.設計画面サイズ.Height;
                                        //video.描画する( dc, new RectangleF( 0f, 0f, w, h ), 0.2f );    // 不透明度は 0.2 で暗くする。
                                    }
                                }
                            }
                            //----------------
                            #endregion
                        }
                        this._左サイドクリアパネル.クリアする();
                        this._左サイドクリアパネル.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                            this._プレイヤー名表示.進行描画する( dcp );
                            this._スコア表示.進行描画する( dcp, グラフィックデバイス.Instance.Animation, new Vector2( +280f, +120f ), this.成績 );
                            this._達成率表示.描画する( dcp, (float) this.成績.Achievement );
                            this._判定パラメータ表示.描画する( dcp, +118f, +372f, this.成績 );
                            this._曲別SKILL.進行描画する( dcp, this.成績.Skill );
                        } );
                        this._左サイドクリアパネル.描画する( dc );

                        this._右サイドクリアパネル.クリアする();
                        this._右サイドクリアパネル.クリアパネル.テクスチャへ描画する( ( dcp ) => {
                            this._コンボ表示.進行描画する( dcp, グラフィックデバイス.Instance.Animation, new Vector2( +228f + 264f / 2f, +234f ), this.成績 );
                        } );
                        this._右サイドクリアパネル.描画する( dc );

                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.BASIC )
                            this._レーンフレームBASIC.描画する( dc, App.ユーザ管理.ログオン中のユーザ.レーンの透明度 );
                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.EXPERT )
                            this._レーンフレームEXPERT.描画する( dc, App.ユーザ管理.ログオン中のユーザ.レーンの透明度 );

                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.BASIC )
                            this._レーンフラッシュBASIC.進行描画する();
                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.EXPERT )
                            this._レーンフラッシュEXPERT.進行描画する();

                        this._小節線拍線を描画する( dc, 演奏時刻sec );

                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.BASIC )
                            this._ドラムパッドBASIC.進行描画する();

                        this._背景画像.描画する( dc, 0f, 0f );

                        this._譜面スクロール速度.描画する( dc, App.ユーザ管理.ログオン中のユーザ.譜面スクロール速度 );

                        this._エキサイトゲージ.進行描画する( dc, this.成績.エキサイトゲージ量 );

                        double 曲の長さsec = App.演奏スコア.チップリスト[ App.演奏スコア.チップリスト.Count - 1 ].描画時刻sec;
                        float 現在位置 = (float) ( 1.0 - ( 曲の長さsec - 演奏時刻sec ) / 曲の長さsec );

                        this._カウントマップライン.カウント値を設定する( 現在位置, this.成績.判定toヒット数 );
                        this._カウントマップライン.進行描画する( dc );

                        this._フェーズパネル.現在位置 = 現在位置;
                        this._フェーズパネル.進行描画する( dc );

                        this._曲名パネル.描画する( dc );

                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.BASIC )
                            this._ヒットバーBASIC.描画する();

                        if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.EXPERT )
                        {
                            this._ドラムキットとヒットバーEXPERT.ヒットバーを進行描画する();
                            this._ドラムキットとヒットバーEXPERT.ドラムキットを進行描画する();
                        }

                        this._描画範囲内のすべてのチップに対して( 演奏時刻sec, ( チップ chip, int index, double ヒット判定バーと描画との時間sec, double ヒット判定バーと発声との時間sec, double ヒット判定バーとの距離dpx ) => {

                            if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.BASIC )
                            {
                                // クリア判定はこの中。
                                if( this._ドラムチップBASIC.進行描画する(
                                        演奏時刻sec, ref this._描画開始チップ番号, this._チップの演奏状態[ chip ],
                                        chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) )
                                {
                                    this.現在のフェーズ = フェーズ.クリア;
                                }
                            }
                            if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.EXPERT )
                            {
                                // クリア判定はこの中。
                                if( this._ドラムチップEXPERT.進行描画する(
                                        this._レーンフレームEXPERT,
                                        演奏時刻sec, ref this._描画開始チップ番号, this._チップの演奏状態[ chip ],
                                        chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) )
                                {
                                    this.現在のフェーズ = フェーズ.クリア;
                                }
                            }

                        } );

                        this._チップ光.進行描画する( dc );

                        this._判定文字列.進行描画する();

                        this._FPS.VPSをカウントする();
                        this._FPS.描画する( dc, 0f, 0f );
                    }
                    break;
            }


            // (2) サービスメッセージ

            {
                var msg = App.サービスメッセージキュー.取得する();

                if( null != msg )
                {
                    switch( msg.種別 )
                    {
                        case API.ServiceMessage.指示種別.指示なし:
                            break;

                        case API.ServiceMessage.指示種別.演奏開始:
                            {
                                this._演奏状態を終了する();

                                App.ビュアー用曲ノード = new 曲.MusicNode( msg.演奏対象曲のファイルパス, null );
                                App.演奏スコア = this._スコアを読み込む();

                                if( null == App.演奏スコア )
                                    return;

                                this._ビュアーモード時にドラムサウンドを発声する = msg.ドラムチップのヒット時に発声する;
                                this._演奏状態を初期化する();

                                this._ビュアー演奏を開始する( msg.演奏を開始する小節番号 );
                                this.現在のフェーズ = フェーズ.表示;

                                this.進行描画する( dc );  // 1フレーム無駄にしないよう、即座に次のフレームに取り掛かる。
                                return;
                            }

                        case API.ServiceMessage.指示種別.演奏停止:
                            this._演奏状態を終了する();
                            break;
                    }
                }
            }
        }

        /// <remarks>
        ///		演奏クリア時には、次の結果ステージに入ってもBGMが鳴り続ける。
        ///		そのため、後からBGMだけを別個に停止するためのメソッドが必要になる。
        /// </remarks>
        public void BGMを停止する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                App.WAV管理.すべての発声を停止する();
            }
        }


        // 画面を構成するもの

        private 画像 _背景画像 = null;

        private 曲名パネル _曲名パネル = null;

        private FPS _FPS = null;

        private BASIC.レーンフレーム _レーンフレームBASIC = null;

        private EXPERT.レーンフレーム _レーンフレームEXPERT = null;

        private BASIC.ヒットバー _ヒットバーBASIC = null;

        private BASIC.ドラムパッド _ドラムパッドBASIC = null;

        private EXPERT.ドラムキットとヒットバー _ドラムキットとヒットバーEXPERT = null;

        private BASIC.ドラムチップ _ドラムチップBASIC = null;

        private EXPERT.ドラムチップ _ドラムチップEXPERT = null;

        private 譜面スクロール速度 _譜面スクロール速度 = null;

        private エキサイトゲージ _エキサイトゲージ = null;

        private フェーズパネル _フェーズパネル = null;

        private カウントマップライン _カウントマップライン = null;

        private 左サイドクリアパネル _左サイドクリアパネル = null;

        private 右サイドクリアパネル _右サイドクリアパネル = null;

        
        // 左サイドクリアパネル内に表示されるもの

        private スコア表示 _スコア表示 = null;

        private プレイヤー名表示 _プレイヤー名表示 = null;

        private 判定パラメータ表示 _判定パラメータ表示 = null;

        private 達成率表示 _達成率表示 = null;

        private 曲別SKILL _曲別SKILL = null;


        // 右サイドクリアパネル内に表示されるもの

        private コンボ表示 _コンボ表示 = null;

        
        // 譜面上に表示されるもの

        private BASIC.レーンフラッシュ _レーンフラッシュBASIC = null;

        private EXPERT.レーンフラッシュ _レーンフラッシュEXPERT = null;

        private 判定文字列 _判定文字列 = null;

        private チップ光 _チップ光 = null;

        private 画像フォント _数字フォント中グレー48x64 = null;

        
        // 小節線・拍線

        private SolidColorBrush _小節線色 = null;

        private SolidColorBrush _小節線影色 = null;

        private SolidColorBrush _拍線色 = null;

        private void _小節線拍線を描画する( DeviceContext1 dc, double 現在の演奏時刻sec )
        {
            // 小節線・拍線 と チップ は描画階層（奥行き）が異なるので、別々のメソッドに分ける。

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {

                this._描画範囲内のすべてのチップに対して( 現在の演奏時刻sec, ( chip, index, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx ) => {

                    if( chip.チップ種別 == チップ種別.小節線 )
                    {
                        float 上位置dpx = (float) ( ヒット判定位置Ydpx + ヒット判定バーとの距離dpx - 1f );   // -1f は小節線の厚みの半分。

                        // 小節線
                        if( App.ユーザ管理.ログオン中のユーザ.演奏中に小節線と拍線を表示する )
                        {
                            float x = 441f;
                            float w = 780f;
                            dc.DrawLine( new Vector2( x, 上位置dpx + 0f ), new Vector2( x + w, 上位置dpx + 0f ), this._小節線色 );
                            dc.DrawLine( new Vector2( x, 上位置dpx + 1f ), new Vector2( x + w, 上位置dpx + 1f ), this._小節線影色 );
                        }

                        // 小節番号
                        if( App.ユーザ管理.ログオン中のユーザ.演奏中に小節番号を表示する )
                        {
                            float 右位置dpx = 441f + 780f - 24f;   // -24f は適当なマージン。
                            this._数字フォント中グレー48x64.描画する( dc, 右位置dpx, 上位置dpx - 84f, chip.小節番号.ToString(), 右揃え: true );    // -84f は適当なマージン。
                        }
                    }
                    else if( chip.チップ種別 == チップ種別.拍線 )
                    {
                        // 拍線
                        if( App.ユーザ管理.ログオン中のユーザ.演奏中に小節線と拍線を表示する )
                        {
                            float 上位置dpx = (float) ( ヒット判定位置Ydpx + ヒット判定バーとの距離dpx - 1f );   // -1f は拍線の厚みの半分。
                            dc.DrawLine( new Vector2( 441f, 上位置dpx ), new Vector2( 441f + 780f, 上位置dpx ), this._拍線色, strokeWidth: 1f );
                        }
                    }

                } );

            } );
        }


        // 演奏状態

        /// <summary>
        ///		<see cref="スコア表示.チップリスト"/> のうち、描画を始めるチップのインデックス番号。
        ///		未演奏時・演奏終了時は -1 。
        /// </summary>
        /// <remarks>
        ///		演奏開始直後は 0 で始まり、対象番号のチップが描画範囲を流れ去るたびに +1 される。
        ///		このメンバの更新は、高頻度進行タスクではなく、進行描画メソッドで行う。（低精度で構わないので）
        /// </remarks>
        private int _描画開始チップ番号 = -1;

        private Dictionary<チップ, チップの演奏状態> _チップの演奏状態 = null;

        private bool _一時停止中 = false;


        private double _演奏開始からの経過時間secを返す()
            => App.サウンドタイマ.現在時刻sec;

        /// <summary>
        ///		<see cref="_描画開始チップ番号"/> から画面上端にはみ出すまでの間の各チップに対して、指定された処理を適用する。
        /// </summary>
        /// <param name="適用する処理">
        ///		引数は、順に、対象のチップ, チップ番号, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx。
        ///		時間と距離はいずれも、負数ならバー未達、0でバー直上、正数でバー通過。
        ///	</param>
        private void _描画範囲内のすべてのチップに対して( double 現在の演奏時刻sec, Action<チップ, int, double, double, double> 適用する処理 )
        {
            var スコア = App.演奏スコア;
            if( null == スコア )
                return;

            for( int i = this._描画開始チップ番号; ( 0 <= i ) && ( i < スコア.チップリスト.Count ); i++ )
            {
                var チップ = スコア.チップリスト[ i ];

                // ヒット判定バーとチップの間の、時間 と 距離 を算出。→ いずれも、負数ならバー未達、0でバー直上、正数でバー通過。
                double ヒット判定バーと描画との時間sec = 現在の演奏時刻sec - チップ.描画時刻sec;
                double ヒット判定バーと発声との時間sec = 現在の演奏時刻sec - チップ.発声時刻sec;
                double 倍率 = this._譜面スクロール速度.補間付き速度;
                double ヒット判定バーとの距離dpx = this._指定された時間secに対応する符号付きピクセル数を返す( 倍率, ヒット判定バーと描画との時間sec );

                // 終了判定。
                bool チップは画面上端より上に出ている = ( ( ヒット判定位置Ydpx + ヒット判定バーとの距離dpx ) < -40.0 );   // -40 はチップが隠れるであろう適当なマージン。
                if( チップは画面上端より上に出ている )
                    break;

                // 処理実行。開始判定（描画開始チップ番号の更新）もこの中で。
                適用する処理( チップ, i, ヒット判定バーと描画との時間sec, ヒット判定バーと発声との時間sec, ヒット判定バーとの距離dpx );
            }
        }

        private double _指定された時間secに対応する符号付きピクセル数を返す( double speed, double 指定時間sec )
        {
            const double _1ミリ秒あたりのピクセル数 = 0.14625 * 2.25 * 1000.0;    // これを変えると、speed あたりの速度が変わる。

            return ( 指定時間sec * _1ミリ秒あたりのピクセル数 * speed );
        }

        private void _チップのヒット処理を行う( チップ chip, 判定種別 judge, bool 再生, bool 判定, bool 非表示, double ヒット判定バーと発声との時間sec )
        {
            this._チップの演奏状態[ chip ].ヒット済みである = true;

            if( 再生 && ( judge != 判定種別.MISS ) )
            {
                #region " チップの発声がまだなら行う。"
                //----------------
                // チップの発声時刻は、描画時刻と同じかそれより過去に位置するので、ここに来た時点で未発声なら発声していい。
                // というか発声時刻が過去なのに未発声というならここが最後のチャンスなので、必ず発声しないといけない。
                if( !( this._チップの演奏状態[ chip ].発声済みである ) )
                {
                    this.チップの発声を行う( chip, 再生 );
                    this._チップの演奏状態[ chip ].発声済みである = true;
                }
                //----------------
                #endregion
            }
            if( 判定 )
            {
                #region " チップの判定処理を行う。"
                //----------------
                var 対応表 = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ];

                if( judge != 判定種別.MISS )
                {
                    // MISS以外（PERFECT～OK）
                    this._チップ光.表示を開始する( 対応表.表示レーン種別 );

                    if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.BASIC )
                    {
                        this._ドラムパッドBASIC.ヒットする( 対応表.表示レーン種別 );
                        this._レーンフラッシュBASIC.開始する( 対応表.表示レーン種別 );
                    }
                    if( App.ユーザ管理.ログオン中のユーザ.演奏モード == PlayMode.EXPERT )
                    {
                        this._レーンフラッシュEXPERT.開始する( 対応表.表示レーン種別 );
                    }
                }

                this._判定文字列.表示を開始する( 対応表.表示レーン種別, judge );

                var ドラムチッププロパティ = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理[ chip.チップ種別 ];
                var AutoPlay = App.ユーザ管理.ログオン中のユーザ.AutoPlay[ ドラムチッププロパティ.AutoPlay種別 ];

                this.成績.ヒット数を加算する( judge, AutoPlay );
                //----------------
                #endregion
            }
            if( 非表示 )
            {
                #region " チップを非表示にする。"
                //----------------
                if( judge == 判定種別.MISS )
                {
                    // MISSチップは最後まで表示し続ける。
                }
                else
                {
                    // PERFECT～POOR チップは非表示。
                    this._チップの演奏状態[ chip ].可視 = false;
                }
                //----------------
                #endregion
            }
        }

        private スコア _スコアを読み込む()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 選択曲ファイルパス = App.演奏曲ノード.曲ファイルの絶対パス;

                // 曲ファイルを読み込む。
                var score = スコア.ファイルから生成する( 選択曲ファイルパス.変数なしパス );

                // 全チップの発声時刻を修正する。
                foreach( var chip in score.チップリスト )
                    chip.発声時刻sec -= App.サウンドデバイス.再生遅延sec;

                // 完了。
                Log.Info( $"曲ファイルを読み込みました。" );
                Log.Info( $"曲名: {score.曲名}" );

                return score;
            }
        }

        // 演奏状態の初期化が完了していること。
        private void _ビュアー演奏を開始する( int 演奏開始小節番号 )
        {
            var score = App.演奏スコア;


            // 演奏開始小節番号に位置する最初のチップを検索する。

            var チップ番号 = -1;

            for( int i = 0; i < score.チップリスト.Count; i++ )
            {
                if( score.チップリスト[ i ].小節番号 >= 演奏開始小節番号 )
                {
                    チップ番号 = i;
                    break;
                }
            }

            if( -1 == チップ番号 )
                return; // すべて演奏終了している


            // 演奏開始時刻を、最初のチップの描画より少し手前に設定。

            double 演奏開始時刻sec = score.チップリスト[ チップ番号 ].描画時刻sec - 0.5;   // 0.5 秒早く

            if( 0.0 > 演奏開始時刻sec ) 演奏開始時刻sec = 0.0;


            // 演奏開始時刻をもとに、描画開始チップ番号を確定する。

            this._描画開始チップ番号 = -1;

            for( int i = 0; i < score.チップリスト.Count; i++ )
            {
                var chip = score.チップリスト[ i ];

                if( chip.描画時刻sec >= 演奏開始時刻sec )
                {
                    this._描画開始チップ番号 = i;
                    break;
                }

                // 同時に、開始時刻より前のチップはヒット済みとする。

                this._チップの演奏状態[ chip ].ヒット済みの状態にする();
            }


            // 必要なら、AVI動画, WAV音声の途中再生を行う。

            // (1) AVI動画
            if( App.ユーザ管理.ログオン中のユーザ.演奏中に動画を表示する )
            {
                var 動画チップリスト = score.チップリスト.Where( ( chip ) => (
                    this._チップの演奏状態[ chip ].ヒット済みである &&  // ヒット済みの
                    chip.チップ種別 == チップ種別.背景動画 ) );         // 動画チップ

                foreach( var aviChip in 動画チップリスト )
                {
                    if( App.AVI管理.動画リスト.TryGetValue( aviChip.チップサブID, out Video video ) )
                    {
                        video.再生を開始する( 演奏開始時刻sec - aviChip.発声時刻sec );
                    }
                }
            }

            // (2) WAV音声
            var BGMチップリスト = score.チップリスト.Where( ( chip ) => (
                this._チップの演奏状態[ chip ].ヒット済みである &&  // ヒット済みの
                chip.チップ種別 == チップ種別.BGM ) );              // BGMチップ

            foreach( var wavChip in BGMチップリスト )
            {
                var prop = App.ユーザ管理.ログオン中のユーザ.ドラムチッププロパティ管理.チップtoプロパティ[ wavChip.チップ種別 ];

                App.WAV管理.発声する( 
                    wavChip.チップサブID,
                    wavChip.チップ種別, 
                    prop.発声前消音, 
                    prop.消音グループ種別, 
                    BGM以外も再生する: App.ユーザ管理.ログオン中のユーザ.ドラムの音を発声する && this._ビュアーモード時にドラムサウンドを発声する,
                    音量: wavChip.音量 / (float) チップ.最大音量, 演奏開始時刻sec - wavChip.発声時刻sec );
            }

            
            // 全部終わってからタイマリセット。

            App.サウンドタイマ.リセットする( 演奏開始時刻sec );

            return;
        }

        // マウス右クリック　→　一時停止／再開する。
        private void App_MouseDown( object sender, MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Right )
            {
                if( !this._一時停止中 )
                {
                    this._一時停止中 = true;

                    App.サウンドタイマ.一時停止する();

                    App.AVI管理?.再生中の動画をすべて一時停止する();
                    App.WAV管理?.再生中の音声をすべて一時停止する();
                }
                else
                {
                    this._一時停止中 = false;

                    App.AVI管理?.一時停止中の動画をすべて再開する();
                    App.WAV管理?.一時停止中の音声をすべて再開する();

                    App.サウンドタイマ.再開する();
                }
            }
        }


        private bool _初めての進行描画 = true;

        private bool _ビュアーモード時にドラムサウンドを発声する = true;

        private class YAMLマップ_ドラムチップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
            public float 縦方向中央位置 { get; set; }
        }
    }
}
