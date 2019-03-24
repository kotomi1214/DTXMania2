using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania
{
    class パッド矢印 : IDisposable
    {
        public enum 種類 { 上_Tom1, 下_Tom2, 左_Snare, 右_Tom3 };



        // 生成と終了


        public パッド矢印()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._矢印画像 = new 画像( @"$(System)images\パッド矢印.png" );

                var yaml = File.ReadAllText( new VariablePath( @"$(System)images\パッド矢印.yaml" ).変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._矢印の矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._矢印の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._矢印画像?.Dispose();
            }
        }



        // 進行と描画


        public void 描画する( DeviceContext dc, 種類 type, Vector2 中央位置dpx, float 拡大率 = 1f )
        {
            var 矩形 = new RectangleF();

            switch( type )
            {
                case 種類.上_Tom1:  矩形 = this._矢印の矩形リスト[ "Up" ]; break;
                case 種類.下_Tom2:  矩形 = this._矢印の矩形リスト[ "Down" ]; break;
                case 種類.左_Snare: 矩形 = this._矢印の矩形リスト[ "Left" ]; break;
                case 種類.右_Tom3:  矩形 = this._矢印の矩形リスト[ "Right" ]; break;
            }

            var 左上位置dpx = new Vector2( 中央位置dpx.X - 矩形.Width * 拡大率 / 2f, 中央位置dpx.Y - 矩形.Height * 拡大率 / 2f );

            var 変換行列 =
                Matrix3x2.Scaling( 拡大率 ) *
                Matrix3x2.Translation( 左上位置dpx );

            this._矢印画像.描画する( dc, 変換行列, 転送元矩形: 矩形 );
        }



        // private


        private 画像 _矢印画像 = null;

        private Dictionary<string, RectangleF> _矢印の矩形リスト = null;

        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
