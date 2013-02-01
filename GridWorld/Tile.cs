/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// A tile on the grid. The tile has a location and a type. From the type
  /// passabity can be derived.
  /// </summary>
  public abstract class Tile<SS, DS> : GridWorldObject<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    public bool LastChoke = false;

    /// <summary>
    /// The number of times the main unit has been on this tile
    /// </summary>
    public int Hits = 0 ; 

    /// <summary>
    /// A constructor based on several fields.
    /// </summary>
    /// <param name="X">The X axis location of the tile.</param>
    /// <param name="Y">The Y axis location of the tile.</param>
    /// <param name="GridWorld">A refernce to the world in which this tile
    /// resides.</param>
    /// <param name="Type">The tile type.</param>
    public Tile( int X, int Y, int Height, int Width )
      : base( X, Y, Height, Width, -1 ) {
      this.LastPassable = true;
      this.BeenExplored = false;
    }

    /// <summary>
    /// Tests if the tile can be passed.
    /// </summary>
    /// <returns>Whether the tile can be passed.</returns>
    public bool IsPassable( SS ss, bool all ) {
      if ( all || ss == null || this.Visible( ss ) ) {
        if ( this is ChokeTile<SS, DS> ) {
          return !ss.InitialDynamicState.ContainsWorldObject( 0, X, Y );
        } else {
          return this is PassableTile<SS, DS>;
        }
      } else {
        return this.LastPassable;   
        /* Interesting note, I wish we could return a maybe passable her
         * so we could recover from a changing map that the visible portions
         * get blocked out of. */
      }
    }

    /// <summary>
    /// Returns whether or not this title is visible by the zeros unit.
    /// </summary>
    /// <param name="ss"></param>
    /// <returns></returns>
    public bool Visible( SS ss ) {
      Unit u = ss.InitialDynamicState.FindUnit( 0 );
      return Distance.OctileDistance( this.X, u.X, this.Y, u.Y ) <= u.LOS;
    }

    /// <summary>
    /// Returns the last passability state of this tile according to the zeros
    /// unit.
    /// </summary>
    public bool LastPassable {
      get;
      set;
    }

    /// <summary>
    /// Returns if this state has been explored.
    /// </summary>
    public bool BeenExplored {
      get;
      set;
    }

  }

  /// <summary>
  /// A passable tile.
  /// </summary>
  public class PassableTile<SS, DS> : Tile<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    /// <summary>
    /// A constructor based on several fields.
    /// </summary>
    /// <param name="X">The X axis location of the tile.</param>
    /// <param name="Y">The Y axis location of the tile.</param>
    /// <param name="GridWorld">A refernce to the world in which this tile
    /// resides.</param>
    /// <param name="Type">The tile type.</param>
    public PassableTile( int X, int Y, int Height, int Width )
      : base( X, Y, Height, Width ) {
    }

  }

  public class ChokeTile<SS, DS> : Tile<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    /// <summary>
    /// A constructor based on several fields.
    /// </summary>
    /// <param name="X">The X axis location of the tile.</param>
    /// <param name="Y">The Y axis location of the tile.</param>
    /// <param name="GridWorld">A refernce to the world in which this tile
    /// resides.</param>
    /// <param name="Type">The tile type.</param>
    public ChokeTile( int X, int Y, int Height, int Width )
      : base( X, Y, Height, Width ) {
    }

  }

  /// <summary>
  /// A passable tile with a recharing station on it.
  /// </summary>
  public class RechargeStationTile<SS, DS> : PassableTile<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {


    /// <summary>
    /// A constructor based on several fields.
    /// </summary>
    /// <param name="X">The X axis location of the tile.</param>
    /// <param name="Y">The Y axis location of the tile.</param>
    /// <param name="GridWorld">A refernce to the world in which this tile
    /// resides.</param>
    /// <param name="Type">The tile type.</param>
    public RechargeStationTile( int X, int Y, int Height, int Width )
      : base( X, Y, Height, Width ) {
    }

  }

  /// <summary>
  /// A blocked tile
  /// </summary>
  public class BlockedTile<SS, DS> : Tile<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    /// <summary>
    /// A constructor based on several fields.
    /// </summary>
    /// <param name="X">The X axis location of the tile.</param>
    /// <param name="Y">The Y axis location of the tile.</param>
    /// <param name="GridWorld">A refernce to the world in which this tile
    /// resides.</param>
    /// <param name="Type">The tile type.</param>
    public BlockedTile( int X, int Y, int Height, int Width )
      : base( X, Y, Height, Width ) {
    }                                
  }

}
