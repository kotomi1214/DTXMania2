using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.結果
{
    class ランク : IDisposable
    {

        // 生成と終了


        public ランク()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._ランク画像 = new Dictionary<ランク種別, テクスチャ>() {
                    { ランク種別.SS, new テクスチャ( @"$(System)Images\結果\リザルトSS.png" ) },
                    { ランク種別.S, new テクスチャ( @"$(System)Images\結果\リザルトS.png" ) },
                    { ランク種別.A, new テクスチャ( @"$(System)Images\結果\リザルトA.png" ) },
                    { ランク種別.B, new テクスチャ( @"$(System)Images\結果\リザルトB.png" ) },
                    { ランク種別.C, new テクスチャ( @"$(System)Images\結果\リザルトC.png" ) },
                    { ランク種別.D, new テクスチャ( @"$(System)Images\結果\リザルトD.png" ) },
                    { ランク種別.E, new テクスチャ( @"$(System)Images\結果\リザルトE.png" ) },
                };
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                foreach( var rank in this._ランク画像 )
                    rank.Value?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( ランク種別 rank )
        {
            this._ランク画像[ rank ].描画する( 200f, 300f, 1f, 3f, 3f );
        }



        // private


        private Dictionary<ランク種別, テクスチャ> _ランク画像 = null;
    }
}
