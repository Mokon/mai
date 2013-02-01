/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// A class to run an algorithm and gather results and statistics
  /// about it.
  /// </summary>
  public class AlgorithmRunner {

    /// <summary>
    /// The static state with which to fun.
    /// </summary>
    private readonly GenericGridWorldStaticState StaticState;

    /// <summary>
    /// The algorithm to run with.
    /// </summary>
    private readonly Algorithm<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Algorithm;

    /// <summary>
    /// The goal for this run.
    /// </summary>
    private readonly Goal<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Goal;

    /// <summary>
    /// The actions for the domain this run is operating on.
    /// </summary>
    private readonly Operator<GenericGridWorldStaticState,
      GenericGridWorldDynamicState>[] Actions;
    private Random gen;

    public List<Results> Resultss = null;

    /// <summary>
    /// A constructor to setup all the applicable fields.
    /// </summary>
    /// <param name="IO"></param>
    /// <param name="WorldLoader"></param>
    /// <param name="Algorithm"></param>
    /// <param name="Weight"></param>
    public AlgorithmRunner( System.IO.TextReader IO,
      GridWorldLoader<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> WorldLoader, String Algorithm,
      Metric Weight, bool Batch, StateVisualizer<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> vis, int Seed ) {
      gen = new Random( Seed );
      this.vis = vis;
      this.Goal = new DestinationsReachedGoal( );
      this.Algorithm = new GenericAlgorithmFactory<GenericGridWorldStaticState,
        GenericGridWorldDynamicState>( ).SetRealTimeData(
        new SingleUnitOctileDistanceHeuristic( ), vis,
        new SingleUnitTransformer( ), Weight, 0.1 ).GetAlgorithm(
        Algorithm, null, 0 );

      GenericGridWorldDynamicState InitialDynamicState;
      WorldLoader.Load( IO, out StaticState, out InitialDynamicState );
      IO.Close( );
      this.Actions = GridWorldOperator.Ops;
    }

    /// <summary>
    /// Solves the algorithm.
    /// </summary>
    /// <param name="Out"></param>
    /// <returns></returns>
    public Solution Solve( TextWriter Out ) {
      var alg = new Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.AStar<
        GenericGridWorldStaticState, GenericGridWorldDynamicState>(
          new SingleUnitOctileDistanceHeuristic( ), true, null );
      var sol =  alg.Compute(
        StaticState, StaticState.InitialDynamicState, Goal, this.Actions );
      if ( Out != null ) {
        lock ( Out ) {
          Out.WriteLine(
              "AStar, N/A, " + alg.Expansions +
                ", " + alg.Generations + ", N/A, " + sol.First( ).Cost );
          Out.Flush( );
        }
      }
      return new Solution( sol.First( ), alg );
    }

    private StateVisualizer<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> vis;

    /// <summary>
    /// Runs the algorithm.
    /// </summary>
    /// <param name="refreshRate"></param>
    /// <param name="ComputationLimit"></param>
    /// <param name="Batch"></param>
    /// <param name="maxPathLength"></param>
    /// <returns></returns>
    public Results Run( long refreshRate, bool Batch, double computationLimitCeilingRatio,
      string Alg, int ComputationLimit, Metric OptimalPathLength, string File,
      bool Dynamic ) {
      if ( Algorithm is RealTimeAlgorithm<GenericGridWorldStaticState,
        GenericGridWorldDynamicState> ) {
        ( Algorithm as RealTimeAlgorithm<GenericGridWorldStaticState,
          GenericGridWorldDynamicState> ).SetComputationLimit( ComputationLimit );
      }
      Path<GenericGridWorldStaticState, GenericGridWorldDynamicState> path = null;
      Path<GenericGridWorldStaticState, GenericGridWorldDynamicState> fullPath =
        new Path<GenericGridWorldStaticState, GenericGridWorldDynamicState>(
          (GenericGridWorldDynamicState)null );
      long MaxMemoryUsage = 0;
      int numStops = 0;
      List<double> ComputationTimes = new List<double>( );
      List<double> Expansions = new List<double>( );
      double LastExpansions = 0;
      long ComputationTimeMax = long.MinValue;
      if ( !Batch ) {
#if OpenGl
        if ( vis != null ) {
          vis.Visualize( StaticState, StaticState.InitialDynamicState );
          vis.SetInitData( Alg, ComputationLimit,
            StaticState.Map != null && StaticState.Map.OptimalSolutionCostSpecified ?
            StaticState.Map.OptimalSolutionCost :
            OptimalPathLength.ToDouble( ), File, Resultss );
        }
#else
          System.Console.WriteLine( StaticState.InitialDynamicState ) ;
#endif
      }

      int StepNumber = 0;
      UncoverMap( );
      Stopwatch total = Stopwatch.StartNew( );
      Stopwatch step = Stopwatch.StartNew( );
      foreach ( var nextPath in
        Algorithm.Compute( StaticState, StaticState.InitialDynamicState, Goal,
          Actions ) ) {
        step.Stop( );
        total.Stop( );
        long CurrentUsage = Process.GetCurrentProcess( ).PrivateMemorySize64;
        if ( CurrentUsage > MaxMemoryUsage ) {
          MaxMemoryUsage = CurrentUsage;
        }
        path = nextPath;
        if ( path != null ) {
          foreach ( var Action in path.Actions ) {
            var oldState = StaticState.InitialDynamicState;
            if ( !Action.IsValidOn( StaticState.InitialDynamicState, StaticState, true ) ) {
              throw new IllegalActionOnStateException(
                "Illegal Action Given By Algorithm" );
            }
            StaticState.InitialDynamicState = Action.PerformOn(
              StaticState.InitialDynamicState, StaticState ).First( );
            fullPath.Push( Action, oldState, StaticState.InitialDynamicState );
            if ( !Batch ) {
              var U =  StaticState.InitialDynamicState.FindUnit( 0 );
              StaticState.Tiles[U.Y][U.X].Hits++;
#if OpenGl
              vis.Visualize( StaticState, StaticState.InitialDynamicState );
              vis.SetStepData( (int)Algorithm.Generations, (int)Algorithm.Expansions,
                total.ElapsedTicks, fullPath.Cost.ToDouble( ) + numStops,
                ComputationTimes.Count != 0 ? ComputationTimes.Average( ): 0,
                MaxMemoryUsage, ComputationTimeMax,
                Expansions.Count != 0 ? Expansions.Average( ): 0 );
#else
              System.Console.WriteLine( StaticState.InitialDynamicState ) ;
#endif
            }
            HandleChanges( Dynamic, StepNumber );
          }
        } else {
          numStops++;
          HandleChanges( Dynamic, StepNumber );
#if OpenGl
          if ( !Batch ) {
            vis.Visualize( StaticState, StaticState.InitialDynamicState );
            ( vis as OpenGLStateVisualizer ).redraw( );
            vis.SetStepData( (int)Algorithm.Generations, (int)Algorithm.Expansions,
                total.ElapsedTicks, fullPath.Cost.ToDouble( )+ numStops,
                ComputationTimes.Count != 0 ? ComputationTimes.Average( ): 0,
                MaxMemoryUsage, ComputationTimeMax,
                Expansions.Count != 0 ? Expansions.Average( ): 0 );
          }
#endif
        }
        Expansions.Add( Algorithm.Expansions - LastExpansions );
        LastExpansions = Algorithm.Expansions;
        ComputationTimes.Add( step.ElapsedTicks );
        if ( step.ElapsedTicks > ComputationTimeMax ) {
          ComputationTimeMax = step.ElapsedTicks;
        }
        if ( Goal.IsSatisfiedBy( StaticState, StaticState.InitialDynamicState ) ) {
          break;
        }
        long t = refreshRate - step.ElapsedMilliseconds;
        if ( t > 0 ) {
          Thread.Sleep( (int)t );
        }
        Metric maxPathLength = new Metric( computationLimitCeilingRatio *
          ( StaticState.Map != null && StaticState.Map.OptimalSolutionCostSpecified ?
            StaticState.Map.OptimalSolutionCost :
            OptimalPathLength.ToDouble( ) ) );
        if ( maxPathLength.Equals( Metric.Infinity  ) && fullPath.Cost + new Metric( numStops ) >= maxPathLength ) {
          throw new MaxPathLengthExceded(
            new Results( Alg, ComputationLimit, Algorithm.Generations,
            Algorithm.Expansions, total.ElapsedTicks, fullPath.Cost.ToDouble( )+ numStops,
            StaticState.Map != null && StaticState.Map.OptimalSolutionCostSpecified ?
            StaticState.Map.OptimalSolutionCost :
            OptimalPathLength.ToDouble( ), File, ComputationTimes.Average( ),
            MaxMemoryUsage, ComputationTimeMax, Expansions.Average( ) ) );
        }
        StepNumber++;
        step.Reset( );
        step.Start( );
        total.Start( );
      }
      total.Stop( );
      if ( !Batch ) {

#if OpenGl
        if ( vis != null ) {
          vis.Cleanup( );
        }
#else
        new GenericGridWorldPathVisualizer( ).Visualize(
          Console.Out, fullPath, StaticState, StaticState.InitialDynamicState );
#endif

      }
      double AverageComputationTime = ComputationTimes.Average( );
      return new Results( Alg, ComputationLimit, Algorithm.Generations,
        Algorithm.Expansions, total.ElapsedTicks, fullPath.Cost.ToDouble( )+ numStops,
        StaticState.Map != null && StaticState.Map.OptimalSolutionCostSpecified ?
            StaticState.Map.OptimalSolutionCost :
            OptimalPathLength.ToDouble( ), File, AverageComputationTime,
        MaxMemoryUsage, ComputationTimeMax, Expansions.Average( ) );
    }

    private const int ChangeChance = 100;

    public void HandleChanges( bool Dynamic, int StepNumber ) {

      StaticState.Changes = new LinkedList<GenericGridWorldDynamicState>( );
      Unit u = StaticState.InitialDynamicState.FindUnit( 0 );
      Step Changes = StaticState.Next( StepNumber );
      if ( Changes != null && Changes.Changes != null ) {
        foreach ( Coord c in Changes.Changes ) {
          if ( StaticState.Tiles[c.Y][c.X] is
              PassableTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState> ) {

            if ( !( u.X == c.X && u.Y == c.Y ) || StaticState.Map.ChangeList.BlockOnAgent ) {
              Tile<GenericGridWorldStaticState,
                  GenericGridWorldDynamicState> newTile = 
              new BlockedTile<GenericGridWorldStaticState,
                    GenericGridWorldDynamicState>(
                    c.X, c.Y, StaticState.Height, StaticState.Width );
              newTile.LastPassable = StaticState.Tiles[c.Y][c.X].LastPassable;
              newTile.BeenExplored = StaticState.Tiles[c.Y][c.X].BeenExplored;
              newTile.Hits = StaticState.Tiles[c.Y][c.X].Hits;
              StaticState.Tiles[c.Y][c.X] = newTile;
            }
          } else if ( StaticState.Tiles[c.Y][c.X] is
              BlockedTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState> ) {
            if ( StaticState.Tiles[c.Y][c.X].LastChoke ) {
              Tile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState> newTile = 
              new ChokeTile<GenericGridWorldStaticState,
                  GenericGridWorldDynamicState>(
                  c.X, c.Y, StaticState.Height, StaticState.Width );
              newTile.LastPassable = StaticState.Tiles[c.Y][c.X].LastPassable;
              newTile.BeenExplored = StaticState.Tiles[c.Y][c.X].BeenExplored;
              newTile.Hits = StaticState.Tiles[c.Y][c.X].Hits;
              StaticState.Tiles[c.Y][c.X] = newTile;
            } else {
              Tile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState> newTile = 
              new PassableTile<GenericGridWorldStaticState,
                  GenericGridWorldDynamicState>(
                  c.X, c.Y, StaticState.Height, StaticState.Width );
              newTile.LastPassable = StaticState.Tiles[c.Y][c.X].LastPassable;
              newTile.BeenExplored = StaticState.Tiles[c.Y][c.X].BeenExplored;
              newTile.Hits = StaticState.Tiles[c.Y][c.X].Hits;
              StaticState.Tiles[c.Y][c.X] = newTile;
            }
          } else if ( StaticState.Tiles[c.Y][c.X] is
              ChokeTile<GenericGridWorldStaticState,
              GenericGridWorldDynamicState> ) {
            if ( !( u.X == c.X && u.Y == c.Y ) || StaticState.Map.ChangeList.BlockOnAgent ) {
              Tile<GenericGridWorldStaticState,
                  GenericGridWorldDynamicState> newTile = 
              new BlockedTile<GenericGridWorldStaticState,
                    GenericGridWorldDynamicState>(
                    c.X, c.Y, StaticState.Height, StaticState.Width );
              newTile.LastPassable = StaticState.Tiles[c.Y][c.X].LastPassable;
              newTile.BeenExplored = StaticState.Tiles[c.Y][c.X].BeenExplored;
              newTile.Hits = StaticState.Tiles[c.Y][c.X].Hits;
              newTile.LastChoke = true;
              StaticState.Tiles[c.Y][c.X] = newTile;
            }
          }


        }
      }
      foreach ( var row in StaticState.Tiles ) {
        foreach ( var t in row ) {
          var tile = t;
          if ( !tile.BeenExplored && tile.Visible( StaticState ) ) {
            tile.BeenExplored = true;
          }
          if ( Dynamic ) {
            bool isGoal = tile.X == u.DstX && tile.Y == u.DstY ||
              tile.X == u.X && tile.Y == u.Y;
            ;
            if ( !isGoal && tile is
              PassableTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState> && gen.Next( )%ChangeChance == 0 ) {
              Tile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState> newTile = 
              new BlockedTile<GenericGridWorldStaticState,
                  GenericGridWorldDynamicState>(
                  tile.X, tile.Y, StaticState.Height, StaticState.Width );
              newTile.LastPassable = tile.LastPassable;
              newTile.BeenExplored = tile.BeenExplored;
              newTile.Hits = tile.Hits;
              tile = newTile;
              StaticState.Tiles[tile.Y][tile.X] = tile;
            } else if ( !isGoal && tile
              is BlockedTile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState> && gen.Next( )%ChangeChance == 0 ) {
              Tile<GenericGridWorldStaticState,
                GenericGridWorldDynamicState> newTile = 
              new PassableTile<GenericGridWorldStaticState,
                  GenericGridWorldDynamicState>(
                  tile.X, tile.Y, StaticState.Height, StaticState.Width );
              newTile.LastPassable = tile.LastPassable;
              newTile.BeenExplored = tile.BeenExplored;
              newTile.Hits = tile.Hits;
              tile = newTile;
              StaticState.Tiles[tile.Y][tile.X] = tile;
            }
          }

          bool nowPassable = tile.IsPassable( StaticState, false );
          if ( tile.LastPassable != nowPassable ) {
            tile.LastPassable = nowPassable;
            var ds = new GenericGridWorldDynamicState( );
            var ut = u.Clone( ) as Unit;
            ds.hash ^= ut.GetHashCode( );
            ut.X = tile.X;
            ut.Y = tile.Y;
            ds.hash ^= ut.GetHashCode( );
            ds.AddWorldObject( ut );
            StaticState.Changes.AddFirst( ds );
          }
        }
      }
    }
    private void UncoverMap( ) {
      foreach ( var row in StaticState.Tiles ) {
        foreach ( var t in row ) {
          var tile = t;
          if ( !tile.BeenExplored && tile.Visible( StaticState ) ) {
            tile.BeenExplored = true;
          }
          tile.LastPassable = tile.IsPassable( StaticState, false );
        }
      }
    }

  }



  public class MaxPathLengthExceded : Exception {
    public readonly Results Results;
    public MaxPathLengthExceded( Results Results ) {
      this.Results = Results;
    }
  }

  /// <summary>
  /// A solution to return on a solve of a map.
  /// </summary>
  public class Solution {

    /// <summary>
    /// The path of the soltution.
    /// </summary>
    public Path<GenericGridWorldStaticState, GenericGridWorldDynamicState> Path;
    public Algorithm<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Algorithm;

    /// <summary>
    /// Constructor which initilizes the data memebers.
    /// </summary>
    /// <param name="Path"></param>
    /// <param name="Algorithm"></param>
    public Solution( Path<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Path, Algorithm<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Algorithm ) {
      this.Path = Path;
      this.Algorithm = Algorithm;
    }
  }

}
