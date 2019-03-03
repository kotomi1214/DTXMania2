using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.曲
{
	partial class Node
	{
        /// <summary>
        ///     プレビュー音声の再生と停止。
        /// </summary>
		class PreviewSound : Activity
		{
            /// <summary>
            ///		プレビューサウンド。
            ///		未使用なら null 。
            /// </summary>
            public Sound Sound { get; protected set; } = null;


            public PreviewSound()
            {
            }

            protected override void On活性化()
            {
                this.Sound = null;
                this._SampleSource = null;
            }

            protected override void On非活性化()
            {
                this.Sound?.Stop();
                this.Sound?.Dispose();
                this.Sound = null;

                this._SampleSource?.Dispose();
                this._SampleSource = null;
            }

            /// <summary>
            ///		500ミリ秒待ってから、プレビュー音声を生成し、ループ再生する。
            /// </summary>
            public void 再生する( string 音声ファイルパス )
            {
                if( 音声ファイルパス?.Nullまたは空である() ?? true )
                    return;	// 未使用。

				// 再生までの時間を稼ぐタイマを作成。
                this._再生タイマ?.Dispose();
                this._再生タイマ = new System.Windows.Forms.Timer() { Interval = 500 };   // ミリ秒
                this._再生タイマ.Tick += this._再生タイマ_Tick;
                this._再生タイマ.Start();

                this._音声ファイルパス = 音声ファイルパス;
            }

            public void 停止する()
            {
                this._再生タイマ?.Stop();
                this._再生タイマ?.Dispose();
                this._再生タイマ = null;

                this.Sound?.Stop();
            }


            private string _音声ファイルパス = null;

            /// <summary>
            ///		プレビューサウンドで使うサウンドソース（サウンドデータ）。
            ///		プレビューサウンド未使用、またはサウンドソースの生成に失敗した場合は null。
            /// </summary>
            private CSCore.ISampleSource _SampleSource = null;

            private System.Windows.Forms.Timer _再生タイマ = null;


            private void _再生タイマ_Tick( object sender, EventArgs e )
            {
                // このタスクはメインスレッドで処理される（排他処理不要）。

                // (1) プレビュー音声を読み込んで再生する。
                this._プレビュー音声を生成する();
                this.Sound?.Play( ループ再生する: true );

                // (2) タイマ停止。２回目以降の Tick はない。
                this._再生タイマ.Stop();
            }

            /// <summary>
            ///     <see cref="_音声ファイルパス"/>から、<see cref="_SampleSource"/>と<see cref="Sound"/> を生成する。
            /// </summary>
            private void _プレビュー音声を生成する()
            {
                if( null != this.Sound )
                    return;  // 生成済み

                // (1) サンプルソースを生成する。
                if( null == this._SampleSource )
                {
                    // 未生成の場合、生成する。
                    this._SampleSource = SampleSourceFactory.Create(
                        App.サウンドデバイス,
                        new VariablePath( this._音声ファイルパス ).変数なしパス,
                        再生速度: 1.0 );   // プレビューは常に再生速度 = 1.0

                    if( null == this._SampleSource )
                        return;
                }

                // (2) サンプルソースからサウンドを生成する。
                this.Sound = new Sound( App.サウンドデバイス, this._SampleSource );
            }
        }
    }
}
