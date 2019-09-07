using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

using User = DTXMania.User13;

namespace DTXMania
{
    /// <summary>
    ///		ユーザ別の設定項目。
    /// </summary>
    /// <remarks>
    ///		全ユーザで共有する項目は<see cref="システム設定"/>で管理すること。
    /// </remarks>
    class ユーザ設定
    {
        /// <summary>
        ///		ユーザID。
        ///		null ならこのインスタンスはどのユーザにも割り当てられていないことを示す。
        /// </summary>
        public string ユーザID => this._User.Id;

        public string ユーザ名 => this._User.Name;

        public double 譜面スクロール速度
        {
            get => this._User.ScrollSpeed;
            set => this._User.ScrollSpeed = value;
        }

        public bool シンバルフリーモードである
        {
            get => ( 0 != this._User.CymbalFree );
            set => this._User.CymbalFree = value ? 1 : 0;
        }

        public bool AutoPlayがすべてONである
        {
            get
            {
                bool すべてON = true;

                foreach( var kvp in this.AutoPlay )
                    すべてON &= kvp.Value;

                return すべてON;
            }
        }

        public HookedDictionary<AutoPlay種別, bool> AutoPlay { get; protected set; } = null;

        /// <summary>
        ///		チップがヒット判定バーから（上または下に）どれだけ離れていると Perfect ～ Ok 判定になるのかの定義。秒単位。
        /// </summary>
        public HookedDictionary<判定種別, double> 最大ヒット距離sec { get; set; } = null;

        public ドラムチッププロパティ管理 ドラムチッププロパティ管理 { get; protected set; } = null;

        public 表示レーンの左右 表示レーンの左右
        {
            get
            {
                return new 表示レーンの左右() {
                    Rideは左 = ( this._User.RideLeft != 0 ),
                    Chinaは左 = ( this._User.ChinaLeft != 0 ),
                    Splashは左 = ( this._User.SplashLeft != 0 ),
                };
            }
            set
            {
                this._User.RideLeft = ( value.Rideは左 ) ? 1 : 0;
                this._User.ChinaLeft = ( value.Chinaは左 ) ? 1 : 0;
                this._User.SplashLeft = ( value.Splashは左 ) ? 1 : 0;
            }
        }

        public bool ドラムの音を発声する
        {
            get => ( 0 != this._User.DrumSound );
            set => this._User.DrumSound = value ? 1 : 0;
        }

        public int レーンの透明度
        {
            get => this._User.LaneTrans;
            set => this._User.LaneTrans = value;
        }

        public bool 演奏中に動画を表示する
        {
            get => ( 0 != this._User.BackgroundMovie );
            set => this._User.BackgroundMovie = value ? 1 : 0;
        }

        public double 再生速度
        {
            get => this._User.PlaySpeed;
            set => this._User.PlaySpeed = value;
        }

        public bool 演奏中に小節線と拍線を表示する
        {
            get => ( 0 != this._User.ShowPartLine );
            set => this._User.ShowPartLine = value ? 1 : 0;
        }

        public bool 演奏中に小節番号を表示する
        {
            get => ( 0 != this._User.ShowPartNumber );
            set => this._User.ShowPartNumber = value ? 1 : 0;
        }

        public bool スコア指定の背景画像を表示する
        {
            get => ( 0 != this._User.ShowScoreWall );
            set => this._User.ShowScoreWall = value ? 1 : 0;
        }

        public 動画の表示サイズ 動画の表示サイズ
        {
            get => (動画の表示サイズ)this._User.BackgroundMovieSize;
            set => this._User.BackgroundMovieSize = (int)value;
        }

        public bool 演奏中に判定FastSlowを表示する
        {
            get => ( 0 != this._User.ShowFastSlow );
            set => this._User.ShowFastSlow = value ? 1 : 0;
        }

        public bool 音量に応じてチップサイズを変更する
        {
            get => ( 0 != this._User.NoteSizeByVolume );
            set => this._User.NoteSizeByVolume = value ? 1 : 0;
        }

        public ダーク種別 ダーク
        {
            get => (ダーク種別) this._User.Dark;
            set => this._User.Dark = (int) value;
        }



        // 生成と終了


        public ユーザ設定()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._User = new User() {
                    Id = null,
                };

                this.ドラムチッププロパティ管理 = new ドラムチッププロパティ管理(
                    new 表示レーンの左右() { Chinaは左 = false, Rideは左 = false, Splashは左 = true },
                    入力グループプリセット種別.基本形 );

