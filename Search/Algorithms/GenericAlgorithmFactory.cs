/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime;
using Net.Mokon.Edu.Unh.CS.AI.Harness;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  /// <summary>
  /// A basic algorithm factor which loads many useful algorithms.
  /// </summary>
  public class GenericAlgorithmFactory<SS, DS> : AlgorithmFactory<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Returns an algorithm based on the given string and the given data
    /// fields. This will throw an AlgorithmNotFoundException if the given
    /// key is invalid based on the given implementation of this interface.
    /// </summary>
    /// <param name="Key">A string key of the algorithm which should be
    /// selected.</param>
    /// <param name="Params">object params of additional optional data
    /// to use when fetching the algorithm. What this data means depends
    /// on the implementation.</param>
    /// <returns></returns>
    public Algorithm<SS, DS> GetAlgorithm( string Key,
      HeuristicFactory<SS, DS> HeuristicFactory, int offset,
      params string[] Data ) {

      if ( RealTimeDataSet ) {
        int i;
        if ( ( i = Key.IndexOf( "-Weight=" ) ) > 0 ) {
          string AfterValue = Key.Substring( i + 8 );
          int j = AfterValue.IndexOf( "-" );
          if ( j > 0 ) {
            AfterValue = AfterValue.Substring( 0, j );
          }
          Weight = new Metric( double.Parse( AfterValue ) );
          if ( j > 0 ) {
            AfterValue = Key.Substring( i + 8 + j );
            Key = Key.Substring( 0, i ) + AfterValue;
          } else {
            Key = Key.Substring( 0, i );
          }
        } else if ( ( i = Key.IndexOf( "-Ratio=" ) ) > 0 ) {
          string AfterValue = Key.Substring( i + 7 );
          int j = AfterValue.IndexOf( "-" );
          if ( j > 0 ) {
            AfterValue = AfterValue.Substring( 0, j );
          }
          Ratio = double.Parse( AfterValue );
          if ( j > 0 ) {
            AfterValue = Key.Substring( i + 7 + j );
            Key = Key.Substring( 0, i ) + AfterValue;
          } else {
            Key = Key.Substring( 0, i );
          }
        }
      }

      try {
        if ( Key.Equals( "Human" ) ) {
          return new Human( vis as OpenGLStateVisualizer ) as Algorithm<SS, DS>;
        } else if ( Key.Equals( "NoOp" ) ) {
          return new NoOp<SS,DS>( ) ;
          // Some Special Algs. for myself.
        } else if ( Key.Equals( "depth-first-closed-list" ) ) {
          return new DepthFirstSearch<SS, DS>( Metric.Zero, true, vis );
        } else if ( Key.Equals( "depth-first-closed-list-bounded" ) ) {
          return new DepthFirstSearch<SS, DS>( new Metric( double.Parse( Data[offset] ) ),
            true, vis );
        } else if ( Key.Equals( "depth-first-duplicate-detection" ) ) {
          return new DepthFirstSearch<SS, DS>( Metric.Zero, false, vis );
        } else if ( Key.Equals( "depth-first-duplicate-detection-bounded" ) ) {
          return new DepthFirstSearch<SS, DS>( new Metric( double.Parse( Data[offset] ) ),
            false, vis );
        /*} else if ( Key.Equals( "depth-first-id-closed-list" ) ) {
          return new IterativeDeepingSearch<SS, DS>(
                 new DepthFirstSearch<SS, DS>( new Metric( 1.0d ), true, vis ) );
        } else if ( Key.Equals( "depth-first-id-duplicate-detection" ) ) {
          return new IterativeDeepingSearch<SS, DS>(
                 new DepthFirstSearch<SS, DS>( new Metric( 1.0d ), false, vis ) );
         */
        }
          // Start A1 Algs.
        else if ( Key.Equals( "depth-first" ) ) {
          return new DepthFirstSearch<SS, DS>( Metric.Zero, true, vis );
        } /*else if ( Key.Equals( "depth-first-id" ) ) {
          return new IterativeDeepingSearch<SS, DS>(
                 new DepthFirstSearch<SS, DS>( new Metric( 1.0d ), false, vis ) );
        } */ else if ( Key.Equals( "uniform-cost" ) ) {
          return new UniformedCostSearch<SS, DS>( true, vis );
        } else if ( Key.Equals( "greedy" ) ) {
          return new Greedy<SS, DS>( h, true, vis );
        } else if ( Key.Equals( "A*" ) ) {
          return new Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.AStar<SS, DS>(
            h, true, vis );
        } else if ( Key.Equals( "weighted-a-star" ) ) {
          return new WeightedAStar<SS, DS>( 
                 HeuristicFactory.GetHeuristic( Data[offset + 1] ), true,
                 new Metric( double.Parse( Data[offset] ) ), vis );
        } else if ( Key.Equals( "weighted-a-star-duplicate-detection" ) ) {
          return new WeightedAStar<SS, DS>( 
                 HeuristicFactory.GetHeuristic( Data[offset + 1] ), false,
                 new Metric( double.Parse( Data[offset] ) ), vis );
        } /*else if ( Key.Equals( "ida-star" ) ) {
          return new IterativeDeepingSearch<SS, DS>(
                 new Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.AStar<SS, DS>(
                   new PathLengthGComputer<SS, DS>( ),
                 HeuristicFactory.GetHeuristic( Data[1] ), new Metric( 1.0d ), true, vis ) );
          // Real Time Algorithms
        } else if ( Key.Equals( "LRTA*" ) ) {
          return new LRTAStar<SS, DS>( h, true, vis, Weight, false );
        } else if ( Key.Equals( "LRTA*-dyn" ) ) {
          return new LRTAStar<SS, DS>( h, false, vis, Weight, false );
        } else if ( Key.Equals( "RTA*" ) ) {
          return new LRTAStar<SS, DS>( h, true, vis, Weight, true );
        } else if ( Key.Equals( "RTA*-dyn" ) ) {
          return new LRTAStar<SS, DS>( h, false, vis, Weight, true );
        } */
            else if ( Key.Equals( "D*Lite" ) ) {
          return new DStarLite<SS, DS>( h, sut, vis );
        } else if ( Key.Equals( "Anytime-D*" ) ) {
          return new AnytimeDStar<SS, DS>( h, sut, vis );
        } else if ( Key.Equals( "LSS-LRTA*" ) ) {
          return new LSSLRTAStar<SS, DS>( h, vis, sut );
        } else if ( Key.Equals( "A*-RealTime" ) ) {
          return new Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime.
          AStar<SS, DS>( h );
        } else if ( Key.StartsWith( "RealTimeD*Lite" ) ) {
          ChooseMethodLandMarkGoal lmg = ChooseMethodLandMarkGoal.Goal;
          ChooseMethodFilterSucc fs = ChooseMethodFilterSucc.All;
          ChooseMethodStep step = ChooseMethodStep.MinHLandmarkGoal;
          if ( Key.Contains( "LSS-LRTA*" ) ) {  // must be b4 lrta*
            step = ChooseMethodStep.LSSLRTAStar;
          } else if ( Key.Contains( "LRTA*" ) ) {
            step = ChooseMethodStep.LRTAStar;
          } else if ( Key.Contains( "RTA*" ) ) {
            step = ChooseMethodStep.RTAStar;
          } else if ( Key.Contains( "AStar" ) ) {
            step = ChooseMethodStep.AStar;
          } else if ( Key.Contains( "NoOp" ) ) {
            step = ChooseMethodStep.Null;
          } else if ( Key.Contains( "Adjusted" ) ) {
            step = ChooseMethodStep.MinHAdustedLandmarkGoal;
          }

          if ( Key.Contains( "Goal" ) ) {
            lmg = ChooseMethodLandMarkGoal.Goal;
          } else if ( Key.Contains( "USample" ) ) {
            lmg = ChooseMethodLandMarkGoal.USample;
          } else if ( Key.Contains( "UTop" ) ) {
            lmg = ChooseMethodLandMarkGoal.UTop;
          }

          if ( Key.Contains( "Lock" ) ) {
            fs = ChooseMethodFilterSucc.Lock;
          }

          return new RealTimeDStarLite<SS, DS>( h, sut, vis,
            lmg, fs, step, Weight, Ratio );
        } else if ( Key.StartsWith( "RealTimeAnytimeD*" ) ) {
          ChooseMethodStep step = ChooseMethodStep.LSSLRTAStar;
          if ( Key.Contains( "LSS-LRTA*" ) ) {  // must be b4 lrta*
            step = ChooseMethodStep.LSSLRTAStar;
          } else if ( Key.Contains( "LRTA*" ) ) {
            step = ChooseMethodStep.LRTAStar;
          } else if ( Key.Contains( "RTA*" ) ) {
            step = ChooseMethodStep.RTAStar;
          } else if ( Key.Contains( "NoOp" ) ) {
            step = ChooseMethodStep.Null;
          }
          return new RealTimeAnytimeDStar<SS, DS>( h, sut, vis, step, Ratio );
        } else if ( Key.Equals( "BRTA*" ) ) {
          return new BRTSAStar<SS, DS>( h, sut );
        } else if ( Key.Equals( "BRTwA*" ) ) {
          return new BRTSwAStar<SS, DS>( h, sut, Weight );
        /*} else if ( Key.Equals( "TBA*-Old" ) ) {
          return new TBAStar<SS, DS>( ( SS, DS, G, Os ) => gc.G( SS, DS, G ) + h.H( SS, DS, G, Os ) );
        } else if ( Key.Equals( "TBA*" ) ) {
          return new TBAStar2<SS, DS>( ( SS, DS, G, Os ) => gc.G( SS, DS, G ) + h.H( SS, DS, G, Os ), 0.9f, 10 );
        } else if ( Key.Equals( "TBA*-LPA*" ) ) {
          return new TBAStar2<SS, DS>( h, sut, 0.9f, 10, vis );
        } else if ( Key.StartsWith( "RealTimeTBA*-LPA*" ) ) {
          ReconstructPathStartingPoint rpsp = ReconstructPathStartingPoint.UTop;
          ShortcutDetection sd = ShortcutDetection.None;

          if ( Key.Contains( "-UTop" ) ) {
            rpsp = ReconstructPathStartingPoint.UTop;
          } else if ( Key.Contains( "-BestH" ) ) {
            rpsp = ReconstructPathStartingPoint.BestH;
          } else if ( Key.Contains( "-Alternate" ) ) {
            rpsp = ReconstructPathStartingPoint.Alternate;
          }

          if ( Key.Contains( "-ShortcutDetection" ) ) {
            sd = ShortcutDetection.AStar;
          }

          return new RealTimeTBAStar<SS, DS>( h, sut, 0.9f, 10, vis, rpsp, sd );*/
        } else if ( Key.Equals( "BRTA*Sorting" ) ) {
          return new BRTSAStarSorting<SS, DS>( h, sut );
        } else if ( Key.Equals( "LPA*" ) ) {
          return new LPAStar<SS, DS>( h, sut, vis, ReconstructPathStartingPoint.UTop );
        } else if ( Key.Equals( "" ) ) {
          return null;
        } else {
          throw new AlgorithmNotFoundException(
            "Algorithm [ " + Key + " ] Not Implemented" );
        }
      } catch ( FormatException ) {
        throw new AlgorithmNotFoundException(
            "Algorithm [ " + Key + " ] Problem Parsing Arguments" );
      }
    }

    public GenericAlgorithmFactory<SS, DS> SetRealTimeData( Heuristic<SS, DS> h,
      StateVisualizer<SS, DS> vis, Transformer<SS, DS> sut, Metric Weight,
      double Ratio ) {
      this.h = h;
      this.vis = vis;
      this.sut = sut;
      this.Weight = Weight;
      this.RealTimeDataSet = true;
      this.Ratio = Ratio;
      return this;
    }

    private Heuristic<SS, DS> h;

    private StateVisualizer<SS, DS> vis;

    private Transformer<SS, DS> sut;

    private Metric Weight = new Metric( 1.0d );

    private double Ratio = 0.3d;

    bool RealTimeDataSet = false;

  }

}
