using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using FDK;

namespace DTXMania2
{
    /// <summary>
    ///     グローバルリソース。すべて static。
    /// </summary>
    static class Global
    {

        // グローバルプロパティ


        /// <summary>
        ///     メインフォームインスタンスへの参照。
        /// </summary>
        public static App App { get; set; } = null!;

        /// <summary>
        ///     メインフォームインスタンスのウィンドウハンドル。
        /// </summary>
        /// <remarks>
        ///     <see cref="DTXMania2.App.Handle"/> メンバと同じ値であるが、GUIスレッド 以 外 のスレッドから参照する場合は、
        ///     <see cref="DTXMania2.App.Handle"/> ではなくこのメンバを参照すること。
        ///     （<see cref="DTXMania2.App"/> のメンバはすべて、必ずGUIスレッドから参照されなれければならないため。）
        /// </remarks>
        public static IntPtr Handle { get; set; } = IntPtr.Zero;

        /// <summary>
        ///     アプリ起動時のコマンドラインオプション。
        /// </summary>
        /// <remarks>
        ///     <see cref="Program.Main(string[])"/> の引数から生成される。
        ///     YAML化することで、ビュアーモードで起動中の DTXMania2 のパイプラインサーバに送信する事が可能。
        /// </remarks>
        public static CommandLineOptions Options { get; set; } = null!;

        /// <summary>
        ///     設計（プログラム側）で想定する固定画面サイズ[dpx]。
        /// </summary>
        /// <remarks>
        ///     物理画面サイズはユーザが自由に変更できるが、プログラム側では常に設計画面サイズを使うことで、
        ///     物理画面サイズに依存しない座標をハードコーディングできるようにする。
        ///     プログラム内では設計画面におけるピクセルの単位として「dpx (designed pixel)」と称することがある。
        ///     なお、int より float での利用が多いので、Size や Size2 ではなく Size2F を使う。
        ///     （int 同士ということを忘れて、割り算しておかしくなるケースも多発したので。）
        /// </remarks>
        public static SharpDX.Size2F 設計画面サイズ { get; private set; }

        /// <summary>
        ///     モニタに実際に表示される画面のサイズ[px]。
        /// </summary>
        /// <remarks>
        ///     物理画面サイズは、スワップチェーンのサイズを表す。
        ///     物理画面サイズは、ユーザが自由に変更することができるため、固定値ではないことに留意。
        ///     プログラム内では物理画面におけるピクセルの単位として「px (physical pixel)」と称することがある。
        ///     なお、int より float での利用が多いので、Size や Size2 ではなく Size2F を使う。
        ///     （int 同士ということを忘れて、割り算しておかしくなるケースも多発したので。）
        /// </remarks>
        public static SharpDX.Size2F 物理画面サイズ { get; private set; }



        // 生成と終了


        /// <summary>
        ///     各種グローバルリソースを生成する。
        /// </summary>
        public static void 生成する( App appForm, SharpDX.Size2F 設計画面サイズ, SharpDX.Size2F 物理画面サイズ )
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            Global.App = appForm;
            Global.Handle = appForm.Handle;

            Global.設計画面サイズ = 設計画面サイズ;
            Global.物理画面サイズ = 物理画面サイズ;
            Log.Info( $"設計画面サイズ: {設計画面サイズ}" );
            Log.Info( $"物理画面サイズ: {物理画面サイズ}" );

            //Global._スワップチェーンに依存しないグラフィックリソースを作成する();
            //Global._スワップチェーンを作成する();
            //Global._スワップチェーンに依存するグラフィックリソースを作成する();

            Global._Dispose済み = false;
        }

        /// <summary>
        ///     各種グローバルリソースを開放する。
        /// </summary>
        public static void 解放する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            #region " Dispose 済みなら何もしない。"
            //----------------
            if( Global._Dispose済み )
                return;

            Global._Dispose済み = true;
            //----------------
            #endregion

            //Global._スワップチェーンに依存するグラフィックリソースを解放する();
            //Global._スワップチェーンを解放する();
            //Global._スワップチェーンに依存しないグラフィックリソースを解放する();

            Global.Handle = IntPtr.Zero;
        }



        // ローカル


        private static bool _Dispose済み = true;
    }
}
