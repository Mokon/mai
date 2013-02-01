/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// An implementation of the PathVisualizer interface to create a path output
  /// readable by the TAs programs.
  /// </summary>
  public class GenericGridWorldPathVisualizer : PathVisualizer<GenericGridWorldStaticState,
    GenericGridWorldDynamicState> {

    /// <summary>
    /// Writes the path out in some way to the given string based on the given
    /// stream, path, and state.
    /// </summary>
    /// <param name="Stream">The stream to output to.</param>
    /// <param name="Path">The path to visualize.</param>
    /// <param name="StaticState">The static state to work off of.</param>
    /// <param name="DynamicState">The dynamic state to work off of.</param>
    public void Visualize( System.IO.TextWriter Stream,
      Path<GenericGridWorldStaticState, GenericGridWorldDynamicState> Path,
      GenericGridWorldStaticState StaticState,
      GenericGridWorldDynamicState DynamicState ) {
      foreach ( Operator<GenericGridWorldStaticState,
        GenericGridWorldDynamicState> a in Path.Actions ) {
        if ( a is MoveEast ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_EAST );
        } else if ( a is MoveNorth ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_NORTH );
        } else if ( a is MoveSouth ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_SOUTH );
        } else if ( a is MoveWest ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_WEST );
        } else if ( a is MoveNorthEast ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_NORTHEAST );
        } else if ( a is MoveNorthWest ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_NORTHWEST );
        } else if ( a is MoveSouthEast ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_SOUTHEAST );
        } else if ( a is MoveSouthWest ) {
          Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_SOUTHWEST );
        } else if ( a is ReverseOperator<GenericGridWorldStaticState,
          GenericGridWorldDynamicState> ) {
          var act = ( a as ReverseOperator<GenericGridWorldStaticState,
            GenericGridWorldDynamicState> ).BaseOperator;
          if ( act is MoveWest ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_EAST );
          } else if ( act is MoveSouth ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_NORTH );
          } else if ( act is MoveNorth ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_SOUTH );
          } else if ( act is MoveEast ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_WEST );
          } else if ( act is MoveNorthEast ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_NORTHEAST );
          } else if ( act is MoveNorthWest ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_NORTHWEST );
          } else if ( act is MoveSouthEast ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_SOUTHEAST );
          } else if ( act is MoveSouthWest ) {
            Stream.Write( (char)GridWorldFormat.PathCharacters.MOVE_SOUTHWEST );
          }
        }
        Stream.Write( '\n' );
      }
    }

  }

}
