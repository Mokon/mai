/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// A GridWorldObject is something that lies on the grid such as tile, bot,
  /// or dirt.
  /// </summary>
  public abstract class GridWorldObject<SS, DS> : ICloneable
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    /// <summary>
    /// A constructor based on the minimum params of a grid world object.
    /// </summary>
    /// <param name="X">The X axis location of the object on the grid.</param>
    /// <param name="Y">The Y axis location of the object on the grid.</param>
    /// <param name="GridWorld">The static world state this object
    /// is working with.</param>
    public GridWorldObject( int X, int Y, int Height, int Width, int UID ) {
      this.Height = Height;
      this.Width = Width;
      this.X = X;
      this.Y = Y;
      this.UID = UID;
    }

    /// <summary>
    /// The height of the world this object is on.
    /// </summary>
    public int Height;

    /// <summary>
    /// The width of the world this object is on.
    /// </summary>
    public int Width;

    /// <summary>
    /// To string method.
    /// </summary>
    /// <returns></returns>
    public override string ToString( ) {
      return "<X: " + this.X + ", Y: " + this.Y + " >";
    }

    /// <summary>
    /// Gets the UID of the object.
    /// </summary>
    public int UID {
      get;
      private set;
    }

    // This was orginally here for checking of the values but lets just
    // leave it off for effcienty.
    /// <summary>
    /// An propety for the x axis.
    /// </summary>
    public int X {
      set {
        if ( value >= this.Width ) {
          throw new ArgumentException( );
        }
        this.x = value;
      }
      get {
        return x;
      }
    }

    /// <summary>
    /// The underlying field for the x axis.
    /// </summary>
    private int x;

    // This was orginally here for checking of the values but lets just
    // leave it off for effcienty.
    /// <summary>
    /// The property for the y axis.
    /// </summary>
    public int Y {
      set {
        if ( value >= this.Height ) {
          throw new ArgumentException( );
        }
        this.y = value;
      }
      get {
        return y;
      }
    }

    /// <summary>
    /// The underlying field for the y axis.
    /// </summary>
    private int y;

    /// <summary>
    /// A method for comparision of two grid world objects.
    /// </summary>
    /// <param name="obj">The object to compare against.</param>
    /// <returns>Whether the two object have equality.</returns>
    public override bool Equals( object obj ) {
      if ( this.GetHashCode( ) == obj.GetHashCode( )
        && obj is GridWorldObject<SS, DS> ) {
        GridWorldObject<SS, DS> o = obj as GridWorldObject<SS, DS>;
        return this.UID == o.UID && o.X == this.X && o.Y == this.Y &&
          this.GetType( ).Equals( o.GetType( ) );
      } else {
        return false;
      }
    }

    /// <summary>
    /// The has method for this grid world object. This should be overridden by
    /// concrete implementations.
    /// </summary>
    /// <returns>The hash code for this object.</returns>
    public override int GetHashCode( ) {
      return (int)this.X ^ (int)this.Y;
    }

    /// <summary>
    /// A method to clone a grid world object. All that needs to be done here is
    /// a memberwise clone since no ptrs are contained.
    /// </summary>
    /// <returns>A copy of this object.</returns>
    public virtual object Clone( ) {
      return this.MemberwiseClone( );
    }

    /// <summary>
    /// Performs and action of a unit.
    /// </summary>
    /// <param name="MovementType"></param>
    public void Action( GridWorldFormat.PathCharacters MovementType ) {
      switch ( MovementType ) {
        case GridWorldFormat.PathCharacters.MOVE_EAST:
          this.Y++;
          break;
        case GridWorldFormat.PathCharacters.MOVE_NORTH:
          this.X--;
          break;
        case GridWorldFormat.PathCharacters.MOVE_SOUTH:
          this.X++;
          break;
        case GridWorldFormat.PathCharacters.MOVE_WEST:
          this.Y--;
          break;
        case GridWorldFormat.PathCharacters.MOVE_NORTHEAST:
          this.X--;
          this.Y++;
          break;
        case GridWorldFormat.PathCharacters.MOVE_NORTHWEST:
          this.X--;
          this.Y--;
          break;
        case GridWorldFormat.PathCharacters.MOVE_SOUTHEAST:
          this.X++;
          this.Y++;
          break;
        case GridWorldFormat.PathCharacters.MOVE_SOUTHWEST:
          this.X++;
          this.Y--;
          break;
        case GridWorldFormat.PathCharacters.WAIT:
          break;
        default:
          throw new Exception( );
      }
    }

  }

}
