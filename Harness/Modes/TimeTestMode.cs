/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.IO;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities.CommandLine;
using System.Diagnostics;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class TimeTestMode : Mode {

    public override string GetHelp( ) {
      return "A time testing mode.";
    }

    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public TimeTestMode( ) {
      base.CLH = new CommandLineHandler( );
    }

    /// <summary>
    /// Takes a playlist and scales it by the given width and height.
    /// </summary>
    public override void Run( ) {
      long lastTime = 0, newTime = 0;
      int times = 25;
      while ( times-- > 0 ) {
        do {
          lastTime = newTime;
          newTime = DateTime.Now.Ticks;
        } while ( lastTime == newTime );
        Console.WriteLine( "DateTime.Now.Ticks: {0} {1}", newTime, newTime-lastTime );
      }

      lastTime = 0;
      newTime = 0;
      times = 25;
      while ( times-- > 0 ) {
        do {
          lastTime = newTime;
          newTime = Stopwatch.GetTimestamp( );
        } while ( lastTime == newTime );
        Console.WriteLine( "Stopwatch GetTimestamp: {0} {1}", newTime, newTime-lastTime );
      }

      times = 25;
      Console.WriteLine( "Stopwatch Is High Res: {0}", Stopwatch.IsHighResolution );
      Console.WriteLine( "Stopwatch Freq: {0}", Stopwatch.Frequency );
      long nanosecPerTick = ( 1000L*1000L*1000L ) / Stopwatch.Frequency;
      Console.WriteLine( "  Timer is accurate within {0} nanoseconds",
          nanosecPerTick );
      Stopwatch sw = new Stopwatch( );
      
      while ( times-- > 0 ) {
        sw.Start( );
        sw.Stop( );
        Console.WriteLine( "Stopwatch: {0}", sw.ElapsedTicks );
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "time-test";
    }

  }

}
