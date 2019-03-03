using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using FDK;

namespace DTXMania.曲
{
    /// <summary>
    ///		曲ノードの基本クラス。
    /// </summary>
    /// <remarks>
    ///		曲ツリーを構成するすべてのノードは、このクラスを継承する。
    /// </remarks>
    abstract class Node : FDK.Activity
    {
        // static 定数

        /// <summary>
        ///		難易度それぞれのカラー。
        ///		具体的には難易度ラベルの背景の色。
        /// </summary>
        public static IReadOnlyDictionary<int, Color4> 難易度色 { get; protected set; } = new Dictionary<int, Color4>() {
            [ 0 ] = new Color4( 0xfffe9551 ),   // BASIC 相当
            [ 1 ] = new Color4( 0xff00aaeb ),   // ADVANCED 相当
            [ 2 ] = new Color4( 0xff7d5cfe ),   // EXTREME 相当
            [ 3 ] = new Color4( 0xfffe55c6 ),   // MASTER 相当
            [ 4 ] = new Color4( 0xff2b28ff ),   // ULTIMATE 相当
        };


        // プロパティ

        /// <summary>
        ///		ノードのタイトル。
        ///		曲名、BOX名など。
        /// </summary>
        public string タイトル { get; set; } = "(no title)";

        /// <summary>
        ///		ノードのサブタイトル。
        ///		制作者名など。
        /// </summary>
        public string サブタイトル { get; set; } = "";

        /// <summary>
        ///		難易度ラベルとその値（0.00～9.99）。
        ///		必要あれば、派生クラスで設定すること。
        ///		なお、配列は5要素で固定とする（0:BASIC～4:ULTIMATE）
        /// </summary>
        public (string label, float level)[] 難易度 { get; } = new (string, float)[ 5 ];


        // 曲ツリー関連

        /// <summary>
        ///		曲ツリー階層において、親となるノード。
        /// </summary>
        public Node 親ノード { get; set; } = null;

        /// <summary>
        ///		曲ツリー階層において、このノードが持つ子ノードのリスト。
        ///		子ノードを持たない場合は空リスト。
        ///		null 不可。
        /// </summary>
        public SelectableList<Node> 子ノードリスト { get; } = new SelectableList<Node>();

        /// <summary>
        ///		このノードの１つ前に位置する兄弟ノードを示す。
        /// </summary>
        /// <remarks>
        ///		このノードが先頭である（このノードの親ノードの子ノードリストの先頭である）場合は、末尾に位置する兄弟ノードを示す。
        /// </remarks>
        public Node 前のノード
        {
            get
            {
                var index = this.親ノード.子ノードリスト.IndexOf( this );
                Trace.Assert( ( 0 <= index ), "[バグあり] 自分が、自分の親の子ノードリストに存在していません。" );

                index = index - 1;

                if( 0 > index )
                    index = this.親ノード.子ノードリスト.Count - 1;    // 先頭なら、末尾へ。

                return this.親ノード.子ノードリスト[ index ];
            }
        }

        /// <summary>
        ///		このノードの１つ後に位置する兄弟ノードを示す。
        /// </summary>
        /// <remarks>
        ///		このノードが末尾である（このノードの親ノードの子ノードリストの末尾である）場合は、先頭に位置する兄弟ノードを示す。
        /// </remarks>
        public Node 次のノード
        {
            get
            {
                var index = this.親ノード.子ノードリスト.IndexOf( this );
                Trace.Assert( ( 0 <= index ), "[バグあり] 自分が、自分の親の子ノードリストに存在していません。" );

                index = index + 1;

                if( this.親ノード.子ノードリスト.Count <= index )
                    index = 0;      // 末尾なら、先頭へ。

                return this.親ノード.子ノードリスト[ index ];
            }
        }


        // ノード画像関連

        /// <summary>
        ///		ノードの全体サイズ（設計単位）。
        ///		すべてのノードで同一、固定値。
        /// </summary>
        public static Size2F 全体サイズ => new Size2F( 314f, 220f );

        /// <summary>
        ///		ノードを表す画像。
        ///		null にすると、既定のノード画像が使用される。
        ///		派生クラスで、適切な画像を割り当てること。
        /// </summary>
        /// <remarks>
        ///		<see cref="SetNode"/> の場合のみ、扱いが異なる。
        ///		詳細は<see cref="SetNode.ノード画像"/>を参照のこと。
        /// </remarks>
        public virtual テクスチャ ノード画像 { get; protected set; } = null;

        /// <summary>
        ///		ノードを表す画像の既定画像。static。
        /// </summary>
        /// <remarks>
        ///		<see cref="ノード画像"/>が null の再に、代わりに表示される。
        ///		static であり、全ノードで１つのインスタンスを共有する。
        /// </remarks>
        public static テクスチャ 既定のノード画像 { get; protected set; } = null;


