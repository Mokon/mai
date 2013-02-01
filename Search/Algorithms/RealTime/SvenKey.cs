/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {
  public class SvenKey : IComparable<SvenKey> {

    public Metric k1;
    public Metric k2;
    public override string ToString( ) {
      return "k1 " + k1 + " - k2 " + k2;
    }

    public int CompareTo( SvenKey o ) {
      if ( o != null && this.k1.Equals( o.k1 ) && this.k2.Equals( o.k2 ) ) {
        return 0;
      } else if ( this < o ) {
        return -1;
      } else {
        return 1;
      }
    }

    public override bool Equals( object obj ) {
      return this.CompareTo( obj as SvenKey ) == 0;
    }

    public override int GetHashCode( ) {
      return this.k1.GetHashCode( ) ^ this.k2.GetHashCode( );
    }

    public static bool operator>( SvenKey arg1, SvenKey arg2 ) {
      return arg1.k1 > arg2.k1 || 
          ( arg1.k1.Equals( arg2.k1 ) && arg1.k2 > arg2.k2 );
    }

    public static bool operator<( SvenKey arg1, SvenKey arg2 ) {
      return arg1.k1 < arg2.k1 || 
          ( arg1.k1.Equals( arg2.k1 ) && arg1.k2 < arg2.k2 );
    }

    public static bool operator>=( SvenKey arg1, SvenKey arg2 ) {
      return arg1.k1 > arg2.k1 || 
          ( arg1.k1.Equals( arg2.k1 ) && arg1.k2 >= arg2.k2 );
    }

    public static bool operator<=( SvenKey arg1, SvenKey arg2 ) {
      return arg1.k1 < arg2.k1 || 
          ( arg1.k1.Equals( arg2.k1 ) && arg1.k2 <= arg2.k2 );
    }

  }
}
