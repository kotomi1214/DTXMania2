using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK32;

namespace DTXMania.ステージ.演奏
{
    class フェーズパネル : Activity
    {
        /// <summary>
        ///		現在の位置を 開始点:0～1:終了点 で示す。
        /// </summary>
        public float 現在位置
        {
            get
                => this._現在位置;
            set
                => this._現在位置 = Math.Min( Math.Max( 0.0f, value ), 1.0f );
        }


        public フェーズパネル()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this.子Activityを追加する( this._演奏位置カーソル画像 = new テクスチャ( @"$(System)images\演奏\演奏位置カーソル.png" ) );
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\演奏位置カーソル.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._演奏位置カーソルの矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._演奏位置カーソルの矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }

                this._現在位置 = 0.0f;
                this._初めての進行描画 = true;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
            }
        }

        public void 進行描画する( DeviceContext1 dc )
        {
            if( this._初めての進行描画 )
            {
                this._左右三角アニメ用カウンタ = new LoopCounter( 0, 100, 5 );
                this._初めての進行描画 = false;
            }

            var 中央位置dpx = new Vector2( 1308f, 876f - this._現在位置 * 767f );

            var バー矩形 = this._演奏位置カーソルの矩形リスト[ "Bar" ];
            this._演奏位置カーソル画像.描画する(
                中央位置dpx.X - バー矩形.Width / 2f,
                中央位置dpx.Y - バー矩形.Height / 2f,
                転送元矩形: バー矩形 );

            var 左三角矩形 = this._演奏位置カーソルの矩形リスト[ "Left" ];
            this._演奏位置カーソル画像.描画する(
                中央位置dpx.X - 左三角矩形.Width / 2f - this._左右三角アニメ用カウンタ.現在値の割合 * 40f,
                中央位置dpx.Y - 左三角矩形.Height / 2f,
                転送元矩形: 左三角矩形 );

            var 右三角矩形 = this._演奏位置カーソルの矩形リスト[ "Right" ];
            this._演奏位置カーソル画像.描画する(
                中央位置dpx.X - 右三角矩形.Width / 2f + this._左右三角アニメ用カウンタ.現在値の割合 * 40f,
                中央位置dpx.Y - 右三角矩形.Height / 2f,
                転送元矩形: 右三角矩形 );
        }


        private bool _初めての進行描画 = true;

        private float _現在位置 = 0.0f;

        private テクスチャ _演奏位置カーソル画像 = null;

        private Dictionary<string, RectangleF> _演奏位置カーソルの矩形リスト = null;

        private LoopCounter _左右三角アニメ用カウンタ = null;

        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
