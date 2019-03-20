using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSCore;
using FDK;

namespace DTXMania
{
    class システムサウンド : IDisposable
    {
        public システムサウンド( string プリセット名 = null )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                this._プリセットサウンドを読み込む( プリセット名 ?? this._既定のプリセット名 );
            }
        }

        public void Dispose()
        {
            foreach( var kvp in this._種別toサウンドマップ )
            {
                kvp.Value.sound.Dispose();
                kvp.Value.source.Dispose();
            }

            this._種別toサウンドマップ = null;
        }

        public void 再生する( システムサウンド種別 type, bool ループ再生する = false )
        {
            if( this._種別toサウンドマップ.TryGetValue( type, out var map ) )
                map.sound?.Play( ループ再生する: ループ再生する );
        }

        public void 停止する( システムサウンド種別 type )
        {
            if( this._種別toサウンドマップ.TryGetValue( type, out var map ) )
                map.sound?.Stop();
        }

        public bool 再生中( システムサウンド種別 type )
            => this._種別toサウンドマップ.TryGetValue( type, out var map ) && map.sound.いずれかが再生中である;


        private readonly string _既定のプリセット名 = "default";

        private Dictionary<システムサウンド種別, (ISampleSource source, PolySound sound)> _種別toサウンドマップ = null;


        private void _プリセットサウンドを読み込む( string プリセット名 )
        {
            this._種別toサウンドマップ = new Dictionary<システムサウンド種別, (ISampleSource source, PolySound sound)>();

            var プリセットフォルダの絶対パス = new VariablePath( $@"$(System)sounds\presets\{プリセット名}" );

            try
            {
                foreach( システムサウンド種別 種別 in Enum.GetValues( typeof( システムサウンド種別 ) ) )
                {
                    // システムサウンド種別名をそのままファイル名として使う。形式は .ogg のみ。
                    var サウンドファイルの絶対パス = new VariablePath( Path.Combine( プリセットフォルダの絶対パス.変数なしパス, 種別.ToString() + ".ogg" ) );

                    // ファイルがないなら無視。
                    if( !File.Exists( サウンドファイルの絶対パス.変数なしパス ) )
                    {
                        Log.ERROR( $"システムサウンドファイルが見つかりません。スキップします。[{サウンドファイルの絶対パス.変数付きパス}]" );
                        continue;
                    }

                    var sampleSource = SampleSourceFactory.Create( App進行描画.サウンドデバイス, サウンドファイルの絶対パス, 1.0 );  // システムサウンドは常に再生速度 = 1.0
                    if( null == sampleSource )
                        throw new Exception( $"システムサウンドの読み込みに失敗しました。[{サウンドファイルの絶対パス.変数付きパス}]" );

                    var sound = new PolySound( App進行描画.サウンドデバイス, sampleSource, 2 );

                    this._種別toサウンドマップ[ 種別 ] = (sampleSource, sound);

                    Log.Info( $"システムサウンドを読み込みました。[{サウンドファイルの絶対パス.変数付きパス}]" );
                }
            }
            catch( Exception e )
            {
                Log.ERROR( $"プリセットサウンドの読み込みに失敗しました。[{プリセット名}][{Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );

                if( !( プリセット名.Equals( this._既定のプリセット名, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    Log.Info( "既定のプリセットの読み込みを行います。" );
                    this.Dispose();
                    this._プリセットサウンドを読み込む( this._既定のプリセット名 );
                    return;
                }
            }

            Log.Info( $"プリセットサウンドを読み込みました。[{プリセット名}]" );
        }
    }
}