/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search {

  /// <summary>
  /// An abstract class that transforms states into another state.
  /// </summary>
  /// <typeparam name="SS"></typeparam>
  /// <typeparam name="DS"></typeparam>
  public abstract class Transformer<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// Transforms a state and goal into a state.
    /// </summary>
    /// <param name="Goal"></param>
    /// <param name="DynamicState"></param>
    /// <returns></returns>
    public abstract DS Transform( Goal<SS, DS> Goal, DS DynamicState );

  }

}
