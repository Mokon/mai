/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Utilities;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;
using Net.Mokon.Edu.Unh.CS.AI.Search.Heuristics;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// A heuristic based on a single unit euclian disntance to destination.
  /// </summary>
  public class SingleUnitEuclianDistanceHeuristic : Heuristic<
    GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    /// <summary>
    /// Heursitic estimate.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <param name="Goal"></param>
    /// <param name="Operators"></param>
    /// <returns></returns>
    public override Metric H( GenericGridWorldStaticState StaticState,
     GenericGridWorldDynamicState DynamicState, Goal<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Goal,
      Operator<GenericGridWorldStaticState, GenericGridWorldDynamicState>[]
      Operators ) {
      Unit U = DynamicState.FindUnit( 0 );
      return new Metric( Net.Mokon.Utilities.Distance.EuclianDistance(
        U.X, U.DstX, U.Y, U.DstY ) );
    }

    /// <summary>
    /// Heurisitic estimate.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <param name="Goal"></param>
    /// <param name="Operators"></param>
    /// <returns></returns>
    public override Metric H( GenericGridWorldStaticState StaticState,
     GenericGridWorldDynamicState DynamicState, GenericGridWorldDynamicState
      Goal, Operator<GenericGridWorldStaticState,
      GenericGridWorldDynamicState>[] Operators ) {
      Unit U = DynamicState.FindUnit( 0 );
      Unit U2 = Goal.FindUnit( 0 );
      return new Metric( Distance.EuclianDistance( U.X, U2.X, U.Y, U2.Y ) );
    }

  }

}
