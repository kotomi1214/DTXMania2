using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using FDK32;

namespace DTXMania.ステージ.演奏.EXPERT
{
    class ドラムキットとヒットバー : Activity
    {
        /// <summary>
        ///		0.0:閉じてる ～ 1.0:開いてる
        /// </summary>
        public float ハイハットの開度 { get; protected set; } = 1f;


        public ドラムキットとヒットバー()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._ドラムキット画像 = new テクスチャ( @"$(System)images\演奏\ドラムキットとヒットバー.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\ドラムキットとヒットバー.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._パーツ画像の矩形リスト = new Dictionary<パーツ, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._パーツ画像の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._パーツ画像の中心位置 = new Dictionary<パーツ, (float X, float Y)>();
                foreach( var kvp in yamlMap.中心位置 )
                {
                    if( 2 == kvp.Value.Length )
                        this._パーツ画像の中心位置[ kvp.Key ] = (kvp.Value[ 0 ], kvp.Value[ 1 ]);
                }

                this._振動パラメータ = new Dictionary<表示レーン種別, 振動パラメータ>();
                foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                    this._振動パラメータ[ lane ] = new 振動パラメータ();
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        /// <summary>
        ///		ベロシティ（開:0～80:閉）に応じたハイハット開度を設定する。
        ///		80超えの指定も可能。
        /// </summary>
        public void ハイハットの開度を設定する( int ベロシティ値 )
        {
            //  ベロシティ80超えはすべて1.0（完全閉じ）とする。
            this.ハイハットの開度 = 1f - ( Math.Min( ベロシティ値, 80 ) / 80f );
        }

        public void ヒットアニメ開始( 表示レーン種別 lane )
        {
            this._振動パラメータ[ lane ] = new 振動パラメータ {
                カウンタ = new Counter( 0, 100, 3 ),
                振動幅 = 0f,
            };
        }

        public void ドラムキットを進行描画する()
        {
            float Bassの振動幅 = 0;

            #region " Bass "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.Bass ].カウンタ;
                Bassの振動幅 = 0f;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) );     // 2 → 0
                    Bassの振動幅 = (float) ( 最大振幅 * Math.Sin( 10.0 * Math.PI * counter.現在値の割合 ) );     // 10周
                }

                this._パーツを描画する( パーツ.Bass, Y方向移動量: Bassの振動幅 );
            }
            //----------------
            #endregion
            #region " LowTom "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.Tom2 ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) );     // 2 → 0
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 15.0 * Math.PI * counter.現在値の割合 ) );           // 15周
                }

                this._パーツを描画する( パーツ.LowTom, Y方向移動量: 振動幅 + Bassの振動幅 );   // Bassと連動
            }
            //----------------
            #endregion
            #region " HiTom "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.Tom1 ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) );     // 2 → 0
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 15.0 * Math.PI * counter.現在値の割合 ) );           // 15周
                }

                this._パーツを描画する( パーツ.HiTom, Y方向移動量: 振動幅 + Bassの振動幅 );   // Bassと連動
            }
            //----------------
            #endregion
            #region " FloorTom "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.Tom3 ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) );     // 2 → 0
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 10.0 * Math.PI * counter.現在値の割合 ) );           // 10周
                }

                this._パーツを描画する( パーツ.FloorTom, Y方向移動量: 振動幅 );
            }
            //----------------
            #endregion
            #region " Snare "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.Snare ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 4.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) );     // 4 → 0
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 17.0 * Math.PI * counter.現在値の割合 ) );           // 17周
                }

                this._パーツを描画する( パーツ.Snare, Y方向移動量: 振動幅 );
            }
            //----------------
            #endregion
            #region " HiHat "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.HiHat ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = ( this.ハイハットの開度 < 0.2f ) ? 1f : (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) ); // 2 → 0, 開度が小さい場合は 1。
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 20.0 * Math.PI * counter.現在値の割合 ) );                                               // 20周
                }

                this._パーツを描画する( パーツ.HiHatBottom );  // Bottom は動かない。
                this._パーツを描画する( パーツ.HiHatTop, Y方向移動量: 振動幅 -20f * this.ハイハットの開度 );
            }
            //----------------
            #endregion
            #region " RightCymbal "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.RightCymbal ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) );   // 2 → 0
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 20.0 * Math.PI * counter.現在値の割合 ) );         // 20周
                }

                this._パーツを描画する( パーツ.RightCymbalStand ); // Standは動かない。
                this._パーツを描画する( パーツ.RightCymbal, Y方向移動量: 振動幅 );
                this._パーツを描画する( パーツ.RightCymbalTop, Y方向移動量: 振動幅 );
            }
            //----------------
            #endregion
            #region " LeftCymbal "
            //----------------
            {
                var counter = this._振動パラメータ[ 表示レーン種別.LeftCymbal ].カウンタ;
                float 振動幅 = 0;

                if( ( null != counter ) && counter.終了値に達していない )
                {
                    float 最大振幅 = (float) ( 2.0 * Math.Cos( ( Math.PI / 2.0 ) * counter.現在値の割合 ) ); // 2 → 0
                    振動幅 = (float) ( 最大振幅 * Math.Sin( 20.0 * Math.PI * counter.現在値の割合 ) );       // 20周
                }

                this._パーツを描画する( パーツ.LeftCymbalStand );  // Standは動かない。
                this._パーツを描画する( パーツ.LeftCymbal, Y方向移動量: 振動幅 );
                this._パーツを描画する( パーツ.LeftCymbalTop, Y方向移動量: 振動幅 );
            }
            //----------------
            #endregion
        }

        public void ヒットバーを進行描画する()
        {
            this._パーツを描画する( パーツ.Bar );
        }


        protected テクスチャ _ドラムキット画像;

        protected Dictionary<パーツ, RectangleF> _パーツ画像の矩形リスト = null;

        protected Dictionary<パーツ, (float X, float Y)> _パーツ画像の中心位置 = null;


        private void _パーツを描画する( パーツ パーツ名, float X方向移動量 = 0f, float Y方向移動量 = 0f )
        {
            var 中心位置 = this._パーツ画像の中心位置[ パーツ名 ];
            var srcRect = this._パーツ画像の矩形リスト[ パーツ名 ];

            this._ドラムキット画像.描画する(
                中心位置.X - srcRect.Width / 2 + X方向移動量,
                中心位置.Y - srcRect.Height / 2 + Y方向移動量,
                転送元矩形: srcRect );
        }


        private class 振動パラメータ
        {
            public Counter カウンタ = null;
            public float 振動幅 = 0f;
        }
        private Dictionary<表示レーン種別, 振動パラメータ> _振動パラメータ = null;

        protected enum パーツ
        {
            LeftCymbalStand,
            LeftCymbal,
            LeftCymbalTop,
            RightCymbalStand,
            RightCymbal,
            RightCymbalTop,
            HiHatBottom,
            HiHatTop,
            Bass,
            Snare,
            HiTom,
            LowTom,
            FloorTom,
            Bar,
        }

        private class YAMLマップ
        {
            public Dictionary<パーツ, float[]> 矩形リスト { get; set; }
            public Dictionary<パーツ, float[]> 中心位置 { get; set; }
        }
    }
}
