/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
namespace Net.Mokon.Edu.Unh.CS.AI.Search.Bookkeepings {

  /// <summary>
  /// This is a interface for all objects wishing to keep book keeping data
  /// for algorithms. What is exzactly contained in a book keeping object
  /// is fully up to the implementing algorithms.
  /// </summary>
  public interface BookKeeping<SS, DS>
    where SS : StaticState<SS, DS>
    where DS : DynamicState<SS, DS> {

  }

}
