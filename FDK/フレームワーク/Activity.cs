using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
    /// <summary>
    ///     活性化状態とグラフィックリソースを持つオブジェクト。
    /// </summary>
    /// <remarks>
    ///     このオブジェクトは活性化状態を持ち、「活性化済み」または「活性化していない」状態のいずれかにある。
    ///     何をもって活性化とするか、また、活性化・非活性化時に何の処理を行うかについては、アプリで任意に設計すること。
    ///     また、このクラスが「スワップチェーンに依存したグラフィックリソース」を持つ場合は、適切なタイミングで
    ///     リソースの解放・復元メソッドが呼び出されることを想定し、それぞれのメソッドに該当リソースの解放・復元処理を
    ///     実装すること。また、このクラスの保持者は、適切なタイミングでそれぞれのメソッドを呼び出すこと。
    /// </remarks>
    public class Activity : IDisposable
    {
        public bool 活性化済み { get; protected set; } = false;

        public bool 活性化していない
        {
            get => !this.活性化済み;
            set => this.活性化済み = !value;
        }

        public bool リソース解放済み { get; protected set; } = false;

        public bool リソースを解放していない
        {
            get => !this.リソース解放済み;
            set => this.リソース解放済み = !value;
        }



        // 生成と終了


        public Activity()
        {
        }

        public void Dispose()
        {
            if( this._Dispose済み )
                return;

            this.非活性化する();
            this.OnDispose();

            this._Dispose済み = true;
        }

        public virtual void OnDispose()
        {
            // 必要あれば、派生クラスで実装する。
        }

        private bool _Dispose済み = false;



        // 活性化と非活性化


        public void 活性化する()
        {
            if( this.活性化済み )
                return;

            this.On活性化();

            this.活性化済み = true;
        }

        public void 非活性化する()
        {
            if( this.活性化していない )
                return;

            this.On非活性化();

            this.活性化していない = true;
        }

        public virtual void On活性化()
        {
            // 必要あれば、派生クラスで実装する。
        }

        public virtual void On非活性化()
        {
            // 必要あれば、派生クラスで実装する。
        }



        // スワップチェーンに依存するリソースの解放と再構築


        public void スワップチェーンに依存するリソースを解放する()
        {
            if( this.活性化していない || this.リソース解放済み )
                return;

            this.Onスワップチェーンに依存するリソースの解放();

            this.リソース解放済み = true;
        }

        public void スワップチェーンに依存するリソースを復元する()
        {
            if( this.活性化していない || this.リソースを解放していない )
                return;

            this.Onスワップチェーンに依存するリソースの復元();

            this.リソースを解放していない = true;
        }

        public virtual void Onスワップチェーンに依存するリソースの解放()
        {
            // 必要あれば、派生クラスで実装する。
        }

        public virtual void Onスワップチェーンに依存するリソースの復元()
        {
            // 必要あれば、派生クラスで実装する。
        }
    }
}
