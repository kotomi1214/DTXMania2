using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SharpDX;

namespace FDK
{
    /// <summary>
    ///		任意の文字列から任意の矩形を引き当てるための辞書。
    ///		辞書の内容は、ファイルから読み込むすることができる。
    /// </summary>
    public class 矩形リスト
    {
        public Dictionary<string, RectangleF> 文字列to矩形 { get; } = new Dictionary<string, RectangleF>();

        /// <summary>
        ///		キー（文字列）に対応する矩形を返す。
        /// </summary>
        /// <param name="文字列">キー文字列。</param>
        /// <returns>キー文字列に対応する矩形。Null許容型であり、キーが存在していなければ null を返す。</returns>
        public RectangleF? this[ string 文字列 ] => this.文字列to矩形.ContainsKey( 文字列 ) ? this.文字列to矩形[ 文字列 ] : (RectangleF?) null;


        public 矩形リスト()
        {
        }

        public 矩形リスト( VariablePath ファイルパス )
            : this()
        {
            this.矩形リストXmlファイルを読み込む( ファイルパス );
        }

        public void 矩形リストXmlファイルを読み込む( VariablePath ファイルパス )
        {
            this.文字列to矩形.Clear();

            if( !File.Exists( ファイルパス.変数なしパス ) )
                throw new FileNotFoundException( $"矩形リストXmlファイルが存在しません。[{ファイルパス.変数付きパス}]" );

            try
            {
                var xml文書 = XDocument.Load( ファイルパス.変数なしパス );

                // <Root>
                var Root要素 = xml文書.Element( nameof( XML.Root ) );
                {
                    // <SubImage Name="..." Rectangle="x,y,w,h" />*
                    foreach( var SubImage要素 in Root要素.Elements( nameof( XML.SubImage ) ) )
                    {
                        // Name 属性を取得。なかったら例外発出。
                        var Name属性 = SubImage要素.Attribute( nameof( XML.Name ) ) ??
                            throw new Exception( $"{nameof( XML.Name )} 属性が存在しません。[{ファイルパス.変数付きパス}]" );

                        // 同じ名前の Name 属性が指定されたらこの要素を無視。
                        if( this.文字列to矩形.ContainsKey( Name属性.Value ) )
                        {
                            Log.WARNING( $"{nameof( XML.SubImage )} 要素の {nameof( XML.Name )} 属性が一意ではありません。これをスキップします。[{nameof( XML.Name )}={Name属性.Value}][{ファイルパス.変数付きパス}]" );
                            continue;
                        }

                        // Rectangle 属性を取得。なかったらスキップ。
                        var Rectangle属性 = SubImage要素.Attribute( nameof( XML.Rectangle ) );
                        if( null == Rectangle属性 )
                        {
                            Log.WARNING( $"{nameof( XML.SubImage )} 要素の {nameof( XML.Rectangle )} 属性が存在しません。これをスキップします。[{ファイルパス.変数付きパス}]" );
                            continue;
                        }

                        // Rectangle 属性値を正規表現に照らし合わせて、x, y, width, height を取得する。
                        var match = Regex.Match( Rectangle属性.Value, @"^\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*$" );
                        if( ( false == match.Success ) || ( 5 != match.Groups.Count ) )
                        {
                            Log.WARNING( $"{nameof( XML.SubImage )} 要素の {nameof( XML.Rectangle )} 属性の値が不正です。x,y,w,h の順に、整数で指定します。[{ファイルパス.変数付きパス}]" );
                            continue;
                        }

                        float x = float.Parse( match.Groups[ 1 ].Value );
                        float y = float.Parse( match.Groups[ 2 ].Value );
                        float width = float.Parse( match.Groups[ 3 ].Value );
                        float height = float.Parse( match.Groups[ 4 ].Value );

                        // 辞書に追加。
                        this.文字列to矩形.Add( Name属性.Value, new RectangleF( x, y, width, height ) );
                    }
                }
            }
            catch( Exception e )
            {
                Log.ERROR( $"矩形リストXmlファイルの読み込みに失敗しました。{VariablePath.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}[{ファイルパス.変数付きパス}]" );
            }
        }

        /// <summary>
        ///     矩形リストの矩形は X, Y, Width, Height の４つだが、
        ///     これを Left, Top, Right, Bottom に変換する。
        ///     主に Margin 属性での利用を想定。
        /// </summary>
        public static RectangleF Margin変換( RectangleF margin )
        {
            return new RectangleF() {
                Left = margin.X,
                Right = margin.Width,
                Top = margin.Y,
                Bottom = margin.Height,
            };
        }
    }
}