                this._Userに依存するメンバを初期化する();
            }
        }

        /// <summary>
        ///		指定したユーザIDをデータベースから検索し、その情報でインスタンスを初期化する。
        ///		検索で見つからなければ、<see cref="ユーザID"/> が null となる。。
        /// </summary>
        public ユーザ設定( string ユーザID )
            : this()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                using( var userdb = new UserDB() )
                {
                    var record = userdb.Users.Where( ( r ) => ( r.Id == ユーザID ) ).SingleOrDefault();

                    if( null != record )
                    {
                        // レコードが存在するなら、その内容を継承する。
                        this._User = record.Clone();
                        this._Userに依存するメンバを初期化する();
                        Log.Info( $"ユーザの設定をデータベースから読み込みました。[{this._User}]" );
                    }
                    else
                    {
                        Log.WARNING( $"ID={ユーザID} のユーザがデータベース上に見つかりませんでした。" );
                    }
                }
            }
        }



        // 作成、保存


        /// <summary>
        ///		指定したユーザ情報を新しいユーザとしてデータベースに登録し、
        ///		その情報で初期化したインスタンスを返す。
        ///		指定したユーザ（と同じユーザIDのユーザ）がすでにデータベースに存在している場合には、null を返す。
        /// </summary>
        public static ユーザ設定 作成する( User user )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                using( var userdb = new UserDB() )
                {
                    var record = userdb.Users.Where( ( r ) => ( r.Id == user.Id ) ).SingleOrDefault();

                    if( null == record )
                    {
                        // (A) データベースに新規追加し、新しいインスタンスを返す。
                        userdb.Users.InsertOnSubmit( user );
                        userdb.DataContext.SubmitChanges();

                        var settings = new ユーザ設定() {
                            _User = user.Clone(),
                        };
                        settings._Userに依存するメンバを初期化する();

                        Log.Info( $"ユーザを新規に作成しました。[{user}]" );
                        return settings;
                    }
                    else
                    {
                        // (B) データベース上にすでに存在している。
                        Log.WARNING( $"指定されたユーザはすでにデータベース上に存在しています。[{user}]" );
                        return null;
                    }
                }
            }
        }

        public void 保存する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                using( var userdb = new UserDB() )
                {
                    // すでにデータベース上にレコードが存在する場合は、いったん削除する。（更新するより手っ取り早いので）
                    var record = userdb.Users.Where( ( r ) => ( r.Id == this.ユーザID ) ).SingleOrDefault();
                    if( null != record )
                        userdb.Users.DeleteOnSubmit( record );

                    // レコードを追加する。
                    userdb.Users.InsertOnSubmit( this._User );
                    userdb.DataContext.SubmitChanges();
                }
            }
        }

        public override string ToString()
            => this._User.ToString();



        // private
        
        private User _User = null;

        private void _Userに依存するメンバを初期化する()
        {
            #region " AutoPlay "
            //----------------
            this.AutoPlay = new HookedDictionary<AutoPlay種別, bool>() {
                { AutoPlay種別.Unknown, true },
                { AutoPlay種別.LeftCrash, ( this._User.AutoPlay_LeftCymbal != 0 ) },
                { AutoPlay種別.HiHat, ( this._User.AutoPlay_HiHat != 0 ) },
                { AutoPlay種別.Foot, ( this._User.AutoPlay_LeftPedal != 0 ) },
                { AutoPlay種別.Snare, ( this._User.AutoPlay_Snare != 0 ) },
                { AutoPlay種別.Bass, ( this._User.AutoPlay_Bass != 0 ) },
                { AutoPlay種別.Tom1, ( this._User.AutoPlay_HighTom != 0 ) },
                { AutoPlay種別.Tom2, ( this._User.AutoPlay_LowTom != 0 ) },
                { AutoPlay種別.Tom3, ( this._User.AutoPlay_FloorTom != 0 ) },
                { AutoPlay種別.RightCrash, ( this._User.AutoPlay_RightCymbal != 0 ) },
            };

            // Dictionary が変更されたらDB用の個別プロパティも変更する。
            this.AutoPlay.get時アクション = null;
            this.AutoPlay.set時アクション = ( type, flag ) => {
                switch( type )
                {
                    case AutoPlay種別.LeftCrash:
                        this._User.AutoPlay_LeftCymbal = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.HiHat:
                        this._User.AutoPlay_HiHat = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.Foot:
                        this._User.AutoPlay_LeftPedal = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.Snare:
                        this._User.AutoPlay_Snare = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.Bass:
                        this._User.AutoPlay_Bass = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.Tom1:
                        this._User.AutoPlay_HighTom = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.Tom2:
                        this._User.AutoPlay_LowTom = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.Tom3:
                        this._User.AutoPlay_FloorTom = flag ? 1 : 0;
                        break;

                    case AutoPlay種別.RightCrash:
                        this._User.AutoPlay_RightCymbal = flag ? 1 : 0;
                        break;
                }
            };
            //----------------
            #endregion
            #region " 最大ヒット距離sec "
            //----------------
            this.最大ヒット距離sec = new HookedDictionary<判定種別, double>() {
                { 判定種別.PERFECT, this._User.MaxRange_Perfect },
                { 判定種別.GREAT, this._User.MaxRange_Great },
                { 判定種別.GOOD, this._User.MaxRange_Good },
                { 判定種別.OK, this._User.MaxRange_Ok },
            };

            this.最大ヒット距離sec.get時アクション = null;
            this.最大ヒット距離sec.set時アクション = ( type, val ) => {
                switch( type )
                {
                    case 判定種別.PERFECT:
                        this._User.MaxRange_Perfect = val;
                        break;

                    case 判定種別.GREAT:
                        this._User.MaxRange_Great = val;
                        break;

                    case 判定種別.GOOD:
                        this._User.MaxRange_Good = val;
                        break;

                    case 判定種別.OK:
                        this._User.MaxRange_Ok = val;
                        break;

                    case 判定種別.MISS:
                        break;
                }
            };
            //----------------
            #endregion
            #region " ドラムチッププロパティ "
            //----------------
            this.ドラムチッププロパティ管理.反映する( this.表示レーンの左右 );
            this.ドラムチッププロパティ管理.反映する( ( this.シンバルフリーモードである ) ? 入力グループプリセット種別.シンバルフリー : 入力グループプリセット種別.基本形 );
            //----------------
            #endregion
        }
    }
}
