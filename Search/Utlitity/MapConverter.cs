/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.IO;
using System.Text;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Utility {

  /// <summary>
  /// A class to convert the format of a map from HOG to gwmap.
  /// </summary>
  public static class MapConverter {

    /// <summary>
    /// Converts a map from HOG to gwmap format.
    /// </summary>
    /// <param name="Stream"></param>
    /// <returns></returns>
    public static String Convert( System.IO.TextReader Stream ) {
      var dim = Stream.ReadLine( ).Trim( ).Split( );
      int H = int.Parse( dim[0] );
      int W = int.Parse( dim[1] );
      Stream.ReadLine( );
      string[] map = new string[H];
      for ( int i = 0 ; i < H ; i++ ) {
        map[i] = Stream.ReadLine( );
      }
      Random Gen = new Random( );
      int x, y, dx, dy;
      do {
        x = Gen.Next( W );
        y = Gen.Next( H );
      } while ( map[y][x].Equals( '#' ) );
      do {
        dx = Gen.Next( W );
        dy = Gen.Next( H );
      } while ( map[dy][dx].Equals( '#' ) );
      var ca = map[y].ToCharArray( );
      ca[x] = '*';
      map[y] = new String( ca );
      for ( int i = 0 ; i < H ; i++ ) {
        map[i] = map[i].Replace( ' ', '_' );
      }
      StringBuilder sb = new StringBuilder( );
      sb.AppendLine( W.ToString( ) );
      sb.AppendLine( H.ToString( ) );
      sb.AppendFormat( "{0} {1} {2} {3}\r\n", x, y, dx, dy );
      sb.AppendLine( "--- end units ---" );
      foreach ( var line in map ) {
        sb.AppendLine( line );
      }
      return sb.ToString( );
    }

  }

}
