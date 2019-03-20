using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania
{
    /// <summary>
    ///     全アイキャッチのインスタンスを保持する。
    /// </summary>
    class アイキャッチ管理 : IDisposable
    {
        public アイキャッチ 現在のアイキャッチ { get; protected set; } = null;


        public アイキャッチ管理()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // アイキャッチが増えたらここに追加する。
                this._アイキャッチリスト = new Dictionary<string, アイキャッチ>() {
                    { nameof( シャッター ),       new シャッター() },
                    { nameof( 回転幕 ),           new 回転幕() },
                    { nameof( GO ),               new GO() },
                    { nameof( 半回転黒フェード ), new 半回転黒フェード() },
                };

                this.現在のアイキャッチ = this._アイキャッチリスト.ElementAt( 0 ).Value;  // 最初は先頭のもの
            }
        }

        public virtual void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                foreach( var kvp in this._アイキャッチリスト )
                    kvp.Value?.Dispose();

                this._アイキャッチリスト = null;
            }
        }


        /// <summary>
        ///     指定した名前のアイキャッチのクローズアニメーションを開始する。
        /// </summary>
        /// <remarks>
        ///     クローズしたアイキャッチをオープンする際には、クローズしたときと同じアイキャッチを使う必要がある。
        ///     ここで指定したアイキャッチは <see cref="現在のアイキャッチ"/> に保存されるので、
        ///     遷移先のステージでオープンするアイキャッチには、これを使用すること。
        /// </remarks>
        public void アイキャッチを選択しクローズする( string 名前 )
        {
            this.現在のアイキャッチ = this._アイキャッチリスト[ 名前 ]; // 保存。
            this.現在のアイキャッチ.クローズする();
        }


        private Dictionary<string, アイキャッチ> _アイキャッチリスト;
    }
}
