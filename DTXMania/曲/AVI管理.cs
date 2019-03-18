using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SSTFormat.v4;
using FDK32;

namespace DTXMania.曲
{
    /// <summary>
    ///		<see cref="スコア.AVIリスト"/> の各動画インスタンスを管理する。
    /// </summary>

    class AVI管理 : IDisposable
    {
        public IReadOnlyDictionary<int, Video> 動画リスト => this._動画リスト;


        public AVI管理()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._動画リスト = new Dictionary<int, Video>();
            }
        }

        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._一時停止中の動画のリスト = null;

                foreach( var kvp in this._動画リスト )
                    kvp.Value.Dispose();

                this._動画リスト.Clear();
                this._動画リスト = null;
            }
        }

        /// <summary>
        ///		指定したAVI番号に動画ファイルを登録する。
        /// </summary>
        public void 登録する( int AVI番号, VariablePath 動画ファイルの絶対パス, double 再生速度 = 1.0 )
        {
            #region " パラメータチェック "
            //----------------
            if( 0 > AVI番号 || 36 * 36 <= AVI番号 )
                throw new ArgumentOutOfRangeException( $"AVI番号が範囲(0～1295)を超えています。[{AVI番号}]" );

            if( !( File.Exists( 動画ファイルの絶対パス.変数なしパス ) ) )
            {
                Log.WARNING( $"動画ファイルが存在しません。[{動画ファイルの絶対パス.変数付きパス}]" );
                return;
            }
            //----------------
            #endregion

            // すでに登録済みなら解放する。
            if( this._動画リスト.ContainsKey( AVI番号 ) )
                this._動画リスト[ AVI番号 ].Dispose();

            // 新しいVideoを生成して登録する。
            this._動画リスト[ AVI番号 ] = new Video( 動画ファイルの絶対パス, 再生速度 );
        }

        public void 再生中の動画をすべて一時停止する()
        {
            this._一時停止中の動画のリスト.Clear();

            foreach( var kvp in this._動画リスト )
            {
                var video = kvp.Value;

                if( video.再生中 )
                {
                    video.一時停止する();
                    this._一時停止中の動画のリスト.Add( video );
                }
            }
        }

        public void 一時停止中の動画をすべて再開する()
        {
            foreach( var video in this._一時停止中の動画のリスト )
            {
                video.再開する();
            }
        }


        /// <summary>
        ///		全AVIのリスト。[key: WAV番号]
        /// </summary>
        private Dictionary<int, Video> _動画リスト = null;

        private List<Video> _一時停止中の動画のリスト = new List<Video>();
    }
}
