/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using Net.Mokon.Utilities;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms;


namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// This is an implementation of the grid world loader which has its file
  /// format defined by the A1 info sheet.
  /// </summary>
  public class ChokepointGridWorldMaker {

    /// <summary>
    /// A constructor with LOS.
    /// </summary>
    /// <param name="LOS"></param>
    public ChokepointGridWorldMaker( int LOS, int NumChokepoints, int Height,
        int Width, int ChokepointRadius, int NumAgents, int NumSteps, int AgentRadius,
      double MoveRate ) {
      this.xml = new Map( );
      xml.Width = Width;
      xml.Height = Height;
      this.LOS = LOS;
      this.NumChokepoints = NumChokepoints;
      this.ChokepointRadius = ChokepointRadius;
      this.NumAgents = NumAgents;
      this.NumSteps = NumSteps;
      this.AgentRadius = AgentRadius;
      this.MoveRate = MoveRate;
    }

    Map xml;

    private int LOS;

    private int NumChokepoints;

    private int NumAgents;
    private int NumSteps;
    private int AgentRadius;
    private double MoveRate;

    private int ChokepointRadius;

    public void Make( int Seed, String Filename ) {
      System.Console.WriteLine( "Generating Map [{0}]", Filename );
      Random Gen = new Random( Seed );
      xml.Version = 1;
      xml.Date = DateTime.Now;
      xml.Generator = Generator.MAIDynamicChokepoint;
      xml.Seed = Seed;
      xml.LOS = LOS;
      xml.Start = new Coord( );
      xml.Start.X = Gen.Next( xml.Width );
      xml.Start.Y = Gen.Next( xml.Height );
      xml.Goal = new Coord( );
      xml.Goal.X = Gen.Next( xml.Width );
      xml.Goal.Y = Gen.Next( xml.Height );
      GenericGridWorldDynamicState DS = new GenericGridWorldDynamicState( );
      GenericGridWorldStaticState SS = new GenericGridWorldStaticState( xml.Height, xml.Width, DS );
      DS.AddWorldObject( new Unit( xml.Start.X, xml.Start.Y,
        xml.Height, xml.Width, xml.Goal.X, xml.Goal.Y, LOS, 0 ) );

      bool[][] tiles = new bool[xml.Height][];
      for ( int i = 0 ; i < xml.Height ; i++ ) {
        tiles[i] = new bool[xml.Width];
      }

      LinkedList<Point> ChokePoints = new LinkedList<Point>( );
      for ( int i = 0 ; i < NumChokepoints ; i++ ) {
        ChokePoints.AddFirst( new Point( Gen.Next( xml.Width ), Gen.Next( xml.Height ) ) );
      }

      for ( int Y = 0 ; Y < xml.Height ; Y++ ) {
        for ( int X = 0 ; X < xml.Width ; X++ ) {
          if ( ChokePoints.Any( cp => Distance.OctileDistance( cp.X, X, cp.Y, Y ) <= ChokepointRadius ) ) {
            SS.Tiles[Y][X] = new ChokeTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
            tiles[Y][X] = false;
          } else {
            SS.Tiles[Y][X] = new PassableTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( X, Y, xml.Height, xml.Width );
            tiles[Y][X] = false;
          }
        }
      }

      Point U = new Point( xml.Start.X, xml.Start.Y );
      foreach ( var From in ChokePoints ) {
        foreach ( var To in ChokePoints ) {
          DrawLine( From, To, xml, SS, U, tiles );
        }
      }

      xml.Tiles = GridWorldFormat.ToTileString( SS, DS );

      try {
        var sol = new AStar<
          GenericGridWorldStaticState, GenericGridWorldDynamicState>(
            new SingleUnitOctileDistanceHeuristic( ), true, null )
            .Compute( SS, SS.InitialDynamicState, new DestinationsReachedGoal( ),
            GridWorldOperator.Ops ).First( );
        System.Console.WriteLine( "\tMap Generated Solution Cost [{0}]", sol.Cost );
      } catch ( PathNotFoundException ) {
        System.Console.WriteLine( "\tMap Generation Failed [{0}]", Filename );
        throw new MapCreationFailed( );
      }

      LinkedList<Point> AgentsLocs = new LinkedList<Point>( ChokePoints.Take( NumAgents ) );
      LinkedList<Agent> Agents = new LinkedList<Agent>( );
      foreach ( Point Loc in AgentsLocs ) {
        Agents.AddFirst( new Agent( ) {
          Location = Loc, Dst = Loc, Start = Loc
        } );
      }

      LinkedList<Step> ChangeList = new LinkedList<Step>( );

      for ( int sN = 0 ; sN < NumSteps || !AllAtStart( Agents ) ; sN++ ) {
        Step step = new Step( );
        step.StepNum = sN;
        List<Coord> Changes = new List<Coord>( );
        if ( sN == 0 ) {
          foreach ( Agent Agent in Agents ) {
            ToogleCircle( tiles, Agent.Location, AgentRadius, Changes, xml.Width, xml.Height,
              Agent.Location, Agents, AgentRadius, false );
          }
        }

        foreach ( Agent Agent in Agents ) {
          if ( sN < NumSteps && sN != 0 && Agent.Location.Equals( Agent.Dst ) && ( Gen.NextDouble( ) < MoveRate  ) ) {
            var NotReserved = ChokePoints.Where( x => !Agents.Any( a => a.Dst.Equals( x ) ) );
            if ( NotReserved.Count( ) > 0 ) {
              Agent.Dst = NotReserved.Skip( Gen.Next( NotReserved.Count( ) ) ).First( );
            }
          } else if ( !( sN < NumSteps ) ) {
            Agent.Dst = Agent.Start;
          }
          if ( !Agent.Location.Equals( Agent.Dst ) ) {
            ToogleCircle( tiles, Agent.Location, AgentRadius, Changes, xml.Width, xml.Height,
              Agent.Location, Agents, AgentRadius, false );

            Agent.Location = NextAll( Agent.Location, xml.Width, xml.Height, Agent.Dst );
            ToogleCircle( tiles, Agent.Location, AgentRadius, Changes, xml.Width, xml.Height,
              Agent.Location, Agents, AgentRadius, false );
          }
        }
        step.Changes = Optimize( Changes ).ToArray( );
        ChangeList.AddLast( step );

      }
      xml.ChangeList = new ChangeList( );
      xml.ChangeList.Repeatable = true;
      xml.ChangeList.BlockOnAgent = true;
      xml.ChangeList.Steps = ChangeList.ToArray( );
      System.Console.WriteLine( "\tMap Generated [{0}]", Filename );
      XmlSerializer s = new XmlSerializer( typeof( Map ) );
      TextWriter w = new StreamWriter( Filename );
      s.Serialize( w, xml );
      w.Close( );
    }

    private static bool AllAtStart( LinkedList<Agent> Agents ) {
      return Agents.All( x => x.Location.Equals( x.Start ) );
    }

    private class Agent {
      public Point Location;
      public Point Dst;
      public Point Start;
    }

    private static void ToogleCircle( bool[][] tiles, Point Loc, int Radius, List<Coord> Changes,
      int Width, int Height, Point ThisAgent, LinkedList<Agent> Agents, int AgentRadius, bool GuarenteeToggle ) {
      HashSet<Point> Done = new HashSet<Point>( );
      Stack<Point> ToDo = new Stack<Point>( );
      ToDo.Push( Loc );

      while ( ToDo.Count != 0 ) {
        Point Next = ToDo.Pop( );
        if ( Distance.OctileDistance( Next.X, Loc.X, Next.Y, Loc.Y ) <= Radius ) {
          if ( !Done.Contains( Next ) ) {
            Done.Add( Next );
            ToogleLoc( tiles, Next, Changes, ThisAgent, Agents, AgentRadius, GuarenteeToggle );
            foreach ( Point p in ExpandComplete( Next, Width, Height ) ) {
              if ( !Done.Contains( p ) ) {
                ToDo.Push( p );
              }
            }
          }
        }
      }
    }

    private static bool NearAnotherAgent( Point ThisAgent, Point Loc, LinkedList<Agent> Agents, int AgentRadius ) {
      var OtherAgents = from a in Agents
                        where !a.Location.Equals( ThisAgent )
                        select a.Location;
      var NearAgents = from a in OtherAgents
                       where Distance.OctileDistance( a.X, Loc.X, a.Y, Loc.Y ) <= AgentRadius
                       select a;
      return NearAgents.Any( );
    }

    private static void ToogleLoc( bool[][] tiles, Point Loc, List<Coord> Changes,
      Point ThisAgent, LinkedList<Agent> Agents, int AgentRadius, bool GuarenteeToggle ) {
      if ( ( GuarenteeToggle || !NearAnotherAgent( ThisAgent, Loc, Agents, AgentRadius ) ) && !tiles[Loc.Y][Loc.X] ) {
        Changes.Add( new Coord( ) {
          X = Loc.X, Y = Loc.Y
        } );
      }
    }

    private static List<Coord> Optimize( List<Coord> Changes ) {
      List<Coord> NewChanges = new List<Coord>( );
      foreach ( Coord c in Changes ) {
        if ( Changes.Count( x => x.X == c.X && x.Y == c.Y ) == 1 ) {
          NewChanges.Add( c );
        }
      }
      return NewChanges;
    }

    public class MapCreationFailed : Exception {

    }

    private static void DrawLine( Point From, Point To, Map xml, GenericGridWorldStaticState SS, Point Unit, bool[][] tiles ) {
      Point Cur = From;
      Stack<Point> Blocked = new Stack<Point>( );
      while ( !Cur.Equals( To ) ) {
        Cur = Next( Cur, xml.Width, xml.Height, To );
        if ( SS.Tiles[Cur.Y][Cur.X]
            is ChokeTile<GenericGridWorldStaticState, GenericGridWorldDynamicState> ) {
          continue;
        } else if ( Unit.X == Cur.X && Unit.Y == Cur.Y ) {
          continue;
        } else if ( SS.Tiles[Cur.Y][Cur.X]
            is BlockedTile<GenericGridWorldStaticState, GenericGridWorldDynamicState> ) {
          while ( Blocked.Count != 0 ) {
            Cur = Blocked.Pop( );
            SS.Tiles[Cur.Y][Cur.X] = new PassableTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState>( Cur.X, Cur.Y, xml.Height, xml.Width );
            tiles[Cur.Y][Cur.X] = false;
          }
          break;
        } else {
          SS.Tiles[Cur.Y][Cur.X] = new BlockedTile<GenericGridWorldStaticState,
            GenericGridWorldDynamicState>( Cur.X, Cur.Y, xml.Height, xml.Width );
          Blocked.Push( Cur );
          tiles[Cur.Y][Cur.X] = true;
        }
      }
    }


    private static IEnumerable<Point> Expand( Point P, int width, int height ) {
      IEnumerable<Point> Points = new Point[] {
          new Point( P.X - 1, P.Y ),
          new Point( P.X + 1, P.Y ),
          new Point( P.X, P.Y - 1 ),
          new Point( P.X, P.Y + 1 ),
        };

      return Points.Where( p => p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height );
    }

    private static IEnumerable<Point> ExpandComplete( Point P, int width, int height ) {
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

      return Points.Where( p => p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height );
    }

    private static Point Next( Point P, int width, int height, Point Dest ) {
      IEnumerable<Point> Points = Expand( P, width, height );
      return Points.ArgMin( p => Distance.OctileDistance( p.X, Dest.X, p.Y, Dest.Y ) );
    }
    private static Point NextAll( Point P, int width, int height, Point Dest ) {
      IEnumerable<Point> Points = ExpandComplete( P, width, height );
      return Points.ArgMin( p => Distance.OctileDistance( p.X, Dest.X, p.Y, Dest.Y ) );
    }

  }

}
