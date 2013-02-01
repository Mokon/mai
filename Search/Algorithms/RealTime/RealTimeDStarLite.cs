/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Harness;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.RealTime {

  public class RealTimeDStarLite<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public RealTimeDStarLite( Heuristic<SS, DS> H,
      Transformer<SS, DS> Transformer,
      StateVisualizer<SS, DS> sv, ChooseMethodLandMarkGoal lgm,
      ChooseMethodFilterSucc fsm, ChooseMethodStep sm, Metric Weight, double Ratio ) {
      this.Transformer = Transformer;
      this.H = H;
      this.sv = sv;
      this.lgm = lgm;
      this.fsm = fsm;
      this.sm = sm;
      this.Weight = Weight;
      this.Ratio = Ratio;
    }

    private double Ratio;
    private StateVisualizer<SS, DS> sv;
    private Transformer<SS, DS> Transformer;
    protected Heuristic<SS, DS> H;
    HashSet<DS> LockedList;
    private USample us;
    private Dictionary<string, Metric> RHS;
    private Dictionary<string, Metric> G;
    private Dictionary<string, Metric> HAdjusted;
    private PriorityQueue<SvenKey, DS> U;
    private Metric km;
    private DS Sgoal;
    private DS Sstart;
    private SS ss;
    private Operator<SS, DS>[] Ops;
    int IterationExpansions;
    private Metric Weight = new Metric( 1.0 );

    private void init( Goal<SS, DS> Goal, SS ss, DS Sstart,
        Operator<SS, DS>[] Actions ) {
      LockedList = new HashSet<DS>( );
      RHS = new Dictionary<string, Metric>( );
      G = new Dictionary<string, Metric>( );
      HAdjusted = new Dictionary<string, Metric>( );
      this.Sgoal = Transformer.Transform( Goal, Sstart );
      this.Sstart = Sstart;
      this.ss = ss;
      this.Ops = Actions;
      us = new USample( );
      switch ( this.sm ) {
        case ChooseMethodStep.LSSLRTAStar:
          h0 = ( x, y ) => H.H( ss, x, y, Actions );
          tree = new ExtendedDictionary<DS, DS>( X => null );
          HLSS = new ExtendedDictionary<DS, Metric>( x => h0( x, Sgoal ) );
          Incumbant = null;
          break;
      }
    }

    private Metric h( DS Sstart, DS Sgoal ) {
      return new Metric(1) * H.H( ss, Sstart, Sgoal, Ops );
    }

    public Metric c( DS s1, DS s2 ) {
      if ( s1.IsInvalid( ss, false ) || s2.IsInvalid( ss, false ) ) {
        return Metric.Infinity;
      } else {
        return h( s1, s2 );
      }
    }

    private Metric rhs( DS ds ) {
      Metric r;
      if ( RHS.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private void rhs( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( RHS.ContainsKey( key ) ) {
        RHS.Remove( key );
      }
      RHS.Add( key, value );
    }

    private Metric g( DS ds ) {
      Metric r;
      if ( G.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private void g( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( G.ContainsKey( key ) ) {
        G.Remove( key );
      }
      G.Add( key, value );
    }

    private Metric hAdjusted( DS ds, DS goal ) {
      Metric r;
      if ( HAdjusted.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return h( ds, goal );
      }
    }

    private void hAdjusted( DS ds, Metric value ) {
      string key = ds.ToString( );
      if ( HAdjusted.ContainsKey( key ) ) {
        HAdjusted.Remove( key );
      }
      HAdjusted.Add( key, value );
    }

    private SvenKey CalculateKey( DS s ) {
      return new SvenKey( ) {
        k1 = Min( g( s ), rhs( s ) ) + h( Sstart, s ) + km,
        k2 = Min( g( s ), rhs( s ) )
      };
    }

    private void Initialize( ) {
      U = new PriorityQueue<SvenKey, DS>( );
      km = Metric.Zero;
      // for all s E S rhs(s) = g(s) = inf ; // Implicit
      rhs( Sgoal, Metric.Zero );
      us.Push( Sgoal );
      U.Insert( Sgoal, new SvenKey( ) {
        k1 = h( Sstart, Sgoal ),
        k2 = Metric.Zero
      } );
    }

    private void UpdateVertex( DS u ) {
      bool there = U.Contains( u );
      if ( !g( u ).Equals( rhs( u ) ) && there ) {
        U.Update( u, CalculateKey( u ) );
      } else if ( !g( u ).Equals( rhs( u ) )&& !there ) {
        U.Insert( u, CalculateKey( u ) );
        us.Push( u );
      } else if ( g( u ).Equals( rhs( u ) ) && there ) {
        U.Remove( u );
        us.Pop( u );
      }
    }

    private bool ComputeShortestPathNormalTermination;

    /* This that must be real time
     * Insertion, Updating, removing, Choose Step, # Changes
     */
    private void ComputeShortestPath( ) {
      while ( TopKey( ) < CalculateKey( Sstart ) || rhs( Sstart ) > g( Sstart ) ) {
        if ( IterationExpansions-- == 0 ) {
          break;
        }
#if OpenGl
        if ( sv != null ) {
          this.sv.VisualizeAlg( ss, ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
            G = G, RHS = this.RHS, U = this.U as PriorityQueue<SvenKey, GenericGridWorldDynamicState>,
            US = us as RealTimeDStarLite<GenericGridWorldStaticState, GenericGridWorldDynamicState>.USample,
            LL = this.LockedList as HashSet<GenericGridWorldDynamicState>
          } );
        }
#endif
        this.Expansions++;
        var u = U.Top( );
        var kold = TopKey( );
        var knew = CalculateKey( u );
        if ( kold < knew ) {
          U.Update( u, knew );
        } else if ( g( u ) > rhs( u ) ) {
          g( u, rhs( u ) );
          U.Remove( u );
          us.Pop( u );
          foreach ( var s in Pred( u ) ) {
            this.Generations++;
            if ( !s.Equals( Sgoal ) ) {
              rhs( s, Min( rhs( s ), c( s, u ) + g( u ) ) );
            }
            UpdateVertex( s );
          }
        } else {
          var gold = g( u );
          g( u, Metric.PositiveInfinity );
          foreach ( var s in PredUnionSelf( u ) ) {
            this.Generations++;
            if ( rhs( s ) == ( c( s, u ) + gold ) ) {
              if ( !s.Equals( Sgoal ) ) {
                rhs( s, Min( sp => c( s, sp ) + g( sp ), Succ( s ) ) );
              }
            }
            UpdateVertex( s );
          }
        }
      }
      ComputeShortestPathNormalTermination = 
        TopKey( ) < CalculateKey( Sstart ) || rhs( Sstart ) > g( Sstart );
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS ss, DS ds,
      Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      init( Goal, ss, ds, Actions );
      IterationExpansions = GetGlobalSearchComputationLimit( );
      var Slast = Sstart;
      Initialize( );
      ComputeShortestPath( );
      while ( !Sstart.Equals( Sgoal ) ) {
        if ( !ComputeShortestPathNormalTermination 
            && rhs( Sstart ).Equals( Metric.PositiveInfinity ) ) {
          yield return ChooseStep( );
        } else {
          yield return ChooseStep( );
        }
        Sstart = ss.InitialDynamicState;
        IterationExpansions = GetGlobalSearchComputationLimit( );
        bool Changes = ss.HasRecentChanges( );
        if ( Changes ) {
          km += h( Slast, Sstart ); //inria
          Slast = Sstart;
          foreach ( var u in ss.Changes ) {
            foreach ( var v in Succ( u ) ) {
              UpdateDirectedEdge( u, v );
              UpdateDirectedEdge( v, u );
            }
          }
        }
        ComputeShortestPath( );
      }
      yield break;
    }

    private void UpdateDirectedEdge( DS u, DS v ) {
      Metric cold = c( u, v ).Equals( Metric.Infinity ) ? h( u, v ) : Metric.Infinity;
      if ( cold > c( u, v ) ) {
        if ( !u.Equals( Sgoal ) ) {
          rhs( u, Min( rhs( u ), c( u, v )+ g( v ) ) );
        }
      } else if ( rhs( u ) == cold + g( v ) ) {
        if ( !u.Equals( Sgoal ) ) {
          rhs( u, Min( sp => c( u, sp ) + g( sp ), Succ( u ) ) );
        }
      }
      UpdateVertex( u );
    }

    private ChooseMethodLandMarkGoal lgm;
    private ChooseMethodFilterSucc fsm;
    private ChooseMethodStep sm;

    private Path<SS, DS> ChooseStep( ) {
      try {
        if ( !ComputeShortestPathNormalTermination && !rhs( Sstart ).Equals( Metric.PositiveInfinity ) ) {
          var ret = this.ExtractPath( );
          ( this.sv as OpenGLStateVisualizer ).Global = true;
          return ret;
        } else {
          ( this.sv as OpenGLStateVisualizer ).Global = false;
          if ( sm.Equals( ChooseMethodStep.Null ) ) {
            return null;
          }
          /// Choose a landmark goal state to step towards.
          DS LandmarkGoal = null;
          switch ( lgm ) {
            case ChooseMethodLandMarkGoal.UTop:
              LandmarkGoal = U.Top( );
              break;
            case ChooseMethodLandMarkGoal.USample:
              LandmarkGoal = Nearest( Sstart, us );
              break;
            case ChooseMethodLandMarkGoal.Goal:
              LandmarkGoal = Sgoal;
              break;

          }
          if ( LandmarkGoal == null ) {
            return null;
          }
          
          /// Choose the successor nodes we want to consider to move toward.
          IEnumerable<DS> succ = null;
          bool LLR = false;
          switch ( fsm ) {
            case ChooseMethodFilterSucc.All:
              succ = Succ( Sstart );
              break;
            case ChooseMethodFilterSucc.Lock:
              succ = Succ( Sstart );
              if ( succ == null ) {
                break;
              }
              succ = succ.Where( st => !this.LockedList.Contains( st ) );
              if ( succ.Count( ) == 0 ) {
                succ = Succ( Sstart );
                LLR = true;
                LockedList.Add( Sstart );
              }
              // Test if should add to dead end list.
              if ( succ.Where( st => !this.LockedList.Contains( st ) ).All(
                  ds => h( ds, LandmarkGoal ) > h( Sstart, LandmarkGoal ) ) ) {
                LockedList.Add( Sstart );
              }
              break;
          }
          if ( succ == null ) {
            return null;
          }
          
          /// Choose the state we want to step towards.
          DS next = null;
          switch ( sm ) {
            case ChooseMethodStep.MinHLandmarkGoal:
              this.Expansions++;
              this.Generations += (uint)succ.Count( );
              next = succ.ArgMin( ds => h( ds, LandmarkGoal ) );
              break;
            case ChooseMethodStep.MinHAdustedLandmarkGoal:
              this.Expansions++;
              this.Generations += (uint)succ.Count( );
              Metric minV = null;
              foreach ( var s in succ ) {
                Metric h = hAdjusted( s, Sgoal );
                if ( minV == null || ( h != null && minV > h ) ) {
                  minV = h;
                  next = s;
                }
              }
              // Update hAdjusted for current state.
              if ( minV != null ) {
                hAdjusted( Sstart, minV + h( next, Sstart ) );
              }
              break;
            /*case ChooseMethodStep.LRTAStar:
              int Expansions = GetLocalSearchComputationLimit( );
              var P = LRTAStar<SS, DS>.AStarLookAhead( ss.InitialDynamicState, ss,
                  Expansions, sv,
                  ref this.Expansions, ref this.Generations, false,
                  Ops, HAdjusted, H, new InStateGoal<SS, DS>( LandmarkGoal ), Weight, false );
              if ( P == null ) {
                next = null;
              } else {
                next = P.Actions.First.Value.PerformOn( Sstart, ss ).First( );
              }
              break;
            case ChooseMethodStep.RTAStar:
              Expansions = GetLocalSearchComputationLimit( );
              P = LRTAStar<SS, DS>.AStarLookAhead( ss.InitialDynamicState, ss,
                  Expansions, sv,
                  ref this.Expansions, ref this.Generations, false,
                  Ops, HAdjusted, H, new InStateGoal<SS, DS>( LandmarkGoal ), Weight, true );
              if ( P == null ) {
                next = null;
              } else {
                next = P.Actions.First.Value.PerformOn( Sstart, ss ).First( );
              }
              break;
            case ChooseMethodStep.AStar:
              Expansions = GetLocalSearchComputationLimit( );
              P = LRTAStar<SS, DS>.AStarLookAhead( ss.InitialDynamicState, ss,
                  Expansions, sv,
                  ref this.Expansions, ref this.Generations, false,
                  Ops, null, H, new InStateGoal<SS, DS>( LandmarkGoal ), Weight, false );
              if ( P == null ) {
                next = null;
              } else {
                next = P.Actions.First.Value.PerformOn( Sstart, ss ).First( );
              }
              break;*/
            case ChooseMethodStep.LSSLRTAStar:
              int Expansions = GetLocalSearchComputationLimit( );
              var P = LSSLRTAStar<SS, DS>.LSSLRTAStarLookahead( Expansions, ss, Sstart,
                Sgoal, Ops, h0, ref Incumbant, ref this.Expansions, sv, tree, HLSS );
              if ( P == null ) {
                next = null;
              } else {
                next = P.Actions.First.Value.PerformOn( Sstart, ss ).First( );
              }
              break;
          }
          
          if ( next == null ) {
            return null;
          }

          if ( LLR ) {
            LockedList.Remove( next );
            LLR = false;
          }

          /// Extract the path info.
          Operator<SS, DS> Op = null;
          foreach ( var action in Ops ) {
            if ( action.IsValidOn( Sstart, ss, false ) && action.PerformOn( Sstart, ss ).First( ).Equals( next ) ) {
              Op = action;
              break;
            }
          }
          if ( Op == null ) {
            return null;
          } else {
            return Operator<SS, DS>.MakePath( Op );
          }
        }
      } catch ( InvalidOperationException ) {
        return null;
      }

    }
    private Func<DS, DS, Metric> h0;
    private ExtendedDictionary<DS, DS> tree;
    private ExtendedDictionary<DS, Metric> HLSS;
    private Path<SS, DS> Incumbant;

    private Path<SS, DS> ExtractPath( ) {
      DS next = ArgMin( sp => c( Sstart, sp ) + g( sp ), Succ( Sstart ) );
      Operator<SS, DS> Op = null;
      foreach ( var action in Ops ) {
        if ( action.IsValidOn( Sstart, ss, false ) && action.PerformOn( Sstart, ss ).First( ).Equals( next ) ) {
          Op = action;
          break;
        }
      }
      if ( Op == null ) {
        Console.WriteLine( "Error, Op ExtractPath Null" );
        return null;
      } else {
        return Operator<SS, DS>.MakePath( Op );
      }
    }

    public int GetGlobalSearchComputationLimit( ) {
      switch ( sm ) {
        case ChooseMethodStep.MinHLandmarkGoal:
        case ChooseMethodStep.MinHAdustedLandmarkGoal:
          return ComputationLimit - 1;
        case ChooseMethodStep.Null:
          return ComputationLimit;
        case ChooseMethodStep.AStar:
        case ChooseMethodStep.LRTAStar:
        case ChooseMethodStep.RTAStar:
        case ChooseMethodStep.LSSLRTAStar:
          return ComputationLimit - (int)Math.Ceiling( ComputationLimit*Ratio );
        default:
          throw new InvalidOperationException( );
      }
    }

    public int GetLocalSearchComputationLimit( ) {
      switch ( sm ) {
        case ChooseMethodStep.MinHLandmarkGoal:
        case ChooseMethodStep.MinHAdustedLandmarkGoal:
          return 1;
        case ChooseMethodStep.Null:
          return 0;
        case ChooseMethodStep.AStar:
        case ChooseMethodStep.LRTAStar:
        case ChooseMethodStep.RTAStar:
        case ChooseMethodStep.LSSLRTAStar:
          return (int)Math.Ceiling( ComputationLimit*Ratio );
        default:
          throw new InvalidOperationException( );
      }
    }

    private Metric Min( Func<DS, Metric> func, params DS[] args ) {
      Metric min = Metric.PositiveInfinity;
      foreach ( DS d in args ) {
        Metric v = func( d );
        if ( v < min ) {
          min = v;
        }
      }
      return min;
    }

    private Metric Min( params Metric[] args ) {
      Metric min = Metric.PositiveInfinity;
      foreach ( Metric d in args ) {
        if ( d < min ) {
          min = d;
        }
      }
      return min;
    }

    private DS ArgMin( Func<DS, Metric> func, params DS[] args ) {
      DS arg = null;
      Metric min = Metric.PositiveInfinity;
      foreach ( DS d in args ) {
        Metric v = func( d );
        if ( v <= min ) {
          min = v;
          arg = d;
        }
      }
      return arg;
    }

    private DS[] Pred( DS u ) {
      return u.Neighbors( Ops, ss, false ).ToArray( );
    }

    private DS[] PredUnionSelf( DS u ) {
      var _ = u.Neighbors( Ops, ss, false );
      _.AddFirst( u );
      return _.ToArray( );
    }

    private DS[] Succ( DS u ) {
      return u.Neighbors( Ops, ss, false ).ToArray( );
    }

    private SvenKey TopKey( ) {
      var _ = U.TopKey( );
      if ( _ == null ) {
        _ = new SvenKey( ) {
          k1 = Metric.PositiveInfinity,
          k2 = Metric.PositiveInfinity
        };
      }
      return _;
    }

    #region USample

    private DS Nearest( DS ds, USample us ) {
      DS nearest = null;
      Metric minValue = Metric.Zero;
      for ( int i = 0 ; i < USample.SampleSize ; i++ ) {
        if ( us.Sample[i] != null ) {
          Metric dis = h( us.Sample[i], ds );
          if ( nearest == null || minValue > dis ) {
            nearest = us.Sample[i];
            minValue = dis;
          }
        }
      }
      return nearest;
    }

    public class USample {
      public const int SampleSize = 10;
      public DS[] Sample = new DS[USample.SampleSize];
      private int Cur = 0;
      private Random gen = new Random( (int)DateTime.Now.Ticks );
      private int Chance = 2;
      public void Push( DS ds ) {
        if ( gen.Next( )%Chance == 0 || !HasAny( ) ) {
          Sample[Cur++] = ds;
          Cur %= SampleSize;
        }
      }

      public bool Contains( String ds ) {
        for ( int i = 0 ; i < SampleSize ; i++ ) {
          if ( Sample[i] != null && Sample[i].ToString( ).Equals( ds ) ) {
            return true;
          }
        }
        return false;
      }

      public bool HasAny( ) {
        for ( int i = 0 ; i < SampleSize ; i++ ) {
          if ( Sample[i] != null ) {
            return true;
          }
        }
        return false;
      }

      public void Pop( DS ds ) {
        bool found = false;
        for ( int i = 0 ; i < SampleSize ; i++ ) {
          if ( Sample[i] != null && Sample[i].Equals( ds ) ) {
            Sample[i] = null;
            found = true;
            for ( int j = 0 ; j < SampleSize ; j++ ) {
              if ( Sample[j] == null ) {
                for ( int k = j + 1 ; k < SampleSize ; k++ ) {
                  Sample[k-1] = Sample[k];
                }
              }
            }
            break;
          }
        }

        if ( found ) {
          for ( int z = 0 ; z < SampleSize ; z++ ) {
            if ( Sample[z] == null ) {
              Cur = z;
              break;
            }
          }
        }
      }
    }

    #endregion
  }

  public enum ChooseMethodLandMarkGoal {
    UTop,
    USample,
    Goal
  }
  public enum ChooseMethodFilterSucc {
    All,
    Lock,
  }

  public enum ChooseMethodStep {
    MinHLandmarkGoal,
    MinHAdustedLandmarkGoal,
    Null,
    AStar,
    LRTAStar,
    RTAStar,
    LSSLRTAStar,
  }

}
