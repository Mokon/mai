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

  /// <summary>
  /// Batch mode for running lots of algorithms at once quickly.
  /// </summary>
  public class BatchMode : AbstractBatchMode {

    public override string GetHelp( ) {
      return "Runs batch mode without threading on many setting, maps, and algorithms.";
    }

    private string dir = "./maps";
    
    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public BatchMode( ) {
      base.CLH.AddFlags( new CommandLineFlag( this, "The base input dir.", "dir", "d", "dir" ),
       new TruncOrCreateNewAutoFlushTextWriterFlag( this, "The output file.", "Out", "o", "output" )
     );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( string[] args ) {
      ParseArgs( args );
      Out.WriteLine( Results.Header( ) );

      // Make a proccessing thread for each processor.
      List<Thread> Threads = new List<Thread>( );
      int ThreadCount = Environment.ProcessorCount;
      if ( maxThreads != 0 && maxThreads < ThreadCount ) {
        ThreadCount = maxThreads;
      }
      for ( int i = 0 ; i < ThreadCount ; i++ ) {
        Thread t = new Thread( this.ThreadProcessor );
        t.Name = i.ToString( ) ;
        Threads.Add( t );
      }

      // Setup the Queues for the processing threads to work with.
      ProcessingQueue = new Queue<ProcessingQueueElement>( );
      var fQ = 
        Files.GetFilesRecursive( dir ).Where(
        file => file.EndsWith( ".gwmap" ) && !file.Contains( "KEY" ) )
        .Take( IterationCountLimit != -1 ? IterationCountLimit : int.MaxValue );
      if ( Shuffle ) {
        fQ = fQ.Shuffle( ) ;
      }

      FilesQueue = new Queue<string>( fQ );
      

      // Start the threads.
      foreach ( Thread t in Threads ) {
        t.Start( );
      }

    }

    /// <summary>
    /// Files to be processed with settings.
    /// </summary>
    private Queue<ProcessingQueueElement> ProcessingQueue;

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
      while ( FilesQueue.Count != 0 || ProcessingQueue.Count != 0 ) {
        if ( ProcessingQueue.Count == 0 ) {
          System.Console.WriteLine( "Dequeing File on Thread {0}", tName );
          SolveFileTop( tName );
        } else {
          System.Console.WriteLine( "Dequeing Processing Element on Thread {0}",
            tName );
          DoProcessingQueueTop( tName );
        }
      }
      System.Console.WriteLine( "Thread {0} Ended", tName );
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
      Solution solution = null ;
      StreamReader s = null;
      try {
        s = new StreamReader( new FileStream( file, FileMode.Open, FileAccess.Read ) );
        solution = new AlgorithmRunner( s,
         Loader, "", new Metric( Weight ), true, null, Seed ).Solve( Out );
        s.Close( );
      } catch ( Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms.PathNotFoundException ) {
        WriteToOut( "No Path Posible " + file );
        s.Close( );
        return;
      } catch ( Exception e ) {
        WriteToOut( "AStar Exception " + e.Message + " for file " + file );
        s.Close( );
        return;
      }

      if ( solution.Path.Cost < new Metric( MinPathLength ) ) {
        WriteToOut( "Min Path Length Violated " + file );
        return;
      } else if ( solution.Path.Cost > new Metric ( MaxPathLength ) ) {
        WriteToOut( "Max Path Length Violated " + file );
        return;
      } else if ( solution.Algorithm.Expansions < AStarMinExpansion ) {
        WriteToOut( "Min Expansions Violated " + file );
        return;
      } else if ( solution.Algorithm.Expansions > AStarMaxExpansion ) {
        WriteToOut( "Max Expansions Violated " + file );
        return;
      }

      double OptimalSolutionCost = solution.Path.Cost.ToDouble( );
      foreach ( var Alg in Algorithms ) {
        int computationLimit, clindex;
        for ( computationLimit = computationLimitMin, clindex  = 1 ;
          computationLimit < computationLimitMax ;
          computationLimit = computationLimitMethod( computationLimit, clindex ),
          clindex++ ) {
          ProcessingQueueElement e;
          e.Algorithm = Alg;
          e.File = file;
          e.OptimalSolutionCost = OptimalSolutionCost;
          e.ComputationLimit = computationLimit;
          lock ( ProcessingQueue ) {
            ProcessingQueue.Enqueue( e );
          }
          if ( !IsRealTime( Alg ) ) {
            break;
          }
        }
      }
    }

    /// <summary>
    /// A struct to represent an element in the priority queue.
    /// </summary>
    private struct ProcessingQueueElement {
      public double OptimalSolutionCost;
      public int ComputationLimit;
      public string File;
      public string Algorithm;
    }
    
    /// <summary>
    /// Handles the top of the processing queue.
    /// </summary>
    private void DoProcessingQueueTop( string tName ) {
      ProcessingQueueElement pqe;
      lock ( ProcessingQueue ) {
        pqe = ProcessingQueue.Dequeue( );
      }
      System.Console.WriteLine(
        "\tpqe is | cl={0} file={1} alg={2} | on thread {3}.",
        pqe.ComputationLimit, pqe.Algorithm, pqe.File, tName );

      Results Result;
      StreamReader s = null;
      try {
        Loader.LOS = LOS;
        s = new StreamReader( new FileStream( pqe.File, FileMode.Open, FileAccess.Read ) );
        Result = new AlgorithmRunner( s,
          Loader, pqe.Algorithm, new Metric( Weight ),
          true, null, Seed ).Run( 0, true, computationLimitCeilingRatio, pqe.Algorithm, pqe.ComputationLimit,
          new Metric( pqe.OptimalSolutionCost ), pqe.File, Dynamic );
        if ( Result.PathLength < pqe.OptimalSolutionCost - 0.1 ) {
          WriteToOut( "Unexplained Solution Cost pqe is | cl=" 
            + pqe.ComputationLimit + " file=" + pqe.File + " alg="
            + pqe.Algorithm + " |" );
        } 
        WriteToOut( Result.ToString( ) );
      } catch ( MaxPathLengthExceded ) {
        WriteToOut( "Ceiling Solution Cost  pqe is | cl=" 
            + pqe.ComputationLimit + " file=" + pqe.File + " alg="
            + pqe.Algorithm + " |" );
      } catch ( Exception e ) {
        WriteToOut( "Exception Caught " + e.Message + "  pqe is | cl=" 
            + pqe.ComputationLimit + " file=" + pqe.File + " alg=" 
            + pqe.Algorithm + " |" );
      } finally {
        if ( s != null ) {
          s.Close( );
        }
      }
    }

    /// <summary>
    /// Gets the name of the mode.
    /// </summary>
    /// <returns></returns>
    public override string Name( ) {
      return "batch";
    }

  }

}
