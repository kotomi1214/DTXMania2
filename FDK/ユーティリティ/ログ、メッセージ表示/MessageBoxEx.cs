using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FDK
{
    /// <summary>
    ///     オーナーの中央位置に表示される MessageBox 。
    /// </summary>
    public class MessageBoxEx
    {
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, bool displayHelpButton )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton, options, displayHelpButton );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon, defaultButton );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon, defaultButton, options );
        }
        public static DialogResult Show( string text )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text );
        }
        public static DialogResult Show( string text, string caption )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator, object param )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator, param );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator, object param )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator, param );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, HelpNavigator navigator )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton, options, helpFilePath, navigator );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, string keyword )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon, defaultButton, options, helpFilePath, keyword );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath, string keyword )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton, options, helpFilePath, keyword );
        }
        public static DialogResult Show( IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text, caption, buttons, icon, defaultButton, options, helpFilePath );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options, string helpFilePath )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton, options, helpFilePath );
        }
        public static DialogResult Show( string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( text, caption, buttons, icon, defaultButton, options );
        }
        public static DialogResult Show( IWin32Window owner, string text )
        {
            m_hHook = SetWindowsHookEx( WH_CBT, CBTProc, IntPtr.Zero, GetCurrentThreadId() );
            return MessageBox.Show( owner, text );
        }

        private static IntPtr m_hHook;

        private static IntPtr CBTProc( int nCode, IntPtr wParam, IntPtr lParam )
        {
            if( nCode == HCBT_ACTIVATE )
            {
                // フックを解除する。
                UnhookWindowsHookEx( m_hHook );

                // 親の中央へ移動。
                IntPtr hMessageBox = wParam;
                IntPtr hParentWnd = GetParent( hMessageBox );
                RECT rectParentWnd, rectMessageBox;
                GetWindowRect( hMessageBox, out rectMessageBox );
                GetWindowRect( hParentWnd, out rectParentWnd );
                SetWindowPos(
                    hMessageBox,
                    hParentWnd,
                    ( rectParentWnd.right + rectParentWnd.left - rectMessageBox.right + rectMessageBox.left ) >> 1,
                    ( rectParentWnd.bottom + rectParentWnd.top - rectMessageBox.bottom + rectMessageBox.top ) >> 1,
                    0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE );
            }
            return IntPtr.Zero;
        }

        #region " Win32 API "
        //----------------
        [DllImport( "user32.dll" )]
        private static extern IntPtr GetParent( IntPtr hWnd );

        [DllImport( "user32.dll" )]
        private static extern bool GetWindowRect( IntPtr hWnd, out RECT lpRect );

        [DllImport( "user32.dll" )]
        private static extern bool SetWindowPos( IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags );

        [DllImport( "user32.dll" )]
        private static extern bool UnhookWindowsHookEx( IntPtr hHook );

        [DllImport( "kernel32.dll" )]
        private static extern IntPtr GetCurrentThreadId();

        [DllImport( "user32.dll" )]
        private static extern IntPtr SetWindowsHookEx( int idHook, HOOKPROC lpfn, IntPtr hInstance, IntPtr threadId );

        private delegate IntPtr HOOKPROC( int nCode, IntPtr wParam, IntPtr lParam );

        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int HCBT_ACTIVATE = 5;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int WH_CBT = 5;
        //----------------
        #endregion
    }
}
