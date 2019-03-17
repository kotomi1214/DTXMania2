using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using FDK32;

namespace DTXMania.ステージ.演奏.EXPERT
{
    class レーンフラッシュ : Activity
    {
        public レーンフラッシュ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._レーンフラッシュ画像 = new テクスチャ( @"$(System)images\演奏\レーンフラッシュEXPERT.png" ) );
                this._レーンフラッシュ画像.加算合成する = true;
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                {
                    var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\レーンフラッシュEXPERT.yaml" );

                    var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                    var deserializer = new YamlDotNet.Serialization.Deserializer();
                    var yamlMap = deserializer.Deserialize<YAMLマップ_レーンフラッシュ>( yaml );

                    this._レーンフラッシュの矩形リスト = new Dictionary<表示レーン種別, RectangleF>();
                    foreach( var kvp in yamlMap.矩形リスト )
                    {
                        if( 4 == kvp.Value.Length )
                            this._レーンフラッシュの矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                    }
                }
                {
                    var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\レーン配置\Expert.yaml" );

                    var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                    var deserializer = new YamlDotNet.Serialization.Deserializer();
                    var yamlMap = deserializer.Deserialize<YAMLマップ_レーン配置>( yaml );

                    this._レーン中央位置Xリスト = new Dictionary<表示レーン種別, float>();
                    foreach( 表示レーン種別 displayLaneType in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                    {
                        this._レーン中央位置Xリスト[ displayLaneType ] = yamlMap.中央位置[ displayLaneType ];
                    }
                }

                this._フラッシュ情報 = new Dictionary<表示レーン種別, Counter>() {
                    { 表示レーン種別.LeftCymbal,  new Counter() },
                    { 表示レーン種別.HiHat,       new Counter() },
                    { 表示レーン種別.Foot,        new Counter() },
                    { 表示レーン種別.Snare,       new Counter() },
                    { 表示レーン種別.Tom1,        new Counter() },
                    { 表示レーン種別.Bass,        new Counter() },
                    { 表示レーン種別.Tom2,        new Counter() },
                    { 表示レーン種別.Tom3,        new Counter() },
                    { 表示レーン種別.RightCymbal, new Counter() },
                };
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 開始する( 表示レーン種別 laneType )
        {
            this._フラッシュ情報[ laneType ].開始する( 0, 10, 15 );
        }

        public void 進行描画する()
        {
            foreach( var kvp in this._フラッシュ情報 )
            {
                var laneType = kvp.Key;

                if( this._フラッシュ情報[ laneType ].終了値に達した )
                    continue;

                var フラッシュ１枚のサイズ = new Size2F( this._レーンフラッシュの矩形リスト[ laneType ].Width, this._レーンフラッシュの矩形リスト[ laneType ].Height );

                float 割合 = this._フラッシュ情報[ laneType ].現在値の割合;   // 0 → 1
                float 横拡大率 = 0.2f + 0.8f * 割合;                          // 0.2 → 1.0
                割合 = (float) Math.Cos( 割合 * Math.PI / 2f );               // 1 → 0（加速しながら）

                for( float y = ( レーンフレーム.領域.Bottom - フラッシュ１枚のサイズ.Height ); y > ( レーンフレーム.領域.Top - フラッシュ１枚のサイズ.Height ); y -= フラッシュ１枚のサイズ.Height - 0.5f )
                {
                    this._レーンフラッシュ画像.描画する(
                        this._レーン中央位置Xリスト[ laneType ] - フラッシュ１枚のサイズ.Width * 横拡大率 / 2f,
                        y,
                        不透明度0to1: 割合 * 0.75f,   // ちょっと暗めに。
                        転送元矩形: this._レーンフラッシュの矩形リスト[ laneType ],
                        X方向拡大率: 横拡大率 );
                }
            }
        }


        private テクスチャ _レーンフラッシュ画像 = null;

        private Dictionary<表示レーン種別, RectangleF> _レーンフラッシュの矩形リスト = null;

        private Dictionary<表示レーン種別, float> _レーン中央位置Xリスト = null;

        private Dictionary<表示レーン種別, Counter> _フラッシュ情報 = null;


        private class YAMLマップ_レーンフラッシュ
        {
            public Dictionary<表示レーン種別, float[]> 矩形リスト { get; set; }
        }

        private class YAMLマップ_レーン配置
        {
            public Dictionary<表示レーン種別, float> 中央位置 { get; set; }
            public Dictionary<表示レーン種別, string> 色 { get; set; }
        }
    }
}
