using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FDK.Tests
{
    [TestClass()]
    public class 矩形リストTests
    {
        // 例外検証用メソッド
        private void 例外が出れば成功( Action action )
        {
            try
            {
                action();
                Assert.Fail();  // ここに来るということは、action() で例外がでなかったということ。
            }
            catch( AssertFailedException )
            {
                throw;  // 失敗。
            }
            catch
            {
                // 成功。
            }
        }

        [TestMethod()]
        public void 矩形リストXmlファイルを読み込むTest()
        {
            #region " 矩形リスト01.xml について "

            // テスト用ファイルがビルド出力ディレクトリにコピーされていること。
            Assert.IsTrue( File.Exists( @".\フレームワーク\画像、テクスチャ\フォント、文字列画像\テストファイル\矩形リスト01.xml" ) );

            // 読み込めること。失敗したら例外発出。
            var recs01 = new 矩形リスト( @".\フレームワーク\画像、テクスチャ\フォント、文字列画像\テストファイル\矩形リスト01.xml" );

            // 取得できた SubImage の数が正しいこと。
            Assert.AreEqual( actual: recs01.文字列to矩形.Count, expected: 3 );   // Name="4" だけ失敗 → 無視される。

            // Name の存在の有無を確認できること。
            Assert.IsTrue( recs01.文字列to矩形.ContainsKey( "1" ) );   // 存在する
            Assert.IsTrue( recs01.文字列to矩形.ContainsKey( "2nd" ) );  // 存在する
            Assert.IsTrue( recs01.文字列to矩形.ContainsKey( "3" ) );  // 存在する
            Assert.IsFalse( recs01.文字列to矩形.ContainsKey( "4" ) );  // 存在しない

            // 内容が正しいこと。
            Assert.AreEqual( actual: recs01.文字列to矩形[ "1" ].X, expected: 1.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "1" ].Y, expected: 2.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "1" ].Width, expected: 3.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "1" ].Height, expected: 4.0f );

            Assert.AreEqual( actual: recs01.文字列to矩形[ "2nd" ].X, expected: 5.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "2nd" ].Y, expected: 6.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "2nd" ].Width, expected: 7.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "2nd" ].Height, expected: 8.0f );

            Assert.AreEqual( actual: recs01.文字列to矩形[ "3" ].X, expected: 9.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "3" ].Y, expected: 10.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "3" ].Width, expected: 11.0f );
            Assert.AreEqual( actual: recs01.文字列to矩形[ "3" ].Height, expected: 12.0f );

            #endregion
            #region " 矩形リスト02.xml について "

            // テスト用ファイルがビルド出力ディレクトリにコピーされていること。
            Assert.IsTrue( File.Exists( @".\フレームワーク\画像、テクスチャ\フォント、文字列画像\テストファイル\矩形リスト02.xml" ) );

            // 読み込めること。失敗したら例外発出。
            var recs02 = new 矩形リスト( @".\フレームワーク\画像、テクスチャ\フォント、文字列画像\テストファイル\矩形リスト02.xml" );

            // 取得できた SubImage の数が正しいこと。
            Assert.AreEqual( actual: recs02.文字列to矩形.Count, expected: 2 );   // 読み込めるのは Name="A","ごばんめ" だけ。

            // Name の存在の有無を確認できること。
            Assert.IsTrue( recs02.文字列to矩形.ContainsKey( "A" ) );   // 存在する
            Assert.IsFalse( recs02.文字列to矩形.ContainsKey( "B" ) );  // 存在しない
            Assert.IsFalse( recs02.文字列to矩形.ContainsKey( "C" ) );  // 存在しない
            Assert.IsFalse( recs02.文字列to矩形.ContainsKey( "D" ) );  // 存在しない
            Assert.IsTrue( recs02.文字列to矩形.ContainsKey( "ごばんめ" ) );   // 存在する

            // 内容が正しいこと。
            Assert.AreEqual( actual: recs02.文字列to矩形[ "A" ].X, expected: 1.0f );
            Assert.AreEqual( actual: recs02.文字列to矩形[ "A" ].Y, expected: 2.0f );
            Assert.AreEqual( actual: recs02.文字列to矩形[ "A" ].Width, expected: 3.0f );
            Assert.AreEqual( actual: recs02.文字列to矩形[ "A" ].Height, expected: 4.0f );

            Assert.AreEqual( actual: recs02.文字列to矩形[ "ごばんめ" ].X, expected: 17.0f );
            Assert.AreEqual( actual: recs02.文字列to矩形[ "ごばんめ" ].Y, expected: 18.0f );
            Assert.AreEqual( actual: recs02.文字列to矩形[ "ごばんめ" ].Width, expected: 19.0f );
            Assert.AreEqual( actual: recs02.文字列to矩形[ "ごばんめ" ].Height, expected: 20.0f );
            #endregion
        }
    }
}