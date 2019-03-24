using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;

namespace FDK.Tests
{
    [TestClass()]
    public class VariablePathTests
    {
        [TestInitialize()]
        public void TestInitialize()
        {
            var exePath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            VariablePath.フォルダ変数を追加または更新する( "Exe", $@"{exePath}\" );
            VariablePath.フォルダ変数を追加または更新する( "System", Path.Combine( exePath, @"System\" ) );
            VariablePath.フォルダ変数を追加または更新する( "AppData", Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create ), @"DTXMania2\" ) );
        }

        [TestMethod()]
        public void YAML出力Test()
        {
            var vpath = new VariablePath( @"$(Exe)test.txt" );

            var sr = new YamlDotNet.Serialization.Serializer();
            var yaml = sr.Serialize( vpath );

            Assert.AreEqual( @"$(Exe)test.txt
...
", yaml );
        }

        [TestMethod()]
        public void YAML入力Test()
        {
            var yaml = Folder.絶対パスに含まれるフォルダ変数を展開して返す( @"$(Exe)test.txt" );

            var ds = new YamlDotNet.Serialization.Deserializer();
            var vpath = ds.Deserialize<VariablePath>( yaml );

            Assert.AreEqual( @"$(Exe)test.txt", vpath.変数付きパス );
            Assert.AreEqual( yaml, vpath.変数なしパス );
        }
    }
}