        // プレビュー音声関連

        public void プレビュー音声を再生する()
        {
            if( this.プレビュー音声ファイルの絶対パス?.Nullまたは空である() ?? true )
                return;

            // 一定期間ごとに Tick イベントを呼び出すタイマを作成。

            this._プレビュー音声再生タイマ?.Dispose();
            this._プレビュー音声再生タイマ = new System.Windows.Forms.Timer() {
                Interval = 500, // ミリ秒
            };

            // Tick イベントでは、プレビュー音声を読み込んで再生する。

            this._プレビュー音声再生タイマ.Tick += ( sender, e ) => { // このタスクはGUIスレッドで処理される（非同期ではない）。

                this._プレビュー音声を生成する();
                this._プレビュー音声?.Play( ループ再生する: true );

                this._プレビュー音声再生タイマ.Stop();    // 実行停止。２回目以降の Tick はない。
            };

            this._プレビュー音声再生タイマ.Start();
        }

        public void プレビュー音声を停止する()
        {
            this._プレビュー音声再生タイマ?.Stop();
            this._プレビュー音声再生タイマ?.Dispose();
            this._プレビュー音声再生タイマ = null;

            this._プレビュー音声?.Stop();
        }

        public virtual string プレビュー音声ファイルの絶対パス { get; protected set; } = null;

        protected CSCore.ISampleSource _プレビュー音声ソース = null;  // 未対応、または生成に失敗した場合は null。

        protected Sound _プレビュー音声 = null;    // 未使用なら null。

        private System.Windows.Forms.Timer _プレビュー音声再生タイマ = null;


        protected void _プレビュー音声を生成する()
        {
            if( null != this._プレビュー音声 )
                return; // 生成済み

            if( this.プレビュー音声ファイルの絶対パス.Nullまたは空である() )
                return; // 指定なし

            if( null == this._プレビュー音声ソース )  // 未生成の場合
                this._プレビュー音声ソース = SampleSourceFactory.Create( App.サウンドデバイス, new VariablePath( this.プレビュー音声ファイルの絶対パス ).変数なしパス, 1.0 );   // プレビューは常に再生速度 = 1.0

            if( null != this._プレビュー音声ソース )
                this._プレビュー音声 = new Sound( App.サウンドデバイス, this._プレビュー音声ソース );
        }


        // メソッド

        public Node()
        {
            this.難易度 = new(string, float)[]{
                ( "", 0.00f ),
                ( "", 0.00f ),
                ( "", 0.00f ),
                ( "", 0.00f ),
                ( "", 0.00f ),
            };

            //this.子を追加する( this._ノード画像 );	--> 派生クラスのコンストラクタで追加することができる。
            this.子Activityを追加する( this._曲名テクスチャ = new 曲名() );
        }

        protected override void On活性化()
        {
            // 全インスタンスで共有する static メンバが未生成なら生成する。
            if( null == Node.既定のノード画像 )
            {
                Node.既定のノード画像 = new テクスチャ( @"$(System)images\既定のプレビュー画像.png" );
                Node.既定のノード画像.活性化する();
            }
        }

        protected override void On非活性化()
        {
            this.プレビュー音声を停止する();

            this._プレビュー音声?.Dispose();
            this._プレビュー音声 = null;

            this._プレビュー音声ソース?.Dispose();
            this._プレビュー音声ソース = null;

            // 全インスタンスで共有する static メンバが生成な済みなら解放する。
            if( null != Node.既定のノード画像 )
            {
                Node.既定のノード画像.非活性化する();
                Node.既定のノード画像 = null;
            }
        }

        public virtual void 進行描画する( DeviceContext1 dc, Matrix ワールド変換行列, bool キャプション表示 = true )
        {
            // (1) ノード画像を描画する。
            if( null != this.ノード画像 )
            {
                this.ノード画像.描画する( ワールド変換行列 );
            }
            else
            {
                Node.既定のノード画像.描画する( ワールド変換行列 );
            }

            // (2) キャプションを描画する。
            if( キャプション表示 )
            {
                ワールド変換行列 *= Matrix.Translation( 0f, 0f, 1f );    // ノード画像よりZ方向手前にほんのり移動
                this._曲名テクスチャ.タイトル = this.タイトル;
                this._曲名テクスチャ.サブタイトル = this.サブタイトル;
                this._曲名テクスチャ.描画する( ワールド変換行列, 不透明度0to1: 1f, new RectangleF( 0f, 138f, Node.全体サイズ.Width, Node.全体サイズ.Height - 138f + 27f ) );
            }
        }


        protected 曲名 _曲名テクスチャ = null;
    }
}
