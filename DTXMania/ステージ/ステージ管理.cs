using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.ステージ
{
    /// <summary>
    ///     全ステージならびに全アイキャッチのインスタンスを保持し、
    ///     ステージ間の切り替えを行う。
    /// </summary>
    class ステージ管理 : Activity, IDisposable
    {
        public ステージ管理()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 各ステージの外部依存アクションを接続。
                var 結果ステージ = (結果.結果ステージ) this.ステージリスト[ nameof( 結果.結果ステージ ) ];
                var 演奏ステージ = (演奏.演奏ステージ) this.ステージリスト[ nameof( 演奏.演奏ステージ ) ];
                結果ステージ.結果を取得する = () => ( 演奏ステージ.成績 );
                結果ステージ.BGMを停止する = () => 演奏ステージ.BGMを停止する();

                // static なメンバの初期化。
                演奏.BASIC.レーンフレーム.初期化する();
            }
        }

        public void Dispose()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 現在活性化しているステージがあれば、すべて非活性化する。
                foreach( var kvp in this.ステージリスト )
                {
                    if( kvp.Value.活性化している )
                    {
                        kvp.Value.非活性化する();
                    }
                }
                // 現在活性化しているアイキャッチがあれば、すべて非活性化する。
                foreach( var kvp in this._アイキャッチリスト )
                {
                    if( kvp.Value.活性化している )
                    {
                        kvp.Value.非活性化する();
                    }
                }
            }
        }

        protected override void On活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                // 現在のステージの初期化
                if( this.現在のステージ?.活性化していない ?? false )
                    this.現在のステージ?.活性化する();

                // 全アイキャッチの初期化
                foreach( var kvp in this._アイキャッチリスト )
                    kvp.Value.活性化する();

                // 現在のアイキャッチを設定。
                this.現在のアイキャッチ = this._アイキャッチリスト.ElementAt( 0 ).Value;
            }
        }

        protected override void On非活性化()
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( this.現在のステージ?.活性化している ?? false )
                    this.現在のステージ?.非活性化する();

                foreach( var kvp in this._アイキャッチリスト )
                    kvp.Value.非活性化する();

                this.現在のアイキャッチ = null;
            }
        }


        // ステージ

        public string 最初のステージ名
            => this.ステージリスト.ElementAt( 0 ).Value.GetType().Name;

        public ステージ 現在のステージ { get; private set; }

        /// <summary>
        ///		全ステージのリスト。
        ///		新しいステージができたら、ここに名前とインスタンスを追加すること。
        /// </summary>
        public Dictionary<string, ステージ> ステージリスト = new Dictionary<string, ステージ>() {
            { nameof( 起動.起動ステージ ),                        new 起動.起動ステージ() },
            { nameof( タイトル.タイトルステージ ),                new タイトル.タイトルステージ() },
            { nameof( 認証.認証ステージ ),                        new 認証.認証ステージ() },
            { nameof( 選曲.選曲ステージ ),                        new 選曲.選曲ステージ() },
            { nameof( オプション設定.オプション設定ステージ ),    new オプション設定.オプション設定ステージ() },
            { nameof( 曲読み込み.曲読み込みステージ ),            new 曲読み込み.曲読み込みステージ() },
            { nameof( 演奏.演奏ステージ ),                        new 演奏.演奏ステージ() },
            { nameof( 演奏.演奏ステージ_ビュアーモード ),         new 演奏.演奏ステージ_ビュアーモード() },
            { nameof( 結果.結果ステージ ),                        new 結果.結果ステージ() },
            { nameof( 終了.終了ステージ ),                        new 終了.終了ステージ() },
        };

        /// <summary>
        ///		現在のステージを非活性化し、指定されたステージに遷移して、活性化する。
        /// </summary>
        /// <param name="遷移先ステージ名">Nullまたは空文字列なら、非活性化のみ行う。</param>
        public void ステージを遷移する( string 遷移先ステージ名 )
        {
            using( Log.Block( FDKUtilities.現在のメソッド名 ) )
            {
                if( null != this.現在のステージ && this.現在のステージ.活性化している )
                    this.現在のステージ.非活性化する();

                if( 遷移先ステージ名.Nullでも空でもない() )
                {
                    Log.Header( $"{遷移先ステージ名} へ遷移します。" );

                    this.現在のステージ = this.ステージリスト[ 遷移先ステージ名 ];
                    this.現在のステージ.活性化する();

                    //App.入力管理.すべての入力デバイスをポーリングする();
                }
                else
                {
                    Log.Header( "ステージの遷移を終了します。" );
                    this.現在のステージ = null;
                }
            }
        }


        // アイキャッチ

        public アイキャッチ.アイキャッチ 現在のアイキャッチ { get; private set; } = null;

        /// <summary>
        ///     指定した名前のアイキャッチのクローズアニメーションを開始する。
        /// </summary>
        /// <remarks>
        ///     クローズしたアイキャッチをオープンする際には、クローズしたときと同じアイキャッチを使う必要がある。
        ///     ここで指定したアイキャッチは <see cref="現在のアイキャッチ"/> に保存されるので、
        ///     遷移先のステージでオープンするアイキャッチには、これを使用すること。
        /// </remarks>
        public void アイキャッチを選択しクローズする( string 名前 )
        {
            this.現在のアイキャッチ = this._アイキャッチリスト[ 名前 ]; // 保存。
            this.現在のアイキャッチ.クローズする();
        }

        /// <summary>
        ///     全アイキャッチのリスト。
        ///		新しいアイキャッチができたら、ここに名前とインスタンスを追加すること。
        /// </summary>
        private Dictionary<string, アイキャッチ.アイキャッチ> _アイキャッチリスト = new Dictionary<string, アイキャッチ.アイキャッチ>() {
            { nameof( アイキャッチ.シャッター ),       new アイキャッチ.シャッター() },
            { nameof( アイキャッチ.回転幕 ),           new アイキャッチ.回転幕() },
            { nameof( アイキャッチ.GO ),               new アイキャッチ.GO() },
            { nameof( アイキャッチ.半回転黒フェード ), new アイキャッチ.半回転黒フェード() },
        };
    }
}
