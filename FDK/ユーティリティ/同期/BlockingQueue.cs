using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FDK
{
    /// <summary>
    ///     ブロック可能なキュー。
    ///     簡単に言えば、Peek の実装された <see cref="System.Collections.Concurrent.BlockingCollection{T}"/> 。
    ///     ただし、本クラスでは "消費者" は一人だけと想定する。
    ///     （"消費者"が複数いる場合には、Peek から Take の間にキューが変化しないことが保証されない。）
    /// </summary>
    /// <typeparam name="T">フレームの型。<see cref="IDisposable"/> を実装していること。</typeparam>
    public class BlockingQueue<T> : IDisposable where T : IDisposable
    {
        public int 最大フレーム数 { get; }

        public BlockingQueue( int 最大フレーム数 )
        {
            Debug.Assert( 0 < 最大フレーム数 );

            this.最大フレーム数 = 最大フレーム数;
            this._Queue = new ConcurrentQueue<T>();
        }
        public void Dispose()
        {
            lock( this._Queue排他 )
                this._キューをクリアする();

            this._Queue = null;
        }

        public void Add( T frame )
        {
            lock( this._Queue排他 )
            {
                // キャンセル済みなら何もしない。
                if( this._Canceled )
                {
                    frame = default;
                    return;
                }

                // キューがいっぱいなら、空くまでブロックする。
                while( this.最大フレーム数 <= this._Queue.Count )
                {
                    Monitor.Wait( this._Queue排他 );

                    if( this._Canceled )
                        return;     // ブロックが解除されたときにキャンセル済みだったら何もしない。
                }

                // キューに格納する。
                this._Queue.Enqueue( frame );

                // キューの中身が変化したことを、Monitor.Wait してるスレッドへ通知する。
                Monitor.PulseAll( this._Queue排他 );
            }
        }
        public T Take()
        {
            lock( this._Queue排他 )
            {
                // キャンセル済みだったら何もしない。
                if( this._Canceled )
                    return default;

                // キューが空なら、フレームが来るまでブロックする。
                while( this._Queue.IsEmpty )
                {
                    Monitor.Wait( this._Queue排他 );

                    if( this._Canceled )
                        return default; // ブロックが解除されたときにキャンセル済みだったら何もせず戻る。
                }

                // キューから取り出す。
                if( this._Queue.TryDequeue( out T frame ) )
                {
                    // キューの中身が変化したことを、Monitor.Wait してるスレッドへ通知する。
                    Monitor.PulseAll( this._Queue排他 );
                    return frame;
                }
                else
                {
                    return default;   // Dequeue失敗
                }
            }
        }
        /// <summary>
        ///    次にキューから取り出されるフレームを返す。（キューからは取り出されない。）
        ///    キューが空だった場合には、ブロックに入らず、すぐに帰る。
        /// </summary>
        /// <param name="frame">フレーム。キューが空だったら null が返される。</param>
        public void Peek( out T frame )
        {
            lock( this._Queue排他 )
            {
                // キャンセル済みなら何もしない。
                if( this._Canceled )
                {
                    frame = default;
                    return;
                }

                // キューを Peek する。
                if( ( !this._Queue.IsEmpty ) &&
                    ( this._Queue.TryPeek( out frame ) ) )
                {
                    //Monitor.PulseAll( this._フレームキュー排他 );  --> キューの中身は変化しないので通知しない。
                }
                else
                {
                    frame = default;   // キューが空、あるいはPeek失敗
                }
            }
        }
        public T Peek()
        {
            this.Peek( out T frame );
            return frame;
        }
        /// <summary>
        ///     <see cref="Add(T)"/> あるいは <see cref="Take"/> でブロックしているスレッドにキャンセルを通知してブロックを解除する。
        /// </summary>
        public void Cancel()
        {
            this._Canceled = true;

            lock( this._Queue排他 )
            {
                // Monitor.Wait してるスレッドへ通知する。
                Monitor.PulseAll( this._Queue排他 );
            }
        }

        private ConcurrentQueue<T> _Queue = null;
        private readonly object _Queue排他 = new object();
        private bool _Canceled = false;

        private void _キューをクリアする()
        {
            while( this._Queue.TryDequeue( out T frame ) )
                frame.Dispose();
        }
    }
}
