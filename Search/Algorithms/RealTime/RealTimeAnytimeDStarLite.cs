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

  public class RealTimeAnytimeDStar<SS, DS> : RealTimeAlgorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    private AnytimeDStarState aS;
    private Transformer<SS, DS> Transformer;
    private Heuristic<SS, DS> H;
    private ChooseMethodStep sm;
    private double Ratio;

    public RealTimeAnytimeDStar( Heuristic<SS, DS> H, Transformer<SS, DS> Transformer,
      StateVisualizer<SS, DS> sv, ChooseMethodStep sm, double Ratio ) {
      this.Transformer = Transformer;
      this.H = H;
      this.aS = new RealTimeAnytimeDStar<SS, DS>.AnytimeDStarState( );
      this.aS.sv = sv;
      this.sm = sm;
      this.Ratio = Ratio;
    }

    private void init( SS ss, DS ds, Operator<SS, DS>[] Ops, Goal<SS, DS> Goal ) {
      this.aS.ss = ss;
      this.aS.Expansions = 0;
      this.aS.Generations = 0;
      this.aS.Sstart = ds;
      this.aS.Ops = Ops;
      this.aS.Sgoal = Transformer.Transform( Goal, ds );
      this.aS.h = ( s1, s2 ) => H.H( ss, s1, s2, Ops );
    }

    public int GetGlobalSearchComputationLimit( ) {
      switch ( sm ) {
        case ChooseMethodStep.Null:
          return ComputationLimit;
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
        case ChooseMethodStep.Null:
          return 0;
        case ChooseMethodStep.LRTAStar:
        case ChooseMethodStep.RTAStar:
        case ChooseMethodStep.LSSLRTAStar:
          return (int)Math.Ceiling( ComputationLimit*Ratio );
        default:
          throw new InvalidOperationException( );
      }
    }

    public override IEnumerable<Path<SS, DS>> Compute( SS ss, DS ds, Goal<SS, DS> Goal, Operator<SS, DS>[] Ops ) {
      this.init( ss, ds, Ops, Goal );
      ExtendedDictionary<DS, Metric> H1 = new ExtendedDictionary<DS, Metric>( x=> aS.h( x, aS.Sgoal) ) ;
      ExtendedDictionary<DS, DS> tree = new ExtendedDictionary<DS, DS>( x => null );
      Path<SS, DS> Incumbant = null;
      int GlobalSearchLimit = GetGlobalSearchComputationLimit( );
      this.aS.ComputationLimit = GlobalSearchLimit;
      int LocalSearchLimit = GetLocalSearchComputationLimit( );
      foreach ( Path<SS, DS> p in AnytimeDStarLiteLookahead( aS, GlobalSearchLimit ) ) {
        this.Expansions += aS.Expansions;
        this.Generations += aS.Generations;
        aS.Expansions = 0;
        aS.Generations = 0;
        if ( p == null ) {
          switch ( sm ) {
            /*case ChooseMethodStep.LRTAStar:
              yield return LRTAStar<SS, DS>.AStarLookAhead( ss.InitialDynamicState, ss,
                  LocalSearchLimit, aS.sv,
                  ref this.Expansions, ref this.Generations, false,
                  Ops, H1, H, Goal, Metric.One, false );
              break;
            case ChooseMethodStep.RTAStar:
              yield return LRTAStar<SS, DS>.AStarLookAhead( ss.InitialDynamicState, ss,
                  LocalSearchLimit, aS.sv,
                  ref this.Expansions, ref this.Generations, false,
                  Ops, H1, H, Goal, Metric.One, true );
              break;*/
            case ChooseMethodStep.LSSLRTAStar:
              yield return LSSLRTAStar<SS, DS>.LSSLRTAStarLookahead( LocalSearchLimit, ss, aS.Sstart,
                aS.Sgoal, Ops, aS.h, ref Incumbant, ref this.Expansions, aS.sv, tree, H1 );
              break;
          }
        } else {
          yield return p;
        }
        aS.Sstart = aS.ss.InitialDynamicState;
      }
      yield break;
    }

    private static SvenKey key( DS s, AnytimeDStarState aS ) {
      if ( g( s, aS ) > rhs( s, aS ) ) {
        return new SvenKey( ) {
          k1 = rhs( s, aS ) + aS.epsilon*aS.h( aS.Sstart, s ),
          k2 = rhs( s, aS )
        };
      } else {
        return new SvenKey( ) {
          k1 = g( s, aS ) + aS.h( aS.Sstart, s ),
          k2 = g( s, aS )
        };
      }
    }

    private static void UpdateState( DS s, AnytimeDStarState aS ) {
      if ( !s.Equals( aS.Sgoal ) ) {
        rhs( s, Min( sp => c( s, sp, aS ) + g( sp, aS ), Succ( s, aS ) ), aS );
      }
      if ( aS.OPEN.Contains( s ) ) {
        aS.OPEN.Remove( s );
      }
      if ( g( s, aS ) != rhs( s, aS ) ) {
        if ( !aS.CLOSED.Contains( s ) ) {
          aS.OPEN.Enqueue( key( s, aS ), s );
        } else {
          aS.INCONS.Add( s );
        }
      }
    }

    private static SvenKey TopKey( AnytimeDStarState aS ) {
      var _ = aS.OPEN.TopKey( );
      if ( _ == null ) {
        _ = new SvenKey( ) {
          k1 = Metric.PositiveInfinity,
          k2 = Metric.PositiveInfinity
        };
      }
      return _;
    }

    private enum Status {
      ComputationLimitExceded,
      NormalTermination,
    }

    private static Status ComputeorImprovePath( AnytimeDStarState aS ) {
      while ( TopKey( aS ) < key( aS.Sstart, aS ) || rhs( aS.Sstart, aS ) != g( aS.Sstart, aS ) ) {
        aS.Expansions++;
#if OpenGl
        if ( aS.sv != null ) {
          aS.sv.VisualizeAlg( aS.ss, aS.ss.InitialDynamicState, new OpenGLStateVisualizer.OpenGlVisulizationData( ) {
            G = aS.G, RHS = aS.RHS, U = aS.OPEN as PriorityQueue<SvenKey, GenericGridWorldDynamicState>
          } );
        }
#endif
        if ( aS.Expansions > aS.ComputationLimit ) {
          return Status.ComputationLimitExceded;
        }
        var s = aS.OPEN.Dequeue( );
        if ( g( s, aS ) > rhs( s, aS ) ) {
          g( s, rhs( s, aS ), aS );
          aS.CLOSED.Add( s );
          foreach ( DS sp in Pred( s, aS ) ) {
            aS.Generations++;
            UpdateState( sp, aS );
          }
        } else {
          g( s, Metric.Infinity, aS );
          foreach ( DS sp in PredUnionSelf( s, aS ) ) {
            aS.Generations++;
            UpdateState( sp, aS );
          }
        }
      }
      return Status.NormalTermination;
    }

    public class AnytimeDStarState {
      public StateVisualizer<SS, DS> sv;
      public DS Sgoal;
      public Operator<SS, DS>[] Ops;
      public SS ss;
      public Dictionary<string, Metric> RHS;
      public Dictionary<string, Metric> G;
      public PriorityQueue<SvenKey, DS> OPEN;
      public HashSet<DS> CLOSED;
      public HashSet<DS> INCONS;
      public DS Sstart;
      public Func<DS, DS, Metric> h;
      public Metric epsilon;
      public uint Expansions;
      public uint Generations;
      public int ComputationLimit;
    }

    private static readonly Metric epsilon0 = new Metric( 3.0d );
    private static readonly Metric epsilonDecreaaseFactor = new Metric( 0.1d );
    private static readonly Metric epsilonIncreaseFactor = new Metric( 0.5d );

    public static IEnumerable<Path<SS, DS>> AnytimeDStarLiteLookahead(
      AnytimeDStarState aS, int ComputationTime ) {
      aS.RHS = new Dictionary<string, Metric>( );
      aS.G = new Dictionary<string, Metric>( );

      g( aS.Sstart, Metric.Infinity, aS );
      rhs( aS.Sstart, Metric.Infinity, aS );
      g( aS.Sgoal, Metric.Infinity, aS );
      rhs( aS.Sgoal, Metric.Zero, aS );
      aS.epsilon = epsilon0;

      aS.CLOSED = new HashSet<DS>( );
      aS.OPEN = new PriorityQueue<SvenKey, DS>( );
      aS.INCONS = new HashSet<DS>( );
      aS.OPEN.Enqueue( key( aS.Sgoal, aS ), aS.Sgoal );
      Status status;
      while ( aS.Sstart != aS.Sgoal ) {
        status = ComputeorImprovePath( aS );
        yield return MoveAgent( aS );
        bool SignficantChanges = false;
        if ( aS.ss.HasRecentChanges() ) {
          foreach ( var u in aS.ss.Changes ) {
            SignficantChanges |= IsSignificantChange( u, aS );
            foreach ( var v in Succ( u, aS ) ) {
              UpdateState( u, aS );
              UpdateState( v, aS );                                                      
            }
          }
          aS.ss.Changes.Clear( );
        }
        if ( SignficantChanges ) {
          aS.epsilon += epsilonIncreaseFactor;
          if ( aS.epsilon > epsilon0 ) {
            aS.epsilon = epsilon0;
          }
        } else if ( aS.epsilon > Metric.One ) {
          aS.epsilon -= epsilonDecreaaseFactor;
          if ( aS.epsilon < Metric.One ) {
            aS.epsilon= Metric.One;
          }
        }
        MoveINCONSintoOPENAndResort( aS );
        aS.CLOSED = new HashSet<DS>( );
      }
      yield break;
    }

    private static readonly Metric SignifcanceRatio = new Metric( 2.0 );
    private static bool IsSignificantChange( DS Change, AnytimeDStarState aS ) {
      //return true;
      return aS.h( Change, aS.Sgoal ) < aS.h( aS.Sstart, aS.Sgoal ) &&
             aS.h( Change, aS.Sstart ) < aS.h( aS.Sstart, aS.Sgoal ) &&
             aS.h( Change, aS.Sstart ) + aS.h( Change, aS.Sgoal ) <
              SignifcanceRatio* aS.h( aS.Sstart, aS.Sgoal );
    }

    private static void MoveINCONSintoOPENAndResort( AnytimeDStarState aS ) {
      var OldOPEN = aS.OPEN;
      aS.OPEN = new PriorityQueue<SvenKey, DS>( );
      foreach ( DS s in aS.INCONS ) {
        aS.OPEN.Enqueue( key( s, aS ), s );
      }
      while ( !OldOPEN.IsEmpty( ) ) {
        DS s = OldOPEN.Dequeue( );
        aS.OPEN.Enqueue( key( s, aS ), s );
      }
    }

    private static Path<SS, DS> MoveAgent( AnytimeDStarState aS ) {
      DS next = ArgMin( sp => c( aS.Sstart, sp, aS ) + g( sp, aS ), Succ( aS.Sstart, aS ) );
      if ( g( next, aS ) == Metric.Infinity ) {
        return null;
      }
      Operator<SS, DS> Op = null;
      foreach ( var action in aS.Ops ) {
        if ( action.IsValidOn( aS.Sstart, aS.ss, false ) && 
            action.PerformOn( aS.Sstart, aS.ss ).First( ).Equals( next ) ) {
          Op = action;
          break;
        }
      }
      if ( Op == null ) {
        return null;
      } else {
        aS.Sstart = next;
        return Operator<SS, DS>.MakePath( Op );
      }
    }

    private static Metric Min( Func<DS, Metric> func, params DS[] args ) {
      Metric min = Metric.PositiveInfinity;
      foreach ( DS d in args ) {
        Metric v = func( d );
        if ( v < min ) {
          min = v;
        }
      }
      return min;
    }

    private static DS ArgMin( Func<DS, Metric> func, params DS[] args ) {
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

    private static DS[] Pred( DS u, AnytimeDStarState aS ) {
      return u.Neighbors( aS.Ops, aS.ss, false ).ToArray( );
    }

    private static DS[] PredUnionSelf( DS u, AnytimeDStarState aS ) {
      var _ = u.Neighbors( aS.Ops, aS.ss, false );
      _.AddFirst( u );
      return _.ToArray( );
    }

    private static DS[] Succ( DS u, AnytimeDStarState aS ) {
      return u.Neighbors( aS.Ops, aS.ss, false ).ToArray( );
    }

    public static Metric c( DS s1, DS s2, AnytimeDStarState aS ) {
      if ( s1.IsInvalid( aS.ss, false ) || s2.IsInvalid( aS.ss, false ) ) {
        return Metric.Infinity;
      } else {
        return aS.h( s1, s2 );
      }
    }

    private static Metric rhs( DS ds, AnytimeDStarState aS ) {
      Metric r;
      if ( aS.RHS.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private static void rhs( DS ds, Metric value, AnytimeDStarState aS ) {
      string key = ds.ToString( );
      if ( aS.RHS.ContainsKey( key ) ) {
        aS.RHS.Remove( key );
      }
      aS.RHS.Add( key, value );
    }

    private static Metric g( DS ds, AnytimeDStarState aS ) {
      Metric r;
      if ( aS.G.TryGetValue( ds.ToString( ), out r ) ) {
        return r;
      } else {
        return Metric.PositiveInfinity;
      }
    }

    private static void g( DS ds, Metric value, AnytimeDStarState aS ) {
      string key = ds.ToString( );
      if ( aS.G.ContainsKey( key ) ) {
        aS.G.Remove( key );
      }
      aS.G.Add( key, value );
    }

  }
}
