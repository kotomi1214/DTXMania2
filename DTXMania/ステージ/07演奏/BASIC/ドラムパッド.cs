using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.ステージ.演奏.BASIC
{
    class ドラムパッド : Activity
    {
        public ドラムパッド()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._パッド絵 = new テクスチャ( @"$(System)images\演奏\ドラムパッド.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\ドラムパッド.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._パッド絵の矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._パッド絵の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._レーンtoパッドContext = new Dictionary<表示レーン種別, パッドContext>();

                foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
                {
                    this._レーンtoパッドContext.Add( lane, new パッドContext() {
                        左上位置dpx = new Vector2(
                            x: レーンフレーム.領域.X + レーンフレーム.現在のレーン配置.表示レーンの左端位置dpx[ lane ],
                            y: 840f ),
                        転送元矩形 = this._パッド絵の矩形リスト[ lane.ToString() ],
                        転送元矩形Flush = this._パッド絵の矩形リスト[ lane.ToString() + "_Flush" ],
                        アニメカウンタ = new Counter(),
                    } );
                }
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._レーンtoパッドContext.Clear();
            }
        }

        public void ヒットする( 表示レーン種別 lane )
        {
            this._レーンtoパッドContext[ lane ].アニメカウンタ.開始する( 0, 100, 1 );
        }

        public void 進行描画する()
        {
            foreach( 表示レーン種別 lane in Enum.GetValues( typeof( 表示レーン種別 ) ) )
            {
                var drumContext = this._レーンtoパッドContext[ lane ];

                // ドラム側アニメーション
                float Yオフセットdpx = 0f;
                float フラッシュ画像の不透明度 = 0f;

                if( drumContext.アニメカウンタ.終了値に達していない )
                {
                    フラッシュ画像の不透明度 = (float) Math.Sin( Math.PI * drumContext.アニメカウンタ.現在値の割合 );    // 0 → 1 → 0
                    Yオフセットdpx = (float) Math.Sin( Math.PI * drumContext.アニメカウンタ.現在値の割合 ) * 18f;     // 0 → 18 → 0
                }

                // ドラムパッド本体表示
                if( 0 < drumContext.転送元矩形.Width && 0 < drumContext.転送元矩形.Height )
                {
                    this._パッド絵.描画する(
                        drumContext.左上位置dpx.X,
                        drumContext.左上位置dpx.Y + Yオフセットdpx,
                        不透明度0to1: 1.0f,
                        転送元矩形: drumContext.転送元矩形 );
                }

                // ドラムフラッシュ表示
                if( 0 < drumContext.転送元矩形Flush.Width && 0 < drumContext.転送元矩形Flush.Height && 0f < フラッシュ画像の不透明度 )
                {
                    this._パッド絵.描画する(
                        drumContext.左上位置dpx.X,
                        drumContext.左上位置dpx.Y + Yオフセットdpx,
                        フラッシュ画像の不透明度,
                        転送元矩形: drumContext.転送元矩形Flush );
                }
            }
        }


        private テクスチャ _パッド絵 = null;

        private Dictionary<string, RectangleF> _パッド絵の矩形リスト = null;

        private struct パッドContext
        {
            public Vector2 左上位置dpx;
            public RectangleF 転送元矩形;
            public RectangleF 転送元矩形Flush;
            public Counter アニメカウンタ;
        };
        private Dictionary<表示レーン種別, パッドContext> _レーンtoパッドContext = null;

        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
