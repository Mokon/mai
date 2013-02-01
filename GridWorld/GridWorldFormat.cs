/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is a special class to organize formatting items in this assignment
  /// into one place.
  /// </summary>
  public static class GridWorldFormat {

    /// <summary>
    /// An enumeration of the tile key codes. @see Input 2.
    /// </summary>
    public enum TileKind {
      UNIT='*',
      CHOKEUNIT='%',
      BLANK='_',
      BLOCKED='#',
      CHOKE='.',
    } ;

    /// <summary>
    /// An enumeration of path characters @see Grad. Extensions and Output
    /// </summary>
    public enum PathCharacters {
      WAIT='S',
      MOVE_NORTH='w',
      MOVE_SOUTH='X',
      MOVE_EAST='D',
      MOVE_WEST='A',
      MOVE_NORTHEAST='E',
      MOVE_NORTHWEST='Q',
      MOVE_SOUTHEAST='C',
      MOVE_SOUTHWEST='Z',
    }

    /// <summary>
    /// A static method useful to convert a tile into its file format
    /// representation.
    /// </summary>
    /// <param name="Tile">The tile to convert.</param>
    /// <param name="GridWorldObjects">Objects from the GridWorld.</param>
    /// <returns><An ascii char for this file format. /returns>
    public static char GetAscii( Tile<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Tile,
      HashSet<GridWorldObject<GenericGridWorldStaticState,
      GenericGridWorldDynamicState>>
      GridWorldObjects ) {
      if ( Tile is ChokeTile<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> ) {
        var ElementOnTile = from e in GridWorldObjects
                            where e.X == Tile.X && e.Y == Tile.Y
                            select e;
        foreach ( var e in ElementOnTile ) {
          if ( e is Unit ) {
            return (char)GridWorldFormat.TileKind.CHOKEUNIT;
          }
        }
        return (char)GridWorldFormat.TileKind.CHOKE;
      } else if ( Tile is PassableTile<GenericGridWorldStaticState,
        GenericGridWorldDynamicState>  ) {
        var ElementOnTile = from e in GridWorldObjects
                            where e.X == Tile.X && e.Y == Tile.Y
                            select e;
        foreach ( var e in ElementOnTile ) {
          if ( e is Unit ) {
            return (char)GridWorldFormat.TileKind.UNIT;
          }
        }
        return (char)GridWorldFormat.TileKind.BLANK;
      } else {
        var ElementOnTile = from e in GridWorldObjects
                            where e.X == Tile.X && e.Y == Tile.Y
                            select e;
        foreach ( var k in ElementOnTile ) {
          if ( k is Unit ) {
            throw new ArgumentException( );
          }
        }
        return (char)GridWorldFormat.TileKind.BLOCKED;
      }
    }

    /// <summary>
    /// Converts the GridWorld into a simple string representation. This
    /// is for debugging purposes.
    /// </summary>
    /// <param name="GridWorldStaticState">The static state to stringilize
    /// </param>
    /// <param name="GridWorldDynamicState">The dynamic state to stringilize
    /// and yes if you are wondering I just made that word up but I think
    /// it sounds really cool.</param>
    /// <returns>A string.</returns>
    public static string ToString( GenericGridWorldStaticState
      GridWorldStaticState,
      GenericGridWorldDynamicState
      GridWorldDynamicState ) {
      System.Text.StringBuilder sb = new System.Text.StringBuilder( );
      sb.Append( "  " );
      for ( int j = 0 ; j < GridWorldStaticState.Width ; j++ ) {
        sb.Append( j % 10 );
      }
      sb.Append( "\r\n" );

      for ( int i = 0 ; i < GridWorldStaticState.Tiles.Length ; i++ ) {
        sb.Append( i % 10 + " " );
        foreach ( var x in GridWorldStaticState.Tiles[i] ) {
          sb.Append( GetAscii( x, GridWorldDynamicState.
                          GetWorldObjects( ) ) );
        }
        sb.Append( "\r\n" );
      }
      return sb.ToString( );
    }

    public static string ToFileString( GridWorldStaticState<
      GenericGridWorldStaticState, GenericGridWorldDynamicState>
      GridWorldStaticState,
      GridWorldDynamicState<GenericGridWorldStaticState,
      GenericGridWorldDynamicState>
      GridWorldDynamicState ) {
      Unit U = GridWorldDynamicState.FindUnit( 0 );
      System.Text.StringBuilder sb = new System.Text.StringBuilder( );
      sb.AppendLine( GridWorldStaticState.Width.ToString( ) );
      sb.AppendLine( GridWorldStaticState.Height.ToString( ) );
      sb.AppendLine( U.X + " " + U.Y + " " + U.DstX + " " + U.DstY );
      sb.AppendLine( "--- end units ---" );

      sb.Append( ToTileString( GridWorldStaticState, GridWorldDynamicState ) );
      return sb.ToString( );
    }

    public static string ToTileString( GridWorldStaticState<
      GenericGridWorldStaticState, GenericGridWorldDynamicState>
      GridWorldStaticState,
      GridWorldDynamicState<GenericGridWorldStaticState,
      GenericGridWorldDynamicState>
      GridWorldDynamicState ) {
      System.Text.StringBuilder sb = new System.Text.StringBuilder( );
      for ( int i = 0 ; i < GridWorldStaticState.Tiles.Length ; i++ ) {
        foreach ( var x in GridWorldStaticState.Tiles[i] ) {
          sb.Append( GetAscii( x, GridWorldDynamicState.
                          GetWorldObjects( ) ) );
        }
        sb.AppendLine( );
      }
      return sb.ToString( );
    }

    public static void FromTileStream( TextReader Stream,
      GridWorldStaticState<GenericGridWorldStaticState, GenericGridWorldDynamicState> ss,
      GridWorldDynamicState<GenericGridWorldStaticState, GenericGridWorldDynamicState> ds,
      List<Unit> Units ) {

      string line;
      for ( int y = 0 ; ss.Height > y &&
        ( line = Stream.ReadLine( ) ) != null ; y++ ) {
        if ( line.Length != ss.Width ) {
          System.Console.WriteLine( line.Length );
          System.Console.WriteLine( line );
          throw new FormatException(
            "Stream Load Failed, Invalid Grid Format" );
        }
        int x = 0;
        foreach ( char c in line ) {
          switch ( c ) {
            case (char)GridWorldFormat.TileKind.UNIT:
              Unit d = Units.First( v => v.X == x && v.Y == y );
              ds.AddWorldObject( d );
              goto case (char)GridWorldFormat.TileKind.BLANK;
            case (char)GridWorldFormat.TileKind.CHOKEUNIT:
              d = Units.First( v => v.X == x && v.Y == y );
              ds.AddWorldObject( d );
              goto case (char)GridWorldFormat.TileKind.CHOKE;
            case (char)GridWorldFormat.TileKind.CHOKE:
              ss.Tiles[y][x] = 
                new ChokeTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState>( x, y, ss.Height, ss.Width );
              break;
            case (char)GridWorldFormat.TileKind.BLANK:
            case ' ':
              ss.Tiles[y][x] = 
                new PassableTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState>( x, y, ss.Height, ss.Width );
              break;
            case (char)GridWorldFormat.TileKind.BLOCKED:
              ss.Tiles[y][x] = 
                new BlockedTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState>( x, y, ss.Height, ss.Width );
              break;
            default:
              throw new FormatException(
                "Stream Load Failed, Invalid Grid " +
                "Format, Unknown Title Type [" + c + "]" );
          }
          x++;
        }
      }
    }
  }

}
