﻿using System.Collections.Generic;

namespace SlideMeister.Model
{
    public class TransitionSequence
    {
        public string Name { get; set; }

        public List<TransitionSequenceStep> Steps { get; set; }
            = new List<TransitionSequenceStep>();

        public override string ToString()
        {
            return Name;
        }
    }
}