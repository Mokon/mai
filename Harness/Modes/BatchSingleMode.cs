/*
 * This file is part of the MAI framework, distributed under The M License.
 * Copyright © 2008-2010, David Bond, All Rights Resevered
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Net.Mokon.Edu.Unh.CS.AI.GridWorld;
using Net.Mokon.Edu.Unh.CS.AI.Search.Algorithms;
using Net.Mokon.Utilities;
using Net.Mokon.Utilities.CommandLine;

namespace Net.Mokon.Edu.Unh.CS.AI.Harness {

  /// <summary>
  /// Batch mode for running lots of algorithms at once quickly.
  /// </summary>
  public class BatchSingleMode : AbstractBatchMode {

    public override string GetHelp( ) {
      return "Runs a single map in batch mode through all the desired settings"
      + "and algorithms.";
    }

    private double opt;

    /// <summary>
    /// Constructor whihc initilizes data fields to their default values.
    /// </summary>
    public BatchSingleMode( ) {
      opt = 0;
      base.CLH.AddFlags( new CommandLineFlag( this, "The A* optimal path length for this file.", "opt", "O", "opt" ),
        new AppendAutoFlushTextWriterFlag( this, "The base output file.", "Out", "o", "output" )
        );
    }

    /// <summary>
    /// Runs the Mode.
    /// </summary>
    /// <param name="args"></param>
    public override void Run( ) {
      Out.WriteLine( Results.Header( ) );

      // Make a proccessing thread for each processor.
      List<Thread> Threads = new List<Thread>( );
      int ThreadCount = Environment.ProcessorCount;
      if ( maxThreads != 0 && maxThreads < ThreadCount ) {
        ThreadCount = maxThreads;
      }
      for ( int i = 0 ; i < ThreadCount ; i++ ) {
        Thread t = new Thread( this.ThreadProcessor );
        t.Name = i.ToString( );
        Threads.Add( t );
      }

      // Setup the Queues for the processing threads to work with.
      ProcessingQueue = new Queue<ProcessingQueueElement>( );

      foreach ( var Alg in Algorithms ) {
        int computationLimit, clindex;
        for ( computationLimit = computationLimitMin, clindex  = 1 ;
          computationLimit < computationLimitMax ;
          computationLimit = computationLimitMethod( computationLimit, clindex ),
          clindex++ ) {
          ProcessingQueueElement e;
          e.Algorithm = Alg;
          e.File = file;
          e.OptimalSolutionCost = opt;
          e.ComputationLimit = computationLimit;
          ProcessingQueue.Enqueue( e );
          if ( !IsRealTime( Alg ) ) {
            break;
          }
        }
      }

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
      while ( ProcessingQueue.Count != 0 ) {
        System.Console.WriteLine( "Dequeing Processing Element on Thread {0}",
          tName );
        DoProcessingQueueTop( tName );
      }
      System.Console.WriteLine( "Thread {0} Ended", tName );
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
        "\tpqe is | cl={0} file={2} alg={1} | on thread {3}.",
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
          WriteToOut( "Exception Occcurd Unexplained Solution Cost pqe is | cl=" 
            + pqe.ComputationLimit + " file=" + pqe.File + " alg="
            + pqe.Algorithm + " |" );
        }
        WriteToOut( Result.ToString( ) );
      } catch ( MaxPathLengthExceded E ) {
        WriteToOut( "Ceiling Solution Cost  pqe is | cl=" 
            + pqe.ComputationLimit + " file=" + pqe.File + " alg="
            + pqe.Algorithm + " |" );
        WriteToOut( E.Results.ToString( ) );
      } catch ( Exception e ) {
        WriteToOut( "Exception Caught " + e.Message + "  pqe is | cl=" 
            + pqe.ComputationLimit + " file=" + pqe.File + " alg=" 
            + pqe.Algorithm + " |" );
        System.Console.WriteLine( e.Message + e.StackTrace );
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
      return "batch-single";
    }

  }

}
