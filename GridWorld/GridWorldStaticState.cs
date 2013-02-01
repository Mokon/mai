/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the static state interface which represents
  /// a grid world.
  /// </summary>
  public abstract class GridWorldStaticState<SS, DS> : StaticState<SS, DS>
    where SS : GridWorldStaticState<SS, DS>
    where DS : GridWorldDynamicState<SS, DS> {

    public void SetChangeList( Map Map ) {
      this.Map = Map;
      this.ChangeList = new Queue<Step>( Map.ChangeList.Steps );
    }
    private Queue<Step> ChangeList;
    internal Map Map;

    public Step Next( int StepNum ) {
      if ( ChangeList == null ) {
        return null;
      } else if ( ChangeList.Count == 0 ) {
        if ( Map.ChangeList.Repeatable ) {
          this.ChangeList = new Queue<Step>( Map.ChangeList.Steps.Skip( 1 ) );
          return this.Next( StepNum );
        } else {
          return null;
        }
      } else if ( !Map.ChangeList.Repeatable && ChangeList.Peek( ).StepNum != StepNum ) {
        return null;
      } else {
        return ChangeList.Dequeue( );
      }
    }

    /// <summary>
    /// The height of the grid. (Y Axis)
    /// </summary>
    public int Height;

    /// <summary>
    /// The width of the grid (X Axis)
    /// </summary>
    public int Width;

    /// <summary>
    /// A constructor which takes the height and width of the grid and creates
    /// a jagged array based on this. This constructor DOES NOT initilize the
    /// tiles on the grid and so any use of this GridWorld object should be
    /// prefixed with an initilization of the tiles. For instance, a
    /// GridWorldLoader object would setup the grid for this.
    /// </summary>
    /// <param name="Height">The number of tiles on the y axis.</param>
    /// <param name="Width">The number of tiles on the x axis.</param>
    public GridWorldStaticState( int Height, int Width,
      DS InitialDynamicState )
      : base( InitialDynamicState ) {
      this.Width = Width;
      this.Height = Height;
      this.Tiles = new Tile<SS, DS>[Height][];
      for ( int i = 0 ; i < Tiles.Length ; i++ ) {
        Tiles[i] = new Tile<SS, DS>[Width];
      }
    }

    public void Scale( double newWidth, double newHeight ) {
      Unit U = this.InitialDynamicState.FindUnit( 0 );
      U.Width = (int)newWidth;
      U.Height = (int)newHeight;
      U.X =    (int)( ( U.X*newWidth )/Width );
      U.DstX = (int)( ( U.DstX*newWidth )/Width );
      U.Y =    (int)( ( U.Y*newHeight )/Height );
      U.DstY = (int)( ( U.DstY*newHeight )/Height );
      Tile<SS, DS>[][] NewTiles = new Tile<SS, DS>[(int)newHeight][];
      for ( int y = 0 ; y < newHeight ; y++ ) {
        NewTiles[y] = new Tile<SS, DS>[(int)newWidth];
        for ( int x = 0 ; x < newWidth ; x++ ) {
          double tX = Math.Ceiling( ( x*Width )/newWidth );
          double tY = Math.Ceiling( ( y*Height )/newHeight );
          if ( tX >= Width ) {
            tX--;
          }
          if ( tY >= Height ) {
            tY--;
          }
          NewTiles[y][x] = Tiles[(int)tY][(int)tX].Clone( ) as Tile<SS, DS>;
          NewTiles[y][x].Height = (int)newHeight;
          NewTiles[y][x].Width = (int)newWidth;
          NewTiles[y][x].X = x;
          NewTiles[y][x].Y = y;
        }
      }

      this.Width = (int)newWidth;
      this.Height = (int)newHeight;
      this.Tiles = NewTiles;

      if ( !Tiles[U.Y][U.X].IsPassable( null, true ) ) {
        throw new Exception( );
      }
    }


    /// <summary>
    /// A property to access the inner tiles. This is here for the possible
    /// addition of bound checking in future implementations.
    /// </summary>
    public Tile<SS, DS>[][] Tiles {
      get {
        return this.tiles;
      }
      private set {
        this.tiles = value;
      }
    }

    /// <summary>
    /// The underlying tiles field.
    /// </summary>
    private Tile<SS, DS>[][] tiles;

  }

}
