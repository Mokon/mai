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
  public class GenericGridWorldLoader : GridWorldLoader<
    GenericGridWorldStaticState, GenericGridWorldDynamicState>  {

    /// <summary>
    /// The load method to parse the file format. This will throw a format
    /// exception if it fails.
    /// </summary>
    /// <param name="Stream">The stream to parse from.</param>
    /// <returns>The grid world.</returns>
    public override void Load( System.IO.TextReader Stream,
      out GenericGridWorldStaticState StaticState,
      out GenericGridWorldDynamicState DynamicState ) {
      // First and Second lines of input are grid dimensions. @see Input 1.
      int columns = int.Parse( Stream.ReadLine( ) );
      int rows = int.Parse( Stream.ReadLine( ) );
      DynamicState = new GenericGridWorldDynamicState( );
      StaticState = new GenericGridWorldStaticState( rows, columns,
        DynamicState );

      // Grid Next in input @see Input 2.
      String line = null;

      List<Unit> Units = new List<Unit>( );
      int i = 0;
      while ( ( line = Stream.ReadLine( ).Trim( ) ) != null 
        && !line.Equals( "--- end units ---" ) ) {
        String[] split = line.Split( );
        int x = int.Parse( split[0] );
        int y = int.Parse( split[1] );
        int dstX = int.Parse( split[2] );
        int dstY = int.Parse( split[3] );
        // ignore split[4] which is LOS.
        Units.Add( new Unit( x, y, rows, columns, dstX, dstY, LOS, i++ ) );

      }

      GridWorldFormat.FromTileStream( Stream, StaticState, DynamicState, Units );
    }

  }

}
