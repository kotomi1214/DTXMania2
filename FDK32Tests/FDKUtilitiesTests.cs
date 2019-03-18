using Microsoft.VisualStudio.TestTools.UnitTesting;
using FDK32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FDK32.Tests
{
    [TestClass()]
    public class FDKUtilitiesTests
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

        // テスト用クラス
        class DisposeTest : IDisposable
        {
            public bool Dispose済み = false;
            public void Dispose()
            {
                this.Dispose済み = true;
            }
        }

        [TestMethod()]
        public void 最大公約数を返すTest()
        {
            int 結果 = 0;

            // 正常系。

            Assert.AreEqual( actual: FDK32.FDKUtilities.最大公約数を返す( 1, 1 ), expected: 1 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最大公約数を返す( 1, 2 ), expected: 1 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最大公約数を返す( 3, 1 ), expected: 1 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最大公約数を返す( 630, 300 ), expected: 30 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最大公約数を返す( 0, 100 ), expected: 100 );  // 0 は OK
            Assert.AreEqual( actual: FDK32.FDKUtilities.最大公約数を返す( 200, 0 ), expected: 200 );  // 0 は OK

            // 準正常系。

            例外が出れば成功( () => { 結果 = FDK32.FDKUtilities.最大公約数を返す( 1, -1 ); } );   // 負数はダメ
        }

        [TestMethod()]
        public void 最小公倍数を返すTest()
        {
            int 結果 = 0;

            // 正常系。

            Assert.AreEqual( actual: FDK32.FDKUtilities.最小公倍数を返す( 1, 1 ), expected: 1 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最小公倍数を返す( 1, 2 ), expected: 2 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最小公倍数を返す( 3, 1 ), expected: 3 );
            Assert.AreEqual( actual: FDK32.FDKUtilities.最小公倍数を返す( 630, 300 ), expected: 6300 );

            // 準正常系。

            例外が出れば成功( () => { 結果 = FDK32.FDKUtilities.最小公倍数を返す( 1, 0 ); } );    // 0 はダメ
            例外が出れば成功( () => { 結果 = FDK32.FDKUtilities.最小公倍数を返す( 1, -1 ); } );   // 負数もダメ
        }

        [TestMethod()]
        public void 現在のメソッド名Test()
        {
            Assert.AreEqual(
                actual: FDK32.FDKUtilities.現在のメソッド名,
                expected: typeof( FDKUtilitiesTests ).ToString() + @"." + nameof( 現在のメソッド名Test ) + @"()",
                ignoreCase: false );
        }

        [TestMethod()]
        public void 約分するTest()
        {
            var ret = FDKUtilities.約分する( 1, 1 );
            Assert.AreEqual( 1, ret.分子 );
            Assert.AreEqual( 1, ret.分母 );

            ret = FDKUtilities.約分する( 2, 3 );
            Assert.AreEqual( 2, ret.分子 );
            Assert.AreEqual( 3, ret.分母 );

            ret = FDKUtilities.約分する( 10, 5 );
            Assert.AreEqual( 2, ret.分子 );
            Assert.AreEqual( 1, ret.分母 );

            ret = FDKUtilities.約分する( 128, 120 );
            Assert.AreEqual( 16, ret.分子 );
            Assert.AreEqual( 15, ret.分母 );

            ret = FDKUtilities.約分する( 0, 100 );  // 分子が 0 なら 0/1 を返す。
            Assert.AreEqual( 0, ret.分子 );
            Assert.AreEqual( 1, ret.分母 );
        }
    }
}
