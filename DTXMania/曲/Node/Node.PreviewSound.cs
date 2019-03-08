using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
                    return; // 未使用。

                // 再生までの時間を稼ぐタイマを作成。
                this._再生タイマ?.Dispose();
                this._再生タイマ = new System.Threading.Timer( this._再生タイマ_Tick, null, 500, System.Threading.Timeout.Infinite );

                Interlocked.Increment( ref _再生番号 );    // +1

                this._音声ファイルパス = 音声ファイルパス;
            }

            public void 停止する()
            {
                this._再生タイマ?.Dispose(); // タイマは停止するが、すでに Tick コールバックが始まってたら、そちらは止まらない。
                this._再生タイマ = null;

                this.Sound?.Stop();
            }


            private string _音声ファイルパス = null;

            /// <summary>
            ///		プレビューサウンドで使うサウンドソース（サウンドデータ）。
            ///		プレビューサウンド未使用、またはサウンドソースの生成に失敗した場合は null。
            /// </summary>
            private CSCore.ISampleSource _SampleSource = null;

            //private System.Windows.Forms.Timer _再生タイマ = null;     --> UIスレッドから呼び出してるのになぜかTickされないので、System.Threading.Timer に変更。
            private System.Threading.Timer _再生タイマ = null;

            /// <summary>
            ///     再生する（新しい再生タイマが開始する）たびに +1 されていくカウンタ。
            /// </summary>
            /// <remarks>
            ///     Tick コールバックの多重実行によるプレビュー音声の多重再生を防止するために使う。
            ///     新しい再生タイマが開始される直前に古いタイマが破棄されるが、その時点で既に Tick コールバックが
            ///     呼び出されていた（プレビュー音声の読み込みが始まっていた）場合は、その実行は止められない。
            ///     そこで、Tick コールバック内でのプレビュー音声の読み込み前と後とでこのカウンタ値を比較し、
            ///     両者が等しければその音声の再生を許可するものとする。
            ///     もし読み込み前後でカウンタ値が異なっていれば、それは読み込み中に新しいタイマが開始されたことを示しているので、
            ///     その音声の再生はキャンセルするものとする。
            /// </remarks>
            private static long _再生番号 = 0;


            /// <summary>
            ///     再生タイマのコールバック。
            /// </summary>
            /// <param name="state">未使用</param>
            private void _再生タイマ_Tick( object state )
            {
                // プレビュー音声を読み込んで再生する。

                var 生成前の再生番号 = Interlocked.Read( ref _再生番号 );

                this._プレビュー音声を生成する();

                var 生成後の再生番号 = Interlocked.Read( ref _再生番号 );


                // 読み込み前後の再生番号を比較し、等しいなら再生を開始し、異なるなら何もしない（再生をキャンセル）。

                if( 生成前の再生番号 == 生成後の再生番号 )
                {
                    // 再生開始。
                    this.Sound?.Play( ループ再生する: true );
                }
                else
                {
                    // 再生中止。
                }
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
