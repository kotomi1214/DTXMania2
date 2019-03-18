using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace FDK.Tests
{
    [TestClass()]
    public class 画像フォントTests
    {
        [TestMethod()]
        public void 設定ファイルTest()
        {
            var 画像ファイルパス = @".\フレームワーク\画像、テクスチャ\フォント、文字列画像\テストファイル\パラメータ文字_小.png";
            var 設定ファイルパス = @".\フレームワーク\画像、テクスチャ\フォント、文字列画像\テストファイル\パラメータ文字_小.yaml";

            // テスト用ファイルがビルド出力ディレクトリにコピーされていること。
            Assert.IsTrue( File.Exists( 画像ファイルパス ) );
            Assert.IsTrue( File.Exists( 設定ファイルパス ) );

            var imageFont = new 画像フォント( 画像ファイルパス, 設定ファイルパス, 8f, 0.9f );

            Assert.AreEqual( 8f, imageFont.文字幅補正dpx );
            Assert.AreEqual( 0.9f, imageFont.不透明度 );

            var 矩形リスト正答 = new (string 文字, RectangleF 矩形)[] {
                ( "0", new RectangleF(  0,  0, 17, 32) ),
                ( "1", new RectangleF( 17,  0, 17, 32) ),
                ( "2", new RectangleF( 34,  0, 17, 32) ),
                ( "3", new RectangleF( 51,  0, 17, 32) ),
                ( "4", new RectangleF( 68,  0, 17, 32) ),
                ( "5", new RectangleF(  0, 32, 17, 32) ),
                ( "6", new RectangleF( 17, 32, 17, 32) ),
                ( "7", new RectangleF( 34, 32, 17, 32) ),
                ( "8", new RectangleF( 51, 32, 17, 32) ),
                ( "9", new RectangleF( 68, 32, 17, 32) ),
                ( "o", new RectangleF(  0, 64, 17, 32) ),
                ( ".", new RectangleF( 17, 64, 17, 32) ),
                ( "%", new RectangleF( 34, 64, 17, 32) ),
                ( "~", new RectangleF( 51, 64, 17, 32) ),
                ( " ", new RectangleF( 68, 64, 17, 32) ),
            };

            Assert.AreEqual( 矩形リスト正答.Length, imageFont._矩形リスト.Count );

            for( int i = 0; i < 矩形リスト正答.Length; i++ )
            {
                Assert.AreEqual( 矩形リスト正答[ i ].文字, imageFont._矩形リスト.ElementAt( i ).Key );
                Assert.AreEqual( 矩形リスト正答[ i ].矩形, imageFont._矩形リスト.ElementAt( i ).Value );
            }
        }
    }

}