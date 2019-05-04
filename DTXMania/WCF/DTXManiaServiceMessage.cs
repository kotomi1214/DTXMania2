using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania.WCF
{
    /// <summary>
    ///		DTXMania に対するサービスメッセージ。
    ///		<see cref="IDTXManiaService"/> のメソッドと対応する。
    /// </summary>
    class DTXManiaServiceMessage
    {
        public enum 指示種別
        {
            指示なし,
            演奏開始,
            演奏停止,
        }
        public 指示種別 種別 { get; set; } = 指示種別.指示なし;

        public VariablePath 演奏対象曲のファイルパス { get; set; } = null;

        public int 演奏を開始する小節番号 { get; set; } = 0;

        public bool ドラムチップのヒット時に発声する { get; set; } = true;


        public override string ToString()
        {
            // デバッグ用。
            return "ViewerMessage: "
                + $"種別={this.種別}"
                + $", 演奏開始小節番号={this.演奏を開始する小節番号}"
                + $", ドラムチップ発声={this.ドラムチップのヒット時に発声する}"
                + $", 曲ファイルパス=[{this.演奏対象曲のファイルパス.変数付きパス}]";
        }
    }
}
