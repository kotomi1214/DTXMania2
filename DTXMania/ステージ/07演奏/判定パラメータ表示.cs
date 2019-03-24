using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.演奏
{
    class 判定パラメータ表示 : IDisposable
    {

        // 生成と終了


        public 判定パラメータ表示()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._パラメータ文字 = new 画像フォント( @"$(System)images\パラメータ文字_小.png", @"$(System)images\パラメータ文字_小.yaml" );
                this._判定種別文字 = new 画像( @"$(System)images\演奏\パラメータ用判定種別文字.png" );

                var 設定ファイルパス = new VariablePath( @"$(System)images\演奏\パラメータ用判定種別文字.yaml" );

                var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                this._判定種別文字の矩形リスト = new Dictionary<string, RectangleF>();
                foreach( var kvp in yamlMap.矩形リスト )
                {
                    if( 4 == kvp.Value.Length )
                        this._判定種別文字の矩形リスト[ kvp.Key ] = new RectangleF( kvp.Value[ 0 ], kvp.Value[ 1 ], kvp.Value[ 2 ], kvp.Value[ 3 ] );
                }
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._パラメータ文字?.Dispose();
                this._判定種別文字?.Dispose();
            }
        }



        // 進行と描画


        public virtual void 描画する( DeviceContext dc, float x, float y, 成績 現在の成績 )
        {
            var scaling = new Size2F( 1.0f, 1.4f );

            var 判定toヒット数 = 現在の成績.判定toヒット数;
            var 割合表 = 現在の成績.判定toヒット割合;
            int MaxCombo = 現在の成績.MaxCombo;
            int 合計 = 0;

            this.パラメータを一行描画する( dc, x, y, scaling, 判定種別.PERFECT, 判定toヒット数[ 判定種別.PERFECT ], 割合表[ 判定種別.PERFECT ] );

            合計 += 判定toヒット数[ 判定種別.PERFECT ];
            y += _改行幅dpx;

            this.パラメータを一行描画する( dc, x, y, scaling, 判定種別.GREAT, 判定toヒット数[ 判定種別.GREAT ], 割合表[ 判定種別.GREAT ] );

            合計 += 判定toヒット数[ 判定種別.GREAT ];
            y += _改行幅dpx;

            this.パラメータを一行描画する( dc, x, y, scaling, 判定種別.GOOD, 判定toヒット数[ 判定種別.GOOD ], 割合表[ 判定種別.GOOD ] );

            合計 += 判定toヒット数[ 判定種別.GOOD ];
            y += _改行幅dpx;

            this.パラメータを一行描画する( dc, x, y, scaling, 判定種別.OK, 判定toヒット数[ 判定種別.OK ], 割合表[ 判定種別.OK ] );

            合計 += 判定toヒット数[ 判定種別.OK ];
            y += _改行幅dpx;

            this.パラメータを一行描画する( dc, x, y, scaling, 判定種別.MISS, 判定toヒット数[ 判定種別.MISS ], 割合表[ 判定種別.MISS ] );

            合計 += 判定toヒット数[ 判定種別.MISS ];
            y += _改行幅dpx;


            y += 3f;    // ちょっと間を開けて


            var 矩形 = this._判定種別文字の矩形リスト[ "MaxCombo" ];

            this._判定種別文字.描画する( dc, x, y, 転送元矩形: 矩形, X方向拡大率: scaling.Width, Y方向拡大率: scaling.Height );

            x += 矩形.Width + 16f;

            this.数値を描画する( dc, x, y, scaling, MaxCombo, 桁数: 4 );
            this.数値を描画する( dc, x + _dr, y, scaling, (int) Math.Floor( 100.0 * MaxCombo / 合計 ), 桁数: 3 );    // 切り捨てでいいやもう
            this._パラメータ文字.描画する( dc, x + _dp, y, "%", scaling );
        }

        public void パラメータを一行描画する( DeviceContext dc, float x, float y, Size2F 拡大率, 判定種別 judge, int ヒット数, int ヒット割合, float 不透明度 = 1.0f )
        {
            var 矩形 = this._判定種別文字の矩形リスト[ judge.ToString() ];
            this._判定種別文字.描画する( dc, x, y - 4f, 不透明度, 転送元矩形: 矩形, X方向拡大率: 拡大率.Width, Y方向拡大率: 拡大率.Height );
            x += 矩形.Width + 16f;

            this.数値を描画する( dc, x, y, 拡大率, ヒット数, 4, 不透明度 );
            this.数値を描画する( dc, x + _dr * 拡大率.Width, y, 拡大率, ヒット割合, 3, 不透明度 );
            this._パラメータ文字.不透明度 = 不透明度;
            this._パラメータ文字.描画する( dc, x + _dp * 拡大率.Width, y, "%", 拡大率 );
        }

        protected void 数値を描画する( DeviceContext dc, float x, float y, Size2F 拡大率, int 描画する数値, int 桁数, float 不透明度 = 1.0f )
        {
            Debug.Assert( 1 <= 桁数 && 10 >= 桁数 );    // 最大10桁まで

            int 最大値 = (int) Math.Pow( 10, 桁数 ) - 1;     // 1桁なら9, 2桁なら99, 3桁なら999, ... でカンスト。
            int 判定数 = Math.Max( Math.Min( 描画する数値, 最大値 ), 0 );   // 丸める。
            var 判定数文字列 = 判定数.ToString().PadLeft( 桁数 ).Replace( ' ', 'o' );  // グレーの '0' は 'o' で描画できる（矩形リスト参照）。

            this._パラメータ文字.不透明度 = 不透明度;
            this._パラメータ文字.描画する( dc, x, y, 判定数文字列, 拡大率 );
        }



        // private


        protected const float _dr = 78f;       // 割合(%)までのXオフセット[dpx]

        protected const float _dp = 131f;      // "%" 文字までのXオフセット[dpx]

        protected const float _改行幅dpx = 40f;

        protected 画像フォント _パラメータ文字 = null;

        protected 画像 _判定種別文字 = null;

        protected Dictionary<string, RectangleF> _判定種別文字の矩形リスト = null;


        private class YAMLマップ
        {
            public Dictionary<string, float[]> 矩形リスト { get; set; }
        }
    }
}
