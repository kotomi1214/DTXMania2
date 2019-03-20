using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using FDK;

namespace DTXMania
{
    partial class システム設定
    {
        internal class キーバインディング : ICloneable
        {
            /// <summary>
            ///		入力コードのマッピング用 Dictionary のキーとなる型。
            ///		入力は、デバイスID（入力デバイスの内部識別用ID; FDKのIInputEvent.DeviceIDと同じ）と、
            ///		キー（キーコード、ノート番号などデバイスから得られる入力値）の組で定義される。
            /// </summary>
            public struct IdKey : IYamlConvertible
            {
                public int deviceId { get; set; }
                public int key { get; set; }

                public IdKey( int deviceId, int key )
                {
                    this.deviceId = deviceId;
                    this.key = key;
                }
                public IdKey( InputEvent ie )
                {
                    this.deviceId = ie.DeviceID;
                    this.key = ie.Key;
                }
                public IdKey( string 文字列 )
                {
                    // 変なの食わせたらそのまま例外発出する。
                    string[] v = 文字列.Split( new char[] { ',' } );

                    this.deviceId = int.Parse( v[ 0 ] );
                    this.key = int.Parse( v[ 1 ] );
                }
                public override string ToString()
                    => $"{this.deviceId},{this.key}";

                void IYamlConvertible.Read( IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer )
                {
                    var devkey = (string) nestedObjectDeserializer( typeof( string ) );

                    string 正規表現パターン = $@"^(\d+),(\d+)$";  // \d は10進数数字
                    var m = Regex.Match( devkey, 正規表現パターン, RegexOptions.IgnoreCase );

                    if( m.Success && ( 3 <= m.Groups.Count ) )
                    {
                        this.deviceId = int.Parse( m.Groups[ 1 ].Value );
                        this.key = int.Parse( m.Groups[ 2 ].Value );
                    }
                }
                void IYamlConvertible.Write( IEmitter emitter, ObjectSerializer nestedObjectSerializer )
                {
                    nestedObjectSerializer( $"{this.deviceId},{this.key}" );
                }
            }

            /// <summary>
            ///		MIDI番号(0～7)とMIDIデバイス名のマッピング用 Dictionary。
            /// </summary>
            public Dictionary<int, string> MIDIデバイス番号toデバイス名 { get; protected set; }

            /// <summary>
            ///		キーボードの入力（<see cref="System.Windows.Forms.Keys"/>）からドラム入力へのマッピング用 Dictionary 。
            /// </summary>
            [Description( "キーボードの入力割り当て（デバイスID,キーID: ドラム入力種別）" )]
            public Dictionary<IdKey, ドラム入力種別> キーボードtoドラム { get; protected set; }


            /// <summary>
            ///		MIDI入力の入力（MIDIノート番号）からドラム入力へのマッピング用 Dictionary 。
            /// </summary>
            public Dictionary<IdKey, ドラム入力種別> MIDItoドラム { get; protected set; }

            public int FootPedal最小値 { get; set; }

            public int FootPedal最大値 { get; set; }


