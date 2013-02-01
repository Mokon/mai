/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Mokon.Edu.Unh.CS.AI.GridWorld {

  public class GenericGridWorldDynamicState :
    GridWorldDynamicState<GenericGridWorldStaticState,
    GenericGridWorldDynamicState> {

    /// <summary>
    /// A getter for the hash value of this object. The only reason I do this
    /// is to shut up the dumb ide that keeps on warning me. I should prob. just
    /// use a warning supression.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode( ) {
      if ( this.WorldObjects != null ) {
        Unit u = this.FindUnit( 0 );
        return u.GetHashCode( );
      } else {
        return 0;
      }
    }

    /// <summary>
    /// Equality method which only uses the fields of concern to us.
    /// </summary>
    /// <param name="obj">The object to compare for equality.</param>
    /// <returns>Whether or not these objects are equal.</returns>
    public override bool Equals( object obj ) {
      if(  obj is GenericGridWorldDynamicState ) {
        GenericGridWorldDynamicState o = obj as GenericGridWorldDynamicState;
        foreach( var e in this.WorldObjects ) {
          bool found = false;
          foreach( var i in o.WorldObjects ) {
            if( i.Equals( e ) ) {
              found = true;
              break;
            }
          }
          if( !found ) {
            return false;
          }
        }
        return true;
      } else {
        return false;
      }
    }

    /// <summary>
    /// Returns true if all units are at their destinations.
    /// </summary>
    /// <returns></returns>
    public bool DstsReached( ) {
      foreach( var f in this.WorldObjects ) {
        if( f is Unit && !(f as Unit).AtDst( ) ) {
          return false ;
        }
      }
      return true ;
    }

    /// <summary>
    /// Converts the static to a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString( ) {
      StringBuilder sb = new StringBuilder( );
      foreach( var obj in this.WorldObjects ) {
        sb.Append( obj );
      }
      return sb.ToString( );
    }

    /// <summary>
    /// Tests if the state is invalid.
    /// </summary>
    /// <param name="ss"></param>
    /// <returns></returns>
    public override bool IsInvalid( GenericGridWorldStaticState ss, bool all ) {
      Unit u = this.FindUnit(0);
      return !ss.Tiles[u.Y][u.X].IsPassable( ss, all );
    }

  }

}
