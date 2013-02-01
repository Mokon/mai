/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Edu.Unh.CS.AI.Search;
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Bookkeepings {

  /// <summary>
  /// An implementation of the book keeping interace that keeps track of a
  /// parent node.
  /// </summary>
  public class DStarBRTSBookKeeping<SS, DS> : ParentBookKeeping<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    public Metric rhs = Metric.PositiveInfinity;
    public Metric g = Metric.PositiveInfinity;
  }

}
