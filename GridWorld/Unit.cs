/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// A simple grid world object that represents a piece of dirt to be vacuumed
  /// up.
  /// </summary>
  public class Unit : GridWorldObject<GenericGridWorldStaticState,
    GenericGridWorldDynamicState> {

    /// <summary>
    /// A default constructor which intilizes the location of the dirt.
    /// </summary>
    /// <param name="X">The x axis location</param>
    /// <param name="Y">The y axis location</param>
    /// <param name="GridWorld">The grid world this dirt is on.</param>
    public Unit( int X, int Y, int Height, int Width,
      int DstX, int DstY, int LOS, int UID )
      : base( X, Y, Height, Width, UID ) {
      this.DstX = DstX;
      this.DstY = DstY;
      this.LOS = LOS;
    }

    /// <summary>
    /// DstX location.
    /// </summary>
    public int DstX;

    /// <summary>
    /// DstY location.
    /// </summary>
    public int DstY;

    /// <summary>
    /// The line of sight of the unit.
    /// </summary>
    public int LOS;

    /// <summary>
    /// Whether or not this unit is at its destination location.
    /// </summary>
    /// <returns></returns>
    public bool AtDst( ) {
      return this.DstX == this.X && this.DstY == this.Y;
    }

  }

}
