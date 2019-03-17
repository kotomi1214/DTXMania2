using Microsoft.VisualStudio.TestTools.UnitTesting;
using DTXMania.設定;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DTXMania.入力;

namespace DTXMania.設定.Tests
{
    [TestClass()]
    public class キーバインディングTests
    {
        [TestMethod()]
        public void YAML出力Test()
        {
            var keybindings = new キーバインディング();

            var sr = new YamlDotNet.Serialization.SerializerBuilder()
                .WithTypeInspector( inner => new FDK32.シリアライズ.YAML.CommentGatheringTypeInspector( inner ) )
                .WithEmissionPhaseObjectGraphVisitor( args => new FDK32.シリアライズ.YAML.CommentsObjectGraphVisitor( args.InnerVisitor ) )
                .Build();

            var yaml = sr.Serialize( keybindings.キーボードtoドラム );

            var 正答 = @"0,16: LeftCrash
0,28: LeftCrash
0,30: HiHat_Open
0,44: HiHat_Close
0,31: HiHat_Foot
0,45: Snare
0,46: Bass
0,57: Bass
0,47: Tom1
0,48: Tom2
0,49: Tom3
0,50: RightCrash
0,37: Ride
";
            Assert.AreEqual( 正答, yaml );
        }

        [TestMethod()]
        public void YAML入力Test()
        {
            var yaml = @"# キーボードの入力割り当て（デバイスID,キーID: ドラム入力種別）
キーボードtoドラム:
    0,16: LeftCrash
    0,28: LeftCrash
    0,30: HiHat_Open
    0,44: HiHat_Close
    0,31: HiHat_Foot
    0,45: Snare
    0,46: Bass
    0,57: Bass
    0,47: Tom1
    0,48: Tom2
    0,49: Tom3
    0,50: RightCrash
    0,37: Ride
";

            var ds = new YamlDotNet.Serialization.Deserializer();
            var keybindings = ds.Deserialize<キーバインディング>( yaml );

            var 正答 = new (int dev, int key, ドラム入力種別 type)[] {
                ( 0, 16, ドラム入力種別.LeftCrash ),
                ( 0, 28, ドラム入力種別.LeftCrash ),
                ( 0, 30, ドラム入力種別.HiHat_Open ),
                ( 0, 44, ドラム入力種別.HiHat_Close ),
                ( 0, 31, ドラム入力種別.HiHat_Foot ),
                ( 0, 45, ドラム入力種別.Snare ),
                ( 0, 46, ドラム入力種別.Bass ),
                ( 0, 57, ドラム入力種別.Bass ),
                ( 0, 47, ドラム入力種別.Tom1 ),
                ( 0, 48, ドラム入力種別.Tom2 ),
                ( 0, 49, ドラム入力種別.Tom3 ),
                ( 0, 50, ドラム入力種別.RightCrash ),
                ( 0, 37, ドラム入力種別.Ride ),
            };

            Assert.AreEqual( 正答.Length, keybindings.キーボードtoドラム.Count );

            for( int i = 0; i < 正答.Length; i++ )
            {
                Assert.AreEqual( 正答[ i ].dev, keybindings.キーボードtoドラム.ElementAt( i ).Key.deviceId );
                Assert.AreEqual( 正答[ i ].key, keybindings.キーボードtoドラム.ElementAt( i ).Key.key );
                Assert.AreEqual( 正答[ i ].type, keybindings.キーボードtoドラム.ElementAt( i ).Value );
            }
        }
    }
}