using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;
using YamlDotNet.Serialization;
using FDK;

namespace DTXMania.演奏.BASIC
{
    /// <summary>
    ///		チップの背景であり、レーン全体を示すフレーム画像。
    /// </summary>
    class レーンフレーム
    {

        // static


        /// <summary>
        ///		画面全体に対する、レーンフレームの表示位置と範囲。
        /// </summary>
        public static RectangleF 領域 => new RectangleF( 445f, 0f, 778f, 938f );

        public static Dictionary<string, レーン配置> レーン配置リスト;


        public class レーン配置
        {
            /// <summary>
            ///     配置名。
            ///     「$(System)images\レーン配置\"配置名".json」ファイルがあれば、それがこのインスタンスの設定ファイルになる。
            /// </summary>
            public string 配置名 { get; set; }

            /// <summary>
            ///		表示レーンの左端X位置を、レーンフレームの左端からの相対位置で示す。
            /// </summary>
            public Dictionary<表示レーン種別, float> 表示レーンの左端位置dpx { get; set; }

            /// <summary>
            ///		表示レーンの幅。
            /// </summary>
            public Dictionary<表示レーン種別, float> 表示レーンの幅dpx { get; set; }

            /// <summary>
            ///		レーンラインの領域。
            ///		<see cref="レーンフレーム.領域"/>.Left からの相対値[dpx]。
            /// </summary>
            public RectangleF[] レーンライン;

            public Color4 レーン色;

            public Color4 レーンライン色;
        }

        public static レーン配置 現在のレーン配置 { get; private set; }



        // 生成と終了(static)


        public static void 初期化する()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // レーン配置フォルダから、レーン配置に適合する設定ファイルを検索。

                var root = new VariablePath( @"$(System)images\演奏\レーン配置" );

                レーン配置リスト = new Dictionary<string, レーン配置>();
                foreach( var fileInfo in new DirectoryInfo( root.変数なしパス ).GetFiles( "Type*.yaml", SearchOption.TopDirectoryOnly ) )
                {
                    var 拡張子なしのファイル名 = Path.GetFileNameWithoutExtension( fileInfo.Name );
                    レーン配置リスト[ 拡張子なしのファイル名] = new レーン配置 { 配置名 = 拡張子なしのファイル名 };
                }


                // 各設定ファイルから設定を読み込む。

                foreach( var kvp in レーン配置リスト )
                {
                    var 設定ファイルパス = new VariablePath( Path.Combine( root.変数なしパス, kvp.Key + ".yaml" ) );

                    try
                    {
                        var yaml = File.ReadAllText( 設定ファイルパス.変数なしパス );
                        var deserializer = new Deserializer();
                        var yamlMap = deserializer.Deserialize<YAMLマップ>( yaml );

                        kvp.Value.表示レーンの左端位置dpx = yamlMap.左端位置;
                        kvp.Value.表示レーンの幅dpx = yamlMap.幅;
                        kvp.Value.レーンライン = new RectangleF[ yamlMap.レーンライン.Length ];
                        for( int i = 0; i < yamlMap.レーンライン.Length; i++ )
                        {
                            if( 4 == yamlMap.レーンライン[ i ].Length )
                                kvp.Value.レーンライン[ i ] = new RectangleF( yamlMap.レーンライン[ i ][ 0 ], yamlMap.レーンライン[ i ][ 1 ], yamlMap.レーンライン[ i ][ 2 ], yamlMap.レーンライン[ i ][ 3 ] );
                        }
                        kvp.Value.レーン色 = new Color4( Convert.ToUInt32( yamlMap.レーン色, 16 ) );
                        kvp.Value.レーンライン色 = new Color4( Convert.ToUInt32( yamlMap.レーンライン色, 16 ) );

                        Log.Info( $"{設定ファイルパス.変数付きパス} ... 完了" );
                    }
                    catch( Exception e )
                    {
                        Log.Info( $"{設定ファイルパス.変数付きパス} ... 失敗 [{Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );
                    }

                }

                レーン配置を設定する( "TypeA" );
            }
        }

        public static void 終了する()
        {
        }

        public static void レーン配置を設定する( string レーン配置名 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( レーン配置リスト.ContainsKey( レーン配置名 ) )
                {
                    現在のレーン配置 = レーン配置リスト[ レーン配置名 ];
                }
                else
                {
                    Log.ERROR( $"指定されたレーン配置名「{レーン配置名}」が存在しません。" );

                    if( 0 < レーン配置リスト.Count )
                    {
                        現在のレーン配置 = レーン配置リスト.ElementAt( 0 ).Value;
                        Log.WARNING( $"既定のレーン配置名「{現在のレーン配置.配置名}」を選択しました。" );
                    }
                    else
                    {
                        throw new Exception( "既定のレーン配置名を選択しようとしましたが、存在しません。" );
                    }
                }

                Log.Info( $"レーン配置「{現在のレーン配置.配置名}」を選択しました。" );
            }
        }



        // 進行と描画


        public void 描画する( DeviceContext dc, int BGAの透明度 )
        {
            DXResources.Instance.D2DBatchDraw( dc, () => {

                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;


                // レーンエリアを描画する。

                var レーン色 = 現在のレーン配置.レーン色;
                レーン色.Alpha *= ( 100 - BGAの透明度 ) / 100.0f;   // BGAの透明度0→100 のとき Alpha×1→×0
                using( var laneBrush = new SolidColorBrush( dc, レーン色 ) )
                {
                    dc.FillRectangle( レーンフレーム.領域, laneBrush );
                }


                // レーンラインを描画する。

                var レーンライン色 = 現在のレーン配置.レーンライン色;
                レーンライン色.Alpha *= ( 100 - BGAの透明度 ) / 100.0f;   // BGAの透明度0→100 のとき Alpha×1→×0
                using( var laneLineBrush = new SolidColorBrush( dc, レーンライン色 ) )
                {
                    for( int i = 0; i < 現在のレーン配置.レーンライン.Length; i++ )
                    {
                        var rc = 現在のレーン配置.レーンライン[ i ];
                        rc.Left += レーンフレーム.領域.Left;
                        rc.Right += レーンフレーム.領域.Left;
                        dc.FillRectangle( rc, laneLineBrush );
                    }
                }

            } );
        }



        // private


        private class YAMLマップ
        {
            public Dictionary<表示レーン種別, float> 左端位置 { get; set; }
            public Dictionary<表示レーン種別, float> 幅 { get; set; }
            public float[][] レーンライン { get; set; }
            public string レーン色 { get; set; }
            public string レーンライン色 { get; set; }
        }
    }
}
