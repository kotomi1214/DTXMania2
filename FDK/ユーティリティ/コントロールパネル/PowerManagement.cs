using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace FDK
{
	/// <summary>
	///		システムとモニタの省電力制御を行う。
	/// </summary>
	public static class PowerManagement
	{
		public static void システムの自動スリープと画面の自動非表示を抑制する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				if( ExecutionState.Null == SetThreadExecutionState( ExecutionState.SystemRequired | ExecutionState.DisplayRequired | ExecutionState.Continuous ) )
					Log.ERROR( "SetThreadExecutionState の実行に失敗しました。" );
			}
		}

		public static void システムの自動スリープと画面の自動非表示の抑制を解除する()
		{
			using( Log.Block( FDKUtilities.現在のメソッド名 ) )
			{
				if( ExecutionState.Null == SetThreadExecutionState( ExecutionState.Continuous ) )
					Log.ERROR( "SetThreadExecutionState の実行に失敗しました。" );
			}
		}


		[Flags]
		private enum ExecutionState : uint
		{
			Null = 0x00000000,				// 関数が失敗した時の戻り値
			SystemRequired = 0x00000001,	// スタンバイを抑止
			DisplayRequired = 0x00000002,	// 画面OFFを抑止
			AwayModeRequired = 0x00000040,	// 離席モードにする
			Continuous = 0x80000000,		// 効果を永続させる。ほかオプションと併用する。
		}

		[DllImport( "kernel32.dll" )]
		private static extern ExecutionState SetThreadExecutionState( ExecutionState esFlags );
	}
}
