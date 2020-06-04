using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FDK;

namespace DTXMania2
{
    public partial class AppForm : Form
    {

        // プロパティ


        /// <summary>
        ///     アプリケーション再起動指示フラグ。
        /// </summary>
        /// <remarks>
        ///     <see cref="AppForm"/> インスタンスの終了時にこのフラグが true になっている場合には、
        ///     このインスタンスの保持者（おそらくProgramクラス）は適切に再起動を行うこと。
        /// </remarks>
        public bool 再起動が必要 { get; protected set; } = false;



        // 生成と終了


        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public AppForm()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     アプリケーションの起動処理を行う。
        /// </summary>
        protected override void OnLoad( EventArgs e )
        {
            Log.Header( "アプリケーション起動" );
            using var _ = new LogBlock( Log.現在のメソッド名 );

            // フォームを設定する。
            this.Text = $"DTXMania2 Release {int.Parse( Application.ProductVersion.Split( '.' ).ElementAt( 0 ) ):000}";
            this.ClientSize = new Size( 1024, 576 );
            this.Icon = Properties.Resources.DTXMania2;
            this._ScreenMode = new ScreenMode( this );

            // 入力デバイスを初期化する。
            this._KeyboardHID = new KeyboardHID();
            this._GameControllersHID = new GameControllersHID( this.Handle );
            this._MidiIns = new MidiIns();

            // 初期化完了。
            this._未初期化 = false;

            base.OnLoad( e );
        }

        /// <summary>
        ///     アプリケーションの終了処理を行う。
        /// </summary>
        protected override void OnClosing( CancelEventArgs e )
        {
            Log.Header( "アプリケーション終了" );
            using var _ = new LogBlock( Log.現在のメソッド名 );

            // 入力デバイスを破棄する。
            this._MidiIns.Dispose();
            this._GameControllersHID.Dispose();
            this._KeyboardHID.Dispose();

            // 未初期化状態へ。
            this._未初期化 = true;

            base.OnClosing( e );
        }

        /// <summary>
        ///     再起動フラグをセットして、アプリケーションを終了する。
        /// </summary>
        public void 再起動する()
        {
            using var _ = new LogBlock( Log.現在のメソッド名 );

            this.再起動が必要 = true;
            this.Close();
        }



        // ローカル


        /// <summary>
        ///     アプリの初期化が完了していなければ true。
        ///     起動直後は true, OnLoad() で false, OnClosing() で true になる。
        /// </summary>
        private bool _未初期化 = true;

        /// <summary>
        ///     画面モード（ウィンドウ、全画面）。
        /// </summary>
        private ScreenMode _ScreenMode = null!;

        /// <summary>
        ///     HIDキーボード入力。
        /// </summary>
        /// <remarks>
        ///     接続されているHIDキーボードを管理する。
        ///     RawInputを使うので、フォームのスレッドで管理する。（RawInputはUIスレッドでのみ動作する）
        /// </remarks>
        private KeyboardHID _KeyboardHID = null!;

        /// <summary>
        ///     すべてのHIDゲームパッド、HIDジョイスティック入力。
        /// </summary>
        /// <remarks>
        ///     すべてのゲームパッド／ジョイスティックデバイスを管理する（<see cref="GameControllersHID.Devices"/>参照）。
        ///     RawInputを使うので、フォームのスレッドで管理する。（RawInputはUIスレッドでのみ動作する）
        /// </remarks>
        private GameControllersHID _GameControllersHID = null!;

        /// <summary>
        ///     すべてのMIDI入力デバイス。
        /// </summary>
        private MidiIns _MidiIns = null!;
    }
}
