using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FDK;

namespace DTXMania
{
    class UpdatingSoglistパネル : IDisposable
    {

        // 生成と終了


        public UpdatingSoglistパネル()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._パネル画像 = new テクスチャ( @"$(System)images\選曲\Updating songlist.png" );
                this._明滅カウンタ = new LoopCounter( 0, 99, 10 );
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._パネル画像?.Dispose();
            }
        }



        // 進行と描画


        public void 進行描画する( float x, float y )
        {
            if( App進行描画.曲ツリー.現行化タスク.IsCompleted )
                return; // 現行化タスクが終わっていれば表示OFF

            float 不透明度 = (float) Math.Sin( Math.PI * this._明滅カウンタ.現在値 / 100.0 );
            this._パネル画像.描画する( x, y, 不透明度0to1: 不透明度 );
        }



        // private


        private テクスチャ _パネル画像;

        private LoopCounter _明滅カウンタ;
    }
}
