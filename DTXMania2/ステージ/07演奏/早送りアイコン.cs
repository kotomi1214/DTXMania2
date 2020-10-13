using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania2.演奏
{
    class 早送りアイコン : IDisposable
    {

        // 生成と終了

        public 早送りアイコン()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._上向きアイコン = new 画像D2D( @"$(Images)\PlayStage\FastForward.png" );
        }

        public virtual void Dispose()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this._上向きアイコン.Dispose();
        }



        // 進行と描画


        public void 早送りする()
        {
            this._早送りカウンタ = new Counter( 0, 30, 10 );
            this._早戻しカウンタ = null;
        }

        public void 早戻しする()
        {
            this._早送りカウンタ = null;
            this._早戻しカウンタ = new Counter( 0, 30, 10 );
        }

        public void 進行描画する( DeviceContext d2ddc )
        {
            if( this._早送りカウンタ != null )
            {
                #region " 早送りアニメ "
                //----------------
                if( this._早送りカウンタ.終了値に達していない )
                {
                    float 上移動量 = this._早送りカウンタ.現在値の割合 * 20f;

                    #region " 影 "
                    //----------------
                    {
                        float 拡大率 = 2f + this._早送りカウンタ.現在値の割合 * 1f;

                        var 変換行列2D =
                            Matrix3x2.Scaling( 拡大率 ) *
                            Matrix3x2.Translation(
                                Global.GraphicResources.設計画面サイズ.Width / 2 - this._上向きアイコン.サイズ.Width * 拡大率 / 2,
                                Global.GraphicResources.設計画面サイズ.Height / 2 - this._上向きアイコン.サイズ.Height * 拡大率 / 2 - 上移動量 );

                        float 不透明度 = ( 1f - this._早送りカウンタ.現在値の割合 ) * 0.25f;

                        this._上向きアイコン.描画する( d2ddc, 変換行列2D, 不透明度0to1: 不透明度 );
                    }
                    //----------------
                    #endregion

                    #region " 本体 "
                    //----------------
                    {
                        float 拡大率 = 2f;

                        var 変換行列2D =
                            Matrix3x2.Scaling( 拡大率 ) *
                            Matrix3x2.Translation(
                                Global.GraphicResources.設計画面サイズ.Width / 2 - this._上向きアイコン.サイズ.Width * 拡大率 / 2,
                                Global.GraphicResources.設計画面サイズ.Height / 2 - this._上向きアイコン.サイズ.Height * 拡大率 / 2 - 上移動量 );

                        float 不透明度 = 1f - this._早送りカウンタ.現在値の割合;

                        this._上向きアイコン.描画する( d2ddc, 変換行列2D, 不透明度0to1: 不透明度 );
                    }
                    //----------------
                    #endregion
                }
                else
                {
                    this._早送りカウンタ = null;
                }
                //----------------
                #endregion
            }
            else if( this._早戻しカウンタ != null )
            {
                #region " 早戻しアニメ "
                //----------------
                if( this._早戻しカウンタ.終了値に達していない )
                {
                    float 下移動量 = this._早戻しカウンタ.現在値の割合 * 20f;

                    #region " 影 "
                    //----------------
                    {
                        float 拡大率 = 2f + this._早戻しカウンタ.現在値の割合 * 1f;
                        float 不透明度 = ( 1f - this._早戻しカウンタ.現在値の割合 ) * 0.25f;

                        var 回転中心 = new Vector2( this._上向きアイコン.サイズ.Width / 2f, this._上向きアイコン.サイズ.Height / 2f );
                        var 変換行列2D =
                            Matrix3x2.Rotation( MathF.PI, 回転中心 ) *    // 回転して下向きに
                            Matrix3x2.Scaling( 拡大率 ) *
                            Matrix3x2.Translation(
                                Global.GraphicResources.設計画面サイズ.Width / 2 - this._上向きアイコン.サイズ.Width * 拡大率 / 2,
                                Global.GraphicResources.設計画面サイズ.Height / 2 - this._上向きアイコン.サイズ.Height * 拡大率 / 2 + 下移動量 );


                        this._上向きアイコン.描画する( d2ddc, 変換行列2D, 不透明度0to1: 不透明度 );
                    }
                    //----------------
                    #endregion

                    #region " 本体 "
                    //----------------
                    {
                        float 拡大率 = 2f;
                        float 不透明度 = 1f - this._早戻しカウンタ.現在値の割合;

                        var 回転中心 = new Vector2( this._上向きアイコン.サイズ.Width / 2f, this._上向きアイコン.サイズ.Height / 2f );
                        var 変換行列2D =
                            Matrix3x2.Rotation( MathF.PI, 回転中心 ) *    // 回転して下向きに
                            Matrix3x2.Scaling( 拡大率 ) *
                            Matrix3x2.Translation(
                                Global.GraphicResources.設計画面サイズ.Width / 2 - this._上向きアイコン.サイズ.Width * 拡大率 / 2,
                                Global.GraphicResources.設計画面サイズ.Height / 2 - this._上向きアイコン.サイズ.Height * 拡大率 / 2 + 下移動量 );

                        this._上向きアイコン.描画する( d2ddc, 変換行列2D, 不透明度0to1: 不透明度 );
                    }
                    //----------------
                    #endregion
                }
                else
                {
                    this._早戻しカウンタ = null;
                }
                //----------------
                #endregion
            }
        }



        // ローカル


        private readonly 画像D2D _上向きアイコン;

        private Counter? _早送りカウンタ = null;

        private Counter? _早戻しカウンタ = null;
    }
}
