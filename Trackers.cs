/*
 * TimeTracker test application
 * Copyright (c) 2020, SailGP, All Rights Reserved
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerTest {

    /// <summary>
    /// A Tracker receives reports of remote time reports, along
    /// with the local time at which the report was received.
    /// The calling app can query the Tracker at any time - generally
    /// much more frequently than tie reports come in - to
    /// ask for the predicted remote time given a local time.
    /// </summary>
    public interface ITracker {
        void Set( int id, double local, double remote );
        double GetRemote( double local );
    }


    /// <summary>
    /// Here's a very simple implementation of a Tracker. It remembers
    /// the last remote time it saw, and continually moves an estimated
    /// remote time value toward that remembered value with every call to
    /// GetRemote().  Note this doesn't try to estimate the rate of remote
    /// time change, and will always lag the actual remote time.
    /// </summary>
    public class SimpleTracker : ITracker {
        double last_remote = 0;
        double filtered_remote = 0;
        double new_weight = 0.3;
        bool first = true;

        public double GetRemote( double local ) {
            // Gradually move our filtered estimate toward the last remote value we saw
            filtered_remote = ( 1 - new_weight ) * filtered_remote + new_weight * last_remote;
            return filtered_remote;
        }

        public void Set( int id, double local, double remote ) {
            if ( first ) {
                // Start by completely trusting the first report we receive
                first = false;
                filtered_remote = remote;
            }
            last_remote = remote;
        }
    }
}
