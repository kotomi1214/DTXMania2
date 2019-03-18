using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace FDK
{
    /// <summary>
    ///		任意個の文字を並べた一枚のテクスチャ画像と、それぞれの文字領域の矩形リストから、
    ///		文字列を連続するテクスチャ画像で表示する。
    /// </summary>
    public class テクスチャフォント : Activity
    {
        /// <summary>
        ///		コンストラクタ。
        ///		指定された画像ファイルと矩形リストファイルを使って、テクスチャフォントを生成する。
        /// </summary>
        public テクスチャフォント( VariablePath 文字盤の画像ファイルパス, VariablePath 文字盤設定ファイルパス )
        {
            this.子Activityを追加する( this._文字盤 = new テクスチャ( 文字盤の画像ファイルパス ) );

            var yaml = File.ReadAllText( 文字盤設定ファイルパス.変数なしパス );
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

            this._文字盤の矩形リスト = new Dictionary<string, RectangleF>();
            foreach( var kvp in yamlMap.矩形リスト )
            {
                if( 4 == kvp.Value.Length )
                    this._文字盤の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
            }
        }

        /// <param name="文字列全体のワールド変換行列">
        ///		スケーリングは、等倍にした「あと」の拡大縮小率を指定すること。
        ///	</param>
        public void 描画する( string 表示文字列, Matrix 文字列全体のワールド変換行列 )
        {
            if( 表示文字列.Nullまたは空である() )
                return;

            // 有効文字（矩形リストに登録されている文字）の矩形、文字数を抽出し、文字列全体のサイズを計算する。

            var 有効文字矩形リスト =
                from 文字 in 表示文字列
                where ( null != this._文字盤の矩形リスト[ 文字.ToString() ] )
                select this._文字盤の矩形リスト[ 文字.ToString() ];

            int 有効文字数 = 有効文字矩形リスト.Count();
            if( 0 == 有効文字数 )
                return;

            var 文字列全体のサイズ = Size2F.Empty;
            foreach( var 文字矩形 in 有効文字矩形リスト )
            {
                文字列全体のサイズ.Width += 文字矩形.Width;

                // 文字列全体の高さは、最大の文字高に一致。
                if( 文字列全体のサイズ.Height < 文字矩形.Height )
                {
                    文字列全体のサイズ.Height = 文字矩形.Height;
                }
            }

            // 描画する。
            float 左端 = 0f;
            for( int i = 0; i < 有効文字数; i++ )
            {
                var 文字矩形 = 有効文字矩形リスト.ElementAt( i );

                float 中央X = 0f - ( 文字列全体のサイズ.Width / 2f ) + 左端 + ( 文字矩形.Width / 2f );

                var world =
                    Matrix.Translation( 中央X, 0f, 0f ) *
                    文字列全体のワールド変換行列;

                this._文字盤.描画する( world, 転送元矩形: 文字矩形 );

                左端 += 文字矩形.Width;
            }
        }


        private テクスチャ _文字盤 = null;

        private Dictionary<string, RectangleF> _文字盤の矩形リスト = null;


        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
