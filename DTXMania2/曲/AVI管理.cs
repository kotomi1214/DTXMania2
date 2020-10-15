using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DTXMania2.演奏;
using FDK;
using Windows.UI.Xaml.Controls;
using SSTF=SSTFormat.v004;

namespace DTXMania2
{
    /// <summary>
    ///		<see cref="スコア.AVIリスト"/> の各動画インスタンスを管理する。
    /// </summary>
    class AVI管理 : IDisposable
    {

        // プロパティ


        public IReadOnlyDictionary<int, Video> 動画リスト
        {
            get => new Dictionary<int, Video>(
                this._動画リスト.Select( ( kvp ) => new KeyValuePair<int, Video>( kvp.Key, kvp.Value.動画 ) ) );
        }



        // 生成と終了


        public AVI管理()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._動画リスト = new Dictionary<int, VideoContext>();
            this._一時停止中の動画のリスト = new List<Video>();
        }

        public void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._一時停止中の動画のリスト.Clear();  // 各要素はDisposeしない; 各要素の本体は動画リストにあるため。

            foreach( var value in this._動画リスト.Values )
                value.動画.Dispose();

            this._動画リスト.Clear();
        }



        // 登録、停止、再開


        /// <summary>
        ///		指定したAVI番号に動画ファイルを登録する。
        /// </summary>
        public void 登録する( int AVI番号, VariablePath 動画ファイルの絶対パス, double 再生速度 = 1.0 )
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            if( 0 > AVI番号 || 36 * 36 <= AVI番号 )
                throw new ArgumentOutOfRangeException( $"AVI番号が範囲(0～1295)を超えています。[{AVI番号}]" );

            if( !( File.Exists( 動画ファイルの絶対パス.変数なしパス ) ) )
            {
                Log.ERROR( $"動画ファイルが存在しません。[{動画ファイルの絶対パス.変数付きパス}]" );
                return;
            }

            // すでに登録済みなら解放する。
            this._削除する( AVI番号 );

            // 新しいVideoを生成して登録する。
            this._登録する( AVI番号, 動画ファイルの絶対パス, 再生速度 );
        }

        public void 再生中の動画をすべて一時停止する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._一時停止中の動画のリスト.Clear();

            foreach( var vc in this._動画リスト.Values )
            {
                if( vc.動画.再生中 )
                {
                    vc.動画.一時停止する();
                    this._一時停止中の動画のリスト.Add( vc.動画 );
                }
            }
        }

        public void 一時停止中の動画をすべて再開する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            foreach( var video in this._一時停止中の動画のリスト )
                video.再開する();

            this._一時停止中の動画のリスト.Clear();
        }

        public void 削除する( int AVI番号 )
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._削除する( AVI番号 );
        }

        /// <summary>
        ///     指定されたAVI番号の動画のみいったん解放し、新しく再構築する。
        /// </summary>
        public void 再構築する( int AVI番号 )
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            if( this._動画リスト.ContainsKey( AVI番号 ) )
            {
                var vc = this._動画リスト[ AVI番号 ];

                // 動画のみ解放
                vc.動画.Dispose();

                // 動画のみ再構築
                vc.動画 = new Video(
                    Global.GraphicResources.MFDXGIDeviceManager,
                    Global.GraphicResources.既定のD2D1DeviceContext,
                    vc.動画ファイルの絶対パス,
                    vc.再生速度 );
            }
        }


        // ローカル


        private class VideoContext
        {
            public int AVI番号;
            public VariablePath 動画ファイルの絶対パス;
            public double 再生速度;
            public Video 動画;

            public VideoContext( int AVI番号, VariablePath 動画ファイルの絶対パス, double 再生速度, Video 動画 )
            {
                this.AVI番号 = AVI番号;
                this.動画ファイルの絶対パス = 動画ファイルの絶対パス;
                this.再生速度 = 再生速度;
                this.動画 = 動画;
            }
        }

        /// <summary>
        ///		全AVIのリスト。[key: WAV番号]
        /// </summary>
        private readonly Dictionary<int, VideoContext> _動画リスト;

        private readonly List<Video> _一時停止中の動画のリスト;


        private void _登録する( int AVI番号, VariablePath 動画ファイルの絶対パス, double 再生速度 )
        {
            this._動画リスト[ AVI番号 ] = new VideoContext(
                AVI番号,
                動画ファイルの絶対パス,
                再生速度,
                new Video(
                    Global.GraphicResources.MFDXGIDeviceManager,
                    Global.GraphicResources.既定のD2D1DeviceContext,
                    動画ファイルの絶対パス,
                    再生速度 ) );
        }

        private void _削除する( int AVI番号 )
        {
            if( this._動画リスト.ContainsKey( AVI番号 ) )
            {
                this._動画リスト[ AVI番号 ].動画.Dispose();
                this._動画リスト.Remove( AVI番号 );
            }
        }
    }
}
