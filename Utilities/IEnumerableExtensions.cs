/*
 * This file is part of the Mokon.Net Utilties Package,
 * distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Net.Mokon.Utilities {

  /// <summary>
  /// Some extension methods for the IEnumerable interface.
  /// </summary>
  static class IEnumerableExtensions {
    private static Random rng = new Random( );

    public static T2 Get<T, T2>( this Dictionary<T, T2> dict, T key ) {
      T2 r;
      if ( !dict.TryGetValue( key, out r ) ) {
        throw new InvalidOperationException( "Key DNE in Dictionary" );
      }
      
      return r;
    }


    public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> c ) {
      T[] a = c.ToArray( );
      byte[] b = new byte[a.Length];
      rng.NextBytes( b );
      Array.Sort( b, a );
      return new List<T>( a );
    }

    public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> c, Random rng ) {
      T[] a = c.ToArray( );
      byte[] b = new byte[a.Length];
      rng.NextBytes( b );
      Array.Sort( b, a );
      return new List<T>( a );
    }

    /// <summary>
    /// Returns the index of the specified element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="items">The this object.</param>
    /// <param name="item">The item to search for.</param>
    /// <param name="Get">How to transform the elements of the collection.
    /// </param>
    /// <returns></returns>
    public static int IndexOf<T, T2>( this IEnumerable<T> items, T2 item,
      Func<T, T2> Get ) {
      if( item == null ) {
        return -1;
      }
      int index = 0;
      foreach( var i in items ) {
        if( Get( i ) != null && Get( i ).Equals( item ) ) {
          return index;
        }
        index++;
      }
      return -1;
    }

    /// <summary>
    /// Returns the specified element.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <param name="Get"></param>
    /// <returns></returns>
    public static T ElementOf<T, T2>( this IEnumerable<T> items, T2 item,
      Func<T, T2> Get ) {
      if( item == null ) {
        return default( T );
      }
      foreach( var i in items ) {
        if( Get( i ) != null && Get( i ).Equals( item ) ) {
          return i;
        }
      }
      return default( T );
    }

    /// <summary>
    /// Arg max for an ienumerable.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="args"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TSource ArgMax<TSource>( this IEnumerable<TSource> args,
      Func<TSource, int?> selector ) {
      TSource maxA = default(TSource);
      int? maxV = null;

      if( !args.Any() ) {
        throw new InvalidOperationException( "args is empty" ) ;
      }

      foreach( var arg in args ) {
        int? curVal = selector( arg );
        if( maxV == null || ( curVal != null && ( (int)maxV ) < ( (int)curVal ) ) ) {
          maxV = curVal;
          maxA = arg;
        }
      }
      return maxA;
    }

    /// <summary>
    /// Another arg max for doubles.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="args"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TSource ArgMaxD<TSource>( this IEnumerable<TSource> args,
      Func<TSource, double?> selector ) {
      TSource maxA = default( TSource );
      double? maxV = null;

      if( !args.Any( ) ) {
        throw new InvalidOperationException( "args is empty" );
      }

      foreach( var arg in args ) {
        double? curVal = selector( arg );
        if( maxV == null || ( curVal != null && ( (double)maxV ) < ( (double)curVal ) ) ) {
          maxV = curVal;
          maxA = arg;
        }
      }
      return maxA;
    }

    /// <summary>
    /// Arg min
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="args"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TSource ArgMin<TSource>( this IEnumerable<TSource> args,
      Func<TSource, int?> selector ) {
      TSource maxA = default( TSource );
      int? maxV = null;

      if( !args.Any( ) ) {
        throw new InvalidOperationException( "args is empty" );
      }

      foreach( var arg in args ) {
        int? curVal = selector( arg );
        if( maxV == null || ( curVal != null && ( (int)maxV ) > ((int)curVal) ) ) {
          maxV = curVal;
          maxA = arg;
        }
      }
      return maxA;
    }

    /// <summary>
    /// Arg min
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="args"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
   /* public static TSource ArgMin<TSource, TType>( this IEnumerable<TSource> args,
      Func<TSource, TType> selector ) {
      var comparer = Comparer<TType>.Default;
      TSource maxA = default( TSource );
      TType maxV = default(TType);

      if ( !args.Any( ) ) {
        throw new InvalidOperationException( "args is empty" );
      }

      foreach ( var arg in args ) {
        TType curVal = selector( arg );
        if ( maxV == null || ( curVal != null && comparer.Compare( maxV, curVal ) > 0 ) ) {
          maxV = curVal;
          maxA = arg;
        }
      }
      return maxA;
    }*/

    /// <summary>
    /// Arg min for doubles.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="args"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static TSource ArgMin<TSource>( this IEnumerable<TSource> args,
      Func<TSource, double?> selector ) {
      TSource maxA = default( TSource );
      double? maxV = null;

      if( !args.Any( ) ) {
        throw new InvalidOperationException( "args is empty" );
      }

      foreach( var arg in args ) {
        double? curVal = selector( arg );
        if( maxV == null || ( curVal != null && ( (double)maxV ) >
          ( (double)curVal ) ) ) {
          maxV = curVal;
          maxA = arg;
        }
      }
      return maxA;
    }

    public static TSource ArgMin<TSource>( this IEnumerable<TSource> args,
  Func<TSource, Metric> selector ) {
      TSource maxA = default( TSource );
      Metric maxV = null;

      if ( !args.Any( ) ) {
        throw new InvalidOperationException( "args is empty" );
      }

      foreach ( var arg in args ) {
        Metric curVal = selector( arg );
        if ( maxV == null || ( curVal != null && maxV > curVal ) ) {
          maxV = curVal;
          maxA = arg;
        }
      }
      return maxA;
    }

  }

}
