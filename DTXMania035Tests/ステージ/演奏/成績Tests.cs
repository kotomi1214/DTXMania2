using Microsoft.VisualStudio.TestTools.UnitTesting;
using DTXMania.ステージ.演奏;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DTXMania.ステージ.演奏.Tests
{
    [TestClass()]
    public class 成績Tests
    {
        [TestMethod()]
        public void 判定toヒット数割合Test()
        {
            // このテストがすべて合格するような計算式を確立する。

            #region " 実サンプル1 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 48 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 1 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 0 );
                result.ヒット数を加算する( 判定種別.OK, false, 0 );
                result.ヒット数を加算する( 判定種別.MISS, false, 0 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 3, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル2 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 49 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 1 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 0 );
                result.ヒット数を加算する( 判定種別.OK, false, 0 );
                result.ヒット数を加算する( 判定種別.MISS, false, 0 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 98, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル3 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 90 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 1 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 0 );
                result.ヒット数を加算する( 判定種別.OK, false, 0 );
                result.ヒット数を加算する( 判定種別.MISS, false, 0 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 98, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル4 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 90 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 2 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 0 );
                result.ヒット数を加算する( 判定種別.OK, false, 0 );
                result.ヒット数を加算する( 判定種別.MISS, false, 0 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 3, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル5 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 148 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 2 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 0 );
                result.ヒット数を加算する( 判定種別.OK, false, 0 );
                result.ヒット数を加算する( 判定種別.MISS, false, 1 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 98, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 1, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル6 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 883 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 19 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 2 );
                result.ヒット数を加算する( 判定種別.OK, false, 2 );
                result.ヒット数を加算する( 判定種別.MISS, false, 1 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 1, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル7 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 1397 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 36 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 1 );
                result.ヒット数を加算する( 判定種別.OK, false, 1 );
                result.ヒット数を加算する( 判定種別.MISS, false, 2 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル8 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 1397 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 36 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 1 );
                result.ヒット数を加算する( 判定種別.OK, false, 1 );
                result.ヒット数を加算する( 判定種別.MISS, false, 2 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 97, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 0, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル9 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 1055 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 41 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 3 );
                result.ヒット数を加算する( 判定種別.OK, false, 0 );
                result.ヒット数を加算する( 判定種別.MISS, false, 3 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 95, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 4, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 1, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 0, dic[ 判定種別.OK ] );
                Assert.AreEqual( 0, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
            #region " 実サンプル10 "
            //----------------
            {
                var result = new 成績();
                result.ヒット数を加算する( 判定種別.PERFECT, false, 403 );
                result.ヒット数を加算する( 判定種別.GREAT, false, 10 );
                result.ヒット数を加算する( 判定種別.GOOD, false, 1 );
                result.ヒット数を加算する( 判定種別.OK, false, 3 );
                result.ヒット数を加算する( 判定種別.MISS, false, 5 );

                var dic = result.判定toヒット割合;
                Assert.AreEqual( 95, dic[ 判定種別.PERFECT ] );
                Assert.AreEqual( 2, dic[ 判定種別.GREAT ] );
                Assert.AreEqual( 1, dic[ 判定種別.GOOD ] );
                Assert.AreEqual( 1, dic[ 判定種別.OK ] );
                Assert.AreEqual( 1, dic[ 判定種別.MISS ] );
            }
            //----------------
            #endregion
        }
    }
}
