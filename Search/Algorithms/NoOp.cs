/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System.Collections.Generic;
using Net.Mokon.Edu.Unh.CS.AI.Search.Goals;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms {

  public class NoOp<SS, DS> : Algorithm<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public override IEnumerable<Path<SS,DS>> Compute( SS StaticState,
        DS DynamicState, Goal<SS, DS> Goal, Operator<SS, DS>[] Actions ) {
      while ( true ) {
        yield return null;
      }
    }

  }

}
