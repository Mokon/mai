/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using Net.Mokon.Utilities;

namespace Net.Mokon.Edu.Unh.CS.AI.Search.Bookkeepings {

  /// <summary>
  /// An implementation of the book keeping interace that keeps track of a
  /// parent node.
  /// </summary>
  public class ParentBookKeeping<SS, DS> : BookKeeping<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

    /// <summary>
    /// The parent dynamic state for this node.
    /// </summary>
    public DS Parent ;

    /// <summary>
    /// A place to store H estimates.
    /// </summary>
    public Metric H;
  }

}
