using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using FDK32;

namespace DTXMania.ステージ.演奏.BASIC
{
    class レーンフラッシュ : Activity
    {
        public レーンフラッシュ()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._レーンフラッシュ画像 = new テクスチャ( @"$(System)images\演奏\レーンフラッシュBASIC.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\レーンフラッシュBASIC.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._レーンフラッシュの矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._レーンフラッシュの矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._レーンtoレーンContext = new Dictionary<表示レーン種別, レーンContext>();

                foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                {
                    this._レーンtoレーンContext.Add( lane, new レーンContext() {
                        開始位置dpx = new Vector2(
                            x: レーンフレーム.領域.X + レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[ lane ],
                            y: レーンフレーム.領域.Bottom ),
                        転送元矩形 = this._レーンフラッシュの矩形リスト[ lane.ToString() ],
                        アニメカウンタ = new Counter(),
                    } );
                }
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._レーンtoレーンContext.Clear();
            }
        }

        public void 開始する( 表示レーン種別 lane )
        {
            this._レーンtoレーンContext[ lane ].アニメカウンタ.開始する( 0, 250, 1 );
        }

        public void 進行描画する()
        {
            foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
            {
                var laneContext = this._レーンtoレーンContext[ lane ];
                if( laneContext.アニメカウンタ.動作中である && laneContext.アニメカウンタ.終了値に達していない )
                {
                    this._レーンフラッシュ画像.描画する(
                        laneContext.開始位置dpx.X,
                        laneContext.開始位置dpx.Y - laneContext.アニメカウンタ.現在値の割合 * BASIC.レーンフレーム.領域.Height,
                        不透明度0to1: 1f - laneContext.アニメカウンタ.現在値の割合,
                        転送元矩形: laneContext.転送元矩形 );
                }
            }
        }


        private struct レーンContext
        {
            public Vector2 開始位置dpx;
            public RectangleF 転送元矩形;
            public Counter アニメカウンタ;
        };
        private Dictionary<表示レーン種別, レーンContext> _レーンtoレーンContext = null;

        private テクスチャ _レーンフラッシュ画像 = null;

        private Dictionary<string, RectangleF> _レーンフラッシュの矩形リスト = null;

        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
