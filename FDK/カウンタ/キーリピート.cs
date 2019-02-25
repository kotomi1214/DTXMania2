using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK.カウンタ
{
    public static class キーリピート
    {
        /// <summary>
        ///     <see cref="押下中"/> が true の間中、<see cref="押下処理"/>を実行する。
        /// </summary>
        /// <remarks>
        ///     キーを押しっぱなしにした際の自動反復入力をシミュレートする。
        ///     このメソッドは、呼び出された回数に応じた値を返す。
        ///     初めてこのメソッドを呼び出した場合、trueが返される（押下処理をすぐに実行すべき）。
        ///     ２回目に呼び出した場合、押下処理は１回目から 200ms の間隔が開くまで false が返されたのち、true が返される。
        ///     ３回目以降の呼び出しでは、それぞれ 30ms 間隔で true が返される。
        ///     <see cref="押下中"/> を false にして呼び出すと、内部の呼び出し回数は 0 にリセットされる。
        /// </remarks>
        /// <returns>
        ///     true なら、すぐに押下処理を実行すべきであることを意味する。
        ///     同様に、false ならすべきではないことを意味する。
        /// </returns>
        public static bool 処理を反復実行する( int キー番号, bool 押下中 )
        {
            // 初めて使用するキー番号であるか、または前回の呼び出しから1000ms以上経過しているなら、リセットする。

            if( !( KeyContextMap.ContainsKey( キー番号 ) ) ||
                KeyContextMap[ キー番号 ].Timer.現在のリアルタイムカウントsec > 1.0 )
            {
                KeyContextMap[ キー番号 ] = new KeyContext();
            }

            var context = KeyContextMap[ キー番号 ];


            bool 押下処理を実行すべき = false;

            if( 押下中 )
            {
                double 経過時間sec = context.Timer.現在のリアルタイムカウントsec;

                switch( context.呼び出された回数 )
                {
                    // 初回
                    case 0:
                        context.呼び出された回数 = 1;
                        context.Timer.リセットする();
                        押下処理を実行すべき = true;
                        break;

                    // ２回目
                    case 1:
                        if( 経過時間sec > 0.2 )     // 200ms 経った
                        {
                            context.呼び出された回数 = 2;
                            context.Timer.リセットする();
                            押下処理を実行すべき = true;
                        }
                        break;

                    // ３回目以降
                    default:
                        if( 経過時間sec > 0.03 )    // 30ms 経った
                        {
                            context.Timer.リセットする();
                            押下処理を実行すべき = true;
                        }
                        break;
                }
            }
            else
            {
                // リセット。
                KeyContextMap[ キー番号 ] = new KeyContext();
            }

            return 押下処理を実行すべき;
        }
        public static bool 処理を反復実行する( SharpDX.DirectInput.Key キー番号, bool 押下中 )
            => 処理を反復実行する( (int) キー番号, 押下中 );

        private class KeyContext
        {
            public int Key = 0;
            public int 呼び出された回数 = 0;
            public QPCTimer Timer = new QPCTimer();
        }
        private static Dictionary<int, KeyContext> KeyContextMap = new Dictionary<int, KeyContext>();
    }
}
