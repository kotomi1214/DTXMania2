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

        // 生成と終了


        public システムサウンド( string プリセット名 = null )
        {
            this._プリセット名を設定する( プリセット名 );
            this._種別toサウンドマップ = new Dictionary<システムサウンド種別, (ISampleSource source, PolySound sound)>();
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

        public void 読み込む()
        {
            this._プリセットサウンドを読み込む();
        }

        public void 読み込む( システムサウンド種別 type )
        {
            this._サウンドを読み込む( type, this._プリセットフォルダの絶対パス.変数なしパス );
        }
        


        // 再生等


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



        // private


        private string _プリセット名;

        private readonly string _既定のプリセット名 = "default";

        private VariablePath _プリセットフォルダの絶対パス;

        private Dictionary<システムサウンド種別, (ISampleSource source, PolySound sound)> _種別toサウンドマップ = null;


        private void _プリセット名を設定する( string プリセット名 )
        {
            this._プリセット名 = プリセット名 ?? this._既定のプリセット名;
            this._プリセットフォルダの絶対パス = new VariablePath( $@"$(System)sounds\presets\{this._プリセット名}" );
        }

        private void _プリセットサウンドを読み込む()
        {
            try
            {
                foreach( システムサウンド種別 種別 in Enum.GetValues( typeof( システムサウンド種別 ) ) )
                {
                    // システムサウンド種別名をそのままファイル名として使う。形式は .ogg のみ。
                    var サウンドファイルの絶対パス = new VariablePath( Path.Combine( this._プリセットフォルダの絶対パス.変数なしパス, 種別.ToString() + ".ogg" ) );

                    this._サウンドを読み込む( 種別, サウンドファイルの絶対パス, 既に登録されているなら何もしない: true );

                }
            }
            catch( Exception e )
            {
                Log.ERROR( $"プリセットサウンドの読み込みに失敗しました。[{this._プリセット名}][{Folder.絶対パスをフォルダ変数付き絶対パスに変換して返す( e.Message )}]" );

                if( !( this._プリセット名.Equals( this._既定のプリセット名, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    Log.Info( "既定のプリセットの読み込みを行います。" );
                    this.Dispose();
                    this._プリセット名を設定する( this._既定のプリセット名 );
                    this._プリセットサウンドを読み込む();
                    return;
                }
            }

            Log.Info( $"プリセットサウンドを読み込みました。[{this._プリセット名}]" );
        }

        private void _サウンドを読み込む( システムサウンド種別 種別, VariablePath サウンドファイルパス, bool 既に登録されているなら何もしない = false )
        {
            // 上書き？
            if( this._種別toサウンドマップ.ContainsKey( 種別 ) )
            {
                if( 既に登録されているなら何もしない )
                    return; // 上書きしない

                // 登録済みのサウンドを破棄。
                this._種別toサウンドマップ[ 種別 ].source?.Dispose();
                this._種別toサウンドマップ[ 種別 ].sound?.Dispose();
            }

            // ファイルがないなら無視。
            if( !File.Exists( サウンドファイルパス.変数なしパス ) )
            {
                Log.ERROR( $"システムサウンドファイルが見つかりません。スキップします。[{サウンドファイルパス.変数付きパス}]" );
                return;
            }

            // サンプルソースを読み込む。
            var sampleSource = SampleSourceFactory.Create( App進行描画.サウンドデバイス, サウンドファイルパス, 1.0 );  // システムサウンドは常に再生速度 = 1.0

            if( null == sampleSource )
                throw new Exception( $"システムサウンドの読み込みに失敗しました。[{サウンドファイルパス.変数付きパス}]" );

            // サウンドを生成する。
            var sound = new PolySound( App進行描画.サウンドデバイス, sampleSource, 2 );

            // マップに登録。
            this._種別toサウンドマップ[ 種別 ] = (sampleSource, sound);

            Log.Info( $"システムサウンドを読み込みました。[{サウンドファイルパス.変数付きパス}]" );
        }
    }
}