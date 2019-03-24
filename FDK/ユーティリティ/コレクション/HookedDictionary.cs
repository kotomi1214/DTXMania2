using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FDK
{
    /// <summary>
    ///		インデクサでのset/getアクションを指定できるDictionary。
    /// </summary>
    public class HookedDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public Action<TKey, TValue> get時アクション = null;
        public Action<TKey, TValue> set時アクション = null;

        public new TValue this[ TKey key ]
        {
            get
            {
                if( null == key )
                    throw new ArgumentNullException();

                if( !( this.TryGetValue( key, out TValue value ) ) )
                    throw new KeyNotFoundException();

                this.get時アクション?.Invoke( key, value );   // Hook
                return value;
            }
            set
            {
                if( this.ContainsKey( key ) )
                    this.Remove( key );
                this.Add( key, value );

                this.set時アクション?.Invoke( key, value );   // Hook
            }
        }
    }
}
