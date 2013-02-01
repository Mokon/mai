/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  /// <summary>
  /// Transforms a unit goal into a dynamic state.
  /// </summary>
  public class SingleUnitTransformer : Transformer<GenericGridWorldStaticState,
    GenericGridWorldDynamicState> {

    /// <summary>
    /// Transforms a state and goal into a state.
    /// </summary>
    /// <param name="Goal"></param>
    /// <param name="DynamicState"></param>
    /// <returns></returns>
    public override GenericGridWorldDynamicState Transform(
      Goal<GenericGridWorldStaticState, GenericGridWorldDynamicState> Goal,
      GenericGridWorldDynamicState DynamicState ) {
      GenericGridWorldDynamicState ds =
        DynamicState.Clone( ) as GenericGridWorldDynamicState;

      foreach ( var Unit in ds.GetWorldObjects( ) ) {
        if ( Unit is Unit ) {
          var U = Unit as Unit;
          ds.hash ^= U.GetHashCode( );
          U.X = U.DstX;
          U.Y = U.DstY;
          ds.hash ^= U.GetHashCode( );
        }
      }
      return ds;
    }

  }

}
