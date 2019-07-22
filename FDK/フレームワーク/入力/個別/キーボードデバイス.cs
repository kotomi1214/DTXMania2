using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX.Multimedia;

namespace FDK
{
    /// <summary>
    ///     Raw Input で HID キーボードの入力を扱う。
    /// </summary>
    public class キーボードデバイス : IInputDevice, IDisposable
    {
        public InputDeviceType 入力デバイス種別 => InputDeviceType.Keyboard;

        public List<InputEvent> 入力イベントリスト { get; protected set; } = new List<InputEvent>();


        /// <summary>
        ///     キーボードの Raw Input を登録し、受信を開始する。
        /// </summary>
        public キーボードデバイス()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 登録したいデバイスの配列（ここでは１個）
                var devs = new RawInput.RawInputDevice[] {
                    new RawInput.RawInputDevice {
                        usUsagePage = UsagePage.Generic,
                        usUsage = UsageId.GenericKeyboard,
                        Flags = RawInput.DeviceFlags.None,
                        hwndTarget = IntPtr.Zero,
                    }
                };

                // デバイスを登録。
                RawInput.RegisterRawInputDevices( devs, 1, Marshal.SizeOf<RawInput.RawInputDevice>() );
            }
        }

        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 特になし
            }
        }

        /// <summary>
        ///     キーボードHID からの WM_INPUT のコールバック。
        /// </summary>
        /// <remarks>
        ///     ウィンドウメッセージループで WM_INPUT を受信した場合は、このコールバックを呼び出すこと。
        /// </remarks>
        public void WM_INPUTを処理する( in System.Windows.Forms.Message wmInputMsg )
        {
            RawInput.RawInputData rawInput;
            int csSize = Marshal.SizeOf<RawInput>();

            // データ取得。
            if( 0 > RawInput.GetRawInputData( wmInputMsg.LParam, RawInput.DataType.Input, out rawInput, ref csSize, Marshal.SizeOf<RawInput.RawInputHeader>() ) )
            {
                //Debug.WriteLine( "WM_INPUT でのデータ取得に失敗しました。" );
                return;
            }
            if( rawInput.Header.Type != RawInput.DeviceType.Keyboard )
            {
                //Debug.WriteLine( "未登録の（キーボード以外の）デバイスからのデータが返されました。" );
                return;
            }

            var keyboard = rawInput.Data.Keyboard;

            // InputEvent 作成。
            var inputEvent = new InputEvent() {
                DeviceID = 0,         // 固定
                Key = keyboard.VKey,  // 仮想キーコード(VK_*)
                押された = ( RawInput.ScanCodeFlags.Make == ( keyboard.Flags & RawInput.ScanCodeFlags.Break ) ),
                Velocity = 255,       // 固定
                TimeStamp = Stopwatch.GetTimestamp(),
                Extra = keyboard.ExtraInformation.ToString( "X8" ),
            };

            lock( this._一時入力イベントリスト )
            {
                // 一時リストに追加。
                this._一時入力イベントリスト.Add( inputEvent );

                // キーの状態を更新。
                this._現在のキーの押下状態[ inputEvent.Key ] = inputEvent.押された;
            }
        }

        public void ポーリングする()
        {
            this.入力イベントリスト.Clear();

            lock( this._一時入力イベントリスト )
            {
                this.入力イベントリスト = this._一時入力イベントリスト; // 一時リストへの参照を直接渡して、
                this._一時入力イベントリスト = new List<InputEvent>();  // 一時リストは新しく確保。
            }
        }

        public bool キーが押された( int deviceID, int key )
            => this.キーが押された( deviceID, key, out _ );

        public bool キーが押された( int deviceID, int key, out InputEvent ev )
        {
            lock( this._一時入力イベントリスト )
                ev = this.入力イベントリスト.Find( ( item ) => ( item.Key == key && item.押された ) );

            return ( null != ev );
        }

        public bool キーが押された( int deviceID, System.Windows.Forms.Keys key )
            => this.キーが押された( deviceID, (int) key, out _ );

        public bool キーが押された( int deviceID, System.Windows.Forms.Keys key, out InputEvent ev )
            => this.キーが押された( deviceID, (int) key, out ev );

        public bool キーが押されている( int deviceID, int key )
        {
            lock( this._一時入力イベントリスト )
                return ( this._現在のキーの押下状態.TryGetValue( key, out bool 押されている ) ) ? 押されている : false;
        }

        public bool キーが押されている( int deviceID, System.Windows.Forms.Keys key )
            => this.キーが押されている( deviceID, (int) key );

        public bool キーが離された( int deviceID, int key )
            => this.キーが離された( deviceID, key, out _ );

        public bool キーが離された( int deviceID, int key, out InputEvent ev )
        {
            lock( this._一時入力イベントリスト )
                ev = this.入力イベントリスト.Find( ( item ) => ( item.Key == key && item.離された ) );

            return ( null != ev );
        }

        public bool キーが離された( int deviceID, System.Windows.Forms.Keys key )
            => this.キーが離された( deviceID, (int) key, out _ );

        public bool キーが離された( int deviceID, System.Windows.Forms.Keys key, out InputEvent ev )
            => this.キーが離された( deviceID, (int) key, out ev );

        public bool キーが離されている( int deviceID, int key )
        {
            lock( this._一時入力イベントリスト )
                return ( this._現在のキーの押下状態.TryGetValue( key, out bool 押されている ) ) ? !( 押されている ) : true;
        }

        public bool キーが離されている( int deviceID, System.Windows.Forms.Keys key )
            => this.キーが離されている( deviceID, (int) key );


        /// <summary>
        ///     <see cref="AddRawData(SharpDX.RawInput.KeyboardInputEventArgs)"/> で受け取ったイベントを一時的に蓄えておくリスト。
        ///     <see cref="ポーリングする"/> の実行で、内容を <see cref="入力イベントリスト"/> にコピーしたのち、クリアされる。
        /// </summary>
        private List<InputEvent> _一時入力イベントリスト = new List<InputEvent>();

        /// <summary>
        ///	    現在のキーの押下状態。
        ///	    [key: 仮想キーコードをintにしたもの]
        ///	    true なら押されている状態、false なら離されている状態。
        /// </summary>
        private Dictionary<int, bool> _現在のキーの押下状態 = new Dictionary<int, bool>();
    }
}