            /// <summary>
            ///		コンストラクタ。
            /// </summary>
            public キーバインディング()
            {
                this.FootPedal最小値 = 0;
                this.FootPedal最大値 = 90; // VH-11 の Normal Resolution での最大値

                this.MIDIデバイス番号toデバイス名 = new Dictionary<int, string>();

                this.キーボードtoドラム = new Dictionary<IdKey, ドラム入力種別>() {
                { new IdKey( 0, (int) Keys.Q ),      ドラム入力種別.LeftCrash },
                { new IdKey( 0, (int) Keys.Return ), ドラム入力種別.LeftCrash },
                { new IdKey( 0, (int) Keys.A ),      ドラム入力種別.HiHat_Open },
                { new IdKey( 0, (int) Keys.Z ),      ドラム入力種別.HiHat_Close },
                { new IdKey( 0, (int) Keys.S ),      ドラム入力種別.HiHat_Foot },
                { new IdKey( 0, (int) Keys.X ),      ドラム入力種別.Snare },
                { new IdKey( 0, (int) Keys.C ),      ドラム入力種別.Bass },
                { new IdKey( 0, (int) Keys.Space ),  ドラム入力種別.Bass },
                { new IdKey( 0, (int) Keys.V ),      ドラム入力種別.Tom1 },
                { new IdKey( 0, (int) Keys.B ),      ドラム入力種別.Tom2 },
                { new IdKey( 0, (int) Keys.N ),      ドラム入力種別.Tom3 },
                { new IdKey( 0, (int) Keys.M ),      ドラム入力種別.RightCrash },
                { new IdKey( 0, (int) Keys.K ),      ドラム入力種別.Ride },
            };

                this.MIDItoドラム = new Dictionary<IdKey, ドラム入力種別>() {
				// うちの環境(2017.6.11)
				{ new IdKey( 0,  36 ), ドラム入力種別.Bass },
                { new IdKey( 0,  30 ), ドラム入力種別.RightCrash },
                { new IdKey( 0,  29 ), ドラム入力種別.RightCrash },
                { new IdKey( 1,  51 ), ドラム入力種別.RightCrash },
                { new IdKey( 1,  52 ), ドラム入力種別.RightCrash },
                { new IdKey( 1,  57 ), ドラム入力種別.RightCrash },
                { new IdKey( 0,  52 ), ドラム入力種別.RightCrash },
                { new IdKey( 0,  43 ), ドラム入力種別.Tom3 },
                { new IdKey( 0,  58 ), ドラム入力種別.Tom3 },
                { new IdKey( 0,  42 ), ドラム入力種別.HiHat_Close },
                { new IdKey( 0,  22 ), ドラム入力種別.HiHat_Close },
                { new IdKey( 0,  26 ), ドラム入力種別.HiHat_Open },
                { new IdKey( 0,  46 ), ドラム入力種別.HiHat_Open },
                { new IdKey( 0,  44 ), ドラム入力種別.HiHat_Foot },
                { new IdKey( 0, 255 ), ドラム入力種別.HiHat_Control },	// FDK の MidiIn クラスは、FootControl を ノート 255 として扱う。
				{ new IdKey( 0,  48 ), ドラム入力種別.Tom1 },
                { new IdKey( 0,  50 ), ドラム入力種別.Tom1 },
                { new IdKey( 0,  49 ), ドラム入力種別.LeftCrash },
                { new IdKey( 0,  55 ), ドラム入力種別.LeftCrash },
                { new IdKey( 1,  48 ), ドラム入力種別.LeftCrash },
                { new IdKey( 1,  49 ), ドラム入力種別.LeftCrash },
                { new IdKey( 1,  59 ), ドラム入力種別.LeftCrash },
                { new IdKey( 0,  45 ), ドラム入力種別.Tom2 },
                { new IdKey( 0,  47 ), ドラム入力種別.Tom2 },
                { new IdKey( 0,  51 ), ドラム入力種別.Ride },
                { new IdKey( 0,  59 ), ドラム入力種別.Ride },
                { new IdKey( 0,  38 ), ドラム入力種別.Snare },
                { new IdKey( 0,  40 ), ドラム入力種別.Snare },
                { new IdKey( 0,  37 ), ドラム入力種別.Snare },
            };
            }

            /// <summary>
            ///     メンバを共有しない深いコピーを返す。
            /// </summary>
            public object Clone()
            {
                var clone = new キーバインディング();


                clone.MIDIデバイス番号toデバイス名 = new Dictionary<int, string>();

                foreach( var kvp in this.MIDIデバイス番号toデバイス名 )
                    clone.MIDIデバイス番号toデバイス名.Add( kvp.Key, kvp.Value );

                clone.キーボードtoドラム = new Dictionary<IdKey, ドラム入力種別>();

                foreach( var kvp in this.キーボードtoドラム )
                    clone.キーボードtoドラム.Add( kvp.Key, kvp.Value );

                clone.MIDItoドラム = new Dictionary<IdKey, ドラム入力種別>();

                foreach( var kvp in this.MIDItoドラム )
                    clone.MIDItoドラム.Add( kvp.Key, kvp.Value );

                clone.FootPedal最小値 = this.FootPedal最小値;
                clone.FootPedal最大値 = this.FootPedal最大値;

                return clone;
            }

        }
    }
}
