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
    public class キャッシュデータレンタルTests
    {
        private string ファイルパス;
        private Stopwatch Stopwatch;
        private const int 生成時間ms = 1000;
        private readonly string コンテンツ = @"これはテスト用ファイルです。";

        // テストメソッドごとの初期化処理。
        [TestInitialize()]
        public void MyTestInitialize()
        {
            // テスト用ファイルを作成。
            this.ファイルパス = Path.GetTempFileName();
            File.AppendAllText( this.ファイルパス, this.コンテンツ );

            this.Stopwatch = new Stopwatch();
        }

        // テストメソッドごとの終了処理。
        [TestCleanup()]
        public void MyTestCleanup()
        {
            // テスト用ファイルを削除。
            if( File.Exists( this.ファイルパス ) )
                File.Delete( this.ファイルパス );

            this.Stopwatch = null;
        }


        [TestMethod()]
        public void キャッシュTest()
        {
            Assert.IsTrue( File.Exists( this.ファイルパス ) );
            Assert.IsNotNull( this.Stopwatch );

            using( var CDR = new キャッシュデータレンタル<string>() )
            {
                // 外部依存アクションを接続。
                CDR.ファイルからデータを生成する = ( path ) => {
                    System.Threading.Thread.Sleep( 生成時間ms );  // 生成は時間のかかる処理とする
                    using( var sr = new StreamReader( path.変数なしパス ) )
                        return sr.ReadToEnd();
                };

                CDR.世代を進める();
                Assert.AreEqual( 1, CDR.現世代 );

                // 初回の取得　→　時間がかかる
                this.Stopwatch.Restart();
                var 内容 = CDR.作成する( this.ファイルパス );
                Assert.IsTrue( this.Stopwatch.ElapsedMilliseconds >= 生成時間ms );
                Assert.AreEqual( this.コンテンツ, 内容 );

                // ２回目の取得　→　時間はかからない
                this.Stopwatch.Restart();
                内容 = CDR.作成する( this.ファイルパス );
                Assert.IsTrue( this.Stopwatch.ElapsedMilliseconds < 生成時間ms / 2 );   // 目安として半分以下
                Assert.AreEqual( this.コンテンツ, 内容 );

                // ファイルの最終更新日時を変更する
                File.SetLastWriteTime( this.ファイルパス, DateTime.Now );

                // ３回目の取得　→　時間がかかる
                this.Stopwatch.Restart();
                内容 = CDR.作成する( this.ファイルパス );
                Assert.IsTrue( this.Stopwatch.ElapsedMilliseconds >= 生成時間ms );
                Assert.AreEqual( this.コンテンツ, 内容 );

                // ４回目の取得　→　時間はかからない
                this.Stopwatch.Restart();
                内容 = CDR.作成する( this.ファイルパス );
                Assert.IsTrue( this.Stopwatch.ElapsedMilliseconds < 生成時間ms / 2 );   // 目安として半分以下
                Assert.AreEqual( this.コンテンツ, 内容 );
            }
        }

        [TestMethod()]
        public void 世代Test()
        {
            Assert.IsTrue( File.Exists( this.ファイルパス ) );
            Assert.IsNotNull( this.Stopwatch );

            using( var CDR = new キャッシュデータレンタル<string>() )
            {
                // 外部依存アクションを接続。
                CDR.ファイルからデータを生成する = ( path ) => {
                    System.Threading.Thread.Sleep( 生成時間ms );  // 生成は時間のかかる処理とする
                    using( var sr = new StreamReader( path.変数なしパス ) )
                        return sr.ReadToEnd();
                };

                CDR.世代を進める();
                Assert.AreEqual( 1, CDR.現世代 );

                // 初回の取得　→　時間がかかる
                this.Stopwatch.Restart();
                var 内容 = CDR.作成する( this.ファイルパス );
                Assert.IsTrue( this.Stopwatch.ElapsedMilliseconds >= 生成時間ms );
                Assert.AreEqual( this.コンテンツ, 内容 );

                // 現在、キャッシュは１個。
                Assert.AreEqual( 1, CDR._キャッシュデータリスト.Count );

                // 世代を進める　→　前世代中にファイルを取得したので、まだキャッシュは１個のまま。
                CDR.世代を進める();
                Assert.AreEqual( 2, CDR.現世代 );
                Assert.AreEqual( 1, CDR._キャッシュデータリスト.Count );

                // 世代を進める　→　前世代中にファイルを取得しなかったので、キャッシュから消された。
                CDR.世代を進める();
                Assert.AreEqual( 3, CDR.現世代 );
                Assert.AreEqual( 0, CDR._キャッシュデータリスト.Count );

                // ２回目の取得　→　また時間がかかる
                this.Stopwatch.Restart();
                内容 = CDR.作成する( this.ファイルパス );
                Assert.IsTrue( this.Stopwatch.ElapsedMilliseconds >= 生成時間ms );
                Assert.AreEqual( this.コンテンツ, 内容 );
                Assert.AreEqual( 1, CDR._キャッシュデータリスト.Count );
            }
        }
    }
}