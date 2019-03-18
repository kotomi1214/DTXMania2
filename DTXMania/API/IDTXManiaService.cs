using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

namespace DTXMania.API
{
    /// <summary>
    ///		DTXMania が WCF を使ってサービスとして公開するインターフェースを定義する。
    /// </summary>
    [ServiceContract]
    public interface IDTXManiaService
    {
        /// <summary>
        ///		曲を読み込み、演奏を開始する。
        ///		DTXManiaがビュアーモードで起動されているときのみ有効。
        /// </summary>
        /// <param name="path">曲ファイルパス</param>
        /// <param name="startPart">演奏開始小節番号(0～)</param>
        /// <param name="drumsSound">ドラムチップ音を発声させるなら true。</param>
        [OperationContract]
        void ViewerPlay( string path, int startPart = 0, bool drumsSound = true );

        /// <summary>
        ///		現在の演奏を停止する。
        ///		DTXManiaがビュアーモードで起動されているときのみ有効。
        /// </summary>
        [OperationContract]
        void ViewerStop();

        /// <summary>
        ///		サウンドデバイスの発声遅延[ms]を返す。
        /// </summary>
        /// <returns>遅延量[ms]</returns>
        [OperationContract]
        float GetSoundDelay();
    }
}
