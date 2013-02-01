using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Mokon.Utilities {
  public class ExtendedDictionary<TKey, TValue> :
    Dictionary<string, TValue> {

    private Func<TKey, TValue> UnknownLookup;

    public ExtendedDictionary( Func<TKey, TValue> UnknownLookup ) {
      this.UnknownLookup = UnknownLookup;
    }

    public TValue Lookup( TKey k ) {
      TValue r;
      if ( this.TryGetValue( k.ToString( ), out r ) ) {
        return r;
      } else {
        return UnknownLookup( k );
      }
    }

    public void Set( TKey k, TValue value ) {
      if ( this.ContainsKey( k.ToString( ) ) ) {
        this.Remove( k.ToString( ) );
      }
      this.Add( k.ToString( ), value );
    }

  }

}
