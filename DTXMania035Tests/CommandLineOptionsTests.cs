using Microsoft.VisualStudio.TestTools.UnitTesting;
using DTXMania;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania.Tests
{
    [TestClass()]
    public class CommandLineOptionsTests
    {
        [TestMethod()]
        public void 解析するTest()
        {
            CommandLineOptions options = null;

            // 正常系

            // オプション指定なし
            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "".Split( ' ' ) ) );
            Assert.IsFalse( options.サウンドファイルの再生 );  // すべて false
            Assert.IsFalse( options.再生停止 );
            Assert.IsFalse( options.ビュアーの設定 );
            Assert.IsFalse( options.再生開始 );
            Assert.IsFalse( options.DTX2WAV );
            Assert.IsTrue( options.ドラム音を発声する ); // true;

            // -S オプション
            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "-S".Split( ' ' ) ) );
            Assert.IsFalse( options.サウンドファイルの再生 );
            Assert.IsTrue( options.再生停止 );          // true
            Assert.IsFalse( options.ビュアーの設定 );
            Assert.IsFalse( options.再生開始 );
            Assert.IsFalse( options.DTX2WAV );

            // -N オプション
            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "-N0".Split( ' ' ) ) );
            Assert.IsFalse( options.サウンドファイルの再生 );
            Assert.IsFalse( options.再生停止 );
            Assert.IsFalse( options.ビュアーの設定 );
            Assert.IsTrue( options.再生開始 );         // true
            Assert.IsFalse( options.DTX2WAV );
            Assert.AreEqual( 0, options.再生開始小節番号 );

            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "-N-1".Split( ' ' ) ) );
            Assert.IsTrue( options.再生開始 );
            Assert.AreEqual( -1, options.再生開始小節番号 );    // 負数も OK

            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "-N".Split( ' ' ) ) );
            Assert.IsTrue( options.再生開始 );
            Assert.AreEqual( -1, options.再生開始小節番号 );    // 省略値は -1
                                                        
            // ファイル名
            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "test.dtx".Split( ' ' ) ) );
            Assert.AreEqual( "test.dtx", options.Filename );
            Assert.IsTrue( options.再生開始 );                  // ファイル名の指定があるのに -N も -S もない場合は、-N-1 が指定されたものとみなす。
            Assert.AreEqual( -1, options.再生開始小節番号 );

            options = new CommandLineOptions();
            Assert.IsTrue( options.解析する( "-N100 test.dtx test2.gda".Split( ' ' ) ) );
            Assert.AreEqual( "test2.gda", options.Filename );   // 複数指定されたら最後のものが有効。
            Assert.IsTrue( options.再生開始 );
            Assert.AreEqual( 100, options.再生開始小節番号 );
            Assert.IsFalse( options.再生停止 );


            // 準正常系

            // 未知のオプション
            options = new CommandLineOptions();
            Assert.IsFalse( options.解析する( "-h".Split( ' ' ) ) );   // false = 解析失敗
        }
    }
}