/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the grid world loader which has its file
  /// format defined by the A1 info sheet.
  /// </summary>
  public class HOGGridWorldLoader : GridWorldLoader<
    GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    /// <summary>
    /// Constructor for a HOG Grid World Loader.
    /// </summary>
    /// <param name="xS"></param>
    /// <param name="yS"></param>
    /// <param name="xD"></param>
    /// <param name="yD"></param>
    public HOGGridWorldLoader( int xS, int yS, int xD, int yD ) {
      this.xD = xD;
      this.xS = xS;
      this.yD = yD;
      this.yS = yS;
    }

    private readonly int xS;
    private readonly int yS;
    private readonly int xD;
    private readonly int yD;

    /// <summary>
    /// The load method to parse the file format. This will throw a format
    /// exception if it fails.
    /// </summary>
    /// <param name="Stream">The stream to parse from.</param>
    /// <returns>The grid world.</returns>
    public override void Load( System.IO.TextReader Stream,
      out GenericGridWorldStaticState StaticState,
      out GenericGridWorldDynamicState DynamicState ) {

      String type = Stream.ReadLine( ).Split( )[1];
      System.Console.WriteLine( "Got HOG File Type " + type );
      int rows = int.Parse( Stream.ReadLine( ).Split( )[1] );
      int columns = int.Parse( Stream.ReadLine( ).Split( )[1] );
      Stream.ReadLine( );

      DynamicState = new GenericGridWorldDynamicState( );
      StaticState = new GenericGridWorldStaticState( rows, columns,
        DynamicState );

      // Grid Next in input @see Input 2.
      String line = null;

      DynamicState.AddWorldObject( new Unit( xS, yS, rows, columns, xD, yD,
        LOS, 0 ) );
       
      for( int y = 0 ; rows > y &&
        ( line = Stream.ReadLine( ) ) != null ; y++ ) {
        if( line.Length != columns ) {
          System.Console.WriteLine( line.Length );
          System.Console.WriteLine( line );
          throw new FormatException(
            "Stream Load Failed, Invalid Grid Format" );
        }
        int x = 0;
        foreach( char c in line ) {
          switch( c ) {
            case ' ':
            case '@':
            case 'O':
            case 'S':
            case 'W':
            case 'T':
              StaticState.Tiles[y][x] = 
                new BlockedTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState>( x, y, rows, columns );
              break;
            default:
              StaticState.Tiles[y][x] = 
                new PassableTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState>( x, y, rows, columns );
              break; 
          }
          x++;
        }
      }
    }

  }

}
