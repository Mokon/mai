/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the grid world loader which has its file
  /// format defined by the A1 info sheet.
  /// </summary>
  public class RoomsGridWorldMaker {

    public RoomsGridWorldMaker( int LOS, int Height, int Width, int NumHorzRooms,
      int NumVertRooms, int NumSteps,
      double ChanceOpen, double ChanceClose, int HorzRoomSkip, int VertRoomSkip,
      bool GuarenteeSolveability, double MinAStarDistance, double MaxAStarDistance,
      bool UseCorners, double MinHDistance, double MaxHDistance ) {
      this.xml = new Map( );
      xml.Width = Width;
      xml.Height = Height;
      this.LOS = LOS;
      this.NumHorzRooms = NumHorzRooms;
      this.NumVertRooms = NumVertRooms;
      this.NumSteps = NumSteps;
      this.ChanceClose = ChanceClose;
      this.ChanceOpen = ChanceOpen;
      this.HorzRoomModifier = HorzRoomSkip + 1;
      this.VertRoomModifier = VertRoomSkip + 1;
      this.GuarenteeSolveability = GuarenteeSolveability;
      this.MaxAStarDistance = MaxAStarDistance;
      this.MinAStarDistance = MinAStarDistance;
      this.UseCorners = UseCorners;
      this.MaxHDistance = MaxHDistance;
      this.MinHDistance = MinHDistance;
    }

    private Map xml;

    private double ChanceOpen;
    private double ChanceClose;
    private int LOS;
    private int NumVertRooms;
    private int NumHorzRooms;
    private int NumSteps;
    private int HorzRoomModifier;
    private int VertRoomModifier;
    private bool GuarenteeSolveability;
    private double MinAStarDistance;
    private double MaxAStarDistance;
    private double MinHDistance;
    private double MaxHDistance;
    private bool UseCorners;

    public void Make( int Seed, String Filename ) {
      System.Console.WriteLine( "Generating Map [{0}]", Filename );
      Random Gen = new Random( Seed );
      xml.Version = 1;
      xml.Date = DateTime.Now;
      xml.Generator = Generator.MAIDynamicRooms;
      xml.Seed = Seed;
      xml.LOS = LOS;
      xml.LOSSpecified = true;
      xml.Start = new Coord( );
      xml.Goal = new Coord( );
      if ( UseCorners ) {
        xml.Start.X = 0;
        xml.Start.Y = 0;
        xml.Goal.X = xml.Width - 1;
        xml.Goal.Y = xml.Height - 1;
      } else {
        double Dis;
        do {
          xml.Start.X = Gen.Next( xml.Width );
          xml.Start.Y = Gen.Next( xml.Height );
          xml.Goal.X = Gen.Next( xml.Width );
          xml.Goal.Y = Gen.Next( xml.Height );
          Dis = Distance.OctileDistance( xml.Start.X, xml.Goal.X, xml.Start.Y, xml.Goal.Y );
        } while ( Dis < MinHDistance || Dis > MaxHDistance );
      }
      GenericGridWorldDynamicState DS = new GenericGridWorldDynamicState( );
      GenericGridWorldStaticState SS = new GenericGridWorldStaticState( xml.Height, xml.Width, DS );
      DS.AddWorldObject( new Unit( xml.Start.X, xml.Start.Y,
        xml.Height, xml.Width, xml.Goal.X, xml.Goal.Y, LOS, 0 ) );

      int RoomWidth = xml.Width / this.NumHorzRooms;
      int RoomHeight = xml.Height / this.NumVertRooms;
      bool[,] TilesBlocked = new bool[xml.Width, xml.Height];

      for ( int Y = 0 ; Y < xml.Height ; Y++ ) {
        for ( int X = 0 ; X < xml.Width ; X++ ) {
          SS.Tiles[Y][X] = new PassableTile<GenericGridWorldStaticState,
            GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
          TilesBlocked[X, Y] = false;
        }
      }

      // Vert Walls
      for ( int Y = RoomHeight ; Y < xml.Height ; Y+=RoomHeight ) {
        for ( int X = 0 ; X < xml.Width ; X++ ) {
          if ( !( X == xml.Start.X && Y == xml.Start.Y ) && !( Y == xml.Goal.Y && X == xml.Goal.X ) ) {
            SS.Tiles[Y][X] = new BlockedTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
            TilesBlocked[X, Y] = true;
          }
        }
      }
      // Horz Walls
      for ( int X = RoomWidth ; X < xml.Width ; X+=RoomWidth ) {
        for ( int Y = 0 ; Y < xml.Height ; Y++ ) {
          if ( !( X == xml.Start.X && Y == xml.Start.Y ) && !( Y == xml.Goal.Y && X == xml.Goal.X ) ) {
            SS.Tiles[Y][X] = new BlockedTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
            TilesBlocked[X, Y] = true;
          }
        }
      }

      List<Door> Doors = new List<Door>( );

      // Vert Doors
      for ( int Y = RoomHeight ; Y < xml.Height ; Y+=RoomHeight ) {
        for ( int X = RoomWidth/2 ; X < xml.Width ; X+=RoomWidth ) {
          if ( !( X == xml.Start.X && Y == xml.Start.Y ) && !( Y == xml.Goal.Y && X == xml.Goal.X ) ) {
            SS.Tiles[Y][X] = new PassableTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
            if ( ( X/RoomWidth ) % VertRoomModifier == 0 ) {
              Doors.Add( new Door( ) {
                Open = true, X = X, Y = Y
              } );
              TilesBlocked[X, Y] = false;
            }
          }
        }
      }
      // Horz Doors
      for ( int X = RoomWidth ; X < xml.Width ; X+=RoomWidth ) {
        for ( int Y = RoomHeight/2 ; Y < xml.Height ; Y+=RoomHeight ) {
          if ( !( X == xml.Start.X && Y == xml.Start.Y ) && !( Y == xml.Goal.Y && X == xml.Goal.X ) ) {
            SS.Tiles[Y][X] = new PassableTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
            if ( ( Y/RoomHeight ) % HorzRoomModifier == 0 ) {
              Doors.Add( new Door( ) {
                Open = true, X = X, Y = Y
              } );
              TilesBlocked[X, Y] = false;
            }
          }
        }
      }

      xml.Tiles = GridWorldFormat.ToTileString( SS, DS );

      try {
        var sol = new AStar<
          GenericGridWorldStaticState, GenericGridWorldDynamicState>(
            new SingleUnitOctileDistanceHeuristic( ), true, null )
            .Compute( SS, SS.InitialDynamicState, new DestinationsReachedGoal( ),
            GridWorldOperator.Ops ).First( );
        xml.OptimalSolutionCost = sol.Cost.ToDouble( );
        xml.OptimalSolutionCostSpecified = true;
        if ( !UseCorners && ( xml.OptimalSolutionCost < MinAStarDistance ||
            xml.OptimalSolutionCost > MaxAStarDistance ) ) {
          throw new ChokepointGridWorldMaker.MapCreationFailed( );
        }
        System.Console.WriteLine( "\tMap Generated Solution Cost [{0}]", sol.Cost );
      } catch ( PathNotFoundException ) {
        System.Console.WriteLine( "\tMap Generation Failed [{0}]", Filename );
        throw new ChokepointGridWorldMaker.MapCreationFailed( );
      }

      List<Step> ChangeList = new List<Step>( );

      Step step = new Step( );
      int sN;
      List<Coord> Changes;
      List<Door> DoorChanges;

      // Add one for skipped.
      step.StepNum = 0;
      step.Changes = new Coord[] { };
      ChangeList.Add( step );

      for ( sN = 1 ; sN < NumSteps + 1; sN++ ) {
        step = new Step( );
        step.StepNum = sN;
        do {
          Changes = new List<Coord>( );
          DoorChanges = new List<Door>( );
          foreach ( Door D in Doors ) {
            if ( D.Open ) {
              if ( Gen.NextDouble( ) < ChanceClose ) {
                D.Open = false;
                Changes.Add( new Coord( ) {
                  X = D.X, Y = D.Y
                } );
                DoorChanges.Add( D );
                TilesBlocked[D.X, D.Y] = true;
              }
            } else {
              if ( Gen.NextDouble( ) < ChanceOpen ) {
                D.Open = true;
                Changes.Add( new Coord( ) {
                  X = D.X, Y = D.Y
                } );
                DoorChanges.Add( D );
                TilesBlocked[D.X, D.Y] = false;
              }
            }
          }
        } while ( !ResolveReachability( DoorChanges, Changes, TilesBlocked,
            new Point( xml.Goal.X, xml.Goal.Y ), xml.Width, xml.Height ) );
        step.Changes = Changes.ToArray( );
        ChangeList.Insert( ChangeList.Count, step );
      }

      // Add one more step to guarentee repeatablity.
      step = new Step( );
      step.StepNum = sN;

      Changes = new List<Coord>( );
      foreach ( Door D in Doors ) {
        if ( !D.Open ) {
          Changes.Add( new Coord( ) {
            X = D.X, Y = D.Y
          } );
        }
      }
      step.Changes = Changes.ToArray( );
      ChangeList.Insert( ChangeList.Count, step );

      xml.ChangeList = new ChangeList( );
      xml.ChangeList.Repeatable = true;
      xml.ChangeList.BlockOnAgent = false;
      xml.ChangeList.Steps = ChangeList.ToArray( );
      System.Console.WriteLine( "\tMap Generated [{0}]", Filename );
      XmlSerializer s = new XmlSerializer( typeof( Map ) );
      TextWriter w = new StreamWriter( Filename );
      s.Serialize( w, xml );
      w.Close( );
    }

    private void ProcessReachability( bool[,] Reachability, bool[,] TilesBlocked, Point Start, int width, int height ) {
      LinkedList<Point> OPEN = new LinkedList<Point>( );
      OPEN.AddFirst( Start );
      Reachability[Start.X, Start.Y] = true;
      while ( OPEN.Count != 0 ) {
        var Cur = OPEN.First.Value;
        OPEN.RemoveFirst( );
        var Neigh = Expand( Cur, width, height );
        foreach ( Point p in Neigh ) {
          if ( !TilesBlocked[p.X, p.Y] && !Reachability[p.X, p.Y] ) {
            OPEN.AddFirst( p );
            Reachability[p.X, p.Y] = true;
          }
        }

      }
    }

    private bool ResolveReachability( List<Door> DoorChanges, List<Coord> Changes,
      bool[,] TilesBlocked, Point Star, int width, int height ) {
      if ( !GuarenteeSolveability ) {
        return true;
      }
      if ( Reachability( TilesBlocked, Star, width, height ) ) {
        return true;
      } else {
        foreach ( Coord c in Changes ) {
          TilesBlocked[c.X, c.Y] = !TilesBlocked[c.X, c.Y];
        }
        foreach ( Door d in DoorChanges ) {
          d.Open = !d.Open;
        }
        return false;
      }
    }

    private bool Reachability( bool[,] TilesBlocked, Point Star, int width, int height ) {
      bool[,] Reachability = new bool[width, height];
      ProcessReachability( Reachability, TilesBlocked, Star, width, height );
      for ( int X = 0 ; X < width ; X++ ) {
        for ( int Y = 0 ; Y < height ; Y++ ) {
          if ( !Reachability[X, Y] && !TilesBlocked[X, Y] ) {
            return false;
          }
        }
      }
      return true;
    }

    private static IEnumerable<Point> Expand( Point P, int width, int height ) {
      IEnumerable<Point> Points = new Point[] {
          new Point( P.X - 1, P.Y + 1),
          new Point( P.X + 1, P.Y - 1 ),
          new Point( P.X - 1, P.Y - 1 ),
          new Point( P.X + 1, P.Y + 1 ),

          new Point( P.X - 1, P.Y ),
          new Point( P.X + 1, P.Y ),
          new Point( P.X, P.Y - 1 ),
          new Point( P.X, P.Y + 1 ),
        };

      List<Point> PL = new List<Point>( );
      foreach ( Point p in Points ) {
        if( p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height ) {
          PL.Add( p );
        }
      }
      return PL ;
    }

    private class Door {
      public int X;
      public int Y;
      public bool Open;

    }

  }

}
