using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Animation;
using FDK;

namespace DTXMania2.選曲
{
    class 表示方法選択パネル : IDisposable
    {

        // プロパティ


        public enum 表示方法
        {
            全曲,
            評価順,
        }

        public 表示方法 現在の表示方法
            => this._パネルs[ this._現在選択中の実パネル番号 ].表示方法;



        // 生成と終了


        public 表示方法選択パネル()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._現在選択中の論理パネル番号 = 0;
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            foreach( var p in this._パネルs )
                p.Dispose();
        }



        // 進行と描画


        public void 進行描画する( DeviceContext dc )
        {
            // パネルを合計８枚表示する。（左隠れ１枚 ＋ 表示６枚 ＋ 右隠れ１枚）

            int 表示元の位置 = this._現在選択中の論理パネル番号;
            for( int i = 0; i < 3; i++ )    // 3つ戻る
                表示元の位置 = ( 表示元の位置 - 1 + this._パネルs.Count ) % this._パネルs.Count;

            for( int i = 0; i < 8; i++ )
            {
                var 画像 = this._パネルs[ 表示元の位置 ].画像;

                const float パネル幅 = 144f;
                画像.描画する(
                    左位置: (float) ( 768f + パネル幅 * i ),
                    上位置: ( 3 == i ) ? 90f : 54f ); // i==3 が現在の選択パネル。他より下に描画。

                表示元の位置 = ( 表示元の位置 + 1 ) % this._パネルs.Count;
            }
        }



        // パネルの選択


        public void 次のパネルを選択する()
        {
            this._現在選択中の論理パネル番号++;
        }

        public void 前のパネルを選択する()
        {
            this._現在選択中の論理パネル番号--;
        }



        // ローカル


        private class Panel : IDisposable
        {
            public 表示方法 表示方法;
            public VariablePath 画像の絶対パス;
            public 画像 画像;

            public Panel( 表示方法 type, VariablePath path )
            {
                this.表示方法 = type;
                this.画像の絶対パス = path;
                this.画像 = new 画像( path );

            }
            public void Dispose()
            {
                this.画像.Dispose();
            }
        };

        private List<Panel> _パネルs = new List<Panel>() {
            new Panel( 表示方法.全曲, @"$(Images)\SelectStage\Sorting_All.png" ),
            new Panel( 表示方法.評価順, @"$(Images)\SelectStage\Sorting_Evaluation.png" ),
        };

        private int _現在選択中の論理パネル番号;

        private int _現在選択中の実パネル番号 => ( 0 <= this._現在選択中の論理パネル番号 ) ?
            this._現在選択中の論理パネル番号 % this._パネルs.Count :
            ( this._パネルs.Count + this._現在選択中の論理パネル番号 ) % this._パネルs.Count;
    }
}
