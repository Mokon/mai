/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using System.Linq;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {
  /// <summary>
  /// This abstract class is for unlimited algorithms to implement.
  /// </summary>
  /// <typeparam name="SS"></typeparam>
  /// <typeparam name="DS"></typeparam>
  public abstract class UnlimitedStaticAlgorithm<SS, DS> : Algorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Converts the algorithm compute method into a more user friendly
    /// compute complete method.
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <param name="Goal"></param>
    /// <param name="Actions"></param>
    /// <returns></returns>
    public sealed override IEnumerable<Path<SS, DS>> Compute( SS StaticState,
    DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      bool valid;
      while ( true ) {
        Path<SS, DS> path = ComputeComplete( StaticState, StaticState.InitialDynamicState, Goal, Actions );
        foreach ( var Action in path.Actions ) {
          valid = Action.IsValidOn( StaticState.InitialDynamicState, StaticState, true );
          if ( valid ) {
            yield return Operator<SS, DS>.MakePath( Action );
            if ( Goal.IsSatisfiedBy( StaticState, StaticState.InitialDynamicState ) ) {
              yield break;
            }
          } else {
            break;
          }
        }

      }
    }

    /// <summary>
    /// A compute complete method that 
    /// </summary>
    /// <param name="StaticState"></param>
    /// <param name="DynamicState"></param>
    /// <param name="Goal"></param>
    /// <param name="Actions"></param>
    /// <returns></returns>
    public abstract Path<SS, DS> ComputeComplete( SS StaticState,
      DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions );

  }

}
