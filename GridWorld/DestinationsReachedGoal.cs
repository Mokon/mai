/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// A goal that classifies based on unit destinations being reached.
  /// </summary>
  public class DestinationsReachedGoal : 
    Goal<GenericGridWorldStaticState, GenericGridWorldDynamicState> {

    /// <summary>
    /// Goal test function.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <returns></returns>
    public bool IsSatisfiedBy( GenericGridWorldStaticState 
      StaticState, GenericGridWorldDynamicState DynamicState ) {
      return DynamicState.DstsReached( );
    }

  }

}
