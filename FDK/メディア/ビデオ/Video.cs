using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct2D1;

namespace FDK
{
    public class Video : IDisposable
    {
        public bool 加算合成 { get; set; } = false;

        public bool 再生中 { get; protected set; } = false;

        public double 再生速度 { get; protected set; } = 1.0;


        public Video( VariablePath ファイルパス, double 再生速度 = 1.0 )
        {
            this.再生速度 = 再生速度;
            try
            {
                this._VideoSource = new MediaFoundationFileVideoSource( ファイルパス, 再生速度 );
            }
            catch
            {
                Log.WARNING( $"動画のデコードに失敗しました。[{ファイルパス.変数付きパス}" );
                this._VideoSource = null;
                return;
            }

            this._ファイルから生成した = true;
        }

        public Video( IVideoSource videoSource, double 再生速度 = 1.0 )
        {
            this.再生速度 = 再生速度;

            this._VideoSource = videoSource;
            this._ファイルから生成した = false;
        }

        public void Dispose()
        {
            this.再生を終了する();

            if( this._ファイルから生成した )
                this._VideoSource?.Dispose();

            this._VideoSource = null;
        }

        public void 再生を開始する( double 再生開始時刻sec = 0.0 )
        {
            this._VideoSource?.Start( 再生開始時刻sec );

            this._再生タイマ = new QPCTimer();
            this._再生タイマ.リセットする( QPCTimer.秒をカウントに変換して返す( 再生開始時刻sec ) );

            this.再生中 = true;
        }

        public void 再生を終了する()
        {
            this._VideoSource?.Stop();

            this._最後に描画したフレーム?.Dispose();
            this._最後に描画したフレーム = null;

            this.再生中 = false;
        }

        public void 描画する( DeviceContext1 dc, RectangleF 描画先矩形, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            var 変換行列2D =
                Matrix3x2.Scaling( 描画先矩形.Width / this._VideoSource.フレームサイズ.Width, 描画先矩形.Height / this._VideoSource.フレームサイズ.Height ) *  // 拡大縮小
                Matrix3x2.Translation( 描画先矩形.Left, 描画先矩形.Top );  // 平行移動

            this.描画する( dc, 変換行列2D, 不透明度0to1 );
        }

        public void 描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            long 次のフレームの表示予定時刻100ns = this._VideoSource.Peek(); // 次のフレームがなければ負数

            if( 0 <= 次のフレームの表示予定時刻100ns )
            {
                if( ( null != this._最後に描画したフレーム ) && ( 次のフレームの表示予定時刻100ns < this._最後に描画したフレーム.表示時刻100ns ) )
                {
                    // (A) 次のフレームが前のフレームより過去 → ループしたので、タイマをリセットしてから描画する。
                    this._再生タイマ.リセットする( QPCTimer.秒をカウントに変換して返す( FDKUtilities.変換_100ns単位からsec単位へ( 次のフレームの表示予定時刻100ns ) ) );
                    this._次のフレームを読み込んで描画する( dc, 変換行列2D, 不透明度0to1 );
                }
                else if( 次のフレームの表示予定時刻100ns <= this._再生タイマ.現在のリアルタイムカウント100ns )
                {
                    // (B) 次のフレームの表示時刻に達したので描画する。
                    this._次のフレームを読み込んで描画する( dc, 変換行列2D, 不透明度0to1 );
                }
                else
                {
                    // (C) 次のフレームの表示時刻にはまだ達していない → 最後に描画したフレームを再描画しておく
                    this.最後のフレームを再描画する( dc, 変換行列2D, 不透明度0to1 );
                }
            }
            else
            {
                // (D) デコードが追い付いてない、またはループせず再生が終わっている　→ 何も表示しない。デコードが追い付いてないなら点滅するだろう。
                this._最後に描画したフレーム?.Dispose();
                this._最後に描画したフレーム = null;
            }
        }

        public void 最後のフレームを再描画する( DeviceContext1 dc, RectangleF 描画先矩形, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            var 変換行列2D =
                Matrix3x2.Scaling( 描画先矩形.Width / this._VideoSource.フレームサイズ.Width, 描画先矩形.Height / this._VideoSource.フレームサイズ.Height ) *  // 拡大縮小
                Matrix3x2.Translation( 描画先矩形.Left, 描画先矩形.Top );  // 平行移動

            this.最後のフレームを再描画する( dc, 変換行列2D, 不透明度0to1 );
        }

        public void 最後のフレームを再描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1 = 1.0f )
        {
            if( null == this._最後に描画したフレーム )
                return;

            this._フレームを描画する( dc, 変換行列2D, 不透明度0to1, this._最後に描画したフレーム );
        }


        private IVideoSource _VideoSource = null;

        /// <summary>
        ///     <see cref="_VideoSource"/> をファイルから生成した場合は true、
        ///     参照を受け取った場合は false。
        /// </summary>
        private bool _ファイルから生成した = false;

        private VideoFrame _最後に描画したフレーム = null;

        private QPCTimer _再生タイマ = null;


        private void _次のフレームを読み込んで描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1 = 1.0f )
        {
            if( null == this._VideoSource )
                return;

            VideoFrame 次のフレーム = null;

            while( true )
            {
                // デコードが間に合ってない場合にはブロックする。
                // ブロックされたくない場合は、事前に Peek() でチェックしておくこと。
                次のフレーム = this._VideoSource.Read();

                // フレームドロップ判定；
                // 次の次のフレームがすでに表示時刻を過ぎてるなら、次のフレームは破棄してループする。

                var 次の次のフレームの表示予定時刻100ns = this._VideoSource.Peek();

                if( 0 > 次の次のフレームの表示予定時刻100ns )
                {
                    // (A) 次の次のフレームがまだない場合　→　ドロップ判定ループを抜けて描画へ。
                    break;
                }
                else
                {
                    if( 次の次のフレームの表示予定時刻100ns <= this._再生タイマ.現在のリアルタイムカウント100ns )
                    {
                        // (B) 次の次のフレームが存在し、かつ表示予定時刻を過ぎてる場合　→　次のフレームは破棄してさらに次へ。
                        次のフレーム.Dispose();
                        continue;
                    }
                    else
                    {
                        // (C) 次の次のフレームが存在し、かつ表示予定時刻をまだすぎてない場合　→　ドロップ判定ループを抜けて描画へ。
                        break;
                    }
                }
            }

            // 描画。
            this._フレームを描画する( dc, 変換行列2D, 不透明度0to1, 次のフレーム );

            // 更新。
            this._最後に描画したフレーム?.Dispose();
            this._最後に描画したフレーム = 次のフレーム;
        }

        private void _フレームを描画する( DeviceContext1 dc, Matrix3x2 変換行列2D, float 不透明度0to1, VideoFrame 描画するフレーム )
        {
            if( null == 描画するフレーム )
                return;

            グラフィックデバイス.Instance.D2DBatchDraw( dc, () => {
                dc.Transform = ( 変換行列2D ) * dc.Transform;
                dc.PrimitiveBlend = ( this.加算合成 ) ? PrimitiveBlend.Add : PrimitiveBlend.SourceOver;
                dc.DrawBitmap( 描画するフレーム.Bitmap, 不透明度0to1, InterpolationMode.NearestNeighbor );
            } );
        }
    }
}
