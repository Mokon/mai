/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Utilities;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  public class MakePlayListMode : Mode {

    public override string GetHelp( ) {
      return "Makes a playlist of files and solves them using A*.";
    }

    protected GridWorldLoader<GenericGridWorldStaticState,
      GenericGridWorldDynamicState> Loader = new GenericGridWorldLoader( );
    private int MinPathLength = 0;
    private int MaxPathLength = int.MaxValue;
    private int AStarMinExpansion = 0;
    private int AStarMaxExpansion = int.MaxValue;
    private int IterationCountLimit = -1;
    private string dir = "./maps/game";
    private TextWriter Out = System.Console.Out;
    private bool Shuffle =  true;
    private int maxThreads = 0;
    private bool Solve = false;

    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public MakePlayListMode( ) {
      base.CLH = new CommandLineHandler(
        new GridWorldLoaderFlag( this, "", "Loader", "l", "loader" ),
        new CommandLineFlag( this, "Whether to shuffle the playlist.", "Shuffle", "s", "shuffle" ),
        new CommandLineFlag( this, "The max number of files to include in the play list.", "IterationCountLimit", "icl", "iterationCountLimit" ),
        new CommandLineFlag( this, "The max number of threads to utilize when computating the playlist.", "maxThreads", "mt", "maxThreads" ),
        new CommandLineFlag( this, "The min path length for files included in the playlist.", "MinPathLength", "minpl", "minPathLength" ),
        new CommandLineFlag( this, "The max path length for files included in the playlist.", "MaxPathLength", "maxpl", "maxPathLength" ),
        new CommandLineFlag( this, "The min A* number of expansions for files included in the playlist.", "AStarMinExpansion", "minexp", "minExpansions" ),
        new CommandLineFlag( this, "The max A* number of expansions for files included in the playlist.", "AStarMaxExpansion", "maxexp", "maxExpansions" ),
        new CommandLineFlag( this, "The name of the playlist to create.", "Out", "o", "output" ),
        new CommandLineFlag( this, "Whether to solve the maps for the playlist", "Solve", "v", "solve" ),
        new CommandLineFlag( this, "The directory to search for gwmap files to parse.", "dir", "d", "dir" )
        );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( ) {
      // Make a proccessing thread for each processor.
      List<Thread> Threads = new List<Thread>( );
      ThreadCount = Environment.ProcessorCount;
      if ( maxThreads != 0 && maxThreads < ThreadCount ) {
        ThreadCount = maxThreads;
      }
      for ( int i = 0 ; i < ThreadCount ; i++ ) {
        Thread t = new Thread( this.ThreadProcessor );
        t.Name = i.ToString( );
        Threads.Add( t );
      }

      // Setup the Queues for the processing threads to work with.
      var fQ = 
        Files.GetFilesRecursive( dir ).Where(
        file => ( file.EndsWith( ".gwmap" ) && !file.Contains( "KEY" ) ) ||
          file.EndsWith( ".xml" ) )
        .Take( IterationCountLimit != -1 ? IterationCountLimit : int.MaxValue );
      if ( Shuffle ) {
        fQ = fQ.Shuffle( );
      }

      System.Console.WriteLine( fQ.Count( ) + " files being processed." );
      FilesQueue = new Queue<string>( fQ );

      // Start the threads.
      foreach ( Thread t in Threads ) {
        t.Start( );
      }
    }

    /// <summary>
    /// The number of active threads.
    /// </summary>
    private int ThreadCount;

    /// <summary>
    /// Files to be solved.
    /// </summary>
    private Queue<string> FilesQueue;

    /// <summary>
    /// A convience method for locking and writing to a file.
    /// </summary>
    /// <param name="Output"></param>
    private void WriteToOut( string Output ) {
      lock ( Out ) {
        Out.WriteLine( Output );
        Out.Flush( );
      }
    }

    /// <summary>
    /// Launches a thread to handle the queue.
    /// </summary>
    public void ThreadProcessor( ) {
      string tName = Thread.CurrentThread.Name;
      System.Console.WriteLine( "Thread {0} Started ", tName );
      while ( FilesQueue.Count != 0 ) {
        System.Console.WriteLine( "Dequeing File on Thread {0}", tName );
        SolveFileTop( tName );
      }
      ThreadCount--;
      System.Console.WriteLine( "Thread {0} Ended", tName );
      if ( ThreadCount <= 0 ) {
        Out.Close( );
      }
    }

    /// <summary>
    /// Solves a file and adds to the processing queue.
    /// </summary>
    private void SolveFileTop( string tName ) {
      string file;
      lock ( FilesQueue ) {
        if ( FilesQueue.Count == 0 ) {
          return;
        } else {
          file = FilesQueue.Dequeue( );
        }
      }
      System.Console.WriteLine( "\tfile is {0} on thread {1}.", file, tName );
      if ( Solve ) {
        Solution solution = null;
        StreamReader s = null;
        AlgorithmRunner ar;
        try {
          s = new StreamReader( new FileStream( file, FileMode.Open,
            FileAccess.Read ) );
          ar = new AlgorithmRunner( s, Loader,
            "", new Metric( 1 ), true, null, 0 );

          solution = ar.Solve( null );
          s.Close( );
        } catch ( Exception e ) {
          System.Console.WriteLine( e.Message );
          s.Close( );
          return;
        }

        if ( solution.Path.Cost < new Metric( MinPathLength )|| 
        solution.Path.Cost > new Metric( MaxPathLength ) ||
        solution.Algorithm.Expansions < AStarMinExpansion ||
        solution.Algorithm.Expansions > AStarMaxExpansion ) {
          return;
        }

        WriteToOut( file + " " + solution.Path.Cost );
      } else {
        WriteToOut( file + " 0" );
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "make-playlist";
    }

  }

}
