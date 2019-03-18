using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace FDK
{
    public static class Extensions
    {
        // SharpDX.IUnknown の拡張メソッド

        /// <summary>
        ///		COM オブジェクトの参照カウントを取得して返す。
        /// </summary>
        /// <param name="unknownObject">COMオブジェクト。</param>
        /// <returns>現在の参照カウントの値。</returns>
        public static int GetRefferenceCount( this IUnknown unknownObject )
        {
            try
            {
                unknownObject.AddReference();
            }
            catch( InvalidOperationException )
            {
                // すでに Dispose されている。
                return 0;
            }

            return unknownObject.Release();
        }

        /// <summary>
        ///     指定された型のインターフェースを取得する。
        /// </summary>
        /// <typeparam name="T">希望する型。</typeparam>
        /// <param name="unknownObject">取得元のオブジェクト。</param>
        /// <returns>
        ///     取得されたオブジェクト。クエリに失敗した場合は null 。
        /// </returns>
        public static T QueryInterface<T>( this IUnknown unknownObject ) where T : ComObject
        {
            if( null == unknownObject )
                return null;

            var guid = SharpDX.Utilities.GetGuidFromType( typeof( T ) );
            var resuult = unknownObject.QueryInterface( ref guid, out IntPtr comObject );

            return ( resuult.Failure ) ? null : CppObject.FromPointer<T>( comObject );
        }

        // System.String の拡張メソッド

        /// <summary>
        ///		文字列が Null でも空でもないなら true を返す。
        /// </summary>
        public static bool Nullでも空でもない( this string 検査対象 )
            => !( string.IsNullOrEmpty( 検査対象 ) );

        /// <summary>
        ///		文字列が Null または空なら true を返す。
        /// </summary>
        public static bool Nullまたは空である( this string 検査対象 )
            => string.IsNullOrEmpty( 検査対象 );


        // SharpDX.Size2F の拡張メソッド

        /// <summary>
        ///		SharpDX.Size2F を System.Drawing.SizeF へ変換する。
        /// </summary>
        public static System.Drawing.SizeF ToDrawingSizeF( this SharpDX.Size2F size )
            => new System.Drawing.SizeF( size.Width, size.Height );

        /// <summary>
        ///		SharpDX.Size2F を System.Drawing.Size へ変換する。
        /// </summary>
        public static System.Drawing.Size ToDrawingSize( this SharpDX.Size2F size )
            => new System.Drawing.Size( (int) size.Width, (int) size.Height );


        // SharpDX.Size2 の拡張メソッド

        /// <summary>
        ///		SharpDX.Size2 を System.Drawing.SizeF へ変換する。
        /// </summary>
        public static System.Drawing.SizeF ToDrawingSizeF( this SharpDX.Size2 size )
            => new System.Drawing.SizeF( size.Width, size.Height );

        /// <summary>
        ///		SharpDX.Size2 を System.Drawing.Size へ変換する。
        /// </summary>
        public static System.Drawing.Size ToDrawingSize( this SharpDX.Size2 size )
            => new System.Drawing.Size( size.Width, size.Height );


        // SharpDX.Direct2D1.RenderTarget の拡張メソッド

        /// <summary>
        ///     指定した領域をクリアする。
        /// </summary>
        /// <param name="rt">レンダーターゲット。</param>
        /// <param name="color">クリアする色。</param>
        /// <param name="rect">クリアする領域。</param>
        public static void Clear( this SharpDX.Direct2D1.RenderTarget rt, Color color, SharpDX.Mathematics.Interop.RawRectangleF rect )
        {
            rt.PushAxisAlignedClip( rect, SharpDX.Direct2D1.AntialiasMode.PerPrimitive );
            rt.Clear( color );
            rt.PopAxisAlignedClip();
        }
    }
}
